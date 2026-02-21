/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2023, Carnegie Mellon University. All rights reserved.
 */

using ArenaUnity.Components;
using ArenaUnity.Schemas;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;

namespace ArenaUnity
{
    public class ArenaWireText : ArenaComponent
    {
        // ARENA text component unity conversion status:
        // DONE: align
        // TODO: alphaTest
        // DONE: anchor
        // DONE: baseline
        // DONE: color
        // TODO: font
        // TODO: fontImage
        // DONE: height
        // DONE: letterSpacing
        // DONE: lineHeight
        // DONE: opacity
        // TODO: shader
        // TODO: side
        // TODO: tabSize
        // DONE: text
        // TODO: transparent
        // DONE: value
        // DONE: whiteSpace
        // DONE: width
        // DONE: wrapCount
        // DONE: wrapPixels
        // TODO: xOffset
        // TODO: zOffset

        public ArenaTextJson json = new ArenaTextJson();

        protected override void ApplyRender()
        {
            // TODO: Implement this component if needed, or note our reasons for not rendering or controlling here.

            TextMeshPro tm = gameObject.GetComponent<TextMeshPro>();
            if (tm == null)
                tm = gameObject.AddComponent<TextMeshPro>();

            tm.enableAutoSizing = false;
            float wrapCount = json.WrapCount > 0 ? json.WrapCount : 40f;
            // Balance scale multiplier to compensate for TextMeshPro default 3D unit scale mappings
            tm.fontSize = (json.Width / wrapCount) * 10f;
            tm.overflowMode = TextOverflowModes.Overflow;

            if (json.Text != null)
                tm.text = json.Text; // data.text is deprecated, users get a console warning at json ingest
            else if (json.Value != null)
                tm.text = json.Value;
            if (json.Color != null)
                tm.color = ArenaUnity.ToUnityColor(json.Color);
            tm.alpha = json.Opacity;
            tm.characterSpacing = json.LetterSpacing;
            if (json.LineHeight.HasValue)
                tm.lineSpacing = json.LineHeight.Value;
            tm.enableWordWrapping = (json.WhiteSpace != ArenaTextJson.WhiteSpaceType.Nowrap);

            RectTransform rt = gameObject.GetComponent<RectTransform>();
            // Scale the RectTransform itself to allow the font to be visible in 3D space
            rt.localScale = new Vector3(0.1f, 0.1f, 0.1f);

            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, json.Width * 10f);
            if (json.Height != null)
                rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, (float)json.Height * 10f);
            else
                rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 100f); // Default large bound to support wrapped height
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

            switch (json.Align)
            {
                case ArenaTextJson.AlignType.Left:
                    tm.horizontalAlignment = HorizontalAlignmentOptions.Left;
                    break;
                case ArenaTextJson.AlignType.Center:
                    tm.horizontalAlignment = HorizontalAlignmentOptions.Center;
                    break;
                case ArenaTextJson.AlignType.Right:
                    tm.horizontalAlignment = HorizontalAlignmentOptions.Right;
                    break;
            }
        }

        // text
        public static JObject ToArenaText(GameObject gobj)
        {
            var data = new ArenaTextJson();
            TextMeshPro tm = gobj.GetComponent<TextMeshPro>();

            data.Value = tm.text;
            data.Color = ArenaUnity.ToArenaColor(tm.color);
            data.Opacity = tm.alpha;
            data.LetterSpacing = tm.characterSpacing;
            data.LineHeight = tm.lineSpacing;
            data.WhiteSpace = tm.enableWordWrapping ? ArenaTextJson.WhiteSpaceType.Normal : ArenaTextJson.WhiteSpaceType.Nowrap;
            data.Width = tm.rectTransform.rect.width;
            data.Height = tm.rectTransform.rect.height;
            switch (tm.horizontalAlignment)
            {
                case HorizontalAlignmentOptions.Left:
                    data.Align = ArenaTextJson.AlignType.Left;
                    break;
                case HorizontalAlignmentOptions.Center:
                    data.Align = ArenaTextJson.AlignType.Center;
                    break;
                case HorizontalAlignmentOptions.Right:
                    data.Align = ArenaTextJson.AlignType.Right;
                    break;
            }
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
            return data != null ? JObject.FromObject(data) : null;
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
