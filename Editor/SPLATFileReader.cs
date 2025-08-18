// edit from: https://github.com/aras-p/UnityGaussianSplatting/blob/main/package/Editor/Utils/SPZFileReader.cs
// edit from: https://github.com/keijiro/SplatVFX/blob/main/jp.keijiro.splat-vfx/Editor/SplatImporter.cs

using System.IO;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;
using System;
using System.Runtime.InteropServices;

#if LIB_GAUSSIAN_SPLATTING
using GaussianSplatting.Runtime;
using GaussianSplatting.Editor.Utils;
#endif

namespace ArenaUnity.Editor
{
#if LIB_GAUSSIAN_SPLATTING
    public static class SPLATFileReader
    {
        // Replicate https://github.com/antimatter15/splat/blob/main/convert.py
        const float SH_C0 = 0.28209479177387814f;

#pragma warning disable CS0649

        struct ReadData
        {
            public float px, py, pz;
            public float sx, sy, sz;
            public byte r, g, b, a;
            public byte rw, rx, ry, rz;
        }

#pragma warning restore CS0649

        [BurstCompile]
        public static void ReadFile(string filePath, out NativeArray<InputSplatData> splats)
        {
            var bytes = (Span<byte>)File.ReadAllBytes(filePath);
            var count = bytes.Length / 32;
            var source = MemoryMarshal.Cast<byte, ReadData>(bytes);

            splats = new NativeArray<InputSplatData>(count, Allocator.Persistent);
            for (var i = 0; i < count; i++)
            {
                var src = source[i];
                var splat = new InputSplatData();

                // position
                splat.pos = new Vector3(src.px, src.py, src.pz);

                // scale
                splat.scale = new Vector3(src.sx, src.sy, src.sz);

                // rotation
                var q = new float4(src.rx, src.ry, src.rz, src.rw);
                var qq = math.normalize(q);
                qq = GaussianUtils.PackSmallest3Rotation(qq);
                splat.rot = new Quaternion(qq.x, qq.y, qq.z, qq.w);

                // color
                Vector3 col = new Vector3(src.r, src.g, src.b);
                col = col / 255.0f - new Vector3(0.5f, 0.5f, 0.5f);
                col /= SH_C0;
                splat.dc0 = GaussianUtils.SH0ToColor(col);
                splat.opacity = src.a / 255.0f;

                splats[i] = splat;
            }

        }
    }
#endif
}
