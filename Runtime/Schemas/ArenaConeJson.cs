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
    /// Cone Geometry.
    /// </summary>
    [Serializable]
    public class ArenaConeJson
    {
        public readonly string object_type = "cone";

        // cone member-fields

        private static float defHeight = 1f;
        [JsonProperty(PropertyName = "height")]
        [Tooltip("height")]
        public float Height = defHeight;
        public bool ShouldSerializeHeight()
        {
            return true; // required in json schema
        }

        private static bool defOpenEnded = false;
        [JsonProperty(PropertyName = "openEnded")]
        [Tooltip("open ended")]
        public bool OpenEnded = defOpenEnded;
        public bool ShouldSerializeOpenEnded()
        {
            // openEnded
            return (OpenEnded != defOpenEnded);
        }

        private static float defRadiusBottom = 1f;
        [JsonProperty(PropertyName = "radiusBottom")]
        [Tooltip("radius bottom")]
        public float RadiusBottom = defRadiusBottom;
        public bool ShouldSerializeRadiusBottom()
        {
            return true; // required in json schema
        }

        private static float defRadiusTop = 0.01f;
        [JsonProperty(PropertyName = "radiusTop")]
        [Tooltip("radius top")]
        public float RadiusTop = defRadiusTop;
        public bool ShouldSerializeRadiusTop()
        {
            return true; // required in json schema
        }

        private static int defSegmentsHeight = 18;
        [JsonProperty(PropertyName = "segmentsHeight")]
        [Tooltip("segments height")]
        public int SegmentsHeight = defSegmentsHeight;
        public bool ShouldSerializeSegmentsHeight()
        {
            // segmentsHeight
            return (SegmentsHeight != defSegmentsHeight);
        }

        private static int defSegmentsRadial = 36;
        [JsonProperty(PropertyName = "segmentsRadial")]
        [Tooltip("segments radial")]
        public int SegmentsRadial = defSegmentsRadial;
        public bool ShouldSerializeSegmentsRadial()
        {
            // segmentsRadial
            return (SegmentsRadial != defSegmentsRadial);
        }

        private static float defThetaLength = 360f;
        [JsonProperty(PropertyName = "thetaLength")]
        [Tooltip("theta length")]
        public float ThetaLength = defThetaLength;
        public bool ShouldSerializeThetaLength()
        {
            // thetaLength
            return (ThetaLength != defThetaLength);
        }

        private static float defThetaStart = 0f;
        [JsonProperty(PropertyName = "thetaStart")]
        [Tooltip("theta start")]
        public float ThetaStart = defThetaStart;
        public bool ShouldSerializeThetaStart()
        {
            // thetaStart
            return (ThetaStart != defThetaStart);
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
