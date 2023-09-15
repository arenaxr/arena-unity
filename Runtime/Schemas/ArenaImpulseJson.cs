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
    /// The force applied using physics. Requires click-listener
    /// </summary>
    [Serializable]
    public class ArenaImpulseJson
    {
        public const string componentName = "impulse";

        // impulse member-fields

        private static string defForce = "";
        [JsonProperty(PropertyName = "force")]
        [Tooltip("force")]
        public string Force = defForce;
        public bool ShouldSerializeForce()
        {
            // force
            return (Force != defForce);
        }

        private static string defOn = "";
        [JsonProperty(PropertyName = "on")]
        [Tooltip("on")]
        public string On = defOn;
        public bool ShouldSerializeOn()
        {
            // on
            return (On != defOn);
        }

        private static string defPosition = "";
        [JsonProperty(PropertyName = "position")]
        [Tooltip("position")]
        public string Position = defPosition;
        public bool ShouldSerializePosition()
        {
            // position
            return (Position != defPosition);
        }

        // General json object management

        [JsonExtensionData]
        private IDictionary<string, JToken> _additionalData;

        public string SaveToString()
        {
            return Regex.Unescape(JsonConvert.SerializeObject(this));
        }

        public static ArenaImpulseJson CreateFromJSON(string jsonString, JToken token)
        {
            ArenaImpulseJson json = null;
            try {
                json = JsonConvert.DeserializeObject<ArenaImpulseJson>(Regex.Unescape(jsonString));
            } catch (JsonReaderException e)
            {
                Debug.LogWarning($"{e.Message}: {jsonString}");
            }
            return json;
        }
    }
}
