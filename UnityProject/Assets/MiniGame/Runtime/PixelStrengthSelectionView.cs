using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MiniGame.UnityHost
{
    /// <summary>First-launch comparison. Selection is provisional until the controller confirms it.</summary>
    public sealed class PixelStrengthSelectionView : MonoBehaviour
    {
        private static readonly PixelDetail[] Details = { PixelDetail.High, PixelDetail.Medium, PixelDetail.Low };
        private static readonly string[] Names = { "高", "中 · 推荐", "低" };
        private readonly CanvasGroup[] selectionFrames = new CanvasGroup[3];
        private readonly Text[] selectionLabels = new Text[3];
        private Font font;
        public PixelDetail SelectedDetail { get; private set; } = PixelDetail.Medium;
        public Button[] ChoiceButtons { get; private set; } = Array.Empty<Button>();
        public Button ConfirmButton { get; private set; }
        public RawImage[] PreviewImages { get; private set; } = Array.Empty<RawImage>();

        public static PixelStrengthSelectionView Create(Transform parent, Font font, Action confirm)
        {
            var go = new GameObject("PixelStrengthSetup", typeof(RectTransform), typeof(PixelStrengthSelectionView));
            go.transform.SetParent(parent, false);
            var view = go.GetComponent<PixelStrengthSelectionView>();
            view.font = font;
            view.Build(confirm);
            view.Hide();
            return view;
        }

        private void Build(Action confirm)
        {
            var root = (RectTransform)transform;
            root.anchorMin = root.anchorMax = root.pivot = Vector2.one * .5f;
            root.sizeDelta = new Vector2(1784, 860);
            Label(root, "Title", "选择像素风强度", 44, 0, 28, 1784, 64).color = TerminalView.Bright;
            Label(root, "Explanation", "像素块越大，像素风越强。请选择你喜欢的画面。", 28, 0, 110, 1784, 44);
            ChoiceButtons = new Button[3];
            PreviewImages = new RawImage[3];
            for (int i = 0; i < 3; i++)
            {
                int index = i;
                var card = RectAt(root, "PixelPreview" + Details[i], 52 + i * 568, 196, 544, 424);
                ChoiceButtons[i] = ButtonOn(card, () => Select(Details[index]));
                PreviewImages[i] = RectAt(card, "GamePreview", 8, 8, 528, 297).gameObject.AddComponent<RawImage>();
                PreviewImages[i].texture = Resources.Load<Texture2D>("MiniGame/PixelPreviews/" + Details[i]);
                PreviewImages[i].raycastTarget = false;
                if (PreviewImages[i].texture == null)
                    Label(card, "MissingPreview", "画面预览暂不可用", 24, 8, 8, 528, 297);
                Label(card, "StrengthName", Names[i], 34, 16, 316, 512, 48);
                Label(card, "StrengthDescription", PixelStrengthTooltip.Description(Details[i]), 24, 16, 370, 512, 40);
                var frame = RectAt(card, "SelectedFrame", 0, 0, 544, 424);
                selectionFrames[i] = frame.gameObject.AddComponent<CanvasGroup>();
                selectionFrames[i].blocksRaycasts = false;
                selectionFrames[i].interactable = false;
                Border(frame, 544, 424, 3, TerminalView.Bright);
                selectionLabels[i] = Label(root, "Selected" + Details[i], "已选择", 24, 52 + i * 568, 632, 544, 36);
            }
            var confirmRect = RectAt(root, "ConfirmPixelStrength", 592, 696, 600, 72);
            ConfirmButton = ButtonOn(confirmRect, () => confirm?.Invoke());
            Border(confirmRect, 600, 72, 2, TerminalView.Ink);
            Label(confirmRect, "Label", "确认并继续", 32, 16, 0, 568, 72);
            Label(root, "NavigationHint", "左右方向键选择  /  Enter 确认  /  可随时在设置中调整", 23, 0, 798, 1784, 40);
        }

        public void Show(PixelDetail detail)
        {
            gameObject.SetActive(true);
            FitToCanvas();
            Select(detail == PixelDetail.High || detail == PixelDetail.Low ? detail : PixelDetail.Medium);
        }

        public void Hide()
        {
            var selected = EventSystem.current?.currentSelectedGameObject;
            if (selected != null && selected.transform.IsChildOf(transform))
                EventSystem.current.SetSelectedGameObject(null);
            gameObject.SetActive(false);
        }

        public void MoveSelection(int delta)
        {
            if (!isActiveAndEnabled || delta == 0) return;
            int index = Array.IndexOf(Details, SelectedDetail);
            int next = ((index + delta) % Details.Length + Details.Length) % Details.Length;
            Select(Details[next]);
        }

        private void LateUpdate() => FitToCanvas();

        private void FitToCanvas()
        {
            if (!(transform.parent is RectTransform canvas)) return;
            float scale = Mathf.Min(1, Mathf.Min(canvas.rect.width / 1920f, canvas.rect.height / 1080f));
            if (scale > 0) transform.localScale = Vector3.one * scale;
        }

        private void Select(PixelDetail detail)
        {
            if (!isActiveAndEnabled) return;
            SelectedDetail = detail;
            for (int i = 0; i < Details.Length; i++)
            {
                bool selected = Details[i] == detail;
                selectionFrames[i].alpha = selected ? 1 : 0;
                selectionLabels[i].gameObject.SetActive(selected);
                if (selected) EventSystem.current?.SetSelectedGameObject(ChoiceButtons[i].gameObject);
            }
        }

        private Button ButtonOn(RectTransform rect, Action click)
        {
            var image = rect.gameObject.AddComponent<Image>();
            image.color = Color.white;
            var button = rect.gameObject.AddComponent<PixelStrengthSelectionButton>();
            button.targetGraphic = image;
            button.navigation = new Navigation { mode = Navigation.Mode.None };
            var colors = button.colors;
            colors.normalColor = new Color(.045f, .035f, .012f, 1);
            colors.highlightedColor = colors.selectedColor = new Color(.11f, .078f, .018f, 1);
            colors.pressedColor = new Color(.2f, .14f, .03f, 1);
            colors.colorMultiplier = 1;
            colors.fadeDuration = .08f;
            button.colors = colors;
            button.onClick.AddListener(() => { if (isActiveAndEnabled) click(); });
            return button;
        }

        private Text Label(Transform parent, string name, string value, int size, float x, float y, float width, float height)
        {
            var text = RectAt(parent, name, x, y, width, height).gameObject.AddComponent<TerminalBitmapText>();
            text.font = font;
            text.fontSize = size;
            text.ScaleToFontSize = true;
            text.LinePitch = size * 1.3f;
            text.color = TerminalView.Ink;
            text.text = value;
            text.alignment = TextAnchor.MiddleCenter;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.raycastTarget = false;
            return text;
        }

        private static RectTransform RectAt(Transform parent, string name, float x, float y, float width, float height)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(x, -y);
            rect.sizeDelta = new Vector2(width, height);
            return rect;
        }

        private static void Border(Transform parent, float width, float height, float thickness, Color color)
        {
            Line(parent, "TopBorder", 0, 0, width, thickness, color);
            Line(parent, "BottomBorder", 0, height - thickness, width, thickness, color);
            Line(parent, "LeftBorder", 0, 0, thickness, height, color);
            Line(parent, "RightBorder", width - thickness, 0, thickness, height, color);
        }

        private static void Line(Transform parent, string name, float x, float y, float width, float height, Color color)
        {
            var image = RectAt(parent, name, x, y, width, height).gameObject.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
        }
    }
}
