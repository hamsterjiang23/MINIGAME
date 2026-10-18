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
    public sealed class CampaignPlayModeTests
    {
        [Serializable] private sealed class SaveEnvelope { public string CoreJson = ""; }

        [UnityTest, Timeout(60000)]
        public IEnumerator TutorialResumesTheCurrentPageWithoutReplayingCompletedLinesAndPurifiesOnce()
        {
            yield return SceneManager.LoadSceneAsync("InspectionApartment");
            yield return null;
            var host = Object.FindObjectOfType<MiniGameBootstrap>();
            host.StartCampaign();
            host.Screen.DialogueCharactersPerSecond = 0;
            Assert.That(host.CampaignActive, Is.True);
            Assert.That(host.SavePath, Does.EndWith("campaign-save.json"));
            Assert.That(host.Runtime.TryInteract(InteractionTarget.Computer).Accepted, Is.True);
            yield return Until(host, () => Find(host, "EnterReview") != null, "tutorial directory");
            yield return Click(Find(host, "EnterReview"));
            yield return Until(host, () => host.Runtime.GetView().Transcript.Any(line => line.SourceNodeId == "tutorial__D0_T1"), "first tutorial line");
            host.Screen.DialogueCharactersPerSecond = 30;
            yield return Click(Find(host, "Continue"));
            yield return Until(host, () => host.Runtime.GetView().Transcript.Any(line => line.SourceNodeId == "tutorial__D0_T2"), "long second tutorial line");
            yield return new WaitForSecondsRealtime(.08f);
            Assert.That(host.Screen.LineIsRevealed, Is.False, "Save while the source line is still being revealed.");
            Assert.That(host.Screen.GetComponentsInChildren<Text>(true).Single(text => text.name == "TerminalDialogueBody").text, Is.Not.Empty);
            Assert.That(host.Runtime.IsDialogueLineComplete("tutorial__D0_T1"), Is.True);
            Assert.That(host.Runtime.IsDialogueLineComplete("tutorial__D0_T2"), Is.False);
            Assert.That(host.Runtime.SetPaused(true).Accepted, Is.True);
            string checkpoint = host.Runtime.Save();
            host.RestoreCheckpoint(checkpoint);
            Assert.That(host.Runtime.GetView().Paused, Is.True);
            Assert.That(host.Runtime.IsDialogueLineComplete("tutorial__D0_T1"), Is.True);
            Assert.That(host.Runtime.IsDialogueLineComplete("tutorial__D0_T2"), Is.False);
            Assert.That(host.Runtime.GetView().Transcript.Count(line => line.SourceNodeId == "tutorial__D0_T2"), Is.EqualTo(1));
            host.Screen.DialogueCharactersPerSecond = 0;
            Assert.That(host.Runtime.SetPaused(false).Accepted, Is.True);
            yield return Until(host, () => Find(host, "Continue") != null && host.Screen.CurrentLinePageCount >= 1, "restored source line starts at page one");
            var restoredBody = host.Screen.GetComponentsInChildren<Text>(true).Single(text => text.name == "TerminalDialogueBody");
            var currentLine = host.Runtime.GetView().Transcript.Single(line => line.SourceNodeId == "tutorial__D0_T2");
            Assert.That(restoredBody.text, Is.Not.Empty);
            Assert.That(currentLine.Text.StartsWith(restoredBody.text, StringComparison.Ordinal), Is.True);
            var pageIndicator = host.Screen.GetComponentsInChildren<Text>(true).Single(label => label.name == "SpeechPage");
            Assert.That(pageIndicator.text, Is.EqualTo(host.Screen.CurrentLinePageCount > 1 ? "1/" + host.Screen.CurrentLinePageCount : ""));
            yield return PrototypePlayModeTests.CaptureIfRequested(host, "campaign-tutorial-restored-page");
            float deadline = Time.realtimeSinceStartup + 20;
            while (host.Runtime.GetView().Game.EncounterStage != EncounterStage.AwaitingDisposition && Time.realtimeSinceStartup < deadline)
            { var next = Find(host, "Continue"); if (next != null) yield return Click(next); yield return null; }
            var view = host.Runtime.GetView();
            Assert.That(view.Game.EncounterStage, Is.EqualTo(EncounterStage.AwaitingDisposition), Describe(host));
            Assert.That(view.Transcript.Count(line => !line.IsPlayer), Is.EqualTo(5));
            Assert.That(view.Transcript.Where(line => !line.IsPlayer).All(line => line.Completed), Is.True);
            Assert.That(view.Game.AvailableDispositions, Is.EqualTo(new[] { Disposition.Purify }));
            Assert.That(Find(host, "Disposition-Treat"), Is.Null);
            Assert.That(Find(host, "Disposition-Release"), Is.Null);
            var purify = Find(host, "Disposition-Purify");
            Assert.That(purify, Is.Not.Null);
            var staleClick = purify.onClick;
            yield return Click(purify);
            staleClick.Invoke();
            deadline = Time.realtimeSinceStartup + 20;
            while (host.Runtime.GetView().TerminalPage != TerminalPage.Archive && Time.realtimeSinceStartup < deadline)
            { var next = Find(host, "Continue"); if (next != null) yield return Click(next); yield return null; }
            view = host.Runtime.GetView();
            Assert.That(view.TerminalPage, Is.EqualTo(TerminalPage.Archive), Describe(host));
            Assert.That(view.Transcript.Count(line => !line.IsPlayer), Is.EqualTo(7));
            Assert.That(view.Transcript.Count(line => line.SourceNodeId == "disposition:Purify"), Is.EqualTo(1));
            Assert.That(view.Transcript.Select(line => line.SourceNodeId).Distinct().Count(), Is.EqualTo(view.Transcript.Count));
            var package = Campaign();
            var saved = JsonUtility.FromJson<SaveEnvelope>(host.Runtime.Save());
            var core = SnapshotCodec.Deserialize(saved.CoreJson, package.Rules).CreateSnapshot().State;
            Assert.That(core.Patients.Single(patient => patient.Id == "tutorial").Trust, Is.EqualTo(-1));
            Assert.That(view.Game.RemainingPurificationQuota, Is.EqualTo(0));
            Assert.That(view.PresentedPatientId, Is.EqualTo("tutorial"));
            Debug.Log("MINIGAME_CAMPAIGN_TUTORIAL_PASSED native Yarn; five intro and two reaction lines; page checkpoint; one -1 purification");
        }

        [UnityTest, Timeout(240000)]
        public IEnumerator NativeCampaignNormalRouteShowsSixActorsAndAllDaysThenTakenAway()
        { yield return RunCampaign(EndingType.TakenAway); }

        [UnityTest, Timeout(240000)]
        public IEnumerator NativeCampaignAllTreatmentRouteVisitsEveryMeetingThenLocksTheRoom()
        { yield return RunCampaign(EndingType.LockedInRoom); }

        [UnityTest, Timeout(120000)]
        public IEnumerator NativeCampaignReleasedPriestAttacksOnTheFollowingMorning()
        { yield return RunCampaign(EndingType.PriestAttack); }

        private static IEnumerator RunCampaign(EndingType expected)
        {
            yield return SceneManager.LoadSceneAsync("MainRoomCombined");
            yield return null;
            var host = Object.FindObjectOfType<MiniGameBootstrap>();
            Assert.That(host.CampaignActive, Is.True, "The combined room must default to the imported full campaign.");
            host.Screen.DialogueCharactersPerSecond = 0;
            var package = Campaign();
            var catalog = Resources.Load<PortraitCatalog>("MiniGame/PortraitCatalog");
            Assert.That(catalog, Is.Not.Null);
            foreach (var portrait in package.Package.Portraits)
                Assert.That(catalog.Find(portrait.Id), Is.Not.Null, "Missing imported sprite " + portrait.Id);
            var visited = new HashSet<string>();
            var photographed = new HashSet<string>();
            var archived = new HashSet<string>();
            var sourceLines = new HashSet<string>();
            bool capturedNarration = false;
            string pendingActor = null;
            string pendingName = null;
            float deadline = Time.realtimeSinceStartup + (expected == EndingType.PriestAttack ? 90 : 210);
            while (host.Runtime.GetView().Game.Phase != GamePhase.Ending && Time.realtimeSinceStartup < deadline)
            {
                var view = host.Runtime.GetView();
                foreach (var line in view.Transcript.Where(line => !line.IsPlayer)) sourceLines.Add(line.SourceNodeId);
                if (view.TerminalPage == TerminalPage.Review && view.PresentedPatientId != null)
                {
                    visited.Add(view.Game.Day + ":" + view.PresentedPatientId);
                    pendingActor = view.PresentedPatientId;
                    pendingName = view.PresentedPatientName;
                    var latest = view.Transcript.LastOrDefault(line => !line.IsPlayer && !line.IsNarration && line.PortraitId.Length > 0);
                    if (latest != null && photographed.Add(view.PresentedPatientId))
                    {
                        var portrait = host.Screen.GetComponentsInChildren<Image>(true).Single(image => image.name == "CurrentPortrait");
                        Assert.That(portrait.enabled && portrait.gameObject.activeInHierarchy, Is.True);
                        Assert.That(portrait.sprite, Is.EqualTo(catalog.Find(latest.PortraitId)));
                        yield return new WaitForSecondsRealtime(.14f);
                        if (expected == EndingType.TakenAway)
                            yield return PrototypePlayModeTests.CaptureIfRequested(host, "campaign-actor-" + view.PresentedPatientId);
                    }
                    var narration = host.Screen.GetComponentsInChildren<Text>(false).FirstOrDefault(text => text.name == "ConclusionBody" && text.gameObject.activeInHierarchy);
                    if (!capturedNarration && narration != null && narration.text.Length > 0)
                    {
                        capturedNarration = true;
                        Assert.That(view.Transcript.Last().IsNarration, Is.True);
                        Assert.That(host.Screen.GetComponentsInChildren<Text>(true).Single(text => text.name == "TerminalDialogueBody").text, Is.Not.Empty);
                        yield return PrototypePlayModeTests.CaptureIfRequested(host, "campaign-conclusion-" + expected);
                    }
                }
                if (view.TerminalPage == TerminalPage.Archive && !view.ArchiveDirectoryVisible)
                {
                    Assert.That(view.PresentedPatientId, Is.EqualTo(pendingActor), "Core may advance, but the archive must retain the last patient.");
                    Assert.That(view.PresentedPatientName, Is.EqualTo(pendingName));
                    Assert.That(host.Screen.GetComponentsInChildren<Text>(true).Single(text => text.name == "IndividualRecord").text, Is.EqualTo(pendingName));
                    Assert.That(view.Transcript.Count(line => line.SourceNodeId.StartsWith("disposition:", StringComparison.Ordinal)), Is.EqualTo(1));
                    archived.Add(view.Game.Day + ":" + pendingActor);
                }
                yield return Step(host, package, expected);
                yield return null;
            }
            var ending = host.Runtime.GetView();
            Assert.That(ending.Game.Phase, Is.EqualTo(GamePhase.Ending), Describe(host));
            Assert.That(ending.Game.Ending, Is.EqualTo(expected));
            Assert.That(ending.Game.Day, Is.EqualTo(expected == EndingType.PriestAttack ? 2 : 4));
            Assert.That(photographed, Is.EquivalentTo(new[] { "tutorial", "child", "former-ruler", "battlefield-doctor", "diplomat", "priest" }));
            Assert.That(visited.Count, Is.EqualTo(expected == EndingType.PriestAttack ? 6 : expected == EndingType.TakenAway ? 13 : 16));
            Assert.That(archived.Count, Is.EqualTo(visited.Count));
            Assert.That(sourceLines.Count, Is.GreaterThan(expected == EndingType.PriestAttack ? 100 : 300));
            if (expected != EndingType.PriestAttack) Assert.That(capturedNarration, Is.True);
            yield return Until(host, () => Find(host, "Continue") != null, "ending line is presented by Yarn");
            yield return PrototypePlayModeTests.CaptureIfRequested(host, "campaign-ending-" + expected);
            Debug.Log("MINIGAME_CAMPAIGN_ROUTE_PASSED ending=" + expected + " meetings=" + visited.Count + " sourceLines=" + sourceLines.Count + " actors=" + photographed.Count);
        }

        private static IEnumerator Step(MiniGameBootstrap host, CompiledDialoguePackage package, EndingType ending)
        {
            var view = host.Runtime.GetView();
            if (host.Screen.TransitionLocked) yield break;
            if (view.Input == InputContext.World)
            {
                Assert.That(view.AllowedInteractions, Is.Not.Empty, Describe(host));
                Assert.That(host.Runtime.TryInteract(view.AllowedInteractions.First()).Accepted, Is.True);
                yield break;
            }
            if (view.TerminalPage == TerminalPage.Archive)
            {
                var action = Find(host, view.ArchiveDirectoryVisible ? "LeaveTerminal" : "ReturnDirectory");
                if (action != null) yield return Click(action);
                yield break;
            }
            if (view.Input == InputContext.TerminalDirectory)
            {
                var enter = Find(host, "EnterReview");
                if (enter != null) yield return Click(enter);
                yield break;
            }
            if (view.Input == InputContext.Dialogue && view.Game.EncounterStage == EncounterStage.AwaitingDisposition)
            {
                Disposition disposition = Disposition.Treat;
                if (view.Game.Day == 0) disposition = Disposition.Purify;
                else if (ending == EndingType.PriestAttack && view.Game.Day == 1 && view.Game.CurrentPatientId == "priest") disposition = Disposition.Release;
                else if (ending == EndingType.TakenAway && (view.Game.Day == 1 && view.Game.CurrentPatientId == "child"
                    || view.Game.Day == 2 && view.Game.CurrentPatientId == "former-ruler"
                    || view.Game.Day == 3 && view.Game.CurrentPatientId == "battlefield-doctor")) disposition = Disposition.Purify;
                var decision = Find(host, "Disposition-" + disposition);
                Assert.That(decision, Is.Not.Null, disposition + ": " + Describe(host));
                yield return Click(decision);
                yield break;
            }
            var next = Find(host, "Continue");
            if (next != null) { yield return Click(next); yield break; }
            var options = host.Screen.GetComponentsInChildren<Button>(false).Where(button => button.name.StartsWith("Reply-", StringComparison.Ordinal) && button.IsInteractable()).ToArray();
            if (options.Length == 0) yield break;
            var meeting = package.Rules.Days.Single(day => day.Day == view.Game.Day).Meetings.Single(item => item.PatientId == view.Game.CurrentPatientId);
            var round = meeting.NormalRounds.Single(item => item.NodeId == view.Game.CurrentNodeId);
            var preferred = round.Responses.OrderByDescending(response => response.TrustDelta).First().Id;
            yield return Click(options.Single(button => button.name == "Reply-" + preferred));
        }

        private static CompiledDialoguePackage Campaign()
        {
            var asset = Resources.Load<DialoguePackageAsset>("MiniGame/ActiveCampaignPackage");
            Assert.That(asset, Is.Not.Null);
            return DialoguePackageCompiler.Compile(asset.PackageSource.text);
        }

        private static Button Find(MiniGameBootstrap host, string name) => host.Screen.GetComponentsInChildren<Button>(false)
            .FirstOrDefault(button => button.name == name && button.IsActive() && button.IsInteractable());

        private static IEnumerator Click(Button button)
        {
            yield return null; // Newly enabled graphics acquire a raycast depth on their first render frame.
            Assert.That(button, Is.Not.Null);
            Canvas.ForceUpdateCanvases();
            var rectangle = (RectTransform)button.transform;
            var canvas = button.GetComponentInParent<Canvas>();
            var pointer = new PointerEventData(EventSystem.current)
            {
                button = PointerEventData.InputButton.Left, clickCount = 1,
                position = RectTransformUtility.WorldToScreenPoint(canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
                    rectangle.TransformPoint(rectangle.rect.center))
            };
            var hits = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointer, hits);
            if (hits.Count == 0 || hits[0].gameObject.GetComponentInParent<Button>() != button)
                Debug.Log("CAMPAIGN_CLICK_DIAGNOSTIC " + button.name + " position=" + pointer.position + " rect=" + rectangle.rect
                    + " mode=" + canvas.renderMode + " depth=" + button.GetComponent<Image>().depth
                    + " cull=" + button.GetComponent<Image>().canvasRenderer.cull
                    + " hits=" + string.Join(";", hits.Select(hit => hit.gameObject.name + ":" + hit.depth)));
            Assert.That(hits, Is.Not.Empty, "No raycast hit for " + button.name);
            Assert.That(hits[0].gameObject.GetComponentInParent<Button>(), Is.EqualTo(button), "An overlay swallowed " + button.name);
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
            return "day=" + view.Game.Day + " phase=" + view.Game.Phase + " actor=" + view.PresentedPatientId + " node=" + view.Game.CurrentNodeId
                + " input=" + view.Input + " cue=" + view.Cue?.Kind + " page=" + view.TerminalPage + " yarn=" + host.GetComponent<MiniGameYarnBridge>().Diagnostics;
        }
    }
}
