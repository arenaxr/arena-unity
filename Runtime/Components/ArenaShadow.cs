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
    public class ArenaShadow : ArenaComponent
    {
        // ARENA shadow component unity conversion status:
        // DONE: cast
        // DONE: receive

        public ArenaShadowJson json = new ArenaShadowJson();

        protected override void ApplyRender()
        {
            foreach (MeshRenderer mr in GetComponentsInChildren<MeshRenderer>(true))
            {
                mr.shadowCastingMode = json.Cast ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.Off;
                mr.receiveShadows = json.Receive;
            }
            foreach (SkinnedMeshRenderer smr in GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                smr.shadowCastingMode = json.Cast ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.Off;
                smr.receiveShadows = json.Receive;
            }
        }

        public override void UpdateObject()
        {
            PublishIfChanged(json.attributeName, JsonConvert.SerializeObject(json));
        }
    }
}
