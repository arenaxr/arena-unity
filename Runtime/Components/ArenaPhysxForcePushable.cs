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
    public class ArenaPhysxForcePushable : ArenaComponent
    {
        // ARENA physx-force-pushable component unity conversion status:
        // DONE: force
        // DONE: on

        public ArenaPhysxForcePushableJson json = new ArenaPhysxForcePushableJson();

        protected override void ApplyRender()
        {
            if (!ArenaSceneOptions.PhysicsEnabled) return;

            SetSuppressTransform(true);
        }

        private void SetSuppressTransform(bool suppress)
        {
            var aobj = GetComponent<ArenaObject>();
            if (aobj != null)
                aobj.suppressTransformPublish = suppress;
        }

        private void OnDisable()
        {
            SetSuppressTransform(false);
        }

        private void OnDestroy()
        {
            SetSuppressTransform(false);
        }

        private void OnMouseDown()
        {
            if (!ArenaSceneOptions.PhysicsEnabled) return;

            if (json.On == ArenaPhysxForcePushableJson.OnType.Mousedown)
            {
                ApplyForce();
            }
        }

        private void OnMouseUp()
        {
            if (!ArenaSceneOptions.PhysicsEnabled) return;

            if (json.On == ArenaPhysxForcePushableJson.OnType.Mouseup)
            {
                ApplyForce();
            }
        }

        private void ApplyForce()
        {
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null && Camera.main != null)
            {
                Vector3 direction = (transform.position - Camera.main.transform.position).normalized;
                rb.AddForce(direction * json.Force, ForceMode.Impulse);
            }
        }

        public override void UpdateObject()
        {
            var newJson = JsonConvert.SerializeObject(json);
            if (updatedJson != newJson)
            {
                var aobj = GetComponent<ArenaObject>();
                if (aobj != null)
                {
                    aobj.PublishUpdate($"{{\"{json.attributeName}\":{newJson}}}");
                    apply = true;
                }
            }
            updatedJson = newJson;
        }
    }
}
