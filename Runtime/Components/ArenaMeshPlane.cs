// Modified from: https://github.com/mattatz/unity-mesh-builder/tree/master/Assets/Packages/MeshBuilder/Scripts/Demo

using ArenaUnity.Schemas;
using MeshBuilder;
using Newtonsoft.Json;
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
        public ArenaPlaneJson json = new ArenaPlaneJson();

        protected override void Build(MeshFilter filter)
        {
            // Since this uses "triangles" = "segments", multiply by 2 so each "segment" gets 2 triangles
            switch (type)
            {
                case PlaneType.Noise:
                    // TODO (mwfarb): try to support noise plane, possibly with ocean wire object from a-frame extras:
                    filter.sharedMesh = PlaneBuilder.Build(new ParametricPlanePerlin(
                        Vector2.zero,
                        new Vector2(2f, 2f), 0.5f),
                        json.Width,
                        json.Height,
                        json.SegmentsWidth * 2,
                        json.SegmentsHeight * 2
                    );
                    break;
                default:
                    filter.sharedMesh = PlaneBuilder.Build(
                        json.Width,
                        json.Height,
                        json.SegmentsWidth * 2,
                        json.SegmentsHeight * 2
                    );
                    break;
            }
        }

        public override void UpdateObject()
        {
            var newJson = JsonConvert.SerializeObject(json);
            if (updatedJson != newJson)
            {
                var aobj = GetComponent<ArenaObject>();
                if (aobj != null)
                {
                    aobj.PublishUpdate($"{{\"{json.componentName}\":{newJson}}}");
                    apply = true;
                }
            }
            updatedJson = newJson;
        }
    }
}
