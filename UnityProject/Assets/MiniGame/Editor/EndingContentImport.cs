using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MiniGame.Presentation;
using UnityEditor;
using UnityEngine;

namespace MiniGame.UnityHost.Editor
{
    public static class EndingContentImport
    {
        [MenuItem("MiniGame/Campaign/Import endings and updated dialogue")]
        public static void Run()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Leave Play Mode before importing dialogue.");
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            var catalog = EndingDialoguePresenter.LoadCatalog();
            if (catalog == null) throw new InvalidOperationException("Ending source snapshot is missing.");
            var candidate = DialoguePackageImporter.ImportPackage(Path.GetFullPath(CampaignBuilder.PackagePath), false);
            var active = AssetDatabase.LoadAssetAtPath<DialoguePackageAsset>(CampaignBuilder.ActivePath);
            if (active == null) throw new InvalidOperationException("Active campaign asset is missing.");
            active.Configure(candidate.PackageSource, candidate.Project, candidate.SourceHash);
            EditorUtility.SetDirty(active);
            NamedPortraitImport.Run();
            ImportPortraits();
            AssetDatabase.SaveAssets();
            Debug.Log("MINIGAME_ENDINGS_IMPORTED " + catalog.Revision);
        }

        public static void ImportPortraits()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<PortraitCatalog>(CampaignBuilder.PortraitPath);
            if (catalog == null) throw new InvalidOperationException("Portrait catalog is missing.");
            var serialized = new SerializedObject(catalog);
            var existing = serialized.FindProperty("entries");
            var entries = new Dictionary<string, Sprite>(StringComparer.Ordinal);
            for (int i = 0; i < existing.arraySize; i++)
            {
                var item = existing.GetArrayElementAtIndex(i);
                entries[item.FindPropertyRelative("id").stringValue] = item.FindPropertyRelative("sprite").objectReferenceValue as Sprite;
            }
            foreach (string id in new[] { "SJ01", "SJ02", "PE01", "PE02" })
            {
                string folder = id.StartsWith("SJ", StringComparison.Ordinal) ? "PlayerPortraits" : "EndingPortraits";
                string path = "Assets/MiniGame/Art/Characters/" + folder + "/" + id + ".png";
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null) throw new InvalidOperationException("Missing ending portrait: " + path);
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = true; importer.mipmapEnabled = false;
                importer.maxTextureSize = 4096; importer.npotScale = TextureImporterNPOTScale.None;
                importer.filterMode = FilterMode.Bilinear;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
                entries[id] = AssetDatabase.LoadAssetAtPath<Sprite>(path)
                    ?? throw new InvalidOperationException("Could not import ending portrait " + id);
            }
            catalog.Configure(entries.Select(item => new PortraitCatalog.Entry { id = item.Key, sprite = item.Value }).ToArray());
            EditorUtility.SetDirty(catalog);
        }
    }
}
