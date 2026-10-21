using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace MiniGame.UnityHost.Tests
{
    public sealed class DialogueBitmapPaginationTests
    {
        private static readonly MethodInfo PaginateMethod = typeof(MiniGameScreen).Assembly
            .GetType("MiniGame.UnityHost.DialogueTextPlayback", true)
            .GetMethod("Paginate", BindingFlags.Static | BindingFlags.NonPublic);
        private GameObject owner;

        [TearDown]
        public void TearDown()
        {
            if (owner != null) Object.DestroyImmediate(owner);
        }

        [Test]
        public void DialogueBodyUsesTheHtmlTwentyNineCharacterLineWithoutADynamicFont()
        {
            var label = Bitmap(415, 32);
            string source = new string('中', 60);
            var pages = Paginate(label, source);
            Assert.That(pages.Select(page => page.Length), Is.EqualTo(new[] { 29, 29, 2 }));
            Assert.That(string.Concat(pages), Is.EqualTo(source));
            Assert.That(label.font, Is.Null, "Pagination must never ask a dynamic font to measure terminal glyphs.");
        }

        [Test]
        public void ConclusionUsesFiveLinesAndRespectsTheConfiguredLinePitchAndSpacing()
        {
            var label = Bitmap(236, 100);
            string source = new string('中', 97);
            Assert.That(Paginate(label, source).Select(page => page.Length), Is.EqualTo(new[] { 80, 17 }));
            label.LinePitch = 18;
            label.lineSpacing = 1.5f;
            Assert.That(Paginate(label, source).Select(page => page.Length), Is.EqualTo(new[] { 64, 33 }));
            label.horizontalOverflow = HorizontalWrapMode.Overflow;
            Assert.That(Paginate(label, source), Is.EqualTo(new[] { source }), "Unwrapped bitmap text uses one explicit line.");
        }

        [TestCase("第一行\r\n第二行\n第三行")]
        [TestCase("第一行\n\n第二行\r\n\r\n第三行")]
        [TestCase("\r\n第一行\r\n")]
        public void ExplicitLinesAndCrLfArePreservedWithoutStrandingASeparator(string source)
        {
            var label = Bitmap(415, 32);
            var pages = Paginate(label, source);
            Assert.That(string.Concat(pages), Is.EqualTo(source));
            Assert.That(pages.All(page => page.Length > 0), Is.True);
            for (int index = 0; index + 1 < pages.Count; index++)
                Assert.That(pages[index].EndsWith("\r") && pages[index + 1].StartsWith("\n"), Is.False,
                    "A CRLF separator stays together even when the viewport fits only one row.");
            if (source == "第一行\r\n第二行\n第三行")
                Assert.That(pages, Is.EqualTo(new[] { "第一行\r\n", "第二行\n", "第三行" }));
        }

        [Test]
        public void TinyViewportAlwaysAdvancesByAWholeTextElementAndRetainsEveryCharacter()
        {
            var label = Bitmap(0, 0);
            string source = "e\u0301中\ud83d\ude42末";
            var pages = Paginate(label, source);
            var boundaries = new HashSet<int>(StringInfo.ParseCombiningCharacters(source)) { source.Length };
            Assert.That(pages.Count, Is.EqualTo(StringInfo.ParseCombiningCharacters(source).Length));
            int end = 0;
            foreach (string page in pages)
            {
                Assert.That(page.Length, Is.GreaterThan(0));
                end += page.Length;
                Assert.That(boundaries.Contains(end), Is.True, "Neither combining marks nor surrogate pairs may be split.");
            }
            Assert.That(string.Concat(pages), Is.EqualTo(source));
            Assert.That(Paginate(label, null), Is.EqualTo(new[] { "" }));
        }

        [Test]
        public void OrdinaryTextStillUsesItsProportionalDynamicFontForPagination()
        {
            owner = new GameObject("Dynamic pagination fixture", typeof(RectTransform), typeof(Text));
            var label = owner.GetComponent<Text>();
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.fontSize = 20;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.rectTransform.sizeDelta = new Vector2(100, 50);
            string source = new string('i', 100);
            var pages = Paginate(label, source);
            Assert.That(string.Concat(pages), Is.EqualTo(source));
            Assert.That(pages.Count, Is.GreaterThan(1));
            Assert.That(pages[0].Length, Is.GreaterThan(16), "Narrow proportional i glyphs must not use the terminal's twelve-pixel Latin cells.");
            var settings = label.GetGenerationSettings(new Vector2(100, 0));
            settings.horizontalOverflow = HorizontalWrapMode.Wrap;
            settings.verticalOverflow = VerticalWrapMode.Overflow;
            using var generator = new TextGenerator();
            foreach (string page in pages)
                Assert.That(generator.GetPreferredHeight(page, settings) / label.pixelsPerUnit, Is.LessThanOrEqualTo(50.01f));
        }

        private TerminalBitmapText Bitmap(float width, float height)
        {
            owner = new GameObject("Bitmap pagination fixture", typeof(RectTransform), typeof(TerminalBitmapText));
            var label = owner.GetComponent<TerminalBitmapText>();
            label.font = null;
            label.fontSize = 14;
            label.LinePitch = 20;
            label.lineSpacing = 1;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Truncate;
            label.rectTransform.sizeDelta = new Vector2(width, height);
            return label;
        }

        private static IReadOnlyList<string> Paginate(Text label, string source)
            => (IReadOnlyList<string>)PaginateMethod.Invoke(null, new object[] { label, source });
    }
}
