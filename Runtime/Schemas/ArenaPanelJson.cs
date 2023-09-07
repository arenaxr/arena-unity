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
    /// The rounded UI panel primitive.
    /// </summary>
    [Serializable]
    public class ArenaPanelJson
    {
        public const string componentName = "panel";

        // panel member-fields

        private static float defDepth = 0.05f;
        [JsonProperty(PropertyName = "depth")]
        [Tooltip("depth")]
        public float Depth = defDepth;
        public bool ShouldSerializeDepth()
        {
            if (_token != null && _token.SelectToken("depth") != null) return true;
            return (Depth != defDepth);
        }

        private static float defHeight = 1f;
        [JsonProperty(PropertyName = "height")]
        [Tooltip("height")]
        public float Height = defHeight;
        public bool ShouldSerializeHeight()
        {
            if (_token != null && _token.SelectToken("height") != null) return true;
            return (Height != defHeight);
        }

        private static float defWidth = 1f;
        [JsonProperty(PropertyName = "width")]
        [Tooltip("width")]
        public float Width = defWidth;
        public bool ShouldSerializeWidth()
        {
            if (_token != null && _token.SelectToken("width") != null) return true;
            return (Width != defWidth);
        }

        // General json object management

        [JsonExtensionData]
        private IDictionary<string, JToken> _additionalData;

        private static JToken _token;

        public string SaveToString()
        {
            return Regex.Unescape(JsonConvert.SerializeObject(this));
        }

        public static ArenaPanelJson CreateFromJSON(string jsonString, JToken token)
        {
            _token = token; // save updated wire json
            ArenaPanelJson json = null;
            try {
                json = JsonConvert.DeserializeObject<ArenaPanelJson>(Regex.Unescape(jsonString));
            } catch (JsonReaderException e)
            {
                Debug.LogWarning($"{e.Message}: {jsonString}");
            }
            return json;
        }
    }
}
