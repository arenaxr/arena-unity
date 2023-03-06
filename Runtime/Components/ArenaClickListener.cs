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

        public delegate void ClientEventMessageDelegate(string event_type, string msg);
        public ClientEventMessageDelegate OnEventCallback = null; // null, until user instantiates.

        private void Start()
        {
            _camera = Camera.main;
            _arenaCam = _camera.GetComponent<ArenaCamera>();
            if (_arenaCam == null) _arenaCam = _camera.gameObject.AddComponent<ArenaCamera>();
        }

        private void Update()
        {
            if (!meshAvailable)
            {
                MeshFilter mf = GetComponent<MeshFilter>();
                if (mf != null)
                {   // primitive geometry
                    MeshCollider mc = gameObject.AddComponent<MeshCollider>();
                    mc.sharedMesh = mf.mesh;
                    mc.convex = true; // simplify collision mesh when possible
                    meshAvailable = true;
                }
                else
                {   // gltf-model
                    // TODO: test for "this arena object only"
                    foreach (MeshRenderer mr in GetComponentsInChildren<MeshRenderer>())
                        AssignColliderMR(mr);
                    foreach (SkinnedMeshRenderer smr in GetComponentsInChildren<SkinnedMeshRenderer>())
                        AssignColliderSMR(smr);
                }
            }
        }

        private void AssignColliderMR(MeshRenderer mr)
        {
            MeshCollider mcChild = mr.gameObject.AddComponent<MeshCollider>();
            if (mcChild != null)
            {
                MeshFilter mf = mr.GetComponent<MeshFilter>();
                mcChild.sharedMesh = mf.sharedMesh;
                ArenaClickListenerModel aclm = mr.gameObject.AddComponent<ArenaClickListenerModel>();
                meshAvailable = true;
            }
        }

        private void AssignColliderSMR(SkinnedMeshRenderer smr)
        {
            MeshCollider mcChild = smr.gameObject.AddComponent<MeshCollider>();
            if (mcChild != null)
            {
                mcChild.sharedMesh = smr.sharedMesh;
                ArenaClickListenerModel aclm = smr.gameObject.AddComponent<ArenaClickListenerModel>();
                meshAvailable = true;
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
            RaycastHit hit;
            Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
            Physics.Raycast(ray, out hit);

            Vector3 camPosition = _camera.transform.localPosition;
            string camName = _arenaCam.camid;

            dynamic data = new ExpandoObject();
            data.clickPos = ArenaUnity.ToArenaPosition(camPosition);
            data.position = ArenaUnity.ToArenaPosition(hit.point);
            data.source = camName;
            string payload = JsonConvert.SerializeObject(data);

            ArenaClientScene.Instance.PublishEvent(name, eventType, data.source, payload);
        }
    }
}
