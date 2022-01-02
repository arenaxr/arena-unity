/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021, The CONIX Research Center. All rights reserved.
 */

using System.Dynamic;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace ArenaUnity
{
    /// <summary>
    /// Class to manage an ARENA object, publishing, and its properties.
    /// </summary>
    [HelpURL("https://arena.conix.io/content/messaging/definitions.html")]
    public class ArenaObject : MonoBehaviour
    {
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

        private string oldName; // test for rename

        public void OnEnable()
        {
        }

        void Start()
        {
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
            else if (oldName != null && name != oldName)
            {
                HandleRename();
            }
            oldName = name;
        }

        private void HandleRename()
        {
            if (ArenaClient.Instance == null || !ArenaClient.Instance.mqttClientConnected)
                return;
            // pub delete old
            dynamic msg = new
            {
                object_id = oldName,
                action = "delete",
                persist = persist,
            };
            string payload = JsonConvert.SerializeObject(msg);
            ArenaClient.Instance.Publish(msg.object_id, payload);
            // add new object with new name, it pubs
            created = false;
            transform.hasChanged = true;
        }

        bool SendUpdateSuccess()
        {
            if (ArenaClient.Instance == null || !ArenaClient.Instance.mqttClientConnected)
                return false;
            if (ArenaClient.Instance.IsShuttingDown) return false;

            dynamic msg = new ExpandoObject();
            msg.object_id = name;
            msg.action = created ? "update" : "create";
            msg.type = storeType;
            msg.persist = persist;

            dynamic dataUp = new ExpandoObject();
            if (data == null || data.object_type == null)
                dataUp.object_type = ArenaUnity.ToArenaObjectType(gameObject);
            else
                dataUp.object_type = (string)data.object_type;
            dataUp.position = ArenaUnity.ToArenaPosition(transform.position);
            Quaternion rotOut = (string)data.object_type == "gltf-model" ? ArenaUnity.UnityToGltfRotationQuat(transform.rotation) : transform.rotation;
            if (data == null || data.rotation == null || data.rotation.w != null)
                dataUp.rotation = ArenaUnity.ToArenaRotationQuat(rotOut);
            else
                dataUp.rotation = ArenaUnity.ToArenaRotationEuler(rotOut.eulerAngles);
            dataUp.scale = ArenaUnity.ToArenaScale(transform.localScale);
            ArenaUnity.ToArenaDimensions(gameObject, ref dataUp);
            if (GetComponent<Light>())
                ArenaUnity.ToArenaLight(gameObject, ref dataUp);
            if (GetComponent<Renderer>())
                ArenaUnity.ToArenaMaterial(gameObject, ref dataUp);
            msg.data = dataUp;
            //jsonData = JsonConvert.SerializeObject(data);
            string payload = JsonConvert.SerializeObject(msg);
            ArenaClient.Instance.Publish(msg.object_id, payload);
            if (!created)
                created = true;

            return true;
        }

        public void OnValidate()
        {
            // TODO: color/material change?
        }

        public void OnDestroy()
        {
            if (ArenaClient.Instance == null || !ArenaClient.Instance.mqttClientConnected)
                return;
            if (ArenaClient.Instance.IsShuttingDown) return;
            if (EditorUtility.DisplayDialog("Delete!",
                 $"Are you sure you want to delete {name}?", "Delete", "Save"))
            {
                dynamic msg = new
                {
                    object_id = name,
                    action = "delete",
                    persist = persist,
                };
                string payload = JsonConvert.SerializeObject(msg);
                ArenaClient.Instance.Publish(msg.object_id, payload);
            }
        }

        public void OnApplicationQuit()
        {
            if (ArenaClient.Instance != null)
                ArenaClient.Instance.IsShuttingDown = true;
        }
    }
}
