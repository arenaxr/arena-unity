using System;

namespace ArenaUnity
{
    //TODO (mwfarb): move ArenaHandJson to automated schema when the schema translator is updated
    [Serializable]
    public class ArenaHandJson
    {
        public string url { get; set; }
        public string dep { get; set; }
    }
}
