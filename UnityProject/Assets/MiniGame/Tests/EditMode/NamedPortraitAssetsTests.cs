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

        // Independent workbook-to-artwork expectations, not computed by the importer under test.
        private static readonly string[][] ExpectedMappings =
        {
            new[] { "ML01", "ML01" }, new[] { "ML02", "ML02" }, new[] { "ML03", "ML03" },
            new[] { "VV01", "VV01" }, new[] { "VV02", "VV02" }, new[] { "VV03", "VV03" }, new[] { "VV04", "VV04" },
            new[] { "AD01", "AD01" }, new[] { "AD02", "AD02" }, new[] { "AD03", "AD03" }, new[] { "AD04", "AD04" },
            new[] { "ED01", "ED01" }, new[] { "ED02", "ED02" }, new[] { "ED03", "ED03" }, new[] { "ED04", "ED04" },
            new[] { "JN01", "JN01" }, new[] { "JN02", "JN02" }, new[] { "JN03", "JN03" }, new[] { "JN04", "JN04" },
            new[] { "D0_01", "D0_01" }, new[] { "D0_02", "D0_02" }, new[] { "D0_03", "D0_03" }
        };

        [TestCaseSource(nameof(ExpectedMappings))]
        public void CatalogUsesTheWorkbookPoseAndKeepsVisiblePixelsAndTransparency(string id, string sourceId)
        {
            var catalog = Resources.Load<PortraitCatalog>("MiniGame/PortraitCatalog");
            Assert.That(catalog, Is.Not.Null);
            var sprite = catalog.Find(id);
            Assert.That(sprite, Is.Not.Null, id);
            string assetPath = NamedDirectory + "/" + sourceId + ".png";
            Assert.That(AssetDatabase.GetAssetPath(sprite.texture), Is.EqualTo(assetPath),
                "Catalog must not retain a sprite from an older sheet.");
            Assert.That(sprite.name, Is.EqualTo(sourceId));
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

        [TestCase("ML04")]
        [TestCase("ML05")]
        [TestCase("JN05")]
        public void LegacyPortraitIdsKeepTheirOwnNamedSprite(string legacyId)
        {
            var catalog = Resources.Load<PortraitCatalog>("MiniGame/PortraitCatalog");
            Assert.That(catalog, Is.Not.Null);
            var target = catalog.Find(legacyId);
            Assert.That(target, Is.Not.Null);
            Assert.That(target.name, Is.EqualTo(legacyId));
            Assert.That(AssetDatabase.GetAssetPath(target.texture), Is.EqualTo("Assets/MiniGame/Art/Characters/" + legacyId + ".png"));
        }

        [Test]
        public void NamedImporterResolvesEachWorkbookPoseDirectlyToItsSourcePng()
        {
            foreach (var mapping in ExpectedMappings)
                Assert.That(MiniGame.UnityHost.Editor.NamedPortraitImport.NamedId(mapping[0]),
                    Is.EqualTo(mapping[1]), mapping[0]);
        }

        [TestCase("battlefield-doctor", "ED01", "ED01")]
        [TestCase("priest", "JN01", "JN01")]
        [TestCase("tutorial", "D0_01", "D0_01")]
        public void NeutralFallbackKeepsTheOriginalNeutralArtwork(string patientId, string logicalId, string sourceId)
        {
            Assert.That(PortraitCatalog.BasePortrait(patientId), Is.EqualTo(logicalId));
            var catalog = Resources.Load<PortraitCatalog>("MiniGame/PortraitCatalog");
            Assert.That(catalog, Is.Not.Null);
            var sprite = catalog.Find(logicalId);
            Assert.That(sprite, Is.Not.Null);
            Assert.That(sprite.name, Is.EqualTo(sourceId));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void ReimportPreservesWorkbookPosesAndRepairsMissingEntries(bool removeReorderedEntries)
        {
            const string catalogPath = "Assets/Resources/MiniGame/PortraitCatalog.asset";
            string diskPath = ProjectPath(catalogPath);
            byte[] original = File.ReadAllBytes(diskPath);
            var catalog = AssetDatabase.LoadAssetAtPath<PortraitCatalog>(catalogPath);
            Assert.That(catalog, Is.Not.Null);
            var playerOne = catalog.Find("SJ01");
            var playerTwo = catalog.Find("SJ02");
            try
            {
                if (removeReorderedEntries)
                {
                    var serialized = new SerializedObject(catalog);
                    var entries = serialized.FindProperty("entries");
                    for (int i = entries.arraySize - 1; i >= 0; i--)
                    {
                        string id = entries.GetArrayElementAtIndex(i).FindPropertyRelative("id").stringValue;
                        if (id == "ED01" || id == "JN01" || id == "D0_01") entries.DeleteArrayElementAtIndex(i);
                    }
                    serialized.ApplyModifiedPropertiesWithoutUndo();
                }
                MiniGame.UnityHost.Editor.NamedPortraitImport.Run();
                foreach (var mapping in ExpectedMappings)
                {
                    var sprite = catalog.Find(mapping[0]);
                    Assert.That(sprite, Is.Not.Null, mapping[0]);
                    Assert.That(AssetDatabase.GetAssetPath(sprite),
                        Is.EqualTo(NamedDirectory + "/" + mapping[1] + ".png"), mapping[0]);
                }
                Assert.That(catalog.Find("SJ01"), Is.SameAs(playerOne));
                Assert.That(catalog.Find("SJ02"), Is.SameAs(playerTwo));
            }
            finally
            {
                File.WriteAllBytes(diskPath, original);
                AssetDatabase.ImportAsset(catalogPath, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
            }
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
