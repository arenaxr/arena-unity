/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2023, Carnegie Mellon University. All rights reserved.
 */

using ArenaUnity.Schemas;
using UnityEditor;
using UnityEngine;

namespace ArenaUnity.Components
{
#if UNITY_EDITOR
    [CustomEditor(typeof(ArenaAnimationMixer))]
    public class ArenaAnimationMixerEditor : Editor
    {

        public override void OnInspectorGUI()
        {
            ArenaAnimationMixer am = (ArenaAnimationMixer)target;

            // add button to publish unity changes
            var aobj = am.GetComponent<ArenaObject>();
            if (aobj != null)
                GUI.enabled = aobj.HasPermissions;
            if (am.json != null)
            {
                if (GUILayout.Button($"Publish {ArenaAnimationMixerJson.componentName}"))
                {
                    am.UpdateObject();
                }
            }
            GUI.enabled = true;

            DrawDefaultInspector();

            // add any animation buttons
            if (am.animations != null && am.animations.Count > 0)
            {
                GUILayout.Space(5f);
                EditorGUILayout.LabelField("Clips ", EditorStyles.boldLabel);
                GUILayout.BeginHorizontal("Box");
                if (GUILayout.Toggle((am.json.clip == "*"), "All"))
                {
                    am.json.clip = "*";
                }
                GUILayout.EndHorizontal();
                for (int i = 0; i < am.animations.Count; i++)
                {
                    GUILayout.BeginHorizontal("Box");
                    if (GUILayout.Toggle((am.json.clip == am.animations[i]), $"{i}: {am.animations[i]}"))
                    {
                        am.json.clip = am.animations[i];
                    }
                    GUILayout.EndHorizontal();
                }
            }

        }
    }
#endif
}
