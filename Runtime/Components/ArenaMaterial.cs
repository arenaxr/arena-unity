/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2023, Carnegie Mellon University. All rights reserved.
 */

using System;
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
        // TODO: anisotropy
        // TODO: blending
        // DONE: color
        // TODO: combine
        // TODO: depthTest
        // TODO: depthWrite
        // TODO: dithering
        // DONE: emissive
        // DONE: emissiveIntensity
        // TODO: flatShading
        // TODO: fog
        // TODO: height
        // DONE: metalness
        // TODO: npot
        // DONE: offset
        // DONE: opacity
        // TODO: reflectivity
        // TODO: refract
        // TODO: refractionRatio
        // DONE: repeat
        // DONE: roughness
        // DONE: shader, TODO: add phong
        // TODO: shininess
        // TODO: side
        // TODO: specular
        // TODO: src
        // TODO: toneMapped
        // DONE: transparent
        // TODO: vertexColorsEnabled
        // DONE: visible
        // TODO: width
        // TODO: wireframe
        // TODO: wireframeLinewidth

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
                // TODO: Implement this component if needed, or note our reasons for not rendering or controlling here.

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
                    material.shader = Shader.Find(json.Shader == ArenaMaterialJson.ShaderType.Flat ? "Unlit/Color" : litShader);

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
    }
}
