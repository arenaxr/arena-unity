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

        // ArenaAnimationMixerJson Member-fields

        private const bool defClampWhenFinished = false;
        [JsonProperty(PropertyName = "clampWhenFinished")]
        [Tooltip("If true, halts the animation at the last frame.")]
        public bool ClampWhenFinished = defClampWhenFinished;
        public bool ShouldSerializeClampWhenFinished()
        {
            if (_token != null && _token.SelectToken("clampWhenFinished") != null) return true;
            return (ClampWhenFinished != defClampWhenFinished);
        }

        private const string defClip = "*";
        [JsonProperty(PropertyName = "clip")]
        [Tooltip("Name of the animation clip(s) to play. Accepts wildcards.")]
        public string Clip = defClip;
        public bool ShouldSerializeClip()
        {
            if (_token != null && _token.SelectToken("clip") != null) return true;
            return (Clip != defClip);
        }

        private const float defCrossFadeDuration = 0;
        [JsonProperty(PropertyName = "crossFadeDuration")]
        [Tooltip("Duration of cross-fades between clips, in seconds.")]
        public float CrossFadeDuration = defCrossFadeDuration;
        public bool ShouldSerializeCrossFadeDuration()
        {
            if (_token != null && _token.SelectToken("crossFadeDuration") != null) return true;
            return (CrossFadeDuration != defCrossFadeDuration);
        }

        private const float defDuration = 0;
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
            once,
            [EnumMember(Value = "repeat")]
            repeat,
            [EnumMember(Value = "pingpong")]
            pingpong,
        }
        private const LoopType defLoop = LoopType.repeat;
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(PropertyName = "loop")]
        [Tooltip("once, repeat, or pingpong. In repeat and pingpong modes, the clip plays once plus the specified number of repetitions. For pingpong, every second clip plays in reverse.")]
        public LoopType Loop = defLoop;
        public bool ShouldSerializeLoop()
        {
            if (_token != null && _token.SelectToken("loop") != null) return true;
            return (Loop != defLoop);
        }

        private const string defRepetitions = "";
        [JsonProperty(PropertyName = "repetitions")]
        [Tooltip("Number of times to play the clip, in addition to the first play (empty string = Infinity). Repetitions are ignored for loop: once.")]
        public string Repetitions = defRepetitions;
        public bool ShouldSerializeRepetitions()
        {
            if (_token != null && _token.SelectToken("repetitions") != null) return true;
            return (Repetitions != defRepetitions);
        }

        private const float defStartAt = 0;
        [JsonProperty(PropertyName = "startAt")]
        [Tooltip("Sets the start of an animation to a specific time (in milliseconds). This is useful when you need to jump to an exact time in an animation. The input parameter will be scaled by the mixer's timeScale.")]
        public float StartAt = defStartAt;
        public bool ShouldSerializeStartAt()
        {
            if (_token != null && _token.SelectToken("startAt") != null) return true;
            return (StartAt != defStartAt);
        }

        private const float defTimeScale = 1;
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
            return JsonConvert.DeserializeObject<ArenaAnimationMixerJson>(Regex.Unescape(jsonString));
        }
    }
}
