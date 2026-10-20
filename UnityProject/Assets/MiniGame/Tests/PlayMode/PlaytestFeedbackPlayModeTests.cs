using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MiniGame.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace MiniGame.UnityHost.Tests
{
    public sealed class PlaytestFeedbackPlayModeTests
    {
        [UnityTest]
        public IEnumerator CompletedParagraphsStayWholeAndWheelScrollRetainsTheReadingPosition()
        {
            var root = new GameObject("Transcript regression", typeof(RectTransform), typeof(Canvas));
            var rect = (RectTransform)root.transform; rect.sizeDelta = new Vector2(800, 600);
            root.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
            var font = Resources.Load<TerminalAssetSet>("MiniGame/TerminalAssets")?.Typeface
                ?? Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var type = typeof(MiniGameScreen).Assembly.GetType("MiniGame.UnityHost.TerminalTranscriptView");
            var view = System.Activator.CreateInstance(type, new object[] { rect, font });
            var entries = new List<TranscriptEntry>();
            string paragraph = "仪器亮红灯，只能说明人害怕、愤怒，或者悲伤。\n它怎么证明谁会伤人？";
            for (int index = 0; index < 12; index++)
                entries.Add((TranscriptEntry)System.Activator.CreateInstance(typeof(TranscriptEntry),
                    BindingFlags.Instance | BindingFlags.NonPublic, null,
                    new object[] { "entry" + index, "encounter", "node", "对象", paragraph, "source" + index, "", false, false, true }, null));
            try
            {
                type.GetMethod("Render").Invoke(view, new object[] { entries, "source11", "伤人？" });
                yield return null;
                Canvas.ForceUpdateCanvases();
                var texts = root.GetComponentsInChildren<Text>().Where(item => item.name.StartsWith("TranscriptText")).ToArray();
                Assert.That(texts.Length, Is.EqualTo(12));
                foreach (var text in texts)
                {
                    Assert.That(text.text, Is.EqualTo(paragraph), "The final playback page must not replace completed speech.");
                    Assert.That(text.rectTransform.rect.height, Is.GreaterThanOrEqualTo(text.preferredHeight - .1f));
                }
                var scroll = root.GetComponentInChildren<ScrollRect>();
                scroll.verticalNormalizedPosition = 0;
                ExecuteEvents.ExecuteHierarchy(scroll.gameObject,
                    new PointerEventData(EventSystem.current) { scrollDelta = Vector2.up }, ExecuteEvents.scrollHandler);
                Assert.That(scroll.verticalNormalizedPosition, Is.GreaterThan(0), "Wheel input must reach the transcript.");
                float offset = scroll.content.anchoredPosition.y;
                type.GetMethod("Render").Invoke(view, new object[] { entries, "source11", "另一个已完成分页" });
                Assert.That(scroll.content.anchoredPosition.y, Is.EqualTo(offset).Within(.2f), "Refreshing speech must not pull a reader back to the bottom.");
            }
            finally { Object.Destroy(root); }
        }

        [UnityTest]
        public IEnumerator PlayerUsesThirtyFrameLimitLowerSensitivityAndAnExitAction()
        {
            yield return SceneManager.LoadSceneAsync("MainRoomCombined");
            yield return null;
            var host = Object.FindObjectOfType<MiniGameBootstrap>();
            Assert.That(Application.targetFrameRate, Is.EqualTo(30));
            Assert.That(QualitySettings.vSyncCount, Is.Zero);
            float sensitivity = (float)typeof(ApartmentPlayer).GetField("lookSensitivity", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(host.Player);
            Assert.That(sensitivity, Is.EqualTo(.7f).Within(.001f));
            host.Runtime.SetPaused(true);
            yield return null;
            var quit = host.Screen.GetComponentsInChildren<Text>(true).Single(text => text.text == "退出游戏");
            Assert.That(quit.GetComponentInParent<Button>().IsInteractable(), Is.True);
            Assert.That(quit.gameObject.activeInHierarchy, Is.True);
        }
    }
}
