using System.Collections.Generic;
using UnityEngine;

namespace MiniGame.UnityHost
{
    /// <summary>The HTML terminal advances each character independently, without font kerning.</summary>
    public static class TerminalTextMetrics
    {
        public readonly struct Line
        {
            public readonly int Start, Length;
            public readonly float Width;
            public Line(int start, int length, float width) { Start = start; Length = length; Width = width; }
        }

        public static float Cell(char character, int size) => character > 255 ? size : Mathf.Ceil(size * .6f);

        public static float Width(string value, int size)
        {
            float width = 0, maximum = 0;
            foreach (char character in value ?? "")
            {
                if (character == '\r') continue;
                if (character == '\n') { maximum = Mathf.Max(maximum, width); width = 0; }
                else width += Cell(character, size);
            }
            return Mathf.Max(maximum, width);
        }

        public static List<Line> Layout(string value, int size, float width, bool wrap = true)
        {
            var lines = new List<Line>();
            if (string.IsNullOrEmpty(value)) return lines;
            int start = 0; float x = 0;
            for (int index = 0; index < value.Length; index++)
            {
                char character = value[index];
                if (character == '\r') continue;
                if (character == '\n')
                {
                    lines.Add(new Line(start, index - start, x));
                    start = index + 1; x = 0; continue;
                }
                float advance = Cell(character, size);
                if (wrap && x > 0 && x + advance > Mathf.Max(0, width))
                {
                    lines.Add(new Line(start, index - start, x));
                    start = index; x = 0;
                }
                x += advance;
            }
            lines.Add(new Line(start, value.Length - start, x));
            return lines;
        }

        public static int MeasureLineCount(string value, int size, float width, bool wrap = true)
            => Layout(value, size, width, wrap).Count;

        public static float MeasureHeight(string value, int size, float width, float linePitch = 20, bool wrap = true)
            => MeasureLineCount(value, size, width, wrap) * linePitch;
    }
}
