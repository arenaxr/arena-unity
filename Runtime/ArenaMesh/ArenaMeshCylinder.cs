// Modifired from: https://github.com/mattatz/unity-mesh-builder/tree/master/Assets/Packages/MeshBuilder/Scripts/Demo

using MeshBuilder;
using UnityEngine;

namespace ArenaUnity
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class ArenaMeshCylinder : ArenaMeshBase
    {
        [SerializeField, Range(0.5f, 10f)] internal float radius = 1f;
        [SerializeField, Range(0.5f, 10f)] internal float height = 4f;
        [SerializeField, Range(3, 16)] internal int radialSegments = 8, heightSegments = 4;
        [SerializeField] internal bool openEnded = false;

        protected override void Build(MeshFilter filter)
        {
            filter.sharedMesh = CylinderBuilder.Build(radius, height, radialSegments, heightSegments, openEnded);
        }
    }
}
