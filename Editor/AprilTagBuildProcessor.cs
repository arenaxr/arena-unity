#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace ArenaUnity.Editor
{
    /// <summary>
    /// Ensures AprilTag native plugins are completely excluded from the build if ARFoundation is missing.
    /// Works by temporarily disabling their platform settings before the build, and restoring them after.
    /// </summary>
    public class AprilTagBuildProcessor : IPreprocessBuildWithReport, IPostprocessBuildWithReport
    {
        public int callbackOrder => 0;

        // Keep track of which plugins we disabled so we can restore them
        private List<PluginImporter> _disabledPlugins = new List<PluginImporter>();

        public void OnPreprocessBuild(BuildReport report)
        {
#if !HAS_AR_FOUNDATION
            Debug.Log("[ArenaAprilTag] HAS_AR_FOUNDATION is missing. Disabling AprilTag native plugins for this build to save space.");

            // Find all AprilTag native plugins
            var pluginImporters = PluginImporter.GetAllImporters()
                .Where(p => p.assetPath.Contains("AprilTag/Plugin") && p.isNativePlugin);

            foreach (var plugin in pluginImporters)
            {
                if (plugin.GetCompatibleWithPlatform(report.summary.platform))
                {
                    plugin.SetCompatibleWithPlatform(report.summary.platform, false);
                    _disabledPlugins.Add(plugin);
                }
            }

            // Save the asset database so the build uses the disabled state
            if (_disabledPlugins.Count > 0)
            {
                AssetDatabase.SaveAssets();
            }
#endif
        }

        public void OnPostprocessBuild(BuildReport report)
        {
#if !HAS_AR_FOUNDATION
            if (_disabledPlugins.Count > 0)
            {
                Debug.Log($"[ArenaAprilTag] Build complete. Restoring {_disabledPlugins.Count} AprilTag plugins.");
                
                foreach (var plugin in _disabledPlugins)
                {
                    plugin.SetCompatibleWithPlatform(report.summary.platform, true);
                }

                _disabledPlugins.Clear();
                AssetDatabase.SaveAssets();
            }
#endif
        }
    }
}
#endif
