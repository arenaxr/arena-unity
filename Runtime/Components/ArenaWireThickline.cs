/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2023, Carnegie Mellon University. All rights reserved.
 */

using System;
using System.Collections.Generic;
using ArenaUnity.Components;
using ArenaUnity.Schemas;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace ArenaUnity
{
    public class ArenaWireThickline : ArenaComponent
    {
        // ARENA thickline component unity conversion status:
        // DONE: color
        // DONE: lineWidth
        // DONE: lineWidthStyler
        // DONE: path

        public ArenaThicklineJson json = new ArenaThicklineJson();

        protected override void ApplyRender()
        {
            LineRenderer line = gameObject.GetComponent<LineRenderer>();
            if (line == null)
                line = gameObject.AddComponent<LineRenderer>();

            float pixelWidth = 1f; // default
            line.useWorldSpace = false; // match arena thickline which always uses position
            if (json.Path != null)
            {
                string[] nodes = json.Path.Split(new char[] { ',' });
                line.positionCount = nodes.Length;
                for (var i = 0; i < nodes.Length; i++)
                {
                    Vector3 position = ArenaUnity.ToUnityPositionString(nodes[i]);
                    line.SetPosition(i, position);
                }
            }
            pixelWidth = json.LineWidth;
            if (line.sharedMaterial == null || line.sharedMaterial.name.Contains("Default-"))
                line.material = new Material(ArenaUnity.GetUnlitShader());
            if (json.Color != null)
            {
                Color color = ArenaUnity.ToUnityColor(json.Color);
                line.startColor = line.endColor = color;
                line.material.SetColor(ArenaUnity.ColorPropertyName, color);
            }

            // convert arena thickline pixels vs unity meters
            float numkeys = 2;
            float w = 1;
            AnimationCurve curve = new AnimationCurve();
            switch (json.LineWidthStyler)
            {
                case ArenaThicklineJson.LineWidthStylerType.CenterSharp: numkeys = 3; break;
                case ArenaThicklineJson.LineWidthStylerType.CenterSmooth: numkeys = 10; break;
                case ArenaThicklineJson.LineWidthStylerType.SineWave: numkeys = 10; break;
            }
            for (int i = 0; i < numkeys; i++)
            {
                float p = i / (numkeys - 1);
                switch (json.LineWidthStyler)
                {
                    case ArenaThicklineJson.LineWidthStylerType.Grow:
                        w = p;
                        break;
                    case ArenaThicklineJson.LineWidthStylerType.Shrink:
                        w = 1 - p;
                        break;
                    case ArenaThicklineJson.LineWidthStylerType.CenterSharp:
                        w = 1 - Math.Abs(2 * p - 1);
                        break;
                    case ArenaThicklineJson.LineWidthStylerType.CenterSmooth:
                        w = (float)Math.Sin(p * 3.1415);
                        break;
                    case ArenaThicklineJson.LineWidthStylerType.SineWave:
                        w = (float)(0.5 + 0.5 * Math.Sin((p - 0.5) * 2 * 3.1415 * 10));
                        break;
                }
                curve.AddKey(p, w);
            }
            line.widthCurve = curve;
            line.widthMultiplier = pixelWidth * ArenaUnity.LineSinglePixelInMeters;
        }

        // thickline
        public static JObject ToArenaThickline(GameObject gobj)
        {
            // TODO: support Material opacity/visibility
            var data = new ArenaThicklineJson();
            LineRenderer line = gobj.GetComponent<LineRenderer>();
            string[] positions = new string[line.positionCount];
            Vector3[] vertices = new Vector3[line.positionCount];
            line.GetPositions(vertices);
            if (line.useWorldSpace)
            {   // line.useWorldSpace does not match arena thickline which always uses position
                for (var i = 0; i < line.positionCount; i++)
                {
                    // arena ignores line position transform, so translate every position?...
                    positions[i] = ArenaUnity.ToArenaPositionString(vertices[i] - gobj.transform.localPosition);
                }
                // TODO: does arena ignores thickline rotation?
            }
            else
            {   // !line.useWorldSpace matches arena thickline which always uses position
                for (var i = 0; i < line.positionCount; i++)
                {
                    positions[i] = ArenaUnity.ToArenaPositionString(vertices[i]);
                }
            }
            data.Path = string.Join(",", positions);
            data.LineWidth = line.startWidth / ArenaUnity.LineSinglePixelInMeters; // TODO: support endWidth
            data.Color = ArenaUnity.ToArenaColor(line.startColor); // TODO: support endColor

            return data != null ? JObject.FromObject(data) : null;
        }

        public override void UpdateObject()
        {
            PublishIfChanged(JsonConvert.SerializeObject(json));
        }
    }
}
