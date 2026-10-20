using UnityEngine;

namespace MiniGame.UnityHost
{
    /// <summary>References the supplied PMS v0.8.12 assets while the existing runtime owns game state.</summary>
    public sealed class TerminalAssetSet : ScriptableObject
    {
        public Font Typeface;
        public Shader DisplayShader;
        public Texture2D DirectoryFrame, ReviewFrame, Selection, GlowLine, GlowDot;
    }
}
