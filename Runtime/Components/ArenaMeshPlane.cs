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
    public class ArenaMeshPlane : ArenaMesh
    {
        // ARENA plane component unity conversion status:
        // TODO: object_type
        // TODO: height
        // TODO: segmentsHeight
        // TODO: segmentsWidth
        // TODO: width

        public ArenaPlaneJson json = new ArenaPlaneJson();

        protected override void ApplyRender()
        {
            filter.sharedMesh = PlaneBuilder.Build(
               json.Width,
               json.Height,
               json.SegmentsWidth,
               json.SegmentsHeight
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
