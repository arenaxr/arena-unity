/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021, The CONIX Research Center. All rights reserved.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Oauth2.v2;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using M2MqttUnity;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Siccity.GLTFUtility;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Networking;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace ArenaUnity
{
    /// <summary>
    /// Class to manage a singleton instance of the ARENA client connection.
    /// </summary>
    [HelpURL("https://arena.conix.io")]
    [DisallowMultipleComponent()]
    [AddComponentMenu("ArenaClient", 0)]
    public class ArenaClient : M2MqttUnityClient
    {
        // Singleton instance of this connection object
        public static ArenaClient Instance { get; private set; }
        public bool IsShuttingDown { get; internal set; }

        protected override void Awake()
        {
            Instance = this;
        }
        [Header("ARENA Connection")]
        [Tooltip("Name of the topic realm for the scene.")]
        private string realm = "realm";
        [Tooltip("Namespace (automated with username), but can be overridden")]
        public string namespaceName = null;
        [Tooltip("Name of the scene, without namespace ('example', not 'username/example'")]
        public string sceneName = "example";
        [Space()]
        [Tooltip("Browser URL for the scene.")]
        [TextArea(minLines: 1, maxLines: 2)]
        public string sceneUrl = null;

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
        [Tooltip("Authenticated user email account.")]
        public string email = null;
        [Tooltip("MQTT JWT Auth Payload and Claims")]
        [TextArea(10, 15)]
        public string permissions;

        // internal variables
        private string idToken = null;
        private string csrfToken = null;
        private List<string> eventMessages = new List<string>();
        private string sceneTopic = null;
        private Dictionary<string, GameObject> arenaObjs = new Dictionary<string, GameObject>();
        private static readonly string ClientName = "ARENA Client Runtime";
        private static UserCredential credential;
        private Transform ArenaClientTransform;

        internal List<string> pendingDelete = new List<string>();

        // local paths
        const string gAuthFile = ".arena_google_auth";
        const string mqttTokenFile = ".arena_mqtt_auth";
        const string userDirArena = ".arena";
        const string userSubDirUnity = "unity";
        static readonly string userHomePath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
        public static string importPath = "Assets/ArenaUnity/import";

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
            cameraForDisplay = Camera.main;

            // ensure consistant name and transform
            name = ClientName;
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
            StartCoroutine(SceneSignin());
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
                        child.name = Regex.Replace(child.name, ArenaUnity.regexArenaObjectId, "-");
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
                        Publish(msg.object_id, payload);
                    }
                }
#endif
                pendingDelete.Clear();
            }
        }

        private IEnumerator SceneSignin()
        {
            string sceneAuthDir = Path.Combine(userHomePath, userDirArena, userSubDirUnity, brokerAddress, "s");
            string gAuthPath = sceneAuthDir;
            string mqttTokenPath = Path.Combine(sceneAuthDir, mqttTokenFile);

            // get app credentials
            CoroutineWithData cd = new CoroutineWithData(this, HttpRequestAuth($"https://{brokerAddress}/conf/gauth.json"));
            yield return cd.coroutine;
            if (!isCrdSuccess(cd.result)) yield break;
            string gauthId = cd.result.ToString();

            // request user auth
            using (var stream = ToStream(gauthId))
            {
                string applicationName = "ArenaClientCSharp";
                IDataStore ds;
                if (Application.isMobilePlatform) ds = new NullDataStore();
                else ds = new FileDataStore(gAuthPath, true);
                GoogleWebAuthorizationBroker.Folder = gAuthPath;
                // GoogleWebAuthorizationBroker.AuthorizeAsync for "installed" creds only
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                        GoogleClientSecrets.FromStream(stream).Secrets,
                        Scopes,
                        "user",
                        CancellationToken.None,
                        ds).Result;
                if (ds.GetType() == typeof(FileDataStore))
                    Debug.Log($"Credential file saved to: {gAuthPath}");

                var oauthService = new Oauth2Service(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = applicationName,
                });

                var userInfo = oauthService.Userinfo.Get().Execute();

                email = userInfo.Email;
                idToken = credential.Token.IdToken;
            }

            // get arena web login token
            yield return HttpRequestAuth($"https://{brokerAddress}/user/login");

            // get arena user account state
            WWWForm form = new WWWForm();
            form.AddField("id_token", idToken);
            cd = new CoroutineWithData(this, HttpRequestAuth($"https://{brokerAddress}/user/user_state", csrfToken, form));
            yield return cd.coroutine;
            if (!isCrdSuccess(cd.result)) yield break;
            var user = JsonConvert.DeserializeObject<UserState>(cd.result.ToString());
            if (user.authenticated && (namespaceName == null || namespaceName.Trim() == ""))
                namespaceName = user.username;

            // get arena user mqtt token
            form.AddField("id_auth", "google-installed");
            form.AddField("username", user.username);
            form.AddField("realm", realm);
            form.AddField("scene", $"{namespaceName}/{sceneName}");
            cd = new CoroutineWithData(this, HttpRequestAuth($"https://{brokerAddress}/user/mqtt_auth", csrfToken, form));
            yield return cd.coroutine;
            if (!isCrdSuccess(cd.result)) yield break;
            var auth = JsonConvert.DeserializeObject<MqttAuth>(cd.result.ToString());
            mqttUserName = auth.username;
            mqttPassword = auth.token;
            var handler = new JwtSecurityTokenHandler();
            JwtPayload payloadJson = handler.ReadJwtToken(auth.token).Payload;
            permissions = JToken.Parse(payloadJson.SerializeToJson()).ToString(Formatting.Indented);
            sceneTopic = $"{realm}/s/{namespaceName}/{sceneName}";
            sceneUrl = $"https://{brokerAddress}/{namespaceName}/{sceneName}";
            base.Start(); // background mqtt connect

            // get persistence objects
            cd = new CoroutineWithData(this, HttpRequestAuth($"https://{brokerAddress}/persist/{namespaceName}/{sceneName}", csrfToken));
            yield return cd.coroutine;
            if (!isCrdSuccess(cd.result)) yield break;
            ArenaClientTransform = FindObjectOfType<ArenaClient>().transform;
            string jsonString = cd.result.ToString();
            JArray jsonVal = JArray.Parse(jsonString);
            dynamic objects = jsonVal;
            // establish objects
            int objects_num = 1;
            if (Directory.Exists(Application.dataPath + "/ArenaUnity"))
                Directory.Delete(Application.dataPath + "/ArenaUnity", true);
            if (File.Exists(Application.dataPath + "/ArenaUnity.meta"))
                File.Delete(Application.dataPath + "/ArenaUnity.meta");
            foreach (dynamic obj in objects)
            {
                DisplayCancelableProgressBar("ARENA Persistance", $"Loading object-id: {(string)obj.object_id}", objects_num / (float)jsonVal.Count);
                string localPath = null;
                if (isElement(obj.attributes) && isElement(obj.attributes.url) && !isElementEmpty(obj.attributes.url))
                {
                    cd = new CoroutineWithData(this, DownloadAssets(obj));
                    yield return cd.coroutine;
                    localPath = cd.result.ToString();
                }
                CreateUpdateObject((string)obj.object_id, (string)obj.type, obj.attributes, localPath);
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

        private bool isElement(dynamic el)
        {
            return el != null;
        }

        private bool isElementEmpty(dynamic el)
        {
            return string.IsNullOrWhiteSpace((string)el);
        }

        private IEnumerator DownloadAssets(dynamic obj)
        {
            string objUrl = null;
            string localPath = null;
            // update urls, if any
            if (obj.type == "object")
            {
                objUrl = ((string)obj.attributes.url).TrimStart('/');
                if (objUrl.StartsWith("store/")) objUrl = $"https://{brokerAddress}/{objUrl}";
                else if (objUrl.StartsWith("models/")) objUrl = $"https://{brokerAddress}/store/{objUrl}";
                else objUrl = objUrl.Replace("www.dropbox.com", "dl.dropboxusercontent.com"); // replace dropbox links to direct links
            }
            // load remote assets
            if (objUrl != null)
            {
                Uri baseUri = new Uri(objUrl);
                string url2Path = baseUri.Host + baseUri.AbsolutePath;
                string objFileName = string.Join("/", url2Path.Split(Path.GetInvalidFileNameChars()));
                localPath = importPath + "/" + objFileName;
                if (!File.Exists(localPath))
                {
                    // get main url src
                    CoroutineWithData cd = new CoroutineWithData(this, HttpRequestRaw(objUrl));
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
                            JObject jData = JObject.Parse(json);
                            foreach (JToken child in jData.SelectTokens("$.*[*].uri"))
                            {
                                string relativeUri = (string)child;
                                if (relativeUri != null)
                                {
                                    Uri subUrl = new Uri(baseUri, relativeUri);
                                    cd = new CoroutineWithData(this, HttpRequestRaw(subUrl.AbsoluteUri));
                                    yield return cd.coroutine;
                                    if (isCrdSuccess(cd.result))
                                    {
                                        byte[] urlSubData = (byte[])cd.result;
                                        string localSubPath = Path.Combine(Path.GetDirectoryName(localPath), relativeUri);
                                        SaveAsset(urlSubData, localSubPath);
                                        // import each sub-file for a deterministic reference
                                        AssetDatabase.ImportAsset(localSubPath);
                                    }
                                }
                            }
                        }
                    }
                    // import master-file to link to the rest
                    AssetDatabase.ImportAsset(localPath);
                    AssetDatabase.Refresh();
                }
            }
            yield return localPath;
        }

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

#if UNITY_EDITOR
        [MenuItem("ARENA/Signout")]
#endif
        internal static void SceneSignout()
        {
            EditorApplication.ExitPlaymode();
            if (Directory.Exists(GoogleWebAuthorizationBroker.Folder))
                Directory.Delete(GoogleWebAuthorizationBroker.Folder, true);
            Debug.Log("Logged out of the ARENA");
        }

        // Add a menu item to create custom GameObjects.
        // Priority 1 ensures it is grouped with the other menu items of the same kind
        // and propagated to the hierarchy dropdown and hierarchy context menus.
#if UNITY_EDITOR
        [MenuItem("GameObject/ARENA/GLTF Model", false, 10)]
#endif
        internal static void CreateArenaGltfModel(MenuCommand menuCommand)
        {
            ArenaObjectAddUrlWindow window = (ArenaObjectAddUrlWindow)EditorWindow.GetWindow(typeof(ArenaObjectAddUrlWindow));
            window.Init("gltf-model", menuCommand);
            window.Show();
        }
#if UNITY_EDITOR
        [MenuItem("GameObject/ARENA/Image", false, 10)]
#endif
        internal static void CreateArenaImage(MenuCommand menuCommand)
        {
            ArenaObjectAddUrlWindow window = (ArenaObjectAddUrlWindow)EditorWindow.GetWindow(typeof(ArenaObjectAddUrlWindow));
            window.Init("image", menuCommand);
            window.Show();
        }

        private void CreateUpdateObject(string object_id, string storeType, dynamic data, string assetPath = null)
        {
            ArenaObject aobj = null;
            if (arenaObjs.TryGetValue(object_id, out GameObject gobj))
            {   // update local
                if (gobj != null)
                    aobj = gobj.GetComponent<ArenaObject>();
            }
            else
            {   // create local
                gobj = ArenaUnity.ToUnityObjectType(data);
                if (assetPath != null)
                {
                    switch ((string)data.object_type)
                    {
                        case "gltf-model":
                            GameObject mobj = Importer.LoadFromFile(assetPath);
                            mobj.transform.parent = gobj.transform;
                            break;
                        case "image":
                            Sprite sprite = LoadSpriteFromFile(assetPath);
                            SpriteRenderer spriteRenderer = gobj.AddComponent<SpriteRenderer>();
                            spriteRenderer.GetComponent<SpriteRenderer>().sprite = sprite;
                            spriteRenderer.drawMode = SpriteDrawMode.Sliced;
                            spriteRenderer.size = Vector2.one;
                            break;
                    }
                }
                gobj.transform.parent = ArenaClientTransform;
                gobj.name = object_id;
                arenaObjs.Add(object_id, gobj);
                aobj = gobj.AddComponent(typeof(ArenaObject)) as ArenaObject;
                aobj.created = true;
                aobj.messageType = storeType;
                aobj.parentId = (string)data.parent;
                aobj.persist = true;
                if ((string)data.object_type == "camera")
                {   // sync camera to main display if requested
                    Camera cam = gobj.GetComponent<Camera>();
                    if (cameraAutoSync && !cameraForDisplay.name.StartsWith("camera_"))
                    {
                        cam.targetDisplay = ArenaUnity.mainDisplay;
                        cameraForDisplay = cam;
                    }
                    else
                        cam.targetDisplay = ArenaUnity.secondDisplay;
                }
            }
            // modify Unity attributes
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
                if ((string)data.object_type == "gltf-model")
                    gobj.transform.rotation = ArenaUnity.GltfToUnityRotationQuat(gobj.transform.localRotation);
            }
            if (isElement(data.scale))
                gobj.transform.localScale = ArenaUnity.ToUnityScale(data.scale);
            if (isElement(data.material) || isElement(data.color))
                ArenaUnity.ToUnityMaterial(data, ref gobj);
            ArenaUnity.ToUnityDimensions(data, ref gobj);
            if ((string)data.object_type == "light")
                ArenaUnity.ToUnityLight(data, ref gobj);
            gobj.transform.hasChanged = false;
            if (aobj != null)
            {
                aobj.data = data;
                aobj.jsonData = JsonConvert.SerializeObject(aobj.data, Formatting.Indented);
            }
        }

        private static Sprite LoadSpriteFromFile(string assetPath)
        {
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
                Debug.LogError($"{www.error}: {www.url}");
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
            yield return www.SendWebRequest();
#if UNITY_2020_1_OR_NEWER
            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
#else
            if (www.isNetworkError || www.isHttpError)
#endif
            {
                Debug.LogError($"{www.error}: {www.url}");
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
            return result.ToString() != "UnityEngine.Networking.UnityWebRequestAsyncOperation";
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

        internal void Publish(string object_id, string msg)
        {
            byte[] payload = System.Text.Encoding.UTF8.GetBytes(msg);
            client.Publish($"{sceneTopic}/{client.ClientId}/{object_id}", payload, MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
            dynamic obj = JsonConvert.DeserializeObject(msg);
            LogMessage("Sent", obj);
        }

        protected override void OnConnecting()
        {
            base.OnConnecting();
            Debug.Log($"Connecting to broker on {brokerAddress}:{brokerPort}...");
        }

        protected override void OnConnected()
        {
            base.OnConnected();
            Debug.Log($"Connected to broker on {brokerAddress}");
        }

        protected override void SubscribeTopics()
        {
            client.Subscribe(new string[] { $"{sceneTopic}/#" }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
        }

        protected override void UnsubscribeTopics()
        {
            client.Unsubscribe(new string[] { $"{sceneTopic}/#" });
        }

        protected override void OnConnectionFailed(string errorMessage)
        {
            Debug.LogError($"CONNECTION FAILED! {errorMessage}");
        }

        protected override void OnDisconnected()
        {
            Debug.Log("Disconnected.");
        }

        protected override void OnConnectionLost()
        {
            Debug.LogWarning("CONNECTION LOST!");
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

        internal void ProcessMessage(string msg)
        {
            dynamic obj = JsonConvert.DeserializeObject(msg);
            LogMessage("Received", obj);
            StartCoroutine(ProcessArenaMessage(obj));
        }

        private IEnumerator ProcessArenaMessage(dynamic obj)
        {
            // consume object updates
            if (obj.type == "object")
            {
                switch ((string)obj.action)
                {
                    case "create":
                    case "update":
                        if (Convert.ToBoolean(obj.persist))
                        {
                            string localPath = null;
                            // TODO: fix coroutine error
                            //if (isElement(obj.data) && isElement(obj.data.url) && !isElementEmpty(obj.data.url))
                            //{
                            //    DisplayCancelableProgressBar("ARENA Message", $"Loading object-id: {(string)obj.object_id}", 0f);
                            //    CoroutineWithData cd = new CoroutineWithData(this, DownloadAssets(obj));
                            //    yield return cd.coroutine;
                            //    localPath = cd.result.ToString();
                            //    ClearProgressBar();
                            //}
                            CreateUpdateObject((string)obj.object_id, (string)obj.type, obj.data, localPath);
                        }
                        else if (obj.data.object_type == "camera") // try to manage camera
                        {
                            CreateUpdateObject((string)obj.object_id, (string)obj.type, obj.data);
                        }
                        break;
                    case "delete":
                        RemoveObject((string)obj.object_id);
                        break;
                    default:
                        break;
                }
                yield break;
            }
        }

        private void LogMessage(string dir, dynamic obj)
        {
            // determine logging level
            if (!Convert.ToBoolean(obj.persist) && !logMqttNonPersist) return;
            if (obj.type == "object")
            {
                if (obj.data != null && obj.data.object_type == "camera" && !logMqttUsers) return;
                if (!logMqttObjects) return;
            }
            if (obj.action == "clientEvent" && !logMqttEvents) return;
            Debug.Log($"{dir}: {JsonConvert.SerializeObject(obj)}");
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
