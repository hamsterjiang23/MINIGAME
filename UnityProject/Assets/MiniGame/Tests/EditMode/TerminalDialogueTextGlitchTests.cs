using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace MiniGame.UnityHost.Tests
{
    public sealed class TerminalDialogueTextGlitchTests
    {
        private GameObject owner;
        private TerminalBitmapText text;
        private TerminalDialogueTextGlitch glitch;
        private TerminalDialogueTextSettings settings;

        [SetUp] public void CreateLabel()
        {
            owner = new GameObject("Dialogue glyph audit", typeof(RectTransform), typeof(TerminalBitmapText));
            text = owner.GetComponent<TerminalBitmapText>();
            text.text = "中文对话记录 ABC 0123"; text.fontSize = 14; text.color = Color.white;
            text.rectTransform.sizeDelta = new Vector2(400, 80);
            glitch = owner.AddComponent<TerminalDialogueTextGlitch>();
            settings = ScriptableObject.CreateInstance<TerminalDialogueTextSettings>();
            settings.BurstChance = settings.GlyphFraction = settings.Intensity = 1;
            settings.HorizontalPixels = 0; settings.VerticalPixels = 2; settings.BrightnessDip = .1f;
            glitch.Configure(settings); glitch.Seed = 123; glitch.PreviewClock = 0;
        }
        [TearDown] public void Cleanup() { Object.DestroyImmediate(owner); Object.DestroyImmediate(settings); }

        [Test] public void ArkAtlasCoversUiAndRetainsAllLogicalSizes()
        {
            var font = TerminalPixelFont.Default;
            Assert.That(font, Is.Not.Null); Assert.That(text.ArkTypeface, Is.SameAs(font));
            Assert.That(font.Atlas.filterMode, Is.EqualTo(FilterMode.Point));
            Assert.That(font.Atlas.mipmapCount, Is.EqualTo(1));
            foreach (int size in new[] { 10, 11, 12, 13, 14, 18, 19 })
                foreach (char character in text.text)
                    Assert.That(font.TryGet(character, size, out _), Is.True, character + " size " + size);
        }
        [Test] public void DisturbanceChangesOnlyGlyphVerticesAndPreservesTextAndRandomState()
        {
            var original = Vertices(false); var random = Random.state;
            string value = text.text;
            var changed = Vertices(true);
            Assert.That(changed.Count, Is.EqualTo(original.Count));
            Assert.That(changed[0].position.y, Is.Not.EqualTo(original[0].position.y));
            for (int index = 0; index < changed.Count; index++)
            {
                Assert.That(changed[index].position.x, Is.EqualTo(original[index].position.x));
                Assert.That(Mathf.Abs(changed[index].position.y - original[index].position.y), Is.LessThanOrEqualTo(2));
                Assert.That(changed[index].uv0, Is.EqualTo(original[index].uv0));
            }
            Assert.That(text.text, Is.EqualTo(value)); Assert.That(Random.state, Is.EqualTo(random));
            var repeated = Vertices(true);
            Assert.That(repeated[0].position, Is.EqualTo(changed[0].position), "Offsets cannot accumulate across mesh rebuilds.");
        }
        [Test] public void DisabledOrIdleEffectRestoresUnchangedGeometry()
        {
            var original = Vertices(false);
            settings.Enabled = false;
            AssertSame(original, Vertices(true));
            settings.Enabled = true; settings.BurstChance = 0;
            AssertSame(original, Vertices(true));
            settings.BurstChance = 1; glitch.PreviewClock = .1f;
            AssertSame(original, Vertices(true));
        }
        private List<UIVertex> Vertices(bool modify)
        {
            using var helper = new VertexHelper();
            typeof(TerminalBitmapText).GetMethod("OnPopulateMesh", BindingFlags.Instance | BindingFlags.NonPublic,
                null, new[] { typeof(VertexHelper) }, null).Invoke(text, new object[] { helper });
            if (modify) glitch.ModifyMesh(helper);
            var result = new List<UIVertex>();
            for (int index = 0; index < helper.currentVertCount; index++)
            { var vertex = new UIVertex(); helper.PopulateUIVertex(ref vertex, index); result.Add(vertex); }
            return result;
        }
        private static void AssertSame(List<UIVertex> expected, List<UIVertex> actual)
        {
            Assert.That(actual.Count, Is.EqualTo(expected.Count));
            for (int index = 0; index < expected.Count; index++)
            { Assert.That(actual[index].position, Is.EqualTo(expected[index].position)); Assert.That(actual[index].color, Is.EqualTo(expected[index].color)); }
        }
    }
}
