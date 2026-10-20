using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MiniGame.UnityHost
{
    internal sealed class TerminalButtonVisual : MiniGameAudioButton
    {
        private readonly List<Text> labels = new List<Text>();
        private Image face;
        private bool selected, hovered;
        public void Initialize(Font font, string value, float width, float height, int size, bool isRow)
        {
            face = GetComponent<Image>();
            var label = TerminalIndustrialSkin.Label(transform, font, "Label", value, 8, 4, width - 16, height - 6, size, TerminalView.Ink);
            label.alignment = TextAnchor.MiddleLeft; label.horizontalOverflow = HorizontalWrapMode.Overflow;
            labels.Add(label); Paint();
        }
        public void SetSelected(bool value) { selected = value; Paint(); }
        public void SetChoiceRow() { Paint(); }
        public void RegisterLabel(Text text) { if (text != null && !labels.Contains(text)) labels.Add(text); Paint(); }
        public override void OnPointerEnter(PointerEventData e) { hovered = true; base.OnPointerEnter(e); Paint(); }
        public override void OnPointerExit(PointerEventData e) { hovered = false; base.OnPointerExit(e); Paint(); }
        // Terminal keyboard confirmation is owned by MiniGameScreen, including a newly created next row.
        public override void OnSubmit(BaseEventData e) { e?.Use(); }
        protected override void OnDisable() { hovered = false; base.OnDisable(); }
        protected override void DoStateTransition(SelectionState state, bool instant) => Paint();
        private void Paint()
        {
            if (face == null) return;
            bool active = IsInteractable() && (selected || hovered || IsPressed());
            face.color = active ? TerminalIndustrialSkin.ColorOf("eeb430") : TerminalView.Desk;
            Color color = !IsInteractable() ? TerminalView.Edge : active ? TerminalIndustrialSkin.ColorOf("170e02") : TerminalView.Ink;
            foreach (Text label in labels) if (label != null) label.color = color;
        }
    }
}
