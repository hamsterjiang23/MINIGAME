using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MiniGame.UnityHost
{
    /// <summary>Hover to read a clipped response without moving its number or click target.</summary>
    public sealed class TerminalChoiceMarquee : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private const float StartDelay = .65f, EndDelay = 1.6f, PixelsPerSecond = 24;
        private Button button;
        private RectTransform viewport, content;
        private Text label;
        private bool hovered;
        private float elapsed;

        public void Initialize(Button owner, RectTransform clip, RectTransform movingContent, Text text)
        {
            button = owner; viewport = clip; content = movingContent; label = text;
            ResetPosition();
        }

        public void OnPointerEnter(PointerEventData data) { hovered = true; ResetPosition(); }
        public void OnPointerExit(PointerEventData data) { hovered = false; ResetPosition(); }
        private void OnDisable() { hovered = false; ResetPosition(); }
        private void OnApplicationFocus(bool focused)
        {
            if (!focused) { hovered = false; ResetPosition(); }
        }

        private void Update()
        {
            if (content == null || label == null || button == null) return;
            if (!hovered || !button.IsActive() || !button.IsInteractable()) { ResetPosition(); return; }
            float distance = Mathf.Max(0, label.preferredWidth - viewport.rect.width);
            if (distance <= 0) { ResetPosition(); return; }
            elapsed += Time.unscaledDeltaTime;
            float travelTime = distance / PixelsPerSecond;
            float cycle = StartDelay + travelTime + EndDelay;
            if (elapsed >= cycle) elapsed %= cycle;
            // Whole logical pixels keep the bitmap glyphs crisp while moving across the CRT.
            float offset = Mathf.Floor(Mathf.Clamp((elapsed - StartDelay) * PixelsPerSecond, 0, distance));
            content.anchoredPosition = new Vector2(-offset, 0);
        }

        private void ResetPosition()
        {
            elapsed = 0;
            if (content != null) content.anchoredPosition = Vector2.zero;
        }
    }
}
