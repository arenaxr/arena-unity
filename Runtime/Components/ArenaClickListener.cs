/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2023, Carnegie Mellon University. All rights reserved.
 */

using System.Dynamic;
using Newtonsoft.Json;
using UnityEngine;

namespace ArenaUnity.Components
{
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    [HelpURL("https://docs.arenaxr.org/content/schemas/arena-aframe-components.html#click-listener")]
    [RequireComponent(typeof(ArenaObject))]
    public class ArenaClickListener : MonoBehaviour
    {
        private Camera _camera;
        private ArenaCamera _arenaCam;
        private Ray _ray;
        private RaycastHit _hit;

        private bool meshAvailable = false;

        public delegate void ClientEventMessageDelegate(string event_type, dynamic data);
        public ClientEventMessageDelegate OnEventCallback = null; // null, until library user instantiates.

        private void Start()
        {
            _camera = Camera.main;
            _arenaCam = _camera.GetComponent<ArenaCamera>();
            if (_arenaCam == null) _arenaCam = _camera.gameObject.AddComponent<ArenaCamera>();
        }

        private void Update()
        {
            // TODO: too many mech colliders?

            if (!meshAvailable)
            {
                MeshCollider mc = GetComponent<MeshCollider>();
                if (mc == null) mc = gameObject.AddComponent<MeshCollider>();

                MeshFilter mf = GetComponent<MeshFilter>();
                if (mf != null)
                {
                    //mf.mesh.RecalculateBounds();
                    mc.sharedMesh = mf.mesh;
                    meshAvailable = true;
                }
                else
                {
                    SkinnedMeshRenderer smr = GetComponentInChildren<SkinnedMeshRenderer>();
                    if (smr != null)
                    {
                        //mc = smr.transform.parent.gameObject.AddComponent<MeshCollider>();
                        mc = gameObject.AddComponent<MeshCollider>();
                        if (mc != null)
                        {
                            mc.sharedMesh = smr.sharedMesh;
                            meshAvailable = true;
                        }
                    }
                }
            }
        }

        internal void OnMouseDown()
        {
            PublishMouseEvent("mousedown");
        }
        internal void OnMouseUp()
        {
            PublishMouseEvent("mouseup");
        }
        internal void OnMouseEnter()
        {
            PublishMouseEvent("mouseenter");
        }
        internal void OnMouseExit()
        {
            PublishMouseEvent("mouseleave");
        }

        internal void PublishMouseEvent(string eventType)
        {
            Debug.Log($"Local Click '{name}' ({eventType})!");

            _ray = _camera.ScreenPointToRay(Input.mousePosition);
            Physics.Raycast(_ray, out _hit);

            Vector3 camPosition = _camera.transform.localPosition;
            string camName = _arenaCam.camid;

            dynamic data = new ExpandoObject();
            data.clickPos = ArenaUnity.ToArenaPosition(_hit.point);
            data.position = ArenaUnity.ToArenaPosition(camPosition);
            data.source = camName;
            string payload = JsonConvert.SerializeObject(data);

            ArenaClientScene.Instance.PublishEvent(name, eventType, data.source, payload);
        }

    }
}
