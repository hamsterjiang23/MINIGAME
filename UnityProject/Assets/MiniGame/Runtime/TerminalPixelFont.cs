using System;
using System.Collections.Generic;
using UnityEngine;

namespace MiniGame.UnityHost
{
    [CreateAssetMenu(menuName = "MiniGame/终端像素字体")]
    public sealed class TerminalPixelFont : ScriptableObject
    {
        public Texture2D Atlas;
        public TextAsset Metrics;
        [Serializable] public sealed class Glyph { public int code, size, x, y, w, h, offsetX, offsetY, advance; }
        [Serializable] private sealed class Data { public int atlasWidth, atlasHeight; public Glyph[] glyphs; }
        private readonly Dictionary<int, Glyph> glyphs = new Dictionary<int, Glyph>();
        private TextAsset loaded;
        public static TerminalPixelFont Default => Resources.Load<TerminalPixelFont>("MiniGame/ArkPixel12");
        public void InvalidateMetrics() { loaded = null; glyphs.Clear(); }
        private void OnEnable() => InvalidateMetrics();
        private void OnValidate() => InvalidateMetrics();
        public bool TryGet(char character, int size, out Glyph glyph)
        {
            if (loaded != Metrics)
            {
                glyphs.Clear(); loaded = Metrics;
                if (Metrics != null)
                {
                    var data = JsonUtility.FromJson<Data>(Metrics.text);
                    if (Atlas == null || Atlas.width != data.atlasWidth || Atlas.height != data.atlasHeight)
                        throw new InvalidOperationException("Terminal pixel atlas was resized on import. Reinstall the generated font atlas.");
                    foreach (var item in data.glyphs) glyphs[(item.size << 16) | item.code] = item;
                }
            }
            return glyphs.TryGetValue((size << 16) | character, out glyph);
        }
        public Rect UV(Glyph glyph) => new Rect((float)glyph.x / Atlas.width,
            1f - (float)(glyph.y + glyph.h) / Atlas.height, (float)glyph.w / Atlas.width, (float)glyph.h / Atlas.height);
    }
}
