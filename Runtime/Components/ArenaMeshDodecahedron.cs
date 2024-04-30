/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2023, Carnegie Mellon University. All rights reserved.
 */

using ArenaUnity.Schemas;
using Newtonsoft.Json;

namespace ArenaUnity
{
    public class ArenaMeshDodecahedron : ArenaMesh
    {
        // ARENA dodecahedron component unity conversion status:
        // DONE: detail
        // DONE: radius

        public ArenaDodecahedronJson json = new ArenaDodecahedronJson();

        protected override void ApplyRender()
        {
            filter.sharedMesh = DodecahedronBuilder.Build(
                json.Radius,
                json.Detail
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
