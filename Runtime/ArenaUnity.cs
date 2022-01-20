/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021, The CONIX Research Center. All rights reserved.
 */

using System;
using System.Dynamic;
using MeshBuilder;
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
        public const string regexArenaObjectId = @"[^\w\-.:]";

        private static float ArenaFloat(float n) { return (float)Math.Round(n, 3); }

        // object type
        public static string ToArenaObjectType(GameObject gobj)
        {
            string objectType = "entity";
            MeshFilter meshFilter = gobj.GetComponent<MeshFilter>();
            Light light = gobj.GetComponent<Light>();
            SpriteRenderer spriteRenderer = gobj.GetComponent<SpriteRenderer>();
            if (meshFilter && meshFilter.sharedMesh)
                objectType = meshFilter.sharedMesh.name.ToLower();
            else if (spriteRenderer && spriteRenderer.sprite && spriteRenderer.sprite.pixelsPerUnit != 0f)
                objectType = "image";
            else if (light)
                objectType = "light";
            return objectType;
        }
        public static void ToUnityObjectType(dynamic indata, ref GameObject gobj)
        {
            dynamic data;
            string type;
            if ((string)indata.object_type == "entity" && indata.geometry != null && indata.geometry.primitive != null)
            {
                // handle raw geometry
                data = indata.geometry;
                type = (string)indata.geometry.primitive;
            }
            else
            {
                data = indata;
                type = (string)indata.object_type;
            }
            switch (type)
            {
                // build your own meshes
                case "box":
                case "cube": // support legacy arena 'cube' == 'box'
                    ArenaMeshCube cube = gobj.AddComponent<ArenaMeshCube>();
                    cube.width = data.width != null ? (float)data.width : 1f;
                    cube.height = data.height != null ? (float)data.height : 1f;
                    cube.depth = data.depth != null ? (float)data.depth : 1f;
                    cube.widthSegments = data.segmentsWidth != null ? (int)data.segmentsWidth : 2;
                    cube.heightSegments = data.segmentsHeight != null ? (int)data.segmentsHeight : 2;
                    cube.depthSegments = data.segmentsDepth != null ? (int)data.segmentsDepth : 2;
                    break;
                case "cone":
                    ArenaMeshCone cone = gobj.AddComponent<ArenaMeshCone>();
                    cone.radius = data.radiusBottom != null ? (float)data.radiusBottom : 1f;
                    cone.height = data.height != null ? (float)data.height : 2f;
                    cone.subdivision = data.segmentsRadial != null ? (int)data.segmentsRadial : 36;
                    break;
                case "cylinder":
                    ArenaMeshCylinder cylinder = gobj.AddComponent<ArenaMeshCylinder>();
                    cylinder.radius = data.radius != null ? (float)data.radius : 1f;
                    cylinder.height = data.height != null ? (float)data.height : 2f;
                    cylinder.radialSegments = data.segmentsRadial != null ? (int)data.segmentsRadial : 36;
                    cylinder.heightSegments = data.segmentsHeight != null ? (int)data.segmentsHeight : 18;
                    cylinder.openEnded = data.openEnded != null ? !Convert.ToBoolean(data.openEnded) : true;
                    break;
                case "icosahedron":
                    ArenaMeshIcosahedron icosahedron = gobj.AddComponent<ArenaMeshIcosahedron>();
                    icosahedron.radius = data.radius != null ? (float)data.radius : 1f;
                    icosahedron.details = 0;
                    break;
                case "octahedron":
                    ArenaMeshOctahedron octahedron = gobj.AddComponent<ArenaMeshOctahedron>();
                    octahedron.radius = data.radius != null ? (float)data.radius : 1f;
                    octahedron.details = 0;
                    break;
                case "plane":
                    ArenaMeshPlane plane = gobj.AddComponent<ArenaMeshPlane>();
                    plane.width = data.width != null ? (float)data.width : 1f;
                    plane.height = data.height != null ? (float)data.height : 1f;
                    plane.wSegments = data.segmentsWidth != null ? (int)data.segmentsWidth : 2;
                    plane.hSegments = data.segmentsHeight != null ? (int)data.segmentsHeight : 2;
                    break;
                case "ring":
                    ArenaMeshRing ring = gobj.AddComponent<ArenaMeshRing>();
                    ring.outerRadius = data.radiusOuter != null ? (float)data.radiusOuter : 1f;
                    ring.innerRadius = data.radiusInner != null ? (float)data.radiusInner : 1f;
                    ring.phiSegments = data.segmentsPhi != null ? (int)data.segmentsPhi : 8;
                    ring.thetaSegments = data.segmentsTheta != null ? (int)data.segmentsTheta : 32;
                    ring.thetaStart = data.thetaStart != null ? (float)data.thetaStart : 0f;
                    ring.thetaLength = (float)(data.thetaLength != null ? Math.PI / 180 * (float)data.thetaLength : Mathf.PI * 2f);
                    break;
                case "sphere":
                    ArenaMeshSphere sphere = gobj.AddComponent<ArenaMeshSphere>();
                    sphere.radius = data.radius != null ? (float)data.radius : 1f;
                    sphere.lonSegments = data.segmentsWidth != null ? (int)data.segmentsWidth : 18;
                    sphere.latSegments = data.segmentsHeight != null ? (int)data.segmentsHeight : 36;
                    break;
                case "torus":
                    ArenaMeshTorus torus = gobj.AddComponent<ArenaMeshTorus>();
                    torus.radius = data.radius != null ? (float)data.radius : 1f;
                    torus.thickness = data.radiusTubular != null ? (float)data.radiusTubular : 0.2f;
                    torus.radialSegments = data.segmentsRadial != null ? (int)data.segmentsRadial : 36;
                    torus.thetaSegments = data.segmentsTubular != null ? (int)data.segmentsTubular : 32;
                    torus.thetaStart = 0f;
                    torus.thetaEnd = (float)(data.arc != null ? Math.PI / 180 * (float)data.arc : Mathf.PI * 2f);
                    break;
                default:
                    break;
            };
        }

        private static void GenerateMeshObject(ref GameObject gobj, Mesh mesh)
        {
            gobj.transform.GetComponent<MeshFilter>();
            if (!gobj.transform.GetComponent<MeshFilter>() || !gobj.transform.GetComponent<MeshRenderer>())
            {
                gobj.transform.gameObject.AddComponent<MeshFilter>();
                gobj.transform.gameObject.AddComponent<MeshRenderer>();
            }
            gobj.transform.GetComponent<MeshFilter>().mesh = mesh;
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
            // used to collect unity-default render sizes
            string collider = gobj.GetComponent<Collider>().GetType().ToString();
            switch (collider)
            {
                case "UnityEngine.BoxCollider":
                    BoxCollider bc = gobj.GetComponent<BoxCollider>();
                    data.width = ArenaFloat(bc.size.x);
                    data.height = ArenaFloat(bc.size.y);
                    data.depth = ArenaFloat(bc.size.z);
                    break;
                case "UnityEngine.SphereCollider":
                    SphereCollider sc = gobj.GetComponent<SphereCollider>();
                    data.radius = ArenaFloat(sc.radius);
                    break;
                case "UnityEngine.CapsuleCollider":
                    CapsuleCollider cc = gobj.GetComponent<CapsuleCollider>();
                    data.height = ArenaFloat(cc.height);
                    data.radius = ArenaFloat(cc.radius);
                    break;
                default:
                    break;
            }
            MeshFilter meshFilter = gobj.GetComponent<MeshFilter>();
            if (meshFilter && meshFilter.sharedMesh)
            {
                switch (meshFilter.sharedMesh.name)
                {
                    case "Cube":
                        data.object_type = "box";
                        break;
                    case "Capsule": // TODO: determine if a-frame has an easy capsule mod
                        data.object_type = "cylinder";
                        break;
                    case "Quad":
                        data.object_type = "plane";
                        data.width = 1f;
                        data.height = 1f;
                        break;
                    case "Plane":
                        Quaternion rotOut = gobj.transform.localRotation;
                        rotOut *= Quaternion.Euler(90, 0, 0);
                        data.rotation = ArenaUnity.ToArenaRotationQuat(rotOut);
                        data.width = 10f;
                        data.height = 10f;
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
            // TODO: translate from RenderSettings.ambientMode, may need centralized one-time publish

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
            data.castShadow = light.shadows != LightShadows.None;
        }
        public static void ToUnityLight(dynamic data, ref GameObject gobj)
        {
            // support legacy lights
            dynamic ldata = data.light ?? data;
            if (ldata.type != null)
            {
                if ((string)ldata.type == "ambient")
                {
                    RenderSettings.ambientMode = AmbientMode.Flat;
                    if (ldata.intensity != null)
                        RenderSettings.ambientIntensity = (float)ldata.intensity;
                    if (ldata.color != null)
                        RenderSettings.ambientLight = ToUnityColor((string)ldata.color);
                }
                else
                {
                    Light light = gobj.AddComponent<Light>();
                    switch ((string)ldata.type)
                    {
                        case "directional":
                            light.type = LightType.Directional;
                            break;
                        case "point":
                            light.type = LightType.Point;
                            if (ldata.distance != null)
                                light.range = (float)ldata.distance;
                            break;
                        case "spot":
                            light.type = LightType.Spot;
                            if (ldata.distance != null)
                                light.range = (float)ldata.distance;
                            if (ldata.angle != null)
                                light.spotAngle = (float)ldata.angle;
                            break;
                    }
                    if (ldata.intensity != null)
                        light.intensity = (float)ldata.intensity;
                    if (ldata.color != null)
                        light.color = ToUnityColor((string)ldata.color);
                    light.shadows = ldata.castShadow == null ? LightShadows.None : LightShadows.Hard;
                }
            }
        }
        // material
        public static void ToArenaMaterial(GameObject obj, ref dynamic data)
        {
            Renderer renderer = obj.GetComponent<Renderer>();
            // object shadows
            if (renderer != null)
            {
                dynamic shadow = new ExpandoObject();
                data.shadow = shadow;
                data.shadow.cast = renderer.shadowCastingMode != ShadowCastingMode.Off;
                data.shadow.receive = renderer.receiveShadows;
            }
            // object material
            Material mat = renderer.material;
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
                // object shadows
                if (data.shadow != null)
                {
                    if (data.shadow.cast != null)
                        renderer.shadowCastingMode = Convert.ToBoolean(data.shadow.cast) ? ShadowCastingMode.On : ShadowCastingMode.Off;
                    if (data.shadow.receive != null)
                        renderer.receiveShadows = Convert.ToBoolean(data.shadow.receive);
                }
                // object material
                var material = renderer.material;
                // legacy color overrides material color in the arena
                if (data.color != null) // support legacy arena color
                    material.SetColor("_Color", ToUnityColor((string)data.color));
                else if (data.material != null && data.material.color != null)
                    material.SetColor("_Color", ToUnityColor((string)data.material.color));
                if (data.material != null)
                {
                    if (data.material.shader != null)
                        material.shader.name = (string)data.material.shader == "flat" ? "Unlit/Color" : "Standard";
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

    }
}
