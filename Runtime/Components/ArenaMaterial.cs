/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2023, Carnegie Mellon University. All rights reserved.
 */

using System;
using ArenaUnity.Schemas;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Rendering;

namespace ArenaUnity.Components
{
    public class ArenaMaterial : ArenaComponent
    {
        // TODO: ARENA Property Handling Status

        public enum MatRendMode
        {   // TODO: the standards for "_Mode" seem to be missing?
            Opaque = 0,
            Cutout = 1,
            Fade = 2,
            Transparent = 3
        }

        // TODO: [Tooltip("Serializable JSON attributes for Arena .........")]
        public ArenaMaterialJson json = new ArenaMaterialJson();

        protected override void ApplyRender()
        {
            var renderer = GetComponent<Renderer>();
            if (renderer != null)
            {
                // object material
                var material = renderer.material;
                if (GraphicsSettings.renderPipelineAsset)
                {
                    if (GraphicsSettings.renderPipelineAsset.GetType().ToString().Contains("HDRenderPipelineAsset"))
                        material.shader = Shader.Find("HDRP/Lit");
                    else
                        material.shader = Shader.Find("Universal Render Pipeline/Lit");
                }
                float opacity = (json != null && json.Opacity != null) ? (float)json.Opacity : 1f;
                if (json != null)
                {
                    bool transparent = Convert.ToBoolean(json.Transparent);
                    bool opaque = opacity >= 1f;
                    if (json.Color != null)
                        material.SetColor(ArenaUnity.ColorPropertyName, ArenaUnity.ToUnityColor((string)json.Color, opacity));
                    if (json.Opacity != null)
                    {
                        Color c = material.GetColor(ArenaUnity.ColorPropertyName);
                        material.SetColor(ArenaUnity.ColorPropertyName, new Color(c.r, c.g, c.b, opacity));
                    }
                    if (json.Shader != null)
                        material.shader.name = (string)json.Shader == "flat" ? "Unlit/Color" : "Standard";
                    // For runtime set/change transparency mode, follow GUI params
                    // https://github.com/Unity-Technologies/UnityCsReference/blob/master/Editor/Mono/Inspector/StandardShaderGUI.cs#L344
                    if (opacity >= 1f)
                    {   // op == 1
                        material.SetFloat("_Mode", (float)MatRendMode.Opaque);
                        material.SetInt("_SrcBlend", (int)BlendMode.One);
                        material.SetInt("_DstBlend", (int)BlendMode.Zero);
                        material.SetInt("_ZWrite", 1);
                        material.DisableKeyword("_ALPHATEST_ON");
                        material.DisableKeyword("_ALPHABLEND_ON");
                        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                        material.renderQueue = -1;
                    }
                    if (opacity <= 0f)
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
        public static void ToArenaMaterial(GameObject obj, ref ArenaMaterialJson data)
        {
            Renderer renderer = obj.GetComponent<Renderer>();
            // object material
            Material mat = renderer.material;
            if (!mat)
                return;
            // shaders only
            switch (mat.shader.name)
            {
                default:
                case "Standard":
                    data.Shader = "standard"; break;
                case "Unlit/Color":
                case "Unlit/Texture":
                case "Unlit/Texture Colored":
                case "Legacy Shaders/Transparent/Diffuse":
                    data.Shader = "flat"; break;
            }
            //data.url = ToArenaTexture(mat);
            if (mat.HasProperty(ArenaUnity.ColorPropertyName))
                data.Color = ArenaUnity.ToArenaColor(mat.color);
            //data.metalness = ArenaFloat(mat.GetFloat("_Metallic"));
            //data.roughness = ArenaFloat(1f - mat.GetFloat("_Glossiness"));
            //data.repeat = ArenaFloat(mat.mainTextureScale.x);
            //data.side = "double";
            switch ((MatRendMode)mat.GetFloat("_Mode"))
            {
                case MatRendMode.Opaque:
                case MatRendMode.Fade:
                    data.Transparent = false; break;
                case MatRendMode.Transparent:
                case MatRendMode.Cutout:
                    data.Transparent = true; break;
            }
            data.Opacity = ArenaUnity.ArenaFloat(mat.color.a);

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
    }
}
