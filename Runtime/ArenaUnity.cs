/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021, The CONIX Research Center. All rights reserved.
 */

using System;
using System.Dynamic;
using UnityEngine;
using UnityEngine.Rendering;

namespace ArenaUnity
{
    /// <summary>
    /// Static utility class for object translation.
    /// </summary>
    public static class ArenaUnity
    {
        private static float ArenaFloat(float n) { return (float)Math.Round(n, 3); }

        // time
        public static string TimeSpanToString(TimeSpan span)
        {
            string spanString = string.Format("{0}{1}{2}{3}",
                span.Duration().Days > 0 ? string.Format("{0:0} day{1}, ", span.Days, span.Days == 1 ? string.Empty : "s") : string.Empty,
                span.Duration().Hours > 0 ? string.Format("{0:0} hour{1}, ", span.Hours, span.Hours == 1 ? string.Empty : "s") : string.Empty,
                span.Duration().Minutes > 0 ? string.Format("{0:0} minute{1}, ", span.Minutes, span.Minutes == 1 ? string.Empty : "s") : string.Empty,
                span.Duration().Seconds > 0 ? string.Format("{0:0} second{1}", span.Seconds, span.Seconds == 1 ? string.Empty : "s") : string.Empty);
            if (spanString.EndsWith(", ")) spanString = spanString.Substring(0, spanString.Length - 2);
            if (string.IsNullOrEmpty(spanString)) spanString = "0 seconds";
            return spanString;
        }

        // object type
        public static string ToArenaObjectType(GameObject gobj)
        {
            string objectType = "entity";
            MeshFilter meshFilter = gobj.GetComponent<MeshFilter>();
            Light light = gobj.GetComponent<Light>();
            SpriteRenderer spriteRenderer = gobj.GetComponent<SpriteRenderer>();
            if (meshFilter && meshFilter.sharedMesh)
                objectType = meshFilter.sharedMesh.name.ToLower();
            else if (spriteRenderer && spriteRenderer.sprite && spriteRenderer.sprite.pixelsPerUnit != 0f)
                objectType = "image";
            else if (light)
                objectType = "light";
            return objectType;
        }
        public static void ToUnityMesh(dynamic indata, ref GameObject gobj)
        {
            dynamic data;
            string type;
            if ((string)indata.object_type == "entity" && indata.geometry != null && indata.geometry.primitive != null)
            {
                // handle raw geometry
                data = indata.geometry;
                type = (string)indata.geometry.primitive;
            }
            else
            {
                data = indata;
                type = (string)indata.object_type;
            }
            switch (type)
            {
                // build your own meshes, defaults here should reflect ARENA/AFRAME/THREE defaults
                case "capsule":
                    ArenaMeshCapsule capsule = gobj.GetComponent<ArenaMeshCapsule>() ?? gobj.AddComponent<ArenaMeshCapsule>();
                    capsule.radius = data.radius != null ? (float)data.radius : 1f;
                    capsule.length = data.length != null ? (float)data.length : 1f;
                    capsule.radialSegments = data.segmentsRadial != null ? (int)data.segmentsRadial : 36;
                    capsule.heightSegments = data.segmentsHeight != null ? (int)data.segmentsHeight : 18;
                    break;
                case "box":
                case "cube": // support legacy arena 'cube' == 'box'
                    ArenaMeshCube cube = gobj.GetComponent<ArenaMeshCube>() ?? gobj.AddComponent<ArenaMeshCube>();
                    cube.width = data.width != null ? (float)data.width : 1f;
                    cube.height = data.height != null ? (float)data.height : 1f;
                    cube.depth = data.depth != null ? (float)data.depth : 1f;
                    cube.widthSegments = data.segmentsWidth != null ? (int)data.segmentsWidth : 2;
                    cube.heightSegments = data.segmentsHeight != null ? (int)data.segmentsHeight : 2;
                    cube.depthSegments = data.segmentsDepth != null ? (int)data.segmentsDepth : 2;
                    break;
                case "cone":
                    ArenaMeshCone cone = gobj.GetComponent<ArenaMeshCone>() ?? gobj.AddComponent<ArenaMeshCone>();
                    cone.radius = data.radiusBottom != null ? (float)data.radiusBottom : 1f;
                    cone.height = data.height != null ? (float)data.height : 1f;
                    cone.subdivision = data.segmentsRadial != null ? (int)data.segmentsRadial : 36;
                    break;
                case "cylinder":
                    ArenaMeshCylinder cylinder = gobj.GetComponent<ArenaMeshCylinder>() ?? gobj.AddComponent<ArenaMeshCylinder>();
                    cylinder.radius = data.radius != null ? (float)data.radius : 1f;
                    cylinder.height = data.height != null ? (float)data.height : 1f;
                    cylinder.radialSegments = data.segmentsRadial != null ? (int)data.segmentsRadial : 36;
                    cylinder.heightSegments = data.segmentsHeight != null ? (int)data.segmentsHeight : 18;
                    cylinder.openEnded = data.openEnded != null ? Convert.ToBoolean(data.openEnded) : false;
                    break;
                case "dodecahedron":
                    ArenaMeshDodecahedron dodecahedron = gobj.GetComponent<ArenaMeshDodecahedron>() ?? gobj.AddComponent<ArenaMeshDodecahedron>();
                    dodecahedron.radius = data.radius != null ? (float)data.radius : 1f;
                    dodecahedron.details = 0;
                    break;
                case "tetrahedron":
                    ArenaMeshTetrahedron tetrahedron = gobj.GetComponent<ArenaMeshTetrahedron>() ?? gobj.AddComponent<ArenaMeshTetrahedron>();
                    tetrahedron.radius = data.radius != null ? (float)data.radius : 1f;
                    tetrahedron.details = 0;
                    break;
                case "icosahedron":
                    ArenaMeshIcosahedron icosahedron = gobj.GetComponent<ArenaMeshIcosahedron>() ?? gobj.AddComponent<ArenaMeshIcosahedron>();
                    icosahedron.radius = data.radius != null ? (float)data.radius : 1f;
                    icosahedron.details = 0;
                    break;
                case "octahedron":
                    ArenaMeshOctahedron octahedron = gobj.GetComponent<ArenaMeshOctahedron>() ?? gobj.AddComponent<ArenaMeshOctahedron>();
                    octahedron.radius = data.radius != null ? (float)data.radius : 1f;
                    octahedron.details = 0;
                    break;
                case "plane":
                    ArenaMeshPlane plane = gobj.GetComponent<ArenaMeshPlane>() ?? gobj.AddComponent<ArenaMeshPlane>();
                    plane.width = data.width != null ? (float)data.width : 1f;
                    plane.height = data.height != null ? (float)data.height : 1f;
                    plane.wSegments = data.segmentsWidth != null ? (int)data.segmentsWidth : 2;
                    plane.hSegments = data.segmentsHeight != null ? (int)data.segmentsHeight : 2;
                    break;
                case "prism":
                    ArenaMeshPrism prism = gobj.GetComponent<ArenaMeshPrism>() ?? gobj.AddComponent<ArenaMeshPrism>();
                    prism.width = data.width != null ? (float)data.width : 1f;
                    prism.height = data.height != null ? (float)data.height : 1f;
                    prism.depth = data.depth != null ? (float)data.depth : 1f;
                    break;
                case "ring":
                    ArenaMeshRing ring = gobj.GetComponent<ArenaMeshRing>() ?? gobj.AddComponent<ArenaMeshRing>();
                    ring.outerRadius = data.radiusOuter != null ? (float)data.radiusOuter : 1f;
                    ring.innerRadius = data.radiusInner != null ? (float)data.radiusInner : 0.5f;
                    ring.phiSegments = data.segmentsPhi != null ? (int)data.segmentsPhi : 8;
                    ring.thetaSegments = data.segmentsTheta != null ? (int)data.segmentsTheta : 32;
                    ring.thetaStart = (float)(data.thetaStart != null ? Mathf.PI / 180 * (float)data.thetaStart : 0f);
                    ring.thetaLength = (float)(data.thetaLength != null ? Mathf.PI / 180 * (float)data.thetaLength : Mathf.PI * 2f);
                    break;
                case "circle":
                    ArenaMeshCircle circle = gobj.GetComponent<ArenaMeshCircle>() ?? gobj.AddComponent<ArenaMeshCircle>();
                    circle.radius = data.radius != null ? (float)data.radius : 1f;
                    circle.segments = data.segments != null ? (int)data.segments : 32;
                    circle.thetaStart = (float)(data.thetaStart != null ? Mathf.PI / 180 * (float)data.thetaStart : 0f);
                    circle.thetaLength = (float)(data.thetaLength != null ? Mathf.PI / 180 * (float)data.thetaLength : Mathf.PI * 2f);
                    break;
                case "sphere":
                    ArenaMeshSphere sphere = gobj.GetComponent<ArenaMeshSphere>() ?? gobj.AddComponent<ArenaMeshSphere>();
                    sphere.radius = data.radius != null ? (float)data.radius : 1f;
                    sphere.lonSegments = data.segmentsWidth != null ? (int)data.segmentsWidth : 18;
                    sphere.latSegments = data.segmentsHeight != null ? (int)data.segmentsHeight : 36;
                    break;
                case "torus":
                    ArenaMeshTorus torus = gobj.GetComponent<ArenaMeshTorus>() ?? gobj.AddComponent<ArenaMeshTorus>();
                    torus.radius = data.radius != null ? (float)data.radius : 1f;
                    torus.radiusTubular = data.radiusTubular != null ? (float)data.radiusTubular : 0.4f;
                    torus.radialSegments = data.segmentsRadial != null ? (int)data.segmentsRadial : 36;
                    torus.thetaSegments = data.segmentsTubular != null ? (int)data.segmentsTubular : 32;
                    torus.thetaStart = 0f;
                    torus.thetaEnd = (float)(data.arc != null ? Mathf.PI / 180 * (float)data.arc : Mathf.PI * 2f);
                    break;
                case "torusKnot":
                    ArenaMeshTorusKnot torusKnot = gobj.GetComponent<ArenaMeshTorusKnot>() ?? gobj.AddComponent<ArenaMeshTorusKnot>();
                    torusKnot.radius = data.radius != null ? (float)data.radius : 1f;
                    torusKnot.radiusTubular = data.radiusTubular != null ? (float)data.radiusTubular : 0.4f;
                    torusKnot.radialSegments = data.segmentsRadial != null ? (int)data.segmentsRadial : 36;
                    torusKnot.thetaSegments = data.segmentsTubular != null ? (int)data.segmentsTubular : 32;
                    torusKnot.p = data.p != null ? (int)data.p : 2;
                    torusKnot.q = data.q != null ? (int)data.q : 3;
                    break;
                case "triangle":
                    ArenaMeshTriangle triangle = gobj.GetComponent<ArenaMeshTriangle>() ?? gobj.AddComponent<ArenaMeshTriangle>();
                    triangle.vertexA = data.vertexA != null ? ToUnityPosition(data.vertexA) : new Vector3(0f, 0.5f);
                    triangle.vertexB = data.vertexB != null ? ToUnityPosition(data.vertexB) : new Vector3(-0.5f, -0.5f);
                    triangle.vertexC = data.vertexC != null ? ToUnityPosition(data.vertexC) : new Vector3(0.5f, -0.5f);
                    break;
                default:
                    break;
            };
        }
        public static void ToArenaMesh(GameObject gobj, ref dynamic data)
        {
            ArenaMesh am = gobj.GetComponent<ArenaMesh>();
            if (am == null) return;
            switch (am.GetType().ToString())
            {
                case "ArenaUnity.ArenaMeshCapsule":
                    var capsule = gobj.GetComponent<ArenaMeshCapsule>();
                    data.radius = ArenaFloat(capsule.radius);
                    data.length = ArenaFloat(capsule.length);
                    break;
                case "ArenaUnity.ArenaMeshCube":
                    var cube = gobj.GetComponent<ArenaMeshCube>();
                    data.width = ArenaFloat(cube.width);
                    data.height = ArenaFloat(cube.height);
                    data.depth = ArenaFloat(cube.depth);
                    break;
                case "ArenaUnity.ArenaMeshCone":
                    var cone = gobj.GetComponent<ArenaMeshCone>();
                    data.radiusBottom = ArenaFloat(cone.radius);
                    data.height = ArenaFloat(cone.height);
                    break;
                case "ArenaUnity.ArenaMeshCylinder":
                    var cylinder = gobj.GetComponent<ArenaMeshCylinder>();
                    data.radius = ArenaFloat(cylinder.radius);
                    data.height = ArenaFloat(cylinder.height);
                    data.openEnded = cylinder.openEnded;
                    break;
                case "ArenaUnity.ArenaMeshDodecahedron":
                    var dodecahedron = gobj.GetComponent<ArenaMeshDodecahedron>();
                    data.radius = ArenaFloat(dodecahedron.radius);
                    break;
                case "ArenaUnity.ArenaMeshTetrahedron":
                    var tetrahedron = gobj.GetComponent<ArenaMeshTetrahedron>();
                    data.radius = ArenaFloat(tetrahedron.radius);
                    break;
                case "ArenaUnity.ArenaMeshIcosahedron":
                    var icosahedron = gobj.GetComponent<ArenaMeshIcosahedron>();
                    data.radius = ArenaFloat(icosahedron.radius);
                    break;
                case "ArenaUnity.ArenaMeshOctahedron":
                    var octahedron = gobj.GetComponent<ArenaMeshOctahedron>();
                    data.radius = ArenaFloat(octahedron.radius);
                    break;
                case "ArenaUnity.ArenaMeshPlane":
                    var plane = gobj.GetComponent<ArenaMeshPlane>();
                    data.width = ArenaFloat(plane.width);
                    data.height = ArenaFloat(plane.height);
                    break;
                case "ArenaUnity.ArenaMeshPrism":
                    var prism = gobj.GetComponent<ArenaMeshPrism>();
                    data.width = ArenaFloat(prism.width);
                    data.height = ArenaFloat(prism.height);
                    data.depth = ArenaFloat(prism.depth);
                    break;
                case "ArenaUnity.ArenaMeshSphere":
                    var sphere = gobj.GetComponent<ArenaMeshSphere>();
                    data.radius = ArenaFloat(sphere.radius);
                    break;
                case "ArenaUnity.ArenaMeshCircle":
                    var circle = gobj.GetComponent<ArenaMeshCircle>();
                    data.radius = ArenaFloat(circle.radius);
                    data.thetaStart = ArenaFloat(circle.thetaStart * 180 / Mathf.PI);
                    data.thetaLength = ArenaFloat(circle.thetaLength * 180 / Mathf.PI);
                    break;
                case "ArenaUnity.ArenaMeshRing":
                    var ring = gobj.GetComponent<ArenaMeshRing>();
                    data.radiusOuter = ArenaFloat(ring.outerRadius);
                    data.radiusInner = ArenaFloat(ring.innerRadius);
                    data.thetaStart = ArenaFloat(ring.thetaStart * 180 / Mathf.PI);
                    data.thetaLength = ArenaFloat(ring.thetaLength * 180 / Mathf.PI);
                    break;
                case "ArenaUnity.ArenaMeshTorus":
                    var torus = gobj.GetComponent<ArenaMeshTorus>();
                    data.radius = ArenaFloat(torus.radius);
                    data.radiusTubular = ArenaFloat(torus.radiusTubular);
                    data.arc = ArenaFloat(torus.thetaEnd * 180 / Mathf.PI);
                    break;
                case "ArenaUnity.ArenaMeshTorusKnot":
                    var torusKnot = gobj.GetComponent<ArenaMeshTorusKnot>();
                    data.radius = ArenaFloat(torusKnot.radius);
                    data.radiusTubular = ArenaFloat(torusKnot.radiusTubular);
                    data.p = torusKnot.p;
                    data.q = torusKnot.q;
                    break;
                case "ArenaUnity.ArenaMeshTriangle":
                    var triangle = gobj.GetComponent<ArenaMeshTriangle>();
                    data.vertexA = ToArenaPosition(triangle.vertexA);
                    data.vertexB = ToArenaPosition(triangle.vertexB);
                    data.vertexC = ToArenaPosition(triangle.vertexC);
                    break;
            }
        }
        private static Vector3 StrPositionToVector3(string strPos)
        {
            string[] axis = strPos.Split(' ');
            return new Vector3(float.Parse(axis[0]), float.Parse(axis[1]), float.Parse(axis[2]));
        }
        private static string Vector3ToStrPosition(Vector3 position)
        {
            return $"{ArenaFloat(position.x)} {ArenaFloat(position.y)} {ArenaFloat(position.z)}";
        }
        // position
        public static dynamic ToArenaPosition(Vector3 position)
        {
            return new
            {
                x = ArenaFloat(position.x),
                y = ArenaFloat(position.y),
                z = ArenaFloat(-position.z)
            };
        }
        public static Vector3 ToUnityPosition(dynamic position)
        {
            return new Vector3(
                (float)position.x,
                (float)position.y,
                -(float)position.z
            );
        }
        // rotation
        public static dynamic ToArenaRotationQuat(Quaternion rotationQuat, bool invertY = true)
        {
            return new
            {
                x = ArenaFloat(-rotationQuat.x),
                y = ArenaFloat(rotationQuat.y * (invertY ? -1 : 1)),
                z = ArenaFloat(rotationQuat.z),
                w = ArenaFloat(rotationQuat.w)
            };
        }
        public static Quaternion ToUnityRotationQuat(dynamic rotationQuat, bool invertY = true)
        {
            return new Quaternion(
                -(float)rotationQuat.x,
                (float)rotationQuat.y * (invertY ? -1 : 1),
                (float)rotationQuat.z,
                (float)rotationQuat.w
            );
        }
        public static dynamic ToArenaRotationEuler(Vector3 rotationEuler, bool invertY = true)
        {
            return new
            {
                x = ArenaFloat(-rotationEuler.x),
                y = ArenaFloat(rotationEuler.y * (invertY ? -1 : 1)),
                z = ArenaFloat(rotationEuler.z)
            };
        }
        public static Quaternion ToUnityRotationEuler(dynamic rotationEuler, bool invertY = true)
        {
            return Quaternion.Euler(
                -(float)rotationEuler.x,
                (float)rotationEuler.y * (invertY ? -1 : 1),
                (float)rotationEuler.z
            );
        }
        public static Quaternion GltfToUnityRotationQuat(Quaternion rotationQuat)
        {
            rotationQuat *= Quaternion.Euler(0, 180f, 0);
            return rotationQuat;
        }
        public static Quaternion UnityToGltfRotationQuat(Quaternion rotationQuat)
        {
            rotationQuat *= Quaternion.Euler(0, -180f, 0);
            return rotationQuat;
        }
        // scale
        public static dynamic ToArenaScale(Vector3 scale)
        {
            return new
            {
                x = ArenaFloat(scale.x),
                y = ArenaFloat(scale.y),
                z = ArenaFloat(scale.z)
            };
        }
        public static Vector3 ToUnityScale(dynamic scale)
        {
            return new Vector3(
                (float)scale.x,
                (float)scale.y,
                (float)scale.z
            );
        }
        // size dimensions
        public static void ToArenaDimensions(GameObject gobj, ref dynamic data)
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
                    data.width = ArenaFloat(bc.size.x);
                    data.height = ArenaFloat(bc.size.y);
                    data.depth = ArenaFloat(bc.size.z);
                    break;
                case "UnityEngine.SphereCollider":
                    SphereCollider sc = gobj.GetComponent<SphereCollider>();
                    data.radius = ArenaFloat(sc.radius);
                    break;
                case "UnityEngine.CapsuleCollider":
                    CapsuleCollider cc = gobj.GetComponent<CapsuleCollider>();
                    switch (mesh)
                    {
                        case "Cylinder":
                            data.height = ArenaFloat(cc.height);
                            break;
                        case "Capsule":
                            data.length = ArenaFloat(cc.height - (cc.radius * 2));
                            break;
                    }
                    data.radius = ArenaFloat(cc.radius);
                    break;
                default:
                    break;
            }
            switch (mesh)
            {
                case "Cube":
                    data.object_type = "box";
                    break;
                case "Quad":
                    data.object_type = "plane";
                    data.width = 1f;
                    data.height = 1f;
                    break;
                case "Plane":
                    Quaternion rotOut = gobj.transform.localRotation;
                    rotOut *= Quaternion.Euler(90, 0, 0);
                    data.rotation = ArenaUnity.ToArenaRotationQuat(rotOut);
                    data.width = 10f;
                    data.height = 10f;
                    break;
                default:
                    break;
            }
        }

        // color
        public static string ToArenaColor(Color color)
        {
            return $"#{ColorUtility.ToHtmlStringRGB(color).ToLower()}";
        }
        public static Color ToUnityColor(string color)
        {
            ColorUtility.TryParseHtmlString(color, out Color colorObj);
            return colorObj;
        }
        public static Color ColorRandom()
        {
            return UnityEngine.Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
        }

        // text
        public static void ToUnityText(dynamic data, ref GameObject gobj)
        {
            TextMesh tm = gobj.AddComponent<TextMesh>();
            // use scale/font size to gain crisp resolution
            gobj.transform.localScale = gobj.transform.localScale / 5f;
            tm.characterSize = 0.1f;
            tm.fontSize = 45;

            if (data.value != null)
                tm.text = (string)data.value;
            else if (data.text != null)
                tm.text = (string)data.text;
            if (data.color != null)
                tm.color = ToUnityColor((string)data.color);
            if (data.align != null)
            {
                switch ((string)data.align)
                {
                    default:
                    case "left":
                        tm.alignment = TextAlignment.Left;
                        break;
                    case "center":
                        tm.alignment = TextAlignment.Center;
                        break;
                    case "right":
                        tm.alignment = TextAlignment.Right;
                        break;
                }
            }

            string anchor = data.anchor != null ? (string)data.anchor : "center";
            string baseline = data.baseline != null ? (string)data.baseline : "center";
            switch ($"{baseline} {anchor}")
            {
                case "top left":
                    tm.anchor = TextAnchor.UpperLeft;
                    break;
                case "top center":
                    tm.anchor = TextAnchor.UpperCenter;
                    break;
                case "top right":
                    tm.anchor = TextAnchor.UpperRight;
                    break;
                case "center left":
                    tm.anchor = TextAnchor.MiddleLeft;
                    break;
                case "center center":
                    tm.anchor = TextAnchor.MiddleCenter;
                    break;
                case "center right":
                    tm.anchor = TextAnchor.MiddleRight;
                    break;
                case "bottom left":
                    tm.anchor = TextAnchor.LowerLeft;
                    break;
                case "bottom center":
                    tm.anchor = TextAnchor.LowerCenter;
                    break;
                case "bottom right":
                    tm.anchor = TextAnchor.LowerRight;
                    break;
            }
            if (data.tabSize != null)
                tm.tabSize = (float)data.tabSize;
            tm.richText = false; // a-frame doesnt support tags?
            //tm.anchor = TextAnchor.MiddleCenter;
            //tm.fontStyle = ???
            //if (data.font != null)
            //{
            //    switch ((string)data.font)
            //    {
            //        default:
            //        case "roboto":
            //        case "aileronsemibold":
            //        case "dejavu":
            //        case "exo2bold":
            //        case "exo2semibold":
            //        case "kelsonsans":
            //        case "monoid":
            //        case "mozillavr":
            //        case "sourcecodepro":
            //            tm.font = ???;
            //            break;
            //    }
            //}

            // font material needs to occlude like "TextMeshPro/Bitmap"
        }

        // light
        public static void ToArenaLight(GameObject gobj, ref dynamic data)
        {
            // TODO: translate from RenderSettings.ambientMode, may need centralized one-time publish

            Light light = gobj.GetComponent<Light>();
            switch (light.type)
            {
                case LightType.Directional:
                    data.type = "directional";
                    break;
                case LightType.Point:
                    data.type = "point";
                    data.distance = ArenaFloat(light.range);
                    break;
                case LightType.Spot:
                    data.type = "spot";
                    data.distance = ArenaFloat(light.range);
                    data.angle = ArenaFloat(light.spotAngle);
                    break;
            }
            data.intensity = ArenaFloat(light.intensity);
            data.color = ToArenaColor(light.color);
            data.castShadow = light.shadows != LightShadows.None;
        }
        public static void ToUnityLight(dynamic data, ref GameObject gobj)
        {
            // support legacy lights
            dynamic ldata = data.light ?? data;
            if (ldata.type != null)
            {
                if ((string)ldata.type == "ambient")
                {
                    RenderSettings.ambientMode = AmbientMode.Flat;
                    if (ldata.intensity != null)
                        RenderSettings.ambientIntensity = (float)ldata.intensity;
                    if (ldata.color != null)
                        RenderSettings.ambientLight = ToUnityColor((string)ldata.color);
                }
                else
                {
                    Light light = gobj.AddComponent<Light>();
                    switch ((string)ldata.type)
                    {
                        case "directional":
                            light.type = LightType.Directional;
                            break;
                        case "point":
                            light.type = LightType.Point;
                            if (ldata.distance != null)
                                light.range = (float)ldata.distance;
                            break;
                        case "spot":
                            light.type = LightType.Spot;
                            if (ldata.distance != null)
                                light.range = (float)ldata.distance;
                            if (ldata.angle != null)
                                light.spotAngle = (float)ldata.angle;
                            break;
                    }
                    if (ldata.intensity != null)
                        light.intensity = (float)ldata.intensity;
                    if (ldata.color != null)
                        light.color = ToUnityColor((string)ldata.color);
                    light.shadows = ldata.castShadow == null ? LightShadows.None : LightShadows.Hard;
                }
            }
        }
        // material
        public static void ToArenaMaterial(GameObject obj, ref dynamic data)
        {
            Renderer renderer = obj.GetComponent<Renderer>();
            // object shadows
            if (renderer != null)
            {
                dynamic shadow = new ExpandoObject();
                data.shadow = shadow;
                data.shadow.cast = renderer.shadowCastingMode != ShadowCastingMode.Off;
                data.shadow.receive = renderer.receiveShadows;
            }
            // object material
            Material mat = renderer.material;
            if (!mat)
                return;
            dynamic material = new ExpandoObject();
            data.material = material;
            // shaders only
            if (mat.shader.name == "Standard")
            {
                data.material.shader = "standard";
                //data.url = ToArenaTexture(mat);
                //data.material.repeat = ArenaFloat(mat.mainTextureScale.x);
                data.material.color = ToArenaColor(mat.color);
                //data.material.metalness = ArenaFloat(mat.GetFloat("_Metallic"));
                //data.material.roughness = ArenaFloat(1f - mat.GetFloat("_Glossiness"));
                data.material.transparent = mat.GetFloat("_Mode") == 3f ? true : false;
                data.material.opacity = ArenaFloat(mat.color.a);
                //if (mat.color.a == 1f)
                //    data.material.side = "double";
            }
            else if (mat.shader.name == "Unlit/Color")
            {
                data.material.shader = "flat";
                //data.material.side = "double";
            }
            else if (mat.shader.name == "Unlit/Texture")
            {
                data.material.shader = "flat";
                //data.url = ToArenaTexture(mat);
                //data.material.repeat = ArenaFloat(mat.mainTextureScale.x);
                //data.material.side = "double";
            }
            else if (mat.shader.name == "Unlit/Texture Colored")
            {
                data.material.shader = "flat";
                //data.url = ToArenaTexture(mat);
                //data.material.repeat = ArenaFloat(mat.mainTextureScale.x);
                data.material.color = ToArenaColor(mat.color);
                //data.material.side = "double";
            }
            else if (mat.shader.name == "Legacy Shaders/Transparent/Diffuse")
            {
                data.material.shader = "flat";
                //data.url = ToArenaTexture(mat);
                //data.material.repeat = ArenaFloat(mat.mainTextureScale.x);
                data.material.color = ToArenaColor(mat.color);
                data.material.transparent = true;
                data.material.opacity = ArenaFloat(mat.color.a);
                //if (mat.color.a == 1f)
                //    data.material.side = "double";
            }
            else
            {
                // other shaders
                data.material.shader = "standard";
                //data.url = ToArenaTexture(mat);
                //data.material.repeat = ArenaFloat(mat.mainTextureScale.x);
                if (mat.HasProperty("_Color"))
                    data.material.color = ToArenaColor(mat.color);
                //data.material.side = "double";
            }
        }
        public static void ToUnityMaterial(dynamic data, ref GameObject gobj)
        {
            var renderer = gobj.GetComponent<Renderer>();
            if (renderer != null)
            {
                // object shadows
                if (data.shadow != null)
                {
                    if (data.shadow.cast != null)
                        renderer.shadowCastingMode = Convert.ToBoolean(data.shadow.cast) ? ShadowCastingMode.On : ShadowCastingMode.Off;
                    if (data.shadow.receive != null)
                        renderer.receiveShadows = Convert.ToBoolean(data.shadow.receive);
                }
                // object material
                var material = renderer.material;
                // legacy color overrides material color in the arena
                if (data.color != null) // support legacy arena color
                    material.SetColor("_Color", ToUnityColor((string)data.color));
                else if (data.material != null && data.material.color != null)
                    material.SetColor("_Color", ToUnityColor((string)data.material.color));
                if (data.material != null)
                {
                    if (data.material.shader != null)
                        material.shader.name = (string)data.material.shader == "flat" ? "Unlit/Color" : "Standard";
                    if (data.material.opacity != null)
                    {
                        Color c = material.GetColor("_Color");
                        material.SetColor("_Color", new Color(c.r, c.g, c.b, (float)data.material.opacity));
                    }
                    if (data.material.transparent != null)
                    {
                        // For runtime set/change transparency mode, follow GUI params
                        // https://github.com/Unity-Technologies/UnityCsReference/blob/master/Editor/Mono/Inspector/StandardShaderGUI.cs#L344
                        if (Convert.ToBoolean(data.material.transparent))
                        {
                            material.SetFloat("_Mode", 3f); // StandardShaderGUI.BlendMode.Transparent
                            material.SetInt("_SrcBlend", (int)BlendMode.One);
                            material.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
                            material.SetInt("_ZWrite", 0);
                            material.DisableKeyword("_ALPHATEST_ON");
                            material.DisableKeyword("_ALPHABLEND_ON");
                            material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                            material.renderQueue = 3000;
                        }
                        else
                        {
                            material.SetFloat("_Mode", 0f); // StandardShaderGUI.BlendMode.Opaque
                            material.SetInt("_SrcBlend", (int)BlendMode.One);
                            material.SetInt("_DstBlend", (int)BlendMode.Zero);
                            material.SetInt("_ZWrite", 1);
                            material.DisableKeyword("_ALPHATEST_ON");
                            material.DisableKeyword("_ALPHABLEND_ON");
                            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                            material.renderQueue = -1;
                        }
                    }
                }
            }
        }

    }
}
