// Modified from: https://github.com/mattatz/unity-mesh-builder/tree/master/Assets/Packages/MeshBuilder/Scripts/Demo

using MeshBuilder;
using UnityEngine;

namespace ArenaUnity
{
    public class ArenaMeshCircle : ArenaMeshBase
    {
        [SerializeField, Range(0f, 10f)] internal float radius = 1f;
        [SerializeField, Range(2, 64)] internal int segments = 16;
        [SerializeField, Range(0f, Mathf.PI * 2f)] internal float thetaStart = 0f, thetaLength = Mathf.PI * 2f;

        protected override void Build(MeshFilter filter)
        {
            filter.sharedMesh = RingBuilder.Build(0f, radius, segments, 1, thetaStart, thetaLength);
        }
    }
}
