using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace MiniGame.UnityHost.Tests
{
    public sealed class SupervisorDialogueLayoutTests
    {
        private static readonly System.Type Style = typeof(MiniGameScreen).Assembly.GetType("MiniGame.UnityHost.SupervisorDialogueStyle", true);
        private GameObject root;
        private Font font;

        [TearDown]
        public void TearDown()
        {
            if (root != null) Object.DestroyImmediate(root);
            if (font != null) Object.DestroyImmediate(font);
        }

        [TestCase(1280, 720)]
        [TestCase(1920, 1080)]
        [TestCase(2560, 1440)]
        [TestCase(1440, 1080)]
        public void AllAuthoredDoorAndDreamLinesKeepTheirRenderedGlyphsInsideTheCard(int width, int height)
        {
            var body = Create(width, height, out var panel);
            var dream = typeof(MiniGameScreen).Assembly.GetType("MiniGame.UnityHost.DreamSupervisorDialogue", true)
                .GetField("Lines", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null) as string[];
            var lines = SupervisorDialogueContent.Load().conversations.SelectMany(day => day.lines).Select(line => line.text)
                .Concat(dream).Concat(new[] { "你就是新来的审查员？", "第一行\n第二行\n第三行\n第四行" }).ToArray();
            foreach (var line in lines)
            {
                Layout(panel, body, line);
                body.text = line;
                Canvas.ForceUpdateCanvases();
                AssertGlyphsFit(body, line);
                Assert.That(panel.rect.height, Is.LessThan(height * .9f), line);
                Assert.That(panel.anchorMin.x, Is.EqualTo(.655f).Within(.0001f));
                Assert.That(panel.anchorMax.x, Is.EqualTo(.97f).Within(.0001f));
            }
        }

        [Test]
        public void ExtraLinesSpendVerticalPaddingBeforeTheyGrowTheCard()
        {
            var body = Create(1920, 1080, out var panel);
            var heights = new float[4];
            for (int lines = 1; lines <= 4; lines++)
            {
                var text = string.Join("\n", Enumerable.Repeat("你就是新来的审查员？", lines));
                Layout(panel, body, text);
                body.text = text;
                Canvas.ForceUpdateCanvases();
                heights[lines - 1] = panel.rect.height;
                AssertGlyphsFit(body, text);
            }
            Assert.That(heights[1], Is.EqualTo(heights[0]).Within(1), "Two lines use the existing padding instead of enlarging the panel.");
            Assert.That(heights[2] - heights[0], Is.LessThan(35), "Three lines should remain close to the short-line silhouette.");
            Assert.That(heights[3] - heights[2], Is.LessThan(75), "Growth is limited to the text's added line height.");
        }

        private Text Create(int width, int height, out RectTransform panel)
        {
            root = new GameObject("Supervisor layout viewport", typeof(RectTransform));
            ((RectTransform)root.transform).sizeDelta = new Vector2(width, height);
            panel = new GameObject("Supervisor panel", typeof(RectTransform)).GetComponent<RectTransform>();
            panel.SetParent(root.transform, false);
            var body = new GameObject("DialogueBody", typeof(RectTransform), typeof(Text)).GetComponent<Text>();
            body.transform.SetParent(panel, false);
            font = GameUiTypography.CreateFont();
            Style.GetMethod("ApplyText", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, new object[] { body, font, null });
            return body;
        }

        private static void Layout(RectTransform panel, Text body, string text) =>
            Style.GetMethod("Layout", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, new object[] { panel, body, text });

        private static void AssertGlyphsFit(Text body, string text)
        {
            var settings = body.GetGenerationSettings(body.rectTransform.rect.size);
            Assert.That(body.cachedTextGenerator.Populate(text, settings), Is.True, text);
            var vertices = body.cachedTextGenerator.verts;
            Assert.That(vertices.Count, Is.GreaterThan(4), text);
            foreach (var vertex in vertices)
                Assert.That(vertex.position.y / body.pixelsPerUnit,
                    Is.InRange(body.rectTransform.rect.yMin - 2, body.rectTransform.rect.yMax + 2), text);
        }
    }
}
