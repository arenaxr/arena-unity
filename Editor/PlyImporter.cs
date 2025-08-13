using UnityEngine;
using UnityEditor.AssetImporters;

namespace ArenaUnity.Editor
{
    [ScriptedImporter(1, new[] { "ply", "spz" })]
    public sealed class PlyImporter : ScriptedImporter
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
