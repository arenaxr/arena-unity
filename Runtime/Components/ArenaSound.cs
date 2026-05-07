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
    public class ArenaSound : ArenaComponent
    {
        // ARENA sound component unity conversion status:
        // DONE: autoplay
        // DONE: distanceModel
        // DONE: loop
        // DONE: maxDistance
        // DONE: on
        // TODO: poolSize
        // DONE: positional
        // TODO: refDistance
        // TODO: rolloffFactor
        // DONE: src
        // DONE: volume

        public ArenaSoundJson json = new ArenaSoundJson();
        private AudioSource audioSource;
        private string lastLoadedSrc;

        protected override void ApplyRender()
        {
            if (audioSource == null)
            {
                audioSource = gameObject.GetComponent<AudioSource>();
                if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
            }

            audioSource.loop = json.Loop;
            audioSource.volume = json.Volume;
            audioSource.spatialBlend = json.Positional ? 1.0f : 0.0f;
            audioSource.maxDistance = json.MaxDistance;

            switch (json.DistanceModel)
            {
                case ArenaSoundJson.DistanceModelType.Linear:
                    audioSource.rolloffMode = AudioRolloffMode.Linear;
                    break;
                case ArenaSoundJson.DistanceModelType.Inverse:
                    audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
                    break;
                case ArenaSoundJson.DistanceModelType.Exponential:
                    audioSource.rolloffMode = AudioRolloffMode.Custom;
                    break;
            }

            if (json.Src != lastLoadedSrc)
            {
                lastLoadedSrc = json.Src;
                if (!string.IsNullOrEmpty(json.Src) && ArenaClientScene.Instance != null)
                {
                    ArenaClientScene.Instance.RegisterAssetCallback(json.Src, OnAudioReady);
                }
            }

            if (json.Autoplay && audioSource.clip != null && !audioSource.isPlaying)
            {
                audioSource.Play();
            }
        }

        private void OnAudioReady()
        {
            if (ArenaClientScene.Instance == null) return;
            string localPath = ArenaClientScene.Instance.checkLocalAsset(json.Src);
            if (localPath != null)
            {
                string absolutePath = System.IO.Path.GetFullPath(localPath);
                StartCoroutine(LoadAudioClip("file://" + absolutePath));
            }
        }

        private System.Collections.IEnumerator LoadAudioClip(string path)
        {
            using (UnityEngine.Networking.UnityWebRequest uwr = UnityEngine.Networking.UnityWebRequestMultimedia.GetAudioClip(path, AudioType.UNKNOWN))
            {
                yield return uwr.SendWebRequest();
                if (uwr.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
                {
                    AudioClip clip = UnityEngine.Networking.DownloadHandlerAudioClip.GetContent(uwr);
                    if (audioSource != null && clip != null)
                    {
                        audioSource.clip = clip;
                        if (json.Autoplay)
                        {
                            audioSource.Play();
                        }
                    }
                }
                else
                {
                    Debug.LogWarning($"Failed to load audio {path}: {uwr.error}");
                }
            }
        }

        internal void OnMouseDown() { TriggerEvent(ArenaSoundJson.OnType.Mousedown); }
        internal void OnMouseUp() { TriggerEvent(ArenaSoundJson.OnType.Mouseup); }
        internal void OnMouseEnter() { TriggerEvent(ArenaSoundJson.OnType.Mouseenter); }
        internal void OnMouseExit() { TriggerEvent(ArenaSoundJson.OnType.Mouseleave); }

        private void TriggerEvent(ArenaSoundJson.OnType eventType)
        {
            if (json.On == eventType && audioSource != null && audioSource.clip != null)
            {
                audioSource.Play();
            }
        }

        public override void UpdateObject()
        {
            PublishIfChanged(json.attributeName, JsonConvert.SerializeObject(json));
        }
    }
}
