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
using UnityEngine;
using UnityEngine.Networking;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace ArenaUnity
{
    [HelpURL("https://arena.conix.io")]
    [DisallowMultipleComponent()]
    [AddComponentMenu("ArenaClient", 0)]
    public class ArenaClient : M2MqttUnityClient
    {
        // Singleton instance of this connection object
        public static ArenaClient Instance { get; private set; }

        protected override void Awake()
        {
            Instance = this;
        }
        [Header("ARENA Connection")]
        [Tooltip("Name of the topic realm for the scene.")]
        private string realm = "realm";
        [Tooltip("Name of the scene, without namespace ('example', not 'username/example'")]
        public string sceneName = "example";
        [Tooltip("Namespace (automated with username), but can be overridden")]
        public string namespaceName = null;
        [Space()]
        [Tooltip("Browser URL for the scene.")]
        [TextArea(minLines: 1, maxLines: 2)]
        public string sceneUrl = null;

        [Header("Performance")]
        [Tooltip("Console log MQTT object messages")]
        public bool logMqttObjects = true;
        [Tooltip("Console log MQTT user messages")]
        public bool logMqttUsers = false;
        [Tooltip("Console log MQTT client event messages")]
        public bool logMqttEvents = false;
        [Tooltip("Frequency to publish detected changes by frames (0 to stop)")]
        [Range(0, 60)]
        public int publishInterval = 30; // in publish per frames

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
        private static string ClientName = "ARENA Client Runtime";

        // local paths
        const string gAuthFile = ".arena_google_auth";
        const string mqttTokenFile = ".arena_mqtt_auth";
        const string userDirArena = ".arena";
        const string userSubDirUnity = "unity";
        static string userHomePath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
        private Transform arenaClientTransform;

        static string[] Scopes = {
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

        public void OnEnable()
        {
            // ensure consistant name and transform
            name = ClientName;
            transform.position = new Vector3(0f, 0f, 0f);
            transform.rotation = Quaternion.identity;
            transform.localScale = new Vector3(1f, 1f, 1f);
        }

        // Start is called before the first frame update
        protected override void Start()
        {
            StartCoroutine(SceneLogin());
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
            { // discover new objects
                foreach (Transform child in transform)
                {
                    ArenaObject aobj = child.gameObject.GetComponent<ArenaObject>();
                    if (aobj == null)
                    {
                        aobj = child.gameObject.AddComponent(typeof(ArenaObject)) as ArenaObject;
                        if (!arenaObjs.ContainsKey(child.name))
                            aobj.objectId = child.name;
                        else
                            aobj.objectId = $"{child.name}-{Random.Range(0, 1000000)}";
                        child.name = $"{aobj.objectId} ({ArenaUnity.ToArenaObjectType(aobj.gameObject)})";
                        arenaObjs.Add(aobj.objectId, child.gameObject);
                    }
                }
            }
        }

        private IEnumerator SceneLogin()
        {
            string sceneAuthDir = Path.Combine(userHomePath, userDirArena, userSubDirUnity, this.brokerAddress, "s");
            string gAuthPath = sceneAuthDir;
            string mqttTokenPath = Path.Combine(sceneAuthDir, mqttTokenFile);

            // get app credentials
            CoroutineWithData cd = new CoroutineWithData(this, HttpRequest($"https://{this.brokerAddress}/conf/gauth.json"));
            yield return cd.coroutine;
            var gauthId = cd.result.ToString();

            // request user auth
            UserCredential credential;
            using (var stream = ToStream(gauthId))
            {
                string applicationName = "ArenaClientCSharp";

                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                        GoogleClientSecrets.FromStream(stream).Secrets,
                        Scopes,
                        "user",
                        CancellationToken.None,
                        new FileDataStore(gAuthPath, true)).Result;
                Debug.Log($"Credential file saved to: {gAuthPath}");

                var oauthService = new Oauth2Service(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = applicationName,
                });

                var userInfo = oauthService.Userinfo.Get().Execute();

                this.email = userInfo.Email;
                this.idToken = credential.Token.IdToken;
            }

            // get arena web login token
            yield return HttpRequest($"https://{this.brokerAddress}/user/login");

            // get arena user account state
            WWWForm form = new WWWForm();
            form.AddField("id_token", idToken);
            cd = new CoroutineWithData(this, HttpRequest($"https://{this.brokerAddress}/user/user_state", csrfToken, form));
            yield return cd.coroutine;
            var user = JsonConvert.DeserializeObject<UserState>(cd.result.ToString());
            if (user.authenticated && (this.namespaceName == null || this.namespaceName.Trim() == ""))
                this.namespaceName = user.username;

            // get arena user mqtt token
            form.AddField("id_auth", "google-installed");
            form.AddField("username", user.username);
            form.AddField("realm", realm);
            form.AddField("scene", $"{namespaceName}/{sceneName}");
            cd = new CoroutineWithData(this, HttpRequest($"https://{this.brokerAddress}/user/mqtt_auth", csrfToken, form));
            yield return cd.coroutine;
            var auth = JsonConvert.DeserializeObject<MqttAuth>(cd.result.ToString());
            this.mqttUserName = auth.username;
            this.mqttPassword = auth.token;
            var handler = new JwtSecurityTokenHandler();
            JwtPayload payloadJson = handler.ReadJwtToken(auth.token).Payload;
            permissions = JValue.Parse(payloadJson.SerializeToJson()).ToString(Formatting.Indented);

            sceneTopic = $"{realm}/s/{namespaceName}/{sceneName}";
            sceneUrl = $"https://{this.brokerAddress}/{namespaceName}/{sceneName}";

            // get persistence objects
            cd = new CoroutineWithData(this, HttpRequest($"https://{this.brokerAddress}/persist/{namespaceName}/{sceneName}", csrfToken));
            yield return cd.coroutine;
            arenaClientTransform = GameObject.FindObjectOfType<ArenaClient>().transform;
            string jsonString = cd.result.ToString();
            JArray jsonVal = JArray.Parse(jsonString) as JArray;
            dynamic objects = jsonVal;
            // establish objects
            foreach (dynamic obj in objects)
            {
                if (obj.type == "object" || obj.attributes.position != null)
                {
                    CreateUpdateObject((string)obj.object_id, obj.attributes);
                }
            }
            // establish parent/child relationships
            foreach (KeyValuePair<string, GameObject> gobj in arenaObjs)
            {
                string parent = gobj.Value.GetComponent<ArenaObject>().parentId;
                if (parent != null)
                {
                    gobj.Value.GetComponent<ArenaObject>().transform.parent = arenaObjs[parent].transform;
                }
            }
            ImportGLTFAsync("/Users/mwfarb/git/arena-services-docker/ARENA-core/store/models/Duck.glb");
            base.Start();
        }

        void ImportGLTFAsync(string filepath)
        {
            Importer.ImportGLTFAsync(filepath, new ImportSettings(), OnFinishAsync);
        }

        void OnFinishAsync(GameObject result)
        {
            Debug.Log("Finished importing " + result.name);
        }

        private void CreateUpdateObject(string object_id, dynamic data)
        {
            GameObject gobj;
            ArenaObject aobj;
            if (arenaObjs.TryGetValue(object_id, out gobj))
            { // update
                aobj = gobj.GetComponent<ArenaObject>();
            }
            else
            { //create
                gobj = ArenaUnity.ToUnityObjectType((string)data.object_type);
                gobj.transform.parent = arenaClientTransform;
                gobj.name = $"{object_id} ({data.object_type})";
                arenaObjs.Add(object_id, gobj);
                aobj = gobj.AddComponent(typeof(ArenaObject)) as ArenaObject;
                aobj.objectId = object_id;
                aobj.parentId = (string)data.parent;
                aobj.persist = true;
            }
            // update Unity attributes
            foreach (JProperty attribute in data)
            {
                switch (attribute.Name)
                {
                    case "position":
                        if (data.position != null)
                            gobj.transform.position = ArenaUnity.ToUnityPosition(data.position);
                        break;
                    case "rotation":
                        if (data.rotation != null)
                        {
                            if (data.rotation.w != null) // quaternion
                                gobj.transform.rotation = ArenaUnity.ToUnityRotationQuat(data.rotation);
                            else // euler
                                gobj.transform.rotation = ArenaUnity.ToUnityRotationEuler(data.rotation);
                        }
                        break;
                    case "scale":
                        if (data.scale != null)
                            gobj.transform.localScale = ArenaUnity.ToUnityScale((string)data.object_type, data.scale);
                        break;
                    case "color":
                        if (data.color != null)
                        {
                            var renderer = gobj.GetComponent<Renderer>();
                            if (renderer != null)
                                renderer.material.SetColor("_Color", ArenaUnity.ToUnityColor((string)data.color));
                        }
                        break;
                }
            }
            // update ARENA attributes
            aobj.data = data;
            aobj.jsonData = aobj.data.ToString();
        }

        private void RemoveObject(string object_id)
        {
            GameObject gobj;
            if (arenaObjs.TryGetValue(object_id, out gobj))
            {
                Destroy(gobj);
            }
            arenaObjs.Remove(object_id);
        }

        private void generateArenaInternalTestObjs()
        {
            GameObject cubeT = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Transform arenaClientTransform = GameObject.FindObjectOfType<ArenaClient>().transform;
            for (int i = 0; i < 10; i++)
            {
                GameObject cube = Instantiate(cubeT, new Vector3(i * 2.0F, 0, 0), Quaternion.identity);
                cube.transform.parent = arenaClientTransform;
                ArenaObject obj = cube.AddComponent(typeof(ArenaObject)) as ArenaObject;
            }
        }

        public static Stream ToStream(string str)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(str);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        IEnumerator HttpRequest(string uri, string csrf = null, WWWForm form = null)
        {
            UnityWebRequest uwr = null;
            if (form == null)
                uwr = UnityWebRequest.Get(uri);
            else
                uwr = UnityWebRequest.Post(uri, form);
            if (csrf != null)
            {
                uwr.SetRequestHeader("Cookie", $"csrftoken={csrf}");
                uwr.SetRequestHeader("X-CSRFToken", csrf);
            }

            yield return uwr.SendWebRequest();

            if (uwr.isNetworkError)
            {
                Debug.Log($"Error While Sending: {uwr.error}");
                yield break;
            }
            else
            {
                // get the csrf cookie
                string SetCookie = uwr.GetResponseHeader("Set-Cookie");
                if (SetCookie != null)
                {
                    if (SetCookie.Contains("csrftoken="))
                        csrfToken = GetCookie(SetCookie, "csrftoken");
                    else if (SetCookie.Contains("csrf="))
                        csrfToken = GetCookie(SetCookie, "csrf");
                }

                Debug.Log($"Received: {uwr.downloadHandler.text}");
                yield return uwr.downloadHandler.text;
            }
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

        public void Publish(string object_id, string msg)
        {
            byte[] payload = System.Text.Encoding.UTF8.GetBytes(msg);
            client.Publish($"{sceneTopic}/{client.ClientId}/{object_id}", payload, MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
            dynamic obj = JsonConvert.DeserializeObject(msg);
            LogMessage("Sent", obj);
        }

        protected override void OnConnecting()
        {
            base.OnConnecting();
            Debug.Log($"Connecting to broker on {brokerAddress}:{brokerPort.ToString()}...\n");
        }

        protected override void OnConnected()
        {
            base.OnConnected();
            Debug.Log($"Connected to broker on {brokerAddress}\n");
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
            Debug.Log("CONNECTION LOST!");
        }

        protected override void DecodeMessage(string topic, byte[] message)
        {
            string msg = System.Text.Encoding.UTF8.GetString(message);
            StoreMessage(msg);
        }

        private void StoreMessage(string eventMsg)
        {
            eventMessages.Add(eventMsg);
        }

        private void ProcessMessage(string msg)
        {
            dynamic obj = JsonConvert.DeserializeObject(msg);
            // consume object updates
            if (obj.type == "object")
            {
                switch ((string)obj.action)
                {
                    case "create":
                    case "update":
                        if (System.Convert.ToBoolean(obj.persist))
                        {
                            CreateUpdateObject((string)obj.object_id, obj.data);
                        }
                        break;
                    case "delete":
                        RemoveObject((string)obj.object_id);
                        break;
                    default:
                        break;
                }
            }
            LogMessage("Received", obj);
        }

        private void LogMessage(string dir, dynamic obj)
        {
            // determine logging level
            if (obj.type == "object")
            {
                if (obj.data != null && obj.data.object_type == "camera" && !logMqttUsers) return;
                if (!logMqttObjects) return;
            }
            if (obj.action == "clientEvent" && !logMqttEvents) return;
            Debug.Log($"{dir}: {JsonConvert.SerializeObject(obj)}");
        }

        private void OnDestroy()
        {
            Disconnect();
        }

        private void OnValidate()
        {
            // TODO
        }
    }
}
