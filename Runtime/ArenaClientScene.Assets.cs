/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2023, Carnegie Mellon University. All rights reserved.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MimeMapping;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace ArenaUnity
{
    /// <summary>
    /// Asset downloading, saving, importing, and URL construction.
    /// </summary>
    public partial class ArenaClientScene
    {
        private static IEnumerable<string> ExtractAssetUris(object data, string[] urlTags)
        {
            List<string> tagList = new List<string>(urlTags);
            var root = (JContainer)JToken.Parse(JsonConvert.SerializeObject(data));
            var uris = root.DescendantsAndSelf().OfType<JProperty>().Where(p => tagList.Contains(p.Name)).Select(p => p.Value.Value<string>());
            return uris;
        }

        internal void RegisterAssetCallback(string msgUrl, Action callback)
        {
            Uri uri = ConstructRemoteUrl(msgUrl);
            if (uri == null) return;
            string absoluteUri = uri.AbsoluteUri;

            if (!pendingAssetCallbacks.ContainsKey(absoluteUri))
                pendingAssetCallbacks[absoluteUri] = null;
                
            pendingAssetCallbacks[absoluteUri] += callback;
        }

        internal string checkLocalAsset(string msgUrl)
        {
            Uri uri = ConstructRemoteUrl(msgUrl);
            if (uri == null) return null;
            string assetPath = ConstructLocalPath(uri);
            if (!File.Exists(assetPath)) return null;
            return assetPath;
        }

        internal Uri ConstructRemoteUrl(string srcUrl)
        {
            if (string.IsNullOrWhiteSpace(srcUrl))
            {
                return null;
            }
            string objUrl = srcUrl.TrimStart('/');
            objUrl = Uri.EscapeUriString(objUrl);
            if (Uri.IsWellFormedUriString(objUrl, UriKind.Relative)) objUrl = $"https://{hostAddress}/{objUrl}";
            else objUrl = objUrl.Replace("www.dropbox.com", "dl.dropboxusercontent.com"); // replace dropbox links to direct links
            if (string.IsNullOrWhiteSpace(objUrl)) return null;
            if (!Uri.IsWellFormedUriString(objUrl, UriKind.Absolute))
            {
                Debug.LogWarning($"Invalid Uri: '{objUrl}'");
                return null;
            }
            objUrl = Uri.UnescapeDataString(objUrl);
            return new Uri(objUrl);
        }

        internal string ConstructLocalPath(Uri uri)
        {
            if (uri == null) return null;
            string url2Path = uri.Host + uri.AbsolutePath;
            string objFileName = string.Join(Path.DirectorySeparatorChar.ToString(), url2Path.Split(Path.GetInvalidFileNameChars()));
            return Path.Combine(importPath, objFileName);
        }

        private IEnumerator DownloadAssets(string messageType, string msgUrl)
        {
            Uri remoteUri = ConstructRemoteUrl(msgUrl);
            if (remoteUri == null) yield break;
            // don't download the same asset twice, or simultaneously
            if (downloadQueue.Contains(remoteUri.AbsoluteUri)) yield break;
            downloadQueue.Add(remoteUri.AbsoluteUri);
            // check asset type
            if (!Path.HasExtension(remoteUri.AbsoluteUri)) yield break;
            string mimeType = MimeUtility.GetMimeMapping(remoteUri.GetLeftPart(UriPartial.Path));
            if (mimeType == null) yield break;
            if (skipMimeClasses.ToList().Contains(mimeType.Split('/')[0])) yield break;
            // load remote assets
            string localPath = ConstructLocalPath(remoteUri);
            if (localPath == null) yield break;
            bool allPathsValid = true;
            if (!File.Exists(localPath))
            {
                // get main url src
                CoroutineWithData cd = new CoroutineWithData(this, HttpRequestRaw(remoteUri.AbsoluteUri));
                yield return cd.coroutine;
                if (isCrdSuccess(cd.result))
                {
                    byte[] urlData = (byte[])cd.result;
                    // get gltf sub-assets
                    if (".gltf" == Path.GetExtension(localPath).ToLower())
                    {
                        string json = System.Text.Encoding.UTF8.GetString(urlData);
                        IEnumerable<string> uris = new string[] { };
                        try
                        {
                            uris = ExtractAssetUris(JsonConvert.DeserializeObject(json), gltfUriTags);
                        }
                        catch (JsonReaderException e)
                        {
                            Debug.LogWarning(e.Message);
                            allPathsValid = false;
                        }
                        foreach (var uri in uris)
                        {
                            if (!string.IsNullOrWhiteSpace(uri))
                            {
                                Uri subUrl = null;
                                try
                                {
                                    subUrl = new Uri(remoteUri, uri);
                                }
                                catch (UriFormatException)
                                {
                                    // formatting errors may be encoded binary data
                                    continue;
                                }
                                catch (Exception err)
                                {
                                    Debug.LogWarning($"Invalid GLTF uri: {err.Message}");
                                }
                                cd = new CoroutineWithData(this, HttpRequestRaw(subUrl.AbsoluteUri));
                                yield return cd.coroutine;
                                if (isCrdSuccess(cd.result))
                                {
                                    byte[] urlSubData = (byte[])cd.result;
                                    string localSubPath = Path.Combine(Path.GetDirectoryName(localPath), uri);
                                    SaveAsset(urlSubData, localSubPath);
#if UNITY_EDITOR
                                    // import each sub-file for a deterministic reference
                                    ImportAsset(localSubPath);
#endif
                                }
                                else allPathsValid = false;
                            }
                        }
                    }
                    SaveAsset(urlData, localPath);
                }
                else allPathsValid = false;

                if (!allPathsValid) yield break;
                // import master-file to link to the rest
                ImportAsset(localPath);
            }

            if (pendingAssetCallbacks.TryGetValue(remoteUri.AbsoluteUri, out Action callbacks))
            {
                callbacks?.Invoke();
                pendingAssetCallbacks.Remove(remoteUri.AbsoluteUri);
            }

            yield return localPath;
        }

        private void ImportAsset(string localPath)
        {
#if UNITY_EDITOR
            try
            {
                UnityEditor.AssetDatabase.ImportAsset(localPath);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Import error. {e.Message}");
            }
            ClearProgressBar();
#endif
        }

        private void SaveAsset(byte[] data, string path)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            if (!File.Exists(path))
            {
                File.WriteAllBytes(path, data);
            }
        }

        private IEnumerator HttpRequestRaw(string url)
        {
            Uri uri = new Uri(url);
            UnityWebRequest www = UnityWebRequest.Get(url);
            www.downloadHandler = new DownloadHandlerBuffer();
            if (!verifyCertificate)
            {
                www.certificateHandler = new SelfSignedCertificateHandler();
            }
            try
            {
                www.SendWebRequest();
                while (!www.isDone)
                {
                    DisplayCancelableProgressBar("ARENA", $"Downloading {uri.Segments[uri.Segments.Length - 1]}...", www.downloadProgress);
                    yield return null;
                }
                if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogWarning($"{www.error}: {www.url}");
                    ClearProgressBar();
                    yield break;
                }
                else
                {
                    byte[] results = www.downloadHandler.data;
                    yield return results;
                }
                ClearProgressBar();
            }
            finally
            {
                www.Dispose();
            }
        }

        private IEnumerator HttpUploadFSRaw(string url, byte[] payload)
        {
            Uri uri = new Uri(url);
            UnityWebRequest www = new UnityWebRequest(url, "POST");
            if (!verifyCertificate)
            {
                www.certificateHandler = new SelfSignedCertificateHandler();
            }
            www.SetRequestHeader("X-Auth", fsToken);
            UploadHandler uploadHandler = new UploadHandlerRaw(payload);
            www.uploadHandler = uploadHandler;
            try
            {
                www.SendWebRequest();
                while (!www.isDone)
                {
                    DisplayCancelableProgressBar("ARENA", $"Uploading {uri.Segments[uri.Segments.Length - 1]}...", www.uploadProgress);
                    yield return null;
                }
                if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogWarning($"{www.error}: {www.url}");
                    ClearProgressBar();
                    yield break;
                }
                else
                {
                    yield return true;
                }
                ClearProgressBar();
            }
            finally
            {
                www.Dispose();
            }
        }
    }
}
