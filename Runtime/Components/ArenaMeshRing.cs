// Modified from: https://github.com/mattatz/unity-mesh-builder/tree/master/Assets/Packages/MeshBuilder/Scripts/Demo

using System.Net.NetworkInformation;
using ArenaUnity.Schemas;
using MeshBuilder;
using Newtonsoft.Json;
using UnityEngine;

namespace ArenaUnity
{
    public class ArenaMeshRing : ArenaMesh
    {
        public ArenaRingJson json = new ArenaRingJson();

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
