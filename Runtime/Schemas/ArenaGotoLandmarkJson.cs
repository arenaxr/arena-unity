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
    /// Teleports user to the landmark with the given name; Requires click-listener
    /// </summary>
    [Serializable]
    public class ArenaGotoLandmarkJson
    {
        public const string componentName = "goto-landmark";

        // goto-landmark member-fields

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
        [Tooltip("Event to listen 'on'.")]
        public OnType On = defOn;
        public bool ShouldSerializeOn()
        {
            return true; // required in json schema 
        }

        private static string defLandmark = "";
        [JsonProperty(PropertyName = "landmark")]
        [Tooltip("Id of landmark to teleport to.")]
        public string Landmark = defLandmark;
        public bool ShouldSerializeLandmark()
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

        public static ArenaGotoLandmarkJson CreateFromJSON(string jsonString, JToken token)
        {
            _token = token; // save updated wire json
            return JsonConvert.DeserializeObject<ArenaGotoLandmarkJson>(Regex.Unescape(jsonString));
        }
    }
}
