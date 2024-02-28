/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2023, Carnegie Mellon University. All rights reserved.
 */

using UnityEditor;
using UnityEngine;

namespace ArenaUnity
{
#if UNITY_EDITOR
    public class ArenaGltfExportAdvancedWindow : EditorWindow
    {
        private MenuCommand menuCommand;

        internal void Init(MenuCommand menuCommand)
        {
            // this.menuCommand = menuCommand;
        }

        void OnGUI()
        {
            ArenaGltfExportAdvanced.ComponentMask = EditorGUILayout.IntField(
                "ComponentMask", ArenaGltfExportAdvanced.ComponentMask);
            ArenaGltfExportAdvanced.Compression = EditorGUILayout.TextField(
                "Compression", ArenaGltfExportAdvanced.Compression);
            ArenaGltfExportAdvanced.Deterministic = EditorGUILayout.Toggle(
                "Deterministic", ArenaGltfExportAdvanced.Deterministic);

            GUILayout.FlexibleSpace();

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUI.backgroundColor = Color.red;

            if (GUILayout.Button("Reset", GUILayout.Width(100), GUILayout.Height(30)))
            {
                ArenaGltfExportAdvanced.ComponentMask = ArenaGltfExportAdvanced.defComponentMask;
                ArenaGltfExportAdvanced.Compression = ArenaGltfExportAdvanced.defCompression;
                ArenaGltfExportAdvanced.Deterministic = ArenaGltfExportAdvanced.defDeterministic;
            }
            EditorGUILayout.EndHorizontal();
        }
    }
#endif
}
