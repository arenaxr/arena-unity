/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2023, Carnegie Mellon University. All rights reserved.
 */

using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace ArenaUnity.Editor
{
    /// <summary>
    /// Editor class to update the project package manager with dependencies.
    /// </summary>
    [InitializeOnLoad]
    internal class ArenaDependencies
    {
        static AddAndRemoveRequest packagesRequest;

        static ArenaDependencies()
        {
            UpdateMissingPackages();
            UpdateMissingPlayerSettings();
            UpdateMissingAssets();
        }

        private static void UpdateMissingPackages()
        {
            // update project manifest with required scoped registries if needed
            string projManifestPath = Path.Combine("Packages", "manifest.json");
            JObject joProjManifestIn = JObject.Parse(File.ReadAllText(projManifestPath));

            //{
            //    'name': 'Unity NuGet',
            //    'url': 'https://unitynuget-registry.openupm.com',
            //    'scopes': [
            //        'org.nuget'
            //  ]
            //},
            string jsonScopedRegReq = @"{'scopedRegistries': [
                {
                    'name': 'package.openupm.com',
                    'url': 'https://package.openupm.com',
                    'scopes': [
                        'org.nesnausk.gaussian-splatting'
                    ]
                }
            ]}";
            JObject joProjManifestReq = JObject.Parse(jsonScopedRegReq);

            JArray jaRegOld = (JArray)joProjManifestIn["scopedRegistries"];
            if (jaRegOld == null) jaRegOld = new JArray();

            JArray jaRegNew = (JArray)joProjManifestReq["scopedRegistries"];

            JArray jaRegMerge = new JArray();
            foreach (JToken regOld in jaRegOld)
            {
                jaRegMerge.Add(regOld);
            }
            foreach (JToken regNew in jaRegNew)
            {
                string urlNew = (string)regNew["url"];
                JToken regOld = jaRegMerge.SelectToken($"$.[?(@.url == '{urlNew}')]");
                if (regOld != null)
                {
                    ((JArray)regOld["scopes"]).Merge((JArray)regNew["scopes"], new JsonMergeSettings
                    {
                        MergeArrayHandling = MergeArrayHandling.Union
                    });
                }
                else
                {
                    jaRegMerge.Add(regNew);
                }
            }
            joProjManifestIn["scopedRegistries"] = jaRegMerge;

            File.WriteAllText(projManifestPath, joProjManifestIn.ToString());

            // TODO wait for scoped registry to load

#if LIB_URP && !UNITY_6000_0_OR_NEWER
            // depending on project migration, users may need to manually remove:
            // - Package Manager: org.nesnausk.gaussian-splatting
            // - Project Settings - Player: Scripted Define Symbol: LIB_GAUSSIAN_SPLATTING
            Debug.LogWarning("Gaussian Splatting in URP requires Unity 6+. Package not included: org.nesnausk.gaussian-splatting");
#else
            // add required packages from scoped registries
            UpdatePackages(new string[]{
                "org.nesnausk.gaussian-splatting@1.1.1"
            }, new string[] { });
#endif
        }

        private static void UpdateMissingPlayerSettings()
        {
            // add scripting define symbols required
            string[] definesAdd = { };
            string[] definesRm = { "SSL", "LIB_GAUSSIAN_SPLATTING" }; // some defines moved to asmdef
            NamedBuildTarget buildTarget = CurrentNamedBuildTarget;
            PlayerSettings.GetScriptingDefineSymbols(buildTarget, out string[] defines);
            string[] definesUpdate = defines.Where(s => !definesRm.Contains(s)).ToArray();
            PlayerSettings.SetScriptingDefineSymbols(buildTarget, definesUpdate.Union(definesAdd).ToArray());
        }

        private static void UpdateMissingAssets()
        {
            // Check if "TextMesh Pro" folder is present in project
            string tmpAssetsPath = Path.Combine("Assets", "TextMesh Pro");
            if (!Directory.Exists(tmpAssetsPath))
            {
                // Import TMP Essentials at minimum to prevent popup when text objects arrive on the wire
                TMP_PackageResourceImporter.ImportResources(true, false, false);
            }
        }

        public static NamedBuildTarget CurrentNamedBuildTarget
        {
            get
            {
#if UNITY_SERVER
                return NamedBuildTarget.Server;
#else
                BuildTarget buildTarget = EditorUserBuildSettings.activeBuildTarget;
                BuildTargetGroup targetGroup = BuildPipeline.GetBuildTargetGroup(buildTarget);
                NamedBuildTarget namedBuildTarget = NamedBuildTarget.FromBuildTargetGroup(targetGroup);
                return namedBuildTarget;
#endif
            }
        }

        static void UpdatePackages(string[] packagesToAdd, string[] packagesToRm)
        {
            // TODO Only add a package to the project if it's missing
            packagesRequest = Client.AddAndRemove(packagesToAdd, packagesToRm);
            EditorApplication.update += Progress;
        }

        static void Progress()
        {
            if (packagesRequest.IsCompleted)
            {
                if (packagesRequest.Status >= StatusCode.Failure)
                    Debug.LogWarning(packagesRequest.Error.message);

                EditorApplication.update -= Progress;
            }
        }
    }
}
