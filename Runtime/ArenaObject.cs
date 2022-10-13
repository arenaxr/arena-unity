/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021, The CONIX Research Center. All rights reserved.
 */

using System.Collections.Generic;
using System.Dynamic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PrettyHierarchy;
using UnityEngine;

namespace ArenaUnity
{
    /// <summary>
    /// Class to manage an ARENA object, publishing, and its properties.
    /// </summary>
    [HelpURL("https://docs.arenaxr.org/content/schemas")]
    [DisallowMultipleComponent]
    public class ArenaObject : PrettyObject
    {
        [Tooltip("Message type in persistence storage schema")]
        public string messageType = "object"; // default to object
        [Tooltip("Persist this object in the ARENA server database (default true = persist on server)")]
        public bool persist = true;
        [TextArea(5, 20)]
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
        internal bool isJsonValidated = false;
        internal List<string> animations = null;
        internal string gltfUrl = null;
        internal bool meshChanged = false;

        public void OnEnable()
        {
#if UNITY_EDITOR
            // sort arena component to the top, below Transform
            while (UnityEditorInternal.ComponentUtility.MoveComponentUp(this)) { }
#endif
        }

        void Start()
        {
            // runtime created arena objects still need to be checked for name uniqueness
            bool found = false;
            foreach (var aobj in FindObjectsOfType<ArenaObject>())
            {
                if (aobj.name == name)
                {
                    if (!found) found = true;
                    else name = $"{name}-{UnityEngine.Random.Range(0, 1000000)}";
                }
            }

            isJsonValidated = jsonData != null;
        }

        void Update()
        {
            // send only when changed, each publishInterval frames, or stop at 0 frames
            if (!ArenaClientScene.Instance || ArenaClientScene.Instance.transformPublishInterval == 0 ||
            Time.frameCount % ArenaClientScene.Instance.transformPublishInterval != 0)
                return;
            if (transform.hasChanged || meshChanged)
            {
                //TODO: prevent child objects of parent.transform.hasChanged = true from publishing unnecessarily

                if (PublishCreateUpdate(true))
                {
                    transform.hasChanged = false;
                    meshChanged = false;
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
            if (ArenaClientScene.Instance == null || !ArenaClientScene.Instance.mqttClientConnected)
                return;
            // pub delete old
            dynamic msg = new
            {
                object_id = oldName,
                action = "delete",
                persist = persist,
            };
            string payload = JsonConvert.SerializeObject(msg);
            ArenaClientScene.Instance.PublishObject(msg.object_id, payload);
            // add new object with new name, it pubs
            created = false;
            transform.hasChanged = true;
        }

        public bool PublishCreateUpdate(bool transformOnly = false)
        {
            if (ArenaClientScene.Instance == null || !ArenaClientScene.Instance.mqttClientConnected)
                return false;
            if (ArenaClientScene.Instance.IsShuttingDown) return false;
            if (messageType != "object") return false;

            if (!ArenaClientScene.Instance.arenaObjs.ContainsKey(name))
                ArenaClientScene.Instance.arenaObjs.Add(name, gameObject);

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
            if (transform.parent && transform.parent.gameObject.GetComponent<ArenaObject>() != null)
            {   // parent
                dataUnity.parent = transform.parent.name;
                parentId = transform.parent.name;
            }
            else if (parentId != null)
            {   // unparent
                dataUnity.parent = null;
                parentId = null;
            }

            if (meshChanged)
            {
                if ((string)data.object_type == "entity" && data.geometry != null && data.geometry.primitive != null)
                {
                    dataUnity.geometry = new ExpandoObject();
                    ArenaUnity.ToArenaMesh(gameObject, ref dataUnity.geometry);
                }
                else
                {
                    ArenaUnity.ToArenaMesh(gameObject, ref dataUnity);
                }
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
            ArenaClientScene.Instance.PublishObject(msg.object_id, payload);
            if (!created)
                created = true;

            return true;
        }

        internal void PublishJson()
        {
            dynamic msg = new ExpandoObject();
            msg.object_id = name;
            msg.action = "update";
            msg.type = messageType;
            msg.persist = persist;
            msg.data = JsonConvert.DeserializeObject(jsonData);
            string payload = JsonConvert.SerializeObject(msg);
            ArenaClientScene.Instance.PublishObject(msg.object_id, payload); // remote
            ArenaClientScene.Instance.ProcessMessage(payload); // local
        }

        public void OnValidate()
        {
            // TODO: handle problematic offline name change

            if (ArenaClientScene.Instance == null || !ArenaClientScene.Instance.mqttClientConnected)
                return;
            if (ArenaClientScene.Instance.IsShuttingDown) return;

            // jsonData edited manually by user?
            if (jsonData != null)
            {
                JToken parsed = null;
                try
                {   // test for valid json
                    parsed = JToken.Parse(jsonData);
                }
                catch { }
                finally
                {
                    // notify GUI to allow publish
                    isJsonValidated = parsed != null;
                }
            }

            // TODO: color/material change?
        }

        public void OnDestroy()
        {
            if (ArenaClientScene.Instance == null || !ArenaClientScene.Instance.mqttClientConnected)
                return;
            if (ArenaClientScene.Instance.IsShuttingDown) return;

            if (!externalDelete)
                ArenaClientScene.Instance.pendingDelete.Add(name);
        }

        public void OnApplicationQuit()
        {
            if (ArenaClientScene.Instance != null)
                ArenaClientScene.Instance.IsShuttingDown = true;
        }
    }
}
