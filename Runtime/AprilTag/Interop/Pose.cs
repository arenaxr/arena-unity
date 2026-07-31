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
    public struct Pose : IDisposable
    {
        IntPtr matd_r;
        IntPtr matd_t;

        unsafe public ref Matd3x3 R => ref Util.AsRef<Matd3x3>((void*)matd_r);
        unsafe public ref Matd3x1 t => ref Util.AsRef<Matd3x1>((void*)matd_t);

        public Pose(ref DetectionInfo info)
        {
            matd_r = matd_t = IntPtr.Zero;
            _Estimate(ref info, ref this);
        }

        public void Dispose()
        {
            if (matd_r != IntPtr.Zero) _MatdDestroy(matd_r);
            if (matd_t != IntPtr.Zero) _MatdDestroy(matd_t);
            matd_r = matd_t = IntPtr.Zero;
        }

        [DllImport(Config.DllName, EntryPoint = "matd_destroy")]
        static extern void _MatdDestroy(IntPtr matd);

        [DllImport(Config.DllName, EntryPoint = "estimate_tag_pose")]
        static extern double _Estimate(ref DetectionInfo info, ref Pose pose);
    }
}
