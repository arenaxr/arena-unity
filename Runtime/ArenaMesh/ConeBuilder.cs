/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2023, Carnegie Mellon University. All rights reserved.
 */

// Modified from: https://github.com/mattatz/unity-mesh-builder/blob/master/Assets/Packages/MeshBuilder/Scripts/Builders/ConeBuilder.cs

using UnityEngine;

namespace ArenaUnity
{
    public class ConeBuilder
    {
        // https://gist.github.com/mattatz/aba0d06fa56ef65e45e2
        public static Mesh Build(int subdivisions = 10, float radius = 1f, float height = 1f)
        {
            Mesh mesh = new Mesh();

            Vector3[] vertices = new Vector3[subdivisions + 2];
            Vector2[] uv = new Vector2[vertices.Length];
            int[] triangles = new int[(subdivisions * 2) * 3];

            // changed to match arena (mwfarb): vertices[0] = Vector3.zero;
            vertices[0] = new Vector3(0f, height / -2f, 0f);
            uv[0] = new Vector2(0.5f, 0f);
            for (int i = 0, n = subdivisions - 1; i < subdivisions; i++)
            {
                float ratio = (float)i / n;
                float r = ratio * (Mathf.PI * 2f);
                float x = Mathf.Cos(r) * radius;
                float z = Mathf.Sin(r) * radius;
                // changed to match arena (mwfarb): vertices[i + 1] = new Vector3(x, 0f, z);
                vertices[i + 1] = new Vector3(x, height / -2, z);
                uv[i + 1] = new Vector2(ratio, 0f);
            }
            // changed to match arena (mwfarb): vertices[subdivisions + 1] = new Vector3(0f, height, 0f);
            vertices[subdivisions + 1] = new Vector3(0f, height / 2, 0f);
            uv[subdivisions + 1] = new Vector2(0.5f, 1f);

            // construct bottom
            for (int i = 0, n = subdivisions - 1; i < n; i++)
            {
                int offset = i * 3;
                triangles[offset] = 0;
                triangles[offset + 1] = i + 1;
                triangles[offset + 2] = i + 2;
            }

            // construct sides
            int bottomOffset = subdivisions * 3;
            for (int i = 0, n = subdivisions - 1; i < n; i++)
            {
                int offset = i * 3 + bottomOffset;
                triangles[offset] = i + 1;
                triangles[offset + 1] = subdivisions + 1;
                triangles[offset + 2] = i + 2;
            }

            mesh.vertices = vertices;
            mesh.uv = uv;
            mesh.triangles = triangles;
            ArenaUnity.ToUnityMesh(ref mesh);
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();

            return mesh;
        }
    }
}
