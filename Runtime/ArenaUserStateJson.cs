using System;

namespace ArenaUnity
{
    public class ArenaUserStateJson
    {
        public bool authenticated { get; set; }
        public string username { get; set; }
        public string fullname { get; set; }
        public string email { get; set; }
        public string type { get; set; }
        public bool is_staff { get; set; }
    }

}