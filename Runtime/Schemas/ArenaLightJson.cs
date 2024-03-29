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
using Newtonsoft.Json.Serialization;
using UnityEngine;

namespace ArenaUnity.Schemas
{
    /// <summary>
    /// A light. More properties at <a href='https://aframe.io/docs/1.4.0/components/light.html'>https://aframe.io/docs/1.4.0/components/light.html</a>
    /// </summary>
    [Serializable]
    public class ArenaLightJson
    {
        public readonly string object_type = "light";

        // light member-fields

        private static float defAngle = 60f;
        [JsonProperty(PropertyName = "angle")]
        [Tooltip("Maximum extent of spot light from its direction (in degrees). NOTE: Spot light type only.")]
        public float Angle = defAngle;
        public bool ShouldSerializeAngle()
        {
            // angle
            return (Angle != defAngle);
        }

        private static bool defCastShadow = false;
        [JsonProperty(PropertyName = "castShadow")]
        [Tooltip("castShadow (point, spot, directional)")]
        public bool CastShadow = defCastShadow;
        public bool ShouldSerializeCastShadow()
        {
            // castShadow
            return (CastShadow != defCastShadow);
        }

        private static string defColor = "#ffffff";
        [JsonProperty(PropertyName = "color")]
        [Tooltip("Light color.")]
        public string Color = defColor;
        public bool ShouldSerializeColor()
        {
            // color
            return (Color != defColor);
        }

        private static float defDecay = 1.0f;
        [JsonProperty(PropertyName = "decay")]
        [Tooltip("Amount the light dims along the distance of the light. NOTE: Point and Spot light type only.")]
        public float Decay = defDecay;
        public bool ShouldSerializeDecay()
        {
            // decay
            return (Decay != defDecay);
        }

        private static float defDistance = 0.0f;
        [JsonProperty(PropertyName = "distance")]
        [Tooltip("Distance where intensity becomes 0. If distance is 0, then the point light does not decay with distance. NOTE: Point and Spot light type only.")]
        public float Distance = defDistance;
        public bool ShouldSerializeDistance()
        {
            // distance
            return (Distance != defDistance);
        }

        private static string defGroundColor = "#ffffff";
        [JsonProperty(PropertyName = "groundColor")]
        [Tooltip("Light color from below. NOTE: Hemisphere light type only")]
        public string GroundColor = defGroundColor;
        public bool ShouldSerializeGroundColor()
        {
            // groundColor
            return (GroundColor != defGroundColor);
        }

        private static float defIntensity = 1f;
        [JsonProperty(PropertyName = "intensity")]
        [Tooltip("Light strength.")]
        public float Intensity = defIntensity;
        public bool ShouldSerializeIntensity()
        {
            // intensity
            return (Intensity != defIntensity);
        }

        private static float defPenumbra = 0.0f;
        [JsonProperty(PropertyName = "penumbra")]
        [Tooltip("Percent of the spotlight cone that is attenuated due to penumbra. NOTE: Spot light type only.")]
        public float Penumbra = defPenumbra;
        public bool ShouldSerializePenumbra()
        {
            // penumbra
            return (Penumbra != defPenumbra);
        }

        private static float defShadowBias = 0f;
        [JsonProperty(PropertyName = "shadowBias")]
        [Tooltip("shadowBias (castShadow=true)")]
        public float ShadowBias = defShadowBias;
        public bool ShouldSerializeShadowBias()
        {
            // shadowBias
            return (ShadowBias != defShadowBias);
        }

        private static float defShadowCameraBottom = -5f;
        [JsonProperty(PropertyName = "shadowCameraBottom")]
        [Tooltip("shadowCameraBottom (castShadow=true)")]
        public float ShadowCameraBottom = defShadowCameraBottom;
        public bool ShouldSerializeShadowCameraBottom()
        {
            // shadowCameraBottom
            return (ShadowCameraBottom != defShadowCameraBottom);
        }

        private static float defShadowCameraFar = 500f;
        [JsonProperty(PropertyName = "shadowCameraFar")]
        [Tooltip("shadowCameraFar (castShadow=true)")]
        public float ShadowCameraFar = defShadowCameraFar;
        public bool ShouldSerializeShadowCameraFar()
        {
            // shadowCameraFar
            return (ShadowCameraFar != defShadowCameraFar);
        }

        private static float defShadowCameraFov = 90f;
        [JsonProperty(PropertyName = "shadowCameraFov")]
        [Tooltip("shadowCameraFov (castShadow=true)")]
        public float ShadowCameraFov = defShadowCameraFov;
        public bool ShouldSerializeShadowCameraFov()
        {
            // shadowCameraFov
            return (ShadowCameraFov != defShadowCameraFov);
        }

        private static float defShadowCameraLeft = -5f;
        [JsonProperty(PropertyName = "shadowCameraLeft")]
        [Tooltip("shadowCameraBottom (castShadow=true)")]
        public float ShadowCameraLeft = defShadowCameraLeft;
        public bool ShouldSerializeShadowCameraLeft()
        {
            // shadowCameraLeft
            return (ShadowCameraLeft != defShadowCameraLeft);
        }

        private static float defShadowCameraNear = 0.5f;
        [JsonProperty(PropertyName = "shadowCameraNear")]
        [Tooltip("shadowCameraNear (castShadow=true)")]
        public float ShadowCameraNear = defShadowCameraNear;
        public bool ShouldSerializeShadowCameraNear()
        {
            // shadowCameraNear
            return (ShadowCameraNear != defShadowCameraNear);
        }

        private static float defShadowCameraRight = 5f;
        [JsonProperty(PropertyName = "shadowCameraRight")]
        [Tooltip("shadowCameraRight (castShadow=true)")]
        public float ShadowCameraRight = defShadowCameraRight;
        public bool ShouldSerializeShadowCameraRight()
        {
            // shadowCameraRight
            return (ShadowCameraRight != defShadowCameraRight);
        }

        private static float defShadowCameraTop = 5f;
        [JsonProperty(PropertyName = "shadowCameraTop")]
        [Tooltip("shadowCameraTop (castShadow=true)")]
        public float ShadowCameraTop = defShadowCameraTop;
        public bool ShouldSerializeShadowCameraTop()
        {
            // shadowCameraTop
            return (ShadowCameraTop != defShadowCameraTop);
        }

        private static bool defShadowCameraVisible = false;
        [JsonProperty(PropertyName = "shadowCameraVisible")]
        [Tooltip("shadowCameraVisible (castShadow=true)")]
        public bool ShadowCameraVisible = defShadowCameraVisible;
        public bool ShouldSerializeShadowCameraVisible()
        {
            // shadowCameraVisible
            return (ShadowCameraVisible != defShadowCameraVisible);
        }

        private static float defShadowMapHeight = 512f;
        [JsonProperty(PropertyName = "shadowMapHeight")]
        [Tooltip("shadowMapHeight (castShadow=true)")]
        public float ShadowMapHeight = defShadowMapHeight;
        public bool ShouldSerializeShadowMapHeight()
        {
            // shadowMapHeight
            return (ShadowMapHeight != defShadowMapHeight);
        }

        private static float defShadowMapWidth = 512f;
        [JsonProperty(PropertyName = "shadowMapWidth")]
        [Tooltip("shadowMapWidth (castShadow=true)")]
        public float ShadowMapWidth = defShadowMapWidth;
        public bool ShouldSerializeShadowMapWidth()
        {
            // shadowMapWidth
            return (ShadowMapWidth != defShadowMapWidth);
        }

        private static float defShadowRadius = 1f;
        [JsonProperty(PropertyName = "shadowRadius")]
        [Tooltip("shadowRadius (castShadow=true)")]
        public float ShadowRadius = defShadowRadius;
        public bool ShouldSerializeShadowRadius()
        {
            // shadowRadius
            return (ShadowRadius != defShadowRadius);
        }

        private static string defTarget = "None";
        [JsonProperty(PropertyName = "target")]
        [Tooltip("Id of element the spot should point to. set to null to transform spotlight by orientation, pointing to it’s -Z axis. NOTE: Spot light type only.")]
        public string Target = defTarget;
        public bool ShouldSerializeTarget()
        {
            // target
            return (Target != defTarget);
        }

        public enum TypeType
        {
            [EnumMember(Value = "ambient")]
            Ambient,
            [EnumMember(Value = "directional")]
            Directional,
            [EnumMember(Value = "hemisphere")]
            Hemisphere,
            [EnumMember(Value = "point")]
            Point,
            [EnumMember(Value = "spot")]
            Spot,
        }
        private static TypeType defType = TypeType.Directional;
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(PropertyName = "type")]
        [Tooltip("One of ambient, directional, hemisphere, point, spot.")]
        public TypeType Type = defType;
        public bool ShouldSerializeType()
        {
            // type
            return (Type != defType);
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
