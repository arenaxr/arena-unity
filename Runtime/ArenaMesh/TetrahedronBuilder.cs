/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2023, Carnegie Mellon University. All rights reserved.
 */

// Modified from: https://github.com/mrdoob/three.js/blob/dev/src/geometries/TetrahedronGeometry.js

using System.Collections.Generic;
using MeshBuilder;
using UnityEngine;

namespace ArenaUnity
{
    internal class TetrahedronBuilder
    {
        internal static Mesh Build(float radius, int details)
        {
            List<Vector3> vertices = new List<Vector3>{
               new Vector3(1, 1, 1), new Vector3( -1, -1, 1), new Vector3( -1, 1, -1), new Vector3( 1, -1, -1)
            };

            List<int> indices = new List<int>{
                2, 1, 0, 0, 3, 2, 1, 3, 0, 2, 3, 1
            };

            return PolyhedronBuilder.Build(vertices, indices, radius/2, details);
        }
    }
}
