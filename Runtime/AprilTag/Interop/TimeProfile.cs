/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2024, Carnegie Mellon University. All rights reserved.
 *
 * Adapted from jp.keijiro.apriltag (BSD-2-Clause)
 * https://github.com/keijiro/jp.keijiro.apriltag
 */

using System;
using System.Runtime.InteropServices;

namespace ArenaUnity.AprilTag.Interop
{
    [StructLayoutAttribute(LayoutKind.Sequential)]
    public unsafe struct TimeProfileEntry
    {
        fixed byte name[32];
        long utime;

        public string Name => ConvertName();
        public long UTime => utime;

        unsafe string ConvertName()
        {
            fixed (byte* p = name) return Marshal.PtrToStringAnsi((IntPtr)p);
        }
    }

    [StructLayoutAttribute(LayoutKind.Sequential)]
    public struct TimeProfile
    {
        long utime;
        IntPtr stamps;

        public long UTime => utime;

        public unsafe Span<TimeProfileEntry> Stamps
          => ((ZArray<TimeProfileEntry>*)stamps)->AsSpan;
    }
}
