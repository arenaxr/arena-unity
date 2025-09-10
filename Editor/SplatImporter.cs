using UnityEngine;
using UnityEditor.AssetImporters;

namespace ArenaUnity.Editor
{
    [ScriptedImporter(1, new[] { "ply", "spz", "splat", "pcd" })]
    public sealed class SplatImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext context)
        {
            //var data = ScriptableObject.CreateInstance<GaussianSplatAsset>();
            //data.name = Path.GetFileNameWithoutExtension(context.assetPath);
            GameObject myObject = new GameObject(context.assetPath);
            context.AddObjectToAsset("main", myObject);
            //context.AddObjectToAsset("data", data);
            context.SetMainObject(myObject);
        }

    }
}
