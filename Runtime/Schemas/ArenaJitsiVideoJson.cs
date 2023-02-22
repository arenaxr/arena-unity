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
    /// Apply a jitsi video source to the geometry
    /// </summary>
    [Serializable]
    public class ArenaJitsiVideoJson
    {
        public const string componentName = "jitsi-video";

        // jitsi-video member-fields

        private static string defJitsiId = "";
        [JsonProperty(PropertyName = "jitsiId")]
        [Tooltip("JitsiId of the video source; If defined will override displayName")]
        public string JitsiId = defJitsiId;
        public bool ShouldSerializeJitsiId()
        {
            if (_token != null && _token.SelectToken("jitsiId") != null) return true;
            return (JitsiId != defJitsiId);
        }

        private static string defDisplayName = "";
        [JsonProperty(PropertyName = "displayName")]
        [Tooltip("ARENA or Jitsi display name of the video source; Will be ignored if jitsiId is given. (* change requires reload* ) ")]
        public string DisplayName = defDisplayName;
        public bool ShouldSerializeDisplayName()
        {
            return true; // required in json schema 
        }

        // General json object management

        [JsonExtensionData]
        private IDictionary<string, JToken> _additionalData;

        private static JToken _token;

        public string SaveToString()
        {
            return Regex.Unescape(JsonConvert.SerializeObject(this));
        }

        public static ArenaJitsiVideoJson CreateFromJSON(string jsonString, JToken token)
        {
            _token = token; // save updated wire json
            return JsonConvert.DeserializeObject<ArenaJitsiVideoJson>(Regex.Unescape(jsonString));
        }
    }
}
