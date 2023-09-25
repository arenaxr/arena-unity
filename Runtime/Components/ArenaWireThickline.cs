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
        public ArenaThicklineJson json = new ArenaThicklineJson();

        protected override void ApplyRender()
        {
            // TODO: Implement this component if needed, or note our reasons for not rendering or controlling here.

            LineRenderer line = gameObject.GetComponent<LineRenderer>();
            if (line == null)
                line = gameObject.AddComponent<LineRenderer>();

            float pixelWidth = 1f; // default
            line.useWorldSpace = false; // match arena thickline which always uses position
            if (json.Path != null)
            {
                string[] nodes = ((string)json.Path).Split(new char[] { ',' });
                line.positionCount = nodes.Length;
                for (var i = 0; i < nodes.Length; i++)
                {
                    Vector3 position = ArenaUnity.ToUnityPositionString(nodes[i]);
                    line.SetPosition(i, position);
                }
            }
            pixelWidth = (float)json.LineWidth;
            if (line.material == null)
                line.material = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply"));
            if (json.Color != null)
                line.startColor = line.endColor = ArenaUnity.ToUnityColor((string)json.Color);

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
            data.LineWidth = (int)(line.startWidth / ArenaUnity.LineSinglePixelInMeters); // TODO: support endWidth
            data.Color = ArenaUnity.ToArenaColor(line.startColor); // TODO: support endColor

            return JObject.FromObject(data);
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
