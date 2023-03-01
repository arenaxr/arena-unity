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
            // TODO: too many mesh colliders?

            if (!meshAvailable)
            {
                MeshFilter mf = GetComponent<MeshFilter>();
                if (mf != null)
                {
                    // primitive geometry
                    MeshCollider mc = gameObject.AddComponent<MeshCollider>();
                    //mf.mesh.RecalculateBounds(); // TODO: necessary?
                    mc.sharedMesh = mf.mesh;
                    //mc.cookingOptions = MeshColliderCookingOptions.None; // TODO: necessary?
                    meshAvailable = true;
                }
                else
                {
                    SkinnedMeshRenderer smr = GetComponentInChildren<SkinnedMeshRenderer>();
                    if (smr != null)
                    {
                        // gltf-model
                        MeshCollider mc = smr.transform.parent.gameObject.AddComponent<MeshCollider>();
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
            RaycastHit _hit;
            Ray _ray = _camera.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(_ray, out _hit)) return;

            Debug.Log($"Local Click '{name}' ({eventType})!");

            Vector3 camPosition = _camera.transform.localPosition;
            string camName = _arenaCam.camid;

            dynamic data = new ExpandoObject();
            data.clickPos = ArenaUnity.ToArenaPosition(camPosition);
            data.position = ArenaUnity.ToArenaPosition(_hit.point);
            data.source = camName;
            string payload = JsonConvert.SerializeObject(data);

            ArenaClientScene.Instance.PublishEvent(name, eventType, data.source, payload);
        }

    }
}
