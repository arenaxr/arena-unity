// Originally inspired from https://github.com/NCEEGEE/PrettyHierarchy

using UnityEditor;
using UnityEngine;
using ArenaUnity;

namespace ArenaUnity.Editor
{
    [InitializeOnLoad]
    public static class ArenaHierarchyColor
    {
        public static Color32 ColorDarkAllow = new Color32(0, 255, 0, 255); // green
        public static Color32 ColorLightAllow = new Color32(0, 128, 0, 255); // dark green

        public static Color32 ColorDarkDisallow = new Color32(255, 165, 0, 255); //orange
        public static Color32 ColorLightDisallow = new Color32(204, 85, 0, 255); // dark orange

        static ArenaHierarchyColor()
        {
            EditorApplication.hierarchyWindowItemOnGUI += HandleHierarchyWindowItemOnGUI;
        }

        public static Color32 GetTextColor(bool hasPermissions)
        {
            if (EditorGUIUtility.isProSkin)
                return hasPermissions ? ColorDarkAllow : ColorDarkDisallow;
            else
                return hasPermissions ? ColorLightAllow : ColorLightDisallow;
        }

        private static void HandleHierarchyWindowItemOnGUI(int instanceID, Rect selectionRect)
        {
            var instance = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
            if (instance != null)
            {
                var permissionsObj = instance.GetComponent<IArenaPermissions>();
                if (permissionsObj != null)
                {
                    bool isSelected = Selection.Contains(instanceID);
                    bool isHovered = selectionRect.Contains(Event.current.mousePosition);

                    if (!isSelected)
                    {
                        // Default background colors for hierarchy
                        Color bgColor;
                        if (isHovered)
                        {
                            bgColor = EditorGUIUtility.isProSkin ? new Color32(68, 68, 68, 255) : new Color32(170, 170, 170, 255);
                        }
                        else
                        {
                            bgColor = EditorGUIUtility.isProSkin ? new Color32(56, 56, 56, 255) : new Color32(200, 200, 200, 255);
                        }

                        // Text color based on permissions
                        Color textColor = GetTextColor(permissionsObj.HasPermissions);
                        textColor.a = instance.activeInHierarchy ? 1f : 0.5f;

                        // Draw background over existing Unity text
                        // x offset 16 is approximately where the text starts
                        Rect bgRect = new Rect(selectionRect.x + 16f, selectionRect.y, selectionRect.width - 16f, selectionRect.height);
                        EditorGUI.DrawRect(bgRect, bgColor);

                        // Draw colored text
                        Rect textRect = new Rect(selectionRect.x + 18f, selectionRect.y, selectionRect.width - 18f, selectionRect.height);
                        GUIStyle labelStyle = new GUIStyle(EditorStyles.label);
                        labelStyle.normal.textColor = textColor;
                        EditorGUI.LabelField(textRect, instance.name, labelStyle);
                    }
                }
            }
        }
    }
}
