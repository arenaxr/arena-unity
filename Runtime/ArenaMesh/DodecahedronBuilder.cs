//http://csharphelper.com/blog/2015/12/platonic-solids-part-7-the-dodecahedron/

using System.Collections.Generic;
using MeshBuilder;
using UnityEngine;

// TODO: fix to render mesh with position at center
namespace ArenaUnity
{
    internal class DodecahedronBuilder
    {
        internal static Mesh Build(float radius, int details)
        {
            var side_length = 1;

            // Value t1 is actually never used.
            float s = side_length;
            //double t1 = 2.0 * Mathf.PI / 5.0;
            float t2 = (float)(Mathf.PI / 10.0);
            float t3 = (float)(3.0 * Mathf.PI / 10.0);
            float t4 = (float)(Mathf.PI / 5.0);
            float d1 = (float)(s / 2.0 / Mathf.Sin(t4));
            float d2 = d1 * Mathf.Cos(t4);
            float d3 = d1 * Mathf.Cos(t2);
            float d4 = d1 * Mathf.Sin(t2);
            float Fx =
                (float)((s * s - (2.0 * d3) * (2.0 * d3) -
                    (d1 * d1 - d3 * d3 - d4 * d4)) /
                        (2.0 * (d4 - d1)));
            float d5 = Mathf.Sqrt((float)(0.5 *
                (s * s + (2.0 * d3) * (2.0 * d3) -
                    (d1 - Fx) * (d1 - Fx) -
                        (d4 - Fx) * (d4 - Fx) - d3 * d3)));
            float Fy = (float)((Fx * Fx - d1 * d1 - d5 * d5) / (2.0 * d5));
            float Ay = d5 + Fy;

            Vector3 A = new Vector3(d1, Ay, 0);
            Vector3 B = new Vector3(d4, Ay, d3);
            Vector3 C = new Vector3(-d2, Ay, s / 2);
            Vector3 D = new Vector3(-d2, Ay, -s / 2);
            Vector3 E = new Vector3(d4, Ay, -d3);
            Vector3 F = new Vector3(Fx, Fy, 0);
            Vector3 G = new Vector3(Fx * Mathf.Sin(t2), Fy,
                Fx * Mathf.Cos(t2));
            Vector3 H = new Vector3(-Fx * Mathf.Sin(t3), Fy,
                Fx * Mathf.Cos(t3));
            Vector3 I = new Vector3(-Fx * Mathf.Sin(t3), Fy,
                -Fx * Mathf.Cos(t3));
            Vector3 J = new Vector3(Fx * Mathf.Sin(t2), Fy,
                -Fx * Mathf.Cos(t2));
            Vector3 K = new Vector3(Fx * Mathf.Sin(t3), -Fy,
                Fx * Mathf.Cos(t3));
            Vector3 L = new Vector3(-Fx * Mathf.Sin(t2), -Fy,
                Fx * Mathf.Cos(t2));
            Vector3 M = new Vector3(-Fx, -Fy, 0);
            Vector3 N = new Vector3(-Fx * Mathf.Sin(t2), -Fy,
                -Fx * Mathf.Cos(t2));
            Vector3 O = new Vector3(Fx * Mathf.Sin(t3), -Fy,
                -Fx * Mathf.Cos(t3));
            Vector3 P = new Vector3(d2, -Ay, s / 2);
            Vector3 Q = new Vector3(-d4, -Ay, d3);
            Vector3 R = new Vector3(-d1, -Ay, 0);
            Vector3 S = new Vector3(-d4, -Ay, -d3);
            Vector3 T = new Vector3(d2, -Ay, -s / 2);

            var vertices = new List<Vector3>() { A, B, C, D, E, F, G, H, I, J, K, L, M, N, O, P, Q, R, S, T };
            var indices = new List<int>() { };
            return PolyhedronBuilder.Build(vertices, indices, radius, details);
        }
    }
}
