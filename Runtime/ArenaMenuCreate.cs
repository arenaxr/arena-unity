using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using Google.Apis.Auth.OAuth2;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace ArenaUnity
{
    public static class ArenaMenuCreate
    {
        [MenuItem("ARENA/Signout")]
        internal static void SceneSignout()
        {
            EditorApplication.ExitPlaymode();
            if (Directory.Exists(GoogleWebAuthorizationBroker.Folder))
                Directory.Delete(GoogleWebAuthorizationBroker.Folder, true);
            Debug.Log("Logged out of the ARENA");
        }

        // Add a menu item to create custom GameObjects.
        // Priority 1 ensures it is grouped with the other menu items of the same kind
        // and propagated to the hierarchy dropdown and hierarchy context menus.

        [MenuItem("GameObject/ARENA/Box", false, 10)]
        internal static void CreateArenaBox(MenuCommand menuCommand)
        {
            PublishPrimitive(menuCommand, "box");
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

        [MenuItem("GameObject/ARENA/Torus", false, 10)]
        internal static void CreateArenaTorus(MenuCommand menuCommand)
        {
            PublishPrimitive(menuCommand, "torus");
        }

        private static void PublishPrimitive(MenuCommand menuCommand, string object_type)
        {
            TextInfo ti = CultureInfo.CurrentCulture.TextInfo;
            string object_id = ti.ToTitleCase(object_type);

            if (ArenaClient.Instance == null)
            {
                Debug.LogError($"Failed to create object '{object_id}', press Play before creating an ARENA {object_type}.");
                return;
            }

            // Set a position in front of the camera
            float distance = 2f;
            Camera cam = Camera.current ?? Camera.main;
            Vector3 cameraPoint = cam.transform.position + cam.transform.forward * distance;

            dynamic msg = new ExpandoObject();
            msg.object_id = Regex.Replace(object_id, ArenaUnity.regexArenaObjectId, "-");
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
            ArenaClient.Instance.Publish(object_id, payload); // remote
            ArenaClient.Instance.ProcessMessage(payload, menuCommand); // local
        }
    }

}
