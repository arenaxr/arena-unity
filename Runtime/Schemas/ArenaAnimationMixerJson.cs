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
    /// A list of available animations can usually be found by inspecting the model file or its documentation. All animations will play by default. To play only a specific set of animations, use wildcards: animation-mixer='clip: run_*'. \n\nMore properties at <a href='https://github.com/n5ro/aframe-extras/tree/master/src/loaders#animation'>https://github.com/n5ro/aframe-extras/tree/master/src/loaders#animation</a>",
    /// </summary>
    [Serializable]
    public class ArenaAnimationMixerJson
    {
        public const string componentName = "animation-mixer";
        public const string defclip = "*";
        public const int defcrossFadeDuration = 0;
        public const LoopType defloop = LoopType.repeat;
        public const string defrepetitions = null;
        public const float deftimeScale = 1;
        public const bool defclampWhenFinished = false;
        public const int defstartAt = 0;

        public enum LoopType
        {
            [EnumMember(Value = "once")]
            once,
            [EnumMember(Value = "repeat")]
            repeat,
            [EnumMember(Value = "pingpong")]
            pingpong,
        }

        // ArenaAnimationMixerJson Member-fields

        [Tooltip("Name of the animation clip(s) to play. Accepts wildcards.")]
        public string clip = defclip;
        public bool ShouldSerializeclip()
        {
            // TODO: operationally the web component appears to require clip to function
            //if (_token != null && _token.SelectToken("clip") != null) return true;
            //return (clip != defclip);
            return true;
        }

        [Tooltip("Duration of cross-fades between clips, in seconds.")]
        public int crossFadeDuration = defcrossFadeDuration;
        public bool ShouldSerializecrossFadeDuration()
        {
            if (_token != null && _token.SelectToken("crossFadeDuration") != null) return true;
            return (crossFadeDuration != defcrossFadeDuration);
        }

        [Tooltip("once, repeat, or pingpong. In repeat and pingpong modes, the clip plays once plus the specified number of repetitions. For pingpong, every second clip plays in reverse.")]
        [JsonConverter(typeof(StringEnumConverter))]
        public LoopType loop = defloop;
        public bool ShouldSerializeloop()
        {
            if (_token != null && _token.SelectToken("loop") != null) return true;
            return (loop != defloop);
        }

        // TODO: empty to serialize as null
        [Tooltip("Number of times to play the clip, in addition to the first play. Repetitions are ignored for loop: once.")]
        public string repetitions = defrepetitions;
        public bool ShouldSerializerepetitions()
        {
            if (_token != null && _token.SelectToken("repetitions") != null) return true;
            return (repetitions != defrepetitions);
        }

        [Tooltip("Scaling factor for playback speed. A value of 0 causes the animation to pause. Negative values cause the animation to play backwards.")]
        public float timeScale = deftimeScale;
        public bool ShouldSerializetimeScale()
        {
            if (_token != null && _token.SelectToken("timeScale") != null) return true;
            return (timeScale != deftimeScale);
        }

        [Tooltip("If true, halts the animation at the last frame.")]
        public bool clampWhenFinished = defclampWhenFinished;
        public bool ShouldSerializeclampWhenFinished()
        {
            if (_token != null && _token.SelectToken("clampWhenFinished") != null) return true;
            return (clampWhenFinished != defclampWhenFinished);
        }

        [Tooltip("Sets the start of an animation to a specific time (in milliseconds). This is useful when you need to jump to an exact time in an animation. The input parameter will be scaled by the mixer's timeScale.")]
        public int startAt = defstartAt;
        public bool ShouldSerializestartAt()
        {
            if (_token != null && _token.SelectToken("startAt") != null) return true;
            return (startAt != defstartAt);
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
