/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2023, Carnegie Mellon University. All rights reserved.
 */

// Modified from: https://github.com/mattatz/unity-mesh-builder/tree/master/Assets/Packages/MeshBuilder/Scripts/Demo

using ArenaUnity.Schemas;
using MeshBuilder;
using Newtonsoft.Json;
using UnityEngine;

namespace ArenaUnity
{
    public class ArenaMeshTorus : ArenaMesh
    {
        public ArenaTorusJson json = new ArenaTorusJson();

        protected override void ApplyRender()
        {
            filter.sharedMesh = TorusBuilder.Build(
                json.Radius,
                json.RadiusTubular * 2,
                json.SegmentsRadial,
                json.SegmentsTubular,
                0f,
                Mathf.PI / 180 * json.Arc
            );
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
