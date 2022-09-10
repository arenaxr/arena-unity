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
using UnityEngine;
using UnityEngine.Networking;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace ArenaUnity
{
    public class ArenaMqttClient : M2MqttUnityClient
    {
        [Header("ARENA MQTT configuration")]
        [Tooltip("Connect as Anonymous user, or Google authenticated user.")]
        public Auth authType = Auth.Google;
        [Tooltip("IP address or URL of the host running broker/auth/persist services.")]
        public string hostAddress = "mqtt.arenaxr.org";

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
        private static UserCredential credential;

        // local paths
        const string gAuthFile = ".arena_google_auth";
        const string mqttTokenFile = ".arena_mqtt_auth";
        const string userDirArena = ".arena";
        const string userSubDirUnity = "unity";
        protected string userHomePath { get; private set; }
        protected string appFilesPath { get; private set; }
        internal string userid { get; private set; }
        internal string camid { get; private set; }

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
            public MqttAuthIds ids { get; set; }
        }

        public class MqttAuthIds
        {
            public string userid { get; set; }
            public string camid { get; set; }
        }

        public enum Auth { Anonymous, Google };
        public bool IsShuttingDown { get; internal set; }


        private List<byte[]> eventMessages = new List<byte[]>();

        // MQTT methods

        protected override void Awake()
        {
            base.Awake();
            // initialize arena-specific mqtt parameters
            brokerAddress = hostAddress;
            brokerPort = 8883;
            isEncrypted = true;
            sslProtocol = MqttSslProtocols.TLSv1_2;
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
            client.Publish(topic, payload);
        }

        public void Subscribe(string[] topics)
        {
            client.Subscribe(topics, new byte[] { MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE });
        }

        public void Unsubscribe(string[] topics)
        {
            client.Unsubscribe(topics);
        }

        protected void OnDestroy()
        {
            Disconnect();
        }

        protected new void OnApplicationQuit()
        {
            IsShuttingDown = true;
            base.OnApplicationQuit();
        }

        // Auth methods

        protected virtual void OnEnable()
        {
            userHomePath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            appFilesPath = Application.isMobilePlatform ? Application.persistentDataPath : "";
        }

        protected IEnumerator SceneSignin(string sceneName = null, string namespaceName = null, string realm = null, bool will = false)
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
                string tokenType = "";
                string userName = "";
                switch (authType)
                {
                    case Auth.Anonymous:
                        // prefix all anon users with "anonymous-"
                        tokenType = "anonymous";
                        userName = $"anonymous-unity-{UnityEngine.Random.Range(0, 1000000)}";
                        if (string.IsNullOrWhiteSpace(namespaceName))
                            namespaceName = "public";
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
                        tokenType = "google-installed";
                        break;
                    default:
                        Debug.LogWarning($"Invalid ARENA authentication type: '{tokenType}'");
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
                    userName = user.username;
                }
                // get arena user mqtt token
                form.AddField("id_auth", tokenType);
                form.AddField("username", userName);
                form.AddField("realm", realm);
                form.AddField("userid", "true");
                form.AddField("camid", "true");
                // handle full ARENA scene
                if (namespaceName != null && sceneName != null)
                {
                    form.AddField("scene", $"{namespaceName}/{sceneName}");
                }
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
            userid = auth.ids.userid;
            camid = auth.ids.camid;
            if (will)
            {
                willFlag = will;
                willTopic = $"{realm}/{namespaceName}/{sceneName}/{camid}";
                willMessage = $"{{'object_id':'{camid}','action':'delete'}}";
            }
            Debug.Log($"will {willFlag} {willTopic} {willMessage}");
            var handler = new JwtSecurityTokenHandler();
            JwtPayload payloadJson = handler.ReadJwtToken(auth.token).Payload;
            permissions = JToken.Parse(payloadJson.SerializeToJson()).ToString(Formatting.Indented);
            if (string.IsNullOrWhiteSpace(namespaceName))
                namespaceName = payloadJson.Sub;
            mqttExpires = (long)payloadJson.Exp;
            DateTimeOffset dateTimeOffSet = DateTimeOffset.FromUnixTimeSeconds(mqttExpires);
            TimeSpan duration = dateTimeOffSet.DateTime.Subtract(DateTime.Now.ToUniversalTime());
            Debug.Log($"MQTT Token expires in {ArenaUnity.TimeSpanToString(duration)}");

            // background mqtt connect
            Connect();
            yield return namespaceName;
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

        protected bool isCrdSuccess(object result)
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

    }

}
