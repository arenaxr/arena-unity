/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2024, Carnegie Mellon University. All rights reserved.
 */

// Requires the built-in XR module (com.unity.modules.xr); compiled out when a
// consuming project has it disabled, so non-XR projects need no extra packages.
#if HAS_XR_MODULE
using System.Collections;
using System.Collections.Generic;
using ArenaUnity.Schemas;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.XR;

namespace ArenaUnity
{
    /// <summary>
    /// Publishes local XR hand-controller pose and button events to the ARENA MQTT broker,
    /// following the arena-web-core hand controller messaging format.
    /// Attach this component to each XR controller rig object (one per hand).
    /// </summary>
    [DisallowMultipleComponent]
    public class ArenaLocalHand : MonoBehaviour, IArenaPermissions
    {
        public bool HasPermissions { get; set; }

        public enum HandType { Left, Right }

        private const float handKeepAliveSec = 1f;

        [Tooltip("XR controller hand side")]
        public HandType hand = HandType.Left;
        [Tooltip("Override controller model URL (leave blank to use ARENA defaults)")]
        public string controllerUrl = null;
        [Tooltip("Override (globalUpdateMs) publish frequency to publish detected transform changes (milliseconds)")]
        [Range(50, 1000)]
        public int handUpdateMs = 100;

        /// <summary>The ARENA object ID assigned by auth for this hand.</summary>
        public string handid { get; internal set; }

        private bool created = false;
        private bool controllerConnected = false;
        private InputDevice xrDevice;
        private float publishInterval;

        // Previous button states for edge-transition detection
        private bool prevGrip = false;
        private bool prevPrimary = false;
        private bool prevSecondary = false;
        private bool prevThumbstick = false;
        private bool prevTrigger = false;

        void Start()
        {
            StartCoroutine(PublishTickThrottle());
            InputDevices.deviceConnected += OnXRDeviceChanged;
            InputDevices.deviceDisconnected += OnXRDeviceChanged;
            UpdateXRDevice();
        }

        void OnDestroy()
        {
            InputDevices.deviceConnected -= OnXRDeviceChanged;
            InputDevices.deviceDisconnected -= OnXRDeviceChanged;
        }

        private void OnXRDeviceChanged(InputDevice device)
        {
            UpdateXRDevice();
        }

        private void UpdateXRDevice()
        {
            XRNode node = hand == HandType.Left ? XRNode.LeftHand : XRNode.RightHand;
            List<InputDevice> devices = new List<InputDevice>();
            InputDevices.GetDevicesAtXRNode(node, devices);

            bool wasConnected = controllerConnected;
            if (devices.Count > 0 && devices[0].isValid)
            {
                xrDevice = devices[0];
                controllerConnected = true;
                if (!wasConnected && handid != null)
                    StartCoroutine(PublishControllerCreate());
            }
            else
            {
                if (wasConnected && handid != null)
                    PublishControllerDelete();
                xrDevice = default;
                controllerConnected = false;
                created = false;
            }
        }

        private IEnumerator PublishControllerCreate()
        {
            yield return null; // defer one frame so transform is ready
            PublishCreateUpdate(isCreate: true);
        }

        private void PublishControllerDelete()
        {
            if (ArenaClientScene.Instance == null || !ArenaClientScene.Instance.mqttClientConnected) return;
            if (ArenaClientScene.Instance.IsShuttingDown) return;

            ArenaMessageJson msg = new ArenaMessageJson
            {
                object_id = handid,
                action = "delete",
            };
            string payload = JsonConvert.SerializeObject(msg);
            ArenaClientScene.Instance.PublishHand(handid, payload);
        }

        IEnumerator PublishTickThrottle()
        {
            publishInterval = handKeepAliveSec;
            while (true)
            {
                if (handid != null && controllerConnected)
                {
                    if (transform.hasChanged && ArenaClientScene.Instance != null)
                    {
                        int ms = handUpdateMs != ArenaClientScene.Instance.globalUpdateMs
                            ? handUpdateMs
                            : ArenaClientScene.Instance.globalUpdateMs;
                        publishInterval = (float)ms / 1000f;
                        transform.hasChanged = false;
                    }
                    else
                    {
                        publishInterval = handKeepAliveSec;
                    }
                    PublishCreateUpdate();
                }
                yield return new WaitForSeconds(publishInterval);
            }
        }

        void Update()
        {
            if (handid == null || !controllerConnected || !xrDevice.isValid) return;

            // Grip button: gripdown / gripup
            if (xrDevice.TryGetFeatureValue(CommonUsages.gripButton, out bool grip))
            {
                if (grip && !prevGrip) PublishButtonEvent("gripdown");
                else if (!grip && prevGrip) PublishButtonEvent("gripup");
                prevGrip = grip;
            }

            // Primary face button (A on right / X on left): abuttondown/abuttonup  xbuttondown/xbuttonup
            if (xrDevice.TryGetFeatureValue(CommonUsages.primaryButton, out bool primary))
            {
                string downEvt = hand == HandType.Right ? "abuttondown" : "xbuttondown";
                string upEvt = hand == HandType.Right ? "abuttonup" : "xbuttonup";
                if (primary && !prevPrimary) PublishButtonEvent(downEvt);
                else if (!primary && prevPrimary) PublishButtonEvent(upEvt);
                prevPrimary = primary;
            }

            // Secondary face button (B on right / Y on left): bbuttondown/bbuttonup  ybuttondown/ybuttonup
            if (xrDevice.TryGetFeatureValue(CommonUsages.secondaryButton, out bool secondary))
            {
                string downEvt = hand == HandType.Right ? "bbuttondown" : "ybuttondown";
                string upEvt = hand == HandType.Right ? "bbuttonup" : "ybuttonup";
                if (secondary && !prevSecondary) PublishButtonEvent(downEvt);
                else if (!secondary && prevSecondary) PublishButtonEvent(upEvt);
                prevSecondary = secondary;
            }

            // Thumbstick click: thumbstickdown / thumbstickup
            if (xrDevice.TryGetFeatureValue(CommonUsages.primary2DAxisClick, out bool thumbstick))
            {
                if (thumbstick && !prevThumbstick) PublishButtonEvent("thumbstickdown");
                else if (!thumbstick && prevThumbstick) PublishButtonEvent("thumbstickup");
                prevThumbstick = thumbstick;
            }

            // Trigger button: triggerdown / triggerup
            if (xrDevice.TryGetFeatureValue(CommonUsages.triggerButton, out bool trigger))
            {
                if (trigger && !prevTrigger) PublishButtonEvent("triggerdown");
                else if (!trigger && prevTrigger) PublishButtonEvent("triggerup");
                prevTrigger = trigger;
            }
        }

        private string GetObjectType() => hand == HandType.Left ? "handLeft" : "handRight";

        private string GetControllerUrl()
        {
            if (!string.IsNullOrWhiteSpace(controllerUrl)) return controllerUrl;
            return hand == HandType.Left
                ? "/static/models/hands/valve_index_left.gltf"
                : "/static/models/hands/valve_index_right.gltf";
        }

        /// <summary>
        /// Publish hand controller pose as a create or update message.
        /// </summary>
        /// <param name="isCreate">Force a create action (used on first connection).</param>
        /// <returns>True if the message was published.</returns>
        public bool PublishCreateUpdate(bool isCreate = false)
        {
            if (ArenaClientScene.Instance == null || !ArenaClientScene.Instance.mqttClientConnected)
                return false;
            if (ArenaClientScene.Instance.IsShuttingDown) return false;

            ArenaMessageJson msg = new ArenaMessageJson
            {
                object_id = handid,
                action = (isCreate || !created) ? "create" : "update",
                type = "object",
                ttl = 30,
            };

            var handData = hand == HandType.Left
                ? (object)new ArenaHandLeftJson
                {
                    position = ArenaUnity.ToArenaPosition(transform.localPosition),
                    rotation = ArenaUnity.ToArenaRotationQuat(transform.localRotation),
                    url = GetControllerUrl(),
                    dep = ArenaClientScene.Instance.userid,
                }
                : (object)new ArenaHandRightJson
                {
                    position = ArenaUnity.ToArenaPosition(transform.localPosition),
                    rotation = ArenaUnity.ToArenaRotationQuat(transform.localRotation),
                    url = GetControllerUrl(),
                    dep = ArenaClientScene.Instance.userid,
                };

            var updatedData = new JObject();
            updatedData.Merge(JObject.FromObject(handData));
            msg.data = updatedData;

            string payload = JsonConvert.SerializeObject(msg);
            ArenaClientScene.Instance.PublishHand(handid, payload);

            if (!created)
                created = true;

            return true;
        }

        /// <summary>
        /// Publish a controller button/action event to ARENA.
        /// </summary>
        /// <param name="eventName">Arena event name, e.g. "gripdown", "abuttondown".</param>
        public void PublishButtonEvent(string eventName)
        {
            if (ArenaClientScene.Instance == null || !ArenaClientScene.Instance.mqttClientConnected) return;
            if (ArenaClientScene.Instance.IsShuttingDown) return;
            if (handid == null) return;

            ArenaVector3Json originPos = ArenaUnity.ToArenaPosition(transform.localPosition);

            ArenaMessageJson msg = new ArenaMessageJson
            {
                object_id = handid,
                action = "clientEvent",
                type = eventName,
                data = new JObject
                {
                    ["originPosition"] = JObject.FromObject(originPos),
                    ["target"] = handid,
                },
            };

            string payload = JsonConvert.SerializeObject(msg);
            ArenaClientScene.Instance.PublishHand(handid, payload);
        }
    }
}
#endif
