// Modified from: https://github.com/mattatz/unity-mesh-builder/tree/master/Assets/Packages/MeshBuilder/Scripts/Demo

using ArenaUnity.Schemas;
using MeshBuilder;
using Newtonsoft.Json;
using UnityEngine;

namespace ArenaUnity
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class ArenaMeshBox : ArenaMesh
    {
        public ArenaBoxJson json = new ArenaBoxJson();

        protected override void Build(MeshFilter filter)
        {
            filter.sharedMesh = CubeBuilder.Build(
                json.Width,
                json.Height,
                json.Depth,
                json.SegmentsWidth,
                json.SegmentsHeight,
                json.SegmentsDepth
            );
        }

        public override void UpdateObject()
        {
            var newJson = JsonConvert.SerializeObject(json);
            if (updatedJson != newJson)
            {
                var aobj = GetComponent<ArenaObject>();
                if (aobj != null)
                {
                    aobj.PublishUpdate($"{{{newJson}}}");
                    apply = true;
                }
            }
            updatedJson = newJson;
        }
    }
}
