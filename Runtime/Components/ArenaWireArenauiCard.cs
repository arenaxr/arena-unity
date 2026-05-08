using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using TMPro;
using ArenaUnity.Components;
using ArenaUnity.Schemas;
using Newtonsoft.Json;

namespace ArenaUnity
{
    public class ArenaWireArenauiCard : ArenaComponent
    {
        // ARENA arenaui-card component unity conversion status:
        // DONE: title
        // DONE: body
        // DONE: bodyAlign
        // DONE: img
        // DONE: imgCaption
        // DONE: imgDirection
        // DONE: imgSize
        // DONE: textImageRatio
        // DONE: fontSize
        // DONE: widthScale
        // DONE: closeButton
        // DONE: font
        // DONE: theme
        // DONE: materialSides

        public ArenaArenauiCardJson json = new ArenaArenauiCardJson();

        private Canvas _canvas;
        private RectTransform _canvasRect;
        private CanvasGroup _canvasGroup;
        private Image _bgImage;
        private HorizontalLayoutGroup _layoutGroup;
        
        private RectTransform _textContainer;
        private Image _textBgImage;
        private TextMeshProUGUI _titleText;
        private TextMeshProUGUI _bodyText;

        private RectTransform _imgContainer;
        private Image _imgBgImage;
        private RawImage _rawImage;
        private RectTransform _captionContainer;
        private Image _captionBgImage;
        private TextMeshProUGUI _captionText;

        private RectTransform _rootContainer;
        private RectTransform _closeButtonContainer;
        private Image _closeBgImage;
        private TextMeshProUGUI _closeText;

        // ARENA UI Constants
        private const float PIXELS_PER_METER = 1000f; // 1 unit in A-Frame = 1000 pixels in Canvas
        private readonly Color LIGHT_BG = new Color(0.95f, 0.95f, 0.95f, 0.8f);
        private readonly Color LIGHT_TEXT_BG = new Color(0f, 0f, 0f, 0.25f);
        private readonly Color LIGHT_TEXT = new Color(0.23f, 0.23f, 0.23f, 1f);
        
        private readonly Color DARK_BG = new Color(0.24f, 0.24f, 0.24f, 1f);
        private readonly Color DARK_TEXT_BG = new Color(0.24f, 0.24f, 0.24f, 0.25f);
        private readonly Color DARK_TEXT = new Color(0.94f, 0.94f, 0.94f, 1f);
        
        private readonly Color CAPTION_BG = new Color(1f, 1f, 1f, 0.75f);

        protected override void ApplyRender()
        {
            if (_canvas == null)
                InitializeHierarchy();

            ApplyProperties();
        }

        private void InitializeHierarchy()
        {
            // Root Canvas must be a child to avoid conflicting with ARENA entity scale
            var canvasObj = new GameObject("ArenauiCardCanvas");
            canvasObj.transform.SetParent(gameObject.transform, false);
            canvasObj.transform.localPosition = Vector3.zero;
            canvasObj.transform.localRotation = Quaternion.identity;

            _canvas = canvasObj.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.WorldSpace;
            _canvas.worldCamera = Camera.main;
            
            _canvasGroup = canvasObj.AddComponent<CanvasGroup>();

            _canvasRect = canvasObj.GetComponent<RectTransform>();
            _canvasRect.localScale = new Vector3(1f / PIXELS_PER_METER, 1f / PIXELS_PER_METER, 1f / PIXELS_PER_METER);
            _canvasRect.pivot = new Vector2(0.5f, 0.5f);

            // Root Container (Stacks Card and Close Button vertically)
            var rootObj = new GameObject("RootContainer");
            rootObj.transform.SetParent(_canvasRect, false);
            _rootContainer = rootObj.AddComponent<RectTransform>();
            _rootContainer.anchorMin = new Vector2(0.5f, 0.5f);
            _rootContainer.anchorMax = new Vector2(0.5f, 0.5f);
            _rootContainer.pivot = new Vector2(0.5f, 0.5f);
            _rootContainer.localPosition = Vector3.zero;

            var rootLayout = rootObj.AddComponent<VerticalLayoutGroup>();
            rootLayout.childControlWidth = true;
            rootLayout.childControlHeight = true;
            rootLayout.childForceExpandWidth = false;
            rootLayout.childForceExpandHeight = false;
            rootLayout.childAlignment = TextAnchor.MiddleCenter;
            rootLayout.spacing = 15;

            var rootCsf = rootObj.AddComponent<ContentSizeFitter>();
            rootCsf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            rootCsf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Content Container (Outer padded container, textBg color)
            var contentObj = new GameObject("CardBackground");
            contentObj.transform.SetParent(_rootContainer, false);
            _bgImage = contentObj.AddComponent<Image>(); 

            var contentRect = contentObj.GetComponent<RectTransform>();

            var csf = contentObj.AddComponent<ContentSizeFitter>();
            csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            _layoutGroup = contentObj.AddComponent<HorizontalLayoutGroup>();
            _layoutGroup.childControlWidth = true;
            _layoutGroup.childControlHeight = true;
            _layoutGroup.childForceExpandWidth = false;
            _layoutGroup.childForceExpandHeight = true;
            _layoutGroup.padding = new RectOffset(25, 25, 25, 25);
            _layoutGroup.spacing = 15;

            // Text Container
            var textObj = new GameObject("TextContainer");
            textObj.transform.SetParent(contentObj.transform, false);
            _textBgImage = textObj.AddComponent<Image>();
            _textContainer = textObj.GetComponent<RectTransform>();
            
            var textLayout = textObj.AddComponent<VerticalLayoutGroup>();
            textLayout.childControlWidth = true;
            textLayout.childControlHeight = true; 
            textLayout.childForceExpandWidth = true;
            textLayout.childForceExpandHeight = false;
            textLayout.padding = new RectOffset(25, 25, 25, 25);
            textLayout.spacing = 15;
            textLayout.childAlignment = TextAnchor.UpperLeft;

            // Title Text
            var titleObj = new GameObject("Title");
            titleObj.transform.SetParent(textObj.transform, false);
            _titleText = titleObj.AddComponent<TextMeshProUGUI>();
            _titleText.fontStyle = FontStyles.Bold;
            _titleText.alignment = TextAlignmentOptions.Center;

            // Body Text
            var bodyObj = new GameObject("Body");
            bodyObj.transform.SetParent(textObj.transform, false);
            _bodyText = bodyObj.AddComponent<TextMeshProUGUI>();
            _bodyText.enableWordWrapping = true;

            // Image Container
            var imgObj = new GameObject("ImageContainer");
            imgObj.transform.SetParent(contentObj.transform, false);
            _imgBgImage = imgObj.AddComponent<Image>();
            _imgContainer = imgObj.GetComponent<RectTransform>();
            imgObj.AddComponent<RectMask2D>(); // Prevent image bleed when EnvelopParent is used
            
            var imgLayout = imgObj.AddComponent<VerticalLayoutGroup>();
            imgLayout.childControlWidth = true;
            imgLayout.childControlHeight = true;
            imgLayout.childForceExpandWidth = true;
            imgLayout.childForceExpandHeight = true;

            // Raw Image (Wrapper)
            var rawObj = new GameObject("ImageWrapper");
            rawObj.transform.SetParent(imgObj.transform, false);
            _rawImage = rawObj.AddComponent<RawImage>();
            var rawLE = rawObj.AddComponent<LayoutElement>();
            rawLE.flexibleHeight = 1f; // allow image to fill vertical space

            // Caption Container
            var captionContObj = new GameObject("CaptionContainer");
            captionContObj.transform.SetParent(rawObj.transform, false); // overlaid on RawImage
            _captionBgImage = captionContObj.AddComponent<Image>();
            _captionContainer = captionContObj.GetComponent<RectTransform>();
            _captionContainer.anchorMin = new Vector2(0.5f, 0f); // Bottom center
            _captionContainer.anchorMax = new Vector2(0.5f, 0f);
            _captionContainer.pivot = new Vector2(0.5f, 0f);
            _captionContainer.anchoredPosition = new Vector2(0, 25f); // 25px bottom margin
            
            var captionLayout = captionContObj.AddComponent<HorizontalLayoutGroup>();
            captionLayout.childControlWidth = true;
            captionLayout.childControlHeight = true;
            captionLayout.childForceExpandWidth = false;
            captionLayout.childForceExpandHeight = false;
            captionLayout.padding = new RectOffset(20, 20, 10, 10);

            var captionCsf = captionContObj.AddComponent<ContentSizeFitter>();
            captionCsf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            captionCsf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var captionTextObj = new GameObject("CaptionText");
            captionTextObj.transform.SetParent(captionContObj.transform, false);
            _captionText = captionTextObj.AddComponent<TextMeshProUGUI>();
            _captionText.alignment = TextAlignmentOptions.Center;
            _captionContainer.gameObject.SetActive(false);
            
            // Close Button Container
            var closeObj = new GameObject("CloseButtonContainer");
            closeObj.transform.SetParent(_rootContainer, false);
            _closeBgImage = closeObj.AddComponent<Image>();
            _closeButtonContainer = closeObj.GetComponent<RectTransform>();
            
            var closeLayout = closeObj.AddComponent<HorizontalLayoutGroup>();
            closeLayout.childControlWidth = true;
            closeLayout.childControlHeight = true;
            closeLayout.childForceExpandWidth = false;
            closeLayout.childForceExpandHeight = false;
            closeLayout.padding = new RectOffset(40, 40, 15, 15);
            
            var closeCsf = closeObj.AddComponent<ContentSizeFitter>();
            closeCsf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            closeCsf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var closeTextObj = new GameObject("CloseText");
            closeTextObj.transform.SetParent(closeObj.transform, false);
            _closeText = closeTextObj.AddComponent<TextMeshProUGUI>();
            _closeText.alignment = TextAlignmentOptions.Center;
            _closeText.text = "Close";
            _closeButtonContainer.gameObject.SetActive(false);
            
            // Add interaction components
            closeObj.AddComponent<BoxCollider>();
            var clickHandler = closeObj.AddComponent<ArenaUI_ButtonClickHandler>();
            clickHandler.ButtonName = "Close";
            clickHandler.Card = this;

            // Assign default fonts if possible
            if (TMP_Settings.defaultFontAsset != null)
            {
                _titleText.font = TMP_Settings.defaultFontAsset;
                _bodyText.font = TMP_Settings.defaultFontAsset;
                _captionText.font = TMP_Settings.defaultFontAsset;
                _closeText.font = TMP_Settings.defaultFontAsset;
            }
        }

        private void ApplyProperties()
        {
            // 1. Theme and Colors
            bool isDark = json.Theme == ArenaArenauiCardJson.ThemeType.Dark;
            Color bgColor = isDark ? DARK_BG : LIGHT_BG;
            Color textBgColor = isDark ? DARK_TEXT_BG : LIGHT_TEXT_BG;
            Color textColor = isDark ? DARK_TEXT : LIGHT_TEXT;
            Color hoverColor = isDark ? new Color(0.21f, 0.21f, 0.21f, 0.8f) : new Color(0.82f, 0.82f, 0.82f, 0.8f);

            _bgImage.color = textBgColor;
            _textBgImage.color = bgColor;
            _imgBgImage.color = bgColor;
            _captionBgImage.color = CAPTION_BG;
            _closeBgImage.color = textBgColor;
            
            _titleText.color = textColor;
            _bodyText.color = textColor;
            _captionText.color = LIGHT_TEXT; // Captions usually have light text against white bg
            _closeText.color = textColor;

            // 2. Text Content
            _titleText.text = json.Title;
            _titleText.gameObject.SetActive(!string.IsNullOrEmpty(json.Title));

            _bodyText.text = json.Body;
            _bodyText.gameObject.SetActive(!string.IsNullOrEmpty(json.Body));

            switch (json.BodyAlign)
            {
                case ArenaArenauiCardJson.BodyAlignType.Left: _bodyText.alignment = TextAlignmentOptions.Left; break;
                case ArenaArenauiCardJson.BodyAlignType.Center: _bodyText.alignment = TextAlignmentOptions.Center; break;
                case ArenaArenauiCardJson.BodyAlignType.Right: _bodyText.alignment = TextAlignmentOptions.Right; break;
                case ArenaArenauiCardJson.BodyAlignType.Justify: _bodyText.alignment = TextAlignmentOptions.Justified; break;
            }

            // 3. Fonts
            float baseSize = json.FontSize * PIXELS_PER_METER;
            _bodyText.fontSize = baseSize;
            _captionText.fontSize = baseSize * 0.8f;
            _titleText.fontSize = baseSize * 1.4f;
            _closeText.fontSize = baseSize;
            
            // Match A-Frame's title margin: [0, containerPadding, containerPadding, containerPadding]
            _titleText.margin = new Vector4(25f, 0f, 25f, 25f);
            
            // Close Button
            _closeButtonContainer.gameObject.SetActive(json.CloseButton);
            if (json.CloseButton)
            {
                var clickHandler = _closeButtonContainer.GetComponent<ArenaUI_ButtonClickHandler>();
                if (clickHandler != null)
                {
                    clickHandler.DefaultColor = textBgColor;
                    clickHandler.HoverColor = hoverColor;
                    clickHandler.ButtonImage = _closeBgImage;
                }
            }

            // 4. Image Loading
            bool hasImage = !string.IsNullOrEmpty(json.Img);
            _imgContainer.gameObject.SetActive(hasImage);

            if (hasImage)
            {
                StartCoroutine(LoadImage(json.Img));

                _captionText.text = json.ImgCaption;
                bool hasCaption = !string.IsNullOrEmpty(json.ImgCaption);
                _captionContainer.gameObject.SetActive(hasCaption);
            }

            // 5. Layout and Dimensions
            float widthScale = json.WidthScale * PIXELS_PER_METER;
            float ratio = json.TextImageRatio;

            if (json.ImgDirection == ArenaArenauiCardJson.ImgDirectionType.Left)
                _imgContainer.SetAsFirstSibling();
            else
                _imgContainer.SetAsLastSibling();

            var textLE = _textContainer.gameObject.GetComponent<LayoutElement>() ?? _textContainer.gameObject.AddComponent<LayoutElement>();
            var imgLE = _imgContainer.gameObject.GetComponent<LayoutElement>() ?? _imgContainer.gameObject.AddComponent<LayoutElement>();

            var bgRect = _bgImage.GetComponent<RectTransform>();

            // The content width calculation must account for the outer container padding (2x25) and spacing (15).
            float outerPadding = 50f;
            float innerSpacing = 15f;

            if (hasImage)
            {
                imgLE.preferredWidth = widthScale;
                textLE.preferredWidth = widthScale * ratio;
                bgRect.sizeDelta = new Vector2(widthScale + (widthScale * ratio) + outerPadding + innerSpacing, 0); 
            }
            else
            {
                textLE.preferredWidth = widthScale * (1f + ratio);
                bgRect.sizeDelta = new Vector2((widthScale * (1f + ratio)) + outerPadding, 0);
            }

            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(bgRect);
            LayoutRebuilder.ForceRebuildLayoutImmediate(_rootContainer);

            // Update button collider size after layout rebuild
            if (json.CloseButton)
            {
                var boxCol = _closeButtonContainer.GetComponent<BoxCollider>();
                if (boxCol != null)
                {
                    boxCol.size = new Vector3(_closeButtonContainer.rect.width, _closeButtonContainer.rect.height, 0.1f);
                }
            }
        }

        private IEnumerator LoadImage(string url)
        {
            var uri = ArenaClientScene.Instance.ConstructRemoteUrl(url);
            if (uri == null) yield break;

            using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(uri.AbsoluteUri))
            {
                yield return uwr.SendWebRequest();

                if (uwr.result == UnityWebRequest.Result.Success)
                {
                    Texture2D texture = DownloadHandlerTexture.GetContent(uwr);
                    _rawImage.texture = texture;

                    var fitter = _rawImage.gameObject.GetComponent<AspectRatioFitter>() ?? _rawImage.gameObject.AddComponent<AspectRatioFitter>();
                    fitter.aspectRatio = (float)texture.width / texture.height;

                    if (json.ImgSize == ArenaArenauiCardJson.ImgSizeType.Contain)
                        fitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
                    else if (json.ImgSize == ArenaArenauiCardJson.ImgSizeType.Cover)
                        fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
                    else
                        fitter.aspectMode = AspectRatioFitter.AspectMode.None; // Stretch
                }
                else
                {
                    Debug.LogWarning($"ArenaWireArenauiCard: Failed to load image from {url}: {uwr.error}");
                }
            }
        }

        protected override void Update()
        {
            base.Update();

            if (_canvasGroup != null && Camera.main != null)
            {
                if (json.MaterialSides == ArenaArenauiCardJson.MaterialSidesType.Front)
                {
                    // Unity UI text is readable from the -Z direction natively
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
                    // Fallback for 'both' (default DoubleSide behavior)
                    _canvasGroup.alpha = 1f;
                }
            }
        }

        public override void UpdateObject()
        {
            PublishIfChanged(JsonConvert.SerializeObject(json));
        }
        
        public class ArenaUI_ButtonClickHandler : MonoBehaviour
        {
            public string ButtonName;
            public ArenaWireArenauiCard Card;
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
                if (Card == null || ArenaClientScene.Instance == null)
                {
                    Debug.LogWarning("[ArenaUI] Card or ArenaClientScene is null");
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
                    ["target"] = Card.gameObject.name,
                    ["targetPosition"] = Newtonsoft.Json.Linq.JToken.FromObject(ArenaUnity.ToArenaPosition(transform.position)),
                    ["originPosition"] = Newtonsoft.Json.Linq.JToken.FromObject(ArenaUnity.ToArenaPosition(cam.transform.position)),
                    ["buttonName"] = ButtonName
                };

                Debug.Log($"[ArenaUI] Publishing buttonClick: {data.ToString(Newtonsoft.Json.Formatting.None)}");
                ArenaClientScene.Instance.PublishEvent("buttonClick", arenaCam.camid, data.ToString(Newtonsoft.Json.Formatting.None));
                
                // Immediately remove locally, matching A-Frame's this.el.remove()
                // Bypass local delete prompt by flagging as external
                var arenaObj = Card.GetComponent<ArenaObject>();
                if (arenaObj != null)
                {
                    arenaObj.externalDelete = true;
                }
                
                // Clean up dictionary reference to prevent missing reference exceptions
                ArenaClientScene.Instance.arenaObjs.Remove(Card.gameObject.name);
                
                Destroy(Card.gameObject);
            }
        }
    }
}
