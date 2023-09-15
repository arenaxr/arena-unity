using System;
using Newtonsoft.Json;

namespace ArenaUnity
{
    [Serializable]
    public class ArenaObjectJson
    {
        public string object_id = null;
        public bool ShouldSerializeobject_id()
        {
            return (object_id != null);
        }
        public bool? persist = null;
        public bool ShouldSerializepersist()
        {
            return (persist != null);
        }
        public string type = null;
        public bool ShouldSerializetype()
        {
            return (type != null);
        }
        public string action = null;
        public bool ShouldSerializeaction()
        {
            return (action != null);
        }
        public float? ttl = null;
        public bool ShouldSerializettl()
        {
            return (ttl != null);
        }
        public bool? overwrite = null;
        public bool ShouldSerializeoverwrite()
        {
            return (overwrite != null);
        }
        public string timestamp = null;
        public bool ShouldSerializetimestamp()
        {
            return (timestamp != null);
        }

        // TODO (mwfarb): consolidate handling of data vs attributes since they are the same
        public object data = null;
        public bool ShouldSerializedata()
        {
            return (data != null);
        }
        public object attributes = null;
        public bool ShouldSerializeattributes()
        {
            return (attributes != null);
        }

        // TODO (mwfarb): remove displayName from transaction level object
        public string displayName = null;
        public bool ShouldSerializedisplayName()
        {
            return (displayName != null);
        }

    }
}
