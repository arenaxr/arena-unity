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
    /// 3D object position
    /// </summary>
    [Serializable]
    public class ArenaPositionJson
    {
        public const string componentName = "position";

        // position member-fields

        private static float defX = 0f;
        [JsonProperty(PropertyName = "x")]
        [Tooltip("x")]
        public float X = defX;
        public bool ShouldSerializeX()
        {
            return true; // required in json schema 
        }

        private static float defY = 0f;
        [JsonProperty(PropertyName = "y")]
        [Tooltip("y")]
        public float Y = defY;
        public bool ShouldSerializeY()
        {
            return true; // required in json schema 
        }

        private static float defZ = 0f;
        [JsonProperty(PropertyName = "z")]
        [Tooltip("z")]
        public float Z = defZ;
        public bool ShouldSerializeZ()
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

        public static ArenaPositionJson CreateFromJSON(string jsonString, JToken token)
        {
            _token = token; // save updated wire json
            ArenaPositionJson json = null;
            try {
                json = JsonConvert.DeserializeObject<ArenaPositionJson>(Regex.Unescape(jsonString));
            } catch (JsonReaderException e)
            {
                Debug.LogWarning($"{e.Message}: {jsonString}");
            }
            return json;
        }
    }
}
