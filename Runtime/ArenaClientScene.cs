/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021, The CONIX Research Center. All rights reserved.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using ArenaUnity.Components;
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
            name = "ARENA (Starting...)";
        }

        [Tooltip("Name of the topic realm for the scene (runtime changes ignored).")]
        private string realm = "realm";
        [Tooltip("Namespace (automated with username), but can be overridden (runtime changes ignored).")]
        public string namespaceName = null;
        [Tooltip("Name of the scene, without namespace ('example', not 'username/example', runtime changes ignored).")]
        public string sceneName = "example";

        [Header("Presence")]
        [Tooltip("Display other camera avatars in the scene")]
        public bool renderCameras = true;

        [Header("Performance")]
        [Tooltip("Console log MQTT object messages")]
        public bool logMqttObjects = false;
        [Tooltip("Console log MQTT user messages")]
        public bool logMqttUsers = false;
        [Tooltip("Console log MQTT client event messages")]
        public bool logMqttEvents = false;
        [Tooltip("Console log MQTT non-persist messages")]
        public bool logMqttNonPersist = false;
        [Tooltip("Publish interval frequency to publish detected transform changes (milliseconds)")]
        [Range(100, 1000)]
        public int camUpdateIntervalMs = 100;

        /// <summary>
        /// Browser URL for the scene.
        /// </summary>
        public string sceneUrl { get; private set; }

        internal bool sceneObjectRights { get; private set; }

        private string sceneTopic = null;
        internal Dictionary<string, GameObject> arenaObjs = new Dictionary<string, GameObject>();
        internal Dictionary<string, GameObject> childObjs = new Dictionary<string, GameObject>();
        internal List<string> pendingDelete = new List<string>();
        internal List<string> downloadQueue = new List<string>();
        internal List<string> parentalQueue = new List<string>();
        internal List<string> localCameraIds = new List<string>();

        // Define callbacks
        public delegate void DecodeMessageDelegate(string topic, byte[] message);
        public DecodeMessageDelegate OnMessageCallback = null; // null, until library user instantiates.

        static string importPath = null;

        const string prefixCam = "camera_";
        const string prefixHandL = "handLeft_";
        const string prefixHandR = "handRight_";
        internal List<string> gltfTypeList = new List<string> { "gltf-model", "handLeft", "handRight" };

        static readonly string[] msgUriTags = { "url", "src", "overrideSrc", "detailedUrl", "headModelPath" };
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
                    arenaObjs.Add(aobj.name, aobj.gameObject);
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

            // start auth flow and MQTT connection
            ArenaCamera[] camlist = FindObjectsOfType<ArenaCamera>();
            name = "ARENA (Authenticating...)";
            CoroutineWithData cd = new CoroutineWithData(this, SigninScene(sceneName, namespaceName, realm, camlist.Length > 0));
            yield return cd.coroutine;
            name = "ARENA (MQTT Connecting...)";
            if (cd.result != null)
            {
                if (string.IsNullOrWhiteSpace(namespaceName)) namespaceName = cd.result.ToString();
                sceneTopic = $"{realm}/s/{namespaceName}/{sceneName}";
                sceneUrl = $"https://{brokerAddress}/{namespaceName}/{sceneName}";
            }
            dynamic perms = JsonConvert.DeserializeObject(permissions);
            foreach (dynamic pubperm in perms.publ)
            {
                if (sceneTopic.StartsWith(((string)pubperm).TrimEnd(new char[] { '/', '#' }))) sceneObjectRights = true;
            }
            // publish arena cameras where requested
            bool foundFirstCam = false;
            foreach (ArenaCamera cam in camlist)
            {
                if ((cam.name == Camera.main.name || camlist.Length == 1) && !foundFirstCam)
                {
                    // publish main/selected camera
                    cam.HasPermissions = true; // TODO: client connection always gets at least one
                    cam.userid = userid;
                    cam.camid = camid;
                    foundFirstCam = true;
                }
                else
                {
                    cam.HasPermissions = sceneObjectRights;
                    // TODO: fix: other cameras are auto-generated, and account must have all scene rights
                    if (!sceneObjectRights)
                    {
                        Debug.LogWarning($"Using more than one ArenaCamera requires full scene permissions. Only one camera will be published.");
                    }
                    var random = UnityEngine.Random.Range(0, 100000000);
                    cam.userid = $"{random:D8}_unity";
                    cam.camid = $"camera_{random:D8}_unity";
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

        /// <summary>
        /// Remove ARENA authentication.
        /// </summary>
#if UNITY_EDITOR
        [MenuItem("ARENA/Signout")]
#endif
        internal static void SignoutArena()
        {
#if UNITY_EDITOR
            if (Application.isPlaying)
                EditorApplication.ExitPlaymode();
#endif
            if (Directory.Exists(GoogleWebAuthorizationBroker.Folder))
                Directory.Delete(GoogleWebAuthorizationBroker.Folder, true);
            Debug.Log("Logged out of the ARENA");
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
                        dynamic msg = new
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
            cd = new CoroutineWithData(this, HttpRequestAuth($"https://{brokerAddress}/persist/{namespaceName}/{sceneName}", csrfToken));
            yield return cd.coroutine;
            if (!isCrdSuccess(cd.result)) yield break;
            string jsonString = cd.result.ToString();
            JArray jsonVal = JArray.Parse(jsonString);
            dynamic persistMessages = jsonVal;
            // establish objects
            int objects_num = 1;
            if (Directory.Exists(importPath))
                Directory.Delete(importPath, true);
            if (File.Exists($"{importPath}.meta"))
                File.Delete($"{importPath}.meta");
            bool persist = true;
            foreach (dynamic msg in persistMessages)
            {
                string object_id = (string)msg.object_id;
                string msg_type = (string)msg.type;
                float ttl = isElement(msg.ttl) ? (float)msg.ttl : 0f;

                if (!arenaObjs.ContainsKey(object_id)) // do not duplicate, local project object takes priority
                {
                    // there isnt already an object in the scene created by the user with the same object_id
                    if (GameObject.Find((string)(msg.object_id)) == null)
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
                    CreateUpdateObject(object_id, msg_type, persist, ttl, msg.attributes);
                }
                objects_num++;
            }
        }

        private static IEnumerable<string> ExtractAssetUris(dynamic data, string[] urlTags)
        {
            List<string> tagList = new List<string>(urlTags);
            var root = (JContainer)JToken.Parse(JsonConvert.SerializeObject(data));
            var uris = root.DescendantsAndSelf().OfType<JProperty>().Where(p => tagList.Contains(p.Name)).Select(p => p.Value.Value<string>());
            return uris;
        }

        private bool isElement(dynamic el)
        {
            return el != null;
        }

        private bool isElementEmpty(dynamic el)
        {
            return string.IsNullOrWhiteSpace((string)el);
        }

        internal Uri ConstructRemoteUrl(string srcUrl)
        {
            if (string.IsNullOrWhiteSpace(srcUrl))
            {
                return null;
            }
            string objUrl = srcUrl.TrimStart('/');
            objUrl = Uri.EscapeUriString(objUrl);
            if (Uri.IsWellFormedUriString(objUrl, UriKind.Relative)) objUrl = $"https://{brokerAddress}/{objUrl}";
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
                    SaveAsset(urlData, localPath);
                    // get gltf sub-assets
                    if (".gltf" == Path.GetExtension(localPath).ToLower())
                    {
                        string json;
                        using (StreamReader r = new StreamReader(localPath))
                        {
                            json = r.ReadToEnd();
                        }
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

        private void CreateUpdateObject(string object_id, string storeType, bool persist, float ttl, dynamic data, string displayName = null, object menuCommand = null)
        {
            ArenaObject aobj = null;
            JObject jData = JObject.Parse(JsonConvert.SerializeObject(data));
            if (arenaObjs.TryGetValue(object_id, out GameObject gobj))
            {   // update local
                if (gobj != null)
                    aobj = gobj.GetComponent<ArenaObject>();
            }
            else
            {   // create local
#if !UNITY_EDITOR
                Debug.Log($"Loading object '{object_id}'..."); // show new objects in log
#endif
                // check if theres already an object in unity, if so dont make a new one
                gobj = GameObject.Find((string)object_id);
                if (gobj == null)
                {
                    gobj = new GameObject();
                    gobj.name = object_id;
                }

                arenaObjs.Add(object_id, gobj);
                aobj = gobj.AddComponent(typeof(ArenaObject)) as ArenaObject;
                aobj.Created = true;
                aobj.persist = persist;
                aobj.messageType = storeType;
                aobj.parentId = (string)data.parent;
                if (ttl > 0)
                {
                    aobj.SetTtlDeleteTimer(ttl);
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

            // modify Unity attributes
            bool worldPositionStays = false; // default: most children need relative position
            string parent = (string)data.parent;
            switch ((string)data.object_type)
            {
                case "handLeft":
                case "handRight":
                case "gltf-model":
                    // load main model
                    if (data.url != null && aobj.gltfUrl == null)
                    {
                        // keep url, to add/remove and check exiting imported urls
                        string assetPath = checkLocalAsset((string)data.url);
                        if (assetPath != null)
                        {
                            AttachGltf(assetPath, gobj);
                            aobj.gltfUrl = (string)data.url;
                        }
                    }
                    // load on-demand-model (LOD) as well
                    //foreach (string detailedUrl in jData.SelectTokens("gltf-model-lod.detailedUrl"))
                    //    AttachGltf(checkLocalAsset(detailedUrl), gobj);
                    break;
                case "image":
                    // load image file
                    if (data.url != null)
                        AttachImage(checkLocalAsset((string)data.url), gobj);
                    break;
                case "camera":
                    if (renderCameras)
                    {
                        Camera cam = gobj.GetComponent<Camera>();
                        if (cam == null)
                        {
                            cam = gobj.transform.gameObject.AddComponent<Camera>();
                            cam.nearClipPlane = 0.1f; // match arena
                            cam.farClipPlane = 10000f; // match arena
                            cam.fieldOfView = 80f; // match arena
                            cam.targetDisplay = 8; // render on least-used display
                        }
                        AttachAvatar(object_id, data, displayName, gobj);
                    }
                    break;
                case "text":
                    ArenaUnity.ToUnityText(data, ref gobj);
                    break;
                case "thickline":
                    ArenaUnity.ToUnityThickline(data, ref gobj);
                    break;
                case "light":
                    ArenaUnity.ToUnityLight(data, ref gobj);
                    break;
            }

            // update transform properties, only apply if updated in mqtt message
            if (isElement(data.position))
                gobj.transform.localPosition = ArenaUnity.ToUnityPosition(data.position);
            if (isElement(data.rotation))
            {
                // TODO: needed? bool invertY = !((string)data.object_type == "camera");
                bool invertY = true;
                if (isElement(data.rotation.w)) // quaternion
                    gobj.transform.localRotation = ArenaUnity.ToUnityRotationQuat(data.rotation, invertY);
                else // euler
                    gobj.transform.localRotation = ArenaUnity.ToUnityRotationEuler(data.rotation, invertY);
                if (gltfTypeList.Where(x => x.Contains((string)data.object_type)).FirstOrDefault() != null)
                    gobj.transform.localRotation = ArenaUnity.GltfToUnityRotationQuat(gobj.transform.localRotation);
            }
            if (isElement(data.scale))
                gobj.transform.localScale = ArenaUnity.ToUnityScale(data.scale);

            // data.parent
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
                    childObjs.Add(object_id, gobj);
                }
            }
            if (parentalQueue.Contains(object_id))
            {
                // find children awaiting a parent
                foreach (KeyValuePair<string, GameObject> cgobj in childObjs)
                {
                    string cparent = cgobj.Value.GetComponent<ArenaObject>().parentId;
                    if (cparent != null && cparent == object_id)
                    {
                        cgobj.Value.SetActive(true);
                        // makes the child keep its local orientation rather than its global orientation
                        cgobj.Value.transform.SetParent(arenaObjs[object_id].transform, worldPositionStays);
                        cgobj.Value.transform.hasChanged = false;
                        //childObjs.Remove(cgobj.Key);
                    }
                }
                parentalQueue.Remove(object_id);
            }

            gobj.transform.hasChanged = false;

            // geometry (mesh)
            ArenaUnity.ToUnityMesh(data, ref gobj);

            // data.material
            if (isElement(data.material) || isElement(data.color))
                ArenaUnity.ToUnityMaterial(data, ref gobj);
            if (isElement(data.material) && isElement(data.material.src))
                AttachMaterialTexture(checkLocalAsset((string)data.material.src), gobj);

            // data.animation-mixer
            JToken amObj = jData.SelectToken("animation-mixer");
            if (amObj != null)
            {
                ArenaUnity.ToUnityAnimationMixer(data, jData, ref gobj);
            }
            JToken clObj = jData.SelectToken("click-listener");
            if (clObj != null)
            {
                Collider c = gobj.GetComponent<Collider>();
                if (c == null) c = gobj.AddComponent<MeshCollider>();
                RaycastClickExample cl = gobj.GetComponent<RaycastClickExample>();
                if (cl == null) cl = gobj.AddComponent<RaycastClickExample>();
            }

            if (aobj != null)
            {
                aobj.data = data;
                var updatedData = new JObject();
                if (aobj.jsonData != null)
                    updatedData.Merge(JObject.Parse(aobj.jsonData));
                updatedData.Merge(data);
                aobj.jsonData = JsonConvert.SerializeObject(updatedData, Formatting.Indented);
            }
        }

        internal void AttachAvatar(string object_id, dynamic data, string displayName, GameObject gobj)
        {
            bool worldPositionStays = false;
            if (data.headModelPath != null)
            {
                string localpath = checkLocalAsset((string)data.headModelPath);
                if (localpath != null)
                {
                    string headModelId = $"head-model_{object_id}";
                    Transform foundHeadModel = gobj.transform.Find(headModelId);
                    if (!foundHeadModel)
                    {
                        // add model child to camera
                        GameObject hmobj = new GameObject(headModelId);
                        AttachGltf(localpath, hmobj);
                        hmobj.transform.localPosition = Vector3.zero;
                        hmobj.transform.localRotation = Quaternion.identity;
                        hmobj.transform.localScale = Vector3.one;
                        // makes the child keep its local orientation rather than its global orientation
                        hmobj.transform.SetParent(gobj.transform, worldPositionStays);
                    }

                    string headTextId = $"headtext_{object_id}";
                    Transform foundHeadText = gobj.transform.Find(headTextId);
                    if (foundHeadText)
                    {
                        // update text
                        TextMeshPro tm = foundHeadText.GetComponent<TextMeshPro>();
                        tm.text = displayName;
                    }
                    else
                    {
                        // add text child to camera
                        GameObject htobj = new GameObject(headTextId);
                        TextMeshPro tm = htobj.transform.gameObject.AddComponent<TextMeshPro>();
                        tm.alignment = TextAlignmentOptions.Center;
                        tm.color = ArenaUnity.ToUnityColor((string)data.color);
                        tm.fontSize = 5;
                        tm.text = displayName;

                        htobj.transform.localPosition = new Vector3(0f, 0.45f, -0.05f);
                        htobj.transform.localRotation = Quaternion.Euler(0, 180f, 0);
                        htobj.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
                        // makes the child keep its local orientation rather than its global orientation
                        htobj.transform.SetParent(gobj.transform, worldPositionStays);
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

        private void AttachGltf(string assetPath, GameObject gobj)
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
                AssignAnimations(mobj, clips);
                mobj.transform.parent = gobj.transform;
                foreach (Transform child in mobj.transform.GetComponentsInChildren<Transform>())
                {   // prevent inadvertent editing of gltf elements
                    child.gameObject.isStatic = true;
                }
            }
        }

        private void AssignAnimations(GameObject mobj, AnimationClip[] clips)
        {
            if (clips != null && clips.Length > 0)
            {
                Animation anim = mobj.AddComponent<Animation>();
                foreach (AnimationClip clip in clips)
                {
                    clip.legacy = true;
                    anim.AddClip(clip, clip.name);
                    anim.clip = anim.GetClip(clip.name);
                    anim.wrapMode = WrapMode.Loop;
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
                gobj.GetComponent<ArenaObject>().externalDelete = true;
                Destroy(gobj);
            }
            arenaObjs.Remove(object_id);
        }

        private IEnumerator HttpRequestRaw(string url)
        {
            UnityWebRequest www = UnityWebRequest.Get(url);
            www.downloadHandler = new DownloadHandlerBuffer();
            //www.timeout = 5; // TODO: when fails like 443 hang, need to prevent curl 28 crash, this should just skip
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

        //TODO: prevent publish and throw errors on publishing without rights

        /// <summary>
        /// Object changes are published using a ClientId + ObjectId topic, a user must have permissions for the entire scene graph.
        /// </summary>
        public void PublishObject(string object_id, string msgJson, bool hasPermissions = true)
        {
            dynamic msg = JsonConvert.DeserializeObject(msgJson);
            msg.timestamp = GetTimestamp();
            PublishSceneMessage($"{sceneTopic}/{client.ClientId}/{object_id}", JsonConvert.SerializeObject(msg), hasPermissions);
        }

        /// <summary>
        /// Camera presence changes are published using a ObjectId-only topic, a user might only have permissions for their camid.
        /// </summary>
        public void PublishCamera(string object_id, string msgJson, bool hasPermissions = true)
        {
            dynamic msg = JsonConvert.DeserializeObject(msgJson);
            msg.timestamp = GetTimestamp();
            PublishSceneMessage($"{sceneTopic}/{object_id}", JsonConvert.SerializeObject(msg), hasPermissions);
        }

        /// <summary>
        /// Camera events are published using a ObjectId-only topic, a user might only have permissions for their camid.
        /// </summary>
        public void PublishEvent(string object_id, string eventType, string msgJsonData, bool hasPermissions = true)
        {
            dynamic msg = new ExpandoObject();
            msg.object_id = camid;
            msg.action = "clientEvent";
            msg.type = eventType;
            msg.data = JsonConvert.DeserializeObject(msgJsonData);
            msg.timestamp = GetTimestamp();
            PublishSceneMessage($"{sceneTopic}/{object_id}", JsonConvert.SerializeObject(msg), hasPermissions);
        }

        private void PublishSceneMessage(string topic, string msg, bool hasPermissions)
        {
            byte[] payload = System.Text.Encoding.UTF8.GetBytes(msg);
            Publish(topic, payload);
            LogMessage("Sending", JsonConvert.DeserializeObject(msg), hasPermissions);
        }

        private static string GetTimestamp()
        {
            return DateTime.Now.ToString("yyyy-MM-dd' 'HH:mm:ss.fffZ", CultureInfo.InvariantCulture);
        }

        protected override void OnConnected()
        {
            base.OnConnected();
            Subscribe(new string[] { $"{sceneTopic}/#" });
            name = "ARENA (MQTT Connected)";
        }

        protected override void OnDisconnected()
        {
            base.OnDisconnected();
            name = "ARENA (MQTT Disconnected)";
        }

        protected override void DecodeMessage(string topic, byte[] message)
        {
            // Call the delegate if a user has defined it
            if (OnMessageCallback != null) OnMessageCallback(topic, message);

            // ignore this client's messages
            if (!topic.Contains(client.ClientId))
            {
                ProcessMessage(message);
            }
        }

        internal void ProcessMessage(string message, object menuCommand = null)
        {
            byte[] payload = System.Text.Encoding.UTF8.GetBytes(message);
            ProcessMessage(payload);
        }

        internal void ProcessMessage(byte[] message, object menuCommand = null)
        {
            string msgJson = System.Text.Encoding.UTF8.GetString(message);
            dynamic msg = JsonConvert.DeserializeObject(msgJson);
            LogMessage("Received", msg);
            StartCoroutine(ProcessArenaMessage(msg, menuCommand));
        }

        private IEnumerator ProcessArenaMessage(dynamic msg, object menuCommand = null)
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
                        float ttl = isElement(msg.ttl) ? (float)msg.ttl : 0f;
                        bool persist = Convert.ToBoolean(msg.persist);
                        string displayName = (string)msg.displayName;

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
                        CreateUpdateObject(object_id, msg_type, persist, ttl, msg.data, displayName, menuCommand);
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
                        ClientEventOnObject(object_id, msg_type, msg.data);
                        break;
                    default:
                        break;
                }
                yield break;
            }
        }

        private void ClientEventOnObject(string object_id, string msg_type, dynamic data)
        {
            //{"object_id":"box","action":"clientEvent","type":"mousedown","data":{"clickPos":{"x":-2.87,"y":1.6,"z":6.225},"position":{"x":-0.195,"y":0.305,"z":1.913},"source":"camera_2418540601_mwfarb"},"timestamp":"2023-02-15T18:59:02.413Z"}
            if (arenaObjs.TryGetValue(object_id, out GameObject gobj))
            {
                switch (msg_type)
                {
                    case "mousedown":
                        //gobj.GetComponent<OnMouseDownExample>().OnMouseDown();
                        gobj.GetComponent<RaycastClickExample>().OnMouseDown();
                        break;
                }
            }
        }

        private void LogMessage(string dir, dynamic msg, bool hasPermissions = true)
        {
            // determine logging level
            //if (!Convert.ToBoolean(msg.persist) && !logMqttNonPersist) return;
            //if (msg.type == "object")
            //{
            //    if (msg.data != null && msg.data.object_type == "camera" && !logMqttUsers) return;
            //    if (!logMqttObjects) return;
            //}
            //if (msg.action == "clientEvent" && !logMqttEvents) return;
            //if (hasPermissions)
            Debug.Log($"{dir}: {JsonConvert.SerializeObject(msg)}");
            //else
            //    Debug.LogWarning($"Permissions FAILED {dir}: {JsonConvert.SerializeObject(msg)}");
        }

        protected override void OnApplicationQuit()
        {
            // send delete of local avatars before connection closes
            foreach (var camid in localCameraIds)
            {
                dynamic msg = new
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
