/**
* Open source software under the terms in /LICENSE
* Copyright (c) 2021-2023, Carnegie Mellon University. All rights reserved.
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Oauth2.v2;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using M2MqttUnity;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
#endif
using UnityEngine;
using UnityEngine.Networking;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace ArenaUnity
{
    public class ArenaMqttClient : M2MqttUnityClient
    {
        [Header("ARENA MQTT configuration")]
        [Tooltip("Connect as Anonymous, Google authenticated, or Manual (advanced) JWT user (runtime changes ignored).")]
        public Auth authType = Auth.Google;
        [Tooltip("Build is headless and cannot use browser based auth. Forces limited input auth (runtime changes ignored).")]
        public bool headless = false;
        [Tooltip("IP address or URL of the host running broker/auth/persist services (runtime changes ignored).")]
        public string hostAddress = "arenaxr.org";
        [Tooltip("Namespace (automated with username), but can be overridden (runtime changes ignored).")]
        public string namespaceName = null;
        [Tooltip("Name of the scene, without namespace ('example', not 'username/example', runtime changes ignored).")]
        public string sceneName = "example";

        [Header("Performance")]
        [Tooltip("Console log MQTT scene object messages")]
        public bool logMqttSceneObjects = false;
        [Tooltip("Console log MQTT user object messages")]
        public bool logMqttUserObjects = false;
        [Tooltip("Console log MQTT user presense messages")]
        public bool logMqttUserPresense = false;
        [Tooltip("Console log MQTT render fusion messsages")]
        public bool logMqttRemoteRender = false;
        [Tooltip("Console log MQTT scene chat messsages")]
        public bool logMqttChats = false;
        [Tooltip("Console log MQTT program messages")]
        public bool logMqttPrograms = false;
        [Tooltip("Console log MQTT environment messages")]
        public bool logMqttEnvironment = false;
        [Tooltip("Console log MQTT debug messages")]
        public bool logMqttDebug = false;
        [Tooltip("Global publish frequency to publish detected transform changes (milliseconds)")]
        [Range(100, 1000)]
        public int globalUpdateMs = 100;

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
        /// Manual request for remote render "r" rights, pending account allowances.
        /// </summary>
        public bool requestRemoteRenderRights { get; set; }
        /// <summary>
        /// Manual request for environment "e" rights, pending account allowances.
        /// </summary>
        public bool requestEnvironmentRights { get; set; }

        // internal variables
        private string id_token = null;
        private string refresh_token = null;
        protected string csrfToken = null;
        protected string fsToken = null;
        protected ArenaUserStateJson authState;
        private string creds = null;
        private bool showDeviceAuthWindow = false;
        private string dev_verification_url = null;
        private string dev_user_code = null;

        // local paths
        const string gAuthFile = ".arena_google_auth";
        const string mqttTokenFile = ".arena_mqtt_auth";
        const string userDirArena = ".arena";
        const string userSubDirUnity = "unity";
        private const string packageNameRenderFusion = "io.conix.arena.renderfusion";

        public string appFilesPath { get; private set; }
        public string username { get; private set; }
        public string userid { get; private set; }
        public string userclient { get; private set; }
        public string camid { get; private set; }
        public string handleftid { get; private set; }
        public string handrightid { get; private set; }
        public string networkLatencyTopic { get; private set; } // network graph latency update
        public ArenaMqttTokenClaimsJson perms { get; private set; }
        static readonly int networkLatencyIntervalMs = 10000; // run network latency update every 10s
        protected const int msgTypeRenderIdx = (int)ArenaTopicTokens.SCENE_MSGTYPE;

        static readonly string[] Scopes = {
            Oauth2Service.Scope.UserinfoProfile,
            Oauth2Service.Scope.UserinfoEmail,
            Oauth2Service.Scope.Openid
        };

        public enum Auth { Anonymous, Google, Manual };
        public bool IsShuttingDown { get; internal set; }

        private List<byte[]> eventMessages = new List<byte[]>();
        protected Dictionary<ushort, string> subscriptions = new Dictionary<ushort, string>();
#if UNITY_EDITOR
        private ListRequest packageListRequest;
#endif

        // MQTT methods

        protected override void Awake()
        {
            base.Awake();
            // initialize arena-specific mqtt parameters
            brokerPort = 8883;
            isEncrypted = true;
            sslProtocol = MqttSslProtocols.TLSv1_2;
            if (hostAddress == "localhost" || hostAddress.EndsWith(".local"))
            {
                verifyCertificate = false;
            }
#if UNITY_EDITOR
            packageListRequest = Client.List(true); // request offline packages installed
#endif
            StartCoroutine(PublishTickLatency());
        }

        IEnumerator PublishTickLatency()
        {
            while (true)
            {
                if (mqttClientConnected && !string.IsNullOrEmpty(networkLatencyTopic))
                {
                    // publish empty message with QoS of 2 to update latency
                    client.Publish(networkLatencyTopic, new byte[] { }, MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);
                }
                yield return new WaitForSeconds(networkLatencyIntervalMs / 1000);
            }
        }

        protected override void Update()
        {
            base.Update(); // call ProcessMqttEvents()

            if (eventMessages.Count > 0)
            {
                foreach (byte[] msg in eventMessages)
                {
                    ProcessMessage(msg);
                }
                eventMessages.Clear();
            }
        }

        protected virtual void ProcessMessage(byte[] msg)
        {
            Debug.LogFormat("Message received of length: {0}", msg.Length);
        }

        private void StoreMessage(byte[] eventMsg)
        {
            eventMessages.Add(eventMsg);
        }

        protected override void DecodeMessage(string topic, byte[] message)
        {
            StoreMessage(message);
        }

        public void Publish(string topic, byte[] payload)
        {
            if (client != null) client.Publish(topic, payload);
            var topicSplit = topic.Split("/");
            if (topicSplit.Length > msgTypeRenderIdx)
            {
                bool hasPermissions = HasPerms(topic);
                LogMessage("Sending", topicSplit[4], topic, System.Text.Encoding.UTF8.GetString(payload), hasPermissions);
            }
        }

        public void Subscribe(string topic)
        {
            if (client != null)
            {
                var mid = client.Subscribe(new string[] { topic }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE });
                subscriptions[mid] = topic;
            }
        }

        public void Unsubscribe(string[] topics)
        {
            if (client != null) { client.Unsubscribe(topics); }
        }

        protected bool HasPerms(string topic)
        {
            foreach (string pubperm in perms.publ)
            {
                if (MqttTopicMatch(pubperm, topic)) return true;
            }
            return false;
        }

        protected void LogMessage(string dir, string sceneMsgType, string topic, string msg, bool hasPermissions = true)
        {
            bool log = !hasPermissions; // default log any permission failure
            // determine logging level for permission success
            switch (sceneMsgType)
            {
                case "x":
                    if (logMqttUserPresense) log = true;
                    break;
                case "o":
                    if (logMqttSceneObjects) log = true;
                    break;
                case "u":
                    if (logMqttUserObjects) log = true;
                    break;
                case "c":
                    if (logMqttChats) log = true;
                    break;
                case "r":
                    if (logMqttRemoteRender) log = true;
                    break;
                case "p":
                    if (logMqttPrograms) log = true;
                    break;
                case "d":
                    if (logMqttDebug) log = true;
                    break;
                case "e":
                    if (logMqttEnvironment) log = true;
                    break;
                default:
                    break;
            }
            if (log)
            {
                if (hasPermissions)
                    Debug.Log($"{dir}: {topic} {msg}");
                else
                    Debug.LogWarning($"Permissions FAILED {dir}: {topic} {msg}");
            }
        }

        protected void OnDestroy()
        {
            Disconnect();
        }

        protected override void OnApplicationQuit()
        {
            IsShuttingDown = true;
            base.OnApplicationQuit();
        }

        // Auth methods

        internal static string GetUnityAuthPath()
        {
#if UNITY_EDITOR && !(UNITY_ANDROID || UNITY_IOS)
            string userHomePath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
#else
            string userHomePath = Application.persistentDataPath;
#endif
            return Path.Combine(userHomePath, userDirArena, userSubDirUnity);
        }

        protected virtual void OnEnable()
        {
            appFilesPath = Application.isMobilePlatform ? Application.persistentDataPath : "";
            if (Application.isMobilePlatform)
            {
                headless = true;
            }
        }

        void ShowDeviceAuthWindow(int windowID)
        {
            GUIUtility.systemCopyBuffer = dev_user_code;
            GUILayout.Label($"Enter device code <b>{dev_user_code}</b> at webpage <b>{dev_verification_url}</b>.");
            GUILayout.Label($"The device code has been copied to your clipboard.");
            GUILayout.Label($"Click the button below to open webpage.");

            if (GUILayout.Button($"Go to <a href='{dev_verification_url}'>{dev_verification_url}</a>"))
            {
                Application.OpenURL(dev_verification_url);
            }
        }

        void OnGUI()
        {
            // show device auth window if headless mode for user auth, do not draw otherwise
            if (showDeviceAuthWindow)
            {
                int windowWidth = (int)(Screen.width * .5);
                int windowHeight = (int)(Screen.height * .5);
                int x = (Screen.width - windowWidth) / 2;
                int y = (Screen.height - windowHeight) / 2;
                var winRect = new Rect(x, y, windowWidth, windowHeight);

                GUI.ModalWindow(0, winRect, ShowDeviceAuthWindow, "ARENA Device Authorization");
            }
        }

        /// <summary>
        /// Sign into the ARENA for a specific scene, optionally using a user avatar camera, per user account.
        /// </summary>
        protected IEnumerator SigninScene(string sceneName, string namespaceName, string realm, bool camera, string latencyTopic = null)
        {
            return Signin(sceneName, namespaceName, realm, camera, latencyTopic);
        }

        /// <summary>
        /// Sign into the ARENA for any available credentials based on authType per user account.
        /// </summary>
        protected IEnumerator Signin()
        {
            return Signin(null, null, null, false, null);
        }

        private IEnumerator Signin(string sceneName, string namespaceName, string realm, bool hasArenaCamera, string latencyTopic)
        {
            networkLatencyTopic = latencyTopic;
            string sceneAuthDir = Path.Combine(GetUnityAuthPath(), hostAddress, "s");
            string userGAuthPath = Path.Combine(sceneAuthDir, gAuthFile);
            string userMqttPath = Path.Combine(sceneAuthDir, mqttTokenFile);
            string mqttToken = null;
            CoroutineWithData cd;
            if (!Directory.Exists(sceneAuthDir))
            {
                Directory.CreateDirectory(sceneAuthDir);
            }
            if (hostAddress == "localhost" || hostAddress.EndsWith(".local"))
            {
                verifyCertificate = false;
            }

            string localMqttPath = Path.Combine(appFilesPath, mqttTokenFile);
            if (File.Exists(localMqttPath))
            {
                // check for local mqtt auth
                Debug.LogWarning("Using local MQTT token.");
                if (authType != Auth.Manual)
                {
                    Debug.LogError($"Authentication type '{authType}' when using local token may create ambiguous results. Switch to '{Auth.Manual}'.");
                    yield break;
                }
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
                WWWForm form = null;
                string tokenType = "";
                switch (authType)
                {
                    case Auth.Anonymous:
                        // prefix all anon users with "anonymous-"
                        Debug.Log("Using anonymous MQTT token.");
                        tokenType = "anonymous";
                        username = $"anonymous-unity";
                        break;
                    case Auth.Google:
                        // get oauth app credentials
                        Debug.Log("Using remote-authenticated MQTT token.");
                        if (headless)
                            cd = new CoroutineWithData(this, HttpRequestAuth($"https://{hostAddress}/conf/gauth-device.json"));
                        else
                            cd = new CoroutineWithData(this, HttpRequestAuth($"https://{hostAddress}/conf/gauth.json"));
                        yield return cd.coroutine;
                        if (!isCrdSuccess(cd.result)) yield break;
                        string gAuthId = cd.result.ToString();
                        JObject joGAuth = JObject.Parse(gAuthId);
                        string client_id = (string)joGAuth["installed"]["client_id"];
                        string client_secret = (string)joGAuth["installed"]["client_secret"];

                        // load user credentials
                        if (File.Exists(userGAuthPath))
                        {
                            using (var sr = new StreamReader(userGAuthPath))
                            {
                                creds = sr.ReadToEnd();
                            }
                        }
                        if (creds != null)
                        {
                            JObject joUserCredsPre = JObject.Parse(creds);
                            id_token = (string)joUserCredsPre["id_token"];
                            string refresh_token = (string)joUserCredsPre["refresh_token"];
                        }
                        if (id_token != null)
                        {
                            string payloadIdPre = Base64UrlDecode(id_token.Split('.')[1]);
                            JObject joIdTokenPre = JObject.Parse(payloadIdPre);
                            string aud = (string)joIdTokenPre["aud"];
                            long exp_s = (long)joIdTokenPre["exp"];
                            // for reuse, client_id must still match
                            if (aud != client_id) creds = null;  // switched auth systems
                            if (exp_s * 1000 <= DateTimeOffset.Now.ToUnixTimeMilliseconds()) creds = null;  // expired token
                        }

                        if (creds != null)
                        {
                            Debug.Log("Using cached Google authentication.");
                        }
                        else
                        {
                            if (refresh_token != null)
                            {
                                Debug.Log("Requesting refreshed Google authentication.");
                                form = new WWWForm();
                                form.AddField("client_id", client_id);
                                form.AddField("client_secret", client_secret);
                                form.AddField("refresh_token", refresh_token);
                                form.AddField("grant_type", refresh_token);
                                cd = new CoroutineWithData(this, HttpRequest("https://oauth2.googleapis.com/token", form));
                                yield return cd.coroutine;
                                if (!isCrdSuccess(cd.result)) yield break;
                                creds = cd.result.ToString();
                            }
                            else
                            {
                                // if no credentials available, let the user log in.
                                if (headless)
                                {
                                    // limited input device auth flow for local client
                                    // get device code
                                    form = new WWWForm();
                                    form.AddField("client_id", client_id);
                                    form.AddField("scope", "email profile");
                                    cd = new CoroutineWithData(this, HttpRequest("https://oauth2.googleapis.com/device/code", form));
                                    yield return cd.coroutine;
                                    if (!isCrdSuccess(cd.result)) yield break;
                                    string device_resp = cd.result.ToString();

                                    // render user code/link and poll for OOB response
                                    JObject joDeviceResp = JObject.Parse(device_resp);
                                    dev_verification_url = (string)joDeviceResp["verification_url"];
                                    dev_user_code = (string)joDeviceResp["user_code"];
                                    string device_code = (string)joDeviceResp["device_code"];
                                    long exp_s = (long)joDeviceResp["expires_in"];
                                    long interval_s = (long)joDeviceResp["interval"];
                                    showDeviceAuthWindow = true; // render gui device auth instructions
                                    Debug.LogWarning($"ARENA Device Authorization: Enter device code {dev_user_code} at webpage  {dev_verification_url}");
                                    var exp_ms = DateTimeOffset.Now.ToUnixTimeMilliseconds() + (exp_s * 1000);

                                    // begin device poll
                                    while (true)
                                    {
                                        yield return new WaitForSecondsRealtime(interval_s);

                                        if (DateTimeOffset.Now.ToUnixTimeMilliseconds() > exp_ms)
                                        {
                                            Debug.LogError($"Device auth request expired after {exp_s / 60} minutes.");
                                            yield break;
                                        }

                                        // try token request
                                        form = new WWWForm();
                                        form.AddField("client_id", client_id);
                                        form.AddField("client_secret", client_secret);
                                        form.AddField("device_code", device_code);
                                        form.AddField("grant_type", "urn:ietf:params:oauth:grant-type:device_code");
                                        cd = new CoroutineWithData(this, HttpRequest("https://oauth2.googleapis.com/token", form));
                                        yield return cd.coroutine;
                                        if (cd.result.ToString() == "428") continue;
                                        if (!isCrdSuccess(cd.result)) yield break;
                                        creds = cd.result.ToString();
                                        showDeviceAuthWindow = false; // remove gui device auth instructions
                                        break;
                                    }
                                }
                                else
                                {
                                    // automated browser flow for local client
                                    using (var stream = ToStream(gAuthId))
                                    {
                                        var credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                                                GoogleClientSecrets.FromStream(stream).Secrets,
                                                Scopes,
                                                "user",
                                                CancellationToken.None,
                                                new NullDataStore()).Result;
                                        creds = JsonConvert.SerializeObject(credential.Token);
                                    }
                                }
                            }
                        }
                        // save user credentials
                        using (FileStream fs = File.Create(userGAuthPath))
                        {
                            byte[] info = new UTF8Encoding(true).GetBytes(creds);
                            fs.Write(info, 0, info.Length);
                        }
                        // use credentials locally
                        JObject joUserCredsPost = JObject.Parse(creds);
                        id_token = (string)joUserCredsPost["id_token"];
                        string payloadIdPost = Base64UrlDecode(id_token.Split('.')[1]);
                        JObject joIdTokenPost = JObject.Parse(payloadIdPost);
                        email = (string)joIdTokenPost.SelectToken("email");
                        tokenType = "google-installed";
                        break;
                    case Auth.Manual:
                        Debug.LogError($"Authentication type Manual missing local token file: {localMqttPath}.");
                        yield break;
                    default:
                        Debug.LogError($"Invalid ARENA authentication type: '{tokenType}'");
                        yield break;
                }

                // get arena CSRF token
                yield return HttpRequestAuth($"https://{hostAddress}/user/v2/login");

                form = new WWWForm();
                if (id_token != null) form.AddField("id_token", id_token);

                // get arena user account state
                cd = new CoroutineWithData(this, HttpRequestAuth($"https://{hostAddress}/user/v2/user_state", csrfToken, form));
                yield return cd.coroutine;
                if (!isCrdSuccess(cd.result)) yield break;
                authState = JsonConvert.DeserializeObject<ArenaUserStateJson>(cd.result.ToString());
                if (authState.authenticated)
                {
                    username = authState.username;
                }
                if (string.IsNullOrWhiteSpace(namespaceName))
                {
                    if (authState.authenticated)
                    {
                        namespaceName = authState.username;
                    }
                    else
                    {
                        namespaceName = "public";
                    }
                }

                // get arena user mqtt token
                form.AddField("id_auth", tokenType);
                form.AddField("username", username);
                // always request user-specific context
                form.AddField("client", "unity");
                form.AddField("userid", "true");
                if (hasArenaCamera)
                {
                    form.AddField("camid", "true");
                }
                if (!string.IsNullOrWhiteSpace(realm))
                {
                    form.AddField("realm", realm);
                }
                // handle full ARENA scene
                if (!string.IsNullOrWhiteSpace(sceneName))
                {
                    form.AddField("scene", $"{namespaceName}/{sceneName}");
                }
                // manual rights requests
                if (requestRemoteRenderRights)
                {
                    form.AddField("renderfusionid", "true");
                }
                if (requestEnvironmentRights)
                {
                    form.AddField("environmentid", "true");
                }
#if UNITY_EDITOR
                // auto-test for render fusion, request permissions if so
                if (packageListRequest.IsCompleted)
                {
                    if (packageListRequest.Status == StatusCode.Success)
                        foreach (var package in packageListRequest.Result)
                            if (package.name == packageNameRenderFusion)
                            {
                                form.AddField("renderfusionid", "true");
                                requestRemoteRenderRights = true; // for display purposes
                            }
                            else if (packageListRequest.Status >= StatusCode.Failure)
                                Debug.LogWarning(packageListRequest.Error.message);
                }
                else
                {
                    Debug.LogWarning("Package Manager unable to query for render-fusion package!");
                }
#endif
                // request token endpoint
                cd = new CoroutineWithData(this, HttpRequestAuth($"https://{hostAddress}/user/v2/mqtt_auth", csrfToken, form));
                yield return cd.coroutine;
                if (!isCrdSuccess(cd.result)) yield break;
                mqttToken = cd.result.ToString();
#if UNITY_EDITOR && !(UNITY_ANDROID || UNITY_IOS)
                StreamWriter writer = new StreamWriter(userMqttPath);
                writer.Write(mqttToken);
                writer.Close();
#endif
            }

            var auth = JsonConvert.DeserializeObject<ArenaMqttAuthJson>(mqttToken);
            // validate received token
            if (auth == null || auth.username == null || auth.token == null)
            {
                Debug.LogError("Missing required jwt auth!!!!");
                yield break;
            }
            mqttUserName = auth.username;
            mqttPassword = auth.token;

            // validate userids
            if (auth.ids == null || auth.ids.userid == null || auth.ids.userclient == null)
            {
                Debug.LogError("Missing required user ids!!!!");
                yield break;
            }
            userid = auth.ids.userid;
            userclient = auth.ids.userclient;

            // publishing cam/hands? then last will is required
            if (hasArenaCamera)
            {
                if (auth.ids.camid == null)
                {
                    Debug.LogError("Missing required camid!!!! Do not 'publish camera' if this is Manual auth.");
                    yield break;
                }
                camid = auth.ids.camid;
                handleftid = auth.ids.handleftid;
                handrightid = auth.ids.handrightid;

                // will message can only remove the primary user presence
                var lwtTopic = new ArenaTopics(
                    realm: realm,
                    name_space: namespaceName,
                    scenename: sceneName,
                    userclient: userclient,
                    idtag: userid
                );
                willFlag = hasArenaCamera;
                willTopic = lwtTopic.PUB_SCENE_PRESENCE;
                ArenaObjectJson msg = new ArenaObjectJson
                {
                    object_id = userid,
                    action = "leave",
                };
                willMessage = JsonConvert.SerializeObject(msg);
                Debug.Log($"MQTT Last will: {willTopic} {willMessage}");
            }

            string payloadJson = Base64UrlDecode(auth.token.Split('.')[1]);
            JObject payload = JObject.Parse(payloadJson);
            permissions = JToken.Parse(payloadJson).ToString(Formatting.Indented);
            perms = JsonConvert.DeserializeObject<ArenaMqttTokenClaimsJson>(permissions);
            if (string.IsNullOrWhiteSpace(namespaceName))
            {
                namespaceName = (string)payload.SelectToken("sub");
            }
            mqttExpires = (long)payload.SelectToken("exp");
            DateTimeOffset dateTimeOffSet = DateTimeOffset.FromUnixTimeSeconds(mqttExpires);
            TimeSpan duration = dateTimeOffSet.DateTime.Subtract(DateTime.Now.ToUniversalTime());
            Debug.Log($"MQTT Token expires in {ArenaUnity.TimeSpanToString(duration)}");

            // background mqtt connect
            Connect();
            yield return namespaceName;
        }

        protected bool ConfirmGoogleAuth()
        {
            return (id_token != null);
        }

        protected IEnumerator GetFSLoginForUser()
        {
            WWWForm form = new WWWForm();
            if (id_token != null) form.AddField("id_token", id_token);

            CoroutineWithData cd = new CoroutineWithData(this, HttpRequestAuth($"https://{hostAddress}/user/v2/storelogin", csrfToken, form));
            yield return cd.coroutine;
            if (!isCrdSuccess(cd.result)) yield break;
            if (string.IsNullOrWhiteSpace(fsToken))
            {
                Debug.LogError($"Invalid file store token = {fsToken}");
                yield break;
            }
            else
            {
                yield return true;
            }
        }

        public static string Base64UrlDecode(string base64)
        {
            string base64Padded = base64.PadRight(base64.Length + (4 - base64.Length % 4) % 4, '=');
            return Encoding.UTF8.GetString(Convert.FromBase64String(base64Padded));
        }

        /// <summary>
        /// Remove ARENA authentication.
        /// </summary>
#if UNITY_EDITOR
        [MenuItem("ARENA/Signout")]
#endif
        public static void SignoutArena()
        {
#if UNITY_EDITOR
            if (Application.isPlaying)
                EditorApplication.ExitPlaymode();
#endif
            string unityAuthPath = GetUnityAuthPath();
            if (Directory.Exists(unityAuthPath))
                Directory.Delete(unityAuthPath, true);
            Debug.Log("Signed out of the ARENA.");
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

        protected IEnumerator HttpRequest(string url, WWWForm form = null)
        {
            UnityWebRequest www;
            if (form == null)
                www = UnityWebRequest.Get(url);
            else
                www = UnityWebRequest.Post(url, form);
            yield return www.SendWebRequest();
            if (www.responseCode == 428)
            {
                yield return www.responseCode.ToString();
            }
#if UNITY_2020_1_OR_NEWER
            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
#else
            if (www.isNetworkError || www.isHttpError)
#endif
            {
                Debug.LogWarning($"{www.error}: {www.url}");
                if (!string.IsNullOrWhiteSpace(www.downloadHandler?.text))
                {
                    Debug.LogWarning(www.downloadHandler.text);
                }
                yield break;
            }
            else
            {
                yield return www.downloadHandler.text;
            }
        }

        protected IEnumerator HttpRequestAuth(string url, string csrf = null, WWWForm form = null)
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
            if (!verifyCertificate)
                www.certificateHandler = new SelfSignedCertificateHandler();
            yield return www.SendWebRequest();
#if UNITY_2020_1_OR_NEWER
            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
#else
            if (www.isNetworkError || www.isHttpError)
#endif
            {
                Debug.LogWarning($"{www.error}: {www.url}");
                if (!string.IsNullOrWhiteSpace(www.downloadHandler?.text))
                {
                    Debug.LogWarning(www.downloadHandler.text);
                }
                if (www.responseCode == 401 || www.responseCode == 403)
                {
                    Debug.LogWarning($"Do you have a valid ARENA account on {www.uri.Host}?");
                    Debug.LogWarning($"Create an account in a web browser at: {www.uri.Scheme}{www.uri.Host}/user");
                }
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
                    if (SetCookie.Contains("auth="))
                        fsToken = GetCookie(SetCookie, "auth");
                }
                yield return www.downloadHandler.text;
            }
        }

        protected bool isCrdSuccess(object result)
        {
            return result != null && result.ToString() != "UnityEngine.Networking.UnityWebRequestAsyncOperation";
        }

        private string GetCookie(string SetCookie, string cookieTag)
        {
            string csrfCookie = null;
            Regex rxCookie = new Regex($"(^| ){cookieTag}=([^;]+)");
            MatchCollection cookieMatches = rxCookie.Matches(SetCookie);
            if (cookieMatches.Count > 0)
                csrfCookie = cookieMatches[0].Groups[2].Value;
            return csrfCookie;
        }

        public static bool MqttTopicMatch(string allowTopic, string attemptTopic)
        {
            var allowedRegex = allowTopic.Replace(@"/", @"\/").Replace("+", @"[a-zA-Z0-9 _+.-]*").Replace("#", @"[a-zA-Z0-9 \/_#+.-]*");
            var re = new Regex(allowedRegex);
            var matches = re.Matches(attemptTopic);
            foreach (var match in matches.ToList())
            {
                if (match.Value == attemptTopic)
                {
                    return true;
                }
            }
            return false;
        }

    }
}
