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
    /// Particle system component for A-Frame. More properties at <a href='https://github.com/c-frame/aframe-particle-system-component'>https://github.com/c-frame/aframe-particle-system-component</a>
    /// </summary>
    [Serializable]
    public class ArenaParticleSystemJson
    {
        public const string componentName = "particle-system";

        // particle-system member-fields

        public enum PresetType
        {
            [EnumMember(Value = "default")]
            Default,
            [EnumMember(Value = "dust")]
            Dust,
            [EnumMember(Value = "snow")]
            Snow,
            [EnumMember(Value = "rain")]
            Rain,
        }
        private static PresetType defPreset = PresetType.Default;
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(PropertyName = "preset")]
        [Tooltip("Preset configuration. Possible values are: default, dust, snow, rain.")]
        public PresetType Preset = defPreset;
        public bool ShouldSerializePreset()
        {
            // preset
            return (Preset != defPreset);
        }

        private static float defMaxAge = 6f;
        [JsonProperty(PropertyName = "maxAge")]
        [Tooltip("The particle's maximum age in seconds.")]
        public float MaxAge = defMaxAge;
        public bool ShouldSerializeMaxAge()
        {
            // maxAge
            return (MaxAge != defMaxAge);
        }

        private static object defPositionSpread = JsonConvert.DeserializeObject("{x: 0, y: 0, z: 0}");
        [JsonProperty(PropertyName = "positionSpread")]
        [Tooltip("Describes this emitter's position variance on a per-particle basis.")]
        public object PositionSpread = defPositionSpread;
        public bool ShouldSerializePositionSpread()
        {
            // positionSpread
            return (PositionSpread != defPositionSpread);
        }

        private static float defType = 1f;
        [JsonProperty(PropertyName = "type")]
        [Tooltip("The default distribution this emitter should use to control its particle's spawn position and force behaviour. Possible values are 1 (box), 2 (sphere), 3 (disc)")]
        public float Type = defType;
        public bool ShouldSerializeType()
        {
            // type
            return (Type != defType);
        }

        public enum RotationAxisType
        {
            [EnumMember(Value = "x")]
            X,
            [EnumMember(Value = "y")]
            Y,
            [EnumMember(Value = "z")]
            Z,
        }
        private static RotationAxisType defRotationAxis = RotationAxisType.X;
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(PropertyName = "rotationAxis")]
        [Tooltip("Describes this emitter's axis of rotation. Possible values are x, y and z.")]
        public RotationAxisType RotationAxis = defRotationAxis;
        public bool ShouldSerializeRotationAxis()
        {
            // rotationAxis
            return (RotationAxis != defRotationAxis);
        }

        private static float defRotationAngle = 0f;
        [JsonProperty(PropertyName = "rotationAngle")]
        [Tooltip("The angle of rotation, given in radians. Dust preset is 3.14.")]
        public float RotationAngle = defRotationAngle;
        public bool ShouldSerializeRotationAngle()
        {
            // rotationAngle
            return (RotationAngle != defRotationAngle);
        }

        private static float defRotationAngleSpread = 0f;
        [JsonProperty(PropertyName = "rotationAngleSpread")]
        [Tooltip("The amount of variance in the angle of rotation per-particle, given in radians.")]
        public float RotationAngleSpread = defRotationAngleSpread;
        public bool ShouldSerializeRotationAngleSpread()
        {
            // rotationAngleSpread
            return (RotationAngleSpread != defRotationAngleSpread);
        }

        private static object defAccelerationValue = JsonConvert.DeserializeObject("{x: 0, y: -10, z: 0}");
        [JsonProperty(PropertyName = "accelerationValue")]
        [Tooltip("Describes this emitter's base acceleration.")]
        public object AccelerationValue = defAccelerationValue;
        public bool ShouldSerializeAccelerationValue()
        {
            // accelerationValue
            return (AccelerationValue != defAccelerationValue);
        }

        private static object defAccelerationSpread = JsonConvert.DeserializeObject("{x: 10, y: 0, z: 10}");
        [JsonProperty(PropertyName = "accelerationSpread")]
        [Tooltip("Describes this emitter's acceleration variance on a per-particle basis.")]
        public object AccelerationSpread = defAccelerationSpread;
        public bool ShouldSerializeAccelerationSpread()
        {
            // accelerationSpread
            return (AccelerationSpread != defAccelerationSpread);
        }

        private static object defVelocityValue = JsonConvert.DeserializeObject("{x: 0, y: 25, z: 0}");
        [JsonProperty(PropertyName = "velocityValue")]
        [Tooltip("Describes this emitter's base velocity.")]
        public object VelocityValue = defVelocityValue;
        public bool ShouldSerializeVelocityValue()
        {
            // velocityValue
            return (VelocityValue != defVelocityValue);
        }

        private static object defVelocitySpread = JsonConvert.DeserializeObject("{x: 10, y: 7.5, z: 10}");
        [JsonProperty(PropertyName = "velocitySpread")]
        [Tooltip("Describes this emitter's acceleration variance on a per-particle basis.")]
        public object VelocitySpread = defVelocitySpread;
        public bool ShouldSerializeVelocitySpread()
        {
            // velocitySpread
            return (VelocitySpread != defVelocitySpread);
        }

        private static float defDragValue = 0f;
        [JsonProperty(PropertyName = "dragValue")]
        [Tooltip("Number between 0 and 1 describing drag applied to all particles.")]
        public float DragValue = defDragValue;
        public bool ShouldSerializeDragValue()
        {
            // dragValue
            return (DragValue != defDragValue);
        }

        private static float defDragSpread = 0f;
        [JsonProperty(PropertyName = "dragSpread")]
        [Tooltip("Number describing drag variance on a per-particle basis.")]
        public float DragSpread = defDragSpread;
        public bool ShouldSerializeDragSpread()
        {
            // dragSpread
            return (DragSpread != defDragSpread);
        }

        private static bool defDragRandomise = false;
        [JsonProperty(PropertyName = "dragRandomise")]
        [Tooltip("WHen a particle is re-spawned, whether it's drag should be re-randomised or not. Can incur a performance hit.")]
        public bool DragRandomise = defDragRandomise;
        public bool ShouldSerializeDragRandomise()
        {
            // dragRandomise
            return (DragRandomise != defDragRandomise);
        }

        private static string[] defColor = {"#0000FF", "#FF0000"};
        [JsonProperty(PropertyName = "color")]
        [Tooltip("Describes a particle's color. This property is a 'value-over-lifetime' property, meaning an array of values can be given to describe specific value changes over a particle's lifetime.")]
        public string[] Color = defColor;
        public bool ShouldSerializeColor()
        {
            // color
            return (Color != defColor);
        }

        private static float[] defSize = {1};
        [JsonProperty(PropertyName = "size")]
        [Tooltip("Describes a particle's size.")]
        public float[] Size = defSize;
        public bool ShouldSerializeSize()
        {
            return true; // required in json schema
        }

        private static float[] defSizeSpread = {0};
        [JsonProperty(PropertyName = "sizeSpread")]
        [Tooltip("sizeSpread")]
        public float[] SizeSpread = defSizeSpread;
        public bool ShouldSerializeSizeSpread()
        {
            // sizeSpread
            return (SizeSpread != defSizeSpread);
        }

        private static float defDirection = 1f;
        [JsonProperty(PropertyName = "direction")]
        [Tooltip("The direction of the emitter. If value is 1, emitter will start at beginning of particle's lifecycle. If value is -1, emitter will start at end of particle's lifecycle and work it's way backwards.")]
        public float Direction = defDirection;
        public bool ShouldSerializeDirection()
        {
            // direction
            return (Direction != defDirection);
        }

        private static float defDuration = 0f;
        [JsonProperty(PropertyName = "duration")]
        [Tooltip("The duration in seconds that this emitter should live for. If not specified, the emitter will emit particles indefinitely.")]
        public float Duration = defDuration;
        public bool ShouldSerializeDuration()
        {
            // duration
            return (Duration != defDuration);
        }

        private static bool defEnabled = true;
        [JsonProperty(PropertyName = "enabled")]
        [Tooltip("When true the emitter will emit particles, when false it will not. This value can be changed dynamically during a scene. While particles are emitting, they will disappear immediately when set to false.")]
        public bool Enabled = defEnabled;
        public bool ShouldSerializeEnabled()
        {
            // enabled
            return (Enabled != defEnabled);
        }

        private static float defParticleCount = 1000f;
        [JsonProperty(PropertyName = "particleCount")]
        [Tooltip("The total number of particles this emitter will hold. NOTE: this is not the number of particles emitted in a second, or anything like that. The number of particles emitted per-second is calculated by particleCount ")]
        public float ParticleCount = defParticleCount;
        public bool ShouldSerializeParticleCount()
        {
            // particleCount
            return (ParticleCount != defParticleCount);
        }

        private static string defTexture = "static/images/textures/star2.png";
        [JsonProperty(PropertyName = "texture")]
        [Tooltip("The texture used by this emitter. Examples: [star2.png, smokeparticle.png, raindrop.png], like path 'static/images/textures/star2.png'")]
        public string Texture = defTexture;
        public bool ShouldSerializeTexture()
        {
            return true; // required in json schema
        }

        private static bool defRandomise = false;
        [JsonProperty(PropertyName = "randomise")]
        [Tooltip("When a particle is re-spawned, whether it's position should be re-randomised or not. Can incur a performance hit.")]
        public bool Randomise = defRandomise;
        public bool ShouldSerializeRandomise()
        {
            // randomise
            return (Randomise != defRandomise);
        }

        private static float[] defOpacity = {1};
        [JsonProperty(PropertyName = "opacity")]
        [Tooltip("Either a single number to describe the opacity of a particle.")]
        public float[] Opacity = defOpacity;
        public bool ShouldSerializeOpacity()
        {
            // opacity
            return (Opacity != defOpacity);
        }

        private static float[] defOpacitySpread = {1};
        [JsonProperty(PropertyName = "opacitySpread")]
        [Tooltip("opacitySpread")]
        public float[] OpacitySpread = defOpacitySpread;
        public bool ShouldSerializeOpacitySpread()
        {
            // opacitySpread
            return (OpacitySpread != defOpacitySpread);
        }

        public enum BlendingType
        {
            [EnumMember(Value = "0")]
            Zero,
            [EnumMember(Value = "1")]
            One,
            [EnumMember(Value = "2")]
            Two,
            [EnumMember(Value = "3")]
            Three,
            [EnumMember(Value = "4")]
            Four,
        }
        private static BlendingType defBlending = BlendingType.Two;
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(PropertyName = "blending")]
        [Tooltip("The blending mode of the particles. Possible values are 0 (no blending), 1 (normal), 2 (additive), 3 (subtractive), 4 (multiply)")]
        public BlendingType Blending = defBlending;
        public bool ShouldSerializeBlending()
        {
            // blending
            return (Blending != defBlending);
        }

        private static float defMaxParticleCount = 250000f;
        [JsonProperty(PropertyName = "maxParticleCount")]
        [Tooltip("maxParticleCount")]
        public float MaxParticleCount = defMaxParticleCount;
        public bool ShouldSerializeMaxParticleCount()
        {
            // maxParticleCount
            return (MaxParticleCount != defMaxParticleCount);
        }

        // General json object management

        [JsonExtensionData]
        private IDictionary<string, JToken> _additionalData;

        public string SaveToString()
        {
            return Regex.Unescape(JsonConvert.SerializeObject(this));
        }

        public static ArenaParticleSystemJson CreateFromJSON(string jsonString, JToken token)
        {
            ArenaParticleSystemJson json = null;
            try {
                json = JsonConvert.DeserializeObject<ArenaParticleSystemJson>(Regex.Unescape(jsonString));
            } catch (JsonReaderException e)
            {
                Debug.LogWarning($"{e.Message}: {jsonString}");
            }
            return json;
        }
    }
}
