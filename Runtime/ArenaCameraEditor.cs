/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2023, Carnegie Mellon University. All rights reserved.
 */

using UnityEditor;
using UnityEngine;

namespace ArenaUnity
{
#if UNITY_EDITOR
    [CustomEditor(typeof(ArenaCamera))]
    public class ArenaCameraEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            ArenaCamera acobj = (ArenaCamera)target;

            // edit authorization
            GUILayout.BeginHorizontal("Box");
            GUILayout.Label("Publish Permission:");
            if (Application.isPlaying)
            {
                var authStyle = new GUIStyle(EditorStyles.label);
                authStyle.normal.textColor = acobj.TextColor;
                var authString = acobj.HasPermissions ? "A" : "Not a";
                GUILayout.Label($"{authString}uthorized to publish changes", authStyle);
            }
            else
            {
                GUILayout.Label("Determined in playmode");
            }
            GUILayout.EndHorizontal();

            DrawDefaultInspector();
        }
    }
#endif
}
