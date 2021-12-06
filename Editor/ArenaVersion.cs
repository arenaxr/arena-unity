using System;
using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace ArenaUnity.Editor
{
    [InitializeOnLoad]
    internal class ArenaVersion
    {
        private static ListRequest _listRequest;

        static ArenaVersion()
        {
            _listRequest = UnityEditor.PackageManager.Client.List();
            EditorApplication.update += OnUpdate;
        }

        private static void HandleListRequestCompletion()
        {
            const string packageName = "io.conix.arena.unity";

            if (_listRequest.Status == StatusCode.Success)
            {
                var package = _listRequest.Result.FirstOrDefault(p => p.name == packageName);
                if (package != null
                    && Version.TryParse(package.version, out var packageVersion)
                    && Version.TryParse(package.versions.latest, out var latestVersion)
                    && packageVersion < latestVersion
                    )
                {
                    Debug.LogWarning($"ARENA for Unity Package version {package.versions.latest} is available to install, you are running version {package.version}.");
                }
            }

            _listRequest = null;
        }

        private static void OnUpdate()
        {
            if (_listRequest != null && _listRequest.IsCompleted)
            {
                HandleListRequestCompletion();
            }
        }

        internal static string PackageVersion()
        {
            var package = UnityEditor.PackageManager.PackageInfo.FindForAssembly(typeof(ArenaVersion).Assembly);
            return package.version;
        }
    }
}
