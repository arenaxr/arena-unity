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
    /// Load a PCD model. Format: <a href='https://pointclouds.org/documentation/tutorials/index.html'>Point Clouds</a>. See guidance to store paths under <a href='https://docs.arenaxr.org/content/interface/filestore.html'>ARENA File Store, CDN, or DropBox</a>.
    /// </summary>
    [Serializable]
    public class ArenaPcdModelJson
    {
        public readonly string object_type = "pcd-model";

        // pcd-model member-fields

        private static string defUrl = null;
        [JsonProperty(PropertyName = "url")]
        [Tooltip("Use File Store paths under 'store/users/username', see CDN and other storage options in the description above.")]
        public string Url = defUrl;
        public bool ShouldSerializeUrl()
        {
            return true; // required in json schema
        }

        private static float defPointSize = 0.01f;
        [JsonProperty(PropertyName = "pointSize")]
        [Tooltip("Size of the points.")]
        public float PointSize = defPointSize;
        public bool ShouldSerializePointSize()
        {
            return true; // required in json schema
        }

        private static string defPointColor = "#7f7f7f";
        [JsonProperty(PropertyName = "pointColor")]
        [Tooltip("Color of the points.")]
        public string PointColor = defPointColor;
        public bool ShouldSerializePointColor()
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
