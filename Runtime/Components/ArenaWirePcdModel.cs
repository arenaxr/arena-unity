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
    public class ArenaWirePcdModel : ArenaComponent
    {
        // ARENA pcd-model component unity conversion status:
        // DONE: url
        // DONE: pointSize
        // TODO: pointColor

        public ArenaPcdModelJson json = new ArenaPcdModelJson();

#if LIB_GAUSSIAN_SPLATTING
        GaussianSplatRenderer gaussiansplat;
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
                GameObject sobj = new GameObject("PCD");
                sobj.transform.SetParent(transform, false);
                gaussiansplat = sobj.AddComponent<GaussianSplatRenderer>();
            }
            gaussiansplat.m_RenderMode = GaussianSplatRenderer.RenderMode.DebugPoints;
            gaussiansplat.m_PointDisplaySize = json.PointSize;
            //gaussiansplat.m_PointColor = json.PointColor;

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
            if (Path.HasExtension(json.Url))
            {
                filetype = Path.GetExtension(json.Url);
            }
            StartCoroutine(HandleSplatAssetConversion(json.Url));
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
            Debug.LogWarning($"PcdModel object '{assetPath}' is Editor only, not yet implemented in Runtime mode.");
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
