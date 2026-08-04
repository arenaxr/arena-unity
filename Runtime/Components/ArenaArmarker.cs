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
    public class ArenaArmarker : ArenaComponent
    {
        /// <summary>Registry of active ARMarkers in the scene, keyed by markerid.</summary>
        public static readonly Dictionary<string, ArenaArmarker> ActiveMarkers = new Dictionary<string, ArenaArmarker>();

        // ARENA armarker component unity conversion status:
        // DONE: publish
        // TODO: buildable
        // DONE: dynamic
        // TODO: ele
        // TODO: lat
        // TODO: long
        // DONE: markerid
        // TODO: markertype
        // TODO: size
        // TODO: url

        public ArenaArmarkerJson json = new ArenaArmarkerJson();
        private string _lastMarkerId;

        protected override void ApplyRender()
        {
            // Update the registry when markerid changes
            if (_lastMarkerId != null && _lastMarkerId != json.Markerid)
            {
                if (ActiveMarkers.TryGetValue(_lastMarkerId, out var old) && old == this)
                    ActiveMarkers.Remove(_lastMarkerId);
            }
            
            if (!string.IsNullOrEmpty(json.Markerid))
            {
                ActiveMarkers[json.Markerid] = this;
                _lastMarkerId = json.Markerid;
            }
        }

        void OnDestroy()
        {
            if (_lastMarkerId != null && ActiveMarkers.TryGetValue(_lastMarkerId, out var marker) && marker == this)
            {
                ActiveMarkers.Remove(_lastMarkerId);
            }
        }

        public override void UpdateObject()
        {
            PublishIfChanged(json.attributeName, JsonConvert.SerializeObject(json));
        }

        /// <summary>
        /// Manually triggers an MQTT update of the GameObject's transform.
        /// Called by ArenaAprilTag when a dynamic armarker's position is updated by vision.
        /// </summary>
        public void PublishTransformUpdate()
        {
            var aobj = GetComponent<ArenaObject>();
            if (aobj != null)
            {
                aobj.PublishCreateUpdate(true);
            }
        }
    }
}
