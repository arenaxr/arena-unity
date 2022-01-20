// Modified from: https://github.com/mattatz/unity-mesh-builder/tree/master/Assets/Packages/MeshBuilder/Scripts/Demo

using MeshBuilder;
using UnityEngine;

namespace ArenaUnity
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class ArenaMeshSphere : ArenaMeshBase
    {
        [SerializeField, Range(0.5f, 10f)] internal float radius = 1f;
        [SerializeField, Range(8, 20)] internal int lonSegments = 10;
        [SerializeField, Range(8, 20)] internal int latSegments = 10;

        protected override void Build(MeshFilter filter)
        {
            filter.sharedMesh = SphereBuilder.Build(radius, lonSegments, latSegments);
        }
    }
}
