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
        // DONE: ambientOcclusionMap (standard/phong texture map)
        // DONE: ambientOcclusionMapIntensity
        // DONE: ambientOcclusionTextureOffset
        // DONE: ambientOcclusionTextureRepeat
        // DONE: anisotropy (HDRP only)
        // DONE: blending
        // DONE: bumpMap (phong texture map)
        // DONE: bumpMapScale
        // DONE: bumpTextureOffset
        // DONE: bumpTextureRepeat
        // DONE: color
        // TODO: combine (requires custom shader for env map blending modes)
        // DONE: depthTest
        // DONE: depthWrite
        // DONE: displacementBias (HDRP only, native tessellation/displacement)
        // DONE: displacementMap (HDRP only, native tessellation/displacement)
        // DONE: displacementScale (HDRP only, native tessellation/displacement)
        // DONE: displacementTextureOffset (HDRP only, native tessellation/displacement)
        // DONE: displacementTextureRepeat (HDRP only, native tessellation/displacement)
        // DONE: dithering (HDRP/URP only)
        // DONE: emissive
        // DONE: emissiveIntensity
        // DONE: envMap (HDRP/URP reflection probes)
        // DONE: flatShading
        // DONE: fog
        // DONE: metalness
        // DONE: metalnessMap (standard texture map)
        // DONE: metalnessTextureOffset
        // DONE: metalnessTextureRepeat
        // DONE: normalMap (standard/phong texture map)
        // DONE: normalScale
        // DONE: normalTextureOffset
        // DONE: normalTextureRepeat
        // N/A:  npot (Unity handles non-power-of-two textures automatically)
        // DONE: offset
        // DONE: opacity
        // DONE: reflectivity
        // TODO: refract (requires custom refraction shader)
        // TODO: refractionRatio (requires custom refraction shader)
        // DONE: repeat
        // DONE: roughness
        // DONE: roughnessMap (standard texture map)
        // DONE: roughnessTextureOffset
        // DONE: roughnessTextureRepeat
        // DONE: shader (standard, flat, phong)
        // DONE: shininess
        // DONE: side
        // DONE: specular
        // DONE: sphericalEnvMap (HDRP/URP reflection probes)
        // DONE: src
        // DONE: toneMapped (HDRP/URP only)
        // DONE: transparent
        // DONE: vertexColorsEnabled
        // DONE: visible
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
                    else if (json.Shader == ArenaMaterialJson.ShaderType.Phong)
                        material.shader = Shader.Find(litShader == "Standard" ? "Standard (Specular setup)" : litShader);
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

                    // Phong-specific properties (Standard Specular setup)
                    if (json.Shader == ArenaMaterialJson.ShaderType.Phong)
                    {
                        // Shininess: A-Frame 0-128 → Unity glossiness 0-1
                        if (material.HasProperty("_Glossiness"))
                            material.SetFloat("_Glossiness", Mathf.Clamp01(json.Shininess / 128f));
                        // Specular color
                        if (json.Specular != null && material.HasProperty("_SpecColor"))
                            material.SetColor("_SpecColor", ArenaUnity.ToUnityColor(json.Specular));
                        // Reflectivity (glossy reflections strength)
                        if (material.HasProperty("_GlossyReflections"))
                            material.SetFloat("_GlossyReflections", json.Reflectivity);
                    }

                    // Side (face culling)
                    // A-Frame: front = render front face, back = render back face, double = render both
                    // Unity _Cull: 0 = Off (double), 1 = Front (render back), 2 = Back (render front, default)
                    if (material.HasProperty("_Cull"))
                    {
                        switch (json.Side)
                        {
                            case ArenaMaterialJson.SideType.Front:
                                material.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Back); break;
                            case ArenaMaterialJson.SideType.Back:
                                material.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Front); break;
                            case ArenaMaterialJson.SideType.Double:
                                material.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off); break;
                        }
                    }

                    // DepthTest
                    if (material.HasProperty("_ZTest"))
                    {
                        material.SetInt("_ZTest", json.DepthTest
                            ? (int)UnityEngine.Rendering.CompareFunction.LessEqual
                            : (int)UnityEngine.Rendering.CompareFunction.Always);
                    }

                    // Normal map
                    if (!string.IsNullOrEmpty(json.NormalMap) && ArenaClientScene.Instance != null)
                    {
                        string normalMapPath = ArenaClientScene.Instance.checkLocalAsset(json.NormalMap);
                        if (normalMapPath == null) ArenaClientScene.Instance.RegisterAssetCallback(json.NormalMap, () => { apply = true; });
                        else AttachTextureMap(normalMapPath, material, "_BumpMap", "_NORMALMAP",
                            json.NormalScale, "_BumpScale",
                            json.NormalTextureOffset, json.NormalTextureRepeat, linear: true);
                    }

                    // Ambient occlusion map
                    if (!string.IsNullOrEmpty(json.AmbientOcclusionMap) && ArenaClientScene.Instance != null)
                    {
                        string aoMapPath = ArenaClientScene.Instance.checkLocalAsset(json.AmbientOcclusionMap);
                        if (aoMapPath == null) ArenaClientScene.Instance.RegisterAssetCallback(json.AmbientOcclusionMap, () => { apply = true; });
                        else
                        {
                            AttachTextureMap(aoMapPath, material, "_OcclusionMap", null,
                                null, null,
                                json.AmbientOcclusionTextureOffset, json.AmbientOcclusionTextureRepeat);
                            if (material.HasProperty("_OcclusionStrength"))
                                material.SetFloat("_OcclusionStrength", json.AmbientOcclusionMapIntensity);
                        }
                    }

                    // Metalness map
                    if (!string.IsNullOrEmpty(json.MetalnessMap) && ArenaClientScene.Instance != null)
                    {
                        string metMapPath = ArenaClientScene.Instance.checkLocalAsset(json.MetalnessMap);
                        if (metMapPath == null) ArenaClientScene.Instance.RegisterAssetCallback(json.MetalnessMap, () => { apply = true; });
                        else AttachTextureMap(metMapPath, material, "_MetallicGlossMap", "_METALLICGLOSSMAP",
                            null, null,
                            json.MetalnessTextureOffset, json.MetalnessTextureRepeat);
                    }

                    // Roughness map (Unity packs smoothness into metallic alpha)
                    if (!string.IsNullOrEmpty(json.RoughnessMap) && ArenaClientScene.Instance != null)
                    {
                        string roughMapPath = ArenaClientScene.Instance.checkLocalAsset(json.RoughnessMap);
                        if (roughMapPath == null) ArenaClientScene.Instance.RegisterAssetCallback(json.RoughnessMap, () => { apply = true; });
                        else
                        {
                            AttachTextureMap(roughMapPath, material, "_MetallicGlossMap", "_METALLICGLOSSMAP",
                                null, null,
                                json.RoughnessTextureOffset, json.RoughnessTextureRepeat);
                            // Roughness maps need smoothness source set to albedo alpha
                            if (material.HasProperty("_SmoothnessTextureChannel"))
                                material.SetFloat("_SmoothnessTextureChannel", 1); // 1 = albedo alpha
                        }
                    }

                    // Bump map (phong — uses same Unity property as normalMap)
                    if (!string.IsNullOrEmpty(json.BumpMap) && ArenaClientScene.Instance != null)
                    {
                        string bumpMapPath = ArenaClientScene.Instance.checkLocalAsset(json.BumpMap);
                        if (bumpMapPath == null) ArenaClientScene.Instance.RegisterAssetCallback(json.BumpMap, () => { apply = true; });
                        else AttachTextureMap(bumpMapPath, material, "_BumpMap", "_NORMALMAP",
                            new ArenaVector2Json { X = json.BumpMapScale, Y = json.BumpMapScale }, "_BumpScale",
                            json.BumpTextureOffset, json.BumpTextureRepeat, linear: true);
                    }

                    // Src texture
                    if (!string.IsNullOrEmpty(json.Src) && ArenaClientScene.Instance != null)
                    {
                        string srcPath = ArenaClientScene.Instance.checkLocalAsset(json.Src);
                        if (srcPath == null) ArenaClientScene.Instance.RegisterAssetCallback(json.Src, () => { apply = true; });
                        else AttachMaterialTexture(srcPath, gameObject);
                    }

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

                    // FlatShading
                    if (json.FlatShading)
                    {
                        var meshFilter = gameObject.GetComponent<MeshFilter>();
                        if (meshFilter != null && meshFilter.sharedMesh != null)
                        {
                            // Duplicate mesh to avoid modifying shared mesh
                            var mesh = Instantiate(meshFilter.sharedMesh);
                            mesh.RecalculateNormals();
                            meshFilter.mesh = mesh;
                        }
                    }

                    // Fog
                    if (!json.Fog)
                    {
                        material.DisableKeyword("_FOG");
                        material.SetShaderPassEnabled("FORWARD", true); // ensure forward pass still runs
                    }
                    else
                    {
                        material.EnableKeyword("_FOG");
                    }

                    // VertexColorsEnabled
                    if (json.VertexColorsEnabled)
                        material.EnableKeyword("_VERTEXCOLOR");
                    else
                        material.DisableKeyword("_VERTEXCOLOR");

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

                    // Pipeline detection for HDRP/URP-specific properties
                    bool isHDRP = ArenaUnity.DefaultRenderPipeline != null &&
                        ArenaUnity.DefaultRenderPipeline.GetType().ToString().Contains("HDRenderPipelineAsset");
                    bool isURP = ArenaUnity.DefaultRenderPipeline != null && !isHDRP;
                    bool isStandardRP = ArenaUnity.DefaultRenderPipeline == null;

                    // Dithering (HDRP/URP)
                    if (!json.Dithering) // default is true, only act when disabled
                    {
                        if (isHDRP && material.HasProperty("_AlphaCutoffEnable"))
                            material.SetFloat("_AlphaCutoffEnable", 0);
                        else if (isStandardRP)
                            Debug.LogWarning("Material property 'dithering' requires HDRP or URP. No effect in Standard RP.");
                    }

                    // ToneMapped (HDRP/URP)
                    if (!json.ToneMapped) // default is true, only act when disabled
                    {
                        if (isHDRP)
                        {
                            // HDRP unlit materials can disable tone mapping
                            if (material.HasProperty("_ExcludeFromToneMapping"))
                                material.SetFloat("_ExcludeFromToneMapping", 1);
                        }
                        else if (isStandardRP)
                            Debug.LogWarning("Material property 'toneMapped' requires HDRP or URP. No effect in Standard RP.");
                    }

                    // Anisotropy (HDRP only)
                    if (json.Anisotropy != 0f)
                    {
                        if (isHDRP)
                        {
                            if (material.HasProperty("_Anisotropy"))
                            {
                                material.EnableKeyword("_ANISOTROPY");
                                material.SetFloat("_Anisotropy", json.Anisotropy);
                            }
                        }
                        else
                            Debug.LogWarning("Material property 'anisotropy' requires HDRP. No effect in Standard RP or URP.");
                    }

                    // Displacement map (HDRP only)
                    if (!string.IsNullOrEmpty(json.DisplacementMap))
                    {
                        if (isHDRP && ArenaClientScene.Instance != null)
                        {
                            string dispMapPath = ArenaClientScene.Instance.checkLocalAsset(json.DisplacementMap);
                            if (dispMapPath == null) ArenaClientScene.Instance.RegisterAssetCallback(json.DisplacementMap, () => { apply = true; });
                            else if (material.HasProperty("_HeightMap"))
                            {
                                AttachTextureMap(dispMapPath, material, "_HeightMap", null,
                                    null, null,
                                    json.DisplacementTextureOffset, json.DisplacementTextureRepeat);
                                // HDRP displacement mode: 1 = pixel displacement, 2 = vertex displacement
                                if (material.HasProperty("_DisplacementMode"))
                                    material.SetFloat("_DisplacementMode", 2); // vertex displacement
                                if (material.HasProperty("_HeightAmplitude"))
                                    material.SetFloat("_HeightAmplitude", json.DisplacementScale);
                                if (material.HasProperty("_HeightCenter"))
                                    material.SetFloat("_HeightCenter", json.DisplacementBias);
                            }
                        }
                        else if (!isHDRP)
                            Debug.LogWarning("Material property 'displacementMap' requires HDRP. No effect in Standard RP or URP.");
                    }

                    // Environment map
                    if (!string.IsNullOrEmpty(json.EnvMap) && ArenaClientScene.Instance != null)
                    {
                        string envMapPath = ArenaClientScene.Instance.checkLocalAsset(json.EnvMap);
                        if (envMapPath == null) ArenaClientScene.Instance.RegisterAssetCallback(json.EnvMap, () => { apply = true; });
                        else if (isHDRP && material.HasProperty("_ReflectionCubemap"))
                            AttachTextureMap(envMapPath, material, "_ReflectionCubemap", null, null, null, null, null);
                        else if (material.HasProperty("_Cube"))
                            AttachTextureMap(envMapPath, material, "_Cube", null, null, null, null, null);
                        else
                            Debug.LogWarning("Material property 'envMap' could not find a reflection texture property on the current shader.");
                    }

                    // Spherical environment map
                    if (!string.IsNullOrEmpty(json.SphericalEnvMap) && ArenaClientScene.Instance != null)
                    {
                        // Spherical env maps (equirectangular) need conversion to cubemap;
                        // for now, attempt direct texture assignment with a warning
                        string sphereEnvPath = ArenaClientScene.Instance.checkLocalAsset(json.SphericalEnvMap);
                        if (sphereEnvPath == null) ArenaClientScene.Instance.RegisterAssetCallback(json.SphericalEnvMap, () => { apply = true; });
                        else if (isHDRP && material.HasProperty("_ReflectionCubemap"))
                            AttachTextureMap(sphereEnvPath, material, "_ReflectionCubemap", null, null, null, null, null);
                        else if (material.HasProperty("_Cube"))
                            AttachTextureMap(sphereEnvPath, material, "_Cube", null, null, null, null, null);
                        else
                            Debug.LogWarning("Material property 'sphericalEnvMap' could not find a reflection texture property. Equirectangular-to-cubemap conversion may be needed.");
                    }

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
                case "Standard (Specular setup)":
                    data.Shader = ArenaMaterialJson.ShaderType.Phong;
                    // Export phong-specific properties
                    if (mat.HasProperty("_Glossiness"))
                        data.Shininess = ArenaUnity.ArenaFloat(mat.GetFloat("_Glossiness") * 128f);
                    if (mat.HasProperty("_SpecColor"))
                        data.Specular = ArenaUnity.ToArenaColor(mat.GetColor("_SpecColor"));
                    if (mat.HasProperty("_GlossyReflections"))
                        data.Reflectivity = ArenaUnity.ArenaFloat(mat.GetFloat("_GlossyReflections"));
                    break;
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

            // FlatShading — cannot reliably detect from material alone, skip in export

            // Fog
            if (mat.IsKeywordEnabled("_FOG"))
                data.Fog = true;
            else
                data.Fog = false; // default is true in A-Frame, but we export the actual state

            // VertexColorsEnabled
            data.VertexColorsEnabled = mat.IsKeywordEnabled("_VERTEXCOLOR");

            // Side (face culling)
            if (mat.HasProperty("_Cull"))
            {
                switch ((UnityEngine.Rendering.CullMode)mat.GetInt("_Cull"))
                {
                    case UnityEngine.Rendering.CullMode.Back:
                        data.Side = ArenaMaterialJson.SideType.Front; break;
                    case UnityEngine.Rendering.CullMode.Front:
                        data.Side = ArenaMaterialJson.SideType.Back; break;
                    case UnityEngine.Rendering.CullMode.Off:
                        data.Side = ArenaMaterialJson.SideType.Double; break;
                }
            }

            // DepthTest
            if (mat.HasProperty("_ZTest"))
            {
                var zTest = (UnityEngine.Rendering.CompareFunction)mat.GetInt("_ZTest");
                data.DepthTest = zTest != UnityEngine.Rendering.CompareFunction.Always;
            }

            // Normal map
            if (mat.HasProperty("_BumpMap") && mat.GetTexture("_BumpMap") != null)
            {
                if (mat.HasProperty("_BumpScale"))
                    data.NormalScale = new ArenaVector2Json { X = ArenaUnity.ArenaFloat(mat.GetFloat("_BumpScale")), Y = ArenaUnity.ArenaFloat(mat.GetFloat("_BumpScale")) };
            }

            // Ambient occlusion map
            if (mat.HasProperty("_OcclusionMap") && mat.GetTexture("_OcclusionMap") != null)
            {
                if (mat.HasProperty("_OcclusionStrength"))
                    data.AmbientOcclusionMapIntensity = ArenaUnity.ArenaFloat(mat.GetFloat("_OcclusionStrength"));
            }

            // Metalness map
            if (mat.HasProperty("_MetallicGlossMap") && mat.GetTexture("_MetallicGlossMap") != null)
            {
                // Note: map URL is not recoverable from Unity texture
            }

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
            PublishIfChanged(json.attributeName, JsonConvert.SerializeObject(json));
        }

        private static readonly HashSet<string> videoExtensions = new HashSet<string>(
            StringComparer.OrdinalIgnoreCase) { ".mp4", ".webm", ".ogg", ".mov" };

        internal static void AttachMaterialTexture(string assetPath, GameObject gobj)
        {
            if (assetPath == null) return;
            if (!File.Exists(assetPath)) return;

            string ext = Path.GetExtension(assetPath);
            if (videoExtensions.Contains(ext))
            {
                // Video: use VideoPlayer → RenderTexture → material
                var vp = gobj.GetComponent<UnityEngine.Video.VideoPlayer>();
                if (vp == null)
                    vp = gobj.AddComponent<UnityEngine.Video.VideoPlayer>();
                vp.source = UnityEngine.Video.VideoSource.Url;
                vp.url = Path.GetFullPath(assetPath);
                vp.isLooping = true;
                vp.playOnAwake = true;
                vp.renderMode = UnityEngine.Video.VideoRenderMode.MaterialOverride;
                var renderer = gobj.GetComponent<Renderer>();
                if (renderer != null)
                {
                    vp.targetMaterialRenderer = renderer;
                    vp.targetMaterialProperty = "_MainTex";
                }
                vp.Play();
            }
            else
            {
                // Image: load texture directly
                var bytes = File.ReadAllBytes(assetPath);
                var tex = new Texture2D(1, 1);
                tex.LoadImage(bytes);
                var renderer = gobj.GetComponent<Renderer>();
                if (renderer != null)
                    renderer.material.mainTexture = tex;
            }
        }

        /// <summary>
        /// Load a texture from a local file and attach it to a material property.
        /// Shared helper for normalMap, ambientOcclusionMap, metalnessMap, roughnessMap, bumpMap.
        /// </summary>
        private static void AttachTextureMap(
            string assetPath, Material material,
            string textureProperty, string keyword,
            ArenaVector2Json scale, string scaleProperty,
            ArenaVector2Json textureOffset, ArenaVector2Json textureRepeat,
            bool linear = false)
        {
            if (string.IsNullOrEmpty(assetPath) || !File.Exists(assetPath)) return;
            if (!material.HasProperty(textureProperty)) return;

            var bytes = File.ReadAllBytes(assetPath);
            var tex = linear
                ? new Texture2D(1, 1, TextureFormat.RGBA32, true, true)
                : new Texture2D(1, 1);
            tex.LoadImage(bytes);

            material.SetTexture(textureProperty, tex);
            if (!string.IsNullOrEmpty(keyword))
                material.EnableKeyword(keyword);

            if (scale != null && !string.IsNullOrEmpty(scaleProperty) && material.HasProperty(scaleProperty))
                material.SetFloat(scaleProperty, scale.X);

            // Apply per-map texture offset and repeat via material property IDs
            string stProperty = textureProperty + "_ST";
            if (material.HasProperty(textureProperty))
            {
                Vector2 offset = textureOffset != null ? new Vector2(textureOffset.X, textureOffset.Y) : Vector2.zero;
                Vector2 repeat = textureRepeat != null ? new Vector2(textureRepeat.X, textureRepeat.Y) : Vector2.one;
                material.SetTextureOffset(textureProperty, offset);
                material.SetTextureScale(textureProperty, repeat);
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
