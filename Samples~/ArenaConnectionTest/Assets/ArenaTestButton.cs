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

            scene.authType = ArenaMqttClient.Auth.Anonymous;
            scene.hostAddress = "arenaxr.org";
            scene.namespaceName = "public";
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
                // setup example cube
                GameObject test = GameObject.CreatePrimitive(PrimitiveType.Cube);
                test.name = "unity-cube-01";
                test.transform.localPosition = new Vector3(0, 1.5f, -2f);
                test.transform.localRotation = Quaternion.Euler(0, 45, 0);
                test.transform.localScale = new Vector3(1, 1, 1);

                ArenaObject aobj = test.AddComponent<ArenaObject>();
                aobj.persist = true;
                
                // publish example cube
                aobj.PublishCreateUpdate();
                Debug.Log($"beginner created: {test.name}");
            }
            else if (button == advanced)
            {
                // advanced manual JSON payload
                ArenaMessageJson msg = new ArenaMessageJson
                {
                    object_id = "unity-cube-02",
                    action = "create",
                    type = "object",
                    persist = true,
                };
                ArenaObjectJson data = new ArenaObjectJson
                {
                    object_type = "cube",
                    Position = new ArenaPositionJson { x = 0f, y = 1.5f, z = 2f },
                    Rotation = new ArenaRotationJson { x = 0f, y = 0.38f, z = 0f, w = 0.92f },
                    Scale = new ArenaScaleJson { x = 1f, y = 1f, z = 1f },
                    Color = "#ff0000"
                };
                msg.data = data;
                string payload = JsonConvert.SerializeObject(msg);
                scene.PublishObject(msg.object_id, payload);
                Debug.Log($"advanced created: {msg.object_id}");
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
