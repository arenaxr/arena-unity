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
        /// gsplat's own defaults are RUF on LoadFromPlyBytes/LoadFromSpz and RUB on the Editor
        /// importer's field (RUB is also what Unspecified falls back to). Either would be wrong
        /// here: RUF applies no correction at all, and RUB flips Z instead of Y, which differs
        /// from RDF by exactly the 180-degree rotation about X under discussion and would leave
        /// splats upside down relative to the web client. RDF is passed explicitly on every
        /// load, so no library default is relied on.
        ///
        /// Unlike the older path, this must not be combined with any further sign flip.
        /// Editor/SPLATFileReader.cs:65 negates a quaternion as (-x, y, -z, w) "like a-frame
        /// gaussian-splatting" -- that is the quaternion half of this same Y flip, hand-rolled
        /// for a library that had no source-frame option. gsplat applies the Y flip to
        /// positions, quaternions and SH coefficients together, so requesting RDF here replaces
        /// that flip rather than adding to it.
        /// </summary>
        public const SourceCoordinates SourceFrameForWebParity = SourceCoordinates.RDF;

        // names of the child GameObjects this component owns; looked up by name on a direct
        // child rather than searched through descendants, see ApplyRender
        const string SplatChildName = "Splat";
        const string CutoutChildName = "Splat Cutout";

        GsplatRenderer gsplatRenderer;
        GsplatCutout gsplatCutout;
        GsplatAsset gsplatAsset;
        string loadedSrc;
        string loadingSrc;
        string failedSrc;
        string requestedSrc;
        string cutoutSeekId;
        // the values the currently loaded asset was parsed with; only read while parsing, so a
        // change to either has to re-read the same src, see OnValidate
        SourceCoordinates loadedCoordinates = SourceFrameForWebParity;
        CompressionMode loadedCompression = CompressionMode.Spark;
        bool warnedColorSpace;
        bool warnedSettings;
        bool warnedPipeline;

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

            // assign splat renderer, on a child so the ARENA entity transform composes on top.
            // Scoped to this component's own direct child: ARENA supports parent/child entities,
            // so a gaussian_splatting object can be a descendant of another one, and
            // GetComponentInChildren would happily adopt the descendant's renderer and then
            // write this object's asset, GammaToLinear and cutout target into it while this
            // object rendered nothing.
            Transform splat = transform.Find(SplatChildName);
            if (splat == null)
            {
                GameObject sobj = new GameObject(SplatChildName);
                sobj.transform.SetParent(transform, false);
                splat = sobj.transform;
            }
            gsplatRenderer = splat.GetComponent<GsplatRenderer>();
            if (gsplatRenderer == null) gsplatRenderer = splat.gameObject.AddComponent<GsplatRenderer>();
            gsplatRenderer.GammaToLinear = gammaToLinear;

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

            // A failed load is remembered only until src changes value, not for the lifetime of
            // the GameObject: a truncated download or a transient read error should not poison
            // the object, so re-publishing a src, or switching away and back, gets a fresh
            // attempt. Repeat messages carrying the same failed src still do not retry.
            if (json.Src != requestedSrc)
            {
                requestedSrc = json.Src;
                failedSrc = null;
            }

            // ARENA deletes an attribute by publishing null, so a cleared src has to remove the
            // splat rather than leave the last one on screen forever.
            if (string.IsNullOrWhiteSpace(json.Src))
            {
                UnloadSplat();
                return;
            }

            // Loading parses the whole file on the main thread, so only do it when src changes;
            // ApplyRender also runs for position and rotation updates.
            if (json.Src == loadedSrc || json.Src == loadingSrc || json.Src == failedSrc) return;
            LoadSplatAtRuntime(json.Src);
        }

        /// <summary>
        /// Local Inspector settings, unlike ARENA attributes, do not change the published json,
        /// so ArenaComponent.OnValidate -> UpdateObject -> PublishIfChanged sees no change and
        /// never sets the apply latch. Set the latch here rather than calling ApplyRender
        /// directly, so the edit is applied by ArenaComponent.Update like every other update --
        /// the same thing ArenaMesh.OnValidate does for its own Inspector fields -- and drop the
        /// src latches when a setting that is only read while parsing has moved, so the same src
        /// is re-read with the new value.
        /// </summary>
        protected override void OnValidate()
        {
            base.OnValidate();
            if (sourceCoordinates != loadedCoordinates || compression != loadedCompression)
            {
                loadedSrc = null;
                failedSrc = null;
            }
            // gammaToLinear needs no reload; ApplyRender re-pushes it to the renderer
            apply = true;
        }

        /// <summary>
        /// Removes the splat currently on screen, for a src that has been cleared. The renderer
        /// component keeps its GPU buffers until it is disabled or destroyed -- upstream only
        /// releases them when a different asset is bound -- but it stops drawing as soon as its
        /// asset is null, because GsplatRenderer.Valid is false without one.
        /// </summary>
        private void UnloadSplat()
        {
            loadedSrc = null;
            loadingSrc = null;
            if (gsplatRenderer != null) gsplatRenderer.GsplatAsset = null;
            if (gsplatAsset != null)
            {
                DestroyGsplatAsset(gsplatAsset);
                gsplatAsset = null;
            }
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
            // scoped to a direct child of the cutout entity, for the same reason as the renderer
            // in ApplyRender: a nested ARENA entity may carry its own cutout for another splat
            Transform cutout = cobj.transform.Find(CutoutChildName);
            if (cutout == null)
            {
                GameObject sobj = new GameObject(CutoutChildName);
                sobj.transform.SetParent(cobj.transform, false);
                // half extents, to match the ARENA a-frame gaussian components
                sobj.transform.localScale = Vector3.one * 0.5f;
                cutout = sobj.transform;
            }
            gsplatCutout = cutout.GetComponent<GsplatCutout>();
            if (gsplatCutout == null) gsplatCutout = cutout.gameObject.AddComponent<GsplatCutout>();
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

            // Warn about what will look wrong only now that there is something to look at. An
            // object with no src, an unsupported format, or a src that never downloaded used to
            // get the full colour-space warning about the rendering of a splat that was not
            // being rendered.
            WarnIfColorSpaceUnsupported();
            WarnIfPipelineHookRequired();

            var previous = gsplatAsset;
            gsplatAsset = loaded;
            gsplatRenderer.GsplatAsset = loaded;
            loadedSrc = msgUrl;
            loadingSrc = null;
            loadedCoordinates = sourceCoordinates;
            loadedCompression = compression;
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

        /// <summary>
        /// Warns once when the render pipeline in use needs a hook that no package can install
        /// on the consuming project's behalf. gsplat draws by injecting a sorting pass into each
        /// camera: the Built-in pipeline gets that from the library's own player-loop hook and
        /// needs no setup, but URP needs a ScriptableRendererFeature added to the Renderer asset
        /// and HDRP needs a CustomPass added to a Custom Pass Volume, both of which live in the
        /// project, not in the package. GsplatURPFeature is declared internal upstream, so it
        /// cannot be registered programmatically even with a reference to the assembly; naming
        /// exactly what to add and where is all that is left.
        ///
        /// Without this, the most likely ARENA configuration -- URP, no renderer feature -- is
        /// an object that loads with no error and draws nothing, which is the symptom this whole
        /// path exists to remove. Called after a successful load, so it fires only when a splat
        /// really should be on screen, and once per object like the other warnings here.
        /// </summary>
        private void WarnIfPipelineHookRequired()
        {
            if (warnedPipeline) return;
            var pipeline = ActiveRenderPipeline();
            if (pipeline == null) return; // Built-in installs its own hook, nothing to do
            string pipelineType = pipeline.GetType().ToString();
            if (pipelineType.Contains("HDRenderPipelineAsset"))
            {
                warnedPipeline = true;
                Debug.LogWarning($"[ArenaWireGaussianSplatting] '{name}': splat loaded, but HDRP only draws gaussian splats through a custom pass that this package cannot add for you. If the splat is invisible, add a Custom Pass Volume to the scene, add a 'Gsplat HDRP Pass' entry to it, and set the injection point to 'Before Transparent'.");
            }
            else if (pipelineType.Contains("UniversalRenderPipelineAsset"))
            {
                warnedPipeline = true;
                Debug.LogWarning($"[ArenaWireGaussianSplatting] '{name}': splat loaded, but URP only draws gaussian splats through a renderer feature that this package cannot add for you. If the splat is invisible, select the Universal Renderer Data asset your project uses (Assets/Settings/PC_Renderer in a default URP project), press 'Add Renderer Feature' and choose 'Gsplat URP Feature'. On Unity 6 and later, Render Graph 'Compatibility Mode' in the URP settings must also be off.");
            }
            // any other scriptable pipeline: there is no advice worth giving, so say nothing
        }

        private static bool IsHdrpActive()
        {
            var pipeline = ActiveRenderPipeline();
            return pipeline != null && pipeline.GetType().ToString().Contains("HDRenderPipelineAsset");
        }

        /// <summary>
        /// The render pipeline asset actually in use. ArenaUnity.DefaultRenderPipeline, the house
        /// idiom, is a static field captured once at static initialisation from
        /// GraphicsSettings.defaultRenderPipeline, so it reads null in a project that sets its
        /// pipeline only per quality level. Both callers here choose which of two user-facing
        /// messages to print -- and on HDRP one of them would be advice HDRP cannot follow -- so
        /// they read the live value instead. GraphicsSettings.currentRenderPipeline returns the
        /// quality-level override when there is one and the default otherwise.
        /// </summary>
        private static UnityEngine.Rendering.RenderPipelineAsset ActiveRenderPipeline()
        {
            return UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline;
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
