using System;

namespace ArenaUnity
{
    public class ArenaMqttAuthJson
    {
        public string username { get; set; }
        public string token { get; set; }
        public ArenaMqttAuthIdsJson ids { get; set; }
    }
}