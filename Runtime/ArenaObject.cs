using System;
using Newtonsoft.Json;
using UnityEngine;
using System.Dynamic;

namespace ArenaUnity
{
    [HelpURL("https://arena.conix.io/content/messaging/definitions.html")]
    public class ArenaObject : MonoBehaviour
    {
        [Tooltip("A uuid or otherwise unique identifier for this object")]
        public string objectId = Guid.NewGuid().ToString();
        [Tooltip("Type in storage schema (RO)")]
        public string storeType = "entity"; // default to entity
        [Tooltip("Persist this object in the ARENA server database (default false = do not persist)")]
        public bool persist = true;
        [TextArea(10, 15)]
        [Tooltip("ARENA JSON-encoded message (debug only for now)")]
        public string jsonData = null;

        [HideInInspector]
        public dynamic data = null;
        [HideInInspector]
        public string parentId = null;
        [HideInInspector]
        public bool created = false;

        public void OnEnable()
        {
            if (ArenaClient.Instance == null || !ArenaClient.Instance.mqttClientConnected)
                return;
            // trigger publish for new object
            transform.hasChanged = true;
        }

        void Start()
        {
            //transform.hasChanged = false;
        }

        void Update()
        {
            // send only when changed, each publishInterval frames, or stop at 0 frames
            if (!ArenaClient.Instance || ArenaClient.Instance.publishInterval == 0 ||
            Time.frameCount % ArenaClient.Instance.publishInterval != 0)
                return;
            if (transform.hasChanged)
            {
                if (SendUpdateSuccess())
                {
                    transform.hasChanged = false;
                }
            }
        }

        bool SendUpdateSuccess()
        {
            if (ArenaClient.Instance == null || !ArenaClient.Instance.mqttClientConnected)
                return false;

            dynamic msg = new ExpandoObject();
            msg.object_id = objectId;
            msg.action = created ? "update" : "create";
            msg.type = storeType;
            msg.persist = persist;

            dynamic dataUp = new ExpandoObject();
            if (data == null || data.object_type == null)
                dataUp.object_type = ArenaUnity.ToArenaObjectType(gameObject);
            else
                dataUp.object_type = (string)data.object_type;
            dataUp.position = ArenaUnity.ToArenaPosition(transform.position);
            if (data == null || data.rotation == null || data.rotation.w != null)
                dataUp.rotation = ArenaUnity.ToArenaRotationQuat(transform.rotation);
            else
                dataUp.rotation = ArenaUnity.ToArenaRotationEuler(transform.rotation.eulerAngles);
            dataUp.scale = ArenaUnity.ToArenaScale(dataUp.object_type, transform.localScale);
            if (GetComponent<Renderer>())
            {
                Color color = GetComponent<Renderer>().material.GetColor("_Color");
                if (color != null)
                {
                    dynamic material = new ExpandoObject();
                    material.color = ArenaUnity.ToArenaColor(color);
                    dataUp.material = material;
                }
            }
            msg.data = dataUp;
            //jsonData = JsonConvert.SerializeObject(data);
            string payload = JsonConvert.SerializeObject(msg);
            ArenaClient.Instance.Publish(msg.object_id, payload);
            if (!created)
                created = true;

            return true;
        }

        public void OnDestroy()
        {
            if (ArenaClient.Instance == null || !ArenaClient.Instance.mqttClientConnected)
                return;

            dynamic msg = new
            {
                object_id = objectId,
                action = "delete",
            };
            string payload = JsonConvert.SerializeObject(msg);
            ArenaClient.Instance.Publish(msg.object_id, payload);
        }
    }
}
