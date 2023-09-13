// Modified from: https://github.com/mattatz/unity-mesh-builder/tree/master/Assets/Packages/MeshBuilder/Scripts/Demo

using ArenaUnity.Schemas;
using MeshBuilder;
using UnityEngine;

namespace ArenaUnity
{
    public class ArenaMeshCircle : ArenaMesh
    {
        public ArenaCircleJson json;

        protected override void Build(MeshFilter filter)
        {
            filter.sharedMesh = RingBuilder.Build(
                0f,
                json.Radius,
                json.Segments,
                1,
                json.ThetaStart,
                json.ThetaLength
            );
        }
    }
}
