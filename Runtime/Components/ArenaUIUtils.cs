/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2026, Carnegie Mellon University. All rights reserved.
 */

using UnityEngine;
using UnityEngine.UI;

namespace ArenaUnity.Components
{
    public static class ArenaUIUtils
    {
        public static readonly Color LIGHT_BG = new Color(0.95f, 0.95f, 0.95f, 0.8f);
        public static readonly Color LIGHT_TEXT_BG = new Color(0f, 0f, 0f, 0.25f);
        public static readonly Color LIGHT_TEXT = new Color(0.23f, 0.23f, 0.23f, 1f);
        
        public static readonly Color DARK_BG = new Color(0.24f, 0.24f, 0.24f, 1f);
        public static readonly Color DARK_TEXT_BG = new Color(0.24f, 0.24f, 0.24f, 0.25f);
        public static readonly Color DARK_TEXT = new Color(0.94f, 0.94f, 0.94f, 1f);
        
        public static readonly Color CAPTION_BG = new Color(1f, 1f, 1f, 0.75f);
        public static readonly Color CAPTION_TEXT = new Color(0.2f, 0.2f, 0.2f, 1f);
        
        public static readonly Color LIGHT_BUTTON_BG = new Color(0.93f, 0.93f, 0.93f, 0.9f);
        public static readonly Color DARK_BUTTON_BG = new Color(0.07f, 0.07f, 0.07f, 0.8f);

        public static readonly Color LIGHT_BUTTON_HOVER = new Color(0.82f, 0.82f, 0.82f, 0.8f);
        public static readonly Color DARK_BUTTON_HOVER = new Color(0.21f, 0.21f, 0.21f, 0.8f);

        public const float PIXELS_PER_METER = 1000f;
    }

    public class ArenaUI_ButtonClickHandler : MonoBehaviour
    {
        public string ButtonName;
        public string TargetObjectId;
        public GameObject TargetObjectToDestroy;
        public bool DestroyOnSelect = false;
        
        public int ButtonIndex = -1;
        public Color DefaultColor;
        public Color HoverColor;
        public Image ButtonImage;

        private void OnMouseEnter()
        {
            if (ButtonImage != null) ButtonImage.color = HoverColor;
        }

        private void OnMouseExit()
        {
            if (ButtonImage != null) ButtonImage.color = DefaultColor;
        }

        private void OnMouseDown()
        {
            Debug.Log($"[ArenaUI] OnMouseDown triggered on {ButtonName}");
            if (string.IsNullOrEmpty(TargetObjectId) || ArenaClientScene.Instance == null)
            {
                Debug.LogWarning("[ArenaUI] TargetObjectId or ArenaClientScene is null");
                return;
            }

            var cam = Camera.main;
            if (cam == null)
            {
                Debug.LogWarning("[ArenaUI] Camera.main is null");
                return;
            }
            
            var arenaCam = cam.GetComponent<ArenaCamera>();
            if (arenaCam == null)
            {
                Debug.LogWarning("[ArenaUI] arenaCam is null");
                return;
            }

            var data = new Newtonsoft.Json.Linq.JObject
            {
                ["target"] = TargetObjectId,
                ["targetPosition"] = Newtonsoft.Json.Linq.JToken.FromObject(ArenaUnity.ToArenaPosition(transform.position)),
                ["originPosition"] = Newtonsoft.Json.Linq.JToken.FromObject(ArenaUnity.ToArenaPosition(cam.transform.position)),
                ["buttonName"] = ButtonName
            };
            if (ButtonIndex >= 0)
            {
                data["buttonIndex"] = ButtonIndex;
            }

            Debug.Log($"[ArenaUI] Publishing buttonClick: {data.ToString(Newtonsoft.Json.Formatting.None)}");
            ArenaClientScene.Instance.PublishEvent("buttonClick", arenaCam.camid, data.ToString(Newtonsoft.Json.Formatting.None));
            
            if (DestroyOnSelect && TargetObjectToDestroy != null)
            {
                // Bypass local delete prompt by flagging as external
                var arenaObj = TargetObjectToDestroy.GetComponent<ArenaObject>();
                if (arenaObj != null)
                {
                    arenaObj.externalDelete = true;
                }
                
                // Clean up dictionary reference to prevent missing reference exceptions
                ArenaClientScene.Instance.arenaObjs.Remove(TargetObjectToDestroy.name);
                
                Destroy(TargetObjectToDestroy);
            }
        }
    }
}
