// Modified from: https://github.com/mattatz/unity-mesh-builder/tree/master/Assets/Packages/MeshBuilder/Scripts/Demo

using UnityEngine;

namespace ArenaUnity
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(ArenaObject), typeof(MeshFilter), typeof(MeshRenderer))]
    public abstract class ArenaMesh : MonoBehaviour
    {
        protected MeshFilter filter;
        internal bool rebuild = false;

        protected virtual void Start()
        {
            filter = GetComponent<MeshFilter>();
            Build(filter);
        }
        protected abstract void Build(MeshFilter filter);

        protected void OnValidate()
        {
            if (filter == null) filter = GetComponent<MeshFilter>();
            var aobj = GetComponent<ArenaObject>();
            if (aobj != null) aobj.meshChanged = true;
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
