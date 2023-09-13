// Modified from: https://github.com/mattatz/unity-mesh-builder/tree/master/Assets/Packages/MeshBuilder/Scripts/Demo

using ArenaUnity.Schemas;
using MeshBuilder;
using UnityEngine;

namespace ArenaUnity
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class ArenaMeshIcosahedron : ArenaMesh
    {
        public ArenaIcosahedronJson json = new ArenaIcosahedronJson();

        protected override void Build(MeshFilter filter)
        {
            filter.sharedMesh = IcosahedronBuilder.Build(
                json.Radius,
                json.Detail
            );
        }
    }
}
