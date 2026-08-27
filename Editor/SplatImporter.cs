// wu.yize.gsplat ships Gsplat.Editor.GsplatImporter, a ScriptedImporter claiming "ply" and
// "spz" at the same version/priority as this one, in a second Editor assembly. Unity does not
// allow two importers to claim an extension at equal priority, so this importer -- which exists
// only to serve the legacy Editor-only splat path, and whose asset lines are commented out --
// compiles only when that path is the active one. Same define scheme as the branches in
// Runtime/Components/ArenaWireGaussianSplatting.cs.
#if !LIB_GSPLAT || ARENA_SPLAT_LEGACY

using UnityEngine;
using UnityEditor.AssetImporters;

namespace ArenaUnity.Editor
{
    [ScriptedImporter(1, new[] { "ply", "spz", "splat" })]
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

#endif
