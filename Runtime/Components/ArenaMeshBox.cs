// Modified from: https://github.com/mattatz/unity-mesh-builder/tree/master/Assets/Packages/MeshBuilder/Scripts/Demo

using ArenaUnity.Schemas;
using MeshBuilder;
using UnityEngine;

namespace ArenaUnity
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class ArenaMeshBox : ArenaMesh
    {
        public ArenaBoxJson json = new ArenaBoxJson();

        protected override void Build(MeshFilter filter)
        {
            filter.sharedMesh = CubeBuilder.Build(
                json.Width,
                json.Height,
                json.Depth,
                json.SegmentsWidth,
                json.SegmentsHeight,
                json.SegmentsDepth
            );
        }
    }
}
