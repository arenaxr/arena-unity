// Modified from: https://github.com/mattatz/unity-mesh-builder/tree/master/Assets/Packages/MeshBuilder/Scripts/Demo

using ArenaUnity.Schemas;
using MeshBuilder;
using UnityEditor;
using Newtonsoft.Json;
using UnityEngine;

namespace ArenaUnity
{
    public class ArenaMeshTorus : ArenaMesh
    {
        public ArenaTorusJson json = new ArenaTorusJson();

        protected override void Build(MeshFilter filter)
        {
            filter.sharedMesh = TorusBuilder.Build(
                json.Radius,
                json.RadiusTubular * 2,
                json.SegmentsRadial,
                json.SegmentsTubular,
                0f,
                Mathf.PI / 180 * json.Arc
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
                    aobj.PublishUpdate($"{{\"{json.componentName}\":{newJson}}}");
                    apply = true;
                }
            }
            updatedJson = newJson;
        }
    }
}
