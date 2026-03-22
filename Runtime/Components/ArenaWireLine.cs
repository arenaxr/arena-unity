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
        // ARENA line component unity conversion status:
        // DONE: color
        // DONE: end
        // DONE: opacity
        // DONE: start
        // DONE: visible

        public ArenaLineJson json = new ArenaLineJson();

        protected override void ApplyRender()
        {
            LineRenderer line = gameObject.GetComponent<LineRenderer>();
            if (line == null)
                line = gameObject.AddComponent<LineRenderer>();

            float pixelWidth = 1f; // default
            line.useWorldSpace = true; // match arena line which always ignores position
            if (json.Start != null && json.End != null)
            {
                Vector3[] nodes = {
                            ArenaUnity.ToUnityPosition(json.Start),
                            ArenaUnity.ToUnityPosition(json.End),
                        };
                line.SetPositions(nodes);
            }
            if (line.sharedMaterial == null || line.sharedMaterial.name.Contains("Default-"))
                line.material = new Material(ArenaUnity.GetUnlitShader());
            if (json.Color != null)
            {
                Color color = ArenaUnity.ToUnityColor(json.Color);
                color.a = json.Opacity;
                line.startColor = line.endColor = color;
                line.material.SetColor(ArenaUnity.ColorPropertyName, color);
            }
            line.widthMultiplier = pixelWidth * ArenaUnity.LineSinglePixelInMeters;

            line.enabled = json.Visible;
        }

        // ToArenaLine not needed since all LineRenderer in Unity are essentially thickline, not line.
        // We could try and support it if the Unity Ray is easily renderable on all platforms.
        // public static void ToArenaLine(GameObject gobj, ref ArenaLineJson data) { }

        public override void UpdateObject()
        {
            PublishIfChanged(JsonConvert.SerializeObject(json));
        }
    }
}
