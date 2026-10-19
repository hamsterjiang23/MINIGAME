using System;
using System.IO;
using MiniGame.Core;
using MiniGame.Presentation.Authoring;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace MiniGame.Presentation.Tests
{
    [TestFixture]
    public sealed class RoomDoorRuntimeTests
    {
        [Test]
        public void FirstMorningOpeningWaitsForMotionThenCompletesInspectionExactlyOnce()
        {
            var runtime = RuntimeScenario.Create();
            var before = CoreJson(runtime);
            var motion = RuntimeScenario.Accept(runtime.ToggleRoomDoor());
            Assert.That(motion.Events, Is.Empty);
            Assert.That(runtime.GetView().RoomDoorOpen, Is.True);
            Assert.That(runtime.GetView().Game.Phase, Is.EqualTo(GamePhase.MorningInspection));
            Assert.That(runtime.GetView().Input, Is.EqualTo(InputContext.Presentation));
            Assert.That(CoreJson(runtime), Is.EqualTo(before));
            var token = runtime.GetView().Cue!.Token;
            RuntimeScenario.RejectWithoutMutation(runtime, runtime.ToggleRoomDoor);
            RuntimeScenario.RejectWithoutMutation(runtime, () => runtime.TryInteract(InteractionTarget.Door));
            RuntimeScenario.RejectWithoutMutation(runtime, () => runtime.TryInteract(InteractionTarget.Computer));
            RuntimeScenario.Complete(runtime, PresentationKind.DoorMotion);
            Assert.That(runtime.GetView().Game.Phase, Is.EqualTo(GamePhase.MorningInspection));
            Assert.That(CoreJson(runtime), Is.EqualTo(before));
            RuntimeScenario.RejectWithoutMutation(runtime, () => runtime.CompleteCue(token));
            RuntimeScenario.Complete(runtime, PresentationKind.MorningInspection);
            RuntimeScenario.Complete(runtime, PresentationKind.Briefing);
            var afterMorning = CoreJson(runtime);
            Assert.That(runtime.GetView().Game.Phase, Is.EqualTo(GamePhase.Apartment));
            RuntimeScenario.Accept(runtime.ToggleRoomDoor());
            Assert.That(runtime.GetView().RoomDoorOpen, Is.False);
            RuntimeScenario.Complete(runtime, PresentationKind.DoorMotion);
            RuntimeScenario.Accept(runtime.ToggleRoomDoor());
            RuntimeScenario.Complete(runtime, PresentationKind.DoorMotion);
            Assert.That(runtime.GetView().Cue, Is.Null);
            Assert.That(CoreJson(runtime), Is.EqualTo(afterMorning));
            RuntimeScenario.RejectWithoutMutation(runtime, () => runtime.TryInteract(InteractionTarget.Door));
        }

        [Test]
        public void PauseFreezesThePendingOpeningAndResumingRetainsItsToken()
        {
            var runtime = RuntimeScenario.Create();
            RuntimeScenario.Accept(runtime.ToggleRoomDoor());
            var token = runtime.GetView().Cue!.Token;
            RuntimeScenario.Accept(runtime.SetPaused(true));
            Assert.That(runtime.GetView().RoomDoorOpen, Is.True);
            RuntimeScenario.RejectWithoutMutation(runtime, runtime.ToggleRoomDoor);
            RuntimeScenario.RejectWithoutMutation(runtime, () => runtime.CompleteCue(token));
            RuntimeScenario.Accept(runtime.SetPaused(false));
            Assert.That(runtime.GetView().Cue!.Token, Is.EqualTo(token));
            RuntimeScenario.Complete(runtime, PresentationKind.DoorMotion);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void RestoringMorningMotionSnapsOpenAndResumesTheInspectionWithAFreshToken(bool paused)
        {
            var rules = PrototypeRules.Create();
            var content = new PrototypeContent(rules);
            var original = new GameRuntime(rules, content);
            RuntimeScenario.Accept(original.ToggleRoomDoor());
            var token = original.GetView().Cue!.Token;
            if (paused) RuntimeScenario.Accept(original.SetPaused(true));
            var core = CoreJson(original);
            var restored = GameRuntime.Restore(original.Save(), rules, content);
            Assert.That(restored.GetView().RoomDoorOpen, Is.True);
            Assert.That(restored.GetView().Cue!.Kind, Is.EqualTo(PresentationKind.MorningInspection));
            Assert.That(CoreJson(restored), Is.EqualTo(core));
            if (paused)
            {
                Assert.That(restored.GetView().Input, Is.EqualTo(InputContext.Paused));
                RuntimeScenario.Accept(restored.SetPaused(false));
            }
            RuntimeScenario.RejectWithoutMutation(restored, () => restored.CompleteCue(token));
            RuntimeScenario.Complete(restored, PresentationKind.MorningInspection);
            RuntimeScenario.Complete(restored, PresentationKind.Briefing);
            var saved = restored.Save();
            Assert.That(GameRuntime.Restore(saved, rules, content).Save(), Is.EqualTo(saved));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void RestoringOrdinaryMotionKeepsItsTargetWithoutReplayingMotionOrMorning(bool closing)
        {
            var rules = PrototypeRules.Create();
            var content = new PrototypeContent(rules);
            var runtime = new GameRuntime(rules, content);
            RuntimeScenario.EnterApartment(runtime);
            RuntimeScenario.Accept(runtime.ToggleRoomDoor());
            if (closing)
            {
                RuntimeScenario.Complete(runtime, PresentationKind.DoorMotion);
                RuntimeScenario.Accept(runtime.ToggleRoomDoor());
            }
            var token = runtime.GetView().Cue!.Token;
            var core = CoreJson(runtime);
            runtime = GameRuntime.Restore(runtime.Save(), rules, content);
            Assert.That(runtime.GetView().RoomDoorOpen, Is.EqualTo(!closing));
            Assert.That(runtime.GetView().Input, Is.EqualTo(InputContext.World));
            Assert.That(runtime.GetView().Cue, Is.Null);
            Assert.That(CoreJson(runtime), Is.EqualTo(core));
            RuntimeScenario.RejectWithoutMutation(runtime, () => runtime.CompleteCue(token));
            var saved = runtime.Save();
            Assert.That(GameRuntime.Restore(saved, rules, content).Save(), Is.EqualTo(saved));
        }

        [Test]
        public void DoorCanToggleBetweenPatientsButNotInsideTerminalOrInspection()
        {
            var runtime = RuntimeScenario.Create();
            RuntimeScenario.OpenTerminal(runtime);
            RuntimeScenario.RejectWithoutMutation(runtime, runtime.ToggleRoomDoor);
            RuntimeScenario.Accept(runtime.Inspect(InspectionKind.Records));
            RuntimeScenario.RejectWithoutMutation(runtime, runtime.ToggleRoomDoor);
            RuntimeScenario.Accept(runtime.CloseInspection());
            RuntimeScenario.ReachDisposition(runtime);
            RuntimeScenario.Accept(runtime.SubmitDisposition(runtime.GetView().Game.CurrentEncounterId!, Disposition.Treat));
            RuntimeScenario.Complete(runtime, PresentationKind.Reaction);
            RuntimeScenario.Complete(runtime, PresentationKind.ExitTerminal);
            var core = CoreJson(runtime);
            RuntimeScenario.Accept(runtime.ToggleRoomDoor());
            RuntimeScenario.Complete(runtime, PresentationKind.DoorMotion);
            Assert.That(runtime.GetView().RoomDoorOpen, Is.True);
            Assert.That(CoreJson(runtime), Is.EqualTo(core));
            var rules = PrototypeRules.Create();
            var restored = GameRuntime.Restore(runtime.Save(), rules, new PrototypeContent(rules));
            Assert.That(restored.GetView().RoomDoorOpen, Is.True);
        }

        [Test]
        public void EndOfDayClosesDoorWithoutReplacingReactionExitSleepOrDream()
        {
            var runtime = RuntimeScenario.Create();
            RuntimeScenario.EnterApartment(runtime);
            RuntimeScenario.Accept(runtime.ToggleRoomDoor());
            RuntimeScenario.Complete(runtime, PresentationKind.DoorMotion);
            RuntimeScenario.DriveUntil(runtime, view => view.Game.Phase == GamePhase.Evening);
            Assert.That(runtime.GetView().RoomDoorOpen, Is.False);
            Assert.That(runtime.GetView().Cue!.Kind, Is.EqualTo(PresentationKind.ExitTerminal));
            RuntimeScenario.RejectWithoutMutation(runtime, runtime.ToggleRoomDoor);
            RuntimeScenario.Complete(runtime, PresentationKind.ExitTerminal);
            RuntimeScenario.RejectWithoutMutation(runtime, runtime.ToggleRoomDoor);
            Assert.That(runtime.GetView().AllowedInteractions, Is.EqualTo(new[] { InteractionTarget.Bed }));
            RuntimeScenario.Accept(runtime.TryInteract(InteractionTarget.Bed));
            Assert.That(runtime.GetView().RoomDoorOpen, Is.False);
            RuntimeScenario.Complete(runtime, PresentationKind.Sleep);
            RuntimeScenario.RejectWithoutMutation(runtime, runtime.ToggleRoomDoor);
            RuntimeScenario.Complete(runtime, PresentationKind.Dream);
            Assert.That(runtime.GetView().RoomDoorOpen, Is.False);
            RuntimeScenario.RejectWithoutMutation(runtime, runtime.ToggleRoomDoor);
            RuntimeScenario.Accept(runtime.TryInteract(InteractionTarget.Bed));
            Assert.That(runtime.GetView().Game.Phase, Is.EqualTo(GamePhase.MorningInspection));
            RuntimeScenario.Accept(runtime.ToggleRoomDoor());
            RuntimeScenario.Complete(runtime, PresentationKind.DoorMotion);
            Assert.That(runtime.GetView().Cue!.Kind, Is.EqualTo(PresentationKind.MorningInspection));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void LegacyCheckpointsWithoutDoorFieldStillRestoreClosedAndKeepTheSameCoreFingerprint(bool campaign)
        {
            var package = campaign ? Campaign() : null;
            var rules = package?.Rules ?? PrototypeRules.Create();
            IGameContent content = package?.Content ?? (IGameContent)new PrototypeContent(rules);
            var runtime = new GameRuntime(rules, content, true);
            var before = JObject.Parse(runtime.Save());
            Assert.That(before.Property("RoomDoorOpen"), Is.Null);
            var restored = GameRuntime.Restore(before.ToString(), rules, content);
            Assert.That(restored.GetView().RoomDoorOpen, Is.False);
            Assert.That(CoreJson(restored), Is.EqualTo(CoreJson(runtime)));
            Assert.That(restored.Save(), Is.EqualTo(runtime.Save()));
            if (campaign)
            {
                RuntimeScenario.Accept(restored.ToggleRoomDoor());
                RuntimeScenario.Complete(restored, PresentationKind.DoorMotion);
                Assert.That(restored.GetView().Game.Day, Is.Zero);
                Assert.That(restored.GetView().Game.Phase, Is.EqualTo(GamePhase.Apartment));
                Assert.That(restored.GetView().Cue, Is.Null);
            }
        }

        [TestCase("closed-morning-motion")]
        [TestCase("terminal-motion")]
        [TestCase("night-open")]
        public void InconsistentDoorCheckpointsAreRejected(string corruption)
        {
            var rules = PrototypeRules.Create();
            var content = new PrototypeContent(rules);
            var runtime = new GameRuntime(rules, content);
            if (corruption == "terminal-motion") RuntimeScenario.OpenTerminal(runtime);
            if (corruption == "night-open") RuntimeScenario.DriveUntil(runtime, view => view.Game.Phase == GamePhase.Evening && view.Cue == null);
            var checkpoint = JObject.Parse(runtime.Save());
            if (corruption == "night-open") checkpoint["RoomDoorOpen"] = true;
            else checkpoint["CueKind"] = "DoorMotion";
            Assert.Throws<SnapshotValidationException>(() => GameRuntime.Restore(checkpoint.ToString(), rules, content));
        }

        private static string CoreJson(GameRuntime runtime) => JObject.Parse(runtime.Save())["CoreJson"]!.Value<string>()!;

        private static CompiledDialoguePackage Campaign()
        {
            var folder = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
            while (folder != null && !Directory.Exists(Path.Combine(folder.FullName, "UnityProject"))) folder = folder.Parent;
            if (folder == null) throw new InvalidOperationException("The campaign fixture requires the project repository.");
            return DialoguePackageCompiler.Compile(File.ReadAllText(Path.Combine(folder.FullName,
                "UnityProject/Assets/MiniGame/Content/Campaign/campaign.minigame.json")));
        }
    }
}
