// Modified from: https://github.com/NCEEGEE/PrettyHierarchy

using System;
using UnityEditor;
using UnityEngine;

namespace PrettyHierarchy
{
    [DisallowMultipleComponent]
    public class PrettyObject : MonoBehaviour
    {
        public static Color32 ColorDarkAllow = new Color32(0, 255, 0, 255); // green
        public static Color32 ColorLightAllow = new Color32(0, 128, 0, 255); // dark green

        public static Color32 ColorDarkDisallow = new Color32(255, 165, 0, 255); //orange
        public static Color32 ColorLightDisallow = new Color32(204, 85, 0, 255); // dark orange

        private bool hasPermissions;

        private void updateText()
        {
            // for now, arena objects colored text consistently in Editor/ThirdParty/PrettyHierarchy/Editor/Utils/EditorColors.cs
            textColor = GetTextColor();
#if UNITY_EDITOR

            EditorApplication.RepaintHierarchyWindow();
#endif
        }

        private Color32 GetTextColor()
        {
            if (EditorGUIUtility.isProSkin)
                return hasPermissions ? ColorDarkAllow : ColorDarkDisallow;
            else
                return hasPermissions ? ColorLightAllow : ColorLightDisallow;
        }

        public bool HasPermissions { get { return hasPermissions; } set { hasPermissions = value; updateText(); } }

#if UNITY_EDITOR
        //[Header("Background")]
        //[SerializeField]
        private bool useDefaultBackgroundColor = true;
        //[SerializeField]
        private Color32 backgroundColor = new Color32(255, 255, 255, 255);
        //[Header("Text")]
        //[SerializeField]
        private bool useDefaultTextColor = false;
        //[SerializeField]
        private Color32 textColor = new Color32(0, 0, 0, 255);
        //[SerializeField]
        private Font font;
        //[SerializeField]
        private int fontSize = 12;
        //[SerializeField]
        private FontStyle fontStyle = FontStyle.Normal;
        //[SerializeField]
        private TextAnchor alignment = TextAnchor.UpperLeft;
        //[SerializeField]
        private bool textDropShadow;

        public bool UseDefaultBackgroundColor { get { return useDefaultBackgroundColor; } }
        public Color32 BackgroundColor { get { return new Color32(backgroundColor.r, backgroundColor.g, backgroundColor.b, 255); } }

        public bool UseDefaultTextColor { get { return useDefaultTextColor; } }
        public Color32 TextColor { get { return GetTextColor(); } }
        public Font Font { get { return font; } }
        public int FontSize { get { return fontSize; } }
        public FontStyle FontStyle { get { return fontStyle; } }
        public TextAnchor Alignment { get { return alignment; } }
        public bool TextDropShadow { get { return textDropShadow; } }

        private void OnValidate()
        {
            EditorApplication.RepaintHierarchyWindow();
        }
#endif
    }
}
