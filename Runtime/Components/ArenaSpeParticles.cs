/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2023, Carnegie Mellon University. All rights reserved.
 */

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
        // DONE: accelerationDistribution
        // DONE: accelerationSpread
        // DONE: activeMultiplier
        // DONE: affectedByFog
        // DONE: alphaTest
        // DONE: angle
        // DONE: angleSpread
        // DONE: blending
        // DONE: color
        // DONE: colorSpread
        // DONE: depthTest
        // DONE: depthWrite
        // DONE: direction
        // DONE: distribution
        // DONE: drag
        // DONE: dragSpread
        // DONE: duration
        // DONE: emitterScale
        // DONE: enableInEditor
        // DONE: enabled
        // DONE: frustumCulled
        // DONE: hasPerspective
        // DONE: maxAge
        // DONE: maxAgeSpread
        // DONE: opacity
        // DONE: opacitySpread
        // DONE: particleCount
        // DONE: positionDistribution
        // DONE: positionOffset
        // DONE: positionSpread
        // DONE: radius
        // DONE: radiusScale
        // DONE: randomizeAcceleration
        // DONE: randomizeAngle
        // DONE: randomizeColor
        // DONE: randomizeDrag
        // DONE: randomizeOpacity
        // DONE: randomizePosition
        // DONE: randomizeRotation
        // DONE: randomizeSize
        // DONE: randomizeVelocity
        // DONE: relative
        // DONE: rotation
        // DONE: rotationAxis
        // DONE: rotationAxisSpread
        // DONE: rotationSpread
        // DONE: rotationStatic
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
        // DONE: wiggle
        // DONE: wiggleSpread

        public ArenaSpeParticlesJson json = new ArenaSpeParticlesJson();
        private ParticleSystem ps;
        private ParticleSystemRenderer psr;


        protected override void ApplyRender()
        {
            ps = gameObject.GetComponent<ParticleSystem>();
            if (ps == null)
            {
                ps = gameObject.AddComponent<ParticleSystem>();
            }
            psr = gameObject.GetComponent<ParticleSystemRenderer>();

            // Stop and clear existing particles before reconfiguring to prevent
            // stale particles from persisting with old shape/velocity settings
            ps.Stop();
            ps.Clear();

            var main = ps.main;

            main.loop = json.Duration < 0;
            if (json.Duration > 0)
                main.duration = json.Duration;

            if (json.MaxAgeSpread > 0)
                main.startLifetime = new ParticleSystem.MinMaxCurve(
                    Mathf.Max(0f, json.MaxAge - json.MaxAgeSpread / 2f),
                    json.MaxAge + json.MaxAgeSpread / 2f);
            else
                main.startLifetime = json.MaxAge;

            main.playOnAwake = json.Enabled;

            // Unity's default StartSpeed is often 5, which blows the shape outwards uncontrollably.
            // A-Frame explicitly controls velocity via VelocityOverLifetime or Custom radial vectors.
            main.startSpeed = 0f;

            // Global sizing ratio to convert A-Frame 'size' parameter space (+ EmitterScale global scalar) into Unity meters.
            // A-Frame SPE size units are not 1:1 with Unity meters. Calibrated from visual comparison.
            float globalScaleRatio = (json.EmitterScale / 100f) * 0.1f;

            // Angle (Initial 2D Rotation)
            if (json.Angle != null && json.Angle.Length > 0 && json.Angle[0].HasValue)
            {
                float angle = json.Angle[0].Value;
                float angleSpread = 0f;
                if (json.AngleSpread != null && json.AngleSpread.Length > 0 && json.AngleSpread[0].HasValue)
                    angleSpread = json.AngleSpread[0].Value;

                main.startRotation3D = false;
                if (angleSpread > 0)
                {
                    main.startRotation = new ParticleSystem.MinMaxCurve(angle - angleSpread / 2f, angle + angleSpread / 2f);
                }
                else
                {
                    main.startRotation = angle;
                }
            }

            // EnableInEditor: play particles in editor when not in play mode
#if UNITY_EDITOR
            if (json.EnableInEditor && !Application.isPlaying)
            {
                if (!ps.isPlaying) ps.Play();
            }
#endif

            if (json.Size != null && json.Size.Length > 0)
            {
                if (json.Size[0].HasValue)
                {
                    float startSize = (float)json.Size[0].Value;

                    if (json.Size.Length > 1)
                    {
                        // Find the maximum size value across the lifetime to use as the normalization reference.
                        // This avoids division-by-zero when startSize == 0 (e.g. size: [0, 2, 0]).
                        float maxSize = startSize;
                        for (int i = 1; i < json.Size.Length; i++)
                        {
                            if (json.Size[i].HasValue)
                                maxSize = Mathf.Max(maxSize, (float)json.Size[i].Value);
                        }

                        // Use the max value as the Unity startSize so the curve can normalize against it
                        main.startSize = maxSize * globalScaleRatio;
                        var sizeOverLifetime = ps.sizeOverLifetime;
                        sizeOverLifetime.enabled = true;

                        int len = Mathf.Max(json.Size.Length, json.SizeSpread != null ? json.SizeSpread.Length : 0);
                        var curveMin = new AnimationCurve();
                        var curveMax = new AnimationCurve();

                        for (int i = 0; i < len; i++)
                        {
                            float t = len == 1 ? 0f : (float)i / (len - 1);

                            float sVal = 1f;
                            int sIdx = Mathf.Min(i, json.Size.Length - 1);
                            if (json.Size[sIdx].HasValue)
                                sVal = maxSize != 0 ? (float)json.Size[sIdx].Value / maxSize : 0;

                            float spreadVal = 0f;
                            if (json.SizeSpread != null && json.SizeSpread.Length > 0) {
                                int spIdx = Mathf.Min(i, json.SizeSpread.Length - 1);
                                if (json.SizeSpread[spIdx].HasValue)
                                    spreadVal = maxSize != 0 ? (float)json.SizeSpread[spIdx].Value / maxSize : 0;
                            }

                            curveMin.AddKey(t, Mathf.Max(0, sVal - spreadVal / 2f));
                            curveMax.AddKey(t, sVal + spreadVal / 2f);
                        }
                        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, curveMin, curveMax);
                    }
                    else
                    {
                        var sizeOverLifetime = ps.sizeOverLifetime;
                        sizeOverLifetime.enabled = false;

                        float spreadVal = 0f;
                        if (json.SizeSpread != null && json.SizeSpread.Length > 0 && json.SizeSpread[0].HasValue) {
                            spreadVal = (float)json.SizeSpread[0].Value;
                        }

                        if (spreadVal > 0)
                        {
                            main.startSize = new ParticleSystem.MinMaxCurve(
                                Mathf.Max(0, startSize - spreadVal / 2f) * globalScaleRatio,
                                (startSize + spreadVal / 2f) * globalScaleRatio
                            );
                        }
                        else
                        {
                            main.startSize = startSize * globalScaleRatio;
                        }
                    }
                }
            }

            if (json.Color != null && json.Color.Length > 0)
            {
                Color startColor = ArenaUnity.ToUnityColor(json.Color[0]);
                if (json.Opacity != null && json.Opacity.Length > 0 && json.Opacity[0].HasValue)
                    startColor.a = json.Opacity[0].Value;

                bool hasColorSpread = json.ColorSpread != null && json.ColorSpread.Length > 0;
                bool hasOpacitySpread = json.OpacitySpread != null && json.OpacitySpread.Length > 0;

                if (json.Color.Length > 1 || (json.Opacity != null && json.Opacity.Length > 1))
                {
                    // For arrays across lifetime, explicit gradient evaluation requires the particle core color
                    // to not multiply the gradient darker. Prevent Blackout bug.
                    main.startColor = Color.white;

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
                        if (cIdx >= 0) c = ArenaUnity.ToUnityColor(json.Color[cIdx]);

                        float sr = 0, sg = 0, sb = 0;
                        if (hasColorSpread) {
                            int sIdx = Mathf.Min(i, json.ColorSpread.Length - 1);
                            if (json.ColorSpread[sIdx] != null) {
                                sr = (float)json.ColorSpread[sIdx].X;
                                sg = (float)json.ColorSpread[sIdx].Y;
                                sb = (float)json.ColorSpread[sIdx].Z;
                            }
                        }

                        Color cMin = new Color(Mathf.Clamp01(c.r - sr / 2f), Mathf.Clamp01(c.g - sg / 2f), Mathf.Clamp01(c.b - sb / 2f));
                        Color cMax = new Color(Mathf.Clamp01(c.r + sr / 2f), Mathf.Clamp01(c.g + sg / 2f), Mathf.Clamp01(c.b + sb / 2f));
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

                        alphaKeysMin[i] = new GradientAlphaKey(Mathf.Clamp01(a - spreadA / 2f), t);
                        alphaKeysMax[i] = new GradientAlphaKey(Mathf.Clamp01(a + spreadA / 2f), t);
                    }

                    gMin.SetKeys(colorKeysMin, alphaKeysMin);
                    gMax.SetKeys(colorKeysMax, alphaKeysMax);

                    if (!hasColorSpread && !hasOpacitySpread) {
                        colorOverLifetime.color = new ParticleSystem.MinMaxGradient(gMin);
                    } else {
                        colorOverLifetime.color = new ParticleSystem.MinMaxGradient(gMin, gMax);
                    }
                }
                else
                {
                    var colorOverLifetime = ps.colorOverLifetime;
                    colorOverLifetime.enabled = false;

                    if (hasColorSpread || hasOpacitySpread)
                    {
                        float sr = 0, sg = 0, sb = 0, spreadA = 0;
                        if (hasColorSpread && json.ColorSpread[0] != null) {
                            sr = (float)json.ColorSpread[0].X;
                            sg = (float)json.ColorSpread[0].Y;
                            sb = (float)json.ColorSpread[0].Z;
                        }
                        if (hasOpacitySpread && json.OpacitySpread[0].HasValue) {
                            spreadA = json.OpacitySpread[0].Value;
                        }

                        Color cMin = new Color(Mathf.Clamp01(startColor.r - sr / 2f), Mathf.Clamp01(startColor.g - sg / 2f), Mathf.Clamp01(startColor.b - sb / 2f), Mathf.Clamp01(startColor.a - spreadA / 2f));
                        Color cMax = new Color(Mathf.Clamp01(startColor.r + sr / 2f), Mathf.Clamp01(startColor.g + sg / 2f), Mathf.Clamp01(startColor.b + sb / 2f), Mathf.Clamp01(startColor.a + spreadA / 2f));

                        // Use Gradient-based MinMaxGradient so Unity picks random color points
                        // between the two gradients, allowing per-channel variation (true multicolor)
                        // instead of simple interpolation between two flat colors (which produces grayscale).
                        Gradient gMinSingle = new Gradient();
                        gMinSingle.SetKeys(
                            new GradientColorKey[] { new GradientColorKey(cMin, 0f), new GradientColorKey(cMin, 1f) },
                            new GradientAlphaKey[] { new GradientAlphaKey(cMin.a, 0f), new GradientAlphaKey(cMin.a, 1f) }
                        );
                        Gradient gMaxSingle = new Gradient();
                        gMaxSingle.SetKeys(
                            new GradientColorKey[] { new GradientColorKey(cMax, 0f), new GradientColorKey(cMax, 1f) },
                            new GradientAlphaKey[] { new GradientAlphaKey(cMax.a, 0f), new GradientAlphaKey(cMax.a, 1f) }
                        );
                        main.startColor = new ParticleSystem.MinMaxGradient(gMinSingle, gMaxSingle);
                    }
                    else
                    {
                         main.startColor = startColor;
                    }
                }
            }

            main.maxParticles = json.ParticleCount;

            var emission = ps.emission;
            emission.enabled = true;
            // ActiveMultiplier scales emission rate; values > 1 create burst-like behavior
            emission.rateOverTime = (json.ParticleCount / json.MaxAge) * json.ActiveMultiplier;

            // Direction: backward is handled after velocity/acceleration setup by adding
            // a negative radial velocity to pull particles inward.
            bool isBackward = json.Direction == ArenaSpeParticlesJson.DirectionType.Backward;

            // Position Distribution (Shape)
            var shape = ps.shape;

            // SPE defaults to 'Distribution' if 'PositionDistribution' is None
            var posDist = json.PositionDistribution != ArenaSpeParticlesJson.PositionDistributionType.None ?
                          (ArenaSpeParticlesJson.DistributionType)(int)json.PositionDistribution :
                          json.Distribution;

            // In A-Frame SPE, box distribution with positionSpread {0,0,0} emits from a single point.
            // Disable the shape module for point-source emission (particles spawn at the transform origin).
            bool isPointSource = (posDist == ArenaSpeParticlesJson.DistributionType.Box &&
                                  json.PositionSpread.X == 0 && json.PositionSpread.Y == 0 && json.PositionSpread.Z == 0) ||
                                 ((posDist == ArenaSpeParticlesJson.DistributionType.Sphere ||
                                   posDist == ArenaSpeParticlesJson.DistributionType.Disc) && json.Radius == 0);

            if (isPointSource)
            {
                shape.enabled = false;
            }
            else
            {
                shape.enabled = true;
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
            }
            shape.position = ArenaUnity.ToUnityPosition(json.PositionOffset);

            // Velocity Distribution
            var velDist = json.VelocityDistribution != ArenaSpeParticlesJson.VelocityDistributionType.None ?
                          (ArenaSpeParticlesJson.DistributionType)(int)json.VelocityDistribution :
                          json.Distribution;

            // Acceleration Distribution
            var accelDist = json.AccelerationDistribution != ArenaSpeParticlesJson.AccelerationDistributionType.None ?
                            (ArenaSpeParticlesJson.DistributionType)(int)json.AccelerationDistribution :
                            json.Distribution;

            var velocityOverLifetime = ps.velocityOverLifetime;
            var forceOverLifetime = ps.forceOverLifetime;

            // Explicitly set velocity/force space to Local to match SPE's emitter-relative behavior
            velocityOverLifetime.space = ParticleSystemSimulationSpace.Local;
            forceOverLifetime.space = ParticleSystemSimulationSpace.Local;

            float maxAge = json.MaxAge > 0 ? json.MaxAge : 1f;

            // 1. Velocity (Initial Impulse)
            velocityOverLifetime.enabled = true;
            velocityOverLifetime.space = ParticleSystemSimulationSpace.Local;

            if (velDist == ArenaSpeParticlesJson.DistributionType.Box)
            {
                main.startSpeed = 0f;
                float signZ = -1f; // Flip Z
                float vX = (float)json.Velocity.X;
                float vY = (float)json.Velocity.Y;
                float vZ = (float)json.Velocity.Z;
                float sX = (float)json.VelocitySpread.X;
                float sY = (float)json.VelocitySpread.Y;
                float sZ = (float)json.VelocitySpread.Z;

                if (isBackward) { vX = -vX; vY = -vY; vZ = -vZ; }

                velocityOverLifetime.x = new ParticleSystem.MinMaxCurve(vX - sX/2f, vX + sX/2f);
                velocityOverLifetime.y = new ParticleSystem.MinMaxCurve(vY - sY/2f, vY + sY/2f);
                velocityOverLifetime.z = new ParticleSystem.MinMaxCurve((vZ - sZ/2f) * signZ, (vZ + sZ/2f) * signZ);
                // Ensure z min is always less than max
                if (velocityOverLifetime.z.constantMin > velocityOverLifetime.z.constantMax)
                {
                    float min = velocityOverLifetime.z.constantMax;
                    float max = velocityOverLifetime.z.constantMin;
                    velocityOverLifetime.z = new ParticleSystem.MinMaxCurve(min, max);
                }
            }
            else
            {
                // Radial Velocity
                float speedMin = (float)(json.Velocity.X - json.VelocitySpread.X / 2.0);
                float speedMax = (float)(json.Velocity.X + json.VelocitySpread.X / 2.0);
                if (isBackward)
                {
                    main.startSpeed = new ParticleSystem.MinMaxCurve(-speedMax, -speedMin);
                    shape.radiusThickness = 0f; // spawn at edge
                }
                else
                {
                    main.startSpeed = new ParticleSystem.MinMaxCurve(speedMin, speedMax);
                }
                // Zero out absolute XYZ velocity to avoid overriding radial movement
                velocityOverLifetime.x = 0;
                velocityOverLifetime.y = 0;
                velocityOverLifetime.z = 0;
            }

            // 2. Acceleration (Constant force or radial ramp)
            forceOverLifetime.enabled = true;
            forceOverLifetime.space = ParticleSystemSimulationSpace.Local;

            if (accelDist == ArenaSpeParticlesJson.DistributionType.Box)
            {
                float signZ = -1f;
                float aX = (float)json.Acceleration.X;
                float aY = (float)json.Acceleration.Y;
                float aZ = (float)json.Acceleration.Z;
                float sX = (float)json.AccelerationSpread.X;
                float sY = (float)json.AccelerationSpread.Y;
                float sZ = (float)json.AccelerationSpread.Z;

                if (isBackward) { aX = -aX; aY = -aY; aZ = -aZ; }

                forceOverLifetime.x = new ParticleSystem.MinMaxCurve(aX - sX/2f, aX + sX/2f);
                forceOverLifetime.y = new ParticleSystem.MinMaxCurve(aY - sY/2f, aY + sY/2f);
                forceOverLifetime.z = new ParticleSystem.MinMaxCurve((aZ - sZ/2f) * signZ, (aZ + sZ/2f) * signZ);
                if (forceOverLifetime.z.constantMin > forceOverLifetime.z.constantMax)
                {
                    float min = forceOverLifetime.z.constantMax;
                    float max = forceOverLifetime.z.constantMin;
                    forceOverLifetime.z = new ParticleSystem.MinMaxCurve(min, max);
                }
                velocityOverLifetime.radial = 0;
            }
            else
            {
                // Radial Acceleration (Simulate as a ramped radial velocity)
                float raMin = (float)(json.Acceleration.X - json.AccelerationSpread.X / 2.0);
                float raMax = (float)(json.Acceleration.X + json.AccelerationSpread.X / 2.0);

                if (isBackward)
                {
                    float tmp = -raMax;
                    raMax = -raMin;
                    raMin = tmp;
                }

                // Ramp from 0 to actual acceleration * maxAge
                var curveMin = AnimationCurve.Linear(0, 0, 1, raMin * maxAge);
                var curveMax = AnimationCurve.Linear(0, 0, 1, raMax * maxAge);
                velocityOverLifetime.radial = new ParticleSystem.MinMaxCurve(1f, curveMin, curveMax);

                forceOverLifetime.x = 0;
                forceOverLifetime.y = 0;
                forceOverLifetime.z = 0;
            }

            // Drag (Air Resistance)
            if (json.Drag > 0 || json.DragSpread > 0)
            {
                var limitVelocity = ps.limitVelocityOverLifetime;
                limitVelocity.enabled = true;
                // In A-Frame SPE, drag is an acceleration coefficient. In Unity, we map it to Dampen
                limitVelocity.dampen = Mathf.Clamp01(json.Drag);
                // We leave the limit at a negligible multiplier to allow the dampen to strictly act as drag against existing velocity
                limitVelocity.limit = new ParticleSystem.MinMaxCurve(
                    Mathf.Max(0f, json.Drag - json.DragSpread / 2f),
                    Mathf.Max(0f, json.Drag + json.DragSpread / 2f)
                );
            }
            else
            {
                var limitVelocity = ps.limitVelocityOverLifetime;
                limitVelocity.enabled = false;
            }

            // Wiggle (Turbulence/Noise)
            if (json.Wiggle > 0 || json.WiggleSpread > 0)
            {
                var noise = ps.noise;
                noise.enabled = true;
                // Unity Noise strength is absolute world-space displacement, so we map Wiggle as strength
                noise.strength = new ParticleSystem.MinMaxCurve(
                    Mathf.Max(0f, json.Wiggle - json.WiggleSpread / 2f),
                    json.Wiggle + json.WiggleSpread / 2f
                );
                // A-Frame Wiggle is usually rapid jitter, so frequency is set reasonably high
                noise.frequency = 1.0f;
                noise.positionAmount = 1.0f;
                noise.rotationAmount = 0.0f;
                noise.sizeAmount = 0.0f;
            }
            else
            {
                var noise = ps.noise;
                noise.enabled = false;
            }

            // Rotation (Angular Velocity or Fixed 3D Rotation)
            var rotOverLifetime = ps.rotationOverLifetime;
            if (json.RotationStatic)
            {
                rotOverLifetime.enabled = false;
                if (json.Rotation != 0 || json.RotationSpread != 0)
                {
                    main.startRotation3D = true;
                    float rotRads = json.Rotation * Mathf.Deg2Rad;
                    float rotSpreadRads = json.RotationSpread * Mathf.Deg2Rad;
                    float minRot = rotRads - rotSpreadRads / 2f;
                    float maxRot = rotRads + rotSpreadRads / 2f;

                    Vector3 axis = new Vector3((float)json.RotationAxis.X, (float)json.RotationAxis.Y, (float)json.RotationAxis.Z);
                    if (axis == Vector3.zero) axis = Vector3.forward;
                    axis.Normalize();

                    main.startRotationX = new ParticleSystem.MinMaxCurve(minRot * axis.x, maxRot * axis.x);
                    main.startRotationY = new ParticleSystem.MinMaxCurve(minRot * axis.y, maxRot * axis.y);
                    main.startRotationZ = new ParticleSystem.MinMaxCurve(minRot * axis.z, maxRot * axis.z);
                }
            }
            else if (json.Rotation != 0 || json.RotationSpread != 0)
            {
                rotOverLifetime.enabled = true;
                // Unity rotation over lifetime takes radians per second in script!
                float angularVel = (json.Rotation * Mathf.Deg2Rad) / json.MaxAge;
                float angularVelSpread = (json.RotationSpread * Mathf.Deg2Rad) / json.MaxAge;

                float minVel = angularVel - angularVelSpread / 2f;
                float maxVel = angularVel + angularVelSpread / 2f;

                Vector3 axis = new Vector3((float)json.RotationAxis.X, (float)json.RotationAxis.Y, (float)json.RotationAxis.Z);
                if (axis == Vector3.zero)
                {
                    rotOverLifetime.separateAxes = false;
                    rotOverLifetime.z = new ParticleSystem.MinMaxCurve(minVel, maxVel);
                }
                else
                {
                    rotOverLifetime.separateAxes = true;
                    axis.Normalize();
                    rotOverLifetime.x = new ParticleSystem.MinMaxCurve(minVel * axis.x, maxVel * axis.x);
                    rotOverLifetime.y = new ParticleSystem.MinMaxCurve(minVel * axis.y, maxVel * axis.y);
                    rotOverLifetime.z = new ParticleSystem.MinMaxCurve(minVel * axis.z, maxVel * axis.z);
                }
            }
            else
            {
                rotOverLifetime.enabled = false;
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

            // Texture support — use sharedMaterial to avoid Unity's copy-on-access behavior
            // (renderer.material creates a new copy every access, causing property loss)
            Material mat = psr.sharedMaterial;
            if (mat == null || mat.name == "Default-Material" || mat.shader.name == "Standard")
            {
                // Ensure a particle material is used if Unity assigned a default Lit material
                mat = new Material(ArenaUnity.GetParticleShader());
                psr.sharedMaterial = mat;
            }

            if (!string.IsNullOrEmpty(json.Texture))
            {
                string assetPath = ArenaClientScene.Instance.checkLocalAsset((string)json.Texture);
                if (assetPath != null)
                {
                    mat.shader = ArenaUnity.GetParticleShader();

                    // Load texture directly onto mat to avoid material instance splits
                    if (System.IO.File.Exists(assetPath))
                    {
                        var bytes = System.IO.File.ReadAllBytes(assetPath);
                        var tex = new Texture2D(1, 1);
                        tex.LoadImage(bytes);
                        mat.mainTexture = tex;
                    }

                    // URP Particle shaders often use _BaseMap instead of _MainTex
                    if (mat.HasProperty("_BaseMap") && mat.mainTexture != null)
                    {
                        mat.SetTexture("_BaseMap", mat.mainTexture);
                    }
                }
            }
            else
            {
                // No texture specified — create a 1x1 white pixel texture to match A-Frame SPE's
                // default behavior where untextured particles render as small white squares.
                if (mat.mainTexture == null)
                {
                    var defaultTex = new Texture2D(1, 1);
                    defaultTex.SetPixel(0, 0, Color.white);
                    defaultTex.Apply();
                    mat.mainTexture = defaultTex;
                    if (mat.HasProperty("_BaseMap"))
                        mat.SetTexture("_BaseMap", defaultTex);
                }
            }

            bool isURP = ArenaUnity.DefaultRenderPipeline != null;

            // AlphaTest — alpha cutoff threshold
            if (json.AlphaTest > 0)
            {
                mat.SetFloat("_Cutoff", json.AlphaTest);
                mat.EnableKeyword("_ALPHATEST_ON");
                if (isURP) mat.SetFloat("_AlphaClip", 1f);
            }

            // AffectedByFog — toggle fog shader keyword
            if (json.AffectedByFog)
                mat.EnableKeyword("_FOG_ON");
            else
                mat.DisableKeyword("_FOG_ON");

            // Setup blending mode — for Standard (non-URP) pipeline, each case exactly mirrors
            // Unity's official StandardParticlesShaderGUI.SetupMaterialWithBlendMode
            if (json.UseTransparency)
            {
                switch(json.Blending)
                {
                    case ArenaSpeParticlesJson.BlendingType.Additive:
                        if (isURP)
                        {
                            mat.SetFloat("_Surface", 1f);
                            mat.SetFloat("_Mode", 2f);
                            mat.SetFloat("_Blend", 2f);
                            mat.SetOverrideTag("RenderType", "Transparent");
                            mat.SetInt("_BlendOp", (int)UnityEngine.Rendering.BlendOp.Add);
                            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
                            mat.SetInt("_ZWrite", 0);
                            mat.EnableKeyword("_ADDITIVEBLEND");
                            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                            mat.SetFloat("_AlphaClip", 0f);
                            mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                        }
                        else
                        {
                            // Unity Particle BlendMode.Additive
                            mat.SetOverrideTag("RenderType", "Transparent");
                            mat.SetInt("_BlendOp", (int)UnityEngine.Rendering.BlendOp.Add);
                            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
                            mat.SetInt("_ZWrite", 0);
                            mat.DisableKeyword("_ALPHATEST_ON");
                            mat.EnableKeyword("_ALPHABLEND_ON");
                            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                            mat.DisableKeyword("_ALPHAMODULATE_ON");
                            mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                        }
                        break;

                    case ArenaSpeParticlesJson.BlendingType.Multiply:
                        if (isURP)
                        {
                            mat.SetFloat("_Surface", 1f);
                            mat.SetFloat("_Mode", 2f);
                            mat.SetFloat("_Blend", 3f);
                            mat.SetOverrideTag("RenderType", "Transparent");
                            mat.SetInt("_BlendOp", (int)UnityEngine.Rendering.BlendOp.Add);
                            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.DstColor);
                            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                            mat.SetInt("_ZWrite", 0);
                            mat.EnableKeyword("_MULTIPLYBLEND");
                            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                            mat.SetFloat("_AlphaClip", 0f);
                            mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                        }
                        else
                        {
                            // Unity Particle BlendMode.Modulate
                            mat.SetOverrideTag("RenderType", "Transparent");
                            mat.SetInt("_BlendOp", (int)UnityEngine.Rendering.BlendOp.Add);
                            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.DstColor);
                            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                            mat.SetInt("_ZWrite", 0);
                            mat.DisableKeyword("_ALPHATEST_ON");
                            mat.DisableKeyword("_ALPHABLEND_ON");
                            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                            mat.EnableKeyword("_ALPHAMODULATE_ON");
                            mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                        }
                        break;

                    case ArenaSpeParticlesJson.BlendingType.Subtractive:
                        if (isURP)
                        {
                            mat.SetFloat("_Surface", 1f);
                            mat.SetFloat("_Mode", 2f);
                            mat.SetFloat("_Blend", 0f);
                            mat.SetOverrideTag("RenderType", "Transparent");
                            mat.SetInt("_BlendOp", (int)UnityEngine.Rendering.BlendOp.ReverseSubtract);
                            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
                            mat.SetInt("_ZWrite", 0);
                            mat.EnableKeyword("_ALPHABLEND_ON");
                            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                            mat.SetFloat("_AlphaClip", 0f);
                            mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                        }
                        else
                        {
                            // Unity Particle BlendMode.Subtractive
                            mat.SetOverrideTag("RenderType", "Transparent");
                            mat.SetInt("_BlendOp", (int)UnityEngine.Rendering.BlendOp.ReverseSubtract);
                            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
                            mat.SetInt("_ZWrite", 0);
                            mat.DisableKeyword("_ALPHATEST_ON");
                            mat.EnableKeyword("_ALPHABLEND_ON");
                            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                            mat.DisableKeyword("_ALPHAMODULATE_ON");
                            mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                        }
                        break;

                    case ArenaSpeParticlesJson.BlendingType.Normal:
                    case ArenaSpeParticlesJson.BlendingType.No:
                    default:
                        if (isURP)
                        {
                            mat.SetFloat("_Surface", 1f);
                            mat.SetFloat("_Mode", 2f);
                            mat.SetFloat("_Blend", 0f);
                            mat.SetOverrideTag("RenderType", "Transparent");
                            mat.SetInt("_BlendOp", (int)UnityEngine.Rendering.BlendOp.Add);
                            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                            mat.SetInt("_ZWrite", 0);
                            mat.EnableKeyword("_ALPHABLEND_ON");
                            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                            mat.SetFloat("_AlphaClip", 0f);
                            mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                        }
                        else
                        {
                            // Unity Particle BlendMode.Fade — standard alpha blending
                            mat.SetOverrideTag("RenderType", "Transparent");
                            mat.SetInt("_BlendOp", (int)UnityEngine.Rendering.BlendOp.Add);
                            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                            mat.SetInt("_ZWrite", 0);
                            mat.DisableKeyword("_ALPHATEST_ON");
                            mat.EnableKeyword("_ALPHABLEND_ON");
                            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                            mat.DisableKeyword("_ALPHAMODULATE_ON");
                            mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                        }
                        break;
                }
            }
            else
            {
                // Opaque — no transparency
                mat.SetOverrideTag("RenderType", "");
                if (isURP) mat.SetFloat("_Surface", 0f);
                mat.SetInt("_BlendOp", (int)UnityEngine.Rendering.BlendOp.Add);
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                mat.SetInt("_ZWrite", 1);
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.DisableKeyword("_ALPHABLEND_ON");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.DisableKeyword("_ALPHAMODULATE_ON");
                mat.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
                mat.DisableKeyword("_ADDITIVEBLEND");
                mat.DisableKeyword("_MULTIPLYBLEND");
                mat.renderQueue = -1;
            }

            if (json.DepthTest)
                mat.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.LessEqual);
            else
                mat.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.Always);

            // Allow explicit DepthWrite override from JSON (default is false)
            if (json.DepthWrite)
                mat.SetInt("_ZWrite", 1);

            // FrustumCulled — when false, expand bounds so particles are never culled off-screen
            if (!json.FrustumCulled)
            {
                var bounds = new Bounds(Vector3.zero, Vector3.one * 10000f);
                psr.bounds = bounds;
            }

            // HasPerspective — when false, particles should maintain constant screen-space size
            // regardless of distance (SPE shader: perspective = 1.0, gl_PointSize = size in pixels).
            // Unity minParticleSize/maxParticleSize are fractions of viewport height.
            // Convert the raw SPE size (pixels) to a viewport-height fraction.
            if (!json.HasPerspective)
            {
                float speSize = (json.Size != null && json.Size.Length > 0 && json.Size[0].HasValue)
                    ? json.Size[0].Value : 1f;
                // Approximate viewport height; use Screen.height when available, fallback to 1080
                float viewportHeight = Screen.height > 0 ? Screen.height : 1080f;
                float screenFraction = Mathf.Clamp(speSize / viewportHeight, 0.001f, 0.5f);
                psr.minParticleSize = screenFraction;
                psr.maxParticleSize = screenFraction;
            }

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
                    aobj.PublishUpdate($"{{\"{json.attributeName}\":{newJson}}}");
                    apply = true;
                }
            }
            updatedJson = newJson;
        }
    }
}
