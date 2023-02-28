// Modified from: https://github.com/mattatz/unity-mesh-builder/tree/master/Assets/Packages/MeshBuilder/Scripts/Demo

using MeshBuilder;
using UnityEngine;

namespace ArenaUnity
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]

    public class ArenaMeshPlane : ArenaMesh
    {
        public enum PlaneType
        {
            Default, Noise
        };

        [SerializeField] internal PlaneType type = PlaneType.Default;
        [SerializeField, Range(0.5f, 10f)] internal float width = 1f;
        [SerializeField, Range(0.5f, 10f)] internal float height = 1f;
        [SerializeField, Range(1, 20)] internal int wSegments = 1;
        [SerializeField, Range(1, 20)] internal int hSegments = 1;

        protected override void Build(MeshFilter filter)
        {
            // convert to triangles
            wSegments *= 2;
            hSegments *= 2;

            switch (type)
            {
                case PlaneType.Noise:
                    filter.sharedMesh = PlaneBuilder.Build(new ParametricPlanePerlin(Vector2.zero, new Vector2(2f, 2f), 0.5f), width, height, wSegments, hSegments);
                    break;
                default:
                    filter.sharedMesh = PlaneBuilder.Build(width, height, wSegments, hSegments);
                    break;
            }
        }
    }
}
