/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2023, Carnegie Mellon University. All rights reserved.
 */

// Modified from: https://github.com/mattatz/unity-mesh-builder/tree/master/Assets/Packages/MeshBuilder/Scripts/Demo

using ArenaUnity.Schemas;
using Newtonsoft.Json;
using UnityEngine;

namespace ArenaUnity
{
    public class ArenaMeshTorus : ArenaMesh
    {
        // ARENA torus component unity conversion status:
        // DONE: arc
        // DONE: radius
        // DONE: radiusTubular
        // DONE: segmentsRadial
        // DONE: segmentsTubular

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
            PublishIfChanged(JsonConvert.SerializeObject(json));
        }
    }
}
