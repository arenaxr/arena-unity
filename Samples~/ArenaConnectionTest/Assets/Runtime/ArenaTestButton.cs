using System.Collections;
using ArenaUnity;
using ArenaUnity.Schemas;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

namespace ArenaUnity.Samples
{
    public class ArenaTestButton : MonoBehaviour
    {
        public Button toggle, beginner, advanced;
        public Text statusText;
        bool lastConnectState = false;

        void Awake()
        {
            // Auto-generate UI if not assigned in the inspector
            if (toggle == null || beginner == null || advanced == null || statusText == null)
            {
                GenerateUI();
            }
        }

        void Start()
        {
            Debug.Log("ArenaTestButton started...");
        }

        private void OnEnable()
        {
            if (toggle != null) toggle.onClick.AddListener(() => StartCoroutine(ButtonCallback(toggle)));
            if (beginner != null) beginner.onClick.AddListener(() => StartCoroutine(ButtonCallback(beginner)));
            if (advanced != null) advanced.onClick.AddListener(() => StartCoroutine(ButtonCallback(advanced)));
        }

        private void OnDisable()
        {
            if (toggle != null) toggle.onClick.RemoveAllListeners();
            if (beginner != null) beginner.onClick.RemoveAllListeners();
            if (advanced != null) advanced.onClick.RemoveAllListeners();
        }

        private void Update()
        {
            ArenaClientScene scene = ArenaClientScene.Instance;
            if (scene && toggle)
            {
                if (scene.mqttClientConnected != lastConnectState)
                {
                    Debug.Log($"mqttClientConnected changed = {scene.mqttClientConnected}");

                    if (statusText != null)
                    {
                        statusText.text = scene.mqttClientConnected ? "Status: Connected" : "Status: Disconnected";
                        statusText.color = scene.mqttClientConnected ? Color.green : Color.red;
                    }

                    if (scene.mqttClientConnected)
                    {
                        ColorBlock cb = toggle.colors;
                        cb.normalColor = Color.green;
                        cb.highlightedColor = Color.green;
                        toggle.colors = cb;
                    }
                    else
                    {
                        ColorBlock cb = toggle.colors;
                        cb.normalColor = Color.white;
                        cb.highlightedColor = Color.white;
                        toggle.colors = cb;
                    }
                }
                lastConnectState = scene.mqttClientConnected;
            }
        }

        /// <summary>
        /// Example callback for demo button clicks.
        /// </summary>
        IEnumerator ButtonCallback(Button button)
        {
            Debug.Log($"Clicked {button.name}...");
            ArenaClientScene scene = ArenaClientScene.Instance;
            if (scene == null)
            {
                // Auto-create ArenaClientScene if it doesn't exist
                GameObject sceneObj = new GameObject("ArenaClientScene");
                scene = sceneObj.AddComponent<ArenaClientScene>();
            }

            scene.authType = ArenaMqttClient.Auth.Google;
            scene.hostAddress = "arenaxr.org";
            scene.sceneName = "example";

            if (button == toggle)
            {
                if (scene.mqttClientConnected)
                {
                    scene.DisconnectArena();
                    yield return new WaitUntil(() => !scene.mqttClientConnected);
                    Debug.Log("toggle now disconnected...");
                }
                else
                {
                    StartCoroutine(scene.ConnectArena());
                    yield return new WaitUntil(() => scene.mqttClientConnected);
                    Debug.Log("toggle now connected...");
                }
            }
            else if (button == beginner)
            {
                Debug.Log("ArenaUnityBeginnerDemo started...");
                StartCoroutine(ArenaUnityBeginnerDemo());
            }
            else if (button == advanced)
            {
                Debug.Log("ArenaUnityAdvancedDemo started...");
                StartCoroutine(ArenaUnityAdvancedDemo());
            }
        }

        /// <summary>
        /// Demonstrate basic usage of the ArenaUnity package.
        /// </summary>
        IEnumerator ArenaUnityBeginnerDemo()
        {
            // Only one singleton connection instance allowed per application.
            ArenaClientScene scene = ArenaClientScene.Instance;
            scene.authType = ArenaMqttClient.Auth.Google;

            // Set the ARENA webserver main host address, default: "arenaxr.org".
            scene.hostAddress = "arenaxr.org";

            // Set the namespace name for the scene, default: [your ARENA username].
            // For google authentication, this is set automatically on login and unnecessary when using your own username.
            //scene.namespaceName = "public";

            // Set the scene name for the scene, default: "example".
            scene.sceneName = "example";

            // Authenticate flow, MQTT connection flow, and Persistence download flow.
            // This will execute an asynchronous coroutine thread for these flows.
            scene.ConnectArena();
            yield return new WaitUntil(() => scene.mqttClientConnected);

            // Display the web browser GUI client URL, set after MQTT connection flow completes.
            Debug.Log($"Scene URL: {scene.sceneUrl}");

            // Instantiate the callback for all messages.
            scene.OnMessageCallback = MessageCallback;

            // Create GameObject, and add ArenaObject script to manage updates, it will automatically send an MQTT create message
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ArenaObject arenaObject = cube.AddComponent(typeof(ArenaObject)) as ArenaObject;

            // Change the parent/name/transform, it will automatically send an MQTT update message
            cube.transform.rotation = UnityEngine.Random.rotation;

            // Publish a custom event under this client's "camera" object
            scene.PublishEvent("my-custom-event-type", scene.camid, "{\"my-attribute\": \"my-custom-attribute\"}");

            // Find other arena users in the scene
            string firstUserId = null;
            ArenaObject[] objlist = FindObjectsOfType<ArenaObject>();
            ArenaCamera[] camlist = FindObjectsOfType<ArenaCamera>();
            string localUserId = null;
            if (camlist.Length > 0)
                localUserId = camlist[0].userid;
            foreach (ArenaObject obj in objlist)
            {
                if (obj.object_type == "camera" && obj.name != localUserId)
                {
                    firstUserId = obj.name;
                    break;
                }
            }

            // Publish a private object update message for first user found in scene
            ArenaMessageJson msgpriv = new ArenaMessageJson
            {
                object_id = "cone-private",
                action = "create",
                type = "object",
                persist = false,
                data = new ArenaDataObjectJson
                {
                    object_type = "cone"
                }
            };
            string payloadpriv = JsonConvert.SerializeObject(msgpriv);
            scene.PublishObject(msgpriv.object_id, payloadpriv, firstUserId);

            // Publish a public object update message
            ArenaMessageJson msgpub = new ArenaMessageJson
            {
                object_id = "box-public",
                action = "create",
                type = "object",
                persist = true,
                data = new ArenaDataObjectJson
                {
                    object_type = "box",
                    // make the box interact with mouse-equivalent events
                    ClickListener = new ArenaClickListenerJson { Enabled = true }
                }
            };
            string payloadpub = JsonConvert.SerializeObject(msgpub);
            scene.PublishObject(msgpub.object_id, payloadpub);

            // Manually ingest a message, not received from MQTT subscriber
            scene.ProcessMessage($"realm/s/public/example/o/{msgpub.object_id}", payloadpub);
        }

        /// <summary>
        /// A delegate method used as a callback to go some special handling on incoming messages.
        /// </summary>
        public static void MessageCallback(string topic, string message)
        {
            ArenaMessageJson m = JsonConvert.DeserializeObject<ArenaMessageJson>(message);
            if (m.action == "clientEvent")
            {
                // parse some event data and log it
                ArenaEventJson evt = JsonConvert.DeserializeObject<ArenaEventJson>(m.data.ToString());
                Debug.LogFormat($"Received event '{m.type}' from {m.object_id}, target={evt.Target}");

                // log any users who use the hand controller to pull the trigger
                if (m.type != "gripdown")
                {
                    Debug.LogFormat($"{m.object_id} pulled the trigger!");
                }
            }
        }

        /// <summary>
        /// Demonstrate advanced usage of the ArenaUnity package.
        /// </summary>
        IEnumerator ArenaUnityAdvancedDemo()
        {
            // Create a simple arena mqtt client and send receive messages.
            GameObject gobj = new GameObject("Arena Mqtt Client Advanced");
            MyArenaClient client = gobj.AddComponent(typeof(MyArenaClient)) as MyArenaClient;

            // Setup a connection using a custom namespace and anonymous authentication.
            client.hostAddress = "arenaxr.org";
            client.authType = ArenaMqttClient.Auth.Google;

            // Alternate, Manual auth: Store any local jwt tokens here, before auth starts.
            // Derive the local path from the next line.
            // string localMqttPath = Path.Combine(client.appFilesPath, ".arena_mqtt_auth");
            // client.authType = ArenaMqttClient.Auth.Manual;

            // Authenticate flow, MQTT connection flow.
            client.ConnectArena();
            yield return new WaitUntil(() => client.mqttClientConnected);

            // Display the MQTT JWT permission payload/claims, set after authentication flow completes.
            Debug.Log($"Permissions: {client.permissions}");

            // Custom MQTT pub/sub
            client.Subscribe("my/custom/topic/#");

            yield return new WaitForSeconds(2);
            client.Publish("my/custom/topic/channel/device-888", System.Text.Encoding.UTF8.GetBytes("some payload"));

            // MQTT disconnect
            client.Disconnect();
        }

        public class MyArenaClient : ArenaMqttClient
        {
            public void ConnectArena()
            {
                // start auth flow and MQTT connection
                StartCoroutine(Signin());
            }

            // Directly override the incoming message handler.
            protected override void DecodeMessage(string topic, byte[] message)
            {
                Debug.LogFormat("Message received on topic {0}: {1}", topic, System.Text.Encoding.UTF8.GetString(message));
            }
        }

        // ---------------------------------------------------------
        // Auto-UI Generation
        // ---------------------------------------------------------
        private void GenerateUI()
        {
            // 1. Create Canvas
            GameObject canvasObj = new GameObject("Canvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();

            // 2. Create EventSystem
            if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject eventSystem = new GameObject("EventSystem");
                eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }

            // 3. Create Status Text
            GameObject textObj = new GameObject("StatusText");
            textObj.transform.SetParent(canvasObj.transform, false);
            statusText = textObj.AddComponent<Text>();
            statusText.text = "Status: Disconnected";
            statusText.color = Color.red;
            statusText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            statusText.alignment = TextAnchor.MiddleCenter;
            statusText.fontSize = 24;
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchoredPosition = new Vector2(0, 100);
            textRect.sizeDelta = new Vector2(300, 50);

            // 4. Create Buttons
            toggle = CreateButton(canvasObj.transform, "Toggle Connection", new Vector2(0, 30));
            beginner = CreateButton(canvasObj.transform, "Beginner Create", new Vector2(0, -30));
            advanced = CreateButton(canvasObj.transform, "Advanced Create", new Vector2(0, -90));

            Debug.Log("[ArenaTestButton] UI generated dynamically because references were null.");
        }

        private Button CreateButton(Transform parent, string title, Vector2 anchoredPos)
        {
            GameObject btnObj = new GameObject(title + " Button");
            btnObj.transform.SetParent(parent, false);

            Image img = btnObj.AddComponent<Image>();
            img.color = Color.white;

            Button btn = btnObj.AddComponent<Button>();

            RectTransform rect = btnObj.GetComponent<RectTransform>();
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta = new Vector2(200, 40);

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);
            Text txt = textObj.AddComponent<Text>();
            txt.text = title;
            txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            txt.color = Color.black;
            txt.alignment = TextAnchor.MiddleCenter;

            RectTransform txtRect = textObj.GetComponent<RectTransform>();
            txtRect.anchorMin = Vector2.zero;
            txtRect.anchorMax = Vector2.one;
            txtRect.sizeDelta = Vector2.zero;
            txtRect.anchoredPosition = Vector2.zero;

            return btn;
        }
    }
}
