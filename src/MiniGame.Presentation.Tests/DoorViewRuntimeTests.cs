using System;
using System.IO;
using MiniGame.Core;
using MiniGame.Presentation.Authoring;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace MiniGame.Presentation.Tests
{
    [TestFixture]
    public sealed class DoorViewRuntimeTests
    {
        [Test]
        public void MorningEntryWaitsForCameraAndDoorThenStoryBeforeHoldingOutside()
        {
            var runtime = RuntimeScenario.Create();
            var core = CoreJson(runtime);
            var entered = RuntimeScenario.Accept(runtime.EnterDoorView());
            Assert.That(entered.Events, Is.Empty);
            Assert.That(runtime.GetView().DoorViewActive, Is.True);
            Assert.That(runtime.GetView().RoomDoorOpen, Is.True);
            Assert.That(runtime.GetView().Input, Is.EqualTo(InputContext.Presentation));
            Assert.That(CoreJson(runtime), Is.EqualTo(core));
            var motionToken = runtime.GetView().Cue!.Token;
            AssertWorldAndReturnLocked(runtime);

            RuntimeScenario.Complete(runtime, PresentationKind.DoorMotion);
            Assert.That(runtime.GetView().Game.Phase, Is.EqualTo(GamePhase.MorningInspection));
            Assert.That(CoreJson(runtime), Is.EqualTo(core));
            RuntimeScenario.RejectWithoutMutation(runtime, () => runtime.CompleteCue(motionToken));
            AssertWorldAndReturnLocked(runtime);
            RuntimeScenario.Complete(runtime, PresentationKind.MorningInspection);
            Assert.That(runtime.GetView().Game.Phase, Is.EqualTo(GamePhase.Briefing));
            AssertWorldAndReturnLocked(runtime);
            RuntimeScenario.Complete(runtime, PresentationKind.Briefing);

            Assert.That(runtime.GetView().Game.Phase, Is.EqualTo(GamePhase.Apartment));
            AssertOutside(runtime);
            ReturnToRoom(runtime);
        }

        [Test]
        public void OrdinaryEntryAndReturnDoNotChangeCoreAndCloseOnlyAfterReturnAcknowledgment()
        {
            var runtime = RuntimeScenario.Create();
            RuntimeScenario.EnterApartment(runtime);
            var core = CoreJson(runtime);
            RuntimeScenario.Accept(runtime.EnterDoorView());
            RuntimeScenario.Complete(runtime, PresentationKind.DoorMotion);
            AssertOutside(runtime);
            var result = RuntimeScenario.Accept(runtime.ExitDoorView());
            Assert.That(result.Events, Is.Empty);
            Assert.That(runtime.GetView().DoorViewActive, Is.True);
            Assert.That(runtime.GetView().RoomDoorOpen, Is.True);
            Assert.That(runtime.GetView().Input, Is.EqualTo(InputContext.Presentation));
            Assert.That(runtime.GetView().Cue!.Kind, Is.EqualTo(PresentationKind.DoorViewReturn));
            var returnToken = runtime.GetView().Cue!.Token;
            AssertWorldAndReturnLocked(runtime);

            RuntimeScenario.Complete(runtime, PresentationKind.DoorViewReturn);
            Assert.That(runtime.GetView().DoorViewActive, Is.False);
            Assert.That(runtime.GetView().RoomDoorOpen, Is.False);
            Assert.That(runtime.GetView().Input, Is.EqualTo(InputContext.Presentation));
            AssertWorldAndReturnLocked(runtime);
            RuntimeScenario.RejectWithoutMutation(runtime, () => runtime.CompleteCue(returnToken));
            RuntimeScenario.Complete(runtime, PresentationKind.DoorMotion);
            Assert.That(runtime.GetView().Input, Is.EqualTo(InputContext.World));
            Assert.That(CoreJson(runtime), Is.EqualTo(core));
            RuntimeScenario.Accept(runtime.EnterDoorView());
        }

        [TestCase("opening")]
        [TestCase("outside")]
        [TestCase("returning")]
        [TestCase("closing")]
        public void PauseRejectsDoorCommandsAndCueCompletionWithoutLosingPendingState(string stage)
        {
            var runtime = RuntimeScenario.Create();
            PrepareStage(runtime, stage);
            var before = runtime.Save();
            var token = runtime.GetView().Cue?.Token;
            RuntimeScenario.Accept(runtime.SetPaused(true));
            Assert.That(runtime.GetView().Input, Is.EqualTo(InputContext.Paused));
            AssertWorldAndReturnLocked(runtime);
            RuntimeScenario.RejectWithoutMutation(runtime, () => runtime.CompleteCue(token ?? "stale"));
            RuntimeScenario.Accept(runtime.SetPaused(false));
            Assert.That(runtime.Save(), Is.EqualTo(before));
            Assert.That(runtime.GetView().Cue?.Token, Is.EqualTo(token));
            if (token != null) RuntimeScenario.Accept(runtime.CompleteCue(token));
            else RuntimeScenario.Accept(runtime.ExitDoorView());
        }

        [TestCase("opening")]
        [TestCase("outside")]
        [TestCase("returning")]
        [TestCase("closing")]
        public void RestoreSnapsToTheSavedTargetAndRejectsOldCallbacks(string stage)
        {
            var rules = PrototypeRules.Create();
            var content = new PrototypeContent(rules);
            var original = new GameRuntime(rules, content);
            PrepareStage(original, stage);
            RuntimeScenario.Accept(original.SetPaused(true));
            var core = CoreJson(original);
            var token = original.GetView().Cue?.Token ?? "stale";
            var restored = GameRuntime.Restore(original.Save(), rules, content);
            Assert.That(restored.GetView().Input, Is.EqualTo(InputContext.Paused));
            Assert.That(restored.GetView().Cue, Is.Null);
            Assert.That(CoreJson(restored), Is.EqualTo(core));
            RuntimeScenario.Accept(restored.SetPaused(false));
            RuntimeScenario.RejectWithoutMutation(restored, () => restored.CompleteCue(token));
            if (stage == "opening" || stage == "outside") AssertOutside(restored);
            else
            {
                Assert.That(restored.GetView().DoorViewActive, Is.False);
                Assert.That(restored.GetView().RoomDoorOpen, Is.False);
                Assert.That(restored.GetView().Input, Is.EqualTo(InputContext.World));
            }
            Assert.That(GameRuntime.Restore(restored.Save(), rules, content).Save(), Is.EqualTo(restored.Save()));
        }

        [TestCase(PresentationKind.DoorMotion)]
        [TestCase(PresentationKind.MorningInspection)]
        [TestCase(PresentationKind.Briefing)]
        public void RestoreMorningEntryPreservesOutsideViewAndResumesStoryWithFreshToken(PresentationKind stage)
        {
            var rules = PrototypeRules.Create();
            var content = new PrototypeContent(rules);
            var original = new GameRuntime(rules, content);
            RuntimeScenario.Accept(original.EnterDoorView());
            if (stage != PresentationKind.DoorMotion) RuntimeScenario.Complete(original, PresentationKind.DoorMotion);
            if (stage == PresentationKind.Briefing) RuntimeScenario.Complete(original, PresentationKind.MorningInspection);
            var token = original.GetView().Cue!.Token;
            var core = CoreJson(original);
            var restored = GameRuntime.Restore(original.Save(), rules, content);
            Assert.That(restored.GetView().DoorViewActive, Is.True);
            Assert.That(restored.GetView().RoomDoorOpen, Is.True);
            Assert.That(CoreJson(restored), Is.EqualTo(core));
            RuntimeScenario.RejectWithoutMutation(restored, () => restored.CompleteCue(token));
            if (stage != PresentationKind.Briefing) RuntimeScenario.Complete(restored, PresentationKind.MorningInspection);
            RuntimeScenario.Complete(restored, PresentationKind.Briefing);
            AssertOutside(restored);
        }

        [Test]
        public void DoorViewIsAvailableBetweenPatientsButBlockedInsideTerminalInspectionAndAtNight()
        {
            var runtime = RuntimeScenario.Create();
            RuntimeScenario.OpenTerminal(runtime);
            RuntimeScenario.RejectWithoutMutation(runtime, runtime.EnterDoorView);
            RuntimeScenario.RejectWithoutMutation(runtime, runtime.ExitDoorView);
            RuntimeScenario.Accept(runtime.Inspect(InspectionKind.Records));
            RuntimeScenario.RejectWithoutMutation(runtime, runtime.EnterDoorView);
            RuntimeScenario.Accept(runtime.CloseInspection());
            RuntimeScenario.ReachDisposition(runtime);
            RuntimeScenario.Accept(runtime.SubmitDisposition(runtime.GetView().Game.CurrentEncounterId!, Disposition.Treat));
            RuntimeScenario.Complete(runtime, PresentationKind.Reaction);
            RuntimeScenario.Complete(runtime, PresentationKind.ExitTerminal);
            Assert.That(runtime.GetView().Game.Phase, Is.EqualTo(GamePhase.Work));
            var core = CoreJson(runtime);
            RuntimeScenario.Accept(runtime.EnterDoorView());
            RuntimeScenario.Complete(runtime, PresentationKind.DoorMotion);
            AssertOutside(runtime);
            var rules = PrototypeRules.Create();
            var restored = GameRuntime.Restore(runtime.Save(), rules, new PrototypeContent(rules));
            AssertOutside(restored);
            ReturnToRoom(restored);
            Assert.That(CoreJson(restored), Is.EqualTo(core));
            RuntimeScenario.DriveUntil(restored, view => view.Game.Phase == GamePhase.Evening && view.Cue == null);
            Assert.That(restored.GetView().DoorViewActive, Is.False);
            Assert.That(restored.GetView().RoomDoorOpen, Is.False);
            RuntimeScenario.RejectWithoutMutation(restored, restored.EnterDoorView);
            RuntimeScenario.RejectWithoutMutation(restored, restored.ExitDoorView);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void LegacyV1AndV2SavesOmitInactiveDoorViewAndKeepTheirVersion(bool campaign)
        {
            var package = campaign ? Campaign() : null;
            var rules = package?.Rules ?? PrototypeRules.Create();
            IGameContent content = package?.Content ?? (IGameContent)new PrototypeContent(rules);
            var runtime = new GameRuntime(rules, content, true);
            Assert.That(JObject.Parse(runtime.Save()).Property("DoorViewActive"), Is.Null);
            var restored = GameRuntime.Restore(runtime.Save(), rules, content);
            Assert.That(restored.GetView().DoorViewActive, Is.False);
            Assert.That(restored.Save(), Is.EqualTo(runtime.Save()));
            if (!campaign) RuntimeScenario.EnterApartment(restored);
            RuntimeScenario.Accept(restored.EnterDoorView());
            Assert.That(JObject.Parse(restored.Save())["DoorViewActive"]!.Value<bool>(), Is.True);
            Assert.That(JObject.Parse(restored.Save())["Version"]!.Value<int>(), Is.EqualTo(campaign ? 2 : 1));
            restored = GameRuntime.Restore(restored.Save(), rules, content);
            AssertOutside(restored);
            ReturnToRoom(restored);
            Assert.That(JObject.Parse(restored.Save()).Property("DoorViewActive"), Is.Null);
        }

        [TestCase("closed-active")]
        [TestCase("terminal-active")]
        [TestCase("inspection-active")]
        [TestCase("night-active")]
        [TestCase("unstarted-morning-active")]
        [TestCase("morning-return")]
        [TestCase("briefing-return")]
        [TestCase("inactive-return")]
        [TestCase("closed-return")]
        public void IncoherentDoorViewCheckpointsAreRejected(string corruption)
        {
            var rules = PrototypeRules.Create();
            var content = new PrototypeContent(rules);
            var runtime = new GameRuntime(rules, content);
            if (corruption == "terminal-active" || corruption == "inspection-active") RuntimeScenario.OpenTerminal(runtime);
            else if (corruption == "night-active")
                RuntimeScenario.DriveUntil(runtime, view => view.Game.Phase == GamePhase.Evening && view.Cue == null);
            else if (corruption == "briefing-return")
            {
                RuntimeScenario.Accept(runtime.EnterDoorView());
                RuntimeScenario.Complete(runtime, PresentationKind.DoorMotion);
                RuntimeScenario.Complete(runtime, PresentationKind.MorningInspection);
            }
            else if (corruption != "unstarted-morning-active" && corruption != "morning-return")
                RuntimeScenario.EnterApartment(runtime);
            if (corruption == "inspection-active") RuntimeScenario.Accept(runtime.Inspect(InspectionKind.Records));
            var checkpoint = JObject.Parse(runtime.Save());
            checkpoint["DoorViewActive"] = corruption != "inactive-return";
            checkpoint["RoomDoorOpen"] = corruption != "closed-active" && corruption != "closed-return";
            if (corruption.EndsWith("-return", StringComparison.Ordinal)) checkpoint["CueKind"] = "DoorViewReturn";
            Assert.Throws<SnapshotValidationException>(() => GameRuntime.Restore(checkpoint.ToString(), rules, content));
        }

        private static void PrepareStage(GameRuntime runtime, string stage)
        {
            RuntimeScenario.EnterApartment(runtime);
            RuntimeScenario.Accept(runtime.EnterDoorView());
            if (stage == "opening") return;
            RuntimeScenario.Complete(runtime, PresentationKind.DoorMotion);
            if (stage == "outside") return;
            RuntimeScenario.Accept(runtime.ExitDoorView());
            if (stage == "closing") RuntimeScenario.Complete(runtime, PresentationKind.DoorViewReturn);
        }

        private static void AssertOutside(GameRuntime runtime)
        {
            Assert.That(runtime.GetView().Input, Is.EqualTo(InputContext.DoorView));
            Assert.That(runtime.GetView().DoorViewActive, Is.True);
            Assert.That(runtime.GetView().RoomDoorOpen, Is.True);
            Assert.That(runtime.GetView().Cue, Is.Null);
            Assert.That(runtime.GetView().AllowedInteractions, Is.Empty);
            RuntimeScenario.RejectWithoutMutation(runtime, runtime.EnterDoorView);
            RuntimeScenario.RejectWithoutMutation(runtime, runtime.ToggleRoomDoor);
            RuntimeScenario.RejectWithoutMutation(runtime, () => runtime.TryInteract(InteractionTarget.Computer));
            RuntimeScenario.RejectWithoutMutation(runtime, () => runtime.TryInteract(InteractionTarget.Bed));
            RuntimeScenario.RejectWithoutMutation(runtime, () => runtime.TryInteract(InteractionTarget.Door));
        }

        private static void AssertWorldAndReturnLocked(GameRuntime runtime)
        {
            Assert.That(runtime.GetView().AllowedInteractions, Is.Empty);
            RuntimeScenario.RejectWithoutMutation(runtime, runtime.EnterDoorView);
            RuntimeScenario.RejectWithoutMutation(runtime, runtime.ExitDoorView);
            RuntimeScenario.RejectWithoutMutation(runtime, runtime.ToggleRoomDoor);
            RuntimeScenario.RejectWithoutMutation(runtime, () => runtime.TryInteract(InteractionTarget.Computer));
            RuntimeScenario.RejectWithoutMutation(runtime, () => runtime.TryInteract(InteractionTarget.Door));
        }

        private static void ReturnToRoom(GameRuntime runtime)
        {
            RuntimeScenario.Accept(runtime.ExitDoorView());
            RuntimeScenario.Complete(runtime, PresentationKind.DoorViewReturn);
            RuntimeScenario.Complete(runtime, PresentationKind.DoorMotion);
            Assert.That(runtime.GetView().Input, Is.EqualTo(InputContext.World));
            Assert.That(runtime.GetView().DoorViewActive, Is.False);
            Assert.That(runtime.GetView().RoomDoorOpen, Is.False);
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
