using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MiniGame.UnityHost
{
    /// <summary>Shared scene-backed pause presentation. Runtime owners provide actions and input policy.</summary>
    public sealed class PauseMenuView : MonoBehaviour
    {
        private static readonly Color Ink = TerminalView.Ink;
        private static readonly Color Bright = TerminalView.Bright;
        private static readonly Color Desk = TerminalView.Desk;
        private static readonly Color Edge = TerminalView.Edge;
        private const float FadeSeconds = .12f;
        private CanvasGroup group;
        private RectTransform root, column, manual;
        private CanvasGroup columnInput;
        private Button manualReturn;
        private Text status;
        private PauseMenuBackdrop backdrop;
        private GameObject previousSelection, manualPreviousSelection;
        private bool visible;
        private float columnHeight;
        private Sprite backdropSprite;
        private Texture2D backdropTexture;
        public Button[] Buttons { get; private set; } = Array.Empty<Button>();
        public bool ManualVisible => visible && manual != null && manual.gameObject.activeSelf;

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
            shadingImage.sprite = backdropSprite;
            shadingImage.raycastTarget = false;

            column = NewRect(root, "PauseColumn");
            column.anchorMin = column.anchorMax = new Vector2(.085f, .52f);
            column.pivot = new Vector2(0, .5f);
            columnHeight = 166 + labels.Length * 66;
            column.sizeDelta = new Vector2(600, columnHeight);
            columnInput = column.gameObject.AddComponent<CanvasGroup>();
            var title = MakeText(column, font, "PauseTitle", 40, 0, 0, 600, 62);
            title.text = "NORM  /  暂停";
            title.color = Bright;
            var state = MakeText(column, font, "PauseSessionState", 20, 0, 72, 600, 32);
            state.text = "检查会话已挂起";
            MakeLine(column, "PauseTitleRule", 0, 120, 600, 1, Edge);

            Buttons = new Button[labels.Length];
            for (int i = 0; i < labels.Length; i++)
            {
                var row = NewRect(column, "PauseAction" + i);
                TopRect(row, 0, 144 + i * 66, 600, 54);
                var hit = row.gameObject.AddComponent<Image>();
                hit.color = Color.clear;
                var button = row.gameObject.AddComponent<PauseMenuButton>();
                button.targetGraphic = hit;
                var outline = NewRect(row, "FocusFrame");
                Stretch(outline);
                var outlineImage = outline.gameObject.AddComponent<Image>();
                outlineImage.color = Ink;
                outlineImage.raycastTarget = false;
                var focus = outline.gameObject.AddComponent<CanvasGroup>();
                focus.alpha = 0; focus.blocksRaycasts = false; focus.interactable = false;
                var mark = NewRect(row, "SelectionMark");
                TopRect(mark, 12, 18, 3, 18);
                var marker = mark.gameObject.AddComponent<Image>();
                marker.raycastTarget = false;
                marker.color = Color.clear;
                var number = MakeText(row, font, "CommandNumber", 21, 28, 0, 44, 54);
                number.text = (i + 1).ToString("00");
                number.alignment = TextAnchor.MiddleLeft;
                var label = MakeText(row, font, "Label", 28, 88, 0, 376, 54);
                label.alignment = TextAnchor.MiddleLeft;
                var key = MakeText(row, font, "Shortcut", 19, 472, 0, 106, 54);
                key.alignment = TextAnchor.MiddleRight;
                label.text = labels[i] ?? string.Empty;
                key.text = i < shortcuts.Length ? shortcuts[i] ?? string.Empty : string.Empty;
                button.ConfigureVisuals(label, key, focus, marker, Ink, number);
                Action action = actions[i];
                int lastActivationFrame = -1;
                button.onClick.AddListener(() =>
                {
                    if (ManualVisible || lastActivationFrame == Time.frameCount) return;
                    lastActivationFrame = Time.frameCount;
                    action?.Invoke();
                });
                Buttons[i] = button;
            }
            // Only action feedback is transient; there is no persistent context or footer.
            status = MakeText(column, font, "PauseStatus", 20, 0, columnHeight + 8, 600, 64);
            status.gameObject.SetActive(false);
            BuildManual(font);
            LayoutColumn();
            gameObject.SetActive(false);
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
            title.text = "NORM  /  操作手册";
            title.color = Bright;
            MakeLine(manual, "ManualTitleRule", 38, 106, 1024, 1, Edge);
            var leftTitle = MakeText(manual, font, "ManualRoomHeading", 24, 38, 132, 478, 42);
            leftTitle.text = "01  房间";
            var left = MakeText(manual, font, "ManualRoomControls", 24, 38, 188, 478, 330);
            left.lineSpacing = 1.3f;
            left.text = "WASD    移动\n鼠标    转动视角\nE    与准星指向的物件交互\n鼠标左键    使用电脑\n鼠标左键    阅读海报与物品";
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
            fill.color = Ink;
            fill.raycastTarget = false;
            var focus = frame.gameObject.AddComponent<CanvasGroup>();
            focus.alpha = 0; focus.blocksRaycasts = false; focus.interactable = false;
            var mark = NewRect(row, "SelectionMark");
            TopRect(mark, 12, 23, 3, 18);
            var marker = mark.gameObject.AddComponent<Image>();
            marker.color = Color.clear; marker.raycastTarget = false;
            var label = MakeText(row, font, "Label", 26, 34, 0, 290, 64);
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
            if (!visible || ManualVisible) return;
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
                RestoreSelection();
                if (group.alpha <= .001f) gameObject.SetActive(false);
            }
        }

        private void Update()
        {
            if (group == null) return;
            if (!backdrop.Capturing)
                group.alpha = Mathf.MoveTowards(group.alpha, visible ? 1 : 0, Time.unscaledDeltaTime / FadeSeconds);
            if (visible && !ManualVisible) RefreshNavigation();
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
            float scale = Mathf.Min(1, height * .86f / (columnHeight + 80), width * .68f / 600);
            column.localScale = Vector3.one * scale;
            if (manual != null)
                manual.localScale = Vector3.one * Mathf.Min(1, height * .88f / 810, width * .88f / 1100);
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
            if (visible) RestoreSelection();
            visible = false;
            ResetManual();
            if (group != null) { group.alpha = 0; group.interactable = false; group.blocksRaycasts = false; }
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
        private static Text MakeText(Transform parent, Font font, string name, int size, float x, float y, float width, float height)
        {
            var rect = NewRect(parent, name);
            TopRect(rect, x, y, width, height);
            var label = rect.gameObject.AddComponent<Text>();
            label.font = font; label.fontSize = size; label.color = Ink;
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
            backdropTexture = new Texture2D(256, 128, TextureFormat.RGBA32, false)
                { name = "Pause amber shading", hideFlags = HideFlags.HideAndDontSave, wrapMode = TextureWrapMode.Clamp };
            var background = new Color32[256 * 128];
            for (int y = 0; y < 128; y++)
                for (int x = 0; x < 256; x++)
                {
                    float u = x / 255f;
                    float alpha = 1 - Mathf.SmoothStep(0, 1, Mathf.InverseLerp(.32f, .85f, u));
                    float grain = Noise(x, y) - .5f;
                    background[y * 256 + x] = new Color(Desk.r + grain * .008f, Desk.g + grain * .005f,
                        Desk.b + grain * .002f, Mathf.Clamp01(alpha * (.88f + grain * .025f)));
                }
            backdropTexture.SetPixels32(background); backdropTexture.Apply(false, true);
            backdropSprite = Sprite.Create(backdropTexture, new Rect(0, 0, 256, 128), Vector2.one * .5f, 100);
            backdropSprite.name = "Pause amber backdrop";
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
            if (backdropSprite != null) Destroy(backdropSprite);
            if (backdropTexture != null) Destroy(backdropTexture);
        }
    }
}
