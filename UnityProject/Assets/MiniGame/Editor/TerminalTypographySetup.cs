using System;
using UnityEditor;
using UnityEngine;

namespace MiniGame.UnityHost.Editor
{
    public static class TerminalTypographySetup
    {
        private const string Folder = "Assets/MiniGame/Art/Fonts/ArkPixel12/";
        public const string AtlasPath = Folder + "ArkPixel12.png";
        public const string MetricsPath = Folder + "ArkPixel12.json";
        public const string FontPath = "Assets/MiniGame/Resources/MiniGame/ArkPixel12.asset";
        public const string SettingsPath = "Assets/MiniGame/Resources/MiniGame/TerminalDialogueTextDefault.asset";

        [MenuItem("MiniGame/终端/安装方舟像素字体")]
        public static void Install()
        {
            AssetDatabase.ImportAsset(Folder + "ArkPixel12.png");
            AssetDatabase.ImportAsset(Folder + "ArkPixel12.json");
            var importer = AssetImporter.GetAtPath(Folder + "ArkPixel12.png") as TextureImporter;
            if (importer == null) throw new InvalidOperationException("先运行 Tools/TerminalTypography/build_ark_atlas.py 生成字体图集。");
            importer.textureType = TextureImporterType.Default;
            importer.filterMode = FilterMode.Point; importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.maxTextureSize = 8192; importer.npotScale = TextureImporterNPOTScale.None;
            importer.wrapMode = TextureWrapMode.Clamp; importer.alphaSource = TextureImporterAlphaSource.FromInput;
            importer.SaveAndReimport();
            var font = AssetDatabase.LoadAssetAtPath<TerminalPixelFont>(FontPath);
            if (font == null) { font = ScriptableObject.CreateInstance<TerminalPixelFont>(); AssetDatabase.CreateAsset(font, FontPath); }
            font.Atlas = AssetDatabase.LoadAssetAtPath<Texture2D>(Folder + "ArkPixel12.png");
            font.Metrics = AssetDatabase.LoadAssetAtPath<TextAsset>(Folder + "ArkPixel12.json");
            EditorUtility.SetDirty(font); AssetDatabase.SaveAssetIfDirty(font);
            RefreshLoadedFont();
            if (AssetDatabase.LoadAssetAtPath<TerminalDialogueTextSettings>(SettingsPath) == null)
                AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<TerminalDialogueTextSettings>(), SettingsPath);
        }

        internal static void RefreshLoadedFont()
        {
            var font = AssetDatabase.LoadAssetAtPath<TerminalPixelFont>(FontPath);
            if (font == null) return;
            font.InvalidateMetrics();
            foreach (var label in Resources.FindObjectsOfTypeAll<TerminalBitmapText>())
                if (label.ArkTypeface == font) label.SetAllDirty();
        }
    }

    internal sealed class TerminalPixelFontRefresh : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(string[] imported, string[] deleted, string[] moved, string[] movedFrom)
        {
            if (Array.IndexOf(imported, TerminalTypographySetup.AtlasPath) < 0
                && Array.IndexOf(imported, TerminalTypographySetup.MetricsPath) < 0) return;
            // Repacking changes every UV; a TextAsset reimport may retain its Unity object identity.
            TerminalTypographySetup.RefreshLoadedFont();
        }
    }
}
