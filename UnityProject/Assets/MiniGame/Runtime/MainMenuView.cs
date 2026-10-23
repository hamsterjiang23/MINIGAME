using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MiniGame.UnityHost
{
    public enum MainMenuPage { Hidden, FirstStart, Menu, Settings, ConfirmNew, PixelSetup }

    /// <summary>Title-screen presentation. The controller owns sessions, input gating and menu actions.</summary>
    public sealed class MainMenuView : MonoBehaviour
    {
        private Font font;
        private RectTransform root, logoArea, firstStart, menu, confirmation;
        private Text statusText, continueLabel;
        private CanvasGroup releaseGroup, startGroup, menuGroup, confirmationGroup;
        private CanvasGroup logoGroup;
        private RawImage logoImage;
        private Image backdrop;
        private bool releaseStarted;
        public float ControlsAlpha => menuGroup != null ? menuGroup.alpha : 1;
        public float ReleaseAlpha => backdrop != null ? backdrop.color.a : 1;
        public Rect LogoScreenRect
        {
            get
            {
                var corners = new Vector3[4]; logoImage.rectTransform.GetWorldCorners(corners);
                var bottomLeft = RectTransformUtility.WorldToScreenPoint(null, corners[0]);
                var topRight = RectTransformUtility.WorldToScreenPoint(null, corners[2]);
                return new Rect(bottomLeft, topRight - bottomLeft);
            }
        }
        public MainMenuPage Page { get; private set; }
        public Canvas Canvas { get; private set; }
        public Button StartButton { get; private set; }
        public Button ContinueButton { get; private set; }
        public Button NewPeriodButton { get; private set; }
        public Button SettingsButton { get; private set; }
        public Button ConfirmButton { get; private set; }
        public Button CancelButton { get; private set; }
        public PlayerSettingsView Settings { get; private set; }
        public PixelStrengthSelectionView PixelSetup { get; private set; }
        public TerminalSignalController SignalTuning { get; private set; }

        public static MainMenuView Create(Transform parent, Action start, Action resume, Action newPeriod,
            Action settings, Action confirm, Action cancel, Action back, Action confirmPixelSetup = null)
        {
            var go = new GameObject("NORM Main Menu", typeof(RectTransform), typeof(Canvas),
                typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(MainMenuView));
            if (parent != null) go.transform.SetParent(parent, false);
            var view = go.GetComponent<MainMenuView>();
            view.Build(start, resume, newPeriod, settings, confirm, cancel, back, confirmPixelSetup);
            view.Show(MainMenuPage.Hidden, false);
            return view;
        }

        private void Build(Action start, Action resume, Action newPeriod, Action settings,
            Action confirm, Action cancel, Action back, Action confirmPixelSetup)
        {
            root = (RectTransform)transform;
            releaseGroup = gameObject.AddComponent<CanvasGroup>();
            Canvas = GetComponent<Canvas>();
            Canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            Canvas.sortingOrder = 1000;
            var scaler = GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = .5f;
            if (FindObjectOfType<EventSystem>() == null)
                new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            font = GameUiTypography.CreateFont();
            SignalTuning = gameObject.AddComponent<TerminalSignalController>();
            SignalTuning.Initialize(root, font, () => false, false);
            backdrop = gameObject.AddComponent<Image>();
            backdrop.color = TerminalView.Desk;
            backdrop.raycastTarget = true;

            logoArea = CenterRect(root, "NormLogoArea", new Vector2(.5f, .65f), 1344, 448);
            logoGroup = logoArea.gameObject.AddComponent<CanvasGroup>();
            var logoRect = CenterRect(logoArea, "NormLogo", Vector2.one * .5f, 1344, 448);
            var logo = logoRect.gameObject.AddComponent<RawImage>();
            logoImage = logo;
            logo.texture = Resources.Load<Texture2D>("MiniGame/NormLogo");
            logo.raycastTarget = false;
            SignalTuning.Attach(logo, true);
            if (logo.texture != null)
            {
                var aspect = logoRect.gameObject.AddComponent<AspectRatioFitter>();
                aspect.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
                aspect.aspectRatio = (float)logo.texture.width / logo.texture.height;
            }

            firstStart = CenterRect(root, "FirstStart", new Vector2(.5f, .27f), 680, 88);
            startGroup = firstStart.gameObject.AddComponent<CanvasGroup>();
            StartButton = ButtonAt(firstStart, "StartGame", "按任意键开始游戏", Vector2.one * .5f, 680, 88,
                MainMenuPage.FirstStart, start, out _);

            menu = CenterRect(root, "MainMenuActions", new Vector2(.5f, .27f), 1180, 88);
            menuGroup = menu.gameObject.AddComponent<CanvasGroup>();
            ContinueButton = ButtonAt(menu, "ContinueInspection", "继续检查", new Vector2(0, .5f), 356, 88,
                MainMenuPage.Menu, resume, out continueLabel);
            ContinueButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(178, 0);
            NewPeriodButton = ButtonAt(menu, "NewInspectionPeriod", "开始新的检查期", Vector2.one * .5f, 400, 88,
                MainMenuPage.Menu, newPeriod, out _);
            SettingsButton = ButtonAt(menu, "OpenSettings", "设置", new Vector2(1, .5f), 356, 88,
                MainMenuPage.Menu, settings, out _);
            SettingsButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(-178, 0);

            confirmation = CenterRect(root, "ConfirmNewPeriod", new Vector2(.5f, .25f), 1040, 280);
            confirmationGroup = confirmation.gameObject.AddComponent<CanvasGroup>();
            var confirmBackground = confirmation.gameObject.AddComponent<Image>(); confirmBackground.color = TerminalView.Desk;
            Border(confirmation, 1040, 280);
            TextAt(confirmation, "ConfirmTitle", "开始新的检查期？", GameUiTypography.ReferenceSize, 30, 24, 980, 48);
            var description = TextAt(confirmation, "ConfirmDescription",
                "当前自动继续进度将被替换。\n手动存档和设置保留。", 26, 30, 82, 980, 82);
            description.lineSpacing = 1.2f;
            ConfirmButton = ButtonAt(confirmation, "ConfirmNew", "确认", new Vector2(.31f, .18f), 300, 64,
                MainMenuPage.ConfirmNew, confirm, out _);
            CancelButton = ButtonAt(confirmation, "CancelNew", "取消", new Vector2(.69f, .18f), 300, 64,
                MainMenuPage.ConfirmNew, cancel, out _);
            Settings = PlayerSettingsView.Create(root, font, () =>
            {
                if (Page == MainMenuPage.Settings) back?.Invoke();
            });
            PixelSetup = PixelStrengthSelectionView.Create(root, font, () =>
            {
                if (Page == MainMenuPage.PixelSetup) confirmPixelSetup?.Invoke();
            });

            var statusRect = CenterRect(root, "MainMenuStatus", new Vector2(.5f, .055f), 1500, 70);
            statusText = statusRect.gameObject.AddComponent<TerminalBitmapText>();
            ConfigureText(statusText, 23); statusText.alignment = TextAnchor.MiddleCenter;
        }

        public void Show(MainMenuPage page, bool canContinue, string status = "")
        {
            releaseStarted = false;
            releaseGroup.alpha = 1; releaseGroup.interactable = true;
            logoGroup.alpha = 1; backdrop.color = TerminalView.Desk;
            startGroup.alpha = menuGroup.alpha = confirmationGroup.alpha = 1;
            logoArea.anchorMin = logoArea.anchorMax = new Vector2(.5f, .65f);
            logoArea.anchoredPosition = Vector2.zero;
            logoArea.sizeDelta = new Vector2(1344, 448);
            Page = page;
            gameObject.SetActive(page != MainMenuPage.Hidden);
            firstStart.gameObject.SetActive(page == MainMenuPage.FirstStart);
            menu.gameObject.SetActive(page == MainMenuPage.Menu);
            confirmation.gameObject.SetActive(page == MainMenuPage.ConfirmNew);
            logoArea.gameObject.SetActive(page != MainMenuPage.Settings && page != MainMenuPage.PixelSetup);
            ContinueButton.interactable = canContinue;
            continueLabel.color = canContinue ? TerminalView.Ink : TerminalView.Dim;
            Settings.Hide();
            PixelSetup.Hide();
            SetStatus(status);
            if (page == MainMenuPage.PixelSetup)
            {
                PixelSetup.Show(PlayerGraphicsSettings.Detail);
                return;
            }
            if (page == MainMenuPage.Settings)
            {
                Settings.Show("返回主菜单");
                return;
            }
            ConfigureNavigation(canContinue);
            var events = EventSystem.current;
            if (events == null) return;
            if (page == MainMenuPage.Hidden)
            {
                var selected = events.currentSelectedGameObject;
                if (selected != null && selected.transform.IsChildOf(transform)) events.SetSelectedGameObject(null);
                return;
            }
            // Showing the title is not a keyboard or pointer selection.
            events.SetSelectedGameObject(null);
        }

        public void SelectInitialForNavigation(bool reverse)
        {
            var events = EventSystem.current;
            if (releaseStarted || events == null || events.currentSelectedGameObject != null) return;
            var initial = Page == MainMenuPage.FirstStart ? StartButton
                : Page == MainMenuPage.ConfirmNew ? CancelButton
                : Page == MainMenuPage.Menu ? reverse ? SettingsButton : ContinueButton.interactable ? ContinueButton : NewPeriodButton : null;
            if (initial != null) events.SetSelectedGameObject(initial.gameObject);
        }

        public void BeginRelease()
        {
            if (releaseStarted) return;
            releaseStarted = true;
            releaseGroup.interactable = false;
            EventSystem.current?.SetSelectedGameObject(null);
        }

        public void SetReleaseProgress(float elapsed, bool monitorPullback)
        {
            startGroup.alpha = menuGroup.alpha = confirmationGroup.alpha = 1 - Mathf.SmoothStep(0, 1, elapsed / .45f);
            float reveal = Mathf.SmoothStep(0, 1, (elapsed - .45f) / .2f);
            var color = TerminalView.Desk; color.a = 1 - reveal; backdrop.color = color;
            if (!monitorPullback) logoGroup.alpha = 1 - reveal;
        }

        public void SetProjectedLogoRect(Rect screenRect)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(root, screenRect.center, null, out var local);
            logoArea.anchorMin = logoArea.anchorMax = Vector2.one * .5f;
            logoArea.anchoredPosition = local;
            logoArea.sizeDelta = screenRect.size / Mathf.Max(.01f, Canvas.scaleFactor);
        }

        public void SetLogoAlpha(float value) => logoGroup.alpha = Mathf.Clamp01(value);

        public void SetStatus(string text, bool error = false)
        {
            statusText.text = text ?? string.Empty;
            statusText.color = error ? TerminalView.Bright : TerminalView.Ink;
            statusText.gameObject.SetActive(!string.IsNullOrWhiteSpace(text));
        }

        private void ConfigureNavigation(bool canContinue)
        {
            StartButton.navigation = new Navigation { mode = Navigation.Mode.None };
            var entries = canContinue ? new[] { ContinueButton, NewPeriodButton, SettingsButton }
                : new[] { NewPeriodButton, SettingsButton };
            for (int i = 0; i < entries.Length; i++)
            {
                var previous = entries[(i - 1 + entries.Length) % entries.Length];
                var next = entries[(i + 1) % entries.Length];
                entries[i].navigation = new Navigation { mode = Navigation.Mode.Explicit,
                    selectOnLeft = previous, selectOnUp = previous, selectOnRight = next, selectOnDown = next };
            }
            ConfirmButton.navigation = new Navigation { mode = Navigation.Mode.Explicit,
                selectOnLeft = CancelButton, selectOnRight = CancelButton, selectOnUp = CancelButton, selectOnDown = CancelButton };
            CancelButton.navigation = new Navigation { mode = Navigation.Mode.Explicit,
                selectOnLeft = ConfirmButton, selectOnRight = ConfirmButton, selectOnUp = ConfirmButton, selectOnDown = ConfirmButton };
        }

        private Button ButtonAt(Transform parent, string name, string text, Vector2 anchor,
            float width, float height, MainMenuPage requiredPage, Action action, out Text label)
        {
            var rect = CenterRect(parent, name, anchor, width, height);
            var image = rect.gameObject.AddComponent<Image>(); image.color = Color.white;
            var button = rect.gameObject.AddComponent<Button>(); button.targetGraphic = image;
            var colors = button.colors;
            colors.normalColor = TerminalView.Desk;
            colors.highlightedColor = colors.selectedColor = new Color(.17f, .115f, .025f, 1);
            colors.pressedColor = new Color(.28f, .19f, .04f, 1);
            colors.disabledColor = TerminalView.Desk; colors.colorMultiplier = 1; colors.fadeDuration = .08f;
            button.colors = colors;
            Border(rect, width, height);
            label = TextAt(rect, "Label", text, GameUiTypography.ReferenceSize, 16, 0, width - 32, height);
            button.onClick.AddListener(() =>
            {
                if (Page == requiredPage && button.isActiveAndEnabled && button.interactable) action?.Invoke();
            });
            return button;
        }

        private static RectTransform CenterRect(Transform parent, string name, Vector2 anchor, float width, float height)
        {
            var go = new GameObject(name, typeof(RectTransform)); go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = anchor; rect.pivot = Vector2.one * .5f;
            rect.sizeDelta = new Vector2(width, height);
            return rect;
        }
        private Text TextAt(Transform parent, string name, string value, int size, float x, float y, float width, float height)
        {
            var rect = CenterRect(parent, name, new Vector2(0, 1), width, height);
            rect.pivot = new Vector2(0, 1); rect.anchoredPosition = new Vector2(x, -y);
            var text = rect.gameObject.AddComponent<TerminalBitmapText>(); ConfigureText(text, size);
            text.text = value; text.alignment = TextAnchor.MiddleCenter;
            return text;
        }
        private void ConfigureText(Text text, int size)
        {
            text.font = font; text.fontStyle = FontStyle.Normal; text.fontSize = size; text.color = TerminalView.Ink; text.raycastTarget = false;
            var pixels = (TerminalBitmapText)text;
            pixels.ScaleToFontSize = true; pixels.LinePitch = size * 1.3f;
            text.horizontalOverflow = HorizontalWrapMode.Wrap; text.verticalOverflow = VerticalWrapMode.Truncate;
        }
        private static void Border(Transform parent, float width, float height)
        {
            Line(parent, "TopBorder", 0, 0, width, 2);
            Line(parent, "BottomBorder", 0, height - 2, width, 2);
            Line(parent, "LeftBorder", 0, 0, 2, height);
            Line(parent, "RightBorder", width - 2, 0, 2, height);
        }
        private static void Line(Transform parent, string name, float x, float y, float width, float height)
        {
            var rect = CenterRect(parent, name, new Vector2(0, 1), width, height);
            rect.pivot = new Vector2(0, 1); rect.anchoredPosition = new Vector2(x, -y);
            var image = rect.gameObject.AddComponent<Image>(); image.color = TerminalView.Ink; image.raycastTarget = false;
        }
        private void OnDestroy()
        {
            if (font == null) return;
            if (Application.isPlaying) Destroy(font); else DestroyImmediate(font);
        }
    }
}
