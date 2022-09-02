/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021, The CONIX Research Center. All rights reserved.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Oauth2.v2;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using M2MqttUnity;
using MimeMapping;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
#if UNITY_EDITOR
using SandolkakosDigital.EditorUtils;
#endif
using Siccity.GLTFUtility;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace ArenaUnity
{
    /// <summary>
    /// Class to manage a singleton instance of the ARENA client connection.
    /// </summary>
    [HelpURL("https://arena.conix.io")]
    [DisallowMultipleComponent]
    [AddComponentMenu("ArenaClient", 0)]
    public class ArenaClient : M2MqttUnityClient
    {
        // Singleton instance of this connection object
        public static ArenaClient Instance { get; private set; }
        public bool IsShuttingDown { get; internal set; }

        public enum Auth { Anonymous, Google };

        protected override void Awake()
        {
            base.Awake();
            Instance = this;
            name = $"ARENA (MQTT Disconnected)";
        }

        [Header("ARENA Connection")]
        [Tooltip("Name of the topic realm for the scene.")]
        private string realm = "realm";
        [Tooltip("Namespace (automated with username), but can be overridden")]
        public string namespaceName = null;
        [Tooltip("Name of the scene, without namespace ('example', not 'username/example'")]
        public string sceneName = "example";

        [Header("Performance & Control")]
        [Tooltip("Cameras for Display 1")]
        [SerializeField]
        public Camera cameraForDisplay;
        [Tooltip("Synchronize camera display to first ARENA user in the scene")]
        public bool cameraAutoSync = false;
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

        [Header("Authentication")]
        [Tooltip("Connect as Anonymous user, or Google authenticated user.")]
        public Auth tokenType = Auth.Google;

        /// <summary>
        /// Authenticated user email account.
        /// </summary>
        public string email { get; private set; }
        /// <summary>
        /// MQTT JWT Auth Payload and Claims.
        /// </summary>
        public string permissions { get; private set; }
        /// <summary>
        /// MQTT JWT expiration epoch.
        /// </summary>
        public long mqttExpires { get; private set; }
        /// <summary>
        /// Browser URL for the scene.
        /// </summary>
        public string sceneUrl { get; private set; }

        // internal variables
        private string idToken = null;
        private string csrfToken = null;
        private List<string> eventMessages = new List<string>();
        private string sceneTopic = null;
        internal Dictionary<string, GameObject> arenaObjs = new Dictionary<string, GameObject>();
        private static UserCredential credential;
        private Transform ArenaClientTransform;

        internal List<string> pendingDelete = new List<string>();

        // local paths
        const string gAuthFile = ".arena_google_auth";
        const string mqttTokenFile = ".arena_mqtt_auth";
        const string userDirArena = ".arena";
        const string userSubDirUnity = "unity";
        static string userHomePath = null;
        static string appFilesPath = null;
        static string importPath = null;

        static readonly string[] msgUriTags = { "url", "src", "overrideSrc", "detailedUrl" };
        static readonly string[] gltfUriTags = { "uri" };
        static readonly string[] skipMimeClasses = { "video", "audio" };

        static readonly string[] Scopes = {
            Oauth2Service.Scope.UserinfoProfile,
            Oauth2Service.Scope.UserinfoEmail,
            Oauth2Service.Scope.Openid
        };

        public class UserState
        {
            public bool authenticated { get; set; }
            public string username { get; set; }
            public string fullname { get; set; }
            public string email { get; set; }
            public string type { get; set; }
        }

        public class MqttAuth
        {
            public string username { get; set; }
            public string token { get; set; }
        }

        protected void OnEnable()
        {
            userHomePath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            appFilesPath = Application.isMobilePlatform ? Application.persistentDataPath : "";
            importPath = Path.Combine(appFilesPath, "Assets", "ArenaUnity", "import");

            cameraForDisplay = Camera.main;

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
            ConnectArena();
            Selection.activeGameObject = gameObject; // client focus at runtime starts
            SceneHierarchyUtility.SetExpanded(gameObject, true); // expand arena list
#endif
        }

        /// <summary>
        /// Authenticate, MQTT connect, and add ARENA objects from Persistence DB to local app.
        /// </summary>
        public void ConnectArena()
        {
            StartCoroutine(SceneSignin());
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
            base.Update(); // call ProcessMqttEvents()

            if (eventMessages.Count > 0)
            {
                foreach (string msg in eventMessages)
                {
                    ProcessMessage(msg);
                }
                eventMessages.Clear();
            }

            if (arenaObjs.Count != transform.childCount)
            { // discover new objects created in Unity as relatives of our client
                foreach (Transform child in transform)
                {
                    ArenaObject aobj = child.gameObject.GetComponent<ArenaObject>();
                    if (aobj == null)
                    {
                        aobj = child.gameObject.AddComponent(typeof(ArenaObject)) as ArenaObject;
                        aobj.created = false;
                        aobj.messageType = "object";
                        child.gameObject.transform.hasChanged = true;
                        child.name = Regex.Replace(child.name, ArenaUnity.regexObjId, ArenaUnity.replaceCharObjId);
                        if (arenaObjs.ContainsKey(child.name))
                            child.name = $"{child.name}-{UnityEngine.Random.Range(0, 1000000)}";
                        arenaObjs.Add(child.name, child.gameObject);
                    }
                }
            }

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

        private IEnumerator SceneSignin()
        {
            string sceneAuthDir = Path.Combine(userHomePath, userDirArena, userSubDirUnity, brokerAddress, "s");
            string userGAuthPath = sceneAuthDir;
            string userMqttPath = Path.Combine(sceneAuthDir, mqttTokenFile);
            string mqttToken = null;
            CoroutineWithData cd;

            string localMqttPath = Path.Combine(appFilesPath, mqttTokenFile);
            if (File.Exists(localMqttPath))
            {
                // check for local mqtt auth
                Debug.LogWarning("Using local MQTT token.");
                try
                {
                    using (var sr = new StreamReader(localMqttPath))
                    {
                        mqttToken = sr.ReadToEnd();
                    }
                }
                catch (IOException e)
                {
                    Debug.LogError(e.Message);
                }
            }
            else
            {
                string authType = "";
                string mqttUsername = "";
                switch (tokenType)
                {
                    case Auth.Anonymous:
                        // prefix all anon users with "anonymous-"
                        authType = "anonymous";
                        mqttUsername = $"anonymous-UnityClient-{UnityEngine.Random.Range(0, 1000000)}";
                        break;
                    case Auth.Google:
                        // get oauth app credentials
                        Debug.Log("Using remote-authenticated MQTT token.");
                        cd = new CoroutineWithData(this, HttpRequestAuth($"https://{brokerAddress}/conf/gauth.json"));
                        yield return cd.coroutine;
                        if (!isCrdSuccess(cd.result)) yield break;
                        string gauthId = cd.result.ToString();

                        // request user auth
                        using (var stream = ToStream(gauthId))
                        {
                            string applicationName = "ArenaClientCSharp";
                            IDataStore ds;
                            if (Application.isMobilePlatform) ds = new NullDataStore();
                            else ds = new FileDataStore(userGAuthPath, true);
                            GoogleWebAuthorizationBroker.Folder = userGAuthPath;
                            // GoogleWebAuthorizationBroker.AuthorizeAsync for "installed" creds only
                            credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                                    GoogleClientSecrets.FromStream(stream).Secrets,
                                    Scopes,
                                    "user",
                                    CancellationToken.None,
                                    ds).Result;
                            if (ds.GetType() == typeof(FileDataStore))
                                Debug.Log($"Credential file saved to: {userGAuthPath}");

                            var oauthService = new Oauth2Service(new BaseClientService.Initializer()
                            {
                                HttpClientInitializer = credential,
                                ApplicationName = applicationName,
                            });

                            var userInfo = oauthService.Userinfo.Get().Execute();

                            email = userInfo.Email;
                            idToken = credential.Token.IdToken;
                        }
                        authType = "google-installed";
                        break;
                    default:
                        Debug.LogWarning($"Invalid ARENA authentication type: '{authType}'");
                        yield break;
                }

                // get arena CSRF token
                yield return HttpRequestAuth($"https://{brokerAddress}/user/login");

                // get arena user account state
                WWWForm form = new WWWForm();
                if (idToken != null) form.AddField("id_token", idToken);
                cd = new CoroutineWithData(this, HttpRequestAuth($"https://{brokerAddress}/user/user_state", csrfToken, form));
                yield return cd.coroutine;
                if (!isCrdSuccess(cd.result)) yield break;
                var user = JsonConvert.DeserializeObject<UserState>(cd.result.ToString());
                if (user.authenticated && (namespaceName == null || namespaceName.Trim() == ""))
                {
                    namespaceName = user.username;
                    mqttUsername = user.username;
                }
                // get arena user mqtt token
                form.AddField("id_auth", authType);
                form.AddField("username", mqttUsername);
                form.AddField("realm", realm);
                // handle full ARENA scene
                form.AddField("scene", $"{namespaceName}/{sceneName}");
                form.AddField("userid", "true");
                form.AddField("camid", "true");
                cd = new CoroutineWithData(this, HttpRequestAuth($"https://{brokerAddress}/user/mqtt_auth", csrfToken, form));
                yield return cd.coroutine;
                if (!isCrdSuccess(cd.result)) yield break;
                mqttToken = cd.result.ToString();

                StreamWriter writer = new StreamWriter(userMqttPath);
                writer.Write(mqttToken);
                writer.Close();
                Debug.Log($"Mqtt file saved to: {userMqttPath}");
            }

            var auth = JsonConvert.DeserializeObject<MqttAuth>(mqttToken);
            mqttUserName = auth.username;
            mqttPassword = auth.token;
            var handler = new JwtSecurityTokenHandler();
            JwtPayload payloadJson = handler.ReadJwtToken(auth.token).Payload;
            permissions = JToken.Parse(payloadJson.SerializeToJson()).ToString(Formatting.Indented);
            if (string.IsNullOrWhiteSpace(namespaceName))
                namespaceName = payloadJson.Sub;
            mqttExpires = (long)payloadJson.Exp;
            DateTimeOffset dateTimeOffSet = DateTimeOffset.FromUnixTimeSeconds(mqttExpires);
            TimeSpan duration = dateTimeOffSet.DateTime.Subtract(DateTime.Now.ToUniversalTime());
            Debug.Log($"MQTT Token expires in {ArenaUnity.TimeSpanToString(duration)}");

            sceneTopic = $"{realm}/s/{namespaceName}/{sceneName}";
            sceneUrl = $"https://{brokerAddress}/{namespaceName}/{sceneName}";

            // background mqtt connect
            base.Start();

            // get persistence objects
            cd = new CoroutineWithData(this, HttpRequestAuth($"https://{brokerAddress}/persist/{namespaceName}/{sceneName}", csrfToken));
            yield return cd.coroutine;
            if (!isCrdSuccess(cd.result)) yield break;
            ArenaClientTransform = FindObjectOfType<ArenaClient>().transform;
            string jsonString = cd.result.ToString();
            JArray jsonVal = JArray.Parse(jsonString);
            dynamic persistMessages = jsonVal;
            // establish objects
            int objects_num = 1;
            if (Directory.Exists(importPath))
                Directory.Delete(importPath, true);
            if (File.Exists($"{importPath}.meta"))
                File.Delete($"{importPath}.meta");
            foreach (dynamic msg in persistMessages)
            {
                DisplayCancelableProgressBar("ARENA Persistence", $"Loading object-id: {(string)msg.object_id}", objects_num / (float)jsonVal.Count);
                IEnumerable<string> uris = ExtractAssetUris(msg.attributes, msgUriTags);
                foreach (var uri in uris)
                {
                    if (!string.IsNullOrWhiteSpace(uri))
                    {
                        cd = new CoroutineWithData(this, DownloadAssets((string)msg.type, uri));
                        yield return cd.coroutine;
                    }
                }
                CreateUpdateObject((string)msg.object_id, (string)msg.type, msg.attributes);
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

        private void CreateUpdateObject(string object_id, string storeType, dynamic data, object menuCommand = null)
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
                gobj.transform.parent = ArenaClientTransform;
                gobj.name = object_id;
                arenaObjs.Add(object_id, gobj);
                aobj = gobj.AddComponent(typeof(ArenaObject)) as ArenaObject;
                aobj.created = true;
                aobj.messageType = storeType;
                aobj.parentId = (string)data.parent;
                aobj.persist = true;
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

                        if (cameraAutoSync && !cameraForDisplay.name.StartsWith("camera_"))
                        {
                            cam.targetDisplay = ArenaUnity.mainDisplay;
                            cameraForDisplay = cam;
                        }
                        else
                            cam.targetDisplay = ArenaUnity.secondDisplay;
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

        private static Stream ToStream(string str)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(str);
            writer.Flush();
            stream.Position = 0;
            return stream;
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

        private IEnumerator HttpRequestAuth(string url, string csrf = null, WWWForm form = null)
        {
            UnityWebRequest www;
            if (form == null)
                www = UnityWebRequest.Get(url);
            else
                www = UnityWebRequest.Post(url, form);
            if (csrf != null)
            {
                www.SetRequestHeader("Cookie", $"csrftoken={csrf}");
                www.SetRequestHeader("X-CSRFToken", csrf);
            }
            if (mqttPassword != null)
                www.SetRequestHeader("Cookie", $"mqtt_token={mqttPassword}");
            yield return www.SendWebRequest();
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
                // get the csrf cookie
                string SetCookie = www.GetResponseHeader("Set-Cookie");
                if (SetCookie != null)
                {
                    if (SetCookie.Contains("csrftoken="))
                        csrfToken = GetCookie(SetCookie, "csrftoken");
                    else if (SetCookie.Contains("csrf="))
                        csrfToken = GetCookie(SetCookie, "csrf");
                }

                Debug.Log($"REST: {www.downloadHandler.text}");
                yield return www.downloadHandler.text;
            }
        }

        private bool isCrdSuccess(object result)
        {
            return result != null && result.ToString() != "UnityEngine.Networking.UnityWebRequestAsyncOperation";
        }

        private string GetCookie(string SetCookie, string csrftag)
        {
            string csrfCookie = null;
            Regex rxCookie = new Regex($"{csrftag}=(?<csrf_token>.{64});");
            MatchCollection cookieMatches = rxCookie.Matches(SetCookie);
            if (cookieMatches.Count > 0)
                csrfCookie = cookieMatches[0].Groups["csrf_token"].Value;
            return csrfCookie;
        }

        internal void PublishObject(string object_id, string msgJson)
        {
            byte[] payload = System.Text.Encoding.UTF8.GetBytes(msgJson);
            client.Publish($"{sceneTopic}/{client.ClientId}/{object_id}", payload, MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
            dynamic msg = JsonConvert.DeserializeObject(msgJson);
            LogMessage("Sent", msg);
        }

        protected override void OnConnected()
        {
            base.OnConnected();
            client.Subscribe(new string[] { $"{sceneTopic}/#" }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
            name = $"ARENA (MQTT Connected)";
        }

        protected override void OnConnectionFailed(string errorMessage)
        {
            Debug.LogWarning($"CONNECTION FAILED! {errorMessage}");
        }

        protected override void OnDisconnected()
        {
            base.OnDisconnected();
            name = $"ARENA (MQTT Disconnected)";
        }

        protected override void DecodeMessage(string topic, byte[] message)
        {
            // ignore this client's messages
            if (!topic.Contains(client.ClientId))
            {
                string msg = System.Text.Encoding.UTF8.GetString(message);
                StoreMessage(msg);
            }
        }

        private void StoreMessage(string eventMsg)
        {
            eventMessages.Add(eventMsg);
        }

        internal void ProcessMessage(string msgJson, object menuCommand = null)
        {
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
                        if (Convert.ToBoolean(msg.persist))
                        {
                            DisplayCancelableProgressBar("ARENA Message", $"Loading object-id: {(string)msg.object_id}", 0f);
                            IEnumerable<string> uris = ExtractAssetUris(msg.data, msgUriTags);
                            foreach (var uri in uris)
                            {
                                if (!string.IsNullOrWhiteSpace(uri))
                                {
                                    CoroutineWithData cd = new CoroutineWithData(this, DownloadAssets((string)msg.type, uri));
                                    yield return cd.coroutine;
                                }
                            }
                            ClearProgressBar();
                            CreateUpdateObject((string)msg.object_id, (string)msg.type, msg.data, menuCommand);
                        }
                        else if (msg.data.object_type == "camera") // try to manage camera
                        {
                            CreateUpdateObject((string)msg.object_id, (string)msg.type, msg.data);
                        }
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

        protected void OnDestroy()
        {
            Disconnect();
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

        protected new void OnApplicationQuit()
        {
            IsShuttingDown = true;
            base.OnApplicationQuit();
        }
    }
}
