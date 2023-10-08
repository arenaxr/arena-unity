/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2023, Carnegie Mellon University. All rights reserved.
 */

using ArenaUnity.Components;
using ArenaUnity.Schemas;
using Newtonsoft.Json;

namespace ArenaUnity
{
    public class ArenaMeshVideosphere : ArenaMesh
    {
        public ArenaVideosphereJson json = new ArenaVideosphereJson();

        protected override void ApplyRender()
        {
            filter.sharedMesh = SphereBuilder.Build(
                json.Radius,
                json.SegmentsHeight,
                json.SegmentsWidth
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
