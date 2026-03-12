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
using Newtonsoft.Json.Serialization;
using UnityEngine;

namespace ArenaUnity.Schemas
{
    [Serializable]
    public class ArenaMessageJson
    {
        public string object_id = null;
        public bool ShouldSerializeobject_id()
        {
            return (object_id != null);
        }
        public bool? persist = null;
        public bool ShouldSerializepersist()
        {
            return (persist != null);
        }
        public string type = null;
        public bool ShouldSerializetype()
        {
            return (type != null);
        }
        public string action = null;
        public bool ShouldSerializeaction()
        {
            return (action != null);
        }
        public float? ttl = null;
        public bool ShouldSerializettl()
        {
            return (ttl != null);
        }
        public bool? overwrite = null;
        public bool ShouldSerializeoverwrite()
        {
            return (overwrite != null);
        }
        public string timestamp = null;
        public bool ShouldSerializetimestamp()
        {
            return (timestamp != null);
        }

        // Incoming JSON may use "data" or "attributes" for this payload; we normalize to "data".
        public object data = null;
        public bool ShouldSerializedata()
        {
            return (data != null);
        }

        [OnDeserialized]
        internal void OnDeserialized(StreamingContext context)
        {
            // If the JSON used "attributes" instead of "data", move it into data.
            if (data == null && _additionalData != null && _additionalData.TryGetValue("attributes", out JToken attrs))
            {
                data = attrs;
                _additionalData.Remove("attributes");
            }
        }

        // General json object management
        [OnError]
        internal void OnError(StreamingContext context, ErrorContext errorContext)
        {
            Debug.LogWarning($"{errorContext.Error.Message}: {errorContext.OriginalObject}");
            errorContext.Handled = true;
        }

        [JsonExtensionData]
        private IDictionary<string, JToken> _additionalData;
    }
}
