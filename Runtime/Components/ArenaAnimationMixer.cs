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
    [HelpURL("https://docs.arenaxr.org/content/schemas/message/animation-mixer")]
    public class ArenaAnimationMixer : ArenaComponent
    {
        // ARENA animation-mixer component unity conversion status:
        // DONE: clampWhenFinished
        // DONE: clip
        // DONE: crossFadeDuration
        // DONE: duration
        // DONE: loop
        // DONE: startAt
        // DONE: timeScale
        // DONE: useRegExp
        // N/A: repetitions not supported by Unity's legacy Animation system

        // NOTE: There is an easy clip parser but only #if UNITY_EDITOR (AnimationUtility.GetAnimationClips()).

        [Tooltip("Serializable JSON attributes for Arena animation-mixer")]
        public ArenaAnimationMixerJson json = new ArenaAnimationMixerJson();
        internal List<string> animations = new List<string>();

        protected override void Start()
        {
            base.Start();
            var gltfModel = GetComponent<ArenaWireGltfModel>();
            if (gltfModel != null)
            {
                gltfModel.OnGltfLoaded.AddListener(() => { apply = true; });
            }
        }

        protected override void ApplyRender()
        {
            // apply changes to local unity object
            Animation anim = GetComponentInChildren<Animation>(true);
            if (anim == null) return;
            // set animation mixer properties
            anim.Stop();
            anim.cullingType = AnimationCullingType.AlwaysAnimate;
            anim.playAutomatically = false;

            var gltfModel = GetComponent<ArenaWireGltfModel>();
            if (gltfModel != null && gltfModel.animations != null)
                animations = gltfModel.animations;
            if (animations.Count > 0) anim.clip = anim[animations[0]].clip;

            if (json == null) return;
            switch (json.Loop)
            {
                default:
                case ArenaAnimationMixerJson.LoopType.Repeat: anim.wrapMode = WrapMode.Loop; break;
                case ArenaAnimationMixerJson.LoopType.Once: anim.wrapMode = WrapMode.Once; break;
                case ArenaAnimationMixerJson.LoopType.Pingpong: anim.wrapMode = WrapMode.PingPong; break;
            }
            if (json.ClampWhenFinished) anim.wrapMode = WrapMode.ClampForever;

            // play animations according to clip pattern
            // useRegExp: true = treat clip as regex directly, false = convert wildcards to regex
            string pattern;
            if (json.UseRegExp)
            {
                pattern = json.Clip;
            }
            else
            {
                pattern = @$"{json.Clip.Replace("*", @"\w*")}"; // convert wildcards for .Net
            }

            if (animations != null && animations.Count > 0)
            {
                for (int i = 0; i < animations.Count; i++)
                {
                    // set each animation on separate layer so all can be played
                    anim[animations[i]].layer = i;
                    anim[animations[i]].speed = json.TimeScale;
                    anim[animations[i]].time = json.StartAt / 1000;

                    // apply duration override (adjust speed to achieve target duration)
                    if (json.Duration > 0 && anim[animations[i]].clip != null)
                    {
                        float clipLength = anim[animations[i]].clip.length;
                        if (clipLength > 0)
                        {
                            anim[animations[i]].speed = clipLength / json.Duration * json.TimeScale;
                        }
                    }

                    bool includeClip = false;
                    if (json.UseRegExp || json.Clip.Contains("*"))
                    {
                        Match m = Regex.Match(animations[i], pattern);
                        if (m.Success) includeClip = true;
                    }
                    else if (json.Clip == animations[i])
                    {
                        includeClip = true;
                    }
                    if (includeClip)
                    {
                        float fadeLength = json.CrossFadeDuration;
                        if (fadeLength > 0)
                            anim.CrossFade(animations[i], fadeLength);
                        else
                            anim.Play(animations[i]);
                    }
                }
            }
        }

        public override void UpdateObject()
        {
            PublishIfChanged(json.attributeName, JsonConvert.SerializeObject(json));
        }
    }
}
