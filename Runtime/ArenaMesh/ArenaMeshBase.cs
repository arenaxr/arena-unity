// Modifired from: https://github.com/mattatz/unity-mesh-builder/tree/master/Assets/Packages/MeshBuilder/Scripts/Demo

using UnityEngine;

namespace ArenaUnity
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(ArenaObject), typeof(MeshFilter), typeof(MeshRenderer))]
    public abstract class ArenaMeshBase : MonoBehaviour
    {
        protected MeshFilter filter;
        private bool rebuild = false;

        protected virtual void Start()
        {
            filter = GetComponent<MeshFilter>();
            Build(filter);
        }
        protected abstract void Build(MeshFilter filter);

        protected void OnValidate()
        {
            if (filter == null) filter = GetComponent<MeshFilter>();
            rebuild = true;
        }

        protected void Update()
        {
            if (rebuild)
            {
                Build(filter);
                rebuild = false;
            }
        }
    }
}
