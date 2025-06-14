/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2023, Carnegie Mellon University. All rights reserved.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ArenaUnity.Components;
using ArenaUnity.Schemas;
using GLTFast;
using GLTFast.Export;
using GLTFast.Logging;
using MimeMapping;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using uPLibrary.Networking.M2Mqtt.Messages;

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

        public string realm { get; private set; }

        [Header("Presence")]
        [Tooltip("Display other camera avatars in the scene")]
        public bool renderCameras = true;
        [Tooltip("Display other avatars' hand controllers in the scene")]
        public bool renderHands = true;
        [Tooltip("Display VR Controller Rays")]
        public bool drawControllerRays = false;

        /// <summary>
        /// Browser URL for the scene.
        /// </summary>
        public string sceneUrl { get; private set; }

        public bool sceneObjectRights { get; private set; } = false;

        public bool persistLoaded { get; private set; } = false;

        private ArenaTopics sceneTopic;
        public Dictionary<string, GameObject> arenaObjs { get; private set; } = new Dictionary<string, GameObject>();
        internal Dictionary<string, GameObject> childObjs = new Dictionary<string, GameObject>();
        internal List<string> pendingDelete = new List<string>();
        internal List<string> downloadQueue = new List<string>();
        internal List<string> parentalQueue = new List<string>();
        internal List<string> localCameraIds = new List<string>();
        internal ArenaDefaultsJson arenaDefaults { get; private set; }

        // Define callbacks
        public delegate void DecodeMessageDelegate(string topic, string message);
        public DecodeMessageDelegate OnMessageCallback = null; // null, until library user instantiates.

        public string originalName { get; private set; }

        static string importPath = null;
        const string prefixHandL = "handLeft_";
        const string prefixHandR = "handRight_";
        static readonly List<string> gltfTypeList = new List<string> { "gltf-model", "handLeft", "handRight" };
        static readonly string[] msgUriTags = { "url", "src", "obj", "mtl", "overrideSrc", "detailedUrl", "headModelPath", "texture", "navMesh" };
        static readonly string[] gltfUriTags = { "uri" };
        static readonly string[] skipMimeClasses = { "video", "audio" };
        static readonly string[] requiredShadersStandardRP = {
            "Standard",
            "Unlit/Color",
            "glTF/PbrMetallicRoughness",
            "glTF/PbrSpecularGlossiness",
            "glTF/Unlit",
#if UNITY_EDITOR
            "Hidden/glTFExportMaskMap",
            "Hidden/glTFExportNormal",
            "Hidden/glTFExportSmoothness",
            "Hidden/glTFExportMetalGloss",
            "Hidden/glTFExportOcclusion",
            "Hidden/glTFExportColor",
#endif
        };
        static readonly string[] requiredShadersURPHDRP = {
            // "Lit",
            "Unlit/Color",
            "glTF/PbrMetallicRoughness",
            "glTF/PbrSpecularGlossiness",
            "glTF/Unlit",
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
            if (ArenaUnity.DefaultRenderPipeline)
                requiredShaders = requiredShadersURPHDRP;

            // ensure shaders are in project
            foreach (string shader in requiredShaders)
            {
                if (Shader.Find(shader) == null)
                {
                    LogAndExit($"Required shader '{shader}' not found in project.");
                }
            }
            //Shader.WarmupAllShaders();

#if UNITY_EDITOR
            // Editor use can change the Auth Method, Hostname, Namespace, Scene in Inspector easily, however
            // built apps (non-editor) want to change those parameters in UIs/UX processes they create.
            // So prevent non-editor builds from connecting automatically, so developers can allow user edits.
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
            realm = arenaDefaults.realm;

            // start auth flow and MQTT connection
            ArenaCamera[] camlist = FindObjectsOfType<ArenaCamera>();
            name = $"{originalName} (Authenticating...)";
            cd = new CoroutineWithData(this, SigninScene(sceneName, namespaceName, realm, camlist.Length > 0, arenaDefaults.latencyTopic));
            yield return cd.coroutine;
            name = $"{originalName} (MQTT Connecting...)";
            if (cd.result != null)
            {
                if (string.IsNullOrWhiteSpace(namespaceName)) namespaceName = cd.result.ToString();
                sceneTopic = new ArenaTopics(
                    realm: realm,
                    name_space: namespaceName,
                    scenename: sceneName,
                    userclient: userclient,
                    idtag: userid,
                    userobj: camid
                );
                sceneUrl = $"https://{hostAddress}/{namespaceName}/{sceneName}";
            }
            if (permissions == null)
            {   // fail when permissions not set
                LogAndExit("Permissions not received.");
                yield break;
            }
            sceneObjectRights = HasPerms(sceneTopic.PUB_SCENE_OBJECTS);
            // publish arena cameras where requested
            bool foundFirstCam = false;
            foreach (ArenaCamera cam in camlist)
            {
                if ((Camera.main != null && cam.name == Camera.main.name || camlist.Length == 1) && !foundFirstCam)
                {
                    // publish main/selected camera
                    cam.userid = userid;
                    cam.camid = camid;
                    foundFirstCam = true;
                }
                else
                {
                    // TODO (mwfarb): fix: other cameras are auto-generated, and account must have all scene rights
                    if (!sceneObjectRights)
                    {
                        Debug.LogWarning($"Using more than one ArenaCamera requires full scene permissions. Only one camera will be published.");
                    }
                    var random = UnityEngine.Random.Range(0, 100000000);
                    cam.userid = $"unpublished-unity_{random:D8}";
                    cam.camid = $"cam-unpublished-unity_{random:D8}";
                }
                var camTopic = new ArenaTopics(
                    realm: realm,
                    name_space: namespaceName,
                    scenename: sceneName,
                    userclient: userclient,
                    idtag: cam.userid,
                    userobj: cam.camid
                );
                cam.HasPermissions = HasPerms(camTopic.PUB_SCENE_USER);
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
                        PublishObject(msg.object_id, payload);
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
                    IEnumerable<string> uris = ExtractAssetUris(msg.attributes, msgUriTags);
                    foreach (var uri in uris)
                    {
                        if (!string.IsNullOrWhiteSpace(uri))
                        {
                            cd = new CoroutineWithData(this, DownloadAssets(msg_type, uri));
                            yield return cd.coroutine;
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


        internal string checkLocalAsset(string msgUrl)
        {
            Uri uri = ConstructRemoteUrl(msgUrl);
            if (uri == null) return null;
            string assetPath = ConstructLocalPath(uri);
            if (!File.Exists(assetPath)) return null;
            return assetPath;
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
            arenaObjs.TryGetValue(msg.object_id, out GameObject gobj);
            if (gobj != null)
            {   // update local
                aobj = gobj.GetComponent<ArenaObject>();
            }
            else
            {   // create local
#if !UNITY_EDITOR
                Debug.Log($"Loading object '{msg.object_id}'..."); // show new objects in log
#endif
                gobj = new GameObject();
                gobj.name = msg.object_id;
                aobj = gobj.AddComponent(typeof(ArenaObject)) as ArenaObject;
                arenaObjs[msg.object_id] = gobj;
                aobj.Created = true;
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

            // apply storage-level object properties
            if (msg.persist.HasValue)
                aobj.persist = (bool)msg.persist;
            aobj.messageType = msg.type;
            if (msg.ttl != null)
            {
                if (!gobj.TryGetComponent<ArenaTtl>(out var ttl))
                    ttl = gobj.AddComponent<ArenaTtl>();
                ttl.SetTtlDeleteTimer((float)msg.ttl);
            }

            JObject jData = JObject.Parse(JsonConvert.SerializeObject(indata));
            //new JsonSerializerSettings
            //{
            //    Error = (sender, args) => {
            //        Debug.LogWarning($"{args.ErrorContext.Error.Message}: {args.ErrorContext.OriginalObject}");
            //        args.ErrorContext.Handled = true;
            //    },
            //});

            if (aobj == null) Debug.LogError($"invalid aobj from {JsonConvert.SerializeObject(indata)}");

            switch (msg.type)
            {
                case "object":
                    UpdateObjectMessage(msg, indata, aobj, gobj, jData);
                    break;
                case "scene-options":
                    UpdateSceneOptionsMessage(indata, aobj, gobj, jData);
                    break;
                case "program":
                    // TODO (mwfarb): define program implementation or lack thereof
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

        private void UpdateSceneOptionsMessage(object indata, ArenaObject aobj, GameObject gobj, JObject jData)
        {
            ArenaArenaSceneOptionsJson data = JsonConvert.DeserializeObject<ArenaArenaSceneOptionsJson>(indata.ToString());

            // handle scene options attributes
            foreach (var result in jData)
            {
                switch (result.Key)
                {
                    case "env-presets": ArenaUnity.ApplyEnvironmentPresets(gobj, data); break;
                    case "scene-options":
                        if (!string.IsNullOrWhiteSpace(data.SceneOptions?.NavMesh))
                        {
                            // Attach navMesh model, to scene-options object
                            var url = data.SceneOptions.NavMesh;
                            string assetPath = checkLocalAsset(url);
                            if (assetPath != null)
                            {
                                aobj.gltfUrl = url;
                                AttachGltf(assetPath, gobj, aobj);
                            }
                        }
                        ArenaUnity.ApplySceneOptions(gobj, data); break;
                    case "renderer-settings": ArenaUnity.ApplyRendererSettings(gobj, data); break;
                    case "post-processing": ArenaUnity.ApplyPostProcessing(gobj, data); break;
                }
            }
        }

        private void UpdateObjectMessage(ArenaObjectJson msg, object indata, ArenaObject aobj, GameObject gobj, JObject jData)
        {
            ArenaDataJson data = JsonConvert.DeserializeObject<ArenaDataJson>(indata.ToString());

            // modify Unity attributes
            bool worldPositionStays = false; // default: most children need relative position
            aobj.parentId = (string)data.Parent;
            string parent = (string)data.Parent;
            string object_type = (string)data.object_type;
            aobj.object_type = object_type;
            var url = !string.IsNullOrEmpty(data.Src) ? data.Src : data.Url;

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

                // ARENAUI objects
                // TODO:
                case "arenaui-card": ArenaUnity.ApplyWireArenauiCard(indata, gobj); break;
                // TODO:
                case "arenaui-button-panel": ArenaUnity.ApplyWireArenauiButtonPanel(indata, gobj); break;
                // TODO:
                case "arenaui-prompt": ArenaUnity.ApplyWireArenauiPrompt(indata, gobj); break;


                // TODO: case "ocean": ArenaUnity.ApplyWireOcean(indata, gobj); break;
                // TODO: case "pcd-model": ArenaUnity.ApplyWirePcdModel(indata, gobj); break;
                // TODO: case "threejs-scene": ArenaUnity.ApplyWireThreejsScene(indata, gobj); break;
                case "gaussian_splatting":
                    ArenaUnity.ApplyWireGaussianSplatting(indata, gobj);
                    break;
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
                        // TODO: cam.nearClipPlane = 0.005f; // match arena
                        cam.nearClipPlane = 0.1f; // move near clip out since local cam hard to see in model
                        cam.farClipPlane = 10000f; // match arena
                        cam.fieldOfView = 80f; // match arena
                        cam.targetDisplay = 8; // render on least-used display
                        var json = new ArenaCameraJson();
                        json = JsonConvert.DeserializeObject<ArenaCameraJson>(ArenaUnity.MergeRawJson(json, data));
                        AttachAvatar(msg.object_id, json.ArenaUser, gobj);
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
            if (data.Position != null)
            {
                gobj.transform.localPosition = ArenaUnity.ToUnityPosition(data.Position);
            }
            if (data.Rotation != null)
            {
                bool invertY = true;
                if (jData.SelectToken("rotation.w") != null) // quaternion
                    gobj.transform.localRotation = ArenaUnity.ToUnityRotationQuat(data.Rotation, invertY);
                else // euler
                    gobj.transform.localRotation = ArenaUnity.ToUnityRotationEuler(data.Rotation, invertY);
            }
            if (data.Scale != null)
            {
                gobj.transform.localScale = ArenaUnity.ToUnityScale(data.Scale);
            }

            // apply rendering visibility attributes, before other on-wire object attributes
            if (data.Visible != null) // visible, if set is highest priority to enable/disable renderer
            {
                ArenaUnity.ApplyVisible(gobj, data);
            }
            else if (data.RemoteRender != null) // remote-render, if set is lowest priority to enable/disable renderer
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
                            data.Material ??= new ArenaMaterialJson();
                            data.Material.Color = data.Color;
                            ArenaUnity.ApplyMaterial(gobj, data);
                        }
                        break;
                    case "light":
                        if (object_type == "light")
                        {
                            Debug.LogWarning($"data.light.[property] is deprecated for object-id: {msg.object_id}, object_type: {object_type}, use data.[property] instead.");
                            ArenaUnity.ApplyWireLight(data.Light, gobj);
                        }
                        break;
                    case "text":
                        if (object_type == "text")
                        {
                            Debug.LogWarning($"data.text is deprecated for object-id: {msg.object_id}, object_type: {object_type}, use data.value instead.");
                        }
                        break;

                    // expected attributes
                    // TODO: case "animation": ArenaUnity.ApplyAnimation(gobj, data); break;
                    case "armarker": ArenaUnity.ApplyArmarker(gobj, data); break;
                    case "click-listener": ArenaUnity.ApplyClickListener(gobj, data); break;
                    // TODO: case "box-collision-listener": ArenaUnity.ApplyBoxCollisionListener(gobj, data); break;
                    // TODO: case "collision-listener": ArenaUnity.ApplyCollisionListener(gobj, data); break;
                    // TODO: case "blip": ArenaUnity.ApplyBlip(gobj, data); break;
                    // TODO: case "dynamic-body": ArenaUnity.ApplyDynamicBody(gobj, data); break;
                    case "geometry":
                        if (object_type == "entity")
                            ArenaUnity.ApplyGeometry(null, data.Geometry, gobj); break;
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
                        if (!string.IsNullOrEmpty(data.Material.Src))
                            AttachMaterialTexture(checkLocalAsset((string)data.Material.Src), gobj);
                        break;
                }
            }
        }

        // TODO (mwfarb): move AttachAvatar to arena-camera component
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
                        tm.color = ArenaUnity.ToUnityColor(json.color);
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

        // TODO (mwfarb): move AttachHand to arena-hand component
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

        // TODO (mwfarb): move AttachMaterialTexture to material component
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

        // TODO (mwfarb): move AttachGltf to gltf-model component
        private async void AttachGltf(string assetPath, GameObject gobj, ArenaObject aobj = null)
        {
            if (assetPath == null) return;
            AnimationClip[] clips = null;
            GameObject mobj = null;
            var imSet = new ImportSettings
            {
                AnimationMethod = AnimationMethod.Legacy
            };
            var gltf = new GltfImport();
            Uri uri = new Uri(Path.GetFullPath(assetPath));
            if (await gltf.LoadFile(assetPath, uri, imSet))
            {
                clips = gltf.GetAnimationClips();
                if (clips != null && aobj != null)
                {   // save animation names for easy animation-mixer reference at runtime
                    aobj.animations = new List<string>();
                    foreach (AnimationClip clip in clips)
                    {
                        aobj.animations.Add(clip.name);
                    }
                }
                var inSet = new InstantiationSettings
                {
                    SceneObjectCreation = SceneObjectCreation.Always
                };
                var instantiator = new GameObjectInstantiator(gltf, gobj.transform, logger: new ConsoleLogger(), inSet);
                if (await gltf.InstantiateSceneAsync(instantiator))
                {
                    mobj = gobj.transform.GetChild(0).gameObject; // TODO (mwfarb): find better child method

                    // TODO (mwfarb): find a better way to chain commponent dependancies than this
                    var am = gobj.GetComponent<ArenaAnimationMixer>();
                    if (am != null)
                    {
                        am.apply = true;
                    }
                }
            }
            else
            {
                Debug.LogWarning($"Unable to load GTLF at {assetPath}.");
            }
            if (mobj != null)
            {
                mobj.transform.localRotation = ArenaUnity.GltfToUnityRotationQuat(mobj.transform.localRotation);
                foreach (Transform child in mobj.transform.GetComponentsInChildren<Transform>())
                {   // prevent inadvertent editing of gltf elements
                    child.gameObject.isStatic = true;
                }
            }
        }

        // TODO (mwfarb): move AttachImage to image component
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

        // TODO (mwfarb): move AssignImage to image component
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
            // try to get internal list ref
            if (!arenaObjs.TryGetValue(object_id, out GameObject gobj))
            {
                // try to get go ref
                gobj = GameObject.Find(object_id);
            }
            if (gobj == null) return;

            // recursively remove children if any of go
            foreach (Transform child in gobj.transform)
            {
                RemoveObject(child.name);
            }
            // remove go
            if (gobj.TryGetComponent<ArenaObject>(out var aobj))
            {
                aobj.externalDelete = true;
            }
            Destroy(gobj);
            // remove internal list ref
            arenaObjs.Remove(object_id);
        }

        private IEnumerator HttpRequestRaw(string url)
        {
            Uri uri = new Uri(url);
            UnityWebRequest www = UnityWebRequest.Get(url);
            www.downloadHandler = new DownloadHandlerBuffer();
            //www.timeout = 5; // TODO (mwfarb): when fails like 443 hang, need to prevent curl 28 crash, this should just skip
            if (!verifyCertificate)
            {
                www.certificateHandler = new SelfSignedCertificateHandler();
            }
            www.SendWebRequest();
            while (!www.isDone)
            {
                DisplayCancelableProgressBar("ARENA", $"Downloading {uri.Segments[uri.Segments.Length - 1]}...", www.downloadProgress);
                yield return null;
            }
            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogWarning($"{www.error}: {www.url}");
                ClearProgressBar();
                yield break;
            }
            else
            {
                byte[] results = www.downloadHandler.data;
                yield return results;
            }
            ClearProgressBar();
        }

        private IEnumerator HttpUploadFSRaw(string url, byte[] payload)
        {
            Uri uri = new Uri(url);
            UnityWebRequest www = new UnityWebRequest(url, "POST");
            //www.timeout = 5; // TODO (mwfarb): when fails like 443 hang, need to prevent curl 28 crash, this should just skip
            if (!verifyCertificate)
            {
                www.certificateHandler = new SelfSignedCertificateHandler();
            }
            www.SetRequestHeader("X-Auth", fsToken);
            UploadHandler uploadHandler = new UploadHandlerRaw(payload);
            www.uploadHandler = uploadHandler;
            www.SendWebRequest();
            while (!www.isDone)
            {
                DisplayCancelableProgressBar("ARENA", $"Uploading {uri.Segments[uri.Segments.Length - 1]}...", www.uploadProgress);
                yield return null;
            }
            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogWarning($"{www.error}: {www.url}");
                ClearProgressBar();
                yield break;
            }
            else
            {
                yield return true;
            }
            ClearProgressBar();
        }

        public void ExportGLTFBinaryStream(string name, GameObject[] gameObjects, ExportSettings exportSettings = null, GameObjectExportSettings goeSettings = null)
        {
            exportSettings ??= new ExportSettings { };
            goeSettings ??= new GameObjectExportSettings { };

            // ARENA upload only supports GLB formats ATM
            exportSettings.Format = GltfFormat.Binary;

            bool success = true;
            foreach (var go in gameObjects)
            {
                if (go.GetComponents<ArenaObject>().Length > 0)
                {
                    success = false;
                    Debug.LogWarning($"GLTF export ignored for existing ArenaObject component {name}.");
                }
                if (go.GetComponents<ArenaCamera>().Length > 0)
                {
                    success = false;
                    Debug.LogWarning($"GLTF export ignored for existing ArenaCamera component {name}.");
                }
                if (go.GetComponents<ArenaClientScene>().Length > 0)
                {
                    success = false;
                    Debug.LogWarning($"GLTF export ignored for existing ArenaClientScene component {name}.");
                }
            }
            if (success)
            {
                StartCoroutine(ExportGLTF(name, gameObjects, exportSettings, goeSettings));
            }
        }

        private IEnumerator ExportGLTF(string name, GameObject[] gameObjects, ExportSettings exportSettings, GameObjectExportSettings goeSettings)
        {
            CoroutineWithData cd;

            // request FS login if this is the first time.
            if (fsToken == null)
            {
                cd = new CoroutineWithData(this, GetFSLoginForUser());
                yield return cd.coroutine;
                if (!isCrdSuccess(cd.result))
                {
                    Debug.LogError($"Filestore login failed!");
                    yield break;
                }
            }

            // TODO (mwfarb): find better way to export with local position/rotation
            // determine if we should update the local position
            var rootObjPos = gameObjects[0].transform.position;

            // ATM glTFast exports models with world origin, so we do a trick for now,
            // by moving the model to the desired export position, export, and then return it.

            // move single model to desired export translation
            gameObjects[0].transform.position = Vector3.zero;

            // export gltf to stream
            var export = new GameObjectExport(exportSettings, goeSettings, logger: new ConsoleLogger());
            export.AddScene(gameObjects, name);

            MemoryStream stream = new MemoryStream();
            var exportTask = export.SaveToStreamAndDispose(stream);
            yield return new WaitUntil(() => exportTask.IsCompleted);
            if (!exportTask.Result)
            {
                Debug.LogError($"GLTF export to stream failed!");
                yield break;
            }

            // return single model to original export translation
            gameObjects[0].transform.position = rootObjPos;

            var destFilePath = $"{name}.glb";
            byte[] fileBuffer = stream.GetBuffer();

            // send stream to filestore
            cd = new CoroutineWithData(this, UploadStoreFile(fileBuffer, destFilePath));
            yield return cd.coroutine;
            var storeExtPath = (string)cd.result;
            if (string.IsNullOrEmpty(storeExtPath)) yield break;

            // send scene object metadata to MQTT
            ArenaObjectJson msg = new ArenaObjectJson
            {
                object_id = name,
                action = "create",
                type = "object",
                persist = true,
                data = new ArenaDataJson
                {
                    object_type = "gltf-model",
                    Url = storeExtPath,
                    Position = ArenaUnity.ToArenaPosition(rootObjPos),
                    Rotation = ArenaUnity.ToArenaRotationQuat(
                        ArenaUnity.GltfToUnityRotationQuat(Quaternion.identity)),
                }
            };
            string payload = JsonConvert.SerializeObject(msg);
            PublishObject(msg.object_id, payload);
            yield return true;
        }

        /// <summary>
        /// Upload a file to the filestore using the user's Google account.
        /// </summary>
        /// <param name="srcFilePath">Local path to the file to upload.</param>
        /// <param name="destFilePath">Destination file path, can include dirs. Defaults to filename from srcFilePath.</param>
        /// <returns></returns>
        public IEnumerator UploadStoreFile(string srcFilePath, string destFilePath=null)
        {
            if (destFilePath == null)
                destFilePath = Path.GetFileName(srcFilePath);
            byte[] fileBuffer = File.ReadAllBytes(srcFilePath);
            var cd = new CoroutineWithData(this, UploadStoreFile(fileBuffer, destFilePath));
            yield return cd.result;
        }

        /// <summary>
        /// Upload a file to the filestore using the user's Google account.
        /// </summary>
        /// <param name="fileBuffer">Binary buffer of file.</param>
        /// <param name="destFilePath">Destination file path, can include dirs.</param>
        /// <returns></returns>
        public IEnumerator UploadStoreFile(byte[] fileBuffer, string destFilePath)
        {
            // confirm user google account is loaded
            if (!ConfirmGoogleAuth())
            {
                // TODO(mwfarb): Add note when headless auth options are available.
                Debug.LogError($"Google auth is required. Remove manual .arena_mqtt_token.");
                yield break;
            }
            var safeFilename = Regex.Replace(destFilePath, @"/(\W+)/gi", "-");
            var storeResPrefix = authState.is_staff ? $"users/{mqttUserName}/" : "";
            var userFilePath = $"scenes/{sceneName}/{safeFilename}";
            var storeResPath = $"{storeResPrefix}{userFilePath}";
            var storeExtPath = $"store/users/{mqttUserName}/{userFilePath}";

            string uploadUrl = $"https://{hostAddress}/storemng/api/resources/{storeResPath}?override=true";
            var cd = new CoroutineWithData(this, HttpUploadFSRaw(uploadUrl, fileBuffer));
            yield return cd.coroutine;
            if (!isCrdSuccess(cd.result))
            {
                Debug.LogError($"Filestore file upload failed!");
                yield break;
            }
            var url = $"https://{hostAddress}/{storeExtPath}";
            Debug.Log($"Filestore file uploaded to {url}");
            yield return url;
        }

        /// <summary>
        /// Object changes are published using a ClientId + ObjectId topic, a user must have permissions for the entire scene graph.
        /// </summary>
        /// <param name="object_id">The object id.</param>
        /// <param name="msgJson">The wire-format JSON string paylod for the MQTT message.</param>
        /// <param name="toUserId">The user id to send this message to, if private (optional).</param>
        public void PublishObject(string object_id, string msgJson, string toUserId = null)
        {
            ArenaObjectJson msg = JsonConvert.DeserializeObject<ArenaObjectJson>(msgJson);
            msg.timestamp = GetTimestamp();
            var objTopic = new ArenaTopics(
                realm: sceneTopic.REALM,
                name_space: sceneTopic.nameSpace,
                scenename: sceneTopic.sceneName,
                userclient: userclient,
                objectid: object_id,
                touid: toUserId
            );
            if (toUserId == null)
                PublishSceneMessage(objTopic.PUB_SCENE_OBJECTS, msg);
            else
                PublishSceneMessage(objTopic.PUB_SCENE_OBJECTS_PRIVATE, msg);
        }

        /// <summary>
        /// Camera presence changes are published using a ObjectId-only topic, a user might only have permissions for their camid.
        /// </summary>
        /// <param name="object_id">The user id.</param>
        /// <param name="msgJson">The wire-format JSON string paylod for the MQTT message.</param>
        /// <param name="toUserId">The user id to send this message to, if private (optional).</param>
        public void PublishCamera(string object_id, string msgJson, string toUserId = null)
        {
            ArenaObjectJson msg = JsonConvert.DeserializeObject<ArenaObjectJson>(msgJson);
            msg.timestamp = GetTimestamp();
            var camTopic = new ArenaTopics(
                realm: sceneTopic.REALM,
                name_space: sceneTopic.nameSpace,
                scenename: sceneTopic.sceneName,
                userclient: userclient,
                userobj: object_id,
                touid: toUserId
            );
            if (toUserId == null)
                PublishSceneMessage(camTopic.PUB_SCENE_USER, msg);
            else
                PublishSceneMessage(camTopic.PUB_SCENE_USER_PRIVATE, msg);
        }

        [Obsolete("PublishEvent message signature has changed. Instreand of object_id, include data.target in msgJsonData.")]
        public void PublishEvent(string object_id, string eventType, string source, string msgJsonData, bool hasPermissions = true)
        {
            PublishEvent(eventType, source, msgJsonData);
        }

        /// <summary>
        /// Camera events are published using a ObjectId-only topic, a user might only have permissions for their camid.
        /// </summary>
        /// <param name="eventType"The type of event message, e.g. "mousedown".</param>
        /// <param name="source">The user id the event is from.</param>
        /// <param name="msgJsonData">The wire-format JSON string paylod for the MQTT message, data section only.</param>
        /// <param name="toUserId">The user id to send this message to, if private (optional).</param>
        public void PublishEvent(string eventType, string source, string msgJsonData, string toUserId = null)
        {
            ArenaObjectJson msg = new ArenaObjectJson
            {
                object_id = source,
                action = "clientEvent",
                type = eventType,
                data = JsonConvert.DeserializeObject(msgJsonData),
            };
            msg.timestamp = GetTimestamp();
            var evtTopic = new ArenaTopics(
                realm: sceneTopic.REALM,
                name_space: sceneTopic.nameSpace,
                scenename: sceneTopic.sceneName,
                userclient: userclient,
                userobj: source,
                touid: toUserId
            );
            if (toUserId == null)
                PublishSceneMessage(evtTopic.PUB_SCENE_USER, msg);
            else
                PublishSceneMessage(evtTopic.PUB_SCENE_USER_PRIVATE, msg);
        }

        /// <summary>
        /// Egress point for messages to send to remote graph scenes.
        /// </summary>
        private void PublishSceneMessage(string topic, ArenaObjectJson msg)
        {
            byte[] payload = System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(msg));
            Publish(topic, payload); // remote
        }

        private static string GetTimestamp()
        {   // o Format Specifier 2008-10-31T17:04:32.0000000Z
            return DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture);
        }

        protected override void OnConnected()
        {
            base.OnConnected();
            name = $"{originalName} (MQTT Connected)";
            var subTopic = new ArenaTopics(
                realm: realm,
                name_space: namespaceName,
                scenename: sceneName,
                idtag: userid
            );
            client.MqttMsgSubscribed += OnSubscribed;
            Subscribe(subTopic.SUB_SCENE_PUBLIC);
            Subscribe(subTopic.SUB_SCENE_PRIVATE);
        }

        private void OnSubscribed(object sender, MqttMsgSubscribedEventArgs e)
        {
            var validQos = new byte[] { MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE, MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE };
            if (e.GrantedQoSLevels.Any(el => validQos.Contains(el)))
            {
                Debug.Log($"Subscribed to: {subscriptions[e.MessageId]}");
            }
            else
            {
                Debug.LogWarning($"Subscribed FAILED to: {subscriptions[e.MessageId]}");
            }
        }

        protected override void OnDisconnected()
        {
            base.OnDisconnected();
            name = $"{originalName} (MQTT Disconnected)";
        }

        protected override void DecodeMessage(string topic, byte[] message)
        {
            string msgJson = System.Text.Encoding.UTF8.GetString(message);
            ProcessMessage(topic, msgJson);
        }

        /// <summary>
        /// Ingest point for messages to be recieved in the local scene.
        /// </summary>
        /// <param name="topic">The MQTT topic.</param>
        /// <param name="message">The JSON ARENA wire format string.</param>
        public void ProcessMessage(string topic, string message)
        {
            // Call the delegate if a user has defined it
            OnMessageCallback?.Invoke(topic, message);

            // TODO (mwfarb): perform any message validation here based on topic

            // TODO (mwfarb): test that ignore own messages is still viable
            // filter messages based on expected payload format
            var topicSplit = topic.Split('/');
            if (topicSplit.Length > ArenaMqttClient.msgTypeRenderIdx)
            {
                var sceneMsgType = topicSplit[ArenaMqttClient.msgTypeRenderIdx];
                LogMessage("Received", sceneMsgType, topic, message);
                switch (sceneMsgType)
                {
                    case "x":
                    case "u":
                    case "o":
                        // handle scene objects, user objects, user presense
                        ArenaObjectJson msg = JsonConvert.DeserializeObject<ArenaObjectJson>(message);
                        StartCoroutine(ProcessArenaMessage(msg));
                        break;
                    case "r": // remote render handled by arena-renderfusion package currently
                    case "c": // chat not implemented in unity currently
                    case "p": // program not implemented in unity currently
                    case "e": // environment not implemented in unity currently
                    case "d": // debug not implemented in unity currently
                        break;
                    default:
                        // ????
                        Debug.LogWarning($"Invalid scene message type: {sceneMsgType}");
                        break;
                }
            }
        }

        private IEnumerator ProcessArenaMessage(ArenaObjectJson msg, object menuCommand = null)
        {
            CoroutineWithData cd;
            // consume object updates
            string object_id;
            switch ((string)msg.action)
            {
                case "create":
                case "update":
                    object_id = (string)msg.object_id;
                    string msg_type = (string)msg.type;
                    float ttl = (msg.ttl != null) ? (float)msg.ttl : 0f;
                    bool persist = Convert.ToBoolean(msg.persist);

                    IEnumerable<string> uris = ExtractAssetUris(msg.data, msgUriTags);
                    foreach (var uri in uris)
                    {
                        if (!string.IsNullOrWhiteSpace(uri))
                        {
                            cd = new CoroutineWithData(this, DownloadAssets(msg_type, uri));
                            yield return cd.coroutine;
                        }
                    }
                    CreateUpdateObject(msg, msg.data, menuCommand);
                    break;
                case "delete":
                    object_id = (string)msg.object_id;
                    RemoveObject(object_id);
                    break;
                case "leave":
                    object_id = (string)msg.object_id;
                    RemoveObject(object_id);
                    // camera special case, look for hands to delete
                    RemoveObject($"{prefixHandL}{object_id}");
                    RemoveObject($"{prefixHandR}{object_id}");
                    break;
                case "clientEvent":
                    ClientEventOnObject(msg);
                    break;
                default:
                    break;
            }
            yield break;
        }

        private void ClientEventOnObject(ArenaObjectJson msg)
        {
            var object_id = (string)msg.object_id;
            var msg_type = (string)msg.type;
            ArenaEventJson evt = JsonConvert.DeserializeObject<ArenaEventJson>(msg.data.ToString());
            var target = evt.Target;
            if (arenaObjs.TryGetValue(target, out GameObject gobj))
            {
                // pass event on to click-listener is defined
                ArenaClickListener acl = gobj.GetComponent<ArenaClickListener>();
                if (acl != null && acl.OnEventCallback != null) acl.OnEventCallback(msg_type, JsonConvert.SerializeObject(msg));
            }
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
                PublishCamera(camid, delCamMsg);
            }
            base.OnApplicationQuit();
        }
    }
}
