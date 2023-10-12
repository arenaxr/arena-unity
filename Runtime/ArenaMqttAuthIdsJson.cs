/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2023, Carnegie Mellon University. All rights reserved.
 */

using System;

namespace ArenaUnity
{
    [Serializable]
    public class ArenaMqttAuthIdsJson
    {
        public string userid { get; set; }
        public string camid { get; set; }
    }
}
