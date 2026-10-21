using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MiniGame.UnityHost
{
    /// <summary>Live presentation controls. Gameplay input policy belongs to the controller.</summary>
    public sealed class TerminalSignalTuningPanel : MonoBehaviour
    {
        private static readonly Color Paper = new Color(.91f, .88f, .78f, 1);
        private static readonly Color Amber = new Color(.90f, .64f, .29f, 1);
        private static readonly Color Quiet = new Color(.58f, .59f, .55f, 1);
        private static readonly Color Surface = new Color(.12f, .14f, .13f, 1);
        private readonly List<ParameterRow> rows = new List<ParameterRow>();
        private TerminalSignalController controller;
        private RectTransform root, panel;
        private Toggle enabledToggle, freezeToggle;
        private Text status, groupDescription;
        private Image logoTab, monitorTab;
        private ScrollRect scroll;
        private Material overlayMaterial;
        private bool logo = true, refreshing;

        private sealed class ParameterRow
        {
            public Slider Slider;
            public Text Value;
            public Func<TerminalSignalParameters, float> Read;
            public Action<TerminalSignalParameters, float> Write;
            public Func<float, string> Format;
        }

        public static TerminalSignalTuningPanel Create(RectTransform parent, Font font,
            TerminalSignalController controller)
        {
            if (parent == null) throw new ArgumentNullException(nameof(parent));
            if (font == null) throw new ArgumentNullException(nameof(font));
            if (controller == null) throw new ArgumentNullException(nameof(controller));
            var go = new GameObject("TerminalSignalTuningPanel", typeof(RectTransform),
                typeof(Canvas), typeof(GraphicRaycaster), typeof(CanvasRenderer),
                typeof(Image), typeof(TerminalSignalTuningPanel));
            go.transform.SetParent(parent, false);
            // Keep the modal above the terminal's custom Overlay-queue composite.
            var parentCanvas = parent.GetComponentInParent<Canvas>();
            var canvas = go.GetComponent<Canvas>();
            canvas.overrideSorting = true;
            canvas.sortingLayerID = parentCanvas != null ? parentCanvas.sortingLayerID : 0;
            canvas.sortingOrder = (parentCanvas != null ? parentCanvas.sortingOrder : 10) + 100;
            var view = go.GetComponent<TerminalSignalTuningPanel>();
            view.controller = controller;
            view.Build(font);
            return view;
        }

        public void SetVisible(bool value)
        {
            gameObject.SetActive(value);
            if (!value) return;
            transform.SetAsLastSibling();
            LayoutPanel();
            Refresh();
        }

        public void Refresh()
        {
            if (controller == null || controller.Profile == null || enabledToggle == null) return;
            refreshing = true;
            try
            {
                var settings = SelectedSettings();
                enabledToggle.SetIsOnWithoutNotify(settings.Enabled);
                freezeToggle.SetIsOnWithoutNotify(controller.FreezePreview);
                foreach (var row in rows)
                {
                    float value = row.Read(settings);
                    row.Slider.SetValueWithoutNotify(value);
                    row.Value.text = row.Format(value);
                }
                logoTab.color = logo ? Amber : Surface;
                monitorTab.color = logo ? Surface : Amber;
                logoTab.GetComponentInChildren<Text>().color = logo ? Surface : Paper;
                monitorTab.GetComponentInChildren<Text>().color = logo ? Paper : Surface;
                groupDescription.text = logo ? "标志局部错位 · 色差 · 间歇闪动" : "扫描线 · 滚动干扰 · 信号噪声";
            }
            finally { refreshing = false; }
        }

        private TerminalSignalParameters SelectedSettings() =>
            logo ? controller.Profile.Logo : controller.Profile.Monitor;

        private void Build(Font font)
        {
            root = (RectTransform)transform;
            Stretch(root);
            // Even fully transparent, this graphic catches clicks outside the panel.
            var blocker = GetComponent<Image>();
            blocker.color = Color.clear;
            blocker.raycastTarget = true;
            panel = NewRect(root, "SignalControls");
            panel.anchorMin = panel.anchorMax = new Vector2(1, .5f);
            panel.pivot = new Vector2(1, .5f);
            panel.anchoredPosition = new Vector2(-24, 0);
            Image background = AddImage(panel, new Color(.045f, .057f, .052f, .98f));
            background.raycastTarget = true;
            var accent = NewRect(panel, "Accent");
            accent.anchorMin = new Vector2(0, 0);
            accent.anchorMax = new Vector2(0, 1);
            accent.pivot = new Vector2(0, .5f);
            accent.sizeDelta = new Vector2(2, 0);
            AddImage(accent, Amber).raycastTarget = false;

            var title = MakeText(panel, font, "Title", "画面扰动", 25, Paper);
            Top(title.rectTransform, 22, 19, 220, 36);
            var hint = MakeText(panel, font, "ShortcutHint", "F6 / Esc 关闭", 13, Quiet);
            Top(hint.rectTransform, 22, 57, 210, 22);
            var close = MakeButton(panel, font, "Close", "关闭 ×", () => controller.SetPanelOpen(false));
            close.GetComponent<RectTransform>().anchorMin = close.GetComponent<RectTransform>().anchorMax = Vector2.one;
            close.GetComponent<RectTransform>().pivot = Vector2.one;
            close.GetComponent<RectTransform>().anchoredPosition = new Vector2(-18, -22);
            close.GetComponent<RectTransform>().sizeDelta = new Vector2(78, 32);

            var logoButton = MakeButton(panel, font, "LogoTab", "Norm 标志", () => SelectGroup(true));
            var monitorButton = MakeButton(panel, font, "MonitorTab", "监控画面", () => SelectGroup(false));
            HalfRow((RectTransform)logoButton.transform, true, 94, 38);
            HalfRow((RectTransform)monitorButton.transform, false, 94, 38);
            logoTab = logoButton.GetComponent<Image>();
            monitorTab = monitorButton.GetComponent<Image>();

            enabledToggle = MakeToggle(panel, font, "Enabled", "启用扰动", value =>
            {
                if (refreshing) return;
                SelectedSettings().Enabled = value;
                SettingsChanged();
            });
            HalfRow((RectTransform)enabledToggle.transform, true, 147, 30);
            freezeToggle = MakeToggle(panel, font, "FreezePreview", "冻结扰动", value =>
            {
                if (!refreshing) controller.FreezePreview = value;
            });
            HalfRow((RectTransform)freezeToggle.transform, false, 147, 30);

            groupDescription = MakeText(panel, font, "GroupDescription", string.Empty, 13, Quiet);
            groupDescription.rectTransform.anchorMin = new Vector2(0, 1);
            groupDescription.rectTransform.anchorMax = Vector2.one;
            groupDescription.rectTransform.pivot = new Vector2(.5f, 1);
            groupDescription.rectTransform.offsetMin = new Vector2(22, -210);
            groupDescription.rectTransform.offsetMax = new Vector2(-22, -186);

            var viewport = NewRect(panel, "ParameterViewport");
            viewport.anchorMin = Vector2.zero;
            viewport.anchorMax = Vector2.one;
            viewport.offsetMin = new Vector2(22, 106);
            viewport.offsetMax = new Vector2(-22, -220);
            AddImage(viewport, Color.clear).raycastTarget = true;
            viewport.gameObject.AddComponent<RectMask2D>();
            scroll = viewport.gameObject.AddComponent<ScrollRect>();
            scroll.viewport = viewport;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 34;
            scroll.inertia = true;
            var content = NewRect(viewport, "Parameters");
            content.anchorMin = new Vector2(0, 1);
            content.anchorMax = Vector2.one;
            content.pivot = new Vector2(.5f, 1);
            content.anchoredPosition = Vector2.zero;
            content.sizeDelta = new Vector2(-10, 12 * 69);
            scroll.content = content;
            AddRow(content, font, "总强度", 0, 1, p => p.Intensity, (p, v) => p.Intensity = v, Percent);
            AddRow(content, font, "横向撕裂", 0, .12f, p => p.Tear, (p, v) => p.Tear = v, WidthPercent);
            AddRow(content, font, "RGB 分离", 0, .03f, p => p.RgbSplit, (p, v) => p.RgbSplit = v, WidthPercent);
            AddRow(content, font, "信号噪声", 0, .25f, p => p.Noise, (p, v) => p.Noise = v, Percent);
            AddRow(content, font, "扫描线强度", 0, .8f, p => p.ScanlineStrength,
                (p, v) => p.ScanlineStrength = v, Percent);
            AddRow(content, font, "扫描线密度", 20, 600, p => p.ScanlineDensity,
                (p, v) => p.ScanlineDensity = v, v => v.ToString("0") + " 条", true);
            AddRow(content, font, "扫描线速度", 0, 10, p => p.ScanlineSpeed,
                (p, v) => p.ScanlineSpeed = v, v => v.ToString("0.00") + " 周期/秒");
            AddRow(content, font, "滚动干扰强度", 0, .15f, p => p.RollStrength,
                (p, v) => p.RollStrength = v, Percent);
            AddRow(content, font, "滚动干扰速度", 0, 3, p => p.RollSpeed,
                (p, v) => p.RollSpeed = v, v => v.ToString("0.00") + " 屏/秒");
            AddRow(content, font, "随机刷新频率", 1, 30, p => p.Rate,
                (p, v) => p.Rate = v, v => v.ToString("0.0") + " 次/秒");
            AddRow(content, font, "突发频率", .1f, 10, p => p.BurstFrequency,
                (p, v) => p.BurstFrequency = v, v => v.ToString("0.00") + " 次/秒");
            AddRow(content, font, "突发时长", .02f, .8f, p => p.BurstDuration,
                (p, v) => p.BurstDuration = v, v => v.ToString("0.00") + " 秒");
            CreateScrollbar(viewport);

            status = MakeText(panel, font, "Status", "拖动实时生效；保存后保留两组参数。", 13, Quiet);
            status.rectTransform.anchorMin = Vector2.zero;
            status.rectTransform.anchorMax = new Vector2(1, 0);
            status.rectTransform.pivot = new Vector2(.5f, 0);
            status.rectTransform.offsetMin = new Vector2(22, 61);
            status.rectTransform.offsetMax = new Vector2(-22, 98);
            var save = MakeButton(panel, font, "SaveSettings", "保存两组参数", Save);
            var reset = MakeButton(panel, font, "ResetGroup", "重置当前组", ResetSelectedGroup);
            BottomHalfRow((RectTransform)save.transform, true);
            BottomHalfRow((RectTransform)reset.transform, false);
            // Canvas depth alone cannot outrank the terminal's Overlay queue in camera captures.
            // Start from UI/Default so font textures and UI clipping/stencil variants remain supported.
            overlayMaterial = new Material(Graphic.defaultGraphicMaterial)
            {
                name = "Terminal signal tuning overlay",
                hideFlags = HideFlags.HideAndDontSave,
                renderQueue = 4100
            };
            foreach (var graphic in GetComponentsInChildren<Graphic>(true))
                graphic.material = overlayMaterial;
            LayoutPanel();
            Refresh();
            gameObject.SetActive(false);
        }

        private void SelectGroup(bool selectLogo)
        {
            logo = selectLogo;
            scroll.StopMovement();
            scroll.verticalNormalizedPosition = 1;
            Refresh();
        }

        private void SettingsChanged()
        {
            controller.ApplySettings();
            status.text = "已实时预览；点击保存保留两组参数。";
            status.color = Quiet;
        }

        private void Save()
        {
            bool saved = controller.SaveSettings();
            status.text = saved ? "已保存，两组参数下次启动继续生效。" : "保存失败，请查看 Console 中的提示。";
            status.color = saved ? Amber : new Color(.92f, .43f, .34f, 1);
        }

        private void ResetSelectedGroup()
        {
            controller.ResetGroup(logo);
            Refresh();
            status.text = (logo ? "Norm 标志" : "监控画面") + "已恢复默认；保存后保留。";
            status.color = Amber;
        }

        private void AddRow(RectTransform parent, Font font, string label, float min, float max,
            Func<TerminalSignalParameters, float> read, Action<TerminalSignalParameters, float> write,
            Func<float, string> format, bool wholeNumbers = false)
        {
            var rect = NewRect(parent, label);
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(.5f, 1);
            rect.offsetMin = new Vector2(0, -rows.Count * 69 - 64);
            rect.offsetMax = new Vector2(0, -rows.Count * 69);
            var nameText = MakeText(rect, font, "Label", label, 16, Paper);
            nameText.rectTransform.anchorMin = new Vector2(0, .5f);
            nameText.rectTransform.anchorMax = new Vector2(.58f, 1);
            nameText.rectTransform.offsetMin = Vector2.zero;
            nameText.rectTransform.offsetMax = Vector2.zero;
            var valueText = MakeText(rect, font, "Value", string.Empty, 14, Amber);
            valueText.alignment = TextAnchor.UpperRight;
            valueText.rectTransform.anchorMin = new Vector2(.50f, .5f);
            valueText.rectTransform.anchorMax = Vector2.one;
            valueText.rectTransform.offsetMin = Vector2.zero;
            valueText.rectTransform.offsetMax = Vector2.zero;
            var track = NewRect(rect, "Slider");
            track.anchorMin = new Vector2(0, 0);
            track.anchorMax = new Vector2(1, 0);
            track.pivot = new Vector2(.5f, 0);
            track.offsetMin = new Vector2(0, 5);
            track.offsetMax = new Vector2(0, 33);
            AddImage(track, Color.clear).raycastTarget = true;
            var groove = NewRect(track, "Groove");
            groove.anchorMin = new Vector2(0, .42f);
            groove.anchorMax = new Vector2(1, .58f);
            groove.offsetMin = groove.offsetMax = Vector2.zero;
            AddImage(groove, Surface).raycastTarget = false;
            var fillArea = NewRect(track, "FillArea");
            Stretch(fillArea);
            fillArea.offsetMin = new Vector2(7, 0);
            fillArea.offsetMax = new Vector2(-7, 0);
            var fill = NewRect(fillArea, "Fill");
            fill.anchorMin = new Vector2(0, .42f);
            fill.anchorMax = new Vector2(1, .58f);
            fill.offsetMin = fill.offsetMax = Vector2.zero;
            AddImage(fill, new Color(.60f, .43f, .22f, 1)).raycastTarget = false;
            var handleArea = NewRect(track, "HandleArea");
            Stretch(handleArea);
            handleArea.offsetMin = new Vector2(7, 0);
            handleArea.offsetMax = new Vector2(-7, 0);
            var handle = NewRect(handleArea, "Handle");
            handle.anchorMin = new Vector2(.5f, .15f);
            handle.anchorMax = new Vector2(.5f, .85f);
            handle.sizeDelta = new Vector2(14, 0);
            var handleImage = AddImage(handle, Amber);
            var slider = track.gameObject.AddComponent<Slider>();
            slider.fillRect = fill;
            slider.handleRect = handle;
            slider.targetGraphic = handleImage;
            slider.direction = Slider.Direction.LeftToRight;
            slider.navigation = new Navigation { mode = Navigation.Mode.None };
            slider.minValue = min;
            slider.maxValue = max;
            slider.wholeNumbers = wholeNumbers;
            var row = new ParameterRow { Slider = slider, Value = valueText, Read = read, Write = write, Format = format };
            rows.Add(row);
            slider.onValueChanged.AddListener(value =>
            {
                if (refreshing) return;
                row.Write(SelectedSettings(), value);
                row.Value.text = row.Format(value);
                SettingsChanged();
            });
        }

        private void CreateScrollbar(RectTransform viewport)
        {
            var bar = NewRect(viewport, "Scrollbar");
            bar.anchorMin = new Vector2(1, 0);
            bar.anchorMax = Vector2.one;
            bar.pivot = new Vector2(1, .5f);
            bar.offsetMin = new Vector2(-5, 0);
            bar.offsetMax = Vector2.zero;
            AddImage(bar, Surface);
            var handle = NewRect(bar, "Handle");
            Stretch(handle);
            var graphic = AddImage(handle, Quiet);
            var scrollbar = bar.gameObject.AddComponent<Scrollbar>();
            scrollbar.direction = Scrollbar.Direction.BottomToTop;
            scrollbar.navigation = new Navigation { mode = Navigation.Mode.None };
            scrollbar.handleRect = handle;
            scrollbar.targetGraphic = graphic;
            scroll.verticalScrollbar = scrollbar;
            scroll.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;
        }

        private void OnRectTransformDimensionsChange() => LayoutPanel();

        private void OnDestroy()
        {
            if (overlayMaterial == null) return;
            foreach (var graphic in GetComponentsInChildren<Graphic>(true))
                if (graphic.material == overlayMaterial) graphic.material = null;
            // MaskableGraphic owns any stencil clones produced from this base material.
            Destroy(overlayMaterial);
        }

        private void LayoutPanel()
        {
            if (root == null || panel == null) return;
            float width = root.rect.width > 0 ? root.rect.width : 1920;
            float height = root.rect.height > 0 ? root.rect.height : 1080;
            float margin = Mathf.Min(24, width * .035f);
            float availableWidth = Mathf.Max(1, width - margin * 2);
            float availableHeight = Mathf.Max(1, height - margin * 2);
            // Keep a usable scroll area even when a canvas is shorter than the fixed header/footer.
            float scale = Mathf.Min(1, availableWidth / 320, availableHeight / 480);
            panel.anchoredPosition = new Vector2(-margin, 0);
            panel.localScale = Vector3.one * scale;
            panel.sizeDelta = new Vector2(Mathf.Min(430, availableWidth / scale), Mathf.Min(850, availableHeight / scale));
        }

        private static string Percent(float value) => (value * 100).ToString("0") + "%";
        private static string WidthPercent(float value) => (value * 100).ToString("0.00") + "% 宽度";

        private static Toggle MakeToggle(Transform parent, Font font, string name, string label, Action<bool> changed)
        {
            var rect = NewRect(parent, name);
            AddImage(rect, Color.clear);
            var box = NewRect(rect, "Box");
            Top(box, 0, 3, 23, 23);
            var background = AddImage(box, Surface);
            var check = NewRect(box, "Check");
            Stretch(check);
            check.offsetMin = new Vector2(5, 5);
            check.offsetMax = new Vector2(-5, -5);
            var checkImage = AddImage(check, Amber);
            checkImage.raycastTarget = false;
            var text = MakeText(rect, font, "Label", label, 16, Paper);
            Stretch(text.rectTransform);
            text.rectTransform.offsetMin = new Vector2(32, 0);
            text.alignment = TextAnchor.MiddleLeft;
            var toggle = rect.gameObject.AddComponent<Toggle>();
            toggle.targetGraphic = background;
            toggle.graphic = checkImage;
            toggle.navigation = new Navigation { mode = Navigation.Mode.None };
            toggle.onValueChanged.AddListener(value => changed(value));
            return toggle;
        }

        private static Button MakeButton(Transform parent, Font font, string name, string label, Action action)
        {
            var rect = NewRect(parent, name);
            var image = AddImage(rect, Surface);
            var button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.navigation = new Navigation { mode = Navigation.Mode.None };
            var text = MakeText(rect, font, "Label", label, 16, Paper);
            Stretch(text.rectTransform);
            text.alignment = TextAnchor.MiddleCenter;
            button.onClick.AddListener(() => action());
            return button;
        }

        private static Text MakeText(Transform parent, Font font, string name, string value, int size, Color color)
        {
            var rect = NewRect(parent, name);
            var text = rect.gameObject.AddComponent<Text>();
            text.font = font;
            text.fontSize = size;
            text.color = color;
            text.text = value;
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            return text;
        }

        private static RectTransform NewRect(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer));
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        private static Image AddImage(RectTransform rect, Color color)
        {
            var image = rect.gameObject.AddComponent<Image>();
            image.color = color;
            return image;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }

        private static void Top(RectTransform rect, float x, float y, float width, float height)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(x, -y);
            rect.sizeDelta = new Vector2(width, height);
        }

        private static void HalfRow(RectTransform rect, bool left, float y, float height)
        {
            rect.anchorMin = new Vector2(left ? 0 : .5f, 1);
            rect.anchorMax = new Vector2(left ? .5f : 1, 1);
            rect.pivot = new Vector2(.5f, 1);
            rect.offsetMin = new Vector2(left ? 22 : 5, -y - height);
            rect.offsetMax = new Vector2(left ? -5 : -22, -y);
        }

        private static void BottomHalfRow(RectTransform rect, bool left)
        {
            rect.anchorMin = new Vector2(left ? 0 : .5f, 0);
            rect.anchorMax = new Vector2(left ? .5f : 1, 0);
            rect.offsetMin = new Vector2(left ? 22 : 5, 20);
            rect.offsetMax = new Vector2(left ? -5 : -22, 54);
        }
    }
}
