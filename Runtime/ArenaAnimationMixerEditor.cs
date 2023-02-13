/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021, The CONIX Research Center. All rights reserved.
 */

using System.Text.RegularExpressions;
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
            var aobj = am.GetComponent<ArenaObject>();
            if (aobj != null)
                GUI.enabled = aobj.HasPermissions;
            if (am.json != null)
            {
                if (GUILayout.Button($"Publish {am.componentName}"))
                {
                    am.UpdateObject();
                }
            }
            GUI.enabled = true;

            DrawDefaultInspector();

            // add any animation buttons
            string pattern = @$"{am.json.clip.Replace("*", @"\w*")}"; // update wildcards for .Net
            if (am.animations != null && am.animations.Count > 0)
            {
                GUILayout.Space(5f);
                EditorGUILayout.LabelField("Clips ", EditorStyles.boldLabel);
                for (int i = 0; i < am.animations.Count; i++)
                {
                    GUILayout.BeginHorizontal("Box");
                    Match m = Regex.Match(am.animations[i], pattern);
                    if (GUILayout.Toggle(m.Success, $"{i}: {am.animations[i]}"))
                    {
                        Debug.Log($"on {am.animations[i]}");
                    }
                    else
                    {
                        Debug.Log($"off {am.animations[i]}");
                    }
                    GUILayout.EndHorizontal();
                }
            }



        }
    }
#endif
}
