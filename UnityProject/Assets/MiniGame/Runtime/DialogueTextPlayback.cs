using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

namespace MiniGame.UnityHost
{
    /// <summary>Pages display text without altering the complete line retained in the transcript.</summary>
    internal sealed class DialogueTextPlayback
    {
        private Text target;
        private Action completed;
        private readonly List<string> pages = new List<string>();
        private int[] elements = Array.Empty<int>();
        private int page;
        private int visible;
        private float accumulated;
        private float speed;
        public bool Active => completed != null;
        public int PageCount => pages.Count;
        public int PageNumber => page + 1;
        public bool PageRevealed => visible >= elements.Length;

        public void Begin(Text label, string text, float charactersPerSecond, Action finish, bool paginate = true)
        {
            Cancel();
            target = label;
            speed = Mathf.Max(0, charactersPerSecond);
            completed = finish;
            if (paginate) pages.AddRange(Paginate(label, text));
            else pages.Add(text ?? string.Empty);
            ShowPage();
        }

        public void Tick(float elapsed)
        {
            if (!Active || PageRevealed) return;
            accumulated += elapsed * speed;
            int next = Mathf.Min(elements.Length, Mathf.FloorToInt(accumulated));
            if (next != visible) { visible = next; Paint(); }
        }

        public void Advance()
        {
            if (!Active) return;
            if (!PageRevealed) { visible = elements.Length; Paint(); return; }
            if (page + 1 < pages.Count) { page++; ShowPage(); return; }
            var finish = completed;
            completed = null;
            finish();
        }

        public void Cancel()
        {
            completed = null; target = null;
            pages.Clear(); elements = Array.Empty<int>(); page = visible = 0; accumulated = 0;
        }

        private void ShowPage()
        {
            elements = StringInfo.ParseCombiningCharacters(pages[page]);
            accumulated = 0;
            visible = speed <= 0 ? elements.Length : 0;
            Paint();
        }

        private void Paint()
        {
            if (target == null) return;
            string value = pages[page];
            target.text = visible >= elements.Length ? value : value.Substring(0, elements[visible]);
        }

        internal static IReadOnlyList<string> Paginate(Text label, string source)
        {
            var result = new List<string>();
            string remaining = source ?? string.Empty;
            var bitmap = label as TerminalBitmapText;
            var settings = default(TextGenerationSettings);
            if (bitmap == null)
            {
                settings = label.GetGenerationSettings(new Vector2(Mathf.Max(1, label.rectTransform.rect.width), 0));
                settings.verticalOverflow = VerticalWrapMode.Overflow;
                settings.horizontalOverflow = HorizontalWrapMode.Wrap;
            }
            using var generator = bitmap == null ? new TextGenerator() : null;
            float height = Mathf.Max(1, label.rectTransform.rect.height);
            while (remaining.Length > 0)
            {
                int[] boundaries = StringInfo.ParseCombiningCharacters(remaining);
                int low = 1, high = boundaries.Length, fit = 1;
                while (low <= high)
                {
                    int count = (low + high) / 2;
                    int end = count == boundaries.Length ? remaining.Length : boundaries[count];
                    string candidate = remaining.Substring(0, end);
                    float measured = bitmap != null ? BitmapHeight(bitmap, candidate) :
                        generator.GetPreferredHeight(candidate, settings) / label.pixelsPerUnit;
                    if (measured <= height + .01f) { fit = count; low = count + 1; }
                    else high = count - 1;
                }
                int split = fit == boundaries.Length ? remaining.Length : boundaries[fit];
                result.Add(remaining.Substring(0, split));
                remaining = remaining.Substring(split);
            }
            if (result.Count == 0) result.Add(string.Empty);
            return result;
        }

        private static float BitmapHeight(TerminalBitmapText label, string value)
        {
            var lines = TerminalTextMetrics.Layout(value, label.fontSize, label.rectTransform.rect.width,
                label.horizontalOverflow == HorizontalWrapMode.Wrap);
            int count = lines.Count;
            // Keep the separator on the preceding page without reserving space for its empty trailing row.
            // Interior blank rows still consume their normal line pitch, and the source is never rewritten.
            if (count > 1 && lines[count - 1].Length == 0) count--;
            return count == 0 ? 0 : label.fontSize + (count - 1) * label.LinePitch * Mathf.Max(.01f, label.lineSpacing);
        }
    }
}
