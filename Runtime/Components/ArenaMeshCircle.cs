/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2023, Carnegie Mellon University. All rights reserved.
 */

// Modified from: https://github.com/mattatz/unity-mesh-builder/tree/master/Assets/Packages/MeshBuilder/Scripts/Demo

using ArenaUnity.Schemas;
using MeshBuilder;
using Newtonsoft.Json;

namespace ArenaUnity
{
    public class ArenaMeshCircle : ArenaMesh
    {
        public ArenaCircleJson json = new ArenaCircleJson();

        protected override void ApplyRender()
        {
            filter.sharedMesh = RingBuilder.Build(
                0f,
                json.Radius,
                json.Segments,
                1,
                json.ThetaStart,
                json.ThetaLength
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
