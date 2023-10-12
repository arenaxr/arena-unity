/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2023, Carnegie Mellon University. All rights reserved.
 */

// Modified from: https://github.com/mattatz/unity-mesh-builder/blob/master/Assets/Packages/MeshBuilder/Scripts/Builders/ParametricPlane.cs

using UnityEngine;
using Random = UnityEngine.Random;

namespace ArenaUnity
{

    public abstract class ParametricPlane
    {
        public abstract float Height(float ux, float uy);
    }

    public class ParametricPlaneDefault : ParametricPlane
    {
        public override float Height(float ux, float uy)
        {
            return 0f;
        }
    }

    public class ParametricPlaneRandom : ParametricPlane
    {

        float height;

        public ParametricPlaneRandom(float height = 1f)
        {
            this.height = height;
        }

        public override float Height(float ux, float uy)
        {
            return Random.value * height;
        }
    }

    public class ParametricPlanePerlin : ParametricPlane
    {

        Vector2 offset;
        Vector2 scale;
        float height;

        public ParametricPlanePerlin(Vector2 offset, Vector2 scale, float height = 1f)
        {
            this.offset = offset;
            this.scale = scale;
            this.height = height;
        }

        public override float Height(float ux, float uy)
        {
            return Mathf.PerlinNoise(offset.x + ux * scale.x, offset.y + uy * scale.y) * height;
        }
    }
}
