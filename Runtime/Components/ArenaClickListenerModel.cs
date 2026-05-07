/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2023, Carnegie Mellon University. All rights reserved.
 */

using UnityEngine;

namespace ArenaUnity.Components
{
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    public class ArenaClickListenerModel : MonoBehaviour
    {
        private ArenaClickListener _arenaCL;

        private void Start()
        {
            _arenaCL = GetComponentInParent<ArenaClickListener>();
        }

        private void Update()
        {
        }

        private void ForwardEvent(string methodName)
        {
            if (_arenaCL == null || _arenaCL.gameObject == null) return;
            foreach (var comp in _arenaCL.gameObject.GetComponents<ArenaComponent>())
            {
                var method = comp.GetType().GetMethod(methodName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
                if (method != null)
                {
                    method.Invoke(comp, null);
                }
            }
        }

        internal void OnMouseDown()
        {
            ForwardEvent("OnMouseDown");
        }
        internal void OnMouseUp()
        {
            ForwardEvent("OnMouseUp");
        }
        internal void OnMouseEnter()
        {
            ForwardEvent("OnMouseEnter");
        }
        internal void OnMouseExit()
        {
            ForwardEvent("OnMouseExit");
        }
    }
}
