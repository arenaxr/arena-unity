/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2023, Carnegie Mellon University. All rights reserved.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ArenaUnity.Schemas;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace ArenaUnity
{
    /// <summary>
    /// Class to manage a singleton instance of the ARENA client connection.
    /// </summary>
    [HelpURL("https://docs.arenaxr.org")]
    [DisallowMultipleComponent]
    public partial class ArenaClientScene : ArenaMqttClient
    {
        // Singleton instance of this connection object
        public static ArenaClientScene Instance { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            Instance = this;
        }

        private new void OnDestroy()
        {
            base.OnDestroy();
            if (Instance == this)
                Instance = null;
        }

        public string realm { get; private set; }

        [Header("Rendering")]
        [Tooltip("Display objects in ARENA Persist storage")]
        public bool loadPersistedObjects = true;
        [Tooltip("Display objects updated live from ARENA MQTT")]
        public bool loadLiveObjects = true;
        [Tooltip("Display other camera avatars in the scene")]
        public bool renderCameras = true;
        [Tooltip("Display other avatars' hand controllers in the scene")]
        public bool renderHands = true;
        [Tooltip("Display VR Controller Rays")]
        public bool renderHandRays = false;

        /// <summary>
        /// Browser URL for the scene.
        /// </summary>
        public string sceneUrl { get; private set; }

        public bool sceneObjectRights { get; private set; } = false;

        public bool persistLoaded { get; private set; } = false;

        private ArenaTopics sceneTopic;
        public Dictionary<string, GameObject> arenaObjs { get; private set; } = new Dictionary<string, GameObject>();
        internal Dictionary<string, GameObject> childObjs = new Dictionary<string, GameObject>();
        internal List<string> pendingDelete = new List<string>();
        internal List<string> downloadQueue = new List<string>();
        internal List<string> parentalQueue = new List<string>();
        internal Dictionary<string, Action> pendingAssetCallbacks = new Dictionary<string, Action>();
        internal List<string> localCameraIds = new List<string>();
        internal ArenaDefaultsJson arenaDefaults { get; private set; }
        public ArenaSceneOptionsJson sceneOptions { get; internal set; }

        // Define callbacks
        public delegate void DecodeMessageDelegate(string topic, string message);
        public DecodeMessageDelegate OnMessageCallback = null; // null, until library user instantiates.

        public string originalName { get; private set; }

        static string importPath = null;
        const string prefixHandL = "handLeft_";
        const string prefixHandR = "handRight_";

        static readonly string[] msgUriTags = { "url", "src", "obj", "mtl", "overrideSrc", "detailedUrl", "headModelPath", "texture", "navMesh", "normalMap", "ambientOcclusionMap", "displacementMap", "metalnessMap", "roughnessMap", "bumpMap", "sphericalEnvMap" };
        static readonly string[] gltfUriTags = { "uri" };
        static readonly string[] skipMimeClasses = { "audio" };
        static readonly string[] requiredShadersStandardRP = {
            "Standard",
            "Standard (Specular setup)",
            "Unlit/Color",
            "glTF/PbrMetallicRoughness",
            "glTF/PbrSpecularGlossiness",
            "glTF/Unlit",
#if UNITY_EDITOR
            "Hidden/glTFExportMaskMap",
            "Hidden/glTFExportNormal",
            "Hidden/glTFExportSmoothness",
            "Hidden/glTFExportMetalGloss",
            "Hidden/glTFExportOcclusion",
            "Hidden/glTFExportColor",
#endif
        };
        static readonly string[] requiredShadersURPHDRP = {
            // "Lit",
            "Unlit/Color",
            "glTF/PbrMetallicRoughness",
            "glTF/PbrSpecularGlossiness",
            "glTF/Unlit",
        };

        protected override void OnEnable()
        {
            base.OnEnable();

            // ensure consistent name and transform
            transform.position = Vector3.zero;
            transform.rotation = Quaternion.identity;
            transform.localScale = Vector3.one;

#if UNITY_EDITOR
            // sort arena component to the top, below Transform
            while (UnityEditorInternal.ComponentUtility.MoveComponentUp(this)) { }
#endif
        }

        private static void LogAndExit(string msg)
        {
            Debug.LogError(msg);
#if UNITY_EDITOR
            if (Application.isPlaying)
                EditorApplication.ExitPlaymode();
#endif
        }

        // Start is called before the first frame update
        protected override void Start()
        {
            originalName = name;
            name = $"{originalName} (Starting...)";

            importPath = Path.Combine(appFilesPath, "Assets", "ArenaUnity", "import");

            var requiredShaders = new List<string>(requiredShadersStandardRP);
            // check if URP or HDR; different shaders are required
            if (ArenaUnity.DefaultRenderPipeline)
            {
                requiredShaders = new List<string>(requiredShadersURPHDRP);
                if (ArenaUnity.DefaultRenderPipeline.GetType().ToString().Contains("HDRenderPipelineAsset"))
                    requiredShaders.Add("HDRP/Lit");
                else
                    requiredShaders.Add("Universal Render Pipeline/Lit");
            }

            // ensure shaders are in project
            foreach (string shader in requiredShaders)
            {
                if (Shader.Find(shader) == null)
                {
                    LogAndExit($"Required shader '{shader}' not found in project.");
                }
            }
            //Shader.WarmupAllShaders();

#if UNITY_EDITOR
            // Editor use can change the Auth Method, Hostname, Namespace, Scene in Inspector easily, however
            // built apps (non-editor) want to change those parameters in UIs/UX processes they create.
            // So prevent non-editor builds from connecting automatically, so developers can allow user edits.
            StartCoroutine(ConnectArena());
#endif
        }

        /// <summary>
        /// Authenticate, MQTT connect, and add ARENA objects from Persistence DB to local app.
        /// </summary>
        public IEnumerator ConnectArena()
        {
            bool nameSafe = true;
            // local inventory before MQTT
            foreach (var aobj in FindObjectsByType<ArenaObject>(FindObjectsSortMode.None))
            {
                if (!arenaObjs.ContainsKey(aobj.name))
                {
                    arenaObjs[aobj.name] = aobj.gameObject;
                }
                else
                {
                    // prevent name collisions before MQTT
                    nameSafe = false; // critical error, arena objects must have unique names
                    Debug.LogError($"More than one ArenaObject is named '{aobj.name}'.");
                }
            }
            if (!nameSafe)
            {
                LogAndExit("All ArenaObjects must have unique names.");
                yield break;
            }

            // get arena default settings
            CoroutineWithData cd;
            cd = new CoroutineWithData(this, HttpRequestAuth($"https://{hostAddress}/conf/defaults.json"));
            yield return cd.coroutine;
            if (!isCrdSuccess(cd.result)) yield break;
            string jsonString = cd.result.ToString();
            JObject jsonVal = JObject.Parse(jsonString);
            arenaDefaults = jsonVal.SelectToken("ARENADefaults").ToObject<ArenaDefaultsJson>();
            brokerAddress = arenaDefaults.mqttHost;
            realm = arenaDefaults.realm;

            // start auth flow and MQTT connection
            ArenaCamera[] camlist = FindObjectsByType<ArenaCamera>(FindObjectsSortMode.None);
            name = $"{originalName} (Authenticating...)";
            cd = new CoroutineWithData(this, SigninScene(sceneName, namespaceName, realm, camlist.Length > 0, arenaDefaults.latencyTopic));
            yield return cd.coroutine;
            name = $"{originalName} (MQTT Connecting...)";
            if (cd.result != null)
            {
                if (string.IsNullOrWhiteSpace(namespaceName)) namespaceName = cd.result.ToString();
                sceneTopic = new ArenaTopics(
                    realm: realm,
                    name_space: namespaceName,
                    scenename: sceneName,
                    userclient: userclient,
                    idtag: userid,
                    userobj: camid
                );
                sceneUrl = $"https://{hostAddress}/{namespaceName}/{sceneName}";
            }
            if (permissions == null)
            {   // fail when permissions not set
                LogAndExit("Permissions not received.");
                yield break;
            }
            sceneObjectRights = HasPerms(sceneTopic.PUB_SCENE_OBJECTS);
            // publish arena cameras where requested
            bool foundFirstCam = false;
            foreach (ArenaCamera cam in camlist)
            {
                if ((Camera.main != null && cam.name == Camera.main.name || camlist.Length == 1) && !foundFirstCam)
                {
                    // publish main/selected camera
                    cam.userid = userid;
                    cam.camid = camid;
                    foundFirstCam = true;
                }
                else
                {
                    // TODO (mwfarb): fix: other cameras are auto-generated, and account must have all scene rights
                    if (!sceneObjectRights)
                    {
                        Debug.LogWarning($"Using more than one ArenaCamera requires full scene permissions. Only one camera will be published.");
                    }
                    var random = UnityEngine.Random.Range(0, 100000000);
                    cam.userid = $"unpublished-unity_{random:D8}";
                    cam.camid = $"cam-unpublished-unity_{random:D8}";
                }
                var camTopic = new ArenaTopics(
                    realm: realm,
                    name_space: namespaceName,
                    scenename: sceneName,
                    userclient: userclient,
                    idtag: cam.userid,
                    userobj: cam.camid
                );
                cam.HasPermissions = HasPerms(camTopic.PUB_SCENE_USER);
                localCameraIds.Add(cam.camid);
            }

            // get persistence objects
            if (loadPersistedObjects)
            {
                StartCoroutine(SceneLoadPersist());
            }
        }

        /// <summary>
        /// Disconnect MQTT and remove ARENA objects from local app.
        /// </summary>
        public void DisconnectArena()
        {
            Disconnect();
            foreach (var aobj in arenaObjs.Values)
            {
                Destroy(aobj);
            }
            arenaObjs.Clear();
        }

        // Update is called once per frame
        protected override void Update()
        {
            base.Update();

            if (pendingDelete.Count > 0)
            {   // confirm for any deletes requested
                string ids = string.Join(", ", pendingDelete);
#if UNITY_EDITOR

                if (EditorUtility.DisplayDialog("Delete!",
                     $"Are you sure you want to delete object(s) from the ARENA: {ids}?", "Delete", "Save"))
                {
                    foreach (string object_id in pendingDelete)
                    {
                        ArenaMessageJson msg = new ArenaMessageJson
                        {
                            object_id = object_id,
                            action = "delete",
                            persist = true,
                        };
                        string payload = JsonConvert.SerializeObject(msg);
                        PublishObject(msg.object_id, payload);
                    }
                }
#endif
                pendingDelete.Clear();
            }
        }

        private IEnumerator SceneLoadPersist()
        {
            CoroutineWithData cd;

            // 1. Fetch scene-options first so their effects (like physics ground planes) apply before other objects
            cd = new CoroutineWithData(this, HttpRequestAuth($"https://{arenaDefaults.persistHost}{arenaDefaults.persistPath}{namespaceName}/{sceneName}?type=scene-options", csrfToken));
            yield return cd.coroutine;
            if (isCrdSuccess(cd.result))
            {
                string optionsJsonString = cd.result.ToString();
                List<ArenaMessageJson> optionsMessages = JsonConvert.DeserializeObject<List<ArenaMessageJson>>(optionsJsonString);
                foreach (ArenaMessageJson msg in optionsMessages)
                {
                    msg.persist = true;
                    // Note: scene-options generally don't have downloadable assets that block instantiation,
                    // so we can apply them directly.
                    CreateUpdateObject(msg, msg.data);
                }
            }

            // 2. Fetch the rest of the objects
            cd = new CoroutineWithData(this, HttpRequestAuth($"https://{arenaDefaults.persistHost}{arenaDefaults.persistPath}{namespaceName}/{sceneName}", csrfToken));
            yield return cd.coroutine;
            if (!isCrdSuccess(cd.result)) yield break;
            string jsonString = cd.result.ToString();
            List<ArenaMessageJson> persistMessages = JsonConvert.DeserializeObject<List<ArenaMessageJson>>(jsonString);
            // establish objects
            int objects_num = 1;
            if (Directory.Exists(importPath))
                Directory.Delete(importPath, true);
            if (File.Exists($"{importPath}.meta"))
                File.Delete($"{importPath}.meta");
            foreach (ArenaMessageJson msg in persistMessages)
            {
                string object_id = msg.object_id;
                string msg_type = msg.type;
                msg.persist = true; // always true coming from persist

                // Skip scene-options as they were already applied
                if (msg_type == "scene-options") continue;

                if (arenaObjs != null && !arenaObjs.ContainsKey(object_id)) // do not duplicate, local project object takes priority
                {
                    IEnumerable<string> uris = ExtractAssetUris(msg.data, msgUriTags);
                    foreach (var uri in uris)
                    {
                        if (!string.IsNullOrWhiteSpace(uri))
                        {
                            StartCoroutine(DownloadAssets(msg_type, uri));
                        }
                    }
                    CreateUpdateObject(msg, msg.data);
                }
                objects_num++;
            }
            persistLoaded = true;
        }

        // methods for the editor
        private void DisplayCancelableProgressBar(string title, string info, float progress)
        {
#if UNITY_EDITOR
            EditorUtility.DisplayCancelableProgressBar(title, info, progress);
#endif
        }

        private void ClearProgressBar()
        {
#if UNITY_EDITOR
            EditorUtility.ClearProgressBar();
#endif
        }

        protected override void OnConnected()
        {
            base.OnConnected();
            name = $"{originalName} (MQTT Connected)";
            var subTopic = new ArenaTopics(
                realm: realm,
                name_space: namespaceName,
                scenename: sceneName,
                idtag: userid
            );
            client.MqttMsgSubscribed += OnSubscribed;
            Subscribe(subTopic.SUB_SCENE_PUBLIC);
            Subscribe(subTopic.SUB_SCENE_PRIVATE);
        }

        private void OnSubscribed(object sender, MqttMsgSubscribedEventArgs e)
        {
            var validQos = new byte[] { MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE, MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE };
            if (e.GrantedQoSLevels.Any(el => validQos.Contains(el)))
            {
                Debug.Log($"Subscribed to: {subscriptions[e.MessageId]}");
            }
            else
            {
                Debug.LogWarning($"Subscribed FAILED to: {subscriptions[e.MessageId]}");
            }
        }

        protected override void OnDisconnected()
        {
            base.OnDisconnected();
            name = $"{originalName} (MQTT Disconnected)";
        }

        protected override void OnApplicationQuit()
        {
            // send delete of local avatars before connection closes
            foreach (var camid in localCameraIds)
            {
                ArenaMessageJson msg = new ArenaMessageJson
                {
                    object_id = camid,
                    action = "delete",
                };
                string delCamMsg = JsonConvert.SerializeObject(msg);
                PublishCamera(camid, delCamMsg);
            }
            base.OnApplicationQuit();
        }
    }
}
