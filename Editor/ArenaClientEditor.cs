/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2023, Carnegie Mellon University. All rights reserved.
 */

using System;
using UnityEditor;
using UnityEngine;

namespace ArenaUnity.Editor
{
    [CustomEditor(typeof(ArenaClientScene))]
    public class ArenaClientEditor : UnityEditor.Editor
    {
        Vector2 scrollPos = Vector2.zero;

        public override void OnInspectorGUI()
        {
            ArenaClientScene script = (ArenaClientScene)target;

            // signout button
            if (GUILayout.Button("Signout"))
            {
                ArenaMqttClient.SignoutArena();
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

            // add readonly auth results
            GUILayout.Space(5f);
            EditorGUILayout.LabelField("Authentication", EditorStyles.boldLabel);

            if (!string.IsNullOrWhiteSpace(script.username))
            {
                GUILayout.BeginHorizontal("Box");
                GUILayout.Label("Username");
                GUILayout.FlexibleSpace();
                GUILayout.Label(script.username);
                GUILayout.EndHorizontal();
            }
            if (!string.IsNullOrWhiteSpace(script.email))
            {
                GUILayout.BeginHorizontal("Box");
                GUILayout.Label("Email");
                GUILayout.FlexibleSpace();
                GUILayout.Label(script.email);
                GUILayout.EndHorizontal();
            }
            if (!string.IsNullOrWhiteSpace(script.permissions))
            {
                GUILayout.BeginVertical("Box");
                GUILayout.Label("Permissions");
                scrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.Height(Mathf.Infinity), GUILayout.Height(250));
                GUILayout.Label(script.permissions);
                GUILayout.EndScrollView();
                GUILayout.EndVertical();
            }
            else
            {
                GUILayout.Label("Permissions granted appear in Play mode.");
            }
            if (script.mqttExpires > 0)
            {
                GUIStyle style = new GUIStyle(GUI.skin.label);
                style.richText = true;
                DateTimeOffset dateTimeOffSet = DateTimeOffset.FromUnixTimeSeconds(script.mqttExpires);
                TimeSpan duration = dateTimeOffSet.DateTime.Subtract(DateTime.Now.ToUniversalTime());
                GUILayout.Label($"Expires in {ArenaUnity.TimeSpanToString(duration)}", style);
            }
            EditorGUILayout.LabelField("Additional Rights", EditorStyles.boldLabel);
            script.requestRemoteRenderRights = EditorGUILayout.Toggle("Request Remote Render Host Rights", script.requestRemoteRenderRights);
            script.requestEnvironmentRights = EditorGUILayout.Toggle("Request Environment Host Rights", script.requestEnvironmentRights);
        }
    }
}
