/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2023, Carnegie Mellon University. All rights reserved.
 */

using System.Collections;
using GLTFast;
using GLTFast.Export;
using UnityEditor;
using UnityEngine;

namespace ArenaUnity.Editor
{
    public class ArenaGltfExportAdvancedWindow : EditorWindow
    {
        private string objectName;
        private GameObject[] gameObjects;
        private string[] layerNames;

        internal void Init(string name, GameObject[] gameObjects)
        {
            this.objectName = name;
            this.gameObjects = gameObjects;

            // generate user's list of layers
            ArrayList layers = new ArrayList();
            for (int i = 0; i <= 31; i++)
            {
                var layer = LayerMask.LayerToName(i);
                layers.Add($"{i + 1}: {layer}");
            }
            layerNames = (string[])layers.ToArray(typeof(string));
        }

        void OnGUI()
        {
            objectName = EditorGUILayout.TextField("Export Object Name", objectName);

            GUILayout.Space(10f);
            GUILayout.Label("Export Settings", EditorStyles.boldLabel);

            ArenaGltfExportAdvanced.Deterministic = EditorGUILayout.Toggle(
                "Deterministic", ArenaGltfExportAdvanced.Deterministic);

            ArenaGltfExportAdvanced.ComponentMask = (int)(ComponentType)EditorGUILayout.EnumFlagsField(
                "Components Exported", (ComponentType)ArenaGltfExportAdvanced.ComponentMask);

            ArenaGltfExportAdvanced.LightIntensityFactor = EditorGUILayout.FloatField(
                "Light Intensity Factor", ArenaGltfExportAdvanced.LightIntensityFactor);

            ArenaGltfExportAdvanced.Compression = (int)(Compression)EditorGUILayout.EnumPopup(
                "Compression", (Compression)ArenaGltfExportAdvanced.Compression);
            if ((Compression)ArenaGltfExportAdvanced.Compression == Compression.Draco)
            {
                GUILayout.Space(10f);
                GUILayout.Label("Draco Compression", EditorStyles.boldLabel);

                ArenaGltfExportAdvanced.DracoColorQuantization = EditorGUILayout.IntField(
                    "Color Quantization", ArenaGltfExportAdvanced.DracoColorQuantization);
                ArenaGltfExportAdvanced.DracoDecodingSpeed = EditorGUILayout.IntField(
                    "Decoding Speed", ArenaGltfExportAdvanced.DracoDecodingSpeed);
                ArenaGltfExportAdvanced.DracoEncodingSpeed = EditorGUILayout.IntField(
                    "Encoding Speed", ArenaGltfExportAdvanced.DracoEncodingSpeed);
                ArenaGltfExportAdvanced.DracoNormalQuantization = EditorGUILayout.IntField(
                    "Normal Quantization", ArenaGltfExportAdvanced.DracoNormalQuantization);
                ArenaGltfExportAdvanced.DracoPositionQuantization = EditorGUILayout.IntField(
                    "Position Quantization", ArenaGltfExportAdvanced.DracoPositionQuantization);
                ArenaGltfExportAdvanced.DracoTexCoordQuantization = EditorGUILayout.IntField(
                    "Tex Coord Quantization", ArenaGltfExportAdvanced.DracoTexCoordQuantization);
            }

            GUILayout.Space(10f);
            GUILayout.Label("Game Object Export Settings", EditorStyles.boldLabel);

            ArenaGltfExportAdvanced.DisabledComponents = EditorGUILayout.Toggle(
                "Disabled Components", ArenaGltfExportAdvanced.DisabledComponents);

            ArenaGltfExportAdvanced.OnlyActiveInHierarchy = EditorGUILayout.Toggle(
                "Only Active In Hierarchy", ArenaGltfExportAdvanced.OnlyActiveInHierarchy);

            ArenaGltfExportAdvanced.LayerMask = (LayerMask)EditorGUILayout.MaskField(
                "Layers Exported", ArenaGltfExportAdvanced.LayerMask, layerNames);

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
                ArenaGltfExportAdvanced.LightIntensityFactor = ArenaGltfExportAdvanced.defES.LightIntensityFactor;
                ArenaGltfExportAdvanced.Compression = (int)ArenaGltfExportAdvanced.defES.Compression;
                ArenaGltfExportAdvanced.DracoSettings = ArenaGltfExportAdvanced.defES.DracoSettings != null;

                ArenaGltfExportAdvanced.DracoColorQuantization = ArenaGltfExportAdvanced.defDES.colorQuantization;
                ArenaGltfExportAdvanced.DracoDecodingSpeed = ArenaGltfExportAdvanced.defDES.decodingSpeed;
                ArenaGltfExportAdvanced.DracoEncodingSpeed = ArenaGltfExportAdvanced.defDES.encodingSpeed;
                ArenaGltfExportAdvanced.DracoNormalQuantization = ArenaGltfExportAdvanced.defDES.normalQuantization;
                ArenaGltfExportAdvanced.DracoPositionQuantization = ArenaGltfExportAdvanced.defDES.positionQuantization;
                ArenaGltfExportAdvanced.DracoTexCoordQuantization = ArenaGltfExportAdvanced.defDES.texCoordQuantization;

                ArenaGltfExportAdvanced.DisabledComponents = ArenaGltfExportAdvanced.defGOES.DisabledComponents;
                ArenaGltfExportAdvanced.OnlyActiveInHierarchy = ArenaGltfExportAdvanced.defGOES.OnlyActiveInHierarchy;
                ArenaGltfExportAdvanced.LayerMask = (int)ArenaGltfExportAdvanced.defGOES.LayerMask;
            }

            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("ARENA Export", GUILayout.Width(100), GUILayout.Height(30)))
            {
                ExportSettings exportSettings = new ExportSettings
                {
                    ComponentMask = (ComponentType)ArenaGltfExportAdvanced.ComponentMask,
                    Deterministic = ArenaGltfExportAdvanced.Deterministic,
                    Compression = (Compression)ArenaGltfExportAdvanced.Compression,
                    LightIntensityFactor = ArenaGltfExportAdvanced.LightIntensityFactor,
                };
                if (ArenaGltfExportAdvanced.DracoSettings)
                {
                    exportSettings.DracoSettings = new DracoExportSettings
                    {
                        colorQuantization = ArenaGltfExportAdvanced.DracoColorQuantization,
                        decodingSpeed = ArenaGltfExportAdvanced.DracoDecodingSpeed,
                        encodingSpeed = ArenaGltfExportAdvanced.DracoEncodingSpeed,
                        normalQuantization = ArenaGltfExportAdvanced.DracoNormalQuantization,
                        positionQuantization = ArenaGltfExportAdvanced.DracoPositionQuantization,
                        texCoordQuantization = ArenaGltfExportAdvanced.DracoTexCoordQuantization,
                    };
                }
                GameObjectExportSettings goeSettings = new GameObjectExportSettings
                {
                    DisabledComponents = ArenaGltfExportAdvanced.DisabledComponents,
                    OnlyActiveInHierarchy = ArenaGltfExportAdvanced.OnlyActiveInHierarchy,
                    LayerMask = (LayerMask)ArenaGltfExportAdvanced.LayerMask,
                };
                ArenaClientScene.Instance.ExportGLTFBinaryStream(objectName, gameObjects, exportSettings, goeSettings);
                Close();
            }

            EditorGUILayout.EndHorizontal();
        }
    }
}
