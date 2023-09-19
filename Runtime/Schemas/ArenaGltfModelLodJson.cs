/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2023, Carnegie Mellon University. All rights reserved.
 */

// CAUTION: This file is autogenerated from https://github.com/arenaxr/arena-schemas. Changes made here may be overwritten.

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using UnityEngine;

namespace ArenaUnity.Schemas
{
    /// <summary>
    /// Simple switch between the default gltf-model and a detailed one when a user camera is within specified distance
    /// </summary>
    [Serializable]
    public class ArenaGltfModelLodJson
    {
        [JsonIgnore]
        public readonly string componentName = "gltf-model-lod";

        // gltf-model-lod member-fields

        private static string defDetailedUrl = "";
        [JsonProperty(PropertyName = "detailedUrl")]
        [Tooltip("Alternative 'detailed' gltf model to load by URL")]
        public string DetailedUrl = defDetailedUrl;
        public bool ShouldSerializeDetailedUrl()
        {
            // detailedUrl
            return (DetailedUrl != defDetailedUrl);
        }

        private static float defDetailedDistance = 10f;
        [JsonProperty(PropertyName = "detailedDistance")]
        [Tooltip("At what distance to switch between the models")]
        public float DetailedDistance = defDetailedDistance;
        public bool ShouldSerializeDetailedDistance()
        {
            // detailedDistance
            return (DetailedDistance != defDetailedDistance);
        }

        private static float defUpdateRate = 333f;
        [JsonProperty(PropertyName = "updateRate")]
        [Tooltip("How often user camera is checked for LOD (default 333ms)")]
        public float UpdateRate = defUpdateRate;
        public bool ShouldSerializeUpdateRate()
        {
            // updateRate
            return (UpdateRate != defUpdateRate);
        }

        private static bool defRetainCache = false;
        [JsonProperty(PropertyName = "retainCache")]
        [Tooltip("Whether to skip freeing the detailed model from browser cache (default false)")]
        public bool RetainCache = defRetainCache;
        public bool ShouldSerializeRetainCache()
        {
            // retainCache
            return (RetainCache != defRetainCache);
        }

        // General json object management
        [OnError]
        internal void OnError(StreamingContext context, ErrorContext errorContext)
        {
            Debug.LogWarning($"{errorContext.Error.Message}: {errorContext.OriginalObject}");
            errorContext.Handled = true;
        }

        [JsonExtensionData]
        private IDictionary<string, JToken> _additionalData;

        public string SaveToString()
        {
            return Regex.Unescape(JsonConvert.SerializeObject(this));
        }

        public static ArenaGltfModelLodJson CreateFromJSON(string jsonString, JToken token)
        {
            ArenaGltfModelLodJson json = null;
            try {
                json = JsonConvert.DeserializeObject<ArenaGltfModelLodJson>(Regex.Unescape(jsonString));
            } catch (JsonReaderException e)
            {
                Debug.LogWarning($"{e.Message}: {jsonString}");
            }
            return json;
        }
    }
}
