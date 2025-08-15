using System.Globalization;
using System.IO;
using UnityEditor;

namespace ArenaUnity.Editor
{
    public class SplatAssetPostProcessor : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            // Check if specific assets were imported
            foreach (string asset in importedAssets)
            {
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
            }
        }
    }
}
