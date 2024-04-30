/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2024, Carnegie Mellon University. All rights reserved.
 */

// CAUTION: This file is autogenerated from https://github.com/arenaxr/arena-schemas. Changes made here may be overwritten.

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
    /// <summary>
    /// Allows you to target and control a gltf model's morphTargets created in Blender. More properties at <a href='https://github.com/elbobo/aframe-gltf-morph-component'>A-Frame GLTF Morph</a> component.
    /// </summary>
    [Serializable]
    public class ArenaGltfMorphJson
    {
        [JsonIgnore]
        public readonly string componentName = "gltf-morph";

        // gltf-morph member-fields

        private static string defMorphtarget = "";
        [JsonProperty(PropertyName = "morphtarget")]
        [Tooltip("Name of morphTarget, can be found as part of the GLTF model.")]
        public string Morphtarget = defMorphtarget;
        public bool ShouldSerializeMorphtarget()
        {
            return true; // required in json schema
        }

        private static float defValue = 0f;
        [JsonProperty(PropertyName = "value")]
        [Tooltip("Value that you want to set that morphTarget to (0 - 1).")]
        public float Value = defValue;
        public bool ShouldSerializeValue()
        {
            return true; // required in json schema
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
