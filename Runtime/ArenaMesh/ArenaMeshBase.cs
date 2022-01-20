// Modifired from: https://github.com/mattatz/unity-mesh-builder/tree/master/Assets/Packages/MeshBuilder/Scripts/Demo

using UnityEngine;

namespace ArenaUnity
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(ArenaObject), typeof(MeshFilter), typeof(MeshRenderer))]
    public abstract class ArenaMeshBase : MonoBehaviour
    {
        protected MeshFilter filter;
        protected Material lineMaterial;
        private bool needsRebuild = false;

        protected virtual void Start()
        {
            filter = GetComponent<MeshFilter>();
            Build(filter);
        }
        protected abstract void Build(MeshFilter filter);

        protected void OnValidate()
        {
            if (filter == null) filter = GetComponent<MeshFilter>();
            needsRebuild = true;
        }

        protected void Update()
        {
            if (needsRebuild)
            {
                Build(filter);
                needsRebuild = false;
            }
        }

        protected void OnRenderObject()
        {
            if (filter == null || filter.sharedMesh == null) return;

            var mesh = filter.sharedMesh;
            var vertices = mesh.vertices;
            var triangles = mesh.triangles;

            CheckInit();
            if (lineMaterial != null)
            {
                lineMaterial.SetPass(0);
            }
            GL.PushMatrix();
            GL.MultMatrix(transform.localToWorldMatrix);

            GL.Begin(GL.LINES);

            for (int i = 0, n = triangles.Length; i < n; i += 3)
            {
                var a = vertices[triangles[i]];
                var b = vertices[triangles[i + 1]];
                var c = vertices[triangles[i + 2]];
                GL.Vertex(a); GL.Vertex(b);
                GL.Vertex(b); GL.Vertex(c);
                GL.Vertex(c); GL.Vertex(a);
            }
            GL.End();

            GL.PopMatrix();
        }
        protected void CheckInit()
        {
            if (lineMaterial == null)
            {
                Shader shader = Shader.Find("MeshBuilder/DebugLine");
                if (shader == null) return;
                lineMaterial = new Material(shader);
                lineMaterial.hideFlags = HideFlags.HideAndDontSave;
            }
        }
    }
}
