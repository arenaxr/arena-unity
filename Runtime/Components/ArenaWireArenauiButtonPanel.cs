/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2023, Carnegie Mellon University. All rights reserved.
 */

using ArenaUnity.Components;
using ArenaUnity.Schemas;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ArenaUnity
{
    public class ArenaWireArenauiButtonPanel : ArenaComponent
    {
        // ARENA arenaui-button-panel component unity conversion status:
        // DONE: buttons
        // DONE: title
        // DONE: vertical
        // TODO: font
        // DONE: theme
        // DONE: materialSides

        public ArenaArenauiButtonPanelJson json = new ArenaArenauiButtonPanelJson();

        private Canvas _canvas;
        private RectTransform _canvasRect;
        private CanvasGroup _canvasGroup;
        
        private Image _bgImage;
        private RectTransform _rootContainer;
        private TextMeshProUGUI _titleText;
        private RectTransform _buttonsContainer;
        private LayoutGroup _buttonsLayoutGroup; // Either Horizontal or Vertical
        
        private List<GameObject> _buttonInstances = new List<GameObject>();

        protected override void Start()
        {
            base.Start();
            InitializeHierarchy();
        }

        private void InitializeHierarchy()
        {
            var canvasObj = new GameObject("ArenauiButtonPanelCanvas");
            canvasObj.transform.SetParent(gameObject.transform, false);
            canvasObj.transform.localPosition = Vector3.zero;
            canvasObj.transform.localRotation = Quaternion.identity;

            _canvas = canvasObj.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.WorldSpace;
            _canvas.worldCamera = Camera.main;
            
            _canvasGroup = canvasObj.AddComponent<CanvasGroup>();

            _canvasRect = canvasObj.GetComponent<RectTransform>();
            _canvasRect.localScale = new Vector3(1f / ArenaUIUtils.PIXELS_PER_METER, 1f / ArenaUIUtils.PIXELS_PER_METER, 1f / ArenaUIUtils.PIXELS_PER_METER);
            _canvasRect.pivot = new Vector2(0.5f, 0.5f);

            // Root Container
            var rootObj = new GameObject("RootContainer");
            _rootContainer = rootObj.AddComponent<RectTransform>();
            _rootContainer.SetParent(_canvasRect, false);
            _rootContainer.anchorMin = new Vector2(0.5f, 0.5f);
            _rootContainer.anchorMax = new Vector2(0.5f, 0.5f);
            _rootContainer.pivot = new Vector2(0.5f, 0.5f);
            
            _bgImage = rootObj.AddComponent<Image>();
            _bgImage.type = Image.Type.Sliced;
            
            var rootLayout = rootObj.AddComponent<VerticalLayoutGroup>();
            rootLayout.childAlignment = TextAnchor.MiddleCenter;
            rootLayout.childControlHeight = true;
            rootLayout.childControlWidth = true;
            rootLayout.childForceExpandHeight = false;
            rootLayout.childForceExpandWidth = true;
            rootLayout.padding = new RectOffset(25, 25, 50, 50); // Matches [containerPadding*2, containerPadding]
            rootLayout.spacing = 25f;

            var rootFitter = rootObj.AddComponent<ContentSizeFitter>();
            rootFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            rootFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Title Text
            var titleObj = new GameObject("TitleText");
            var titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.SetParent(_rootContainer, false);
            _titleText = titleObj.AddComponent<TextMeshProUGUI>();
            _titleText.alignment = TextAlignmentOptions.Center;
            _titleText.fontSize = 75f; // Matches ARENATypography.button
            _titleText.enableAutoSizing = false;
            _titleText.enableWordWrapping = false;

            // Buttons Container
            var buttonsObj = new GameObject("ButtonsContainer");
            _buttonsContainer = buttonsObj.AddComponent<RectTransform>();
            _buttonsContainer.SetParent(_rootContainer, false);
        }

        protected override void ApplyRender()
        {
            if (_canvas == null) return;
            ApplyProperties();
        }

        private void ApplyProperties()
        {
            // 1. Theme and Colors
            bool isDark = json.Theme == ArenaArenauiButtonPanelJson.ThemeType.Dark;
            Color bgColor = isDark ? ArenaUIUtils.DARK_BG : ArenaUIUtils.LIGHT_BG;
            Color textBgColor = isDark ? ArenaUIUtils.DARK_TEXT_BG : ArenaUIUtils.LIGHT_TEXT_BG;
            Color buttonBgColor = isDark ? ArenaUIUtils.DARK_BUTTON_BG : ArenaUIUtils.LIGHT_BUTTON_BG;
            Color textColor = isDark ? ArenaUIUtils.DARK_TEXT : ArenaUIUtils.LIGHT_TEXT;
            Color hoverColor = isDark ? ArenaUIUtils.DARK_BUTTON_HOVER : ArenaUIUtils.LIGHT_BUTTON_HOVER;

            _bgImage.color = bgColor;
            
            // 2. Title
            if (!string.IsNullOrEmpty(json.Title))
            {
                _titleText.gameObject.SetActive(true);
                _titleText.text = json.Title;
                _titleText.color = textColor;
                
                if (TMP_Settings.defaultFontAsset != null)
                {
                    _titleText.font = TMP_Settings.defaultFontAsset;
                }
            }
            else
            {
                _titleText.gameObject.SetActive(false);
            }

            // 3. Layout Direction
            if (_buttonsLayoutGroup != null)
            {
                if ((json.Vertical && _buttonsLayoutGroup is HorizontalLayoutGroup) ||
                    (!json.Vertical && _buttonsLayoutGroup is VerticalLayoutGroup))
                {
                    DestroyImmediate(_buttonsLayoutGroup);
                    _buttonsLayoutGroup = null;
                }
            }

            if (_buttonsLayoutGroup == null)
            {
                if (json.Vertical)
                {
                    var vlg = _buttonsContainer.gameObject.AddComponent<VerticalLayoutGroup>();
                    vlg.childAlignment = TextAnchor.MiddleCenter;
                    vlg.childControlHeight = true;
                    vlg.childControlWidth = true;
                    vlg.childForceExpandHeight = false;
                    vlg.childForceExpandWidth = true;
                    vlg.spacing = 20f; // Matches ARENALayout.buttonMargin
                    _buttonsLayoutGroup = vlg;
                }
                else
                {
                    var hlg = _buttonsContainer.gameObject.AddComponent<HorizontalLayoutGroup>();
                    hlg.childAlignment = TextAnchor.MiddleCenter;
                    hlg.childControlHeight = true;
                    hlg.childControlWidth = true;
                    hlg.childForceExpandHeight = true;
                    hlg.childForceExpandWidth = false;
                    hlg.spacing = 20f; // Matches ARENALayout.buttonMargin
                    _buttonsLayoutGroup = hlg;
                }
                
                var fitter = _buttonsContainer.gameObject.GetComponent<ContentSizeFitter>();
                if (fitter == null)
                {
                    fitter = _buttonsContainer.gameObject.AddComponent<ContentSizeFitter>();
                }
                fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            }

            // 4. Buttons
            int targetButtonCount = json.Buttons != null ? json.Buttons.Length : 0;
            
            // Sync list with actual hierarchy to survive hot-reloads
            _buttonInstances.Clear();
            foreach (Transform child in _buttonsContainer)
            {
                _buttonInstances.Add(child.gameObject);
            }

            // Create or destroy buttons to match count
            while (_buttonInstances.Count < targetButtonCount)
            {
                _buttonInstances.Add(CreateButton());
            }
            while (_buttonInstances.Count > targetButtonCount)
            {
                int lastIdx = _buttonInstances.Count - 1;
                DestroyImmediate(_buttonInstances[lastIdx]);
                _buttonInstances.RemoveAt(lastIdx);
            }

            // Update button texts, images, and handlers
            for (int i = 0; i < targetButtonCount; i++)
            {
                var btnObj = _buttonInstances[i];
                string btnName = $"Button {i}";
                string btnImg = null;
                float imgSize = 0.3f; // ARENALayout.buttonImgDefaultSize
                
                if (json.Buttons[i] is string strBtn)
                {
                    btnName = strBtn;
                }
                else if (json.Buttons[i] is JValue jVal && jVal.Type == JTokenType.String)
                {
                    btnName = jVal.ToString();
                }
                else if (json.Buttons[i] is JObject jObj)
                {
                    btnName = jObj.TryGetValue("name", out var nameToken) ? nameToken.ToString() : btnName;
                    btnImg = jObj.TryGetValue("img", out var imgToken) ? imgToken.ToString() : null;
                    imgSize = jObj.TryGetValue("size", out var sizeToken) ? sizeToken.Value<float>() : 0.3f;
                }
                
                var btnText = btnObj.GetComponentInChildren<TextMeshProUGUI>(true);
                var rawImages = btnObj.GetComponentsInChildren<RawImage>(true);
                var btnIcon = rawImages.Length > 0 ? rawImages[0] : null;

                if (!string.IsNullOrEmpty(btnImg))
                {
                    if (btnText != null) btnText.gameObject.SetActive(false);
                    
                    if (btnIcon == null)
                    {
                        var iconObj = new GameObject("Icon");
                        iconObj.transform.SetParent(btnObj.transform, false);
                        btnIcon = iconObj.AddComponent<RawImage>();
                        iconObj.AddComponent<LayoutElement>();
                    }
                    btnIcon.gameObject.SetActive(true);
                    
                    var iconLE = btnIcon.GetComponent<LayoutElement>();
                    iconLE.preferredWidth = imgSize * ArenaUIUtils.PIXELS_PER_METER;
                    iconLE.preferredHeight = imgSize * ArenaUIUtils.PIXELS_PER_METER;
                    
                    StartCoroutine(LoadButtonImage(btnIcon, btnImg));
                }
                else
                {
                    if (btnIcon != null) btnIcon.gameObject.SetActive(false);
                    
                    if (btnText != null)
                    {
                        btnText.gameObject.SetActive(true);
                        btnText.text = btnName;
                        btnText.color = textColor;
                        if (TMP_Settings.defaultFontAsset != null) btnText.font = TMP_Settings.defaultFontAsset;
                    }
                }

                var btnBg = btnObj.GetComponent<Image>();
                if (btnBg != null)
                {
                    btnBg.color = buttonBgColor;
                }

                var clickHandler = btnObj.GetComponent<ArenaUI_ButtonClickHandler>();
                if (clickHandler != null)
                {
                    clickHandler.TargetObjectId = gameObject.name;
                    clickHandler.ButtonName = btnName;
                    clickHandler.ButtonIndex = i;
                    clickHandler.DefaultColor = buttonBgColor;
                    clickHandler.HoverColor = hoverColor;
                    clickHandler.ButtonImage = btnBg;
                }
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(_rootContainer);

            // Re-apply box colliders after layout rebuilt
            for (int i = 0; i < _buttonInstances.Count; i++)
            {
                var btnObj = _buttonInstances[i];
                var rect = btnObj.GetComponent<RectTransform>();
                var boxCol = btnObj.GetComponent<BoxCollider>();
                if (boxCol != null && rect != null)
                {
                    boxCol.size = new Vector3(rect.rect.width, rect.rect.height, 0.1f);
                }
            }
        }

        private GameObject CreateButton()
        {
            var btnObj = new GameObject("Button");
            var btnRect = btnObj.AddComponent<RectTransform>();
            btnRect.SetParent(_buttonsContainer, false);
            
            var btnBg = btnObj.AddComponent<Image>();
            btnBg.type = Image.Type.Sliced;
            
            var layout = btnObj.AddComponent<HorizontalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.padding = new RectOffset(75, 75, 15, 15); // Matches ARENALayout.buttonPadding
            
            var fitter = btnObj.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var textObj = new GameObject("Text");
            var textRect = textObj.AddComponent<RectTransform>();
            textRect.SetParent(btnRect, false);
            
            var text = textObj.AddComponent<TextMeshProUGUI>();
            text.alignment = TextAlignmentOptions.Center;
            text.fontSize = 75f; // Matches ARENATypography.button
            text.enableAutoSizing = false;
            text.enableWordWrapping = false;

            btnObj.AddComponent<BoxCollider>();
            btnObj.AddComponent<ArenaUI_ButtonClickHandler>();

            return btnObj;
        }

        private System.Collections.IEnumerator LoadButtonImage(RawImage rawImage, string url)
        {
            if (string.IsNullOrEmpty(url)) yield break;

            string assetPath = ArenaClientScene.Instance != null ? ArenaClientScene.Instance.checkLocalAsset(url) : null;
            if (assetPath == null && ArenaClientScene.Instance != null)
            {
                bool loaded = false;
                ArenaClientScene.Instance.RegisterAssetCallback(url, () => { loaded = true; });
                yield return new WaitUntil(() => loaded);
                assetPath = ArenaClientScene.Instance.checkLocalAsset(url);
            }
            
            string loadUrl = assetPath != null ? "file://" + System.IO.Path.GetFullPath(assetPath) : url;

            UnityEngine.Networking.UnityWebRequest request = UnityEngine.Networking.UnityWebRequestTexture.GetTexture(loadUrl);
            yield return request.SendWebRequest();
            
            if (request.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                var texture = ((UnityEngine.Networking.DownloadHandlerTexture)request.downloadHandler).texture;
                rawImage.texture = texture;
            }
            else
            {
                Debug.LogError($"[ArenaUI] Failed to load image: {loadUrl} - {request.error}");
            }
        }

        protected override void Update()
        {
            base.Update();

            if (_canvasGroup != null && Camera.main != null)
            {
                if (json.MaterialSides == ArenaArenauiButtonPanelJson.MaterialSidesType.Front)
                {
                    Vector3 toCam = Camera.main.transform.position - _canvasRect.position;
                    if (Vector3.Dot(-_canvasRect.forward, toCam) < 0)
                    {
                        _canvasGroup.alpha = 0f;
                    }
                    else
                    {
                        _canvasGroup.alpha = 1f;
                    }
                }
                else if (_canvasGroup.alpha != 1f)
                {
                    _canvasGroup.alpha = 1f;
                }
            }
        }

        public override void UpdateObject()
        {
            PublishIfChanged(JsonConvert.SerializeObject(json));
        }
    }
}
