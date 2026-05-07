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
    public class ArenaGotoUrl : ArenaComponent
    {
        // ARENA goto-url component unity conversion status:
        // DONE: dest
        // DONE: on
        // DONE: url

        public ArenaGotoUrlJson json = new ArenaGotoUrlJson();

        protected override void ApplyRender()
        {
            Collider c = gameObject.GetComponent<Collider>();
            if (c == null)
            {
                c = gameObject.AddComponent<BoxCollider>();
            }
        }

        internal void OnMouseDown() { TriggerEvent(ArenaGotoUrlJson.OnType.Mousedown); }
        internal void OnMouseUp() { TriggerEvent(ArenaGotoUrlJson.OnType.Mouseup); }

        private void TriggerEvent(ArenaGotoUrlJson.OnType eventType)
        {
            if (json.On == eventType && !string.IsNullOrEmpty(json.Url))
            {
                Application.OpenURL(json.Url);
            }
        }

        public override void UpdateObject()
        {
            PublishIfChanged(json.attributeName, JsonConvert.SerializeObject(json));
        }
    }
}
