using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MiniGame.Core;
using MiniGame.Presentation.Authoring;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace MiniGame.Presentation.Tests
{
    public sealed class CampaignRuntimeTests
    {
        [Test]
        public void DayZeroOpeningUsesReviewCueAndRestoresWithoutDuplicatingSelectionOrDisposition()
        {
            var rules = Rules().WithDreamFlow(0, 100, 80, new Dictionary<string, int>());
            var content = new FixtureContent();
            var runtime = new GameRuntime(rules, content, true);
            Assert.That(runtime.BeginDayZeroOpening().Accepted, Is.True);
            var opening = runtime.GetView();
            Assert.That(opening.Game.Day, Is.Zero);
            Assert.That(opening.TerminalPage, Is.EqualTo(TerminalPage.Review));
            Assert.That(opening.Cue!.Kind, Is.EqualTo(PresentationKind.EnterTerminal));
            Assert.That(opening.Transcript, Is.Empty);
            string staleToken = opening.Cue.Token;
            Assert.That(runtime.BeginDayZeroOpening().Accepted, Is.False);
            runtime = GameRuntime.Restore(runtime.Save(), rules, content);
            Assert.That(runtime.CompleteCue(staleToken).Accepted, Is.False);
            RuntimeScenario.Complete(runtime, PresentationKind.EnterTerminal);
            Assert.That(runtime.GetView().Cue!.Kind, Is.EqualTo(PresentationKind.Introduction));
            RuntimeScenario.Complete(runtime, PresentationKind.Introduction);
            Assert.That(runtime.GetView().Game.AvailableDispositions, Is.EqualTo(new[] { Disposition.Purify }));
            string encounter = runtime.GetView().Game.CurrentEncounterId!;
            Assert.That(runtime.SubmitDisposition(encounter, Disposition.Purify).Accepted, Is.True);
            Assert.That(runtime.SubmitDisposition(encounter, Disposition.Purify).Accepted, Is.False);
            RuntimeScenario.Complete(runtime, PresentationKind.Reaction);
            RuntimeScenario.Complete(runtime, PresentationKind.ExitTerminal);
            RuntimeScenario.Complete(runtime, PresentationKind.ExitTerminal);
            Assert.That(runtime.GetView().TerminalOpen, Is.False);
            Assert.That(runtime.BeginNightDream().Accepted, Is.True);
            Assert.That(runtime.GetView().Game.Phase, Is.EqualTo(GamePhase.AfterDream));
            Assert.That(runtime.GetView().AllowedInteractions, Is.EqualTo(new[] { InteractionTarget.Bed }));
            Assert.That(runtime.TryInteract(InteractionTarget.Computer).Accepted, Is.False);
            Assert.That(runtime.ToggleRoomDoor().Accepted, Is.False);
            Assert.That(runtime.TryInteract(InteractionTarget.Bed).Accepted, Is.True);
            Assert.That(runtime.GetView().Game.Day, Is.Zero);
            runtime = GameRuntime.Restore(runtime.Save(), rules, content);
            RuntimeScenario.Complete(runtime, PresentationKind.Sleep);
            Assert.That(runtime.GetView().Game.Day, Is.EqualTo(1));
            Assert.That(runtime.GetView().Game.Phase, Is.EqualTo(GamePhase.MorningInspection));
        }

        [TestCase(EndingType.TakenAway, 3)]
        [TestCase(EndingType.LockedInRoom, 3)]
        [TestCase(EndingType.PriestAttack, 3)]
        [TestCase(EndingType.PriestAttack, 1)]
        public void ImportedCampaignPlaysRealSegmentsToAllEndingsIncludingEarliestPriestRelease(EndingType expected, int releaseDay)
        {
            var folder = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
            while (folder != null && !Directory.Exists(Path.Combine(folder.FullName, "UnityProject"))) folder = folder.Parent;
            Assert.That(folder, Is.Not.Null);
            var package = DialoguePackageCompiler.Compile(File.ReadAllText(Path.Combine(folder!.FullName,
                "UnityProject/Assets/MiniGame/Content/Campaign/campaign.minigame.json")));
            var runtime = new GameRuntime(package.Rules, package.Content, true);
            var visited = new HashSet<string>();
            var presentedLines = new HashSet<string>();
            var dayTwoDiplomatLines = new HashSet<string>();
            Assert.That(package.Content.TextVariants, Is.Empty, "The revised source no longer uses the old historical wording.");
            for (var step = 0; step < 1500 && runtime.GetView().Game.Phase != GamePhase.Ending; step++)
            {
                var view = runtime.GetView();
                if (view.TerminalPage == TerminalPage.Review && view.Game.CurrentNodeId != null
                    && (view.Input == InputContext.Dialogue || view.Cue?.Kind == PresentationKind.Introduction
                        || view.Cue?.Kind == PresentationKind.ResponseBranch || view.Cue?.Kind == PresentationKind.Reaction))
                {
                    visited.Add(view.Game.Day + ":" + view.Game.CurrentPatientId);
                    var segment = package.Content.GetSegment(view.Game.CurrentNodeId);
                    foreach (var line in segment.Lines)
                    {
                        var resolved = runtime.ResolveDialogueText(line.SourceNodeId, line.Text);
                        Assert.That(resolved, Is.EqualTo(line.Text), line.SourceNodeId);
                        if (view.Game.Day == 2 && view.Game.CurrentPatientId == "diplomat")
                        {
                            dayTwoDiplomatLines.Add(line.SourceNodeId);
                        }
                        runtime.RecordDialogueLine(line.Speaker, resolved, line.SourceNodeId, line.PortraitId, line.IsNarration);
                        runtime.CompleteDialogueLine(line.SourceNodeId);
                        Assert.That(runtime.IsDialogueLineComplete(line.SourceNodeId), Is.True);
                        presentedLines.Add(line.SourceNodeId);
                    }
                }
                if (view.Cue != null)
                {
                    if (view.Cue.Kind == PresentationKind.Reaction || view.Cue.Kind == PresentationKind.Introduction)
                    {
                        var stale = view.Cue.Token;
                        runtime = GameRuntime.Restore(runtime.Save(), package.Rules, package.Content);
                        RuntimeScenario.RejectWithoutMutation(runtime, () => runtime.CompleteCue(stale));
                    }
                    RuntimeScenario.Accept(runtime.CompleteCue(runtime.GetView().Cue!.Token));
                }
                else if (view.TerminalPage == TerminalPage.Archive)
                {
                    if (view.ArchiveDirectoryVisible) RuntimeScenario.Accept(runtime.LeaveTerminal());
                    else RuntimeScenario.Accept(runtime.ReturnToDirectory());
                }
                else if (view.TerminalPage == TerminalPage.Directory)
                    RuntimeScenario.Accept(runtime.EnterSelectedPatient(view.Game.CurrentPatientId!));
                else if (view.Game.EncounterStage == EncounterStage.AwaitingDisposition)
                {
                    var disposition = Disposition.Treat;
                    if (view.Game.Day == 0) disposition = Disposition.Purify;
                    else if (expected == EndingType.TakenAway) disposition = RuntimeScenario.NormalPolicy(view.Game.Day, view.Game.CurrentPatientId!);
                    else if (expected == EndingType.PriestAttack && view.Game.Day == releaseDay && view.Game.CurrentPatientId == "priest") disposition = Disposition.Release;
                    RuntimeScenario.Accept(runtime.SubmitDisposition(view.Game.CurrentEncounterId!, disposition));
                }
                else if (view.TerminalOpen)
                {
                    var meeting = package.Rules.Days.Single(day => day.Day == view.Game.Day).Meetings.Single(item => item.PatientId == view.Game.CurrentPatientId);
                    var round = view.Game.EncounterStage == EncounterStage.SpecialDialogue
                        ? meeting.SpecialRound!
                        : meeting.NormalRounds.Single(item => item.NodeId == view.Game.CurrentNodeId);
                    var response = round.Responses.OrderByDescending(item => item.TrustDelta).First();
                    RuntimeScenario.Accept(runtime.SelectResponse(view.Game.CurrentEncounterId!, view.Game.CurrentNodeId!, response.Id));
                }
                else if (view.Game.Phase == GamePhase.MorningInspection) RuntimeScenario.Accept(runtime.TryInteract(InteractionTarget.Door));
                else RuntimeScenario.Accept(runtime.TryInteract(view.Game.Phase == GamePhase.Apartment ? InteractionTarget.Computer : InteractionTarget.Bed));
            }
            Assert.That(runtime.GetView().Game.Phase, Is.EqualTo(GamePhase.Ending));
            var earlyAttack = expected == EndingType.PriestAttack && releaseDay == 1;
            Assert.That(runtime.GetView().Game.Day, Is.EqualTo(earlyAttack ? 2 : 4));
            Assert.That(runtime.GetView().Game.Ending, Is.EqualTo(expected));
            Assert.That(presentedLines.Count, Is.GreaterThan(earlyAttack ? 100 : 300));
            Assert.That(dayTwoDiplomatLines.Count, earlyAttack ? Is.Zero : Is.GreaterThan(0));
            Assert.That(visited, Does.Contain("0:tutorial").And.Contain("1:child").And.Contain("1:former-ruler")
                .And.Contain("1:battlefield-doctor").And.Contain("1:diplomat").And.Contain("1:priest"));
            if (earlyAttack) Assert.That(visited.Count, Is.EqualTo(6));
            else
            {
                Assert.That(visited, Does.Contain("3:priest"));
                if (expected != EndingType.TakenAway) Assert.That(visited.Count, Is.EqualTo(16));
            }
            Assert.That(GameRuntime.Restore(runtime.Save(), package.Rules, package.Content).Save(), Is.EqualTo(runtime.Save()));
        }

        [Test]
        public void FiveLineIntroductionResumesAtTheFirstIncompleteLineAndPurifiesOnlyOnce()
        {
            var rules = Rules(); var content = new FixtureContent();
            var runtime = OpenTutorial(rules, content);
            var oldToken = runtime.GetView().Cue!.Token;
            for (var line = 1; line <= 3; line++)
            {
                runtime.RecordDialogueLine("无名对象", "教学对白 " + line, "D0_LINE" + line, "D0_01");
                if (line < 3) runtime.CompleteDialogueLine("D0_LINE" + line);
            }
            var saved = runtime.Save();
            runtime = GameRuntime.Restore(saved, rules, content);
            Assert.That(runtime.Save(), Is.EqualTo(saved));
            Assert.That(JObject.Parse(saved)["Version"]!.Value<int>(), Is.EqualTo(2));
            Assert.That(runtime.IsDialogueLineComplete("D0_LINE1"), Is.True);
            Assert.That(runtime.IsDialogueLineComplete("D0_LINE2"), Is.True);
            Assert.That(runtime.IsDialogueLineComplete("D0_LINE3"), Is.False);
            Assert.That(runtime.IsDialogueLineComplete("D0_LINE4"), Is.False);
            RuntimeScenario.RejectWithoutMutation(runtime, () => runtime.CompleteCue(oldToken));
            // The host skips the first two completed lines, replays line three, then finishes the segment.
            for (var line = 1; line <= 5; line++)
            {
                var id = "D0_LINE" + line;
                if (runtime.IsDialogueLineComplete(id)) continue;
                runtime.RecordDialogueLine("无名对象", "教学对白 " + line, id, "D0_01");
                runtime.CompleteDialogueLine(id);
            }
            Assert.That(runtime.GetView().Transcript.Count, Is.EqualTo(5));
            Assert.That(runtime.GetView().Transcript.All(entry => entry.Completed), Is.True);
            RuntimeScenario.Complete(runtime, PresentationKind.Introduction);
            Assert.That(runtime.GetView().Game.AvailableDispositions, Is.EqualTo(new[] { Disposition.Purify }));
            var encounter = runtime.GetView().Game.CurrentEncounterId!;
            RuntimeScenario.Accept(runtime.SubmitDisposition(encounter, Disposition.Purify));
            runtime = GameRuntime.Restore(runtime.Save(), rules, content);
            RuntimeScenario.RejectWithoutMutation(runtime, () => runtime.SubmitDisposition(encounter, Disposition.Purify));
            RuntimeScenario.Complete(runtime, PresentationKind.Reaction);
            RuntimeScenario.Complete(runtime, PresentationKind.ExitTerminal);
            var core = CoreState(runtime);
            Assert.That(core["Days"]![0]!["Purifications"]!.Value<int>(), Is.EqualTo(1));
            Assert.That(core["Patients"]![0]!["Trust"]!.Value<int>(), Is.EqualTo(-1));
            Assert.That(runtime.GetView().PresentedPatientId, Is.EqualTo("tutorial"));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void TutorialEntryRestoresBeforeAnimationAndStartsIntroductionInBothTerminalModes(bool directory)
        {
            var rules = Rules(); var content = new FixtureContent();
            var runtime = new GameRuntime(rules, content, directory);
            RuntimeScenario.Accept(runtime.TryInteract(InteractionTarget.Computer));
            runtime = GameRuntime.Restore(runtime.Save(), rules, content);
            RuntimeScenario.Complete(runtime, PresentationKind.EnterTerminal);
            if (directory)
            {
                Assert.That(runtime.GetView().Cue, Is.Null);
                runtime = GameRuntime.Restore(runtime.Save(), rules, content);
                RuntimeScenario.Accept(runtime.EnterSelectedPatient("tutorial"));
            }
            Assert.That(runtime.GetView().Cue!.Kind, Is.EqualTo(PresentationKind.Introduction));
            Assert.That(runtime.GetView().Cue!.Text.Split('\n').Length, Is.EqualTo(5));
            RuntimeScenario.Complete(runtime, PresentationKind.Introduction);
            Assert.That(runtime.GetView().Input, Is.EqualTo(InputContext.Dialogue));
            Assert.That(runtime.GetView().Game.EncounterStage, Is.EqualTo(EncounterStage.AwaitingDisposition));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void ArchiveRetainsCompletedActorAcrossRestoreAndTheNextActorUsesTheSameTerminal(bool leaveBetween)
        {
            var rules = Rules(); var content = new FixtureContent();
            var runtime = OpenDayOne(rules, content);
            FinishFirstReaction(runtime);
            var previousId = runtime.GetView().PresentedEncounterId;
            Assert.That(runtime.GetView().Game.CurrentPatientId, Is.EqualTo("second"));
            Assert.That(runtime.GetView().PresentedPatientId, Is.EqualTo("first"));
            Assert.That(runtime.GetView().PresentedPatientName, Is.EqualTo("第一位"));
            Assert.That(runtime.GetView().PresentedDisposition, Is.EqualTo(Disposition.Treat));
            var beforeLateLine = runtime.Save();
            runtime.RecordDialogueLine("第一位", "过期回调", "stale.reaction", "FIRST01");
            runtime.CompleteDialogueLine("stale.reaction");
            Assert.That(runtime.Save(), Is.EqualTo(beforeLateLine));
            runtime = GameRuntime.Restore(runtime.Save(), rules, content);
            RuntimeScenario.Complete(runtime, PresentationKind.ExitTerminal);
            runtime = GameRuntime.Restore(runtime.Save(), rules, content);
            Assert.That(runtime.GetView().TerminalPage, Is.EqualTo(TerminalPage.Archive));
            Assert.That(runtime.GetView().PresentedEncounterId, Is.EqualTo(previousId));
            Assert.That(runtime.GetView().Transcript.All(entry => entry.EncounterId == previousId), Is.True);
            RuntimeScenario.Accept(runtime.ReturnToDirectory());
            Assert.That(runtime.GetView().TerminalPage, Is.EqualTo(TerminalPage.Directory));
            Assert.That(runtime.GetView().TerminalOpen, Is.True);
            runtime = GameRuntime.Restore(runtime.Save(), rules, content);
            if (leaveBetween)
            {
                RuntimeScenario.Accept(runtime.LeaveTerminal());
                runtime = GameRuntime.Restore(runtime.Save(), rules, content);
                RuntimeScenario.Complete(runtime, PresentationKind.ExitTerminal);
                runtime = GameRuntime.Restore(runtime.Save(), rules, content);
                RuntimeScenario.Accept(runtime.TryInteract(InteractionTarget.Computer));
                RuntimeScenario.Complete(runtime, PresentationKind.EnterTerminal);
            }
            RuntimeScenario.RejectWithoutMutation(runtime, () => runtime.EnterSelectedPatient("first"));
            RuntimeScenario.Accept(runtime.EnterSelectedPatient("second"));
            Assert.That(runtime.GetView().PresentedPatientId, Is.EqualTo("second"));
            Assert.That(runtime.GetView().Transcript, Is.Empty);
            Assert.That(runtime.GetView().Cue!.Kind, Is.EqualTo(PresentationKind.Introduction));
            runtime = GameRuntime.Restore(runtime.Save(), rules, content);
            RuntimeScenario.Complete(runtime, PresentationKind.Introduction);
            Assert.That(runtime.GetView().Dialogue!.NodeId, Is.EqualTo("second.round"));
            Assert.That(runtime.GetView().TerminalOpen, Is.True);
        }

        [Test]
        public void CompletedLineFlagsCannotAdvanceDuringPauseDirectoryOrSignalLoss()
        {
            var rules = Rules(); var content = new FixtureContent();
            var runtime = new GameRuntime(rules, content, true);
            RuntimeScenario.Accept(runtime.TryInteract(InteractionTarget.Computer));
            RuntimeScenario.Complete(runtime, PresentationKind.EnterTerminal);
            runtime.RecordDialogueLine("无名对象", "未进入会谈", "D0_DIRECTORY", "D0_01");
            Assert.That(runtime.GetView().Transcript, Is.Empty);
            RuntimeScenario.Accept(runtime.EnterSelectedPatient("tutorial"));
            runtime.RecordDialogueLine("无名对象", "当前未完成", "D0_CURRENT", "D0_01");
            RuntimeScenario.Accept(runtime.SetPaused(true));
            var paused = runtime.Save();
            runtime.CompleteDialogueLine("D0_CURRENT");
            runtime.RecordDialogueLine("无名对象", "暂停中的回调", "D0_PAUSED", "D0_01");
            Assert.That(runtime.Save(), Is.EqualTo(paused));
            runtime = GameRuntime.Restore(paused, rules, content);
            RuntimeScenario.Accept(runtime.SetPaused(false));
            Assert.That(runtime.IsDialogueLineComplete("D0_CURRENT"), Is.False);
            runtime.CompleteDialogueLine("D0_CURRENT");
            Assert.That(runtime.IsDialogueLineComplete("D0_CURRENT"), Is.True);
        }

        [Test]
        public void NarrationIsRetainedSeparatelyFromTheLastActorLineAcrossReactionRestore()
        {
            var rules = Rules(); var content = new FixtureContent();
            var runtime = OpenDayOne(rules, content);
            Reply(runtime);
            RuntimeScenario.Accept(runtime.SubmitDisposition(runtime.GetView().Game.CurrentEncounterId!, Disposition.Treat));
            runtime.RecordDialogueLine("第一位", "我会配合。", "first.last-speech", "FIRST02");
            runtime.CompleteDialogueLine("first.last-speech");
            runtime.RecordDialogueLine("结案记录", "档案已结束。", "first.case-note", "", narration: true);
            runtime.CompleteDialogueLine("first.case-note");
            runtime = GameRuntime.Restore(runtime.Save(), rules, content);
            var transcript = runtime.GetView().Transcript;
            Assert.That(transcript.Last().IsNarration, Is.True);
            Assert.That(transcript.Last().Completed, Is.True);
            Assert.That(transcript.Last(entry => !entry.IsPlayer && !entry.IsNarration).Text, Is.EqualTo("我会配合。"));
            Assert.That(transcript.Last(entry => !entry.IsPlayer && !entry.IsNarration).PortraitId, Is.EqualTo("FIRST02"));
            RuntimeScenario.Complete(runtime, PresentationKind.Reaction);
            Assert.That(runtime.GetView().PresentedPatientId, Is.EqualTo("first"));
        }

        [Test]
        public void TutorialAndTwoFormalDaysCanRestoreEveryBoundaryWithoutDuplicatingSideEffects()
        {
            var rules = Rules(); var content = new FixtureContent();
            var runtime = new GameRuntime(rules, content, true);
            var reached = new HashSet<string>();
            for (var step = 0; step < 120 && runtime.GetView().Game.Phase != GamePhase.Ending; step++)
            {
                var saved = runtime.Save();
                var previousToken = runtime.GetView().Cue?.Token;
                runtime = GameRuntime.Restore(saved, rules, content);
                Assert.That(runtime.Save(), Is.EqualTo(saved));
                if (previousToken != null) RuntimeScenario.RejectWithoutMutation(runtime, () => runtime.CompleteCue(previousToken));
                var view = runtime.GetView();
                reached.Add(view.Game.Day + ":" + view.Game.CurrentPatientId);
                if (view.Cue != null) RuntimeScenario.Accept(runtime.CompleteCue(view.Cue.Token));
                else if (view.TerminalPage == TerminalPage.Archive) RuntimeScenario.Accept(runtime.ReturnToDirectory());
                else if (view.TerminalPage == TerminalPage.Directory) RuntimeScenario.Accept(runtime.EnterSelectedPatient(view.Game.CurrentPatientId!));
                else if (view.TerminalOpen && view.Game.EncounterStage == EncounterStage.AwaitingDisposition)
                    RuntimeScenario.Accept(runtime.SubmitDisposition(view.Game.CurrentEncounterId!, view.Game.Day == 0 ? Disposition.Purify : Disposition.Treat));
                else if (view.TerminalOpen) Reply(runtime);
                else if (view.Game.Phase == GamePhase.MorningInspection) RuntimeScenario.Accept(runtime.TryInteract(InteractionTarget.Door));
                else RuntimeScenario.Accept(runtime.TryInteract(view.Game.Phase == GamePhase.Apartment ? InteractionTarget.Computer : InteractionTarget.Bed));
                if (runtime.GetView().ArchiveDirectoryVisible) RuntimeScenario.Accept(runtime.LeaveTerminal());
            }
            Assert.That(runtime.GetView().Game.Phase, Is.EqualTo(GamePhase.Ending));
            Assert.That(runtime.GetView().Game.Ending, Is.EqualTo(EndingType.TakenAway));
            Assert.That(reached, Does.Contain("0:tutorial").And.Contain("1:first").And.Contain("1:second").And.Contain("2:first"));
            var core = CoreState(runtime);
            Assert.That(core["Encounters"]!.Count(), Is.EqualTo(4));
            Assert.That(core["Patients"]![0]!["Trust"]!.Value<int>(), Is.EqualTo(-1));
            Assert.That(core["Patients"]![1]!["TreatmentCount"]!.Value<int>(), Is.EqualTo(2));
            Assert.That(core["Patients"]![2]!["TreatmentCount"]!.Value<int>(), Is.EqualTo(1));
        }

        [Test]
        public void RestoreRejectsIntroductionOnDirectoryAndArchiveBoundToTheNextActor()
        {
            var rules = Rules(); var content = new FixtureContent();
            var runtime = OpenTutorial(rules, content);
            var json = JObject.Parse(runtime.Save());
            json["Page"] = "Directory";
            Assert.Throws<SnapshotValidationException>(() => GameRuntime.Restore(json.ToString(), rules, content));
            runtime = OpenDayOne(rules, content);
            FinishFirstReaction(runtime);
            RuntimeScenario.Complete(runtime, PresentationKind.ExitTerminal);
            json = JObject.Parse(runtime.Save());
            json["HistoryEncounterId"] = runtime.GetView().Game.CurrentEncounterId;
            Assert.Throws<SnapshotValidationException>(() => GameRuntime.Restore(json.ToString(), rules, content));
        }

        private static GameRuntime OpenTutorial(GameRules rules, IGameContent content)
        {
            var runtime = new GameRuntime(rules, content, true);
            RuntimeScenario.Accept(runtime.TryInteract(InteractionTarget.Computer));
            RuntimeScenario.Complete(runtime, PresentationKind.EnterTerminal);
            RuntimeScenario.Accept(runtime.EnterSelectedPatient("tutorial"));
            return runtime;
        }

        private static GameRuntime OpenDayOne(GameRules rules, IGameContent content)
        {
            var runtime = OpenTutorial(rules, content);
            RuntimeScenario.Complete(runtime, PresentationKind.Introduction);
            RuntimeScenario.Accept(runtime.SubmitDisposition(runtime.GetView().Game.CurrentEncounterId!, Disposition.Purify));
            RuntimeScenario.Complete(runtime, PresentationKind.Reaction);
            RuntimeScenario.Complete(runtime, PresentationKind.ExitTerminal);
            Assert.That(runtime.GetView().LeavingTerminal, Is.True);
            RuntimeScenario.Complete(runtime, PresentationKind.ExitTerminal);
            RuntimeScenario.Accept(runtime.TryInteract(InteractionTarget.Bed));
            RuntimeScenario.Complete(runtime, PresentationKind.Sleep);
            RuntimeScenario.Complete(runtime, PresentationKind.Dream);
            RuntimeScenario.Accept(runtime.TryInteract(InteractionTarget.Bed));
            RuntimeScenario.EnterApartment(runtime);
            RuntimeScenario.Accept(runtime.TryInteract(InteractionTarget.Computer));
            RuntimeScenario.Complete(runtime, PresentationKind.EnterTerminal);
            RuntimeScenario.Accept(runtime.EnterSelectedPatient("first"));
            return runtime;
        }

        private static void Reply(GameRuntime runtime)
        {
            var view = runtime.GetView();
            RuntimeScenario.Accept(runtime.SelectResponse(view.Game.CurrentEncounterId!, view.Game.CurrentNodeId!, "reply"));
            RuntimeScenario.Complete(runtime, PresentationKind.ResponseBranch);
        }

        private static void FinishFirstReaction(GameRuntime runtime)
        {
            runtime.RecordDialogueLine("第一位", "第一位的发言", "first.opening", "FIRST01");
            Reply(runtime);
            RuntimeScenario.Accept(runtime.SubmitDisposition(runtime.GetView().Game.CurrentEncounterId!, Disposition.Treat));
            runtime.RecordDialogueLine("第一位", "第一位的处置反应", "first.reaction", "FIRST02");
            RuntimeScenario.Complete(runtime, PresentationKind.Reaction);
        }

        private static JToken CoreState(GameRuntime runtime) => JObject.Parse(JObject.Parse(runtime.Save())["CoreJson"]!.Value<string>()!)["State"]!;

        private static GameRules Rules()
        {
            var tutorial = new PatientDefinition("tutorial", "无名对象", 0, 0, null, PatientPresence.TreatmentEnded);
            var first = new PatientDefinition("first", "第一位", 1, 0, 3, PatientPresence.Deceased);
            var second = new PatientDefinition("second", "第二位", 1, 0, 3, PatientPresence.TreatmentEnded);
            var introduction = new MeetingDefinition(tutorial.Id, Array.Empty<RoundDefinition>(), null, 0, 3,
                new Dictionary<Disposition, string> { [Disposition.Purify] = "tutorial.purify" }, "tutorial.intro",
                new Dictionary<Disposition, int> { [Disposition.Purify] = -1 });
            MeetingDefinition Meeting(string patient, bool intro = false) => new MeetingDefinition(patient,
                new[] { new RoundDefinition(patient + ".round", new[] { new ResponseDefinition("reply", patient + ".branch", 1) }) },
                null, int.MaxValue, 3, new Dictionary<Disposition, string>
                {
                    [Disposition.Treat] = patient + ".treat", [Disposition.Purify] = patient + ".purify", [Disposition.Release] = patient + ".release"
                }, intro ? patient + ".intro" : null);
            return new GameRules("runtime-campaign-fixture", new[] { tutorial, first, second }, new[]
            {
                new DayDefinition(0, 1, new[] { introduction }),
                new DayDefinition(1, 0, new[] { Meeting("first"), Meeting("second", true) }),
                new DayDefinition(2, 0, new[] { Meeting("first") })
            }, new[] { EndingType.PriestAttack, EndingType.LockedInRoom, EndingType.TakenAway });
        }

        private sealed class FixtureContent : IGameContent
        {
            public string Revision => "runtime-campaign-fixture-v2";
            public DialogueContent GetDialogue(PlayerView view) => new DialogueContent(view.CurrentNodeId!, view.CurrentPatientName!,
                view.CurrentNodeId == "tutorial.intro" ? string.Join("\n", Enumerable.Range(1, 5).Select(i => "教学对白 " + i)) : "会谈文本",
                view.AvailableResponseIds.Select(id => new ResponseContent(id, "实际回答")));
            public string GetPresentation(PresentationKind kind, PlayerView view) => kind.ToString();
            public InspectionContent GetInspection(string patientId, int day, InspectionKind kind) => new InspectionContent(kind, "观察", "记录");
        }
    }
}
