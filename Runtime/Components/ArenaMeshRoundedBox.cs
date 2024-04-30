/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2023, Carnegie Mellon University. All rights reserved.
 */

// Modified from: https://github.com/mattatz/unity-mesh-builder/tree/master/Assets/Packages/MeshBuilder/Scripts/Demo

using ArenaUnity.Schemas;
using Newtonsoft.Json;

namespace ArenaUnity
{
    public class ArenaMeshRoundedbox : ArenaMesh
    {
        // ARENA roundedbox component unity conversion status:
        // DONE: depth
        // DONE: height
        // DONE: width
        // TODO: radius
        // TODO: radiusSegments

        public ArenaRoundedboxJson json = new ArenaRoundedboxJson();

        protected override void ApplyRender()
        {
            filter.sharedMesh = CubeBuilder.Build(
                json.Width,
                json.Height,
                json.Depth
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
