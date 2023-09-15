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
using UnityEngine;

namespace ArenaUnity.Schemas
{
    /// <summary>
    /// Octahedron Geometry
    /// </summary>
    [Serializable]
    public class ArenaOctahedronJson
    {
        public const string componentName = "octahedron";

        // octahedron member-fields

        private static int defDetail = 0;
        [JsonProperty(PropertyName = "detail")]
        [Tooltip("detail")]
        public int Detail = defDetail;
        public bool ShouldSerializeDetail()
        {
            // detail
            return (Detail != defDetail);
        }

        private static float defRadius = 1f;
        [JsonProperty(PropertyName = "radius")]
        [Tooltip("radius")]
        public float Radius = defRadius;
        public bool ShouldSerializeRadius()
        {
            return true; // required in json schema
        }

        // General json object management

        [JsonExtensionData]
        private IDictionary<string, JToken> _additionalData;

        public string SaveToString()
        {
            return Regex.Unescape(JsonConvert.SerializeObject(this));
        }

        public static ArenaOctahedronJson CreateFromJSON(string jsonString, JToken token)
        {
            ArenaOctahedronJson json = null;
            try {
                json = JsonConvert.DeserializeObject<ArenaOctahedronJson>(Regex.Unescape(jsonString));
            } catch (JsonReaderException e)
            {
                Debug.LogWarning($"{e.Message}: {jsonString}");
            }
            return json;
        }
    }
}
