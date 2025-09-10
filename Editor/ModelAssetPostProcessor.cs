using System.IO;
using UnityEditor;

namespace ArenaUnity.Editor
{
    public class ModelAssetPostProcessor : AssetPostprocessor
    {
        void OnPreprocessModel()
        {
            // TODO (mwfarb): might only be needed for .mtl import of .obj models
            var importSettingsMissing = assetImporter.importSettingsMissing;
            if (!importSettingsMissing)
                return; // Asset imported already, do not process.

            var modelImporter = assetImporter as ModelImporter;
            modelImporter.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
            modelImporter.materialLocation = ModelImporterMaterialLocation.External;
            modelImporter.materialSearch = ModelImporterMaterialSearch.RecursiveUp;
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
                    case ".pcd":
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
