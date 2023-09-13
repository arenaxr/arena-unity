// Modified from: https://github.com/mattatz/unity-mesh-builder/tree/master/Assets/Packages/MeshBuilder/Scripts/Demo

using ArenaUnity.Schemas;
using MeshBuilder;
using UnityEngine;

namespace ArenaUnity
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class ArenaMeshCone : ArenaMesh
    {
        public ArenaConeJson json = new ArenaConeJson();

        protected override void Build(MeshFilter filter)
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
    }
}
