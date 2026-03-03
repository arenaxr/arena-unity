/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2023, Carnegie Mellon University. All rights reserved.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using ArenaUnity.Schemas;
using Newtonsoft.Json;
using UnityEngine;

namespace ArenaUnity.Components
{
    public class ArenaAnimation : ArenaComponent
    {
        // ARENA animation component unity conversion status:
        // DONE: autoplay
        // DONE: delay
        // DONE: dir (normal, reverse, alternate)
        // DONE: dur
        // DONE: easing (all 28 types + linear)
        // DONE: elasticity
        // DONE: enabled
        // DONE: from
        // TODO: isRawProperty (requires generic property path resolution)
        // DONE: loop
        // TODO: pauseEvents (requires ARENA event bus integration)
        // DONE: property (position, rotation, scale, material.color, material.opacity)
        // TODO: resumeEvents (requires ARENA event bus integration)
        // DONE: round
        // TODO: startEvents (requires ARENA event bus integration)
        // DONE: to
        // TODO: type (color type hint, currently inferred from property)

        // TODO: multiple animations per object (animation__name syntax)

        public ArenaAnimationJson json = new ArenaAnimationJson();

        private Coroutine tweenCoroutine;
        private bool isTransformProperty = false;
        private int axisIndex = -1; // 0=x, 1=y, 2=z for single-axis properties

        protected override void ApplyRender()
        {
            // Stop any existing tween
            StopTween();

            if (!json.Enabled)
                return;

            // Determine if this animation targets a transform property
            isTransformProperty = IsTransformProperty(json.Property);
            axisIndex = GetAxisIndex(json.Property);

            // Suppress transform publishing for the entire duration of the animation
            if (isTransformProperty)
                SetSuppressTransform(true);

            if (json.Autoplay)
            {
                tweenCoroutine = StartCoroutine(RunTween());
            }
            // TODO: if !autoplay, wait for startEvents to trigger
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

        private void OnDisable()
        {
            StopTween();
        }

        private void OnDestroy()
        {
            StopTween();
        }

        private void StopTween()
        {
            if (tweenCoroutine != null)
            {
                StopCoroutine(tweenCoroutine);
                tweenCoroutine = null;
            }
            if (isTransformProperty)
                SetSuppressTransform(false);
        }

        private void SetSuppressTransform(bool suppress)
        {
            var aobj = GetComponent<ArenaObject>();
            if (aobj != null)
                aobj.suppressTransformPublish = suppress;
        }

        // ==========================================
        // Tween Engine
        // ==========================================

        private IEnumerator RunTween()
        {
            // Delay before starting
            if (json.Delay > 0)
                yield return new WaitForSeconds(json.Delay / 1000f);

            float durSeconds = json.Dur / 1000f;
            if (durSeconds <= 0) durSeconds = 0.001f;

            // Parse loop count: "true" = infinite, "0"/"false" = no loop, number = repeat count
            int loopCount = ParseLoopCount(json.Loop);

            // Parse from/to values based on the property being animated
            PropertyType propType = GetPropertyType(json.Property);
            object fromValue = null;
            object toValue = null;

            if (!string.IsNullOrEmpty(json.From))
                fromValue = ParseValue(json.From, propType);
            else
                fromValue = SampleCurrentValue(propType);

            if (!string.IsNullOrEmpty(json.To))
                toValue = ParseValue(json.To, propType);
            else
                toValue = SampleCurrentValue(propType);

            if (fromValue == null || toValue == null)
            {
                Debug.LogWarning($"ArenaAnimation: Could not parse from/to for property '{json.Property}' on '{gameObject.name}'");
                yield break;
            }

            int iteration = 0;
            bool infinite = loopCount < 0;

            while (infinite || iteration <= loopCount)
            {
                // Determine direction for this iteration
                object iterFrom = fromValue;
                object iterTo = toValue;

                switch (json.Dir)
                {
                    case ArenaAnimationJson.DirType.Reverse:
                        iterFrom = toValue;
                        iterTo = fromValue;
                        break;
                    case ArenaAnimationJson.DirType.Alternate:
                        if (iteration % 2 == 1)
                        {
                            iterFrom = toValue;
                            iterTo = fromValue;
                        }
                        break;
                    case ArenaAnimationJson.DirType.Normal:
                    default:
                        break;
                }

                // Run one cycle
                float elapsed = 0f;
                while (elapsed < durSeconds)
                {
                    float t = Mathf.Clamp01(elapsed / durSeconds);
                    float easedT = ApplyEasing(t, json.Easing, json.Elasticity);

                    object interpolated = Interpolate(iterFrom, iterTo, easedT, propType);
                    if (json.Round)
                        interpolated = RoundValue(interpolated, propType);

                    ApplyValue(interpolated, propType);
                    elapsed += Time.deltaTime;
                    yield return null;
                }

                // Ensure final value is set exactly
                object finalVal = json.Round ? RoundValue(iterTo, propType) : iterTo;
                ApplyValue(finalVal, propType);

                iteration++;
            }

            tweenCoroutine = null;
        }

        // ==========================================
        // Property Type Detection
        // ==========================================

        private enum PropertyType
        {
            Position,
            PositionAxis, // position.x, position.y, position.z
            Rotation,
            RotationAxis, // rotation.x, rotation.y, rotation.z
            Scale,
            ScaleAxis,    // scale.x, scale.y, scale.z
            Color,
            Opacity,
            Float,
            Unknown
        }

        private static bool IsTransformProperty(string property)
        {
            if (string.IsNullOrEmpty(property)) return false;
            string p = property.ToLower();
            return p.StartsWith("position") || p.StartsWith("rotation") || p.StartsWith("scale") ||
                   p.StartsWith("object3d.position") || p.StartsWith("object3d.rotation") || p.StartsWith("object3d.scale");
        }

        /// <summary>
        /// Extract the axis index (0=x, 1=y, 2=z) from a dotted property like "rotation.y".
        /// Returns -1 if not a single-axis property.
        /// </summary>
        private static int GetAxisIndex(string property)
        {
            if (string.IsNullOrEmpty(property)) return -1;
            string p = property.ToLower();
            if (p.EndsWith(".x")) return 0;
            if (p.EndsWith(".y")) return 1;
            if (p.EndsWith(".z")) return 2;
            return -1;
        }

        private static PropertyType GetPropertyType(string property)
        {
            if (string.IsNullOrEmpty(property)) return PropertyType.Unknown;
            string p = property.ToLower();
            int axis = GetAxisIndex(p);

            // Position
            if (p == "position" || p == "object3d.position")
                return PropertyType.Position;
            if ((p.StartsWith("position.") || p.StartsWith("object3d.position.")) && axis >= 0)
                return PropertyType.PositionAxis;

            // Rotation
            if (p == "rotation" || p == "object3d.rotation")
                return PropertyType.Rotation;
            if ((p.StartsWith("rotation.") || p.StartsWith("object3d.rotation.")) && axis >= 0)
                return PropertyType.RotationAxis;

            // Scale
            if (p == "scale" || p == "object3d.scale")
                return PropertyType.Scale;
            if ((p.StartsWith("scale.") || p.StartsWith("object3d.scale.")) && axis >= 0)
                return PropertyType.ScaleAxis;

            if (p == "material.color" || p == "color" || p.Contains("material.color"))
                return PropertyType.Color;
            if (p == "material.opacity" || p == "opacity" ||
                p.Contains("material.opacity") || p.Contains("material.material.opacity"))
                return PropertyType.Opacity;

            // Generic float properties
            return PropertyType.Float;
        }

        // ==========================================
        // Value Parsing
        // ==========================================

        private static object ParseValue(string value, PropertyType propType)
        {
            if (string.IsNullOrEmpty(value)) return null;
            value = value.Trim();

            switch (propType)
            {
                case PropertyType.Position:
                case PropertyType.Scale:
                    return ParseVector3(value);
                case PropertyType.Rotation:
                    return ParseVector3(value); // A-Frame sends euler degrees in animation
                case PropertyType.Color:
                    return ParseColor(value);
                case PropertyType.PositionAxis:
                case PropertyType.RotationAxis:
                case PropertyType.ScaleAxis:
                case PropertyType.Opacity:
                case PropertyType.Float:
                    if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float f))
                        return f;
                    return null;
                default:
                    return null;
            }
        }

        private static Vector3? ParseVector3(string value)
        {
            // Handle JSON object format: {"x":1,"y":2,"z":3}
            if (value.TrimStart().StartsWith("{"))
            {
                try
                {
                    var obj = JsonConvert.DeserializeObject<Dictionary<string, float>>(value);
                    if (obj != null)
                    {
                        obj.TryGetValue("x", out float x);
                        obj.TryGetValue("y", out float y);
                        obj.TryGetValue("z", out float z);
                        return new Vector3(x, y, z);
                    }
                }
                catch { /* fall through to other formats */ }
            }
            // Handle space-delimited "x y z" format
            string[] parts = value.Split(new char[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 3)
            {
                if (float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out float x) &&
                    float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float y) &&
                    float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out float z))
                {
                    return new Vector3(x, y, z);
                }
            }
            // Single value — apply to all axes
            if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float single))
                return new Vector3(single, single, single);
            return null;
        }

        private static Color? ParseColor(string value)
        {
            if (ColorUtility.TryParseHtmlString(value.StartsWith("#") ? value : $"#{value}", out Color c))
                return c;
            // Try space-delimited RGB (0-1 range)
            string[] parts = value.Split(new char[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 3)
            {
                if (float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out float r) &&
                    float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float g) &&
                    float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out float b))
                {
                    return new Color(r, g, b);
                }
            }
            return null;
        }

        private object SampleCurrentValue(PropertyType propType)
        {
            switch (propType)
            {
                case PropertyType.Position:
                    // Sample in A-Frame coordinate space for interpolation consistency
                    var arenaPos = ArenaUnity.ToArenaPosition(transform.localPosition);
                    return (Vector3?)new Vector3(arenaPos.X, arenaPos.Y, arenaPos.Z);
                case PropertyType.PositionAxis:
                    var aPos = ArenaUnity.ToArenaPosition(transform.localPosition);
                    Vector3 pv = new Vector3(aPos.X, aPos.Y, aPos.Z);
                    return pv[axisIndex];
                case PropertyType.Rotation:
                    // Sample in A-Frame euler space so from/to values are consistent
                    return (Vector3?)ArenaUnity.ToArenaRotationEuler(transform.localRotation);
                case PropertyType.RotationAxis:
                    return ArenaUnity.ToArenaRotationEuler(transform.localRotation)[axisIndex];
                case PropertyType.Scale:
                    return (Vector3?)transform.localScale;
                case PropertyType.ScaleAxis:
                    return transform.localScale[axisIndex];
                case PropertyType.Color:
                    var renderer = GetComponent<Renderer>();
                    if (renderer != null && renderer.sharedMaterial != null)
                    {
                        string colorProp = ArenaUnity.ColorPropertyName;
                        if (renderer.sharedMaterial.HasProperty(colorProp))
                            return (Color?)renderer.sharedMaterial.GetColor(colorProp);
                    }
                    return (Color?)Color.white;
                case PropertyType.Opacity:
                    var rend = GetComponent<Renderer>();
                    if (rend != null && rend.sharedMaterial != null)
                    {
                        string colorProp = ArenaUnity.ColorPropertyName;
                        if (rend.sharedMaterial.HasProperty(colorProp))
                            return rend.sharedMaterial.GetColor(colorProp).a;
                    }
                    return 1f;
                default:
                    return null;
            }
        }

        // ==========================================
        // Value Application
        // ==========================================

        private void ApplyValue(object value, PropertyType propType)
        {

            switch (propType)
            {
                case PropertyType.Position:
                    if (value is Vector3 pos)
                    {
                        // Convert from A-Frame coordinate space to Unity
                        transform.localPosition = ArenaUnity.ToUnityPosition(pos);
                    }
                    break;
                case PropertyType.PositionAxis:
                    if (value is float pAxis)
                    {
                        // Sample current A-Frame position, update one axis, convert back
                        var curAPos = ArenaUnity.ToArenaPosition(transform.localPosition);
                        Vector3 pVec = new Vector3(curAPos.X, curAPos.Y, curAPos.Z);
                        pVec[axisIndex] = pAxis;
                        transform.localPosition = ArenaUnity.ToUnityPosition(pVec);
                    }
                    break;
                case PropertyType.Rotation:
                    if (value is Vector3 rot)
                    {
                        // A-Frame animation sends euler degrees; convert to Unity rotation
                        transform.localRotation = ArenaUnity.ToUnityRotationEuler(
                            new ArenaRotationJson { X = rot.x, Y = rot.y, Z = rot.z });
                    }
                    break;
                case PropertyType.RotationAxis:
                    if (value is float rAxis)
                    {
                        // Get current rotation in A-Frame euler space, update one axis, convert back
                        Vector3 arenaEuler = ArenaUnity.ToArenaRotationEuler(transform.localRotation);
                        arenaEuler[axisIndex] = rAxis;
                        transform.localRotation = ArenaUnity.ToUnityRotationEuler(
                            new ArenaRotationJson { X = arenaEuler.x, Y = arenaEuler.y, Z = arenaEuler.z });
                    }
                    break;
                case PropertyType.Scale:
                    if (value is Vector3 scl)
                        transform.localScale = scl;
                    break;
                case PropertyType.ScaleAxis:
                    if (value is float sAxis)
                    {
                        Vector3 scale = transform.localScale;
                        scale[axisIndex] = sAxis;
                        transform.localScale = scale;
                    }
                    break;
                case PropertyType.Color:
                    if (value is Color col)
                    {
                        var renderer = GetComponent<Renderer>();
                        if (renderer != null)
                        {
                            // Use material instance to avoid polluting shared material
                            string colorProp = ArenaUnity.ColorPropertyName;
                            if (renderer.material.HasProperty(colorProp))
                                renderer.material.SetColor(colorProp, col);
                        }
                    }
                    break;
                case PropertyType.Opacity:
                    if (value is float opacity)
                    {
                        var renderer = GetComponent<Renderer>();
                        if (renderer != null)
                        {
                            string colorProp = ArenaUnity.ColorPropertyName;
                            if (renderer.material.HasProperty(colorProp))
                            {
                                Color c = renderer.material.GetColor(colorProp);
                                c.a = opacity;
                                renderer.material.SetColor(colorProp, c);
                            }
                        }
                    }
                    break;
            }
        }

        // ==========================================
        // Interpolation
        // ==========================================

        private static object Interpolate(object from, object to, float t, PropertyType propType)
        {
            switch (propType)
            {
                case PropertyType.Position:
                case PropertyType.Scale:
                    if (from is Vector3 vFrom && to is Vector3 vTo)
                        return Vector3.LerpUnclamped(vFrom, vTo, t);
                    return from;
                case PropertyType.Rotation:
                    if (from is Vector3 rFrom && to is Vector3 rTo)
                    {
                        // Lerp euler angles — matches A-Frame behavior for animation
                        return new Vector3(
                            Mathf.LerpUnclamped(rFrom.x, rTo.x, t),
                            Mathf.LerpUnclamped(rFrom.y, rTo.y, t),
                            Mathf.LerpUnclamped(rFrom.z, rTo.z, t));
                    }
                    return from;
                case PropertyType.Color:
                    if (from is Color cFrom && to is Color cTo)
                        return Color.LerpUnclamped(cFrom, cTo, t);
                    return from;
                case PropertyType.PositionAxis:
                case PropertyType.RotationAxis:
                case PropertyType.ScaleAxis:
                case PropertyType.Opacity:
                case PropertyType.Float:
                    if (from is float fFrom && to is float fTo)
                        return Mathf.LerpUnclamped(fFrom, fTo, t);
                    return from;
                default:
                    return from;
            }
        }

        private static object RoundValue(object value, PropertyType propType)
        {
            switch (propType)
            {
                case PropertyType.Position:
                case PropertyType.Rotation:
                case PropertyType.Scale:
                    if (value is Vector3 v)
                        return new Vector3(Mathf.Round(v.x), Mathf.Round(v.y), Mathf.Round(v.z));
                    return value;
                case PropertyType.PositionAxis:
                case PropertyType.RotationAxis:
                case PropertyType.ScaleAxis:
                case PropertyType.Opacity:
                case PropertyType.Float:
                    if (value is float f)
                        return Mathf.Round(f);
                    return value;
                default:
                    return value;
            }
        }

        // ==========================================
        // Loop Parsing
        // ==========================================

        /// <summary>
        /// Parse A-Frame loop value: "true" = infinite (-1), "false"/"0" = no loop (0),
        /// number = repeat count.
        /// </summary>
        private static int ParseLoopCount(string loop)
        {
            if (string.IsNullOrEmpty(loop)) return 0;
            string l = loop.Trim().ToLower();
            if (l == "true") return -1; // infinite
            if (l == "false") return 0;
            if (int.TryParse(l, out int count))
                return count > 0 ? count : 0;
            return 0;
        }

        // ==========================================
        // Easing Functions
        // Matches A-Frame / easings.net exactly
        // ==========================================

        private static float ApplyEasing(float t, ArenaAnimationJson.EasingType easing, float elasticity)
        {
            // Normalize elasticity from A-Frame range (default 400) to a usable parameter
            float p = elasticity > 0 ? 1f / (elasticity / 400f) : 0.3f;

            switch (easing)
            {
                case ArenaAnimationJson.EasingType.Linear:
                    return t;

                // Quad
                case ArenaAnimationJson.EasingType.EaseInQuad:
                    return t * t;
                case ArenaAnimationJson.EasingType.EaseOutQuad:
                    return t * (2f - t);
                case ArenaAnimationJson.EasingType.EaseInOutQuad:
                    return t < 0.5f ? 2f * t * t : -1f + (4f - 2f * t) * t;

                // Cubic
                case ArenaAnimationJson.EasingType.EaseInCubic:
                    return t * t * t;
                case ArenaAnimationJson.EasingType.EaseOutCubic:
                    return (--t) * t * t + 1f;
                case ArenaAnimationJson.EasingType.EaseInOutCubic:
                    return t < 0.5f ? 4f * t * t * t : (t - 1f) * (2f * t - 2f) * (2f * t - 2f) + 1f;

                // Quart
                case ArenaAnimationJson.EasingType.EaseInQuart:
                    return t * t * t * t;
                case ArenaAnimationJson.EasingType.EaseOutQuart:
                    return 1f - (--t) * t * t * t;
                case ArenaAnimationJson.EasingType.EaseInOutQuart:
                    return t < 0.5f ? 8f * t * t * t * t : 1f - 8f * (--t) * t * t * t;

                // Quint
                case ArenaAnimationJson.EasingType.EaseInQuint:
                    return t * t * t * t * t;
                case ArenaAnimationJson.EasingType.EaseOutQuint:
                    return 1f + (--t) * t * t * t * t;
                case ArenaAnimationJson.EasingType.EaseInOutQuint:
                    return t < 0.5f ? 16f * t * t * t * t * t : 1f + 16f * (--t) * t * t * t * t;

                // Sine
                case ArenaAnimationJson.EasingType.EaseInSine:
                    return 1f - Mathf.Cos(t * Mathf.PI / 2f);
                case ArenaAnimationJson.EasingType.EaseOutSine:
                    return Mathf.Sin(t * Mathf.PI / 2f);
                case ArenaAnimationJson.EasingType.EaseInOutSine:
                    return 0.5f * (1f - Mathf.Cos(Mathf.PI * t));

                // Expo
                case ArenaAnimationJson.EasingType.EaseInExpo:
                    return t == 0f ? 0f : Mathf.Pow(2f, 10f * (t - 1f));
                case ArenaAnimationJson.EasingType.EaseOutExpo:
                    return t == 1f ? 1f : 1f - Mathf.Pow(2f, -10f * t);
                case ArenaAnimationJson.EasingType.EaseInOutExpo:
                    if (t == 0f) return 0f;
                    if (t == 1f) return 1f;
                    return t < 0.5f
                        ? 0.5f * Mathf.Pow(2f, 20f * t - 10f)
                        : 1f - 0.5f * Mathf.Pow(2f, -20f * t + 10f);

                // Circ
                case ArenaAnimationJson.EasingType.EaseInCirc:
                    return 1f - Mathf.Sqrt(1f - t * t);
                case ArenaAnimationJson.EasingType.EaseOutCirc:
                    return Mathf.Sqrt(1f - (--t) * t);
                case ArenaAnimationJson.EasingType.EaseInOutCirc:
                    return t < 0.5f
                        ? 0.5f * (1f - Mathf.Sqrt(1f - 4f * t * t))
                        : 0.5f * (Mathf.Sqrt(1f - (2f * t - 2f) * (2f * t - 2f)) + 1f);

                // Back
                case ArenaAnimationJson.EasingType.EaseInBack:
                    const float s1 = 1.70158f;
                    return t * t * ((s1 + 1f) * t - s1);
                case ArenaAnimationJson.EasingType.EaseOutBack:
                    const float s2 = 1.70158f;
                    t -= 1f;
                    return t * t * ((s2 + 1f) * t + s2) + 1f;
                case ArenaAnimationJson.EasingType.EaseInOutBack:
                    const float s3 = 1.70158f * 1.525f;
                    t *= 2f;
                    if (t < 1f) return 0.5f * (t * t * ((s3 + 1f) * t - s3));
                    t -= 2f;
                    return 0.5f * (t * t * ((s3 + 1f) * t + s3) + 2f);

                // Elastic
                case ArenaAnimationJson.EasingType.EaseInElastic:
                    if (t == 0f) return 0f;
                    if (t == 1f) return 1f;
                    return -Mathf.Pow(2f, 10f * (t - 1f)) * Mathf.Sin((t - 1f - p / 4f) * (2f * Mathf.PI) / p);
                case ArenaAnimationJson.EasingType.EaseOutElastic:
                    if (t == 0f) return 0f;
                    if (t == 1f) return 1f;
                    return Mathf.Pow(2f, -10f * t) * Mathf.Sin((t - p / 4f) * (2f * Mathf.PI) / p) + 1f;
                case ArenaAnimationJson.EasingType.EaseInOutElastic:
                    if (t == 0f) return 0f;
                    if (t == 1f) return 1f;
                    t *= 2f;
                    if (t < 1f)
                        return -0.5f * Mathf.Pow(2f, 10f * (t - 1f)) * Mathf.Sin((t - 1f - p / 4f) * (2f * Mathf.PI) / p);
                    return Mathf.Pow(2f, -10f * (t - 1f)) * Mathf.Sin((t - 1f - p / 4f) * (2f * Mathf.PI) / p) * 0.5f + 1f;

                default:
                    return t;
            }
        }
    }
}
