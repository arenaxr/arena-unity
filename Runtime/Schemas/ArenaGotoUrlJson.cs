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
    /// Goto given URL; Requires click-listener
    /// </summary>
    [Serializable]
    public class ArenaGotoUrlJson
    {
        public const string componentName = "goto-url";

        // goto-url member-fields

        public enum DestType
        {
            [EnumMember(Value = "popup")]
            Popup,
            [EnumMember(Value = "newtab")]
            Newtab,
            [EnumMember(Value = "sametab")]
            Sametab,
        }
        private static DestType defDest = DestType.Sametab;
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(PropertyName = "dest")]
        [Tooltip("dest")]
        public DestType Dest = defDest;
        public bool ShouldSerializeDest()
        {
            return true; // required in json schema 
        }

        public enum OnType
        {
            [EnumMember(Value = "mousedown")]
            Mousedown,
            [EnumMember(Value = "mouseup")]
            Mouseup,
        }
        private static OnType defOn = OnType.Mousedown;
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(PropertyName = "on")]
        [Tooltip("on")]
        public OnType On = defOn;
        public bool ShouldSerializeOn()
        {
            return true; // required in json schema 
        }

        private static string defUrl = "";
        [JsonProperty(PropertyName = "url")]
        [Tooltip("url")]
        public string Url = defUrl;
        public bool ShouldSerializeUrl()
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

        public static ArenaGotoUrlJson CreateFromJSON(string jsonString, JToken token)
        {
            _token = token; // save updated wire json
            ArenaGotoUrlJson json = null;
            try {
                json = JsonConvert.DeserializeObject<ArenaGotoUrlJson>(Regex.Unescape(jsonString));
            } catch (JsonReaderException e)
            {
                Debug.LogWarning($"{e.Message}: {jsonString}");
            }
            return json;
        }
    }
}