using PMSAmber;
using UnityEditor;
using UnityEngine;

namespace MiniGame.UnityHost.Editor
{
    public static class TerminalHtmlMatchedSetup
    {
        public const string FontPath = "Assets/PMSAmberTerminal/Resources/PMSAmberTerminal/HTMLBitmapFont.asset";
        public const string AtlasPath = "Assets/PMSAmberTerminal/Art/FontAtlas/HTML_SimSun_Lucida.png";

        [MenuItem("MiniGame/终端/接入 HTML 字体与高亮资源")]
        public static void Install()
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(AtlasPath);
            importer.maxTextureSize = 8192;
            importer.filterMode = FilterMode.Point; importer.mipmapEnabled = false;
            importer.sRGBTexture = false; importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.npotScale = TextureImporterNPOTScale.None; importer.alphaIsTransparency = false;
            importer.SaveAndReimport();
            var assets = AssetDatabase.LoadAssetAtPath<TerminalAssetSet>("Assets/MiniGame/Resources/MiniGame/TerminalAssets.asset");
            if (assets == null) throw new System.InvalidOperationException("正式终端资源配置不存在。");
            assets.HtmlTypeface = AssetDatabase.LoadAssetAtPath<PMSBitmapFont>(FontPath);
            if (assets.HtmlTypeface == null || assets.HtmlTypeface.Atlas == null || assets.HtmlTypeface.Atlas.width < 4096)
                throw new System.InvalidOperationException("HTML 字形图集未按原尺寸导入。");
            assets.HtmlTypeface.InvalidateMetrics();
            foreach (var label in Resources.FindObjectsOfTypeAll<TerminalBitmapText>()) label.SetAllDirty();
            assets.DisplayShader = AssetDatabase.LoadAssetAtPath<Shader>("Assets/PMSAmberTerminal/Shaders/PMSAmberCRT.shader");
            EditorUtility.SetDirty(assets); AssetDatabase.SaveAssetIfDirty(assets);
            Debug.Log("MINIGAME_HTML_FONT_INSTALLED 4096x4096 SimSun/Lucida Console; native terminal 800x600");
        }
    }

    internal sealed class TerminalHtmlFontRefresh : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(string[] imported, string[] deleted, string[] moved, string[] movedFrom)
        {
            if (System.Array.IndexOf(imported, TerminalHtmlMatchedSetup.AtlasPath) < 0 &&
                System.Array.IndexOf(imported, "Assets/PMSAmberTerminal/Art/FontAtlas/HTML_FontMetrics.json") < 0) return;
            var font = AssetDatabase.LoadAssetAtPath<PMSBitmapFont>(TerminalHtmlMatchedSetup.FontPath);
            if (font == null) return;
            font.InvalidateMetrics();
            foreach (var label in Resources.FindObjectsOfTypeAll<TerminalBitmapText>()) label.SetAllDirty();
        }
    }
}
