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
using UnityEngine;

namespace ArenaUnity.Schemas
{
    /// <summary>
    /// Display text. More properties at <a href='https://aframe.io/docs/1.4.0/components/text.html'>https://aframe.io/docs/1.4.0/components/text.html</a>
    /// </summary>
    [Serializable]
    public class ArenaTextJson
    {
        public const string componentName = "text";

        // text member-fields

        public enum AlignType
        {
            [EnumMember(Value = "left")]
            Left,
            [EnumMember(Value = "center")]
            Center,
            [EnumMember(Value = "right")]
            Right,
        }
        private static AlignType defAlign = AlignType.Left;
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(PropertyName = "align")]
        [Tooltip("Multi-line text alignment (left, center, right).")]
        public AlignType Align = defAlign;
        public bool ShouldSerializeAlign()
        {
            if (_token != null && _token.SelectToken("align") != null) return true;
            return (Align != defAlign);
        }

        private static float defAlphaTest = 0.5f;
        [JsonProperty(PropertyName = "alphaTest")]
        [Tooltip("Discard text pixels if alpha is less than this value.")]
        public float AlphaTest = defAlphaTest;
        public bool ShouldSerializeAlphaTest()
        {
            if (_token != null && _token.SelectToken("alphaTest") != null) return true;
            return (AlphaTest != defAlphaTest);
        }

        public enum AnchorType
        {
            [EnumMember(Value = "left")]
            Left,
            [EnumMember(Value = "right")]
            Right,
            [EnumMember(Value = "center")]
            Center,
            [EnumMember(Value = "align")]
            Align,
        }
        private static AnchorType defAnchor = AnchorType.Center;
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(PropertyName = "anchor")]
        [Tooltip("Horizontal positioning (left, center, right, align).")]
        public AnchorType Anchor = defAnchor;
        public bool ShouldSerializeAnchor()
        {
            if (_token != null && _token.SelectToken("anchor") != null) return true;
            return (Anchor != defAnchor);
        }

        public enum BaselineType
        {
            [EnumMember(Value = "top")]
            Top,
            [EnumMember(Value = "center")]
            Center,
            [EnumMember(Value = "bottom")]
            Bottom,
        }
        private static BaselineType defBaseline = BaselineType.Center;
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(PropertyName = "baseline")]
        [Tooltip("Vertical positioning (top, center, bottom).")]
        public BaselineType Baseline = defBaseline;
        public bool ShouldSerializeBaseline()
        {
            if (_token != null && _token.SelectToken("baseline") != null) return true;
            return (Baseline != defBaseline);
        }

        private static string defColor = "white";
        [JsonProperty(PropertyName = "color")]
        [Tooltip("Text color.")]
        public string Color = defColor;
        public bool ShouldSerializeColor()
        {
            if (_token != null && _token.SelectToken("color") != null) return true;
            return (Color != defColor);
        }

        public enum FontType
        {
            [EnumMember(Value = "aileronsemibold")]
            Aileronsemibold,
            [EnumMember(Value = "dejavu")]
            Dejavu,
            [EnumMember(Value = "exo2bold")]
            Exo2bold,
            [EnumMember(Value = "exo2semibold")]
            Exo2semibold,
            [EnumMember(Value = "kelsonsans")]
            Kelsonsans,
            [EnumMember(Value = "monoid")]
            Monoid,
            [EnumMember(Value = "mozillavr")]
            Mozillavr,
            [EnumMember(Value = "roboto")]
            Roboto,
            [EnumMember(Value = "sourcecodepro")]
            Sourcecodepro,
        }
        private static FontType defFont = FontType.Roboto;
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(PropertyName = "font")]
        [Tooltip("Font to render text, either the name of one of A-Frame's stock fonts or a URL to a font file")]
        public FontType Font = defFont;
        public bool ShouldSerializeFont()
        {
            if (_token != null && _token.SelectToken("font") != null) return true;
            return (Font != defFont);
        }

        private static string defFontImage = "";
        [JsonProperty(PropertyName = "fontImage")]
        [Tooltip("Font image texture path to render text. Defaults to the font's name with extension replaced to .png. Don't need to specify if using a stock font. (derived from font name)")]
        public string FontImage = defFontImage;
        public bool ShouldSerializeFontImage()
        {
            if (_token != null && _token.SelectToken("fontImage") != null) return true;
            return (FontImage != defFontImage);
        }

        private static float defHeight = f;
        [JsonProperty(PropertyName = "height")]
        [Tooltip("Height of text block. (derived from text size)")]
        public float Height = defHeight;
        public bool ShouldSerializeHeight()
        {
            if (_token != null && _token.SelectToken("height") != null) return true;
            return (Height != defHeight);
        }

        private static float defLetterSpacing = 0f;
        [JsonProperty(PropertyName = "letterSpacing")]
        [Tooltip("Letter spacing in pixels.")]
        public float LetterSpacing = defLetterSpacing;
        public bool ShouldSerializeLetterSpacing()
        {
            if (_token != null && _token.SelectToken("letterSpacing") != null) return true;
            return (LetterSpacing != defLetterSpacing);
        }

        private static float defLineHeight = f;
        [JsonProperty(PropertyName = "lineHeight")]
        [Tooltip("Line height in pixels. (derived from font file)")]
        public float LineHeight = defLineHeight;
        public bool ShouldSerializeLineHeight()
        {
            if (_token != null && _token.SelectToken("lineHeight") != null) return true;
            return (LineHeight != defLineHeight);
        }

        private static bool defNegate = true;
        [JsonProperty(PropertyName = "negate")]
        [Tooltip("negate")]
        public bool Negate = defNegate;
        public bool ShouldSerializeNegate()
        {
            if (_token != null && _token.SelectToken("negate") != null) return true;
            return (Negate != defNegate);
        }

        private static float defOpacity = 1.0f;
        [JsonProperty(PropertyName = "opacity")]
        [Tooltip("Opacity, on a scale from 0 to 1, where 0 means fully transparent and 1 means fully opaque.")]
        public float Opacity = defOpacity;
        public bool ShouldSerializeOpacity()
        {
            if (_token != null && _token.SelectToken("opacity") != null) return true;
            return (Opacity != defOpacity);
        }

        public enum ShaderType
        {
            [EnumMember(Value = "portal")]
            Portal,
            [EnumMember(Value = "flat")]
            Flat,
            [EnumMember(Value = "standard")]
            Standard,
            [EnumMember(Value = "sdf")]
            Sdf,
            [EnumMember(Value = "msdf")]
            Msdf,
            [EnumMember(Value = "ios10hls")]
            Ios10hls,
            [EnumMember(Value = "skyshader")]
            Skyshader,
            [EnumMember(Value = "gradientshader")]
            Gradientshader,
        }
        private static ShaderType defShader = ShaderType.Sdf;
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(PropertyName = "shader")]
        [Tooltip("Shader used to render text.")]
        public ShaderType Shader = defShader;
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
        private static SideType defSide = SideType.Double;
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(PropertyName = "side")]
        [Tooltip("Side to render. (front, back, double)")]
        public SideType Side = defSide;
        public bool ShouldSerializeSide()
        {
            if (_token != null && _token.SelectToken("side") != null) return true;
            return (Side != defSide);
        }

        private static float defTabSize = 4f;
        [JsonProperty(PropertyName = "tabSize")]
        [Tooltip("Tab size in spaces.")]
        public float TabSize = defTabSize;
        public bool ShouldSerializeTabSize()
        {
            if (_token != null && _token.SelectToken("tabSize") != null) return true;
            return (TabSize != defTabSize);
        }

        private static bool defTransparent = true;
        [JsonProperty(PropertyName = "transparent")]
        [Tooltip("Whether text is transparent.")]
        public bool Transparent = defTransparent;
        public bool ShouldSerializeTransparent()
        {
            if (_token != null && _token.SelectToken("transparent") != null) return true;
            return (Transparent != defTransparent);
        }

        private static string defValue = "";
        [JsonProperty(PropertyName = "value")]
        [Tooltip("The actual content of the text. Line breaks and tabs are supported with \n and \t.")]
        public string Value = defValue;
        public bool ShouldSerializeValue()
        {
            if (_token != null && _token.SelectToken("value") != null) return true;
            return (Value != defValue);
        }

        public enum WhiteSpaceType
        {
            [EnumMember(Value = "normal")]
            Normal,
            [EnumMember(Value = "pre")]
            Pre,
            [EnumMember(Value = "nowrap")]
            Nowrap,
        }
        private static WhiteSpaceType defWhiteSpace = WhiteSpaceType.Normal;
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(PropertyName = "whiteSpace")]
        [Tooltip("How whitespace should be handled (i.e., normal, pre, nowrap).</a>")]
        public WhiteSpaceType WhiteSpace = defWhiteSpace;
        public bool ShouldSerializeWhiteSpace()
        {
            if (_token != null && _token.SelectToken("whiteSpace") != null) return true;
            return (WhiteSpace != defWhiteSpace);
        }

        private static float defWidth = 5f;
        [JsonProperty(PropertyName = "width")]
        [Tooltip("Width in meters. (derived from geometry if exists)")]
        public float Width = defWidth;
        public bool ShouldSerializeWidth()
        {
            if (_token != null && _token.SelectToken("width") != null) return true;
            return (Width != defWidth);
        }

        private static float defWrapCount = 40f;
        [JsonProperty(PropertyName = "wrapCount")]
        [Tooltip("Number of characters before wrapping text (more or less).")]
        public float WrapCount = defWrapCount;
        public bool ShouldSerializeWrapCount()
        {
            if (_token != null && _token.SelectToken("wrapCount") != null) return true;
            return (WrapCount != defWrapCount);
        }

        private static float defWrapPixels = f;
        [JsonProperty(PropertyName = "wrapPixels")]
        [Tooltip("Number of pixels before wrapping text. (derived from wrapCount)")]
        public float WrapPixels = defWrapPixels;
        public bool ShouldSerializeWrapPixels()
        {
            if (_token != null && _token.SelectToken("wrapPixels") != null) return true;
            return (WrapPixels != defWrapPixels);
        }

        private static float defXoffset = 0f;
        [JsonProperty(PropertyName = "xOffset")]
        [Tooltip("X-offset to apply to add padding.")]
        public float Xoffset = defXoffset;
        public bool ShouldSerializeXoffset()
        {
            if (_token != null && _token.SelectToken("xOffset") != null) return true;
            return (Xoffset != defXoffset);
        }

        private static float defZoffset = 0.001f;
        [JsonProperty(PropertyName = "zOffset")]
        [Tooltip("Z-offset to apply to avoid Z-fighting if using with a geometry as a background.")]
        public float Zoffset = defZoffset;
        public bool ShouldSerializeZoffset()
        {
            if (_token != null && _token.SelectToken("zOffset") != null) return true;
            return (Zoffset != defZoffset);
        }

        // General json object management

        [JsonExtensionData]
        private IDictionary<string, JToken> _additionalData;

        private static JToken _token;

        public string SaveToString()
        {
            return Regex.Unescape(JsonConvert.SerializeObject(this));
        }

        public static ArenaTextJson CreateFromJSON(string jsonString, JToken token)
        {
            _token = token; // save updated wire json
            ArenaTextJson json = null;
            try {
                json = JsonConvert.DeserializeObject<ArenaTextJson>(Regex.Unescape(jsonString));
            } catch (JsonReaderException e)
            {
                Debug.LogWarning($"{e.Message}: {jsonString}");
            }
            return json;
        }
    }
}
