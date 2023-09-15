using System;
using System.Runtime.Serialization;
using Newtonsoft.Json.Serialization;
using UnityEngine;

namespace ArenaUnity
{
    //TODO (mwfarb): move ArenaCameraJson to automated schema when the schema translator is updated
    [Serializable]
    public class ArenaCameraJson
    {
        // Currently the commented out properties are in the higher level 'transaction' wire message format
        // public string displayName { get; set; }
        // public string presence { get; set; }
        // public string jitsiId { get; set; }
        // public bool hasAudio { get; set; }
        // public bool hasVideo { get; set; }
        // public bool hasAvatar { get; set; }
        // TODO (mwfarb): we need to migrate these properties into 'data' in arena-camera.js first

        public string color { get; set; }
        public string headModelPath { get; set; }

        [OnError]
        internal void OnError(StreamingContext context, ErrorContext errorContext)
        {
            Debug.LogWarning($"{errorContext.Error.Message}: {errorContext.OriginalObject}");
            errorContext.Handled = true;
        }
    }
}
