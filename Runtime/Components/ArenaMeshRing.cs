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
    public class ArenaMeshRing : ArenaMesh
    {
        // ARENA ring component unity conversion status:
        // TODO: object_type
        // TODO: radiusInner
        // TODO: radiusOuter
        // TODO: segmentsPhi
        // TODO: segmentsTheta
        // TODO: thetaLength
        // TODO: thetaStart

        public ArenaRingJson json = new ArenaRingJson();

        protected override void ApplyRender()
        {
            filter.sharedMesh = RingBuilder.Build(
                json.RadiusInner,
                json.RadiusOuter,
                json.SegmentsTheta,
                json.SegmentsPhi,
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
