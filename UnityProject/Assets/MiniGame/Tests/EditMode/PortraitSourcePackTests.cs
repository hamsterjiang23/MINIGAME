using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace MiniGame.UnityHost.Tests
{
    public sealed class PortraitSourcePackTests
    {
        [Serializable]
        private sealed class SourceEntry
        {
            public string id, catalogId, label, role, target, sourceSha256, assetGuid;
        }

        [Serializable]
        private sealed class SourceManifest
        {
            public int pngCount, referenceJpegCount;
            public SourceEntry[] entries;
        }

        [Test]
        public void AllThirtyPortraitsKeepApprovedSourceBytesAndAssetGuids()
        {
            var manifest = ReadManifest();
            Assert.That(manifest.pngCount, Is.EqualTo(30));
            Assert.That(manifest.referenceJpegCount, Is.EqualTo(6));
            Assert.That(manifest.entries.Length, Is.EqualTo(30));
            Assert.That(manifest.entries.Select(entry => entry.id).Distinct().Count(), Is.EqualTo(30));
            Assert.That(manifest.entries.Select(entry => entry.catalogId).Distinct().Count(), Is.EqualTo(30));
            Assert.That(manifest.entries.Count(entry => entry.role == "supervisor"), Is.EqualTo(6));
            Assert.That(manifest.entries.Count(entry => entry.role == "player"), Is.EqualTo(2));

            string project = Directory.GetParent(Application.dataPath).FullName;
            using (var sha = SHA256.Create())
                foreach (var entry in manifest.entries)
                {
                    Assert.That(entry.catalogId, Is.Not.Null.And.Not.Empty, entry.id);
                    string path = Path.Combine(project, entry.target);
                    Assert.That(File.Exists(path), Is.True, entry.id);
                    string hash = BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(path)))
                        .Replace("-", "").ToLowerInvariant();
                    Assert.That(hash, Is.EqualTo(entry.sourceSha256), entry.id + " source pixels were replaced or edited.");
                    Assert.That(AssetDatabase.AssetPathToGUID(entry.target), Is.EqualTo(entry.assetGuid), entry.id);
                }
        }

        [Test]
        public void CatalogAndSupervisorDialogueResolveToTheirOwnSourceAssets()
        {
            var catalog = Resources.Load<PortraitCatalog>("MiniGame/PortraitCatalog");
            Assert.That(catalog, Is.Not.Null);
            foreach (var entry in ReadManifest().entries)
            {
                Texture2D texture;
                if (entry.role == "supervisor")
                    texture = Resources.Load<Texture2D>(SupervisorDialogueContent.PortraitResource(entry.catalogId));
                else
                {
                    var sprite = catalog.Find(entry.catalogId);
                    Assert.That(sprite, Is.Not.Null, entry.catalogId);
                    texture = sprite.texture;
                }
                Assert.That(texture, Is.Not.Null, entry.id);
                Assert.That(AssetDatabase.GetAssetPath(texture), Is.EqualTo(entry.target), entry.id);
            }
        }

        [TestCase("SJ01")]
        [TestCase("SJ02")]
        public void PlayerPortraitsRemainSeparateTransparentSprites(string id)
        {
            string path = "Assets/MiniGame/Art/Characters/PlayerPortraits/" + id + ".png";
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            Assert.That(importer, Is.Not.Null);
            Assert.That(importer.textureType, Is.EqualTo(TextureImporterType.Sprite));
            Assert.That(importer.spriteImportMode, Is.EqualTo(SpriteImportMode.Single));
            Assert.That(importer.alphaIsTransparency, Is.True);
            Assert.That(importer.mipmapEnabled, Is.False);
            Assert.That(importer.maxTextureSize, Is.EqualTo(4096));
            var sprite = Resources.Load<PortraitCatalog>("MiniGame/PortraitCatalog").Find(id);
            Assert.That(sprite, Is.Not.Null);
            Assert.That(sprite.pivot.y, Is.EqualTo(0));
            Assert.That(AssetDatabase.GetAssetPath(sprite), Is.EqualTo(path));
        }

        private static SourceManifest ReadManifest()
        {
            string repository = Environment.GetEnvironmentVariable("MINIGAME_REPOSITORY_ROOT");
            if (string.IsNullOrEmpty(repository))
                repository = Directory.GetParent(Directory.GetParent(Application.dataPath).FullName).FullName;
            string path = Path.Combine(repository, "docs", "portrait-remap", "source-manifest.json");
            Assert.That(File.Exists(path), Is.True, "Approved source pack manifest is required: " + path);
            var manifest = JsonUtility.FromJson<SourceManifest>(File.ReadAllText(path));
            Assert.That(manifest, Is.Not.Null);
            return manifest;
        }
    }
}
