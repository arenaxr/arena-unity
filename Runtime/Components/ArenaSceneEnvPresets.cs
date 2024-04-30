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
    public class ArenaSceneEnvPresets : ArenaComponent
    {
        // ARENA env-presets component unity conversion status:
        // TODO: active
        // TODO: dressing
        // TODO: dressingAmount
        // TODO: dressingColor
        // TODO: dressingOnPlayArea
        // TODO: dressingScale
        // TODO: dressingUniformScale
        // TODO: dressingVariance
        // TODO: flatShading
        // TODO: fog
        // TODO: grid
        // TODO: gridColor
        // TODO: ground
        // TODO: groundColor
        // TODO: groundColor2
        // TODO: groundScale
        // TODO: groundTexture
        // TODO: groundYScale
        // TODO: hideInAR
        // TODO: horizonColor
        // TODO: lighting
        // TODO: lightPosition
        // TODO: playArea
        // TODO: preset
        // TODO: seed
        // TODO: shadow
        // TODO: shadowSize
        // TODO: skyColor
        // TODO: skyType

        public ArenaEnvPresetsJson json = new ArenaEnvPresetsJson();

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
                    aobj.PublishUpdate($"{{\"{json.componentName}\":{newJson}}}");
                    apply = true;
                }
            }
            updatedJson = newJson;
        }
    }
}
