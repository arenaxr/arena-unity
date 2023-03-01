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
    /// 3D object scale
    /// </summary>
    [Serializable]
    public class ArenaScaleJson
    {
        public const string componentName = "scale";

        // scale member-fields

        private static float defX = 1f;
        [JsonProperty(PropertyName = "x")]
        [Tooltip("x")]
        public float X = defX;
        public bool ShouldSerializeX()
        {
            return true; // required in json schema 
        }

        private static float defY = 1f;
        [JsonProperty(PropertyName = "y")]
        [Tooltip("y")]
        public float Y = defY;
        public bool ShouldSerializeY()
        {
            return true; // required in json schema 
        }

        private static float defZ = 1f;
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

        public static ArenaScaleJson CreateFromJSON(string jsonString, JToken token)
        {
            _token = token; // save updated wire json
            ArenaScaleJson json = null;
            try {
                json = JsonConvert.DeserializeObject<ArenaScaleJson>(Regex.Unescape(jsonString));
            } catch (JsonReaderException e)
            {
                Debug.LogWarning($"{e.Message}: {jsonString}");
            }
            return json;
        }
    }
}