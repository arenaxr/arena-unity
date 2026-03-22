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
    public class ArenaBoxCollisionListener : ArenaComponent
    {
        // ARENA box-collision-listener component unity conversion status:
        // DONE: enabled
        // TODO: dynamic

        public ArenaBoxCollisionListenerJson json = new ArenaBoxCollisionListenerJson();

        protected override void ApplyRender()
        {
            if (!ArenaSceneOptions.PhysicsEnabled) return;

            if (json.Enabled)
            {
                Collider c = gameObject.GetComponent<Collider>();
                if (c == null)
                {
                    c = gameObject.AddComponent<BoxCollider>();
                }
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!ArenaSceneOptions.PhysicsEnabled) return;

            if (json.Enabled)
            {
                PublishCollisionEvent("collision-start", collision);
            }
        }

        private void OnCollisionExit(Collision collision)
        {
            if (!ArenaSceneOptions.PhysicsEnabled) return;

            if (json.Enabled)
            {
                PublishCollisionEvent("collision-end", collision);
            }
        }

        private void PublishCollisionEvent(string eventType, Collision collision)
        {
            if (ArenaClientScene.Instance == null || Camera.main == null) return;

            ArenaEventJson data = new ArenaEventJson
            {
                OriginPosition = ArenaUnity.ToArenaPosition(transform.localPosition),
                TargetPosition = ArenaUnity.ToArenaPosition(collision.transform.localPosition),
                Target = collision.gameObject.name,
            };
            string clientEventData = JsonConvert.SerializeObject(data);
            ArenaCamera arenaCam = Camera.main.GetComponent<ArenaCamera>();
            string camName = arenaCam != null ? arenaCam.camid : "local-camera";
            ArenaClientScene.Instance.PublishEvent(eventType, camName, clientEventData);
        }

        public override void UpdateObject()
        {
            PublishIfChanged(json.attributeName, JsonConvert.SerializeObject(json));
        }
    }
}
