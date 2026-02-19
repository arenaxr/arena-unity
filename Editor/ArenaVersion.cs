/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2023, Carnegie Mellon University. All rights reserved.
 */

using System;
using System.Collections;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using TMPro;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.Networking;

namespace ArenaUnity.Editor
{
    /// <summary>
    /// Editor class to check installed package versions against new releases.
    /// </summary>
    [InitializeOnLoad]
    internal class ArenaVersion
    {
        const string unityPackageName = "io.conix.arena.unity";
        const string githubOrg = "arenaxr";
        const string githubName = "arena-unity";
        private static string gitLatestUrl = $"https://api.github.com/repos/{githubOrg}/{githubName}/releases/latest";
        private static ListRequest _listRequest;
        private static bool checkGithub = false;
        static AddAndRemoveRequest packagesRequest;

        // first version to avoid hitting Github's rate-limit
        private const string GH_RATE_LIMIT_VERSION = "0.0.4";

        internal static string InstalledVersion = "0.0.0";
        internal static string LatestVersion = GH_RATE_LIMIT_VERSION;
        internal static bool IsNewVersionAvailable = false;

        // Use project-specific keys to ensure settings are isolated per project
        private static string GetKey(string suffix) => $"ArenaUnity_{Application.productName}_{suffix}";
        private static string KeyCheckTime => GetKey("GitVersionCheckTime");
        private static string KeyLatestVersion => GetKey("GitVersionLatest");

        static ArenaVersion()
        {
            // Get local version immediately
            InstalledVersion = LocalVersion();

            long time = (long)PlayerPrefs.GetFloat(KeyCheckTime, 0);
            TimeSpan t = DateTime.UtcNow - new DateTime(time);
            // only check github every 24 hours to avoid hitting api rate limit
            if (t.TotalDays > 1f) checkGithub = true;

            // Load last known latest version
            LatestVersion = PlayerPrefs.GetString(KeyLatestVersion, GH_RATE_LIMIT_VERSION).Trim('v');
            CheckVersionStatus();

            _listRequest = Client.List();
            EditorApplication.update += OnUpdate;

            UpdateMissingPackages();
            UpdateMissingPlayerSettings();
            UpdateMissingAssets();
        }

        private static void CheckVersionStatus()
        {
            if (Version.TryParse(InstalledVersion, out var local) && Version.TryParse(LatestVersion, out var latest))
            {
                IsNewVersionAvailable = local < latest;
            }
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

        private static void HandleListRequestCompletion()
        {
            if (_listRequest.Status == StatusCode.Success)
            {
                var package = _listRequest.Result.FirstOrDefault(p => p.name == unityPackageName);
                if (package != null)
                {
                    InstalledVersion = package.version.Trim('v');

                    if (Version.TryParse(InstalledVersion, out var local))
                    {
                        // Check unity package manager/github automated version manager
                        if (Version.TryParse(package.versions.latest.Trim('v'), out var latest))
                        {
                            LatestVersion = package.versions.latest.Trim('v');
                            if (local < latest)
                            {
                                Debug.LogWarning(UpgradeMessage(local, latest));
                            }
                        }
                        else
                        {
                            // Minimal, check last saved version check
                            LatestVersion = PlayerPrefs.GetString(KeyLatestVersion, GH_RATE_LIMIT_VERSION).Trim('v');
                            if (Version.TryParse(LatestVersion, out latest))
                            {
                                if (local < latest)
                                {
                                    Debug.LogWarning(UpgradeMessage(local, latest));
                                }
                            }
                        }
                        // Check github directly next
                        if (checkGithub)
                        {
                            EditorCoroutineUtility.StartCoroutineOwnerless(CheckGithubVersion(local));
                        }
                        else
                        {
                            // If not checking github, still re-evaluate status with loaded prefs
                            CheckVersionStatus();
                        }
                        Debug.Log(CurrentMessage(local));
                    }
                }
                _listRequest = null;
            }
        }

        static IEnumerator CheckGithubVersion(Version local)
        {
            UnityWebRequest www = UnityWebRequest.Get(gitLatestUrl);
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"{www.error}: {www.url}");
            }
            else
            {
                var git = JObject.Parse(www.downloadHandler.text);
                if (git != null)
                {
                    if (Version.TryParse(git["tag_name"].ToString().Trim('v'), out var latest))
                    {
                        LatestVersion = latest.ToString();
                        PlayerPrefs.SetString(KeyLatestVersion, LatestVersion);

                        CheckVersionStatus(); // Update status flag

                        if (local < latest)
                        {
                            Debug.LogWarning(UpgradeMessage(local, latest));
                        }
                    }
                }
            }
            PlayerPrefs.SetFloat(KeyCheckTime, DateTimeOffset.Now.Ticks);
            PlayerPrefs.Save();
        }

        private static string UpgradeMessage(Version local, Version latest)
        {
            return $"ARENA for Unity Package version {latest} is available, however {local} is installed.\nUpdate to https://github.com/{githubOrg}/{githubName}.git#latest";
        }

        private static string CurrentMessage(Version local)
        {
            return $"ARENA for Unity Package version {local} is installed.\nLatest: https://github.com/{githubOrg}/{githubName}/releases";
        }

        private static void OnUpdate()
        {
            if (_listRequest != null && _listRequest.IsCompleted)
            {
                HandleListRequestCompletion();
            }
        }

        internal static string LocalVersion()
        {
            var package = UnityEditor.PackageManager.PackageInfo.FindForAssembly(typeof(ArenaVersion).Assembly);
            if (package != null) return package.version;
            return "0.0.0";
        }

        static bool WantsToQuit()
        {
            Debug.Log("Exiting play mode...");
            EditorApplication.ExitPlaymode();

            // Check that this instance was actually launched from a batch mode session, so that game code
            // doesn't inadvertently exit the editor during development.
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(0);
            }
            else
            {
                Debug.Log("Exiting application.");
            }
            return true;
        }

        [RuntimeInitializeOnLoadMethod]
        static void RunOnStart()
        {
            Application.wantsToQuit += WantsToQuit;
        }
    }
}
