using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
using uPLibrary.Networking.M2Mqtt.Messages;
//using Siccity.GLTFUtility;

//[Serializable]
//public struct Permissions
//{
//    [SerializeField] private string mqttPayload;

//    public string MqttPayload => mqttPayload;
//}

[HelpURL("https://arena.conix.io")]
[DisallowMultipleComponent()]
public class ArenaClient : M2MqttUnityClient
{
    // Singleton instance of this connection object
    public static ArenaClient Instance { get; private set; }

    protected override void Awake()
    {
        Instance = this;
    }

    [Header("ARENA Configuration")]
    [Tooltip("Name of the topic realm for the scene.")]
    private string realm = "realm";
    [Tooltip("Name of the scene, without namespace ('example', not 'username/example'")]
    public string sceneName = "example";
    [ReadOnly]
    [Tooltip("Authenticated user email account.")]
    public string email = null;
    [ReadOnly]
    [Tooltip("Browser URL for the scene.")]
    [TextArea(minLines: 1, maxLines: 2)]
    public string sceneUrl = null;

    [Header("Optional Parameters")]
    [Tooltip("Namespace (automated with username), but can be overridden")]
    public string namespaceName = null;

    //[Space()]
    //[SerializeField] private Permissions permissions;

    // internal variables
    private string idToken = null;
    private string csrfToken = null;
    private List<string> eventMessages = new List<string>();
    private string sceneTopic = null;

    // local paths
    const string gAuthFile = ".arena_google_auth";
    const string mqttTokenFile = ".arena_mqtt_auth";
    const string userDirArena = ".arena";
    const string userSubDirUnity = "unity";
    static string userHomePath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
    private string gAuthPath = Path.Combine(userHomePath, userDirArena, userSubDirUnity);
    private string mqttTokenPath = Path.Combine(userHomePath, userDirArena, userSubDirUnity, mqttTokenFile);

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
    }

    private IEnumerator SceneLogin()
    {
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
                    new FileDataStore(this.gAuthPath, true)).Result;
            Debug.Log("Credential file saved to: " + this.gAuthPath);

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

        sceneTopic = $"{realm}/s/{namespaceName}/{sceneName}";
        sceneUrl = $"https://{this.brokerAddress}/{namespaceName}/{sceneName}";

        // get persistence objects
        cd = new CoroutineWithData(this, HttpRequest($"https://{this.brokerAddress}/persist/{namespaceName}/{sceneName}", csrfToken));
        yield return cd.coroutine;

        Transform arenaClientTransform = GameObject.FindObjectOfType<ArenaClient>().transform;
        string jsonString = cd.result.ToString();
        JArray jsonVal = JArray.Parse(jsonString) as JArray;
        dynamic objects = jsonVal;
        foreach (dynamic obj in objects)
        {
            if (obj.type == "object")
            {
                // default
                GameObject objT = new GameObject();
                Vector3 position = new(0f, 0f, 0f);
                Quaternion rotation = Quaternion.identity;
                Vector3 scale = new(1f, 1f, 1f);
                // actual
                foreach (JProperty attribute in obj.attributes)
                {
                    switch (attribute.Name)
                    {
                        case "object_type":
                            dynamic t = obj.attributes.object_type;
                            objT = getPrimitiveByObjType((string)t);
                            break;
                        case "position":
                            dynamic p = obj.attributes.position;
                            if (p != null && p.z != null)
                                position = new Vector3((float)p.x, (float)p.y, (float)p.z);
                            break;
                        case "rotation":
                            dynamic r = obj.attributes.rotation;
                            if (r != null && r.w != null) // quaternion
                                rotation = new Quaternion((float)r.x, (float)r.y, (float)r.z, (float)r.w);
                            else if (r != null && r.z != null) // euler
                                rotation = Quaternion.Euler((float)r.x, (float)r.y, (float)r.z);
                            break;
                        case "scale":
                            dynamic s = obj.attributes.scale;
                            if (s != null && s.z != null)
                                scale = new Vector3((float)s.x, (float)s.y, (float)s.z);
                            break;
                    }
                }
                GameObject gobj = Instantiate(objT, position, rotation, arenaClientTransform);
                gobj.transform.localScale = scale;
                ArenaObject aobj = gobj.AddComponent(typeof(ArenaObject)) as ArenaObject;
                aobj.objectId = obj.object_id;
                aobj.persist = true;
                aobj.arenaJson = obj.ToString();
                gobj.name = $"{obj.object_id} ({obj.attributes.object_type})";
            }
        }

        //GameObject import = Importer.LoadFromFile("/Users/mwfarb/git/arena-core/models/Duck.glb");
        //Debug.Log($"GLTFUtility: {import.name}");
        //GameObject model = Instantiate(import, new Vector3(10f, 0f, 10f), Quaternion.identity);
        //Debug.Log(model);

        base.Start();
    }

    private GameObject getPrimitiveByObjType(string obj_type)
    {
        Debug.Log($"Adding ARENA object Type: {obj_type}");
        return obj_type switch
        {
            "box" or "cube" => GameObject.CreatePrimitive(PrimitiveType.Cube),
            "cylinder" => GameObject.CreatePrimitive(PrimitiveType.Cylinder),
            "sphere" => GameObject.CreatePrimitive(PrimitiveType.Sphere),
            "plane" => GameObject.CreatePrimitive(PrimitiveType.Quad),
            //"quad" => GameObject.CreatePrimitive(PrimitiveType.Quad),
            //"capsule" => GameObject.CreatePrimitive(PrimitiveType.Capsule),
            _ => new GameObject(),
        };
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
            Debug.Log("Error While Sending: " + uwr.error);
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

            Debug.Log("Received: " + uwr.downloadHandler.text);
            yield return uwr.downloadHandler.text;
        }
    }

    private string GetCookie(string SetCookie, string csrftag)
    {
        string csrfCookie = null;
        Regex rxCookie = new Regex(csrftag + "=(?<csrf_token>.{64});");
        MatchCollection cookieMatches = rxCookie.Matches(SetCookie);
        if (cookieMatches.Count > 0)
            csrfCookie = cookieMatches[0].Groups["csrf_token"].Value;
        return csrfCookie;
    }

    public void Publish(string object_id, string msg)
    {
        byte[] payload = System.Text.Encoding.UTF8.GetBytes(msg);
        client.Publish($"{sceneTopic}/{client.ClientId}/{object_id}", payload, MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
        Debug.Log("Sending: " + msg);
    }

    protected override void OnConnecting()
    {
        base.OnConnecting();
        Debug.Log("Connecting to broker on " + brokerAddress + ":" + brokerPort.ToString() + "...\n");
    }

    protected override void OnConnected()
    {
        base.OnConnected();
        Debug.Log("Connected to broker on " + brokerAddress + "\n");
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
        Debug.LogError("CONNECTION FAILED! " + errorMessage);
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
        //Debug.Log("Received: " + msg);
        StoreMessage(msg);
    }

    private void StoreMessage(string eventMsg)
    {
        eventMessages.Add(eventMsg);
    }

    private void ProcessMessage(string msg)
    {
        Debug.Log("Received: " + msg);
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
