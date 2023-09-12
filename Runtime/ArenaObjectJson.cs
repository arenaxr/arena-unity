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
        public ArenaObjectDataJson data { get; set; }
    }
}
