// Modified from: https://github.com/mattatz/unity-mesh-builder/tree/master/Assets/Packages/MeshBuilder/Scripts/Demo

using ArenaUnity.Schemas;
using MeshBuilder;
using UnityEngine;

namespace ArenaUnity
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class ArenaMeshOctahedron : ArenaMesh
    {
        public ArenaOctahedronJson json = new ArenaOctahedronJson();

        protected override void Build(MeshFilter filter)
        {
            filter.sharedMesh = OctahedronBuilder.Build(
                json.Radius,
                json.Detail
            );
        }
    }
}
