/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2023, Carnegie Mellon University. All rights reserved.
 */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ArenaUnity.Components;
using ArenaUnity.Schemas;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace ArenaUnity
{
    /// <summary>
    /// Object CRUD, message routing, and publishing for the ARENA scene.
    /// </summary>
    public partial class ArenaClientScene
    {
        private static readonly JsonSerializerSettings JsonIgnoreNulls = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };

        private void CreateUpdateObject(ArenaMessageJson msg, object indata, object menuCommand = null)
        {
            ArenaObject aobj = null;
            arenaObjs.TryGetValue(msg.object_id, out GameObject gobj);
            if (gobj != null)
            {   // update local
                aobj = gobj.GetComponent<ArenaObject>();
            }
            else
            {   // create local
#if !UNITY_EDITOR
                Debug.Log($"Loading object '{msg.object_id}'..."); // show new objects in log
#endif
                gobj = new GameObject();
                gobj.name = msg.object_id;
                aobj = gobj.AddComponent(typeof(ArenaObject)) as ArenaObject;
                arenaObjs[msg.object_id] = gobj;
                aobj.Created = true;
#if UNITY_EDITOR
                // local create context auto-select
                if (menuCommand != null)
                {
                    // Register the creation in the undo system
                    UnityEditor.Undo.RegisterCreatedObjectUndo(gobj, "Create " + gobj.name);
                    UnityEditor.Selection.activeObject = gobj;
                }
#endif
            }

            // apply storage-level object properties
            if (msg.persist.HasValue)
                aobj.persist = (bool)msg.persist;
            aobj.messageType = msg.type;
            if (msg.ttl != null)
            {
                if (!gobj.TryGetComponent<ArenaTtl>(out var ttl))
                    ttl = gobj.AddComponent<ArenaTtl>();
                ttl.SetTtlDeleteTimer((float)msg.ttl);
            }

            JObject jData = JObject.Parse(JsonConvert.SerializeObject(indata));

            if (aobj == null) Debug.LogError($"invalid aobj from {JsonConvert.SerializeObject(indata)}");

            switch (msg.type)
            {
                case "object":
                    UpdateObjectMessage(msg, indata, aobj, gobj, jData);
                    break;
                case "scene-options":
                    UpdateSceneOptionsMessage(indata, aobj, gobj, jData);
                    break;
                case "program":
                    // TODO (mwfarb): define program implementation or lack thereof
                    break;
            }

            gobj.transform.hasChanged = false;

            if (aobj != null)
            {
                aobj.data = indata;
                var updatedData = jData;
                if (aobj.jsonData != null)
                    updatedData.Merge(JObject.Parse(aobj.jsonData));
                updatedData.Merge(indata);
                aobj.jsonData = JsonConvert.SerializeObject(updatedData, Formatting.Indented);
            }
        }

        private void UpdateSceneOptionsMessage(object indata, ArenaObject aobj, GameObject gobj, JObject jData)
        {
            ArenaDataSceneOptionsJson data = JsonConvert.DeserializeObject<ArenaDataSceneOptionsJson>(indata.ToString(), JsonIgnoreNulls);

            // handle scene options attributes
            foreach (var result in jData)
            {
                switch (result.Key)
                {
                    case "env-presets": ArenaUnity.ApplyEnvironmentPresets(gobj, jData["env-presets"]); break;
                    case "scene-options":
                        ArenaUnity.ApplySceneOptions(gobj, data); break;
                    case "renderer-settings": ArenaUnity.ApplyRendererSettings(gobj, data); break;
                    case "post-processing": ArenaUnity.ApplyPostProcessing(gobj, data); break;
                }
            }
        }

        private void UpdateObjectMessage(ArenaMessageJson msg, object indata, ArenaObject aobj, GameObject gobj, JObject jData)
        {
            ArenaDataObjectJson data = JsonConvert.DeserializeObject<ArenaDataObjectJson>(indata.ToString(), JsonIgnoreNulls);

            // modify Unity attributes
            bool worldPositionStays = false; // default: most children need relative position
            aobj.parentId = (string)data.Parent;
            string parent = (string)data.Parent;
            string object_type = (string)data.object_type;
            aobj.object_type = object_type;
            var url = !string.IsNullOrEmpty(data.Src) ? data.Src : data.Url;

            // handle wire object attributes
            switch (object_type)
            {
                // deprecation warnings
                case "cube":
                    Debug.LogWarning($"data.object_type: {object_type} is deprecated for object-id: {msg.object_id}, use data.object_type: box instead.");
                    ArenaUnity.ApplyWireBox(indata, gobj); break;

                // wire object primitives
                case "box":
                case "capsule":
                case "circle":
                case "cone":
                case "cylinder":
                case "dodecahedron":
                case "icosahedron":
                case "octahedron":
                case "plane":
                case "ring":
                case "roundedbox":
                case "sphere":
                case "tetrahedron":
                case "torus":
                case "torusKnot":
                case "triangle":
                case "videosphere":
                    ArenaUnity.ApplyGeometry(object_type, indata, gobj); break;

                // other wire objects
                case "entity": /* general GameObject */ break;
                case "light": ArenaUnity.ApplyWireLight(indata, gobj); break;
                case "text": ArenaUnity.ApplyWireText(indata, gobj); break;
                case "line": ArenaUnity.ApplyWireLine(indata, gobj); break;
                case "thickline": ArenaUnity.ApplyWireThickline(indata, gobj); break;

                // ARENAUI objects
                case "arenaui-card": ArenaUnity.ApplyWireArenauiCard(indata, gobj); break;
                case "arenaui-button-panel": ArenaUnity.ApplyWireArenauiButtonPanel(indata, gobj); break;
                case "arenaui-prompt": ArenaUnity.ApplyWireArenauiPrompt(indata, gobj); break;

                // other wire objects
                case "ocean": ArenaUnity.ApplyWireOcean(indata, gobj); break;
                case "pcd-model": ArenaUnity.ApplyWirePcdModel(indata, gobj); break;
                case "threejs-scene": ArenaUnity.ApplyWireThreejsScene(indata, gobj); break;
                case "gaussian_splatting": ArenaUnity.ApplyWireGaussianSplatting(indata, gobj); break;
                case "obj-model": ArenaUnity.ApplyWireObjModel(indata, gobj); break;
                case "gltf-model": ArenaUnity.ApplyWireGltfModel(indata, gobj); break;
                case "image": ArenaUnity.ApplyWireImage(indata, gobj); break;

                // user avatar objects
                case "camera":
                    if (renderCameras)
                        ArenaUnity.ApplyCameraAvatar(msg.object_id, indata, gobj);
                    break;
                case "handLeft":
                case "handRight":
                    if (renderHands)
                        ArenaUnity.ApplyHandAvatar(msg.object_id, url, gobj);
                    break;
            }

            // handle data.parent BEFORE setting transform in case object becomes unparented
            if (parent != null)
            {
                // establish parent/child relationships
                if (arenaObjs.ContainsKey(parent))
                {
                    gobj.SetActive(true);
                    // makes the child keep its local orientation rather than its global orientation
                    gobj.transform.SetParent(arenaObjs[parent].transform, worldPositionStays);
                }
                else
                {
                    gobj.SetActive(false);
                    parentalQueue.Add(parent);
                    childObjs[msg.object_id] = gobj;
                }
            }
            else
            {
                if (gobj.transform.parent != null)
                {
                    gobj.transform.SetParent(null);
                }
            }

            if (parentalQueue.Contains(msg.object_id))
            {
                // find children awaiting a parent
                foreach (KeyValuePair<string, GameObject> cgobj in childObjs)
                {
                    string cparent = cgobj.Value.GetComponent<ArenaObject>().parentId;
                    if (cparent != null && cparent == msg.object_id)
                    {
                        cgobj.Value.SetActive(true);
                        // makes the child keep its local orientation rather than its global orientation
                        cgobj.Value.transform.SetParent(arenaObjs[msg.object_id].transform, worldPositionStays);
                        cgobj.Value.transform.hasChanged = false;
                        //childObjs.Remove(cgobj.Key);
                    }
                }
                parentalQueue.Remove(msg.object_id);
            }

            // apply transform triad
            if (data.Position != null)
            {
                gobj.transform.localPosition = ArenaUnity.ToUnityPosition(data.Position);
            }
            if (data.Rotation != null)
            {
                bool invertY = true;
                if (jData.SelectToken("rotation.w") != null) // quaternion
                    gobj.transform.localRotation = ArenaUnity.ToUnityRotationQuat(data.Rotation, invertY);
                else // euler
                    gobj.transform.localRotation = ArenaUnity.ToUnityRotationEuler(data.Rotation, invertY);
            }
            if (data.Scale != null)
            {
                gobj.transform.localScale = ArenaUnity.ToUnityScale(data.Scale);
            }

            // apply rendering visibility attributes, before other on-wire object attributes
            if (data.Visible != null && !(bool)data.Visible) // visible, if set is highest priority to enable/disable renderer
            {
                ArenaUnity.ApplyVisible(gobj, data);
            }
            else if (data.RemoteRender != null) // remote-render, if set is lowest priority to enable/disable renderer
            {
                ArenaUnity.ApplyRemoteRender(gobj, data);
            }

            // handle non-wire object attributes, as they occur in json
            foreach (var result in jData)
            {
                switch (result.Key)
                {
                    // deprecation warnings
                    case "color":
                        if (ArenaUnity.primitives.Contains(object_type))
                        {
                            Debug.LogWarning($"data.color is deprecated for object-id: {msg.object_id}, object_type: {object_type}, use data.material.color instead.");
                            data.Material ??= new ArenaMaterialJson();
#pragma warning disable 0618 // Intentionally consuming deprecated property for backward compatibility
                            data.Material.Color = data.Color;
#pragma warning restore 0618
                            ArenaUnity.ApplyMaterial(gobj, data);
                        }
                        break;
                    case "light":
                        if (object_type == "light")
                        {
                            Debug.LogWarning($"data.light.[property] is deprecated for object-id: {msg.object_id}, object_type: {object_type}, use data.[property] instead.");
#pragma warning disable 0618 // Intentionally consuming deprecated property for backward compatibility
                            ArenaUnity.ApplyWireLight(data.Light, gobj);
#pragma warning restore 0618
                        }
                        break;
                    case "text":
                        if (object_type == "text")
                        {
                            Debug.LogWarning($"data.text is deprecated for object-id: {msg.object_id}, object_type: {object_type}, use data.value instead.");
                        }
                        break;

                    // expected attributes
                    case "animation": ArenaUnity.ApplyAnimation(gobj, data); break;
                    case "armarker": ArenaUnity.ApplyArmarker(gobj, data); break;
                    case "click-listener": ArenaUnity.ApplyClickListener(gobj, data); break;
                    // TODO: case "blip": ArenaUnity.ApplyBlip(gobj, data); break;
                    case "physx-body": ArenaUnity.ApplyPhysxBody(gobj, data); break;
                    case "physx-material": ArenaUnity.ApplyPhysxMaterial(gobj, data); break;
                    case "box-collision-listener": ArenaUnity.ApplyBoxCollisionListener(gobj, data); break;
                    case "physx-joint": ArenaUnity.ApplyPhysxJoint(gobj, data); break;
                    case "physx-force-pushable": ArenaUnity.ApplyPhysxForcePushable(gobj, data); break;
                    case "geometry":
                        if (object_type == "entity")
                            ArenaUnity.ApplyGeometry(null, data.Geometry, gobj); break;
                    case "goto-landmark": ArenaUnity.ApplyGotoLandmark(gobj, data); break;
                    case "goto-url": ArenaUnity.ApplyGotoUrl(gobj, data); break;
                    // TODO: case "hide-on-enter-ar": ArenaUnity.ApplyHideOnEnterAr(gobj, data); break;
                    // TODO: case "hide-on-enter-vr": ArenaUnity.ApplyHideOnEnterVr(gobj, data); break;
                    // TODO: case "show-on-enter-ar": ArenaUnity.ApplyShowOnEnterAr(gobj, data); break;
                    // TODO: case "show-on-enter-vr": ArenaUnity.ApplyShowOnEnterVr(gobj, data); break;
                    // TODO: case "landmark": ArenaUnity.ApplyLandmark(gobj, data); break;
                    case "material-extras": ArenaUnity.ApplyMaterialExtras(gobj, data); break;
                    case "shadow": ArenaUnity.ApplyShadow(gobj, data); break;
                    case "sound": ArenaUnity.ApplySound(gobj, data); break;
                    // TODO: case "textinput": ArenaUnity.ApplyTextInput(gobj, data); break;
                    // TODO: case "screenshareable": ArenaUnity.ApplyScreensharable(gobj, data); break;
                    case "video-control": ArenaUnity.ApplyVideoControl(gobj, data); break;
                    case "attribution": ArenaUnity.ApplyAttribution(gobj, data); break;
                    case "spe-particles": ArenaUnity.ApplySpeParticles(gobj, data); break;
                    // TODO: case "buffer": ArenaUnity.ApplyBuffer(gobj, data); break;
                    // TODO: case "jitsi-video": ArenaUnity.ApplyJitsiVideo(gobj, data); break;
                    // TODO: case "multisrc": ArenaUnity.ApplyMultiSrc(gobj, data); break;
                    // TODO: case "skipCache": ArenaUnity.ApplySkipCache(gobj, data); break;
                    case "animation-mixer": ArenaUnity.ApplyAnimationMixer(gobj, data); break;
                    case "gltf-model-lod": ArenaUnity.ApplyGltfModelLod(gobj, data); break;
                    case "gltf-morph": ArenaUnity.ApplyGltfMorph(gobj, data); break;
                    case "modelUpdate": ArenaUnity.ApplyModelUpdate(gobj, data); break;
                    case "material": ArenaUnity.ApplyMaterial(gobj, data); break;
                }
            }
        }

        private void RemoveObject(string object_id)
        {
            if (!arenaObjs.TryGetValue(object_id, out GameObject gobj))
                return;
            if (gobj == null) return;

            // recursively remove children if any of go
            foreach (Transform child in gobj.transform)
            {
                RemoveObject(child.name);
            }
            // remove go
            if (gobj.TryGetComponent<ArenaObject>(out var aobj))
            {
                aobj.externalDelete = true;
            }
            Destroy(gobj);
            // remove internal list ref
            arenaObjs.Remove(object_id);
        }

        protected override void DecodeMessage(string topic, byte[] message)
        {
            string msgJson = System.Text.Encoding.UTF8.GetString(message);
            ProcessMessage(topic, msgJson);
        }

        /// <summary>
        /// Ingest point for messages to be received in the local scene.
        /// </summary>
        /// <param name="topic">The MQTT topic.</param>
        /// <param name="message">The JSON ARENA wire format string.</param>
        public void ProcessMessage(string topic, string message)
        {
            // Call the delegate if a user has defined it
            OnMessageCallback?.Invoke(topic, message);

            // Section 2.3: Payload must not be empty/null → warned and dropped
            if (string.IsNullOrWhiteSpace(message))
            {
                Debug.LogWarning($"Received empty MQTT payload on topic '{topic}', dropping.");
                return;
            }

            // filter messages based on expected payload format
            var topicSplit = topic.Split('/');
            if (topicSplit.Length > ArenaMqttClient.msgTypeRenderIdx)
            {
                var sceneMsgType = topicSplit[ArenaMqttClient.msgTypeRenderIdx];
                LogMessage("Received", sceneMsgType, topic, message);
                switch (sceneMsgType)
                {
                    case "x":
                    case "u":
                    case "o":
                        // Section 4.1: Messages from own userClient (topic position 5) are ignored → silently skipped
                        if (!string.IsNullOrEmpty(userclient)
                            && topicSplit.Length > (int)ArenaTopicTokens.USER_CLIENT
                            && topicSplit[(int)ArenaTopicTokens.USER_CLIENT] == userclient)
                            return;

                        // Section 4.2: Object messages for own camera/hands are ignored → silently skipped
                        if (topicSplit.Length > (int)ArenaTopicTokens.UUID)
                        {
                            var topicUuid = topicSplit[(int)ArenaTopicTokens.UUID];
                            if (localCameraIds.Contains(topicUuid)
                                || (!string.IsNullOrEmpty(handleftid) && topicUuid == handleftid)
                                || (!string.IsNullOrEmpty(handrightid) && topicUuid == handrightid))
                                return;
                        }

                        // Section 2.1: Payload must be valid JSON → silently dropped
                        ArenaMessageJson msg;
                        try
                        {
                            msg = JsonConvert.DeserializeObject<ArenaMessageJson>(message);
                        }
                        catch (JsonException ex)
                        {
                            Debug.LogWarning($"Invalid JSON in MQTT payload on topic '{topic}', dropping: {ex.Message}");
                            return;
                        }
                        if (msg == null)
                        {
                            Debug.LogWarning($"Null MQTT payload on topic '{topic}', dropping.");
                            return;
                        }

                        // Section 3.1: object_id must be present → warned and dropped
                        if (string.IsNullOrWhiteSpace(msg.object_id))
                        {
                            Debug.LogWarning($"Missing object_id in MQTT message on topic '{topic}', dropping.");
                            return;
                        }

                        // Section 2.2: Payload object_id must match topic UUID (position 6) → silently dropped
                        if (topicSplit.Length > (int)ArenaTopicTokens.UUID)
                        {
                            var topicUuid = topicSplit[(int)ArenaTopicTokens.UUID];
                            if (msg.object_id != topicUuid)
                            {
                                Debug.LogWarning($"object_id '{msg.object_id}' does not match topic UUID '{topicUuid}', dropping.");
                                return;
                            }
                        }

                        if (loadLiveObjects)
                        {
                            ProcessArenaMessage(msg, topicSplit);
                        }
                        break;
                    case "r": // remote render handled by arena-renderfusion package currently
                    case "c": // chat not implemented in unity currently
                    case "p": // program not implemented in unity currently
                    case "e": // environment not implemented in unity currently
                    case "d": // debug not implemented in unity currently
                        break;
                    default:
                        // ????
                        Debug.LogWarning($"Invalid scene message type: {sceneMsgType}");
                        break;
                }
            }
        }

        private void ProcessArenaMessage(ArenaMessageJson msg, string[] topicSplit = null, object menuCommand = null)
        {
            // Section 3.5: type field should be present → warning, processing continues
            if (string.IsNullOrWhiteSpace(msg.type))
            {
                Debug.LogWarning($"Missing type in MQTT message for object_id '{msg.object_id}'.");
            }

            // Section 3.2: action must be present → warned and dropped
            if (string.IsNullOrWhiteSpace(msg.action))
            {
                Debug.LogWarning($"Missing action in MQTT message for object_id '{msg.object_id}', dropping.");
                return;
            }

            // Section 3.3: action must be one of: create, update, delete, clientEvent → warned and dropped
            // 'join'/'leave' presence announcements are also valid wire traffic from web
            // clients (e.g. RenderFusion viewers announce with 'join'); 'leave' is consumed
            // below, 'join' is not ours to handle — accepted and ignored without warning.
            if (msg.action != "create" && msg.action != "update" && msg.action != "delete"
                && msg.action != "clientEvent" && msg.action != "leave" && msg.action != "join")
            {
                Debug.LogWarning($"Invalid action '{msg.action}' in MQTT message for object_id '{msg.object_id}', dropping.");
                return;
            }

            // consume object updates
            string object_id;
            switch ((string)msg.action)
            {
                case "create":
                case "update":
                    // Section 3.4: data must be present for create/update → warned and dropped
                    if (msg.data == null)
                    {
                        Debug.LogWarning($"Missing data in MQTT {msg.action} message for object_id '{msg.object_id}', dropping.");
                        return;
                    }

                    // Section 3.6: object_type should be present for object creates → warning, processing continues
                    if (msg.action == "create" && msg.type == "object")
                    {
                        if (msg.data is JObject jData)
                        {
                            if (string.IsNullOrWhiteSpace(jData["object_type"]?.ToString()))
                            {
                                Debug.LogWarning($"Missing object_type in MQTT create message for object_id '{msg.object_id}'.");
                            }
                        }
                    }

                    // Section 6.3/6.4: camera-override and rig messages only processed if addressed to own camera → silently ignored
                    if (msg.type == "camera-override" || msg.type == "rig")
                    {
                        bool addressedToOwnCamera = topicSplit != null
                            && topicSplit.Length > (int)ArenaTopicTokens.TO_UID
                            && localCameraIds.Contains(topicSplit[(int)ArenaTopicTokens.TO_UID]);
                        if (!addressedToOwnCamera)
                            return;
                    }

                    object_id = (string)msg.object_id;
                    string msg_type = (string)msg.type;

                    IEnumerable<string> uris = ExtractAssetUris(msg.data, msgUriTags);
                    foreach (var uri in uris)
                    {
                        if (!string.IsNullOrWhiteSpace(uri))
                        {
                            StartCoroutine(DownloadAssets(msg_type, uri));
                        }
                    }
                    CreateUpdateObject(msg, msg.data, menuCommand);
                    break;
                case "delete":
                    object_id = (string)msg.object_id;
                    RemoveObject(object_id);
                    break;
                case "leave":
                    object_id = (string)msg.object_id;
                    RemoveObject(object_id);
                    // camera special case, look for hands to delete
                    RemoveObject($"{prefixHandL}{object_id}");
                    RemoveObject($"{prefixHandR}{object_id}");
                    break;
                case "join":
                    // presence announcement; consumed by signaling layers
                    // (e.g. arena-renderfusion), nothing to do in the scene graph
                    break;
                case "clientEvent":
                    // Section 3.4: data must be present for clientEvent → silently dropped
                    if (msg.data == null)
                        return;
                    ClientEventOnObject(msg, topicSplit);
                    break;
                default:
                    break;
            }
        }

        private void ClientEventOnObject(ArenaMessageJson msg, string[] topicSplit = null)
        {
            var object_id = (string)msg.object_id;
            var msg_type = (string)msg.type;

            // Section 6.2: goto-url and textinput events from remote clients are blocked → silently ignored
            if (msg_type == "goto-url" || msg_type == "textinput")
                return;

            ArenaEventJson evt;
            try
            {
                evt = JsonConvert.DeserializeObject<ArenaEventJson>(msg.data.ToString());
            }
            catch (JsonException ex)
            {
                Debug.LogWarning($"Invalid JSON in clientEvent data for object_id '{object_id}', dropping: {ex.Message}");
                return;
            }
            if (evt == null)
                return;

            // Section 6.1: clientEvent source must match topic toUid (position 7) → warned and dropped
            if (topicSplit != null && topicSplit.Length > (int)ArenaTopicTokens.TO_UID)
            {
                string toUid = topicSplit[(int)ArenaTopicTokens.TO_UID];
#pragma warning disable 0618
                string source = evt.Source;
#pragma warning restore 0618
                if (!string.IsNullOrEmpty(source) && source != toUid)
                {
                    Debug.LogWarning($"clientEvent source '{source}' does not match topic toUid '{toUid}' for object_id '{object_id}', dropping.");
                    return;
                }
            }

            var target = evt.Target;
            if (arenaObjs.TryGetValue(target, out GameObject gobj))
            {
                // pass event on to click-listener is defined
                ArenaClickListener acl = gobj.GetComponent<ArenaClickListener>();
                if (acl != null && acl.OnEventCallback != null) acl.OnEventCallback(msg_type, JsonConvert.SerializeObject(msg));
            }
        }

        /// <summary>
        /// Object changes are published using a ClientId + ObjectId topic, a user must have permissions for the entire scene graph.
        /// </summary>
        /// <param name="object_id">The object id.</param>
        /// <param name="msgJson">The wire-format JSON string paylod for the MQTT message.</param>
        /// <param name="toUserId">The user id to send this message to, if private (optional).</param>
        public void PublishObject(string object_id, string msgJson, string toUserId = null)
        {
            ArenaMessageJson msg = JsonConvert.DeserializeObject<ArenaMessageJson>(msgJson);
            msg.timestamp = GetTimestamp();
            var objTopic = new ArenaTopics(
                realm: sceneTopic.REALM,
                name_space: sceneTopic.nameSpace,
                scenename: sceneTopic.sceneName,
                userclient: userclient,
                objectid: object_id,
                touid: toUserId
            );
            if (toUserId == null)
                PublishSceneMessage(objTopic.PUB_SCENE_OBJECTS, msg);
            else
                PublishSceneMessage(objTopic.PUB_SCENE_OBJECTS_PRIVATE, msg);
        }

        /// <summary>
        /// Camera presence changes are published using a ObjectId-only topic, a user might only have permissions for their camid.
        /// </summary>
        /// <param name="object_id">The user id.</param>
        /// <param name="msgJson">The wire-format JSON string paylod for the MQTT message.</param>
        /// <param name="toUserId">The user id to send this message to, if private (optional).</param>
        public void PublishCamera(string object_id, string msgJson, string toUserId = null)
        {
            ArenaMessageJson msg = JsonConvert.DeserializeObject<ArenaMessageJson>(msgJson);
            msg.timestamp = GetTimestamp();
            var camTopic = new ArenaTopics(
                realm: sceneTopic.REALM,
                name_space: sceneTopic.nameSpace,
                scenename: sceneTopic.sceneName,
                userclient: userclient,
                userobj: object_id,
                touid: toUserId
            );
            if (toUserId == null)
                PublishSceneMessage(camTopic.PUB_SCENE_USER, msg);
            else
                PublishSceneMessage(camTopic.PUB_SCENE_USER_PRIVATE, msg);
        }

        /// <summary>
        /// Hand controller presence changes are published using a ObjectId-only topic, a user might only have permissions for their handid.
        /// </summary>
        /// <param name="object_id">The hand object id (handleftid or handrightid).</param>
        /// <param name="msgJson">The wire-format JSON string payload for the MQTT message.</param>
        /// <param name="toUserId">The user id to send this message to, if private (optional).</param>
        public void PublishHand(string object_id, string msgJson, string toUserId = null)
        {
            ArenaMessageJson msg = JsonConvert.DeserializeObject<ArenaMessageJson>(msgJson);
            msg.timestamp = GetTimestamp();
            var handTopic = new ArenaTopics(
                realm: sceneTopic.REALM,
                name_space: sceneTopic.nameSpace,
                scenename: sceneTopic.sceneName,
                userclient: userclient,
                userobj: object_id,
                touid: toUserId
            );
            if (toUserId == null)
                PublishSceneMessage(handTopic.PUB_SCENE_USER, msg);
            else
                PublishSceneMessage(handTopic.PUB_SCENE_USER_PRIVATE, msg);
        }

        [Obsolete("PublishEvent message signature has changed. Instead of object_id, include data.target in msgJsonData.")]
        public void PublishEvent(string object_id, string eventType, string source, string msgJsonData, bool hasPermissions = true)
        {
            PublishEvent(eventType, source, msgJsonData);
        }

        /// <summary>
        /// Camera events are published using a ObjectId-only topic, a user might only have permissions for their camid.
        /// </summary>
        /// <param name="eventType"The type of event message, e.g. "mousedown".</param>
        /// <param name="source">The user id the event is from.</param>
        /// <param name="msgJsonData">The wire-format JSON string paylod for the MQTT message, data section only.</param>
        /// <param name="toUserId">The user id to send this message to, if private (optional).</param>
        public void PublishEvent(string eventType, string source, string msgJsonData, string toUserId = null)
        {
            ArenaMessageJson msg = new ArenaMessageJson
            {
                object_id = source,
                action = "clientEvent",
                type = eventType,
                data = JsonConvert.DeserializeObject(msgJsonData),
            };
            msg.timestamp = GetTimestamp();
            var evtTopic = new ArenaTopics(
                realm: sceneTopic.REALM,
                name_space: sceneTopic.nameSpace,
                scenename: sceneTopic.sceneName,
                userclient: userclient,
                userobj: source,
                touid: toUserId
            );
            if (toUserId == null)
                PublishSceneMessage(evtTopic.PUB_SCENE_USER, msg);
            else
                PublishSceneMessage(evtTopic.PUB_SCENE_USER_PRIVATE, msg);
        }

        /// <summary>
        /// Egress point for messages to send to remote graph scenes.
        /// </summary>
        private void PublishSceneMessage(string topic, ArenaMessageJson msg)
        {
            byte[] payload = System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(msg));
            Publish(topic, payload); // remote
        }

        private static string GetTimestamp()
        {   // o Format Specifier 2008-10-31T17:04:32.0000000Z
            return DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture);
        }
    }
}
