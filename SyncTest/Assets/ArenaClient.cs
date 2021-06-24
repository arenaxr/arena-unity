using System;
//using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Oauth2.v2;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using UnityEngine;
//using UnityEngine.UI;
//using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using M2MqttUnity;

[HelpURL("https://arena.conix.io")]
public class ArenaClient : M2MqttUnityClient
{
    [Header("ARENA Configuration")]
    [Tooltip("IP address or URL of the host running the broker")]
    public string host = "arenaxr.org";
    [Tooltip("Name of the scene, without namespace ('example', not 'username/example'")]
    public string sceneName = "example";
    [Tooltip("Authorization code from web authentication.")]
    public string authCode = null;
    [Tooltip("Browser URL for the scene.")]
    [TextArea(minLines: 1, maxLines: 2)]
    public string sceneUrl = null;


    [Header("Optional Parameters")]
    [Tooltip("Namespace (automated with username), but can be overridden")]
    public string namespaceName = null;


    public string gAuthId = null;
    public string gAuthEmail = null;
    private List<string> eventMessages = new List<string>();


    static string[] Scopes = {
        Oauth2Service.Scope.UserinfoProfile,
        Oauth2Service.Scope.UserinfoEmail,
        Oauth2Service.Scope.Openid
    };

    // Start is called before the first frame update
    protected override void Start()
    {
        Debug.Log("Ready.");
        base.Start();

        UserCredential credential;
        using (var stream = new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
        {
            // string credPath = System.Environment.GetFolderPath(
            //      System.Environment.SpecialFolder.Personal);
            // credPath = Path.Combine(credPath, ".arena_google_auth");
            string credPath = "token.json";
            string applicationName = "ArenaClientCSharp";

            credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.FromStream(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
            Console.WriteLine("Credential file saved to: " + credPath);

            var oauthService = new Oauth2Service(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = applicationName,
            });

            var userInfo = oauthService.Userinfo.Get().Execute();
            Console.WriteLine("userInfo: " + userInfo);

            gAuthEmail = userInfo.Email;
            gAuthId = userInfo.Id;
            sceneUrl = "https://arena-dev1.conix.io/mwfarb/test";
        }

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


    public void TestPublish()
    {
        client.Publish("M2MQTT_Unity/test", System.Text.Encoding.UTF8.GetBytes("Test message"), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);
        Debug.Log("Test message published.");
    }

    public void SetBrokerAddress(string brokerAddress)
    {
        //if (addressInputField)
        //{
            this.brokerAddress = brokerAddress;
        //}
    }

    public void SetBrokerPort(string brokerPort)
    {
        //if (portInputField)
        //{
            int.TryParse(brokerPort, out this.brokerPort);
        //}
    }

    public void SetEncrypted(bool isEncrypted)
    {
        this.isEncrypted = isEncrypted;
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

        //if (autoTest)
        //{
        //    TestPublish();
        //}
    }

    protected override void SubscribeTopics()
    {
        client.Subscribe(new string[] { "M2MQTT_Unity/test" }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
    }

    protected override void UnsubscribeTopics()
    {
        client.Unsubscribe(new string[] { "M2MQTT_Unity/test" });
    }

    protected override void OnConnectionFailed(string errorMessage)
    {
        Debug.Log("CONNECTION FAILED! " + errorMessage);
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
        Debug.Log("Received: " + msg);
        StoreMessage(msg);
        //if (topic == "M2MQTT_Unity/test")
        //{
        //    if (autoTest)
        //    {
        //        autoTest = false;
        //        Disconnect();
        //    }
        //}
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
        //if (autoTest)
        //{
        //    autoConnect = true;
        //}
    }
}
