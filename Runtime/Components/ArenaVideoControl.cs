/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2023, Carnegie Mellon University. All rights reserved.
 */

using System.Collections.Generic;
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
                    Debug.LogWarning($"video-control video_object not found: {json.VideoObject}");
                    return;
                }
            }

            // video_path, defer to ArenaMaterial's texture path, which owns the VideoPlayer
            if (!string.IsNullOrEmpty(json.VideoPath) && ArenaClientScene.Instance != null)
            {
                string videoPath = ArenaClientScene.Instance.checkLocalAsset(json.VideoPath);
                if (videoPath == null) ArenaClientScene.Instance.RegisterAssetCallback(json.VideoPath, () => { apply = true; });
                else ArenaMaterial.AttachMaterialTexture(videoPath, vobj);
            }

            // no VideoPlayer yet, material src or video_path will re-apply when the asset arrives
            var videoPlayer = vobj.GetComponent<UnityEngine.Video.VideoPlayer>();
            if (videoPlayer == null) return;

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
