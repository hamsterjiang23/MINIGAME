using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MiniGame.UnityHost
{
    /// <summary>A two-line, read-only window over the complete authored intake summary.</summary>
    public sealed class TerminalDossierSummary : MonoBehaviour, IScrollHandler
    {
        private Text label;
        private Button up, down;
        private List<TerminalTextMetrics.Line> lines = new List<TerminalTextMetrics.Line>();
        private string patientId;
        private bool inputEnabled;
        public string FullText { get; private set; } = "";
        public int FirstLine { get; private set; }
        public int LineCount => lines.Count;
        public int MaximumFirstLine => Mathf.Max(0, lines.Count - 2);

        public void Initialize(Text textLabel, Font font)
        {
            label = textLabel;
            label.horizontalOverflow = HorizontalWrapMode.Overflow;
            label.verticalOverflow = VerticalWrapMode.Truncate;
            label.raycastTarget = false;
            if (label is TerminalBitmapText bitmap) bitmap.LinePitch = 20;
            up = Arrow("SummaryScrollUp", "↑", 4, -1, font);
            down = Arrow("SummaryScrollDown", "↓", 30, 1, font);
            Paint();
        }

        private Button Arrow(string name, string text, float y, int amount, Font font)
        {
            var root = TerminalIndustrialSkin.Panel(transform, name, Color.clear, 268, y, 20, 22);
            root.GetComponent<Image>().raycastTarget = true;
            var button = root.gameObject.AddComponent<Button>();
            button.transition = Selectable.Transition.None;
            button.navigation = new Navigation { mode = Navigation.Mode.None };
            button.onClick.AddListener(() => Scroll(amount));
            var arrow = TerminalIndustrialSkin.Label(root, font, "Arrow", text, 3, 4, 14, 14, 12,
                TerminalIndustrialSkin.ColorOf("170e02"));
            arrow.raycastTarget = false;
            return button;
        }

        public void SetContent(string id, string text)
        {
            text = text ?? "";
            if (patientId == id && FullText == text) return;
            patientId = id; FullText = text; FirstLine = 0;
            // Leave room for the scroll arrows only when the original 270px text field overflows.
            lines = TerminalTextMetrics.Layout(FullText, 14, 270);
            bool overflow = lines.Count > 2;
            if (overflow) lines = TerminalTextMetrics.Layout(FullText, 14, 256);
            label.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, overflow ? 256 : 270);
            Paint();
        }

        public void ResetScroll() { FirstLine = 0; Paint(); }
        public void SetInputEnabled(bool value) { inputEnabled = value; Paint(); }

        public void OnScroll(PointerEventData data)
        {
            if (!inputEnabled || !isActiveAndEnabled || Mathf.Approximately(data.scrollDelta.y, 0)) return;
            Scroll(data.scrollDelta.y > 0 ? -1 : 1);
            data.Use();
        }

        public void Scroll(int amount)
        {
            if (!inputEnabled || !isActiveAndEnabled) return;
            FirstLine = Mathf.Clamp(FirstLine + amount, 0, MaximumFirstLine);
            Paint();
        }

        private void Paint()
        {
            if (label == null || up == null || down == null) return;
            var visible = new StringBuilder();
            for (int index = FirstLine; index < Mathf.Min(lines.Count, FirstLine + 2); index++)
            {
                if (visible.Length > 0) visible.Append('\n');
                var line = lines[index];
                visible.Append(FullText, line.Start, line.Length);
            }
            label.text = visible.ToString();
            up.gameObject.SetActive(MaximumFirstLine > 0);
            down.gameObject.SetActive(MaximumFirstLine > 0);
            up.interactable = inputEnabled && FirstLine > 0;
            down.interactable = inputEnabled && FirstLine < MaximumFirstLine;
            up.GetComponentInChildren<Text>().color = TerminalIndustrialSkin.ColorOf(up.interactable ? "170e02" : "a77d2d");
            down.GetComponentInChildren<Text>().color = TerminalIndustrialSkin.ColorOf(down.interactable ? "170e02" : "a77d2d");
        }
    }
}
