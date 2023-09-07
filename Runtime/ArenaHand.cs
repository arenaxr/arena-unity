/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2023, Carnegie Mellon University. All rights reserved.
 */

using UnityEngine;

namespace ArenaUnity
{
    /// <summary>
    /// Temporary manager of drawing remote rays from hand controllers
    /// TODO (mwfarb): resolve oculus-touch controls publishing +43 x-axis rotation orientationOffset from arena-web
    /// TODO (mwfarb): set a proper arena-hand manager when we integrate with unity vr helmets
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ArenaObject))]
    public class ArenaHand : MonoBehaviour
    {
        Vector3 start;
        Vector3 end;
        Color color;

        private void Start()
        {
            start = Vector3.zero;
            end = Vector3.forward * 1000f;
            color = Color.white;

            GameObject rayObj = new GameObject($"ray_{name}");
            LineRenderer line = rayObj.AddComponent<LineRenderer>();
            line.useWorldSpace = false;
            Vector3[] nodes = { start, end };
            line.SetPositions(nodes);
            line.startColor = line.endColor = color;
            //line.material = new Material(Shader.Find("Standard"));
            line.material = new Material(Shader.Find("Unlit/Color"));
            //line.material = new Material(Shader.Find("Default-Line"));
            //line.material = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply"));
            line.widthMultiplier = 1f * ArenaUnity.LineSinglePixelInMeters;
            // we also need to apply the gltf rotation matrix transform, since the hand models did as well
            rayObj.transform.localRotation = ArenaUnity.GltfToUnityRotationQuat(rayObj.transform.localRotation);
            // makes the child keep its local orientation rather than its global orientation
            rayObj.transform.SetParent(transform, false);
        }

        private void Update()
        {
            // draw a controller guiding line, visible in SceneView with Gizmos on only
            //UnityEngine.Debug.DrawRay(transform.position, transform.forward, color);
            //UnityEngine.Debug.DrawLine(transform.position, transform.forward * 100f, color);
        }

    }
}
