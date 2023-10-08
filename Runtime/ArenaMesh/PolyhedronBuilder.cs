/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2023, Carnegie Mellon University. All rights reserved.
 */

// Modified from: https://github.com/mattatz/unity-mesh-builder/blob/master/Assets/Packages/MeshBuilder/Scripts/Builders/PolyhedronBuilder.cs

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ArenaUnity
{
    public class PolyhedronBuilder
    {
        public static Mesh Build(List<Vector3> vertices, List<int> indices, float radius, int details)
        {
            var midCache = new Dictionary<int, int>();
            Func<int, int, int> MidPoint = (int a, int b) =>
            {
                var key = CalculateCantorPair(a, b);
                if (midCache.ContainsKey(key))
                    return midCache[key];

                var mid = (vertices[a] + vertices[b]) / 2;
                vertices.Add(mid.normalized);

                var idx = vertices.Count - 1;
                midCache.Add(key, idx);

                return idx;
            };

            // treatise on shared vs unique vertices, helped mwfarb generate flat normals:
            // https://blog.nobel-joergensen.com/2010/12/25/procedural-generated-mesh-in-unity

            var mesh = new Mesh();
            if (details == 0)
            {
                // flat normals
                List<Vector3> vertexBuffer = new List<Vector3>();
                List<int> indexBuffer = new List<int>();
                for (var i = 0; i < indices.Count; i++)
                {
                    vertexBuffer.Add(vertices[indices[i]]);
                    indexBuffer.Add(i);
                }
                mesh.SetVertices(vertexBuffer.Select(v => v * radius).ToList());
                mesh.SetIndices(indexBuffer, MeshTopology.Triangles, 0);
            }
            else
            {
                // smooth normals
                for (int i = 0; i < details; i++)
                {
                    int n = indices.Count;
                    for (int k = 0; k < n; k += 3)
                    {
                        var i0 = indices[k + 0];
                        var i1 = indices[k + 1];
                        var i2 = indices[k + 2];
                        var a = MidPoint(i0, i1);
                        var b = MidPoint(i1, i2);
                        var c = MidPoint(i2, i0);
                        indices.Add(i0); indices.Add(a); indices.Add(c);
                        indices.Add(a); indices.Add(i1); indices.Add(b);
                        indices.Add(c); indices.Add(b); indices.Add(i2);
                        indices.Add(a); indices.Add(b); indices.Add(c);
                    }
                }
                mesh.SetVertices(vertices.Select(v => v * radius).ToList());
                mesh.SetIndices(indices, MeshTopology.Triangles, 0);
            }
            ArenaUnity.ToUnityMesh(ref mesh);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            return mesh;
        }

        // https://medium.com/@PraveenMathew92/cantor-pairing-function-e213a8a89c2b
        protected static int CalculateCantorPair(int k1, int k2)
        {
            int sum = k1 + k2;
            return Mathf.FloorToInt(sum * (sum + 1) / 2) + Mathf.Min(k1, k2);
        }
    }
}
