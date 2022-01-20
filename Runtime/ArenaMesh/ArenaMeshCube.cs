// Modifired from: https://github.com/mattatz/unity-mesh-builder/tree/master/Assets/Packages/MeshBuilder/Scripts/Demo

using MeshBuilder;
using UnityEngine;

namespace ArenaUnity
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class ArenaMeshCube : ArenaMeshBase
    {
        [SerializeField, Range(0.5f, 10f)] internal float width = 1f;
        [SerializeField, Range(0.5f, 10f)] internal float height = 1f;
        [SerializeField, Range(0.5f, 10f)] internal float depth = 1f;
        [SerializeField, Range(2, 20)] internal int widthSegments = 2;
        [SerializeField, Range(2, 20)] internal int heightSegments = 2;
        [SerializeField, Range(2, 20)] internal int depthSegments = 2;

        protected override void Build(MeshFilter filter)
        {
            filter.sharedMesh = CubeBuilder.Build(
                width, height, depth,
                widthSegments, heightSegments, depthSegments
            );
        }
    }
}
