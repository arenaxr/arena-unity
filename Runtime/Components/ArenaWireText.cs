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
        // DONE: letterSpacing
        // DONE: opacity
        // TODO: shader
        // TODO: side
        // TODO: tabSize
        // DONE: text
        // TODO: transparent
        // DONE: value
        // DONE: whiteSpace
        // DONE: wrapCount
        // TODO: xOffset
        // TODO: zOffset

        public ArenaTextJson json = new ArenaTextJson();

        protected override void ApplyRender()
        {
            TextMeshPro tm = gameObject.GetComponent<TextMeshPro>();
            if (tm == null)
                tm = gameObject.AddComponent<TextMeshPro>();

            tm.enableAutoSizing = false;
            float wrapCount = json.WrapCount > 0 ? json.WrapCount : 40f;
            // Balance scale multiplier to compensate for TextMeshPro default 3D unit scale mappings
            float defaultWidth = 5f;
            tm.fontSize = (defaultWidth / wrapCount) * 20f;
            tm.overflowMode = TextOverflowModes.Overflow;

            if (json.Text != null)
                tm.text = json.Text; // data.text is deprecated, users get a console warning at json ingest
            else if (json.Value != null)
                tm.text = json.Value;
            if (json.Color != null)
                tm.color = ArenaUnity.ToUnityColor(json.Color);
            tm.alpha = json.Opacity;
            tm.characterSpacing = json.LetterSpacing;
            tm.enableWordWrapping = (json.WhiteSpace != ArenaTextJson.WhiteSpaceType.Nowrap);

            RectTransform rt = gameObject.GetComponent<RectTransform>();

            // Calculate exact visual height needed natively
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, defaultWidth);
            rt.ForceUpdateRectTransforms(); // Process wrap lines

            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, tm.preferredHeight);
            rt.ForceUpdateRectTransforms();

            // Adjust margins to prevent clipping while keeping bounds accurate
            tm.margin = Vector4.zero;
            string align = json.Align.ToString();
            string anchor = json.Anchor.ToString();
            string baseline = json.Baseline.ToString();

            // TextMeshPro alignments map to AFrame's text justification (align) and vertical positioning (baseline)
            switch ($"{baseline.ToLower()} {align.ToLower()}")
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
                case "center left":
                    tm.alignment = TextAlignmentOptions.MidlineLeft;
                    break;
                case "center center":
                    tm.alignment = TextAlignmentOptions.Midline;
                    break;
                case "center right":
                    tm.alignment = TextAlignmentOptions.MidlineRight;
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
                default:
                    tm.alignment = TextAlignmentOptions.MidlineLeft;
                    break;
            }

            // Adjust RectTransform pivot based on alignment to keep origin consistent
            switch (baseline.ToLower())
            {
                case "top":
                    rt.pivot = new Vector2(rt.pivot.x, 1f);
                    break;
                case "bottom":
                    rt.pivot = new Vector2(rt.pivot.x, 0f);
                    break;
                case "center":
                default:
                    rt.pivot = new Vector2(rt.pivot.x, 0.5f);
                    break;
            }

            float rtWidth = rt.rect.width;
            float textW = tm.preferredWidth;
            float pivotX = 0.5f;

            if (rtWidth > 0)
            {
                // Calculate where the text block geometrically starts within the RectTransform based on TMPro alignment
                float textNormStart = 0f;
                switch (align.ToLower())
                {
                    case "left":
                        textNormStart = 0f;
                        break;
                    case "center":
                        textNormStart = (rtWidth - textW) / 2f / rtWidth;
                        break;
                    case "right":
                        textNormStart = (rtWidth - textW) / rtWidth;
                        break;
                }

                float textNormW = textW / rtWidth;
                switch (anchor.ToLower())
                {
                    case "left":
                        pivotX = textNormStart;
                        break;
                    case "center":
                    case "align":
                    default:
                        pivotX = textNormStart + (textNormW / 2f);
                        break;
                    case "right":
                        pivotX = textNormStart + textNormW;
                        break;
                }
            }
            rt.pivot = new Vector2(pivotX, rt.pivot.y);
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
            data.WhiteSpace = tm.enableWordWrapping ? ArenaTextJson.WhiteSpaceType.Normal : ArenaTextJson.WhiteSpaceType.Nowrap;
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
                case TextAlignmentOptions.MidlineLeft:
                    data.Baseline = ArenaTextJson.BaselineType.Center;
                    data.Align = ArenaTextJson.AlignType.Left;
                    break;
                case TextAlignmentOptions.Midline:
                    data.Baseline = ArenaTextJson.BaselineType.Center;
                    data.Align = ArenaTextJson.AlignType.Center;
                    break;
                case TextAlignmentOptions.MidlineRight:
                    data.Baseline = ArenaTextJson.BaselineType.Center;
                    data.Align = ArenaTextJson.AlignType.Right;
                    break;
                case TextAlignmentOptions.BottomLeft:
                    data.Baseline = ArenaTextJson.BaselineType.Bottom;
                    data.Align = ArenaTextJson.AlignType.Left;
                    break;
                case TextAlignmentOptions.Bottom:
                    data.Baseline = ArenaTextJson.BaselineType.Bottom;
                    data.Align = ArenaTextJson.AlignType.Center;
                    break;
                case TextAlignmentOptions.BottomRight:
                    data.Baseline = ArenaTextJson.BaselineType.Bottom;
                    data.Align = ArenaTextJson.AlignType.Right;
                    break;
            }

            // Unity RectTransform Pivot maps mapping back to A-Frame anchor
            if (tm.rectTransform.pivot.x <= 0.1f)
                data.Anchor = ArenaTextJson.AnchorType.Left;
            else if (tm.rectTransform.pivot.x >= 0.9f)
                data.Anchor = ArenaTextJson.AnchorType.Right;
            else
                data.Anchor = ArenaTextJson.AnchorType.Center;
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
