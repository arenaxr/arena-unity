/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2024, Carnegie Mellon University. All rights reserved.
 */

using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ArenaUnity.Schemas.Converter
{
    /// <summary>
    /// Complete dictionary of CSS named colors and normalization utility.
    /// Converts CSS color names (e.g. "violet") to hex values (e.g. "#EE82EE")
    /// that Unity's ColorUtility.TryParseHtmlString can parse.
    /// Reference: https://developer.mozilla.org/en-US/docs/Web/CSS/named-color
    /// </summary>
    public static class ArenaCssColors
    {
        // All 148 CSS named colors (case-insensitive lookup)
        public static readonly Dictionary<string, string> NameToHex = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            // Reds
            { "indianred", "#CD5C5C" },
            { "lightcoral", "#F08080" },
            { "salmon", "#FA8072" },
            { "darksalmon", "#E9967A" },
            { "lightsalmon", "#FFA07A" },
            { "crimson", "#DC143C" },
            { "firebrick", "#B22222" },
            { "darkred", "#8B0000" },

            // Pinks
            { "pink", "#FFC0CB" },
            { "lightpink", "#FFB6C1" },
            { "hotpink", "#FF69B4" },
            { "deeppink", "#FF1493" },
            { "mediumvioletred", "#C71585" },
            { "palevioletred", "#DB7093" },

            // Oranges
            { "coral", "#FF7F50" },
            { "tomato", "#FF6347" },
            { "orangered", "#FF4500" },
            { "darkorange", "#FF8C00" },
            { "orange", "#FFA500" },

            // Yellows
            { "gold", "#FFD700" },
            { "lightyellow", "#FFFFE0" },
            { "lemonchiffon", "#FFFACD" },
            { "lightgoldenrodyellow", "#FAFAD2" },
            { "papayawhip", "#FFEFD5" },
            { "moccasin", "#FFE4B5" },
            { "peachpuff", "#FFDAB9" },
            { "palegoldenrod", "#EEE8AA" },
            { "khaki", "#F0E68C" },
            { "darkkhaki", "#BDB76B" },

            // Purples
            { "lavender", "#E6E6FA" },
            { "thistle", "#D8BFD8" },
            { "plum", "#DDA0DD" },
            { "violet", "#EE82EE" },
            { "orchid", "#DA70D6" },
            { "fuchsia", "#FF00FF" },
            { "magenta", "#FF00FF" },
            { "mediumorchid", "#BA55D3" },
            { "mediumpurple", "#9370DB" },
            { "rebeccapurple", "#663399" },
            { "blueviolet", "#8A2BE2" },
            { "darkviolet", "#9400D3" },
            { "darkorchid", "#9932CC" },
            { "darkmagenta", "#8B008B" },
            { "indigo", "#4B0082" },
            { "slateblue", "#6A5ACD" },
            { "darkslateblue", "#483D8B" },
            { "mediumslateblue", "#7B68EE" },

            // Greens
            { "greenyellow", "#ADFF2F" },
            { "chartreuse", "#7FFF00" },
            { "lawngreen", "#7CFC00" },
            { "limegreen", "#32CD32" },
            { "palegreen", "#98FB98" },
            { "lightgreen", "#90EE90" },
            { "mediumspringgreen", "#00FA9A" },
            { "springgreen", "#00FF7F" },
            { "mediumseagreen", "#3CB371" },
            { "seagreen", "#2E8B57" },
            { "forestgreen", "#228B22" },
            { "darkgreen", "#006400" },
            { "yellowgreen", "#9ACD32" },
            { "olivedrab", "#6B8E23" },
            { "olive", "#808000" },
            { "darkolivegreen", "#556B2F" },
            { "mediumaquamarine", "#66CDAA" },
            { "darkseagreen", "#8FBC8F" },
            { "lightseagreen", "#20B2AA" },
            { "darkcyan", "#008B8B" },

            // Blues
            { "cyan", "#00FFFF" },
            { "lightcyan", "#E0FFFF" },
            { "paleturquoise", "#AFEEEE" },
            { "aquamarine", "#7FFFD4" },
            { "turquoise", "#40E0D0" },
            { "mediumturquoise", "#48D1CC" },
            { "darkturquoise", "#00CED1" },
            { "cadetblue", "#5F9EA0" },
            { "steelblue", "#4682B4" },
            { "lightsteelblue", "#B0C4DE" },
            { "powderblue", "#B0E0E6" },
            { "lightblue", "#ADD8E6" },
            { "skyblue", "#87CEEB" },
            { "lightskyblue", "#87CEFA" },
            { "deepskyblue", "#00BFFF" },
            { "dodgerblue", "#1E90FF" },
            { "cornflowerblue", "#6495ED" },
            { "royalblue", "#4169E1" },
            { "mediumblue", "#0000CD" },
            { "darkblue", "#00008B" },
            { "midnightblue", "#191970" },

            // Browns
            { "cornsilk", "#FFF8DC" },
            { "blanchedalmond", "#FFEBCD" },
            { "bisque", "#FFE4C4" },
            { "navajowhite", "#FFDEAD" },
            { "wheat", "#F5DEB3" },
            { "burlywood", "#DEB887" },
            { "tan", "#D2B48C" },
            { "rosybrown", "#BC8F8F" },
            { "sandybrown", "#F4A460" },
            { "goldenrod", "#DAA520" },
            { "darkgoldenrod", "#B8860B" },
            { "peru", "#CD853F" },
            { "chocolate", "#D2691E" },
            { "saddlebrown", "#8B4513" },
            { "sienna", "#A0522D" },
            { "brown", "#A52A2A" },

            // Whites
            { "snow", "#FFFAFA" },
            { "honeydew", "#F0FFF0" },
            { "mintcream", "#F5FFFA" },
            { "azure", "#F0FFFF" },
            { "aliceblue", "#F0F8FF" },
            { "ghostwhite", "#F8F8FF" },
            { "whitesmoke", "#F5F5F5" },
            { "seashell", "#FFF5EE" },
            { "beige", "#F5F5DC" },
            { "oldlace", "#FDF5E6" },
            { "floralwhite", "#FFFAF0" },
            { "ivory", "#FFFFF0" },
            { "antiquewhite", "#FAEBD7" },
            { "linen", "#FAF0E6" },
            { "lavenderblush", "#FFF0F5" },
            { "mistyrose", "#FFE4E1" },

            // Grays
            { "gainsboro", "#DCDCDC" },
            { "lightgray", "#D3D3D3" },
            { "lightgrey", "#D3D3D3" },
            { "darkgray", "#A9A9A9" },
            { "darkgrey", "#A9A9A9" },
            { "gray", "#808080" },
            { "grey", "#808080" },
            { "dimgray", "#696969" },
            { "dimgrey", "#696969" },
            { "lightslategray", "#778899" },
            { "lightslategrey", "#778899" },
            { "slategray", "#708090" },
            { "slategrey", "#708090" },
            { "darkslategray", "#2F4F4F" },
            { "darkslategrey", "#2F4F4F" },

            // CSS Level 1 keywords (supported by Unity but included for completeness)
            { "black", "#000000" },
            { "silver", "#C0C0C0" },
            { "white", "#FFFFFF" },
            { "maroon", "#800000" },
            { "red", "#FF0000" },
            { "purple", "#800080" },
            { "green", "#008000" },
            { "lime", "#00FF00" },
            { "yellow", "#FFFF00" },
            { "navy", "#000080" },
            { "blue", "#0000FF" },
            { "teal", "#008080" },
            { "aqua", "#00FFFF" },
        };

        /// <summary>
        /// Normalizes a CSS color string to a format Unity can parse.
        /// Named colors are converted to hex; hex/rgb strings pass through unchanged.
        /// </summary>
        public static string Normalize(string cssColor)
        {
            if (string.IsNullOrEmpty(cssColor))
                return cssColor;

            string trimmed = cssColor.Trim();

            // Already a hex color or other format Unity handles natively
            if (trimmed.StartsWith("#") || trimmed.StartsWith("rgb"))
                return trimmed;

            // Look up named color
            if (NameToHex.TryGetValue(trimmed, out string hex))
                return hex;

            // Return as-is (let Unity try to parse it)
            return trimmed;
        }
    }

    /// <summary>
    /// JSON converter that normalizes CSS color name strings to hex values during deserialization.
    /// Apply via [JsonConverter(typeof(ArenaColorJsonConverter))] on string color fields.
    /// </summary>
    public class ArenaColorJsonConverter : JsonConverter<string>
    {
        public override string ReadJson(JsonReader reader, Type objectType, string existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;

            if (reader.TokenType == JsonToken.String)
                return ArenaCssColors.Normalize((string)reader.Value);

            return (string)reader.Value;
        }

        public override void WriteJson(JsonWriter writer, string value, JsonSerializer serializer)
        {
            writer.WriteValue(value);
        }
    }

    /// <summary>
    /// JSON converter that normalizes an array of CSS color name strings to hex values during deserialization.
    /// Apply via [JsonConverter(typeof(ArenaColorArrayJsonConverter))] on string[] color fields.
    /// </summary>
    public class ArenaColorArrayJsonConverter : JsonConverter<string[]>
    {
        public override string[] ReadJson(JsonReader reader, Type objectType, string[] existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;

            var token = Newtonsoft.Json.Linq.JToken.Load(reader);

            if (token.Type == Newtonsoft.Json.Linq.JTokenType.Array)
            {
                var arr = (Newtonsoft.Json.Linq.JArray)token;
                var result = new string[arr.Count];
                for (int i = 0; i < arr.Count; i++)
                {
                    result[i] = ArenaCssColors.Normalize(arr[i].ToString());
                }
                return result;
            }

            // Single string → single-element array
            if (token.Type == Newtonsoft.Json.Linq.JTokenType.String)
            {
                return new[] { ArenaCssColors.Normalize(token.ToString()) };
            }

            return existingValue;
        }

        public override void WriteJson(JsonWriter writer, string[] value, JsonSerializer serializer)
        {
            writer.WriteStartArray();
            foreach (var v in value)
                writer.WriteValue(v);
            writer.WriteEndArray();
        }
    }
}
