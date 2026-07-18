/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2024, Carnegie Mellon University. All rights reserved.
 *
 * Adapted from jp.keijiro.apriltag (BSD-2-Clause)
 * https://github.com/keijiro/jp.keijiro.apriltag
 */

using System;
using Unity.Burst;
using Color32 = UnityEngine.Color32;
using ImageU8 = ArenaUnity.AprilTag.Interop.ImageU8;

namespace ArenaUnity.AprilTag
{
    [BurstCompile]
    static class ImageConverter
    {
        public unsafe static void Convert(ReadOnlySpan<Color32> data, ImageU8 image)
        {
            fixed (Color32* src = &data.GetPinnableReference())
                fixed (byte* dst = &image.Buffer.GetPinnableReference())
                    BurstConvert(src, dst, image.Width, image.Height, image.Stride);
        }

        [BurstCompile]
        unsafe static void BurstConvert
          (Color32* src, byte* dst, int width, int height, int stride)
        {
            var offs_src = 0;
            var offs_dst = stride * (height - 1);

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                    dst[offs_dst + x] = src[offs_src + x].g;

                offs_src += width;
                offs_dst -= stride;
            }
        }
    }
}
