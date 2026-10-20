using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MiniGame.UnityHost
{
    /// <summary>Shared scene-backed pause presentation. Runtime owners provide actions and input policy.</summary>
    public sealed class PauseMenuView : MonoBehaviour
    {
        private static readonly Color Ivory = new Color(.91f, .88f, .78f, 1);
        private const float FadeSeconds = .12f;
        private CanvasGroup group;
        private RectTransform root, column;
        private Text status;
        private PauseMenuBackdrop backdrop;
        private GameObject previousSelection;
        private bool visible;
        private float columnHeight;
        private Sprite backdropSprite, frameSprite;
        private Texture2D backdropTexture, frameTexture;
        public Button[] Buttons { get; private set; } = Array.Empty<Button>();

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
            column.anchorMin = column.anchorMax = new Vector2(.11f, .52f);
            column.pivot = new Vector2(0, .5f);
            columnHeight = 155 + labels.Length * 90;
            column.sizeDelta = new Vector2(560, columnHeight);
            var title = MakeText(column, font, "PauseTitle", 70, -3, 0, 560, 100);
            title.text = "暂停";
            title.fontStyle = FontStyle.Bold;
            var rule = NewRect(column, "PauseTitleRule");
            TopRect(rule, 0, 114, 74, 2);
            var ruleImage = rule.gameObject.AddComponent<Image>();
            ruleImage.color = new Color(Ivory.r, Ivory.g, Ivory.b, .47f);
            ruleImage.raycastTarget = false;

            Buttons = new Button[labels.Length];
            for (int i = 0; i < labels.Length; i++)
            {
                var row = NewRect(column, "PauseAction" + i);
                TopRect(row, -22, 146 + i * 90, 560, 80);
                var hitRect = NewRect(row, "ProjectedHitArea");
                var hit = hitRect.gameObject.AddComponent<PauseMenuHitArea>();
                hit.Configure(column, row);
                var button = row.gameObject.AddComponent<PauseMenuButton>();
                button.targetGraphic = hit;
                var outline = NewRect(row, "FocusFrame");
                Stretch(outline);
                var outlineImage = outline.gameObject.AddComponent<Image>();
                outlineImage.sprite = frameSprite;
                outlineImage.type = Image.Type.Sliced;
                outlineImage.color = new Color(Ivory.r, Ivory.g, Ivory.b, .90f);
                outlineImage.raycastTarget = false;
                var focus = outline.gameObject.AddComponent<CanvasGroup>();
                focus.alpha = 0; focus.blocksRaycasts = false; focus.interactable = false;
                var mark = NewRect(row, "SelectionMark");
                TopRect(mark, 10, 31, 4, 18);
                var marker = mark.gameObject.AddComponent<Image>();
                marker.raycastTarget = false;
                marker.color = Color.clear;
                var label = MakeText(row, font, "Label", 36, 24, 9, 420, 62);
                label.alignment = TextAnchor.MiddleLeft;
                var key = MakeText(row, font, "Shortcut", 19, 438, 12, 92, 56);
                key.alignment = TextAnchor.MiddleRight;
                label.text = labels[i] ?? string.Empty;
                key.text = i < shortcuts.Length ? shortcuts[i] ?? string.Empty : string.Empty;
                button.ConfigureVisuals(label, key, focus, marker, Ivory);
                Action action = actions[i];
                int lastActivationFrame = -1;
                button.onClick.AddListener(() =>
                {
                    if (lastActivationFrame == Time.frameCount) return;
                    lastActivationFrame = Time.frameCount;
                    action?.Invoke();
                });
                Buttons[i] = button;
            }
            // Only action feedback is transient; there is no persistent context or footer.
            status = MakeText(column, font, "PauseStatus", 20, 0, columnHeight + 8, 550, 64);
            status.gameObject.SetActive(false);
            foreach (var graphic in column.GetComponentsInChildren<Graphic>(true))
            {
                if (graphic is PauseMenuHitArea) continue;
                graphic.gameObject.AddComponent<PauseMenuPerspective>().Configure(column);
            }
            LayoutColumn();
            gameObject.SetActive(false);
        }

        // Retain the owner API without recreating the removed context/footer UI.
        public void SetContext(string value) { }
        public void SetHint(string value) { }
        public void SetStatus(string value, bool error = false)
        {
            if (status == null) return;
            status.text = value ?? string.Empty;
            status.gameObject.SetActive(!string.IsNullOrWhiteSpace(value));
            status.color = error ? new Color(.83f, .40f, .31f, 1) : new Color(Ivory.r, Ivory.g, Ivory.b, .83f);
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
                RestoreSelection();
                if (group.alpha <= .001f) gameObject.SetActive(false);
            }
        }

        private void Update()
        {
            if (group == null) return;
            if (!backdrop.Capturing)
                group.alpha = Mathf.MoveTowards(group.alpha, visible ? 1 : 0, Time.unscaledDeltaTime / FadeSeconds);
            if (visible) RefreshNavigation();
            else if (group.alpha <= 0) gameObject.SetActive(false);
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
            float scale = Mathf.Min(1, height * .86f / columnHeight, width * .68f / 560);
            column.localScale = Vector3.one * scale;
        }

        private void OnDisable()
        {
            if (visible) RestoreSelection();
            visible = false;
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
            label.font = font; label.fontSize = size; label.color = Ivory;
            label.alignment = TextAnchor.UpperLeft; label.raycastTarget = false;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Truncate;
            return label;
        }

        private void CreateTextures()
        {
            backdropTexture = new Texture2D(256, 128, TextureFormat.RGBA32, false)
                { name = "Original pause charcoal grain", hideFlags = HideFlags.HideAndDontSave, wrapMode = TextureWrapMode.Clamp };
            var background = new Color32[256 * 128];
            for (int y = 0; y < 128; y++)
                for (int x = 0; x < 256; x++)
                {
                    float u = x / 255f;
                    float alpha = 1 - Mathf.SmoothStep(0, 1, Mathf.InverseLerp(.32f, .85f, u));
                    float grain = Noise(x, y) - .5f;
                    background[y * 256 + x] = new Color(.055f + grain * .015f, .063f + grain * .015f,
                        .056f + grain * .015f, Mathf.Clamp01(alpha * (.63f + grain * .035f)));
                }
            backdropTexture.SetPixels32(background); backdropTexture.Apply(false, true);
            backdropSprite = Sprite.Create(backdropTexture, new Rect(0, 0, 256, 128), Vector2.one * .5f, 100);
            backdropSprite.name = "Original pause backdrop";
            frameTexture = new Texture2D(256, 64, TextureFormat.RGBA32, false)
                { name = "Original distressed pause frame", hideFlags = HideFlags.HideAndDontSave,
                    wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
            var border = new Color32[256 * 64];
            for (int y = 0; y < 64; y++)
                for (int x = 0; x < 256; x++)
                {
                    int edge = Mathf.Min(Mathf.Min(x, 255 - x), Mathf.Min(y, 63 - y));
                    float grain = Noise(x + 317, y + 71);
                    float alpha = edge >= 2 && edge <= 4 ? grain < .13f ? .18f : .60f + grain * .4f : 0;
                    border[y * 256 + x] = new Color(1, 1, 1, alpha);
                }
            frameTexture.SetPixels32(border); frameTexture.Apply(false, true);
            frameSprite = Sprite.Create(frameTexture, new Rect(0, 0, 256, 64), Vector2.one * .5f, 100, 0,
                SpriteMeshType.FullRect, new Vector4(8, 8, 8, 8));
            frameSprite.name = "Original pause focus border";
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
            if (frameSprite != null) Destroy(frameSprite);
            if (backdropTexture != null) Destroy(backdropTexture);
            if (frameTexture != null) Destroy(frameTexture);
        }
    }
}
