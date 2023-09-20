/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2023, Carnegie Mellon University. All rights reserved.
 */

// Modified from: https://github.com/mattatz/unity-mesh-builder/tree/master/Assets/Packages/MeshBuilder/Scripts/Demo

using UnityEngine;
using ArenaUnity.Components;

namespace ArenaUnity
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(ArenaObject), typeof(MeshFilter), typeof(MeshRenderer))]
    public abstract class ArenaMesh : ArenaComponent
    {
        protected MeshFilter filter;
        protected MeshCollider mc;

        protected override void Start()
        {
            apply = true;
            filter = GetComponent<MeshFilter>();
            ApplyRender();
            mc = GetComponent<MeshCollider>();
            if (mc != null) mc.sharedMesh = filter.mesh;
        }

        protected override void OnValidate()
        {
            if (filter == null) filter = GetComponent<MeshFilter>();
            apply = true;

            if (!scriptLoaded)
            {
                scriptLoaded = true;
            }
            else
            {   // do not publish update on script load
                UpdateObject();
            }
        }

        protected override void Update()
        {
            if (apply)
            {
                ApplyRender();
                mc = GetComponent<MeshCollider>();
                if (mc != null) mc.sharedMesh = filter.mesh;
                apply = false;
            }
        }

    }
}
