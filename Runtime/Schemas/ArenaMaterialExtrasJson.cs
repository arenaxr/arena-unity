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
    /// Define extra material properties, namely texture encoding, whether to render the material's color and render order. The properties set here access directly Three.js material component. More properties at <a href='https://threejs.org/docs/#api/en/materials/Material'>https://threejs.org/docs/#api/en/materials/Material</a>
    /// </summary>
    [Serializable]
    public class ArenaMaterialExtrasJson
    {
        public const string componentName = "material-extras";

        // material-extras member-fields

        private static string defOverrideSrc = "";
        [JsonProperty(PropertyName = "overrideSrc")]
        [Tooltip("Overrides the material source in all meshes of an object (e.g. a basic shape or a GLTF); Use, for example, to change the texture of a GLTF.")]
        public string OverrideSrc = defOverrideSrc;
        public bool ShouldSerializeOverrideSrc()
        {
            if (_token != null && _token.SelectToken("overrideSrc") != null) return true;
            return (OverrideSrc != defOverrideSrc);
        }

        public enum EncodingType
        {
            [EnumMember(Value = "LinearEncoding")]
            LinearEncoding,
            [EnumMember(Value = "sRGBEncoding")]
            SrGBEncoding,
            [EnumMember(Value = "GammaEncoding")]
            GammaEncoding,
            [EnumMember(Value = "RGBEEncoding")]
            RgBEEncoding,
            [EnumMember(Value = "LogLuvEncoding")]
            LogLuvEncoding,
            [EnumMember(Value = "RGBM7Encoding")]
            RgBM7encoding,
            [EnumMember(Value = "RGBM16Encoding")]
            RgBM16encoding,
            [EnumMember(Value = "RGBDEncoding")]
            RgBDEncoding,
            [EnumMember(Value = "BasicDepthPacking")]
            BasicDepthPacking,
            [EnumMember(Value = "RGBADepthPacking")]
            RgBADepthPacking,
        }
        private static EncodingType defEncoding = EncodingType.SrGBEncoding;
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(PropertyName = "encoding")]
        [Tooltip("encoding")]
        public EncodingType Encoding = defEncoding;
        public bool ShouldSerializeEncoding()
        {
            if (_token != null && _token.SelectToken("encoding") != null) return true;
            return (Encoding != defEncoding);
        }

        private static bool defColorWrite = true;
        [JsonProperty(PropertyName = "colorWrite")]
        [Tooltip("Whether to render the material's color.")]
        public bool ColorWrite = defColorWrite;
        public bool ShouldSerializeColorWrite()
        {
            if (_token != null && _token.SelectToken("colorWrite") != null) return true;
            return (ColorWrite != defColorWrite);
        }

        private static float defRenderOrder = 1f;
        [JsonProperty(PropertyName = "renderOrder")]
        [Tooltip("Allows the default rendering order of scene graph objects to be overridden.")]
        public float RenderOrder = defRenderOrder;
        public bool ShouldSerializeRenderOrder()
        {
            if (_token != null && _token.SelectToken("renderOrder") != null) return true;
            return (RenderOrder != defRenderOrder);
        }

        private static bool defTransparentOccluder = false;
        [JsonProperty(PropertyName = "transparentOccluder")]
        [Tooltip("If `true`, will set `colorWrite=false` and `renderOrder=0` to make the material a transparent occluder.")]
        public bool TransparentOccluder = defTransparentOccluder;
        public bool ShouldSerializeTransparentOccluder()
        {
            if (_token != null && _token.SelectToken("transparentOccluder") != null) return true;
            return (TransparentOccluder != defTransparentOccluder);
        }

        // General json object management

        [JsonExtensionData]
        private IDictionary<string, JToken> _additionalData;

        private static JToken _token;

        public string SaveToString()
        {
            return Regex.Unescape(JsonConvert.SerializeObject(this));
        }

        public static ArenaMaterialExtrasJson CreateFromJSON(string jsonString, JToken token)
        {
            _token = token; // save updated wire json
            ArenaMaterialExtrasJson json = null;
            try {
                json = JsonConvert.DeserializeObject<ArenaMaterialExtrasJson>(Regex.Unescape(jsonString));
            } catch (JsonReaderException e)
            {
                Debug.LogWarning($"{e.Message}: {jsonString}");
            }
            return json;
        }
    }
}
