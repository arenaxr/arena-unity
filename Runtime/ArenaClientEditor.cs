using UnityEditor;
using UnityEngine;

namespace ArenaUnity
{
    [CustomEditor(typeof(ArenaClient))]
    public class ArenaClientEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Signout"))
            {
                ArenaMenuCreate.SceneSignout();
            }

            DrawDefaultInspector();
        }
    }
}
