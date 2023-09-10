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
    /// Tetrahedron Geometry
    /// </summary>
    [Serializable]
    public class ArenaTetrahedronJson
    {
        public const string componentName = "tetrahedron";

        // tetrahedron member-fields

        private static float defDetail = 0f;
        [JsonProperty(PropertyName = "detail")]
        [Tooltip("detail")]
        public float Detail = defDetail;
        public bool ShouldSerializeDetail()
        {
            if (_token != null && _token.SelectToken("detail") != null) return true;
            return (Detail != defDetail);
        }

        private static float defRadius = 1f;
        [JsonProperty(PropertyName = "radius")]
        [Tooltip("radius")]
        public float Radius = defRadius;
        public bool ShouldSerializeRadius()
        {
            return true; // required in json schema 
        }

        private static string defCollisionListener = "";
        [JsonProperty(PropertyName = "collision-listener")]
        [Tooltip("Name of the collision-listener, default can be empty string. Collisions trigger click events")]
        public string CollisionListener = defCollisionListener;
        public bool ShouldSerializeCollisionListener()
        {
            if (_token != null && _token.SelectToken("collision-listener") != null) return true;
            return (CollisionListener != defCollisionListener);
        }

        private static bool defHideOnEnterAr = true;
        [JsonProperty(PropertyName = "hide-on-enter-ar")]
        [Tooltip("Hide object when entering AR. Remove component to *not* hide")]
        public bool HideOnEnterAr = defHideOnEnterAr;
        public bool ShouldSerializeHideOnEnterAr()
        {
            if (_token != null && _token.SelectToken("hide-on-enter-ar") != null) return true;
            return (HideOnEnterAr != defHideOnEnterAr);
        }

        private static bool defHideOnEnterVr = true;
        [JsonProperty(PropertyName = "hide-on-enter-vr")]
        [Tooltip("Hide object when entering VR. Remove component to *not* hide")]
        public bool HideOnEnterVr = defHideOnEnterVr;
        public bool ShouldSerializeHideOnEnterVr()
        {
            if (_token != null && _token.SelectToken("hide-on-enter-vr") != null) return true;
            return (HideOnEnterVr != defHideOnEnterVr);
        }

        private static bool defShowOnEnterAr = true;
        [JsonProperty(PropertyName = "show-on-enter-ar")]
        [Tooltip("Show object when entering AR. Hidden otherwise")]
        public bool ShowOnEnterAr = defShowOnEnterAr;
        public bool ShouldSerializeShowOnEnterAr()
        {
            if (_token != null && _token.SelectToken("show-on-enter-ar") != null) return true;
            return (ShowOnEnterAr != defShowOnEnterAr);
        }

        private static bool defShowOnEnterVr = true;
        [JsonProperty(PropertyName = "show-on-enter-vr")]
        [Tooltip("Show object when entering VR. Hidden otherwise")]
        public bool ShowOnEnterVr = defShowOnEnterVr;
        public bool ShouldSerializeShowOnEnterVr()
        {
            if (_token != null && _token.SelectToken("show-on-enter-vr") != null) return true;
            return (ShowOnEnterVr != defShowOnEnterVr);
        }

        private static string defUrl = "";
        [JsonProperty(PropertyName = "url")]
        [Tooltip("Model URL. Store files paths under 'store/users/<username>' (e.g. store/users/wiselab/models/factory_robot_arm/scene.gltf); to use CDN, prefix with `https://arena-cdn.conix.io/` (e.g. https://arena-cdn.conix.io/store/users/wiselab/models/factory_robot_arm/scene.gltf)")]
        public string Url = defUrl;
        public bool ShouldSerializeUrl()
        {
            if (_token != null && _token.SelectToken("url") != null) return true;
            return (Url != defUrl);
        }

        private static bool defScreenshareable = true;
        [JsonProperty(PropertyName = "screenshareable")]
        [Tooltip("Whether or not a user can screenshare on an object")]
        public bool Screenshareable = defScreenshareable;
        public bool ShouldSerializeScreenshareable()
        {
            if (_token != null && _token.SelectToken("screenshareable") != null) return true;
            return (Screenshareable != defScreenshareable);
        }

        private static bool defBuffer = true;
        [JsonProperty(PropertyName = "buffer")]
        [Tooltip("Transform geometry into a BufferGeometry to reduce memory usage at the cost of being harder to manipulate (geometries only: box, circle, cone, ...).")]
        public bool Buffer = defBuffer;
        public bool ShouldSerializeBuffer()
        {
            if (_token != null && _token.SelectToken("buffer") != null) return true;
            return (Buffer != defBuffer);
        }

        private static bool defSkipCache = true;
        [JsonProperty(PropertyName = "skipCache")]
        [Tooltip("Disable retrieving the shared geometry object from the cache. (geometries only: box, circle, cone, ...).")]
        public bool SkipCache = defSkipCache;
        public bool ShouldSerializeSkipCache()
        {
            if (_token != null && _token.SelectToken("skipCache") != null) return true;
            return (SkipCache != defSkipCache);
        }

        // General json object management

        [JsonExtensionData]
        private IDictionary<string, JToken> _additionalData;

        private static JToken _token;

        public string SaveToString()
        {
            return Regex.Unescape(JsonConvert.SerializeObject(this));
        }

        public static ArenaTetrahedronJson CreateFromJSON(string jsonString, JToken token)
        {
            _token = token; // save updated wire json
            ArenaTetrahedronJson json = null;
            try {
                json = JsonConvert.DeserializeObject<ArenaTetrahedronJson>(Regex.Unescape(jsonString));
            } catch (JsonReaderException e)
            {
                Debug.LogWarning($"{e.Message}: {jsonString}");
            }
            return json;
        }
    }
}
