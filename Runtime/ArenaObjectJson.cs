using System;
using Newtonsoft.Json;

namespace ArenaUnity
{
    [Serializable]
    public class ArenaObjectJson
    {
        public string object_id { get; set; }
        public bool persist { get; set; }
        public string type { get; set; }
        public string action { get; set; }
        public int ttl { get; set; }
        [JsonProperty(PropertyName = "override")]
        public bool _override { get; set; }
        public string timestamp { get; set; }

        // TODO (mwfarb): consolidate handling of data vs attributes since they are the same
        public object data { get; set; }
        public object attributes { get; set; }

        // TODO (mwfarb): remove displayName from transaction level object
        public string displayName = null;
    }
}
