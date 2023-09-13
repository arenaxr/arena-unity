/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2023, Carnegie Mellon University. All rights reserved.
 */

using ArenaUnity.Schemas;
using UnityEngine;

namespace ArenaUnity
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class ArenaMeshCapsule : ArenaMesh
    {
        public ArenaCapsuleJson json;

        protected override void Build(MeshFilter filter)
        {
            filter.sharedMesh = CapsuleBuilder.CapsuleData(
                json.Radius,
                json.Length,
                json.SegmentsRadial,
                json.SegmentsCap
            );
        }
    }
}
