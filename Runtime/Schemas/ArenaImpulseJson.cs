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
    /// The force applied using physics. Requires click-listener
    /// </summary>
    [Serializable]
    public class ArenaImpulseJson
    {
        public const string componentName = "impulse";

        // impulse member-fields

        private static string defForce = "";
        [JsonProperty(PropertyName = "force")]
        [Tooltip("force")]
        public string Force = defForce;
        public bool ShouldSerializeForce()
        {
            if (_token != null && _token.SelectToken("force") != null) return true;
            return (Force != defForce);
        }

        private static string defOn = "";
        [JsonProperty(PropertyName = "on")]
        [Tooltip("on")]
        public string On = defOn;
        public bool ShouldSerializeOn()
        {
            if (_token != null && _token.SelectToken("on") != null) return true;
            return (On != defOn);
        }

        private static string defPosition = "";
        [JsonProperty(PropertyName = "position")]
        [Tooltip("position")]
        public string Position = defPosition;
        public bool ShouldSerializePosition()
        {
            if (_token != null && _token.SelectToken("position") != null) return true;
            return (Position != defPosition);
        }

        // General json object management

        [JsonExtensionData]
        private IDictionary<string, JToken> _additionalData;

        private static JToken _token;

        public string SaveToString()
        {
            return Regex.Unescape(JsonConvert.SerializeObject(this));
        }

        public static ArenaImpulseJson CreateFromJSON(string jsonString, JToken token)
        {
            _token = token; // save updated wire json
            ArenaImpulseJson json = null;
            try {
                json = JsonConvert.DeserializeObject<ArenaImpulseJson>(Regex.Unescape(jsonString));
            } catch (JsonReaderException e)
            {
                Debug.LogWarning($"{e.Message}: {jsonString}");
            }
            return json;
        }
    }
}