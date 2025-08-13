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
            gaussiansplat.m_ShaderComposite = Shader.Find("Hidden/Gaussian Splatting/Composite");
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
            switch (filetype)
            {
                case ".spz":
                case ".ply":
                    StartCoroutine(HandleDotPlyAssetConversion(Path.GetFileNameWithoutExtension(json.Src)));
                    break;
                case ".splat":
                    StartCoroutine(HandleDotSplatAssetConversion(json.Src));
                    break;
                default:
                    Debug.LogWarning($"GaussianSplatting object '{name}' type {filetype} not supported.");
                    return;
            }
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

        private IEnumerator HandleDotSplatAssetConversion(string msgUrl)
        {
            string assetPath = ArenaClientScene.Instance.checkLocalAsset(msgUrl);
            if (File.Exists(assetPath))
            {
                var bytes = File.ReadAllBytes(assetPath);
            }
            GaussianSplatAsset asset = ScriptableObject.CreateInstance<GaussianSplatAsset>();
#if UNITY_EDITOR
            // manually load ComputeShader, it is required
            var splatData = (SplatData)AssetDatabase.LoadAssetAtPath(assetPath, typeof(SplatData));

            //byte[] bPos = new byte[splatData.PositionBuffer.count * sizeof(float) * 3];
            //splatData.PositionBuffer.GetData(bPos);

            //TextAsset dataPos = new TextAsset(System.Text.Encoding.UTF8.GetString(bPos));
            //TextAsset dataOther = null;
            //TextAsset dataColor = null;
            //TextAsset dataSh = null;
            //TextAsset dataChunk = null;

            //float3 boundsMin = float.PositiveInfinity;
            //float3 boundsMax = float.NegativeInfinity;
            //GaussianSplatAsset.CameraInfo[] cameras = null;// LoadJsonCamerasFile(m_InputFile, m_ImportCameras);

            //asset.Initialize(splatData.SplatCount, m_FormatPos, m_FormatScale, m_FormatColor, m_FormatSH, boundsMin, boundsMax, cameras);

            //var dataHash = new Hash128((uint)asset.splatCount, (uint)asset.formatVersion, 0, 0);

            //asset.SetAssetFiles(dataChunk, dataPos, dataOther, dataColor, dataSh);
            //asset.SetDataHash(dataHash);

            //gaussiansplat.m_Asset = asset;

            Debug.LogWarning($"GaussianSplatting object '{name}' type .splat not yet implemented.");
            yield return null;
#else
            Debug.LogWarning($"GaussianSplatting object '{name}' type .splat is Editor only, not yet implemented in Runtime mode.");
#endif
            yield return null;
        }

        private IEnumerator HandleDotPlyAssetConversion(string assetName)
        {
#if UNITY_EDITOR
            string assetPath = ArenaClientScene.Instance.checkLocalAsset(json.Src);
            // wait for asset creation...
            // var ply = new PlyProcessor();
            // var asset = ply.ImportAsPlyData(assetPath);
            var mainAssetPath = $"{assetPath}.asset";
            //var mainAssetPath = assetName;
            //AssetDatabase.AddObjectToAsset(asset, assetPath);
            yield return new WaitUntil(() => AssetDatabase.LoadAssetAtPath<GaussianSplatAsset>(mainAssetPath) != null);
            gaussiansplat.m_Asset = AssetDatabase.LoadAssetAtPath<GaussianSplatAsset>(mainAssetPath);
#else
            Debug.LogWarning($"GaussianSplatting object '{name}' type .ply is Editor only, not yet implemented in Runtime mode.");
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
