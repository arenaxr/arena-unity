/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2023, Carnegie Mellon University. All rights reserved.
 */

using System;
using System.Collections.Generic;
using System.IO;
using ArenaUnity.Schemas;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace ArenaUnity.Components
{
    public class ArenaMaterial : ArenaComponent
    {
        // ARENA material component unity conversion status:
        // DONE: alphaTest
        // TODO: ambientOcclusionMap (standard/phong texture map)
        // TODO: ambientOcclusionMapIntensity
        // TODO: ambientOcclusionTextureOffset
        // TODO: ambientOcclusionTextureRepeat
        // TODO: anisotropy
        // DONE: blending
        // TODO: bumpMap (phong texture map)
        // TODO: bumpMapScale
        // TODO: bumpTextureOffset
        // TODO: bumpTextureRepeat
        // DONE: color
        // TODO: combine
        // TODO: depthTest
        // DONE: depthWrite
        // TODO: displacementBias (standard/phong texture map)
        // TODO: displacementMap
        // TODO: displacementScale
        // TODO: displacementTextureOffset
        // TODO: displacementTextureRepeat
        // TODO: dithering
        // DONE: emissive
        // DONE: emissiveIntensity
        // TODO: envMap (standard/phong environment map)
        // TODO: flatShading
        // TODO: fog
        // TODO: height
        // DONE: metalness
        // TODO: metalnessMap (standard texture map)
        // TODO: metalnessTextureOffset
        // TODO: metalnessTextureRepeat
        // TODO: normalMap (standard/phong texture map)
        // TODO: normalScale
        // TODO: normalTextureOffset
        // TODO: normalTextureRepeat
        // TODO: npot
        // DONE: offset
        // DONE: opacity
        // TODO: reflectivity
        // TODO: refract
        // TODO: refractionRatio
        // DONE: repeat
        // DONE: roughness
        // TODO: roughnessMap (standard texture map)
        // TODO: roughnessTextureOffset
        // TODO: roughnessTextureRepeat
        // DONE: shader, TODO: add phong
        // TODO: shininess
        // TODO: side
        // TODO: specular
        // TODO: sphericalEnvMap (standard/phong environment map)
        // TODO: src
        // TODO: toneMapped
        // DONE: transparent
        // TODO: vertexColorsEnabled
        // DONE: visible
        // TODO: width
        // DONE: wireframe
        // DONE: wireframeLinewidth

        private const string WireframeShaderName = "Hidden/Arena/Wireframe";
        private const string UnlitBlendShaderName = "Hidden/Arena/UnlitBlend";

        public enum MatRendMode
        {   // TODO: the standards for "_Mode" seem to be missing?
            Opaque = 0,
            Cutout = 1,
            Fade = 2,
            Transparent = 3
        }

        public ArenaMaterialJson json = new ArenaMaterialJson();

        protected override void ApplyRender()
        {
            var renderer = gameObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                // object material
                var material = renderer.material;
                string litShader = "Standard";
                if (ArenaUnity.DefaultRenderPipeline)
                {
                    if (ArenaUnity.DefaultRenderPipeline.GetType().ToString().Contains("HDRenderPipelineAsset"))
                        litShader = "HDRP/Lit";
                    else
                        litShader = "Universal Render Pipeline/Lit";
                }

                if (json != null)
                {
                    renderer.enabled = json.Visible;

                    bool transparent = Convert.ToBoolean(json.Transparent);
                    if (json.Color != null)
                        material.SetColor(ArenaUnity.ColorPropertyName, ArenaUnity.ToUnityColor(json.Color, json.Opacity));

                    Color c = material.GetColor(ArenaUnity.ColorPropertyName);
                    material.SetColor(ArenaUnity.ColorPropertyName, new Color(c.r, c.g, c.b, json.Opacity));

                    // Use custom unlit shader when flat + blending/transparency/depthWrite overrides are needed
                    bool needsCustomUnlit = json.Shader == ArenaMaterialJson.ShaderType.Flat &&
                        (json.Blending != ArenaMaterialJson.BlendingType.Normal || !json.DepthWrite ||
                         Convert.ToBoolean(json.Transparent));
                    if (json.Shader == ArenaMaterialJson.ShaderType.Flat)
                        material.shader = Shader.Find(needsCustomUnlit ? UnlitBlendShaderName : "Unlit/Color");
                    else
                        material.shader = Shader.Find(litShader);

                    // Custom unlit uses _Color property directly
                    if (needsCustomUnlit && json.Color != null)
                        material.SetColor("_Color", ArenaUnity.ToUnityColor(json.Color, json.Opacity));

                    if (material.HasProperty("_Cutoff"))
                        material.SetFloat("_Cutoff", json.AlphaTest);
                    if (material.HasProperty("_Metallic"))
                        material.SetFloat("_Metallic", json.Metalness);
                    if (material.HasProperty("_Glossiness"))
                        material.SetFloat("_Glossiness", 1f - json.Roughness);

                    if (json.Emissive != null && json.Emissive != "#000000" && material.HasProperty("_EmissionColor"))
                    {
                        material.EnableKeyword("_EMISSION");
                        material.SetColor("_EmissionColor", ArenaUnity.ToUnityColor(json.Emissive) * json.EmissiveIntensity);
                    }
                    else
                    {
                        material.DisableKeyword("_EMISSION");
                        if (material.HasProperty("_EmissionColor"))
                            material.SetColor("_EmissionColor", Color.black);
                    }

                    if (json.Repeat != null)
                        material.mainTextureScale = new Vector2(json.Repeat.X, json.Repeat.Y);
                    if (json.Offset != null)
                        material.mainTextureOffset = new Vector2(json.Offset.X, json.Offset.Y);

                    // For runtime set/change transparency mode, follow GUI params
                    // https://github.com/Unity-Technologies/UnityCsReference/blob/master/Editor/Mono/Inspector/StandardShaderGUI.cs#L344
                    if (!transparent || json.Opacity >= 1f)
                    {   // op == 1 or not transparent
                        material.SetFloat("_Mode", (float)MatRendMode.Opaque);
                        material.SetInt("_SrcBlend", (int)BlendMode.One);
                        material.SetInt("_DstBlend", (int)BlendMode.Zero);
                        material.SetInt("_ZWrite", 1);
                        material.DisableKeyword("_ALPHATEST_ON");
                        material.DisableKeyword("_ALPHABLEND_ON");
                        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                        material.renderQueue = -1;
                    }
                    else if (json.Opacity <= 0f)
                    {   // op == 0
                        material.SetFloat("_Mode", (float)MatRendMode.Transparent);
                        material.SetInt("_SrcBlend", (int)BlendMode.One);
                        material.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
                        material.SetInt("_ZWrite", 0);
                        material.EnableKeyword("_ALPHATEST_ON");
                        material.EnableKeyword("_ALPHABLEND_ON");
                        material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                        material.renderQueue = 3000;
                    }
                    else
                    {   // op 0-1
                        material.SetFloat("_Mode", (float)MatRendMode.Fade);
                        material.SetInt("_SrcBlend", (int)BlendMode.One);
                        material.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
                        material.SetInt("_ZWrite", 0);
                        material.DisableKeyword("_ALPHATEST_ON");
                        material.DisableKeyword("_ALPHABLEND_ON");
                        material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                        material.renderQueue = 3000;
                    }

                    // Blending mode (override blend factors set above)
                    if (json.Blending != ArenaMaterialJson.BlendingType.Normal)
                    {
                        switch (json.Blending)
                        {
                            case ArenaMaterialJson.BlendingType.Additive:
                                material.SetFloat("_SrcBlend", (float)BlendMode.One);
                                material.SetFloat("_DstBlend", (float)BlendMode.One);
                                material.renderQueue = 3000;
                                break;
                            case ArenaMaterialJson.BlendingType.Multiply:
                                material.SetFloat("_SrcBlend", (float)BlendMode.DstColor);
                                material.SetFloat("_DstBlend", (float)BlendMode.Zero);
                                material.renderQueue = 3000;
                                break;
                            case ArenaMaterialJson.BlendingType.Subtractive:
                                material.SetFloat("_SrcBlend", (float)BlendMode.Zero);
                                material.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcColor);
                                material.renderQueue = 3000;
                                break;
                            case ArenaMaterialJson.BlendingType.None:
                                material.SetFloat("_SrcBlend", (float)BlendMode.One);
                                material.SetFloat("_DstBlend", (float)BlendMode.Zero);
                                break;
                        }
                    }

                    // DepthWrite override
                    if (!json.DepthWrite)
                        material.SetFloat("_ZWrite", 0);

                    // Wireframe: swap to wireframe shader, inject barycentric coords
                    if (json.Wireframe)
                    {
                        Shader wireShader = Shader.Find(WireframeShaderName);
                        if (wireShader != null)
                        {
                            material.shader = wireShader;
                            Color wireColor = json.Color != null
                                ? ArenaUnity.ToUnityColor(json.Color, json.Opacity)
                                : Color.white;
                            material.SetColor("_Color", wireColor);
                            material.SetFloat("_WireThickness", json.WireframeLinewidth);

                            // Inject barycentric coordinates into UV2 for wireframe rendering
                            var meshFilter = gameObject.GetComponent<MeshFilter>();
                            if (meshFilter != null && meshFilter.mesh != null)
                                InjectBarycentricCoords(meshFilter.mesh);
                        }
                        else
                        {
                            Debug.LogWarning($"Wireframe shader '{WireframeShaderName}' not found. Ensure it is included in the build.");
                        }
                    }
                }
            }
        }

        // material
        public static JObject ToArenaMaterial(GameObject obj)
        {
            var data = new ArenaMaterialJson();
            Renderer renderer = obj.GetComponent<Renderer>();
            // object material
            Material mat = renderer.material;
            if (!mat) return null;
            // shaders only
            switch (mat.shader.name)
            {
                default:
                case "Standard":
                    data.Shader = ArenaMaterialJson.ShaderType.Standard; break;
                case "Unlit/Color":
                case "Unlit/Texture":
                case "Unlit/Texture Colored":
                case "Legacy Shaders/Transparent/Diffuse":
                case UnlitBlendShaderName:
                    data.Shader = ArenaMaterialJson.ShaderType.Flat; break;
            }
            //data.url = ToArenaTexture(mat);
            if (mat.HasProperty(ArenaUnity.ColorPropertyName))
                data.Color = ArenaUnity.ToArenaColor(mat.color);
            if (mat.HasProperty("_Metallic"))
                data.Metalness = ArenaUnity.ArenaFloat(mat.GetFloat("_Metallic"));
            if (mat.HasProperty("_Glossiness"))
                data.Roughness = ArenaUnity.ArenaFloat(1f - mat.GetFloat("_Glossiness"));
            if (mat.HasProperty("_Cutoff"))
                data.AlphaTest = ArenaUnity.ArenaFloat(mat.GetFloat("_Cutoff"));
            if (mat.HasProperty("_EmissionColor"))
                data.Emissive = ArenaUnity.ToArenaColor(mat.GetColor("_EmissionColor"));
            data.Repeat = new ArenaVector2Json { X = ArenaUnity.ArenaFloat(mat.mainTextureScale.x), Y = ArenaUnity.ArenaFloat(mat.mainTextureScale.y) };
            data.Offset = new ArenaVector2Json { X = ArenaUnity.ArenaFloat(mat.mainTextureOffset.x), Y = ArenaUnity.ArenaFloat(mat.mainTextureOffset.y) };
            data.Visible = renderer.enabled;
            //data.side = "double";
            switch ((MatRendMode)mat.GetFloat("_Mode"))
            {
                case MatRendMode.Opaque:
                    data.Transparent = false; break;
                case MatRendMode.Fade:
                case MatRendMode.Transparent:
                case MatRendMode.Cutout:
                    data.Transparent = true; break;
            }
            data.Opacity = ArenaUnity.ArenaFloat(mat.color.a);

            // Wireframe
            if (mat.shader.name == WireframeShaderName)
                data.Wireframe = true;

            // DepthWrite
            if (mat.HasProperty("_ZWrite") && mat.GetInt("_ZWrite") == 0)
                data.DepthWrite = false;

            return data != null ? JObject.FromObject(data) : null;
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

        internal static void AttachMaterialTexture(string assetPath, GameObject gobj)
        {
            if (assetPath == null) return;
            if (File.Exists(assetPath))
            {
                var bytes = File.ReadAllBytes(assetPath);
                var tex = new Texture2D(1, 1);
                tex.LoadImage(bytes);
                var renderer = gobj.GetComponent<Renderer>();
                if (renderer != null)
                    renderer.material.mainTexture = tex;
            }
        }

        /// <summary>
        /// Inject barycentric coordinates into a mesh's UV2 channel for wireframe rendering.
        /// Each triangle vertex gets a unique (1,0,0), (0,1,0), (0,0,1) coordinate.
        /// Vertices are duplicated so each triangle has its own unique set.
        /// </summary>
        private static void InjectBarycentricCoords(Mesh mesh)
        {
            if (mesh.uv2 != null && mesh.uv2.Length == mesh.vertexCount && mesh.vertexCount > 0)
            {
                // Check if barycentric coords were already injected
                var existingUv2 = mesh.uv2;
                if (existingUv2.Length > 0 && (existingUv2[0].x > 0.9f || existingUv2[0].y > 0.9f))
                    return;
            }

            int[] triangles = mesh.triangles;
            Vector3[] oldVerts = mesh.vertices;
            Vector3[] oldNormals = mesh.normals;
            Vector2[] oldUv = mesh.uv;

            int triCount = triangles.Length;
            var newVerts = new Vector3[triCount];
            var newNormals = new Vector3[triCount];
            var newUv = new Vector2[triCount];
            var baryCoords = new List<Vector3>(triCount);
            var newTriangles = new int[triCount];

            Vector3[] baryValues = { new Vector3(1, 0, 0), new Vector3(0, 1, 0), new Vector3(0, 0, 1) };

            for (int i = 0; i < triCount; i++)
            {
                int oldIdx = triangles[i];
                newVerts[i] = oldVerts[oldIdx];
                if (oldNormals.Length > oldIdx)
                    newNormals[i] = oldNormals[oldIdx];
                if (oldUv.Length > oldIdx)
                    newUv[i] = oldUv[oldIdx];
                baryCoords.Add(baryValues[i % 3]);
                newTriangles[i] = i;
            }

            mesh.vertices = newVerts;
            if (oldNormals.Length > 0)
                mesh.normals = newNormals;
            if (oldUv.Length > 0)
                mesh.uv = newUv;
            mesh.SetUVs(1, baryCoords);
            mesh.triangles = newTriangles;
        }
    }
}
