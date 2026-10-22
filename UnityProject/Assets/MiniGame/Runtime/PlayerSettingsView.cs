using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MiniGame.UnityHost
{
    /// <summary>The same device settings panel is used by the title screen and the paused game.</summary>
    public sealed class PlayerSettingsView : MonoBehaviour
    {
        private static readonly Color Ink = TerminalView.Ink;
        private RectTransform root;
        private Text brightnessValue, volumeValue, returnLabel;
        private readonly Text[] detailLabels = new Text[3];
        private static readonly PixelDetail[] Details = { PixelDetail.High, PixelDetail.Medium, PixelDetail.Low };
        private static readonly string[] DetailNames = { "高", "中", "低" };
        private PixelDetail shownDetail;
        private float shownBrightness = -1, shownVolume = -1;
        public Slider BrightnessSlider { get; private set; }
        public Slider VolumeSlider { get; private set; }
        public Button[] PixelDetailButtons { get; private set; } = Array.Empty<Button>();
        public Button ReturnButton { get; private set; }

        public static PlayerSettingsView Create(Transform parent, Font font, Action onBack)
        {
            if (parent == null) throw new ArgumentNullException(nameof(parent));
            if (font == null) throw new ArgumentNullException(nameof(font));
            var go = new GameObject("PlayerSettings", typeof(RectTransform), typeof(PlayerSettingsView));
            go.transform.SetParent(parent, false);
            var view = go.GetComponent<PlayerSettingsView>();
            view.Build(font, onBack);
            go.SetActive(false);
            return view;
        }

        private void Build(Font font, Action onBack)
        {
            root = (RectTransform)transform;
            root.anchorMin = root.anchorMax = root.pivot = Vector2.one * .5f;
            root.sizeDelta = new Vector2(740, 600);
            var panel = gameObject.AddComponent<Image>();
            var desk = TerminalView.Desk;
            panel.color = new Color(desk.r, desk.g, desk.b, .97f);
            Border(root, 740, 600);
            TextAt(root, font, "SettingsTitle", "NORM  /  游戏设置", GameUiTypography.ReferenceSize, 38, 28, 664, 56).color = TerminalView.Bright;
            Line(root, "TitleRule", 38, 106, 664, 1, TerminalView.Edge);

            TextAt(root, font, "BrightnessHeading", "亮度", 26, 38, 125, 420, 40);
            brightnessValue = TextAt(root, font, "BrightnessValue", "100%", 24, 528, 125, 174, 40);
            brightnessValue.alignment = TextAnchor.MiddleRight;
            BrightnessSlider = MakeSlider("Brightness", 170, .5f, 1.5f, PlayerPreferences.SetBrightness);
            TextAt(root, font, "VolumeHeading", "音量", 26, 38, 231, 420, 40);
            volumeValue = TextAt(root, font, "VolumeValue", "100%", 24, 528, 231, 174, 40);
            volumeValue.alignment = TextAnchor.MiddleRight;
            VolumeSlider = MakeSlider("Volume", 276, 0, 1, PlayerPreferences.SetMasterVolume);
            TextAt(root, font, "PixelDetailHeading", "复古效果", 26, 38, 337, 664, 40);
            PixelDetailButtons = new Button[3];
            for (int i = 0; i < PixelDetailButtons.Length; i++)
            {
                int index = i;
                PixelDetailButtons[i] = MakeButton(font, "PixelDetail" + Details[i], 38 + i * 228, 389, 208,
                    DetailNames[i], string.Empty, () =>
                    {
                        if (!isActiveAndEnabled) return;
                        PlayerGraphicsSettings.SetDetail(Details[index]);
                        Refresh();
                        EventSystem.current?.SetSelectedGameObject(PixelDetailButtons[index].gameObject);
                    }, out detailLabels[i]);
            }
            TextAt(root, font, "SettingsSaveHint", "即时生效，自动保存", 21, 38, 462, 664, 34);
            ReturnButton = MakeButton(font, "SettingsReturn", 38, 506, 664, "返回", "ESC", () =>
            {
                if (isActiveAndEnabled) onBack?.Invoke();
            }, out returnLabel);
            ConfigureNavigation();
            Refresh();
        }

        public void Show(string returnText)
        {
            returnLabel.text = string.IsNullOrEmpty(returnText) ? "返回" : returnText;
            gameObject.SetActive(true);
            Refresh();
            EventSystem.current?.SetSelectedGameObject(BrightnessSlider.gameObject);
        }

        public void Hide()
        {
            var selected = EventSystem.current?.currentSelectedGameObject;
            if (selected != null && selected.transform.IsChildOf(transform)) EventSystem.current.SetSelectedGameObject(null);
            gameObject.SetActive(false);
        }

        public void Refresh()
        {
            if (BrightnessSlider == null || VolumeSlider == null || PixelDetailButtons.Length != 3) return;
            shownBrightness = PlayerPreferences.Brightness;
            shownVolume = PlayerPreferences.MasterVolume;
            shownDetail = PlayerGraphicsSettings.Detail;
            BrightnessSlider.SetValueWithoutNotify(shownBrightness);
            VolumeSlider.SetValueWithoutNotify(shownVolume);
            brightnessValue.text = Mathf.RoundToInt(shownBrightness * 100) + "%";
            volumeValue.text = Mathf.RoundToInt(shownVolume * 100) + "%";
            for (int i = 0; i < detailLabels.Length; i++)
                detailLabels[i].text = DetailNames[i] + (shownDetail == Details[i] ? " · 已选择" : string.Empty);
        }

        private void OnEnable() { PlayerPreferences.Changed += Refresh; Refresh(); }
        private void OnDisable() => PlayerPreferences.Changed -= Refresh;
        private void Update()
        {
            if (shownDetail != PlayerGraphicsSettings.Detail || shownBrightness != PlayerPreferences.Brightness
                || shownVolume != PlayerPreferences.MasterVolume) Refresh();
        }

        private void ConfigureNavigation()
        {
            BrightnessSlider.navigation = new Navigation { mode = Navigation.Mode.Explicit,
                selectOnUp = ReturnButton, selectOnDown = VolumeSlider };
            VolumeSlider.navigation = new Navigation { mode = Navigation.Mode.Explicit,
                selectOnUp = BrightnessSlider, selectOnDown = PixelDetailButtons[0] };
            for (int i = 0; i < PixelDetailButtons.Length; i++)
                PixelDetailButtons[i].navigation = new Navigation { mode = Navigation.Mode.Explicit,
                    selectOnUp = VolumeSlider, selectOnDown = ReturnButton,
                    selectOnLeft = i > 0 ? PixelDetailButtons[i - 1] : null,
                    selectOnRight = i + 1 < PixelDetailButtons.Length ? PixelDetailButtons[i + 1] : null };
            ReturnButton.navigation = new Navigation { mode = Navigation.Mode.Explicit,
                selectOnUp = PixelDetailButtons[0], selectOnDown = BrightnessSlider };
        }

        private Slider MakeSlider(string name, float y, float min, float max, Action<float> onChange)
        {
            var rect = RectAt(root, name + "Slider", 38, y, 664, 44);
            rect.gameObject.AddComponent<Image>().color = Color.clear;
            var slider = rect.gameObject.AddComponent<Slider>();
            slider.minValue = min; slider.maxValue = max;
            Line(rect, "Track", 8, 20, 648, 4, TerminalView.Edge);
            var fillArea = RectAt(rect, "FillArea", 8, 20, 648, 4);
            var fill = RectAt(fillArea, "Fill", 0, 0, 0, 0);
            Stretch(fill);
            fill.gameObject.AddComponent<Image>().color = Ink;
            var handleArea = RectAt(rect, "HandleArea", 8, 0, 648, 44);
            var handle = RectAt(handleArea, "Handle", 0, 0, 16, 32);
            handle.anchorMin = handle.anchorMax = handle.pivot = Vector2.one * .5f;
            var handleImage = handle.gameObject.AddComponent<Image>();
            handleImage.color = Ink;
            slider.fillRect = fill; slider.handleRect = handle; slider.targetGraphic = handleImage;
            var colors = slider.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = colors.selectedColor = TerminalView.Bright / Ink.maxColorComponent;
            colors.pressedColor = new Color(.8f, .8f, .8f, 1);
            colors.colorMultiplier = 1;
            slider.colors = colors;
            slider.onValueChanged.AddListener(value => { if (isActiveAndEnabled) onChange(value); });
            return slider;
        }

        private Button MakeButton(Font font, string name, float x, float y, float width,
            string value, string hint, Action action, out Text label)
        {
            var row = RectAt(root, name, x, y, width, 56);
            var hit = row.gameObject.AddComponent<Image>(); hit.color = Color.clear;
            Border(row, width, 56);
            var button = row.gameObject.AddComponent<PauseMenuButton>(); button.targetGraphic = hit;
            var frame = RectAt(row, "FocusFrame", 0, 0, width, 56);
            var fill = frame.gameObject.AddComponent<Image>(); fill.color = Ink; fill.raycastTarget = false;
            var focus = frame.gameObject.AddComponent<CanvasGroup>();
            focus.alpha = 0; focus.blocksRaycasts = false; focus.interactable = false;
            var mark = RectAt(row, "SelectionMark", 10, 19, 3, 18);
            var marker = mark.gameObject.AddComponent<Image>(); marker.color = Color.clear; marker.raycastTarget = false;
            label = TextAt(row, font, "Label", value, GameUiTypography.ReferenceSize, 24, 0, width - 40, 56);
            label.alignment = TextAnchor.MiddleLeft;
            var shortcut = TextAt(row, font, "Shortcut", hint, 20, width - 160, 0, 136, 56);
            shortcut.alignment = TextAnchor.MiddleRight;
            button.ConfigureVisuals(label, shortcut, focus, marker, Ink);
            button.onClick.AddListener(() => action());
            return button;
        }

        private static RectTransform RectAt(Transform parent, string name, float x, float y, float width, float height)
        {
            var go = new GameObject(name, typeof(RectTransform)); go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(x, -y); rect.sizeDelta = new Vector2(width, height);
            return rect;
        }
        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }
        private static Text TextAt(Transform parent, Font font, string name, string text, int size,
            float x, float y, float width, float height)
        {
            var label = RectAt(parent, name, x, y, width, height).gameObject.AddComponent<TerminalBitmapText>();
            label.ScaleToFontSize = true; label.LinePitch = size * 1.3f;
            label.font = font; label.fontStyle = FontStyle.Normal; label.fontSize = size; label.text = text; label.color = Ink;
            label.alignment = TextAnchor.MiddleLeft; label.raycastTarget = false;
            label.horizontalOverflow = HorizontalWrapMode.Wrap; label.verticalOverflow = VerticalWrapMode.Truncate;
            return label;
        }
        private static void Line(Transform parent, string name, float x, float y, float width, float height, Color color)
        {
            var image = RectAt(parent, name, x, y, width, height).gameObject.AddComponent<Image>();
            image.color = color; image.raycastTarget = false;
        }
        private static void Border(Transform parent, float width, float height)
        {
            Line(parent, "TopBorder", 0, 0, width, 1, Ink);
            Line(parent, "BottomBorder", 0, height - 1, width, 1, Ink);
            Line(parent, "LeftBorder", 0, 0, 1, height, Ink);
            Line(parent, "RightBorder", width - 1, 0, 1, height, Ink);
        }
    }
}
