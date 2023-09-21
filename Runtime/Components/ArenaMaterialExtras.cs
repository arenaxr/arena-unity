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
    public class ArenaMaterialExtras : ArenaComponent
    {
        // TODO: ARENA Property Handling Status

        // TODO: [Tooltip("Serializable JSON attributes for Arena .........")]
        public ArenaMaterialExtrasJson json = new ArenaMaterialExtrasJson();

        protected override void ApplyRender()
        {
            // TODO: Debug.LogWarning("{json.componentName} not yet handling render updates from Arena!!!!");
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
