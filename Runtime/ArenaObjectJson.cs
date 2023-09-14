using System;
using Newtonsoft.Json;

namespace ArenaUnity
{
    [Serializable]
    public class ArenaObjectJson
    {
        public string object_id = null;
        public bool? persist = null;
        public string type = null;
        public string action = null;
        public float? ttl = null;
        public bool? overwrite = null;
        public string timestamp = null;

        // TODO (mwfarb): consolidate handling of data vs attributes since they are the same
        public object data = null;
        public object attributes = null;

        // TODO (mwfarb): remove displayName from transaction level object
        public string displayName = null;

    }
}
