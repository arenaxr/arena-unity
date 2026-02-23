/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2023, Carnegie Mellon University. All rights reserved.
 */

using System.Collections.Generic;
using System.Text.RegularExpressions;
using ArenaUnity.Schemas;
using Newtonsoft.Json;
using UnityEngine;

namespace ArenaUnity.Components
{
    public class ArenaSpeParticles : ArenaComponent
    {
        // ARENA spe-particles component unity conversion status:
        // DONE: acceleration
        // TODO: accelerationDistribution
        // TODO: accelerationSpread
        // TODO: activeMultiplier
        // TODO: affectedByFog
        // TODO: alphaTest
        // TODO: angle
        // TODO: angleSpread
        // DONE: blending
        // DONE: color
        // TODO: colorSpread
        // DONE: depthTest
        // DONE: depthWrite
        // TODO: direction
        // DONE: distribution
        // TODO: drag
        // TODO: dragSpread
        // DONE: duration
        // TODO: emitterScale
        // TODO: enableInEditor
        // DONE: enabled
        // TODO: frustumCulled
        // TODO: hasPerspective
        // DONE: maxAge
        // TODO: maxAgeSpread
        // DONE: opacity
        // TODO: opacitySpread
        // DONE: particleCount
        // TODO: positionDistribution
        // DONE: positionOffset
        // DONE: positionSpread
        // DONE: radius
        // TODO: radiusScale
        // TODO: randomizeAcceleration
        // TODO: randomizeAngle
        // TODO: randomizeColor
        // TODO: randomizeDrag
        // TODO: randomizeOpacity
        // TODO: randomizePosition
        // TODO: randomizeRotation
        // TODO: randomizeSize
        // TODO: randomizeVelocity
        // DONE: relative
        // TODO: rotation
        // TODO: rotationAxis
        // TODO: rotationAxisSpread
        // TODO: rotationSpread
        // TODO: rotationStatic
        // DONE: size
        // TODO: sizeSpread
        // DONE: texture
        // TODO: textureFrameCount
        // TODO: textureFrameLoop
        // TODO: textureFrames
        // TODO: useTransparency
        // DONE: velocity
        // TODO: velocityDistribution
        // TODO: velocitySpread
        // TODO: wiggle
        // TODO: wiggleSpread

        // NEXT STEPS FOR ADVANCED PARTICLES:
        // 1. Implementing SizeOverLifetime & ColorOverLifetime arrays based on SPE's logic.
        //    A-Frame's SPE component can array lengths beyond simple start/end, Unity's `MinMaxCurve` or `Gradient` will be needed.
        // 2. Add spritesheet animation mapping (textureFrames, textureFrameCount, textureFrameLoop) using `ParticleSystem.TextureSheetAnimationModule`.
        // 3. Add random spread mapping. For instance, `PositionSpread` and `ColorSpread` require `ParticleSystem.MinMaxCurve` / `MinMaxGradient` with random between two constants.

        public ArenaSpeParticlesJson json = new ArenaSpeParticlesJson();
        private ParticleSystem ps;
        private ParticleSystemRenderer psr;

        protected override void ApplyRender()
        {
            if (ps == null)
            {
                ps = gameObject.AddComponent<ParticleSystem>();
                psr = gameObject.GetComponent<ParticleSystemRenderer>();
            }

            var main = ps.main;

            main.loop = json.Duration < 0;
            if (json.Duration > 0)
                main.duration = json.Duration;

            main.startLifetime = json.MaxAge;
            main.playOnAwake = json.Enabled;
            // TODO: handle enableInEditor, affectedByFog

            if (json.Size != null && json.Size.Length > 0)
            {
                if (json.Size[0].HasValue)
                    main.startSize = json.Size[0].Value;
                // TODO: use SizeOverLifetime if multiple sizes provided
            }

            if (json.Color != null && json.Color.Length > 0)
            {
                ColorUtility.TryParseHtmlString(json.Color[0], out Color startColor);
                if (json.Opacity != null && json.Opacity.Length > 0 && json.Opacity[0].HasValue)
                    startColor.a = json.Opacity[0].Value;
                main.startColor = startColor;
                // TODO: use ColorOverLifetime if multiple colors/opacities provided
            }

            main.maxParticles = json.ParticleCount;
            // TODO: handle ActiveMultiplier

            var emission = ps.emission;
            emission.enabled = true;
            // Rough approximation of rate, A-Frame SPE is more complex
            emission.rateOverTime = json.ParticleCount / json.MaxAge;

            var shape = ps.shape;
            shape.enabled = true;
            switch (json.Distribution)
            {
                case ArenaSpeParticlesJson.DistributionType.Box:
                    shape.shapeType = ParticleSystemShapeType.Box;
                    shape.scale = ArenaUnity.ToUnityScale(json.PositionSpread);
                    break;
                case ArenaSpeParticlesJson.DistributionType.Sphere:
                    shape.shapeType = ParticleSystemShapeType.Sphere;
                    shape.radius = json.Radius;
                    // TODO: how to handle json.RadiusScale
                    break;
                case ArenaSpeParticlesJson.DistributionType.Disc:
                    shape.shapeType = ParticleSystemShapeType.Circle;
                    shape.radius = json.Radius;
                    // TODO: how to handle json.RadiusScale
                    break;
            }
            shape.position = ArenaUnity.ToUnityPosition(json.PositionOffset);

            var velocityOverLifetime = ps.velocityOverLifetime;
            velocityOverLifetime.enabled = true;
            velocityOverLifetime.x = new ParticleSystem.MinMaxCurve((float)json.Velocity.X);
            velocityOverLifetime.y = new ParticleSystem.MinMaxCurve((float)json.Velocity.Y);
            velocityOverLifetime.z = new ParticleSystem.MinMaxCurve(-(float)json.Velocity.Z);

            var forceOverLifetime = ps.forceOverLifetime;
            forceOverLifetime.enabled = true;
            forceOverLifetime.x = new ParticleSystem.MinMaxCurve((float)json.Acceleration.X);
            forceOverLifetime.y = new ParticleSystem.MinMaxCurve((float)json.Acceleration.Y);
            forceOverLifetime.z = new ParticleSystem.MinMaxCurve(-(float)json.Acceleration.Z);

            main.simulationSpace = json.Relative == ArenaSpeParticlesJson.RelativeType.World ? ParticleSystemSimulationSpace.World : ParticleSystemSimulationSpace.Local;

            // Texture support
            psr.material = new Material(ArenaUnity.GetUnlitShader());
            if (!string.IsNullOrEmpty(json.Texture))
            {
                string assetPath = ArenaClientScene.Instance.checkLocalAsset((string)json.Texture);
                if (assetPath != null)
                {
                    ArenaMaterial.AttachMaterialTexture(assetPath, gameObject);
                }
            }
            else
            {
                // Assign a sensible default circular particle if no texture is provided, matching SPE behavior
                // (This can be refined later)
            }

            switch(json.Blending)
            {
                case ArenaSpeParticlesJson.BlendingType.Additive:
                    psr.material.SetFloat("_Mode", 3f); // Transparent
                    psr.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    psr.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    break;
                case ArenaSpeParticlesJson.BlendingType.Multiply:
                    psr.material.SetFloat("_Mode", 3f);
                    psr.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.DstColor);
                    psr.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                    break;
                case ArenaSpeParticlesJson.BlendingType.Subtractive:
                    // Unity doesn't have a direct "subtractive" mapping in the standard setup easily accessible here without a custom shader, using Fade as fallback
                case ArenaSpeParticlesJson.BlendingType.Normal:
                case ArenaSpeParticlesJson.BlendingType.No:
                default:
                    psr.material.SetFloat("_Mode", 2f); // Fade
                    psr.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    psr.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    break;
            }

            if (json.DepthTest)
                psr.material.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.LessEqual);
            else
                psr.material.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Always);

            if (json.DepthWrite)
                psr.material.SetFloat("_ZWrite", 1f);
            else
                psr.material.SetFloat("_ZWrite", 0f);

            if (json.Enabled) {
                if (!ps.isPlaying) {
                     ps.Play();
                }
            } else {
                ps.Stop();
                ps.Clear();
            }
        }

        public override void UpdateObject()
        {
            var newJson = JsonConvert.SerializeObject(json);
            if (updatedJson != newJson)
            {
                var aobj = GetComponent<ArenaObject>();
                if (aobj != null)
                {
                    aobj.PublishUpdate($"{{\"{json.componentName}\":{newJson}}}");
                    apply = true;
                }
            }
            updatedJson = newJson;
        }
    }
}
