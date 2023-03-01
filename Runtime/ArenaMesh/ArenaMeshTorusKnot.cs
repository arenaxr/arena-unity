/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2023, Carnegie Mellon University. All rights reserved.
 */

using UnityEngine;

namespace ArenaUnity
{
    public class ArenaMeshTorusKnot : ArenaMesh
    {
        [SerializeField, Range(0.1f, 10f)] internal float radius = 0.5f;
        [SerializeField, Range(0.05f, 10f)] internal float radiusTubular = 0.1f;
        [SerializeField, Range(2, 64)] internal int radialSegments = 16;
        [SerializeField, Range(3, 64)] internal int thetaSegments = 8;
        [SerializeField, Range(2, 5)] internal float p = 2;
        [SerializeField, Range(2, 5)] internal float q = 3;

        protected override void Build(MeshFilter filter)
        {
            float thickness = radiusTubular * 2;
            // TODO: filter.sharedMesh = TorusKnotBuilder.Build(radius, thickness, radialSegments, thetaSegments, p, q);
        }
    }
}
