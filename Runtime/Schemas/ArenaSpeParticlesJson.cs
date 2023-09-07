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
    /// GPU based particle systems in A-Frame. More properties at <a href='https://github.com/harlyq/aframe-spe-particles-component'>https://github.com/harlyq/aframe-spe-particles-component</a>
    /// </summary>
    [Serializable]
    public class ArenaSpeParticlesJson
    {
        public const string componentName = "spe-particles";

        // spe-particles member-fields

        private static object defAcceleration = JsonConvert.DeserializeObject("{x: 0, y: 0, z: 0}");
        [JsonProperty(PropertyName = "acceleration")]
        [Tooltip("Vector3")]
        public object Acceleration = defAcceleration;
        public bool ShouldSerializeAcceleration()
        {
            if (_token != null && _token.SelectToken("acceleration") != null) return true;
            return (Acceleration != defAcceleration);
        }

        public enum AccelerationDistributionType
        {
            [EnumMember(Value = "none")]
            None,
            [EnumMember(Value = "box")]
            Box,
            [EnumMember(Value = "sphere")]
            Sphere,
            [EnumMember(Value = "disc")]
            Disc,
        }
        private static AccelerationDistributionType defAccelerationDistribution = AccelerationDistributionType.None;
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(PropertyName = "accelerationDistribution")]
        [Tooltip("distribution of particle acceleration, for disc and sphere, only the x component will be used. if set to NONE use the 'distribution' attribute for accelerationDistribution")]
        public AccelerationDistributionType AccelerationDistribution = defAccelerationDistribution;
        public bool ShouldSerializeAccelerationDistribution()
        {
            if (_token != null && _token.SelectToken("accelerationDistribution") != null) return true;
            return (AccelerationDistribution != defAccelerationDistribution);
        }

        private static object defAccelerationSpread = JsonConvert.DeserializeObject("{x: 0, y: 0, z: 0}");
        [JsonProperty(PropertyName = "accelerationSpread")]
        [Tooltip("Vector3")]
        public object AccelerationSpread = defAccelerationSpread;
        public bool ShouldSerializeAccelerationSpread()
        {
            if (_token != null && _token.SelectToken("accelerationSpread") != null) return true;
            return (AccelerationSpread != defAccelerationSpread);
        }

        private static float defActiveMultiplier = 1f;
        [JsonProperty(PropertyName = "activeMultiplier")]
        [Tooltip("multiply the rate of particles emission, if larger than 1 then the particles will be emitted in bursts. note, very large numbers will emit all particles at once")]
        public float ActiveMultiplier = defActiveMultiplier;
        public bool ShouldSerializeActiveMultiplier()
        {
            if (_token != null && _token.SelectToken("activeMultiplier") != null) return true;
            return (ActiveMultiplier != defActiveMultiplier);
        }

        private static bool defAffectedByFog = true;
        [JsonProperty(PropertyName = "affectedByFog")]
        [Tooltip("if true, the particles are affected by THREE js fog")]
        public bool AffectedByFog = defAffectedByFog;
        public bool ShouldSerializeAffectedByFog()
        {
            if (_token != null && _token.SelectToken("affectedByFog") != null) return true;
            return (AffectedByFog != defAffectedByFog);
        }

        private static float defAlphaTest = 0f;
        [JsonProperty(PropertyName = "alphaTest")]
        [Tooltip("alpha values below the alphaTest threshold are considered invisible")]
        public float AlphaTest = defAlphaTest;
        public bool ShouldSerializeAlphaTest()
        {
            if (_token != null && _token.SelectToken("alphaTest") != null) return true;
            return (AlphaTest != defAlphaTest);
        }

        private static float[] defAngle = {0};
        [JsonProperty(PropertyName = "angle")]
        [Tooltip("2D rotation of the particle over the particle's lifetime, max 4 elements")]
        public float[] Angle = defAngle;
        public bool ShouldSerializeAngle()
        {
            if (_token != null && _token.SelectToken("angle") != null) return true;
            return (Angle != defAngle);
        }

        private static float[] defAngleSpread = {0};
        [JsonProperty(PropertyName = "angleSpread")]
        [Tooltip("spread in angle over the particle's lifetime, max 4 elements")]
        public float[] AngleSpread = defAngleSpread;
        public bool ShouldSerializeAngleSpread()
        {
            if (_token != null && _token.SelectToken("angleSpread") != null) return true;
            return (AngleSpread != defAngleSpread);
        }

        public enum BlendingType
        {
            [EnumMember(Value = "no")]
            No,
            [EnumMember(Value = "normal")]
            Normal,
            [EnumMember(Value = "additive")]
            Additive,
            [EnumMember(Value = "subtractive")]
            Subtractive,
            [EnumMember(Value = "multiply")]
            Multiply,
            [EnumMember(Value = "custom")]
            Custom,
        }
        private static BlendingType defBlending = BlendingType.Normal;
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(PropertyName = "blending")]
        [Tooltip("blending mode, when drawing particles")]
        public BlendingType Blending = defBlending;
        public bool ShouldSerializeBlending()
        {
            if (_token != null && _token.SelectToken("blending") != null) return true;
            return (Blending != defBlending);
        }

        private static string[] defColor = {"#fff"};
        [JsonProperty(PropertyName = "color")]
        [Tooltip("array of colors over the particle's lifetime, max 4 elements")]
        public string[] Color = defColor;
        public bool ShouldSerializeColor()
        {
            if (_token != null && _token.SelectToken("color") != null) return true;
            return (Color != defColor);
        }

        private static string[] defColorSpread = {};
        [JsonProperty(PropertyName = "colorSpread")]
        [Tooltip("spread to apply to colors, spread an array of vec3 (r g b) with 0 for no spread. note the spread will be re-applied through-out the lifetime of the particle")]
        public string[] ColorSpread = defColorSpread;
        public bool ShouldSerializeColorSpread()
        {
            if (_token != null && _token.SelectToken("colorSpread") != null) return true;
            return (ColorSpread != defColorSpread);
        }

        private static bool defDepthTest = true;
        [JsonProperty(PropertyName = "depthTest")]
        [Tooltip("if true, don't render a particle's pixels if something is closer in the depth buffer")]
        public bool DepthTest = defDepthTest;
        public bool ShouldSerializeDepthTest()
        {
            if (_token != null && _token.SelectToken("depthTest") != null) return true;
            return (DepthTest != defDepthTest);
        }

        private static bool defDepthWrite = false;
        [JsonProperty(PropertyName = "depthWrite")]
        [Tooltip("if true, particles write their depth into the depth buffer. this should be false if we use transparent particles")]
        public bool DepthWrite = defDepthWrite;
        public bool ShouldSerializeDepthWrite()
        {
            if (_token != null && _token.SelectToken("depthWrite") != null) return true;
            return (DepthWrite != defDepthWrite);
        }

        public enum DirectionType
        {
            [EnumMember(Value = "forward")]
            Forward,
            [EnumMember(Value = "backward")]
            Backward,
        }
        private static DirectionType defDirection = DirectionType.Forward;
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(PropertyName = "direction")]
        [Tooltip("make the emitter operate forward or backward in time")]
        public DirectionType Direction = defDirection;
        public bool ShouldSerializeDirection()
        {
            if (_token != null && _token.SelectToken("direction") != null) return true;
            return (Direction != defDirection);
        }

        public enum DistributionType
        {
            [EnumMember(Value = "box")]
            Box,
            [EnumMember(Value = "sphere")]
            Sphere,
            [EnumMember(Value = "disc")]
            Disc,
        }
        private static DistributionType defDistribution = DistributionType.Box;
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(PropertyName = "distribution")]
        [Tooltip("distribution for particle positions, velocities and acceleration. will be overriden by specific '...Distribution' attributes")]
        public DistributionType Distribution = defDistribution;
        public bool ShouldSerializeDistribution()
        {
            if (_token != null && _token.SelectToken("distribution") != null) return true;
            return (Distribution != defDistribution);
        }

        private static float defDrag = 0f;
        [JsonProperty(PropertyName = "drag")]
        [Tooltip("apply resistance to moving the particle, 0 is no resistance, 1 is full resistance, particle will stop moving at half of it's maxAge")]
        public float Drag = defDrag;
        public bool ShouldSerializeDrag()
        {
            if (_token != null && _token.SelectToken("drag") != null) return true;
            return (Drag != defDrag);
        }

        private static float defDragSpread = 0f;
        [JsonProperty(PropertyName = "dragSpread")]
        [Tooltip("spread to apply to the drag attribute")]
        public float DragSpread = defDragSpread;
        public bool ShouldSerializeDragSpread()
        {
            if (_token != null && _token.SelectToken("dragSpread") != null) return true;
            return (DragSpread != defDragSpread);
        }

        private static float defDuration = -1f;
        [JsonProperty(PropertyName = "duration")]
        [Tooltip("duration of the emitter (seconds), if less than 0 then continuously emit particles")]
        public float Duration = defDuration;
        public bool ShouldSerializeDuration()
        {
            if (_token != null && _token.SelectToken("duration") != null) return true;
            return (Duration != defDuration);
        }

        private static float defEmitterScale = 100f;
        [JsonProperty(PropertyName = "emitterScale")]
        [Tooltip("global scaling factor for all particles from the emitter")]
        public float EmitterScale = defEmitterScale;
        public bool ShouldSerializeEmitterScale()
        {
            if (_token != null && _token.SelectToken("emitterScale") != null) return true;
            return (EmitterScale != defEmitterScale);
        }

        private static bool defEnableInEditor = false;
        [JsonProperty(PropertyName = "enableInEditor")]
        [Tooltip("enable the emitter while the editor is active (i.e. while scene is paused)")]
        public bool EnableInEditor = defEnableInEditor;
        public bool ShouldSerializeEnableInEditor()
        {
            if (_token != null && _token.SelectToken("enableInEditor") != null) return true;
            return (EnableInEditor != defEnableInEditor);
        }

        private static bool defEnabled = true;
        [JsonProperty(PropertyName = "enabled")]
        [Tooltip("enable/disable the emitter")]
        public bool Enabled = defEnabled;
        public bool ShouldSerializeEnabled()
        {
            if (_token != null && _token.SelectToken("enabled") != null) return true;
            return (Enabled != defEnabled);
        }

        private static bool defFrustumCulled = false;
        [JsonProperty(PropertyName = "frustumCulled")]
        [Tooltip("enable/disable frustum culling")]
        public bool FrustumCulled = defFrustumCulled;
        public bool ShouldSerializeFrustumCulled()
        {
            if (_token != null && _token.SelectToken("frustumCulled") != null) return true;
            return (FrustumCulled != defFrustumCulled);
        }

        private static bool defHasPerspective = true;
        [JsonProperty(PropertyName = "hasPerspective")]
        [Tooltip("if true, particles will be larger the closer they are to the camera")]
        public bool HasPerspective = defHasPerspective;
        public bool ShouldSerializeHasPerspective()
        {
            if (_token != null && _token.SelectToken("hasPerspective") != null) return true;
            return (HasPerspective != defHasPerspective);
        }

        private static float defMaxAge = 1f;
        [JsonProperty(PropertyName = "maxAge")]
        [Tooltip("maximum age of a particle before reusing")]
        public float MaxAge = defMaxAge;
        public bool ShouldSerializeMaxAge()
        {
            if (_token != null && _token.SelectToken("maxAge") != null) return true;
            return (MaxAge != defMaxAge);
        }

        private static float defMaxAgeSpread = 0f;
        [JsonProperty(PropertyName = "maxAgeSpread")]
        [Tooltip("variance for the 'maxAge' attribute")]
        public float MaxAgeSpread = defMaxAgeSpread;
        public bool ShouldSerializeMaxAgeSpread()
        {
            if (_token != null && _token.SelectToken("maxAgeSpread") != null) return true;
            return (MaxAgeSpread != defMaxAgeSpread);
        }

        private static float[] defOpacity = {1};
        [JsonProperty(PropertyName = "opacity")]
        [Tooltip("opacity over the particle's lifetime, max 4 elements")]
        public float[] Opacity = defOpacity;
        public bool ShouldSerializeOpacity()
        {
            if (_token != null && _token.SelectToken("opacity") != null) return true;
            return (Opacity != defOpacity);
        }

        private static float[] defOpacitySpread = {0};
        [JsonProperty(PropertyName = "opacitySpread")]
        [Tooltip("spread in opacity over the particle's lifetime, max 4 elements")]
        public float[] OpacitySpread = defOpacitySpread;
        public bool ShouldSerializeOpacitySpread()
        {
            if (_token != null && _token.SelectToken("opacitySpread") != null) return true;
            return (OpacitySpread != defOpacitySpread);
        }

        private static int defParticleCount = 100;
        [JsonProperty(PropertyName = "particleCount")]
        [Tooltip("maximum number of particles for this emitter")]
        public int ParticleCount = defParticleCount;
        public bool ShouldSerializeParticleCount()
        {
            if (_token != null && _token.SelectToken("particleCount") != null) return true;
            return (ParticleCount != defParticleCount);
        }

        public enum PositionDistributionType
        {
            [EnumMember(Value = "none")]
            None,
            [EnumMember(Value = "box")]
            Box,
            [EnumMember(Value = "sphere")]
            Sphere,
            [EnumMember(Value = "disc")]
            Disc,
        }
        private static PositionDistributionType defPositionDistribution = PositionDistributionType.None;
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(PropertyName = "positionDistribution")]
        [Tooltip("distribution of particle positions, disc and sphere will use the radius attributes. For box particles emit at 0,0,0, for sphere they emit on the surface of the sphere and for disc on the edge of a 2D disc on the XY plane")]
        public PositionDistributionType PositionDistribution = defPositionDistribution;
        public bool ShouldSerializePositionDistribution()
        {
            if (_token != null && _token.SelectToken("positionDistribution") != null) return true;
            return (PositionDistribution != defPositionDistribution);
        }

        private static object defPositionOffset = JsonConvert.DeserializeObject("{x: 0, y: 0, z: 0}");
        [JsonProperty(PropertyName = "positionOffset")]
        [Tooltip("Vector3")]
        public object PositionOffset = defPositionOffset;
        public bool ShouldSerializePositionOffset()
        {
            if (_token != null && _token.SelectToken("positionOffset") != null) return true;
            return (PositionOffset != defPositionOffset);
        }

        private static object defPositionSpread = JsonConvert.DeserializeObject("{x: 0, y: 0, z: 0}");
        [JsonProperty(PropertyName = "positionSpread")]
        [Tooltip("Vector3")]
        public object PositionSpread = defPositionSpread;
        public bool ShouldSerializePositionSpread()
        {
            if (_token != null && _token.SelectToken("positionSpread") != null) return true;
            return (PositionSpread != defPositionSpread);
        }

        private static float defRadius = 1f;
        [JsonProperty(PropertyName = "radius")]
        [Tooltip("radius of the disc or sphere emitter (ignored for box). note radius of 0 will prevent velocity and acceleration if they use a sphere or disc distribution")]
        public float Radius = defRadius;
        public bool ShouldSerializeRadius()
        {
            if (_token != null && _token.SelectToken("radius") != null) return true;
            return (Radius != defRadius);
        }

        private static object defRadiusScale = JsonConvert.DeserializeObject("{x: 1, y: 1, z: 1}");
        [JsonProperty(PropertyName = "radiusScale")]
        [Tooltip("Vector3")]
        public object RadiusScale = defRadiusScale;
        public bool ShouldSerializeRadiusScale()
        {
            if (_token != null && _token.SelectToken("radiusScale") != null) return true;
            return (RadiusScale != defRadiusScale);
        }

        private static bool defRandomizeAcceleration = false;
        [JsonProperty(PropertyName = "randomizeAcceleration")]
        [Tooltip("if true, re-randomize acceleration when re-spawning a particle, can incur a performance hit")]
        public bool RandomizeAcceleration = defRandomizeAcceleration;
        public bool ShouldSerializeRandomizeAcceleration()
        {
            if (_token != null && _token.SelectToken("randomizeAcceleration") != null) return true;
            return (RandomizeAcceleration != defRandomizeAcceleration);
        }

        private static bool defRandomizeAngle = false;
        [JsonProperty(PropertyName = "randomizeAngle")]
        [Tooltip("if true, re-randomize angle when re-spawning a particle, can incur a performance hit")]
        public bool RandomizeAngle = defRandomizeAngle;
        public bool ShouldSerializeRandomizeAngle()
        {
            if (_token != null && _token.SelectToken("randomizeAngle") != null) return true;
            return (RandomizeAngle != defRandomizeAngle);
        }

        private static bool defRandomizeColor = false;
        [JsonProperty(PropertyName = "randomizeColor")]
        [Tooltip("if true, re-randomize colour when re-spawning a particle, can incur a performance hit")]
        public bool RandomizeColor = defRandomizeColor;
        public bool ShouldSerializeRandomizeColor()
        {
            if (_token != null && _token.SelectToken("randomizeColor") != null) return true;
            return (RandomizeColor != defRandomizeColor);
        }

        private static bool defRandomizeDrag = false;
        [JsonProperty(PropertyName = "randomizeDrag")]
        [Tooltip("if true, re-randomize drag when re-spawning a particle, can incur a performance hit")]
        public bool RandomizeDrag = defRandomizeDrag;
        public bool ShouldSerializeRandomizeDrag()
        {
            if (_token != null && _token.SelectToken("randomizeDrag") != null) return true;
            return (RandomizeDrag != defRandomizeDrag);
        }

        private static bool defRandomizeOpacity = false;
        [JsonProperty(PropertyName = "randomizeOpacity")]
        [Tooltip("if true, re-randomize opacity when re-spawning a particle, can incur a performance hit")]
        public bool RandomizeOpacity = defRandomizeOpacity;
        public bool ShouldSerializeRandomizeOpacity()
        {
            if (_token != null && _token.SelectToken("randomizeOpacity") != null) return true;
            return (RandomizeOpacity != defRandomizeOpacity);
        }

        private static bool defRandomizePosition = false;
        [JsonProperty(PropertyName = "randomizePosition")]
        [Tooltip("if true, re-randomize position when re-spawning a particle, can incur a performance hit")]
        public bool RandomizePosition = defRandomizePosition;
        public bool ShouldSerializeRandomizePosition()
        {
            if (_token != null && _token.SelectToken("randomizePosition") != null) return true;
            return (RandomizePosition != defRandomizePosition);
        }

        private static bool defRandomizeRotation = false;
        [JsonProperty(PropertyName = "randomizeRotation")]
        [Tooltip("if true, re-randomize rotation when re-spawning a particle, can incur a performance hit")]
        public bool RandomizeRotation = defRandomizeRotation;
        public bool ShouldSerializeRandomizeRotation()
        {
            if (_token != null && _token.SelectToken("randomizeRotation") != null) return true;
            return (RandomizeRotation != defRandomizeRotation);
        }

        private static bool defRandomizeSize = false;
        [JsonProperty(PropertyName = "randomizeSize")]
        [Tooltip("if true, re-randomize size when re-spawning a particle, can incur a performance hit")]
        public bool RandomizeSize = defRandomizeSize;
        public bool ShouldSerializeRandomizeSize()
        {
            if (_token != null && _token.SelectToken("randomizeSize") != null) return true;
            return (RandomizeSize != defRandomizeSize);
        }

        private static bool defRandomizeVelocity = false;
        [JsonProperty(PropertyName = "randomizeVelocity")]
        [Tooltip("if true, re-randomize velocity when re-spawning a particle, can incur a performance hit")]
        public bool RandomizeVelocity = defRandomizeVelocity;
        public bool ShouldSerializeRandomizeVelocity()
        {
            if (_token != null && _token.SelectToken("randomizeVelocity") != null) return true;
            return (RandomizeVelocity != defRandomizeVelocity);
        }

        public enum RelativeType
        {
            [EnumMember(Value = "local")]
            Local,
            [EnumMember(Value = "world")]
            World,
        }
        private static RelativeType defRelative = RelativeType.Local;
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(PropertyName = "relative")]
        [Tooltip("world relative particles move relative to the world, while local particles move relative to the emitter (i.e. if the emitter moves, all particles move with it)")]
        public RelativeType Relative = defRelative;
        public bool ShouldSerializeRelative()
        {
            if (_token != null && _token.SelectToken("relative") != null) return true;
            return (Relative != defRelative);
        }

        private static float defRotation = 0f;
        [JsonProperty(PropertyName = "rotation")]
        [Tooltip("rotation in degrees")]
        public float Rotation = defRotation;
        public bool ShouldSerializeRotation()
        {
            if (_token != null && _token.SelectToken("rotation") != null) return true;
            return (Rotation != defRotation);
        }

        private static object defRotationAxis = JsonConvert.DeserializeObject("{x: 0, y: 0, z: 0}");
        [JsonProperty(PropertyName = "rotationAxis")]
        [Tooltip("Vector3")]
        public object RotationAxis = defRotationAxis;
        public bool ShouldSerializeRotationAxis()
        {
            if (_token != null && _token.SelectToken("rotationAxis") != null) return true;
            return (RotationAxis != defRotationAxis);
        }

        private static object defRotationAxisSpread = JsonConvert.DeserializeObject("{x: 0, y: 0, z: 0}");
        [JsonProperty(PropertyName = "rotationAxisSpread")]
        [Tooltip("Vector3")]
        public object RotationAxisSpread = defRotationAxisSpread;
        public bool ShouldSerializeRotationAxisSpread()
        {
            if (_token != null && _token.SelectToken("rotationAxisSpread") != null) return true;
            return (RotationAxisSpread != defRotationAxisSpread);
        }

        private static float defRotationSpread = 0f;
        [JsonProperty(PropertyName = "rotationSpread")]
        [Tooltip("rotation variance in degrees")]
        public float RotationSpread = defRotationSpread;
        public bool ShouldSerializeRotationSpread()
        {
            if (_token != null && _token.SelectToken("rotationSpread") != null) return true;
            return (RotationSpread != defRotationSpread);
        }

        private static bool defRotationStatic = false;
        [JsonProperty(PropertyName = "rotationStatic")]
        [Tooltip("if true, the particles are fixed at their initial rotation value. if false, the particle will rotate from 0 to the 'rotation' value")]
        public bool RotationStatic = defRotationStatic;
        public bool ShouldSerializeRotationStatic()
        {
            if (_token != null && _token.SelectToken("rotationStatic") != null) return true;
            return (RotationStatic != defRotationStatic);
        }

        private static float[] defSize = {1};
        [JsonProperty(PropertyName = "size")]
        [Tooltip("array of sizes over the particle's lifetime, max 4 elements")]
        public float[] Size = defSize;
        public bool ShouldSerializeSize()
        {
            if (_token != null && _token.SelectToken("size") != null) return true;
            return (Size != defSize);
        }

        private static float[] defSizeSpread = {0};
        [JsonProperty(PropertyName = "sizeSpread")]
        [Tooltip("spread in size over the particle's lifetime, max 4 elements")]
        public float[] SizeSpread = defSizeSpread;
        public bool ShouldSerializeSizeSpread()
        {
            if (_token != null && _token.SelectToken("sizeSpread") != null) return true;
            return (SizeSpread != defSizeSpread);
        }

        private static string defTexture = "";
        [JsonProperty(PropertyName = "texture")]
        [Tooltip("texture to be used for each particle, may be a spritesheet.  Examples: [blob.png, fog.png, square.png, explosion_sheet.png, fireworks_sheet.png], like path 'static/images/textures/blob.png'")]
        public string Texture = defTexture;
        public bool ShouldSerializeTexture()
        {
            if (_token != null && _token.SelectToken("texture") != null) return true;
            return (Texture != defTexture);
        }

        private static int defTextureFrameCount = -1;
        [JsonProperty(PropertyName = "textureFrameCount")]
        [Tooltip("number of frames in the spritesheet, negative numbers default to textureFrames.x * textureFrames.y")]
        public int TextureFrameCount = defTextureFrameCount;
        public bool ShouldSerializeTextureFrameCount()
        {
            if (_token != null && _token.SelectToken("textureFrameCount") != null) return true;
            return (TextureFrameCount != defTextureFrameCount);
        }

        private static int defTextureFrameLoop = 1;
        [JsonProperty(PropertyName = "textureFrameLoop")]
        [Tooltip("number of times the spritesheet should be looped over the lifetime of a particle")]
        public int TextureFrameLoop = defTextureFrameLoop;
        public bool ShouldSerializeTextureFrameLoop()
        {
            if (_token != null && _token.SelectToken("textureFrameLoop") != null) return true;
            return (TextureFrameLoop != defTextureFrameLoop);
        }

        private static object defTextureFrames = JsonConvert.DeserializeObject("{x: 1, y: 1}");
        [JsonProperty(PropertyName = "textureFrames")]
        [Tooltip("Vector2")]
        public object TextureFrames = defTextureFrames;
        public bool ShouldSerializeTextureFrames()
        {
            if (_token != null && _token.SelectToken("textureFrames") != null) return true;
            return (TextureFrames != defTextureFrames);
        }

        private static bool defUseTransparency = true;
        [JsonProperty(PropertyName = "useTransparency")]
        [Tooltip("should the particles be rendered with transparency?")]
        public bool UseTransparency = defUseTransparency;
        public bool ShouldSerializeUseTransparency()
        {
            if (_token != null && _token.SelectToken("useTransparency") != null) return true;
            return (UseTransparency != defUseTransparency);
        }

        private static object defVelocity = JsonConvert.DeserializeObject("{x: 0, y: 0, z: 0}");
        [JsonProperty(PropertyName = "velocity")]
        [Tooltip("Vector3")]
        public object Velocity = defVelocity;
        public bool ShouldSerializeVelocity()
        {
            if (_token != null && _token.SelectToken("velocity") != null) return true;
            return (Velocity != defVelocity);
        }

        public enum VelocityDistributionType
        {
            [EnumMember(Value = "none")]
            None,
            [EnumMember(Value = "box")]
            Box,
            [EnumMember(Value = "sphere")]
            Sphere,
            [EnumMember(Value = "disc")]
            Disc,
        }
        private static VelocityDistributionType defVelocityDistribution = VelocityDistributionType.None;
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(PropertyName = "velocityDistribution")]
        [Tooltip("distribution of particle velocities, for disc and sphere, only the x component will be used. if set to NONE use the 'distribution' attribute for velocityDistribution")]
        public VelocityDistributionType VelocityDistribution = defVelocityDistribution;
        public bool ShouldSerializeVelocityDistribution()
        {
            if (_token != null && _token.SelectToken("velocityDistribution") != null) return true;
            return (VelocityDistribution != defVelocityDistribution);
        }

        private static object defVelocitySpread = JsonConvert.DeserializeObject("{x: 0, y: 0, z: 0}");
        [JsonProperty(PropertyName = "velocitySpread")]
        [Tooltip("Vector3")]
        public object VelocitySpread = defVelocitySpread;
        public bool ShouldSerializeVelocitySpread()
        {
            if (_token != null && _token.SelectToken("velocitySpread") != null) return true;
            return (VelocitySpread != defVelocitySpread);
        }

        private static float defWiggle = 0f;
        [JsonProperty(PropertyName = "wiggle")]
        [Tooltip("extra distance the particle moves over its lifetime")]
        public float Wiggle = defWiggle;
        public bool ShouldSerializeWiggle()
        {
            if (_token != null && _token.SelectToken("wiggle") != null) return true;
            return (Wiggle != defWiggle);
        }

        private static float defWiggleSpread = 0f;
        [JsonProperty(PropertyName = "wiggleSpread")]
        [Tooltip("+- spread for the wiggle attribute")]
        public float WiggleSpread = defWiggleSpread;
        public bool ShouldSerializeWiggleSpread()
        {
            if (_token != null && _token.SelectToken("wiggleSpread") != null) return true;
            return (WiggleSpread != defWiggleSpread);
        }

        // General json object management

        [JsonExtensionData]
        private IDictionary<string, JToken> _additionalData;

        private static JToken _token;

        public string SaveToString()
        {
            return Regex.Unescape(JsonConvert.SerializeObject(this));
        }

        public static ArenaSpeParticlesJson CreateFromJSON(string jsonString, JToken token)
        {
            _token = token; // save updated wire json
            ArenaSpeParticlesJson json = null;
            try {
                json = JsonConvert.DeserializeObject<ArenaSpeParticlesJson>(Regex.Unescape(jsonString));
            } catch (JsonReaderException e)
            {
                Debug.LogWarning($"{e.Message}: {jsonString}");
            }
            return json;
        }
    }
}
