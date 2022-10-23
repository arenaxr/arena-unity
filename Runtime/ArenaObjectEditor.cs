/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021, The CONIX Research Center. All rights reserved.
 */

using UnityEditor;
using UnityEngine;

namespace ArenaUnity
{
#if UNITY_EDITOR
    [CustomEditor(typeof(ArenaObject))]
    public class ArenaObjectEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            ArenaObject aobj = (ArenaObject)target;

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

            GUI.enabled = !Application.isPlaying && aobj.HasPermissions;
            DrawDefaultInspector();
            GUI.enabled = true;

            // add button to publish manual json data changes if valid
            GUI.enabled = aobj.HasPermissions && aobj.isJsonValidated;
            if (GUILayout.Button("Publish Json Data"))
            {
                aobj.PublishJson();
            }
            GUI.enabled = true;

            // add any animation buttons
            if (aobj.animations != null && aobj.animations.Count > 0)
            {
                GUILayout.Space(5f);
                EditorGUILayout.LabelField("Animations", EditorStyles.boldLabel);
                foreach (string animation in aobj.animations)
                {
                    Animation anim = aobj.GetComponentInChildren<Animation>(true);
                    GUILayout.BeginHorizontal("Box");
                    //float seconds = anim[animation].length;
                    //string timespan = seconds > 0 ? TimeSpan.FromSeconds(seconds).ToString(@"(mm\:ss)") : "";
                    //GUIStyle style = new GUIStyle();
                    //style.richText = true;
                    //GUILayout.Label($"<b>{animation}</b> ({timespan})", style);
                    GUILayout.Label(animation);
                    if (GUILayout.Button($"Play", GUILayout.Width(40)))
                    {
                        anim.Play(animation);
                    }
                    if (GUILayout.Button($"Stop", GUILayout.Width(40)))
                    {
                        anim.Stop(animation);
                    }
                    if (GUILayout.Button($"Rewind", GUILayout.Width(60)))
                    {
                        anim.Rewind(animation);
                    }
                    GUILayout.EndHorizontal();
                }
            }

        }
    }
#endif
}
