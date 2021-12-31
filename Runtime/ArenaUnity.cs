/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021, The CONIX Research Center. All rights reserved.
 */

using System;
using System.Dynamic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace ArenaUnity
{
    /// <summary>
    /// Static utility class for object translation.
    /// </summary>
    public static class ArenaUnity
    {
        public static int mainDisplay = 0;
        public static int secondDisplay = 1;

        private static float ArenaFloat(float n) { return (float)Math.Round(n, 3); }

        // object type
        public static string ToArenaObjectType(GameObject gobj)
        {
            string objectType = "entity";
            MeshFilter meshFilter = gobj.GetComponent<MeshFilter>();
            Light light = gobj.GetComponent<Light>();
            SpriteRenderer spriteRenderer = gobj.GetComponent<SpriteRenderer>();
            if (meshFilter && meshFilter.sharedMesh)
            {
                if (meshFilter.sharedMesh.name == "Cube")
                    objectType = "box";
                else
                    objectType = meshFilter.sharedMesh.name.ToLower();
            }
            else if (spriteRenderer && spriteRenderer.sprite && spriteRenderer.sprite.pixelsPerUnit != 0f)
                objectType = "image";
            else if (light)
                objectType = "light";
            return objectType;
        }
        public static GameObject ToUnityObjectType(dynamic data)
        {
            switch ((string)data.object_type)
            {
                case "cube": // support legacy arena 'cube' == 'box'
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
                case "light":
                    if (data.type == null || (string)data.type == "ambient")
                        return new GameObject();
                    else
                    {
                        GameObject lgobj = new GameObject();
                        lgobj.transform.gameObject.AddComponent<Light>();
                        return lgobj;
                    }
                case "camera":
                    GameObject cgobj = new GameObject();
                    Camera camera = cgobj.transform.gameObject.AddComponent<Camera>();
                    camera.nearClipPlane = 0.1f; // match arena
                    camera.farClipPlane = 10000f; // match arena
                    camera.fieldOfView = 80f; // match arena
                    return cgobj;
                default:
                    return new GameObject();
            };
        }
        // position
        public static dynamic ToArenaPosition(Vector3 position)
        {
            return new
            {
                x = ArenaFloat(position.x),
                y = ArenaFloat(position.y),
                z = ArenaFloat(-position.z)
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
        public static dynamic ToArenaRotationQuat(Quaternion rotationQuat, bool invertY = true)
        {
            return new
            {
                x = ArenaFloat(-rotationQuat.x),
                y = ArenaFloat(rotationQuat.y * (invertY ? -1 : 1)),
                z = ArenaFloat(rotationQuat.z),
                w = ArenaFloat(rotationQuat.w)
            };
        }
        public static Quaternion ToUnityRotationQuat(dynamic rotationQuat, bool invertY = true)
        {
            return new Quaternion(
                -(float)rotationQuat.x,
                (float)rotationQuat.y * (invertY ? -1 : 1),
                (float)rotationQuat.z,
                (float)rotationQuat.w
            );
        }
        public static dynamic ToArenaRotationEuler(Vector3 rotationEuler, bool invertY = true)
        {
            return new
            {
                x = ArenaFloat(-rotationEuler.x),
                y = ArenaFloat(rotationEuler.y * (invertY ? -1 : 1)),
                z = ArenaFloat(rotationEuler.z)
            };
        }
        public static Quaternion ToUnityRotationEuler(dynamic rotationEuler, bool invertY = true)
        {
            return Quaternion.Euler(
                -(float)rotationEuler.x,
                (float)rotationEuler.y * (invertY ? -1 : 1),
                (float)rotationEuler.z
            );
        }
        public static Quaternion GltfToUnityRotationQuat(Quaternion rotationQuat)
        {
            rotationQuat *= Quaternion.Euler(0, 180f, 0);
            return rotationQuat;
        }
        public static Quaternion UnityToGltfRotationQuat(Quaternion rotationQuat)
        {
            // TODO: implement
            rotationQuat *= Quaternion.Euler(0, -180f, 0);
            return rotationQuat;
        }
        // scale
        public static dynamic ToArenaScale(Vector3 scale)
        {
            return new
            {
                x = ArenaFloat(scale.x),
                y = ArenaFloat(scale.y),
                z = ArenaFloat(scale.z)
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
                    data.width = ArenaFloat(bc.size.x);
                    data.height = ArenaFloat(bc.size.y);
                    data.depth = ArenaFloat(bc.size.z);
                    break;
                case "SphereCollider":
                    SphereCollider sc = gobj.GetComponent<SphereCollider>();
                    data.radius = ArenaFloat(sc.radius);
                    break;
                case "CapsuleCollider":
                    CapsuleCollider cc = gobj.GetComponent<CapsuleCollider>();
                    data.height = ArenaFloat(cc.height);
                    data.radius = ArenaFloat(cc.radius);
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
        // light
        public static void ToArenaLight(GameObject gobj, ref dynamic data)
        {
            // TODO: translate from RenderSettings.ambientMode, made need centralized one-time publish

            Light light = gobj.GetComponent<Light>();
            switch (light.type)
            {
                case LightType.Directional:
                    data.type = "directional";
                    break;
                case LightType.Point:
                    data.type = "point";
                    data.distance = ArenaFloat(light.range);
                    break;
                case LightType.Spot:
                    data.type = "spot";
                    data.distance = ArenaFloat(light.range);
                    data.angle = ArenaFloat(light.spotAngle);
                    break;
            }
            data.intensity = ArenaFloat(light.intensity);
            data.color = ToArenaColor(light.color);
        }
        public static void ToUnityLight(dynamic data, ref GameObject gobj)
        {
            if (data.type != null)
            {
                if ((string)data.type == "ambient")
                {
                    RenderSettings.ambientLight = ToUnityColor((string)data.color);
                    RenderSettings.ambientMode = AmbientMode.Flat;
                }
                else
                {
                    Light light = gobj.GetComponent<Light>();
                    switch ((string)data.type)
                    {
                        case "directional":
                            light.type = LightType.Directional;
                            break;
                        case "point":
                            light.type = LightType.Point;
                            if (data.distance != null)
                                light.range = (float)data.distance;
                            break;
                        case "spot":
                            light.type = LightType.Spot;
                            if (data.distance != null)
                                light.range = (float)data.distance;
                            if (data.angle != null)
                                light.spotAngle = (float)data.angle;
                            break;
                    }
                    if (data.intensity != null)
                        light.intensity = (float)data.intensity;
                    if (data.color != null)
                        light.color = ToUnityColor((string)data.color);
                }
            }
        }
        // material
        public static void ToArenaMaterial(GameObject obj, ref dynamic data)
        {
            Material mat = obj.GetComponent<Renderer>().material;
            if (!mat)
                return;
            dynamic material = new ExpandoObject();
            data.material = material;
            // shaders only
            if (mat.shader.name == "Standard")
            {
                data.material.shader = "standard";
                //data.url = ToArenaTexture(mat);
                //data.material.repeat = ArenaFloat(mat.mainTextureScale.x);
                data.material.color = ToArenaColor(mat.color);
                //data.material.metalness = ArenaFloat(mat.GetFloat("_Metallic"));
                //data.material.roughness = ArenaFloat(1f - mat.GetFloat("_Glossiness"));
                data.material.transparent = mat.GetFloat("_Mode") == 3f ? true : false;
                data.material.opacity = ArenaFloat(mat.color.a);
                //if (mat.color.a == 1f)
                //    data.material.side = "double";
            }
            else if (mat.shader.name == "Unlit/Color")
            {
                data.material.shader = "flat";
                //data.material.side = "double";
            }
            else if (mat.shader.name == "Unlit/Texture")
            {
                data.material.shader = "flat";
                //data.url = ToArenaTexture(mat);
                //data.material.repeat = ArenaFloat(mat.mainTextureScale.x);
                //data.material.side = "double";
            }
            else if (mat.shader.name == "Unlit/Texture Colored")
            {
                data.material.shader = "flat";
                //data.url = ToArenaTexture(mat);
                //data.material.repeat = ArenaFloat(mat.mainTextureScale.x);
                data.material.color = ToArenaColor(mat.color);
                //data.material.side = "double";
            }
            else if (mat.shader.name == "Legacy Shaders/Transparent/Diffuse")
            {
                data.material.shader = "flat";
                //data.url = ToArenaTexture(mat);
                //data.material.repeat = ArenaFloat(mat.mainTextureScale.x);
                data.material.color = ToArenaColor(mat.color);
                data.material.transparent = true;
                data.material.opacity = ArenaFloat(mat.color.a);
                //if (mat.color.a == 1f)
                //    data.material.side = "double";
            }
            else
            {
                // other shaders
                data.material.shader = "standard";
                //data.url = ToArenaTexture(mat);
                //data.material.repeat = ArenaFloat(mat.mainTextureScale.x);
                if (mat.HasProperty("_Color"))
                    data.material.color = ToArenaColor(mat.color);
                //data.material.side = "double";
            }
        }
        public static void ToUnityMaterial(dynamic data, ref GameObject gobj)
        {
            var renderer = gobj.GetComponent<Renderer>();
            if (renderer != null)
            {
                var material = renderer.material;
                material.shader.name = "Standard";
                if (data.material != null)
                {
                    if (data.material.color != null)
                        material.SetColor("_Color", ToUnityColor((string)data.material.color));
                }
                if (data.color != null) // support legacy arena color
                {   // legacy color overrides material color in the arena
                    material.SetColor("_Color", ToUnityColor((string)data.color));
                }
                if (data.material != null)
                {
                    if (data.material.opacity != null)
                    {
                        Color c = material.GetColor("_Color");
                        material.SetColor("_Color", new Color(c.r, c.g, c.b, (float)data.material.opacity));
                    }
                    if (data.material.transparent != null)
                    {
                        // For runtime set/change transparency mode, follow GUI params
                        // https://github.com/Unity-Technologies/UnityCsReference/blob/master/Editor/Mono/Inspector/StandardShaderGUI.cs#L344
                        if (Convert.ToBoolean(data.material.transparent))
                        {
                            material.SetFloat("_Mode", 3f); // StandardShaderGUI.BlendMode.Transparent
                            material.SetInt("_SrcBlend", (int)BlendMode.One);
                            material.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
                            material.SetInt("_ZWrite", 0);
                            material.DisableKeyword("_ALPHATEST_ON");
                            material.DisableKeyword("_ALPHABLEND_ON");
                            material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                            material.renderQueue = 3000;
                        }
                        else
                        {
                            material.SetFloat("_Mode", 0f); // StandardShaderGUI.BlendMode.Opaque
                            material.SetInt("_SrcBlend", (int)BlendMode.One);
                            material.SetInt("_DstBlend", (int)BlendMode.Zero);
                            material.SetInt("_ZWrite", 1);
                            material.DisableKeyword("_ALPHATEST_ON");
                            material.DisableKeyword("_ALPHABLEND_ON");
                            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                            material.renderQueue = -1;
                        }
                    }
                }
            }
        }
        // texture
        public static string ToArenaTexture(Material mat)
        {
            Texture tex = mat.GetTexture("_MainTex");
            if (tex)
            {
                string texture_path = AssetDatabase.GetAssetPath(tex);
                string new_path = $"{ArenaClient.importPath}/images/{Path.GetFileName(texture_path)}";
                // copy if there is no texture
                if (AssetDatabase.AssetPathToGUID(new_path) == "")
                {
                    AssetDatabase.CopyAsset(texture_path, new_path);
                }
                return $"images/{Path.GetFileName(texture_path)}";
            }
            return "";
        }

    }
}
