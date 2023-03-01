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
    /// 3D object rotation in quaternion representation; Right-handed coordinate system. Euler degrees are deprecated in wire message format.
    /// </summary>
    [Serializable]
    public class ArenaRotationJson
    {
        public const string componentName = "rotation";

        // rotation member-fields

        private static float defW = 1f;
        [JsonProperty(PropertyName = "w")]
        [Tooltip("w")]
        public float W = defW;
        public bool ShouldSerializeW()
        {
            return true; // required in json schema 
        }

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

        public static ArenaRotationJson CreateFromJSON(string jsonString, JToken token)
        {
            _token = token; // save updated wire json
            ArenaRotationJson json = null;
            try {
                json = JsonConvert.DeserializeObject<ArenaRotationJson>(Regex.Unescape(jsonString));
            } catch (JsonReaderException e)
            {
                Debug.LogWarning($"{e.Message}: {jsonString}");
            }
            return json;
        }
    }
}