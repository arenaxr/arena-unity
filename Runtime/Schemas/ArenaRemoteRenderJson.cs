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
    /// Whether or not an object should be remote rendered [Experimental]
    /// </summary>
    [Serializable]
    public class ArenaRemoteRenderJson
    {
        public const string componentName = "remote-render";

        // remote-render member-fields

        private static bool defEnabled = true;
        [JsonProperty(PropertyName = "enabled")]
        [Tooltip("Remote Render this object")]
        public bool Enabled = defEnabled;
        public bool ShouldSerializeEnabled()
        {
            if (_token != null && _token.SelectToken("enabled") != null) return true;
            return (Enabled != defEnabled);
        }

        // General json object management

        [JsonExtensionData]
        private IDictionary<string, JToken> _additionalData;

        private static JToken _token;

        public string SaveToString()
        {
            return Regex.Unescape(JsonConvert.SerializeObject(this));
        }

        public static ArenaRemoteRenderJson CreateFromJSON(string jsonString, JToken token)
        {
            _token = token; // save updated wire json
            ArenaRemoteRenderJson json = null;
            try {
                json = JsonConvert.DeserializeObject<ArenaRemoteRenderJson>(Regex.Unescape(jsonString));
            } catch (JsonReaderException e)
            {
                Debug.LogWarning($"{e.Message}: {jsonString}");
            }
            return json;
        }
    }
}