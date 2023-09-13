/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2023, Carnegie Mellon University. All rights reserved.
 */

using ArenaUnity;
using ArenaUnity.Schemas;
using Newtonsoft.Json;
using UnityEngine;

namespace ArenaUnity
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]

    public class ArenaMeshTriangle : ArenaMesh
    {
        public ArenaTriangleJson json = new ArenaTriangleJson();

        protected override void Build(MeshFilter filter)
        {
            filter.sharedMesh = TriangleBuilder.Build(
                ArenaUnity.ToUnityPosition(JsonConvert.DeserializeObject<ArenaPositionJson>(json.VertexA.ToString())),
                ArenaUnity.ToUnityPosition(JsonConvert.DeserializeObject<ArenaPositionJson>(json.VertexB.ToString())),
                ArenaUnity.ToUnityPosition(JsonConvert.DeserializeObject<ArenaPositionJson>(json.VertexC.ToString()))
            );
        }
    }
}
