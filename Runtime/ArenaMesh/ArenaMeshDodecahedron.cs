// Modified from: https://github.com/mattatz/unity-mesh-builder/tree/master/Assets/Packages/MeshBuilder/Scripts/Demo

using MeshBuilder;
using UnityEngine;

namespace ArenaUnity
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class ArenaMeshDodecahedron : ArenaMesh
    {
        [SerializeField, Range(0.5f, 10f)] internal float radius = 1f;
        [SerializeField, Range(0, 5)] internal int details = 1;

        protected override void Build(MeshFilter filter)
        {
            filter.sharedMesh = DodecahedronBuilder.Build(radius, details);
        }
    }
}
