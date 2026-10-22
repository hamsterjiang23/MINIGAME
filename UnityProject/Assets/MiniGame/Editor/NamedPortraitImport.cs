using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace MiniGame.UnityHost.Editor
{
    public static class NamedPortraitImport
    {
        public const string Folder = "Assets/MiniGame/Art/Characters/NamedPortraits";

        public static int ExpressionCount(string patientId)
        {
            string baseline = PortraitCatalog.BasePortrait(patientId);
            if (baseline.Length < 2 || !Directory.Exists(Folder)) return 1;
            string prefix = baseline.Substring(0, baseline.Length - 2);
            return Math.Max(1, Directory.GetFiles(Folder, prefix + "*.png").Length);
        }

        public static string NamedId(string id)
        {
            switch (id)
            {
                case "ML04": case "ML05": return "ML03";
                case "JN05": return "JN04";
                default: return id;
            }
        }

        [MenuItem("MiniGame/人物素材/更新命名PNG目录")]
        public static void Run()
        {
            var replacements = new Dictionary<string, Sprite>(StringComparer.Ordinal);
            foreach (string path in Directory.GetFiles(Folder, "*.png").OrderBy(value => value, StringComparer.Ordinal))
            {
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
                string id = Path.GetFileNameWithoutExtension(path);
                var importer = (TextureImporter)AssetImporter.GetAtPath(path);
                var portrait = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().FirstOrDefault(sprite => sprite.name == id);
                if (portrait == null)
                {
                    importer.textureShape = TextureImporterShape.Texture2D;
                    importer.textureType = TextureImporterType.Sprite;
                    importer.spriteImportMode = SpriteImportMode.Multiple;
                    importer.alphaIsTransparency = true;
                    importer.mipmapEnabled = false;
                    importer.maxTextureSize = 4096;
                    importer.npotScale = TextureImporterNPOTScale.None;
                    importer.filterMode = FilterMode.Bilinear;
                    importer.textureCompression = TextureImporterCompression.Uncompressed;
                    importer.GetSourceTextureWidthAndHeight(out int width, out int height);
                    Rect crop = PatientMonitorAssets.CharacterCrop(path, width, height, 1, out var pivots);
                    importer.spritesheet = new[] { new SpriteMetaData { name = id, rect = crop,
                        alignment = (int)SpriteAlignment.Custom, pivot = pivots[0] } };
                    var settings = new TextureImporterSettings();
                    importer.ReadTextureSettings(settings);
                    settings.textureShape = TextureImporterShape.Texture2D;
                    settings.spriteMeshType = SpriteMeshType.FullRect;
                    importer.SetTextureSettings(settings);
                    importer.SaveAndReimport();
                    AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
                    var imported = AssetDatabase.LoadAllAssetsAtPath(path);
                    portrait = imported.OfType<Sprite>().SingleOrDefault(sprite => sprite.name == id);
                    if (portrait == null) throw new InvalidOperationException("Named sprite was not imported: " + path
                        + "; assets=" + string.Join(",", imported.Select(asset => asset.GetType().Name + ":" + asset.name)));
                }
                replacements.Add(id, portrait);
            }
            if (replacements.Count != 22) throw new InvalidOperationException("The complete portrait pack requires 22 named PNGs.");
            var catalog = AssetDatabase.LoadAssetAtPath<PortraitCatalog>(CampaignBuilder.PortraitPath);
            if (catalog == null) throw new InvalidOperationException("Campaign portrait catalog is missing.");
            var serialized = new SerializedObject(catalog);
            var entries = serialized.FindProperty("entries");
            var values = new List<PortraitCatalog.Entry>();
            var seen = new HashSet<string>(StringComparer.Ordinal);
            for (int index = 0; index < entries.arraySize; index++)
            {
                var entry = entries.GetArrayElementAtIndex(index);
                string id = entry.FindPropertyRelative("id").stringValue;
                var old = entry.FindPropertyRelative("sprite").objectReferenceValue as Sprite;
                values.Add(new PortraitCatalog.Entry { id = id,
                    sprite = replacements.TryGetValue(NamedId(id), out var replacement) ? replacement : old });
                seen.Add(id);
            }
            foreach (var pair in replacements.OrderBy(pair => pair.Key, StringComparer.Ordinal))
                if (seen.Add(pair.Key)) values.Add(new PortraitCatalog.Entry { id = pair.Key, sprite = pair.Value });
            catalog.Configure(values.ToArray());
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssetIfDirty(catalog);
            string root = Environment.GetEnvironmentVariable("MINIGAME_REPOSITORY_ROOT");
            if (!string.IsNullOrEmpty(root))
            {
                string folder = Path.Combine(root, "Artifacts", "NamedPortraits");
                Directory.CreateDirectory(folder);
                File.WriteAllText(Path.Combine(folder, "import.txt"), "Named PNGs: " + replacements.Count
                    + "\nCatalog IDs: " + values.Count + "\nAliases: ML04/ML05 -> ML03; JN05 -> JN04\n");
            }
            Debug.Log("MINIGAME_NAMED_PORTRAITS_IMPORTED " + replacements.Count);
        }
    }
}
