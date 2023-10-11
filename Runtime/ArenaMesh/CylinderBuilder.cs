/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2023, Carnegie Mellon University. All rights reserved.
 */

// Modified from: https://github.com/mrdoob/three.js/blob/dev/src/geometries/CylinderGeometry.js

using System.Collections.Generic;
using UnityEngine;

namespace ArenaUnity
{
    public class CylinderBuilder
    {
        public static Mesh Build(float radiusTop = 1, float radiusBottom = 1, float height = 1, int radialSegments = 32, int heightSegments = 1, bool openEnded = false, float thetaStart = 0, float thetaLength = Mathf.PI * 2)
        {
            // buffers
            var indices = new List<int>();
            var vertices = new List<Vector3>();
            var normals = new List<Vector3>();
            var uvs = new List<Vector2>();

            // helper variables
            int index = 0;
            var indexArray = new List<List<int>>();
            var halfHeight = height / 2;

            // generate geometry
            generateTorso();
            if (openEnded == false)
            {
                if (radiusTop > 0) generateCap(true);
                if (radiusBottom > 0) generateCap(false);
            }

            // build geometry
            Mesh mesh = new Mesh();
            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.SetUVs(0, uvs);
            mesh.SetIndices(indices, MeshTopology.Triangles, 0);
            ArenaUnity.ToUnityMesh(ref mesh);
            mesh.RecalculateBounds();
            return mesh;

            void generateTorso()
            {
                // this will be used to calculate the normal
                var slope = (radiusBottom - radiusTop) / height;

                // generate vertices, normals and uvs
                for (int y = 0; y <= heightSegments; y++)
                {
                    var indexRow = new List<int>();
                    var v = 1f * y / heightSegments;

                    // calculate the radius of the current row
                    var radius = v * (radiusBottom - radiusTop) + radiusTop;
                    for (int x = 0; x <= radialSegments; x++)
                    {
                        var u = 1f * x / radialSegments;

                        var theta = u * thetaLength + thetaStart;

                        var sinTheta = Mathf.Sin(theta);
                        var cosTheta = Mathf.Cos(theta);

                        // vertex
                        var vertex = new Vector3();
                        vertex.x = radius * sinTheta;
                        vertex.y = -v * height + halfHeight;
                        vertex.z = radius * cosTheta;
                        vertices.Add(vertex);

                        // normal
                        var normal = new Vector3(sinTheta, slope, cosTheta).normalized;
                        normals.Add(normal);

                        // uv
                        uvs.Add(new Vector2(u, 1 - v));

                        // save index of vertex in respective row
                        indexRow.Add(index++);
                    }

                    // now save vertices of the row in our index array
                    indexArray.Add(indexRow);
                }

                // generate indices
                for (int x = 0; x < radialSegments; x++)
                {
                    for (int y = 0; y < heightSegments; y++)
                    {
                        // we use the index array to access the correct indices
                        var a = indexArray[y][x];
                        var b = indexArray[y + 1][x];
                        var c = indexArray[y + 1][x + 1];
                        var d = indexArray[y][x + 1];

                        // faces
                        indices.Add(a); indices.Add(b); indices.Add(d);
                        indices.Add(b); indices.Add(c); indices.Add(d);
                    }
                }
            }

            void generateCap(bool top)
            {
                // save the index of the first center vertex
                var centerIndexStart = index;

                var uv = new Vector2();
                var vertex = new Vector3();

                var radius = (top == true) ? radiusTop : radiusBottom;
                var sign = (top == true) ? 1 : -1;

                // first we generate the center vertex data of the cap.
                // because the geometry needs one set of uvs per face,
                // we must generate a center vertex per face/segment
                for (int x = 1; x <= radialSegments; x++)
                {
                    // vertex
                    vertices.Add(new Vector3(0, halfHeight * sign, 0));

                    // normal
                    normals.Add(new Vector3(0, sign, 0));

                    // uv
                    uvs.Add(new Vector2(0.5f, 0.5f));

                    // increase index
                    index++;
                }

                // save the index of the last center vertex
                var centerIndexEnd = index;

                // now we generate the surrounding vertices, normals and uvs
                for (int x = 0; x <= radialSegments; x++)
                {
                    var u = 1f * x / radialSegments;
                    var theta = u * thetaLength + thetaStart;

                    var cosTheta = Mathf.Cos(theta);
                    var sinTheta = Mathf.Sin(theta);

                    // vertex
                    vertex.x = radius * sinTheta;
                    vertex.y = halfHeight * sign;
                    vertex.z = radius * cosTheta;
                    vertices.Add(new Vector3(vertex.x, vertex.y, vertex.z));

                    // normal
                    normals.Add(new Vector3(0, sign, 0));

                    // uv
                    uv.x = (cosTheta * 0.5f) + 0.5f;
                    uv.y = (sinTheta * 0.5f * sign) + 0.5f;
                    uvs.Add(new Vector2(uv.x, uv.y));

                    // increase index
                    index++;
                }

                // generate indices
                for (int x = 0; x < radialSegments; x++)
                {
                    var c = centerIndexStart + x;
                    var i = centerIndexEnd + x;

                    if (top == true)
                    {
                        // face top
                        indices.Add(i); indices.Add(i + 1); indices.Add(c);
                    }
                    else
                    {
                        // face bottom
                        indices.Add(i + 1); indices.Add(i); indices.Add(c);
                    }
                }
            }
        }
    }
}
