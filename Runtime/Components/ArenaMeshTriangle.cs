/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2023, Carnegie Mellon University. All rights reserved.
 */

using ArenaUnity;
using ArenaUnity.Schemas;
using UnityEngine;

namespace ArenaUnity
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]

    public class ArenaMeshTriangle : ArenaMesh
    {
        public ArenaTriangleJson json;

        protected override void Build(MeshFilter filter)
        {
            filter.sharedMesh = TriangleBuilder.Build(
                ArenaUnity.ToUnityPosition((ArenaPositionJson)json.VertexA),
                ArenaUnity.ToUnityPosition((ArenaPositionJson)json.VertexB),
                ArenaUnity.ToUnityPosition((ArenaPositionJson)json.VertexC)
            );
        }
    }
}
