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
    /// Define multiple visual sources applied to an object.
    /// </summary>
    [Serializable]
    public class ArenaMultisrcJson
    {
        public const string componentName = "multisrc";

        // multisrc member-fields

        private static string defSrcs = "";
        [JsonProperty(PropertyName = "srcs")]
        [Tooltip("A comma-delimited list if URIs, e.g. “side1.png, side2.png, side3.png, side4.png, side5.png, side6.png” (required).")]
        public string Srcs = defSrcs;
        public bool ShouldSerializeSrcs()
        {
            return true; // required in json schema 
        }

        private static string defSrcspath = "";
        [JsonProperty(PropertyName = "srcspath")]
        [Tooltip("URI, relative or full path of a directory containing srcs, e.g. “store/users/wiselab/images/dice/” (required).")]
        public string Srcspath = defSrcspath;
        public bool ShouldSerializeSrcspath()
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

        public static ArenaMultisrcJson CreateFromJSON(string jsonString, JToken token)
        {
            _token = token; // save updated wire json
            ArenaMultisrcJson json = null;
            try {
                json = JsonConvert.DeserializeObject<ArenaMultisrcJson>(Regex.Unescape(jsonString));
            } catch (JsonReaderException e)
            {
                Debug.LogWarning($"{e.Message}: {jsonString}");
            }
            return json;
        }
    }
}
