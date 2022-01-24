// Modified from: https://github.com/mattatz/unity-mesh-builder/tree/master/Assets/Packages/MeshBuilder/Scripts/Demo

using MeshBuilder;
using UnityEngine;

namespace ArenaUnity
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]

    public class ArenaMeshTriangle : ArenaMesh
    {
        [SerializeField] internal Vector3 vertexA = new Vector3(0f, 0.5f, 0f);
        [SerializeField] internal Vector3 vertexB = new Vector3(-0.5f, -0.5f, 0f);
        [SerializeField] internal Vector3 vertexC = new Vector3(0.5f, -0.5f, 0f);

        protected override void Build(MeshFilter filter)
        {
            // TODO: filter.sharedMesh = TriangleBuilder.Build(vertexA, vertexB, vertexC);
        }
    }
}
