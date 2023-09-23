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
    /// Draw a line
    /// </summary>
    [Serializable]
    public class ArenaLineJson
    {
        public readonly string object_type = "line";

        // line member-fields

        private static string defColor = "#7f7f7f";
        [JsonProperty(PropertyName = "color")]
        [Tooltip("color")]
        public string Color = defColor;
        public bool ShouldSerializeColor()
        {
            // color
            return (Color != defColor);
        }

        private static ArenaVector3Json defEnd = JsonConvert.DeserializeObject<ArenaVector3Json>("{'x': -0.5, 'y': -0.5, 'z': 0}");
        [JsonProperty(PropertyName = "end")]
        [Tooltip("vertex B (end)")]
        public ArenaVector3Json End = defEnd;
        public bool ShouldSerializeEnd()
        {
            return true; // required in json schema
        }

        private static float defOpacity = 1f;
        [JsonProperty(PropertyName = "opacity")]
        [Tooltip("Line Opacity")]
        public float Opacity = defOpacity;
        public bool ShouldSerializeOpacity()
        {
            // opacity
            return (Opacity != defOpacity);
        }

        private static ArenaVector3Json defStart = JsonConvert.DeserializeObject<ArenaVector3Json>("{'x': 0, 'y': 0.5, 'z': 0}");
        [JsonProperty(PropertyName = "start")]
        [Tooltip("vertex A (start)")]
        public ArenaVector3Json Start = defStart;
        public bool ShouldSerializeStart()
        {
            return true; // required in json schema
        }

        private static bool defVisible = true;
        [JsonProperty(PropertyName = "visible")]
        [Tooltip("Visible")]
        public bool Visible = defVisible;
        public bool ShouldSerializeVisible()
        {
            // visible
            return (Visible != defVisible);
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
