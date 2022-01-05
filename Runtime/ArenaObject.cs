/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021, The CONIX Research Center. All rights reserved.
 */

using System;
using System.Dynamic;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace ArenaUnity
{
    /// <summary>
    /// Class to manage an ARENA object, publishing, and its properties.
    /// </summary>
    [HelpURL("https://arena.conix.io/content/messaging/definitions.html")]
    public class ArenaObject : MonoBehaviour
    {
        [Tooltip("Message type in persistance storage schema")]
        public string messageType = "object"; // default to object
        [Tooltip("Persist this object in the ARENA server database (default true = persist on server)")]
        public bool persist = true;
        [TextArea(5, 10)]
        [Tooltip("ARENA JSON-encoded message (debug only for now)")]
        public string jsonData = null;

        [HideInInspector]
        public dynamic data = null; // original message data for object, if any
        [HideInInspector]
        public string parentId = null;
        [HideInInspector]
        public bool created = false;

        internal string oldName; // test for rename
        internal bool externalDelete = false;
        //private string lastValidJsonData = null;

        public void OnEnable()
        {
#if UNITY_EDITOR
            // sort arena component to the top, below Transform
            while (UnityEditorInternal.ComponentUtility.MoveComponentUp(this)) { }
#endif
        }

        void Start()
        {
        }

        void Update()
        {
            // send only when changed, each publishInterval frames, or stop at 0 frames
            if (!ArenaClient.Instance || ArenaClient.Instance.transformPublishInterval == 0 ||
            Time.frameCount % ArenaClient.Instance.transformPublishInterval != 0)
                return;
            if (transform.hasChanged)
            {
                if (PublishCreateUpdate(true))
                {
                    transform.hasChanged = false;
                }
            }
            else if (oldName != null && name != oldName)
            {
                // Ensure arena-compatible naming
                name = Regex.Replace(name, ArenaUnity.regexArenaObjectId, "-");
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

        public bool PublishCreateUpdate(bool transformOnly = false)
        {
            if (ArenaClient.Instance == null || !ArenaClient.Instance.mqttClientConnected)
                return false;
            if (ArenaClient.Instance.IsShuttingDown) return false;

            // message type information
            dynamic msg = new ExpandoObject();
            msg.object_id = name;
            msg.action = created ? "update" : "create";
            msg.type = messageType;
            msg.persist = persist;
            transformOnly = created ? transformOnly : false;
            dynamic dataUnity = new ExpandoObject();
            if (data == null || data.object_type == null)
                dataUnity.object_type = ArenaUnity.ToArenaObjectType(gameObject);
            else
                dataUnity.object_type = (string)data.object_type;

            // minimum transform information
            dataUnity.position = ArenaUnity.ToArenaPosition(transform.localPosition);
            Quaternion rotOut = dataUnity.object_type == "gltf-model" ? ArenaUnity.UnityToGltfRotationQuat(transform.localRotation) : transform.localRotation;
            if (data == null || data.rotation == null || data.rotation.w != null)
                dataUnity.rotation = ArenaUnity.ToArenaRotationQuat(rotOut);
            else
                dataUnity.rotation = ArenaUnity.ToArenaRotationEuler(rotOut.eulerAngles);
            dataUnity.scale = ArenaUnity.ToArenaScale(transform.localScale);
            ArenaUnity.ToArenaDimensions(gameObject, ref dataUnity);
            if (transform.parent != null && transform.parent.gameObject.GetComponent<ArenaObject>() != null)
            {
                dataUnity.parent = transform.parent.name;
                parentId = transform.parent.name;
            }

            // other attributes information
            if (!transformOnly)
            {
                if (GetComponent<Light>())
                    ArenaUnity.ToArenaLight(gameObject, ref dataUnity);
                if (GetComponent<Renderer>())
                    ArenaUnity.ToArenaMaterial(gameObject, ref dataUnity);
            }

            // merge unity data with original message data
            var updatedData = new JObject();
            if (data != null)
                updatedData.Merge(JObject.Parse(JsonConvert.SerializeObject(data)));
            updatedData.Merge(JObject.Parse(JsonConvert.SerializeObject(dataUnity)));

            // publish
            msg.data = transformOnly ? dataUnity : updatedData;
            jsonData = JsonConvert.SerializeObject(updatedData, Formatting.Indented);
            string payload = JsonConvert.SerializeObject(msg);
            ArenaClient.Instance.Publish(msg.object_id, payload);
            if (!created)
                created = true;

            return true;
        }

        public void OnValidate()
        {
            if (EditorWindow.focusedWindow != null)
            {
                string winName = EditorWindow.focusedWindow.ToString();
                Debug.Log(winName + "" + jsonData);
            }

            if (ArenaClient.Instance == null || !ArenaClient.Instance.mqttClientConnected)
                return;
            if (ArenaClient.Instance.IsShuttingDown) return;

            // jsonData edited manually by user?
            if (jsonData != null)
            {
                JToken parsed = null;
                try
                {   // test for valid json
                    parsed = JToken.Parse(jsonData);
                }
                catch (Exception ex) //some other exception
                {
                    Debug.LogError($"Invalid Json Data: {ex}");
                }
                if (parsed != null)
                {
                    // TODO: publish internal
                    data = JsonConvert.DeserializeObject(jsonData);
                    PublishCreateUpdate();

                    // publish external
                    dynamic msg = new ExpandoObject();
                    msg.object_id = name;
                    msg.action = "update";
                    msg.type = messageType;
                    msg.persist = persist;
                    msg.data = JsonConvert.DeserializeObject(jsonData);
                    string payload = JsonConvert.SerializeObject(msg);
                    ArenaClient.Instance.Publish(msg.object_id, payload);
                }
            }
            // TODO: color/material change?

        }

        public void OnDestroy()
        {
            if (ArenaClient.Instance == null || !ArenaClient.Instance.mqttClientConnected)
                return;
            if (ArenaClient.Instance.IsShuttingDown) return;

            if (!externalDelete)
                ArenaClient.Instance.pendingDelete.Add(name);
        }

        public void OnApplicationQuit()
        {
            if (ArenaClient.Instance != null)
                ArenaClient.Instance.IsShuttingDown = true;
        }
    }
}
