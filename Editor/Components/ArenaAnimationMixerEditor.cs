/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2023, Carnegie Mellon University. All rights reserved.
 */

using UnityEditor;
using UnityEngine;
using ArenaUnity.Components;

namespace ArenaUnity.Editor.Components
{
    [CustomEditor(typeof(ArenaAnimationMixer))]
    public class ArenaAnimationMixerEditor : UnityEditor.Editor
    {

        public override void OnInspectorGUI()
        {
            ArenaAnimationMixer am = (ArenaAnimationMixer)target;

            // add button to publish unity changes
            var aobj = am.GetComponent<ArenaObject>();
            if (aobj != null)
                GUI.enabled = aobj.HasPermissions;

            DrawDefaultInspector();

            if (am.json == null) return; // not ready

            // add any animation buttons
            if (aobj != null && aobj.animations != null && aobj.animations.Count > 0)
            {
                GUILayout.Space(5f);
                EditorGUILayout.LabelField("Clips ", EditorStyles.boldLabel);
                GUILayout.BeginHorizontal("Box");
                if (GUILayout.Toggle((am.json.Clip == "*"), "All"))
                {
                    am.json.Clip = "*";
                    if (am.json != null) am.UpdateObject();
                }
                GUILayout.EndHorizontal();
                for (int i = 0; i < aobj.animations.Count; i++)
                {
                    GUILayout.BeginHorizontal("Box");
                    if (GUILayout.Toggle((am.json.Clip == aobj.animations[i]), $"{i}: {aobj.animations[i]}"))
                    {
                        am.json.Clip = aobj.animations[i];
                        if (am.json != null) am.UpdateObject();
                    }
                    GUILayout.EndHorizontal();
                }
            }

            GUI.enabled = true;
        }
    }
}
