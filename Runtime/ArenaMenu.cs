/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2023, Carnegie Mellon University. All rights reserved.
 */

using System.Globalization;
using ArenaUnity.Schemas;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace ArenaUnity
{
    public static class ArenaMenu
    {
        private const string MatColor = "#7f7f7f";
#if UNITY_EDITOR
        // Add a menu item to create custom GameObjects.
        // Priority 1 ensures it is grouped with the other menu items of the same kind
        // and propagated to the hierarchy dropdown and hierarchy context menus.

        [MenuItem("GameObject/ARENA/Entity", false, 5)]
        internal static void CreateArenaEntity(MenuCommand menuCommand)
        {
            PublishWireObject(menuCommand, "entity");
        }

        [MenuItem("GameObject/ARENA/GLTF Model", false, 5)]
        internal static void CreateArenaGltfModel(MenuCommand menuCommand)
        {
            ArenaObjectAddUrlWindow window = (ArenaObjectAddUrlWindow)EditorWindow.GetWindow(typeof(ArenaObjectAddUrlWindow));
            window.Init("gltf-model", menuCommand);
            window.Show();
        }

        [MenuItem("GameObject/ARENA/Image", false, 5)]
        internal static void CreateArenaImage(MenuCommand menuCommand)
        {
            ArenaObjectAddUrlWindow window = (ArenaObjectAddUrlWindow)EditorWindow.GetWindow(typeof(ArenaObjectAddUrlWindow));
            window.Init("image", menuCommand);
            window.Show();
        }

        [MenuItem("GameObject/ARENA/Light", false, 5)]
        internal static void CreateArenaLight(MenuCommand menuCommand)
        {
            PublishWireObject(menuCommand, "light");
        }

        // Skip "line" since we don't have a Ray equivilent in Unity yet.
        // [MenuItem("GameObject/ARENA/Line", false, 5)]

        [MenuItem("GameObject/ARENA/Text", false, 5)]
        internal static void CreateArenaText(MenuCommand menuCommand)
        {
            PublishWireObject(menuCommand, "text");
        }

        [MenuItem("GameObject/ARENA/Thickline", false, 5)]
        internal static void CreateArenaThickline(MenuCommand menuCommand)
        {
            PublishWireObject(menuCommand, "thickline");
        }

        // Skip "videosphere" since we don't support the same Jitsi client in Unity yet.
        // [MenuItem("GameObject/ARENA/Videosphere", false, 5)]

        // primitives

        [MenuItem("GameObject/ARENA/Box", false, 10)]
        internal static void CreateArenaBox(MenuCommand menuCommand)
        {
            PublishWireObject(menuCommand, "box", MatColor);
        }

        [MenuItem("GameObject/ARENA/Capsule", false, 10)]
        internal static void CreateArenaCapsule(MenuCommand menuCommand)
        {
            PublishWireObject(menuCommand, "capsule", MatColor);
        }

        [MenuItem("GameObject/ARENA/Circle", false, 10)]
        internal static void CreateArenaCircle(MenuCommand menuCommand)
        {
            PublishWireObject(menuCommand, "circle", MatColor);
        }

        [MenuItem("GameObject/ARENA/Cone", false, 10)]
        internal static void CreateArenaCone(MenuCommand menuCommand)
        {
            PublishWireObject(menuCommand, "cone", MatColor);
        }

        [MenuItem("GameObject/ARENA/Cylinder", false, 10)]
        internal static void CreateArenaCylinder(MenuCommand menuCommand)
        {
            PublishWireObject(menuCommand, "cylinder", MatColor);
        }

        [MenuItem("GameObject/ARENA/Dodecahedron", false, 10)]
        internal static void CreateArenaDodecahedron(MenuCommand menuCommand)
        {
            PublishWireObject(menuCommand, "dodecahedron", MatColor);
        }

        [MenuItem("GameObject/ARENA/Icosahedron", false, 10)]
        internal static void CreateArenaIcosahedron(MenuCommand menuCommand)
        {
            PublishWireObject(menuCommand, "icosahedron", MatColor);
        }

        [MenuItem("GameObject/ARENA/Octahedron", false, 10)]
        internal static void CreateArenaOctahedron(MenuCommand menuCommand)
        {
            PublishWireObject(menuCommand, "octahedron", MatColor);
        }

        [MenuItem("GameObject/ARENA/Plane", false, 10)]
        internal static void CreateArenaPlane(MenuCommand menuCommand)
        {
            PublishWireObject(menuCommand, "plane", MatColor);
        }

        [MenuItem("GameObject/ARENA/Ring", false, 10)]
        internal static void CreateArenaRing(MenuCommand menuCommand)
        {
            PublishWireObject(menuCommand, "ring", MatColor);
        }

        [MenuItem("GameObject/ARENA/Sphere", false, 10)]
        internal static void CreateArenaSphere(MenuCommand menuCommand)
        {
            PublishWireObject(menuCommand, "sphere", MatColor);
        }

        [MenuItem("GameObject/ARENA/Tetrahedron", false, 10)]
        internal static void CreateArenaTetrahedron(MenuCommand menuCommand)
        {
            PublishWireObject(menuCommand, "tetrahedron", MatColor);
        }

        [MenuItem("GameObject/ARENA/Torus", false, 10)]
        internal static void CreateArenaTorus(MenuCommand menuCommand)
        {
            PublishWireObject(menuCommand, "torus", MatColor);
        }

        [MenuItem("GameObject/ARENA/TorusKnot", false, 10)]
        internal static void CreateArenaTorusKnot(MenuCommand menuCommand)
        {
            PublishWireObject(menuCommand, "torusKnot", MatColor);
        }

        [MenuItem("GameObject/ARENA/Triangle", false, 10)]
        internal static void CreateArenaTriangle(MenuCommand menuCommand)
        {
            PublishWireObject(menuCommand, "triangle", MatColor);
        }

        [MenuItem("GameObject/ARENA/Entity", true)]
        [MenuItem("GameObject/ARENA/GLTF Model", true)]
        [MenuItem("GameObject/ARENA/Image", true)]
        [MenuItem("GameObject/ARENA/Light", true)]
        [MenuItem("GameObject/ARENA/Text", true)]
        [MenuItem("GameObject/ARENA/Thickline", true)]
        [MenuItem("GameObject/ARENA/Box", true)]
        [MenuItem("GameObject/ARENA/Capsule", true)]
        [MenuItem("GameObject/ARENA/Circle", true)]
        [MenuItem("GameObject/ARENA/Cone", true)]
        [MenuItem("GameObject/ARENA/Cylinder", true)]
        [MenuItem("GameObject/ARENA/Dodecahedron", true)]
        [MenuItem("GameObject/ARENA/Icosahedron", true)]
        [MenuItem("GameObject/ARENA/Octahedron", true)]
        [MenuItem("GameObject/ARENA/Plane", true)]
        [MenuItem("GameObject/ARENA/Ring", true)]
        [MenuItem("GameObject/ARENA/Sphere", true)]
        [MenuItem("GameObject/ARENA/Tetrahedron", true)]
        [MenuItem("GameObject/ARENA/Torus", true)]
        [MenuItem("GameObject/ARENA/TorusKnot", true)]
        [MenuItem("GameObject/ARENA/Triangle", true)]
        static bool ValidateCreateArenaObject()
        {
            return ArenaClientScene.Instance != null && ArenaClientScene.Instance.mqttClientConnected && ArenaClientScene.Instance.sceneObjectRights;
        }

        private static void PublishWireObject(MenuCommand menuCommand, string object_type, string matColor = null)
        {
            TextInfo ti = CultureInfo.CurrentCulture.TextInfo;
            string object_id = ti.ToTitleCase(object_type);

            // Set a position in front of the camera
            float distance = 2f;
            Camera cam = Camera.current ?? Camera.main;
            Vector3 cameraPoint = cam.transform.position + cam.transform.forward * distance;

            var client = ArenaClientScene.Instance;
            if (client.arenaObjs.ContainsKey(object_id))
                object_id = $"{object_id}-{UnityEngine.Random.Range(0, 1000000)}";
            ArenaObjectJson msg = new ArenaObjectJson
            {
                object_id = object_id,
                action = "create",
                type = "object",
                persist = true,
            };
            ArenaObjectDataJson data = new ArenaObjectDataJson
            {
                object_type = object_type,
                position = ArenaUnity.ToArenaPosition(cameraPoint),
            };
            if (matColor != null)
                data.material = new ArenaMaterialJson
                {
                    Color = matColor,
                };
            msg.data = data;
            string payload = JsonConvert.SerializeObject(msg);
            client.PublishObject(msg.object_id, payload, client.sceneObjectRights);
        }
#endif
    }
}
