using System;
using MiniGame.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MiniGame.UnityHost
{
    /// <summary>Shared scene-backed pause presentation. Runtime owners provide actions and input policy.</summary>
    public sealed class PauseMenuView : MonoBehaviour
    {
        private static readonly Color Ink = Color.white;
        private static readonly Color Bright = Color.white;
        private static readonly Color Desk = Color.black;
        private static readonly Color Edge = new Color(1, 1, 1, .35f);
        private const float ColumnWidth = 512, RowHeight = 80, RowStride = 96;
        private const float FadeSeconds = .12f;
        private CanvasGroup group;
        private RectTransform root, column, manual;
        private RectTransform settings;
        private RectTransform gm;
        private Action<EndingType> gmAction;
        private GameObject gmPreviousSelection;
        private bool gmChordConsumed;
        private CanvasGroup columnInput;
        private Button manualReturn;
        private GameObject settingsPreviousSelection;
        private Text status;
        private PauseMenuBackdrop backdrop;
        private GameObject previousSelection, manualPreviousSelection;
        private bool visible;
        private float columnHeight;
        public const int HiddenEndingClickCount = 4;
        public const float HiddenEndingClickInterval = 1.25f;
        private Button hiddenEndingButton;
        private Action hiddenEndingAction;
        private int hiddenEndingClicks, hiddenEndingClickFrame = -1;
        private float hiddenEndingLastClick;
        private bool hiddenEndingTriggered;
        private Sprite stripSprite, focusSprite;
        private Texture2D stripTexture, focusTexture;
        private Material lettering;
        public Button[] Buttons { get; private set; } = Array.Empty<Button>();
        public bool ManualVisible => visible && manual != null && manual.gameObject.activeSelf;
        public bool SettingsVisible => visible && settings != null && settings.gameObject.activeSelf;
        public bool GmVisible => visible && gm != null && gm.gameObject.activeSelf;
        public Button[] GmEndingButtons { get; private set; } = Array.Empty<Button>();
        private bool SubmenuVisible => ManualVisible || SettingsVisible || GmVisible;
        public PlayerSettingsView SharedSettings { get; private set; }
        public Button[] PixelDetailButtons => SharedSettings != null ? SharedSettings.PixelDetailButtons : Array.Empty<Button>();

        public static PauseMenuView Create(Transform parent, Font font, string name,
            string[] labels, string[] shortcuts, Action[] actions)
        {
            if (parent == null) throw new ArgumentNullException(nameof(parent));
            if (font == null) throw new ArgumentNullException(nameof(font));
            if (labels == null || actions == null || labels.Length != actions.Length || labels.Length == 0)
                throw new ArgumentException("Pause labels and actions must have matching, non-empty lengths.");
            var go = new GameObject(string.IsNullOrEmpty(name) ? "PauseMenu" : name,
                typeof(RectTransform), typeof(CanvasGroup), typeof(PauseMenuView));
            go.transform.SetParent(parent, false);
            var parentCanvas = parent.GetComponentInParent<Canvas>();
            if (parentCanvas == null || parentCanvas.rootCanvas.sortingOrder < 950)
            {
                var overlay = go.AddComponent<Canvas>();
                overlay.overrideSorting = true;
                overlay.sortingOrder = 950;
                go.AddComponent<GraphicRaycaster>();
            }
            var view = go.GetComponent<PauseMenuView>();
            view.Build(font, labels, shortcuts ?? Array.Empty<string>(), actions);
            return view;
        }

        private void Build(Font font, string[] labels, string[] shortcuts, Action[] actions)
        {
            root = (RectTransform)transform;
            Stretch(root);
            group = GetComponent<CanvasGroup>();
            group.alpha = 0; group.interactable = false; group.blocksRaycasts = false;
            backdrop = gameObject.AddComponent<PauseMenuBackdrop>();
            backdrop.Configure(root, group);
            CreateTextures();
            var background = gameObject.AddComponent<Image>();
            background.color = Color.clear;
            background.raycastTarget = true;
            var shading = NewRect(root, "PauseShading");
            Stretch(shading);
            var shadingImage = shading.gameObject.AddComponent<Image>();
            shadingImage.color = new Color(0, 0, 0, .22f);
            shadingImage.raycastTarget = false;

            column = NewRect(root, "PauseColumn");
            column.anchorMin = column.anchorMax = new Vector2(.06f, .5f);
            column.pivot = new Vector2(0, .5f);
            columnHeight = 100 + labels.Length * RowStride - (RowStride - RowHeight);
            column.sizeDelta = new Vector2(ColumnWidth, columnHeight);
            columnInput = column.gameObject.AddComponent<CanvasGroup>();
            var title = MakeText(column, font, "PauseTitle", 46, 20, 0, ColumnWidth - 40, 68);
            title.text = "暂停";
            title.color = Bright;
            hiddenEndingButton = title.gameObject.AddComponent<Button>();
            hiddenEndingButton.targetGraphic = title;
            hiddenEndingButton.transition = Selectable.Transition.None;
            hiddenEndingButton.navigation = new Navigation { mode = Navigation.Mode.None };
            hiddenEndingButton.interactable = false;
            hiddenEndingButton.onClick.AddListener(ClickHiddenEnding);

            Buttons = new Button[labels.Length];
            for (int i = 0; i < labels.Length; i++)
            {
                var row = NewRect(column, "PauseAction" + i);
                TopRect(row, 0, 100 + i * RowStride, ColumnWidth, RowHeight);
                var hit = row.gameObject.AddComponent<Image>();
                hit.sprite = stripSprite;
                hit.color = Color.white;
                var button = row.gameObject.AddComponent<PauseMenuButton>();
                button.targetGraphic = hit;
                var outline = NewRect(row, "FocusFrame");
                Stretch(outline);
                var outlineImage = outline.gameObject.AddComponent<Image>();
                outlineImage.sprite = focusSprite;
                outlineImage.color = Ink;
                outlineImage.raycastTarget = false;
                var focus = outline.gameObject.AddComponent<CanvasGroup>();
                focus.alpha = 0; focus.blocksRaycasts = false; focus.interactable = false;
                var label = MakeText(row, font, "Label", 40, 32, 0, 380, RowHeight);
                label.alignment = TextAnchor.MiddleLeft;
                var key = MakeText(row, font, "Shortcut", 28, 412, 0, 66, RowHeight);
                key.alignment = TextAnchor.MiddleRight;
                label.text = labels[i] ?? string.Empty;
                key.text = i < shortcuts.Length ? shortcuts[i] ?? string.Empty : string.Empty;
                button.ConfigureVisuals(label, key, focus, null, Ink);
                Action action = actions[i];
                int lastActivationFrame = -1;
                button.onClick.AddListener(() =>
                {
                    if (SubmenuVisible || lastActivationFrame == Time.frameCount) return;
                    lastActivationFrame = Time.frameCount;
                    ResetHiddenEndingClicks();
                    action?.Invoke();
                });
                Buttons[i] = button;
            }
            // Only action feedback is transient; there is no persistent context or footer.
            status = MakeText(column, font, "PauseStatus", 22, 20, columnHeight + 12, ColumnWidth - 40, 64);
            status.gameObject.SetActive(false);
            BuildManual(font);
            BuildSettings(font);
            BuildGm(font);
            LayoutColumn();
            gameObject.SetActive(false);
        }

        public void ConfigureHiddenEndingShortcut(Action action)
        {
            hiddenEndingAction = action;
            hiddenEndingButton.interactable = action != null;
            hiddenEndingButton.targetGraphic.raycastTarget = action != null;
            ResetHiddenEndingClicks();
        }

        private void ClickHiddenEnding()
        {
            if (!visible || SubmenuVisible || hiddenEndingAction == null || hiddenEndingTriggered
                || hiddenEndingClickFrame == Time.frameCount) return;
            hiddenEndingClickFrame = Time.frameCount;
            float now = Time.unscaledTime;
            if (now - hiddenEndingLastClick > HiddenEndingClickInterval) hiddenEndingClicks = 0;
            hiddenEndingLastClick = now;
            if (++hiddenEndingClicks < HiddenEndingClickCount) return;
            hiddenEndingTriggered = true;
            hiddenEndingAction();
        }

        private void ResetHiddenEndingClicks()
        {
            hiddenEndingClicks = 0;
            hiddenEndingTriggered = false;
        }

        public void ConfigureGmEndings(Action<EndingType> action)
        {
            gmAction = action;
            if (action == null) TryCloseGm();
        }

        private void BuildGm(Font font)
        {
            gm = NewRect(root, "PauseGmEndings");
            gm.anchorMin = gm.anchorMax = new Vector2(.06f, .5f);
            gm.pivot = new Vector2(0, .5f);
            gm.sizeDelta = new Vector2(ColumnWidth, 100 + 5 * RowStride - (RowStride - RowHeight));
            var title = MakeText(gm, font, "GmTitle", 46, 20, 0, ColumnWidth - 40, 68);
            title.text = "GM · 结局";
            string[] labels = { "正常结局", "配额失败结局", "神父结局", "隐藏自审结局", "返回暂停菜单" };
            EndingType[] endings = { EndingType.TakenAway, EndingType.LockedInRoom, EndingType.PriestAttack, EndingType.DreamSea };
            var allButtons = new Button[labels.Length];
            GmEndingButtons = new Button[endings.Length];
            for (int i = 0; i < labels.Length; i++)
            {
                var row = NewRect(gm, "GmAction" + i);
                TopRect(row, 0, 100 + i * RowStride, ColumnWidth, RowHeight);
                var hit = row.gameObject.AddComponent<Image>();
                hit.sprite = stripSprite;
                var button = row.gameObject.AddComponent<PauseMenuButton>();
                button.targetGraphic = hit;
                var frame = NewRect(row, "FocusFrame");
                Stretch(frame);
                var outline = frame.gameObject.AddComponent<Image>();
                outline.sprite = focusSprite; outline.color = Ink; outline.raycastTarget = false;
                var focus = frame.gameObject.AddComponent<CanvasGroup>();
                focus.alpha = 0; focus.blocksRaycasts = false; focus.interactable = false;
                var label = MakeText(row, font, "Label", 40, 32, 0, 400, RowHeight);
                label.alignment = TextAnchor.MiddleLeft; label.text = labels[i];
                var key = MakeText(row, font, "Shortcut", 28, 412, 0, 66, RowHeight);
                key.alignment = TextAnchor.MiddleRight; key.text = i == endings.Length ? "ESC" : string.Empty;
                button.ConfigureVisuals(label, key, focus, null, Ink);
                if (i < endings.Length)
                {
                    var ending = endings[i];
                    int lastActivationFrame = -1;
                    button.onClick.AddListener(() =>
                    {
                        if (!GmVisible || gmAction == null || lastActivationFrame == Time.frameCount) return;
                        lastActivationFrame = Time.frameCount;
                        gmAction(ending);
                    });
                    GmEndingButtons[i] = button;
                }
                else button.onClick.AddListener(() => TryCloseGm());
                allButtons[i] = button;
            }
            for (int i = 0; i < allButtons.Length; i++)
                allButtons[i].navigation = new Navigation { mode = Navigation.Mode.Explicit,
                    selectOnUp = allButtons[(i + allButtons.Length - 1) % allButtons.Length],
                    selectOnDown = allButtons[(i + 1) % allButtons.Length] };
            gm.gameObject.SetActive(false);
        }

        private void UpdateGmChord(bool n, bool o, bool r, bool m)
        {
            bool held = n && o && r && m;
            if (!held) { gmChordConsumed = false; return; }
            if (gmChordConsumed || !visible || SubmenuVisible || gmAction == null || MainMenuController.BlocksGameplayInput) return;
            // Consume only a successful opening. A chord already held when the pause root becomes
            // eligible must still open GM; returning from GM waits for release to avoid reopening.
            gmChordConsumed = true;
            ResetHiddenEndingClicks();
            gmPreviousSelection = EventSystem.current?.currentSelectedGameObject;
            columnInput.interactable = columnInput.blocksRaycasts = false;
            column.gameObject.SetActive(false);
            gm.gameObject.SetActive(true);
            LayoutColumn();
            EventSystem.current?.SetSelectedGameObject(GmEndingButtons[0].gameObject);
        }

        public bool TryCloseGm()
        {
            if (!GmVisible) return false;
            gm.gameObject.SetActive(false);
            column.gameObject.SetActive(true);
            columnInput.interactable = columnInput.blocksRaycasts = true;
            RefreshNavigation();
            var selectable = gmPreviousSelection != null ? gmPreviousSelection.GetComponent<Selectable>() : null;
            bool valid = selectable != null && selectable.transform.IsChildOf(column) && selectable.IsActive() && selectable.IsInteractable();
            EventSystem.current?.SetSelectedGameObject(valid ? gmPreviousSelection : FirstEnabled()?.gameObject);
            gmPreviousSelection = null;
            return true;
        }

        private void BuildManual(Font font)
        {
            manual = NewRect(root, "PauseOperationManual");
            manual.anchorMin = manual.anchorMax = manual.pivot = Vector2.one * .5f;
            manual.sizeDelta = new Vector2(1100, 810);
            var panel = manual.gameObject.AddComponent<Image>();
            panel.color = new Color(Desk.r, Desk.g, Desk.b, .97f);
            panel.raycastTarget = true;
            MakeLine(manual, "ManualTopBorder", 0, 0, 1100, 1, Ink);
            MakeLine(manual, "ManualBottomBorder", 0, 809, 1100, 1, Ink);
            MakeLine(manual, "ManualLeftBorder", 0, 0, 1, 810, Ink);
            MakeLine(manual, "ManualRightBorder", 1099, 0, 1, 810, Ink);
            var title = MakeText(manual, font, "ManualTitle", 34, 38, 28, 1024, 56);
            title.text = "操作手册";
            title.color = Bright;
            MakeLine(manual, "ManualTitleRule", 38, 106, 1024, 1, Edge);
            var leftTitle = MakeText(manual, font, "ManualRoomHeading", 24, 38, 132, 478, 42);
            leftTitle.text = "01  房间";
            var left = MakeText(manual, font, "ManualRoomControls", 24, 38, 188, 478, 330);
            left.lineSpacing = 1.3f;
            left.text = "WASD    移动\n鼠标    转动视角\nE / 鼠标左键    阅读 / 开门 / 睡觉 / 工作\nE / 鼠标左键    在门外返回房间\n准星图标    提示当前可进行的交互";
            var rightTitle = MakeText(manual, font, "ManualTerminalHeading", 24, 570, 132, 492, 42);
            rightTitle.text = "02  终端";
            var right = MakeText(manual, font, "ManualTerminalControls", 24, 570, 188, 492, 330);
            right.lineSpacing = 1.3f;
            right.text = "鼠标    选择并点击命令\n↑ / ↓    切换对象或应答\nEnter    确认当前选择 / 下一句\n空格    推进对白\n1–4    确认对应应答\n鼠标滚轮    查阅对话记录";
            MakeLine(manual, "ManualSystemRule", 38, 530, 1024, 1, Edge);
            var system = MakeText(manual, font, "ManualSystemControls", 23, 38, 555, 1024, 104);
            system.lineSpacing = 1.3f;
            system.text = "03  系统\nEsc    暂停 / 返回    ·    F5    保存进度    ·    F9    读取存档";
            var row = NewRect(manual, "ManualReturn");
            TopRect(row, 570, 704, 492, 64);
            var hit = row.gameObject.AddComponent<Image>();
            hit.color = Color.clear;
            manualReturn = row.gameObject.AddComponent<PauseMenuButton>();
            manualReturn.targetGraphic = hit;
            var frame = NewRect(row, "FocusFrame");
            Stretch(frame);
            var fill = frame.gameObject.AddComponent<Image>();
            fill.sprite = focusSprite;
            fill.color = Ink;
            fill.raycastTarget = false;
            var focus = frame.gameObject.AddComponent<CanvasGroup>();
            focus.alpha = 0; focus.blocksRaycasts = false; focus.interactable = false;
            var mark = NewRect(row, "SelectionMark");
            TopRect(mark, 12, 23, 3, 18);
            var marker = mark.gameObject.AddComponent<Image>();
            marker.color = Color.clear; marker.raycastTarget = false;
            var label = MakeText(row, font, "Label", GameUiTypography.ReferenceSize, 34, 0, 290, 64);
            label.alignment = TextAnchor.MiddleLeft;
            label.text = "返回暂停菜单";
            var key = MakeText(row, font, "Shortcut", 19, 340, 0, 130, 64);
            key.alignment = TextAnchor.MiddleRight;
            key.text = "ESC / Enter";
            ((PauseMenuButton)manualReturn).ConfigureVisuals(label, key, focus, marker, Ink);
            manualReturn.navigation = new Navigation { mode = Navigation.Mode.None };
            manualReturn.onClick.AddListener(() => TryCloseManual());
            manual.gameObject.SetActive(false);
        }

        public void ShowManual()
        {
            if (!visible || SubmenuVisible) return;
            ResetHiddenEndingClicks();
            var events = EventSystem.current;
            manualPreviousSelection = events != null ? events.currentSelectedGameObject : null;
            columnInput.interactable = false;
            columnInput.blocksRaycasts = false;
            column.gameObject.SetActive(false);
            manual.gameObject.SetActive(true);
            LayoutColumn();
            if (events != null) events.SetSelectedGameObject(manualReturn.gameObject);
        }

        public bool TryCloseManual()
        {
            if (!ManualVisible) return false;
            manual.gameObject.SetActive(false);
            column.gameObject.SetActive(true);
            columnInput.interactable = true;
            columnInput.blocksRaycasts = true;
            RefreshNavigation();
            var events = EventSystem.current;
            var selectable = manualPreviousSelection != null ? manualPreviousSelection.GetComponent<Selectable>() : null;
            bool valid = selectable != null && selectable.transform.IsChildOf(column)
                && selectable.IsActive() && selectable.IsInteractable();
            if (events != null) events.SetSelectedGameObject(valid ? manualPreviousSelection : FirstEnabled()?.gameObject);
            manualPreviousSelection = null;
            return true;
        }

        private void BuildSettings(Font font)
        {
            SharedSettings = PlayerSettingsView.Create(root, font, () => TryCloseSettings());
            settings = (RectTransform)SharedSettings.transform;
            settings.name = "PauseGameSettings";
            settings.anchorMin = settings.anchorMax = new Vector2(.085f, .52f);
            settings.pivot = new Vector2(0, .5f);
        }

        public void ShowSettings()
        {
            if (!visible || SubmenuVisible) return;
            ResetHiddenEndingClicks();
            settingsPreviousSelection = EventSystem.current?.currentSelectedGameObject;
            columnInput.interactable = columnInput.blocksRaycasts = false;
            column.gameObject.SetActive(false); SharedSettings.Show("返回暂停菜单");
            backdrop.SetLivePreview(true); group.alpha = 1;
            LayoutColumn();
        }

        public bool TryCloseSettings()
        {
            if (!SettingsVisible) return false;
            SharedSettings.Hide(); column.gameObject.SetActive(true);
            columnInput.interactable = columnInput.blocksRaycasts = true;
            backdrop.SetLivePreview(false); RefreshNavigation();
            var selectable = settingsPreviousSelection != null ? settingsPreviousSelection.GetComponent<Selectable>() : null;
            bool valid = selectable != null && selectable.transform.IsChildOf(column) && selectable.IsActive() && selectable.IsInteractable();
            EventSystem.current?.SetSelectedGameObject(valid ? settingsPreviousSelection : FirstEnabled()?.gameObject);
            settingsPreviousSelection = null;
            return true;
        }

        // Retain the owner API without recreating the removed context/footer UI.
        public void SetContext(string value) { }
        public void SetHint(string value) { }
        public void SetStatus(string value, bool error = false)
        {
            if (status == null) return;
            status.text = value ?? string.Empty;
            status.gameObject.SetActive(!string.IsNullOrWhiteSpace(value));
            status.color = error ? Bright : Ink;
        }

        public void SetVisible(bool value)
        {
            if (group == null || visible == value) return;
            ResetHiddenEndingClicks();
            visible = value;
            group.interactable = value;
            group.blocksRaycasts = value;
            if (value)
            {
                var events = EventSystem.current;
                previousSelection = events != null ? events.currentSelectedGameObject : null;
                if (previousSelection != null && previousSelection.transform.IsChildOf(transform)) previousSelection = null;
                gameObject.SetActive(true);
                backdrop.Capture();
                transform.SetAsLastSibling();
                LayoutColumn();
                RefreshNavigation();
                if (events != null) events.SetSelectedGameObject(FirstEnabled()?.gameObject);
            }
            else
            {
                ResetManual();
                ResetSettings();
                ResetGm();
                RestoreSelection();
                if (group.alpha <= .001f) gameObject.SetActive(false);
            }
        }

        private void Update()
        {
            if (group == null) return;
            UpdateGmChord(Input.GetKey(KeyCode.N), Input.GetKey(KeyCode.O), Input.GetKey(KeyCode.R), Input.GetKey(KeyCode.M));
            if (!backdrop.Capturing)
                group.alpha = Mathf.MoveTowards(group.alpha, visible ? 1 : 0, Time.unscaledDeltaTime / FadeSeconds);
            if (visible && !SubmenuVisible) RefreshNavigation();
            else if (!visible && group.alpha <= 0) gameObject.SetActive(false);
        }

        private Button FirstEnabled()
        {
            foreach (var button in Buttons) if (button != null && button.IsActive() && button.IsInteractable()) return button;
            return null;
        }

        private void RefreshNavigation()
        {
            for (int i = 0; i < Buttons.Length; i++)
            {
                var button = Buttons[i];
                if (button == null) continue;
                Button above = null, below = null;
                for (int step = 1; step <= Buttons.Length; step++)
                {
                    var up = Buttons[(i - step + Buttons.Length) % Buttons.Length];
                    var down = Buttons[(i + step) % Buttons.Length];
                    if (above == null && up != null && up.IsActive() && up.IsInteractable()) above = up;
                    if (below == null && down != null && down.IsActive() && down.IsInteractable()) below = down;
                }
                button.navigation = new Navigation { mode = Navigation.Mode.Explicit, selectOnUp = above, selectOnDown = below };
            }
            var selected = EventSystem.current?.currentSelectedGameObject;
            if (selected != null && selected.transform.IsChildOf(transform))
            {
                var selectable = selected.GetComponent<Selectable>();
                if (selectable != null && !selectable.IsInteractable())
                    EventSystem.current.SetSelectedGameObject(FirstEnabled()?.gameObject);
            }
        }

        private void RestoreSelection()
        {
            var events = EventSystem.current;
            if (events == null) { previousSelection = null; return; }
            bool valid = previousSelection != null && previousSelection.activeInHierarchy;
            var selectable = valid ? previousSelection.GetComponent<Selectable>() : null;
            if (selectable != null) valid &= selectable.IsActive() && selectable.IsInteractable();
            events.SetSelectedGameObject(valid ? previousSelection : null);
            previousSelection = null;
        }

        private void OnRectTransformDimensionsChange() => LayoutColumn();
        private void LayoutColumn()
        {
            if (root == null || column == null || columnHeight <= 0) return;
            float height = root.rect.height > 0 ? root.rect.height : 1080;
            float width = root.rect.width > 0 ? root.rect.width : 1920;
            float scale = Mathf.Min(height * .82f / columnHeight, width * .30f / ColumnWidth);
            column.localScale = Vector3.one * scale;
            if (manual != null)
                manual.localScale = Vector3.one * Mathf.Min(1, height * .88f / 810, width * .88f / 1100);
            if (settings != null)
                settings.localScale = Vector3.one * Mathf.Min(1, height * .88f / 600, width * .88f / 740);
            if (gm != null)
                gm.localScale = Vector3.one * Mathf.Min(height * .82f / gm.sizeDelta.y, width * .30f / ColumnWidth);
        }

        private void ResetManual()
        {
            if (manual != null) manual.gameObject.SetActive(false);
            if (column != null) column.gameObject.SetActive(true);
            if (columnInput != null) { columnInput.interactable = true; columnInput.blocksRaycasts = true; }
            manualPreviousSelection = null;
        }

        private void OnDisable()
        {
            ResetHiddenEndingClicks();
            if (visible) RestoreSelection();
            visible = false;
            ResetManual();
            ResetSettings();
            ResetGm();
            if (group != null) { group.alpha = 0; group.interactable = false; group.blocksRaycasts = false; }
        }

        private void ResetSettings()
        {
            if (SharedSettings != null) SharedSettings.Hide();
            if (backdrop != null) backdrop.SetLivePreview(false, false);
            settingsPreviousSelection = null;
        }

        private void ResetGm()
        {
            if (gm != null) gm.gameObject.SetActive(false);
            gmPreviousSelection = null;
            gmChordConsumed = false;
        }

        private static RectTransform NewRect(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }
        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }
        private static void TopRect(RectTransform rect, float x, float y, float width, float height)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 1); rect.anchoredPosition = new Vector2(x, -y); rect.sizeDelta = new Vector2(width, height);
        }
        private Text MakeText(Transform parent, Font font, string name, int size, float x, float y, float width, float height)
        {
            var rect = NewRect(parent, name);
            TopRect(rect, x, y, width, height);
            var label = rect.gameObject.AddComponent<TerminalBitmapText>();
            label.ScaleToFontSize = true; label.LinePitch = size * 1.3f;
            label.font = font; label.fontStyle = FontStyle.Normal; label.fontSize = size; label.color = Ink;
            label.material = lettering;
            label.alignment = TextAnchor.UpperLeft; label.raycastTarget = false;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Truncate;
            return label;
        }

        private static void MakeLine(Transform parent, string name, float x, float y, float width, float height, Color color)
        {
            var rect = NewRect(parent, name);
            TopRect(rect, x, y, width, height);
            var image = rect.gameObject.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
        }

        private void CreateTextures()
        {
            const int width = (int)ColumnWidth, height = (int)RowHeight;
            var strip = new Color32[width * height];
            var focus = new Color32[width * height];
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                {
                    float verticalEdge = Mathf.Min(y, height - 1 - y);
                    float horizontalEdge = Mathf.Min(x, width - 1 - x);
                    float edge = Mathf.Min(verticalEdge - Noise(x / 3, 7) * 1.4f,
                        horizontalEdge - Noise(11, y / 3) * 1.4f);
                    strip[y * width + x] = new Color(0, 0, 0, Mathf.Clamp01(edge));
                    float frameEdge = Mathf.Min(verticalEdge - Noise(x / 4, 19) * .8f,
                        horizontalEdge - Noise(23, y / 4) * .8f);
                    float alpha = Mathf.Clamp01(frameEdge - 5) * Mathf.Clamp01(9 - frameEdge);
                    focus[y * width + x] = new Color(1, 1, 1, alpha);
                }
            stripSprite = MakeSprite("Pause black strip", strip, width, height, out stripTexture);
            focusSprite = MakeSprite("Pause white outline", focus, width, height, out focusTexture);
            var shader = Resources.Load<Shader>("PauseLettering");
            if (shader != null && shader.isSupported)
                lettering = new Material(shader) { name = "Pause worn white lettering", hideFlags = HideFlags.HideAndDontSave };
        }

        private static Sprite MakeSprite(string name, Color32[] pixels, int width, int height, out Texture2D texture)
        {
            texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
                { name = name, hideFlags = HideFlags.HideAndDontSave, filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
            texture.SetPixels32(pixels); texture.Apply(false, true);
            var sprite = Sprite.Create(texture, new Rect(0, 0, width, height), Vector2.one * .5f, 100, 0, SpriteMeshType.FullRect);
            sprite.name = name;
            return sprite;
        }

        private static float Noise(int x, int y)
        {
            unchecked
            {
                uint bits = (uint)(x * 374761393 + y * 668265263 + 911);
                bits = (bits ^ (bits >> 13)) * 1274126177;
                return (bits & 65535) / 65535f;
            }
        }

        private void OnDestroy()
        {
            if (stripSprite != null) Destroy(stripSprite);
            if (focusSprite != null) Destroy(focusSprite);
            if (stripTexture != null) Destroy(stripTexture);
            if (focusTexture != null) Destroy(focusTexture);
            if (lettering != null) Destroy(lettering);
        }
    }
}
