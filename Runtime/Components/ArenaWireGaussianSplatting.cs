/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2023, Carnegie Mellon University. All rights reserved.
 */

using System.Collections;
using System.IO;
using ArenaUnity.Components;
using ArenaUnity.Schemas;
using Newtonsoft.Json;
using UnityEngine;
using Unity.Burst;
#if UNITY_EDITOR
using UnityEditor;
#endif

#if LIB_GAUSSIAN_SPLATTING
using GaussianSplatting.Runtime;
#endif

namespace ArenaUnity
{
    [BurstCompile]
    public class ArenaWireGaussianSplatting : ArenaComponent
    {
        // ARENA gaussian_splatting component unity conversion status:
        // DONE: src
        // DONE: cutoutEntity
        // TODO: pixelRatio
        // TODO: xrPixelRatio

        // References
        // https://github.com/aras-p/UnityGaussianSplatting
        // https://github.com/quadjr/aframe-gaussian-splatting
        // https://github.com/akbartus/Gaussian-Splatting-WebViewers
        // https://github.com/antimatter15/splat
        // https://github.com/keijiro/SplatVFX
        // https://github.com/mkkellogg/GaussianSplats3D

        public ArenaGaussianSplattingJson json = new ArenaGaussianSplattingJson();

#if LIB_GAUSSIAN_SPLATTING
        GaussianSplatRenderer gaussiansplat;
        GaussianCutout gaussiancutout;
        ComputeShader compShader;

        void OnEnable()
        {
            // TODO (mwfarb): add an editor check for compute shader at build time.
#if UNITY_EDITOR
            // manually load ComputeShader, it is required
            compShader = (ComputeShader)AssetDatabase.LoadAssetAtPath("Packages/org.nesnausk.gaussian-splatting/Shaders/SplatUtilities.compute", typeof(ComputeShader));
#endif
            //ApplyQualityLevel();
        }

        protected override void ApplyRender()
        {
            // assign splat renderer
            gaussiansplat = GetComponentInChildren<GaussianSplatRenderer>();
            if (gaussiansplat == null)
            {
                GameObject sobj = new GameObject("Splat");
                sobj.transform.SetParent(transform, false);
                gaussiansplat = sobj.AddComponent<GaussianSplatRenderer>();
            }

            // assign splat cutout
            if (json.CutoutEntity != null)
            {
                string cutout_id = json.CutoutEntity.TrimStart('#');
                StartCoroutine(SeekCutout(cutout_id));
            }

            // load required shaders
            gaussiansplat.m_ShaderSplats = Shader.Find("Gaussian Splatting/Render Splats");
            gaussiansplat.m_ShaderComposite = Shader.Find("Hidden/Gaussian Splatting/CompositeArena");
            gaussiansplat.m_ShaderDebugPoints = Shader.Find("Gaussian Splatting/Debug/Render Points");
            gaussiansplat.m_ShaderDebugBoxes = Shader.Find("Gaussian Splatting/Debug/Render Boxes");
            ComputeShader[] compShaders = Resources.FindObjectsOfTypeAll<ComputeShader>();
            for (int i = 0; i < compShaders.Length; i++)
            {
                if (compShaders[i].name == "SplatUtilities")
                {
                    gaussiansplat.m_CSSplatUtilities = compShaders[i];
                    break;
                }
            }


            string filetype = null;
            if (Path.HasExtension(json.Src))
            {
                filetype = Path.GetExtension(json.Src);
            }
            StartCoroutine(HandleSplatAssetConversion(json.Src));
        }

        private IEnumerator SeekCutout(string cutout_id)
        {
            yield return new WaitUntil(() => GameObject.Find(cutout_id) != null);
            var cobj = GameObject.Find(cutout_id);
            var aobj = cobj.GetComponent<ArenaObject>();
            if (aobj == null) yield return null;
            gaussiancutout = cobj.GetComponentInChildren<GaussianCutout>();
            if (gaussiancutout == null)
            {
                GameObject sobj = new GameObject("Splat Cutout");
                sobj.transform.SetParent(cobj.transform, false);
                gaussiancutout = sobj.AddComponent<GaussianCutout>();
            }
            gaussiancutout.m_Type = (aobj.object_type == "box" || aobj.object_type == "roundedbox") ? GaussianCutout.Type.Box : GaussianCutout.Type.Ellipsoid;
            gaussiancutout.transform.localScale = gaussiancutout.transform.localScale / 2; // match ARENA a-frame gaussian components
            gaussiancutout.m_Invert = false; // aframe-gaussian-splatting does not support inverted cutouts yet
            gaussiansplat.m_Cutouts = new GaussianCutout[] { gaussiancutout };
            yield return null;
        }

        private IEnumerator HandleSplatAssetConversion(string msgUrl)
        {
            string assetPath = ArenaClientScene.Instance.checkLocalAsset(msgUrl);
#if UNITY_EDITOR
            // wait for asset creation from import post processing...
            var mainAssetPath = $"{assetPath}.asset";
            yield return new WaitUntil(() => AssetDatabase.LoadAssetAtPath<GaussianSplatAsset>(mainAssetPath) != null);
            gaussiansplat.m_Asset = AssetDatabase.LoadAssetAtPath<GaussianSplatAsset>(mainAssetPath);
#else
            Debug.LogWarning($"GaussianSplatting object '{assetPath}' is Editor only, not yet implemented in Runtime mode.");
#endif
            yield return null;
        }

#else
        protected override void ApplyRender()
        {
            // placeholder before LIB_GAUSSIAN_SPLATTING load
        }
#endif

        public override void UpdateObject()
        {
            var newJson = JsonConvert.SerializeObject(json);
            if (updatedJson != newJson)
            {
                var aobj = GetComponent<ArenaObject>();
                if (aobj != null)
                {
                    aobj.PublishUpdate($"{newJson}");
                    apply = true;
                }
            }
            updatedJson = newJson;
        }
    }
}
