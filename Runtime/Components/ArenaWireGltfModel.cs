/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2023, Carnegie Mellon University. All rights reserved.
 */

using System;
using System.Collections.Generic;
using System.IO;
using ArenaUnity.Components;
using ArenaUnity.Schemas;
using GLTFast;
using GLTFast.Logging;
using Newtonsoft.Json;
using UnityEngine;

namespace ArenaUnity
{
    public class ArenaWireGltfModel : ArenaComponent
    {
        // ARENA gltf-model component unity conversion status:
        // DONE: url

        // TODO: Handle KHR_draco_mesh_compression — GLBs requiring Draco decode render pink
        //       if com.unity.cloud.draco package is not installed. Consider adding a warning
        //       or graceful fallback when Draco is required but unavailable.
        // TODO: Handle GLBs with empty materials array — assign a sensible default material
        //       (e.g. gray PBR) instead of relying on glTFast's fallback which may produce pink.
        // TODO: GLTF models with negative parent scale (e.g. scale z=-1) face backwards.
        //       The -180° Y correction (GltfToUnityRotationQuat) interacts incorrectly with
        //       negative parent scale. Needs investigation into glTFast's internal coordinate
        //       conversion (X-axis inversion) to find a fix that doesn't break ARENA-level
        //       child objects inheriting the parent's scale (e.g. PlaneProp parented to Plane).

        public ArenaGltfModelJson json = new ArenaGltfModelJson();

        protected override void ApplyRender()
        {
            var aobj = GetComponent<ArenaObject>();
            var url = json.Url;
            if (url != null && aobj != null && aobj.gltfUrl == null)
            {
                if (ArenaClientScene.Instance != null)
                {
                    string assetPath = ArenaClientScene.Instance.checkLocalAsset(url);
                    if (assetPath != null)
                    {
                        aobj.gltfUrl = url;
                        AttachGltf(assetPath, gameObject, aobj);
                    }
                }
            }
        }

        public override void UpdateObject()
        {
            PublishIfChanged(JsonConvert.SerializeObject(json));
        }

        internal static async void AttachGltf(string assetPath, GameObject gobj, ArenaObject aobj = null)
        {
            if (assetPath == null) return;
            AnimationClip[] clips = null;
            GameObject mobj = null;
            var imSet = new ImportSettings
            {
                AnimationMethod = AnimationMethod.Legacy
            };
            var gltf = new GltfImport();
            Uri uri = new Uri(Path.GetFullPath(assetPath));
            if (await gltf.LoadFile(assetPath, uri, imSet))
            {
                clips = gltf.GetAnimationClips();
                if (clips != null && aobj != null)
                {   // save animation names for easy animation-mixer reference at runtime
                    aobj.animations = new List<string>();
                    foreach (AnimationClip clip in clips)
                    {
                        aobj.animations.Add(clip.name);
                    }
                }
                var inSet = new InstantiationSettings
                {
                    SceneObjectCreation = SceneObjectCreation.Always
                };
                var instantiator = new GameObjectInstantiator(gltf, gobj.transform, logger: new ConsoleLogger(), inSet);
                if (await gltf.InstantiateSceneAsync(instantiator))
                {
                    mobj = gobj.transform.GetChild(0).gameObject; // TODO (mwfarb): find better child method

                    // TODO (mwfarb): find a better way to chain commponent dependancies than this
                    var am = gobj.GetComponent<ArenaAnimationMixer>();
                    if (am != null)
                    {
                        am.apply = true;
                    }
                }
            }
            else
            {
                Debug.LogWarning($"Unable to load GTLF at {assetPath}.");
            }
            if (mobj != null)
            {
                mobj.transform.localRotation = ArenaUnity.GltfToUnityRotationQuat(mobj.transform.localRotation);

                foreach (Transform child in mobj.transform.GetComponentsInChildren<Transform>())
                {   // prevent inadvertent editing of gltf elements
                    child.gameObject.isStatic = true;
                }
            }
        }
    }
}
