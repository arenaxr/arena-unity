/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2023, Carnegie Mellon University. All rights reserved.
 */

using ArenaUnity.Components;
using ArenaUnity.Schemas;
using Newtonsoft.Json;

namespace ArenaUnity
{
    public class ArenaWireUrdfModel : ArenaComponent
    {
        // ARENA urdf-model component unity conversion status:
        // TODO: url
        // TODO: urlBase
        // TODO: joints

        public ArenaUrdfModelJson json = new ArenaUrdfModelJson();

        protected override void ApplyRender()
        {
            // TODO: Implement this component if needed, or note our reasons for not rendering or controlling here.
        }

        public override void UpdateObject()
        {
            PublishIfChanged(JsonConvert.SerializeObject(json));
        }
    }
}
