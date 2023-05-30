/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2023, Carnegie Mellon University. All rights reserved.
 */

using UnityEngine;

namespace ArenaUnity
{
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
            end = Vector3.forward * 100f;  // use back vector to simulate gltf rotation matrix transform
            color = Color.white;

            GameObject rayObj = new GameObject($"ray_{name}");
            LineRenderer line = rayObj.AddComponent<LineRenderer>();
            line.useWorldSpace = false;
            Vector3[] nodes = { start, end };
            line.SetPositions(nodes);
            line.startColor = line.endColor = color;
            line.widthMultiplier = 1f * ArenaUnity.LineSinglePixelInMeters;

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
