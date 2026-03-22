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
    public class ArenaLandmark : ArenaComponent
    {
        // ARENA landmark component unity conversion status:
        // TODO: randomRadiusMin
        // TODO: randomRadiusMax
        // TODO: offsetPosition
        // TODO: constrainToNavMesh
        // TODO: startingPosition
        // TODO: lookAtLandmark
        // TODO: label

        public ArenaLandmarkJson json = new ArenaLandmarkJson();

        protected override void ApplyRender()
        {
            // TODO: Implement this component if needed, or note our reasons for not rendering or controlling here.
        }

        public override void UpdateObject()
        {
            PublishIfChanged(json.attributeName, JsonConvert.SerializeObject(json));
        }
    }
}
