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
    /// The material properties of the object’s surface. More properties at <a href='https://aframe.io/docs/1.3.0/components/material.html'>https://aframe.io/docs/1.3.0/components/material.html</a>
    /// </summary>
    [Serializable]
    public class ArenaMaterialJson
    {
        public const string componentName = "material";

        // material member-fields

        private static float defAlphaTest = 0f;
        [JsonProperty(PropertyName = "alphaTest")]
        [Tooltip("Alpha test threshold for transparency.")]
        public float AlphaTest = defAlphaTest;
        public bool ShouldSerializeAlphaTest()
        {
            if (_token != null && _token.SelectToken("alphaTest") != null) return true;
            return (AlphaTest != defAlphaTest);
        }

        public enum BlendingType
        {
            [EnumMember(Value = "none")]
            None,
            [EnumMember(Value = "normal")]
            Normal,
            [EnumMember(Value = "additive")]
            Additive,
            [EnumMember(Value = "subtractive")]
            Subtractive,
            [EnumMember(Value = "multiply")]
            Multiply,
        }
        private static BlendingType defBlending = BlendingType.Normal;
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(PropertyName = "blending")]
        [Tooltip("The blending mode for the material’s RGB and Alpha sent to the WebGLRenderer. Can be one of none, normal, additive, subtractive or multiply")]
        public BlendingType Blending = defBlending;
        public bool ShouldSerializeBlending()
        {
            if (_token != null && _token.SelectToken("blending") != null) return true;
            return (Blending != defBlending);
        }

        private static string defColor = "#7f7f7f";
        [JsonProperty(PropertyName = "color")]
        [Tooltip("Base diffuse color.")]
        public string Color = defColor;
        public bool ShouldSerializeColor()
        {
            if (_token != null && _token.SelectToken("color") != null) return true;
            return (Color != defColor);
        }

        private static bool defDepthTest = true;
        [JsonProperty(PropertyName = "depthTest")]
        [Tooltip("Whether depth testing is enabled when rendering the material.")]
        public bool DepthTest = defDepthTest;
        public bool ShouldSerializeDepthTest()
        {
            if (_token != null && _token.SelectToken("depthTest") != null) return true;
            return (DepthTest != defDepthTest);
        }

        private static bool defDithering = true;
        [JsonProperty(PropertyName = "dithering")]
        [Tooltip("Whether material is dithered with noise. Removes banding from gradients like ones produced by lighting.")]
        public bool Dithering = defDithering;
        public bool ShouldSerializeDithering()
        {
            if (_token != null && _token.SelectToken("dithering") != null) return true;
            return (Dithering != defDithering);
        }

        private static bool defFlatShading = false;
        [JsonProperty(PropertyName = "flatShading")]
        [Tooltip("Use THREE.FlatShading rather than THREE.StandardShading.")]
        public bool FlatShading = defFlatShading;
        public bool ShouldSerializeFlatShading()
        {
            if (_token != null && _token.SelectToken("flatShading") != null) return true;
            return (FlatShading != defFlatShading);
        }

        private static bool defNpot = false;
        [JsonProperty(PropertyName = "npot")]
        [Tooltip("Use settings for non-power-of-two (NPOT) texture.")]
        public bool Npot = defNpot;
        public bool ShouldSerializeNpot()
        {
            if (_token != null && _token.SelectToken("npot") != null) return true;
            return (Npot != defNpot);
        }

        private static object defOffset = JsonConvert.DeserializeObject("{'x': 1, 'y': 1}");
        [JsonProperty(PropertyName = "offset")]
        [Tooltip("Texture offset to be used.")]
        public object Offset = defOffset;
        public bool ShouldSerializeOffset()
        {
            if (_token != null && _token.SelectToken("offset") != null) return true;
            return (Offset != defOffset);
        }

        private static float defOpacity = 1f;
        [JsonProperty(PropertyName = "opacity")]
        [Tooltip("Extent of transparency. If the transparent property is not true, then the material will remain opaque and opacity will only affect color.")]
        public float Opacity = defOpacity;
        public bool ShouldSerializeOpacity()
        {
            if (_token != null && _token.SelectToken("opacity") != null) return true;
            return (Opacity != defOpacity);
        }

        private static object defRepeat = JsonConvert.DeserializeObject("{'x': 1, 'y': 1}");
        [JsonProperty(PropertyName = "repeat")]
        [Tooltip("Texture repeat to be used.")]
        public object Repeat = defRepeat;
        public bool ShouldSerializeRepeat()
        {
            if (_token != null && _token.SelectToken("repeat") != null) return true;
            return (Repeat != defRepeat);
        }

        private static string defShader = "standard";
        [JsonProperty(PropertyName = "shader")]
        [Tooltip("Which material to use. Defaults to the standard material. Can be set to the flat material or to a registered custom shader material.")]
        public string Shader = defShader;
        public bool ShouldSerializeShader()
        {
            if (_token != null && _token.SelectToken("shader") != null) return true;
            return (Shader != defShader);
        }

        public enum SideType
        {
            [EnumMember(Value = "front")]
            Front,
            [EnumMember(Value = "back")]
            Back,
            [EnumMember(Value = "double")]
            Double,
        }
        private static SideType defSide = SideType.Front;
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(PropertyName = "side")]
        [Tooltip("Which sides of the mesh to render. Can be one of front, back, or double.")]
        public SideType Side = defSide;
        public bool ShouldSerializeSide()
        {
            if (_token != null && _token.SelectToken("side") != null) return true;
            return (Side != defSide);
        }

        private static string defSrc = "";
        [JsonProperty(PropertyName = "src")]
        [Tooltip("URI, relative or full path of an image/video file. e.g. 'store/users/wiselab/images/360falls.mp4'")]
        public string Src = defSrc;
        public bool ShouldSerializeSrc()
        {
            if (_token != null && _token.SelectToken("src") != null) return true;
            return (Src != defSrc);
        }

        private static bool defTransparent = false;
        [JsonProperty(PropertyName = "transparent")]
        [Tooltip("Whether material is transparent. Transparent entities are rendered after non-transparent entities.")]
        public bool Transparent = defTransparent;
        public bool ShouldSerializeTransparent()
        {
            if (_token != null && _token.SelectToken("transparent") != null) return true;
            return (Transparent != defTransparent);
        }

        public enum VertexColorsType
        {
            [EnumMember(Value = "none")]
            None,
            [EnumMember(Value = "vertex")]
            Vertex,
            [EnumMember(Value = "face")]
            Face,
        }
        private static VertexColorsType defVertexColors = VertexColorsType.None;
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(PropertyName = "vertexColors")]
        [Tooltip("Whether to use vertex or face colors to shade the material. Can be one of none, vertex, or face.")]
        public VertexColorsType VertexColors = defVertexColors;
        public bool ShouldSerializeVertexColors()
        {
            if (_token != null && _token.SelectToken("vertexColors") != null) return true;
            return (VertexColors != defVertexColors);
        }

        private static bool defVisible = true;
        [JsonProperty(PropertyName = "visible")]
        [Tooltip("Whether material is visible. Raycasters will ignore invisible materials.")]
        public bool Visible = defVisible;
        public bool ShouldSerializeVisible()
        {
            if (_token != null && _token.SelectToken("visible") != null) return true;
            return (Visible != defVisible);
        }

        // General json object management

        [JsonExtensionData]
        private IDictionary<string, JToken> _additionalData;

        private static JToken _token;

        public string SaveToString()
        {
            return Regex.Unescape(JsonConvert.SerializeObject(this));
        }

        public static ArenaMaterialJson CreateFromJSON(string jsonString, JToken token)
        {
            _token = token; // save updated wire json
            return JsonConvert.DeserializeObject<ArenaMaterialJson>(Regex.Unescape(jsonString));
        }
    }
}
