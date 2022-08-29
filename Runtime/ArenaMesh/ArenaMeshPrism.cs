using MeshBuilder;
using UnityEngine;

namespace ArenaUnity
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class ArenaMeshPrism : ArenaMesh
    {
        [SerializeField, Range(0.5f, 10f)] internal float width = 1f;
        [SerializeField, Range(0.5f, 10f)] internal float height = 1f;
        [SerializeField, Range(0.5f, 10f)] internal float depth = 1f;

        protected override void Build(MeshFilter filter)
        {
            // TODO: filter.sharedMesh = PrismBuilder.Build( width, height, depth );
        }
    }
}
