using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MiniGame.Core;
using MiniGame.Presentation;
using MiniGame.Presentation.Authoring;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace MiniGame.UnityHost.Tests
{
    public sealed class HistoryDispositionPlayModeTests
    {
        [Serializable] private sealed class SaveEnvelope { public string CoreJson = ""; }

        [UnityTest, Timeout(60000)]
        public IEnumerator DayZeroPassiveRecordKeepsPurificationLockedUntilDialogueThenPlaysBothReactionsOnce()
        {
            yield return SceneManager.LoadSceneAsync("MainRoomCombined");
            yield return null;
            var host = Object.FindObjectOfType<MiniGameBootstrap>();
            host.StartCampaign();
            host.Screen.DialogueCharactersPerSecond = 0;
            Assert.That(host.Runtime.GetView().Game.Day, Is.EqualTo(0));
            yield return Until(host, () => host.Screen.DayZeroOpeningWaiting, "supervisor opening");
            Assert.That(host.Screen.RequestDayZeroOpening(), Is.True);
            yield return Until(host, () => Find(host, "Continue") != null
                && host.Runtime.GetView().Transcript.Any(line => line.SourceNodeId == "tutorial__P00_D0_R1_COMMON_001"), "first native tutorial line");

            Assert.That(ActiveButton(host, "HistoryTab"), Is.Null);
            var introKeys = host.Runtime.GetView().Transcript.Select(line => line.Key).ToArray();
            Assert.That(host.Screen.HandleTerminalKey(KeyCode.F3), Is.False);
            Assert.That(host.Screen.HistoryOpen, Is.False);
            yield return null;
            Assert.That(host.Runtime.GetView().Game.EncounterStage, Is.EqualTo(EncounterStage.Introduction));
            Assert.That(host.Runtime.GetView().Transcript.Select(line => line.Key), Is.EqualTo(introKeys));
            Assert.That(host.Runtime.IsDialogueLineComplete("tutorial__P00_D0_R1_COMMON_001"), Is.False,
                "Removed record browsing must not complete unfinished dialogue.");
            Assert.That(Find(host, "Disposition-Purify"), Is.Null,
                "Purification stays locked until the tutorial finishes.");
            Assert.That(host.Runtime.GetView().Game.RemainingPurificationQuota, Is.EqualTo(1));

            yield return AdvanceUntil(host, () => host.Runtime.GetView().Game.EncounterStage == EncounterStage.AwaitingDisposition,
                "all five tutorial lines complete");
            var ready = host.Runtime.GetView();
            Assert.That(ready.Transcript.Count(line => !line.IsPlayer), Is.EqualTo(5));
            Assert.That(ready.Transcript.Where(line => !line.IsPlayer).All(line => line.Completed), Is.True);
            Assert.That(ready.Game.AvailableDispositions, Is.EqualTo(new[] { Disposition.Purify }));
            Assert.That(host.Screen.HistoryOpen, Is.False);
            var purify = Find(host, "Disposition-Purify");
            Assert.That(purify, Is.Not.Null, "Completed dialogue must allow the unique purification action.");
            Assert.That(ActiveButton(host, "Disposition-Treat"), Is.Null);
            Assert.That(ActiveButton(host, "Disposition-Release"), Is.Null);
            yield return PrototypePlayModeTests.CaptureIfRequested(host, "history-day0-purify-ready");

            Assert.That(host.Runtime.SetPaused(true).Accepted, Is.True);
            Assert.That(purify.IsInteractable(), Is.False, "Pause still locks disposition controls.");
            Assert.That(host.Runtime.GetView().Game.RemainingPurificationQuota, Is.EqualTo(1));
            Assert.That(host.Runtime.SetPaused(false).Accepted, Is.True);
            Assert.That(host.Screen.HistoryOpen, Is.False);
            Assert.That(purify.IsInteractable(), Is.True);
            var repeatedClick = purify.onClick;
            yield return Click(purify);
            repeatedClick.Invoke();
            Assert.That(host.Screen.HistoryOpen, Is.False);
            Assert.That(host.Runtime.GetView().Transcript.Count(line => line.SourceNodeId == "disposition:Purify"), Is.EqualTo(1));
            yield return Until(host, () => Find(host, "Continue") != null
                && host.Runtime.GetView().Transcript.Any(line => line.SourceNodeId == "tutorial__P00_D0_R1_ACTION_001"), "first native purification reaction");
            yield return PrototypePlayModeTests.CaptureIfRequested(host, "history-day0-purify-reaction");
            yield return AdvanceUntil(host, () => host.Runtime.GetView().TerminalPage == TerminalPage.Archive,
                "both native reactions finish before signal loss and archive");

            var archived = host.Runtime.GetView();
            Assert.That(archived.PresentedPatientId, Is.EqualTo("tutorial"));
            Assert.That(archived.PresentedDisposition, Is.EqualTo(Disposition.Purify));
            Assert.That(archived.Transcript.Where(line => !line.IsPlayer).Select(line => line.SourceNodeId), Is.EqualTo(new[]
            {
                "tutorial__P00_D0_R1_COMMON_001", "tutorial__P00_D0_R1_COMMON_002", "tutorial__P00_D0_R1_COMMON_003",
                "tutorial__P00_D0_R1_COMMON_004", "tutorial__P00_D0_R1_COMMON_005",
                "tutorial__P00_D0_R1_ACTION_001", "tutorial__P00_D0_R1_ACTION_002"
            }));
            Assert.That(archived.Transcript.Where(line => !line.IsPlayer).All(line => line.Completed), Is.True);
            Assert.That(archived.Transcript.Count(line => line.SourceNodeId == "disposition:Purify"), Is.EqualTo(1));
            Assert.That(archived.Game.RemainingPurificationQuota, Is.Zero);
            var saved = JsonUtility.FromJson<SaveEnvelope>(host.Runtime.Save());
            var snapshot = SnapshotCodec.Deserialize(saved.CoreJson, MiniGameSessionHost.Current.Rules).CreateSnapshot();
            Assert.That(snapshot.State.Patients.Single(patient => patient.Id == "tutorial").Trust, Is.Zero);
            Assert.That(snapshot.State.Days.Single(day => day.Day == 0).Purifications, Is.EqualTo(1));
            Assert.That(snapshot.CommandHistory.Count(command => command.Kind == CommandKind.SubmitDisposition), Is.EqualTo(1));
            yield return PrototypePlayModeTests.CaptureIfRequested(host, "history-day0-purify-archive");
            Debug.Log("MINIGAME_HISTORY_DISPOSITION_PASSED day=0; passive transcript; F3 removed; purification waits for dialogue; pause locks; two native reactions; one disposition; trust=0");
        }

        private static Button ActiveButton(MiniGameBootstrap host, string name) => host.Screen.GetComponentsInChildren<Button>(false)
            .FirstOrDefault(button => button.name == name && button.IsActive());

        private static Button Find(MiniGameBootstrap host, string name)
        {
            var button = ActiveButton(host, name);
            return button != null && button.IsInteractable() ? button : null;
        }

        private static IEnumerator Click(Button button)
        {
            yield return null;
            Assert.That(button, Is.Not.Null);
            Canvas.ForceUpdateCanvases();
            var rectangle = (RectTransform)button.transform;
            var pointer = new PointerEventData(EventSystem.current)
            {
                button = PointerEventData.InputButton.Left, clickCount = 1,
                position = button.GetComponentInParent<MiniGameScreen>().TerminalDisplay.ScreenPointFor(rectangle)
            };
            var hits = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointer, hits);
            Assert.That(hits, Is.Not.Empty, "No pointer hit for " + button.name);
            Assert.That(hits[0].gameObject.GetComponentInParent<Button>(), Is.EqualTo(button), "An overlay blocked " + button.name);
            ExecuteEvents.ExecuteHierarchy(hits[0].gameObject, pointer, ExecuteEvents.pointerDownHandler);
            ExecuteEvents.ExecuteHierarchy(hits[0].gameObject, pointer, ExecuteEvents.pointerUpHandler);
            ExecuteEvents.ExecuteHierarchy(hits[0].gameObject, pointer, ExecuteEvents.pointerClickHandler);
        }

        private static IEnumerator AdvanceUntil(MiniGameBootstrap host, Func<bool> condition, string expected)
        {
            float deadline = Time.realtimeSinceStartup + 20;
            while (!condition() && Time.realtimeSinceStartup < deadline)
            {
                var next = Find(host, "Continue");
                if (next != null) yield return Click(next);
                yield return null;
            }
            Assert.That(condition(), Is.True, expected + ": " + Describe(host));
        }

        private static IEnumerator Until(MiniGameBootstrap host, Func<bool> condition, string expected)
        {
            float deadline = Time.realtimeSinceStartup + 15;
            while (!condition() && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.That(condition(), Is.True, expected + ": " + Describe(host));
        }

        private static string Describe(MiniGameBootstrap host)
        {
            var view = host.Runtime.GetView();
            return "day=" + view.Game.Day + " stage=" + view.Game.EncounterStage + " input=" + view.Input
                + " cue=" + view.Cue?.Kind + " page=" + view.TerminalPage + " history=" + host.Screen.HistoryOpen
                + " yarn=" + host.GetComponent<MiniGameYarnBridge>().Diagnostics;
        }
    }
}
