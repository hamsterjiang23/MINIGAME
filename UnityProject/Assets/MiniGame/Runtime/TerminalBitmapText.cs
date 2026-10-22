using PMSAmber;
using UnityEngine;
using UnityEngine.UI;

namespace MiniGame.UnityHost
{
    /// <summary>Pixel atlas renderer retaining the terminal's logical cell grid and pagination.</summary>
    [AddComponentMenu("MiniGame/Terminal Bitmap Text")]
    public sealed class TerminalBitmapText : Text
    {
        private const string FontResource = "PMSAmberTerminal/HTMLBitmapFont";
        private static readonly Color32[] ExportedColors =
        {
            new Color32(239,184,63,255), new Color32(255,207,104,255),
            new Color32(167,125,45,255), new Color32(103,75,25,255), new Color32(23,14,2,255)
        };
        [SerializeField] private PMSBitmapFont bitmapFont;
        [SerializeField] private float linePitch = 20;
        [SerializeField] private bool commandRow;
        [SerializeField] private bool scaleToFontSize;
        public bool ScaleToFontSize
        {
            get => scaleToFontSize;
            set { if (scaleToFontSize == value) return; scaleToFontSize = value; SetAllDirty(); }
        }
        private Mesh bitmapMesh;
        private bool warnedMissingResource, warnedSize;
        private TerminalPixelFont arkTypeface;
        public TerminalPixelFont ArkTypeface => arkTypeface != null ? arkTypeface : arkTypeface = TerminalPixelFont.Default;

        public PMSBitmapFont BitmapFont
        {
            get
            {
                if (bitmapFont == null) bitmapFont = Resources.Load<TerminalAssetSet>("MiniGame/TerminalAssets")?.HtmlTypeface
                    ?? Resources.Load<PMSBitmapFont>(FontResource);
                return bitmapFont;
            }
            set { if (bitmapFont == value) return; bitmapFont = value; SetAllDirty(); }
        }

        public float LinePitch
        {
            get => Mathf.Max(1, linePitch);
            set { linePitch = Mathf.Max(1, value); SetVerticesDirty(); SetLayoutDirty(); }
        }

        public bool CommandRow
        {
            get => commandRow;
            set { if (commandRow == value) return; commandRow = value; SetVerticesDirty(); SetLayoutDirty(); }
        }

        public override Texture mainTexture => ArkTypeface != null && ArkTypeface.Atlas != null ? ArkTypeface.Atlas
            : BitmapFont && BitmapFont.Atlas ? BitmapFont.Atlas : Texture2D.whiteTexture;

        public override float preferredWidth => scaleToFontSize ? TerminalTextMetrics.Width(text, 12) * GlyphScale : IsNumberedCommand(out int bodyStart)
            ? 34 + TerminalTextMetrics.Width(text.Substring(bodyStart), 14)
            : TerminalTextMetrics.Width(text, fontSize);

        public override float preferredHeight => IsNumberedCommand(out _)
            ? LinePitch
            : TerminalTextMetrics.MeasureHeight(text, scaleToFontSize ? 12 : fontSize, rectTransform.rect.width / GlyphScale,
                LinePitch * Mathf.Max(.01f, lineSpacing), horizontalOverflow == HorizontalWrapMode.Wrap);

        // Unity's Text.UpdateGeometry refuses to draw without a dynamic Font. The bitmap host does not need one.
        protected override void UpdateGeometry()
        {
            if (bitmapMesh == null) bitmapMesh = new Mesh { name = "Terminal bitmap glyphs", hideFlags = HideFlags.HideAndDontSave };
            using (var vertices = new VertexHelper())
            {
                OnPopulateMesh(vertices);
                var modifiers = GetComponents<IMeshModifier>();
                foreach (var modifier in modifiers) modifier.ModifyMesh(vertices);
                vertices.FillMesh(bitmapMesh);
            }
            canvasRenderer.SetMesh(bitmapMesh);
        }

        protected override void OnDestroy()
        {
            if (bitmapMesh != null)
            {
                if (Application.isPlaying) Destroy(bitmapMesh); else DestroyImmediate(bitmapMesh);
            }
            base.OnDestroy();
        }

        protected override void OnPopulateMesh(VertexHelper vertices)
        {
            vertices.Clear();
            if (ArkTypeface != null && ArkTypeface.Atlas != null)
            {
                PopulateArk(vertices);
                return;
            }
            var glyphs = BitmapFont;
            if (glyphs == null || glyphs.Atlas == null)
            {
                if (!warnedMissingResource)
                {
                    warnedMissingResource = true;
                    Debug.LogError("Terminal HTML bitmap font resource is unavailable: " + FontResource, this);
                }
                return;
            }
            if (string.IsNullOrEmpty(text)) return;
            Rect rect = rectTransform.rect;
            Color32 atlasColor = ResolveColor((Color32)color, out Color32 tint);
            if (IsNumberedCommand(out int bodyStart))
            {
                RenderRun(vertices, glyphs, text, 0, 2, 12, rect.xMin, rect.yMax - 1, atlasColor, tint);
                RenderRun(vertices, glyphs, text, bodyStart, text.Length - bodyStart, 14,
                    rect.xMin + 34, rect.yMax, atlasColor, tint);
                return;
            }

            int pixelSize = SupportedSize(fontSize);
            var lines = TerminalTextMetrics.Layout(text, pixelSize, rect.width, horizontalOverflow == HorizontalWrapMode.Wrap);
            float pitch = LinePitch * Mathf.Max(.01f, lineSpacing);
            int count = lines.Count;
            if (verticalOverflow == VerticalWrapMode.Truncate)
                count = Mathf.Min(count, rect.height < pixelSize ? 0 : 1 + Mathf.FloorToInt((rect.height - pixelSize) / pitch));
            if (count == 0) return;
            var anchor = GetTextAnchorPivot(alignment);
            float blockHeight = (count - 1) * pitch + pixelSize;
            float top = rect.yMax - Mathf.Floor((rect.height - blockHeight) * (1 - anchor.y));
            for (int index = 0; index < count; index++)
            {
                var line = lines[index];
                float left = rect.xMin + Mathf.Floor((rect.width - line.Width) * anchor.x);
                RenderRun(vertices, glyphs, text, line.Start, line.Length, pixelSize, left, top - index * pitch, atlasColor, tint);
            }
        }

        private bool IsNumberedCommand(out int bodyStart)
        {
            bodyStart = 0;
            if (!commandRow || text.Length < 3 || !char.IsDigit(text[0]) || !char.IsDigit(text[1]) || text[2] != ' ') return false;
            bodyStart = 2;
            while (bodyStart < text.Length && text[bodyStart] == ' ') bodyStart++;
            return true;
        }

        private void PopulateArk(VertexHelper vertices)
        {
            if (string.IsNullOrEmpty(text)) return;
            Rect rect = rectTransform.rect;
            if (IsNumberedCommand(out int start))
            {
                RenderArkRun(vertices, 0, 2, 12, rect.xMin, rect.yMax - 1);
                RenderArkRun(vertices, start, text.Length - start, 14, rect.xMin + 34, rect.yMax);
                return;
            }
            int size = scaleToFontSize ? 12 : SupportedSize(fontSize);
            float scale = GlyphScale;
            var lines = TerminalTextMetrics.Layout(text, size, rect.width / scale, horizontalOverflow == HorizontalWrapMode.Wrap);
            float pitch = LinePitch * Mathf.Max(.01f, lineSpacing);
            int count = lines.Count;
            if (verticalOverflow == VerticalWrapMode.Truncate)
                count = Mathf.Min(count, rect.height < size * scale ? 0 : 1 + Mathf.FloorToInt((rect.height - size * scale) / pitch));
            var anchor = GetTextAnchorPivot(alignment);
            float height = (count - 1) * pitch + size * scale;
            float top = rect.yMax - Mathf.Floor((rect.height - height) * (1 - anchor.y));
            for (int index = 0; index < count; index++)
            {
                var line = lines[index];
                float left = rect.xMin + Mathf.Floor((rect.width - line.Width * scale) * anchor.x);
                RenderArkRun(vertices, line.Start, line.Length, size, left, top - index * pitch, scale);
            }
        }

        private float GlyphScale => scaleToFontSize ? Mathf.Max(1, fontSize) / 12f : 1;

        private void RenderArkRun(VertexHelper vertices, int start, int length, int size, float left, float top, float scale = 1)
        {
            float x = left;
            for (int index = start; index < start + length; index++)
            {
                char character = text[index];
                if (character == '\r') continue;
                if (character == '\n') break;
                if (!ArkTypeface.TryGet(character, size, out var glyph))
                    ArkTypeface.TryGet('?', size, out glyph);
                if (glyph != null && glyph.w > 0 && glyph.h > 0)
                {
                    Rect uv = ArkTypeface.UV(glyph);
                    float x0 = x + glyph.offsetX * scale, y1 = top - glyph.offsetY * scale;
                    int first = vertices.currentVertCount;
                    vertices.AddVert(new Vector3(x0, y1 - glyph.h * scale), color, new Vector2(uv.xMin, uv.yMin));
                    vertices.AddVert(new Vector3(x0, y1), color, new Vector2(uv.xMin, uv.yMax));
                    vertices.AddVert(new Vector3(x0 + glyph.w * scale, y1), color, new Vector2(uv.xMax, uv.yMax));
                    vertices.AddVert(new Vector3(x0 + glyph.w * scale, y1 - glyph.h * scale), color, new Vector2(uv.xMax, uv.yMin));
                    vertices.AddTriangle(first, first + 1, first + 2); vertices.AddTriangle(first, first + 2, first + 3);
                }
                x += TerminalTextMetrics.Cell(character, size) * scale;
            }
        }

        private int SupportedSize(int requested)
        {
            if (requested == 10 || requested == 11 || requested == 12 || requested == 13 || requested == 14 || requested == 18 || requested == 19)
                return requested;
            if (!warnedSize)
            {
                warnedSize = true;
                Debug.LogWarning("Terminal HTML bitmap font has no " + requested + "px export; using the nearest exported size.", this);
            }
            int nearest = 10;
            foreach (int candidate in new[] { 11, 12, 13, 14, 18, 19 })
                if (Mathf.Abs(requested - candidate) < Mathf.Abs(requested - nearest)) nearest = candidate;
            return nearest;
        }

        // The five HTML colors retain their own exported edge pixels. Other UI colors use the nearest
        // exported run with a bounded tint; they never cause an entire label to fall back to question marks.
        public static Color32 ResolveColor(Color32 requested, out Color32 tint)
        {
            int best = 0, distance = int.MaxValue;
            for (int index = 0; index < ExportedColors.Length; index++)
            {
                var item = ExportedColors[index];
                int dr = requested.r - item.r, dg = requested.g - item.g, db = requested.b - item.b;
                int next = dr * dr + dg * dg + db * db;
                if (next < distance) { distance = next; best = index; }
            }
            var selected = ExportedColors[best];
            tint = new Color32((byte)Mathf.Min(255, requested.r * 255 / selected.r),
                (byte)Mathf.Min(255, requested.g * 255 / selected.g),
                (byte)Mathf.Min(255, requested.b * 255 / selected.b), requested.a);
            return selected;
        }

        private static void RenderRun(VertexHelper vertices, PMSBitmapFont glyphs, string value, int start, int length,
            int size, float left, float top, Color32 atlasColor, Color32 tint)
        {
            float x = left;
            for (int index = start; index < start + length; index++)
            {
                char character = value[index];
                if (character == '\r') continue;
                if (character == '\n') break;
                if (glyphs.Get(character, size, atlasColor, out var glyph) && glyph.w > 0 && glyph.h > 0)
                {
                    Rect uv = glyphs.UV(glyph);
                    float x0 = x + glyph.offsetX, y1 = top - glyph.offsetY;
                    int first = vertices.currentVertCount;
                    vertices.AddVert(new Vector3(x0, y1 - glyph.h), tint, new Vector2(uv.xMin, uv.yMin));
                    vertices.AddVert(new Vector3(x0, y1), tint, new Vector2(uv.xMin, uv.yMax));
                    vertices.AddVert(new Vector3(x0 + glyph.w, y1), tint, new Vector2(uv.xMax, uv.yMax));
                    vertices.AddVert(new Vector3(x0 + glyph.w, y1 - glyph.h), tint, new Vector2(uv.xMax, uv.yMin));
                    vertices.AddTriangle(first, first + 1, first + 2);
                    vertices.AddTriangle(first, first + 2, first + 3);
                }
                x += TerminalTextMetrics.Cell(character, size);
            }
        }
    }
}
