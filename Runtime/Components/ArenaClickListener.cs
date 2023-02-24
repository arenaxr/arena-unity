/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2023, Carnegie Mellon University. All rights reserved.
 */

using System.Dynamic;
using Newtonsoft.Json;
using UnityEditor;
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

        private void Start()
        {
            _mainCamera = Camera.main;
            _renderer = GetComponent<Renderer>();
        }

        //private void Update()
        //{
        //    if (Input.GetMouseButtonDown(0))
        //    {
        //        //_ray = new Ray(
        //        //_mainCamera.ScreenToWorldPoint(Input.mousePosition),
        //        //_mainCamera.transform.forward);
        //        // or:
        //        _ray = _mainCamera.ScreenPointToRay(Input.mousePosition);

        //        if (Physics.Raycast(_ray, out _hit))
        //        {
        //            if (_hit.transform == transform)
        //            {
        //                Debug.Log($"Local Click {name} (Raycast MouseDown)!");
        //                MyMouseDown();
        //            }
        //        }
        //    }
        //}

        internal void OnMouseDown()
        {
            PublishMouseEvent("mousedown");
            MyMouseDown();
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
            MyMouseDown();
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

        private void MyMouseDown()
        {
            if (_renderer)
            {
                _renderer.material.color =
                    _renderer.material.color == Color.red ? Color.blue : Color.red;
            }

            var aobj = GetComponent<ArenaObject>();
            if (aobj != null) aobj.PublishCreateUpdate();
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
            Debug.Log(payload) ;

            ArenaClientScene.Instance.PublishEvent(name, eventType, payload); // remote
        }

    }
}
