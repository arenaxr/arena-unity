/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2024, Carnegie Mellon University. All rights reserved.
 *
 * Adapted from jp.keijiro.apriltag (BSD-2-Clause)
 * https://github.com/keijiro/jp.keijiro.apriltag
 */

using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace ArenaUnity.AprilTag
{
    struct PoseEstimationJob : Unity.Jobs.IJobParallelFor
    {
        public struct Input
        {
            unsafe Interop.Detection* p;

            unsafe public Input(ref Interop.Detection r)
              => p = (Interop.Detection*)Interop.Util.AsPointer(ref r);

            unsafe public ref Interop.Detection Ref
              => ref Interop.Util.AsRef<Interop.Detection>(p);
        }

        [ReadOnly] NativeArray<Input> _input;
        [WriteOnly] NativeArray<TagPose> _output;

        double _tagSize;
        double _focalLength;
        double2 _focalCenter;

        public PoseEstimationJob
          (NativeArray<Input> input, NativeArray<TagPose> output,
           int width, int height, float fov, float tagSize)
        {
            _input = input;
            _output = output;
            _tagSize = tagSize;
            _focalLength = height / 2.0 / math.tan(fov / 2.0);
            _focalCenter = math.double2(width, height) / 2;
        }

        public void Execute(int i)
        {
            var info = new Interop.DetectionInfo(ref _input[i].Ref, _tagSize,
               _focalLength, _focalLength, _focalCenter.x, _focalCenter.y);

            using var pose = new Interop.Pose(ref info);

            var pos = pose.t.AsFloat3() * math.float3(1, -1, 1);

            var rot = math.quaternion(pose.R.AsFloat3x3());
            rot = rot.value * math.float4(-1, 1, -1, 1);

            _output[i] = new TagPose(_input[i].Ref.ID, pos, rot);
        }
    }
}
