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
    public sealed class FreePatientSelectionRuntimeTests
    {
        [TestCase(EndingType.TakenAway, 3)]
        [TestCase(EndingType.LockedInRoom, 3)]
        [TestCase(EndingType.PriestAttack, 3)]
        [TestCase(EndingType.PriestAttack, 2)]
        public void ImportedCampaignAllowsDifferentDailyOrdersThroughAllEndingsAndRestores(EndingType expected, int releaseDay)
        {
            var package = Campaign();
            var runtime = new GameRuntime(package.Rules, package.Content, true);
            var selected = new List<string>();
            var dayTwoDiplomatLines = new HashSet<string>();
            Assert.That(package.Content.TextVariants, Is.Empty, "The revised source no longer uses the old historical wording.");
            var orders = new[]
            {
                new[] { "tutorial" },
                new[] { "battlefield-doctor", "child", "former-ruler" },
                new[] { "child", "priest", "battlefield-doctor", "former-ruler" },
                new[] { "battlefield-doctor", "former-ruler", "priest", "child", "diplomat" }
            };
            for (var step = 0; step < 1600 && runtime.GetView().Game.Phase != GamePhase.Ending; step++)
            {
                var view = runtime.GetView();
                if (view.TerminalPage == TerminalPage.Review && view.Game.CurrentNodeId != null
                    && (view.Input == InputContext.Dialogue || view.Cue?.Kind == PresentationKind.Introduction
                        || view.Cue?.Kind == PresentationKind.ResponseBranch || view.Cue?.Kind == PresentationKind.Reaction))
                {
                    foreach (var line in package.Content.GetSegment(view.Game.CurrentNodeId).Lines)
                    {
                        var resolved = runtime.ResolveDialogueText(line.SourceNodeId, line.Text);
                        Assert.That(resolved, Is.EqualTo(line.Text), line.SourceNodeId);
                        if (view.Game.Day == 2 && view.Game.CurrentPatientId == "diplomat")
                        {
                            dayTwoDiplomatLines.Add(line.SourceNodeId);
                        }
                        runtime.RecordDialogueLine(line.Speaker, resolved, line.SourceNodeId, line.PortraitId, line.IsNarration);
                        runtime.CompleteDialogueLine(line.SourceNodeId);
                    }
                }
                if (view.Cue != null)
                {
                    var completedPatient = view.PresentedPatientId;
                    RuntimeScenario.Accept(runtime.CompleteCue(view.Cue.Token));
                    if (view.Cue.Kind == PresentationKind.Reaction)
                    {
                        Assert.That(runtime.GetView().PresentedPatientId, Is.EqualTo(completedPatient));
                        var saved = runtime.Save();
                        runtime = GameRuntime.Restore(saved, package.Rules, package.Content);
                        Assert.That(runtime.Save(), Is.EqualTo(saved));
                        RuntimeScenario.RejectWithoutMutation(runtime, () => runtime.CompleteCue(view.Cue.Token));
                    }
                }
                else if (view.TerminalPage == TerminalPage.Archive)
                {
                    RuntimeScenario.Accept(view.ArchiveDirectoryVisible ? runtime.LeaveTerminal() : runtime.ReturnToDirectory());
                }
                else if (view.TerminalPage == TerminalPage.Directory)
                {
                    var available = view.Patients.Where(patient => patient.CanReceiveToday).Select(patient => patient.Id).ToArray();
                    var id = orders[view.Game.Day].First(available.Contains);
                    if (view.Game.Day == 1 && selected.Count == 1) Assert.That(available.Length, Is.EqualTo(3));
                    RuntimeScenario.Accept(runtime.EnterSelectedPatient(id));
                    selected.Add(view.Game.Day + ":" + id);
                    Assert.That(runtime.GetView().PresentedPatientId, Is.EqualTo(id));
                    Assert.That(runtime.GetView().Game.CurrentPatientId, Is.EqualTo(id));
                    Assert.That(runtime.GetView().Transcript, Is.Empty);
                    RuntimeScenario.RejectWithoutMutation(runtime, () => runtime.EnterSelectedPatient(id));
                    var saved = runtime.Save();
                    runtime = GameRuntime.Restore(saved, package.Rules, package.Content);
                    Assert.That(runtime.Save(), Is.EqualTo(saved));
                }
                else if (view.Game.EncounterStage == EncounterStage.AwaitingDisposition)
                {
                    var disposition = view.Game.Day == 0 ? Disposition.Purify : Disposition.Treat;
                    if (view.Game.Day > 0 && expected == EndingType.TakenAway)
                        disposition = RuntimeScenario.NormalPolicy(view.Game.Day, view.Game.CurrentPatientId!);
                    if (expected == EndingType.PriestAttack && view.Game.Day == releaseDay && view.Game.CurrentPatientId == "priest")
                        disposition = Disposition.Release;
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
            Assert.That(runtime.GetView().Game.Ending, Is.EqualTo(expected));
            var earlyAttack = expected == EndingType.PriestAttack && releaseDay == 2;
            Assert.That(runtime.GetView().Game.Day, Is.EqualTo(earlyAttack ? 3 : 4));
            Assert.That(dayTwoDiplomatLines, Is.Empty, "The diplomat has not arrived on Day2.");
            if (earlyAttack) Assert.That(selected.Count, Is.EqualTo(8));
            Assert.That(selected.Where(id => id.StartsWith("1:", StringComparison.Ordinal)), Is.EqualTo(orders[1].Select(id => "1:" + id)));
            Assert.That(selected.Distinct().Count(), Is.EqualTo(selected.Count));
            var state = Core(runtime)["State"]!;
            Assert.That(state["Encounters"]!.Count(), Is.EqualTo(selected.Count));
            Assert.That(state["Encounters"]!.All(encounter => encounter["Stage"]!.Value<string>() == "Completed"), Is.True);
            Assert.That(state["Patients"]!.Single(patient => patient["Id"]!.Value<string>() == "tutorial")["Trust"]!.Value<int>(), Is.EqualTo(0));
            Assert.That(GameRuntime.Restore(runtime.Save(), package.Rules, package.Content).Save(), Is.EqualTo(runtime.Save()));
        }

        [Test]
        public void PausedEntryAndEarlyExitDoNotLockOrConsumeTheDailySelection()
        {
            var rules = PrototypeRules.Create();
            var content = new PrototypeContent(rules);
            var runtime = new GameRuntime(rules, content, true);
            RuntimeScenario.EnterApartment(runtime);
            RuntimeScenario.Accept(runtime.TryInteract(InteractionTarget.Computer));
            RuntimeScenario.RejectWithoutMutation(runtime, () => runtime.EnterSelectedPatient(PrototypeRules.Doctor));
            RuntimeScenario.Complete(runtime, PresentationKind.EnterTerminal);
            Assert.That(runtime.GetView().Patients.Count(patient => patient.CanReceiveToday), Is.EqualTo(4));
            RuntimeScenario.Accept(runtime.SetPaused(true));
            RuntimeScenario.RejectWithoutMutation(runtime, () => runtime.EnterSelectedPatient(PrototypeRules.Doctor));
            RuntimeScenario.Accept(runtime.SetPaused(false));
            RuntimeScenario.Accept(runtime.LeaveTerminal());
            RuntimeScenario.Complete(runtime, PresentationKind.ExitTerminal);
            runtime = GameRuntime.Restore(runtime.Save(), rules, content);
            RuntimeScenario.Accept(runtime.TryInteract(InteractionTarget.Computer));
            RuntimeScenario.Complete(runtime, PresentationKind.EnterTerminal);
            Assert.That(runtime.GetView().Patients.Count(patient => patient.CanReceiveToday), Is.EqualTo(4));
            RuntimeScenario.Accept(runtime.EnterSelectedPatient(PrototypeRules.Doctor));
            Assert.That(runtime.GetView().PresentedPatientId, Is.EqualTo(PrototypeRules.Doctor));
            Assert.That(runtime.GetView().Game.CurrentPatientName, Is.EqualTo("埃德温"));
            Assert.That(runtime.GetView().Patients.All(patient => !patient.CanReceiveToday), Is.True);
            RuntimeScenario.RejectWithoutMutation(runtime, () => runtime.EnterSelectedPatient(PrototypeRules.Child));
            RuntimeScenario.RejectWithoutMutation(runtime, () => runtime.LeaveTerminal());
            var forged = JObject.Parse(runtime.Save());
            forged["Page"] = "Directory";
            Assert.Throws<SnapshotValidationException>(() => GameRuntime.Restore(forged.ToString(), rules, content));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void OldV1AndV2CheckpointsWithoutExplicitSelectionFieldsStillRestore(bool campaign)
        {
            var package = campaign ? Campaign() : null;
            var rules = package?.Rules ?? PrototypeRules.Create();
            IGameContent content = package?.Content ?? (IGameContent)new PrototypeContent(rules);
            var runtime = new GameRuntime(rules, content, true);
            if (!campaign) RuntimeScenario.EnterApartment(runtime);
            RuntimeScenario.Accept(runtime.TryInteract(InteractionTarget.Computer));
            RuntimeScenario.Complete(runtime, PresentationKind.EnterTerminal);
            RuntimeScenario.Accept(runtime.EnterSelectedPatient(runtime.GetView().Game.CurrentPatientId!));
            // The former runtime entered its preallocated first actor without issuing a core command.
            var checkpoint = JObject.Parse(runtime.Save());
            var core = JObject.Parse(checkpoint["CoreJson"]!.Value<string>()!);
            foreach (var encounter in core["State"]!["Encounters"]!.Cast<JObject>()) encounter.Remove("ExplicitlySelected");
            foreach (var command in core["CommandHistory"]!.Where(command => command["Kind"]!.Value<string>() == "SelectPatient").ToArray()) command.Remove();
            checkpoint["CoreJson"] = core.ToString();
            var restored = GameRuntime.Restore(checkpoint.ToString(), rules, content);
            Assert.That(restored.GetView().Game.CurrentPatientId, Is.EqualTo(runtime.GetView().Game.CurrentPatientId));
            Assert.That(restored.GetView().TerminalPage, Is.EqualTo(TerminalPage.Review));
            Assert.That(JObject.Parse(restored.Save())["Version"]!.Value<int>(), Is.EqualTo(campaign ? 2 : 1));
            Assert.That(restored.GetView().Cue?.Kind, Is.EqualTo(runtime.GetView().Cue?.Kind));
        }

        [Test]
        public void ReleasedAndPurifiedLeaveTheNextDaysDirectoryWhileTreatmentCountsAndHistorySurviveRestore()
        {
            var package = Campaign();
            var runtime = new GameRuntime(package.Rules, package.Content, true);
            bool checkedSameDay = false;
            for (int step = 0; step < 2000; step++)
            {
                var view = runtime.GetView();
                if (view.Game.Day == 1 && view.Game.Phase == GamePhase.Evening)
                {
                    Assert.That(view.Patients.Select(patient => patient.Id), Is.EqualTo(new[] { "former-ruler", "battlefield-doctor", "child" }));
                    checkedSameDay = true;
                }
                if (view.Game.Day == 2 && view.TerminalPage == TerminalPage.Directory && view.Cue == null) break;
                RuntimeResult result;
                if (view.Cue != null) result = runtime.CompleteCue(view.Cue.Token);
                else if (view.TerminalPage == TerminalPage.Archive) result = runtime.LeaveTerminal();
                else if (view.TerminalPage == TerminalPage.Directory)
                    result = runtime.EnterSelectedPatient(view.Patients.First(patient => patient.CanReceiveToday).Id);
                else if (view.Game.EncounterStage == EncounterStage.AwaitingDisposition)
                    result = runtime.SubmitDisposition(view.Game.CurrentEncounterId!, view.Game.Day == 0 || view.Game.CurrentPatientId == "child"
                        ? Disposition.Purify : view.Game.CurrentPatientId == "former-ruler" ? Disposition.Release : Disposition.Treat);
                else if (view.Input == InputContext.Dialogue)
                {
                    var meeting = package.Rules.Days.Single(day => day.Day == view.Game.Day).Meetings.Single(item => item.PatientId == view.Game.CurrentPatientId);
                    var round = view.Game.EncounterStage == EncounterStage.SpecialDialogue ? meeting.SpecialRound!
                        : meeting.NormalRounds.Single(item => item.NodeId == view.Game.CurrentNodeId);
                    result = runtime.SelectResponse(view.Game.CurrentEncounterId!, view.Game.CurrentNodeId!, round.Responses.OrderByDescending(item => item.TrustDelta).First().Id);
                }
                else result = runtime.TryInteract(view.Game.Phase == GamePhase.MorningInspection ? InteractionTarget.Door
                    : view.Game.Phase == GamePhase.Evening || view.Game.Phase == GamePhase.AfterDream ? InteractionTarget.Bed : InteractionTarget.Computer);
                Assert.That(result.Accepted, Is.True, $"{view.Game.Day}/{view.Game.Phase}/{view.Input}/{view.TerminalPage}: {result.Reason}");
            }
            Assert.That(checkedSameDay, Is.True);
            Assert.That(runtime.GetView().Game.Day, Is.EqualTo(2));
            Assert.That(runtime.GetView().Patients.Select(patient => patient.Id), Is.EqualTo(new[] { "battlefield-doctor", "priest" }));
            Assert.That(runtime.GetView().Patients.Single(patient => patient.Id == "battlefield-doctor").TreatmentCount, Is.EqualTo(1));
            var saved = runtime.Save();
            var restored = GameRuntime.Restore(saved, package.Rules, package.Content);
            Assert.That(restored.Save(), Is.EqualTo(saved));
            Assert.That(restored.GetView().Patients.Select(patient => patient.Id), Is.EqualTo(new[] { "battlefield-doctor", "priest" }));
            Assert.That(Core(restored)["State"]!["Patients"]!.Single(patient => patient["Id"]!.Value<string>() == "former-ruler")["Presence"]!.Value<string>(), Is.EqualTo("Released"));
            Assert.That(Core(restored)["State"]!["Patients"]!.Single(patient => patient["Id"]!.Value<string>() == "child")["Presence"]!.Value<string>(), Is.EqualTo("Purified"));
        }

        private static JObject Core(GameRuntime runtime) => JObject.Parse(JObject.Parse(runtime.Save())["CoreJson"]!.Value<string>()!);

        private static CompiledDialoguePackage Campaign()
        {
            var folder = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
            while (folder != null && !Directory.Exists(Path.Combine(folder.FullName, "UnityProject"))) folder = folder.Parent;
            if (folder == null) throw new FileNotFoundException("Campaign fixture not found.");
            return DialoguePackageCompiler.Compile(File.ReadAllText(Path.Combine(folder.FullName,
                "UnityProject/Assets/MiniGame/Content/Campaign/campaign.minigame.json")));
        }
    }
}
