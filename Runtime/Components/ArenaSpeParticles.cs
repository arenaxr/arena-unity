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
        // DONE: accelerationSpread
        // TODO: activeMultiplier
        // TODO: affectedByFog
        // TODO: alphaTest
        // TODO: angle
        // TODO: angleSpread
        // DONE: blending
        // DONE: color
        // DONE: colorSpread
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
        // DONE: opacitySpread
        // DONE: particleCount
        // DONE: positionDistribution
        // DONE: positionOffset
        // DONE: positionSpread
        // DONE: radius
        // DONE: radiusScale
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
        // DONE: sizeSpread
        // DONE: texture
        // DONE: textureFrameCount
        // DONE: textureFrameLoop
        // DONE: textureFrames
        // DONE: useTransparency
        // DONE: velocity
        // DONE: velocityDistribution
        // DONE: velocitySpread
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
                {
                    float startSize = (float)json.Size[0].Value;
                    main.startSize = startSize;

                    if (json.Size.Length > 1 || (json.SizeSpread != null && json.SizeSpread.Length > 0))
                    {
                        var sizeOverLifetime = ps.sizeOverLifetime;
                        sizeOverLifetime.enabled = true;

                        int len = Mathf.Max(json.Size.Length, json.SizeSpread != null ? json.SizeSpread.Length : 0);
                        var curveMin = new AnimationCurve();
                        var curveMax = new AnimationCurve();

                        for (int i = 0; i < len; i++)
                        {
                            float t = len == 1 ? 0f : (float)i / (len - 1);

                            float sVal = 1f;
                            if (json.Size.Length > 0) {
                                int sIdx = Mathf.Min(i, json.Size.Length - 1);
                                if (json.Size[sIdx].HasValue)
                                    sVal = startSize != 0 ? (float)json.Size[sIdx].Value / startSize : 0;
                            }

                            float spreadVal = 0f;
                            if (json.SizeSpread != null && json.SizeSpread.Length > 0) {
                                int spIdx = Mathf.Min(i, json.SizeSpread.Length - 1);
                                if (json.SizeSpread[spIdx].HasValue)
                                    spreadVal = startSize != 0 ? (float)json.SizeSpread[spIdx].Value / startSize : 0;
                            }

                            curveMin.AddKey(t, Mathf.Max(0, sVal - spreadVal));
                            curveMax.AddKey(t, sVal + spreadVal);
                        }
                        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, curveMin, curveMax);
                    }
                }
            }

            if (json.Color != null && json.Color.Length > 0)
            {
                ColorUtility.TryParseHtmlString(json.Color[0], out Color startColor);
                if (json.Opacity != null && json.Opacity.Length > 0 && json.Opacity[0].HasValue)
                    startColor.a = json.Opacity[0].Value;
                main.startColor = startColor;

                bool hasColorSpread = json.ColorSpread != null && json.ColorSpread.Length > 0;
                bool hasOpacitySpread = json.OpacitySpread != null && json.OpacitySpread.Length > 0;

                if (json.Color.Length > 1 || (json.Opacity != null && json.Opacity.Length > 1) || hasColorSpread || hasOpacitySpread)
                {
                    var colorOverLifetime = ps.colorOverLifetime;
                    colorOverLifetime.enabled = true;

                    int colorLen = Mathf.Max(json.Color.Length, hasColorSpread ? json.ColorSpread.Length : 0);
                    int alphaLen = Mathf.Max(json.Opacity != null ? json.Opacity.Length : 0, hasOpacitySpread ? json.OpacitySpread.Length : 0);

                    // Unity requires at least 2 keys
                    int cKeysLength = Mathf.Max(2, colorLen);
                    int aKeysLength = Mathf.Max(2, alphaLen);

                    Gradient gMin = new Gradient();
                    Gradient gMax = new Gradient();
                    var colorKeysMin = new GradientColorKey[cKeysLength];
                    var colorKeysMax = new GradientColorKey[cKeysLength];
                    var alphaKeysMin = new GradientAlphaKey[aKeysLength];
                    var alphaKeysMax = new GradientAlphaKey[aKeysLength];

                    for (int i = 0; i < cKeysLength; i++) {
                        float t = (cKeysLength == 1) ? 0f : (float)i / (cKeysLength - 1);

                        int cIdx = Mathf.Min(i, json.Color.Length - 1);
                        Color c = startColor;
                        if (cIdx >= 0) ColorUtility.TryParseHtmlString(json.Color[cIdx], out c);

                        float sr = 0, sg = 0, sb = 0;
                        if (hasColorSpread) {
                            int sIdx = Mathf.Min(i, json.ColorSpread.Length - 1);
                            if (json.ColorSpread[sIdx] != null) {
                                sr = (float)json.ColorSpread[sIdx].X;
                                sg = (float)json.ColorSpread[sIdx].Y;
                                sb = (float)json.ColorSpread[sIdx].Z;
                            }
                        }

                        Color cMin = new Color(Mathf.Clamp01(c.r - sr), Mathf.Clamp01(c.g - sg), Mathf.Clamp01(c.b - sb));
                        Color cMax = new Color(Mathf.Clamp01(c.r + sr), Mathf.Clamp01(c.g + sg), Mathf.Clamp01(c.b + sb));
                        colorKeysMin[i] = new GradientColorKey(cMin, t);
                        colorKeysMax[i] = new GradientColorKey(cMax, t);
                    }

                    for (int i = 0; i < aKeysLength; i++) {
                        float t = (aKeysLength == 1) ? 0f : (float)i / (aKeysLength - 1);

                        float a = startColor.a;
                        if (json.Opacity != null && json.Opacity.Length > 0) {
                            int aIdx = Mathf.Min(i, json.Opacity.Length - 1);
                            if (json.Opacity[aIdx].HasValue) a = json.Opacity[aIdx].Value;
                        }

                        float spreadA = 0f;
                        if (hasOpacitySpread) {
                            int sIdx = Mathf.Min(i, json.OpacitySpread.Length - 1);
                            if (json.OpacitySpread[sIdx].HasValue) spreadA = json.OpacitySpread[sIdx].Value;
                        }

                        alphaKeysMin[i] = new GradientAlphaKey(Mathf.Clamp01(a - spreadA), t);
                        alphaKeysMax[i] = new GradientAlphaKey(Mathf.Clamp01(a + spreadA), t);
                    }

                    gMin.SetKeys(colorKeysMin, alphaKeysMin);
                    gMax.SetKeys(colorKeysMax, alphaKeysMax);

                    if (!hasColorSpread && !hasOpacitySpread) {
                        colorOverLifetime.color = new ParticleSystem.MinMaxGradient(gMin);
                    } else {
                        colorOverLifetime.color = new ParticleSystem.MinMaxGradient(gMin, gMax);
                    }
                }
            }

            main.maxParticles = json.ParticleCount;
            // TODO: handle ActiveMultiplier

            var emission = ps.emission;
            emission.enabled = true;
            // Rough approximation of rate, A-Frame SPE is more complex
            emission.rateOverTime = json.ParticleCount / json.MaxAge;

            // Position Distribution (Shape)
            var shape = ps.shape;
            shape.enabled = true;

            // SPE defaults to 'Distribution' if 'PositionDistribution' is None
            var posDist = json.PositionDistribution != ArenaSpeParticlesJson.PositionDistributionType.None ?
                          (ArenaSpeParticlesJson.DistributionType)(int)json.PositionDistribution :
                          json.Distribution;

            switch (posDist)
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

            // Velocity Distribution
            var velDist = json.VelocityDistribution != ArenaSpeParticlesJson.VelocityDistributionType.None ?
                          (ArenaSpeParticlesJson.DistributionType)(int)json.VelocityDistribution :
                          json.Distribution;

            var velocityOverLifetime = ps.velocityOverLifetime;
            var forceOverLifetime = ps.forceOverLifetime;

            if (velDist == ArenaSpeParticlesJson.DistributionType.Box)
            {
                // Box / None: Particles move along the explicit Velocity/Acceleration vectors
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
            }
            else
            {
                // Sphere / Disc: Velocity dictates "Speed" outwards from center, not rigid XYZ vectors
                // A-Frame SPE only uses Velocity.X for speed in these distributions.
                velocityOverLifetime.enabled = false;
                forceOverLifetime.enabled = false;

                var speedCurve = new ParticleSystem.MinMaxCurve(
                    (float)(json.Velocity.X - json.VelocitySpread.X / 2.0),
                    (float)(json.Velocity.X + json.VelocitySpread.X / 2.0)
                );

                main.startSpeed = speedCurve;

                // Acceleration mapping for radial shapes is more complex in Shuriken,
                // roughly mapped to radial multiplier or linear velocity over time.
                if (json.Acceleration.X != 0 || json.AccelerationSpread.X != 0) {
                    var radialVel = velocityOverLifetime;
                    radialVel.enabled = true;
                    radialVel.radial = new ParticleSystem.MinMaxCurve(
                        (float)(json.Acceleration.X - json.AccelerationSpread.X / 2.0),
                        (float)(json.Acceleration.X + json.AccelerationSpread.X / 2.0));
                }
            }

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
