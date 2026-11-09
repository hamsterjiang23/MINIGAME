using System;
using System.IO;
using System.Linq;
using MiniGame.Core;
using MiniGame.Presentation.Authoring;
using NUnit.Framework;

namespace MiniGame.Presentation.Tests
{
    public sealed class AutoSaveCheckpointTests
    {
        [TestCase(Disposition.Release, 2)]
        [TestCase(Disposition.Release, 3)]
        [TestCase(Disposition.Treat, 2)]
        [TestCase(Disposition.Purify, 2)]
        public void PriestJudgmentCheckpointFreezesOnlyAfterReleaseAndSurvivesRestore(Disposition choice, int day)
        {
            var folder = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
            while (folder != null && !Directory.Exists(Path.Combine(folder.FullName, "UnityProject"))) folder = folder.Parent;
            var package = DialoguePackageCompiler.Compile(File.ReadAllText(Path.Combine(folder!.FullName,
                "UnityProject/Assets/MiniGame/Content/Campaign/campaign.minigame.json")));
            var runtime = new GameRuntime(package.Rules, package.Content, true);
            string? automatic = null;
            int requests = 0;
            void Bind() => runtime.AutoSaveRequested += () => { requests++; automatic = runtime.Save(); };
            Bind();
            for (int step = 0; step < 2000 && !(runtime.GetView().Game.Day == day
                && runtime.GetView().Game.CurrentPatientId == "priest"
                && runtime.GetView().Game.EncounterStage == EncounterStage.AwaitingDisposition); step++)
                StepCampaign(runtime, package);
            var judgment = runtime.GetView();
            Assert.That(judgment.Game.CurrentPatientId, Is.EqualTo("priest"));
            Assert.That(judgment.Game.Day, Is.EqualTo(day));
            Assert.That(judgment.Game.EncounterStage, Is.EqualTo(EncounterStage.AwaitingDisposition));
            Assert.That(automatic, Is.EqualTo(runtime.Save()), "Save must be at the actionable judgment, not its preceding response.");
            string checkpoint = automatic!;
            int before = requests;
            runtime.SetPaused(true);
            runtime.SetPaused(false);
            RuntimeScenario.RejectWithoutMutation(runtime, () => runtime.SubmitDisposition("stale", choice));
            Assert.That(requests, Is.EqualTo(before));
            RuntimeScenario.Accept(runtime.SubmitDisposition(judgment.Game.CurrentEncounterId!, choice));
            Assert.That(runtime.IsAutomaticSaveLocked, Is.EqualTo(choice == Disposition.Release));
            string manualAfterChoice = runtime.Save();
            runtime = GameRuntime.Restore(manualAfterChoice, package.Rules, package.Content);
            Bind();
            Assert.That(runtime.IsAutomaticSaveLocked, Is.EqualTo(choice == Disposition.Release));
            for (int step = 0; step < 2000 && runtime.GetView().Game.Phase != GamePhase.Ending; step++)
                StepCampaign(runtime, package);
            Assert.That(runtime.GetView().Game.Phase, Is.EqualTo(GamePhase.Ending));
            if (choice == Disposition.Release)
            {
                Assert.That(runtime.GetView().Game.Ending, Is.EqualTo(EndingType.PriestAttack));
                Assert.That(requests, Is.EqualTo(before), "No later interaction or response may overwrite the judgment.");
                Assert.That(automatic, Is.EqualTo(checkpoint));
            }
            else Assert.That(requests, Is.GreaterThan(before));

            runtime = GameRuntime.Restore(checkpoint, package.Rules, package.Content);
            Bind();
            Assert.That(runtime.IsAutomaticSaveLocked, Is.False);
            Assert.That(runtime.GetView().Game.AvailableDispositions, Does.Contain(Disposition.Treat));
            RuntimeScenario.Accept(runtime.SubmitDisposition(runtime.GetView().Game.CurrentEncounterId!, Disposition.Treat));
            before = requests;
            for (int step = 0; step < 500 && requests == before; step++) StepCampaign(runtime, package);
            Assert.That(requests, Is.GreaterThan(before), "Choosing a different route after loading must resume autosaves.");
            Assert.That(new GameRuntime(package.Rules, package.Content).IsAutomaticSaveLocked, Is.False);
        }

        private static void StepCampaign(GameRuntime runtime, CompiledDialoguePackage package)
        {
            var view = runtime.GetView();
            RuntimeResult result;
            if (view.Cue != null) result = runtime.CompleteCue(view.Cue.Token);
            else if (view.TerminalPage == TerminalPage.Archive) result = runtime.LeaveTerminal();
            else if (view.TerminalPage == TerminalPage.Directory) result = runtime.EnterSelectedPatient(view.Game.CurrentPatientId!);
            else if (view.Game.EncounterStage == EncounterStage.AwaitingDisposition)
                result = runtime.SubmitDisposition(view.Game.CurrentEncounterId!,
                    view.Game.RemainingPurificationQuota > 0 && view.Game.CurrentPatientId != "priest"
                        ? Disposition.Purify : Disposition.Treat);
            else if (view.Input == InputContext.Dialogue)
            {
                var meeting = package.Rules.Days.Single(d => d.Day == view.Game.Day).Meetings.Single(m => m.PatientId == view.Game.CurrentPatientId);
                var round = view.Game.EncounterStage == EncounterStage.SpecialDialogue ? meeting.SpecialRound!
                    : meeting.NormalRounds.Single(r => r.NodeId == view.Game.CurrentNodeId);
                result = runtime.SelectResponse(view.Game.CurrentEncounterId!, view.Game.CurrentNodeId!,
                    round.Responses.OrderByDescending(r => r.TrustDelta).First().Id);
            }
            else result = runtime.TryInteract(view.Game.Phase == GamePhase.MorningInspection ? InteractionTarget.Door
                : view.Game.Phase == GamePhase.Apartment || view.Game.Phase == GamePhase.Work ? InteractionTarget.Computer : InteractionTarget.Bed);
            RuntimeScenario.Accept(result);
        }

        [TestCase("inspection")]
        [TestCase("toggle")]
        [TestCase("door-view")]
        public void ValidDoorCommandsSaveBeforeMutationAndRejectedRetriesDoNotSave(string command)
        {
            var runtime = RuntimeScenario.Create();
            string? saved = null;
            int requests = 0;
            runtime.AutoSaveRequested += () => { requests++; saved = runtime.Save(); };
            var before = runtime.Save();
            Func<RuntimeResult> interact = command == "toggle" ? runtime.ToggleRoomDoor
                : command == "door-view" ? runtime.EnterDoorView
                : () => runtime.TryInteract(InteractionTarget.Door);
            RuntimeScenario.Accept(interact());
            Assert.That(saved, Is.EqualTo(before));
            Assert.That(runtime.Save(), Is.Not.EqualTo(before));
            RuntimeScenario.RejectWithoutMutation(runtime, interact);
            Assert.That(requests, Is.EqualTo(1));
        }

        [Test]
        public void BedSavesBeforeSleepAndIgnoresUnavailableInteraction()
        {
            var runtime = RuntimeScenario.Create();
            int requests = 0;
            string? saved = null;
            runtime.AutoSaveRequested += () => { requests++; saved = runtime.Save(); };
            RuntimeScenario.RejectWithoutMutation(runtime, () => runtime.TryInteract(InteractionTarget.Bed));
            Assert.That(requests, Is.Zero);
            RuntimeScenario.DriveUntil(runtime, view => view.Game.Phase == GamePhase.Evening && view.Cue == null);
            requests = 0;
            var before = runtime.Save();
            RuntimeScenario.Accept(runtime.TryInteract(InteractionTarget.Bed));
            Assert.That(saved, Is.EqualTo(before));
            Assert.That(requests, Is.EqualTo(1));
            RuntimeScenario.RejectWithoutMutation(runtime, () => runtime.TryInteract(InteractionTarget.Bed));
            Assert.That(requests, Is.EqualTo(1));
        }

        [Test]
        public void AnswerSavesCommittedBranchAndRestoresWithoutRepeatingChoice()
        {
            var runtime = RuntimeScenario.Create();
            RuntimeScenario.OpenTerminal(runtime);
            int requests = 0;
            string? saved = null;
            runtime.AutoSaveRequested += () => { requests++; saved = runtime.Save(); };
            var game = runtime.GetView().Game;
            RuntimeScenario.RejectWithoutMutation(runtime,
                () => runtime.SelectResponse(game.CurrentEncounterId!, game.CurrentNodeId!, "invalid"));
            Assert.That(requests, Is.Zero);
            RuntimeScenario.Accept(runtime.SelectResponse(game.CurrentEncounterId!, game.CurrentNodeId!, "neutral"));
            Assert.That(saved, Is.EqualTo(runtime.Save()));
            var rules = PrototypeRules.Create();
            var restored = GameRuntime.Restore(saved!, rules, new PrototypeContent(rules));
            Assert.That(restored.GetView().Cue!.Kind, Is.EqualTo(PresentationKind.ResponseBranch));
            RuntimeScenario.RejectWithoutMutation(restored,
                () => restored.SelectResponse(game.CurrentEncounterId!, game.CurrentNodeId!, "neutral"));
            RuntimeScenario.Complete(restored, PresentationKind.ResponseBranch);
            Assert.That(requests, Is.EqualTo(1));
        }

        [Test]
        public void PauseAndPresentationCompletionDoNotRequestAutomaticSaves()
        {
            var runtime = RuntimeScenario.Create();
            RuntimeScenario.Accept(runtime.TryInteract(InteractionTarget.Door));
            int requests = 0;
            runtime.AutoSaveRequested += () => requests++;
            runtime.SetPaused(true);
            runtime.SetPaused(false);
            RuntimeScenario.Complete(runtime, PresentationKind.MorningInspection);
            RuntimeScenario.Complete(runtime, PresentationKind.Briefing);
            Assert.That(requests, Is.Zero);
        }
    }
}
