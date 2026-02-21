/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2023, Carnegie Mellon University. All rights reserved.
 */

using System;
using System.Collections;
using System.Linq;
using Newtonsoft.Json.Linq;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
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
        }

        private static void CheckVersionStatus()
        {
            if (Version.TryParse(InstalledVersion, out var local) && Version.TryParse(LatestVersion, out var latest))
            {
                IsNewVersionAvailable = local < latest;
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
