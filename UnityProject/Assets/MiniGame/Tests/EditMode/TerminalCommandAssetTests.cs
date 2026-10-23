using System;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace MiniGame.UnityHost.Tests
{
    public sealed class TerminalCommandAssetTests
    {
        [Test]
        public void CommandsReferenceAllTwelveUnscaledPointFilteredArtworkStates()
        {
            var assets = Resources.Load<TerminalAssetSet>("MiniGame/TerminalAssets");
            Assert.That(assets, Is.Not.Null);
            var commands = new[] { assets.ReleaseCommand, assets.TreatmentCommand, assets.PurificationCommand };
            Assert.That(commands.All(command => command != null), Is.True);
            var textures = commands.SelectMany(command => new[] { command.Normal, command.Selected, command.Pressed, command.Disabled }).ToArray();
            Assert.That(textures.All(texture => texture != null), Is.True, "Every command state must have supplied artwork.");
            Assert.That(textures.Distinct().Count(), Is.EqualTo(12));
            foreach (var texture in textures)
            {
                Assert.That(texture.width, Is.EqualTo(248), texture.name);
                Assert.That(texture.height, Is.EqualTo(48), texture.name);
                AssertTextureImport(texture);
            }
        }

        [Test]
        public void QuotaAtlasContainsEveryTwoDigitValueAndPreservesTopLeftSourceRects()
        {
            var assets = Resources.Load<TerminalAssetSet>("MiniGame/TerminalAssets");
            Assert.That(assets, Is.Not.Null);
            Assert.That(assets.QuotaFields, Is.Not.Null);
            Assert.That(assets.QuotaFieldMetrics, Is.Not.Null);
            Assert.That(assets.QuotaFields.width, Is.EqualTo(660));
            Assert.That(assets.QuotaFields.height, Is.EqualTo(320));
            AssertTextureImport(assets.QuotaFields);
            var metrics = JsonUtility.FromJson<QuotaMetrics>(assets.QuotaFieldMetrics.text);
            Assert.That(metrics.origin, Is.EqualTo("top-left"));
            Assert.That(metrics.size, Is.EqualTo(new[] { 660, 320 }));
            Assert.That(metrics.fields.Select(field => field.value), Is.EqualTo(Enumerable.Range(0, 100)));
            foreach (var field in metrics.fields)
            {
                Assert.That(field.text, Is.EqualTo(field.value.ToString("00")));
                Assert.That(field.rect, Is.EqualTo(new[] { field.value % 10 * 66, field.value / 10 * 32, 66, 32 }), field.text);
            }
        }

        [TestCase(-1, "00", true)]
        [TestCase(0, "00", true)]
        [TestCase(99, "99", true)]
        [TestCase(100, "100", false)]
        public void QuotaFieldUsesArtworkWithoutClampingValidThreeDigitValues(int value, string expected, bool usesArtwork)
        {
            var owner = new GameObject("Quota field boundary", typeof(RectTransform));
            var labelObject = new GameObject("Remaining", typeof(RectTransform), typeof(Text));
            labelObject.transform.SetParent(owner.transform, false);
            try
            {
                var label = labelObject.GetComponent<Text>();
                var field = owner.AddComponent<TerminalQuotaField>();
                field.Initialize(Resources.Load<TerminalAssetSet>("MiniGame/TerminalAssets"), label);
                field.SetValue(value);
                var artwork = owner.GetComponentInChildren<RawImage>();
                Assert.That(label.text, Is.EqualTo(expected));
                Assert.That(label.enabled, Is.EqualTo(!usesArtwork));
                Assert.That(artwork.enabled, Is.EqualTo(usesArtwork));
                Assert.That(artwork.raycastTarget, Is.False);
                if (usesArtwork)
                {
                    int clamped = Math.Max(0, value);
                    Assert.That(artwork.uvRect.x, Is.EqualTo(clamped % 10 / 10f).Within(.00001f));
                    Assert.That(artwork.uvRect.y, Is.EqualTo(1 - (clamped / 10 + 1) / 10f).Within(.00001f));
                }
            }
            finally { Object.DestroyImmediate(owner); }
        }

        private static void AssertTextureImport(Texture2D texture)
        {
            Assert.That(texture.filterMode, Is.EqualTo(FilterMode.Point), texture.name);
            Assert.That(texture.wrapMode, Is.EqualTo(TextureWrapMode.Clamp), texture.name);
            var importer = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(texture)) as TextureImporter;
            Assert.That(importer, Is.Not.Null, texture.name);
            Assert.That(importer.mipmapEnabled, Is.False, texture.name);
            Assert.That(importer.textureCompression, Is.EqualTo(TextureImporterCompression.Uncompressed), texture.name);
            Assert.That(importer.npotScale, Is.EqualTo(TextureImporterNPOTScale.None), texture.name);
        }

        [Serializable] private sealed class QuotaMetrics
        {
            public string origin;
            public int[] size;
            public QuotaField[] fields;
        }

        [Serializable] private sealed class QuotaField
        {
            public int value;
            public string text;
            public int[] rect;
        }
    }
}
