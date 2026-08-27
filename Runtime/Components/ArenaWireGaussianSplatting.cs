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

// Two splat back ends can be installed at once so they can be compared on a device.
// wu.yize.gsplat (LIB_GSPLAT) loads at runtime; org.nesnausk.gaussian-splatting
// (LIB_GAUSSIAN_SPLATTING) only loads through the Editor AssetDatabase. Define
// ARENA_SPLAT_LEGACY in Project Settings > Player > Scripting Define Symbols to force the
// older Editor-only path even when wu.yize.gsplat is present.
#if LIB_GSPLAT && !ARENA_SPLAT_LEGACY
using Gsplat;
#elif LIB_GAUSSIAN_SPLATTING
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
        // https://github.com/wuyize25/gsplat-unity
        // https://github.com/aras-p/UnityGaussianSplatting
        // https://github.com/quadjr/aframe-gaussian-splatting
        // https://github.com/akbartus/Gaussian-Splatting-WebViewers
        // https://github.com/antimatter15/splat
        // https://github.com/keijiro/SplatVFX
        // https://github.com/mkkellogg/GaussianSplats3D

        public ArenaGaussianSplattingJson json = new ArenaGaussianSplattingJson();

#if LIB_GSPLAT && !ARENA_SPLAT_LEGACY

        [Tooltip("Coordinate frame the splat file was authored in. RDF (OpenCV/COLMAP) reproduces the single 180-degree-about-X correction the ARENA web client applies; see the comment on SourceFrameForWebParity.")]
        public SourceCoordinates sourceCoordinates = SourceFrameForWebParity;

        [Tooltip("Spark packs positions to half floats and colors to bytes; Uncompressed keeps float32 at higher memory cost.")]
        public CompressionMode compression = CompressionMode.Spark;

        [Tooltip("Convert splat color from Gamma to Linear in the shader. The library author describes this as producing incorrect results, because the conversion happens before the alpha blend; prefer setting the project color space to Gamma.")]
        public bool gammaToLinear = false;

        /// <summary>
        /// The one fixed coordinate correction ARENA applies to splat data, expressed in
        /// gsplat's source-frame terms.
        ///
        /// arena-web-core applies exactly one correction, on the splat mesh and beneath the
        /// entity transform, so authored position/rotation/scale compose on top of it:
        ///   arena-web-core/src/components/object/gaussian-splatting.js
        ///     splatMesh.quaternion.set(1, 0, 0, 0);   // 180 degrees about +X
        /// That maps raw data (x, y, z) to (x, -y, -z) in three.js (right-up-back), and ARENA's
        /// three.js-to-Unity mapping (ArenaUnity.ToUnityPosition) then negates z again, so the
        /// net data-to-Unity mapping is (x, -y, z) -- a flip of Y alone. gsplat expresses that
        /// as a source frame of right-DOWN-front, i.e. SourceCoordinates.RDF, which is also what
        /// the coordinate-system note in ArenaUnity.cs records for PLY.
        ///
        /// Choosing RDF instead of gsplat's own RUB default is exactly a 180-degree rotation
        /// about X, which is the correction under discussion; RUB would leave splats upside down
        /// relative to the web client.
        ///
        /// Unlike the older path, this must not be combined with any further sign flip.
        /// Editor/SPLATFileReader.cs:65 negates a quaternion as (-x, y, -z, w) "like a-frame
        /// gaussian-splatting" -- that is the quaternion half of this same Y flip, hand-rolled
        /// for a library that had no source-frame option. gsplat applies the Y flip to
        /// positions, quaternions and SH coefficients together, so requesting RDF here replaces
        /// that flip rather than adding to it.
        /// </summary>
        public const SourceCoordinates SourceFrameForWebParity = SourceCoordinates.RDF;

        GsplatRenderer gsplatRenderer;
        GsplatCutout gsplatCutout;
        GsplatAsset gsplatAsset;
        string loadedSrc;
        string loadingSrc;
        string failedSrc;
        string cutoutSeekId;
        bool warnedColorSpace;
        bool warnedSettings;

        protected override void ApplyRender()
        {
            // gsplat keeps its shader, compute-shader and material references as serialized
            // fields on a GsplatSettings asset under a Resources folder, which is what keeps
            // them out of the build stripper's way -- there is no Shader.Find to fail. The
            // asset itself lives in the consuming project and is created by the Editor the
            // first time it is needed, so a project that has never touched gsplat in the
            // Editor can ship a player without it.
            if (GsplatSettings.Instance == null)
            {
                if (!warnedSettings)
                {
                    warnedSettings = true;
                    Debug.LogWarning($"GaussianSplatting object '{name}' cannot render: no GsplatSettings asset was found in Resources. Open Edit > Project Settings > Gsplat once in the Editor to create Assets/Gsplat/Settings/Resources/GsplatSettings.asset, then rebuild.");
                }
                return;
            }

            // assign splat renderer, on a child so the ARENA entity transform composes on top
            gsplatRenderer = GetComponentInChildren<GsplatRenderer>();
            if (gsplatRenderer == null)
            {
                GameObject sobj = new GameObject("Splat");
                sobj.transform.SetParent(transform, false);
                gsplatRenderer = sobj.AddComponent<GsplatRenderer>();
            }
            gsplatRenderer.GammaToLinear = gammaToLinear;

            WarnIfColorSpaceUnsupported();

            // assign splat cutout. Start one seek per distinct cutout id: the seek waits on
            // another ARENA object arriving, and ApplyRender runs again for every object
            // update, so gating on gsplatCutout (only set when the coroutine finishes) would
            // pile up coroutines for as long as the cutout entity is missing.
            if (!string.IsNullOrEmpty(json.CutoutEntity))
            {
                string cutout_id = json.CutoutEntity.TrimStart('#');
                if (cutout_id != cutoutSeekId)
                {
                    cutoutSeekId = cutout_id;
                    StartCoroutine(SeekCutout(cutout_id));
                }
            }

            // Loading parses the whole file on the main thread, so only do it when src changes;
            // ApplyRender also runs for position and rotation updates.
            if (json.Src == loadedSrc || json.Src == loadingSrc || json.Src == failedSrc) return;
            LoadSplatAtRuntime(json.Src);
        }

        private IEnumerator SeekCutout(string cutout_id)
        {
            yield return new WaitUntil(() => GameObject.Find(cutout_id) != null);
            var cobj = GameObject.Find(cutout_id);
            var aobj = cobj.GetComponent<ArenaObject>();
            if (aobj == null)
            {
                // not an ARENA object, so there is no object_type to pick a cutout shape from
                Debug.LogWarning($"GaussianSplatting object '{name}' cutoutEntity '{cutout_id}' is not an ARENA object; no cutout applied.");
                yield break;
            }
            gsplatCutout = cobj.GetComponentInChildren<GsplatCutout>();
            if (gsplatCutout == null)
            {
                GameObject sobj = new GameObject("Splat Cutout");
                sobj.transform.SetParent(cobj.transform, false);
                gsplatCutout = sobj.AddComponent<GsplatCutout>();
                // half extents, to match the ARENA a-frame gaussian components
                sobj.transform.localScale = Vector3.one * 0.5f;
            }
            gsplatCutout.m_Type = (aobj.object_type == "box" || aobj.object_type == "roundedbox") ? GsplatCutout.Type.Box : GsplatCutout.Type.Ellipsoid;
            gsplatCutout.m_Invert = false; // aframe-gaussian-splatting does not support inverted cutouts yet
            // the cutout is parented to the ARENA cutout entity, not to the renderer, so it has
            // to name the renderer it applies to explicitly (upstream spells the field
            // "Specifc")
            gsplatCutout.m_Target = GsplatCutout.Target.Specific;
            gsplatCutout.m_SpecifcRenderer = gsplatRenderer;
            yield return null;
        }

        /// <summary>
        /// Loads a splat from the local copy ArenaClientScene already downloaded. Runs in a
        /// player build: no AssetDatabase, no importer, no ScriptedImporter.
        /// </summary>
        private void LoadSplatAtRuntime(string msgUrl)
        {
            if (string.IsNullOrWhiteSpace(msgUrl)) return;
            if (ArenaClientScene.Instance == null) return;

            string assetPath = ArenaClientScene.Instance.checkLocalAsset(msgUrl);
            if (assetPath == null)
            {
                // not downloaded yet; re-apply once ArenaClientScene has the bytes on disk
                ArenaClientScene.Instance.RegisterAssetCallback(msgUrl, () => { apply = true; });
                return;
            }

            loadingSrc = msgUrl;
            string filetype = Path.GetExtension(assetPath).ToLowerInvariant();
            GsplatAsset loaded = null;
            try
            {
                switch (filetype)
                {
                    case ".ply":
                        loaded = compression == CompressionMode.Uncompressed
                            ? ScriptableObject.CreateInstance<GsplatAssetUncompressed>()
                            : (GsplatAsset)ScriptableObject.CreateInstance<GsplatAssetSpark>();
                        loaded.LoadFromPlyBytes(File.ReadAllBytes(assetPath), null, sourceCoordinates);
                        break;
                    case ".spz":
                        // SPZ decode reads the file itself and has no byte-array entry point.
                        if (compression == CompressionMode.Uncompressed)
                        {
                            var spzUncompressed = ScriptableObject.CreateInstance<GsplatAssetSpzUncompressed>();
                            spzUncompressed.LoadFromSpz(assetPath, sourceCoordinates);
                            loaded = spzUncompressed;
                        }
                        else
                        {
                            var spz = ScriptableObject.CreateInstance<GsplatAssetSpz>();
                            spz.LoadFromSpz(assetPath, sourceCoordinates);
                            loaded = spz;
                        }
                        break;
                    case ".splat":
                        // gsplat reads no .splat, so convert to an equivalent 3DGS PLY in memory
                        // and let its PLY reader apply sourceCoordinates as for any other PLY.
                        loaded = compression == CompressionMode.Uncompressed
                            ? ScriptableObject.CreateInstance<GsplatAssetUncompressed>()
                            : (GsplatAsset)ScriptableObject.CreateInstance<GsplatAssetSpark>();
                        loaded.LoadFromPlyBytes(ArenaSplatFile.ToPlyBytes(File.ReadAllBytes(assetPath)), null, sourceCoordinates);
                        break;
                    default:
                        Debug.LogWarning($"GaussianSplatting object '{name}' src '{msgUrl}' has unsupported format '{filetype}'. Supported: .ply, .spz, .splat. The ARENA web client also accepts .ksplat and .sog, which this client cannot read.");
                        loadingSrc = null;
                        failedSrc = msgUrl;
                        return;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"GaussianSplatting object '{name}' failed to load '{assetPath}': {e.GetType().Name}: {e.Message}");
                if (loaded != null) DestroyGsplatAsset(loaded);
                loadingSrc = null;
                failedSrc = msgUrl;
                return;
            }

            // a newer src may have arrived while this one was parsing
            if (msgUrl != json.Src)
            {
                DestroyGsplatAsset(loaded);
                loadingSrc = null;
                return;
            }

            var previous = gsplatAsset;
            gsplatAsset = loaded;
            gsplatRenderer.GsplatAsset = loaded;
            loadedSrc = msgUrl;
            loadingSrc = null;
            // the renderer rebinds on its next Update and never dereferences the old asset, so
            // the previous copy can go now instead of leaking its CPU-side arrays
            if (previous != null) DestroyGsplatAsset(previous);
        }

        /// <summary>
        /// Warns when the project color space cannot render gsplat correctly. Follows the
        /// reporting pattern of ArenaSceneRendererSettings.ApplyRender: a Unity console warning
        /// only, no project setting is changed and the load is not failed.
        /// Fires whether or not gammaToLinear is set, because that compensation converts before
        /// the alpha blend and its own author describes the result as incorrect.
        /// </summary>
        private void WarnIfColorSpaceUnsupported()
        {
            if (warnedColorSpace) return;
            if (QualitySettings.activeColorSpace == ColorSpace.Gamma) return;
            warnedColorSpace = true;

            if (IsHdrpActive())
            {
                Debug.LogWarning($"[ArenaWireGaussianSplatting] '{name}': gaussian splats are trained in Gamma space and blend in Gamma space, but this project is '{QualitySettings.activeColorSpace}' and uses HDRP, which does not support Gamma color space at all. Expect over-bright, over-saturated splat edges wherever splats of different colors overlap. There is no project setting that fixes this combination: render splats in a Built-in or URP project set to Gamma, or re-train the splat from linear-space images.");
            }
            else
            {
                Debug.LogWarning($"[ArenaWireGaussianSplatting] '{name}': gaussian splats are trained in Gamma space and blend in Gamma space, but this project is baked with '{QualitySettings.activeColorSpace}'. Expect over-bright, over-saturated splat edges wherever splats of different colors overlap. Set Edit > Project Settings > Player > Other Settings > Rendering > Color Space to 'Gamma', or re-train the splat from linear-space images.");
            }
        }

        private static bool IsHdrpActive()
        {
            // same runtime test ArenaUnity.GetLitShader uses to pick HDRP/Lit
            var pipeline = ArenaUnity.DefaultRenderPipeline;
            return pipeline != null && pipeline.GetType().ToString().Contains("HDRenderPipelineAsset");
        }

        private static void DestroyGsplatAsset(GsplatAsset asset)
        {
            // ArenaComponent runs in edit mode, where Destroy is not allowed
            if (Application.isPlaying) Destroy(asset);
            else DestroyImmediate(asset);
        }

        void OnDestroy()
        {
            // a GsplatAsset built with CreateInstance is not owned by the GameObject, so it
            // would outlive this component and hold its CPU-side arrays until a domain reload
            if (gsplatAsset != null)
            {
                DestroyGsplatAsset(gsplatAsset);
                gsplatAsset = null;
            }
        }

#elif LIB_GAUSSIAN_SPLATTING
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
            if (ArenaClientScene.Instance == null) yield break;
            string assetPath = ArenaClientScene.Instance.checkLocalAsset(msgUrl);
            if (assetPath == null)
            {
                ArenaClientScene.Instance.RegisterAssetCallback(msgUrl, () => { apply = true; });
                yield break;
            }
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
            // placeholder before LIB_GSPLAT / LIB_GAUSSIAN_SPLATTING load
        }
#endif

        public override void UpdateObject()
        {
            PublishIfChanged(JsonConvert.SerializeObject(json));
        }
    }
}
