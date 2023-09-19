﻿/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2023, Carnegie Mellon University. All rights reserved.
 */

using System;

namespace ArenaUnity
{
    [Serializable]
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
