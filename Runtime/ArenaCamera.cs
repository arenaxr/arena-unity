/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021, The CONIX Research Center. All rights reserved.
 */

using System.Collections;
using System.Dynamic;
using Newtonsoft.Json;
using PrettyHierarchy;
using UnityEngine;

namespace ArenaUnity
{
    [DisallowMultipleComponent]
    public class ArenaCamera : PrettyObject
    {
        private const float cameraKeepAliveMs = 1f; // 1 second
        private float publishInterval; // varies

        private string messageType = "object";
        private bool persist = false;
        private Color displayColor = Color.white;
        internal string userid = null;
        internal string camid = null;

        [Tooltip("User display name")]
        public string displayName = null;
        [Tooltip("Path to user head model")]
        public string headModelPath = "/static/models/avatars/robobit.glb";
        [Tooltip("Override (globalUpdateMs) publish frequency to publish detected transform changes (milliseconds)")]
        [Range(100, 1000)]
        public int cameraUpdateMs = 100;

        [HideInInspector]
        protected bool created = false;

        public void OnEnable()
        {
#if UNITY_EDITOR
            // sort arena component to the top, below Transform
            while (UnityEditorInternal.ComponentUtility.MoveComponentUp(this)) { }
#endif
        }

        void Start()
        {
            displayColor = ArenaUnity.ColorRandom();
            StartCoroutine(PublishTickThrottle());
        }

        IEnumerator PublishTickThrottle()
        {
            while (true)
            {
                if (userid != null && camid != null)
                {
                    // send more frequently when changed, otherwise minimum 1 second keep alive
                    if (transform.hasChanged && ArenaClientScene.Instance)
                    {
                        publishInterval = ((float)ArenaClientScene.Instance.globalUpdateMs / 1000f);
                        transform.hasChanged = false;
                    }
                    else
                    {
                        publishInterval = cameraKeepAliveMs;
                    }
                    PublishCreateUpdate();
                }
                yield return new WaitForSeconds(publishInterval);
            }
        }

        void Update()
        {
            // let CameraTickThrottle handle publish frequency
        }

        public bool PublishCreateUpdate()
        {
            if (ArenaClientScene.Instance == null || !ArenaClientScene.Instance.mqttClientConnected)
                return false;
            if (ArenaClientScene.Instance.IsShuttingDown) return false;
            if (messageType != "object") return false;

            // message type information
            dynamic msg = new ExpandoObject();
            msg.object_id = camid;
            msg.action = created ? "update" : "create";
            msg.type = messageType;
            msg.persist = persist;
            if (string.IsNullOrWhiteSpace(displayName))
            {   // provide default name if needed
                displayName = name;
            }
            msg.displayName = displayName;

            dynamic dataUnity = new ExpandoObject();
            dataUnity.object_type = "camera";
            dataUnity.headModelPath = headModelPath;
            dataUnity.color = ArenaUnity.ToArenaColor(displayColor);

            // minimum transform information
            dataUnity.position = ArenaUnity.ToArenaPosition(transform.localPosition);
            dataUnity.rotation = ArenaUnity.ToArenaRotationQuat(transform.localRotation); // always send quaternions over the wire

            // publish
            msg.data = dataUnity;
            string payload = JsonConvert.SerializeObject(msg);
            ArenaClientScene.Instance.PublishCamera(msg.object_id, payload, HasPermissions);
            if (!created)
                created = true;

            return true;
        }

    }
}
