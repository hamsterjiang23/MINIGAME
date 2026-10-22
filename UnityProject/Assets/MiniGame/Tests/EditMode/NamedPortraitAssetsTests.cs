using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MiniGame.UnityHost.Tests
{
    public sealed class NamedPortraitAssetsTests
    {
        private const string NamedDirectory = "Assets/MiniGame/Art/Characters/NamedPortraits";
        private static readonly string[] SourceIds =
        {
            "ML01", "ML02", "ML03",
            "VV01", "VV02", "VV03", "VV04",
            "AD01", "AD02", "AD03", "AD04",
            "ED01", "ED02", "ED03", "ED04",
            "JN01", "JN02", "JN03", "JN04",
            "D0_01", "D0_02", "D0_03"
        };

        [TestCaseSource(nameof(SourceIds))]
        public void CatalogUsesTheNamedPngAndKeepsVisiblePixelsAndTransparency(string id)
        {
            var catalog = Resources.Load<PortraitCatalog>("MiniGame/PortraitCatalog");
            Assert.That(catalog, Is.Not.Null);
            var sprite = catalog.Find(id);
            Assert.That(sprite, Is.Not.Null, id);
            string assetPath = NamedDirectory + "/" + id + ".png";
            Assert.That(AssetDatabase.GetAssetPath(sprite.texture), Is.EqualTo(assetPath),
                "Catalog must not retain a sprite from an older sheet.");
            Assert.That(sprite.name, Is.EqualTo(id));
            Assert.That(AssetDatabase.LoadAllAssetsAtPath(assetPath).OfType<Sprite>(), Does.Contain(sprite));
            var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            Assert.That(importer, Is.Not.Null);
            Assert.That(importer.textureType, Is.EqualTo(TextureImporterType.Sprite));
            Assert.That(importer.alphaSource, Is.EqualTo(TextureImporterAlphaSource.FromInput));
            Assert.That(importer.alphaIsTransparency, Is.True);

            // Read checked-in PNG bytes without changing the importer's readability or compression.
            var source = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            try
            {
                Assert.That(ImageConversion.LoadImage(source, File.ReadAllBytes(ProjectPath(assetPath))), Is.True);
                var pixels = source.GetPixels32();
                Assert.That(sprite.rect.width, Is.GreaterThan(1));
                Assert.That(sprite.rect.height, Is.GreaterThan(1));
                float xScale = (float)source.width / sprite.texture.width;
                float yScale = (float)source.height / sprite.texture.height;
                int left = Mathf.Clamp(Mathf.FloorToInt(sprite.rect.xMin * xScale), 0, source.width - 1);
                int right = Mathf.Clamp(Mathf.CeilToInt(sprite.rect.xMax * xScale), left + 1, source.width);
                int bottom = Mathf.Clamp(Mathf.FloorToInt(sprite.rect.yMin * yScale), 0, source.height - 1);
                int top = Mathf.Clamp(Mathf.CeilToInt(sprite.rect.yMax * yScale), bottom + 1, source.height);
                int visible = 0, transparent = 0;
                for (int y = bottom; y < top; y++)
                    for (int x = left; x < right; x++)
                    {
                        byte alpha = pixels[y * source.width + x].a;
                        if (alpha > 200) visible++;
                        if (alpha == 0) transparent++;
                    }
                Assert.That(visible, Is.GreaterThan(100), id + " must contain a visible character inside its imported sprite rectangle.");
                Assert.That(transparent, Is.GreaterThan(100), id + " must preserve a transparent surround inside its imported sprite rectangle.");
            }
            finally { Object.DestroyImmediate(source); }
        }

        [TestCase("ML04", "ML03")]
        [TestCase("ML05", "ML03")]
        [TestCase("JN05", "JN04")]
        public void LegacyPortraitIdsResolveToTheNewCharacterSprite(string legacyId, string namedId)
        {
            var catalog = Resources.Load<PortraitCatalog>("MiniGame/PortraitCatalog");
            Assert.That(catalog, Is.Not.Null);
            var target = catalog.Find(namedId);
            Assert.That(target, Is.Not.Null);
            Assert.That(catalog.Find(legacyId), Is.SameAs(target));
            Assert.That(AssetDatabase.GetAssetPath(target.texture), Is.EqualTo(NamedDirectory + "/" + namedId + ".png"));
        }

        [TestCase("child", 3)]
        [TestCase("former-ruler", 4)]
        [TestCase("battlefield-doctor", 4)]
        [TestCase("diplomat", 4)]
        [TestCase("priest", 4)]
        [TestCase("tutorial", 3)]
        public void MonitorPreviewExpressionLimitMatchesTheNamedSourceDirectory(string patientId, int expected)
        {
            Assert.That(MiniGame.UnityHost.Editor.NamedPortraitImport.ExpressionCount(patientId), Is.EqualTo(expected));
        }

        [Test]
        public void NamedSourceDirectoryContainsEveryExpectedPortraitExactlyOnce()
        {
            var files = Directory.GetFiles(ProjectPath(NamedDirectory), "*.png", SearchOption.TopDirectoryOnly);
            var ids = files.Select(Path.GetFileNameWithoutExtension).ToArray();
            Assert.That(ids, Is.EquivalentTo(SourceIds));
            Assert.That(ids.Distinct(StringComparer.Ordinal).Count(), Is.EqualTo(22));
            foreach (string id in SourceIds)
                Assert.That(AssetDatabase.LoadAssetAtPath<Texture2D>(NamedDirectory + "/" + id + ".png"), Is.Not.Null, id);
        }

        private static string ProjectPath(string assetPath) =>
            Path.Combine(Directory.GetParent(Application.dataPath).FullName, assetPath);
    }
}
