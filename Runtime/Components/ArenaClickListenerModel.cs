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
            _arenaCL = transform.parent.gameObject.GetComponent<ArenaClickListener>();
        }

        private void Update()
        {
        }

        internal void OnMouseDown()
        {
            _arenaCL.PublishMouseEvent("mousedown");
        }
        internal void OnMouseUp()
        {
            _arenaCL.PublishMouseEvent("mouseup");
        }
        internal void OnMouseEnter()
        {
            _arenaCL.PublishMouseEvent("mouseenter");
        }
        internal void OnMouseExit()
        {
            _arenaCL.PublishMouseEvent("mouseleave");
        }
    }
}
