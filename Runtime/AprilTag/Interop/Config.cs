/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2024, Carnegie Mellon University. All rights reserved.
 *
 * Adapted from jp.keijiro.apriltag (BSD-2-Clause)
 * https://github.com/keijiro/jp.keijiro.apriltag
 */

namespace ArenaUnity.AprilTag.Interop
{
    static class Config
    {
#if UNITY_EDITOR || !UNITY_IOS
        public const string DllName = "ArenaAprilTag";
#else
        public const string DllName = "__Internal";
#endif
    }
}
