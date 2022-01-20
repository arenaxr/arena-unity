// Modifired from: https://github.com/mattatz/unity-mesh-builder/tree/master/Assets/Packages/MeshBuilder/Scripts/Demo

using MeshBuilder;
using UnityEngine;

namespace ArenaUnity
{
    public class ArenaMeshRing : ArenaMeshBase
    {
        [SerializeField, Range(0f, 1f)] internal float innerRadius = 0.1f, outerRadius = 1f;
        [SerializeField, Range(2, 64)] internal int thetaSegments = 16, phiSegments = 16;
        [SerializeField, Range(0f, Mathf.PI * 2f)] internal float thetaStart = 0f, thetaLength = Mathf.PI * 2f;

        protected override void Build(MeshFilter filter)
        {
            filter.sharedMesh = RingBuilder.Build(innerRadius, outerRadius, thetaSegments, phiSegments, thetaStart, thetaLength);
        }
    }
}
