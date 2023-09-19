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
    /// Draw a line that can have a custom width
    /// </summary>
    [Serializable]
    public class ArenaThicklineJson
    {
        [JsonIgnore]
        public readonly string componentName = "thickline";

        // thickline member-fields

        private static string defColor = "#7f7f7f";
        [JsonProperty(PropertyName = "color")]
        [Tooltip("color")]
        public string Color = defColor;
        public bool ShouldSerializeColor()
        {
            // color
            return (Color != defColor);
        }

        private static float defLineWidth = 5f;
        [JsonProperty(PropertyName = "lineWidth")]
        [Tooltip("Line width")]
        public float LineWidth = defLineWidth;
        public bool ShouldSerializeLineWidth()
        {
            // lineWidth
            return (LineWidth != defLineWidth);
        }

        public enum LineWidthStylerType
        {
            [EnumMember(Value = "default")]
            Default,
            [EnumMember(Value = "grow")]
            Grow,
            [EnumMember(Value = "shrink")]
            Shrink,
            [EnumMember(Value = "center-sharp")]
            CenterSharp,
            [EnumMember(Value = "center-smooth")]
            CenterSmooth,
            [EnumMember(Value = "sine-wave")]
            SineWave,
        }
        private static LineWidthStylerType defLineWidthStyler = LineWidthStylerType.Default;
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(PropertyName = "lineWidthStyler")]
        [Tooltip("Allows defining the line width as a function of relative position p along the path of the line. By default it is set to a constant 1. You may also choose one of the preset functions")]
        public LineWidthStylerType LineWidthStyler = defLineWidthStyler;
        public bool ShouldSerializeLineWidthStyler()
        {
            // lineWidthStyler
            return (LineWidthStyler != defLineWidthStyler);
        }

        private static string defPath = "-2 -1 0, 0 20 0, 10 -1 10";
        [JsonProperty(PropertyName = "path")]
        [Tooltip("Comma-separated list of x y z coordinates of the line vertices")]
        public string Path = defPath;
        public bool ShouldSerializePath()
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

        public string SaveToString()
        {
            return Regex.Unescape(JsonConvert.SerializeObject(this));
        }

        public static ArenaThicklineJson CreateFromJSON(string jsonString, JToken token)
        {
            ArenaThicklineJson json = null;
            try {
                json = JsonConvert.DeserializeObject<ArenaThicklineJson>(Regex.Unescape(jsonString));
            } catch (JsonReaderException e)
            {
                Debug.LogWarning($"{e.Message}: {jsonString}");
            }
            return json;
        }
    }
}
