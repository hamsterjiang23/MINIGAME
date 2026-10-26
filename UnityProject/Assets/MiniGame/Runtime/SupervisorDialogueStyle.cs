using UnityEngine;
using UnityEngine.UI;

namespace MiniGame.UnityHost
{
    /// <summary>Shared doorway and dream dialogue presentation.</summary>
    internal static class SupervisorDialogueStyle
    {
        internal const int FontSize = 50;
        internal const float PixelStep = 2f * 1.4f;
        internal static readonly Color Red = new Color32(0xF2, 0x45, 0x35, 0xFF);

        internal static Material CreateTextMaterial()
        {
            var shader = Resources.Load<Shader>("PropNarration");
            if (shader == null || !shader.isSupported) return null;
            var material = new Material(shader) {
                name = "Supervisor red pixel text", hideFlags = HideFlags.HideAndDontSave
            };
            material.SetFloat("_PixelStep", PixelStep);
            return material;
        }

        internal static void ApplyText(Text body, Font font, Material material)
        {
            body.font = font;
            body.material = material;
            body.fontStyle = FontStyle.Bold;
            body.fontSize = FontSize;
            body.color = Red;
            body.alignment = TextAnchor.MiddleCenter;
            body.lineSpacing = 1.1f;
            body.horizontalOverflow = HorizontalWrapMode.Wrap;
            body.verticalOverflow = VerticalWrapMode.Overflow;
            body.resizeTextForBestFit = false;
            body.raycastTarget = false;
            var rect = body.rectTransform;
            rect.anchorMin = new Vector2(.025f, .07f);
            rect.anchorMax = new Vector2(.975f, .93f);
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }

        internal static void Layout(RectTransform panel, string text)
        {
            int longest = 0;
            foreach (string line in (text ?? string.Empty).Split('\n')) longest = Mathf.Max(longest, line.Length);
            int lines = Mathf.Clamp(Mathf.CeilToInt(longest / 11f), 1, 4);
            float halfHeight = (.11f + (lines - 1) * .06f) * .5f;
            panel.anchorMin = new Vector2(.655f, .51f - halfHeight);
            panel.anchorMax = new Vector2(.97f, .51f + halfHeight);
            panel.offsetMin = panel.offsetMax = Vector2.zero;
            Canvas.ForceUpdateCanvases();
        }

        internal static void ApplyBackdrop(Image image)
        {
            image.color = Color.black;
            image.material = null;
            image.raycastTarget = false;
            var rect = image.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(0, -60);
            rect.offsetMax = new Vector2(0, 60);
        }
    }
}
