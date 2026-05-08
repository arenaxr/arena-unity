/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2023, Carnegie Mellon University. All rights reserved.
 */

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ArenaUnity.Components;
using ArenaUnity.Schemas;
using Newtonsoft.Json;

namespace ArenaUnity
{
    public class ArenaWireArenauiPrompt : ArenaComponent
    {
        // ARENA arenaui-prompt component unity conversion status:
        // TODO: title
        // TODO: description
        // TODO: buttons
        // TODO: width
        // TODO: font
        // TODO: theme
        // TODO: materialSides

        public ArenaArenauiPromptJson json = new ArenaArenauiPromptJson();

        private Canvas _canvas;
        private RectTransform _canvasRect;
        private CanvasGroup _canvasGroup;

        private RectTransform _rootContainer;
        private Image _bgImage;
        private TextMeshProUGUI _titleText;
        private TextMeshProUGUI _descText;

        private RectTransform _buttonsContainer;
        private List<GameObject> _buttonInstances = new List<GameObject>();

        protected override void ApplyRender()
        {
            if (_canvas == null)
                InitializeHierarchy();

            ApplyProperties();
        }

        private void InitializeHierarchy()
        {
            // Canvas
            var canvasObj = new GameObject("ArenauiPromptCanvas");
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
            rootObj.transform.SetParent(_canvasRect, false);
            _rootContainer = rootObj.AddComponent<RectTransform>();
            _rootContainer.localPosition = Vector3.zero;

            _bgImage = rootObj.AddComponent<Image>();

            var rootLayout = rootObj.AddComponent<VerticalLayoutGroup>();
            rootLayout.childControlWidth = true;
            rootLayout.childControlHeight = true;
            rootLayout.childForceExpandWidth = true;
            rootLayout.childForceExpandHeight = false;
            rootLayout.childAlignment = TextAnchor.MiddleCenter;
            rootLayout.padding = new RectOffset(40, 40, 40, 40);
            rootLayout.spacing = 20;

            var rootCsf = rootObj.AddComponent<ContentSizeFitter>();
            rootCsf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained; // width is fixed by json.Width
            rootCsf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Title
            var titleObj = new GameObject("Title");
            titleObj.transform.SetParent(_rootContainer, false);
            _titleText = titleObj.AddComponent<TextMeshProUGUI>();
            _titleText.fontStyle = FontStyles.Bold;
            _titleText.alignment = TextAlignmentOptions.Center;

            // Description
            var descObj = new GameObject("Description");
            descObj.transform.SetParent(_rootContainer, false);
            _descText = descObj.AddComponent<TextMeshProUGUI>();
            _descText.alignment = TextAlignmentOptions.Center;
            _descText.enableWordWrapping = true;

            // Buttons Container
            var buttonsObj = new GameObject("ButtonsContainer");
            buttonsObj.transform.SetParent(_rootContainer, false);
            _buttonsContainer = buttonsObj.AddComponent<RectTransform>();

            var buttonsLayout = buttonsObj.AddComponent<HorizontalLayoutGroup>();
            buttonsLayout.childControlWidth = true;
            buttonsLayout.childControlHeight = true;
            buttonsLayout.childForceExpandWidth = true;
            buttonsLayout.childForceExpandHeight = false;
            buttonsLayout.childAlignment = TextAnchor.MiddleCenter;
            buttonsLayout.spacing = 20;
        }

        private void ApplyProperties()
        {
            // 1. Theme and Colors
            bool isDark = json.Theme == ArenaArenauiPromptJson.ThemeType.Dark;
            Color bgColor = isDark ? ArenaUIUtils.DARK_BG : ArenaUIUtils.LIGHT_BG;
            Color buttonBgColor = isDark ? ArenaUIUtils.DARK_BUTTON_BG : ArenaUIUtils.LIGHT_BUTTON_BG;
            Color textColor = isDark ? ArenaUIUtils.DARK_TEXT : ArenaUIUtils.LIGHT_TEXT;
            Color hoverColor = isDark ? ArenaUIUtils.DARK_BUTTON_HOVER : ArenaUIUtils.LIGHT_BUTTON_HOVER;

            _bgImage.color = bgColor;
            _titleText.color = textColor;
            _descText.color = textColor;

            // 2. Texts
            _titleText.text = json.Title;
            _titleText.gameObject.SetActive(!string.IsNullOrEmpty(json.Title));

            _descText.text = json.Description;
            _descText.gameObject.SetActive(!string.IsNullOrEmpty(json.Description));

            // 3. Layout Width
            float fixedWidth = json.Width * ArenaUIUtils.PIXELS_PER_METER;
            _rootContainer.sizeDelta = new Vector2(fixedWidth, 0);

            // 4. Fonts
            if (TMP_Settings.defaultFontAsset != null)
            {
                _titleText.font = TMP_Settings.defaultFontAsset;
                _descText.font = TMP_Settings.defaultFontAsset;
            }

            float baseSize = 0.04f * ArenaUIUtils.PIXELS_PER_METER;
            _titleText.fontSize = baseSize * 1.5f;
            _descText.fontSize = baseSize;

            // 5. Buttons
            int targetButtonCount = json.Buttons != null ? json.Buttons.Length : 0;
            
            // Sync list with actual hierarchy to survive hot-reloads
            _buttonInstances.Clear();
            foreach (Transform child in _buttonsContainer)
            {
                _buttonInstances.Add(child.gameObject);
            }

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

            for (int i = 0; i < targetButtonCount; i++)
            {
                var btnObj = _buttonInstances[i];
                string btnName = json.Buttons[i];

                var btnText = btnObj.GetComponentInChildren<TextMeshProUGUI>();
                if (btnText != null)
                {
                    btnText.text = btnName;
                    btnText.color = textColor;
                    if (TMP_Settings.defaultFontAsset != null) btnText.font = TMP_Settings.defaultFontAsset;
                    btnText.fontSize = baseSize;
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
                    clickHandler.TargetObjectToDestroy = gameObject;
                    clickHandler.DestroyOnSelect = true; // Prompts destroy themselves on action
                    clickHandler.ButtonName = btnName;
                    clickHandler.ButtonIndex = i;
                    clickHandler.DefaultColor = buttonBgColor;
                    clickHandler.HoverColor = hoverColor;
                    clickHandler.ButtonImage = btnBg;
                }
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(_rootContainer);

            // Apply colliders to buttons
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
            var btnObj = new GameObject("PromptButton");
            var btnRect = btnObj.AddComponent<RectTransform>();
            btnRect.SetParent(_buttonsContainer, false);

            var btnBg = btnObj.AddComponent<Image>();

            var btnLayout = btnObj.AddComponent<HorizontalLayoutGroup>();
            btnLayout.childControlWidth = true;
            btnLayout.childControlHeight = true;
            btnLayout.childForceExpandWidth = false;
            btnLayout.childForceExpandHeight = false;
            btnLayout.childAlignment = TextAnchor.MiddleCenter;
            btnLayout.padding = new RectOffset(20, 20, 10, 10);

            var textObj = new GameObject("ButtonText");
            textObj.transform.SetParent(btnObj.transform, false);
            var btnText = textObj.AddComponent<TextMeshProUGUI>();
            btnText.alignment = TextAlignmentOptions.Center;

            btnObj.AddComponent<BoxCollider>();
            btnObj.AddComponent<ArenaUI_ButtonClickHandler>();

            return btnObj;
        }

        protected override void Update()
        {
            base.Update();

            if (_canvasGroup != null && Camera.main != null)
            {
                if (json.MaterialSides == ArenaArenauiPromptJson.MaterialSidesType.Front)
                {
                    Vector3 toCam = Camera.main.transform.position - _canvasRect.position;
                    _canvasGroup.alpha = (Vector3.Dot(-_canvasRect.forward, toCam) < 0) ? 0f : 1f;
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
