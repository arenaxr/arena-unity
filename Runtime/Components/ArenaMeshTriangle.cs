/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2023, Carnegie Mellon University. All rights reserved.
 */

using ArenaUnity.Schemas;
using Newtonsoft.Json;
using UnityEngine;

namespace ArenaUnity
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]

    public class ArenaMeshTriangle : ArenaMesh
    {
        // ARENA triangle component unity conversion status:
        // TODO: object_type
        // TODO: vertexA
        // TODO: vertexB
        // TODO: vertexC

        public ArenaTriangleJson json = new ArenaTriangleJson();

        protected override void ApplyRender()
        {
            filter.sharedMesh = TriangleBuilder.Build(
                 ArenaUnity.ToUnityPosition(json.VertexA),
                 ArenaUnity.ToUnityPosition(json.VertexB),
                 ArenaUnity.ToUnityPosition(json.VertexC)
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
