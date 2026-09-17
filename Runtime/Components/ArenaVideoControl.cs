/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2023, Carnegie Mellon University. All rights reserved.
 */

using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using ArenaUnity.Schemas;
using Newtonsoft.Json;
using UnityEngine;

namespace ArenaUnity.Components
{
    public class ArenaVideoControl : ArenaComponent
    {
        // ARENA video-control component unity conversion status:
        // TODO: frame_object
        // DONE: video_object
        // DONE: video_path
        // TODO: anyone_clicks
        // DONE: video_loop
        // DONE: autoplay
        // DONE: volume
        // N/A:  cleanup (Unity destroys the VideoPlayer with the object, there is no DOM to clean)

        public ArenaVideoControlJson json = new ArenaVideoControlJson();

        // A video-control message can arrive before the object named by video_object exists, or
        // before ArenaMaterial has created the VideoPlayer that owns the clip. ApplyRender returns
        // in both cases, so re-arm apply from Update while the dependency is still missing, and
        // stop at pendingTimeout so a dependency that never arrives is not re-checked every frame
        // for the life of the scene.
        private const float pendingTimeout = 30f;
        private string pendingDependency = null;
        private float pendingDeadline = 0f;

        protected override void Update()
        {
            if (pendingDependency != null && !apply)
            {
                if (Time.realtimeSinceStartup < pendingDeadline) apply = true;
                else
                {
                    Debug.LogWarning($"video-control gave up waiting for {pendingDependency}");
                    pendingDependency = null;
                }
            }
            base.Update();
        }

        private void WaitFor(string dependency)
        {
            if (pendingDependency == null) pendingDeadline = Time.realtimeSinceStartup + pendingTimeout;
            pendingDependency = dependency;
        }

        protected override void ApplyRender()
        {
            // video_object names the object holding the video, otherwise this object holds it
            GameObject vobj = gameObject;
            if (!string.IsNullOrEmpty(json.VideoObject))
            {
                if (ArenaClientScene.Instance != null && ArenaClientScene.Instance.arenaObjs.TryGetValue(json.VideoObject, out GameObject namedObj))
                    vobj = namedObj;
                else
                {
                    // the named object may not have arrived yet, retry until pendingTimeout
                    WaitFor($"video_object: {json.VideoObject}");
                    return;
                }
            }

            // video_path, defer to ArenaMaterial's texture path, which owns the VideoPlayer
            if (!string.IsNullOrEmpty(json.VideoPath) && ArenaClientScene.Instance != null)
            {
                string videoPath = ArenaClientScene.Instance.checkLocalAsset(json.VideoPath);
                if (videoPath == null) ArenaClientScene.Instance.RegisterAssetCallback(json.VideoPath, () => { apply = true; });
                else
                {
                    // attach only for a new clip, AttachMaterialTexture restarts playback and resets loop/autoplay
                    var attached = vobj.GetComponent<UnityEngine.Video.VideoPlayer>();
                    if (attached == null || attached.url != Path.GetFullPath(videoPath))
                        ArenaMaterial.AttachMaterialTexture(videoPath, vobj);
                }
            }

            var videoPlayer = vobj.GetComponent<UnityEngine.Video.VideoPlayer>();
            if (videoPlayer == null)
            {
                // ArenaMaterial owns the VideoPlayer and only creates it once material.src is a
                // local file, so hook that asset once for a download slower than pendingTimeout,
                // and retry meanwhile in case material is applied after us in the same message
                if (pendingDependency == null)
                {
                    var material = vobj.GetComponent<ArenaMaterial>();
                    if (material != null && !string.IsNullOrEmpty(material.json.Src) && ArenaClientScene.Instance != null
                        && ArenaClientScene.Instance.checkLocalAsset(material.json.Src) == null)
                        ArenaClientScene.Instance.RegisterAssetCallback(material.json.Src, () => { apply = true; });
                }
                WaitFor($"VideoPlayer on: {vobj.name}");
                return;
            }
            pendingDependency = null;

            videoPlayer.isLooping = json.VideoLoop;
            videoPlayer.playOnAwake = json.Autoplay;

            // volume, only the direct output mode has a scalar counterpart
            videoPlayer.audioOutputMode = UnityEngine.Video.VideoAudioOutputMode.Direct;
            if (videoPlayer.controlledAudioTrackCount > 0)
                videoPlayer.SetDirectAudioVolume(0, json.Volume);

            if (json.Autoplay)
            {
                if (!videoPlayer.isPlaying) videoPlayer.Play();
            }
            else if (videoPlayer.isPlaying) videoPlayer.Pause();
        }

        public override void UpdateObject()
        {
            PublishIfChanged(json.attributeName, JsonConvert.SerializeObject(json));
        }
    }
}
