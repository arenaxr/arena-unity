/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2024, Carnegie Mellon University. All rights reserved.
 *
 * Adapted from glTFast DocExamples/WebP.cs (Apache-2.0)
 * https://github.com/Unity-Technologies/com.unity.cloud.gltfast
 *
 * Static WebP decoder wrapping libwebp native calls. Uses Unity Jobs
 * for async decode without blocking the main thread.
 */

using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Profiling;

namespace ArenaUnity
{
    /// <summary>
    /// Decodes WebP image data into a Unity Texture2D using the native libwebp plugin.
    /// The decode operation runs on a worker thread via Unity's Job System.
    /// </summary>
    static class ArenaWebpDecoder
    {
        /// <summary>
        /// Decodes WebP data into a Texture2D.
        /// </summary>
        /// <param name="data">Raw WebP image bytes.</param>
        /// <param name="linear">True for linear color space, false for sRGB.</param>
        /// <param name="readable">True to keep the texture CPU-readable after upload.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Decoded Texture2D, or null on failure.</returns>
        public static async Task<Texture2D> Decode(
            NativeArray<byte>.ReadOnly data,
            bool linear,
            bool readable,
            CancellationToken cancellationToken
            )
        {
            if (!TryGetInfo(data, out var width, out var height))
            {
                return null;
            }

            Profiler.BeginSample("ArenaWebP.CreateTexture2D");
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false, linear);
            var textureData = texture.GetRawTextureData<byte>();
            using var result = new NativeArray<IntPtr>(1, Allocator.Persistent);
            var job = new WebPDecodeJob
            {
                data = data,
                textureData = textureData,
                width = width,
                result = result
            }.Schedule();
            Profiler.EndSample();

            while (!job.IsCompleted)
            {
                await Task.Yield();
            }
            job.Complete();

            if (result[0] == IntPtr.Zero)
            {
                UnityEngine.Object.Destroy(texture);
                Debug.LogError("[ArenaWebP] Failed to decode WebP image data.");
                return null;
            }

            Profiler.BeginSample("ArenaWebP.Apply");
            texture.Apply(false, !readable);
            Profiler.EndSample();
            return texture;
        }

        static unsafe bool TryGetInfo(NativeArray<byte>.ReadOnly data, out int width, out int height)
        {
            width = 0;
            height = 0;
            try
            {
                var returnValue = WebPGetInfo(
                    (byte*)data.GetUnsafeReadOnlyPtr(),
                    (uint)data.Length,
                    ref width,
                    ref height);
                return returnValue != 0;
            }
            catch (DllNotFoundException)
            {
                Debug.LogError(
                    "[ArenaWebP] Native libwebp plugin not found. "
                    + "Ensure the 'webp-unity' native plugin is installed for your target platform. "
                    + "See Runtime/WebP/Plugin/README.md for build instructions.");
            }

            return false;
        }

        unsafe struct WebPDecodeJob : IJob
        {
            [WriteOnly]
            public NativeArray<IntPtr> result;

            [ReadOnly]
            public NativeArray<byte>.ReadOnly data;

            [WriteOnly]
            public NativeArray<byte> textureData;

            public int width;

            public void Execute()
            {
                var decodeResult = WebPDecodeRGBAInto(
                    (byte*)data.GetUnsafeReadOnlyPtr(), (uint)data.Length,
                    (byte*)textureData.GetUnsafePtr(), (uint)textureData.Length,
                    sizeof(Color32) * width
                );

                result[0] = decodeResult;
            }
        }

        // libwebp P/Invoke declarations
        // https://chromium.googlesource.com/webm/libwebp

        [DllImport("webp-unity")]
        public static extern unsafe int WebPGetInfo(byte* data, uint size, ref int width, ref int height);

        [DllImport("webp-unity")]
        public static extern unsafe IntPtr WebPDecodeRGBAInto(
            byte* data, uint size,
            byte* outputBuffer, uint outputBufferSize,
            int outputStride);
    }
}
