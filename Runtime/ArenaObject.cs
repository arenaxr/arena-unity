using System;
using Newtonsoft.Json;
using UnityEngine;

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
            if (Application.isEditor)
            {
                if (ArenaClient.Instance == null || !ArenaClient.Instance.mqttClientConnected)
                    return;
                // trigger publish for new object
                transform.hasChanged = true;
            }
        }

        void Start()
        {
            transform.hasChanged = false;
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

            dynamic msg = new System.Dynamic.ExpandoObject();
            msg.object_id = this.objectId;
            msg.action = this.created ? "update" : "create";
            msg.type = this.storeType;
            msg.persist = this.persist;
            dynamic dataUp = new System.Dynamic.ExpandoObject();
            dataUp.object_type = ArenaUnity.ToArenaObjectType(this.gameObject);
            dataUp.position = ArenaUnity.ToArenaPosition(transform.position);
            if (data != null && data.rotation != null && data.rotation.w != null)
                dataUp.rotation = ArenaUnity.ToArenaRotationQuat(transform.rotation);
            else
                dataUp.rotation = ArenaUnity.ToArenaRotationEuler(transform.rotation.eulerAngles);
            dataUp.scale = ArenaUnity.ToArenaScale(dataUp.object_type, transform.localScale);
            if (GetComponent<Renderer>())
            {
                Color color = GetComponent<Renderer>().material.GetColor("_Color");
                dataUp.color = ArenaUnity.ToArenaColor(color);
            }
            data = dataUp;
            msg.data = data;
            jsonData = data.ToString();
            string payload = JsonConvert.SerializeObject(msg);
            ArenaClient.Instance.Publish(msg.object_id, payload);
            if (!this.created)
                this.created = true;

            return true;
        }

        public void OnDestroy()
        {
            if (Application.isEditor)
            {
                if (ArenaClient.Instance == null || !ArenaClient.Instance.mqttClientConnected)
                    return;

                dynamic msg = new
                {
                    object_id = this.objectId,
                    action = "delete",
                };
                string payload = JsonConvert.SerializeObject(msg);
                ArenaClient.Instance.Publish(msg.object_id, payload);
            }
        }
    }
}
