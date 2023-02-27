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
        private Camera _mainCamera;
        private Renderer _renderer;

        private Ray _ray;
        private RaycastHit _hit;

        private bool meshAvailable = false;

        private void Start()
        {
            _mainCamera = Camera.main;
            _renderer = GetComponent<Renderer>();
        }


        private void Update()
        {
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
            //MyMouseDown();
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

        //{"object_id":"box","action":"clientEvent","type":"mousedown","data":{"clickPos":{"x":-2.87,"y":1.6,"z":6.225},"position":{"x":-0.195,"y":0.305,"z":1.913},"source":"camera_2418540601_mwfarb"},"timestamp":"2023-02-15T18:59:02.413Z"}
        internal void ExternalMouseDown(dynamic data)
        {
            Debug.Log($"Remote Click {name} (OnMouseDown)!");
            MyMouseDown(data);
        }
        internal void ExternalMouseUp(dynamic data)
        {
            Debug.Log($"Remote Click {name} (OnMouseUp)!");
        }
        internal void ExternalMouseEnter(dynamic data)
        {
            Debug.Log($"Remote Click {name} (OnMouseEnter)!");
        }
        internal void ExternalMouseExit(dynamic data)
        {
            Debug.Log($"Remote Click {name} (OnMouseExit)!");
        }

        private void MyMouseDown(dynamic data)
        {
            //color
            if (_renderer)
            {
                _renderer.material.color =
                    _renderer.material.color == Color.red ? Color.blue : Color.red;
            }
            var aobj = GetComponent<ArenaObject>();
            if (aobj != null) aobj.PublishCreateUpdate();

            //laser
            string line_id = $"line-{UnityEngine.Random.Range(0, 100000000)}";
            dynamic msg = new ExpandoObject();
            msg.object_id = line_id;
            msg.action = "update";
            msg.type = "object";
            msg.ttl = 1;
            msg.data = new ExpandoObject();
            msg.data.object_type = "thickline";
            string start = ArenaUnity.ToArenaPositionString(new Vector3(
                (float)data.clickPos.x,
                (float)data.clickPos.y,
                (float)data.clickPos.z
            ));
            string end = ArenaUnity.ToArenaPositionString(new Vector3(
                (float)data.position.x,
                (float)data.position.y - .1f,
                (float)data.position.z
            ));
            msg.data.path = $"{start},{end}";
            msg.data.color = ArenaUnity.ToArenaColor(new Color32(255, 0, 0, 255));
            msg.data.lineWidth = 5;

            string payload = JsonConvert.SerializeObject(msg);
            ArenaClientScene.Instance.PublishObject(msg.object_id, payload, aobj.HasPermissions); // remote
            ArenaClientScene.Instance.ProcessMessage(payload); // local

        }

        internal void PublishMouseEvent(string eventType)
        {
            Debug.Log($"Local Click {name} ({eventType})!");

            _ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
            Physics.Raycast(_ray, out _hit);

            Vector3 camPosition = _mainCamera.transform.localPosition;
            string camName = _mainCamera.name;

            dynamic data = new ExpandoObject();
            data.clickPos = ArenaUnity.ToArenaPosition(_hit.point);
            data.position = ArenaUnity.ToArenaPosition(camPosition);
            data.source = camName;
            string payload = JsonConvert.SerializeObject(data);

            ArenaClientScene.Instance.PublishEvent(name, eventType, payload); // remote
        }

    }
}
