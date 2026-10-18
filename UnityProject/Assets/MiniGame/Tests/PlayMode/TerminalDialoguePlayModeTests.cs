using System;
using System.Collections;
using System.Linq;
using MiniGame.Core;
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
    public sealed class TerminalDialoguePlayModeTests
    {
        [UnityTest]
        [Timeout(120000)]
        public IEnumerator NativeYarnDrivesSixRoundsAndAllThreeDispositionReactions()
        {
            foreach (var disposition in new[] { Disposition.Treat, Disposition.Purify, Disposition.Release })
            {
                yield return SceneManager.LoadSceneAsync("InspectionApartment");
                yield return null;
                var host = Object.FindObjectOfType<MiniGameBootstrap>();
                Assert.That(host.TerminalDemoActive, Is.True, "The validated resource package must boot by default.");
                var runtime = host.Runtime;
                int replies = 0;
                bool submitted = false;
                bool capturedReview = false;
                bool capturedGlitch = false;
                float deadline = Time.realtimeSinceStartup + 35f;
                while (runtime.GetView().TerminalPage != TerminalPage.Archive && Time.realtimeSinceStartup < deadline)
                {
                    var view = runtime.GetView();
                    if (disposition == Disposition.Treat && !capturedGlitch
                        && view.Cue?.Kind == PresentationKind.ExitTerminal && !view.LeavingTerminal
                        && host.Screen.GetComponentsInChildren<Transform>(false).Any(item => item.name == "MonitorSignalLoss"))
                    {
                        capturedGlitch = true;
                        yield return PrototypePlayModeTests.CaptureIfRequested(host, "terminal-glitch");
                    }
                    if (view.Input == InputContext.World)
                        Assert.That(runtime.TryInteract(view.AllowedInteractions.First()).Accepted, Is.True);
                    else if (view.Input == InputContext.TerminalDirectory)
                    {
                        if (disposition == Disposition.Treat) yield return PrototypePlayModeTests.CaptureIfRequested(host, "terminal-directory");
                        Click(Find(host, "EnterReview"));
                    }
                    else if (view.Game.EncounterStage == EncounterStage.AwaitingDisposition && view.Input == InputContext.Dialogue)
                    {
                        Assert.That(replies, Is.EqualTo(6));
                        if (disposition == Disposition.Release)
                            Assert.That(view.Game.AvailableDispositions, Does.Contain(Disposition.Release));
                        if (disposition == Disposition.Treat) yield return PrototypePlayModeTests.CaptureIfRequested(host, "terminal-measure");
                        var button = Find(host, "Disposition-" + disposition);
                        Click(button); Click(button); // The stale second pointer click must not submit twice.
                        submitted = true;
                    }
                    else
                    {
                        var next = Find(host, "Continue");
                        if (next != null) Click(next);
                        else
                        {
                            var options = host.Screen.GetComponentsInChildren<Button>(true)
                                .Where(button => button.name.StartsWith("Reply-", StringComparison.Ordinal)
                                    && button.gameObject.activeInHierarchy && button.interactable).ToArray();
                            if (options.Length > 0)
                            {
                                var preferred = view.Game.CurrentNodeId.Contains("R3") ? "D1_R3_O2"
                                    : view.Game.CurrentNodeId.Contains("R6") ? "D1_R6_O3" : null;
                                var option = options.FirstOrDefault(button => button.name == "Reply-" + preferred) ?? options[0];
                                if (!capturedReview)
                                {
                                    capturedReview = true;
                                    if (disposition == Disposition.Treat)
                                    {
                                        yield return PrototypePlayModeTests.CaptureIfRequested(host, "terminal-review");
                                        Click(Find(host, "HistoryTab"));
                                        yield return PrototypePlayModeTests.CaptureIfRequested(host, "terminal-history");
                                        Click(Find(host, "AnswersTab"));
                                    }
                                }
                                Click(option); replies++;
                            }
                        }
                    }
                    yield return null;
                }
                Assert.That(submitted, Is.True, host.GetComponent<MiniGameYarnBridge>().Diagnostics);
                Assert.That(runtime.GetView().TerminalPage, Is.EqualTo(TerminalPage.Archive), host.GetComponent<MiniGameYarnBridge>().Diagnostics);
                Assert.That(runtime.GetView().Transcript.Count(entry => entry.IsPlayer && !entry.SourceNodeId.StartsWith("disposition:")), Is.EqualTo(6));
                Assert.That(runtime.GetView().Transcript.Any(entry => entry.SourceNodeId == "D1_R1_OPEN1" && entry.Text == "这里还要检测吗？"), Is.True);
                Assert.That(runtime.GetView().Transcript.Where(entry => !entry.IsPlayer).All(entry => entry.PortraitId.StartsWith("ML", StringComparison.Ordinal)), Is.True);
                Assert.That(runtime.GetView().Game.RemainingPurificationQuota, Is.EqualTo(disposition == Disposition.Purify ? 4 : 5));
                var quota = host.Screen.GetComponentsInChildren<Text>(true).Single(text => text.name == "PurificationQuota");
                Assert.That(quota.text, Is.EqualTo(runtime.GetView().Game.RemainingPurificationQuota.ToString("00")));
                Assert.That(quota.preferredWidth, Is.LessThanOrEqualTo(((RectTransform)quota.transform).rect.width));
                if (disposition == Disposition.Treat) yield return PrototypePlayModeTests.CaptureIfRequested(host, "terminal-signal-lost");
                Click(Find(host, "ReturnDirectory"));
                Assert.That(runtime.GetView().ArchiveDirectoryVisible, Is.True);
                Assert.That(Find(host, "EnterReview"), Is.Null);
                var checkpoint = runtime.Save();
                host.RestoreCheckpoint(checkpoint);
                Assert.That(host.Runtime.GetView().ArchiveDirectoryVisible, Is.True);
                Assert.That(host.Runtime.GetView().Game.RemainingPurificationQuota, Is.EqualTo(disposition == Disposition.Purify ? 4 : 5));
                if (disposition == Disposition.Treat) yield return PrototypePlayModeTests.CaptureIfRequested(host, "terminal-archived-directory");
            }
        }

        [UnityTest]
        [Timeout(30000)]
        public IEnumerator PausingImmediatelyAfterContinueDoesNotLoseTheNextTranscriptLine()
        {
            yield return SceneManager.LoadSceneAsync("InspectionApartment");
            yield return null;
            var host = Object.FindObjectOfType<MiniGameBootstrap>();
            var runtime = host.Runtime;
            runtime.TryInteract(InteractionTarget.Door);
            float deadline = Time.realtimeSinceStartup + 15f;
            while (runtime.GetView().Input != InputContext.TerminalDirectory && Time.realtimeSinceStartup < deadline)
            {
                if (runtime.GetView().Input == InputContext.World) runtime.TryInteract(InteractionTarget.Computer);
                else { var next = Find(host, "Continue"); if (next != null) Click(next); }
                yield return null;
            }
            Click(Find(host, "EnterReview"));
            while (!runtime.GetView().Transcript.Any(entry => entry.SourceNodeId == "D1_R1_OPEN1") && Time.realtimeSinceStartup < deadline)
                yield return null;
            Click(Find(host, "Continue"));
            runtime.SetPaused(true);
            yield return null; yield return null; yield return null;
            Assert.That(runtime.GetView().Transcript.Any(entry => entry.SourceNodeId == "D1_R1_OPEN2"), Is.False);
            runtime.SetPaused(false);
            while (!runtime.GetView().Transcript.Any(entry => entry.SourceNodeId == "D1_R1_OPEN2") && Time.realtimeSinceStartup < deadline)
                yield return null;
            Assert.That(runtime.GetView().Transcript.Count(entry => entry.SourceNodeId == "D1_R1_OPEN2"), Is.EqualTo(1));
            Click(Find(host, "Continue"));
            Click(Find(host, "HistoryTab"));
            yield return null; yield return null; yield return null;
            Assert.That(Find(host, "Reply-D1_R1_O1"), Is.Null, "Choice presentation waits behind the history overlay.");
            Assert.That(host.Screen.TryCloseHistory(), Is.True);
            while (Find(host, "Reply-D1_R1_O1") == null && Time.realtimeSinceStartup < deadline) yield return null;
            Click(Find(host, "Reply-D1_R1_O1"));
            Click(Find(host, "HistoryTab"));
            yield return null; yield return null; yield return null;
            Assert.That(runtime.GetView().Transcript.Any(entry => entry.SourceNodeId == "D1_R1_CLOSING1"), Is.False,
                "The next Yarn line must wait while history covers the conversation.");
            Assert.That(host.Screen.TryCloseHistory(), Is.True);
            while (!runtime.GetView().Transcript.Any(entry => entry.SourceNodeId == "D1_R1_CLOSING1") && Time.realtimeSinceStartup < deadline)
                yield return null;
            Assert.That(runtime.GetView().Transcript.Count(entry => entry.SourceNodeId == "D1_R1_CLOSING1"), Is.EqualTo(1));
            Assert.That(runtime.GetView().Paused, Is.False);
        }

        private static Button Find(MiniGameBootstrap host, string name) => host.Screen.GetComponentsInChildren<Button>(true)
            .FirstOrDefault(button => button.name == name && button.gameObject.activeInHierarchy && button.interactable);

        private static void Click(Button button)
        {
            Assert.That(button, Is.Not.Null);
            Canvas.ForceUpdateCanvases();
            var rectangle = (RectTransform)button.transform;
            var camera = button.GetComponentInParent<Canvas>().worldCamera;
            var point = RectTransformUtility.WorldToScreenPoint(camera, rectangle.TransformPoint(rectangle.rect.center));
            var pointer = new PointerEventData(EventSystem.current) { button = PointerEventData.InputButton.Left, position = point };
            ExecuteEvents.Execute(button.gameObject, pointer, ExecuteEvents.pointerDownHandler);
            ExecuteEvents.Execute(button.gameObject, pointer, ExecuteEvents.pointerUpHandler);
            ExecuteEvents.Execute(button.gameObject, pointer, ExecuteEvents.pointerClickHandler);
        }
    }
}
