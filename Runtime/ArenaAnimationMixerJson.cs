using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine;


/// <summary>
/// A list of available animations can usually be found by inspecting the model file or its documentation. All animations will play by default. To play only a specific set of animations, use wildcards: animation-mixer='clip: run_*'. \n\nMore properties at <a href='https://github.com/n5ro/aframe-extras/tree/master/src/loaders#animation'>https://github.com/n5ro/aframe-extras/tree/master/src/loaders#animation</a>",
/// </summary>
[System.Serializable]
public class ArenaAnimationMixerJson
{
    // https://pavcreations.com/json-advanced-parsing-in-unity/2/

    public const string defClip = "*";
    public const int defDuration = 0;
    public const int defCrossFadeDuration = 0;
    public const LoopType defLoop = LoopType.repeat;
    public const string defRepetitions = null;
    public const int defTimeScale = 1;
    public const bool defClampWhenFinished = false;
    public const int defStartAt = 0;

    public enum LoopType
    {
        [EnumMember(Value = "once")]
        once,
        [EnumMember(Value = "repeat")]
        repeat,
        [EnumMember(Value = "pingpong")]
        pingpong,
    }

    /// <summary>
    /// Name of the animation clip(s) to play. Accepts wildcards.
    /// </summary>
    public string clip = defClip;

    /// <summary>
    /// Duration of the animation, in seconds.
    /// </summary>
    public int duration = defDuration;

    /// <summary>
    /// Duration of cross-fades between clips, in seconds.
    /// </summary>
    public int crossFadeDuration = defCrossFadeDuration;

    /// <summary>
    /// once, repeat, or pingpong. In repeat and pingpong modes, the clip plays once plus the specified number of repetitions. For pingpong, every second clip plays in reverse.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public LoopType loop = defLoop;

    /// <summary>
    /// Number of times to play the clip, in addition to the first play. Repetitions are ignored for loop: once.
    /// </summary>
    public string repetitions = defRepetitions;

    /// <summary>
    /// Scaling factor for playback speed. A value of 0 causes the animation to pause. Negative values cause the animation to play backwards.
    /// </summary>
    public int timeScale = defTimeScale;

    /// <summary>
    /// If true, halts the animation at the last frame.
    /// </summary>
    public bool clampWhenFinished = defClampWhenFinished;

    /// <summary>
    /// Sets the start of an animation to a specific time (in milliseconds). This is useful when you need to jump to an exact time in an animation. The input parameter will be scaled by the mixer's timeScale.
    /// </summary>
    public int startAt = defStartAt;

    public string SaveToString()
    {
        return JsonConvert.SerializeObject(this);
    }

    public static ArenaAnimationMixerJson CreateFromJSON(string jsonString)
    {
        return JsonConvert.DeserializeObject<ArenaAnimationMixerJson>(jsonString);
    }

}
