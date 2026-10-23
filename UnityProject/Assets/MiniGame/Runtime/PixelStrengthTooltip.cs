using UnityEngine;
using UnityEngine.UI;

namespace MiniGame.UnityHost
{
    /// <summary>A fixed-size visual explanation of how many pixel blocks fit in the same area.</summary>
    public sealed class PixelStrengthTooltip : MonoBehaviour
    {
        private readonly Image[] verticalLines = new Image[7];
        private readonly Image[] horizontalLines = new Image[7];
        private readonly Image[] borders = new Image[4];
        private float shownLineThickness = -1;
        public RectTransform GridRect { get; private set; }
        public int GridCount { get; private set; }
        public Text GridLabel { get; private set; }
        public Text DescriptionLabel { get; private set; }
        public GameObject Owner { get; private set; }

        public static PixelStrengthTooltip Create(Transform parent, Font font)
        {
            var rect = RectAt(parent, "PixelStrengthLegend", 0, 0, 208, 216);
            var tooltip = rect.gameObject.AddComponent<PixelStrengthTooltip>();
            var background = rect.gameObject.AddComponent<Image>();
            background.color = TerminalView.Desk;
            background.raycastTarget = false;
            var group = rect.gameObject.AddComponent<CanvasGroup>();
            group.blocksRaycasts = false;
            group.interactable = false;
            tooltip.GridRect = RectAt(rect, "YellowPixelGrid", 40, 12, 128, 128);
            var grid = tooltip.GridRect.gameObject.AddComponent<Image>();
            grid.color = TerminalView.Bright;
            grid.raycastTarget = false;
            for (int i = 0; i < 7; i++)
            {
                tooltip.verticalLines[i] = Line(tooltip.GridRect, "Vertical" + i, 0, 0, 1, 128);
                tooltip.horizontalLines[i] = Line(tooltip.GridRect, "Horizontal" + i, 0, 0, 128, 1);
            }
            tooltip.borders[0] = Line(tooltip.GridRect, "TopBorder", 0, 0, 128, 1);
            tooltip.borders[1] = Line(tooltip.GridRect, "BottomBorder", 0, 127, 128, 1);
            tooltip.borders[2] = Line(tooltip.GridRect, "LeftBorder", 0, 0, 1, 128);
            tooltip.borders[3] = Line(tooltip.GridRect, "RightBorder", 127, 0, 1, 128);
            tooltip.GridLabel = Label(rect, font, "GridCount", 146, 25, 24);
            tooltip.DescriptionLabel = Label(rect, font, "GridDescription", 176, 30, 19);
            tooltip.Hide();
            return tooltip;
        }

        public void Show(GameObject owner, PixelDetail detail, Vector2 position)
        {
            Owner = owner;
            GridCount = detail == PixelDetail.High ? 2 : detail == PixelDetail.Low ? 8 : 4;
            GridLabel.text = GridCount + "×" + GridCount + " 格";
            DescriptionLabel.text = Description(detail);
            for (int i = 0; i < 7; i++)
            {
                bool visible = i < GridCount - 1;
                verticalLines[i].gameObject.SetActive(visible);
                horizontalLines[i].gameObject.SetActive(visible);
                float offset = 128f * (i + 1) / GridCount;
                verticalLines[i].rectTransform.anchoredPosition = new Vector2(offset, 0);
                horizontalLines[i].rectTransform.anchoredPosition = new Vector2(0, -offset);
            }
            ((RectTransform)transform).anchoredPosition = position;
            transform.SetAsLastSibling();
            gameObject.SetActive(true);
            RefreshLineThickness();
        }

        private void LateUpdate() => RefreshLineThickness();

        private void RefreshLineThickness()
        {
            if (GridRect == null) return;
            var scale = GridRect.lossyScale;
            float effectiveScale = Mathf.Min(Mathf.Abs(scale.x), Mathf.Abs(scale.y));
            float thickness = effectiveScale > 0 ? Mathf.Max(1, 1 / effectiveScale) : 1;
            if (Mathf.Approximately(shownLineThickness, thickness)) return;
            shownLineThickness = thickness;
            // A one-unit UI line can disappear after CanvasScaler rounds a subpixel edge.
            for (int i = 0; i < verticalLines.Length; i++)
            {
                verticalLines[i].rectTransform.sizeDelta = new Vector2(thickness, 128);
                horizontalLines[i].rectTransform.sizeDelta = new Vector2(128, thickness);
            }
            borders[0].rectTransform.sizeDelta = borders[1].rectTransform.sizeDelta = new Vector2(128, thickness);
            borders[2].rectTransform.sizeDelta = borders[3].rectTransform.sizeDelta = new Vector2(thickness, 128);
            borders[1].rectTransform.anchoredPosition = new Vector2(0, -(128 - thickness));
            borders[3].rectTransform.anchoredPosition = new Vector2(128 - thickness, 0);
        }

        public void Hide(GameObject owner = null)
        {
            if (owner != null && Owner != owner) return;
            Owner = null;
            gameObject.SetActive(false);
        }

        public static string Description(PixelDetail detail)
            => detail == PixelDetail.High ? "大格子，颗粒感强"
                : detail == PixelDetail.Low ? "小格子，细节更多" : "颗粒与细节适中";

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

        private static Image Line(Transform parent, string name, float x, float y, float width, float height)
        {
            var image = RectAt(parent, name, x, y, width, height).gameObject.AddComponent<Image>();
            image.color = Color.black;
            image.raycastTarget = false;
            return image;
        }

        private static Text Label(Transform parent, Font font, string name, float y, float height, int size)
        {
            var text = RectAt(parent, name, 4, y, 200, height).gameObject.AddComponent<TerminalBitmapText>();
            text.font = font;
            text.fontSize = size;
            text.ScaleToFontSize = true;
            text.LinePitch = size * 1.3f;
            text.color = TerminalView.Ink;
            text.alignment = TextAnchor.MiddleCenter;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.raycastTarget = false;
            return text;
        }
    }
}
