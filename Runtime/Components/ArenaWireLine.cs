/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2023, Carnegie Mellon University. All rights reserved.
 */

using ArenaUnity.Components;
using ArenaUnity.Schemas;
using Newtonsoft.Json;
using UnityEngine;

namespace ArenaUnity
{
    public class ArenaWireLine : ArenaComponent
    {
        public ArenaLineJson json = new ArenaLineJson();

        protected override void ApplyRender()
        {
            // TODO: Implement this component if needed, or note our reasons for not rendering or controlling here.

            LineRenderer line = gameObject.GetComponent<LineRenderer>();
            if (line == null)
                line = gameObject.AddComponent<LineRenderer>();

            float pixelWidth = 1f; // default
            line.useWorldSpace = true; // match arena line which always ignores position
            if (json.Start != null && json.End != null)
            {
                Vector3[] nodes = {
                            ArenaUnity.ToUnityPosition((ArenaPositionJson)json.Start),
                            ArenaUnity.ToUnityPosition((ArenaPositionJson)json.End),
                        };
                line.SetPositions(nodes);
            }
            if (json.Color != null)
                line.startColor = line.endColor = ArenaUnity.ToUnityColor((string)json.Color);
            line.widthMultiplier = pixelWidth * ArenaUnity.LineSinglePixelInMeters;
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
