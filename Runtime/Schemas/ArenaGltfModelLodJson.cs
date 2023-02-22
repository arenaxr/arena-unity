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
    /// Simple switch between the default gltf-model and a detailed one when a user camera is within specified distance
    /// </summary>
    [Serializable]
    public class ArenaGltfModelLodJson
    {
        public const string componentName = "gltf-model-lod";

        // gltf-model-lod member-fields

        private static string defDetailedUrl = "";
        [JsonProperty(PropertyName = "detailedUrl")]
        [Tooltip("Alternative 'detailed' gltf model to load by URL")]
        public string DetailedUrl = defDetailedUrl;
        public bool ShouldSerializeDetailedUrl()
        {
            if (_token != null && _token.SelectToken("detailedUrl") != null) return true;
            return (DetailedUrl != defDetailedUrl);
        }

        private static float defDetailedDistance = 10f;
        [JsonProperty(PropertyName = "detailedDistance")]
        [Tooltip("At what distance to switch between the models")]
        public float DetailedDistance = defDetailedDistance;
        public bool ShouldSerializeDetailedDistance()
        {
            if (_token != null && _token.SelectToken("detailedDistance") != null) return true;
            return (DetailedDistance != defDetailedDistance);
        }

        private static float defUpdateRate = 333f;
        [JsonProperty(PropertyName = "updateRate")]
        [Tooltip("How often user camera is checked for LOD (default 333ms)")]
        public float UpdateRate = defUpdateRate;
        public bool ShouldSerializeUpdateRate()
        {
            if (_token != null && _token.SelectToken("updateRate") != null) return true;
            return (UpdateRate != defUpdateRate);
        }

        private static bool defRetainCache = false;
        [JsonProperty(PropertyName = "retainCache")]
        [Tooltip("Whether to skip freeing the detailed model from browser cache (default false)")]
        public bool RetainCache = defRetainCache;
        public bool ShouldSerializeRetainCache()
        {
            if (_token != null && _token.SelectToken("retainCache") != null) return true;
            return (RetainCache != defRetainCache);
        }

        // General json object management

        [JsonExtensionData]
        private IDictionary<string, JToken> _additionalData;

        private static JToken _token;

        public string SaveToString()
        {
            return Regex.Unescape(JsonConvert.SerializeObject(this));
        }

        public static ArenaGltfModelLodJson CreateFromJSON(string jsonString, JToken token)
        {
            _token = token; // save updated wire json
            return JsonConvert.DeserializeObject<ArenaGltfModelLodJson>(Regex.Unescape(jsonString));
        }
    }
}
