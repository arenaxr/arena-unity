/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2023, Carnegie Mellon University. All rights reserved.
 */

using ArenaUnity.Schemas;
using Newtonsoft.Json;
using UnityEngine;

namespace ArenaUnity
{
    public class ArenaMeshTorusKnot : ArenaMesh
    {
        public ArenaTorusKnotJson json = new ArenaTorusKnotJson();

        protected override void Build(MeshFilter filter)
        {
            // TODO (mwfarb): filter.sharedMesh = TorusKnotBuilder.Build(json.radius, json.thickness, json.radialSegments, json.thetaSegments, json.p, json.q);
            Debug.LogWarning("TorusKnot rendering not yet supported in ARENA Unity!!!!");
        }

        public override void UpdateObject()
        {
            var newJson = JsonConvert.SerializeObject(json);
            if (updatedJson != newJson)
            {
                var aobj = GetComponent<ArenaObject>();
                if (aobj != null)
                {
                    aobj.PublishUpdate($"{{\"{json.componentName}\":{newJson}}}");
                    apply = true;
                }
            }
            updatedJson = newJson;
        }
    }
}
