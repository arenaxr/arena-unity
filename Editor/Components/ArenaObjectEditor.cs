/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2023, Carnegie Mellon University. All rights reserved.
 */

using UnityEditor;
using UnityEngine;

namespace ArenaUnity.Editor.Components
{
    [CustomEditor(typeof(ArenaObject))]
    public class ArenaObjectEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            ArenaObject aobj = (ArenaObject)target;

            // sort arena component to the top, below Transform
            while (UnityEditorInternal.ComponentUtility.MoveComponentUp(aobj)) { }

            // add button to publish unity changes
            GUI.enabled = aobj.HasPermissions && aobj.messageType == "object";
            if (GUILayout.Button("Publish Unity Data"))
            {
                aobj.PublishCreateUpdate();
            }
            GUI.enabled = true;

            // edit authorization
            GUILayout.BeginHorizontal("Box");
            GUILayout.Label("Publish Permission:");
            if (Application.isPlaying)
            {
                var authStyle = new GUIStyle(EditorStyles.label);
                authStyle.normal.textColor = aobj.TextColor;
                var authString = aobj.HasPermissions ? "A" : "Not a";
                GUILayout.Label($"{authString}uthorized to publish changes", authStyle);
            }
            else
            {
                GUILayout.Label("Determined in playmode");
            }
            GUILayout.EndHorizontal();

            DrawDefaultInspector();

            // add button to publish manual json data changes if valid
            GUI.enabled = aobj.HasPermissions && aobj.isJsonValidated;
            if (GUILayout.Button("Publish Json Data"))
            {
                aobj.PublishUpdate(aobj.jsonData, true, true); // overwrite when doing full object update
            }
            GUI.enabled = true;

        }
    }
}
