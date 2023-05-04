using UniFan;
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace HotCode.FrameworkEditor
{
    internal static class UIExDefaultControls
    {

        private const float kWidth = 160f;
        private const float kThickHeight = 30f;
        private const float kThinHeight = 20f;
        private static Vector2 s_ThickElementSize = new Vector2(kWidth, kThickHeight);
        private static Vector2 s_ThinElementSize = new Vector2(kWidth, kThinHeight);
        private static Vector2 s_ImageElementSize = new Vector2(100f, 100f);
        private static Color s_DefaultSelectableColor = new Color(1f, 1f, 1f, 1f);
        private static Color s_PanelColor = new Color(1f, 1f, 1f, 0.392f);
        private static Color s_TextColor = new Color(50f / 255f, 50f / 255f, 50f / 255f, 1f);

        public static GameObject CreateGameObject(string name, params Type[] components)
        {
            return new GameObject(name, components);
        }

        // Helper methods at top
        private static GameObject CreateUIElementRoot(string name, Vector2 size, params Type[] components)
        {
            GameObject child = CreateGameObject(name, components);
            RectTransform rectTransform = child.GetComponent<RectTransform>();
            rectTransform.sizeDelta = size;
            return child;
        }

        private static GameObject CreateUIObject(string name, GameObject parent, params Type[] components)
        {
            GameObject go = CreateGameObject(name, components);
            SetParentAndAlign(go, parent);
            return go;
        }

        private static void SetParentAndAlign(GameObject child, GameObject parent)
        {
            if (parent == null)
                return;

#if UNITY_EDITOR
            Undo.SetTransformParent(child.transform, parent.transform, "");
#else
            child.transform.SetParent(parent.transform, false);
#endif
            SetLayerRecursively(child, parent.layer);
        }

        private static void SetLayerRecursively(GameObject go, int layer)
        {
            go.layer = layer;
            Transform t = go.transform;
            for (int i = 0; i < t.childCount; i++)
                SetLayerRecursively(t.GetChild(i).gameObject, layer);
        }


        private static void SetDefaultColorTransitionValues(Selectable slider)
        {
            ColorBlock colors = slider.colors;
            colors.highlightedColor = new Color(0.882f, 0.882f, 0.882f);
            colors.pressedColor = new Color(0.698f, 0.698f, 0.698f);
            colors.disabledColor = new Color(0.521f, 0.521f, 0.521f);
        }


        /// <summary>
        /// Create the basic UI Text.
        /// </summary>
        /// <remarks>
        /// Hierarchy:
        /// (root)
        ///     Text
        /// </remarks>
        /// <param name="resources">The resources to use for creation.</param>
        /// <returns>The root GameObject of the created element.</returns>
        public static GameObject CreateText(DefaultControls.Resources resources)
        {
            GameObject go = CreateUIElementRoot("Text", s_ThickElementSize, typeof(ExText));

            ExText lbl = go.GetComponent<ExText>();
            lbl.text = "New Text";
            lbl.fontSize = 26;
            SetDefaultTextValues(lbl);

            return go;
        }

        /// <summary>
        /// Create the basic UI Image.
        /// </summary>
        /// <remarks>
        /// Hierarchy:
        /// (root)
        ///     Image
        /// </remarks>
        /// <param name="resources">The resources to use for creation.</param>
        /// <returns>The root GameObject of the created element.</returns>
        public static GameObject CreateImage(DefaultControls.Resources resources)
        {
            GameObject go = CreateUIElementRoot("Image", s_ImageElementSize, typeof(ExImage));
            return go;
        }

        /// <summary>
        /// Create the basic UI RawImage.
        /// </summary>
        /// <remarks>
        /// Hierarchy:
        /// (root)
        ///     RawImage
        /// </remarks>
        /// <param name="resources">The resources to use for creation.</param>
        /// <returns>The root GameObject of the created element.</returns>
        public static GameObject CreateRawImage(DefaultControls.Resources resources)
        {
            GameObject go = CreateUIElementRoot("RawImage", s_ImageElementSize, typeof(ExRawImage));
            return go;
        }



        /// <summary>
        /// Create the basic UI Panel.
        /// </summary>
        /// <remarks>
        /// Hierarchy:
        /// (root)
        ///     Image
        /// </remarks>
        /// <param name="resources">The resources to use for creation.</param>
        /// <returns>The root GameObject of the created element.</returns>
        public static GameObject CreateUINoDrawRaycastPanel(DefaultControls.Resources resources)
        {
            GameObject panelRoot = CreateUIElementRoot("Panel", s_ThickElementSize, typeof(UINoDrawRaycast));

            // Set RectTransform to stretch
            RectTransform rectTransform = panelRoot.GetComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = Vector2.zero;

            return panelRoot;
        }

        /// <summary>
        /// Create the basic UI button.
        /// </summary>
        /// <remarks>
        /// Hierarchy:
        /// (root)
        ///     Button
        ///         -Text
        /// </remarks>
        /// <param name="resources">The resources to use for creation.</param>
        /// <returns>The root GameObject of the created element.</returns>
        public static GameObject CreateButton(DefaultControls.Resources resources)
        {
            GameObject buttonRoot = CreateUIElementRoot("Button", s_ThickElementSize, typeof(ExImage), typeof(ExButton));

            GameObject childText = CreateUIObject("Text", buttonRoot, typeof(ExText));

            ExImage image = buttonRoot.GetComponent<ExImage>();
            image.sprite = resources.standard;
            image.type = ExImage.Type.Sliced;
            image.color = s_DefaultSelectableColor;

            ExButton bt = buttonRoot.GetComponent<ExButton>();
            SetDefaultColorTransitionValues(bt);

            ExText text = childText.GetComponent<ExText>();
            text.text = "Button";
            text.alignment = TextAnchor.MiddleCenter;
            SetDefaultTextValues(text);

            RectTransform textRectTransform = childText.GetComponent<RectTransform>();
            textRectTransform.anchorMin = Vector2.zero;
            textRectTransform.anchorMax = Vector2.one;
            textRectTransform.sizeDelta = Vector2.zero;

            return buttonRoot;
        }

        /// <summary>
        /// Create the basic UI Toggle.
        /// </summary>
        /// <remarks>
        /// Hierarchy:
        /// (root)
        ///     Toggle
        ///         - Background
        ///             - Checkmark
        ///         - Label
        /// </remarks>
        /// <param name="resources">The resources to use for creation.</param>
        /// <returns>The root GameObject of the created element.</returns>
        public static GameObject CreateToggle(DefaultControls.Resources resources)
        {
            // Set up hierarchy
            GameObject toggleRoot = CreateUIElementRoot("Toggle", s_ThinElementSize, typeof(ExToggle));

            GameObject background = CreateUIObject("Background", toggleRoot, typeof(ExImage));
            GameObject checkmark = CreateUIObject("Checkmark", background, typeof(ExImage));
            GameObject childLabel = CreateUIObject("Label", toggleRoot, typeof(ExText));

            // Set up components
            ExToggle toggle = toggleRoot.GetComponent<ExToggle>();
            toggle.isOn = true;

            ExImage bgImage = background.GetComponent<ExImage>();
            bgImage.sprite = resources.standard;
            bgImage.type = ExImage.Type.Sliced;
            bgImage.color = s_DefaultSelectableColor;

            ExImage checkmarkImage = checkmark.GetComponent<ExImage>();
            checkmarkImage.sprite = resources.checkmark;

            ExText label = childLabel.GetComponent<ExText>();
            label.text = "Toggle";
            SetDefaultTextValues(label);

            toggle.graphic = checkmarkImage;
            toggle.targetGraphic = bgImage;
            SetDefaultColorTransitionValues(toggle);

            RectTransform bgRect = background.GetComponent<RectTransform>();
            bgRect.anchorMin = new Vector2(0f, 1f);
            bgRect.anchorMax = new Vector2(0f, 1f);
            bgRect.anchoredPosition = new Vector2(10f, -10f);
            bgRect.sizeDelta = new Vector2(kThinHeight, kThinHeight);

            RectTransform checkmarkRect = checkmark.GetComponent<RectTransform>();
            checkmarkRect.anchorMin = new Vector2(0.5f, 0.5f);
            checkmarkRect.anchorMax = new Vector2(0.5f, 0.5f);
            checkmarkRect.anchoredPosition = Vector2.zero;
            checkmarkRect.sizeDelta = new Vector2(20f, 20f);

            RectTransform labelRect = childLabel.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0f, 0f);
            labelRect.anchorMax = new Vector2(1f, 1f);
            labelRect.offsetMin = new Vector2(23f, 1f);
            labelRect.offsetMax = new Vector2(-5f, -2f);

            return toggleRoot;
        }


        /// <summary>
        /// Create the basic UI Slider.
        /// </summary>
        /// <remarks>
        /// Hierarchy:
        /// (root)
        ///     Slider
        ///         - Background
        ///         - Fill Area
        ///             - Fill
        ///         - Handle Slide Area
        ///             - Handle
        /// </remarks>
        /// <param name="resources">The resources to use for creation.</param>
        /// <returns>The root GameObject of the created element.</returns>
        public static GameObject CreateSlider(DefaultControls.Resources resources)
        {
            // Create GOs Hierarchy
            GameObject root = CreateUIElementRoot("Slider", s_ThinElementSize, typeof(ExSlider));

            GameObject background = CreateUIObject("Background", root, typeof(ExImage));
            GameObject fillArea = CreateUIObject("Fill Area", root, typeof(RectTransform));
            GameObject fill = CreateUIObject("Fill", fillArea, typeof(ExImage));
            GameObject handleArea = CreateUIObject("Handle Slide Area", root, typeof(RectTransform));
            GameObject handle = CreateUIObject("Handle", handleArea, typeof(ExImage));

            // Background
            ExImage backgroundImage = background.GetComponent<ExImage>();
            backgroundImage.sprite = resources.background;
            backgroundImage.type = ExImage.Type.Sliced;
            backgroundImage.color = s_DefaultSelectableColor;
            RectTransform backgroundRect = background.GetComponent<RectTransform>();
            backgroundRect.anchorMin = new Vector2(0, 0.25f);
            backgroundRect.anchorMax = new Vector2(1, 0.75f);
            backgroundRect.sizeDelta = new Vector2(0, 0);

            // Fill Area
            RectTransform fillAreaRect = fillArea.GetComponent<RectTransform>();
            fillAreaRect.anchorMin = new Vector2(0, 0.25f);
            fillAreaRect.anchorMax = new Vector2(1, 0.75f);
            fillAreaRect.anchoredPosition = new Vector2(-5, 0);
            fillAreaRect.sizeDelta = new Vector2(-20, 0);

            // Fill
            ExImage fillImage = fill.GetComponent<ExImage>();
            fillImage.sprite = resources.standard;
            fillImage.type = ExImage.Type.Sliced;
            fillImage.color = s_DefaultSelectableColor;

            RectTransform fillRect = fill.GetComponent<RectTransform>();
            fillRect.sizeDelta = new Vector2(10, 0);

            // Handle Area
            RectTransform handleAreaRect = handleArea.GetComponent<RectTransform>();
            handleAreaRect.sizeDelta = new Vector2(-20, 0);
            handleAreaRect.anchorMin = new Vector2(0, 0);
            handleAreaRect.anchorMax = new Vector2(1, 1);

            // Handle
            ExImage handleImage = handle.GetComponent<ExImage>();
            handleImage.sprite = resources.knob;
            handleImage.color = s_DefaultSelectableColor;

            RectTransform handleRect = handle.GetComponent<RectTransform>();
            handleRect.sizeDelta = new Vector2(20, 0);

            // Setup slider component
            ExSlider slider = root.GetComponent<ExSlider>();
            slider.fillRect = fill.GetComponent<RectTransform>();
            slider.handleRect = handle.GetComponent<RectTransform>();
            slider.targetGraphic = handleImage;
            slider.direction = ExSlider.Direction.LeftToRight;
            SetDefaultColorTransitionValues(slider);

            return root;
        }

        /// <summary>
        /// Create the basic UI dropdown.
        /// </summary>
        /// <remarks>
        /// Hierarchy:
        /// (root)
        ///     Dropdown
        ///         - Label
        ///         - Arrow
        ///         - Template
        ///             - Viewport
        ///                 - Content
        ///                     - Item
        ///                         - Item Background
        ///                         - Item Checkmark
        ///                         - Item Label
        ///             - Scrollbar
        ///                 - Sliding Area
        ///                     - Handle
        /// </remarks>
        /// <param name="resources">The resources to use for creation.</param>
        /// <returns>The root GameObject of the created element.</returns>
        public static GameObject CreateDropdown(DefaultControls.Resources resources)
        {
            GameObject root = CreateUIElementRoot("Dropdown", s_ThickElementSize, typeof(ExImage), typeof(ExDropdown));

            GameObject label = CreateUIObject("Label", root, typeof(ExText));
            GameObject arrow = CreateUIObject("Arrow", root, typeof(ExImage));
            GameObject template = CreateUIObject("Template", root, typeof(ExImage), typeof(ScrollRect));
            GameObject viewport = CreateUIObject("Viewport", template, typeof(ExImage), typeof(Mask));
            GameObject content = CreateUIObject("Content", viewport, typeof(RectTransform));
            GameObject item = CreateUIObject("Item", content, typeof(ExToggle));
            GameObject itemBackground = CreateUIObject("Item Background", item, typeof(ExImage));
            GameObject itemCheckmark = CreateUIObject("Item Checkmark", item, typeof(ExImage));
            GameObject itemLabel = CreateUIObject("Item Label", item, typeof(ExText));

            // Sub controls.

            GameObject scrollbar = CreateScrollbar(resources);
            scrollbar.name = "Scrollbar";
            SetParentAndAlign(scrollbar, template);

            Scrollbar scrollbarScrollbar = scrollbar.GetComponent<Scrollbar>();
            scrollbarScrollbar.SetDirection(Scrollbar.Direction.BottomToTop, true);

            RectTransform vScrollbarRT = scrollbar.GetComponent<RectTransform>();
            vScrollbarRT.anchorMin = Vector2.right;
            vScrollbarRT.anchorMax = Vector2.one;
            vScrollbarRT.pivot = Vector2.one;
            vScrollbarRT.sizeDelta = new Vector2(vScrollbarRT.sizeDelta.x, 0);

            // Setup item UI components.

            ExText itemLabelText = itemLabel.GetComponent<ExText>();
            SetDefaultTextValues(itemLabelText);
            itemLabelText.alignment = TextAnchor.MiddleLeft;

            ExImage itemBackgroundImage = itemBackground.GetComponent<ExImage>();
            itemBackgroundImage.color = new Color32(245, 245, 245, 255);

            ExImage itemCheckmarkImage = itemCheckmark.GetComponent<ExImage>();
            itemCheckmarkImage.sprite = resources.checkmark;

            ExToggle itemToggle = item.GetComponent<ExToggle>();
            itemToggle.targetGraphic = itemBackgroundImage;
            itemToggle.graphic = itemCheckmarkImage;
            itemToggle.isOn = true;

            // Setup template UI components.

            ExImage templateImage = template.GetComponent<ExImage>();
            templateImage.sprite = resources.standard;
            templateImage.type = ExImage.Type.Sliced;

            ScrollRect templateScrollRect = template.GetComponent<ScrollRect>();
            templateScrollRect.content = content.GetComponent<RectTransform>();
            templateScrollRect.viewport = viewport.GetComponent<RectTransform>();
            templateScrollRect.horizontal = false;
            templateScrollRect.movementType = ScrollRect.MovementType.Clamped;
            templateScrollRect.verticalScrollbar = scrollbarScrollbar;
            templateScrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
            templateScrollRect.verticalScrollbarSpacing = -3;

            Mask scrollRectMask = viewport.GetComponent<Mask>();
            scrollRectMask.showMaskGraphic = false;

            ExImage viewportImage = viewport.GetComponent<ExImage>();
            viewportImage.sprite = resources.mask;
            viewportImage.type = ExImage.Type.Sliced;

            // Setup dropdown UI components.

            ExText labelText = label.GetComponent<ExText>();
            SetDefaultTextValues(labelText);
            labelText.alignment = TextAnchor.MiddleLeft;

            ExImage arrowImage = arrow.GetComponent<ExImage>();
            arrowImage.sprite = resources.dropdown;

            ExImage backgroundImage = root.GetComponent<ExImage>();
            backgroundImage.sprite = resources.standard;
            backgroundImage.color = s_DefaultSelectableColor;
            backgroundImage.type = ExImage.Type.Sliced;

            ExDropdown dropdown = root.GetComponent<ExDropdown>();
            dropdown.targetGraphic = backgroundImage;
            SetDefaultColorTransitionValues(dropdown);
            dropdown.template = template.GetComponent<RectTransform>();
            dropdown.captionText = labelText;
            dropdown.itemText = itemLabelText;

            // Setting default Item list.
            itemLabelText.text = "Option A";
            dropdown.options.Add(new ExDropdown.OptionData { text = "Option A" });
            dropdown.options.Add(new ExDropdown.OptionData { text = "Option B" });
            dropdown.options.Add(new ExDropdown.OptionData { text = "Option C" });
            dropdown.RefreshShownValue();

            // Set up RectTransforms.

            RectTransform labelRT = label.GetComponent<RectTransform>();
            labelRT.anchorMin = Vector2.zero;
            labelRT.anchorMax = Vector2.one;
            labelRT.offsetMin = new Vector2(10, 6);
            labelRT.offsetMax = new Vector2(-25, -7);

            RectTransform arrowRT = arrow.GetComponent<RectTransform>();
            arrowRT.anchorMin = new Vector2(1, 0.5f);
            arrowRT.anchorMax = new Vector2(1, 0.5f);
            arrowRT.sizeDelta = new Vector2(20, 20);
            arrowRT.anchoredPosition = new Vector2(-15, 0);

            RectTransform templateRT = template.GetComponent<RectTransform>();
            templateRT.anchorMin = new Vector2(0, 0);
            templateRT.anchorMax = new Vector2(1, 0);
            templateRT.pivot = new Vector2(0.5f, 1);
            templateRT.anchoredPosition = new Vector2(0, 2);
            templateRT.sizeDelta = new Vector2(0, 150);

            RectTransform viewportRT = viewport.GetComponent<RectTransform>();
            viewportRT.anchorMin = new Vector2(0, 0);
            viewportRT.anchorMax = new Vector2(1, 1);
            viewportRT.sizeDelta = new Vector2(-18, 0);
            viewportRT.pivot = new Vector2(0, 1);

            RectTransform contentRT = content.GetComponent<RectTransform>();
            contentRT.anchorMin = new Vector2(0f, 1);
            contentRT.anchorMax = new Vector2(1f, 1);
            contentRT.pivot = new Vector2(0.5f, 1);
            contentRT.anchoredPosition = new Vector2(0, 0);
            contentRT.sizeDelta = new Vector2(0, 28);

            RectTransform itemRT = item.GetComponent<RectTransform>();
            itemRT.anchorMin = new Vector2(0, 0.5f);
            itemRT.anchorMax = new Vector2(1, 0.5f);
            itemRT.sizeDelta = new Vector2(0, 20);

            RectTransform itemBackgroundRT = itemBackground.GetComponent<RectTransform>();
            itemBackgroundRT.anchorMin = Vector2.zero;
            itemBackgroundRT.anchorMax = Vector2.one;
            itemBackgroundRT.sizeDelta = Vector2.zero;

            RectTransform itemCheckmarkRT = itemCheckmark.GetComponent<RectTransform>();
            itemCheckmarkRT.anchorMin = new Vector2(0, 0.5f);
            itemCheckmarkRT.anchorMax = new Vector2(0, 0.5f);
            itemCheckmarkRT.sizeDelta = new Vector2(20, 20);
            itemCheckmarkRT.anchoredPosition = new Vector2(10, 0);

            RectTransform itemLabelRT = itemLabel.GetComponent<RectTransform>();
            itemLabelRT.anchorMin = Vector2.zero;
            itemLabelRT.anchorMax = Vector2.one;
            itemLabelRT.offsetMin = new Vector2(20, 1);
            itemLabelRT.offsetMax = new Vector2(-10, -2);

            template.SetActive(false);

            return root;
        }

        /// <summary>
        /// Create the basic UI input field.
        /// </summary>
        /// <remarks>
        /// Hierarchy:
        /// (root)
        ///     InputField
        ///         - PlaceHolder
        ///         - Text
        /// </remarks>
        /// <param name="resources">The resources to use for creation.</param>
        /// <returns>The root GameObject of the created element.</returns>
        public static GameObject CreateInputField(DefaultControls.Resources resources)
        {
            GameObject root = CreateUIElementRoot("InputField (Legacy)", s_ThickElementSize, typeof(ExImage), typeof(ExInputField));

            GameObject childPlaceholder = CreateUIObject("Placeholder", root, typeof(ExText));
            GameObject childText = CreateUIObject("Text (Legacy)", root, typeof(ExText));

            ExImage image = root.GetComponent<ExImage>();
            image.sprite = resources.inputField;
            image.type = ExImage.Type.Sliced;
            image.color = s_DefaultSelectableColor;

            ExInputField inputField = root.GetComponent<ExInputField>();
            SetDefaultColorTransitionValues(inputField);

            ExText text = childText.GetComponent<ExText>();
            text.text = "";
            text.supportRichText = false;
            SetDefaultTextValues(text);

            ExText placeholder = childPlaceholder.GetComponent<ExText>();
            placeholder.text = "Enter text...";
            placeholder.fontStyle = FontStyle.Italic;
            // Make placeholder color half as opaque as normal text color.
            Color placeholderColor = text.color;
            placeholderColor.a *= 0.5f;
            placeholder.color = placeholderColor;

            RectTransform textRectTransform = childText.GetComponent<RectTransform>();
            textRectTransform.anchorMin = Vector2.zero;
            textRectTransform.anchorMax = Vector2.one;
            textRectTransform.sizeDelta = Vector2.zero;
            textRectTransform.offsetMin = new Vector2(10, 6);
            textRectTransform.offsetMax = new Vector2(-10, -7);

            RectTransform placeholderRectTransform = childPlaceholder.GetComponent<RectTransform>();
            placeholderRectTransform.anchorMin = Vector2.zero;
            placeholderRectTransform.anchorMax = Vector2.one;
            placeholderRectTransform.sizeDelta = Vector2.zero;
            placeholderRectTransform.offsetMin = new Vector2(10, 6);
            placeholderRectTransform.offsetMax = new Vector2(-10, -7);

            inputField.textComponent = text;
            inputField.placeholder = placeholder;

            return root;
        }


        /// <summary>
        /// Create the basic UI Scrollbar.
        /// </summary>
        /// <remarks>
        /// Hierarchy:
        /// (root)
        ///     Scrollbar
        ///         - Sliding Area
        ///             - Handle
        /// </remarks>
        /// <param name="resources">The resources to use for creation.</param>
        /// <returns>The root GameObject of the created element.</returns>
        public static GameObject CreateScrollbar(DefaultControls.Resources resources)
        {
            // Create GOs Hierarchy
            GameObject scrollbarRoot = CreateUIElementRoot("Scrollbar", s_ThinElementSize, typeof(ExImage), typeof(Scrollbar));

            GameObject sliderArea = CreateUIObject("Sliding Area", scrollbarRoot, typeof(RectTransform));
            GameObject handle = CreateUIObject("Handle", sliderArea, typeof(ExImage));

            ExImage bgImage = scrollbarRoot.GetComponent<ExImage>();
            bgImage.sprite = resources.background;
            bgImage.type = ExImage.Type.Sliced;
            bgImage.color = s_DefaultSelectableColor;

            ExImage handleImage = handle.GetComponent<ExImage>();
            handleImage.sprite = resources.standard;
            handleImage.type = ExImage.Type.Sliced;
            handleImage.color = s_DefaultSelectableColor;

            RectTransform sliderAreaRect = sliderArea.GetComponent<RectTransform>();
            sliderAreaRect.sizeDelta = new Vector2(-20, -20);
            sliderAreaRect.anchorMin = Vector2.zero;
            sliderAreaRect.anchorMax = Vector2.one;

            RectTransform handleRect = handle.GetComponent<RectTransform>();
            handleRect.sizeDelta = new Vector2(20, 20);

            Scrollbar scrollbar = scrollbarRoot.GetComponent<Scrollbar>();
            scrollbar.handleRect = handleRect;
            scrollbar.targetGraphic = handleImage;
            SetDefaultColorTransitionValues(scrollbar);

            return scrollbarRoot;
        }


        private static void SetDefaultTextValues(ExText lbl)
        {
            // Set text values we want across UI elements in default controls.
            // Don't set values which are the same as the default values for the Text component,
            // since there's no point in that, and it's good to keep them as consistent as possible.
            lbl.color = s_TextColor;

            //todo 替换默认字体文件
            if (lbl.font == null)
                lbl.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

    }
}
