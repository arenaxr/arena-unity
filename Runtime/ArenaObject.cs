/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2023, Carnegie Mellon University. All rights reserved.
 */

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ArenaUnity.Components;
using ArenaUnity.Schemas;
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
        public object data = null; // original message data for object, if any
        [HideInInspector]
        public string object_type = null; // original message data for object, if any
        [HideInInspector]
        public string parentId = null;

        internal bool Created { get { return created; } set { created = value; } }

        private float publishInterval; // varies
        private bool created = false;
        private string oldName; // test for rename
        internal bool externalDelete = false;
        internal bool isJsonValidated = false;
        internal string gltfUrl = null;
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
            // TODO (mwfarb): prevent child objects of parent.transform.hasChanged = true from publishing unnecessarily

            while (true)
            {
                // send only when changed, each publishInterval
                if ((transform.hasChanged) && ArenaClientScene.Instance)
                {
                    int ms = objectUpdateMs != ArenaClientScene.Instance.globalUpdateMs ? objectUpdateMs : ArenaClientScene.Instance.globalUpdateMs;
                    publishInterval = (float)ms / 1000f;
                    if (PublishCreateUpdate(true))
                    {
                        transform.hasChanged = false;
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
            ArenaObjectJson msg = new ArenaObjectJson
            {
                object_id = oldName,
                action = "delete",
                persist = persist,
            };
            string payload = JsonConvert.SerializeObject(msg);
            if (ArenaClientScene.Instance)
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
                ArenaClientScene.Instance.arenaObjs[name] = gameObject;

            // message type information
            ArenaObjectJson msg = new ArenaObjectJson
            {
                object_id = name,
                action = created ? "update" : "create",
                type = messageType,
                persist = persist,
            };
            transformOnly = created ? transformOnly : false;
            ArenaObjectDataJson dataUnity = new ArenaObjectDataJson();
            if (!string.IsNullOrEmpty(object_type))
                dataUnity.object_type = object_type;
            else
                dataUnity.object_type = ToArenaObjectType(gameObject);

            //Debug.LogWarning(JsonConvert.SerializeObject(dataUnity));

            // minimum transform information
            dataUnity.position = ArenaUnity.ToArenaPosition(transform.localPosition);
            Quaternion rotOut = transform.localRotation;
            if (gltfTypeList.Where(x => x.Contains(dataUnity.object_type)).FirstOrDefault() != null)
                rotOut = ArenaUnity.UnityToGltfRotationQuat(transform.localRotation);
            dataUnity.rotation = ArenaUnity.ToArenaRotationQuat(rotOut); // always send quaternions over the wire
            dataUnity.scale = ArenaUnity.ToArenaScale(transform.localScale);
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

            var updatedData = new JObject();

            // other attributes information
            if (!transformOnly)
            {
                // wire objects
                if (GetComponent<ArenaMesh>())
                    updatedData.Merge(ArenaMesh.ToArenaMesh(gameObject));
                else if (GetComponent<Collider>())
                    updatedData.Merge(ArenaMesh.ToArenaDimensions(gameObject));
                else if (GetComponent<Light>())
                    updatedData.Merge(ArenaWireLight.ToArenaLight(gameObject));
                else if (GetComponent<TextMeshPro>())
                    updatedData.Merge(ArenaWireText.ToArenaText(gameObject));
                else if (GetComponent<LineRenderer>())
                    updatedData.Merge(ArenaWireThickline.ToArenaThickline(gameObject));

                // components
                if (GetComponent<Renderer>())
                    updatedData.Merge(ArenaMaterial.ToArenaMaterial(gameObject));
            }

            // merge unity data with original message data
            if (data != null)
                updatedData.Merge(JObject.FromObject(data));
            updatedData.Merge(JObject.FromObject(dataUnity));
            // TODO (mwfarb): check for deletions and pollution
            jsonData = JsonConvert.SerializeObject(updatedData, Formatting.Indented);

            // publish
            msg.data = transformOnly ? (object)dataUnity : updatedData;
            string payload = JsonConvert.SerializeObject(msg);
            if (ArenaClientScene.Instance)
                ArenaClientScene.Instance.PublishObject(msg.object_id, payload, HasPermissions);
            if (!created)
                created = true;

            return true;
        }

        internal void PublishUpdate(string objData, bool all = false, bool overwrite = false)
        {
            ArenaObjectJson msg = new ArenaObjectJson
            {
                object_id = name,
                action = "update",
                type = messageType,
                persist = persist,
            };
            if (overwrite) msg.overwrite = overwrite;

            // merge new data with original message data
            var updatedData = new JObject();
            if (jsonData != null)
                updatedData.Merge(JObject.Parse(jsonData));
            //Debug.Log(objData);
            updatedData.Merge(JObject.Parse(objData));

            jsonData = JsonConvert.SerializeObject(updatedData, Formatting.Indented);

            // publish
            msg.data = all ? updatedData : JObject.Parse(objData);
            string payload = JsonConvert.SerializeObject(msg);
            if (ArenaClientScene.Instance)
                ArenaClientScene.Instance.PublishObject(msg.object_id, payload, HasPermissions);
        }

        // object type
        public static string ToArenaObjectType(GameObject gobj)
        {
            string objectType = "entity";
            MeshFilter meshFilter = gobj.GetComponent<MeshFilter>();
            TextMeshPro tm = gobj.GetComponent<TextMeshPro>();
            Light light = gobj.GetComponent<Light>();
            SpriteRenderer spriteRenderer = gobj.GetComponent<SpriteRenderer>();
            LineRenderer lr = gobj.GetComponent<LineRenderer>();
            // initial priority is primitive
            if (spriteRenderer && spriteRenderer.sprite && spriteRenderer.sprite.pixelsPerUnit != 0f)
                objectType = "image";
            else if (lr)
                objectType = "thickline";
            else if (tm)
                objectType = "text";
            else if (light)
                objectType = "light";
            else if (meshFilter && meshFilter.sharedMesh)
                switch (meshFilter.sharedMesh.name) {
                    case "Cube": objectType = "box"; break;
                    case "Sphere": objectType = "sphere"; break;
                    case "Cylinder": objectType = "cylinder"; break;
                    case "Capsule": objectType = "capsule"; break;
                    case "Plane": objectType = "plane"; break;
                    case "Quad": objectType = "plane"; break;
                }
            return objectType;
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
