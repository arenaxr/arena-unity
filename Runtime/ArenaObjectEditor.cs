using UnityEditor;
using UnityEngine;

namespace ArenaUnity
{
    [CustomEditor(typeof(ArenaObject))]
    public class ArenaObjectEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            ArenaObject script = (ArenaObject)target;
            if (GUILayout.Button("Publish Object Update"))
            {
                script.SendUpdateSuccess();
            }

            DrawDefaultInspector();
        }
    }
}