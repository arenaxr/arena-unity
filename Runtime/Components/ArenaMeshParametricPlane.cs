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
    public class ArenaMeshParametricPlane : ArenaMesh
    {
        public ArenaPlaneJson json = new ArenaPlaneJson();

        protected override void ApplyRender()
        {
            // TODO (mwfarb): try to support noise plane, possibly with ocean wire object from a-frame extras:
            filter.sharedMesh = PlaneBuilder.Build(new ParametricPlanePerlin(
                Vector2.zero,
                new Vector2(2f, 2f), 0.5f),
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
