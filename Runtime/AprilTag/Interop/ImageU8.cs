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
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace ArenaUnity.AprilTag.Interop
{
    public sealed class ImageU8 : SafeHandleZeroOrMinusOneIsInvalid
    {
        ImageU8() : base(true) {}

        protected override bool ReleaseHandle()
        {
            _Destroy(handle);
            return true;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct InternalData
        {
            internal int width;
            internal int height;
            internal int stride;
            internal IntPtr buf;
        }

        unsafe ref InternalData Data
          => ref Util.AsRef<InternalData>((void*)handle);

        public int Width => Data.width;
        public int Height => Data.height;
        public int Stride => Data.stride;

        unsafe public Span<byte> Buffer
          => new Span<byte>((void*)Data.buf, Stride * Height);

        public static ImageU8 Create(int width, int height)
          => _Create((uint)width, (uint)height);

        [DllImport(Config.DllName, EntryPoint = "image_u8_create_stride")]
        static extern ImageU8 _CreateStride(uint width, uint height, uint stride);

        [DllImport(Config.DllName, EntryPoint = "image_u8_create")]
        static extern ImageU8 _Create(uint width, uint height);

        [DllImport(Config.DllName, EntryPoint = "image_u8_destroy")]
        static extern void _Destroy(IntPtr image);
    }
}
