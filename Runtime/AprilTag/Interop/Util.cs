/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2024, Carnegie Mellon University. All rights reserved.
 *
 * Adapted from jp.keijiro.apriltag (BSD-2-Clause)
 * https://github.com/keijiro/jp.keijiro.apriltag
 */

namespace ArenaUnity.AprilTag.Interop
{
    static class Util
    {
        public unsafe static ref T AsRef<T>(void* source) where T : unmanaged
          => ref Unity.Collections.LowLevel.Unsafe.UnsafeUtility.AsRef<T>(source);

        public unsafe static void* AsPointer<T>(ref T value) where T : unmanaged
          => Unity.Collections.LowLevel.Unsafe.UnsafeUtility.AddressOf(ref value);
    }
}
