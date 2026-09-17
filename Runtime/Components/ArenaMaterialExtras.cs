/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2023, Carnegie Mellon University. All rights reserved.
 */

using System.Collections.Generic;
using System.Text.RegularExpressions;
using ArenaUnity.Schemas;
using Newtonsoft.Json;
using UnityEngine;

namespace ArenaUnity.Components
{
    public class ArenaMaterialExtras : ArenaComponent
    {
        // ARENA material-extras component unity conversion status:
        // DONE: overrideSrc
        // TODO: colorSpace
        // DONE: colorWrite
        // DONE: renderOrder
        // DONE: transparentOccluder
        // DONE: gltfOpacity
        // TODO: encoding

        public ArenaMaterialExtrasJson json = new ArenaMaterialExtrasJson();

        private bool materialApplied = false;

        protected override void ApplyRender()
        {
            materialApplied = false;
        }

        protected override void Update()
        {
            base.Update();

            if (!materialApplied)
            {
                var gltfModel = GetComponent<ArenaWireGltfModel>();
                if (gltfModel == null || gltfModel.isLoaded)
                {
                    bool isHDRP = ArenaUnity.DefaultRenderPipeline != null && ArenaUnity.DefaultRenderPipeline.GetType().ToString().Contains("HDRenderPipelineAsset");
                    bool isURP = ArenaUnity.DefaultRenderPipeline != null && !isHDRP;
                    string mainTexProp = (isURP || isHDRP) ? "_BaseMap" : "_MainTex";

                    foreach (var renderer in GetComponentsInChildren<Renderer>())
                    {
                        if (renderer.gameObject.name == "ArenaClickListenerModel") continue;

                        foreach (var material in renderer.materials)
                        {
                            // gltfOpacity
                            if (json.GltfOpacity < 1f)
                            {
                                string oldColorProp = null;
                                string[] colorProps = { "_BaseColor", "_Color", "baseColorFactor" };
                                foreach (var p in colorProps) { if (material.HasProperty(p)) { oldColorProp = p; break; } }
                                
                                string oldTexProp = null;
                                string[] texProps = { "_BaseMap", "_MainTex", "baseColorTexture" };
                                foreach (var p in texProps) { if (material.HasProperty(p)) { oldTexProp = p; break; } }

                                Color c = oldColorProp != null ? material.GetColor(oldColorProp) : Color.white;
                                Texture tex = oldTexProp != null ? material.GetTexture(oldTexProp) : null;

                                // Force standard shader to allow transparency keyword overrides
                                Shader litShader = ArenaUnity.GetLitShader();
                                if (material.shader != litShader)
                                    material.shader = litShader;

                                material.SetColor(ArenaUnity.ColorPropertyName, new Color(c.r, c.g, c.b, json.GltfOpacity));
                                if (tex != null)
                                {
                                    if (material.HasProperty("_BaseMap")) material.SetTexture("_BaseMap", tex);
                                    else if (material.HasProperty("_MainTex")) material.SetTexture("_MainTex", tex);
                                }
                                
                                if (material.HasProperty("_Mode")) material.SetFloat("_Mode", 2); // Standard Fade mode
                                if (material.HasProperty("_Surface")) material.SetFloat("_Surface", 1); // URP Transparent mode
                                if (material.HasProperty("_Blend")) material.SetFloat("_Blend", 0); // URP Alpha Blend
                                
                                material.SetOverrideTag("RenderType", "Transparent");
                                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                                material.SetInt("_ZWrite", 0);
                                material.DisableKeyword("_ALPHATEST_ON");
                                material.EnableKeyword("_ALPHABLEND_ON");
                                material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                                material.renderQueue = 3000;
                            }

                            // renderOrder
                            if (json.RenderOrder != 1f)
                            {
                                // Unity default opaque queue is 2000, transparent is 3000
                                // A-Frame renderOrder simply shifts drawing order. We add it to the base queue.
                                material.renderQueue += (int)json.RenderOrder;
                            }

                            // transparentOccluder
                            if (json.TransparentOccluder)
                            {
                                material.renderQueue = 1999; // Render just before opaque
                                // URP allows ColorMask overrides
                                if (material.HasProperty("_ColorWriteMask"))
                                    material.SetInt("_ColorWriteMask", 0);
                            }

                            // overrideSrc
                            if (!string.IsNullOrEmpty(json.OverrideSrc))
                            {
                                if (ArenaClientScene.Instance != null)
                                {
                                    string srcPath = ArenaClientScene.Instance.checkLocalAsset(json.OverrideSrc);
                                    if (srcPath == null)
                                    {
                                        ArenaClientScene.Instance.RegisterAssetCallback(json.OverrideSrc, () => { apply = true; });
                                    }
                                    else if (System.IO.File.Exists(srcPath))
                                    {
                                        var bytes = System.IO.File.ReadAllBytes(srcPath);
                                        var tex = new Texture2D(1, 1);
                                        tex.LoadImage(bytes);
                                        if (material.HasProperty(mainTexProp))
                                            material.SetTexture(mainTexProp, tex);
                                    }
                                }
                            }
                        }
                    }
                    materialApplied = true;
                }
            }
        }

        public override void UpdateObject()
        {
            PublishIfChanged(json.attributeName, JsonConvert.SerializeObject(json));
        }
    }
}
