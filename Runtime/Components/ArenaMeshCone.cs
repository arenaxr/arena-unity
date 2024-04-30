﻿/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2023, Carnegie Mellon University. All rights reserved.
 */

// Modified from: https://github.com/mattatz/unity-mesh-builder/tree/master/Assets/Packages/MeshBuilder/Scripts/Demo

using ArenaUnity.Schemas;
using Newtonsoft.Json;
using UnityEngine;

namespace ArenaUnity
{
    public class ArenaMeshCone : ArenaMesh
    {
        // ARENA cone component unity conversion status:
        // TODO: object_type
        // TODO: height
        // TODO: openEnded
        // TODO: radiusBottom
        // TODO: radiusTop
        // TODO: segmentsHeight
        // TODO: segmentsRadial
        // TODO: thetaLength
        // TODO: thetaStart

        public ArenaConeJson json = new ArenaConeJson();

        protected override void ApplyRender()
        {
            filter.sharedMesh = CylinderBuilder.Build(
                json.RadiusTop,
                json.RadiusBottom,
                json.Height,
                json.SegmentsRadial,
                json.SegmentsHeight,
                json.OpenEnded,
                Mathf.PI / 180 * json.ThetaStart,
                Mathf.PI / 180 * json.ThetaLength
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
