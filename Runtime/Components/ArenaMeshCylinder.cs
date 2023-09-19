// Modified from: https://github.com/mattatz/unity-mesh-builder/tree/master/Assets/Packages/MeshBuilder/Scripts/Demo

using ArenaUnity.Schemas;
using MeshBuilder;
using Newtonsoft.Json;
using UnityEngine;

namespace ArenaUnity
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class ArenaMeshCylinder : ArenaMesh
    {
        public ArenaCylinderJson json = new ArenaCylinderJson();

        protected override void Build(MeshFilter filter)
        {
            
            filter.sharedMesh = CylinderBuilder.Build(
                json.Radius,
                json.Height,
                json.SegmentsRadial,
                json.SegmentsHeight,
                !json.OpenEnded // TODO (mwfarb): for some reason openEnded is inverted
            );
            // TODO (mwfarb): can we support extra mesh construction from a-frame?
            //cylinder.thetaStart = (float)(data.thetaStart != null ? Mathf.PI / 180 * (float)data.thetaStart : 0f);
            //cylinder.thetaLength = (float)(data.thetaLength != null ? Mathf.PI / 180 * (float)data.thetaLength : Mathf.PI * 2f);
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
