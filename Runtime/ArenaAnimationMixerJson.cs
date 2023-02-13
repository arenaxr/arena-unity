using System.Runtime.Serialization;
using System.Text.RegularExpressions;
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

    public const string defclip = "*";
    public const int defduration = 0;
    public const int defcrossFadeDuration = 0;
    public const LoopType defloop = LoopType.repeat;
    public const string defrepetitions = null;
    public const float deftimeScale = 1;
    public const bool defclampWhenFinished = false;
    public const int defstartAt = 0;

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
    [Tooltip("Name of the animation clip(s) to play. Accepts wildcards.")]
    public string clip = defclip;
    //public bool ShouldSerializeclip() // <-- TODO should mark required?
    //{
    //    return (clip != defclip);
    //}

    /// <summary>
    /// Duration of the animation, in seconds.
    /// </summary>
    [Tooltip("Duration of the animation, in seconds.")]
    public int duration = defduration;
    public bool ShouldSerializeduration()
    {
        return (duration != defduration);
    }

    /// <summary>
    /// Duration of cross-fades between clips, in seconds.
    /// </summary>
    [Tooltip("Duration of cross-fades between clips, in seconds.")]
    public int crossFadeDuration = defcrossFadeDuration;
    public bool ShouldSerializecrossFadeDuration()
    {
        return (crossFadeDuration != defcrossFadeDuration);
    }

    /// <summary>
    /// once, repeat, or pingpong. In repeat and pingpong modes, the clip plays once plus the specified number of repetitions. For pingpong, every second clip plays in reverse.
    /// </summary>
    [Tooltip("once, repeat, or pingpong. In repeat and pingpong modes, the clip plays once plus the specified number of repetitions. For pingpong, every second clip plays in reverse.")]
    [JsonConverter(typeof(StringEnumConverter))]
    public LoopType loop = defloop;
    public bool ShouldSerializeloop()
    {
        return (loop != defloop);
    }

    /// <summary>
    /// Number of times to play the clip, in addition to the first play. Repetitions are ignored for loop: once.
    /// </summary>
    // TODO: empty to serialize as null
    [Tooltip("Number of times to play the clip, in addition to the first play. Repetitions are ignored for loop: once.")]
    public string repetitions = defrepetitions;
    public bool ShouldSerializerepetitions()
    {
        return (repetitions != defrepetitions);
    }

    /// <summary>
    /// Scaling factor for playback speed. A value of 0 causes the animation to pause. Negative values cause the animation to play backwards.
    /// </summary>
    [Tooltip("Scaling factor for playback speed. A value of 0 causes the animation to pause. Negative values cause the animation to play backwards.")]
    public float timeScale = deftimeScale;
    public bool ShouldSerializetimeScale()
    {
        return (timeScale != deftimeScale);
    }

    /// <summary>
    /// If true, halts the animation at the last frame.
    /// </summary>
    [Tooltip("If true, halts the animation at the last frame.")]
    public bool clampWhenFinished = defclampWhenFinished;
    public bool ShouldSerializeclampWhenFinished()
    {
        return (clampWhenFinished != defclampWhenFinished);
    }

    /// <summary>
    /// Sets the start of an animation to a specific time (in milliseconds). This is useful when you need to jump to an exact time in an animation. The input parameter will be scaled by the mixer's timeScale.
    /// </summary>
    [Tooltip("Sets the start of an animation to a specific time (in milliseconds). This is useful when you need to jump to an exact time in an animation. The input parameter will be scaled by the mixer's timeScale.")]
    public int startAt = defstartAt;
    public bool ShouldSerializestartAt()
    {
        return (startAt != defstartAt);
    }

    public string SaveToString()
    {
        return JsonConvert.SerializeObject(this);
    }

    public static ArenaAnimationMixerJson CreateFromJSON(string jsonString)
    {
        string value = Regex.Unescape(jsonString);
        Debug.Log($"jsonString {value}");
        return JsonConvert.DeserializeObject<ArenaAnimationMixerJson>(value);
    }

}
