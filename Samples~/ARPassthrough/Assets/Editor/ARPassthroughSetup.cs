#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.XR.Management;
using UnityEditor.XR.Management.Metadata;
using UnityEngine;

namespace ArenaUnity.Samples
{
    public class ARPassthroughSetup
    {
        [MenuItem("ARENA/Generate AR Passthrough Sample Scene")]
        public static void GenerateScene()
        {
            // 1. Create a new scene
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 2. Add Directional Light
            var light = new GameObject("Directional Light");
            var lightComp = light.AddComponent<Light>();
            lightComp.type = LightType.Directional;
            light.transform.rotation = Quaternion.Euler(50, -30, 0);

            // 3. Create AR Session & XR Origin using Unity's built-in XR menu items
            EditorApplication.ExecuteMenuItem("GameObject/XR/AR Session");
            EditorApplication.ExecuteMenuItem("GameObject/XR/XR Origin (Mobile AR)");

            // 4. Find the generated XR Origin
            var xrOrigin = GameObject.Find("XR Origin");
            if (xrOrigin == null) xrOrigin = GameObject.Find("XR Origin (Mobile AR)"); // Alternative name depending on ARF version
            if (xrOrigin == null)
            {
                Debug.LogError("Failed to create XR Origin. Ensure AR Foundation and an XR Plugin (ARKit/ARCore) are installed.");
                return;
            }

            // 5. Create ArenaClientScene GameObject
            var arenaScene = new GameObject("ArenaClientScene");
            arenaScene.AddComponent<ArenaClientScene>();

            // 6. Setup ArenaAprilTag for relocalization
            var aprilTag = xrOrigin.AddComponent<ArenaAprilTag>();
            aprilTag.sceneRoot = xrOrigin.transform; // Relocalize the XR rig
            aprilTag.originTagId = 0; // Default origin tag

            // 7. Create local transparent cube over the origin tag
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = "OriginTagVerificationCube";
            // 0.15m is the default AprilTag size. Y is half-height so it rests on the tag.
            cube.transform.localScale = new Vector3(0.15f, 0.15f, 0.15f);
            cube.transform.localPosition = new Vector3(0, 0.075f, 0);
            
            // Make the cube transparent
            var renderer = cube.GetComponent<Renderer>();
            if (renderer != null)
            {
                var mat = new Material(Shader.Find("Standard"));
                // Setup Standard shader for transparency
                mat.SetFloat("_Mode", 3);
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.EnableKeyword("_ALPHABLEND_ON");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.renderQueue = 3000;
                mat.color = new Color(0, 1, 0, 0.3f); // Semi-transparent green
                renderer.sharedMaterial = mat;
            }

            // 8. Auto-configure iOS Player Settings for AR
            PlayerSettings.iOS.cameraUsageDescription = "Required for Augmented Reality and AprilTag tracking.";

            // 9. Auto-configure XR Plug-in Management for iOS (ARKit) and Android (ARCore)
            var iosXrSettings = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(BuildTargetGroup.iOS);
            if (iosXrSettings != null && iosXrSettings.Manager != null)
            {
                XRPackageMetadataStore.AssignLoader(iosXrSettings.Manager, "Unity.XR.ARKit.ARKitLoader", BuildTargetGroup.iOS);
            }

            var androidXrSettings = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(BuildTargetGroup.Android);
            if (androidXrSettings != null && androidXrSettings.Manager != null)
            {
                XRPackageMetadataStore.AssignLoader(androidXrSettings.Manager, "Unity.XR.ARCore.ARCoreLoader", BuildTargetGroup.Android);
            }

            Debug.Log("AR Passthrough Sample Scene generated successfully! iOS Camera Usage and ARKit/ARCore XR loaders auto-configured.");
        }
    }
}
#endif
