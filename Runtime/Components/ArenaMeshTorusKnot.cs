/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2023, Carnegie Mellon University. All rights reserved.
 */

using ArenaUnity.Schemas;
using Newtonsoft.Json;

namespace ArenaUnity
{
    public class ArenaMeshTorusKnot : ArenaMesh
    {
        // ARENA torusKnot component unity conversion status:
        // TODO: object_type
        // TODO: p
        // TODO: q
        // TODO: radius
        // TODO: radiusTubular
        // TODO: segmentsRadial
        // TODO: segmentsTubular

        public ArenaTorusKnotJson json = new ArenaTorusKnotJson();

        protected override void ApplyRender()
        {
            filter.sharedMesh = TorusKnotBuilder.Build(
                json.Radius,
                json.RadiusTubular * 2,
                json.SegmentsTubular,
                json.SegmentsRadial,
                json.P,
                json.Q
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
