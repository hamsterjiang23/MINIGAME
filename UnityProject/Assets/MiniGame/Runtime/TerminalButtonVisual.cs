using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MiniGame.UnityHost
{
    /// <summary>Explicit color states avoid uGUI ColorTint multiplying the physical rim.</summary>
    internal sealed class TerminalButtonVisual : MiniGameAudioButton
    {
        private Image mount;
        private Image face;
        private Image top;
        private Image left;
        private Image bottom;
        private Image right;
        private Image gutter;
        private Image underline;
        private Text label;
        private GameObject focusOutline;
        private bool rowStyle;
        private bool choiceRow;
        private bool selected;
        private bool hovered;
        private bool initialized;
        private readonly List<(Text Text, Color Idle)> additionalLabels = new List<(Text, Color)>();

        public void Initialize(Font font, string value, float width, float height, int size, bool isRow)
        {
            rowStyle = isRow;
            if (rowStyle)
            {
                face = GetComponent<Image>();
                bottom = Part("RowSeparator", 0, height - 1, width, 1);
            }
            else
            {
                mount = Part("RecessedMount", 1, 1, width - 2, height - 2);
                face = Part("Actuator", 3, 2, width - 6, height - 5);
                top = Part("TopRim", 3, 2, width - 6, 1);
                left = Part("LeftRim", 3, 2, 1, height - 5);
                bottom = Part("BottomRim", 3, height - 4, width - 6, 1);
                right = Part("RightRim", width - 4, 2, 1, height - 5);
                gutter = Part("RegistrationGutter", 5, 5, 2, Mathf.Max(2, height - 11));
                underline = Part("ActuatorRule", 9, height - 6, width - 18, 1);
            }
            focusOutline = CreateFocusOutline(width, height, rowStyle ? 2 : 8, rowStyle ? 1 : 3);
            label = TerminalIndustrialSkin.Label(transform, font, "Label", value,
                8, 0, width - 16, height - (rowStyle ? 0 : 2), size,
                TerminalIndustrialSkin.ColorOf("26312c"));
            label.alignment = rowStyle ? TextAnchor.MiddleLeft : TextAnchor.MiddleCenter;
            initialized = true;
            Paint();
        }

        public void SetSelected(bool value)
        {
            if (selected == value) return;
            selected = value;
            Paint();
        }

        public void SetChoiceRow()
        {
            choiceRow = true;
            Paint();
        }

        public void RegisterLabel(Text text)
        {
            if (text == null || text == label) return;
            foreach (var entry in additionalLabels)
                if (entry.Text == text) return;
            additionalLabels.Add((text, text.color));
            text.raycastTarget = false;
            Paint();
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            hovered = true;
            base.OnPointerEnter(eventData);
            Paint();
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            hovered = false;
            base.OnPointerExit(eventData);
            Paint();
        }

        protected override void OnDisable()
        {
            hovered = false;
            base.OnDisable();
        }

        protected override void DoStateTransition(SelectionState state, bool instant)
        {
            Paint();
        }

        private void Paint()
        {
            if (!initialized) return;
            bool enabled = IsInteractable() && IsActive();
            bool hover = enabled && hovered;
            bool down = enabled && IsPressed();
            bool active = selected || down;
            if (rowStyle)
            {
                bool dark = enabled && (choiceRow ? hover || down : active);
                face.color = ColorOf(!enabled ? "c7cbc4" : dark ? "455b4e" :
                    choiceRow && selected ? "c0cac0" : hover ? "b0bdb1" : "d1d3cd");
                bottom.color = ColorOf("707b72");
                PaintLabels(ColorOf(!enabled ? "929990" : dark ? "e0e3da" : "26312c"), enabled && !dark);
                focusOutline.SetActive(enabled && choiceRow && selected && !hover);
                return;
            }

            mount.color = ColorOf(enabled ? "aeb7ac" : "b2b9b0");
            face.color = ColorOf(active ? "394e43" : hover ? "a6b8a8" : enabled ? "d7dcd3" : "c7cbc4");
            top.color = left.color = ColorOf(active ? "263e34" : enabled ? "f5f4ea" : "d0d4ca");
            bottom.color = right.color = ColorOf(active ? "80988b" : "56645a");
            gutter.color = ColorOf(active ? "b5c8b8" : !enabled ? "a9b0a7" : hover ? "516d5d" : "8b9b8e");
            underline.color = ColorOf(active ? "657d6d" : "bbc5b8");
            PaintLabels(ColorOf(!enabled ? "929990" : active ? "f1f0e7" : "26312c"), enabled && !active);
            focusOutline.SetActive(hover && !active);
        }

        private void PaintLabels(Color stateColor, bool idle)
        {
            label.color = stateColor;
            foreach (var entry in additionalLabels)
                if (entry.Text != null) entry.Text.color = idle ? entry.Idle : stateColor;
        }

        private Image Part(string name, float x, float y, float width, float height) =>
            TerminalIndustrialSkin.Panel(transform, name, Color.white, x, y, width, height).GetComponent<Image>();

        private GameObject CreateFocusOutline(float width, float height, float insetX, float insetY)
        {
            var root = new GameObject("FocusOutline", typeof(RectTransform), typeof(TerminalFocusOutline));
            root.transform.SetParent(transform, false);
            TerminalIndustrialSkin.Position((RectTransform)root.transform, 0, 0, width, height);
            var outline = root.GetComponent<TerminalFocusOutline>();
            outline.color = ColorOf("4a6554");
            outline.raycastTarget = false;
            outline.Inset = new Vector2(insetX, insetY);
            root.SetActive(false);
            return root;
        }

        private static Color ColorOf(string hex) => TerminalIndustrialSkin.ColorOf(hex);
    }

    internal sealed class TerminalFocusOutline : MaskableGraphic
    {
        public Vector2 Inset;

        protected override void OnPopulateMesh(VertexHelper helper)
        {
            helper.Clear();
            var rect = rectTransform.rect;
            float endX = rect.width - Inset.x - 1;
            float endY = rect.height - Inset.y - 2;
            for (float x = Inset.x; x < endX; x += 4)
            {
                AddDash(helper, rect, x, Inset.y, Mathf.Min(2, endX - x), 1);
                AddDash(helper, rect, x, endY, Mathf.Min(2, endX - x), 1);
            }
            for (float y = Inset.y + 4; y < endY; y += 4)
            {
                AddDash(helper, rect, Inset.x, y, 1, Mathf.Min(2, endY - y));
                AddDash(helper, rect, endX, y, 1, Mathf.Min(2, endY - y));
            }
        }

        private void AddDash(VertexHelper helper, Rect rect, float x, float y, float width, float height)
        {
            int start = helper.currentVertCount;
            float left = rect.xMin + x;
            float top = rect.yMax - y;
            helper.AddVert(new Vector3(left, top), color, Vector2.zero);
            helper.AddVert(new Vector3(left + width, top), color, Vector2.zero);
            helper.AddVert(new Vector3(left + width, top - height), color, Vector2.zero);
            helper.AddVert(new Vector3(left, top - height), color, Vector2.zero);
            helper.AddTriangle(start, start + 1, start + 2);
            helper.AddTriangle(start + 2, start + 3, start);
        }
    }
}
