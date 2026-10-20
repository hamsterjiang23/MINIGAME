using System.Collections.Generic;
using MiniGame.Presentation;
using UnityEngine;
using UnityEngine.UI;

namespace MiniGame.UnityHost
{
    internal sealed class TerminalTranscriptView
    {
        private sealed class Row
        {
            public RectTransform Root;
            public Text Speaker, Body;
        }

        private readonly Font font;
        private readonly RectTransform viewport, content, thumb;
        private readonly ScrollRect scroll;
        private readonly List<Row> rows = new List<Row>();
        private bool rendering;
        private string signature;
        public GameObject GameObject => viewport.gameObject;

        public TerminalTranscriptView(RectTransform parent, Font typeface)
        {
            font = typeface;
            viewport = TerminalIndustrialSkin.Panel(parent, "TranscriptPanel", Color.clear, 23, 395, 478, 135);
            viewport.GetComponent<Image>().raycastTarget = true;
            viewport.gameObject.AddComponent<RectMask2D>();
            content = NewRect(viewport, "TranscriptContents");
            content.anchorMin = new Vector2(0, 1); content.anchorMax = Vector2.one;
            content.pivot = new Vector2(.5f, 1); content.sizeDelta = Vector2.zero;
            scroll = viewport.gameObject.AddComponent<ScrollRect>();
            scroll.viewport = viewport; scroll.content = content;
            scroll.horizontal = false; scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.inertia = false; scroll.scrollSensitivity = 28;
            scroll.onValueChanged.AddListener(position =>
            {
                if (!rendering) UpdateThumb();
            });
            thumb = TerminalIndustrialSkin.Panel(viewport, "HistoryThumb", TerminalView.Dim, 475, 0, 2, 135);
            thumb.GetComponent<Image>().raycastTarget = false;
        }

        public void Render(IReadOnlyList<TranscriptEntry> entries, string activeNode, string currentPage)
        {
            var last = entries.Count > 0 ? entries[entries.Count - 1] : null;
            string next = entries.Count + "|" + last?.Key + "|" + last?.Completed + "|" + activeNode + "|" + currentPage;
            if (signature == next) return;
            signature = next;
            bool follow = content.rect.height <= viewport.rect.height || scroll.verticalNormalizedPosition <= .01f;
            float offset = content.anchoredPosition.y;
            rendering = true;
            try
            {
                float y = 4;
                for (int index = 0; index < entries.Count; index++)
                {
                    var row = EnsureRow(index);
                    var entry = entries[index];
                    // Completed speech retains every paragraph, rather than the final playback page.
                    bool playing = index == entries.Count - 1 && !entry.Completed && !entry.IsPlayer
                        && !entry.IsNarration && entry.SourceNodeId == activeNode;
                    row.Body.text = (playing ? currentPage : entry.Text).Replace("\r\n", "\n").Replace('\r', '\n');
                    row.Body.color = index == entries.Count - 1 ? TerminalView.Bright : TerminalView.Ink;
                    row.Speaker.text = entry.IsPlayer ? "值班员" : entry.IsNarration ? "终端" : "对象";
                    var settings = row.Body.GetGenerationSettings(new Vector2(415, 0));
                    settings.verticalOverflow = VerticalWrapMode.Overflow;
                    float height = Mathf.Max(24, row.Body.cachedTextGeneratorForLayout.GetPreferredHeight(row.Body.text, settings)
                        / row.Body.pixelsPerUnit + 6);
                    TerminalIndustrialSkin.Position(row.Root, 0, y, 475, height);
                    TerminalIndustrialSkin.Position(row.Body.rectTransform, 56, 0, 415, height);
                    row.Root.gameObject.SetActive(true);
                    y += height;
                }
                for (int index = entries.Count; index < rows.Count; index++) rows[index].Root.gameObject.SetActive(false);
                content.sizeDelta = new Vector2(0, y + 4);
                if (follow) scroll.verticalNormalizedPosition = 0;
                else content.anchoredPosition = new Vector2(0, Mathf.Clamp(offset, 0, Mathf.Max(0, content.rect.height - viewport.rect.height)));
                UpdateThumb();
            }
            finally { rendering = false; }
        }

        public void Scroll(int amount)
        {
            scroll.verticalNormalizedPosition = Mathf.Clamp01(scroll.verticalNormalizedPosition + amount * .15f);
            UpdateThumb();
        }

        public void Reset()
        {
            signature = null;
            scroll.verticalNormalizedPosition = 0;
        }

        private Row EnsureRow(int index)
        {
            if (index < rows.Count) return rows[index];
            var root = NewRect(content, "TranscriptRow" + index);
            var speaker = TerminalIndustrialSkin.Label(root, font, "TranscriptSpeaker" + index, "", 3, 0, 52, 18, 11, TerminalView.Dim);
            var body = TerminalIndustrialSkin.Label(root, font, index == 0 ? "TranscriptText" : "TranscriptText" + index,
                "", 56, 0, 415, 24, 14, TerminalView.Ink);
            body.horizontalOverflow = HorizontalWrapMode.Wrap; body.verticalOverflow = VerticalWrapMode.Overflow;
            body.alignment = TextAnchor.UpperLeft;
            speaker.raycastTarget = body.raycastTarget = false;
            var row = new Row { Root = root, Speaker = speaker, Body = body }; rows.Add(row); return row;
        }

        private void UpdateThumb()
        {
            float full = content.rect.height, visible = viewport.rect.height;
            thumb.gameObject.SetActive(full > visible + 1);
            if (full <= visible) return;
            float height = Mathf.Max(12, visible * visible / full);
            TerminalIndustrialSkin.Position(thumb, 475, (1 - scroll.verticalNormalizedPosition) * (visible - height), 2, height);
        }

        private static RectTransform NewRect(Transform parent, string name)
        {
            var item = new GameObject(name, typeof(RectTransform)); item.transform.SetParent(parent, false);
            return (RectTransform)item.transform;
        }
    }
}
