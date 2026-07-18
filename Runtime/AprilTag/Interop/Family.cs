/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2024, Carnegie Mellon University. All rights reserved.
 *
 * Adapted from jp.keijiro.apriltag (BSD-2-Clause)
 * https://github.com/keijiro/jp.keijiro.apriltag
 *
 * Adds tag36h11 family support for ARENA scene relocalization.
 */

using System;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace ArenaUnity.AprilTag.Interop
{
    public sealed class Family : SafeHandleZeroOrMinusOneIsInvalid
    {
        Family() : base(true) {}

        protected override bool ReleaseHandle()
        {
            _DestroyTag36h11(handle);
            return true;
        }

        /// <summary>Creates a tag36h11 family used by ARENA for scene origin localization.</summary>
        public static Family CreateTag36h11() => _CreateTag36h11();

        [DllImport(Config.DllName, EntryPoint = "tag36h11_create")]
        static extern Family _CreateTag36h11();

        [DllImport(Config.DllName, EntryPoint = "tag36h11_destroy")]
        static extern void _DestroyTag36h11(IntPtr ptr);
    }
}
