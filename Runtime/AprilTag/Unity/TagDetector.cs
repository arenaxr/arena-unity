/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2024, Carnegie Mellon University. All rights reserved.
 *
 * Adapted from jp.keijiro.apriltag (BSD-2-Clause)
 * https://github.com/keijiro/jp.keijiro.apriltag
 *
 * Modified to use the tag36h11 family required by ARENA scene localization.
 */

using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Color32 = UnityEngine.Color32;

namespace ArenaUnity.AprilTag
{
    /// <summary>
    /// Multithreaded AprilTag detector using the tag36h11 family required by ARENA.
    /// </summary>
    public sealed class TagDetector : IDisposable
    {
        public IEnumerable<TagPose> DetectedTags => _detectedTags;

        public IEnumerable<(string name, long time)> ProfileData
          => _profileData ?? (_profileData = GenerateProfileData());

        public TagDetector(int width, int height, int decimation = 2)
        {
            _detector = Interop.Detector.Create();
            _family = Interop.Family.CreateTag36h11();
            _image = Interop.ImageU8.Create(width, height);

            _detector.ThreadCount = SystemConfig.PreferredThreadCount;
            _detector.QuadDecimate = decimation;
            _detector.AddFamily(_family);
        }

        public void Dispose()
        {
            _detector?.RemoveFamily(_family);
            _detector?.Dispose();
            _family?.Dispose();
            _image?.Dispose();

            _detector = null;
            _family = null;
            _image = null;
        }

        public void ProcessImage(ReadOnlySpan<Color32> image, float fov, float tagSize)
        {
            ImageConverter.Convert(image, _image);
            RunDetectorAndEstimator(fov, tagSize);
        }

        Interop.Detector _detector;
        Interop.Family _family;
        Interop.ImageU8 _image;

        List<TagPose> _detectedTags = new List<TagPose>();
        List<(string, long)> _profileData;

        void RunDetectorAndEstimator(float fov, float tagSize)
        {
            _profileData = null;

            using var tags = _detector.Detect(_image);
            var tagCount = tags.Length;

            using var jobInput = new NativeArray<PoseEstimationJob.Input>
              (tagCount, Allocator.TempJob);

            var slice = new NativeSlice<PoseEstimationJob.Input>(jobInput);
            for (var i = 0; i < tagCount; i++)
                slice[i] = new PoseEstimationJob.Input(ref tags[i]);

            using var jobOutput = new NativeArray<TagPose>(tagCount, Allocator.TempJob);

            var job = new PoseEstimationJob
              (jobInput, jobOutput, _image.Width, _image.Height, fov, tagSize);

            job.Schedule(tagCount, 1, default(JobHandle)).Complete();
            jobOutput.CopyTo(_detectedTags);
        }

        List<(string, long)> GenerateProfileData()
        {
            var list = new List<(string, long)>();
            var stamps = _detector.TimeProfile.Stamps;
            var time = _detector.TimeProfile.UTime;
            for (var i = 0; i < stamps.Length; i++)
            {
                var stamp = stamps[i];
                list.Add((stamp.Name, stamp.UTime - time));
                time = stamp.UTime;
            }
            return list;
        }
    }
}
