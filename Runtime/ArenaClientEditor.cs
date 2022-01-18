using UnityEditor;
using UnityEngine;

namespace ArenaUnity
{
    [CustomEditor(typeof(ArenaClient))]
    public class ArenaClientEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            ArenaClient script = (ArenaClient)target;

            // signout button
            if (GUILayout.Button("Signout"))
            {
                ArenaMenuCreate.SceneSignout();
            }

            // clickable scene url
            if (!string.IsNullOrWhiteSpace(script.sceneUrl))
            {
                GUILayout.Space(5f);
                GUIStyle style = new GUIStyle(GUI.skin.label);
                style.richText = true;
                if (GUILayout.Button($"<a href='{script.sceneUrl}'>{script.sceneUrl}</a>", style))
                {
                    Application.OpenURL(script.sceneUrl);
                }
                GUILayout.Space(5f);
            }

            DrawDefaultInspector();
        }
    }
}
