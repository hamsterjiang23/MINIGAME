using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MiniGame.UnityHost
{
    internal sealed class TerminalButtonVisual : MiniGameAudioButton, IPointerMoveHandler
    {
        internal System.Action PointerSelected;
        private readonly List<Text> labels = new List<Text>();
        private readonly Dictionary<Text, Vector2> restingLabelPositions = new Dictionary<Text, Vector2>();
        private Image face;
        private TerminalHighlightGroup highlightGroup;
        private RectTransform pressedFrame, commandFrame;
        private Image[] commandEdges;
        private bool selected, hovered, pressed, rowStyle, commandField, alwaysHighlighted;

        public void Initialize(Font font, string value, float width, float height, int size, bool isRow)
        {
            face = GetComponent<Image>();
            rowStyle = isRow;
            highlightGroup = GetComponentInParent<TerminalHighlightGroup>();
            highlightGroup?.Register(this);
            var label = TerminalIndustrialSkin.Label(transform, font, "Label", value, 8, 4, width - 16, height - 6, size, TerminalView.Ink);
            label.alignment = TextAnchor.UpperLeft;
            label.horizontalOverflow = HorizontalWrapMode.Overflow;
            if (label is TerminalBitmapText bitmap) bitmap.CommandRow = isRow;
            labels.Add(label);
            pressedFrame = CreateFrame("PressedInset", 1, TerminalIndustrialSkin.ColorOf("170e02"));
            commandFrame = CreateFrame("CommandOutline", 0, TerminalView.Bright);
            commandEdges = commandFrame.GetComponentsInChildren<Image>();
            Paint();
        }

        public void SetSelected(bool value) { selected = value; Paint(); }
        public void SetChoiceRow()
        {
            rowStyle = true; commandField = false;
            if (labels.Count > 0 && labels[0] is TerminalBitmapText bitmap) bitmap.CommandRow = true;
            Paint();
        }
        public void SetCommandField(bool value = true)
        {
            commandField = value;
            if (value) ResetPointerState();
            Paint();
        }
        public void SetAlwaysHighlighted(bool value = true)
        {
            alwaysHighlighted = value;
            if (value) ResetPointerState();
            Paint();
        }

        public void RegisterLabel(Text text)
        {
            if (text != null && !labels.Contains(text))
            {
                labels.Add(text);
                if (pressed) OffsetLabel(text);
            }
            Paint();
        }

        public override void OnPointerEnter(PointerEventData e)
        {
            base.OnPointerEnter(e);
            HighlightPointer();
        }

        public void OnPointerMove(PointerEventData e)
        {
            if (e.delta.sqrMagnitude > 0) HighlightPointer();
        }

        private void HighlightPointer()
        {
            if (IsActive() && IsInteractable() && ParticipatesInHighlight)
            {
                hovered = true;
                highlightGroup?.SetPointer(this);
                PointerSelected?.Invoke();
            }
            Paint();
        }

        public override void OnPointerExit(PointerEventData e)
        {
            hovered = false;
            ResetPressed();
            highlightGroup?.ReleasePointer(this);
            base.OnPointerExit(e);
            Paint();
        }

        public override void OnPointerDown(PointerEventData e)
        {
            base.OnPointerDown(e);
            if (e.button == PointerEventData.InputButton.Left && IsActive() && IsInteractable())
            {
                pressed = true;
                foreach (var label in labels) OffsetLabel(label);
            }
            Paint();
        }

        public override void OnPointerUp(PointerEventData e)
        {
            if (e.button == PointerEventData.InputButton.Left) ResetPressed();
            base.OnPointerUp(e);
            Paint();
        }

        // MiniGameScreen owns keyboard confirmation, including a newly created next row.
        public override void OnSubmit(BaseEventData e) { e?.Use(); }

        protected override void OnEnable()
        {
            base.OnEnable();
            highlightGroup?.Register(this);
            Paint();
        }

        protected override void OnDisable()
        {
            hovered = false;
            ResetPressed();
            highlightGroup?.Unregister(this);
            base.OnDisable();
            Paint();
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus) ResetPointerState();
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused) ResetPointerState();
        }

        protected override void DoStateTransition(SelectionState state, bool instant)
        {
            if (state == SelectionState.Disabled) ResetPointerState();
            Paint();
        }

        internal bool ParticipatesInHighlight => rowStyle && !commandField && !alwaysHighlighted;

        internal void ClearPointerState()
        {
            hovered = false;
            ResetPressed();
        }

        internal void Paint()
        {
            if (face == null) return;
            bool enabled = IsActive() && IsInteractable();
            bool pointerActive = highlightGroup != null ? highlightGroup.IsPointer(this) : hovered;
            bool keyboardActive = selected && (highlightGroup == null || !highlightGroup.HasPointer);
            bool active = alwaysHighlighted || enabled && !commandField && (pointerActive || keyboardActive);
            face.color = active ? TerminalIndustrialSkin.ColorOf("eeb430") : TerminalView.Desk;
            Color color = alwaysHighlighted ? TerminalIndustrialSkin.ColorOf("170e02") :
                !enabled ? TerminalView.Edge : active ? TerminalIndustrialSkin.ColorOf("170e02") :
                commandField ? TerminalView.Bright : TerminalView.Ink;
            foreach (Text label in labels) if (label != null) label.color = color;
            if (pressedFrame != null) pressedFrame.gameObject.SetActive(enabled && pressed);
            if (commandFrame != null)
            {
                commandFrame.gameObject.SetActive(commandField);
                foreach (var edge in commandEdges)
                    edge.color = enabled ? TerminalView.Bright : TerminalView.Edge;
            }
        }

        private void ResetPointerState()
        {
            ClearPointerState();
            highlightGroup?.ReleasePointer(this);
            Paint();
        }

        private void OffsetLabel(Text label)
        {
            if (label == null || restingLabelPositions.ContainsKey(label)) return;
            restingLabelPositions.Add(label, label.rectTransform.anchoredPosition);
            label.rectTransform.anchoredPosition += new Vector2(1, -1);
        }

        private void ResetPressed()
        {
            pressed = false;
            foreach (var pair in restingLabelPositions)
                if (pair.Key != null) pair.Key.rectTransform.anchoredPosition = pair.Value;
            restingLabelPositions.Clear();
        }

        private RectTransform CreateFrame(string frameName, float inset, Color color)
        {
            var root = new GameObject(frameName, typeof(RectTransform));
            root.layer = gameObject.layer;
            root.transform.SetParent(transform, false);
            var rect = (RectTransform)root.transform;
            rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.one * inset; rect.offsetMax = -Vector2.one * inset;
            AddEdge(rect, "Top", new Vector2(0, 1), Vector2.one, new Vector2(0, -1), Vector2.zero, color);
            AddEdge(rect, "Bottom", Vector2.zero, new Vector2(1, 0), Vector2.zero, new Vector2(0, 1), color);
            AddEdge(rect, "Left", Vector2.zero, new Vector2(0, 1), Vector2.zero, new Vector2(1, 0), color);
            AddEdge(rect, "Right", new Vector2(1, 0), Vector2.one, new Vector2(-1, 0), Vector2.zero, color);
            return rect;
        }

        private static void AddEdge(RectTransform parent, string edgeName, Vector2 anchorMin,
            Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, Color color)
        {
            var edge = new GameObject(edgeName, typeof(RectTransform), typeof(Image));
            edge.layer = parent.gameObject.layer;
            edge.transform.SetParent(parent, false);
            var rect = (RectTransform)edge.transform;
            rect.anchorMin = anchorMin; rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin; rect.offsetMax = offsetMax;
            var image = edge.GetComponent<Image>(); image.color = color; image.raycastTarget = false;
        }
    }
}
