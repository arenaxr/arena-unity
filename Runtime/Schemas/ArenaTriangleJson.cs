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
    /// Triangle Geometry.
    /// </summary>
    [Serializable]
    public class ArenaTriangleJson
    {
        public readonly string object_type = "triangle";

        // triangle member-fields

        private static ArenaVector3Json defVertexA = JsonConvert.DeserializeObject<ArenaVector3Json>("{'x': 0, 'y': 0.5, 'z': 0}");
        [JsonProperty(PropertyName = "vertexA")]
        [Tooltip("vertex A")]
        public ArenaVector3Json VertexA = defVertexA;
        public bool ShouldSerializeVertexA()
        {
            return true; // required in json schema
        }

        private static ArenaVector3Json defVertexB = JsonConvert.DeserializeObject<ArenaVector3Json>("{'x': -0.5, 'y': -0.5, 'z': 0}");
        [JsonProperty(PropertyName = "vertexB")]
        [Tooltip("vertex B")]
        public ArenaVector3Json VertexB = defVertexB;
        public bool ShouldSerializeVertexB()
        {
            return true; // required in json schema
        }

        private static ArenaVector3Json defVertexC = JsonConvert.DeserializeObject<ArenaVector3Json>("{'x': 0.5, 'y': -0.5, 'z': 0}");
        [JsonProperty(PropertyName = "vertexC")]
        [Tooltip("vertex C")]
        public ArenaVector3Json VertexC = defVertexC;
        public bool ShouldSerializeVertexC()
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
