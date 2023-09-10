/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2023, Carnegie Mellon University. All rights reserved.
 */

// CAUTION: This file is autogenerated from https://github.com/arenaxr/arena-schemas. Changes made here may be overwritten.

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace ArenaUnity.Schemas
{
    /// <summary>
    /// Draw a line
    /// </summary>
    [Serializable]
    public class ArenaLineJson
    {
        public const string componentName = "line";

        // line member-fields

        private static  defEnd = {'x': -0.5, 'y': -0.5, 'z': 0};
        [JsonProperty(PropertyName = "end")]
        [Tooltip("vertex B (end)")]
        public  End = defEnd;
        public bool ShouldSerializeEnd()
        {
            return true; // required in json schema 
        }

        private static float defOpacity = 1f;
        [JsonProperty(PropertyName = "opacity")]
        [Tooltip("Line Opacity")]
        public float Opacity = defOpacity;
        public bool ShouldSerializeOpacity()
        {
            if (_token != null && _token.SelectToken("opacity") != null) return true;
            return (Opacity != defOpacity);
        }

        private static  defStart = {'x': 0, 'y': 0.5, 'z': 0};
        [JsonProperty(PropertyName = "start")]
        [Tooltip("vertex A (start)")]
        public  Start = defStart;
        public bool ShouldSerializeStart()
        {
            return true; // required in json schema 
        }

        private static bool defVisible = true;
        [JsonProperty(PropertyName = "visible")]
        [Tooltip("Visible")]
        public bool Visible = defVisible;
        public bool ShouldSerializeVisible()
        {
            if (_token != null && _token.SelectToken("visible") != null) return true;
            return (Visible != defVisible);
        }

        private static object defPosition = JsonConvert.DeserializeObject("");
        [JsonProperty(PropertyName = "position")]
        [Tooltip("3D object position")]
        public object Position = defPosition;
        public bool ShouldSerializePosition()
        {
            if (_token != null && _token.SelectToken("position") != null) return true;
            return (Position != defPosition);
        }

        private static object defRotation = JsonConvert.DeserializeObject("");
        [JsonProperty(PropertyName = "rotation")]
        [Tooltip("3D object rotation in quaternion representation; Right-handed coordinate system. Euler degrees are deprecated in wire message format.")]
        public object Rotation = defRotation;
        public bool ShouldSerializeRotation()
        {
            if (_token != null && _token.SelectToken("rotation") != null) return true;
            return (Rotation != defRotation);
        }

        private static object defScale = JsonConvert.DeserializeObject("");
        [JsonProperty(PropertyName = "scale")]
        [Tooltip("3D object scale")]
        public object Scale = defScale;
        public bool ShouldSerializeScale()
        {
            if (_token != null && _token.SelectToken("scale") != null) return true;
            return (Scale != defScale);
        }

        private static object defAnimation = JsonConvert.DeserializeObject("");
        [JsonProperty(PropertyName = "animation")]
        [Tooltip("Animate and tween values. ")]
        public object Animation = defAnimation;
        public bool ShouldSerializeAnimation()
        {
            if (_token != null && _token.SelectToken("animation") != null) return true;
            return (Animation != defAnimation);
        }

        private static object defArmarker = JsonConvert.DeserializeObject("");
        [JsonProperty(PropertyName = "armarker")]
        [Tooltip("A location marker (such as an AprilTag, a lightAnchor, or an UWB tag), used to anchor scenes, or scene objects, in the real world.")]
        public object Armarker = defArmarker;
        public bool ShouldSerializeArmarker()
        {
            if (_token != null && _token.SelectToken("armarker") != null) return true;
            return (Armarker != defArmarker);
        }

        private static object defClickListener = JsonConvert.DeserializeObject("");
        [JsonProperty(PropertyName = "click-listener")]
        [Tooltip("Object will listen for clicks")]
        public object ClickListener = defClickListener;
        public bool ShouldSerializeClickListener()
        {
            if (_token != null && _token.SelectToken("click-listener") != null) return true;
            return (ClickListener != defClickListener);
        }

        private static object defBoxCollisionListener = JsonConvert.DeserializeObject("");
        [JsonProperty(PropertyName = "box-collision-listener")]
        [Tooltip("Listen for bounding-box collisions with user camera and hands. Must be applied to an object or model with geometric mesh. Collisions are determined by course bounding-box overlaps")]
        public object BoxCollisionListener = defBoxCollisionListener;
        public bool ShouldSerializeBoxCollisionListener()
        {
            if (_token != null && _token.SelectToken("box-collision-listener") != null) return true;
            return (BoxCollisionListener != defBoxCollisionListener);
        }

        private static string defCollisionListener = "";
        [JsonProperty(PropertyName = "collision-listener")]
        [Tooltip("Name of the collision-listener, default can be empty string. Collisions trigger click events")]
        public string CollisionListener = defCollisionListener;
        public bool ShouldSerializeCollisionListener()
        {
            if (_token != null && _token.SelectToken("collision-listener") != null) return true;
            return (CollisionListener != defCollisionListener);
        }

        private static object defBlip = JsonConvert.DeserializeObject("");
        [JsonProperty(PropertyName = "blip")]
        [Tooltip("When the object is created or deleted, it will animate in/out of the scene instead of appearing/disappearing instantly. Must have a geometric mesh.")]
        public object Blip = defBlip;
        public bool ShouldSerializeBlip()
        {
            if (_token != null && _token.SelectToken("blip") != null) return true;
            return (Blip != defBlip);
        }

        private static object defDynamicBody = JsonConvert.DeserializeObject("");
        [JsonProperty(PropertyName = "dynamic-body")]
        [Tooltip("Physics type attached to the object. ")]
        public object DynamicBody = defDynamicBody;
        public bool ShouldSerializeDynamicBody()
        {
            if (_token != null && _token.SelectToken("dynamic-body") != null) return true;
            return (DynamicBody != defDynamicBody);
        }

        private static object defGotoLandmark = JsonConvert.DeserializeObject("");
        [JsonProperty(PropertyName = "goto-landmark")]
        [Tooltip("Teleports user to the landmark with the given name; Requires click-listener")]
        public object GotoLandmark = defGotoLandmark;
        public bool ShouldSerializeGotoLandmark()
        {
            if (_token != null && _token.SelectToken("goto-landmark") != null) return true;
            return (GotoLandmark != defGotoLandmark);
        }

        private static object defGotoUrl = JsonConvert.DeserializeObject("");
        [JsonProperty(PropertyName = "goto-url")]
        [Tooltip("Goto given URL; Requires click-listener")]
        public object GotoUrl = defGotoUrl;
        public bool ShouldSerializeGotoUrl()
        {
            if (_token != null && _token.SelectToken("goto-url") != null) return true;
            return (GotoUrl != defGotoUrl);
        }

        private static bool defHideOnEnterAr = true;
        [JsonProperty(PropertyName = "hide-on-enter-ar")]
        [Tooltip("Hide object when entering AR. Remove component to *not* hide")]
        public bool HideOnEnterAr = defHideOnEnterAr;
        public bool ShouldSerializeHideOnEnterAr()
        {
            if (_token != null && _token.SelectToken("hide-on-enter-ar") != null) return true;
            return (HideOnEnterAr != defHideOnEnterAr);
        }

        private static bool defHideOnEnterVr = true;
        [JsonProperty(PropertyName = "hide-on-enter-vr")]
        [Tooltip("Hide object when entering VR. Remove component to *not* hide")]
        public bool HideOnEnterVr = defHideOnEnterVr;
        public bool ShouldSerializeHideOnEnterVr()
        {
            if (_token != null && _token.SelectToken("hide-on-enter-vr") != null) return true;
            return (HideOnEnterVr != defHideOnEnterVr);
        }

        private static bool defShowOnEnterAr = true;
        [JsonProperty(PropertyName = "show-on-enter-ar")]
        [Tooltip("Show object when entering AR. Hidden otherwise")]
        public bool ShowOnEnterAr = defShowOnEnterAr;
        public bool ShouldSerializeShowOnEnterAr()
        {
            if (_token != null && _token.SelectToken("show-on-enter-ar") != null) return true;
            return (ShowOnEnterAr != defShowOnEnterAr);
        }

        private static bool defShowOnEnterVr = true;
        [JsonProperty(PropertyName = "show-on-enter-vr")]
        [Tooltip("Show object when entering VR. Hidden otherwise")]
        public bool ShowOnEnterVr = defShowOnEnterVr;
        public bool ShouldSerializeShowOnEnterVr()
        {
            if (_token != null && _token.SelectToken("show-on-enter-vr") != null) return true;
            return (ShowOnEnterVr != defShowOnEnterVr);
        }

        private static object defImpulse = JsonConvert.DeserializeObject("");
        [JsonProperty(PropertyName = "impulse")]
        [Tooltip("The force applied using physics. Requires click-listener")]
        public object Impulse = defImpulse;
        public bool ShouldSerializeImpulse()
        {
            if (_token != null && _token.SelectToken("impulse") != null) return true;
            return (Impulse != defImpulse);
        }

        private static object defLandmark = JsonConvert.DeserializeObject("");
        [JsonProperty(PropertyName = "landmark")]
        [Tooltip("Define entities as a landmark; Landmarks appears in the landmark list and you can move (teleport) to them; You can define the behavior of the teleport: if you will be at a fixed or random distance, looking at the landmark, fixed offset or if it is constrained by a navmesh (when it exists)")]
        public object Landmark = defLandmark;
        public bool ShouldSerializeLandmark()
        {
            if (_token != null && _token.SelectToken("landmark") != null) return true;
            return (Landmark != defLandmark);
        }

        private static object defMaterialExtras = JsonConvert.DeserializeObject("");
        [JsonProperty(PropertyName = "material-extras")]
        [Tooltip("Define extra material properties, namely texture encoding, whether to render the material's color and render order. The properties set here access directly Three.js material component. ")]
        public object MaterialExtras = defMaterialExtras;
        public bool ShouldSerializeMaterialExtras()
        {
            if (_token != null && _token.SelectToken("material-extras") != null) return true;
            return (MaterialExtras != defMaterialExtras);
        }

        private static object defShadow = JsonConvert.DeserializeObject("");
        [JsonProperty(PropertyName = "shadow")]
        [Tooltip("shadow")]
        public object Shadow = defShadow;
        public bool ShouldSerializeShadow()
        {
            if (_token != null && _token.SelectToken("shadow") != null) return true;
            return (Shadow != defShadow);
        }

        private static object defSound = JsonConvert.DeserializeObject("");
        [JsonProperty(PropertyName = "sound")]
        [Tooltip("The sound component defines the entity as a source of sound or audio. The sound component is positional and is thus affected by the component's position. ")]
        public object Sound = defSound;
        public bool ShouldSerializeSound()
        {
            if (_token != null && _token.SelectToken("sound") != null) return true;
            return (Sound != defSound);
        }

        private static object defTextinput = JsonConvert.DeserializeObject("");
        [JsonProperty(PropertyName = "textinput")]
        [Tooltip("Opens an HTML prompt when clicked. Sends text input as an event on MQTT. Requires click-listener.")]
        public object Textinput = defTextinput;
        public bool ShouldSerializeTextinput()
        {
            if (_token != null && _token.SelectToken("textinput") != null) return true;
            return (Textinput != defTextinput);
        }

        private static string defUrl = "";
        [JsonProperty(PropertyName = "url")]
        [Tooltip("Model URL. Store files paths under 'store/users/<username>' (e.g. store/users/wiselab/models/factory_robot_arm/scene.gltf); to use CDN, prefix with `https://arena-cdn.conix.io/` (e.g. https://arena-cdn.conix.io/store/users/wiselab/models/factory_robot_arm/scene.gltf)")]
        public string Url = defUrl;
        public bool ShouldSerializeUrl()
        {
            if (_token != null && _token.SelectToken("url") != null) return true;
            return (Url != defUrl);
        }

        private static bool defScreenshareable = true;
        [JsonProperty(PropertyName = "screenshareable")]
        [Tooltip("Whether or not a user can screenshare on an object")]
        public bool Screenshareable = defScreenshareable;
        public bool ShouldSerializeScreenshareable()
        {
            if (_token != null && _token.SelectToken("screenshareable") != null) return true;
            return (Screenshareable != defScreenshareable);
        }

        private static object defRemoteRender = JsonConvert.DeserializeObject("");
        [JsonProperty(PropertyName = "remote-render")]
        [Tooltip("Whether or not an object should be remote rendered [Experimental]")]
        public object RemoteRender = defRemoteRender;
        public bool ShouldSerializeRemoteRender()
        {
            if (_token != null && _token.SelectToken("remote-render") != null) return true;
            return (RemoteRender != defRemoteRender);
        }

        private static object defVideoControl = JsonConvert.DeserializeObject("");
        [JsonProperty(PropertyName = "video-control")]
        [Tooltip("Video Control")]
        public object VideoControl = defVideoControl;
        public bool ShouldSerializeVideoControl()
        {
            if (_token != null && _token.SelectToken("video-control") != null) return true;
            return (VideoControl != defVideoControl);
        }

        private static object defAttribution = JsonConvert.DeserializeObject("");
        [JsonProperty(PropertyName = "attribution")]
        [Tooltip("Attribution Component. Saves attribution data in any entity.")]
        public object Attribution = defAttribution;
        public bool ShouldSerializeAttribution()
        {
            if (_token != null && _token.SelectToken("attribution") != null) return true;
            return (Attribution != defAttribution);
        }

        private static object defParticleSystem = JsonConvert.DeserializeObject("");
        [JsonProperty(PropertyName = "particle-system")]
        [Tooltip("Particle system component for A-Frame. ")]
        public object ParticleSystem = defParticleSystem;
        public bool ShouldSerializeParticleSystem()
        {
            if (_token != null && _token.SelectToken("particle-system") != null) return true;
            return (ParticleSystem != defParticleSystem);
        }

        private static object defSpeParticles = JsonConvert.DeserializeObject("");
        [JsonProperty(PropertyName = "spe-particles")]
        [Tooltip("GPU based particle systems in A-Frame. ")]
        public object SpeParticles = defSpeParticles;
        public bool ShouldSerializeSpeParticles()
        {
            if (_token != null && _token.SelectToken("spe-particles") != null) return true;
            return (SpeParticles != defSpeParticles);
        }

        private static bool defBuffer = true;
        [JsonProperty(PropertyName = "buffer")]
        [Tooltip("Transform geometry into a BufferGeometry to reduce memory usage at the cost of being harder to manipulate (geometries only: box, circle, cone, ...).")]
        public bool Buffer = defBuffer;
        public bool ShouldSerializeBuffer()
        {
            if (_token != null && _token.SelectToken("buffer") != null) return true;
            return (Buffer != defBuffer);
        }

        private static object defJitsiVideo = JsonConvert.DeserializeObject("");
        [JsonProperty(PropertyName = "jitsi-video")]
        [Tooltip("Apply a jitsi video source to the geometry")]
        public object JitsiVideo = defJitsiVideo;
        public bool ShouldSerializeJitsiVideo()
        {
            if (_token != null && _token.SelectToken("jitsi-video") != null) return true;
            return (JitsiVideo != defJitsiVideo);
        }

        private static object defMaterial = JsonConvert.DeserializeObject("");
        [JsonProperty(PropertyName = "material")]
        [Tooltip("The material properties of the object’s surface. ")]
        public object Material = defMaterial;
        public bool ShouldSerializeMaterial()
        {
            if (_token != null && _token.SelectToken("material") != null) return true;
            return (Material != defMaterial);
        }

        private static object defMultisrc = JsonConvert.DeserializeObject("");
        [JsonProperty(PropertyName = "multisrc")]
        [Tooltip("Define multiple visual sources applied to an object.")]
        public object Multisrc = defMultisrc;
        public bool ShouldSerializeMultisrc()
        {
            if (_token != null && _token.SelectToken("multisrc") != null) return true;
            return (Multisrc != defMultisrc);
        }

        private static bool defSkipCache = true;
        [JsonProperty(PropertyName = "skipCache")]
        [Tooltip("Disable retrieving the shared geometry object from the cache. (geometries only: box, circle, cone, ...).")]
        public bool SkipCache = defSkipCache;
        public bool ShouldSerializeSkipCache()
        {
            if (_token != null && _token.SelectToken("skipCache") != null) return true;
            return (SkipCache != defSkipCache);
        }

        // General json object management

        [JsonExtensionData]
        private IDictionary<string, JToken> _additionalData;

        private static JToken _token;

        public string SaveToString()
        {
            return Regex.Unescape(JsonConvert.SerializeObject(this));
        }

        public static ArenaLineJson CreateFromJSON(string jsonString, JToken token)
        {
            _token = token; // save updated wire json
            ArenaLineJson json = null;
            try {
                json = JsonConvert.DeserializeObject<ArenaLineJson>(Regex.Unescape(jsonString));
            } catch (JsonReaderException e)
            {
                Debug.LogWarning($"{e.Message}: {jsonString}");
            }
            return json;
        }
    }
}
