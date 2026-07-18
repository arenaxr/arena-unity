/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2024, Carnegie Mellon University. All rights reserved.
 *
 * Adapted from jp.keijiro.apriltag (BSD-2-Clause)
 * https://github.com/keijiro/jp.keijiro.apriltag
 */

using System;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace ArenaUnity.AprilTag.Interop
{
    public sealed class DetectionArray : SafeHandleZeroOrMinusOneIsInvalid
    {
        DetectionArray() : base(true) {}

        protected override bool ReleaseHandle()
        {
            _Destroy(handle);
            return true;
        }

        unsafe ref ZArray<IntPtr> AsPointerArray
          => ref Util.AsRef<ZArray<IntPtr>>((void*)handle);

        public int Length => AsPointerArray.AsSpan.Length;

        public unsafe ref Detection this[int i]
          => ref Util.AsRef<Detection>((void*)AsPointerArray.AsSpan[i]);

        [DllImport(Config.DllName, EntryPoint = "apriltag_detections_destroy")]
        static extern void _Destroy(IntPtr detections);
    }
}
