/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021, The CONIX Research Center. All rights reserved.
 */

using UnityEngine;

namespace ArenaUnity
{
    public static class ArenaUnity
    {
        public static string ToArenaObjectType(GameObject gobj)
        {
            string objectType = "entity";
            if (gobj.GetComponent<MeshFilter>())
            {
                objectType = gobj.GetComponent<MeshFilter>().sharedMesh.name.ToLower();
            }
            return objectType.ToLower();
        }
        public static GameObject ToUnityObjectType(string obj_type)
        {
            switch (obj_type)
            {
                case "cube":
                case "box":
                    return GameObject.CreatePrimitive(PrimitiveType.Cube);
                case "cylinder":
                    return GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                case "sphere":
                    return GameObject.CreatePrimitive(PrimitiveType.Sphere);
                case "plane":
                    return GameObject.CreatePrimitive(PrimitiveType.Plane);
                case "quad":
                    return GameObject.CreatePrimitive(PrimitiveType.Quad);
                case "capsule":
                    return GameObject.CreatePrimitive(PrimitiveType.Capsule);
                case "camera":
                    GameObject gobj = new GameObject();
                    Camera camera = gobj.transform.gameObject.AddComponent<Camera>();
                    camera.nearClipPlane = 0.1f; // match arena
                    camera.farClipPlane = 10000f; // match arena
                    camera.fieldOfView = 80f; // match arena
                    return gobj;
                default:
                    return new GameObject();
            };
        }

        // Position Conversions:
        // all: z is inverted between a-frame/unity
        public static dynamic ToArenaPosition(Vector3 position)
        {
            return new
            {
                x = position.x,
                y = position.y,
                z = -position.z
            };
        }
        public static Vector3 ToUnityPosition(dynamic position)
        {
            return new Vector3(
                (float)position.x,
                (float)position.y,
                -(float)position.z
            );
        }

        public static dynamic ToArenaRotationQuat(Quaternion rotationQuat)
        {
            return new
            {
                x = -rotationQuat.x,
                y = -rotationQuat.y,
                z = rotationQuat.z,
                w = rotationQuat.w
            };
        }
        public static Quaternion ToUnityRotationQuat(dynamic rotationQuat)
        {
            return new Quaternion(
                -(float)rotationQuat.x,
                -(float)rotationQuat.y,
                (float)rotationQuat.z,
                (float)rotationQuat.w
            );
        }
        public static dynamic ToArenaRotationEuler(Vector3 rotationEuler)
        {
            return new
            {
                x = -rotationEuler.x,
                y = -rotationEuler.y,
                z = rotationEuler.z
            };
        }
        public static Quaternion ToUnityRotationEuler(dynamic rotationEuler)
        {
            return Quaternion.Euler(
                -(float)rotationEuler.x,
                -(float)rotationEuler.y,
                (float)rotationEuler.z
            );
        }

        public static dynamic ToArenaScale(string object_type, Vector3 scale)
        {
            float[] f = GetScaleFactor(object_type);
            return new
            {
                x = scale.x * f[0],
                y = scale.y * f[1],
                z = scale.z * f[2]
            };
        }
        public static Vector3 ToUnityScale(string object_type, dynamic scale)
        {
            float[] f = GetScaleFactor(object_type);
            return new Vector3(
                (float)scale.x / f[0],
                (float)scale.y / f[1],
                (float)scale.z / f[2]
            );
        }

        // Scale Conversions
        // cube: unity (side) 1, a-frame (side)  1
        // sphere: unity (diameter) 1, a-frame (radius)  0.5
        // cylinder: unity (y height) 1, a-frame (y height) 2
        // cylinder: unity (x,z diameter) 1, a-frame (x,z radius) 0.5
        private static float[] GetScaleFactor(string object_type)
        {
            switch (object_type)
            {
                case "sphere":
                    return new float[3] { 0.5f, 0.5f, 0.5f };
                case "cylinder":
                    return new float[3] { 0.5f, 2f, 0.5f };
                default:
                    return new float[3] { 1f, 1f, 1f };
            }
        }

        public static string ToArenaColor(Color color)
        {
            return $"#{ColorUtility.ToHtmlStringRGB(color)}";
        }
        public static Color ToUnityColor(string color)
        {
            Color colorObj;
            ColorUtility.TryParseHtmlString(color, out colorObj);
            return colorObj;
        }

    }
}
