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
    public sealed class FreePatientSelectionPlayModeTests
    {
        [UnityTest, Timeout(90000)]
        public IEnumerator DailyRosterCanReceivePriestThenDoctorAndRestoreTheChosenConversation()
        {
            yield return SceneManager.LoadSceneAsync("MainRoomCombined");
            yield return null;
            var host = Object.FindObjectOfType<MiniGameBootstrap>();
            Assert.That(host.CampaignActive, Is.True);
            host.RestoreCheckpoint(DayOneDirectory());
            host.Screen.DialogueCharactersPerSecond = 0;
            yield return Until(host, () => Find(host, "Roster-4") != null && !host.Screen.TransitionLocked,
                "day one directory is interactive");
            var available = host.Runtime.GetView();
            Assert.That(available.Game.Day, Is.EqualTo(1));
            Assert.That(available.Patients.Where(patient => patient.CanReceiveToday).Select(patient => patient.Id),
                Is.EquivalentTo(new[] { "child", "former-ruler", "battlefield-doctor", "diplomat", "priest" }));
            Assert.That(available.Game.CurrentPatientId, Is.EqualTo("child"), "The suggested first row must not lock the roster order.");

            yield return Click(Find(host, "Roster-4"), 2);
            yield return Until(host, () => HasPortraitLine(host, "priest"), "the selected priest starts native Yarn");
            AssertActor(host, "priest", "JN");
            Assert.That(host.Runtime.EnterSelectedPatient("child").Accepted, Is.False,
                "Selecting freely must not abandon a conversation already in progress.");
            yield return PrototypePlayModeTests.CaptureIfRequested(host, "free-roster-priest-first");

            Assert.That(host.Runtime.SetPaused(true).Accepted, Is.True);
            string encounterId = host.Runtime.GetView().Game.CurrentEncounterId;
            string[] recordedKeys = host.Runtime.GetView().Transcript.Select(line => line.Key).ToArray();
            host.RestoreCheckpoint(host.Runtime.Save());
            Assert.That(host.Runtime.GetView().Paused, Is.True);
            Assert.That(host.Runtime.GetView().Game.CurrentEncounterId, Is.EqualTo(encounterId));
            Assert.That(host.Runtime.GetView().Transcript.Select(line => line.Key), Is.EqualTo(recordedKeys));
            Assert.That(host.Runtime.SetPaused(false).Accepted, Is.True);
            host.Screen.DialogueCharactersPerSecond = 0;
            yield return Until(host, () => Find(host, "Continue") != null || Reply(host) != null,
                "the chosen conversation resumes");
            AssertActor(host, "priest", "JN");
            Assert.That(host.Runtime.EnterSelectedPatient("battlefield-doctor").Accepted, Is.False);

            yield return CompleteWithTreatment(host);
            var archived = host.Runtime.GetView();
            Assert.That(archived.PresentedPatientId, Is.EqualTo("priest"), "The archive belongs to the actor just received.");
            Assert.That(archived.Transcript.Count(line => line.SourceNodeId == "disposition:Treat"), Is.EqualTo(1));
            Assert.That(archived.Transcript.Select(line => line.Key).Distinct().Count(), Is.EqualTo(archived.Transcript.Count));
            Assert.That(archived.Patients.Single(patient => patient.Id == "priest").TreatmentCount, Is.EqualTo(1));
            yield return Click(Find(host, "ReturnDirectory"));
            yield return Until(host, () => Find(host, "Roster-4") != null, "remaining daily patients appear");
            Assert.That(host.Runtime.GetView().Patients.Where(patient => patient.CanReceiveToday).Select(patient => patient.Id),
                Is.EquivalentTo(new[] { "child", "former-ruler", "battlefield-doctor", "diplomat" }));

            yield return Click(Find(host, "Roster-4"), 2);
            Assert.That(host.Runtime.GetView().TerminalPage, Is.EqualTo(TerminalPage.Directory));
            Assert.That(Find(host, "EnterReview"), Is.Null, "A completed patient's enter button must be disabled.");
            Assert.That(host.Runtime.EnterSelectedPatient("priest").Accepted, Is.False);
            Assert.That(host.Runtime.GetView().Patients.Single(patient => patient.Id == "priest").TreatmentCount, Is.EqualTo(1));

            yield return Click(Find(host, "Roster-2"));
            Assert.That(host.Runtime.GetView().TerminalPage, Is.EqualTo(TerminalPage.Directory),
                "A single roster click previews the selected actor without starting a meeting.");
            Assert.That(Find(host, "EnterReview"), Is.Not.Null);
            yield return PrototypePlayModeTests.CaptureIfRequested(host, "free-roster-doctor-selected");
            yield return Click(Find(host, "EnterReview"));
            yield return Until(host, () => HasPortraitLine(host, "battlefield-doctor"), "doctor starts before the first two roster rows");
            AssertActor(host, "battlefield-doctor", "ED");
            Assert.That(host.Runtime.GetView().Game.Day, Is.EqualTo(1));
            Assert.That(host.Runtime.GetView().Transcript.All(line => !line.SourceNodeId.StartsWith("priest__", StringComparison.Ordinal)), Is.True,
                "The previous patient's transcript must not leak into the new actor's record.");
            yield return PrototypePlayModeTests.CaptureIfRequested(host, "free-roster-doctor-second");
            Debug.Log("MINIGAME_FREE_ROSTER_PASSED day=1 order=priest,battlefield-doctor; actual pointer; native Yarn; actor portraits; restored active meeting; one treatment; completed actor locked");
        }

        private static string DayOneDirectory()
        {
            var asset = Resources.Load<DialoguePackageAsset>("MiniGame/ActiveCampaignPackage");
            Assert.That(asset, Is.Not.Null);
            var package = DialoguePackageCompiler.Compile(asset.PackageSource.text);
            var runtime = new GameRuntime(package.Rules, package.Content, useTerminalDirectory: true);
            Assert.That(runtime.TryInteract(InteractionTarget.Computer).Accepted, Is.True);
            Complete(runtime, PresentationKind.EnterTerminal);
            Assert.That(runtime.EnterSelectedPatient("tutorial").Accepted, Is.True);
            Complete(runtime, PresentationKind.Introduction);
            Assert.That(runtime.SubmitDisposition(runtime.GetView().Game.CurrentEncounterId, Disposition.Purify).Accepted, Is.True);
            Complete(runtime, PresentationKind.Reaction);
            Complete(runtime, PresentationKind.ExitTerminal);
            Assert.That(runtime.ReturnToDirectory().Accepted, Is.True);
            Assert.That(runtime.LeaveTerminal().Accepted, Is.True);
            Complete(runtime, PresentationKind.ExitTerminal);
            Assert.That(runtime.TryInteract(InteractionTarget.Bed).Accepted, Is.True);
            Complete(runtime, PresentationKind.Sleep);
            Complete(runtime, PresentationKind.Dream);
            Assert.That(runtime.TryInteract(InteractionTarget.Bed).Accepted, Is.True);
            Assert.That(runtime.TryInteract(InteractionTarget.Door).Accepted, Is.True);
            Complete(runtime, PresentationKind.MorningInspection);
            Complete(runtime, PresentationKind.Briefing);
            Assert.That(runtime.TryInteract(InteractionTarget.Computer).Accepted, Is.True);
            Complete(runtime, PresentationKind.EnterTerminal);
            Assert.That(runtime.GetView().TerminalPage, Is.EqualTo(TerminalPage.Directory));
            return runtime.Save();
        }

        private static void Complete(GameRuntime runtime, PresentationKind kind)
        {
            var cue = runtime.GetView().Cue;
            Assert.That(cue, Is.Not.Null);
            Assert.That(cue.Kind, Is.EqualTo(kind));
            Assert.That(runtime.CompleteCue(cue.Token).Accepted, Is.True);
        }

        private static bool HasPortraitLine(MiniGameBootstrap host, string actor)
        {
            var view = host.Runtime.GetView();
            return view.TerminalPage == TerminalPage.Review && view.PresentedPatientId == actor
                && view.Transcript.Any(line => !line.IsPlayer && line.PortraitId.Length > 0);
        }

        private static void AssertActor(MiniGameBootstrap host, string actor, string portraitPrefix)
        {
            var view = host.Runtime.GetView();
            Assert.That(view.Game.CurrentPatientId, Is.EqualTo(actor));
            Assert.That(view.PresentedPatientId, Is.EqualTo(actor));
            var latest = view.Transcript.Last(line => !line.IsPlayer && line.PortraitId.Length > 0);
            Assert.That(latest.SourceNodeId, Does.StartWith(actor + "__"));
            Assert.That(latest.PortraitId, Does.StartWith(portraitPrefix));
            var portrait = host.Screen.GetComponentsInChildren<Image>(true).Single(image => image.name == "CurrentPortrait");
            Assert.That(portrait.enabled && portrait.gameObject.activeInHierarchy, Is.True);
            Assert.That(portrait.sprite, Is.EqualTo(Resources.Load<PortraitCatalog>("MiniGame/PortraitCatalog").Find(latest.PortraitId)));
            Assert.That(host.Screen.GetComponentsInChildren<Text>(true).Single(text => text.name == "IndividualRecord").text,
                Is.EqualTo(view.PresentedPatientName));
        }

        private static IEnumerator CompleteWithTreatment(MiniGameBootstrap host)
        {
            float deadline = Time.realtimeSinceStartup + 45;
            while (host.Runtime.GetView().TerminalPage != TerminalPage.Archive && Time.realtimeSinceStartup < deadline)
            {
                var view = host.Runtime.GetView();
                if (!host.Screen.TransitionLocked)
                {
                    var action = view.Input == InputContext.Dialogue && view.Game.EncounterStage == EncounterStage.AwaitingDisposition
                        ? Find(host, "Disposition-Treat") : Find(host, "Continue") ?? Reply(host);
                    if (action != null) yield return Click(action);
                }
                yield return null;
            }
            Assert.That(host.Runtime.GetView().TerminalPage, Is.EqualTo(TerminalPage.Archive), Describe(host));
        }

        private static Button Find(MiniGameBootstrap host, string name) => host.Screen.GetComponentsInChildren<Button>(false)
            .FirstOrDefault(button => button.name == name && button.IsActive() && button.IsInteractable());

        private static Button Reply(MiniGameBootstrap host) => host.Screen.GetComponentsInChildren<Button>(false)
            .FirstOrDefault(button => button.name.StartsWith("Reply-", StringComparison.Ordinal) && button.IsActive() && button.IsInteractable());

        private static IEnumerator Click(Button button, int clickCount = 1)
        {
            yield return null;
            Assert.That(button, Is.Not.Null);
            Canvas.ForceUpdateCanvases();
            var rectangle = (RectTransform)button.transform;
            var canvas = button.GetComponentInParent<Canvas>();
            var pointer = new PointerEventData(EventSystem.current)
            {
                button = PointerEventData.InputButton.Left, clickCount = clickCount,
                position = RectTransformUtility.WorldToScreenPoint(canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
                    rectangle.TransformPoint(rectangle.rect.center))
            };
            var hits = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointer, hits);
            Assert.That(hits, Is.Not.Empty, "No pointer hit for " + button.name);
            Assert.That(hits[0].gameObject.GetComponentInParent<Button>(), Is.EqualTo(button), "An overlay blocked " + button.name);
            ExecuteEvents.ExecuteHierarchy(hits[0].gameObject, pointer, ExecuteEvents.pointerDownHandler);
            ExecuteEvents.ExecuteHierarchy(hits[0].gameObject, pointer, ExecuteEvents.pointerUpHandler);
            ExecuteEvents.ExecuteHierarchy(hits[0].gameObject, pointer, ExecuteEvents.pointerClickHandler);
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
            return "day=" + view.Game.Day + " phase=" + view.Game.Phase + " actor=" + view.PresentedPatientId
                + " node=" + view.Game.CurrentNodeId + " input=" + view.Input + " cue=" + view.Cue?.Kind
                + " page=" + view.TerminalPage + " yarn=" + host.GetComponent<MiniGameYarnBridge>().Diagnostics;
        }
    }
}
