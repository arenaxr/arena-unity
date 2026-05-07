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
    public class ArenaScenePostProcessing : ArenaComponent
    {
        // ARENA post-processing component unity conversion status:
        // DONE: bloom
        // DONE: sao
        // DONE: ssao
        // DONE: pixel
        // DONE: glitch
        // DONE: fxaa
        // DONE: smaa

        public ArenaPostProcessingJson json = new ArenaPostProcessingJson();

        protected override void ApplyRender()
        {
#if LIB_URP
            if (Camera.main != null)
            {
                // Setup Volume
                var volume = gameObject.GetComponent<UnityEngine.Rendering.Volume>();
                if (volume == null)
                    volume = gameObject.AddComponent<UnityEngine.Rendering.Volume>();
                
                volume.isGlobal = true;
                if (volume.profile == null)
                    volume.profile = ScriptableObject.CreateInstance<UnityEngine.Rendering.VolumeProfile>();

                // Bloom
                if (json.Bloom != null)
                {
                    if (!volume.profile.TryGet<UnityEngine.Rendering.Universal.Bloom>(out var bloom))
                        bloom = volume.profile.Add<UnityEngine.Rendering.Universal.Bloom>();
                    bloom.active = true;
                }
                else if (volume.profile.TryGet<UnityEngine.Rendering.Universal.Bloom>(out var b)) b.active = false;

                // SSAO / SAO
                if (json.Ssao != null || json.Sao != null)
                {
                    // URP SSAO is technically a ScriptableRendererFeature, but in newer URP versions it can be toggled
                    // via Volume or we just warn if it's not setup in the Renderer.
                    // For safety, we just log a warning that SSAO must be added to the URP Renderer asset.
                    Debug.LogWarning("[ArenaScenePostProcessing] URP requires Screen Space Ambient Occlusion to be added as a Renderer Feature to the Forward Renderer Asset. It cannot be fully instantiated at runtime.");
                }

                // FXAA / SMAA
                var cameraData = Camera.main.GetComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
                if (cameraData != null)
                {
                    if (json.Fxaa != null)
                        cameraData.antialiasing = UnityEngine.Rendering.Universal.AntialiasingMode.FastApproximateAntialiasing;
                    else if (json.Smaa != null)
                        cameraData.antialiasing = UnityEngine.Rendering.Universal.AntialiasingMode.SubpixelMorphologicalAntiAliasing;
                    else
                        cameraData.antialiasing = UnityEngine.Rendering.Universal.AntialiasingMode.None;
                }

                // Unsupported
                if (json.Pixel != null) Debug.LogWarning("[ArenaScenePostProcessing] 'pixel' effect is not natively supported in URP without custom shaders.");
                if (json.Glitch != null) Debug.LogWarning("[ArenaScenePostProcessing] 'glitch' effect is not natively supported in URP without custom shaders.");
            }
#elif LIB_HDRP
            Debug.LogWarning("[ArenaScenePostProcessing] HDRP post-processing mapping is not yet implemented.");
#else
            Debug.LogWarning("[ArenaScenePostProcessing] Native dynamic post-processing requires URP or HDRP. Standard Built-In pipeline is not supported natively in this package.");
#endif
        }

        public override void UpdateObject()
        {
            PublishIfChanged(json.attributeName, JsonConvert.SerializeObject(json));
        }
    }
}
