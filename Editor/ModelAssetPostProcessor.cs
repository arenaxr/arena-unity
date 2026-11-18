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
                // The old-format asset is only of use to the legacy Editor-only path, so gate it
                // on that path being active as well as on the library being installed. Without
                // the second half of this condition, installing wu.yize.gsplat leaves every
                // splat arriving over MQTT parsed three times in the Editor: once by the
                // dependency's own ScriptedImporter, once here into a nesnausk asset that
                // nothing on the new path reads, and once at runtime by the component. Only the
                // middle one is ours to drop.
#if LIB_GAUSSIAN_SPLATTING && (!LIB_GSPLAT || ARENA_SPLAT_LEGACY)
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
