/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2023, Carnegie Mellon University. All rights reserved.
 */

using System.Collections;
using System.IO;
using ArenaUnity.Components;
using ArenaUnity.Schemas;
using GaussianSplatting.Editor;
using GaussianSplatting.Runtime;
using Newtonsoft.Json;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ArenaUnity
{
    public class ArenaWireGaussianSplatting : ArenaComponent
    {
        // ARENA gaussiansplatting component unity conversion status:
        // TODO: src
        // TODO: cutoutEntity
        // TODO: pixelRatio
        // TODO: xrPixelRatio

        // References
        // https://github.com/aras-p/UnityGaussianSplatting
        // https://github.com/quadjr/aframe-gaussian-splatting
        // https://github.com/akbartus/Gaussian-Splatting-WebViewers
        // https://github.com/antimatter15/splat
        // https://github.com/keijiro/SplatVFX

        public ArenaGaussianSplattingJson json = new ArenaGaussianSplattingJson();
        GaussianSplatRenderer gaussiansplat;
        GaussianCutout gaussiancutout;

        protected override void ApplyRender()
        {
            // TODO: Implement this component if needed, or note our reasons for not rendering or controlling here.

            GameObject sobj = new GameObject("Splat");
            sobj.transform.SetParent(transform, false);
            sobj.transform.rotation *= Quaternion.AngleAxis(180, transform.right);
            // assign splat renderer
            gaussiansplat = sobj.GetComponent<GaussianSplatRenderer>();
            if (gaussiansplat == null)
                gaussiansplat = sobj.AddComponent<GaussianSplatRenderer>();

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

        private IEnumerator SeekCutout(string cutoutEntity)
        {
            yield return new WaitUntil(() => GameObject.Find(cutoutEntity) != null);
            var cobj = GameObject.Find(cutoutEntity);
            gaussiancutout = cobj.GetComponent<GaussianCutout>();
            if (gaussiancutout == null)
                gaussiancutout = cobj.AddComponent<GaussianCutout>();
            gaussiancutout.m_Type = GaussianCutout.Type.Box;
            gaussiansplat.m_Cutouts = new GaussianCutout[] { gaussiancutout };
            yield return null;
        }

        private IEnumerator HandleDotSplatAssetConversion(string msgUrl)
        {
            var asset = new GaussianSplatAsset();
            string assetPath = ArenaClientScene.Instance.checkLocalAsset(msgUrl);
            if (File.Exists(assetPath))
            {
                var bytes = File.ReadAllBytes(assetPath);
            }

            //TextAsset dataChunk = null;
            //TextAsset dataPos = null;
            //TextAsset dataOther = null;
            //TextAsset dataColor = null;
            //TextAsset dataSh = default;
            //Hash128 hash = default;

            //int splats;
            //VectorFormat formatPos = ;
            //VectorFormat formatScale;
            //ColorFormat formatColor;
            //SHFormat formatSh;
            //Vector3 bMin;
            //Vector3 bMax;
            //CameraInfo[] cameraInfos;

            //asset.Initialize();
            //asset.SetAssetFiles(dataChunk, dataPos, dataOther, dataColor, dataSh);
            //asset.SetDataHash(hash);

            gaussiansplat.m_Asset = asset;

            //Debug.LogWarning($"GaussianSplatting object '{name}' type .splat not yet implemented.");
            yield return null;
        }

        private IEnumerator HandleDotPlyAssetConversion(string assetName)
        {
#if UNITY_EDITOR
            var w = EditorWindow.GetWindowWithRect<GaussianSplatAssetCreator>(new Rect(50, 50, 360, 340), false, $"Import '{name}' GaussianSplatting", true);
            w.minSize = new Vector2(320, 320);
            w.maxSize = new Vector2(1500, 1500);
            w.Show();
            // wait for asset creation...
            var mainAssetPath = $"Assets/GaussianAssets/{assetName}";
            yield return new WaitUntil(() => AssetDatabase.LoadAssetAtPath(mainAssetPath, typeof(GaussianSplatAsset)) != null);
            w.Close();
            gaussiansplat.m_Asset = AssetDatabase.LoadAssetAtPath<GaussianSplatAsset>(mainAssetPath);
            // TODO (mwfarb): this path does not setup shaders well yet....
#else
            Debug.LogWarning($"GaussianSplatting object '{name}' type .pyl is Editor only, not yet implemented in Runtime mode.");
            //m_InputFile = json.Src;
            //gaussiansplat.m_Asset = GaussianSplatAssetCreator.CreateAsset();
#endif
            yield return null;
        }

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
