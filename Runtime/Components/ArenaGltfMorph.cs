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
    public class ArenaGltfMorph : ArenaComponent
    {
        // ARENA gltf-morph component unity conversion status:
        // DONE: morphtarget
        // DONE: value

        public ArenaGltfMorphJson json = new ArenaGltfMorphJson();

        private bool morphApplied = false;

        protected override void ApplyRender()
        {
            morphApplied = false;
        }

        protected override void Update()
        {
            base.Update();

            if (!morphApplied)
            {
                var gltfModel = GetComponent<ArenaWireGltfModel>();
                if (gltfModel == null || gltfModel.isLoaded)
                {
                    if (!string.IsNullOrEmpty(json.Morphtarget))
                    {
                        foreach (var smr in GetComponentsInChildren<SkinnedMeshRenderer>())
                        {
                            if (smr.sharedMesh != null)
                            {
                                int index = smr.sharedMesh.GetBlendShapeIndex(json.Morphtarget);
                                if (index >= 0)
                                {
                                    smr.SetBlendShapeWeight(index, json.Value * 100f);
                                }
                            }
                        }
                    }
                    morphApplied = true;
                }
            }
        }

        public override void UpdateObject()
        {
            PublishIfChanged(json.attributeName, JsonConvert.SerializeObject(json));
        }
    }
}
