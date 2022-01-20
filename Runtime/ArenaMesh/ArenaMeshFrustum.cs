// Modifired from: https://github.com/mattatz/unity-mesh-builder/tree/master/Assets/Packages/MeshBuilder/Scripts/Demo

using MeshBuilder;
using UnityEngine;

namespace ArenaUnity
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class ArenaMeshFrustum : ArenaMeshBase
    {
        [SerializeField, Range(0.1f, 1f)] internal float nearClip = 0.1f;
        [SerializeField, Range(1f, 5f)] internal float farClip = 1f;
        [SerializeField, Range(45f, 90f)] internal float fieldOfView = 60f;
        [SerializeField, Range(0f, 1f)] internal float aspectRatio = 1f;

        protected override void Build(MeshFilter filter)
        {
            filter.sharedMesh = FrustumBuilder.Build(Vector3.forward, Vector3.up, nearClip, farClip, fieldOfView, aspectRatio);
        }
    }
}
