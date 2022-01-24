using UnityEngine;

namespace ArenaUnity
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class ArenaMeshCapsule : ArenaMesh
    {
        [SerializeField, Range(0.5f, 10f)] internal float radius = 1f;
        [SerializeField, Range(0.5f, 10f)] internal float height = 4f;
        [SerializeField, Range(3, 16)] internal int radialSegments = 8, heightSegments = 4;

        protected override void Build(MeshFilter filter)
        {
            filter.sharedMesh = CapsuleBuilder.CapsuleData(radius, height - (radius * 2), radialSegments, heightSegments);
        }

    }
}
