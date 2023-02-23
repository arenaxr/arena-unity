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

        internal void OnMouseDown(UnityEngine.UIElements.MouseDownEvent evt)
        {
            Debug.Log($"Local Click {name} (OnMouseDown)!");
            Vector3 rayPosition = _mainCamera.ScreenToWorldPoint(evt.mousePosition);
            Vector3 camPosition = _mainCamera.transform.localPosition;
            string camName = _mainCamera.name;
            PublishMouseEvent("mousedown", rayPosition, camPosition, camName);
            MyMouseDown();
        }
        internal void OnMouseUp()
        {
            Debug.Log($"Local Click {name} (OnMouseUp)!");
        }
        internal void OnMouseOver()
        {
            Debug.Log($"Local Click {name} (OnMouseOver)!");
        }
        internal void OnMouseExit()
        {
            Debug.Log($"Local Click {name} (OnMouseExit)!");
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
        internal void ExternalMouseOver(dynamic data)
        {
            Debug.Log($"Remote Click {name} (OnMouseOver)!");
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

        internal void PublishMouseEvent(string eventType, Vector3 rayPosition, Vector3 camPosition, string camName)
        {
            dynamic data = new ExpandoObject();
            data.clickPos = ArenaUnity.ToArenaPosition(rayPosition);
            data.position = ArenaUnity.ToArenaPosition(camPosition);
            data.source = camName;

            ArenaClientScene.Instance.PublishEvent(name, eventType, JsonConvert.DeserializeObject(data)); // remote
        }

    }
}