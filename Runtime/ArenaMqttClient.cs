/**
* Open source software under the terms in /LICENSE
* Copyright (c) 2021-2023, Carnegie Mellon University. All rights reserved.
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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
        [Tooltip("IP address or URL of the host running broker/auth/persist services (runtime changes ignored).")]
        public string hostAddress = "arenaxr.org";

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

        // internal variables
        private string idToken = null;
        protected string csrfToken = null;
        protected string fsToken = null;
        protected ArenaUserStateJson authState;
        private static UserCredential credential;

        // local paths
        const string gAuthFile = ".arena_google_auth";
        const string mqttTokenFile = ".arena_mqtt_auth";
        const string userDirArena = ".arena";
        const string userSubDirUnity = "unity";
        public string appFilesPath { get; private set; }
        public string userid { get; private set; }
        public string camid { get; private set; }
        public string networkLatencyTopic { get; private set; } // network graph latency update
        static readonly int networkLatencyIntervalMs = 10000; // run network latency update every 10s

        static readonly string[] Scopes = {
            Oauth2Service.Scope.UserinfoProfile,
            Oauth2Service.Scope.UserinfoEmail,
            Oauth2Service.Scope.Openid
        };

        public enum Auth { Anonymous, Google, Manual };
        public bool IsShuttingDown { get; internal set; }

        private List<byte[]> eventMessages = new List<byte[]>();

        // MQTT methods

        protected override void Awake()
        {
            base.Awake();
            // initialize arena-specific mqtt parameters
            brokerPort = 8883;
            isEncrypted = true;
            sslProtocol = MqttSslProtocols.TLSv1_2;
            if (hostAddress == "localhost")
            {
                verifyCertificate = false;
            }
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
        }

        public void Subscribe(string[] topics)
        {
            if (client != null) client.Subscribe(topics, new byte[] { MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE });
        }

        public void Unsubscribe(string[] topics)
        {
            if (client != null) client.Unsubscribe(topics);
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
            string userHomePath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            return Path.Combine(userHomePath, userDirArena, userSubDirUnity);
        }

        protected virtual void OnEnable()
        {
            appFilesPath = Application.isMobilePlatform ? Application.persistentDataPath : "";
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

        private IEnumerator Signin(string sceneName, string namespaceName, string realm, bool camera, string latencyTopic)
        {
            networkLatencyTopic = latencyTopic;
            string sceneAuthDir = Path.Combine(GetUnityAuthPath(), hostAddress, "s");
            string userGAuthPath = sceneAuthDir;
            string userMqttPath = Path.Combine(sceneAuthDir, mqttTokenFile);
            string mqttToken = null;
            CoroutineWithData cd;
#if UNITY_EDITOR && !( UNITY_ANDROID || UNITY_IOS )
            if (!Directory.Exists(sceneAuthDir))
            {
                Directory.CreateDirectory(sceneAuthDir);
            }
#endif
            if (hostAddress == "localhost")
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
                string tokenType = "";
                string userName = "";
                switch (authType)
                {
                    case Auth.Anonymous:
                        // prefix all anon users with "anonymous-"
                        Debug.Log("Using anonymous MQTT token.");
                        tokenType = "anonymous";
                        userName = $"anonymous-unity";
                        break;
                    case Auth.Google:
                        // get oauth app credentials
                        Debug.Log("Using remote-authenticated MQTT token.");
                        cd = new CoroutineWithData(this, HttpRequestAuth($"https://{hostAddress}/conf/gauth.json"));
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

                            var oauthService = new Oauth2Service(new BaseClientService.Initializer()
                            {
                                HttpClientInitializer = credential,
                                ApplicationName = applicationName,
                            });

                            var userInfo = oauthService.Userinfo.Get().Execute();

                            email = userInfo.Email;
                            idToken = credential.Token.IdToken;
                        }
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
                yield return HttpRequestAuth($"https://{hostAddress}/user/login");

                WWWForm form = new WWWForm();
                if (idToken != null) form.AddField("id_token", idToken);

                // get arena user account state
                cd = new CoroutineWithData(this, HttpRequestAuth($"https://{hostAddress}/user/user_state", csrfToken, form));
                yield return cd.coroutine;
                if (!isCrdSuccess(cd.result)) yield break;
                authState = JsonConvert.DeserializeObject<ArenaUserStateJson>(cd.result.ToString());
                if (authState.authenticated)
                {
                    userName = authState.username;

                    // get fs login
                    cd = new CoroutineWithData(this, HttpRequestAuth($"https://{hostAddress}/user/storelogin", csrfToken, form));
                    yield return cd.coroutine;
                    if (!isCrdSuccess(cd.result)) yield break;
                    if (string.IsNullOrWhiteSpace(fsToken))
                    {
                        Debug.LogError($"Invalid file store token = {fsToken}");
                        yield break;
                    }
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
                form.AddField("username", userName);
                if (camera)
                {
                    form.AddField("userid", "true");
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
                cd = new CoroutineWithData(this, HttpRequestAuth($"https://{hostAddress}/user/mqtt_auth", csrfToken, form));
                yield return cd.coroutine;
                if (!isCrdSuccess(cd.result)) yield break;
                mqttToken = cd.result.ToString();
#if UNITY_EDITOR && !( UNITY_ANDROID || UNITY_IOS )
                StreamWriter writer = new StreamWriter(userMqttPath);
                writer.Write(mqttToken);
                writer.Close();
#endif
            }

            var auth = JsonConvert.DeserializeObject<ArenaMqttAuthJson>(mqttToken);
            mqttUserName = auth.username;
            mqttPassword = auth.token;

            if (camera)
            {
                if (auth == null || auth.ids == null || auth.ids.userid == null || auth.ids.camid == null)
                {
                    Debug.LogError("Missing required userid and camid!!!! Do not 'publish camera' if this is Manual auth.");
                    yield break;
                }
                userid = auth.ids.userid;
                camid = auth.ids.camid;

                // TODO: will message can only delete the primary camera object, need a solution for multiple cameras

                willFlag = camera;
                willTopic = $"{realm}/s/{namespaceName}/{sceneName}/{camid}";
                ArenaObjectJson msg = new ArenaObjectJson
                {
                    object_id = camid,
                    action = "delete",
                };
                willMessage = JsonConvert.SerializeObject(msg);
            }

            string payloadJson = Base64UrlDecode(auth.token.Split('.')[1]);
            JObject payload = JObject.Parse(payloadJson);
            permissions = JToken.Parse(payloadJson).ToString(Formatting.Indented);
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
        internal static void SignoutArena()
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

    }
}
