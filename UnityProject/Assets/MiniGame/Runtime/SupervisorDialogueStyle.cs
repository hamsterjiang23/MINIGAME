using UnityEngine;
using UnityEngine.UI;

namespace MiniGame.UnityHost
{
    /// <summary>Shared doorway and dream dialogue presentation.</summary>
    internal static class SupervisorDialogueStyle
    {
        internal const int FontSize = 50;
        internal const float PixelStep = 2f * 1.4f;
        internal const float MinimumVerticalPadding = FontSize * .75f;
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
            rect.anchorMin = new Vector2(.025f, 0);
            rect.anchorMax = new Vector2(.975f, 1);
            rect.offsetMin = new Vector2(0, MinimumVerticalPadding);
            rect.offsetMax = new Vector2(0, -MinimumVerticalPadding);
        }

        internal static void Layout(RectTransform panel, Text body, string fullText)
        {
            var parent = (RectTransform)panel.parent;
            // Keep the short-line silhouette. Extra lines consume its generous top/bottom
            // space first; only grow once the reference's minimum padding is reached.
            float width = Mathf.Max(1, parent.rect.width * (.97f - .655f) * .95f);
            var settings = body.GetGenerationSettings(new Vector2(width, 0));
            settings.horizontalOverflow = HorizontalWrapMode.Wrap;
            settings.verticalOverflow = VerticalWrapMode.Overflow;
            float textHeight = Mathf.Max(FontSize,
                body.cachedTextGeneratorForLayout.GetPreferredHeight(fullText ?? string.Empty, settings) / body.pixelsPerUnit);
            // Legacy Text's preferred height omits some serif ascender/descender overhang.
            // Leave a small font-relative guard inside the text area, separate from the black padding.
            textHeight += FontSize * .4f;
            float baselineHeight = parent.rect.height * .11f + 120;
            float height = Mathf.Max(baselineHeight, textHeight + MinimumVerticalPadding * 2 + 2);
            panel.anchorMin = new Vector2(.655f, .51f);
            panel.anchorMax = new Vector2(.97f, .51f);
            panel.offsetMin = new Vector2(0, -height * .5f);
            panel.offsetMax = new Vector2(0, height * .5f);
        }

        internal static void ApplyBackdrop(Image image)
        {
            image.color = Color.black;
            image.material = null;
            image.raycastTarget = false;
            var rect = image.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }
    }
}
