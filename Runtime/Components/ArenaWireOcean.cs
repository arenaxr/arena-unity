﻿/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2023, Carnegie Mellon University. All rights reserved.
 */

using ArenaUnity.Components;
using ArenaUnity.Schemas;
using Newtonsoft.Json;

namespace ArenaUnity
{
    public class ArenaWireOcean : ArenaComponent
    {
        // ARENA ocean component unity conversion status:
        // TODO: object_type
        // TODO: width
        // TODO: depth
        // TODO: density
        // TODO: amplitude
        // TODO: amplitudeVariance
        // TODO: speed
        // TODO: speedVariance
        // TODO: color
        // TODO: opacity

        public ArenaOceanJson json = new ArenaOceanJson();

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
                    aobj.PublishUpdate($"{newJson}");
                    apply = true;
                }
            }
            updatedJson = newJson;
        }
    }
}
