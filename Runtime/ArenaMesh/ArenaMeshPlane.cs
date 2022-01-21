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
        [SerializeField, Range(2, 40)] internal int wSegments = 2;
        [SerializeField, Range(2, 40)] internal int hSegments = 2;

        protected override void Build(MeshFilter filter)
        {
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
