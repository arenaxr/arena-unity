/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2023, Carnegie Mellon University. All rights reserved.
 */

using System;
using System.Runtime.Serialization;
using ArenaUnity.Components;
using ArenaUnity.Schemas;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using UnityEngine;

namespace ArenaUnity
{
    [Serializable]
    public class ArenaObjectDataJson
    {
        [JsonProperty(PropertyName = "object_type")]
        [Tooltip("3D object type.")]
        public string object_type = null;
        public bool ShouldSerializeobject_type()
        {
            return (object_type != null);
        }

        [JsonProperty(PropertyName = "parent")]
        [Tooltip("Parent's object_id. Child objects inherit attributes of their parent, for example scale and translation.")]
        public string parent = null;
        public bool ShouldSerializeparent()
        {
            return (parent != null);
        }

        [JsonProperty(PropertyName = "position")]
        [Tooltip("3D object position")]
        public ArenaVector3Json position = null;
        public bool ShouldSerializeposition()
        {
            return (position != null);
        }

        [JsonProperty(PropertyName = "rotation")]
        [Tooltip("3D object rotation in quaternion representation; Right-handed coordinate system. Euler degrees are deprecated in wire message format.")]
        public ArenaRotationJson rotation = null;
        public bool ShouldSerializerotation()
        {
            return (rotation != null);
        }

        [JsonProperty(PropertyName = "scale")]
        [Tooltip("3D object scale")]
        public ArenaVector3Json scale = null;
        public bool ShouldSerializescale()
        {
            return (scale != null);
        }

        [JsonProperty(PropertyName = "animation")]
        [Tooltip("Animate and tween values. ")]
        public ArenaAnimationJson animation = null;
        public bool ShouldSerializeanimation()
        {
            return (animation != null);
        }

        [JsonProperty(PropertyName = "armarker")]
        [Tooltip("A location marker (such as an AprilTag, a lightAnchor, or an UWB tag), used to anchor scenes, or scene objects, in the real world.")]
        public ArenaArmarkerJson armarker = null;
        public bool ShouldSerializearmarker()
        {
            return (armarker != null);
        }

        [JsonProperty(PropertyName = "click-listener")]
        [Tooltip("Object will listen for clicks")]
        public ArenaClickListenerJson clickListener = null;
        public bool ShouldSerializeclickListener()
        {
            return (clickListener != null);
        }

        [JsonProperty(PropertyName = "box-collision-listener")]
        [Tooltip("Listen for bounding-box collisions with user camera and hands. Must be applied to an object or model with geometric mesh. Collisions are determined by course bounding-box overlaps")]
        public ArenaBoxCollisionListenerJson boxCollisionListener = null;
        public bool ShouldSerializeboxCollisionListener()
        {
            return (boxCollisionListener != null);
        }

        [JsonProperty(PropertyName = "collision-listener")]
        [Tooltip("Name of the collision-listener, default can be empty string. Collisions trigger click events")]
        public string collisionListener = null;
        public bool ShouldSerializecollisionListener()
        {
            return (collisionListener != null);
        }

        [JsonProperty(PropertyName = "blip")]
        [Tooltip("When the object is created or deleted, it will animate in/out of the scene instead of appearing/disappearing instantly. Must have a geometric mesh.")]
        public ArenaBlipJson blip = null;
        public bool ShouldSerializeblip()
        {
            return (blip != null);
        }

        [JsonProperty(PropertyName = "dynamic-body")]
        [Tooltip("Physics type attached to the object. ")]
        public ArenaDynamicBodyJson dynamicBody = null;
        public bool ShouldSerializedynamicBody()
        {
            return (dynamicBody != null);
        }

        [JsonProperty(PropertyName = "goto-landmark")]
        [Tooltip("Teleports user to the landmark with the given name; Requires click-listener")]
        public ArenaGotoLandmarkJson gotoLandmark = null;
        public bool ShouldSerializegotoLandmark()
        {
            return (gotoLandmark != null);
        }

        [JsonProperty(PropertyName = "goto-url")]
        [Tooltip("Goto given URL; Requires click-listener")]
        public ArenaGotoUrlJson gotoUrl = null;
        public bool ShouldSerializegotoUrl()
        {
            return (gotoUrl != null);
        }

        [JsonProperty(PropertyName = "hide-on-enter-ar")]
        [Tooltip("Hide object when entering AR. Remove component to *not* hide")]
        public bool? hideOnEnterAr = null;
        public bool ShouldSerializehideOnEnterAr()
        {
            return (hideOnEnterAr != null);
        }

        [JsonProperty(PropertyName = "hide-on-enter-vr")]
        [Tooltip("Hide object when entering VR. Remove component to *not* hide")]
        public bool? hideOnEnterVr = null;
        public bool ShouldSerializehideOnEnterVr()
        {
            return (hideOnEnterVr != null);
        }

        [JsonProperty(PropertyName = "show-on-enter-ar")]
        [Tooltip("Show object when entering AR. Hidden otherwise")]
        public bool? showOnEnterAr = null;
        public bool ShouldSerializeshowOnEnterAr()
        {
            return (showOnEnterAr != null);
        }

        [JsonProperty(PropertyName = "show-on-enter-vr")]
        [Tooltip("Show object when entering VR. Hidden otherwise")]
        public bool? showOnEnterVr = null;
        public bool ShouldSerializeshowOnEnterVr()
        {
            return (showOnEnterVr != null);
        }

        [JsonProperty(PropertyName = "impulse")]
        [Tooltip("The force applied using physics. Requires click-listener")]
        public ArenaImpulseJson impulse = null;
        public bool ShouldSerializeimpulse()
        {
            return (impulse != null);
        }

        [JsonProperty(PropertyName = "landmark")]
        [Tooltip("Define entities as a landmark; Landmarks appears in the landmark list and you can move (teleport) to them; You can define the behavior of the teleport: if you will be at a fixed or random distance, looking at the landmark, fixed offset or if it is constrained by a navmesh (when it exists)")]
        public ArenaLandmarkJson landmark = null;
        public bool ShouldSerializelandmark()
        {
            return (landmark != null);
        }

        [JsonProperty(PropertyName = "material-extras")]
        [Tooltip("Define extra material properties, namely texture encoding, whether to render the material's color and render order. The properties set here access directly Three.js material component. ")]
        public ArenaMaterialExtrasJson materialExtras = null;
        public bool ShouldSerializematerialExtras()
        {
            return (materialExtras != null);
        }

        [JsonProperty(PropertyName = "shadow")]
        [Tooltip("shadow")]
        public ArenaShadowJson shadow = null;
        public bool ShouldSerializeshadow()
        {
            return (shadow != null);
        }

        [JsonProperty(PropertyName = "sound")]
        [Tooltip("The sound component defines the entity as a source of sound or audio. The sound component is positional and is thus affected by the component's position. ")]
        public ArenaSoundJson sound = null;
        public bool ShouldSerializesound()
        {
            return (sound != null);
        }

        [JsonProperty(PropertyName = "textinput")]
        [Tooltip("Opens an HTML prompt when clicked. Sends text input as an event on MQTT. Requires click-listener.")]
        public ArenaTextinputJson textinput = null;
        public bool ShouldSerializetextinput()
        {
            return (textinput != null);
        }

        [JsonProperty(PropertyName = "url")]
        [Tooltip("Model URL. Store files paths under 'store/users/<username>' (e.g. store/users/wiselab/models/factory_robot_arm/scene.gltf); to use CDN, prefix with `https://arena-cdn.conix.io/` (e.g. https://arena-cdn.conix.io/store/users/wiselab/models/factory_robot_arm/scene.gltf)")]
        public string url = null;
        public bool ShouldSerializeurl()
        {
            return (url != null);
        }

        [JsonProperty(PropertyName = "screenshareable")]
        [Tooltip("Whether or not a user can screenshare on an object")]
        public bool? screenshareable = null;
        public bool ShouldSerializescreenshareable()
        {
            return (screenshareable != null);
        }

        [JsonProperty(PropertyName = "remote-render")]
        [Tooltip("Whether or not an object should be remote rendered [Experimental]")]
        public ArenaRemoteRenderJson remoteRender = null;
        public bool ShouldSerializeremoteRender()
        {
            return (remoteRender != null);
        }

        [JsonProperty(PropertyName = "video-control")]
        [Tooltip("Video Control")]
        public ArenaVideoControlJson videoControl = null;
        public bool ShouldSerializevideoControl()
        {
            return (videoControl != null);
        }

        [JsonProperty(PropertyName = "attribution")]
        [Tooltip("Attribution Component. Saves attribution data in any entity.")]
        public ArenaAttributionJson attribution = null;
        public bool ShouldSerializeattribution()
        {
            return (attribution != null);
        }

        [JsonProperty(PropertyName = "particle-system")]
        [Tooltip("Particle system component for A-Frame. ")]
        public ArenaParticleSystemJson particleSystem = null;
        public bool ShouldSerializeparticleSystem()
        {
            return (particleSystem != null);
        }

        [JsonProperty(PropertyName = "spe-particles")]
        [Tooltip("GPU based particle systems in A-Frame. ")]
        public ArenaSpeParticlesJson speParticles = null;
        public bool ShouldSerializespeParticles()
        {
            return (speParticles != null);
        }

        [JsonProperty(PropertyName = "buffer")]
        [Tooltip("Transform geometry into a BufferGeometry to reduce memory usage at the cost of being harder to manipulate (geometries only: box, circle, cone, ...).")]
        public bool? buffer = null;
        public bool ShouldSerializebuffer()
        {
            return (buffer != null);
        }

        [JsonProperty(PropertyName = "jitsi-video")]
        [Tooltip("Apply a jitsi video source to the geometry")]
        public ArenaJitsiVideoJson jitsiVideo = null;
        public bool ShouldSerializejitsiVideo()
        {
            return (jitsiVideo != null);
        }

        [JsonProperty(PropertyName = "material")]
        [Tooltip("The material properties of the object’s surface. ")]
        public ArenaMaterialJson material = null;
        public bool ShouldSerializematerial()
        {
            return (material != null);
        }

        [JsonProperty(PropertyName = "multisrc")]
        [Tooltip("Define multiple visual sources applied to an object.")]
        public ArenaMultisrcJson multisrc = null;
        public bool ShouldSerializemultisrc()
        {
            return (multisrc != null);
        }

        [JsonProperty(PropertyName = "skipCache")]
        [Tooltip("Disable retrieving the shared geometry object from the cache. (geometries only: box, circle, cone, ...).")]
        public bool? skipCache = null;
        public bool ShouldSerializeskipCache()
        {
            return (skipCache != null);
        }

        [JsonProperty(PropertyName = "visible")]
        [Tooltip("Whether to render the object.")]
        public bool? visible = null;
        public bool ShouldSerializevisible()
        {
            return (visible != null);
        }

        [JsonProperty(PropertyName = "animation-mixer")]
        [Tooltip("A list of available animations can usually be found by inspecting the model file or its documentation. All animations will play by default. To play only a specific set of animations, use wildcards: animation-mixer='clip: run_*'. \n\nMore properties at <a href='https://github.com/n5ro/aframe-extras/tree/master/src/loaders#animation'>https://github.com/n5ro/aframe-extras/tree/master/src/loaders#animation</a>")]
        public ArenaAnimationMixerJson animationMixer = null;
        public bool ShouldSerializeanimationMixer()
        {
            return (animationMixer != null);
        }


        [JsonProperty(PropertyName = "gltf-model-lod")]
        [Tooltip("Simple switch between the default gltf-model and a detailed one when a user camera is within specified distance")]
        public ArenaGltfModelLodJson gltfModelLod = null;
        public bool ShouldSerializegltfModelLod()
        {
            return (gltfModelLod != null);
        }


        [JsonProperty(PropertyName = "modelUpdate")]
        [Tooltip("The GLTF-specific `modelUpdate` attribute is an object with child component names as keys. The top-level keys are the names of the child components to be updated. The values of each are nested `position` and `rotation` attributes to set as new values, respectively. Either `position` or `rotation` can be omitted if unchanged.")]
        public ArenaModelUpdateJson modelUpdate = null;
        public bool ShouldSerializemodelUpdate()
        {
            return (modelUpdate != null);
        }

        [OnError]
        internal void OnError(StreamingContext context, ErrorContext errorContext)
        {
            Debug.LogWarning($"{errorContext.Error.Message}: {errorContext.OriginalObject}");
            errorContext.Handled = true;
        }
    }
}
