/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2023, Carnegie Mellon University. All rights reserved.
 */

using System;
using System.Globalization;
using System.Text;

namespace ArenaUnity
{
    /// <summary>
    /// Converts an antimatter15-style <c>.splat</c> file into an equivalent in-memory 3DGS
    /// binary PLY, so that a splat renderer which can only read PLY still accepts <c>.splat</c>.
    ///
    /// The <c>.splat</c> container is a flat array of 32-byte records produced by
    /// https://github.com/antimatter15/splat/blob/main/convert.py from a 3DGS PLY:
    ///   bytes  0..11  position   x, y, z            float32
    ///   bytes 12..23  scale      x, y, z            float32, already exp()'d (linear)
    ///   bytes 24..27  color      r, g, b, a         uint8,  (0.5 + SH_C0*f_dc)*255 and sigmoid(opacity)*255
    ///   bytes 28..31  rotation   w, x, y, z         uint8,  quaternion*128 + 128
    /// This class inverts each of those four steps, which is why the encode constants below
    /// have to match convert.py exactly.
    ///
    /// No axis or handedness correction happens here: the output PLY carries the same
    /// coordinate frame as the source <c>.splat</c>, and therefore the same frame as the
    /// 3DGS PLY it was converted from. Frame correction is the renderer's job (see
    /// ArenaWireGaussianSplatting), so that <c>.ply</c>, <c>.spz</c> and <c>.splat</c> all get
    /// one correction and never two.
    ///
    /// Deliberately free of any UnityEngine dependency so the arithmetic can be exercised
    /// outside a Unity Editor.
    /// </summary>
    public static class ArenaSplatFile
    {
        /// <summary>Bytes per splat in the <c>.splat</c> container.</summary>
        public const int SplatRecordSize = 32;

        /// <summary>
        /// Properties per vertex in the PLY this class writes. The standard 3DGS PLY layout
        /// (x y z nx ny nz f_dc_0..2 opacity scale_0..2 rot_0..3) with no SH rest terms.
        /// Unused normals are written as zero rather than omitted only to keep the record layout
        /// the same as a standard SH-degree-0 3DGS export. gsplat would parse either form: it
        /// takes every field offset from the header and derives the SH band count by counting
        /// f_rest_ properties (GsplatAsset.PlyHeaderInfo), not from the total property count.
        /// </summary>
        public const int PlyPropertyCount = 17;

        /// <summary>Bytes per vertex in the PLY this class writes.</summary>
        public const int PlyRecordSize = PlyPropertyCount * 4;

        /// <summary>
        /// Band-0 spherical-harmonic constant, 1/(2*sqrt(pi)). Same value as
        /// Editor/SPLATFileReader.cs and antimatter15/splat convert.py.
        /// </summary>
        public const float ShC0 = 0.28209479177387814f;

        // A .splat alpha byte of 0 or 255 inverts to -inf / +inf through the logit, so the
        // normalized alpha is clamped first. 1/2048 keeps the round trip exact under the
        // reader's round(sigmoid(opacity)*255): sigmoid(logit(1/2048))*255 rounds back to 0
        // and sigmoid(logit(1-1/2048))*255 rounds back to 255.
        const float AlphaEpsilon = 1f / 2048f;

        // A .splat scale of 0 inverts to -inf through the log. Renderers clamp tiny scales to
        // invisible anyway, so any sufficiently small positive value is equivalent.
        const float MinLinearScale = 1e-9f;

        /// <summary>
        /// True when <paramref name="splatBytes"/> could be a <c>.splat</c> container, i.e. a
        /// non-empty whole number of 32-byte records small enough to convert. This only checks
        /// the framing; the format carries no magic number, so a wrong-format file whose length
        /// happens to be a multiple of 32 cannot be rejected here.
        /// </summary>
        public static bool TryGetSplatCount(byte[] splatBytes, out int splatCount, out string error)
        {
            splatCount = 0;
            if (splatBytes == null || splatBytes.Length == 0)
            {
                error = "file is empty";
                return false;
            }
            if (splatBytes.Length % SplatRecordSize != 0)
            {
                error = $"length {splatBytes.Length} is not a multiple of the {SplatRecordSize}-byte .splat record";
                return false;
            }
            long count = splatBytes.Length / SplatRecordSize;
            // The converted PLY is a single byte[], so it has to fit in one array.
            long plyBytes = count * PlyRecordSize + 512;
            if (plyBytes > int.MaxValue)
            {
                error = $"{count} splats would convert to {plyBytes} PLY bytes, over the {int.MaxValue}-byte array limit";
                return false;
            }
            splatCount = (int)count;
            error = null;
            return true;
        }

        /// <summary>
        /// Converts <c>.splat</c> bytes to 3DGS binary little-endian PLY bytes.
        /// </summary>
        /// <exception cref="ArgumentException">
        /// <paramref name="splatBytes"/> is not a plausible <c>.splat</c> container.
        /// </exception>
        public static byte[] ToPlyBytes(byte[] splatBytes)
        {
            if (!TryGetSplatCount(splatBytes, out int splatCount, out string error))
                throw new ArgumentException($"not a .splat file: {error}", nameof(splatBytes));

            byte[] header = Encoding.ASCII.GetBytes(BuildPlyHeader(splatCount));
            byte[] ply = new byte[header.Length + (long)splatCount * PlyRecordSize];
            Buffer.BlockCopy(header, 0, ply, 0, header.Length);

            int write = header.Length;
            for (int i = 0; i < splatCount; i++)
            {
                int read = i * SplatRecordSize;

                // position, unchanged
                WriteFloatLE(ply, write + 0, ReadFloatLE(splatBytes, read + 0));
                WriteFloatLE(ply, write + 4, ReadFloatLE(splatBytes, read + 4));
                WriteFloatLE(ply, write + 8, ReadFloatLE(splatBytes, read + 8));

                // normals: unused by 3DGS, written as zero (already zero from array init)

                // color: uint8 -> SH band-0 coefficient
                WriteFloatLE(ply, write + 24, ByteToSh(splatBytes[read + 24]));
                WriteFloatLE(ply, write + 28, ByteToSh(splatBytes[read + 25]));
                WriteFloatLE(ply, write + 32, ByteToSh(splatBytes[read + 26]));

                // opacity: post-sigmoid uint8 -> pre-sigmoid logit
                WriteFloatLE(ply, write + 36, ByteToLogit(splatBytes[read + 27]));

                // scale: linear -> log
                WriteFloatLE(ply, write + 40, LinearToLogScale(ReadFloatLE(splatBytes, read + 12)));
                WriteFloatLE(ply, write + 44, LinearToLogScale(ReadFloatLE(splatBytes, read + 16)));
                WriteFloatLE(ply, write + 48, LinearToLogScale(ReadFloatLE(splatBytes, read + 20)));

                // rotation: uint8 wxyz -> normalized float wxyz, in PLY's rot_0..rot_3 order
                DecodeQuaternion(
                    splatBytes[read + 28], splatBytes[read + 29],
                    splatBytes[read + 30], splatBytes[read + 31],
                    out float qw, out float qx, out float qy, out float qz);
                WriteFloatLE(ply, write + 52, qw);
                WriteFloatLE(ply, write + 56, qx);
                WriteFloatLE(ply, write + 60, qy);
                WriteFloatLE(ply, write + 64, qz);

                write += PlyRecordSize;
            }

            return ply;
        }

        internal static string BuildPlyHeader(int splatCount)
        {
            // Readers split header lines on a single space and compare "end_header" exactly,
            // so keep single spaces, LF endings, and no trailing whitespace.
            var sb = new StringBuilder();
            sb.Append("ply\n");
            sb.Append("format binary_little_endian 1.0\n");
            sb.Append("comment converted from .splat by arena-unity\n");
            sb.Append("element vertex ").Append(splatCount.ToString(CultureInfo.InvariantCulture)).Append('\n');
            foreach (string property in new[]
                     {
                         "x", "y", "z",
                         "nx", "ny", "nz",
                         "f_dc_0", "f_dc_1", "f_dc_2",
                         "opacity",
                         "scale_0", "scale_1", "scale_2",
                         "rot_0", "rot_1", "rot_2", "rot_3",
                     })
            {
                sb.Append("property float ").Append(property).Append('\n');
            }
            sb.Append("end_header\n");
            return sb.ToString();
        }

        internal static float ByteToSh(byte channel)
        {
            // inverse of (0.5 + SH_C0 * f_dc) * 255
            return (channel / 255f - 0.5f) / ShC0;
        }

        internal static float ByteToLogit(byte alpha)
        {
            // inverse of sigmoid(opacity) * 255
            float a = alpha / 255f;
            if (a < AlphaEpsilon) a = AlphaEpsilon;
            else if (a > 1f - AlphaEpsilon) a = 1f - AlphaEpsilon;
            return (float)Math.Log(a / (1f - a));
        }

        internal static float LinearToLogScale(float linearScale)
        {
            // inverse of exp(scale_n); NaN would propagate into the renderer's bounds, so map it
            // to the same invisible floor as zero.
            if (float.IsNaN(linearScale) || linearScale < MinLinearScale) linearScale = MinLinearScale;
            return (float)Math.Log(linearScale);
        }

        internal static void DecodeQuaternion(byte bw, byte bx, byte by, byte bz,
            out float qw, out float qx, out float qy, out float qz)
        {
            // inverse of quaternion * 128 + 128
            float w = (bw - 128f) / 128f;
            float x = (bx - 128f) / 128f;
            float y = (by - 128f) / 128f;
            float z = (bz - 128f) / 128f;
            double length = Math.Sqrt((double)w * w + (double)x * x + (double)y * y + (double)z * z);
            if (length < 1e-6)
            {
                // all four bytes landed on the quantization center; identity is the only safe guess
                qw = 1f;
                qx = qy = qz = 0f;
                return;
            }
            float inv = (float)(1.0 / length);
            qw = w * inv;
            qx = x * inv;
            qy = y * inv;
            qz = z * inv;
        }

        static float ReadFloatLE(byte[] src, int offset)
        {
            int bits = src[offset]
                       | (src[offset + 1] << 8)
                       | (src[offset + 2] << 16)
                       | (src[offset + 3] << 24);
            return BitConverter.Int32BitsToSingle(bits);
        }

        static void WriteFloatLE(byte[] dst, int offset, float value)
        {
            int bits = BitConverter.SingleToInt32Bits(value);
            dst[offset] = (byte)bits;
            dst[offset + 1] = (byte)(bits >> 8);
            dst[offset + 2] = (byte)(bits >> 16);
            dst[offset + 3] = (byte)(bits >> 24);
        }
    }
}
