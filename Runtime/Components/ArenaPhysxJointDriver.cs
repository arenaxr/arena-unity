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
    public class ArenaPhysxJointDriver : ArenaComponent
    {
        // ARENA physx-joint-driver component unity conversion status:
        // TODO: angularVelocity
        // TODO: axes
        // TODO: damping
        // TODO: forceLimit
        // TODO: linearVelocity
        // TODO: lockOtherAxes
        // TODO: slerpRotation
        // TODO: stiffness
        // TODO: useAcceleration

        public ArenaPhysxJointDriverJson json = new ArenaPhysxJointDriverJson();

        protected override void ApplyRender()
        {
            // TODO: Implement this component if needed, or note our reasons for not rendering or controlling here.
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
