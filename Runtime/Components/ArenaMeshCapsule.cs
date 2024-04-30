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
        // TODO: object_type
        // TODO: length
        // TODO: radius
        // TODO: segmentsCap
        // TODO: segmentsRadial

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
