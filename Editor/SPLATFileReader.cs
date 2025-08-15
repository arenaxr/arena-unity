// edit from: https://github.com/aras-p/UnityGaussianSplatting/blob/main/package/Editor/Utils/SPZFileReader.cs
// edit from: https://github.com/keijiro/SplatVFX/blob/main/jp.keijiro.splat-vfx/Editor/SplatImporter.cs

using System.IO;
using Unity.Collections;
using System.IO.Compression;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;
using System;
using Unity.Jobs;
using System.Runtime.InteropServices;

#if LIB_GAUSSIAN_SPLATTING
using GaussianSplatting.Runtime;
using GaussianSplatting.Editor.Utils;
#endif

namespace ArenaUnity.Editor
{
#if LIB_GAUSSIAN_SPLATTING
    [BurstCompile]
    public static class SPLATFileReader
    {
        struct SplatHeader
        {
            public uint magic; // 0x5053474e "NGSP"
            public uint version; // 2
            public uint numPoints;
            public uint sh_fracbits_flags_reserved;
        };
        public static void ReadFileHeader(string filePath, out int vertexCount)
        {
            vertexCount = 0;
            if (!File.Exists(filePath))
                return;
            using var fs = File.OpenRead(filePath);
            using var gz = new GZipStream(fs, CompressionMode.Decompress);
            ReadHeaderImpl(filePath, gz, out vertexCount, out _, out _, out _);
        }

        static void ReadHeaderImpl(string filePath, Stream fs, out int vertexCount, out int shLevel, out int fractBits, out int flags)
        {
            var header = new NativeArray<SplatHeader>(1, Allocator.Temp);
            var readBytes = fs.Read(header.Reinterpret<byte>(16));
            if (readBytes != 16)
                throw new IOException($"SPLAT {filePath} read error, failed to read header");

            if (header[0].magic != 0x5053474e)
                throw new IOException($"SPLAT {filePath} read error, header magic unexpected {header[0].magic}");
            if (header[0].version != 2)
                throw new IOException($"SPLAT {filePath} read error, header version unexpected {header[0].version}");

            vertexCount = (int)header[0].numPoints;
            shLevel = (int)(header[0].sh_fracbits_flags_reserved & 0xFF);
            fractBits = (int)((header[0].sh_fracbits_flags_reserved >> 8) & 0xFF);
            flags = (int)((header[0].sh_fracbits_flags_reserved >> 16) & 0xFF);
        }

        static int SHCoeffsForLevel(int level)
        {
            return level switch
            {
                0 => 0,
                1 => 3,
                2 => 8,
                3 => 15,
                _ => 0
            };
        }

        public static void ReadFileSPZ(string filePath, out NativeArray<InputSplatData> splats)
        {
            using var fs = File.OpenRead(filePath);
            using var gz = new GZipStream(fs, CompressionMode.Decompress);
            ReadHeaderImpl(filePath, gz, out var splatCount, out var shLevel, out var fractBits, out var flags);

            if (splatCount < 1 || splatCount > 10_000_000) // 10M hardcoded in SPLAT code
                throw new IOException($"SPLAT {filePath} read error, out of range splat count {splatCount}");
            if (shLevel < 0 || shLevel > 3)
                throw new IOException($"SPLAT {filePath} read error, out of range SH level {shLevel}");
            if (fractBits < 0 || fractBits > 24)
                throw new IOException($"SPLAT {filePath} read error, out of range fractional bits {fractBits}");

            // allocate temporary storage
            int shCoeffs = SHCoeffsForLevel(shLevel);
            NativeArray<byte> packedPos = new(splatCount * 3 * 3, Allocator.Persistent);
            NativeArray<byte> packedScale = new(splatCount * 3, Allocator.Persistent);
            NativeArray<byte> packedRot = new(splatCount * 3, Allocator.Persistent);
            NativeArray<byte> packedAlpha = new(splatCount, Allocator.Persistent);
            NativeArray<byte> packedCol = new(splatCount * 3, Allocator.Persistent);
            NativeArray<byte> packedSh = new(splatCount * 3 * shCoeffs, Allocator.Persistent);

            // read file contents into temporaries
            bool readOk = true;
            readOk &= gz.Read(packedPos) == packedPos.Length;
            readOk &= gz.Read(packedAlpha) == packedAlpha.Length;
            readOk &= gz.Read(packedCol) == packedCol.Length;
            readOk &= gz.Read(packedScale) == packedScale.Length;
            readOk &= gz.Read(packedRot) == packedRot.Length;
            readOk &= gz.Read(packedSh) == packedSh.Length;

            // unpack into full splat data
            splats = new NativeArray<InputSplatData>(splatCount, Allocator.Persistent);
            UnpackDataJob job = new UnpackDataJob();
            job.packedPos = packedPos;
            job.packedScale = packedScale;
            job.packedRot = packedRot;
            job.packedAlpha = packedAlpha;
            job.packedCol = packedCol;
            job.packedSh = packedSh;
            job.shCoeffs = shCoeffs;
            job.fractScale = 1.0f / (1 << fractBits);
            job.splats = splats;
            job.Schedule(splatCount, 4096).Complete();

            // cleanup
            packedPos.Dispose();
            packedScale.Dispose();
            packedRot.Dispose();
            packedAlpha.Dispose();
            packedCol.Dispose();
            packedSh.Dispose();

            if (!readOk)
            {
                splats.Dispose();
                throw new IOException($"SPLAT {filePath} read error, file smaller than it should be");
            }
        }

        [BurstCompile]
        struct UnpackDataJob : IJobParallelFor
        {
            [NativeDisableParallelForRestriction] [ReadOnly] public NativeArray<byte> packedPos;
            [NativeDisableParallelForRestriction] [ReadOnly] public NativeArray<byte> packedScale;
            [NativeDisableParallelForRestriction] [ReadOnly] public NativeArray<byte> packedRot;
            [NativeDisableParallelForRestriction] [ReadOnly] public NativeArray<byte> packedAlpha;
            [NativeDisableParallelForRestriction] [ReadOnly] public NativeArray<byte> packedCol;
            [NativeDisableParallelForRestriction] [ReadOnly] public NativeArray<byte> packedSh;
            public float fractScale;
            public int shCoeffs;
            public NativeArray<InputSplatData> splats;

            public void Execute(int index)
            {
                var splat = splats[index];

                splat.pos = new Vector3(UnpackFloat(index * 3 + 0) * fractScale, UnpackFloat(index * 3 + 1) * fractScale, UnpackFloat(index * 3 + 2) * fractScale);

                splat.scale = new Vector3(packedScale[index * 3 + 0], packedScale[index * 3 + 1], packedScale[index * 3 + 2]) / 16.0f - new Vector3(10.0f, 10.0f, 10.0f);
                splat.scale = GaussianUtils.LinearScale(splat.scale);

                Vector3 xyz = new Vector3(packedRot[index * 3 + 0], packedRot[index * 3 + 1], packedRot[index * 3 + 2]) * (1.0f / 127.5f) - new Vector3(1, 1, 1);
                float w = math.sqrt(math.max(0.0f, 1.0f - xyz.sqrMagnitude));
                var q = new float4(xyz.x, xyz.y, xyz.z, w);
                var qq = math.normalize(q);
                qq = GaussianUtils.PackSmallest3Rotation(qq);
                splat.rot = new Quaternion(qq.x, qq.y, qq.z, qq.w);

                splat.opacity = packedAlpha[index] / 255.0f;

                Vector3 col = new Vector3(packedCol[index * 3 + 0], packedCol[index * 3 + 1], packedCol[index * 3 + 2]);
                col = col / 255.0f - new Vector3(0.5f, 0.5f, 0.5f);
                col /= 0.15f;
                splat.dc0 = GaussianUtils.SH0ToColor(col);

                int shIdx = index * shCoeffs * 3;
                splat.sh1 = UnpackSH(shIdx); shIdx += 3;
                splat.sh2 = UnpackSH(shIdx); shIdx += 3;
                splat.sh3 = UnpackSH(shIdx); shIdx += 3;
                splat.sh4 = UnpackSH(shIdx); shIdx += 3;
                splat.sh5 = UnpackSH(shIdx); shIdx += 3;
                splat.sh6 = UnpackSH(shIdx); shIdx += 3;
                splat.sh7 = UnpackSH(shIdx); shIdx += 3;
                splat.sh8 = UnpackSH(shIdx); shIdx += 3;
                splat.sh9 = UnpackSH(shIdx); shIdx += 3;
                splat.shA = UnpackSH(shIdx); shIdx += 3;
                splat.shB = UnpackSH(shIdx); shIdx += 3;
                splat.shC = UnpackSH(shIdx); shIdx += 3;
                splat.shD = UnpackSH(shIdx); shIdx += 3;
                splat.shE = UnpackSH(shIdx); shIdx += 3;
                splat.shF = UnpackSH(shIdx); shIdx += 3;

                splats[index] = splat;
            }

            float UnpackFloat(int idx)
            {
                int fx = packedPos[idx * 3 + 0] | (packedPos[idx * 3 + 1] << 8) | (packedPos[idx * 3 + 2] << 16);
                fx |= (fx & 0x800000) != 0 ? -16777216 : 0; // sign extension with 0xff000000
                return fx;
            }

            Vector3 UnpackSH(int idx)
            {
                Vector3 sh = new Vector3(packedSh[idx], packedSh[idx + 1], packedSh[idx + 2]) - new Vector3(128.0f, 128.0f, 128.0f);
                sh /= 128.0f;
                return sh;
            }
        }


        #region Reader implementation

        static SplatData ImportAsSplatData(string path)
        {
            var data = ScriptableObject.CreateInstance<SplatData>();
            data.name = Path.GetFileNameWithoutExtension(path);

            var arrays = LoadDataArrays(path);
            data.PositionArray = arrays.position;
            data.AxisArray = arrays.axis;
            data.ColorArray = arrays.color;
            data.ReleaseGpuResources();

            return data;
        }

#pragma warning disable CS0649

        struct ReadData
        {
            public float px, py, pz;
            public float sx, sy, sz;
            public byte r, g, b, a;
            public byte rw, rx, ry, rz;
        }

#pragma warning restore CS0649

        static (Vector3[] position, Vector3[] axis, Color[] color)
               LoadDataArrays(string path)
        {
            var bytes = (Span<byte>)File.ReadAllBytes(path);
            var count = bytes.Length / 32;

            var source = MemoryMarshal.Cast<byte, ReadData>(bytes);

            var position = new Vector3[count];
            var axis = new Vector3[count * 3];
            var color = new Color[count];

            for (var i = 0; i < count; i++)
                ParseReadData(source[i],
                              out position[i],
                              out axis[i * 3],
                              out axis[i * 3 + 1],
                              out axis[i * 3 + 2],
                              out color[i]);

            return (position, axis, color);
        }

        [BurstCompile]
        static void ParseReadData(in ReadData src,
                           out Vector3 position,
                           out Vector3 axis1,
                           out Vector3 axis2,
                           out Vector3 axis3,
                           out Color color)
        {
            var rv = (math.float4(src.rx, src.ry, src.rz, src.rw) - 128) / 128;
            var q = math.quaternion(-rv.x, -rv.y, rv.z, rv.w);
            position = math.float3(src.px, src.py, -src.pz);
            axis1 = math.mul(q, math.float3(src.sx, 0, 0));
            axis2 = math.mul(q, math.float3(0, src.sy, 0));
            axis3 = math.mul(q, math.float3(0, 0, src.sz));
            color = (Vector4)math.float4(src.r, src.g, src.b, src.a) / 255;
        }

        #endregion

        public static void ReadFile(string filePath, out NativeArray<InputSplatData> splats)
        {
            var bytes = (Span<byte>)File.ReadAllBytes(filePath);
            var count = bytes.Length / 32;
            splats = new NativeArray<InputSplatData>(count, Allocator.Persistent);

            var source = MemoryMarshal.Cast<byte, ReadData>(bytes);

            for (var i = 0; i < count; i++)
            {
                var src = source[i];

                //var rv = (math.float4(src.rx, src.ry, src.rz, src.rw) - 128) / 128;
                //var q = math.quaternion(-rv.x, -rv.y, rv.z, rv.w);
                //position = math.float3(src.px, src.py, -src.pz);
                //axis1 = math.mul(q, math.float3(src.sx, 0, 0));
                //axis2 = math.mul(q, math.float3(0, src.sy, 0));
                //axis3 = math.mul(q, math.float3(0, 0, src.sz));
                //color = (Vector4)math.float4(src.r, src.g, src.b, src.a) / 255;

                var splat = new InputSplatData();

                splat.pos = new Vector3(src.px, src.py, src.pz); ;

                //splat.scale = new Vector3(src.sx, src.sy, src.sz) / 16.0f - new Vector3(10.0f, 10.0f, 10.0f);
                //splat.scale = GaussianUtils.LinearScale(splat.scale);
                splat.scale = new Vector3(src.sx, src.sy, src.sz);

                var q = new float4(src.rx, src.ry, src.rz, src.rw);
                var qq = math.normalize(q);
                qq = GaussianUtils.PackSmallest3Rotation(qq);
                splat.rot = new Quaternion(qq.x, qq.y, qq.z, qq.w);

                splat.opacity = src.a / 255.0f;

                Vector3 col = new Vector3(src.r, src.g, src.b);
                col = col / 255.0f - new Vector3(0.5f, 0.5f, 0.5f);
                col /= 0.15f;
                splat.dc0 = GaussianUtils.SH0ToColor(col);

                splats[i] = splat;
            }

        }
    }
#endif 
}
