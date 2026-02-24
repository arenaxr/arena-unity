/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2024, Carnegie Mellon University. All rights reserved.
 */

using System;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ArenaUnity.Schemas.Converter
{
    /// <summary>
    /// Custom JSON converter for ArenaVector3Json that handles both object {"x":1,"y":2,"z":3}
    /// and string representations like "3" (→ {3,3,3}) or "1 4.4 0" (→ {1, 4.4, 0}).
    /// Mirrors A-Frame's AFRAME.utils.coordinates.parse behavior.
    /// </summary>
    public class ArenaVector3JsonConverter : JsonConverter<ArenaVector3Json>
    {
        public override ArenaVector3Json ReadJson(JsonReader reader, Type objectType, ArenaVector3Json existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;

            if (reader.TokenType == JsonToken.String)
            {
                string str = (string)reader.Value;
                return ParseVector3String(str);
            }

            // For object tokens, use default deserialization
            JObject obj = JObject.Load(reader);
            var result = new ArenaVector3Json();
            if (obj["x"] != null) result.X = obj["x"].Value<float>();
            if (obj["y"] != null) result.Y = obj["y"].Value<float>();
            if (obj["z"] != null) result.Z = obj["z"].Value<float>();
            return result;
        }

        public override void WriteJson(JsonWriter writer, ArenaVector3Json value, JsonSerializer serializer)
        {
            // Always serialize as object
            writer.WriteStartObject();
            writer.WritePropertyName("x");
            writer.WriteValue(value.X);
            writer.WritePropertyName("y");
            writer.WriteValue(value.Y);
            writer.WritePropertyName("z");
            writer.WriteValue(value.Z);
            writer.WriteEndObject();
        }

        private static ArenaVector3Json ParseVector3String(string str)
        {
            var result = new ArenaVector3Json();
            str = str.Trim();

            // Try splitting by space (A-Frame coordinate format: "1 4.4 0")
            string[] parts = str.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 1)
            {
                // Single number → fill all components: "3" → {3, 3, 3}
                if (float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out float val))
                {
                    result.X = val;
                    result.Y = val;
                    result.Z = val;
                }
            }
            else if (parts.Length == 2)
            {
                // Two numbers → x, y, z=0: "1 2" → {1, 2, 0}
                if (float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out float x))
                    result.X = x;
                if (float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float y))
                    result.Y = y;
            }
            else if (parts.Length >= 3)
            {
                // Three numbers: "1 4.4 0" → {1, 4.4, 0}
                if (float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out float x))
                    result.X = x;
                if (float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float y))
                    result.Y = y;
                if (float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out float z))
                    result.Z = z;
            }

            return result;
        }
    }

    /// <summary>
    /// Custom JSON converter for ArenaVector2Json that handles both object {"x":1,"y":2}
    /// and string representations like "3" (→ {3,3}) or "1 4.4" (→ {1, 4.4}).
    /// </summary>
    public class ArenaVector2JsonConverter : JsonConverter<ArenaVector2Json>
    {
        public override ArenaVector2Json ReadJson(JsonReader reader, Type objectType, ArenaVector2Json existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;

            if (reader.TokenType == JsonToken.String)
            {
                string str = (string)reader.Value;
                return ParseVector2String(str);
            }

            // For object tokens, use default deserialization
            JObject obj = JObject.Load(reader);
            var result = new ArenaVector2Json();
            if (obj["x"] != null) result.X = obj["x"].Value<float>();
            if (obj["y"] != null) result.Y = obj["y"].Value<float>();
            return result;
        }

        public override void WriteJson(JsonWriter writer, ArenaVector2Json value, JsonSerializer serializer)
        {
            // Always serialize as object
            writer.WriteStartObject();
            writer.WritePropertyName("x");
            writer.WriteValue(value.X);
            writer.WritePropertyName("y");
            writer.WriteValue(value.Y);
            writer.WriteEndObject();
        }

        private static ArenaVector2Json ParseVector2String(string str)
        {
            var result = new ArenaVector2Json();
            str = str.Trim();

            string[] parts = str.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 1)
            {
                // Single number → fill all components: "3" → {3, 3}
                if (float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out float val))
                {
                    result.X = val;
                    result.Y = val;
                }
            }
            else if (parts.Length >= 2)
            {
                // Two numbers: "1 4.4" → {1, 4.4}
                if (float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out float x))
                    result.X = x;
                if (float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float y))
                    result.Y = y;
            }

            return result;
        }
    }
}
