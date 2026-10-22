using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace MiniGame.UnityHost.Tests
{
    public sealed class TerminalBitmapTextTests
    {
        private GameObject owner;
        private TerminalBitmapText label;

        [SetUp]
        public void SetUp()
        {
            owner = new GameObject("HTML bitmap glyph test", typeof(RectTransform), typeof(TerminalBitmapText));
            label = owner.GetComponent<TerminalBitmapText>();
            label.font = null;
            label.text = "AA";
            label.fontSize = 14;
            label.color = new Color32(239, 184, 63, 255);
            label.alignment = TextAnchor.UpperLeft;
            label.rectTransform.pivot = new Vector2(0, 1);
            label.rectTransform.sizeDelta = new Vector2(100, 40);
            label.verticalOverflow = VerticalWrapMode.Overflow;
        }

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(owner);

        [Test]
        public void MixedTextUsesHtmlCharacterAdvancesAndPreservesExplicitLines()
        {
            Assert.That(TerminalTextMetrics.Width("A中B", 14), Is.EqualTo(32));
            Assert.That(TerminalTextMetrics.Width("A中\r\nBB", 14), Is.EqualTo(23));
            var lines = TerminalTextMetrics.Layout("A中B\r\nC", 14, 23);
            Assert.That(lines.Count, Is.EqualTo(3));
            Assert.That(lines[0].Start, Is.Zero);
            Assert.That(lines[0].Length, Is.EqualTo(2));
            Assert.That(lines[1].Start, Is.EqualTo(2));
            Assert.That(lines[2].Start, Is.EqualTo(5));
            Assert.That(TerminalTextMetrics.MeasureHeight("A中B", 14, 23), Is.EqualTo(40));
            Assert.That(TerminalTextMetrics.Layout("", 14, 23), Is.Empty);
        }

        [TestCase(10)] [TestCase(11)] [TestCase(12)] [TestCase(13)]
        [TestCase(14)] [TestCase(18)] [TestCase(19)]
        public void EveryExportedSizeDrawsAtlasPixelsWithoutADynamicFont(int size)
        {
            label.fontSize = size;
            var vertices = ReadVertices();
            Assert.That(vertices.Count, Is.EqualTo(8));
            Assert.That(label.font, Is.Null);
            Assert.That(label.mainTexture, Is.Not.EqualTo(Texture2D.whiteTexture));
            Assert.That(vertices[4].position.x - vertices[0].position.x,
                Is.EqualTo(Mathf.Ceil(size * .6f)));
            foreach (var vertex in vertices)
            {
                Assert.That(vertex.color, Is.EqualTo((Color32)label.color));
                Assert.That(vertex.uv0.x, Is.InRange(0f, 1f));
                Assert.That(vertex.uv0.y, Is.InRange(0f, 1f));
            }
        }

        [TestCase(239, 184, 63)] [TestCase(255, 207, 104)] [TestCase(167, 125, 45)]
        [TestCase(103, 75, 25)] [TestCase(23, 14, 2)]
        public void ExportedColorsAreNotRetinted(int red, int green, int blue)
        {
            var requested = new Color32((byte)red, (byte)green, (byte)blue, 187);
            Assert.That(TerminalBitmapText.ResolveColor(requested, out var tint).r, Is.EqualTo(red));
            Assert.That(tint, Is.EqualTo(new Color32(255, 255, 255, 187)));
        }

        [Test]
        public void NonExportedColorsReuseCoveredGlyphsRatherThanFallbackQuestionMarks()
        {
            label.color = new Color32(228, 220, 180, 255);
            var requested = TerminalBitmapText.ResolveColor((Color32)label.color, out var tint);
            var approximated = ReadVertices();
            label.color = requested;
            var exported = ReadVertices();
            Assert.That(approximated.Count, Is.EqualTo(exported.Count));
            for (int index = 0; index < approximated.Count; index++)
            {
                Assert.That(approximated[index].uv0, Is.EqualTo(exported[index].uv0));
                Assert.That(approximated[index].color, Is.EqualTo(new Color32(228, 220, 180, 255)));
            }
        }

        [Test]
        public void AlignmentWrapAndTruncationUseTheSameFixedMetrics()
        {
            var original = ReadVertices();
            label.alignment = TextAnchor.MiddleCenter;
            var centered = ReadVertices();
            Assert.That(centered[0].position.x - original[0].position.x, Is.EqualTo(41));
            Assert.That(original[0].position.y - centered[0].position.y, Is.EqualTo(13));
            label.alignment = TextAnchor.LowerRight;
            var lowerRight = ReadVertices();
            Assert.That(lowerRight[0].position.x - original[0].position.x, Is.EqualTo(82));
            Assert.That(original[0].position.y - lowerRight[0].position.y, Is.EqualTo(26));
            label.alignment = TextAnchor.UpperLeft;
            label.rectTransform.sizeDelta = new Vector2(9, 40);
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            var wrapped = ReadVertices();
            Assert.That(wrapped[0].position.y - wrapped[4].position.y, Is.EqualTo(20));
            Assert.That(label.preferredHeight, Is.EqualTo(40));
            label.rectTransform.sizeDelta = new Vector2(9, 14);
            label.verticalOverflow = VerticalWrapMode.Truncate;
            Assert.That(ReadVertices().Count, Is.EqualTo(4));
            label.verticalOverflow = VerticalWrapMode.Overflow;
            Assert.That(ReadVertices().Count, Is.EqualTo(8));
        }

        [Test]
        public void CenteredLabelsUseIntegerHtmlCoordinatesAtOddWidths()
        {
            label.text = "A";
            label.rectTransform.sizeDelta = new Vector2(100, 41);
            var original = ReadVertices();
            label.alignment = TextAnchor.MiddleCenter;
            var centered = ReadVertices();
            Assert.That(centered[0].position.x - original[0].position.x, Is.EqualTo(45));
            Assert.That(original[0].position.y - centered[0].position.y, Is.EqualTo(13));
        }

        [Test]
        public void CommandRowsKeepTheOriginalValueAndUseSeparateTwelveAndFourteenPixelRuns()
        {
            label.text = "01  A";
            label.CommandRow = true;
            var row = ReadVertices();
            Assert.That(label.text, Is.EqualTo("01  A"));
            Assert.That(row.Count, Is.EqualTo(12));
            label.CommandRow = false;
            label.text = "A";
            var body = ReadVertices();
            Assert.That(row[8].position.x - body[0].position.x, Is.EqualTo(34));
            Assert.That(row[8].position.y, Is.EqualTo(body[0].position.y));
            label.fontSize = 12;
            label.text = "01";
            var number = ReadVertices();
            Assert.That(row[0].position.y, Is.EqualTo(number[0].position.y - 1));
            Assert.That(row[0].uv0, Is.EqualTo(number[0].uv0));
        }

        [Test]
        public void GeometryUpdatesWithoutFontAndParticipatesInRectMaskClipping()
        {
            var canvasObject = new GameObject("Bitmap clipping canvas", typeof(RectTransform), typeof(Canvas));
            try
            {
                canvasObject.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
                var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(RectMask2D));
                viewport.transform.SetParent(canvasObject.transform, false);
                var viewportRect = (RectTransform)viewport.transform;
                viewportRect.sizeDelta = new Vector2(100, 40);
                owner.transform.SetParent(viewport.transform, false);
                label.rectTransform.anchoredPosition = Vector2.zero;
                label.Rebuild(CanvasUpdate.PreRender);
                var generated = label.canvasRenderer.GetMesh();
                Assert.That(generated, Is.Not.Null);
                Assert.That(generated.vertexCount, Is.EqualTo(8));
                label.rectTransform.anchoredPosition = new Vector2(500, 0);
                Canvas.ForceUpdateCanvases();
                viewport.GetComponent<RectMask2D>().PerformClipping();
                Assert.That(label.canvasRenderer.cull, Is.True);
            }
            finally { Object.DestroyImmediate(canvasObject); }
        }

        private List<UIVertex> ReadVertices()
        {
            using (var helper = new VertexHelper())
            {
                typeof(TerminalBitmapText).GetMethod("OnPopulateMesh", BindingFlags.Instance | BindingFlags.NonPublic,
                    null, new[] { typeof(VertexHelper) }, null)
                    .Invoke(label, new object[] { helper });
                var result = new List<UIVertex>();
                for (int index = 0; index < helper.currentVertCount; index++)
                {
                    var vertex = new UIVertex(); helper.PopulateUIVertex(ref vertex, index); result.Add(vertex);
                }
                return result;
            }
        }
    }
}
