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
        // ARENA Property Handling Status
        // DONE: clip
        // TODO: duration
        // DONE: crossFadeDuration
        // DONE: loop
        // TODO: repetitions
        // DONE: timeScale
        // DONE: clampWhenFinished
        // DONE: startAt

        // NOTE: There is an easy clip parser but only #if UNITY_EDITOR (AnimationUtility.GetAnimationClips()).

        [Tooltip("Serializable JSON attributes for Arena animation-mixer")]
        public ArenaAnimationMixerJson json = new ArenaAnimationMixerJson();
        internal List<string> animations = null;

        protected override void ApplyRender()
        {
            // TODO: Implement this component if needed, or note our reasons for not rendering or controlling here.

            // apply changes to local unity object
            Animation anim = GetComponentInChildren<Animation>(true);
            if (anim == null) return;
            // set animation mixer properties
            anim.Stop();
            anim.cullingType = AnimationCullingType.AlwaysAnimate;
            anim.playAutomatically = false;

            var aobj = GetComponent<ArenaObject>();
            if (aobj != null)
                animations = aobj.animations;
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

            // play animations according to clip and wildcard
            string pattern = @$"{json.Clip.Replace("*", @"\w*")}"; // update wildcards for .Net
            if (animations != null && animations.Count > 0)
            {
                for (int i = 0; i < animations.Count; i++)
                {
                    // set each animation on separate layer so all can be played
                    anim[animations[i]].layer = i;
                    anim[animations[i]].speed = (float)json.TimeScale;
                    anim[animations[i]].time = (float)(json.StartAt / 1000);
                    bool includeClip = false;
                    if (json.Clip.Contains("*")) // only use regex for wildcards
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
                        float fadeLength = (float)(json.CrossFadeDuration);
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
            var newJson = JsonConvert.SerializeObject(json);
            if (updatedJson != newJson)
            {
                var aobj = GetComponent<ArenaObject>();
                if (aobj != null)
                {
                    aobj.PublishUpdate($"{{\"{json.componentName}\":{newJson}}}");
                    apply = true;
                }
            }
            updatedJson = newJson;
        }
    }
}
