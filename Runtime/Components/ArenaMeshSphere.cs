/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2023, Carnegie Mellon University. All rights reserved.
 */

// Modified from: https://github.com/mattatz/unity-mesh-builder/tree/master/Assets/Packages/MeshBuilder/Scripts/Demo

using ArenaUnity.Schemas;
using Newtonsoft.Json;

namespace ArenaUnity
{
    public class ArenaMeshSphere : ArenaMesh
    {
        public ArenaSphereJson json = new ArenaSphereJson();

        protected override void ApplyRender()
        {
            filter.sharedMesh = SphereBuilder.Build(
                json.Radius,
                json.SegmentsWidth,
                json.SegmentsHeight
            );
            // TODO (mwfarb): can we support extra mesh construction from a-frame?
            //sphere.phiStart = (float)(data.phiStart != null ? Mathf.PI / 180 * (float)data.phiStart : 0f);
            //sphere.phiLength = (float)(data.phiLength != null ? Mathf.PI / 180 * (float)data.phiLength : Mathf.PI * 2f);
            //sphere.thetaStart = (float)(data.thetaStart != null ? Mathf.PI / 180 * (float)data.thetaStart : 0f);
            //sphere.thetaLength = (float)(data.thetaLength != null ? Mathf.PI / 180 * (float)data.thetaLength : Mathf.PI);
        }

        public override void UpdateObject()
        {
            var newJson = JsonConvert.SerializeObject(json);
            if (updatedJson != newJson)
            {
                var aobj = GetComponent<ArenaObject>();
                if (aobj != null)
                {
                    aobj.PublishUpdate($"{newJson}");
                    apply = true;
                }
            }
            updatedJson = newJson;
        }
    }
}
