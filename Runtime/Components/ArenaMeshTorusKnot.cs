/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2023, Carnegie Mellon University. All rights reserved.
 */

using ArenaUnity.Schemas;
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
    }
}
