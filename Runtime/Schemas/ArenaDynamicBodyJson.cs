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
    /// Physics type attached to the object. More properties at <a href='https://github.com/n5ro/aframe-physics-system#dynamic-body-and-static-body'>https://github.com/n5ro/aframe-physics-system#dynamic-body-and-static-body</a>
    /// </summary>
    [Serializable]
    public class ArenaDynamicBodyJson
    {
        public const string componentName = "dynamic-body";

        // dynamic-body member-fields

        public enum TypeType
        {
            [EnumMember(Value = "static")]
            Static,
            [EnumMember(Value = "dynamic")]
            Dynamic,
        }
        private static TypeType defType = TypeType.Static;
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(PropertyName = "type")]
        [Tooltip("type")]
        public TypeType Type = defType;
        public bool ShouldSerializeType()
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

        public static ArenaDynamicBodyJson CreateFromJSON(string jsonString, JToken token)
        {
            _token = token; // save updated wire json
            return JsonConvert.DeserializeObject<ArenaDynamicBodyJson>(Regex.Unescape(jsonString));
        }
    }
}
