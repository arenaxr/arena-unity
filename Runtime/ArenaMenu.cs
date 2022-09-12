﻿/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021, The CONIX Research Center. All rights reserved.
 */

using System.Dynamic;
using System.Globalization;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace ArenaUnity
{
    public static class ArenaMenu
    {
#if UNITY_EDITOR
        // Add a menu item to create custom GameObjects.
        // Priority 1 ensures it is grouped with the other menu items of the same kind
        // and propagated to the hierarchy dropdown and hierarchy context menus.

        [MenuItem("GameObject/ARENA/Box", false, 10)]
        internal static void CreateArenaBox(MenuCommand menuCommand)
        {
            PublishPrimitive(menuCommand, "box");
        }

        [MenuItem("GameObject/ARENA/Capsule", false, 10)]
        internal static void CreateArenaCapsule(MenuCommand menuCommand)
        {
            PublishPrimitive(menuCommand, "capsule");
        }

        [MenuItem("GameObject/ARENA/Circle", false, 10)]
        internal static void CreateArenaCircle(MenuCommand menuCommand)
        {
            PublishPrimitive(menuCommand, "circle");
        }

        [MenuItem("GameObject/ARENA/Cone", false, 10)]
        internal static void CreateArenaCone(MenuCommand menuCommand)
        {
            PublishPrimitive(menuCommand, "cone");
        }

        [MenuItem("GameObject/ARENA/Cylinder", false, 10)]
        internal static void CreateArenaCylinder(MenuCommand menuCommand)
        {
            PublishPrimitive(menuCommand, "cylinder");
        }

        [MenuItem("GameObject/ARENA/Dodecahedron", false, 10)]
        internal static void CreateArenaDodecahedron(MenuCommand menuCommand)
        {
            PublishPrimitive(menuCommand, "dodecahedron");
        }

        [MenuItem("GameObject/ARENA/GLTF Model", false, 10)]
        internal static void CreateArenaGltfModel(MenuCommand menuCommand)
        {
            ArenaObjectAddUrlWindow window = (ArenaObjectAddUrlWindow)EditorWindow.GetWindow(typeof(ArenaObjectAddUrlWindow));
            window.Init("gltf-model", menuCommand);
            window.Show();
        }

        [MenuItem("GameObject/ARENA/Icosahedron", false, 10)]
        internal static void CreateArenaIcosahedron(MenuCommand menuCommand)
        {
            PublishPrimitive(menuCommand, "icosahedron");
        }

        [MenuItem("GameObject/ARENA/Image", false, 10)]
        internal static void CreateArenaImage(MenuCommand menuCommand)
        {
            ArenaObjectAddUrlWindow window = (ArenaObjectAddUrlWindow)EditorWindow.GetWindow(typeof(ArenaObjectAddUrlWindow));
            window.Init("image", menuCommand);
            window.Show();
        }

        [MenuItem("GameObject/ARENA/Octahedron", false, 10)]
        internal static void CreateArenaOctahedron(MenuCommand menuCommand)
        {
            PublishPrimitive(menuCommand, "octahedron");
        }

        [MenuItem("GameObject/ARENA/Plane", false, 10)]
        internal static void CreateArenaPlane(MenuCommand menuCommand)
        {
            PublishPrimitive(menuCommand, "plane");
        }

        [MenuItem("GameObject/ARENA/Ring", false, 10)]
        internal static void CreateArenaRing(MenuCommand menuCommand)
        {
            PublishPrimitive(menuCommand, "ring");
        }

        [MenuItem("GameObject/ARENA/Sphere", false, 10)]
        internal static void CreateArenaSphere(MenuCommand menuCommand)
        {
            PublishPrimitive(menuCommand, "sphere");
        }

        [MenuItem("GameObject/ARENA/Tetrahedron", false, 10)]
        internal static void CreateArenaTetrahedron(MenuCommand menuCommand)
        {
            PublishPrimitive(menuCommand, "tetrahedron");
        }

        [MenuItem("GameObject/ARENA/Torus", false, 10)]
        internal static void CreateArenaTorus(MenuCommand menuCommand)
        {
            PublishPrimitive(menuCommand, "torus");
        }

        [MenuItem("GameObject/ARENA/Box", true)]
        [MenuItem("GameObject/ARENA/Capsule", true)]
        [MenuItem("GameObject/ARENA/Circle", true)]
        [MenuItem("GameObject/ARENA/Cone", true)]
        [MenuItem("GameObject/ARENA/Cylinder", true)]
        [MenuItem("GameObject/ARENA/Dodecahedron", true)]
        [MenuItem("GameObject/ARENA/GLTF Model", true)]
        [MenuItem("GameObject/ARENA/Icosahedron", true)]
        [MenuItem("GameObject/ARENA/Image", true)]
        [MenuItem("GameObject/ARENA/Octahedron", true)]
        [MenuItem("GameObject/ARENA/Plane", true)]
        [MenuItem("GameObject/ARENA/Ring", true)]
        [MenuItem("GameObject/ARENA/Sphere", true)]
        [MenuItem("GameObject/ARENA/Tetrahedron", true)]
        [MenuItem("GameObject/ARENA/Torus", true)]
        static bool ValidateCreateArenaObject()
        {
            return ArenaClientScene.Instance != null && ArenaClientScene.Instance.mqttClientConnected;
        }

        private static void PublishPrimitive(MenuCommand menuCommand, string object_type)
        {
            TextInfo ti = CultureInfo.CurrentCulture.TextInfo;
            string object_id = ti.ToTitleCase(object_type);

            // Set a position in front of the camera
            float distance = 2f;
            Camera cam = Camera.current ?? Camera.main;
            Vector3 cameraPoint = cam.transform.position + cam.transform.forward * distance;

            dynamic msg = new ExpandoObject();
            if (ArenaClientScene.Instance.arenaObjs.ContainsKey(object_id))
                object_id = $"{object_id}-{UnityEngine.Random.Range(0, 1000000)}";
            msg.object_id = object_id;
            msg.action = "create";
            msg.type = "object";
            msg.persist = true;
            dynamic data = new ExpandoObject();
            data.object_type = object_type;
            data.position = ArenaUnity.ToArenaPosition(cameraPoint);
            dynamic material = new ExpandoObject();
            material.color = "#7f7f7f";

            data.material = material;
            msg.data = data;
            string payload = JsonConvert.SerializeObject(msg);
            ArenaClientScene.Instance.PublishObject(msg.object_id, payload); // remote
            ArenaClientScene.Instance.ProcessMessage(payload, menuCommand); // local
        }
#endif
    }
}
