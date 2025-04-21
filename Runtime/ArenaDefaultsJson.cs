using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using ArenaUnity.Schemas;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using UnityEngine;

namespace ArenaUnity
{
    [Serializable]
    public class ArenaDefaultsJson
    {
        public string realm { get; set; }
        public int camUpdateIntervalMs { get; set; }
        [JsonProperty(PropertyName = "namespace")]
        public string _namespace { get; set; }
        public string sceneName { get; set; }
        public string userName { get; set; }
        public ArenaVector3Json startCoords { get; set; }
        public float camHeight { get; set; }
        public string mqttHost { get; set; }
        public string jitsiHost { get; set; }
        public string ATLASurl { get; set; }
        public string vioTopic { get; set; }
        public string graphTopic { get; set; }
        public string latencyTopic { get; set; }
        public string[] mqttPath { get; set; }
        public string persistHost { get; set; }
        public string persistPath { get; set; }
        public bool devInstance { get; set; }
        public string headModelPath { get; set; }

        // General json object management
        [OnError]
        internal void OnError(StreamingContext context, ErrorContext errorContext)
        {
            Debug.LogWarning($"{errorContext.Error.Message}: {errorContext.OriginalObject}");
            errorContext.Handled = true;
        }

        [JsonExtensionData]
        private IDictionary<string, JToken> _additionalData;
    }
}
