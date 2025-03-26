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

        protected override void ApplyRender()
        {
            // TODO: Implement this component if needed, or note our reasons for not rendering or controlling here.

            gaussiansplat = gameObject.GetComponent<GaussianSplatRenderer>();
            if (gaussiansplat == null)
                gaussiansplat = gameObject.AddComponent<GaussianSplatRenderer>();

            string filetype = null;
            if (Path.HasExtension(json.Src))
            {
                filetype = Path.GetExtension(json.Src);
            }
            switch (filetype)
            {
                case ".ply":
                    StartCoroutine(HandlePlyAssetConversion(Path.GetFileNameWithoutExtension(json.Src)));
                    break;
                case ".splat":
                    //gaussiansplat.m_Asset = some asset;
                    Debug.LogWarning($"GaussianSplatting object '{name}' type {filetype} not yet implemented.");
                    break;
                default:
                    Debug.LogWarning($"GaussianSplatting object '{name}' type {filetype} not supported.");
                    return;
            }
        }

        private IEnumerator HandlePlyAssetConversion(string assetName)
        {
#if UNITY_EDITOR
            var w = EditorWindow.CreateWindow<GaussianSplatAssetCreator>();
            w.titleContent = new GUIContent($"Import GaussianSplatting object '{name}'");
            w.Show();
            // wait for asset creation...
            var mainAssetPath = $"Assets/GaussianAssets/{assetName}";
            Debug.Log($"Waiting for {mainAssetPath}");
            yield return new WaitUntil(() => AssetDatabase.LoadAssetAtPath(mainAssetPath, typeof(GaussianSplatAsset))!=null);
            w.Close();
            gaussiansplat.m_Asset = AssetDatabase.LoadAssetAtPath<GaussianSplatAsset>(mainAssetPath);
            // TODO (mwfarb): this path does not setup shaders well yet....
#else
            Debug.LogWarning($"GaussianSplatting object '{name}' type {filetype} is Editor only, not yet implemented in Runtime mode.");
            //m_InputFile = json.Src;
            //gaussiansplat.m_Asset = GaussianSplatAssetCreator.CreateAsset();
            yield return null;
#endif
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
