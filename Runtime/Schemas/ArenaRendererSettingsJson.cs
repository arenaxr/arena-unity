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
    /// These settings are fed into three.js WebGLRenderer properties
    /// </summary>
    [Serializable]
    public class ArenaRendererSettingsJson
    {
        public const string componentName = "renderer-settings";

        // renderer-settings member-fields

        private static float defGammaFactor = 2.2f;
        [JsonProperty(PropertyName = "gammaFactor")]
        [Tooltip("Gamma factor (three.js default is 2.0; we use 2.2 as default)")]
        public float GammaFactor = defGammaFactor;
        public bool ShouldSerializeGammaFactor()
        {
            if (_token != null && _token.SelectToken("gammaFactor") != null) return true;
            return (GammaFactor != defGammaFactor);
        }

        private static bool defLocalClippingEnabled = false;
        [JsonProperty(PropertyName = "localClippingEnabled")]
        [Tooltip("Defines whether the renderer respects object-level clipping planes")]
        public bool LocalClippingEnabled = defLocalClippingEnabled;
        public bool ShouldSerializeLocalClippingEnabled()
        {
            if (_token != null && _token.SelectToken("localClippingEnabled") != null) return true;
            return (LocalClippingEnabled != defLocalClippingEnabled);
        }

        public enum OutputEncodingType
        {
            [EnumMember(Value = "BasicDepthPacking")]
            BasicDepthPacking,
            [EnumMember(Value = "GammaEncoding")]
            GammaEncoding,
            [EnumMember(Value = "LinearEncoding")]
            LinearEncoding,
            [EnumMember(Value = "LogLuvEncoding")]
            LogLuvEncoding,
            [EnumMember(Value = "RGBADepthPacking")]
            RgBADepthPacking,
            [EnumMember(Value = "RGBDEncoding")]
            RgBDEncoding,
            [EnumMember(Value = "RGBEEncoding")]
            RgBEEncoding,
            [EnumMember(Value = "RGBM16Encoding")]
            RgBM16encoding,
            [EnumMember(Value = "RGBM7Encoding")]
            RgBM7encoding,
            [EnumMember(Value = "sRGBEncoding")]
            SrGBEncoding,
        }
        private static OutputEncodingType defOutputEncoding = OutputEncodingType.SrGBEncoding;
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(PropertyName = "outputEncoding")]
        [Tooltip("Defines the output encoding of the renderer (three.js default is LinearEncoding; we use sRGBEncoding as default)")]
        public OutputEncodingType OutputEncoding = defOutputEncoding;
        public bool ShouldSerializeOutputEncoding()
        {
            return true; // required in json schema 
        }

        private static bool defPhysicallyCorrectLights = false;
        [JsonProperty(PropertyName = "physicallyCorrectLights")]
        [Tooltip("Whether to use physically correct lighting mode.")]
        public bool PhysicallyCorrectLights = defPhysicallyCorrectLights;
        public bool ShouldSerializePhysicallyCorrectLights()
        {
            if (_token != null && _token.SelectToken("physicallyCorrectLights") != null) return true;
            return (PhysicallyCorrectLights != defPhysicallyCorrectLights);
        }

        private static bool defSortObjects = true;
        [JsonProperty(PropertyName = "sortObjects")]
        [Tooltip("Defines whether the renderer should sort objects")]
        public bool SortObjects = defSortObjects;
        public bool ShouldSerializeSortObjects()
        {
            if (_token != null && _token.SelectToken("sortObjects") != null) return true;
            return (SortObjects != defSortObjects);
        }

        // General json object management

        [JsonExtensionData]
        private IDictionary<string, JToken> _additionalData;

        private static JToken _token;

        public string SaveToString()
        {
            return Regex.Unescape(JsonConvert.SerializeObject(this));
        }

        public static ArenaRendererSettingsJson CreateFromJSON(string jsonString, JToken token)
        {
            _token = token; // save updated wire json
            return JsonConvert.DeserializeObject<ArenaRendererSettingsJson>(Regex.Unescape(jsonString));
        }
    }
}
