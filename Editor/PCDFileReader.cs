// edit from: https://github.com/MaikelH/Pointcloud/blob/master/PointCloud/io/PCDReader.cs
// edit from: https://github.com/LimeRabbit/PcdReader/blob/master/PcdReader.cs

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Unity.Collections;
using UnityEngine;

#if LIB_GAUSSIAN_SPLATTING
using GaussianSplatting.Editor.Utils;
#endif

namespace ArenaUnity.Editor
{
    public static class PCDFileReader
    {
#if LIB_GAUSSIAN_SPLATTING
        /// <summary>
        /// Get header information
        /// </summary>
        /// <param name="s"></param>
        static Header GetHeader(string s)
        {
            var header = new Header();
            Regex reg_VERSION = new Regex("VERSION .*"); //Version
            Regex reg_FIELDS = new Regex("FIELDS .*"); //Field
            Regex reg_SIZE = new Regex("SIZE .*"); //Data size
            Regex reg_TYPE = new Regex("TYPE .*"); //Format of stored data F-Float U-uint
            Regex reg_COUNT = new Regex("COUNT .*");
            Regex reg_WIDTH = new Regex("WIDTH .*");
            Regex reg_HEIGHT = new Regex("HEIGHT .*");
            Regex reg_VIEWPOINT = new Regex("VIEWPOINT .*");
            Regex reg_POINTS = new Regex("POINTS .*"); //Number of points
            Regex reg_DATA = new Regex("DATA .*"); //Data type

            Match m_VERSION = reg_VERSION.Match(s);
            header.VERSION = m_VERSION.Value;

            header.FirstLine = "# .PCD v" + header.VERSION.Split(' ')[1] + " - Point Cloud Data file format";

            Match m_FIELDS = reg_FIELDS.Match(s);
            header.FIELDS = m_FIELDS.Value;

            Match m_SIZE = reg_SIZE.Match(s);
            header.SIZE = m_SIZE.Value;

            Match m_TYPE = reg_TYPE.Match(s);
            header.TYPE = m_TYPE.Value;

            Match m_COUNT = reg_COUNT.Match(s);
            header.COUNT = m_COUNT.Value;

            Match m_WIDTH = reg_WIDTH.Match(s);
            header.WIDTH = m_WIDTH.Value;

            Match m_HEIGHT = reg_HEIGHT.Match(s);
            header.HEIGHT = m_HEIGHT.Value;

            Match m_VIEWPOINT = reg_VIEWPOINT.Match(s);
            header.VIEWPOINT = m_VIEWPOINT.Value;

            Match m_POINTS = reg_POINTS.Match(s);
            header.POINTS = m_POINTS.Value;

            Match m_DATA = reg_DATA.Match(s);
            header.DATA = m_DATA.Value;

            return header;
        }

        /// <summary>
        /// Read binary and binary_compressed format pcd files
        /// </summary>
        public static void ReadFile(string path, out NativeArray<InputSplatData> splats)
        {
            List<Point3D> point3Ds;
            int pointCount = 0;

            byte[] bytes = File.ReadAllBytes(path); //Read file into byte array
            string text = Encoding.UTF8.GetString(bytes); //Convert to text to separate file header information

            var header = GetHeader(text);

            string dataType = header.DATA.Split(' ')[1]; //Stored data type, ascii binary binary_compressed

            int size = header.SIZE.Split(' ').Length - 1; //SIZE length, can be used to determine whether it contains RGB information

            pointCount = Convert.ToInt32(header.POINTS.Split(' ')[1]); //Number of points
            int index = text.IndexOf(header.DATA) + header.DATA.Length + 1; //Data start index
            point3Ds = new List<Point3D>();
            Point3D point;

            splats = new NativeArray<InputSplatData>(pointCount, Allocator.Persistent);

            // if (dataType == "ascii")
            // {
            //     //pointList = readData(sr);
            // }
            if (dataType == "binary")
            {
                //Binary file, just read it by byte array
                for (int i = index; i < bytes.Length;)
                {
                    point = new Point3D();
                    point.x = BitConverter.ToSingle(bytes, i);
                    point.y = BitConverter.ToSingle(bytes, i + 4);
                    point.z = BitConverter.ToSingle(bytes, i + 8);
                    if (size == 4)
                    {
                        point.color = BitConverter.ToUInt32(bytes, i + 12);
                    }
                    point3Ds.Add(point);

                    var splat = new InputSplatData();
                    splat.pos = new Vector3(point3Ds[i].x, point3Ds[i].y, point3Ds[i].z);
                    splat.dc0 = ConvertUInt32ToRGB(point3Ds[i].color);
                    splat.opacity = ConvertUInt32ToAlpha(point3Ds[i].color);
                    splats[i] = splat;

                    if (point3Ds.Count == pointCount) break;
                    i += 4 * size;
                }
            }
            else if (dataType == "binary_compressed")
            {
                //Binary compression, first parse the amount of data before and after compression
                int[] bys = new int[2];
                int dataIndex = 0;
                for (int i = 0; i < bys.Length; i++)
                {
                    bys[i] = BitConverter.ToInt32(bytes, index + i * 4);
                    dataIndex = index + i * 4;
                }
                dataIndex += 4; //Data start index
                int compressedSize = bys[0]; //Compressed length
                int decompressedSize = bys[1]; //Decompressed length

                //Take out the compressed data separately
                byte[] compress = new byte[compressedSize];
                //Copy bs, starting from index a, to the range [0 to compressedSize] of compress
                Array.Copy(bytes, dataIndex, compress, 0, compressedSize);

                //LZF decompression algorithm
                byte[] data = Decompress(compress, decompressedSize);
                int type = 0;
                for (int i = 0; i < data.Length; i += 4)
                {
                    //Read the x coordinate first
                    if (type == 0)
                    {
                        point = new Point3D();
                        point.x = BitConverter.ToSingle(data, i);
                        point3Ds.Add(point);
                        if (point3Ds.Count == pointCount) type++;
                    }
                    else if (type == 1) //y coordinate
                    {
                        point3Ds[i / 4 - pointCount].y = BitConverter.ToSingle(data, i);
                        if (i / 4 == pointCount * 2 - 1) type++;
                    }
                    else if (type == 2) //z coordinate
                    {
                        point3Ds[i / 4 - pointCount * 2].z = BitConverter.ToSingle(data, i);
                        if (i / 4 == pointCount * 3 - 1) type++;
                    }
                    else if (size == 4) //Color information
                    {
                        point3Ds[i / 4 - pointCount * 3].color = BitConverter.ToUInt32(data, i);
                        if (i / 4 == pointCount * 4 - 1) break;
                    }

                    var splat = new InputSplatData();
                    splat.pos = new Vector3(point3Ds[i].x, point3Ds[i].y, point3Ds[i].z);
                    splat.dc0 = ConvertUInt32ToRGB(point3Ds[i].color);
                    splat.opacity = ConvertUInt32ToAlpha(point3Ds[i].color);
                    splats[i] = splat;
                }
            }

        }

        public static Vector3 ConvertUInt32ToRGB(uint color)
        {
            // Extract the RGB components
            float r = (color >> 16) & 0xFF; // Red component
            float g = (color >> 8) & 0xFF;  // Green component
            float b = color & 0xFF;         // Blue component

            // Normalize to 0-1 range
            return new Vector3(r, g, b) / 255f;
        }

        public static float ConvertUInt32ToAlpha(uint color)
        {
            float a = (color >> 24) & 0xFF;
            return a;
        }

        /// <summary>
        /// Use LZF algorithm to decompress data
        /// </summary>
        /// <param name="input">Data to be decompressed</param>
        /// <param name="outputLength">Length after decompression</param>
        /// <returns>Returns the decompressed content</returns>
        public static byte[] Decompress(byte[] input, int outputLength)
        {
            uint iidx = 0;
            uint oidx = 0;
            int inputLength = input.Length;
            byte[] output = new byte[outputLength];
            do
            {
                uint ctrl = input[iidx++];

                if (ctrl < (1 << 5))
                {
                    ctrl++;

                    if (oidx + ctrl > outputLength)
                    {
                        return null;
                    }

                    do
                        output[oidx++] = input[iidx++];
                    while ((--ctrl) != 0);
                }
                else
                {
                    var len = ctrl >> 5;
                    var reference = (int)(oidx - ((ctrl & 0x1f) << 8) - 1);
                    if (len == 7)
                        len += input[iidx++];
                    reference -= input[iidx++];
                    if (oidx + len + 2 > outputLength)
                    {
                        return null;
                    }
                    if (reference < 0)
                    {
                        return null;
                    }
                    output[oidx++] = output[reference++];
                    output[oidx++] = output[reference++];
                    do
                        output[oidx++] = output[reference++];
                    while ((--len) != 0);
                }
            }
            while (iidx < inputLength);

            return output;
        }
    }

    /// <summary>
    /// Points in Pcd
    /// </summary>
    class Point3D
    {
        public float x;
        public float y;
        public float z;
        public uint color;
    }

    /// <summary>
    /// Header information of Pcd file
    /// </summary>
    class Header
    {
        public string FirstLine = ""; //The first line of the pcd file
        public string VERSION;
        public string FIELDS;
        public string SIZE;
        public string TYPE;
        public string COUNT;
        public string WIDTH;
        public string HEIGHT;
        public string VIEWPOINT;
        public string POINTS;
        public string DATA;
    }
#endif
}
