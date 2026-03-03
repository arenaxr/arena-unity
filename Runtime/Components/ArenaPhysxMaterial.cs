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
                    c.sharedMaterial = new PhysicMaterial("ArenaPhysxMaterial");
                }
                c.sharedMaterial.staticFriction = json.StaticFriction;
                c.sharedMaterial.dynamicFriction = json.DynamicFriction;
                c.sharedMaterial.bounciness = json.Restitution;
                c.sharedMaterial.bounceCombine = PhysicMaterialCombine.Maximum;
                c.sharedMaterial.frictionCombine = PhysicMaterialCombine.Minimum;
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
            var newJson = JsonConvert.SerializeObject(json);
            if (updatedJson != newJson)
            {
                var aobj = GetComponent<ArenaObject>();
                if (aobj != null)
                {
                    aobj.PublishUpdate($"{{\"{json.componentName}\":{newJson}}}");
                    apply = true;
                }
            }
            updatedJson = newJson;
        }
    }
}
