/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2023, Carnegie Mellon University. All rights reserved.
 */

using System;

namespace ArenaUnity
{
    [Serializable]
    public class ArenaMqttAuthJson
    {
        public string username { get; set; }
        public string token { get; set; }
        public ArenaMqttAuthIdsJson ids { get; set; }
    }
}
