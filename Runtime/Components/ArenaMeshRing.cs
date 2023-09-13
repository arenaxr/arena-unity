// Modified from: https://github.com/mattatz/unity-mesh-builder/tree/master/Assets/Packages/MeshBuilder/Scripts/Demo

using System.Net.NetworkInformation;
using ArenaUnity.Schemas;
using MeshBuilder;
using UnityEngine;

namespace ArenaUnity
{
    public class ArenaMeshRing : ArenaMesh
    {
        public ArenaRingJson json;

        protected override void Build(MeshFilter filter)
        {
            filter.sharedMesh = RingBuilder.Build(
                json.RadiusInner,
                json.RadiusOuter,
                json.SegmentsTheta,
                json.SegmentsPhi,
                Mathf.PI / 180 * json.ThetaStart,
                Mathf.PI / 180 * json.ThetaLength
            );
        }
    }
}
