/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2023, Carnegie Mellon University. All rights reserved.
 */

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace ArenaUnity.Schemas
{
    /// <summary>
    /// Video Control
    /// </summary>
    [Serializable]
    public class ArenaVideoControlJson
    {
        public const string componentName = "video-control";

        // video-control member-fields

        private static string defFrameObject = "";
        [JsonProperty(PropertyName = "frame_object")]
        [Tooltip("URL of a thumbnail image, e.g. 'store/users/wiselab/images/conix-face-white.jpg'")]
        public string FrameObject = defFrameObject;
        public bool ShouldSerializeFrameObject()
        {
            return true; // required in json schema 
        }

        private static string defVideoObject = "";
        [JsonProperty(PropertyName = "video_object")]
        [Tooltip("Name of object where to put the video, e.g. 'square_vid6'")]
        public string VideoObject = defVideoObject;
        public bool ShouldSerializeVideoObject()
        {
            return true; // required in json schema 
        }

        private static string defVideoPath = "";
        [JsonProperty(PropertyName = "video_path")]
        [Tooltip("URL of the video file, e.g. 'store/users/wiselab/videos/kungfu.mp4'")]
        public string VideoPath = defVideoPath;
        public bool ShouldSerializeVideoPath()
        {
            return true; // required in json schema 
        }

        private static bool defAnyoneClicks = true;
        [JsonProperty(PropertyName = "anyone_clicks")]
        [Tooltip("Responds to clicks from any user")]
        public bool AnyoneClicks = defAnyoneClicks;
        public bool ShouldSerializeAnyoneClicks()
        {
            if (_token != null && _token.SelectToken("anyone_clicks") != null) return true;
            return (AnyoneClicks != defAnyoneClicks);
        }

        private static bool defVideoLoop = true;
        [JsonProperty(PropertyName = "video_loop")]
        [Tooltip("Video automatically loops")]
        public bool VideoLoop = defVideoLoop;
        public bool ShouldSerializeVideoLoop()
        {
            if (_token != null && _token.SelectToken("video_loop") != null) return true;
            return (VideoLoop != defVideoLoop);
        }

        private static bool defAutoplay = false;
        [JsonProperty(PropertyName = "autoplay")]
        [Tooltip("Video starts playing automatically")]
        public bool Autoplay = defAutoplay;
        public bool ShouldSerializeAutoplay()
        {
            if (_token != null && _token.SelectToken("autoplay") != null) return true;
            return (Autoplay != defAutoplay);
        }

        private static float defVolume = 1f;
        [JsonProperty(PropertyName = "volume")]
        [Tooltip("Video sound volume")]
        public float Volume = defVolume;
        public bool ShouldSerializeVolume()
        {
            if (_token != null && _token.SelectToken("volume") != null) return true;
            return (Volume != defVolume);
        }

        // General json object management

        [JsonExtensionData]
        private IDictionary<string, JToken> _additionalData;

        private static JToken _token;

        public string SaveToString()
        {
            return Regex.Unescape(JsonConvert.SerializeObject(this));
        }

        public static ArenaVideoControlJson CreateFromJSON(string jsonString, JToken token)
        {
            _token = token; // save updated wire json
            return JsonConvert.DeserializeObject<ArenaVideoControlJson>(Regex.Unescape(jsonString));
        }
    }
}
