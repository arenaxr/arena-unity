﻿/**
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
        private const float avatarPublishIntervalSeconds = 1f;

        private string messageType = "object";
        private bool persist = false;
        private Color displayColor = Color.white;

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
            StartCoroutine(CameraUpdater());
        }

        IEnumerator CameraUpdater()
        {
            while (true)
            {
                PublishCreateUpdate();
                yield return new WaitForSeconds(avatarPublishIntervalSeconds);
            }
        }

        void Update()
        {
            // send only when changed, each publishInterval frames, or stop at 0 frames
            if (!ArenaClientScene.Instance || ArenaClientScene.Instance.transformPublishInterval == 0 ||
            Time.frameCount % ArenaClientScene.Instance.transformPublishInterval != 0)
                return;
            if (transform.hasChanged)
            {
                //TODO: prevent child objects of parent.transform.hasChanged = true from publishing unnecessarily

                if (PublishCreateUpdate())
                {
                    transform.hasChanged = false;
                }
            }
        }

        public bool PublishCreateUpdate()
        {
            if (ArenaClientScene.Instance == null || !ArenaClientScene.Instance.mqttClientConnected)
                return false;
            if (ArenaClientScene.Instance.IsShuttingDown) return false;
            if (messageType != "object") return false;
            if (!ArenaClientScene.Instance.publishCamera) return false;

            // message type information
            dynamic msg = new ExpandoObject();
            msg.object_id = ArenaClientScene.Instance.camid;
            msg.action = created ? "update" : "create";
            msg.type = messageType;
            msg.persist = persist;
            msg.displayName = !string.IsNullOrWhiteSpace(ArenaClientScene.Instance.displayName) ?
                ArenaClientScene.Instance.displayName : ArenaClientScene.Instance.userid;

            dynamic dataUnity = new ExpandoObject();
            dataUnity.object_type = "camera";
            dataUnity.headModelPath = ArenaClientScene.Instance.headModelPath;
            dataUnity.color = ArenaUnity.ToArenaColor(displayColor);

            // minimum transform information
            dataUnity.position = ArenaUnity.ToArenaPosition(transform.localPosition);
            dataUnity.rotation = ArenaUnity.ToArenaRotationQuat(transform.localRotation);

            // publish
            msg.data = dataUnity;
            string payload = JsonConvert.SerializeObject(msg);
            ArenaClientScene.Instance.PublishObject(msg.object_id, payload);
            if (!created)
                created = true;

            return true;
        }

    }
}
