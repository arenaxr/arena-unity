/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2023, Carnegie Mellon University. All rights reserved.
 */

using ArenaUnity.Components;
using ArenaUnity.Schemas;
using Newtonsoft.Json;

namespace ArenaUnity
{
    public class ArenaMeshVideosphere : ArenaMesh
    {
        // ARENA videosphere component unity conversion status:
        // TODO: autoplay
        // TODO: crossOrigin
        // TODO: loop
        // DONE: radius
        // DONE: segmentsHeight
        // DONE: segmentsWidth
        // TODO: src

        public ArenaVideosphereJson json = new ArenaVideosphereJson();

        protected override void ApplyRender()
        {
            filter.sharedMesh = SphereBuilder.Build(
                json.Radius,
                json.SegmentsHeight,
                json.SegmentsWidth
            );
        }

        public override void UpdateObject()
        {
            PublishIfChanged(JsonConvert.SerializeObject(json));
        }
    }
}
