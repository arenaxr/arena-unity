// Modified from: https://github.com/mattatz/unity-mesh-builder/tree/master/Assets/Packages/MeshBuilder/Scripts/Demo

using MeshBuilder;
using UnityEngine;

namespace ArenaUnity
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class ArenaMeshCone : ArenaMesh
    {
        [SerializeField, Range(0.5f, 10f)] internal float radius = 1f;
        [SerializeField, Range(0.5f, 10f)] internal float height = 1f;
        [SerializeField, Range(5, 20)] internal int subdivision = 10;

        protected override void Build(MeshFilter filter)
        {
            filter.sharedMesh = ConeBuilder.Build(subdivision, radius, height);
        }
    }
}
