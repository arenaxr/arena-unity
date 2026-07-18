/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2024, Carnegie Mellon University. All rights reserved.
 *
 * Adapted from jp.keijiro.apriltag (BSD-2-Clause)
 * https://github.com/keijiro/jp.keijiro.apriltag
 */

using UnityEngine;

namespace ArenaUnity.AprilTag
{
    /// <summary>
    /// Estimated pose of a detected AprilTag.
    /// </summary>
    public struct TagPose
    {
        public int ID { get; }
        public Vector3 Position { get; }
        public Quaternion Rotation { get; }

        public TagPose(int id, Vector3 position, Quaternion rotation)
        {
            ID = id;
            Position = position;
            Rotation = rotation;
        }
    }
}
