/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021, The CONIX Research Center. All rights reserved.
 */

using System.Collections.ObjectModel;
using UnityEditor;
using UnityEngine;

namespace ArenaUnity
{
#if UNITY_EDITOR
    [CustomEditor(typeof(ArenaAnimationMixer))]
    public class ArenaAnimationMixerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            ArenaAnimationMixer am = (ArenaAnimationMixer)target;

            // add button to publish unity changes
            //GUI.enabled = am.HasPermissions && am.messageType == "object";
            if (GUILayout.Button("Update animation-mixer"))
            {
               // am.PublishCreateUpdate();
            }
            GUI.enabled = true;

            DrawDefaultInspector();

            // add any animation buttons
            if (am.animations != null && am.animations.Count > 0)
            {
                GUILayout.Space(5f);
                EditorGUILayout.LabelField("Clips Selected", EditorStyles.boldLabel);
                GUILayout.BeginHorizontal("Box");
                if (GUILayout.Toggle(true, $"All (total = {am.animations.Count})"))
                {
                    // anim.Play(animation);
                }
                else
                {
                    //anim.Stop(animation);
                }
                GUILayout.EndHorizontal();
                for (int i = 0; i < am.animations.Count; i++)
                {
                    GUILayout.BeginHorizontal("Box");
                    if (GUILayout.Toggle(true, $"{i}: {am.animations[i]}"))
                    {
                       // anim.Play(animation);
                    }
                    else
                    {
                        //anim.Stop(animation);
                    }
                    GUILayout.EndHorizontal();
                }
            }

        }
    }
#endif
}
