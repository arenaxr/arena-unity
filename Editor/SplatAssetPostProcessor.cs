using System;
using System.Globalization;
using GaussianSplatting.Runtime;
using UnityEditor;
using UnityEngine;

namespace ArenaUnity.Editor
{
    public class SplatAssetPostProcessor : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            // Check if specific assets were imported
            foreach (string asset in importedAssets)
            {
                if (asset.EndsWith(".splat", true, CultureInfo.InvariantCulture))
                {
                    var ply = new PlyProcessor();
                    var gsa = ply.ImportSplatData(asset);
                    AssetDatabase.SaveAssets();
                }
                else if (asset.EndsWith(".ply", true, CultureInfo.InvariantCulture))
                {
                    var ply = new PlyProcessor();
                    var gsa = ply.ImportSplatData(asset);
                    AssetDatabase.SaveAssets();
                }
                else if (asset.EndsWith(".spz", true, CultureInfo.InvariantCulture))
                {
                    var ply = new PlyProcessor();
                    var gsa = ply.ImportSplatData(asset);
                    AssetDatabase.SaveAssets();
                }
            }
        }
    }
}
