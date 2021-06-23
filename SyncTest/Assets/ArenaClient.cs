using System;
using System.IO;
using System.Threading;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Oauth2.v2;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using UnityEngine;

[HelpURL("https://arena.conix.io")]
public class ArenaClient : MonoBehaviour
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
    [Tooltip("UserName for the MQTT broker. Keep blank if no user name is required.")]
    public string mqttUserName = null;
    [Tooltip("Password for the MQTT broker. Keep blank if no password is required.")]
    public string mqttPassword = null;


    public string gAuthId = null;
    public string gAuthEmail = null;


    static string[] Scopes = {
        Oauth2Service.Scope.UserinfoProfile,
        Oauth2Service.Scope.UserinfoEmail,
        Oauth2Service.Scope.Openid
    };

    // Start is called before the first frame update
    void Start()
    {
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
        }

    }

    // Update is called once per frame
    void Update()
    {
        sceneUrl = "https://arena-dev1.conix.io/mwfarb/test";
    }
}
