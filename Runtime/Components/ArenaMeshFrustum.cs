// Modified from: https://github.com/mattatz/unity-mesh-builder/tree/master/Assets/Packages/MeshBuilder/Scripts/Demo

using MeshBuilder;
using Newtonsoft.Json;
using UnityEngine;

namespace ArenaUnity
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class ArenaMeshFrustum : ArenaMesh
    {
        [SerializeField, Range(0.1f, 1f)] internal float nearClip = 0.1f;
        [SerializeField, Range(1f, 5f)] internal float farClip = 1f;
        [SerializeField, Range(45f, 90f)] internal float fieldOfView = 60f;
        [SerializeField, Range(0f, 1f)] internal float aspectRatio = 1f;

        protected override void Build(MeshFilter filter)
        {
            filter.sharedMesh = FrustumBuilder.Build(
                Vector3.forward,
                Vector3.up,
                nearClip,
                farClip,
                fieldOfView,
                aspectRatio
            );
            // TODO (mwfarb): introduce frustum to arena
            Debug.LogWarning("Frustum rendering not yet supported in ARENA A-Frame!!!!");
        }

        public override void UpdateObject()
        {
            //var newJson = JsonConvert.SerializeObject(json);
            //if (updatedJson != newJson)
            //{
            //    var aobj = GetComponent<ArenaObject>();
            //    if (aobj != null)
            //    {
            //        aobj.PublishUpdate($"{{\"{json.componentName}\":{newJson}}}");
            //        apply = true;
            //    }
            //}
            //updatedJson = newJson;
        }
    }
}
