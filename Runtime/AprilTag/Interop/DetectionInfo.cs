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
    public struct DetectionInfo
    {
        IntPtr det;
        double tagsize;
        double fx, fy;
        double cx, cy;

        unsafe public DetectionInfo
          (ref Detection detection, double tagSize,
           double fx, double fy, double cx, double cy)
        {
            this.det = (IntPtr)Util.AsPointer(ref detection);
            this.tagsize = tagSize;
            this.fx = fx;
            this.fy = fy;
            this.cx = cx;
            this.cy = cy;
        }
    }
}
