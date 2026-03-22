/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2023, Carnegie Mellon University. All rights reserved.
 */

using ArenaUnity.Schemas;
using Newtonsoft.Json;

namespace ArenaUnity
{
    public class ArenaMeshCapsule : ArenaMesh
    {
        // ARENA capsule component unity conversion status:
        // DONE: length
        // DONE: radius
        // DONE: segmentsCap
        // DONE: segmentsRadial

        public ArenaCapsuleJson json = new ArenaCapsuleJson();

        protected override void ApplyRender()
        {
            filter.sharedMesh = CapsuleBuilder.CapsuleData(
                json.Radius,
                json.Length,
                json.SegmentsRadial,
                json.SegmentsCap * 2 // CapsuleData computes total latitudes for both hemispheres
            );
        }

        public override void UpdateObject()
        {
            PublishIfChanged(JsonConvert.SerializeObject(json));
        }
    }
}
