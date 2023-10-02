/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2023, Carnegie Mellon University. All rights reserved.
 */

// Modified from: https://github.com/mrdoob/three.js/blob/dev/src/geometries/TorusKnotGeometry.js

using System.Collections.Generic;
using MeshBuilder;
using UnityEngine;

namespace ArenaUnity
{
    internal class TorusKnotBuilder : MeshBuilderBase
    {
        internal static Mesh Build(float radius = 1, float tube = 0.4f, int tubularSegments = 64, int radialSegments = 8, float p = 2, float q = 3)
        {
            // buffers
            var indices = new List<int>();
            var vertices = new List<Vector3>();
            var normals = new List<Vector3>();
            var uvs = new List<Vector2>();

            // generate vertices, normals and uvs
            for (var i = 0; i <= tubularSegments; ++i)
            {
                // the radian "u" is used to calculate the position on the torus curve of the current tubular segment
                var u = 1f * i / tubularSegments * p * Mathf.PI * 2;

                // now we calculate two points. P1 is our current position on the curve, P2 is a little farther ahead.
                // these points are used to create a special "coordinate space", which is necessary to calculate the correct vertex positions
                calculatePositionOnCurve(u, p, q, radius, out Vector3 P1);
                calculatePositionOnCurve(u + 0.01f, p, q, radius, out Vector3 P2);

                // calculate orthonormal basis
                var T = (P2 - P1);
                var N = (P2 + P1);
                var B = Vector3.Cross(T, N);
                N = Vector3.Cross(B, T);

                // normalize B, N. T can be ignored, we don't use it
                B.Normalize();
                N.Normalize();

                for (var j = 0; j <= radialSegments; ++j)
                {
                    // now calculate the vertices. they are nothing more than an extrusion of the torus curve.
                    // because we extrude a shape in the xy-plane, there is no need to calculate a z-value.
                    var v = 1f * j / radialSegments * Mathf.PI * 2;
                    var cx = -tube * Mathf.Cos(v);
                    var cy = tube * Mathf.Sin(v);

                    // now calculate the final vertex position.
                    // first we orient the extrusion with our basis vectors, then we add it to the current position on the curve
                    var vertex = new Vector3();
                    vertex.x = P1.x + (cx * N.x + cy * B.x);
                    vertex.y = P1.y + (cx * N.y + cy * B.y);
                    vertex.z = P1.z + (cx * N.z + cy * B.z);

                    vertices.Add(vertex);

                    // normal (P1 is always the center/origin of the extrusion, thus we can use it to calculate the normal)
                    normals.Add((vertex - P1).normalized);

                    // uv
                    uvs.Add(new Vector2(1f * i / tubularSegments, 1f * j / radialSegments));
                }
            }

            // generate indices
            for (var j = 1; j <= tubularSegments; j++)
            {
                for (var i = 1; i <= radialSegments; i++)
                {
                    // indices
                    var a = (radialSegments + 1) * (j - 1) + (i - 1);
                    var b = (radialSegments + 1) * j + (i - 1);
                    var c = (radialSegments + 1) * j + i;
                    var d = (radialSegments + 1) * (j - 1) + i;

                    // faces
                    indices.Add(a); indices.Add(b); indices.Add(d);
                    indices.Add(b); indices.Add(c); indices.Add(d);
                }
            }

            // build geometry
            Mesh mesh = new Mesh();
            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.SetUVs(0, uvs);
            mesh.SetIndices(indices, MeshTopology.Triangles, 0);
            mesh.RecalculateBounds();
            return mesh;

            // this function calculates the current position on the torus curve
            void calculatePositionOnCurve(float u, float p, float q, float radius, out Vector3 position)
            {
                var cu = Mathf.Cos(u);
                var su = Mathf.Sin(u);
                var quOverP = q / p * u;
                var cs = Mathf.Cos(quOverP);

                position.x = radius * (2 + cs) * 0.5f * cu;
                position.y = radius * (2 + cs) * su * 0.5f;
                position.z = radius * Mathf.Sin(quOverP) * 0.5f;
            }
        }
    }
}