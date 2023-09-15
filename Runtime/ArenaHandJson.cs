using System;
using System.Runtime.Serialization;
using Newtonsoft.Json.Serialization;
using UnityEngine;

namespace ArenaUnity
{
    //TODO (mwfarb): move ArenaHandJson to automated schema when the schema translator is updated
    [Serializable]
    public class ArenaHandJson
    {
        public string url { get; set; }
        public string dep { get; set; }

        [OnError]
        internal void OnError(StreamingContext context, ErrorContext errorContext)
        {
            Debug.LogWarning($"{errorContext.Error.Message}: {errorContext.OriginalObject}");
            errorContext.Handled = true;
        }
    }
}
