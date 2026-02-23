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
        // DONE: textureFrameCount
        // DONE: textureFrameLoop
        // DONE: textureFrames
        // TODO: useTransparency
        // DONE: velocity
        // TODO: velocityDistribution
        // TODO: velocitySpread
        // TODO: wiggle
        // TODO: wiggleSpread

        // NEXT STEPS FOR ADVANCED PARTICLES:
        // 1. Implementing SizeOverLifetime & ColorOverLifetime arrays based on SPE's logic.
        //    A-Frame's SPE component can array lengths beyond simple start/end, Unity's `MinMaxCurve` or `Gradient` will be needed.
        // 2. Add random spread mapping. For instance, `PositionSpread` and `ColorSpread` require `ParticleSystem.MinMaxCurve` / `MinMaxGradient` with random between two constants.

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
                    main.startSize = (float)json.Size[0].Value;

                if (json.Size.Length > 1)
                {
                    var sizeOverLifetime = ps.sizeOverLifetime;
                    sizeOverLifetime.enabled = true;
                    var curve = new AnimationCurve();
                    for (int i = 0; i < json.Size.Length; i++)
                    {
                        if (json.Size[i].HasValue)
                        {
                            float t = (float)i / (json.Size.Length - 1);
                            // Unity's sizeOverLifetime applies a multiplier to startSize
                            float multiplier = (float)json.Size[0].Value != 0 ? (float)json.Size[i].Value / (float)json.Size[0].Value : 0;
                            curve.AddKey(t, multiplier);
                        }
                    }
                    sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, curve);
                }
            }

            if (json.Color != null && json.Color.Length > 0)
            {
                ColorUtility.TryParseHtmlString(json.Color[0], out Color startColor);
                if (json.Opacity != null && json.Opacity.Length > 0 && json.Opacity[0].HasValue)
                    startColor.a = json.Opacity[0].Value;
                main.startColor = startColor;

                if (json.Color.Length > 1 || (json.Opacity != null && json.Opacity.Length > 1))
                {
                    var colorOverLifetime = ps.colorOverLifetime;
                    colorOverLifetime.enabled = true;

                    int colorLen = json.Color.Length;
                    int alphaLen = json.Opacity != null ? json.Opacity.Length : 0;

                    Gradient g = new Gradient();
                    var colorKeys = new GradientColorKey[colorLen];
                    var alphaKeys = new GradientAlphaKey[alphaLen == 0 ? 1 : alphaLen];

                    for (int i = 0; i < colorLen; i++) {
                        ColorUtility.TryParseHtmlString(json.Color[i], out Color c);
                        float t = colorLen == 1 ? 0f : (float)i / (colorLen - 1);
                        colorKeys[i] = new GradientColorKey(c, t);
                    }
                    if (alphaLen == 0) {
                        alphaKeys = new GradientAlphaKey[2];
                        alphaKeys[0] = new GradientAlphaKey(startColor.a, 0f);
                        alphaKeys[1] = new GradientAlphaKey(startColor.a, 1f);
                    } else if (alphaLen == 1) {
                        alphaKeys = new GradientAlphaKey[2];
                        float a = json.Opacity[0].HasValue ? json.Opacity[0].Value : 1f;
                        alphaKeys[0] = new GradientAlphaKey(a, 0f);
                        alphaKeys[1] = new GradientAlphaKey(a, 1f);
                    } else {
                        for (int i = 0; i < alphaLen; i++) {
                            float a = json.Opacity[i].HasValue ? json.Opacity[i].Value : 1f;
                            float t = (float)i / (alphaLen - 1);
                            alphaKeys[i] = new GradientAlphaKey(a, t);
                        }
                    }
                    g.SetKeys(colorKeys, alphaKeys);
                    colorOverLifetime.color = new ParticleSystem.MinMaxGradient(g);
                }
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
                    shape.scale = ArenaUnity.ToUnityScale(json.RadiusScale);
                    break;
                case ArenaSpeParticlesJson.DistributionType.Disc:
                    shape.shapeType = ParticleSystemShapeType.Circle;
                    shape.radius = json.Radius;
                    shape.scale = ArenaUnity.ToUnityScale(json.RadiusScale);
                    break;
            }
            shape.position = ArenaUnity.ToUnityPosition(json.PositionOffset);

            var velocityOverLifetime = ps.velocityOverLifetime;
            velocityOverLifetime.enabled = true;
            velocityOverLifetime.x = new ParticleSystem.MinMaxCurve(
                (float)(json.Velocity.X - json.VelocitySpread.X / 2.0),
                (float)(json.Velocity.X + json.VelocitySpread.X / 2.0));
            velocityOverLifetime.y = new ParticleSystem.MinMaxCurve(
                (float)(json.Velocity.Y - json.VelocitySpread.Y / 2.0),
                (float)(json.Velocity.Y + json.VelocitySpread.Y / 2.0));
            velocityOverLifetime.z = new ParticleSystem.MinMaxCurve(
                (float)(-json.Velocity.Z - json.VelocitySpread.Z / 2.0), // Z axis is flipped for Unity
                (float)(-json.Velocity.Z + json.VelocitySpread.Z / 2.0));

            var forceOverLifetime = ps.forceOverLifetime;
            forceOverLifetime.enabled = true;
            forceOverLifetime.x = new ParticleSystem.MinMaxCurve(
                (float)(json.Acceleration.X - json.AccelerationSpread.X / 2.0),
                (float)(json.Acceleration.X + json.AccelerationSpread.X / 2.0));
            forceOverLifetime.y = new ParticleSystem.MinMaxCurve(
                (float)(json.Acceleration.Y - json.AccelerationSpread.Y / 2.0),
                (float)(json.Acceleration.Y + json.AccelerationSpread.Y / 2.0));
            forceOverLifetime.z = new ParticleSystem.MinMaxCurve(
                (float)(-json.Acceleration.Z - json.AccelerationSpread.Z / 2.0), // Z axis is flipped for Unity
                (float)(-json.Acceleration.Z + json.AccelerationSpread.Z / 2.0));

            main.simulationSpace = json.Relative == ArenaSpeParticlesJson.RelativeType.World ? ParticleSystemSimulationSpace.World : ParticleSystemSimulationSpace.Local;

            if (json.TextureFrames != null && (json.TextureFrames.X > 1 || json.TextureFrames.Y > 1))
            {
                var textureSheetAnimation = ps.textureSheetAnimation;
                textureSheetAnimation.enabled = true;
                textureSheetAnimation.numTilesX = (int)json.TextureFrames.X;
                textureSheetAnimation.numTilesY = (int)json.TextureFrames.Y;

                int frameCount = json.TextureFrameCount >= 0 ? json.TextureFrameCount : textureSheetAnimation.numTilesX * textureSheetAnimation.numTilesY;

                // SPE 'textureFrameLoop' means how many times to play the animation over the particle's lifetime
                textureSheetAnimation.cycleCount = json.TextureFrameLoop;
                textureSheetAnimation.animation = ParticleSystemAnimationType.WholeSheet;

                // Set the frame over time curve to map through all frames
                var curve = new AnimationCurve();
                curve.AddKey(0f, 0f);
                curve.AddKey(1f, (float)frameCount / (textureSheetAnimation.numTilesX * textureSheetAnimation.numTilesY));
                textureSheetAnimation.frameOverTime = new ParticleSystem.MinMaxCurve(1f, curve);
            }
            else
            {
                var textureSheetAnimation = ps.textureSheetAnimation;
                textureSheetAnimation.enabled = false;
            }

            // Texture support
            Material mat = psr.material;
            if (mat == null || mat.name == "Default-Material" || mat.shader.name == "Standard")
            {
                // Ensure a particle material is used if Unity assigned a default Lit material
                mat = new Material(ArenaUnity.GetParticleShader());
                psr.material = mat;
            }

            if (!string.IsNullOrEmpty(json.Texture))
            {
                string assetPath = ArenaClientScene.Instance.checkLocalAsset((string)json.Texture);
                if (assetPath != null)
                {
                    mat.shader = ArenaUnity.GetParticleShader();
                    ArenaMaterial.AttachMaterialTexture(assetPath, gameObject);

                    // URP Particle shaders often use _BaseMap instead of _MainTex
                    if (mat.HasProperty("_BaseMap") && mat.mainTexture != null)
                    {
                        mat.SetTexture("_BaseMap", mat.mainTexture);
                    }
                }
            }

            bool isURP = ArenaUnity.DefaultRenderPipeline != null;
            if (json.UseTransparency)
            {
                mat.SetOverrideTag("RenderType", "Transparent");
                if (isURP) mat.SetFloat("_Surface", 1f); // URP Transparent
                mat.renderQueue = 3000;
            }

            switch(json.Blending)
            {
                case ArenaSpeParticlesJson.BlendingType.Additive:
                    if (isURP) { mat.SetFloat("_Mode", 2f); mat.SetFloat("_Blend", 2f); }
                    else mat.SetFloat("_Mode", 3f); // Transparent in standard
                    mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    if (json.UseTransparency) {
                        if (isURP) mat.EnableKeyword("_ADDITIVEBLEND");
                        else mat.EnableKeyword("_ALPHABLEND_ON");
                    }
                    break;
                case ArenaSpeParticlesJson.BlendingType.Multiply:
                    if (isURP) { mat.SetFloat("_Mode", 2f); mat.SetFloat("_Blend", 3f); }
                    else mat.SetFloat("_Mode", 3f); // Transparent in standard
                    mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.DstColor);
                    mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                    mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    if (json.UseTransparency) {
                        if (isURP) mat.EnableKeyword("_MULTIPLYBLEND");
                        else mat.EnableKeyword("_ALPHABLEND_ON");
                    }
                    break;
                case ArenaSpeParticlesJson.BlendingType.Subtractive:
                case ArenaSpeParticlesJson.BlendingType.Normal:
                case ArenaSpeParticlesJson.BlendingType.No:
                default:
                    if (isURP) { mat.SetFloat("_Mode", 2f); mat.SetFloat("_Blend", 0f); }
                    else mat.SetFloat("_Mode", 3f); // Transparent in standard

                    // Standard straight alpha blending for Normal/No mode
                    mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    mat.DisableKeyword("_ALPHAPREMULTIPLY_ON"); // Prevent black edges

                    if (json.UseTransparency)
                    {
                        if (isURP) mat.EnableKeyword("_ALPHABLEND_ON");
                        else mat.EnableKeyword("_ALPHABLEND_ON");
                    }
                    break;
            }

            if (json.UseTransparency)
            {
                mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                // URP requires this specific alpha test tag for standard straight transparency
                mat.SetFloat("_AlphaClip", 0f);
            }
            else
            {
                mat.SetOverrideTag("RenderType", "Opaque");
                if (isURP) mat.SetFloat("_Surface", 0f); // URP Opaque
                mat.renderQueue = -1;
                mat.SetFloat("_Mode", 0f); // Opaque in standard
                mat.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
                mat.DisableKeyword("_ADDITIVEBLEND");
                mat.DisableKeyword("_MULTIPLYBLEND");
                mat.DisableKeyword("_ALPHABLEND_ON");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            }

            if (json.DepthTest)
                mat.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.LessEqual);
            else
                mat.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Always);

            if (json.DepthWrite)
                mat.SetFloat("_ZWrite", 1f);
            else
                mat.SetFloat("_ZWrite", 0f);

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
