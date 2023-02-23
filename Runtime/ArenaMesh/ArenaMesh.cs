// Modified from: https://github.com/mattatz/unity-mesh-builder/tree/master/Assets/Packages/MeshBuilder/Scripts/Demo

using UnityEngine;

namespace ArenaUnity
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(ArenaObject), typeof(MeshFilter), typeof(MeshRenderer))]
    public abstract class ArenaMesh : MonoBehaviour
    {
        protected MeshFilter filter;
        internal bool build = false;
        internal bool scriptLoaded = false;

        protected virtual void Start()
        {
            filter = GetComponent<MeshFilter>();
            Build(filter);
        }
        protected abstract void Build(MeshFilter filter);

        protected void OnValidate()
        {
            if (filter == null) filter = GetComponent<MeshFilter>();
            build = true;

            if (!scriptLoaded)
            {
                scriptLoaded = true;
            }
            else
            {   // do not publish mesh update on script load
                var aobj = GetComponent<ArenaObject>();
                if (aobj != null) aobj.meshChanged = true;
            }
        }

        protected void Update()
        {
            if (build)
            {
                Build(filter);
                build = false;
            }
        }
    }
}
