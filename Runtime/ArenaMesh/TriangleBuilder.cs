using UnityEngine;

namespace ArenaUnity
{
    internal class TriangleBuilder
    {
        internal static Mesh Build(Vector3 vertexA, Vector3 vertexB, Vector3 vertexC)
        {
            Mesh mesh = new Mesh();
            mesh.vertices = new Vector3[] { vertexA, vertexB, vertexC };
            mesh.uv = new Vector2[] {
                new Vector2(vertexA.x, vertexA.y),
                new Vector2(vertexB.x, vertexB.y),
                new Vector2(vertexC.x, vertexC.y) };
            mesh.triangles = new int[] { 0, 1, 2 };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }
    }
}
