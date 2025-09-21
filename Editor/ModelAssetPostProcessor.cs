using System.IO;
using UnityEditor;
using UnityEngine;

namespace ArenaUnity.Editor
{
    public class ModelAssetPostProcessor : AssetPostprocessor
    {
        public Material OnAssignMaterialModel(Material material, Renderer renderer)
        {
            ModelImporter importer = (ModelImporter)assetImporter;
            importer.AddRemap(new AssetImporter.SourceAssetIdentifier(material), (Material)AssetDatabase.LoadAssetAtPath("Assets/ProfilingData/Materials/material.2.mat", typeof(Material)));
            return null;
        }

        void OnPreprocessModel()
        {
            // TODO (mwfarb): might only be needed for .mtl import of .obj models
            var importSettingsMissing = assetImporter.importSettingsMissing;
            if (!importSettingsMissing)
                return; // Asset imported already, do not process.

            var modelImporter = assetImporter as ModelImporter;
            modelImporter.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
            modelImporter.useSRGBMaterialColor = true;
            modelImporter.materialLocation = ModelImporterMaterialLocation.InPrefab;
            modelImporter.SearchAndRemapMaterials(
                ModelImporterMaterialName.BasedOnMaterialName,
                ModelImporterMaterialSearch.Local);
            modelImporter.SaveAndReimport();
        }

        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            // Check if specific assets were imported
            foreach (string asset in importedAssets)
            {
#if LIB_GAUSSIAN_SPLATTING
                switch (Path.GetExtension(asset)?.ToLower())
                {
                    case ".ply":
                    case ".spz":
                    case ".splat":
                        var splat = new SplatAssetCreator();
                        var gsa = splat.ImportSplatData(asset);
                        AssetDatabase.SaveAssets();
                        break;
                };
#endif
            }
        }
    }
}
