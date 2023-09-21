/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2023, Carnegie Mellon University. All rights reserved.
 */

using ArenaUnity.Components;
using ArenaUnity.Schemas;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;

namespace ArenaUnity
{
    public class ArenaWireText : ArenaComponent
    {
        public ArenaTextJson json = new ArenaTextJson();

        protected override void ApplyRender()
        {
            // TODO: Implement this component if needed, or note our reasons for not rendering or controlling here.

            TextMeshPro tm = gameObject.GetComponent<TextMeshPro>();
            if (tm == null)
                tm = gameObject.AddComponent<TextMeshPro>();
            tm.fontSize = 2;

            if (json.Value != null)
                tm.text = (string)json.Value;
            if (json.Color != null)
                tm.color = ArenaUnity.ToUnityColor((string)json.Color);

            RectTransform rt = gameObject.GetComponent<RectTransform>();
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, (float)json.Width);
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, (float)json.Height);
            rt.ForceUpdateRectTransforms();
            string anchor = json.Anchor.ToString();
            string baseline = json.Baseline.ToString();
            switch ($"{baseline} {anchor}")
            {
                case "top left":
                    tm.alignment = TextAlignmentOptions.TopLeft;
                    break;
                case "top center":
                    tm.alignment = TextAlignmentOptions.Top;
                    break;
                case "top right":
                    tm.alignment = TextAlignmentOptions.TopRight;
                    break;
                case "top align":
                    tm.alignment = TextAlignmentOptions.TopGeoAligned;
                    break;
                case "center left":
                    tm.alignment = TextAlignmentOptions.BaselineLeft;
                    break;
                case "center center":
                    tm.alignment = TextAlignmentOptions.Center;
                    break;
                case "center right":
                    tm.alignment = TextAlignmentOptions.BaselineRight;
                    break;
                case "center align":
                    tm.alignment = TextAlignmentOptions.CenterGeoAligned;
                    break;
                case "bottom left":
                    tm.alignment = TextAlignmentOptions.BottomLeft;
                    break;
                case "bottom center":
                    tm.alignment = TextAlignmentOptions.Bottom;
                    break;
                case "bottom right":
                    tm.alignment = TextAlignmentOptions.BottomRight;
                    break;
                case "bottom align":
                    tm.alignment = TextAlignmentOptions.BottomGeoAligned;
                    break;
            }
        }

        // text
        public static void ToArenaText(GameObject gobj, ref ArenaTextJson data)
        {
            TextMeshPro tm = gobj.GetComponent<TextMeshPro>();
            //tm.fontSize;
            data.Value = tm.text;
            data.Color = ArenaUnity.ToArenaColor(tm.color);
            data.Width = tm.rectTransform.rect.width;
            data.Height = tm.rectTransform.rect.height;
            switch (tm.alignment)
            {
                case TextAlignmentOptions.TopLeft:
                    data.Baseline = ArenaTextJson.BaselineType.Top;
                    data.Anchor = ArenaTextJson.AnchorType.Left;
                    break;
                case TextAlignmentOptions.Top:
                    data.Baseline = ArenaTextJson.BaselineType.Top;
                    data.Anchor = ArenaTextJson.AnchorType.Center;
                    break;
                case TextAlignmentOptions.TopRight:
                    data.Baseline = ArenaTextJson.BaselineType.Top;
                    data.Anchor = ArenaTextJson.AnchorType.Right;
                    break;
                case TextAlignmentOptions.TopGeoAligned:
                    data.Baseline = ArenaTextJson.BaselineType.Top;
                    data.Anchor = ArenaTextJson.AnchorType.Align;
                    break;
                case TextAlignmentOptions.BaselineLeft:
                    data.Baseline = ArenaTextJson.BaselineType.Center;
                    data.Anchor = ArenaTextJson.AnchorType.Left;
                    break;
                case TextAlignmentOptions.Center:
                    data.Baseline = ArenaTextJson.BaselineType.Center;
                    data.Anchor = ArenaTextJson.AnchorType.Center;
                    break;
                case TextAlignmentOptions.BaselineRight:
                    data.Baseline = ArenaTextJson.BaselineType.Center;
                    data.Anchor = ArenaTextJson.AnchorType.Right;
                    break;
                case TextAlignmentOptions.CenterGeoAligned:
                    data.Baseline = ArenaTextJson.BaselineType.Center;
                    data.Anchor = ArenaTextJson.AnchorType.Align;
                    break;
                case TextAlignmentOptions.BottomLeft:
                    data.Baseline = ArenaTextJson.BaselineType.Bottom;
                    data.Anchor = ArenaTextJson.AnchorType.Left;
                    break;
                case TextAlignmentOptions.Bottom:
                    data.Baseline = ArenaTextJson.BaselineType.Bottom;
                    data.Anchor = ArenaTextJson.AnchorType.Center;
                    break;
            }
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
