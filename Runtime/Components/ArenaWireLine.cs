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
                            ArenaUnity.ToUnityPosition((ArenaVector3Json)json.Start),
                            ArenaUnity.ToUnityPosition((ArenaVector3Json)json.End),
                        };
                line.SetPositions(nodes);
            }
            if (line.material == null) // TODO (mwfarb): find "Default-Line" material
                line.material = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply"));
            if (json.Color != null)
                line.startColor = line.endColor = ArenaUnity.ToUnityColor((string)json.Color);
            line.widthMultiplier = pixelWidth * ArenaUnity.LineSinglePixelInMeters;
        }

        // ToArenaLine not needed since all LineRenderer in Unity are essentially thickline, not line.
        // We could try and support it if the Unity Ray is easily renderable on all platforms.
        // public static void ToArenaLine(GameObject gobj, ref ArenaLineJson data) { }

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
