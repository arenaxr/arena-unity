// Modified from: https://github.com/mattatz/unity-mesh-builder/tree/master/Assets/Packages/MeshBuilder/Scripts/Demo

using ArenaUnity.Schemas;
using MeshBuilder;
using UnityEditor;
using UnityEngine;

namespace ArenaUnity
{
    public class ArenaMeshTorus : ArenaMesh
    {
        public ArenaTorusJson json = new ArenaTorusJson();

        protected override void Build(MeshFilter filter)
        {
            filter.sharedMesh = TorusBuilder.Build(
                json.Radius,
                json.RadiusTubular * 2,
                json.SegmentsRadial,
                json.SegmentsTubular,
                0f,
                Mathf.PI / 180 * json.Arc
            );
        }
    }
}
