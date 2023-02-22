/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2023, Carnegie Mellon University. All rights reserved.
 */

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
    /// The sound component defines the entity as a source of sound or audio. The sound component is positional and is thus affected by the component's position. More properties at <a href='https://aframe.io/docs/1.3.0/components/sound.html'>https://aframe.io/docs/1.3.0/components/sound.html</a>
    /// </summary>
    [Serializable]
    public class ArenaSoundJson
    {
        public const string componentName = "sound";

        // sound member-fields

        private static bool defAutoplay = false;
        [JsonProperty(PropertyName = "autoplay")]
        [Tooltip("Whether to automatically play sound once set.")]
        public bool Autoplay = defAutoplay;
        public bool ShouldSerializeAutoplay()
        {
            if (_token != null && _token.SelectToken("autoplay") != null) return true;
            return (Autoplay != defAutoplay);
        }

        public enum DistanceModelType
        {
            [EnumMember(Value = "linear")]
            Linear,
            [EnumMember(Value = "inverse")]
            Inverse,
            [EnumMember(Value = "exponential")]
            Exponential,
        }
        private static DistanceModelType defDistanceModel = DistanceModelType.Inverse;
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(PropertyName = "distanceModel")]
        [Tooltip("Sound model: linear, inverse, or exponential.")]
        public DistanceModelType DistanceModel = defDistanceModel;
        public bool ShouldSerializeDistanceModel()
        {
            if (_token != null && _token.SelectToken("distanceModel") != null) return true;
            return (DistanceModel != defDistanceModel);
        }

        private static bool defLoop = false;
        [JsonProperty(PropertyName = "loop")]
        [Tooltip("Whether to loop the sound once the sound finishes playing.")]
        public bool Loop = defLoop;
        public bool ShouldSerializeLoop()
        {
            if (_token != null && _token.SelectToken("loop") != null) return true;
            return (Loop != defLoop);
        }

        private static float defMaxDistance = 10000f;
        [JsonProperty(PropertyName = "maxDistance")]
        [Tooltip("Maximum distance between the audio source and the listener, after which the volume is not reduced any further.")]
        public float MaxDistance = defMaxDistance;
        public bool ShouldSerializeMaxDistance()
        {
            if (_token != null && _token.SelectToken("maxDistance") != null) return true;
            return (MaxDistance != defMaxDistance);
        }

        public enum OnType
        {
            [EnumMember(Value = "mousedown")]
            Mousedown,
            [EnumMember(Value = "mouseup")]
            Mouseup,
            [EnumMember(Value = "mouseenter")]
            Mouseenter,
            [EnumMember(Value = "mouseleave")]
            Mouseleave,
            [EnumMember(Value = "triggerdown")]
            Triggerdown,
            [EnumMember(Value = "triggerup")]
            Triggerup,
            [EnumMember(Value = "gripdown")]
            Gripdown,
            [EnumMember(Value = "gripup")]
            Gripup,
            [EnumMember(Value = "menudown")]
            Menudown,
            [EnumMember(Value = "menuup")]
            Menuup,
            [EnumMember(Value = "systemdown")]
            Systemdown,
            [EnumMember(Value = "systemup")]
            Systemup,
            [EnumMember(Value = "trackpaddown")]
            Trackpaddown,
            [EnumMember(Value = "trackpadup")]
            Trackpadup,
        }
        private static OnType defOn = OnType.Mousedown;
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(PropertyName = "on")]
        [Tooltip("An event for the entity to listen to before playing sound.")]
        public OnType On = defOn;
        public bool ShouldSerializeOn()
        {
            if (_token != null && _token.SelectToken("on") != null) return true;
            return (On != defOn);
        }

        private static float defPoolSize = 1f;
        [JsonProperty(PropertyName = "poolSize")]
        [Tooltip("Numbers of simultaneous instances of this sound that can be playing at the same time")]
        public float PoolSize = defPoolSize;
        public bool ShouldSerializePoolSize()
        {
            if (_token != null && _token.SelectToken("poolSize") != null) return true;
            return (PoolSize != defPoolSize);
        }

        private static bool defPositional = true;
        [JsonProperty(PropertyName = "positional")]
        [Tooltip("Whether or not the audio is positional (movable).")]
        public bool Positional = defPositional;
        public bool ShouldSerializePositional()
        {
            if (_token != null && _token.SelectToken("positional") != null) return true;
            return (Positional != defPositional);
        }

        private static float defRefDistance = 1f;
        [JsonProperty(PropertyName = "refDistance")]
        [Tooltip("Reference distance for reducing volume as the audio source moves further from the listener.")]
        public float RefDistance = defRefDistance;
        public bool ShouldSerializeRefDistance()
        {
            if (_token != null && _token.SelectToken("refDistance") != null) return true;
            return (RefDistance != defRefDistance);
        }

        private static float defRolloffFactor = 1f;
        [JsonProperty(PropertyName = "rolloffFactor")]
        [Tooltip("Describes how quickly the volume is reduced as the source moves away from the listener.")]
        public float RolloffFactor = defRolloffFactor;
        public bool ShouldSerializeRolloffFactor()
        {
            if (_token != null && _token.SelectToken("rolloffFactor") != null) return true;
            return (RolloffFactor != defRolloffFactor);
        }

        private static string defSrc = "";
        [JsonProperty(PropertyName = "src")]
        [Tooltip("URL path to sound file e.g. 'store/users/wiselab/sound/wave.mp3'")]
        public string Src = defSrc;
        public bool ShouldSerializeSrc()
        {
            if (_token != null && _token.SelectToken("src") != null) return true;
            return (Src != defSrc);
        }

        private static float defVolume = 1f;
        [JsonProperty(PropertyName = "volume")]
        [Tooltip("How loud to play the sound")]
        public float Volume = defVolume;
        public bool ShouldSerializeVolume()
        {
            if (_token != null && _token.SelectToken("volume") != null) return true;
            return (Volume != defVolume);
        }

        // General json object management

        [JsonExtensionData]
        private IDictionary<string, JToken> _additionalData;

        private static JToken _token;

        public string SaveToString()
        {
            return Regex.Unescape(JsonConvert.SerializeObject(this));
        }

        public static ArenaSoundJson CreateFromJSON(string jsonString, JToken token)
        {
            _token = token; // save updated wire json
            return JsonConvert.DeserializeObject<ArenaSoundJson>(Regex.Unescape(jsonString));
        }
    }
}
