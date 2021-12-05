/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021, The CONIX Research Center. All rights reserved.
 */

using UnityEngine;

namespace ArenaUnity
{
    /// <summary>
    /// Static utility class for object translation.
    /// </summary>
    public static class ArenaUnity
    {
        // object type
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
        // position
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
        // rotation
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
        // scale
        public static dynamic ToArenaScale(Vector3 scale)
        {
            return new
            {
                x = scale.x,
                y = scale.y,
                z = scale.z
            };
        }
        public static Vector3 ToUnityScale(dynamic scale)
        {
            return new Vector3(
                (float)scale.x,
                (float)scale.y,
                (float)scale.z
            );
        }
        // size dimensions
        public static void ToArenaDimensions(GameObject gobj, ref dynamic data)
        {
            string collider = gobj.GetComponent<Collider>().GetType().ToString();
            switch (collider)
            {
                case "BoxCollider":
                    BoxCollider bc = gobj.GetComponent<BoxCollider>();
                    data.width = bc.size.x;
                    data.height = bc.size.y;
                    data.depth = bc.size.z;
                    break;
                case "SphereCollider":
                    SphereCollider sc = gobj.GetComponent<SphereCollider>();
                    data.radius = sc.radius;
                    break;
                case "CapsuleCollider":
                    CapsuleCollider cc = gobj.GetComponent<CapsuleCollider>();
                    data.height = cc.height;
                    data.radius = cc.radius;
                    break;
                default:
                    break;
            }
        }
        public static void ToUnityDimensions(dynamic data, ref GameObject gobj)
        {
            if (data.object_type != null)
            {
                // use arena defaults if missing for consistency
                switch ((string)data.object_type)
                {
                    case "box":
                    case "cube":
                        BoxCollider bc = gobj.GetComponent<BoxCollider>();
                        bc.size = new Vector3(
                            data.width != null ? (float)data.width : 1f,
                            data.height != null ? (float)data.height : 1f,
                            data.depth != null ? (float)data.depth : 1f
                        );
                        break;
                    case "cylinder":
                    case "capsule":
                        CapsuleCollider cc = gobj.GetComponent<CapsuleCollider>();
                        cc.height = data.height != null ? (float)data.height : 2f;
                        cc.radius = data.radius != null ? (float)data.radius : 1f;
                        break;
                    case "sphere":
                        SphereCollider sc = gobj.GetComponent<SphereCollider>();
                        sc.radius = data.radius != null ? (float)data.radius : 1f;
                        break;
                }
            }
        }
        // color
        public static string ToArenaColor(Color color)
        {
            return $"#{ColorUtility.ToHtmlStringRGB(color)}";
        }
        public static Color ToUnityColor(string color)
        {
            ColorUtility.TryParseHtmlString(color, out Color colorObj);
            return colorObj;
        }

    }
}
