﻿/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2023, Carnegie Mellon University. All rights reserved.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using ArenaUnity.Components;
using ArenaUnity.Schemas;
using Google.Apis.Auth.OAuth2;
using MimeMapping;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Siccity.GLTFUtility;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Rendering;

namespace ArenaUnity
{
    /// <summary>
    /// Class to manage a singleton instance of the ARENA client connection.
    /// </summary>
    [HelpURL("https://docs.arenaxr.org")]
    [DisallowMultipleComponent]
    public class ArenaClientScene : ArenaMqttClient
    {
        // Singleton instance of this connection object
        public static ArenaClientScene Instance { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            Instance = this;
        }

        [Tooltip("Namespace (automated with username), but can be overridden (runtime changes ignored).")]
        public string namespaceName = null;
        [Tooltip("Name of the scene, without namespace ('example', not 'username/example', runtime changes ignored).")]
        public string sceneName = "example";

        [Header("Presence")]
        [Tooltip("Display other camera avatars in the scene")]
        public bool renderCameras = true;
        [Tooltip("Display other avatars' hand controllers in the scene")]
        public bool renderHands = true;
        [Tooltip("Display VR Controller Rays")]
        public bool drawControllerRays = false;

        [Header("Performance")]
        [Tooltip("Console log MQTT object messages")]
        public bool logMqttObjects = false;
        [Tooltip("Console log MQTT user messages")]
        public bool logMqttUsers = false;
        [Tooltip("Console log MQTT client event messages")]
        public bool logMqttEvents = false;
        [Tooltip("Console log MQTT non-persist messages")]
        public bool logMqttNonPersist = false;
        [Tooltip("Global publish frequency to publish detected transform changes (milliseconds)")]
        [Range(100, 1000)]
        public int globalUpdateMs = 100;

        /// <summary>
        /// Browser URL for the scene.
        /// </summary>
        public string sceneUrl { get; private set; }

        internal bool sceneObjectRights { get; private set; } = false;

        public bool persistLoaded { get; private set; } = false;

        private string sceneTopic = null;
        public Dictionary<string, GameObject> arenaObjs { get; private set; } = new Dictionary<string, GameObject>();
        internal Dictionary<string, GameObject> childObjs = new Dictionary<string, GameObject>();
        internal List<string> pendingDelete = new List<string>();
        internal List<string> downloadQueue = new List<string>();
        internal List<string> parentalQueue = new List<string>();
        internal List<string> localCameraIds = new List<string>();
        internal ArenaDefaultsJson arenaDefaults { get; private set; }

        // Define callbacks
        public delegate void DecodeMessageDelegate(string topic, byte[] message);
        public DecodeMessageDelegate OnMessageCallback = null; // null, until library user instantiates.

        public string originalName { get; private set; }

        static string importPath = null;

        const string prefixCam = "camera_";
        const string prefixHandL = "handLeft_";
        const string prefixHandR = "handRight_";
        static readonly List<string> gltfTypeList = new List<string> { "gltf-model", "handLeft", "handRight" };
        static readonly string[] msgUriTags = { "url", "src", "overrideSrc", "detailedUrl", "headModelPath", "texture" };
        static readonly string[] gltfUriTags = { "uri" };
        static readonly string[] skipMimeClasses = { "video", "audio" };
        static readonly string[] requiredShadersStandardRP = {
            "Standard",
            "Unlit/Color",
            "GLTFUtility/Standard (Metallic)",
            "GLTFUtility/Standard Transparent (Metallic)",
            "GLTFUtility/Standard (Specular)",
            "GLTFUtility/Standard Transparent (Specular)",
        };
        static readonly string[] requiredShadersURPHDRP = {
            // "Standard",
            // "Unlit/Color",
            "GLTFUtility/URP/Standard (Metallic)",
            "GLTFUtility/URP/Standard Transparent (Metallic)",
            "GLTFUtility/URP/Standard (Specular)",
            "GLTFUtility/URP/Standard Transparent (Specular)",
        };

        protected override void OnEnable()
        {
            base.OnEnable();

            // ensure consistent name and transform
            transform.position = Vector3.zero;
            transform.rotation = Quaternion.identity;
            transform.localScale = Vector3.one;

#if UNITY_EDITOR
            // sort arena component to the top, below Transform
            while (UnityEditorInternal.ComponentUtility.MoveComponentUp(this)) { }
#endif
        }

        private static void LogAndExit(string msg)
        {
            Debug.LogError(msg);
#if UNITY_EDITOR
            if (Application.isPlaying)
                EditorApplication.ExitPlaymode();
#endif
        }

        // Start is called before the first frame update
        protected override void Start()
        {
            originalName = name;
            name = $"{originalName} (Starting...)";

            importPath = Path.Combine(appFilesPath, "Assets", "ArenaUnity", "import");

            var requiredShaders = requiredShadersStandardRP;
            // check if URP or HDR; different shaders are required
            if (GraphicsSettings.renderPipelineAsset)
                requiredShaders = requiredShadersURPHDRP;

            // ensure shaders are in project
            foreach (string shader in requiredShaders)
            {
                if (Shader.Find(shader) == null)
                {
                    LogAndExit($"Required shader '{shader}' not found in project.");
                }
            }

#if UNITY_EDITOR
            StartCoroutine(ConnectArena());
#endif
        }

        /// <summary>
        /// Authenticate, MQTT connect, and add ARENA objects from Persistence DB to local app.
        /// </summary>
        public IEnumerator ConnectArena()
        {
            bool nameSafe = true;
            // local inventory before MQTT
            foreach (var aobj in FindObjectsOfType<ArenaObject>())
            {
                if (!arenaObjs.ContainsKey(aobj.name))
                {
                    arenaObjs[aobj.name] = aobj.gameObject;
                }
                else
                {
                    // prevent name collisions before MQTT
                    nameSafe = false; // critical error, arena objects must have unique names
                    Debug.LogError($"More than one ArenaObject is named '{aobj.name}'.");
                }
            }
            if (!nameSafe)
            {
                LogAndExit("All ArenaObjects must have unique names.");
                yield break;
            }

            // get arena default settings
            CoroutineWithData cd;
            cd = new CoroutineWithData(this, HttpRequestAuth($"https://{hostAddress}/conf/defaults.json"));
            yield return cd.coroutine;
            if (!isCrdSuccess(cd.result)) yield break;
            string jsonString = cd.result.ToString();
            JObject jsonVal = JObject.Parse(jsonString);
            arenaDefaults = jsonVal.SelectToken("ARENADefaults").ToObject<ArenaDefaultsJson>();
            brokerAddress = arenaDefaults.mqttHost;

            // start auth flow and MQTT connection
            ArenaCamera[] camlist = FindObjectsOfType<ArenaCamera>();
            name = $"{originalName} (Authenticating...)";
            cd = new CoroutineWithData(this, SigninScene(sceneName, namespaceName, arenaDefaults.realm, camlist.Length > 0, arenaDefaults.latencyTopic));
            yield return cd.coroutine;
            name = $"{originalName} (MQTT Connecting...)";
            if (cd.result != null)
            {
                if (string.IsNullOrWhiteSpace(namespaceName)) namespaceName = cd.result.ToString();
                sceneTopic = $"{arenaDefaults.realm}/s/{namespaceName}/{sceneName}";
                sceneUrl = $"https://{hostAddress}/{namespaceName}/{sceneName}";
            }
            if (permissions == null)
            {   // fail when permissions not set
                LogAndExit("Permissions not received.");
                yield break;
            }
            ArenaMqttTokenClaimsJson perms = JsonConvert.DeserializeObject<ArenaMqttTokenClaimsJson>(permissions);
            foreach (string pubperm in perms.publ)
            {
                if (sceneTopic.StartsWith((pubperm).TrimEnd(new char[] { '/', '#' }))) sceneObjectRights = true;
            }
            // publish arena cameras where requested
            bool foundFirstCam = false;
            foreach (ArenaCamera cam in camlist)
            {
                if ((cam.name == Camera.main.name || camlist.Length == 1) && !foundFirstCam)
                {
                    // publish main/selected camera
                    cam.HasPermissions = true; // TODO (mwfarb): client connection always gets at least one
                    cam.userid = userid;
                    cam.camid = camid;
                    foundFirstCam = true;
                }
                else
                {
                    cam.HasPermissions = sceneObjectRights;
                    // TODO (mwfarb): fix: other cameras are auto-generated, and account must have all scene rights
                    if (!sceneObjectRights)
                    {
                        Debug.LogWarning($"Using more than one ArenaCamera requires full scene permissions. Only one camera will be published.");
                    }
                    var random = UnityEngine.Random.Range(0, 100000000);
                    cam.userid = $"{random:D8}_unity";
                    cam.camid = $"camera-{random:D8}_unity";
                }
                localCameraIds.Add(cam.camid);
            }

            // get persistence objects
            StartCoroutine(SceneLoadPersist());
        }

        /// <summary>
        /// Disconnect MQTT and remove ARENA objects from local app.
        /// </summary>
        public void DisconnectArena()
        {
            Disconnect();
            foreach (var aobj in arenaObjs.Values)
            {
                Destroy(aobj);
            }
            arenaObjs.Clear();
        }

        // Update is called once per frame
        protected override void Update()
        {
            base.Update();

            if (pendingDelete.Count > 0)
            {   // confirm for any deletes requested
                string ids = string.Join(", ", pendingDelete);
#if UNITY_EDITOR

                if (EditorUtility.DisplayDialog("Delete!",
                     $"Are you sure you want to delete object(s) from the ARENA: {ids}?", "Delete", "Save"))
                {
                    foreach (string object_id in pendingDelete)
                    {
                        ArenaObjectJson msg = new ArenaObjectJson
                        {
                            object_id = object_id,
                            action = "delete",
                            persist = true,
                        };
                        string payload = JsonConvert.SerializeObject(msg);
                        PublishObject(msg.object_id, payload, sceneObjectRights);
                    }
                }
#endif
                pendingDelete.Clear();
            }
        }

        private IEnumerator SceneLoadPersist()
        {
            CoroutineWithData cd;
            cd = new CoroutineWithData(this, HttpRequestAuth($"https://{arenaDefaults.persistHost}{arenaDefaults.persistPath}{namespaceName}/{sceneName}", csrfToken));
            yield return cd.coroutine;
            if (!isCrdSuccess(cd.result)) yield break;
            string jsonString = cd.result.ToString();
            List<ArenaObjectJson> persistMessages = JsonConvert.DeserializeObject<List<ArenaObjectJson>>(jsonString);
            // establish objects
            int objects_num = 1;
            if (Directory.Exists(importPath))
                Directory.Delete(importPath, true);
            if (File.Exists($"{importPath}.meta"))
                File.Delete($"{importPath}.meta");
            foreach (ArenaObjectJson msg in persistMessages)
            {
                string object_id = msg.object_id;
                string msg_type = msg.type;
                msg.persist = true; // always true coming from persist

                if (arenaObjs != null && !arenaObjs.ContainsKey(object_id)) // do not duplicate, local project object takes priority
                {
                    // there isnt already an object in the scene created by the user with the same object_id
                    if (GameObject.Find(msg.object_id) == null)
                    {
                        IEnumerable<string> uris = ExtractAssetUris(msg.attributes, msgUriTags);
                        foreach (var uri in uris)
                        {
                            if (!string.IsNullOrWhiteSpace(uri))
                            {
                                cd = new CoroutineWithData(this, DownloadAssets(msg_type, uri));
                                yield return cd.coroutine;
                            }
                        }
                    }
                    CreateUpdateObject(msg, msg.attributes);
                }
                objects_num++;
            }
            persistLoaded = true;
        }

        private static IEnumerable<string> ExtractAssetUris(object data, string[] urlTags)
        {
            List<string> tagList = new List<string>(urlTags);
            var root = (JContainer)JToken.Parse(JsonConvert.SerializeObject(data));
            var uris = root.DescendantsAndSelf().OfType<JProperty>().Where(p => tagList.Contains(p.Name)).Select(p => p.Value.Value<string>());
            return uris;
        }

        internal Uri ConstructRemoteUrl(string srcUrl)
        {
            if (string.IsNullOrWhiteSpace(srcUrl))
            {
                return null;
            }
            string objUrl = srcUrl.TrimStart('/');
            objUrl = Uri.EscapeUriString(objUrl);
            if (Uri.IsWellFormedUriString(objUrl, UriKind.Relative)) objUrl = $"https://{hostAddress}/{objUrl}";
            else objUrl = objUrl.Replace("www.dropbox.com", "dl.dropboxusercontent.com"); // replace dropbox links to direct links
            if (string.IsNullOrWhiteSpace(objUrl)) return null;
            if (!Uri.IsWellFormedUriString(objUrl, UriKind.Absolute))
            {
                Debug.LogWarning($"Invalid Uri: '{objUrl}'");
                return null;
            }
            objUrl = Uri.UnescapeDataString(objUrl);
            return new Uri(objUrl);
        }

        internal string ConstructLocalPath(Uri uri)
        {
            if (uri == null) return null;
            string url2Path = uri.Host + uri.AbsolutePath;
            string objFileName = string.Join(Path.DirectorySeparatorChar.ToString(), url2Path.Split(Path.GetInvalidFileNameChars()));
            return Path.Combine(importPath, objFileName);
        }

        private IEnumerator DownloadAssets(string messageType, string msgUrl)
        {
            if (messageType != "object") yield break;
            Uri remoteUri = ConstructRemoteUrl(msgUrl);
            if (remoteUri == null) yield break;
            // don't download the same asset twice, or simultaneously
            if (downloadQueue.Contains(remoteUri.AbsoluteUri)) yield break;
            downloadQueue.Add(remoteUri.AbsoluteUri);
            // check asset type
            if (!Path.HasExtension(remoteUri.AbsoluteUri)) yield break;
            string mimeType = MimeUtility.GetMimeMapping(remoteUri.GetLeftPart(UriPartial.Path));
            if (mimeType == null) yield break;
            if (skipMimeClasses.ToList().Contains(mimeType.Split('/')[0])) yield break;
            // load remote assets
            string localPath = ConstructLocalPath(remoteUri);
            if (localPath == null) yield break;
            bool allPathsValid = true;
            if (!File.Exists(localPath))
            {
                // get main url src
                CoroutineWithData cd = new CoroutineWithData(this, HttpRequestRaw(remoteUri.AbsoluteUri));
                yield return cd.coroutine;
                if (isCrdSuccess(cd.result))
                {
                    byte[] urlData = (byte[])cd.result;
                    // get gltf sub-assets
                    if (".gltf" == Path.GetExtension(localPath).ToLower())
                    {
                        string json = System.Text.Encoding.UTF8.GetString(urlData);
                        IEnumerable<string> uris = new string[] { };
                        try
                        {
                            uris = ExtractAssetUris(JsonConvert.DeserializeObject(json), gltfUriTags);
                        }
                        catch (JsonReaderException e)
                        {
                            Debug.LogWarning(e.Message);
                            allPathsValid = false;
                        }
                        foreach (var uri in uris)
                        {
                            if (!string.IsNullOrWhiteSpace(uri))
                            {
                                Uri subUrl = null;
                                try
                                {
                                    subUrl = new Uri(remoteUri, uri);
                                }
                                catch (UriFormatException)
                                {
                                    // formatting errors may be encoded binary data
                                    continue;
                                }
                                catch (Exception err)
                                {
                                    Debug.LogWarning($"Invalid GLTF uri: {err.Message}");
                                }
                                cd = new CoroutineWithData(this, HttpRequestRaw(subUrl.AbsoluteUri));
                                yield return cd.coroutine;
                                if (isCrdSuccess(cd.result))
                                {
                                    byte[] urlSubData = (byte[])cd.result;
                                    string localSubPath = Path.Combine(Path.GetDirectoryName(localPath), uri);
                                    SaveAsset(urlSubData, localSubPath);
#if UNITY_EDITOR
                                    // import each sub-file for a deterministic reference
                                    ImportAsset(localSubPath);
#endif
                                }
                                else allPathsValid = false;
                            }
                        }
                    }
                    SaveAsset(urlData, localPath);
                }
                else allPathsValid = false;

                if (!allPathsValid) yield break;
                // import master-file to link to the rest
                ImportAsset(localPath);
            }

            yield return localPath;
        }

        private void ImportAsset(string localPath)
        {
#if UNITY_EDITOR
            try
            {
                AssetDatabase.ImportAsset(localPath);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Import error. {e.Message}");
            }
            ClearProgressBar();
#endif
        }

        private void SaveAsset(byte[] data, string path)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            if (!File.Exists(path))
            {
                using (FileStream fs = new FileStream(path, FileMode.Create))
                {
                    for (int i = 0; i < data.Length; i++)
                    {
                        fs.WriteByte(data[i]);
                    }
                }
            }
        }

        // methods for the editor
        private void DisplayCancelableProgressBar(string title, string info, float progress)
        {
#if UNITY_EDITOR
            EditorUtility.DisplayCancelableProgressBar(title, info, progress);
#endif
        }

        private void ClearProgressBar()
        {
#if UNITY_EDITOR
            EditorUtility.ClearProgressBar();
#endif
        }

        private void CreateUpdateObject(ArenaObjectJson msg, object indata, object menuCommand = null)
        {
            ArenaObject aobj = null;
            if (arenaObjs.TryGetValue(msg.object_id, out GameObject gobj))
            {   // update local
                if (gobj != null)
                    aobj = gobj.GetComponent<ArenaObject>();
            }
            else
            {   // create local
#if !UNITY_EDITOR
                Debug.Log($"Loading object '{msg.object_id}'..."); // show new objects in log
#endif
                // check if theres already an object in unity, if so don't make a new one
                gobj = GameObject.Find((string)msg.object_id);
                if (gobj == null)
                {
                    gobj = new GameObject();
                    gobj.name = msg.object_id;
                }
                aobj = gobj.GetComponent<ArenaObject>();
                if (aobj == null)
                {
                    aobj = gobj.AddComponent(typeof(ArenaObject)) as ArenaObject;
                    arenaObjs[msg.object_id] = gobj;
                }
                aobj.Created = true;
                if (msg.persist.HasValue)
                    aobj.persist = (bool)msg.persist;
                aobj.messageType = msg.type;
                if (msg.ttl > 0)
                {
                    aobj.SetTtlDeleteTimer((float)msg.ttl);
                }
#if UNITY_EDITOR
                // local create context auto-select
                if (menuCommand != null)
                {
                    // Register the creation in the undo system
                    Undo.RegisterCreatedObjectUndo(gobj, "Create " + gobj.name);
                    Selection.activeObject = gobj;
                }
#endif
            }

            JObject jData = JObject.Parse(JsonConvert.SerializeObject(indata));
            //new JsonSerializerSettings
            //{
            //    Error = (sender, args) => {
            //        Debug.LogWarning($"{args.ErrorContext.Error.Message}: {args.ErrorContext.OriginalObject}");
            //        args.ErrorContext.Handled = true;
            //    },
            //});

            switch (msg.type)
            {
                case "object":
                    UpdateObjectMessage(msg, indata, aobj, gobj, jData);
                    break;
                case "scene-options":
                    UpdateSceneOptionsMessage(indata, gobj, jData);
                    break;
                case "program":
                    // TODO (mwfarb): define program implementation or lack thereofd
                    break;
            }

            gobj.transform.hasChanged = false;

            if (aobj != null)
            {
                aobj.data = indata;
                var updatedData = jData;
                if (aobj.jsonData != null)
                    updatedData.Merge(JObject.Parse(aobj.jsonData));
                updatedData.Merge(indata);
                aobj.jsonData = JsonConvert.SerializeObject(updatedData, Formatting.Indented);
            }
        }

        private void UpdateSceneOptionsMessage(object indata, GameObject gobj, JObject jData)
        {
            ArenaArenaSceneOptionsJson data = JsonConvert.DeserializeObject<ArenaArenaSceneOptionsJson>(indata.ToString());

            // handle scene options attributes
            foreach (var result in jData)
            {
                switch (result.Key)
                {
                    case "env-presets": ArenaUnity.ApplyEnvironmentPresets(gobj, data); break;
                    case "scene-options": ArenaUnity.ApplySceneOptions(gobj, data); break;
                    case "renderer-settings": ArenaUnity.ApplyRendererSettings(gobj, data); break;
                    case "post-processing": ArenaUnity.ApplyPostProcessing(gobj, data); break;
                }
            }
        }

        private void UpdateObjectMessage(ArenaObjectJson msg, object indata, ArenaObject aobj, GameObject gobj, JObject jData)
        {
            ArenaObjectDataJson data = JsonConvert.DeserializeObject<ArenaObjectDataJson>(indata.ToString());

            // modify Unity attributes
            bool worldPositionStays = false; // default: most children need relative position
            aobj.parentId = (string)data.parent;
            string parent = (string)data.parent;
            string object_type = (string)data.object_type;
            aobj.object_type = object_type;
            var url = !string.IsNullOrEmpty(data.src) ? data.src : data.url;

            // handle wire object attributes
            switch (object_type)
            {
                // deprecation warnings
                case "cube":
                    Debug.LogWarning($"data.object_type: {object_type} is deprecated for object-id: {msg.object_id}, use data.object_type: box instead.");
                    ArenaUnity.ApplyWireBox(indata, gobj); break;

                // wire object primitives
                case "box":
                case "capsule":
                case "circle":
                case "cone":
                case "cylinder":
                case "dodecahedron":
                case "icosahedron":
                case "octahedron":
                case "plane":
                case "ring":
                case "roundedbox":
                case "sphere":
                case "tetrahedron":
                case "torus":
                case "torusKnot":
                case "triangle":
                case "videosphere":
                    ArenaUnity.ApplyGeometry(object_type, indata, gobj); break;

                // other wire objects
                case "entity": /* general GameObject */ break;
                case "light": ArenaUnity.ApplyWireLight(indata, gobj); break;
                case "text": ArenaUnity.ApplyWireText(indata, gobj); break;
                case "line": ArenaUnity.ApplyWireLine(indata, gobj); break;
                case "thickline": ArenaUnity.ApplyWireThickline(indata, gobj); break;
                // TODO: case "arenaui-card": ArenaUnity.ApplyWireArenauiCard(indata, gobj); break;
                // TODO: case "arenaui-button-panel": ArenaUnity.ApplyWireArenauiButtonPanel(indata, gobj); break;
                // TODO: case "arenaui-prompt": ArenaUnity.ApplyWireArenauiPrompt(indata, gobj); break;
                // TODO: case "ocean": ArenaUnity.ApplyWireOcean(indata, gobj); break;
                // TODO: case "pcd-model": ArenaUnity.ApplyWirePcdModel(indata, gobj); break;
                // TODO: case "threejs-scene": ArenaUnity.ApplyWireThreejsScene(indata, gobj); break;
                case "gltf-model":
                    // load main model
                    if (url != null && aobj.gltfUrl == null)
                    {
                        // keep url, to add/remove and check exiting imported urls
                        string assetPath = checkLocalAsset(url);
                        if (assetPath != null)
                        {
                            aobj.gltfUrl = url;
                            AttachGltf(assetPath, gobj, aobj);
                        }
                    }
                    break;
                case "image":
                    // load image file
                    if (url != null)
                        AttachImage(checkLocalAsset(url), gobj);
                    break;

                // user avatar objects
                case "camera":
                    if (renderCameras)
                    {
                        if (!gobj.TryGetComponent<Camera>(out var cam))
                            cam = gobj.AddComponent<Camera>();
                        //cam = gobj.transform.gameObject.AddComponent<Camera>();
                        cam.nearClipPlane = 0.1f; // match arena
                        cam.farClipPlane = 10000f; // match arena
                        cam.fieldOfView = 80f; // match arena
                        cam.targetDisplay = 8; // render on least-used display
                        var json = new ArenaArenaUserJson();
                        json = JsonConvert.DeserializeObject<ArenaArenaUserJson>(ArenaUnity.MergeRawJson(json, data.arenaUser));
                        AttachAvatar(msg.object_id, json, gobj);
                    }
                    break;
                case "handLeft":
                case "handRight":
                    if (renderHands)
                    {
                        AttachHand(msg.object_id, url, gobj);
                    }
                    break;
            }

            // handle data.parent BEFORE setting transform in case object becomes unparented
            if (parent != null)
            {
                // establish parent/child relationships
                if (arenaObjs.ContainsKey(parent))
                {
                    gobj.SetActive(true);
                    // makes the child keep its local orientation rather than its global orientation
                    gobj.transform.SetParent(arenaObjs[parent].transform, worldPositionStays);
                }
                else
                {
                    gobj.SetActive(false);
                    parentalQueue.Add(parent);
                    childObjs.Add(msg.object_id, gobj);
                }
            }
            else
            {
                if (gobj.transform.parent != null)
                {
                    gobj.transform.SetParent(null);
                }
            }

            if (parentalQueue.Contains(msg.object_id))
            {
                // find children awaiting a parent
                foreach (KeyValuePair<string, GameObject> cgobj in childObjs)
                {
                    string cparent = cgobj.Value.GetComponent<ArenaObject>().parentId;
                    if (cparent != null && cparent == msg.object_id)
                    {
                        cgobj.Value.SetActive(true);
                        // makes the child keep its local orientation rather than its global orientation
                        cgobj.Value.transform.SetParent(arenaObjs[msg.object_id].transform, worldPositionStays);
                        cgobj.Value.transform.hasChanged = false;
                        //childObjs.Remove(cgobj.Key);
                    }
                }
                parentalQueue.Remove(msg.object_id);
            }

            // apply transform triad
            if (data.position != null)
            {
                gobj.transform.localPosition = ArenaUnity.ToUnityPosition(data.position);
            }
            if (data.rotation != null)
            {
                bool invertY = true;
                if (jData.SelectToken("rotation.w") != null) // quaternion
                    gobj.transform.localRotation = ArenaUnity.ToUnityRotationQuat(data.rotation, invertY);
                else // euler
                    gobj.transform.localRotation = ArenaUnity.ToUnityRotationEuler(data.rotation, invertY);
            }
            if (data.scale != null)
            {
                gobj.transform.localScale = ArenaUnity.ToUnityScale(data.scale);
            }

            // apply rendering visibility attributes, before other on-wire object attributes
            if (data.visible != null) // visible, if set is highest priority to enable/disable renderer
            {
                ArenaUnity.ApplyVisible(gobj, data);
            }
            else if (data.remoteRender != null) // remote-render, if set is lowest priority to enable/disable renderer
            {
                ArenaUnity.ApplyRemoteRender(gobj, data);
            }

            // handle non-wire object attributes, as they occur in json
            foreach (var result in jData)
            {
                switch (result.Key)
                {
                    // deprecation warnings
                    case "color":
                        if (ArenaUnity.primitives.Contains(object_type))
                        {
                            Debug.LogWarning($"data.color is deprecated for object-id: {msg.object_id}, object_type: {object_type}, use data.material.color instead.");
                            data.material ??= new ArenaMaterialJson();
                            data.material.Color = data.color;
                            ArenaUnity.ApplyMaterial(gobj, data);
                        }
                        break;
                    case "light":
                        if (object_type == "light")
                        {
                            Debug.LogWarning($"data.light.[property] is deprecated for object-id: {msg.object_id}, object_type: {object_type}, use data.[property] instead.");
                            ArenaUnity.ApplyWireLight(data.light, gobj);
                        }
                        break;
                    case "src":
                        Debug.LogWarning($"data.src is deprecated for object-id: {msg.object_id}, for many object_types, use data.url instead.");
                        break;
                    case "text":
                        if (object_type == "text")
                        {
                            Debug.LogWarning($"data.text is deprecated for object-id: {msg.object_id}, object_type: {object_type}, use data.value instead.");
                        }
                        break;

                    // expected attributes
                    // TODO: case "animation": ArenaUnity.ApplyAnimation(gobj, data); break;
                    // TODO: case "armarker": ArenaUnity.ApplyArmarker(gobj, data); break;
                    case "click-listener": ArenaUnity.ApplyClickListener(gobj, data); break;
                    // TODO: case "box-collision-listener": ArenaUnity.ApplyBoxCollisionListener(gobj, data); break;
                    // TODO: case "collision-listener": ArenaUnity.ApplyCollisionListener(gobj, data); break;
                    // TODO: case "blip": ArenaUnity.ApplyBlip(gobj, data); break;
                    // TODO: case "dynamic-body": ArenaUnity.ApplyDynamicBody(gobj, data); break;
                    case "geometry":
                        if (object_type == "entity")
                            ArenaUnity.ApplyGeometry(null, data.geometry, gobj); break;
                    // TODO: case "goto-landmark": ArenaUnity.ApplyGotoLandmark(gobj, data); break;
                    // TODO: case "goto-url": ArenaUnity.ApplyGotoUrl(gobj, data); break;
                    // TODO: case "hide-on-enter-ar": ArenaUnity.ApplyHideOnEnterAr(gobj, data); break;
                    // TODO: case "hide-on-enter-vr": ArenaUnity.ApplyHideOnEnterVr(gobj, data); break;
                    // TODO: case "show-on-enter-ar": ArenaUnity.ApplyShowOnEnterAr(gobj, data); break;
                    // TODO: case "show-on-enter-vr": ArenaUnity.ApplyShowOnEnterVr(gobj, data); break;
                    // TODO: case "impulse": ArenaUnity.ApplyImpulse(gobj, data); break;
                    // TODO: case "landmark": ArenaUnity.ApplyLandmark(gobj, data); break;
                    // TODO: case "material-extras": ArenaUnity.ApplyMaterialExtras(gobj, data); break;
                    // TODO: case "shadow": ArenaUnity.ApplyShadow(gobj, data); break;
                    // TODO: case "sound": ArenaUnity.ApplySound(gobj, data); break;
                    // TODO: case "textinput": ArenaUnity.ApplyTextInput(gobj, data); break;
                    // TODO: case "screenshareable": ArenaUnity.ApplyScreensharable(gobj, data); break;
                    // TODO: case "video-control": ArenaUnity.ApplyVideoControl(gobj, data); break;
                    case "attribution": ArenaUnity.ApplyAttribution(gobj, data); break;
                    // TODO: case "particle-system": ArenaUnity.ApplyParticleSystem(gobj, data); break;
                    // TODO: case "spe-particles": ArenaUnity.ApplySpeParticles(gobj, data); break;
                    // TODO: case "buffer": ArenaUnity.ApplyBuffer(gobj, data); break;
                    // TODO: case "jitsi-video": ArenaUnity.ApplyJitsiVideo(gobj, data); break;
                    // TODO: case "multisrc": ArenaUnity.ApplyMultiSrc(gobj, data); break;
                    // TODO: case "skipCache": ArenaUnity.ApplySkipCache(gobj, data); break;
                    case "animation-mixer": ArenaUnity.ApplyAnimationMixer(gobj, data); break;
                    // TODO: case "gltf-model-lod": ArenaUnity.ApplyGltfModelLod(gobj, data); break;
                    // TODO: case "modelUpdate": ArenaUnity.ApplyModelUpdate(gobj, data); break;
                    case "material":
                        ArenaUnity.ApplyMaterial(gobj, data);
                        if (!string.IsNullOrEmpty(data.material.Src))
                            AttachMaterialTexture(checkLocalAsset((string)data.material.Src), gobj);
                        break;
                }
            }
        }

        internal void AttachAvatar(string object_id, ArenaArenaUserJson json, GameObject gobj)
        {
            json.headModelPath ??= arenaDefaults.headModelPath;
            json.displayName ??= arenaDefaults.userName;

            bool worldPositionStays = false;
            if (json.headModelPath != null)
            {
                string localpath = checkLocalAsset(json.headModelPath);
                if (localpath != null)
                {
                    string headModelId = $"head-model-{object_id}";
                    Transform foundHeadModel = gobj.transform.Find(headModelId);
                    if (!foundHeadModel)
                    {
                        // add model child to camera
                        GameObject hmobj = new GameObject(headModelId);
                        AttachGltf(localpath, hmobj);
                        hmobj.transform.localPosition = Vector3.zero;
                        hmobj.transform.localRotation = Quaternion.Euler(0, 180f, 0);
                        hmobj.transform.localScale = Vector3.one;
                        // makes the child keep its local orientation rather than its global orientation
                        hmobj.transform.SetParent(gobj.transform, worldPositionStays);
                    }

                    string headTextId = $"headtext-{object_id}";
                    Transform foundHeadText = gobj.transform.Find(headTextId);
                    if (foundHeadText)
                    {
                        // update text
                        TextMeshPro tm = foundHeadText.GetComponent<TextMeshPro>();
                        tm.text = json.displayName;
                    }
                    else
                    {
                        // add text child to camera
                        GameObject htobj = new GameObject(headTextId);
                        TextMeshPro tm = htobj.transform.gameObject.AddComponent<TextMeshPro>();
                        tm.alignment = TextAlignmentOptions.Center;
                        tm.color = ArenaUnity.ToUnityColor((string)json.color);
                        tm.fontSize = 5;
                        tm.text = json.displayName;

                        htobj.transform.localPosition = new Vector3(0f, 0.45f, -0.05f);
                        htobj.transform.localRotation = Quaternion.Euler(0, 180f, 0);
                        htobj.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
                        // makes the child keep its local orientation rather than its global orientation
                        htobj.transform.SetParent(gobj.transform, worldPositionStays);
                    }
                }
            }
        }

        internal void AttachHand(string object_id, string url, GameObject gobj)
        {
            bool worldPositionStays = false;
            if (url != null)
            {
                string localpath = checkLocalAsset(url);
                if (localpath != null)
                {
                    string handModelId = $"hand-model-{object_id}";
                    Transform foundHandModel = gobj.transform.Find(handModelId);
                    if (!foundHandModel)
                    {
                        // add model child to hand
                        GameObject hmobj = new GameObject(handModelId);
                        AttachGltf(localpath, hmobj);
                        hmobj.transform.localPosition = Vector3.zero;
                        hmobj.transform.localRotation = Quaternion.identity;
                        hmobj.transform.localScale = Vector3.one;
                        // makes the child keep its local orientation rather than its global orientation
                        hmobj.transform.SetParent(gobj.transform, worldPositionStays);

                        if (drawControllerRays)
                        {
                            gobj.AddComponent<ArenaHand>();
                        }
                    }
                }
            }
        }

        internal string checkLocalAsset(string msgUrl)
        {
            Uri uri = ConstructRemoteUrl(msgUrl);
            if (uri == null) return null;
            string assetPath = ConstructLocalPath(uri);
            if (!File.Exists(assetPath)) return null;
            return assetPath;
        }

        private void AttachMaterialTexture(string assetPath, GameObject gobj)
        {
            if (assetPath == null) return;
            if (File.Exists(assetPath))
            {
                var bytes = File.ReadAllBytes(assetPath);
                var tex = new Texture2D(1, 1);
                tex.LoadImage(bytes);
                var renderer = gobj.GetComponent<Renderer>();
                if (renderer != null)
                    renderer.material.mainTexture = tex;
            }
        }

        private void AttachGltf(string assetPath, GameObject gobj, ArenaObject aobj = null)
        {
            if (assetPath == null) return;
            AnimationClip[] clips = null;
            GameObject mobj = null;
            var i = new ImportSettings();
            i.animationSettings.useLegacyClips = true;
            try
            {
                mobj = Importer.LoadFromFile(assetPath, i, out clips);
            }
            catch (Exception err)
            {
                Debug.LogWarning($"Unable to load GTLF at {assetPath}. {err.Message}");
            }
            if (mobj != null)
            {
                if (clips != null && aobj != null)
                    AssignAnimations(aobj, mobj, clips);
                mobj.transform.parent = gobj.transform;
                mobj.transform.localRotation = ArenaUnity.GltfToUnityRotationQuat(mobj.transform.localRotation);
                foreach (Transform child in mobj.transform.GetComponentsInChildren<Transform>())
                {   // prevent inadvertent editing of gltf elements
                    child.gameObject.isStatic = true;
                }
            }
        }

        private void AssignAnimations(ArenaObject aobj, GameObject mobj, AnimationClip[] clips)
        {
            if (clips != null && clips.Length > 0)
            {
                aobj.animations = new List<string>();
                Animation anim = mobj.AddComponent<Animation>();
                foreach (AnimationClip clip in clips)
                {
                    clip.legacy = true;
                    anim.AddClip(clip, clip.name);
                    //anim.clip = anim.GetClip(clip.name);
                    //anim.wrapMode = WrapMode.Loop;
                    aobj.animations.Add(clip.name);
                }
            }
        }

        private void AttachImage(string assetPath, GameObject gobj)
        {
            if (assetPath == null) return;
            Sprite sprite = LoadSpriteFromFile(assetPath);
            if (sprite != null)
            {
                SpriteRenderer spriteRenderer = gobj.AddComponent<SpriteRenderer>();
                spriteRenderer.GetComponent<SpriteRenderer>().sprite = sprite;
                spriteRenderer.drawMode = SpriteDrawMode.Sliced;
                spriteRenderer.size = Vector2.one;
            }
        }

        private static Sprite LoadSpriteFromFile(string assetPath)
        {
            if (assetPath == null) return null;
            Texture2D tex = new Texture2D(2, 2, TextureFormat.RGB24, false);
            tex.filterMode = FilterMode.Trilinear;
            var imgdata = File.ReadAllBytes(assetPath);
            tex.LoadImage(imgdata);
            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 1f, 1, SpriteMeshType.FullRect);
            return sprite;
        }

        private void RemoveObject(string object_id)
        {
            if (arenaObjs.TryGetValue(object_id, out GameObject gobj))
            {
                // recursively remove children if any
                foreach (Transform child in gobj.transform)
                {
                    RemoveObject(child.name);
                }
                // remove this
                ArenaObject aobj = gobj.GetComponent<ArenaObject>();
                aobj.externalDelete = true;
                Destroy(gobj);
            }
            arenaObjs.Remove(object_id);
        }

        private IEnumerator HttpRequestRaw(string url)
        {
            UnityWebRequest www = UnityWebRequest.Get(url);
            www.downloadHandler = new DownloadHandlerBuffer();
            //www.timeout = 5; // TODO (mwfarb): when fails like 443 hang, need to prevent curl 28 crash, this should just skip
            if (!verifyCertificate && url.StartsWith("https://localhost"))
            {   // TODO (mwfarb): should check for arena/not-arena host when debugging on localhost
                www.certificateHandler = new SelfSignedCertificateHandler();
            }
            www.SendWebRequest();
            while (!www.isDone)
            {
                DisplayCancelableProgressBar("ARENA URL", $"Downloading {url}", www.downloadProgress);
                yield return null;
            }
#if UNITY_2020_1_OR_NEWER
            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
#else
            if (www.isNetworkError || www.isHttpError)
#endif
            {
                Debug.LogWarning($"{www.error}: {www.url}");
                yield break;
            }
            else
            {
                byte[] results = www.downloadHandler.data;
                yield return results;
            }
        }

        //TODO (mwfarb): prevent publish and throw errors on publishing without rights

        /// <summary>
        /// Object changes are published using a ClientId + ObjectId topic, a user must have permissions for the entire scene graph.
        /// </summary>
        public void PublishObject(string object_id, string msgJson, bool hasPermissions = true)
        {
            ArenaObjectJson msg = JsonConvert.DeserializeObject<ArenaObjectJson>(msgJson);
            msg.timestamp = GetTimestamp();
            PublishSceneMessage($"{sceneTopic}/{client.ClientId}/{object_id}", msg, hasPermissions);
        }

        /// <summary>
        /// Camera presence changes are published using a ObjectId-only topic, a user might only have permissions for their camid.
        /// </summary>
        public void PublishCamera(string object_id, string msgJson, bool hasPermissions = true)
        {
            ArenaObjectJson msg = JsonConvert.DeserializeObject<ArenaObjectJson>(msgJson);
            msg.timestamp = GetTimestamp();
            PublishSceneMessage($"{sceneTopic}/{object_id}", msg, hasPermissions);
        }

        /// <summary>
        /// Camera events are published using a ObjectId-only topic, a user might only have permissions for their camid.
        /// </summary>
        public void PublishEvent(string object_id, string eventType, string source, string msgJsonData, bool hasPermissions = true)
        {
            ArenaObjectJson msg = new ArenaObjectJson
            {
                object_id = object_id,
                action = "clientEvent",
                type = eventType,
                data = JsonConvert.DeserializeObject(msgJsonData),
            };
            msg.timestamp = GetTimestamp();
            PublishSceneMessage($"{sceneTopic}/{source}", msg, hasPermissions);
        }

        private void PublishSceneMessage(string topic, ArenaObjectJson msg, bool hasPermissions)
        {
            byte[] payload = System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(msg));
            Publish(topic, payload); // remote
            LogMessage("Sending", msg, hasPermissions);
        }

        private static string GetTimestamp()
        {   // o Format Specifier 2008-10-31T17:04:32.0000000Z
            return DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture);
        }

        protected override void OnConnected()
        {
            base.OnConnected();
            Subscribe(new string[] { $"{sceneTopic}/#" });
            name = $"{originalName} (MQTT Connected)";
        }

        protected override void OnDisconnected()
        {
            base.OnDisconnected();
            name = $"{originalName} (MQTT Disconnected)";
        }

        protected override void DecodeMessage(string topic, byte[] message)
        {
            // Call the delegate if a user has defined it
            if (OnMessageCallback != null) OnMessageCallback(topic, message);
            ProcessMessage(message);
        }

        internal void ProcessMessage(string message, object menuCommand = null)
        {
            byte[] payload = System.Text.Encoding.UTF8.GetBytes(message);
            ProcessMessage(payload);
        }

        internal void ProcessMessage(byte[] message, object menuCommand = null)
        {
            string msgJson = System.Text.Encoding.UTF8.GetString(message);
            ArenaObjectJson msg = JsonConvert.DeserializeObject<ArenaObjectJson>(msgJson);
            LogMessage("Received", msg);
            StartCoroutine(ProcessArenaMessage(msg, menuCommand));
        }

        private IEnumerator ProcessArenaMessage(ArenaObjectJson msg, object menuCommand = null)
        {
            CoroutineWithData cd;
            // consume object updates
            if (!localCameraIds.Contains((string)msg.object_id))
            {
                string object_id;
                switch ((string)msg.action)
                {
                    case "create":
                    case "update":
                        object_id = (string)msg.object_id;
                        string msg_type = (string)msg.type;
                        float ttl = (msg.ttl != null) ? (float)msg.ttl : 0f;
                        bool persist = Convert.ToBoolean(msg.persist);

                        // there isnt already an object in the scene created by the user with the same object_id
                        if (GameObject.Find((string)object_id) == null)
                        {
                            IEnumerable<string> uris = ExtractAssetUris(msg.data, msgUriTags);
                            foreach (var uri in uris)
                            {
                                if (!string.IsNullOrWhiteSpace(uri))
                                {
                                    cd = new CoroutineWithData(this, DownloadAssets(msg_type, uri));
                                    yield return cd.coroutine;
                                }
                            }
                        }
                        CreateUpdateObject(msg, msg.data, menuCommand);
                        break;
                    case "delete":
                        object_id = (string)msg.object_id;
                        RemoveObject(object_id);
                        // camera special case, look for hands to delete
                        if (object_id.StartsWith(prefixCam))
                        {
                            string hand_left_id = $"{prefixHandL}{object_id.Substring(prefixCam.Length)}";
                            string hand_right_id = $"{prefixHandR}{object_id.Substring(prefixCam.Length)}";
                            RemoveObject(hand_left_id);
                            RemoveObject(hand_right_id);
                        }
                        break;
                    case "clientEvent":
                        object_id = (string)msg.object_id;
                        msg_type = (string)msg.type;
                        ClientEventOnObject(object_id, msg_type, JsonConvert.SerializeObject(msg));
                        break;
                    default:
                        break;
                }
                yield break;
            }
        }

        private void ClientEventOnObject(string object_id, string msg_type, string msg)
        {
            if (arenaObjs.TryGetValue(object_id, out GameObject gobj))
            {
                // pass event on to click-listener is defined
                ArenaClickListener acl = gobj.GetComponent<ArenaClickListener>();
                if (acl != null && acl.OnEventCallback != null) acl.OnEventCallback(msg_type, msg);
            }
        }

        private void LogMessage(string dir, ArenaObjectJson msg, bool hasPermissions = true)
        {
            // determine logging level
            if (!Convert.ToBoolean(msg.persist) && !logMqttNonPersist) return;
            if (msg.type == "object")
            {
                if (msg.object_id.StartsWith("camera_") && !logMqttUsers) return;
                if (!logMqttObjects) return;
            }
            if (msg.action == "clientEvent" && !logMqttEvents) return;
            if (hasPermissions)
                Debug.Log($"{dir}: {JsonConvert.SerializeObject(msg)}");
            else
                Debug.LogWarning($"Permissions FAILED {dir}: {JsonConvert.SerializeObject(msg)}");
        }

        protected override void OnApplicationQuit()
        {
            // send delete of local avatars before connection closes
            foreach (var camid in localCameraIds)
            {
                ArenaObjectJson msg = new ArenaObjectJson
                {
                    object_id = camid,
                    action = "delete",
                };
                string delCamMsg = JsonConvert.SerializeObject(msg);
                PublishCamera(camid, delCamMsg, sceneObjectRights);
            }
            base.OnApplicationQuit();
        }
    }
}
