/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021, The CONIX Research Center. All rights reserved.
 */

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ArenaUnity
{
    public class ArenaAnimationMixer : MonoBehaviour
    {
        internal readonly string componentName = "animation-mixer";

        public ArenaAnimationMixerJson json = new ArenaAnimationMixerJson();
        internal List<string> animations = null;

        protected bool apply = false;
        internal bool scriptLoaded = false;

        protected virtual void Start()
        {
            var aobj = GetComponent<ArenaObject>();
            if (aobj != null && aobj.data != null && aobj.data.url != null)
            {
                FindAnimations((string)aobj.data.url);
            }
            ApplyAnimations();
        }

        protected void OnValidate()
        {
            apply = true;

            if (!scriptLoaded)
            {
                scriptLoaded = true;
            }
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
            Debug.Log("animation-mixer: " + json.SaveToString());

            Animation anim = GetComponentInChildren<Animation>(true);

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


            //if (json.clip == "*")
            //{
            //    anim.Play(json.clip);
            //    //anim.Play(anim.clip.name);
            //    Debug.Log("playing: " + anim.clip.name + ", wrapMode: " + anim.wrapMode.ToString());
            //}
            if (animations != null)
            {
                foreach (string animation in animations)
                {
                    //anim.PlayQueued(animation, QueueMode.CompleteOthers);
                    anim.PlayQueued(animation);
                }
            }

            //clip* Name of the animation clip(s) to play. Accepts wildcards.
            //duration AUTO    Duration of the animation, in seconds.
            //crossFadeDuration   0   Duration of cross - fades between clips, in seconds.
            //loop repeat  once, repeat, or pingpong. In repeat and pingpong modes, the clip plays once plus the specified number of repetitions. For pingpong, every second clip plays in reverse.
            //repetitions Infinity    Number of times to play the clip, in addition to the first play.Repetitions are ignored for loop: once.
            //timeScale   1   Scaling factor for playback speed. A value of 0 causes the animation to pause.Negative values cause the animation to play backwards.
            //clampWhenFinished   false   If true, halts the animation at the last frame.
            //startAt 0   Sets the start of an animation to a specific time(in milliseconds).This is useful when you need to jump to an exact time in an animation.The input parameter will be scaled by the mixer's timeScale.


            //AddClip Adds a clip to the animation with name newName.
            //Blend Blends the animation named animation towards targetWeight over the next time seconds.
            //CrossFade Fades the animation with name animation in over a period of time seconds and fades other animations out.
            //CrossFadeQueued Cross fades an animation after previous animations has finished playing.
            //GetClipCount Get the number of clips currently assigned to this animation.
            //IsPlaying Is the animation named name playing?
            //Play    Plays an animation without blending.
            //PlayQueued Plays an animation after previous animations has finished playing.
            //RemoveClip Remove clip from the animation list.
            //Rewind Rewinds the animation named name.
            //Sample Samples animations at the current state.
            //Stop Stops all playing animations that were started with this Animation.

        }

        private void FindAnimations(string url)
        {
            // check for animations
            var assetRepresentationsAtPath = AssetDatabase.LoadAllAssetRepresentationsAtPath(url);
            List<string> animations = new List<string>();

            foreach (var assetRepresentation in assetRepresentationsAtPath)
            {
                var animationClip = assetRepresentation as AnimationClip;
                if (animationClip != null)
                {
                    animations.Add(animationClip.name);
                }
            }
        }

    }
}
