/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021, The CONIX Research Center. All rights reserved.
 */

using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using ArenaUnity.Components;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PrettyHierarchy;
using TMPro;
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
        [Tooltip("Override (globalUpdateMs) publish frequency to publish detected transform changes (milliseconds)")]
        [Range(100, 1000)]
        public int objectUpdateMs = 100;
        [TextArea(5, 20)]
        [Tooltip("ARENA JSON-encoded message")]
        [SerializeField]
        public string jsonData = null;

        [HideInInspector]
        public dynamic data = null; // original message data for object, if any
        [HideInInspector]
        public string parentId = null;

        internal bool Created { get { return created; } set { created = value; } }

        private float publishInterval; // varies
        private bool created = false;
        private string oldName; // test for rename
        internal bool externalDelete = false;
        internal bool isJsonValidated = false;
        internal string gltfUrl = null;
        internal bool meshChanged = false;
        internal List<string> animations = null; // TODO (mwfarb): ideal location: ArenaGltfModel component

        internal List<string> gltfTypeList = new List<string> { "gltf-model", "handLeft", "handRight" };

        public void OnEnable()
        {
#if UNITY_EDITOR
            // sort arena component to the top, below Transform
            while (UnityEditorInternal.ComponentUtility.MoveComponentUp(this)) { }
#endif
        }

        void Start()
        {
            // TODO: consider how inactive objects react to find here, might need to use arenaObjs array

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
            StartCoroutine(PublishTickThrottle());
        }


        public void SetTtlDeleteTimer(float seconds)
        {
            StartCoroutine(TtlUpdater(seconds));
        }

        IEnumerator TtlUpdater(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            externalDelete = true;
            Destroy(gameObject);
        }

        IEnumerator PublishTickThrottle()
        {
            //TODO: prevent child objects of parent.transform.hasChanged = true from publishing unnecessarily

            while (true)
            {
                // send only when changed, each publishInterval
                if ((transform.hasChanged || meshChanged) && ArenaClientScene.Instance)
                {
                    publishInterval = ((float)ArenaClientScene.Instance.globalUpdateMs / 1000f);
                    if (PublishCreateUpdate(true))
                    {
                        transform.hasChanged = false;
                        meshChanged = false;
                    }
                }
                yield return new WaitForSeconds(publishInterval);
            }
        }

        void Update()
        {
            if (ArenaClientScene.Instance != null)
                HasPermissions = ArenaClientScene.Instance.sceneObjectRights;

            if (oldName != null && name != oldName)
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
            ArenaClientScene.Instance.PublishObject(msg.object_id, payload, HasPermissions);
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
            Quaternion rotOut = transform.localRotation;
            if (gltfTypeList.Where(x => x.Contains(dataUnity.object_type)).FirstOrDefault() != null)
                ArenaUnity.UnityToGltfRotationQuat(transform.localRotation);
            dataUnity.rotation = ArenaUnity.ToArenaRotationQuat(rotOut); // always send quaternions over the wire
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
                if (GetComponent<TextMeshPro>())
                    ArenaUnity.ToArenaText(gameObject, ref dataUnity);
                if (GetComponent<LineRenderer>())
                    ArenaUnity.ToArenaLine(gameObject, ref dataUnity);
            }

            // merge unity data with original message data
            var updatedData = new JObject();
            if (data != null)
                updatedData.Merge(JObject.Parse(JsonConvert.SerializeObject(data)));
            updatedData.Merge(JObject.Parse(JsonConvert.SerializeObject(dataUnity)));
            // TODO: temp location until JObject completely replaces dynamic object
            if (GetComponent<ArenaAnimationMixer>())
                ArenaUnity.ToArenaAnimationMixer(gameObject, ref updatedData);
            if (GetComponent<ArenaClickListener>())
                ArenaUnity.ToArenaClickListener(gameObject, ref updatedData);

            jsonData = JsonConvert.SerializeObject(updatedData, Formatting.Indented);

            // publish
            msg.data = transformOnly ? dataUnity : updatedData;
            string payload = JsonConvert.SerializeObject(msg);
            ArenaClientScene.Instance.PublishObject(msg.object_id, payload, HasPermissions);
            if (!created)
                created = true;

            return true;
        }

        internal void PublishUpdate(string objData, bool all = false, bool overwrite = false)
        {
            dynamic msg = new ExpandoObject();
            msg.object_id = name;
            msg.action = "update";
            msg.type = messageType;
            msg.persist = persist;
            if (overwrite) msg.overwrite = overwrite;

            // merge new data with original message data
            var updatedData = new JObject();
            if (jsonData != null)
                updatedData.Merge(JObject.Parse(jsonData));
            updatedData.Merge(JObject.Parse(objData));

            jsonData = JsonConvert.SerializeObject(updatedData, Formatting.Indented);

            // publish
            msg.data = all ? updatedData : JObject.Parse(objData);
            string payload = JsonConvert.SerializeObject(msg);
            ArenaClientScene.Instance.PublishObject(msg.object_id, payload, HasPermissions);
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

            // check if delete messages should be sent to other subscribers
            if (!externalDelete || (transform.parent != null && transform.parent.GetComponent<ArenaObject>() == null))
                ArenaClientScene.Instance.pendingDelete.Add(name);
        }

        public void OnApplicationQuit()
        {
            if (ArenaClientScene.Instance != null)
                ArenaClientScene.Instance.IsShuttingDown = true;
        }
    }
}
