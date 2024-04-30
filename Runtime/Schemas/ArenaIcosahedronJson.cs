/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2024, Carnegie Mellon University. All rights reserved.
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
    /// Icosahedron Geometry.
    /// </summary>
    [Serializable]
    public class ArenaIcosahedronJson
    {
        public readonly string object_type = "icosahedron";

        // icosahedron member-fields

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
        [OnError]
        internal void OnError(StreamingContext context, ErrorContext errorContext)
        {
            Debug.LogWarning($"{errorContext.Error.Message}: {errorContext.OriginalObject}");
            errorContext.Handled = true;
        }

        [JsonExtensionData]
        private IDictionary<string, JToken> _additionalData;
    }
}
