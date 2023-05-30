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
        private void Start()
        {
        }

        private void Update()
        {
            // draw a controller guiding line
            UnityEngine.Debug.DrawRay(transform.position, transform.forward, Color.white);
        }

    }
}
