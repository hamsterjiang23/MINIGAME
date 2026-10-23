using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace MiniGame.UnityHost.Editor
{
    public static class TerminalDossierSetup
    {
        public const string ArtworkRoot = "Assets/PMSAmberUI_v0826/";
        public const string CatalogPath = "Assets/MiniGame/Resources/MiniGame/TerminalDossiers.asset";
        public const string SourcePath = "Assets/MiniGame/Content/TerminalDossier/profiles.json";

        [MenuItem("MiniGame/终端/接入 v0.8.26 角色档案")]
        public static void Install()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Stop Play Mode before importing dossiers.");
            var source = JsonUtility.FromJson<Source>(File.ReadAllText(SourcePath));
            if (source == null || source.formatVersion != 1 || source.entries == null || source.entries.Length != 7
                || source.entries.Select(entry => entry.patientId).Distinct().Count() != 7)
                throw new InvalidOperationException("Expected seven unique dossier profiles.");
            foreach (var entry in source.entries)
            {
                if (entry.patientId == "tutorial") continue;
                string path = ArtworkRoot + "Portraits/Native/" + entry.portraitId + "_Amber.png";
                ImportTexture(path, true);
                entry.portrait = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (entry.portrait == null || entry.portrait.rect.size != new Vector2(96, 128))
                    throw new InvalidOperationException("Expected an unscaled 96x128 dossier portrait: " + path);
            }
            string framePath = ArtworkRoot + "UI/Frame_Directory_800x600.png";
            string quotaPath = ArtworkRoot + "Fonts/DossierQuota_00_99.png";
            ImportTexture(framePath, false); ImportTexture(quotaPath, false);
            var catalog = AssetDatabase.LoadAssetAtPath<TerminalDossierCatalog>(CatalogPath);
            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<TerminalDossierCatalog>();
                AssetDatabase.CreateAsset(catalog, CatalogPath);
            }
            catalog.Configure(source.entries);
            EditorUtility.SetDirty(catalog); AssetDatabase.SaveAssetIfDirty(catalog);
            var assets = AssetDatabase.LoadAssetAtPath<TerminalAssetSet>("Assets/MiniGame/Resources/MiniGame/TerminalAssets.asset");
            if (assets == null) throw new InvalidOperationException("Terminal assets are missing.");
            assets.Dossiers = catalog;
            assets.DirectoryFrame = AssetDatabase.LoadAssetAtPath<Texture2D>(framePath);
            assets.DossierQuotaFields = AssetDatabase.LoadAssetAtPath<Texture2D>(quotaPath);
            assets.DossierQuotaMetrics = AssetDatabase.LoadAssetAtPath<TextAsset>(ArtworkRoot + "Fonts/DossierQuota.json");
            EditorUtility.SetDirty(assets); AssetDatabase.SaveAssetIfDirty(assets);
            Debug.Log("MINIGAME_DOSSIERS_INSTALLED v0.8.26 seven profiles, six native portraits; authored summary text preserved.");
        }

        private static void ImportTexture(string path, bool sprite)
        {
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = sprite ? TextureImporterType.Sprite : TextureImporterType.Default;
            if (sprite) { importer.spriteImportMode = SpriteImportMode.Single; importer.spritePixelsPerUnit = 100; }
            importer.filterMode = FilterMode.Point; importer.wrapMode = TextureWrapMode.Clamp;
            importer.mipmapEnabled = false; importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.npotScale = TextureImporterNPOTScale.None; importer.alphaIsTransparency = false;
            importer.sRGBTexture = false; importer.maxTextureSize = 2048;
            importer.SaveAndReimport();
        }

        [Serializable] private sealed class Source
        {
            public int formatVersion;
            public TerminalDossierCatalog.Entry[] entries;
        }
    }
}
