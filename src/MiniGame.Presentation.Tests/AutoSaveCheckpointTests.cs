using System;
using MiniGame.Core;
using NUnit.Framework;

namespace MiniGame.Presentation.Tests
{
    public sealed class AutoSaveCheckpointTests
    {
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
