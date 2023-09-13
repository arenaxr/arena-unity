// Modified from: https://github.com/mattatz/unity-mesh-builder/tree/master/Assets/Packages/MeshBuilder/Scripts/Demo

using ArenaUnity.Schemas;
using MeshBuilder;
using UnityEngine;

namespace ArenaUnity
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class ArenaMeshSphere : ArenaMesh
    {
        public ArenaSphereJson json = new ArenaSphereJson();

        protected override void Build(MeshFilter filter)
        {
            filter.sharedMesh = SphereBuilder.Build(
                json.Radius,
                json.SegmentsHeight,
                json.SegmentsWidth
            );
            // TODO (mwfarb): can we support extra mesh construction from a-frame?
            //sphere.phiStart = (float)(data.phiStart != null ? Mathf.PI / 180 * (float)data.phiStart : 0f);
            //sphere.phiLength = (float)(data.phiLength != null ? Mathf.PI / 180 * (float)data.phiLength : Mathf.PI * 2f);
            //sphere.thetaStart = (float)(data.thetaStart != null ? Mathf.PI / 180 * (float)data.thetaStart : 0f);
            //sphere.thetaLength = (float)(data.thetaLength != null ? Mathf.PI / 180 * (float)data.thetaLength : Mathf.PI);
        }
    }
}
