/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2023, Carnegie Mellon University. All rights reserved.
 */

// Modified from: https://github.com/mattatz/unity-mesh-builder/tree/master/Assets/Packages/MeshBuilder/Scripts/Demo

using UnityEngine;
using ArenaUnity.Components;
using ArenaUnity.Schemas;
using Newtonsoft.Json;

namespace ArenaUnity
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
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

        public static void ToArenaMesh(GameObject gobj, ref object data)
        {
            ArenaMesh am = gobj.GetComponent<ArenaMesh>();
            if (am == null) return;
            switch (am.GetType().ToString())
            {
                case "ArenaUnity.ArenaMeshCapsule":
                    var capsule = gobj.GetComponent<ArenaMeshCapsule>();
                    data = capsule.json;
                    break;
                case "ArenaUnity.ArenaMeshBox":
                    var box = gobj.GetComponent<ArenaMeshBox>();
                    data = box.json;
                    break;
                case "ArenaUnity.ArenaMeshCone":
                    var cone = gobj.GetComponent<ArenaMeshCone>();
                    data = cone.json;
                    break;
                case "ArenaUnity.ArenaMeshCylinder":
                    var cylinder = gobj.GetComponent<ArenaMeshCylinder>();
                    data = cylinder.json;
                    break;
                case "ArenaUnity.ArenaMeshDodecahedron":
                    var dodecahedron = gobj.GetComponent<ArenaMeshDodecahedron>();
                    data = dodecahedron.json;
                    break;
                case "ArenaUnity.ArenaMeshTetrahedron":
                    var tetrahedron = gobj.GetComponent<ArenaMeshTetrahedron>();
                    data = tetrahedron.json;
                    break;
                case "ArenaUnity.ArenaMeshIcosahedron":
                    var icosahedron = gobj.GetComponent<ArenaMeshIcosahedron>();
                    data = icosahedron.json;
                    break;
                case "ArenaUnity.ArenaMeshOctahedron":
                    var octahedron = gobj.GetComponent<ArenaMeshOctahedron>();
                    data = octahedron.json;
                    break;
                case "ArenaUnity.ArenaMeshPlane":
                    var plane = gobj.GetComponent<ArenaMeshPlane>();
                    data = plane.json;
                    break;
                case "ArenaUnity.ArenaMeshSphere":
                    var sphere = gobj.GetComponent<ArenaMeshSphere>();
                    data = sphere.json;
                    break;
                case "ArenaUnity.ArenaMeshCircle":
                    var circle = gobj.GetComponent<ArenaMeshCircle>();
                    data = circle.json;
                    break;
                case "ArenaUnity.ArenaMeshRing":
                    var ring = gobj.GetComponent<ArenaMeshRing>();
                    data = ring.json;
                    break;
                case "ArenaUnity.ArenaMeshTorus":
                    var torus = gobj.GetComponent<ArenaMeshTorus>();
                    data = torus.json;
                    break;
                case "ArenaUnity.ArenaMeshTorusKnot":
                    var torusKnot = gobj.GetComponent<ArenaMeshTorusKnot>();
                    data = torusKnot.json;
                    break;
                case "ArenaUnity.ArenaMeshTriangle":
                    var triangle = gobj.GetComponent<ArenaMeshTriangle>();
                    data = triangle.json;
                    break;
            }
        }


        // size dimensions
        public static void ToArenaDimensions(GameObject gobj, ref object data)
        {
            // used to collect unity-default render sizes
            string collider = gobj.GetComponent<Collider>().GetType().ToString();
            string mesh = null;
            MeshFilter meshFilter = gobj.GetComponent<MeshFilter>();
            if (meshFilter && meshFilter.sharedMesh)
            {
                mesh = meshFilter.sharedMesh.name;
            }
            switch (collider)
            {
                case "UnityEngine.BoxCollider":
                    BoxCollider bc = gobj.GetComponent<BoxCollider>();
                    data = new ArenaBoxJson
                    {
                        Width = ArenaUnity.ArenaFloat(bc.size.x),
                        Height = ArenaUnity.ArenaFloat(bc.size.y),
                        Depth = ArenaUnity.ArenaFloat(bc.size.z),
                    };
                    break;
                case "UnityEngine.SphereCollider":
                    SphereCollider sc = gobj.GetComponent<SphereCollider>();
                    data = new ArenaSphereJson
                    {
                        Radius = ArenaUnity.ArenaFloat(sc.radius),
                    };
                    break;
                case "UnityEngine.CapsuleCollider":
                    CapsuleCollider cc = gobj.GetComponent<CapsuleCollider>();
                    switch (mesh)
                    {
                        case "Cylinder":
                            data = new ArenaCylinderJson
                            {
                                Height = ArenaUnity.ArenaFloat(cc.height),
                                Radius = ArenaUnity.ArenaFloat(cc.radius),
                            };
                            break;
                        case "Capsule":
                            data = new ArenaCapsuleJson
                            {
                                Length = ArenaUnity.ArenaFloat(cc.height - (cc.radius * 2)),
                                Radius = ArenaUnity.ArenaFloat(cc.radius),
                            };
                            break;
                    }
                    break;
                default:
                    break;
            }
            // TODO (mwfarb): switch (mesh)
            //{
            //    case "Cube":
            //        data.object_type = "box";
            //        break;
            //    case "Quad":
            //        data.object_type = "plane";
            //        data.width = 1f;
            //        data.height = 1f;
            //        break;
            //    case "Plane":
            //        Quaternion rotOut = gobj.transform.localRotation;
            //        rotOut *= Quaternion.Euler(90, 0, 0);
            //        data.rotation = ArenaUnity.ToArenaRotationQuat(rotOut);
            //        data.width = 10f;
            //        data.height = 10f;
            //        break;
            //    default:
            //        break;
            //}
        }

    }
}
