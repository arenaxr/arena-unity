/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2024, Carnegie Mellon University. All rights reserved.
 */

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ArenaUnity.Schemas.Converter
{
    /// <summary>
    /// JSON converter for string fields that may receive object values.
    /// When an object is received, it is serialized to a JSON string.
    /// When a string, number, or boolean is received, it is converted to string.
    /// Used for animation "to"/"from" fields which accept both "1 2 3" and {"x":1,"y":2,"z":3}.
    /// </summary>
    public class ArenaStringObjectJsonConverter : JsonConverter<string>
    {
        public override string ReadJson(JsonReader reader, Type objectType, string existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;

            if (reader.TokenType == JsonToken.String)
                return (string)reader.Value;

            if (reader.TokenType == JsonToken.StartObject || reader.TokenType == JsonToken.StartArray)
            {
                // Serialize the object/array to a compact JSON string
                JToken token = JToken.Load(reader);
                return token.ToString(Formatting.None);
            }

            // For numbers, booleans, etc. — convert to string
            return reader.Value?.ToString();
        }

        public override void WriteJson(JsonWriter writer, string value, JsonSerializer serializer)
        {
            writer.WriteValue(value);
        }
    }
}
