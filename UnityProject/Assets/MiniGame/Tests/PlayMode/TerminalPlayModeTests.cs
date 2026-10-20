using System.Collections;
using System.Linq;
using MiniGame.Core;
using MiniGame.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace MiniGame.UnityHost.Tests
{
    public sealed class TerminalPlayModeTests
    {
        [UnityTest]
        public IEnumerator OverlayTerminalFitsActualScreenAndDifferentViewportAspectRatios()
        {
            var rules = PrototypeRules.Create();
            var runtime = new GameRuntime(rules, new PrototypeContent(rules), useTerminalDirectory: true);
            var owner = new GameObject("Terminal Bounds Test");
            var screen = owner.AddComponent<MiniGameScreen>();
            try
            {
                screen.Bind(runtime);
                OpenComputer(runtime);
                yield return null;
                Assert.That(screen.Canvas.renderMode, Is.EqualTo(RenderMode.ScreenSpaceOverlay));
                Canvas.ForceUpdateCanvases();
                screen.ReflowTerminal();
                var logical = screen.TerminalDisplay.OutputRect;
                var viewport = screen.GetComponentsInChildren<RectTransform>(false).Single(rect => rect.name == "Terminal");
                AssertOverlayBounds(logical, new Rect(0, 0, Screen.width, Screen.height));
                foreach (var size in new[] { new Vector2(640, 360), new Vector2(640, 480), new Vector2(960, 360), new Vector2(360, 640) })
                {
                    viewport.anchorMin = viewport.anchorMax = viewport.pivot = new Vector2(.5f, .5f);
                    viewport.anchoredPosition = Vector2.zero;
                    viewport.sizeDelta = size;
                    Canvas.ForceUpdateCanvases();
                    screen.ReflowTerminal();
                    var viewportCorners = new Vector3[4];
                    viewport.GetWorldCorners(viewportCorners);
                    var lower = RectTransformUtility.WorldToScreenPoint(null, viewportCorners[0]);
                    var upper = RectTransformUtility.WorldToScreenPoint(null, viewportCorners[2]);
                    AssertOverlayBounds(logical, Rect.MinMaxRect(lower.x, lower.y, upper.x, upper.y));
                    Assert.That(logical.rect.width / 800f, Is.EqualTo(Mathf.Min(size.x / 800f, size.y / 600f)).Within(.001f));
                }
            }
            finally { Object.Destroy(owner); }
            yield return null;
        }

        private static void AssertOverlayBounds(RectTransform logical, Rect expectedBounds)
        {
            var corners = new Vector3[4];
            logical.GetWorldCorners(corners);
            var projected = corners.Select(corner => RectTransformUtility.WorldToScreenPoint(null, corner)).ToArray();
            float left = projected.Min(point => point.x), right = projected.Max(point => point.x);
            float bottom = projected.Min(point => point.y), top = projected.Max(point => point.y);
            Debug.Log("MINIGAME_TERMINAL_OVERLAY_BOUNDS screen=" + Screen.width + "x" + Screen.height
                + " expected=" + expectedBounds + " actual=" + Rect.MinMaxRect(left, bottom, right, top)
                + " logical=" + logical.rect + " scale=" + logical.localScale + " worldScale=" + logical.lossyScale);
            Assert.That(left, Is.GreaterThanOrEqualTo(expectedBounds.xMin - 1));
            Assert.That(right, Is.LessThanOrEqualTo(expectedBounds.xMax + 1));
            Assert.That(bottom, Is.GreaterThanOrEqualTo(expectedBounds.yMin - 1));
            Assert.That(top, Is.LessThanOrEqualTo(expectedBounds.yMax + 1));
            Assert.That(right - left, Is.GreaterThan(1));
            Assert.That((right - left) / (top - bottom), Is.EqualTo(4f / 3).Within(.005f));
        }

        [UnityTest]
        public IEnumerator DirectoryRejectsUnscheduledPatientAndDoubleClickSelectsAnyAvailablePatient()
        {
            var rules = PrototypeRules.Create();
            var runtime = new GameRuntime(rules, new PrototypeContent(rules), useTerminalDirectory: true);
            var owner = new GameObject("Terminal UI Test");
            var screen = owner.AddComponent<MiniGameScreen>();
            try
            {
                screen.Bind(runtime);
                OpenComputer(runtime);
                yield return null;
                Assert.That(runtime.GetView().TerminalPage, Is.EqualTo(TerminalPage.Directory));
                var buttons = screen.GetComponentsInChildren<Button>(false);
                Assert.That(buttons.Count(button => button.name.StartsWith("Roster-")), Is.EqualTo(5));
                var unavailable = buttons.Single(button => button.name == "Roster-4");
                unavailable.onClick.Invoke();
                var enter = buttons.Single(button => button.name == "EnterReview");
                Assert.That(enter.interactable, Is.False);
                unavailable.GetComponent<TerminalRosterPointer>().OnPointerClick(new PointerEventData(EventSystem.current)
                { button = PointerEventData.InputButton.Left, clickCount = 2 });
                Assert.That(runtime.GetView().TerminalPage, Is.EqualTo(TerminalPage.Directory));
                var doctor = buttons.Single(button => button.name == "Roster-2");
                doctor.GetComponent<TerminalRosterPointer>().OnPointerClick(new PointerEventData(EventSystem.current)
                { button = PointerEventData.InputButton.Left, clickCount = 2 });
                Assert.That(runtime.GetView().TerminalPage, Is.EqualTo(TerminalPage.Review));
                Assert.That(runtime.GetView().Game.CurrentPatientId, Is.EqualTo(PrototypeRules.Doctor));
            }
            finally { Object.Destroy(owner); }
            yield return null;
        }

        [UnityTest]
        public IEnumerator PassiveTranscriptDoesNotTakeKeyboardFocus()
        {
            var rules=PrototypeRules.Create();var runtime=new GameRuntime(rules,new PrototypeContent(rules),true);
            var owner=new GameObject("Passive transcript test");var screen=owner.AddComponent<MiniGameScreen>();
            try
            {
                screen.Bind(runtime);OpenComputer(runtime);runtime.EnterSelectedPatient(runtime.GetView().Game.CurrentPatientId);
                if(runtime.GetView().Cue!=null)runtime.CompleteCue(runtime.GetView().Cue.Token);
                screen.UseYarnPresentation();int advanced=0;
                screen.SetDialogueLine("对象","我数了数，一共有七扇门。","terminal-test-line","ML02");
                screen.ShowCueText("继续",()=>advanced++);
                screen.HandleTerminalKey(KeyCode.F2);screen.HandleTerminalKey(KeyCode.F3);
                Assert.That(screen.HistoryOpen,Is.False);
                screen.HandleTerminalKey(KeyCode.Space);Assert.That(advanced,Is.EqualTo(1));
                Assert.That(screen.GetComponentsInChildren<Transform>(true).Any(t=>t.name=="TerminalSettings"||t.name=="HistoryTab"),Is.False);
            }
            finally {Object.Destroy(owner);}
            yield return null;
        }

        private static void OpenComputer(GameRuntime runtime)
        {
            Assert.That(runtime.TryInteract(InteractionTarget.Door).Accepted, Is.True);
            Assert.That(runtime.CompleteCue(runtime.GetView().Cue.Token).Accepted, Is.True);
            Assert.That(runtime.CompleteCue(runtime.GetView().Cue.Token).Accepted, Is.True);
            Assert.That(runtime.TryInteract(InteractionTarget.Computer).Accepted, Is.True);
            var cue = runtime.GetView().Cue;
            if (cue != null) Assert.That(runtime.CompleteCue(cue.Token).Accepted, Is.True);
        }
    }
}
