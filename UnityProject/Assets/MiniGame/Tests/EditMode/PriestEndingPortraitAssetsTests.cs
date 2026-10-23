using System;
using System.IO;
using System.Security.Cryptography;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MiniGame.UnityHost.EditorTests
{
    public sealed class PriestEndingPortraitAssetsTests
    {
        [TestCase("PE01", "4cc984e332f503b48a84a52dd7923b1264c54f93067758d3b6d61bac7446ed01")]
        [TestCase("PE02", "1168aea49a56cd949ea0d2f50e2cd805a57effdd6fbd0911b657ed7a408612ed")]
        public void SuppliedArtworkKeepsExactPixelsAndRegistersVisibleFeet(string id, string expectedHash)
        {
            string relative = "MiniGame/Art/Characters/EndingPortraits/" + id + ".png";
            byte[] bytes = File.ReadAllBytes(Path.Combine(Application.dataPath, relative));
            using (var sha = SHA256.Create())
                Assert.That(BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-", "").ToLowerInvariant(), Is.EqualTo(expectedHash));
            var catalog = Resources.Load<PortraitCatalog>("MiniGame/PortraitCatalog");
            var sprite = catalog.Find(id);
            Assert.That(AssetDatabase.GetAssetPath(sprite), Is.EqualTo("Assets/" + relative));
            Assert.That(sprite.rect.size, Is.EqualTo(new Vector2(1024, 1536)));
            Assert.That(sprite.pixelsPerUnit, Is.EqualTo(100));
            var image = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            try
            {
                Assert.That(ImageConversion.LoadImage(image, bytes), Is.True);
                var pixels = image.GetPixels32();
                int minY = image.height, maxY = -1;
                for (int y = 0; y < image.height; y++)
                    for (int x = 0; x < image.width; x++)
                        if (pixels[y * image.width + x].a > 16) { minY = Math.Min(minY, y); maxY = Math.Max(maxY, y); }
                int bandHeight = Mathf.CeilToInt((maxY - minY + 1) * .1f);
                double sum = 0; int count = 0;
                for (int y = minY; y < minY + bandHeight; y++)
                    for (int x = 0; x < image.width; x++)
                        if (pixels[y * image.width + x].a > 16) { sum += x + .5; count++; }
                Assert.That(count, Is.GreaterThan(0));
                Assert.That(sprite.pivot.x, Is.EqualTo(sum / count).Within(.001), "Foot center must not follow transparent canvas margins.");
                Assert.That(sprite.pivot.y, Is.EqualTo(minY).Within(.001), "The visible foot baseline must stay at the doorway anchor.");
            }
            finally { Object.DestroyImmediate(image); }
        }

        [Test]
        public void ReimportPreservesBothFootPivotsAndSharedPixelScale()
        {
            var catalog = Resources.Load<PortraitCatalog>("MiniGame/PortraitCatalog");
            Vector2 first = catalog.Find("PE01").pivot, second = catalog.Find("PE02").pivot;
            MiniGame.UnityHost.Editor.EndingContentImport.ImportPortraits();
            Assert.That(catalog.Find("PE01").pivot, Is.EqualTo(first));
            Assert.That(catalog.Find("PE02").pivot, Is.EqualTo(second));
            Assert.That(catalog.Find("PE01").bounds.size, Is.EqualTo(catalog.Find("PE02").bounds.size));
        }
    }
}
