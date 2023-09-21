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

        public static void ToUnityMesh(object indata, ref GameObject gobj)
        {
            //if ((string)indata.object_type == "entity" && indata.geometry != null && indata.geometry.primitive != null)
            //{
            //    // handle raw geometry
            //    data = indata.geometry;
            //    type = (string)indata.geometry.primitive;
            //}
            ArenaObjectDataJson data = JsonConvert.DeserializeObject<ArenaObjectDataJson>(indata.ToString());
            string type = data.object_type;
            switch (type)
            {
                // build your own meshes, defaults here should reflect ARENA/AFRAME defaults
                case "capsule":
                    ArenaMeshCapsule capsule = gobj.GetComponent<ArenaMeshCapsule>() ?? gobj.AddComponent<ArenaMeshCapsule>();
                    capsule.json = JsonConvert.DeserializeObject<ArenaCapsuleJson>(indata.ToString());
                    break;
                case "box":
                case "cube": // support legacy arena 'cube' == 'box'
                    ArenaMeshBox box = gobj.GetComponent<ArenaMeshBox>() ?? gobj.AddComponent<ArenaMeshBox>();
                    box.json = JsonConvert.DeserializeObject<ArenaBoxJson>(indata.ToString());
                    break;
                case "cone":
                    ArenaMeshCone cone = gobj.GetComponent<ArenaMeshCone>() ?? gobj.AddComponent<ArenaMeshCone>();
                    cone.json = JsonConvert.DeserializeObject<ArenaConeJson>(indata.ToString());
                    break;
                case "cylinder":
                    ArenaMeshCylinder cylinder = gobj.GetComponent<ArenaMeshCylinder>() ?? gobj.AddComponent<ArenaMeshCylinder>();
                    cylinder.json = JsonConvert.DeserializeObject<ArenaCylinderJson>(indata.ToString());
                    break;
                case "dodecahedron":
                    ArenaMeshDodecahedron dodecahedron = gobj.GetComponent<ArenaMeshDodecahedron>() ?? gobj.AddComponent<ArenaMeshDodecahedron>();
                    dodecahedron.json = JsonConvert.DeserializeObject<ArenaDodecahedronJson>(indata.ToString());
                    break;
                case "tetrahedron":
                    ArenaMeshTetrahedron tetrahedron = gobj.GetComponent<ArenaMeshTetrahedron>() ?? gobj.AddComponent<ArenaMeshTetrahedron>();
                    tetrahedron.json = JsonConvert.DeserializeObject<ArenaTetrahedronJson>(indata.ToString());
                    break;
                case "icosahedron":
                    ArenaMeshIcosahedron icosahedron = gobj.GetComponent<ArenaMeshIcosahedron>() ?? gobj.AddComponent<ArenaMeshIcosahedron>();
                    icosahedron.json = JsonConvert.DeserializeObject<ArenaIcosahedronJson>(indata.ToString());
                    break;
                case "octahedron":
                    ArenaMeshOctahedron octahedron = gobj.GetComponent<ArenaMeshOctahedron>() ?? gobj.AddComponent<ArenaMeshOctahedron>();
                    octahedron.json = JsonConvert.DeserializeObject<ArenaOctahedronJson>(indata.ToString());
                    break;
                case "plane":
                    ArenaMeshPlane plane = gobj.GetComponent<ArenaMeshPlane>() ?? gobj.AddComponent<ArenaMeshPlane>();
                    plane.json = JsonConvert.DeserializeObject<ArenaPlaneJson>(indata.ToString());
                    break;
                case "ring":
                    ArenaMeshRing ring = gobj.GetComponent<ArenaMeshRing>() ?? gobj.AddComponent<ArenaMeshRing>();
                    ring.json = JsonConvert.DeserializeObject<ArenaRingJson>(indata.ToString());
                    break;
                case "circle":
                    ArenaMeshCircle circle = gobj.GetComponent<ArenaMeshCircle>() ?? gobj.AddComponent<ArenaMeshCircle>();
                    circle.json = JsonConvert.DeserializeObject<ArenaCircleJson>(indata.ToString());
                    break;
                case "videosphere": // use sphere as a videosphere placeholder
                case "sphere":
                    ArenaMeshSphere sphere = gobj.GetComponent<ArenaMeshSphere>() ?? gobj.AddComponent<ArenaMeshSphere>();
                    sphere.json = JsonConvert.DeserializeObject<ArenaSphereJson>(indata.ToString());
                    break;
                case "torus":
                    ArenaMeshTorus torus = gobj.GetComponent<ArenaMeshTorus>() ?? gobj.AddComponent<ArenaMeshTorus>();
                    torus.json = JsonConvert.DeserializeObject<ArenaTorusJson>(indata.ToString());
                    break;
                case "torusKnot":
                    ArenaMeshTorusKnot torusKnot = gobj.GetComponent<ArenaMeshTorusKnot>() ?? gobj.AddComponent<ArenaMeshTorusKnot>();
                    torusKnot.json = JsonConvert.DeserializeObject<ArenaTorusKnotJson>(indata.ToString());
                    break;
                case "triangle":
                    ArenaMeshTriangle triangle = gobj.GetComponent<ArenaMeshTriangle>() ?? gobj.AddComponent<ArenaMeshTriangle>();
                    triangle.json = JsonConvert.DeserializeObject<ArenaTriangleJson>(indata.ToString());
                    break;
                default:
                    break;
            };
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

    }
}
