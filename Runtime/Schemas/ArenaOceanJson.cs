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
    /// Flat-shaded ocean primitive.
    /// </summary>
    [Serializable]
    public class ArenaOceanJson
    {
        [JsonIgnore]
        public readonly string componentName = "ocean";

        // ocean member-fields

        private static float defWidth = 10f;
        [JsonProperty(PropertyName = "width")]
        [Tooltip("Width of the ocean area.")]
        public float Width = defWidth;
        public bool ShouldSerializeWidth()
        {
            return true; // required in json schema
        }

        private static float defDepth = 10f;
        [JsonProperty(PropertyName = "depth")]
        [Tooltip("Depth of the ocean area.")]
        public float Depth = defDepth;
        public bool ShouldSerializeDepth()
        {
            return true; // required in json schema
        }

        private static float defDensity = 10f;
        [JsonProperty(PropertyName = "density")]
        [Tooltip("Density of waves.")]
        public float Density = defDensity;
        public bool ShouldSerializeDensity()
        {
            // density
            return (Density != defDensity);
        }

        private static float defAmplitude = 0.1f;
        [JsonProperty(PropertyName = "amplitude")]
        [Tooltip("Wave amplitude.")]
        public float Amplitude = defAmplitude;
        public bool ShouldSerializeAmplitude()
        {
            // amplitude
            return (Amplitude != defAmplitude);
        }

        private static float defAmplitudeVariance = 0.3f;
        [JsonProperty(PropertyName = "amplitudeVariance")]
        [Tooltip("Wave amplitude variance.")]
        public float AmplitudeVariance = defAmplitudeVariance;
        public bool ShouldSerializeAmplitudeVariance()
        {
            // amplitudeVariance
            return (AmplitudeVariance != defAmplitudeVariance);
        }

        private static float defSpeed = 1f;
        [JsonProperty(PropertyName = "speed")]
        [Tooltip("Wave speed.")]
        public float Speed = defSpeed;
        public bool ShouldSerializeSpeed()
        {
            // speed
            return (Speed != defSpeed);
        }

        private static float defSpeedVariance = 2f;
        [JsonProperty(PropertyName = "speedVariance")]
        [Tooltip("Wave speed variance.")]
        public float SpeedVariance = defSpeedVariance;
        public bool ShouldSerializeSpeedVariance()
        {
            // speedVariance
            return (SpeedVariance != defSpeedVariance);
        }

        private static string defColor = "#7AD2F7";
        [JsonProperty(PropertyName = "color")]
        [Tooltip("Wave color.")]
        public string Color = defColor;
        public bool ShouldSerializeColor()
        {
            return true; // required in json schema
        }

        private static float defOpacity = 0.8f;
        [JsonProperty(PropertyName = "opacity")]
        [Tooltip("Wave opacity.")]
        public float Opacity = defOpacity;
        public bool ShouldSerializeOpacity()
        {
            // opacity
            return (Opacity != defOpacity);
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

        public static ArenaOceanJson CreateFromJSON(string jsonString, JToken token)
        {
            ArenaOceanJson json = null;
            try {
                json = JsonConvert.DeserializeObject<ArenaOceanJson>(Regex.Unescape(jsonString));
            } catch (JsonReaderException e)
            {
                Debug.LogWarning($"{e.Message}: {jsonString}");
            }
            return json;
        }
    }
}
