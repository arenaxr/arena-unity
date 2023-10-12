/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2023, Carnegie Mellon University. All rights reserved.
 */

using System;

namespace ArenaUnity
{
    [Serializable]
    public class ArenaMqttTokenClaimsJson
    {
        public string sub { get; set; }
        public int exp { get; set; }
        public string aud { get; set; }
        public string iss { get; set; }
        public string room { get; set; }
        public string[] subs { get; set; }
        public string[] publ { get; set; }
    }
}
