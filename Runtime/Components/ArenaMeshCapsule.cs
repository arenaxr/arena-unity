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
        public ArenaCapsuleJson json = new ArenaCapsuleJson();

        protected override void ApplyRender()
        {
            filter.sharedMesh = CapsuleBuilder.CapsuleData(
                json.Radius,
                json.Length,
                json.SegmentsRadial,
                json.SegmentsCap
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
