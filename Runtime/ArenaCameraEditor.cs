/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021, The CONIX Research Center. All rights reserved.
 */

using PrettyHierarchy;
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

            Color textColorPerms;
            if (EditorGUIUtility.isProSkin)
                textColorPerms = acobj.HasPermissions ? PrettyObject.ColorDarkAllow : PrettyObject.ColorDarkDisallow;
            else
                textColorPerms = acobj.HasPermissions ? PrettyObject.ColorLightAllow : PrettyObject.ColorLightDisallow;

            // edit authorization
            GUILayout.BeginHorizontal("Box");
            GUILayout.Label("Publish Permission:");
            if (Application.isPlaying)
            {
                var authStyle = new GUIStyle(EditorStyles.label);
                authStyle.normal.textColor = textColorPerms;
                var authString = acobj.HasPermissions ? "A" : "Not a";
                GUILayout.Label($"{authString}uthorized to publish changes", authStyle);
            }
            else
            {
                GUILayout.Label("Determined in playmode");
            }
            GUILayout.EndHorizontal();

            GUI.enabled = !Application.isPlaying && acobj.HasPermissions;
            DrawDefaultInspector();
            GUI.enabled = true;
        }
    }
#endif
}
