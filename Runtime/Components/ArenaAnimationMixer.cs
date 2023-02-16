/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2023, Carnegie Mellon University. All rights reserved.
 */

using System.Collections.Generic;
using System.Text.RegularExpressions;
using ArenaUnity.Schemas;
using UnityEngine;

namespace ArenaUnity.Components
{
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    [HelpURL("https://docs.arenaxr.org/content/schemas/message/animation-mixer")]
    public class ArenaAnimationMixer : MonoBehaviour
    {
        // STATUS
        // DONE clip: * Name of the animation clip(s) to play. Accepts wildcards.
        // TODO duration: AUTO    Duration of the animation, in seconds.
        // DONE crossFadeDuration:   0   Duration of cross - fades between clips, in seconds.
        // DONE loop:  once, repeat, or pingpong. In repeat and pingpong modes, the clip plays once plus the specified number of repetitions. For pingpong, every second clip plays in reverse.
        // TODO repetitions: Infinity    Number of times to play the clip, in addition to the first play.Repetitions are ignored for loop: once.
        // DONE timeScale:   1   Scaling factor for playback speed. A value of 0 causes the animation to pause.Negative values cause the animation to play backwards.
        // DONE clampWhenFinished:   false   If true, halts the animation at the last frame.
        // DONE startAt: 0   Sets the start of an animation to a specific time(in milliseconds).This is useful when you need to jump to an exact time in an animation.The input parameter will be scaled by the mixer's timeScale.

        // NOTE: There is an easy clip parser but only #if UNITY_EDITOR (AnimationUtility.GetAnimationClips()).

        [Tooltip("Serializable JSON attributes for Arena animation-mixer")]
        public ArenaAnimationMixerJson json = new ArenaAnimationMixerJson();
        internal List<string> animations = null;

        internal bool apply = false;

        protected virtual void Start()
        {
            apply = true;
        }

        protected void OnValidate()
        {
            apply = true;
        }

        protected void Update()
        {
            if (apply)
            {
                ApplyAnimations();
                apply = false;
            }
        }

        internal void ApplyAnimations()
        {
            // apply changes to local unity object
            Animation anim = GetComponentInChildren<Animation>(true);
            if (anim == null) return;
            // set animation mixer properties
            if (anim.isPlaying) anim.Stop();
            anim.cullingType = AnimationCullingType.BasedOnRenderers;
            anim.playAutomatically = true;
            switch (json.loop.ToString())
            {
                default:
                case "repeat": anim.wrapMode = WrapMode.Loop; break;
                case "once": anim.wrapMode = WrapMode.Once; break;
                case "pingpong": anim.wrapMode = WrapMode.PingPong; break;
            }
            if (json.clampWhenFinished) anim.wrapMode = WrapMode.ClampForever;

            var aobj = GetComponent<ArenaObject>();
            if (aobj != null)
                animations = aobj.animations;

            // play animations according to clip and wildcard
            string pattern = @$"{json.clip.Replace("*", @"\w*")}"; // update wildcards for .Net
            if (animations != null && animations.Count > 0)
            {
                for (int i = 0; i < animations.Count; i++)
                {
                    // set each animation on separate layer so all can be played
                    anim[animations[i]].layer = i;
                    anim[animations[i]].speed = (float)json.timeScale;
                    anim[animations[i]].time = (float)(json.startAt / 1000);
                    bool includeClip = false;
                    if (json.clip.Contains("*")) // only use regex for wildcards
                    {
                        Match m = Regex.Match(animations[i], pattern);
                        if (m.Success) includeClip = true;
                    }
                    else if (json.clip == animations[i])
                    {
                        includeClip = true;
                    }
                    if (includeClip)
                    {
                        float fadeLength = (float)(json.crossFadeDuration);
                        if (fadeLength > 0)
                            anim.CrossFade(animations[i], fadeLength);
                        else
                            anim.Play(animations[i]);
                    }
                }
            }
        }

        internal void UpdateObject()
        {
            var aobj = GetComponent<ArenaObject>();
            if (aobj != null)
            {
                aobj.PublishUpdate($"{{\"{ArenaAnimationMixerJson.componentName}\":{json.SaveToString()}}}");
                apply = true;
            }
        }
    }
}
