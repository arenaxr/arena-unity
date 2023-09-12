/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021, The CONIX Research Center. All rights reserved.
 */

using System;
using System.Dynamic;
using ArenaUnity.Components;
using ArenaUnity.Schemas;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

namespace ArenaUnity
{
    /// <summary>
    /// Static utility class for object translation.
    /// </summary>
    public static class ArenaUnity
    {
        public const float LineSinglePixelInMeters = 0.005f;
        private static string ColorPropertyName = (!GraphicsSettings.renderPipelineAsset ? "_Color" : "_BaseColor");
        public enum MatRendMode
        {   // TODO: the standards for "_Mode" seem to be missing?
            Opaque = 0,
            Cutout = 1,
            Fade = 2,
            Transparent = 3
        }

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
            TextMeshPro tm = gobj.GetComponent<TextMeshPro>();
            Light light = gobj.GetComponent<Light>();
            SpriteRenderer spriteRenderer = gobj.GetComponent<SpriteRenderer>();
            LineRenderer lr = gobj.GetComponent<LineRenderer>();
            // initial priority is primitive
            if (spriteRenderer && spriteRenderer.sprite && spriteRenderer.sprite.pixelsPerUnit != 0f)
                objectType = "image";
            else if (lr)
                objectType = "thickline";
            else if (tm)
                objectType = "text";
            else if (light)
                objectType = "light";
            else if (meshFilter && meshFilter.sharedMesh)
                objectType = meshFilter.sharedMesh.name.ToLower();
            return objectType;
        }

        public static void ToUnityMesh(DYNAMIC indata, ref GameObject gobj)
        {
            DYNAMIC data;
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
                // build your own meshes, defaults here should reflect ARENA/AFRAME defaults
                case "capsule":
                    ArenaMeshCapsule capsule = gobj.GetComponent<ArenaMeshCapsule>() ?? gobj.AddComponent<ArenaMeshCapsule>();
                    capsule.radius = data.radius != null ? (float)data.radius : 1f;
                    capsule.length = data.length != null ? (float)data.length : 1f;
                    capsule.radialSegments = data.segmentsRadial != null ? (int)data.segmentsRadial : 36;
                    capsule.heightSegments = data.segmentsCap != null ? (int)data.segmentsCap : 18;
                    break;
                case "box":
                case "cube": // support legacy arena 'cube' == 'box'
                    ArenaMeshBox cube = gobj.GetComponent<ArenaMeshBox>() ?? gobj.AddComponent<ArenaMeshBox>();
                    cube.width = data.width != null ? (float)data.width : 1f;
                    cube.height = data.height != null ? (float)data.height : 1f;
                    cube.depth = data.depth != null ? (float)data.depth : 1f;
                    cube.widthSegments = data.segmentsWidth != null ? (int)data.segmentsWidth : 1;
                    cube.heightSegments = data.segmentsHeight != null ? (int)data.segmentsHeight : 1;
                    cube.depthSegments = data.segmentsDepth != null ? (int)data.segmentsDepth : 1;
                    break;
                case "cone":
                    ArenaMeshCone cone = gobj.GetComponent<ArenaMeshCone>() ?? gobj.AddComponent<ArenaMeshCone>();
                    cone.radius = data.radiusBottom != null ? (float)data.radiusBottom : 1f;
                    //cone.radiusTop = data.radiusTop != null ? (float)data.radiusTop : 0.01f;
                    cone.height = data.height != null ? (float)data.height : 1f;
                    cone.subdivision = data.segmentsRadial != null ? (int)data.segmentsRadial : 36;
                    //cone.segmentsHeight = data.segmentsHeight != null ? (int)data.segmentsHeight : 18;
                    //cone.openEnded = data.openEnded != null ? Convert.ToBoolean(data.openEnded) : false;
                    //cone.thetaStart = (float)(data.thetaStart != null ? Mathf.PI / 180 * (float)data.thetaStart : 0f);
                    //cone.thetaLength = (float)(data.thetaLength != null ? Mathf.PI / 180 * (float)data.thetaLength : Mathf.PI * 2f);
                    break;
                case "cylinder":
                    ArenaMeshCylinder cylinder = gobj.GetComponent<ArenaMeshCylinder>() ?? gobj.AddComponent<ArenaMeshCylinder>();
                    cylinder.radius = data.radius != null ? (float)data.radius : 1f;
                    cylinder.height = data.height != null ? (float)data.height : 1f;
                    cylinder.radialSegments = data.segmentsRadial != null ? (int)data.segmentsRadial : 36;
                    cylinder.heightSegments = data.segmentsHeight != null ? (int)data.segmentsHeight : 18;
                    cylinder.openEnded = data.openEnded != null ? Convert.ToBoolean(data.openEnded) : false;
                    //cylinder.thetaStart = (float)(data.thetaStart != null ? Mathf.PI / 180 * (float)data.thetaStart : 0f);
                    //cylinder.thetaLength = (float)(data.thetaLength != null ? Mathf.PI / 180 * (float)data.thetaLength : Mathf.PI * 2f);
                    break;
                case "dodecahedron":
                    ArenaMeshDodecahedron dodecahedron = gobj.GetComponent<ArenaMeshDodecahedron>() ?? gobj.AddComponent<ArenaMeshDodecahedron>();
                    dodecahedron.radius = data.radius != null ? (float)data.radius : 1f;
                    dodecahedron.details = data.detail != null ? (int)data.detail : 0;
                    break;
                case "tetrahedron":
                    ArenaMeshTetrahedron tetrahedron = gobj.GetComponent<ArenaMeshTetrahedron>() ?? gobj.AddComponent<ArenaMeshTetrahedron>();
                    tetrahedron.radius = data.radius != null ? (float)data.radius : 1f;
                    tetrahedron.details = data.detail != null ? (int)data.detail : 0;
                    break;
                case "icosahedron":
                    ArenaMeshIcosahedron icosahedron = gobj.GetComponent<ArenaMeshIcosahedron>() ?? gobj.AddComponent<ArenaMeshIcosahedron>();
                    icosahedron.radius = data.radius != null ? (float)data.radius : 1f;
                    icosahedron.details = data.detail != null ? (int)data.detail : 0;
                    break;
                case "octahedron":
                    ArenaMeshOctahedron octahedron = gobj.GetComponent<ArenaMeshOctahedron>() ?? gobj.AddComponent<ArenaMeshOctahedron>();
                    octahedron.radius = data.radius != null ? (float)data.radius : 1f;
                    octahedron.details = data.detail != null ? (int)data.detail : 0;
                    break;
                case "plane":
                    ArenaMeshPlane plane = gobj.GetComponent<ArenaMeshPlane>() ?? gobj.AddComponent<ArenaMeshPlane>();
                    plane.width = data.width != null ? (float)data.width : 1f;
                    plane.height = data.height != null ? (float)data.height : 1f;
                    plane.wSegments = data.segmentsWidth != null ? (int)data.segmentsWidth : 1;
                    plane.hSegments = data.segmentsHeight != null ? (int)data.segmentsHeight : 1;
                    break;
                case "ring":
                    ArenaMeshRing ring = gobj.GetComponent<ArenaMeshRing>() ?? gobj.AddComponent<ArenaMeshRing>();
                    ring.outerRadius = data.radiusOuter != null ? (float)data.radiusOuter : 1.2f;
                    ring.innerRadius = data.radiusInner != null ? (float)data.radiusInner : 0.8f;
                    ring.phiSegments = data.segmentsPhi != null ? (int)data.segmentsPhi : 10;
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
                case "videosphere": // use sphere as a videosphere placeholder
                case "sphere":
                    ArenaMeshSphere sphere = gobj.GetComponent<ArenaMeshSphere>() ?? gobj.AddComponent<ArenaMeshSphere>();
                    sphere.radius = data.radius != null ? (float)data.radius : 1f;
                    sphere.latSegments = data.segmentsWidth != null ? (int)data.segmentsWidth : 18;
                    sphere.lonSegments = data.segmentsHeight != null ? (int)data.segmentsHeight : 36;
                    //sphere.phiStart = (float)(data.phiStart != null ? Mathf.PI / 180 * (float)data.phiStart : 0f);
                    //sphere.phiLength = (float)(data.phiLength != null ? Mathf.PI / 180 * (float)data.phiLength : Mathf.PI * 2f);
                    //sphere.thetaStart = (float)(data.thetaStart != null ? Mathf.PI / 180 * (float)data.thetaStart : 0f);
                    //sphere.thetaLength = (float)(data.thetaLength != null ? Mathf.PI / 180 * (float)data.thetaLength : Mathf.PI);
                    break;
                case "torus":
                    ArenaMeshTorus torus = gobj.GetComponent<ArenaMeshTorus>() ?? gobj.AddComponent<ArenaMeshTorus>();
                    torus.radius = data.radius != null ? (float)data.radius : 1f;
                    torus.radiusTubular = data.radiusTubular != null ? (float)data.radiusTubular : 0.2f;
                    torus.radialSegments = data.segmentsRadial != null ? (int)data.segmentsRadial : 36;
                    torus.thetaSegments = data.segmentsTubular != null ? (int)data.segmentsTubular : 32;
                    torus.thetaStart = 0f;
                    torus.thetaEnd = (float)(data.arc != null ? Mathf.PI / 180 * (float)data.arc : Mathf.PI * 2f);
                    break;
                case "torusKnot":
                    ArenaMeshTorusKnot torusKnot = gobj.GetComponent<ArenaMeshTorusKnot>() ?? gobj.AddComponent<ArenaMeshTorusKnot>();
                    torusKnot.radius = data.radius != null ? (float)data.radius : 1f;
                    torusKnot.radiusTubular = data.radiusTubular != null ? (float)data.radiusTubular : 0.2f;
                    torusKnot.radialSegments = data.segmentsRadial != null ? (int)data.segmentsRadial : 8;
                    torusKnot.thetaSegments = data.segmentsTubular != null ? (int)data.segmentsTubular : 100;
                    torusKnot.p = data.p != null ? (float)data.p : 2f;
                    torusKnot.q = data.q != null ? (float)data.q : 3f;
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
        public static void ToArenaMesh(GameObject gobj, ref DYNAMIC data)
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
                case "ArenaUnity.ArenaMeshBox":
                    var cube = gobj.GetComponent<ArenaMeshBox>();
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

        // position
        public static string ToArenaPositionString(Vector3 position)
        {
            return $"{ArenaFloat(position.x)} {ArenaFloat(position.y)} {ArenaFloat(-position.z)}";
        }
        public static Vector3 ToUnityPositionString(string strPos)
        {
            string[] axis = strPos.Split(new char[] { ' ' }, 3, StringSplitOptions.RemoveEmptyEntries);
            return new Vector3(
                float.Parse(axis[0]),
                float.Parse(axis[1]),
                -float.Parse(axis[2])
            );
        }
        public static ArenaPositionJson ToArenaPosition(Vector3 position)
        {
            return new ArenaPositionJson
            {
                X = ArenaFloat(position.x),
                Y = ArenaFloat(position.y),
                Z = ArenaFloat(-position.z)
            };
        }
        public static Vector3 ToUnityPosition(ArenaPositionJson position)
        {
            return new Vector3(
                (float)position.X,
                (float)position.Y,
                -(float)position.Z
            );
        }

        // rotation
        public static ArenaRotationJson ToArenaRotationQuat(Quaternion rotationQuat, bool invertY = true)
        {
            return new ArenaRotationJson
            {
                X = ArenaFloat(-rotationQuat.x),
                Y = ArenaFloat(rotationQuat.y * (invertY ? -1 : 1)),
                Z = ArenaFloat(rotationQuat.z),
                W = ArenaFloat(rotationQuat.w)
            };
        }
        public static Quaternion ToUnityRotationQuat(ArenaRotationJson rotationQuat, bool invertY = true)
        {
            return new Quaternion(
                -(float)rotationQuat.X,
                (float)rotationQuat.Y * (invertY ? -1 : 1),
                (float)rotationQuat.Z,
                (float)rotationQuat.W
            );
        }
        /// <summary>
        /// Converts Vector3 rotationEuler to DYNAMIC rotationEuler. CAUTION: Do not use for ARENA!
        /// A merge with quaternion type will leave a mix of xyz euler and w quaternion = badness.
        /// </summary>
        public static ArenaVector3Json ToArenaRotationEuler(Vector3 rotationEuler, bool invertY = true)
        {
            return new ArenaVector3Json
            {
                X = ArenaFloat(-rotationEuler.x),
                Y = ArenaFloat(rotationEuler.y * (invertY ? -1 : 1)),
                Z = ArenaFloat(rotationEuler.z)
            };
        }
        public static Quaternion ToUnityRotationEuler(ArenaVector3Json rotationEuler, bool invertY = true)
        {
            return Quaternion.Euler(
                -(float)rotationEuler.X,
                (float)rotationEuler.Y * (invertY ? -1 : 1),
                (float)rotationEuler.Z
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
        public static ArenaScaleJson ToArenaScale(Vector3 scale)
        {
            return new ArenaScaleJson
            {
                X = ArenaFloat(scale.x),
                Y = ArenaFloat(scale.y),
                Z = ArenaFloat(scale.z)
            };
        }
        public static Vector3 ToUnityScale(ArenaScaleJson scale)
        {
            return new Vector3(
                (float)scale.X,
                (float)scale.Y,
                (float)scale.Z
            );
        }
        // size dimensions
        public static void ToArenaDimensions(GameObject gobj, ref DYNAMIC data)
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
        public static Color ToUnityColor(string color, float opacity)
        {
            Color c = ToUnityColor(color);
            return new Color(c.r, c.g, c.b, opacity);
        }
        public static Color ColorRandom()
        {
            return UnityEngine.Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
        }

        // line/thickline
        public static void ToArenaThickline(GameObject gobj, ref ArenaThicklineJson data)
        {
            // TODO: support Material opacity/visibility
            LineRenderer line = gobj.GetComponent<LineRenderer>();
            // always use thickline, too many variables otherwise
            data.object_type = "thickline";
            string[] positions = new string[line.positionCount];
            Vector3[] vertices = new Vector3[line.positionCount];
            line.GetPositions(vertices);
            if (line.useWorldSpace)
            {   // line.useWorldSpace does not match arena thickline which always uses position
                for (var i = 0; i < line.positionCount; i++)
                {
                    // arena ignores line position transform, so translate every position?...
                    positions[i] = ToArenaPositionString(vertices[i] - gobj.transform.localPosition);
                }
                // TODO: does arena ignores thickline rotation?
            }
            else
            {   // !line.useWorldSpace matches arena thickline which always uses position
                for (var i = 0; i < line.positionCount; i++)
                {
                    positions[i] = ToArenaPositionString(vertices[i]);
                }
            }
            data.Path = string.Join(",", positions);
            data.LineWidth = (int)(line.startWidth / LineSinglePixelInMeters); // TODO: support endWidth
            data.Color = ToArenaColor(line.startColor); // TODO: support endColor
        }
        public static void ToUnityLine(ArenaLineJson data, ref GameObject gobj)
        {
            LineRenderer line = gobj.GetComponent<LineRenderer>();
            if (line == null)
                line = gobj.AddComponent<LineRenderer>();

            float pixelWidth = 1f; // default
            line.useWorldSpace = true; // match arena line which always ignores position
            if (data.Start != null && data.End != null)
            {
                Vector3[] nodes = {
                            ToUnityPosition(data.Start),
                            ToUnityPosition(data.End),
                        };
                line.SetPositions(nodes);
            }
            if (data.Color != null)
                line.startColor = line.endColor = ToUnityColor((string)data.Color);
            line.widthMultiplier = pixelWidth * LineSinglePixelInMeters;
        }
        public static void ToUnityThickline(ArenaThicklineJson data, ref GameObject gobj)
        {
            LineRenderer line = gobj.GetComponent<LineRenderer>();
            if (line == null)
                line = gobj.AddComponent<LineRenderer>();

            float pixelWidth = 1f; // default
            line.useWorldSpace = false; // match arena thickline which always uses position
            if (data.Path != null)
            {
                string[] nodes = ((string)data.Path).Split(new char[] { ',' });
                line.positionCount = nodes.Length;
                for (var i = 0; i < nodes.Length; i++)
                {
                    Vector3 position = ToUnityPositionString(nodes[i]);
                    line.SetPosition(i, position);
                }
            }
            pixelWidth = (float)data.LineWidth;
            if (data.Color != null)
                line.startColor = line.endColor = ToUnityColor((string)data.Color);
            // convert arena thickline pixels vs unity meters
            float numkeys = 2;
            float w = 1;
            AnimationCurve curve = new AnimationCurve();
            switch (data.LineWidthStyler)
            {
                case ArenaThicklineJson.LineWidthStylerType.CenterSharp: numkeys = 3; break;
                case ArenaThicklineJson.LineWidthStylerType.CenterSmooth: numkeys = 10; break;
                case ArenaThicklineJson.LineWidthStylerType.SineWave: numkeys = 10; break;
            }
            for (int i = 0; i < numkeys; i++)
            {
                float p = i / (numkeys - 1);
                switch (data.LineWidthStyler)
                {
                    case ArenaThicklineJson.LineWidthStylerType.Grow:
                        w = p;
                        break;
                    case ArenaThicklineJson.LineWidthStylerType.Shrink:
                        w = 1 - p;
                        break;
                    case ArenaThicklineJson.LineWidthStylerType.CenterSharp:
                        w = 1 - Math.Abs(2 * p - 1);
                        break;
                    case ArenaThicklineJson.LineWidthStylerType.CenterSmooth:
                        w = (float)Math.Sin(p * 3.1415);
                        break;
                    case ArenaThicklineJson.LineWidthStylerType.SineWave:
                        w = (float)(0.5 + 0.5 * Math.Sin((p - 0.5) * 2 * 3.1415 * 10));
                        break;
                }
                curve.AddKey(p, w);
            }
            line.widthCurve = curve;
            line.widthMultiplier = pixelWidth * LineSinglePixelInMeters;
        }

        // text
        public static void ToArenaText(GameObject gobj, ref ArenaTextJson data)
        {
            TextMeshPro tm = gobj.GetComponent<TextMeshPro>();
            //tm.fontSize;
            data.Value = tm.text;
            data.Color = ToArenaColor(tm.color);
            data.Width = tm.rectTransform.rect.width;
            data.Height = tm.rectTransform.rect.height;
            // TODO (mwfarb): restore alignment
            //switch (tm.alignment)
            //{
            //    case TextAlignmentOptions.TopLeft:
            //        data.Baseline = "top";
            //        data.Anchor = "left";
            //        break;
            //    case TextAlignmentOptions.Top:
            //        data.Baseline = "top";
            //        data.Anchor = "center";
            //        break;
            //    case TextAlignmentOptions.TopRight:
            //        data.Baseline = "top";
            //        data.Anchor = "right";
            //        break;
            //    case TextAlignmentOptions.TopGeoAligned:
            //        data.Baseline = "top";
            //        data.Anchor = "align";
            //        break;
            //    case TextAlignmentOptions.BaselineLeft:
            //        data.Baseline = "center";
            //        data.Anchor = "left";
            //        break;
            //    case TextAlignmentOptions.Center:
            //        data.Baseline = "center";
            //        data.Anchor = "center";
            //        break;
            //    case TextAlignmentOptions.BaselineRight:
            //        data.Baseline = "center";
            //        data.Anchor = "right";
            //        break;
            //    case TextAlignmentOptions.CenterGeoAligned:
            //        data.Baseline = "center";
            //        data.Anchor = "align";
            //        break;
            //    case TextAlignmentOptions.BottomLeft:
            //        data.Baseline = "bottom";
            //        data.Anchor = "left";
            //        break;
            //    case TextAlignmentOptions.Bottom:
            //        data.Baseline = "bottom";
            //        data.Anchor = "center";
            //        break;
            //}
        }
        public static void ToUnityText(ArenaTextJson data, ref GameObject gobj)
        {
            TextMeshPro tm = gobj.GetComponent<TextMeshPro>();
            if (tm == null)
                tm = gobj.AddComponent<TextMeshPro>();
            tm.fontSize = 2;

            if (data.Value != null)
                tm.text = (string)data.Value;
            if (data.Color != null)
                tm.color = ToUnityColor((string)data.Color);

            RectTransform rt = gobj.GetComponent<RectTransform>();
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, (float)data.Width);
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, (float)data.Height);
            rt.ForceUpdateRectTransforms();
            // TODO (mwfarb): restore alignment
            //string anchor = data.Anchor != null ? data.Anchor : ArenaTextJson.AnchorType.Center;
            //string baseline = data.Baseline != null ? data.Baseline : ArenaTextJson.BaselineType.Center;
            //switch ($"{baseline} {anchor}")
            //{
            //    case "top left":
            //        tm.alignment = TextAlignmentOptions.TopLeft;
            //        break;
            //    case "top center":
            //        tm.alignment = TextAlignmentOptions.Top;
            //        break;
            //    case "top right":
            //        tm.alignment = TextAlignmentOptions.TopRight;
            //        break;
            //    case "top align":
            //        tm.alignment = TextAlignmentOptions.TopGeoAligned;
            //        break;
            //    case "center left":
            //        tm.alignment = TextAlignmentOptions.BaselineLeft;
            //        break;
            //    case "center center":
            //        tm.alignment = TextAlignmentOptions.Center;
            //        break;
            //    case "center right":
            //        tm.alignment = TextAlignmentOptions.BaselineRight;
            //        break;
            //    case "center align":
            //        tm.alignment = TextAlignmentOptions.CenterGeoAligned;
            //        break;
            //    case "bottom left":
            //        tm.alignment = TextAlignmentOptions.BottomLeft;
            //        break;
            //    case "bottom center":
            //        tm.alignment = TextAlignmentOptions.Bottom;
            //        break;
            //    case "bottom right":
            //        tm.alignment = TextAlignmentOptions.BottomRight;
            //        break;
            //    case "bottom align":
            //        tm.alignment = TextAlignmentOptions.BottomGeoAligned;
            //        break;
            //}
        }

        // light
        public static void ToArenaLight(GameObject gobj, ref ArenaLightJson data)
        {
            // TODO: translate from RenderSettings.ambientMode, may need centralized one-time publish

            Light light = gobj.GetComponent<Light>();
            switch (light.type)
            {
                case LightType.Directional:
                    data.Type = ArenaLightJson.TypeType.Directional;
                    break;
                case LightType.Point:
                    data.Type = ArenaLightJson.TypeType.Point;
                    data.Distance = ArenaFloat(light.range);
                    break;
                case LightType.Spot:
                    data.Type = ArenaLightJson.TypeType.Spot;
                    data.Distance = ArenaFloat(light.range);
                    data.Angle = ArenaFloat(light.spotAngle);
                    break;
            }
            data.Intensity = ArenaFloat(light.intensity);
            data.Color = ToArenaColor(light.color);
            data.CastShadow = light.shadows != LightShadows.None;
        }
        public static void ToUnityLight(ArenaLightJson data, ref GameObject gobj)
        {
                if (data.Type == ArenaLightJson.TypeType.Ambient)
                {
                    RenderSettings.ambientMode = AmbientMode.Flat;
                    RenderSettings.ambientIntensity = (float)data.Intensity;
                    if (data.Color != null)
                        RenderSettings.ambientLight = ToUnityColor((string)data.Color);
                }
                else
                {
                    Light light = gobj.GetComponent<Light>();
                    if (light == null)
                        light = gobj.AddComponent<Light>();
                    switch (data.Type)
                    {
                        case ArenaLightJson.TypeType.Directional:
                            light.type = LightType.Directional;
                            break;
                        case ArenaLightJson.TypeType.Point:
                            light.type = LightType.Point;
                            light.range = (float)data.Distance;
                            break;
                        case ArenaLightJson.TypeType.Spot:
                            light.type = LightType.Spot;
                            light.range = (float)data.Distance;
                            light.spotAngle = (float)data.Angle;
                            break;
                    }
                    light.intensity = (float)data.Intensity;
                    if (data.Color != null)
                        light.color = ToUnityColor((string)data.Color);
                    light.shadows = !data.CastShadow ? LightShadows.None : LightShadows.Hard;
                }
        }
        // material
        public static void ToArenaMaterial(GameObject obj, ref ArenaMaterialJson data)
        {
            Renderer renderer = obj.GetComponent<Renderer>();
            // object material
            Material mat = renderer.material;
            if (!mat)
                return;
            // shaders only
            switch (mat.shader.name)
            {
                default:
                case "Standard":
                    data.Shader = "standard"; break;
                case "Unlit/Color":
                case "Unlit/Texture":
                case "Unlit/Texture Colored":
                case "Legacy Shaders/Transparent/Diffuse":
                    data.Shader = "flat"; break;
            }
            //data.url = ToArenaTexture(mat);
            if (mat.HasProperty(ColorPropertyName))
                data.Color = ToArenaColor(mat.color);
            //data.metalness = ArenaFloat(mat.GetFloat("_Metallic"));
            //data.roughness = ArenaFloat(1f - mat.GetFloat("_Glossiness"));
            //data.repeat = ArenaFloat(mat.mainTextureScale.x);
            //data.side = "double";
            switch ((MatRendMode)mat.GetFloat("_Mode"))
            {
                case MatRendMode.Opaque:
                case MatRendMode.Fade:
                    data.Transparent = false; break;
                case MatRendMode.Transparent:
                case MatRendMode.Cutout:
                    data.Transparent = true; break;
            }
            data.Opacity = ArenaFloat(mat.color.a);

        }
        public static void ToUnityMaterial(ArenaMaterialJson data, ref GameObject gobj)
        {
            var renderer = gobj.GetComponent<Renderer>();
            if (renderer != null)
            {
                // object material
                var material = renderer.material;
                if (GraphicsSettings.renderPipelineAsset)
                {
                    if (GraphicsSettings.renderPipelineAsset.GetType().ToString().Contains("HDRenderPipelineAsset"))
                        material.shader = Shader.Find("HDRP/Lit");
                    else
                        material.shader = Shader.Find("Universal Render Pipeline/Lit");
                }
                float opacity = (data != null && data.Opacity != null) ? (float)data.Opacity : 1f;
                if (data != null)
                {
                    bool transparent = Convert.ToBoolean(data.Transparent);
                    bool opaque = opacity >= 1f;
                    if (data.Color != null)
                        material.SetColor(ColorPropertyName, ToUnityColor((string)data.Color, opacity));
                    if (data.Opacity != null)
                    {
                        Color c = material.GetColor(ColorPropertyName);
                        material.SetColor(ColorPropertyName, new Color(c.r, c.g, c.b, opacity));
                    }
                    if (data.Shader != null)
                        material.shader.name = (string)data.Shader == "flat" ? "Unlit/Color" : "Standard";
                    // For runtime set/change transparency mode, follow GUI params
                    // https://github.com/Unity-Technologies/UnityCsReference/blob/master/Editor/Mono/Inspector/StandardShaderGUI.cs#L344
                    if (opacity >= 1f)
                    {   // op == 1
                        material.SetFloat("_Mode", (float)MatRendMode.Opaque);
                        material.SetInt("_SrcBlend", (int)BlendMode.One);
                        material.SetInt("_DstBlend", (int)BlendMode.Zero);
                        material.SetInt("_ZWrite", 1);
                        material.DisableKeyword("_ALPHATEST_ON");
                        material.DisableKeyword("_ALPHABLEND_ON");
                        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                        material.renderQueue = -1;
                    }
                    if (opacity <= 0f)
                    {   // op == 0
                        material.SetFloat("_Mode", (float)MatRendMode.Transparent);
                        material.SetInt("_SrcBlend", (int)BlendMode.One);
                        material.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
                        material.SetInt("_ZWrite", 0);
                        material.EnableKeyword("_ALPHATEST_ON");
                        material.EnableKeyword("_ALPHABLEND_ON");
                        material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                        material.renderQueue = 3000;
                    }
                    else
                    {   // op 0-1
                        material.SetFloat("_Mode", (float)MatRendMode.Fade);
                        material.SetInt("_SrcBlend", (int)BlendMode.One);
                        material.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
                        material.SetInt("_ZWrite", 0);
                        material.DisableKeyword("_ALPHATEST_ON");
                        material.DisableKeyword("_ALPHABLEND_ON");
                        material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                        material.renderQueue = 3000;
                    }
                }
            }
        }
        // animation-mixer
        internal static void ToArenaAnimationMixer(GameObject gobj, ref JObject jData)
        {
            ArenaAnimationMixer am = gobj.GetComponent<ArenaAnimationMixer>();
            jData["animation-mixer"] = am.json.SaveToString();
        }
        internal static void ToUnityAnimationMixer(JObject jData, ref GameObject gobj)
        {
            JToken jToken = jData.SelectToken("animation-mixer");
            if (jToken != null && jToken.Type != JTokenType.Null)
            {
                ArenaAnimationMixer am = gobj.GetComponent<ArenaAnimationMixer>();
                if (am == null)
                    am = gobj.AddComponent<ArenaAnimationMixer>();
                am.json = ArenaAnimationMixerJson.CreateFromJSON(JsonConvert.SerializeObject(jToken), jToken);
                am.apply = true;
            }
        }
        // click-listener
        internal static void ToArenaClickListener(GameObject gobj, ref JObject jData)
        {
            ArenaClickListener am = gobj.GetComponent<ArenaClickListener>();
            jData["click-listener"] = true;
        }
        internal static void ToUnityClickListener(JObject jData, ref GameObject gobj)
        {
            JToken jToken = jData.SelectToken("click-listener");
            // we accept any string or boolean true
            if ((jToken != null && jToken.Type != JTokenType.Null) || (jToken.Type == JTokenType.Boolean && jToken.Value<bool>()))
            {
                ArenaClickListener cl = gobj.GetComponent<ArenaClickListener>();
                if (cl == null)
                    cl = gobj.AddComponent<ArenaClickListener>();
            }
        }
    }
}
