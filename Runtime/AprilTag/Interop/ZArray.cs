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
    public struct ZArray<T> where T : unmanaged
    {
        ulong el_sz;
        int size;
        int alloc;
        IntPtr data;

        public unsafe Span<T> AsSpan => new Span<T>((void*)data, size);
    }
}
