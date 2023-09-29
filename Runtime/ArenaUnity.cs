/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2023, Carnegie Mellon University. All rights reserved.
 */

using System;
using ArenaUnity.Components;
using ArenaUnity.Schemas;
using Newtonsoft.Json;
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
        public static string ColorPropertyName = (!GraphicsSettings.renderPipelineAsset ? "_Color" : "_BaseColor");
        public const float LineSinglePixelInMeters = 0.005f;
        public static float ArenaFloat(float n) { return (float)Math.Round(n, 3); }

        public static readonly string[] primitives = {
            "box",
            "capsule",
            "circle",
            "cone",
            "cube", // deprecated
            "cylinder",
            "dodecahedron",
            "icosahedron",
            "octahedron",
            "plane",
            "ring",
            "roundedbox",
            "sphere",
            "tetrahedron",
            "torus",
            "torusKnot",
            "triangle",
            "videosphere"
        };

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
        public static ArenaVector3Json ToArenaPosition(Vector3 position)
        {
            return new ArenaVector3Json
            {
                X = ArenaFloat(position.x),
                Y = ArenaFloat(position.y),
                Z = ArenaFloat(-position.z)
            };
        }
        public static Vector3 ToUnityPosition(ArenaVector3Json position)
        {
            return new Vector3(
                (float)position.X,
                (float)position.Y,
                -(float)position.Z
            );
        }
        public static Vector3 ToUnityPosition(Vector3 position)
        {
            return new Vector3(
                (float)position.x,
                (float)position.y,
                -(float)position.z
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
        public static Quaternion ToUnityRotationQuat(Quaternion rotationQuat, bool invertY = true)
        {
            return new Quaternion(
                -(float)rotationQuat.x,
                (float)rotationQuat.y * (invertY ? -1 : 1),
                (float)rotationQuat.z,
                (float)rotationQuat.w
            );
        }
        /// <summary>
        /// Converts Vector3 rotationEuler to dynamic rotationEuler. CAUTION: Do not use for ARENA!
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
        public static Quaternion ToUnityRotationEuler(ArenaRotationJson rotationEuler, bool invertY = true)
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
        public static ArenaRotationJson ToArenaRotationPlaneMesh(Quaternion rotationQuat)
        {
            rotationQuat *= Quaternion.Euler(90, 0, 0);
            return ToArenaRotationQuat(rotationQuat);
        }

        // scale
        public static ArenaVector3Json ToArenaScale(Vector3 scale)
        {
            return new ArenaVector3Json
            {
                X = ArenaFloat(scale.x),
                Y = ArenaFloat(scale.y),
                Z = ArenaFloat(scale.z)
            };
        }
        public static Vector3 ToUnityScale(ArenaVector3Json scale)
        {
            return new Vector3(
                (float)scale.X,
                (float)scale.Y,
                (float)scale.Z
            );
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

        // components

        public static void ApplyVisible(GameObject gobj, ArenaObjectDataJson data)
        {
            // TODO (mwfarb): handle realtime renderer changes from unity.
            // arena visible component does not render, but object scripts still run, so avoid keep object Active, but do not Render.
            var renderer = gobj.GetComponent<Renderer>();
            if (renderer != null)
                renderer.enabled = (bool)data.visible;
        }
        public static void ApplyRemoteRender(GameObject gobj, ArenaObjectDataJson data)
        {
            // arena visible component does not render, but object scripts still run, so avoid keep object Active, but do not Render.
            var renderer = gobj.GetComponent<Renderer>();
            if (renderer != null)
                renderer.enabled = data.remoteRender.Enabled;
        }

        public static void ApplyClickListener(GameObject gobj, ArenaObjectDataJson data)
        {
            if (!gobj.TryGetComponent<ArenaClickListener>(out var c))
                c = gobj.AddComponent<ArenaClickListener>();
            c.json = data.clickListener; c.apply = true;
        }

        public static void ApplyAnimationMixer(GameObject gobj, ArenaObjectDataJson data)
        {
            if (!gobj.TryGetComponent<ArenaAnimationMixer>(out var c))
                c = gobj.AddComponent<ArenaAnimationMixer>();
            c.json = data.animationMixer; c.apply = true;
        }

        public static void ApplyAttribution(GameObject gobj, ArenaObjectDataJson data)
        {
            if (!gobj.TryGetComponent<ArenaAttribution>(out var c))
                c = gobj.AddComponent<ArenaAttribution>();
            c.json = data.attribution; c.apply = true;
        }

        public static void ApplyMaterial(GameObject gobj, ArenaObjectDataJson data)
        {
            if (!gobj.TryGetComponent<ArenaMaterial>(out var c))
                c = gobj.AddComponent<ArenaMaterial>();
            c.json = data.material; c.apply = true;
        }

        // wire objects


        public static void ApplyWireTorus(object indata, GameObject gobj)
        {
            if (!gobj.TryGetComponent<ArenaMeshTorus>(out var torus))
                torus = gobj.AddComponent<ArenaMeshTorus>();
            torus.json = JsonConvert.DeserializeObject<ArenaTorusJson>(indata.ToString());
        }

        public static void ApplyWireTriangle(object indata, GameObject gobj)
        {
            if (!gobj.TryGetComponent<ArenaMeshTriangle>(out var triangle))
                triangle = gobj.AddComponent<ArenaMeshTriangle>();
            triangle.json = JsonConvert.DeserializeObject<ArenaTriangleJson>(indata.ToString());
        }

        public static void ApplyWireTorusKnot(object indata, GameObject gobj)
        {
            if (!gobj.TryGetComponent<ArenaMeshTorusKnot>(out var torusKnot))
                torusKnot = gobj.AddComponent<ArenaMeshTorusKnot>();
            torusKnot.json = JsonConvert.DeserializeObject<ArenaTorusKnotJson>(indata.ToString());
        }

        public static void ApplyWireSphere(object indata, GameObject gobj)
        {
            if (!gobj.TryGetComponent<ArenaMeshSphere>(out var sphere))
                sphere = gobj.AddComponent<ArenaMeshSphere>();
            sphere.json = JsonConvert.DeserializeObject<ArenaSphereJson>(indata.ToString());
        }

        public static void ApplyWireVideosphere(object indata, GameObject gobj)
        {
            if (!gobj.TryGetComponent<ArenaMeshVideosphere>(out var videosphere))
                videosphere = gobj.AddComponent<ArenaMeshVideosphere>();
            videosphere.json = JsonConvert.DeserializeObject<ArenaVideosphereJson>(indata.ToString());
        }

        public static void ApplyWireCircle(object indata, GameObject gobj)
        {
            if (!gobj.TryGetComponent<ArenaMeshCircle>(out var circle))
                circle = gobj.AddComponent<ArenaMeshCircle>();
            circle.json = JsonConvert.DeserializeObject<ArenaCircleJson>(indata.ToString());
        }

        public static void ApplyWireRing(object indata, GameObject gobj)
        {
            if (!gobj.TryGetComponent<ArenaMeshRing>(out var ring))
                ring = gobj.AddComponent<ArenaMeshRing>();
            ring.json = JsonConvert.DeserializeObject<ArenaRingJson>(indata.ToString());
        }

        public static void ApplyWirePlane(object indata, GameObject gobj)
        {
            if (!gobj.TryGetComponent<ArenaMeshPlane>(out var plane))
                plane = gobj.AddComponent<ArenaMeshPlane>();
            plane.json = JsonConvert.DeserializeObject<ArenaPlaneJson>(indata.ToString());
        }

        public static void ApplyWireOctahedron(object indata, GameObject gobj)
        {
            if (!gobj.TryGetComponent<ArenaMeshOctahedron>(out var octahedron))
                octahedron = gobj.AddComponent<ArenaMeshOctahedron>();
            octahedron.json = JsonConvert.DeserializeObject<ArenaOctahedronJson>(indata.ToString());
        }

        public static void ApplyWireIcosahedron(object indata, GameObject gobj)
        {
            if (!gobj.TryGetComponent<ArenaMeshIcosahedron>(out var icosahedron))
                icosahedron = gobj.AddComponent<ArenaMeshIcosahedron>();
            icosahedron.json = JsonConvert.DeserializeObject<ArenaIcosahedronJson>(indata.ToString());
        }

        public static void ApplyWireTetrahedron(object indata, GameObject gobj)
        {
            if (!gobj.TryGetComponent<ArenaMeshTetrahedron>(out var tetrahedron))
                tetrahedron = gobj.AddComponent<ArenaMeshTetrahedron>();
            tetrahedron.json = JsonConvert.DeserializeObject<ArenaTetrahedronJson>(indata.ToString());
        }

        public static void ApplyWireDodecahedron(object indata, GameObject gobj)
        {
            if (!gobj.TryGetComponent<ArenaMeshDodecahedron>(out var dodecahedron))
                dodecahedron = gobj.AddComponent<ArenaMeshDodecahedron>();
            dodecahedron.json = JsonConvert.DeserializeObject<ArenaDodecahedronJson>(indata.ToString());
        }

        public static void ApplyWireCylinder(object indata, GameObject gobj)
        {
            if (!gobj.TryGetComponent<ArenaMeshCylinder>(out var cylinder))
                cylinder = gobj.AddComponent<ArenaMeshCylinder>();
            cylinder.json = JsonConvert.DeserializeObject<ArenaCylinderJson>(indata.ToString());
        }

        public static void ApplyWireCone(object indata, GameObject gobj)
        {
            if (!gobj.TryGetComponent<ArenaMeshCone>(out var cone))
                cone = gobj.AddComponent<ArenaMeshCone>();
            cone.json = JsonConvert.DeserializeObject<ArenaConeJson>(indata.ToString());
        }

        public static void ApplyWireBox(object indata, GameObject gobj)
        {
            if (!gobj.TryGetComponent<ArenaMeshBox>(out var box))
                box = gobj.AddComponent<ArenaMeshBox>();
            box.json = JsonConvert.DeserializeObject<ArenaBoxJson>(indata.ToString());
        }

        public static void ApplyWireCapsule(object indata, GameObject gobj)
        {
            if (!gobj.TryGetComponent<ArenaMeshCapsule>(out var capsule))
                capsule = gobj.AddComponent<ArenaMeshCapsule>();
            capsule.json = JsonConvert.DeserializeObject<ArenaCapsuleJson>(indata.ToString());
        }

        public static void ApplyWireLight(object indata, GameObject gobj)
        {
            if (!gobj.TryGetComponent<ArenaWireLight>(out var lg))
                lg = gobj.AddComponent<ArenaWireLight>();
            lg.json = JsonConvert.DeserializeObject<ArenaLightJson>(indata.ToString()); lg.apply = true;
        }

        public static void ApplyWireThickline(object indata, GameObject gobj)
        {
            if (!gobj.TryGetComponent<ArenaWireThickline>(out var tl))
                tl = gobj.AddComponent<ArenaWireThickline>();
            tl.json = JsonConvert.DeserializeObject<ArenaThicklineJson>(indata.ToString()); tl.apply = true;
        }

        public static void ApplyWireLine(object indata, GameObject gobj)
        {
            if (!gobj.TryGetComponent<ArenaWireLine>(out var l))
                l = gobj.AddComponent<ArenaWireLine>();
            l.json = JsonConvert.DeserializeObject<ArenaLineJson>(indata.ToString()); l.apply = true;
        }

        public static void ApplyWireText(object indata, GameObject gobj)
        {
            if (!gobj.TryGetComponent<ArenaWireText>(out var t))
                t = gobj.AddComponent<ArenaWireText>();
            t.json = JsonConvert.DeserializeObject<ArenaTextJson>(indata.ToString()); t.apply = true;
        }

        // scene options

        public static void ApplyEnvironmentPresets(GameObject gobj, ArenaArenaSceneOptionsJson data)
        {
            if (!gobj.TryGetComponent<ArenaSceneEnvironmentalPresets>(out var c))
                c = gobj.AddComponent<ArenaSceneEnvironmentalPresets>();
            c.json = JsonConvert.DeserializeObject<ArenaEnvironmentPresetsJson>(data.EnvPresets.ToString()); c.apply = true;
        }

        public static void ApplySceneOptions(GameObject gobj, ArenaArenaSceneOptionsJson data)
        {
            if (!gobj.TryGetComponent<ArenaSceneOptions>(out var c))
                c = gobj.AddComponent<ArenaSceneOptions>();
            c.json = JsonConvert.DeserializeObject<ArenaSceneOptionsJson>(data.SceneOptions.ToString()); c.apply = true;
        }

        public static void ApplyRendererSettings(GameObject gobj, ArenaArenaSceneOptionsJson data)
        {
            if (!gobj.TryGetComponent<ArenaSceneRendererSettings>(out var c))
                c = gobj.AddComponent<ArenaSceneRendererSettings>();
            c.json = JsonConvert.DeserializeObject<ArenaRendererSettingsJson>(data.RendererSettings.ToString()); c.apply = true;
        }

        public static void ApplyPostProcessing(GameObject gobj, ArenaArenaSceneOptionsJson data)
        {
            if (!gobj.TryGetComponent<ArenaScenePostProcessing>(out var c))
                c = gobj.AddComponent<ArenaScenePostProcessing>();
            c.json = JsonConvert.DeserializeObject<ArenaPostProcessingJson>(data.PostProcessing.ToString()); c.apply = true;
        }
    }
}
