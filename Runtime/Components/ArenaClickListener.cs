/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2023, Carnegie Mellon University. All rights reserved.
 */

using ArenaUnity.Schemas;
using Newtonsoft.Json;
using UnityEngine;

namespace ArenaUnity.Components
{
    [HelpURL("https://docs.arenaxr.org/content/schemas/arena-aframe-components.html#click-listener")]
    public class ArenaClickListener : ArenaComponent
    {
        // ARENA click-listener component unity conversion status:
        // TODO: enabled
        // TODO: bubble

        private Camera _camera;
        private ArenaCamera _arenaCam;

        private bool meshAvailable = false;

        public delegate void ClientEventMessageDelegate(string event_type, string msg);
        public ClientEventMessageDelegate OnEventCallback = null; // null, until user instantiates.

        public ArenaClickListenerJson json = new ArenaClickListenerJson();

        protected override void Update()
        {
            base.Update();

            // discover which camera to use for collisions
            _camera = Camera.main;
            if (_camera != null && _arenaCam == null)
            {
                // if user has chosen to add ArenaCamera, only then publish clientEvent events,
                // do not auto-add ArenaCamera component
                _arenaCam = _camera.GetComponent<ArenaCamera>();
            }

            // update colliders
            if (!meshAvailable)
            {
                if (GetComponent<Collider>() != null)
                {
                    meshAvailable = true;
                }
                else
                {
                    MeshFilter mf = GetComponent<MeshFilter>();
                    if (mf != null)
                    {   // primitive geometry
                        if (mf.sharedMesh != null)
                        {
                            MeshCollider mc = gameObject.AddComponent<MeshCollider>();
                            mc.sharedMesh = mf.sharedMesh;
                            if (mf.sharedMesh.triangles.Length / 3 <= 256)
                            {
                                mc.convex = true; // simplify collision mesh when possible
                            }
                            meshAvailable = true;
                        }
                    }
                    else
                    {   // gltf-model
                        bool foundMeshes = false;
                        foreach (MeshRenderer mr in GetComponentsInChildren<MeshRenderer>())
                        {
                            AssignColliderMesh(mr);
                            foundMeshes = true;
                        }
                        foreach (SkinnedMeshRenderer smr in GetComponentsInChildren<SkinnedMeshRenderer>())
                        {
                            AssignColliderSkinnedMesh(smr);
                            foundMeshes = true;
                        }
                        if (foundMeshes)
                        {
                            meshAvailable = true;
                        }
                    }
                }
            }
        }

        private void AssignColliderMesh(MeshRenderer mr)
        {
            MeshCollider mcChild = mr.gameObject.AddComponent<MeshCollider>();
            if (mcChild != null)
            {
                MeshFilter mf = mr.GetComponent<MeshFilter>();
                mcChild.sharedMesh = mf.sharedMesh;
                ArenaClickListenerModel aclm = mr.gameObject.AddComponent<ArenaClickListenerModel>();
            }
        }

        private void AssignColliderSkinnedMesh(SkinnedMeshRenderer smr)
        {
            MeshCollider mcChild = smr.gameObject.AddComponent<MeshCollider>();
            if (mcChild != null)
            {
                mcChild.sharedMesh = smr.sharedMesh;
                ArenaClickListenerModel aclm = smr.gameObject.AddComponent<ArenaClickListenerModel>();
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
            // if user has chosen to add ArenaCamera, only then publish clientEvent events
            if (_camera == null || _arenaCam == null) return;

            RaycastHit hit;
            Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
            Physics.Raycast(ray, out hit);

            Vector3 camPosition = _camera.transform.localPosition;
            string camName = _arenaCam.camid;

            ArenaEventJson data = new ArenaEventJson
            {
                OriginPosition = ArenaUnity.ToArenaPosition(camPosition),
                TargetPosition = ArenaUnity.ToArenaPosition(hit.point),
                Target = name,
            };
            string clientEventData = JsonConvert.SerializeObject(data);
            if (ArenaClientScene.Instance)
                ArenaClientScene.Instance.PublishEvent(eventType, camName, clientEventData);
        }

        protected override void ApplyRender()
        {
            // no Unity conversion yet, warn only when the scene asks for non-default behavior
            if (!json.Enabled || !json.Bubble)
                Debug.LogWarning($"click-listener 'enabled' and 'bubble' are not implemented in Unity, ignored for '{name}'.");
        }

        public override void UpdateObject()
        {
            PublishIfChanged(json.attributeName, JsonConvert.SerializeObject(json));
        }
    }
}
