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
    /// Generic JSON converter that gracefully handles component properties as:
    /// - boolean (true → new T() with defaults, false → null)
    /// - string (any non-null string → new T() with defaults)
    /// - object (normal deserialization of T)
    /// Use on any component property that may receive legacy boolean/string values
    /// instead of the expected object. Example usage:
    ///   [JsonConverter(typeof(ArenaBooleanObjectJsonConverter&lt;ArenaClickListenerJson&gt;))]
    /// </summary>
    public class ArenaBooleanObjectJsonConverter<T> : JsonConverter<T> where T : class, new()
    {
        public override T ReadJson(JsonReader reader, Type objectType, T existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;

            if (reader.TokenType == JsonToken.Boolean)
            {
                bool val = (bool)reader.Value;
                return val ? new T() : null;
            }

            if (reader.TokenType == JsonToken.String)
            {
                // Any non-null string equates to a default-enabled component
                return new T();
            }

            if (reader.TokenType == JsonToken.StartObject)
            {
                JObject obj = JObject.Load(reader);
                return obj.ToObject<T>(
                    JsonSerializer.CreateDefault(new JsonSerializerSettings
                    {
                        // Avoid infinite recursion by not using this converter
                        Converters = { }
                    })
                );
            }

            return existingValue;
        }

        public override void WriteJson(JsonWriter writer, T value, JsonSerializer serializer)
        {
            // Always serialize as the full object
            JObject obj = JObject.FromObject(value, JsonSerializer.CreateDefault(new JsonSerializerSettings
            {
                Converters = { }
            }));
            obj.WriteTo(writer);
        }
    }
}
