/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2023, Carnegie Mellon University. All rights reserved.
 */

using System.Collections.Generic;
using System.Text.RegularExpressions;
using ArenaUnity.Schemas;
using Newtonsoft.Json;
using UnityEngine;

namespace ArenaUnity.Components
{
    public class ArenaGotoLandmark : ArenaComponent
    {
        // ARENA goto-landmark component unity conversion status:
        // DONE: on
        // DONE: landmark

        public ArenaGotoLandmarkJson json = new ArenaGotoLandmarkJson();

        protected override void ApplyRender()
        {
            Collider c = gameObject.GetComponent<Collider>();
            if (c == null)
            {
                c = gameObject.AddComponent<BoxCollider>();
            }
        }

        internal void OnMouseDown() { TriggerEvent(ArenaGotoLandmarkJson.OnType.Mousedown); }
        internal void OnMouseUp() { TriggerEvent(ArenaGotoLandmarkJson.OnType.Mouseup); }

        private void TriggerEvent(ArenaGotoLandmarkJson.OnType eventType)
        {
            if (json.On == eventType && !string.IsNullOrEmpty(json.Landmark))
            {
                GameObject landmarkObj = GameObject.Find(json.Landmark);
                if (landmarkObj != null && Camera.main != null)
                {
                    Transform rig = Camera.main.transform.parent;
                    if (rig != null)
                    {
                        rig.position = landmarkObj.transform.position;
                        rig.rotation = landmarkObj.transform.rotation;
                    }
                    else
                    {
                        Camera.main.transform.position = landmarkObj.transform.position;
                        Camera.main.transform.rotation = landmarkObj.transform.rotation;
                    }
                }
            }
        }

        public override void UpdateObject()
        {
            PublishIfChanged(json.attributeName, JsonConvert.SerializeObject(json));
        }
    }
}
