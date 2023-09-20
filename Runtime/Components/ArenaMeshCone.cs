/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2023, Carnegie Mellon University. All rights reserved.
 */

// Modified from: https://github.com/mattatz/unity-mesh-builder/tree/master/Assets/Packages/MeshBuilder/Scripts/Demo

using ArenaUnity.Schemas;
using MeshBuilder;
using Newtonsoft.Json;

namespace ArenaUnity
{
    public class ArenaMeshCone : ArenaMesh
    {
        public ArenaConeJson json = new ArenaConeJson();

        protected override void ApplyRender()
        {
            filter.sharedMesh = ConeBuilder.Build(
                json.SegmentsRadial,
                json.RadiusBottom,
                json.Height
            );
            // TODO (mwfarb): can we support extra mesh construction from a-frame?
            //cone.radiusTop = json.radiusTop != null ? (float)json.radiusTop : 0.01f;
            //cone.segmentsHeight = json.segmentsHeight != null ? (int)json.segmentsHeight : 18;
            //cone.openEnded = json.openEnded != null ? Convert.ToBoolean(json.openEnded) : false;
            //cone.thetaStart = (float)(json.thetaStart != null ? Mathf.PI / 180 * (float)json.thetaStart : 0f);
            //cone.thetaLength = (float)(json.thetaLength != null ? Mathf.PI / 180 * (float)json.thetaLength : Mathf.PI * 2f);
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
