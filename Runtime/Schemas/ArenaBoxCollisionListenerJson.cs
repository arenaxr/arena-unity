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
    /// Listen for bounding-box collisions with user camera and hands. Must be applied to an object or model with geometric mesh. Collisions are determined by course bounding-box overlaps
    /// </summary>
    [Serializable]
    public class ArenaBoxCollisionListenerJson
    {
        public const string componentName = "box-collision-listener";

        // box-collision-listener member-fields

        private static bool defEnabled = true;
        [JsonProperty(PropertyName = "enabled")]
        [Tooltip("Publish detections, set `false` to disable")]
        public bool Enabled = defEnabled;
        public bool ShouldSerializeEnabled()
        {
            if (_token != null && _token.SelectToken("enabled") != null) return true;
            return (Enabled != defEnabled);
        }

        private static bool defDynamic = false;
        [JsonProperty(PropertyName = "dynamic")]
        [Tooltip("Set true for a moving object, which should have its bounding box recalculated regularly to determine proper collision")]
        public bool Dynamic = defDynamic;
        public bool ShouldSerializeDynamic()
        {
            return true; // required in json schema 
        }

        // General json object management

        [JsonExtensionData]
        private IDictionary<string, JToken> _additionalData;

        private static JToken _token;

        public string SaveToString()
        {
            return Regex.Unescape(JsonConvert.SerializeObject(this));
        }

        public static ArenaBoxCollisionListenerJson CreateFromJSON(string jsonString, JToken token)
        {
            _token = token; // save updated wire json
            ArenaBoxCollisionListenerJson json = null;
            try {
                json = JsonConvert.DeserializeObject<ArenaBoxCollisionListenerJson>(Regex.Unescape(jsonString));
            } catch (JsonReaderException e)
            {
                Debug.LogWarning($"{e.Message}: {jsonString}");
            }
            return json;
        }
    }
}
