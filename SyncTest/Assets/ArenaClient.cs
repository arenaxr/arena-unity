using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Google.Apis.Auth.OAuth2;
using Google.Apis.Oauth2.v2;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System.Linq;
using System.Text;

public class ArenaClient : MonoBehaviour
{
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

            credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
            Console.WriteLine("Credential file saved to: " + credPath);

            var oauthService = new Oauth2Service(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "ArenaClientCSharp",
            });

            var userInfo = oauthService.Userinfo.Get().Execute();
            Console.WriteLine("userInfo: " + userInfo);
        }

    }

    // Update is called once per frame
    void Update()
    {

    }
}
