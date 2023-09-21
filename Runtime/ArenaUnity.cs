/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2023, Carnegie Mellon University. All rights reserved.
 */

using System;
using ArenaUnity.Schemas;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

namespace ArenaUnity
{
    /// <summary>
    /// Static utility class for object translation.
    /// </summary>
    public static class ArenaUnity
    {
        public static string ColorPropertyName = (!GraphicsSettings.renderPipelineAsset ? "_Color" : "_BaseColor");
        public const float LineSinglePixelInMeters = 0.005f;
        public static float ArenaFloat(float n) { return (float)Math.Round(n, 3); }

        // time
        public static string TimeSpanToString(TimeSpan span)
        {
            string spanString = string.Format("{0}{1}{2}{3}",
                span.Duration().Days > 0 ? string.Format("{0:0} day{1}, ", span.Days, span.Days == 1 ? string.Empty : "s") : string.Empty,
                span.Duration().Hours > 0 ? string.Format("{0:0} hour{1}, ", span.Hours, span.Hours == 1 ? string.Empty : "s") : string.Empty,
                span.Duration().Minutes > 0 ? string.Format("{0:0} minute{1}, ", span.Minutes, span.Minutes == 1 ? string.Empty : "s") : string.Empty,
                span.Duration().Seconds > 0 ? string.Format("{0:0} second{1}", span.Seconds, span.Seconds == 1 ? string.Empty : "s") : string.Empty);
            if (spanString.EndsWith(", ")) spanString = spanString.Substring(0, spanString.Length - 2);
            if (string.IsNullOrEmpty(spanString)) spanString = "0 seconds";
            return spanString;
        }

        // object type
        public static string ToArenaObjectType(GameObject gobj)
        {
            string objectType = "entity";
            MeshFilter meshFilter = gobj.GetComponent<MeshFilter>();
            TextMeshPro tm = gobj.GetComponent<TextMeshPro>();
            Light light = gobj.GetComponent<Light>();
            SpriteRenderer spriteRenderer = gobj.GetComponent<SpriteRenderer>();
            LineRenderer lr = gobj.GetComponent<LineRenderer>();
            // initial priority is primitive
            if (spriteRenderer && spriteRenderer.sprite && spriteRenderer.sprite.pixelsPerUnit != 0f)
                objectType = "image";
            else if (lr)
                objectType = "thickline";
            else if (tm)
                objectType = "text";
            else if (light)
                objectType = "light";
            else if (meshFilter && meshFilter.sharedMesh)
                objectType = meshFilter.sharedMesh.name.ToLower();
            return objectType;
        }

        // position
        public static string ToArenaPositionString(Vector3 position)
        {
            return $"{ArenaFloat(position.x)} {ArenaFloat(position.y)} {ArenaFloat(-position.z)}";
        }
        public static Vector3 ToUnityPositionString(string strPos)
        {
            string[] axis = strPos.Split(new char[] { ' ' }, 3, StringSplitOptions.RemoveEmptyEntries);
            return new Vector3(
                float.Parse(axis[0]),
                float.Parse(axis[1]),
                -float.Parse(axis[2])
            );
        }
        public static ArenaPositionJson ToArenaPosition(Vector3 position)
        {
            return new ArenaPositionJson
            {
                X = ArenaFloat(position.x),
                Y = ArenaFloat(position.y),
                Z = ArenaFloat(-position.z)
            };
        }
        public static Vector3 ToUnityPosition(ArenaPositionJson position)
        {
            return new Vector3(
                (float)position.X,
                (float)position.Y,
                -(float)position.Z
            );
        }

        // rotation
        public static ArenaRotationJson ToArenaRotationQuat(Quaternion rotationQuat, bool invertY = true)
        {
            return new ArenaRotationJson
            {
                X = ArenaFloat(-rotationQuat.x),
                Y = ArenaFloat(rotationQuat.y * (invertY ? -1 : 1)),
                Z = ArenaFloat(rotationQuat.z),
                W = ArenaFloat(rotationQuat.w)
            };
        }
        public static Quaternion ToUnityRotationQuat(ArenaRotationJson rotationQuat, bool invertY = true)
        {
            return new Quaternion(
                -(float)rotationQuat.X,
                (float)rotationQuat.Y * (invertY ? -1 : 1),
                (float)rotationQuat.Z,
                (float)rotationQuat.W
            );
        }
        /// <summary>
        /// Converts Vector3 rotationEuler to dynamic rotationEuler. CAUTION: Do not use for ARENA!
        /// A merge with quaternion type will leave a mix of xyz euler and w quaternion = badness.
        /// </summary>
        public static ArenaVector3Json ToArenaRotationEuler(Vector3 rotationEuler, bool invertY = true)
        {
            return new ArenaVector3Json
            {
                X = ArenaFloat(-rotationEuler.x),
                Y = ArenaFloat(rotationEuler.y * (invertY ? -1 : 1)),
                Z = ArenaFloat(rotationEuler.z)
            };
        }
        public static Quaternion ToUnityRotationEuler(ArenaRotationJson rotationEuler, bool invertY = true)
        {
            return Quaternion.Euler(
                -(float)rotationEuler.X,
                (float)rotationEuler.Y * (invertY ? -1 : 1),
                (float)rotationEuler.Z
            );
        }
        public static Quaternion GltfToUnityRotationQuat(Quaternion rotationQuat)
        {
            rotationQuat *= Quaternion.Euler(0, 180f, 0);
            return rotationQuat;
        }
        public static Quaternion UnityToGltfRotationQuat(Quaternion rotationQuat)
        {
            rotationQuat *= Quaternion.Euler(0, -180f, 0);
            return rotationQuat;
        }
        // scale
        public static ArenaScaleJson ToArenaScale(Vector3 scale)
        {
            return new ArenaScaleJson
            {
                X = ArenaFloat(scale.x),
                Y = ArenaFloat(scale.y),
                Z = ArenaFloat(scale.z)
            };
        }
        public static Vector3 ToUnityScale(ArenaScaleJson scale)
        {
            return new Vector3(
                (float)scale.X,
                (float)scale.Y,
                (float)scale.Z
            );
        }

        // color
        public static string ToArenaColor(Color color)
        {
            return $"#{ColorUtility.ToHtmlStringRGB(color).ToLower()}";
        }
        public static Color ToUnityColor(string color)
        {
            ColorUtility.TryParseHtmlString(color, out Color colorObj);
            return colorObj;
        }
        public static Color ToUnityColor(string color, float opacity)
        {
            Color c = ToUnityColor(color);
            return new Color(c.r, c.g, c.b, opacity);
        }
        public static Color ColorRandom()
        {
            return UnityEngine.Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
        }

    }
}
