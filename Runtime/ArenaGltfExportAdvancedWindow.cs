/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2023, Carnegie Mellon University. All rights reserved.
 */

using GLTFast;
using GLTFast.Export;
using UnityEditor;
using UnityEngine;

namespace ArenaUnity
{
#if UNITY_EDITOR
    public class ArenaGltfExportAdvancedWindow : EditorWindow
    {
        private MenuCommand menuCommand;
        private string objectName;
        private GameObject[] gameObjects;

        internal void Init(string name, GameObject[] gameObjects, MenuCommand menuCommand)
        {
            this.objectName = name;
            this.gameObjects = gameObjects;
            this.menuCommand = menuCommand;
        }

        void OnGUI()
        {
            objectName = EditorGUILayout.TextField("Export Object Name", objectName);

            ArenaGltfExportAdvanced.Deterministic = EditorGUILayout.Toggle(
                "Deterministic", ArenaGltfExportAdvanced.Deterministic);

            ArenaGltfExportAdvanced.ComponentMask = (int)(ComponentType)EditorGUILayout.EnumFlagsField(
                "Export Components", (ComponentType)ArenaGltfExportAdvanced.ComponentMask);

            ArenaGltfExportAdvanced.Compression = (int)(Compression)EditorGUILayout.EnumPopup(
                "Compression", (Compression)ArenaGltfExportAdvanced.Compression);

            ArenaGltfExportAdvanced.LightIntensityFactor = EditorGUILayout.FloatField(
                "Light Intensity Factor", ArenaGltfExportAdvanced.LightIntensityFactor);


            GUILayout.FlexibleSpace();

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Cancel", GUILayout.Width(100), GUILayout.Height(30)))
                Close();

            GUI.backgroundColor = Color.red;
            if (GUILayout.Button("Reset", GUILayout.Width(100), GUILayout.Height(30)))
            {
                ArenaGltfExportAdvanced.ComponentMask = (int)ArenaGltfExportAdvanced.defES.ComponentMask;
                ArenaGltfExportAdvanced.Deterministic = ArenaGltfExportAdvanced.defES.Deterministic;
                ArenaGltfExportAdvanced.Compression = (int)ArenaGltfExportAdvanced.defES.Compression;
                ArenaGltfExportAdvanced.LightIntensityFactor = ArenaGltfExportAdvanced.defES.LightIntensityFactor;
                // TODO (mwfarb) ArenaGltfExportAdvanced.DracoSettings = ArenaGltfExportAdvanced.defES.DracoSettings;
            }

            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("ARENA Export", GUILayout.Width(100), GUILayout.Height(30)))
            {
                ArenaClientScene.Instance.ExportGLTFBinaryStream(objectName, gameObjects, new ExportSettings
                {
                    ComponentMask = (ComponentType)ArenaGltfExportAdvanced.ComponentMask,
                    Deterministic = ArenaGltfExportAdvanced.Deterministic,
                    Compression = (Compression)ArenaGltfExportAdvanced.Compression,
                    LightIntensityFactor = ArenaGltfExportAdvanced.LightIntensityFactor,
                });
                Close();
            }

            EditorGUILayout.EndHorizontal();
        }
    }
#endif
}
