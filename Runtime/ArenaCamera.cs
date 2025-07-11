﻿/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2023, Carnegie Mellon University. All rights reserved.
 */

using System.Collections;
using ArenaUnity.Components;
using ArenaUnity.Schemas;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
        public Color displayColor { get; internal set; }
        public string userid { get; internal set; }
        public string camid { get; internal set; }

        [Tooltip("User display name")]
        public string displayName = null;
        [Tooltip("Path to user head model")]
        public string headModelPath = "/static/models/avatars/robobit.glb";
        [Tooltip("Override (cameraUpdateMs) publish frequency to publish detected transform changes (milliseconds)")]
        [Range(100, 1000)]
        public int cameraUpdateMs = 100;

        [HideInInspector]
        protected bool created = false;

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
                        int ms = cameraUpdateMs != ArenaClientScene.Instance.globalUpdateMs ? cameraUpdateMs : ArenaClientScene.Instance.globalUpdateMs;
                        publishInterval = (float)ms / 1000f;
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
            ArenaObjectJson msg = new ArenaObjectJson
            {
                object_id = camid,
                action = created ? "update" : "create",
                type = messageType,
                ttl = 30,
            };
            if (string.IsNullOrWhiteSpace(displayName))
            {   // provide default name if needed
                displayName = name;
            }

            // minimum transform information
            ArenaCameraJson dataUnity = new ArenaCameraJson
            {
                position = ArenaUnity.ToArenaPosition(transform.localPosition),
                rotation = ArenaUnity.ToArenaRotationQuat(transform.localRotation), // always send quaternions over the wire
                ArenaUser = new ArenaArenaUserJson
                {
                    displayName = displayName,
                    headModelPath = headModelPath,
                    color = ArenaUnity.ToArenaColor(displayColor),
                }
            };

            var updatedData = new JObject();
            updatedData.Merge(JObject.FromObject(dataUnity));

            // publish
            msg.data = updatedData;
            string payload = JsonConvert.SerializeObject(msg);
            if (ArenaClientScene.Instance)
                ArenaClientScene.Instance.PublishCamera(msg.object_id, payload);
            if (!created)
                created = true;

            return true;
        }

    }
}
