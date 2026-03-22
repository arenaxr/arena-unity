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
    public class ArenaSceneRendererSettings: ArenaComponent
    {
        // ARENA renderer-settings component unity conversion status:
        // TODO: localClippingEnabled
        // TODO: outputColorSpace
        // TODO: physicallyCorrectLights
        // TODO: sortObjects

        public ArenaRendererSettingsJson json = new ArenaRendererSettingsJson();

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
