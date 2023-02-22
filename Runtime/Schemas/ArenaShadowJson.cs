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
    /// Shadow
    /// </summary>
    [Serializable]
    public class ArenaShadowJson
    {
        public const string componentName = "shadow";

        // shadow member-fields

        private static bool defCast = false;
        [JsonProperty(PropertyName = "cast")]
        [Tooltip("Whether the entity casts shadows onto the surrounding scene")]
        public bool Cast = defCast;
        public bool ShouldSerializeCast()
        {
            if (_token != null && _token.SelectToken("cast") != null) return true;
            return (Cast != defCast);
        }

        private static bool defReceive = false;
        [JsonProperty(PropertyName = "receive")]
        [Tooltip("Whether the entity receives shadows from the surrounding scene")]
        public bool Receive = defReceive;
        public bool ShouldSerializeReceive()
        {
            if (_token != null && _token.SelectToken("receive") != null) return true;
            return (Receive != defReceive);
        }

        // General json object management

        [JsonExtensionData]
        private IDictionary<string, JToken> _additionalData;

        private static JToken _token;

        public string SaveToString()
        {
            return Regex.Unescape(JsonConvert.SerializeObject(this));
        }

        public static ArenaShadowJson CreateFromJSON(string jsonString, JToken token)
        {
            _token = token; // save updated wire json
            ArenaShadowJson json = null;
            try {
                json = JsonConvert.DeserializeObject<ArenaShadowJson>(Regex.Unescape(jsonString));
            } catch (JsonReaderException e)
            {
                Debug.LogWarning($"{e.Message}: {jsonString}");
            }
            return json;
        }
    }
}
