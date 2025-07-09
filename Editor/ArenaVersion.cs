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

        static ArenaVersion()
        {
            long time = (long)PlayerPrefs.GetFloat("GitVersionCheckTime", 0);
            TimeSpan t = DateTime.UtcNow - new DateTime(time);
            // only check github every 24 hours to avoid hitting api rate limit
            if (t.TotalDays > 1f) checkGithub = true;

            _listRequest = Client.List();
            EditorApplication.update += OnUpdate;

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
                },
                {
                  'name': 'registry.npmjs.com',
                  'url': 'https://registry.npmjs.com',
                  'scopes': [ 'jp.keijiro' ]
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

            // add required packages from scoped registries
            AddPackages(new string[]{
                "org.nesnausk.gaussian-splatting@1.1.1",
                "jp.keijiro.pcx@1.0.1",
            });
        }

        private static void UpdateMissingPlayerSettings()
        {
            // add scripting define symbols required
            string[] definesReq = { "SSL", "LIB_GAUSSIAN_SPLATTING" };
            NamedBuildTarget buildTarget = CurrentNamedBuildTarget;
            PlayerSettings.GetScriptingDefineSymbols(buildTarget, out string[] defines);
            PlayerSettings.SetScriptingDefineSymbols(buildTarget, defines.Union(definesReq).ToArray());
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

        static void AddPackages(string[] packagesToAdd)
        {
            // TODO Only add a package to the project if it's missing
            packagesRequest = Client.AddAndRemove(packagesToAdd);
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
                if (package != null && Version.TryParse(package.version.Trim('v'), out var local))
                {
                    // Check unity package manager/github automated version manager
                    if (Version.TryParse(package.versions.latest.Trim('v'), out var latest))
                    {
                        if (local < latest)
                        {
                            Debug.LogWarning(UpgradeMessage(local, latest));
                        }
                    }
                    else
                    {
                        // Minimal, check last saved version check
                        string latest_sav = PlayerPrefs.GetString("GitVersionLatest", GH_RATE_LIMIT_VERSION).Trim('v');
                        if (Version.TryParse(latest_sav, out latest))
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
                    Debug.Log(CurrentMessage(local));
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

                //GitReleasesLatestJson git = JsonConvert.DeserializeObject<GitReleasesLatestJson>(www.downloadHandler.text);
                var git = JObject.Parse(www.downloadHandler.text);
                if (git != null)
                {
                    if (Version.TryParse(git["tag_name"].ToString().Trim('v'), out var latest))
                    {
                        PlayerPrefs.SetString("GitVersionLatest", latest.ToString());
                        if (local < latest)
                        {
                            Debug.LogWarning(UpgradeMessage(local, latest));
                        }
                    }
                }
            }
            PlayerPrefs.SetFloat("GitVersionCheckTime", DateTimeOffset.Now.Ticks);
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
            return package.version;
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
