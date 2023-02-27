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
    /// A list of available animations can usually be found by inspecting the model file or its documentation. All animations will play by default. To play only a specific set of animations, use wildcards: animation-mixer='clip: run_*'. More properties at <a href='https://github.com/n5ro/aframe-extras/tree/master/src/loaders#animation'>https://github.com/n5ro/aframe-extras/tree/master/src/loaders#animation</a>
    /// </summary>
    [Serializable]
    public class ArenaAnimationMixerJson
    {
        public const string componentName = "animation-mixer";

        // animation-mixer member-fields

        private static bool defClampWhenFinished = false;
        [JsonProperty(PropertyName = "clampWhenFinished")]
        [Tooltip("If true, halts the animation at the last frame.")]
        public bool ClampWhenFinished = defClampWhenFinished;
        public bool ShouldSerializeClampWhenFinished()
        {
            if (_token != null && _token.SelectToken("clampWhenFinished") != null) return true;
            return (ClampWhenFinished != defClampWhenFinished);
        }

        private static string defClip = "*";
        [JsonProperty(PropertyName = "clip")]
        [Tooltip("Name of the animation clip(s) to play. Accepts wildcards.")]
        public string Clip = defClip;
        public bool ShouldSerializeClip()
        {
            return true; // required in json schema
        }

        private static float defCrossFadeDuration = 0f;
        [JsonProperty(PropertyName = "crossFadeDuration")]
        [Tooltip("Duration of cross-fades between clips, in seconds.")]
        public float CrossFadeDuration = defCrossFadeDuration;
        public bool ShouldSerializeCrossFadeDuration()
        {
            if (_token != null && _token.SelectToken("crossFadeDuration") != null) return true;
            return (CrossFadeDuration != defCrossFadeDuration);
        }

        private static float defDuration = 0f;
        [JsonProperty(PropertyName = "duration")]
        [Tooltip("Duration of the animation, in seconds (0 = auto).")]
        public float Duration = defDuration;
        public bool ShouldSerializeDuration()
        {
            if (_token != null && _token.SelectToken("duration") != null) return true;
            return (Duration != defDuration);
        }

        public enum LoopType
        {
            [EnumMember(Value = "once")]
            Once,
            [EnumMember(Value = "repeat")]
            Repeat,
            [EnumMember(Value = "pingpong")]
            Pingpong,
        }
        private static LoopType defLoop = LoopType.Repeat;
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(PropertyName = "loop")]
        [Tooltip("once, repeat, or pingpong. In repeat and pingpong modes, the clip plays once plus the specified number of repetitions. For pingpong, every second clip plays in reverse.")]
        public LoopType Loop = defLoop;
        public bool ShouldSerializeLoop()
        {
            if (_token != null && _token.SelectToken("loop") != null) return true;
            return (Loop != defLoop);
        }

        private static string defRepetitions = "";
        [JsonProperty(PropertyName = "repetitions")]
        [Tooltip("Number of times to play the clip, in addition to the first play (empty string = Infinity). Repetitions are ignored for loop: once.")]
        public string Repetitions = defRepetitions;
        public bool ShouldSerializeRepetitions()
        {
            if (_token != null && _token.SelectToken("repetitions") != null) return true;
            return (Repetitions != defRepetitions);
        }

        private static float defStartAt = 0f;
        [JsonProperty(PropertyName = "startAt")]
        [Tooltip("Sets the start of an animation to a specific time (in milliseconds). This is useful when you need to jump to an exact time in an animation. The input parameter will be scaled by the mixer's timeScale.")]
        public float StartAt = defStartAt;
        public bool ShouldSerializeStartAt()
        {
            if (_token != null && _token.SelectToken("startAt") != null) return true;
            return (StartAt != defStartAt);
        }

        private static float defTimeScale = 1f;
        [JsonProperty(PropertyName = "timeScale")]
        [Tooltip("Scaling factor for playback speed. A value of 0 causes the animation to pause. Negative values cause the animation to play backwards.")]
        public float TimeScale = defTimeScale;
        public bool ShouldSerializeTimeScale()
        {
            if (_token != null && _token.SelectToken("timeScale") != null) return true;
            return (TimeScale != defTimeScale);
        }

        // General json object management

        [JsonExtensionData]
        private IDictionary<string, JToken> _additionalData;

        private static JToken _token;

        public string SaveToString()
        {
            return Regex.Unescape(JsonConvert.SerializeObject(this));
        }

        public static ArenaAnimationMixerJson CreateFromJSON(string jsonString, JToken token)
        {
            _token = token; // save updated wire json
            ArenaAnimationMixerJson json = null;
            try {
                json = JsonConvert.DeserializeObject<ArenaAnimationMixerJson>(Regex.Unescape(jsonString));
            } catch (JsonReaderException e)
            {
                Debug.LogWarning($"{e.Message}: {jsonString}");
            }
            return json;
        }
    }
}