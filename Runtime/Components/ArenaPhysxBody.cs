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
    public class ArenaPhysxBody : ArenaComponent
    {
        // ARENA physx-body component unity conversion status:
        // DONE: angularDamping
        // TODO: emitCollisionEvents
        // DONE: highPrecision
        // DONE: linearDamping
        // DONE: mass
        // TODO: shapeOffset
        // DONE: type

        public ArenaPhysxBodyJson json = new ArenaPhysxBodyJson();

        protected override void ApplyRender()
        {
            if (!ArenaSceneOptions.PhysicsEnabled) return;

            Rigidbody rb = gameObject.GetComponent<Rigidbody>();

            if (json.Type == ArenaPhysxBodyJson.TypeType.Static)
            {
                if (rb != null)
                {
                    Destroy(rb);
                }
                SetSuppressTransform(false);
            }
            else
            {
                if (rb == null)
                {
                    rb = gameObject.AddComponent<Rigidbody>();
                }

                rb.isKinematic = (json.Type == ArenaPhysxBodyJson.TypeType.Kinematic);
                rb.mass = json.Mass;
                rb.drag = json.LinearDamping;
                rb.angularDrag = json.AngularDamping;
                rb.collisionDetectionMode = json.HighPrecision ? CollisionDetectionMode.ContinuousDynamic : CollisionDetectionMode.Discrete;

                SetSuppressTransform(true);
            }
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
