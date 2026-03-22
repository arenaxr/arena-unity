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
    public class ArenaPhysxMaterial : ArenaComponent
    {
        // ARENA physx-material component unity conversion status:
        // TODO: collidesWithLayers
        // TODO: collisionGroup
        // TODO: collisionLayers
        // TODO: contactOffset
        // DONE: density
        // DONE: dynamicFriction
        // TODO: restOffset
        // DONE: restitution
        // DONE: staticFriction

        public ArenaPhysxMaterialJson json = new ArenaPhysxMaterialJson();

        protected override void ApplyRender()
        {
            if (!ArenaSceneOptions.PhysicsEnabled) return;

            Collider[] colliders = gameObject.GetComponents<Collider>();
            foreach (Collider c in colliders)
            {
                if (c.sharedMaterial == null || c.sharedMaterial.name != "ArenaPhysxMaterial")
                {
#if UNITY_6000_0_OR_NEWER
                    c.sharedMaterial = new PhysicsMaterial("ArenaPhysxMaterial");
#else
                    c.sharedMaterial = new PhysicMaterial("ArenaPhysxMaterial");
#endif
                }
                c.sharedMaterial.staticFriction = json.StaticFriction;
                c.sharedMaterial.dynamicFriction = json.DynamicFriction;
                c.sharedMaterial.bounciness = json.Restitution;
            }

            if (json.Density.HasValue)
            {
                Rigidbody rb = gameObject.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.SetDensity(json.Density.Value);
                }
            }
        }

        public override void UpdateObject()
        {
            PublishIfChanged(json.attributeName, JsonConvert.SerializeObject(json));
        }
    }
}
