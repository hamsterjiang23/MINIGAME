using UnityEngine;

namespace MiniGame.UnityHost
{
    /// <summary>References the supplied PMS v0.8.12 assets while the existing runtime owns game state.</summary>
    public sealed class TerminalAssetSet : ScriptableObject
    {
        [Tooltip("HTML 原字号、原颜色字形图集；正式终端使用此资源，不再替换为动态字体。")]
        public PMSAmber.PMSBitmapFont HtmlTypeface;
        [Tooltip("保留旧接口；HTML 终端实际字形由 HtmlTypeface 提供。")]
        public Font Typeface;
        public Shader DisplayShader;
        public Texture2D DirectoryFrame, ReviewFrame, Selection, GlowLine, GlowDot;
    }
}
