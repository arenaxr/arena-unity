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
    //TODO (mwfarb): move ArenaHandJson to automated schema when the schema translator is updated
    [Serializable]
    public class ArenaHandJson
    {
        public string url { get; set; }
        public string dep { get; set; }

        [JsonProperty(PropertyName = "parent")]
        [Tooltip("Parent's object_id. Child objects inherit attributes of their parent, for example scale and translation.")]
        public string parent = null;
        public bool ShouldSerializeparent()
        {
            return (parent != null);
        }

        [JsonProperty(PropertyName = "position")]
        [Tooltip("3D object position")]
        public ArenaVector3Json position = null;
        public bool ShouldSerializeposition()
        {
            return (position != null);
        }

        [JsonProperty(PropertyName = "rotation")]
        [Tooltip("3D object rotation in quaternion representation; Right-handed coordinate system. Euler degrees are deprecated in wire message format.")]
        public ArenaRotationJson rotation = null;
        public bool ShouldSerializerotation()
        {
            return (rotation != null);
        }

        [JsonProperty(PropertyName = "scale")]
        [Tooltip("3D object scale")]
        public ArenaVector3Json scale = null;
        public bool ShouldSerializescale()
        {
            return (scale != null);
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
