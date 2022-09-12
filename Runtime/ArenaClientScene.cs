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
using MimeMapping;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Siccity.GLTFUtility;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

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

        [Tooltip("Name of the topic realm for the scene.")]
        private string realm = "realm";
        [Tooltip("Namespace (automated with username), but can be overridden")]
        public string namespaceName = null;
        [Tooltip("Name of the scene, without namespace ('example', not 'username/example'")]
        public string sceneName = "example";

        [Header("Perspective")]
        [Tooltip("User display name")]
        public string displayName = null;
        [Tooltip("Path to user head model")]
        public string headModelPath = "/store/models/robobit.glb";
        [Tooltip("Camera for display")]
        [SerializeField]
        public Camera cameraForDisplay;
        [Tooltip("Publish cameraForDisplay pose as user avatar")]
        public bool publishCamera = true;

        [Header("Performance")]
        [Tooltip("Console log MQTT object messages")]
        public bool logMqttObjects = false;
        [Tooltip("Console log MQTT user messages")]
        public bool logMqttUsers = false;
        [Tooltip("Console log MQTT client event messages")]
        public bool logMqttEvents = false;
        [Tooltip("Console log MQTT non-persist messages")]
        public bool logMqttNonPersist = false;
        [Tooltip("Publish per frames frequency to publish detected transform changes (0 to stop)")]
        [Range(0, 60)]
        public int transformPublishInterval = 30; // in publish per frames

        /// <summary>
        /// Browser URL for the scene.
        /// </summary>
        public string sceneUrl { get; private set; }

        private string sceneTopic = null;
        internal Dictionary<string, GameObject> arenaObjs = new Dictionary<string, GameObject>();
        internal List<string> pendingDelete = new List<string>();

        static string importPath = null;

        static readonly string[] msgUriTags = { "url", "src", "overrideSrc", "detailedUrl" };
        static readonly string[] gltfUriTags = { "uri" };
        static readonly string[] skipMimeClasses = { "video", "audio" };


        protected override void OnEnable()
        {
            base.OnEnable();
            importPath = Path.Combine(appFilesPath, "Assets", "ArenaUnity", "import");

            // ensure consistent name and transform
            transform.position = Vector3.zero;
            transform.rotation = Quaternion.identity;
            transform.localScale = Vector3.one;

#if UNITY_EDITOR
            // sort arena component to the top, below Transform
            while (UnityEditorInternal.ComponentUtility.MoveComponentUp(this)) { }
#endif
        }

        // Start is called before the first frame update
        protected override void Start()
        {
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
                    Debug.LogError($"More than one ArenaObject is named '{aobj.name}'. All ArenaObjects must have unique names.");
                }
            }
            if (!nameSafe)
            {
#if UNITY_EDITOR
                if (Application.isPlaying)
                    EditorApplication.ExitPlaymode();
#endif
                yield break;
            }

            bool will = true;

            // start auth flow and MQTT connection
            name = "ARENA (Authenticating...)";
            CoroutineWithData cd = new CoroutineWithData(this, SceneSignin(sceneName, namespaceName, realm, will));
            yield return cd.coroutine;
            name = "ARENA (MQTT Connecting...)";
            if (cd.result != null)
            {
                namespaceName = cd.result.ToString();
                sceneTopic = $"{realm}/s/{namespaceName}/{sceneName}";
                sceneUrl = $"https://{brokerAddress}/{namespaceName}/{sceneName}";
            }

            // publish main/selected camera
            cameraForDisplay = Camera.main;
            ArenaCamera acobj = cameraForDisplay.gameObject.AddComponent(typeof(ArenaCamera)) as ArenaCamera;

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
        public void SignoutArena()
        {
            ArenaMenu.SignoutArena();
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
                     $"Are you sure you want to delete object: {ids}?", "Delete", "Save"))
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
                DisplayCancelableProgressBar("ARENA Persistence", $"Loading object-id: {object_id}", objects_num / (float)jsonVal.Count);
                if (!arenaObjs.ContainsKey(object_id)) // do not duplicate, local project object takes priority
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
                    CreateUpdateObject(object_id, msg_type, persist, msg.attributes);
                }
                objects_num++;
            }
            ClearProgressBar();
            // establish parent/child relationships
            foreach (KeyValuePair<string, GameObject> gobj in arenaObjs)
            {
                string parent = gobj.Value.GetComponent<ArenaObject>().parentId;
                if (parent != null && arenaObjs.ContainsKey(parent))
                {
                    bool worldPositionStays = false;
                    // makes the child keep its local orientation rather than its global orientation
                    gobj.Value.transform.SetParent(arenaObjs[parent].transform, worldPositionStays);
                    gobj.Value.GetComponent<ArenaObject>().transform.hasChanged = false;
                }
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
            string objUrl = srcUrl.TrimStart('/');
            if (objUrl.StartsWith("store/")) objUrl = $"https://{brokerAddress}/{objUrl}";
            else if (objUrl.StartsWith("models/")) objUrl = $"https://{brokerAddress}/store/{objUrl}";
            else objUrl = objUrl.Replace("www.dropbox.com", "dl.dropboxusercontent.com"); // replace dropbox links to direct links
            if (string.IsNullOrWhiteSpace(objUrl)) return null;
            if (!Uri.IsWellFormedUriString(objUrl, UriKind.Absolute))
            {
                Debug.LogWarning($"Invalid Uri: '{objUrl}'");
                return null;
            }
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
                        IEnumerable<string> uris = ExtractAssetUris(JsonConvert.DeserializeObject(json), gltfUriTags);
                        foreach (var uri in uris)
                        {
                            if (!string.IsNullOrWhiteSpace(uri))
                            {
                                Uri subUrl = new Uri(remoteUri, uri);
                                cd = new CoroutineWithData(this, HttpRequestRaw(subUrl.AbsoluteUri));
                                yield return cd.coroutine;
                                if (isCrdSuccess(cd.result))
                                {
                                    byte[] urlSubData = (byte[])cd.result;
                                    string localSubPath = Path.Combine(Path.GetDirectoryName(localPath), uri);
                                    SaveAsset(urlSubData, localSubPath);
#if UNITY_EDITOR
                                    // import each sub-file for a deterministic reference
                                    AssetDatabase.ImportAsset(localSubPath);
#endif
                                }
                                else allPathsValid = false;
                            }
                        }
                    }
                    if (!allPathsValid) yield break;
#if UNITY_EDITOR
                    // import master-file to link to the rest
                    AssetDatabase.ImportAsset(localPath);
                    AssetDatabase.Refresh();
#endif
                }
            }

            yield return localPath;
        }

        private static void SaveAsset(byte[] data, string path)
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

        private void CreateUpdateObject(string object_id, string storeType, bool persist, dynamic data, object menuCommand = null)
        {
            ArenaObject aobj = null;
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
                gobj = new GameObject();
                gobj.name = object_id;
                arenaObjs.Add(object_id, gobj);
                aobj = gobj.AddComponent(typeof(ArenaObject)) as ArenaObject;
                aobj.created = true;
                aobj.persist = persist;
                aobj.messageType = storeType;
                aobj.parentId = (string)data.parent;
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
            switch ((string)data.object_type)
            {
                case "gltf-model":
                    // load main model
                    if (data.url != null)
                        AttachGltf(checkLocalAsset((string)data.url), gobj);
                    // load on-demand-model (LOD) as well
                    JObject d = JObject.Parse(JsonConvert.SerializeObject(data));
                    foreach (string detailedUrl in d.SelectTokens("gltf-model-lod.detailedUrl"))
                        AttachGltf(checkLocalAsset(detailedUrl), gobj);
                    FindAnimations(data, aobj);
                    break;
                case "image":
                    // load image file
                    if (data.url != null)
                        AttachImage(checkLocalAsset((string)data.url), gobj);
                    break;
                case "camera":
                    // sync camera to main display if requested
                    Camera cam = gobj.GetComponent<Camera>();
                    if (cam == null)
                    {
                        cam = gobj.transform.gameObject.AddComponent<Camera>();
                        cam.nearClipPlane = 0.1f; // match arena
                        cam.farClipPlane = 10000f; // match arena
                        cam.fieldOfView = 80f; // match arena
                    }
                    break;
                case "light":
                    ArenaUnity.ToUnityLight(data, ref gobj);
                    break;
            }

            if (isElement(data.position))
                gobj.transform.localPosition = ArenaUnity.ToUnityPosition(data.position);
            else
                gobj.transform.localPosition = Vector3.zero;
            if (isElement(data.rotation))
            {
                // TODO: needed? bool invertY = !((string)data.object_type == "camera");
                bool invertY = true;
                if (isElement(data.rotation.w)) // quaternion
                    gobj.transform.localRotation = ArenaUnity.ToUnityRotationQuat(data.rotation, invertY);
                else // euler
                    gobj.transform.localRotation = ArenaUnity.ToUnityRotationEuler(data.rotation, invertY);
            }
            else
                gobj.transform.localRotation = Quaternion.identity;
            if ((string)data.object_type == "gltf-model")
                gobj.transform.localRotation = ArenaUnity.GltfToUnityRotationQuat(gobj.transform.localRotation);
            if (isElement(data.scale))
                gobj.transform.localScale = ArenaUnity.ToUnityScale(data.scale);
            else
                gobj.transform.localScale = Vector3.one;

            ArenaUnity.ToUnityMesh(data, ref gobj);

            if (isElement(data.material) || isElement(data.color))
                ArenaUnity.ToUnityMaterial(data, ref gobj);
            if (isElement(data.material) && isElement(data.material.src))
                AttachMaterialTexture(checkLocalAsset((string)data.material.src), gobj);

            gobj.transform.hasChanged = false;
            if (aobj != null)
            {
                aobj.data = data;
                aobj.jsonData = JsonConvert.SerializeObject(aobj.data, Formatting.Indented);
            }
        }

        private void FindAnimations(dynamic data, ArenaObject aobj)
        {
#if UNITY_EDITOR
            // check for animations
            var assetRepresentationsAtPath = AssetDatabase.LoadAllAssetRepresentationsAtPath(checkLocalAsset((string)data.url));
            foreach (var assetRepresentation in assetRepresentationsAtPath)
            {
                var animationClip = assetRepresentation as AnimationClip;
                if (animationClip != null)
                {
                    if (aobj.animations == null)
                        aobj.animations = new List<string>();
                    aobj.animations.Add(animationClip.name);
                }
            }
#endif
        }

        private string checkLocalAsset(string msgUrl)
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
            try
            {
                mobj = Importer.LoadFromFile(assetPath, new ImportSettings(), out clips);
            }
            catch (Exception err)
            {
                Debug.LogWarning($"Unable to load GTLF at {assetPath}. {err.Message}");
            }
            AssignAnimations(mobj, clips);
            if (mobj != null)
            {
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
                DisplayCancelableProgressBar("ARENA URL", $"{url} downloading...", www.downloadProgress);
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

        public void PublishObject(string object_id, string msgJson)
        {
            //TODO: prevent publish and throw errors on publishing without rights

            dynamic msg = JsonConvert.DeserializeObject(msgJson);
            msg.timestamp = DateTime.Now.ToString("yyyy-MM-dd' 'HH:mm:ss.fffZ", CultureInfo.InvariantCulture);
            byte[] payload = System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(msg));
            // TODO: Revist ClientId, ATM anonymous user has no client id reights: Publish($"{sceneTopic}/{client.ClientId}/{object_id}", payload);
            Publish($"{sceneTopic}/{object_id}", payload);
            LogMessage("Sent", msg);
        }

        public void PublishEvent(string msgJsonData)
        {
            dynamic msgData = JsonConvert.DeserializeObject(msgJsonData);
            dynamic msg = new ExpandoObject();
            msg.object_id = camid;
            msg.action = "clientEvent";
            msg.data = msgData;
            PublishObject(camid, JsonConvert.SerializeObject(msg));
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
            // consume object updates
            if (msg.type == "object")
            {
                switch ((string)msg.action)
                {
                    case "create":
                    case "update":
                        IEnumerable<string> uris = ExtractAssetUris(msg.data, msgUriTags);
                        if (uris.Count() > 0)
                        {
                            DisplayCancelableProgressBar("ARENA Message", $"Loading object-id: {(string)msg.object_id}", 0f);
                            foreach (var uri in uris)
                            {
                                if (!string.IsNullOrWhiteSpace(uri))
                                {
                                    CoroutineWithData cd = new CoroutineWithData(this, DownloadAssets((string)msg.type, uri));
                                    yield return cd.coroutine;
                                }
                            }
                            ClearProgressBar();
                        }
                        CreateUpdateObject((string)msg.object_id, (string)msg.type, Convert.ToBoolean(msg.persist), msg.data, menuCommand);
                        break;
                    case "delete":
                        RemoveObject((string)msg.object_id);
                        break;
                    default:
                        break;
                }
                yield break;
            }
        }

        private void LogMessage(string dir, dynamic msg)
        {
            // determine logging level
            if (!Convert.ToBoolean(msg.persist) && !logMqttNonPersist) return;
            if (msg.type == "object")
            {
                if (msg.data != null && msg.data.object_type == "camera" && !logMqttUsers) return;
                if (!logMqttObjects) return;
            }
            if (msg.action == "clientEvent" && !logMqttEvents) return;
            Debug.Log($"{dir}: {JsonConvert.SerializeObject(msg)}");
        }

        protected void OnValidate()
        {
            // camera change?
            if (cameraForDisplay != null)
            {
                foreach (Camera cam in Camera.allCameras)
                {
                    if (cam.name == cameraForDisplay.name)
                    {
                        cam.targetDisplay = ArenaUnity.mainDisplay;
                        Debug.Log($"{cam.name} now using Display {cam.targetDisplay + 1}.");
                    }
                    else
                    {
                        cam.targetDisplay = ArenaUnity.secondDisplay;
                    }
                }
            }
        }
    }
}
