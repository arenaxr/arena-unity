/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2023, Carnegie Mellon University. All rights reserved.
 */

using System.Collections;
using System.IO;
using System.Text.RegularExpressions;
using ArenaUnity.Schemas;
using GLTFast;
using GLTFast.Export;
using GLTFast.Logging;
using Newtonsoft.Json;
using UnityEngine;

namespace ArenaUnity
{
    /// <summary>
    /// GLTF export and file upload to the ARENA filestore.
    /// </summary>
    public partial class ArenaClientScene
    {
        public void ExportGLTFBinaryStream(string name, GameObject[] gameObjects, ExportSettings exportSettings = null, GameObjectExportSettings goeSettings = null)
        {
            exportSettings ??= new ExportSettings { };
            goeSettings ??= new GameObjectExportSettings { };

            // ARENA upload only supports GLB formats ATM
            exportSettings.Format = GltfFormat.Binary;

            bool success = true;
            foreach (var go in gameObjects)
            {
                if (go.GetComponents<ArenaObject>().Length > 0)
                {
                    success = false;
                    Debug.LogWarning($"GLTF export ignored for existing ArenaObject component {name}.");
                }
                if (go.GetComponents<ArenaCamera>().Length > 0)
                {
                    success = false;
                    Debug.LogWarning($"GLTF export ignored for existing ArenaCamera component {name}.");
                }
                if (go.GetComponents<ArenaClientScene>().Length > 0)
                {
                    success = false;
                    Debug.LogWarning($"GLTF export ignored for existing ArenaClientScene component {name}.");
                }
            }
            if (success)
            {
                StartCoroutine(ExportGLTF(name, gameObjects, exportSettings, goeSettings));
            }
        }

        private IEnumerator ExportGLTF(string name, GameObject[] gameObjects, ExportSettings exportSettings, GameObjectExportSettings goeSettings)
        {
            CoroutineWithData cd;

            // request FS login if this is the first time.
            if (fsToken == null)
            {
                cd = new CoroutineWithData(this, GetFSLoginForUser());
                yield return cd.coroutine;
                if (!isCrdSuccess(cd.result))
                {
                    Debug.LogError($"Filestore login failed!");
                    yield break;
                }
            }

            // TODO (mwfarb): find better way to export with local position/rotation
            // determine if we should update the local position
            var rootObjPos = gameObjects[0].transform.position;

            // ATM glTFast exports models with world origin, so we do a trick for now,
            // by moving the model to the desired export position, export, and then return it.

            // move single model to desired export translation
            gameObjects[0].transform.position = Vector3.zero;

            // export gltf to stream
            var export = new GameObjectExport(exportSettings, goeSettings, logger: new ConsoleLogger());
            export.AddScene(gameObjects, name);

            MemoryStream stream = new MemoryStream();
            var exportTask = export.SaveToStreamAndDispose(stream);
            yield return new WaitUntil(() => exportTask.IsCompleted);
            if (!exportTask.Result)
            {
                Debug.LogError($"GLTF export to stream failed!");
                yield break;
            }

            // return single model to original export translation
            gameObjects[0].transform.position = rootObjPos;

            var destFilePath = $"{name}.glb";
            byte[] fileBuffer = stream.GetBuffer();

            // send stream to filestore
            cd = new CoroutineWithData(this, UploadStoreFile(fileBuffer, destFilePath));
            yield return cd.coroutine;
            var storeExtPath = (string)cd.result;
            if (string.IsNullOrEmpty(storeExtPath)) yield break;

            // send scene object metadata to MQTT
            ArenaMessageJson msg = new ArenaMessageJson
            {
                object_id = name,
                action = "create",
                type = "object",
                persist = true,
                data = new ArenaDataObjectJson
                {
                    object_type = "gltf-model",
                    Url = storeExtPath,
                    Position = ArenaUnity.ToArenaPosition(rootObjPos),
                    Rotation = ArenaUnity.ToArenaRotationQuat(
                        ArenaUnity.GltfToUnityRotationQuat(Quaternion.identity)),
                }
            };
            string payload = JsonConvert.SerializeObject(msg);
            PublishObject(msg.object_id, payload);
            yield return true;
        }

        /// <summary>
        /// Upload a file to the filestore using the user's Google account.
        /// </summary>
        /// <param name="srcFilePath">Local path to the file to upload.</param>
        /// <param name="destFilePath">Destination file path, can include dirs. Defaults to filename from srcFilePath.</param>
        /// <returns></returns>
        public IEnumerator UploadStoreFile(string srcFilePath, string destFilePath = null)
        {
            if (destFilePath == null)
                destFilePath = Path.GetFileName(srcFilePath);
            byte[] fileBuffer = File.ReadAllBytes(srcFilePath);
            var cd = new CoroutineWithData(this, UploadStoreFile(fileBuffer, destFilePath));
            yield return cd.result;
        }

        /// <summary>
        /// Upload a file to the filestore using the user's Google account.
        /// </summary>
        /// <param name="fileBuffer">Binary buffer of file.</param>
        /// <param name="destFilePath">Destination file path, can include dirs.</param>
        /// <returns></returns>
        public IEnumerator UploadStoreFile(byte[] fileBuffer, string destFilePath)
        {
            // confirm user google account is loaded
            if (!ConfirmGoogleAuth())
            {
                // TODO(mwfarb): Add note when headless auth options are available.
                Debug.LogError($"Google auth is required. Remove manual .arena_mqtt_token.");
                yield break;
            }
            var safeFilename = Regex.Replace(destFilePath, @"/(\W+)/gi", "-");
            var storeResPrefix = authState.is_staff ? $"users/{mqttUserName}/" : "";
            var userFilePath = $"scenes/{sceneName}/{safeFilename}";
            var storeResPath = $"{storeResPrefix}{userFilePath}";
            var storeExtPath = $"store/users/{mqttUserName}/{userFilePath}";

            string uploadUrl = $"https://{hostAddress}/storemng/api/resources/{storeResPath}?override=true";
            var cd = new CoroutineWithData(this, HttpUploadFSRaw(uploadUrl, fileBuffer));
            yield return cd.coroutine;
            if (!isCrdSuccess(cd.result))
            {
                Debug.LogError($"Filestore file upload failed!");
                yield break;
            }
            var url = $"https://{hostAddress}/{storeExtPath}";
            Debug.Log($"Filestore file uploaded to {url}");
            yield return url;
        }
    }
}
