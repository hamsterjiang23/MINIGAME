using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace MiniGame.UnityHost.Tests
{
    public sealed class TerminalDossierAssetTests
    {
        [Test]
        public void CatalogMatchesEveryAuthoredFieldAndKeepsTheTutorialPortraitSeparate()
        {
            var source = JsonUtility.FromJson<Source>(File.ReadAllText("Assets/MiniGame/Content/TerminalDossier/profiles.json"));
            var catalog = Resources.Load<TerminalDossierCatalog>("MiniGame/TerminalDossiers");
            Assert.That(source.entries.Length, Is.EqualTo(7));
            foreach (var expected in source.entries)
            {
                var actual = catalog.Find(expected.patientId);
                Assert.That(actual, Is.Not.Null, expected.patientId);
                Assert.That(actual.age, Is.EqualTo(expected.age));
                Assert.That(actual.occupation, Is.EqualTo(expected.occupation));
                Assert.That(actual.summary, Is.EqualTo(expected.summary));
                Assert.That(actual.portraitId, Is.EqualTo(expected.portraitId));
                if (expected.patientId == "tutorial") { Assert.That(actual.portrait, Is.Null); continue; }
                Assert.That(actual.portrait.rect.size, Is.EqualTo(new Vector2(96, 128)));
                Assert.That(AssetDatabase.GetAssetPath(actual.portrait), Does.EndWith(expected.portraitId + "_Amber.png"));
                var importer = (TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(actual.portrait));
                Assert.That(importer.filterMode, Is.EqualTo(FilterMode.Point));
                Assert.That(importer.mipmapEnabled, Is.False);
                Assert.That(importer.textureCompression, Is.EqualTo(TextureImporterCompression.Uncompressed));
                Assert.That(importer.npotScale, Is.EqualTo(TextureImporterNPOTScale.None));
            }
            Assert.That(catalog.Find("former-ruler").occupation, Is.EqualTo("***"));
            Assert.That(catalog.Find("diplomat").summary, Does.Contain("********"));
            Assert.That(catalog.Find("not-a-patient"), Is.Null);
        }

        [TestCase(0, true)]
        [TestCase(5, true)]
        [TestCase(99, true)]
        [TestCase(100, false)]
        public void DossierQuotaUsesItsOwnNativeAtlasAndPreservesActualValues(int value, bool artworkExpected)
        {
            var assets = Resources.Load<TerminalAssetSet>("MiniGame/TerminalAssets");
            var owner = new GameObject("Dossier quota test", typeof(RectTransform));
            var child = new GameObject("Value", typeof(RectTransform), typeof(Text));
            child.transform.SetParent(owner.transform);
            try
            {
                var label = child.GetComponent<Text>();
                var field = owner.AddComponent<TerminalQuotaField>();
                field.Initialize(assets.DossierQuotaFields, assets.DossierQuotaMetrics, label, new Vector2Int(56, 24));
                field.SetValue(value);
                Assert.That(label.text, Is.EqualTo(value.ToString("00")));
                Assert.That(owner.GetComponent<RectTransform>().rect.size, Is.EqualTo(new Vector2(56, 24)));
                var art = owner.GetComponentInChildren<RawImage>();
                Assert.That(art.enabled, Is.EqualTo(artworkExpected));
                Assert.That(label.enabled, Is.EqualTo(!artworkExpected));
                if (artworkExpected)
                {
                    Assert.That(art.uvRect.x, Is.EqualTo(value % 10 / 10f).Within(.00001));
                    Assert.That(art.uvRect.y, Is.EqualTo(1 - (value / 10 + 1) / 10f).Within(.00001));
                }
            }
            finally { UnityEngine.Object.DestroyImmediate(owner); }
        }

        [Serializable] private sealed class Source { public TerminalDossierCatalog.Entry[] entries; }
    }
}
