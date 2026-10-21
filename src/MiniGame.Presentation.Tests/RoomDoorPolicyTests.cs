using System;
using System.Linq;
using MiniGame.Core;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace MiniGame.Presentation.Tests
{
    [TestFixture]
    public sealed class RoomDoorPolicyTests
    {
        [Test]
        public void DefaultPolicyPreservesPostBriefingDoorAccess()
        {
            var runtime = RuntimeScenario.Create();
            Assert.That(runtime.LockDoorAfterSupervisor, Is.False);
            Assert.That(runtime.IsDoorLockedAfterSupervisor, Is.False);
            RuntimeScenario.EnterApartment(runtime);
            Assert.That(runtime.GetView().AllowedInteractions, Does.Contain(InteractionTarget.Door));
            RuntimeScenario.Accept(runtime.EnterDoorView());
            RuntimeScenario.Complete(runtime, PresentationKind.DoorMotion);
            ReturnInside(runtime);
            RuntimeScenario.Accept(runtime.ToggleRoomDoor());
            RuntimeScenario.Complete(runtime, PresentationKind.DoorMotion);
            Assert.That(runtime.GetView().RoomDoorOpen, Is.True);
            Assert.That(runtime.GetView().Cue, Is.Null);
        }

        [Test]
        public void MorningAndBriefingFinishOnceBeforeTheClosedDoorLocks()
        {
            var runtime = RuntimeScenario.Create();
            runtime.ConfigureDoorLockAfterSupervisor(true);
            Assert.That(runtime.IsDoorLockedAfterSupervisor, Is.False);
            RuntimeScenario.Accept(runtime.EnterDoorView());
            RuntimeScenario.Complete(runtime, PresentationKind.DoorMotion);
            RuntimeScenario.Complete(runtime, PresentationKind.MorningInspection);
            Assert.That(runtime.GetView().Game.Phase, Is.EqualTo(GamePhase.Briefing));
            Assert.That(runtime.IsDoorLockedAfterSupervisor, Is.False);
            RuntimeScenario.Complete(runtime, PresentationKind.Briefing);
            var afterSupervisor = CoreJson(runtime);
            Assert.That(runtime.IsDoorLockedAfterSupervisor, Is.True);
            Assert.That(runtime.GetView().Input, Is.EqualTo(InputContext.DoorView));
            Assert.That(runtime.GetView().RoomDoorOpen, Is.True, "Finishing the supervisor must not close the door on an outside camera.");
            ReturnInside(runtime);
            AssertClosedDoorLocked(runtime);
            Assert.That(CoreJson(runtime), Is.EqualTo(afterSupervisor), "Returning and rejected reopening must not repeat supervisor commands.");
        }

        [Test]
        public void ConfigChangesRefreshTheCachedViewOnlyWhenChangedAndDoNotEnterTheSave()
        {
            var runtime = RuntimeScenario.Create();
            RuntimeScenario.EnterApartment(runtime);
            var previousView = runtime.GetView();
            var saved = runtime.Save();
            int notifications = 0;
            RuntimeView? notified = null;
            runtime.Changed += view => { notifications++; notified = view; };
            runtime.ConfigureDoorLockAfterSupervisor(true);
            Assert.That(notifications, Is.EqualTo(1));
            Assert.That(notified, Is.SameAs(runtime.GetView()));
            Assert.That(runtime.GetView(), Is.Not.SameAs(previousView));
            Assert.That(runtime.GetView().AllowedInteractions, Does.Not.Contain(InteractionTarget.Door));
            var lockedView = runtime.GetView();
            runtime.ConfigureDoorLockAfterSupervisor(true);
            Assert.That(notifications, Is.EqualTo(1));
            Assert.That(runtime.GetView(), Is.SameAs(lockedView));
            Assert.That(runtime.Save(), Is.EqualTo(saved));
            Assert.That(JObject.Parse(saved).Property("LockDoorAfterSupervisor"), Is.Null);
            runtime.ConfigureDoorLockAfterSupervisor(false);
            Assert.That(notifications, Is.EqualTo(2));
            Assert.That(runtime.IsDoorLockedAfterSupervisor, Is.False);
            Assert.That(runtime.GetView().AllowedInteractions, Does.Contain(InteractionTarget.Door));
            RuntimeScenario.Accept(runtime.EnterDoorView());
            RuntimeScenario.Complete(runtime, PresentationKind.DoorMotion);
            Assert.That(runtime.GetView().Input, Is.EqualTo(InputContext.DoorView));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void EnablingPolicyWithAnOpenDoorAllowsClosingOrReturningThroughDoorView(bool enterOutsideView)
        {
            var runtime = RuntimeScenario.Create();
            RuntimeScenario.EnterApartment(runtime);
            RuntimeScenario.Accept(runtime.ToggleRoomDoor());
            RuntimeScenario.Complete(runtime, PresentationKind.DoorMotion);
            runtime.ConfigureDoorLockAfterSupervisor(true);
            Assert.That(runtime.IsDoorLockedAfterSupervisor, Is.True);
            Assert.That(runtime.GetView().RoomDoorOpen, Is.True);
            Assert.That(runtime.GetView().AllowedInteractions, Does.Contain(InteractionTarget.Door));
            if (enterOutsideView)
            {
                RuntimeScenario.Accept(runtime.EnterDoorView());
                RuntimeScenario.Complete(runtime, PresentationKind.DoorMotion);
                ReturnInside(runtime);
            }
            else
            {
                RuntimeScenario.Accept(runtime.ToggleRoomDoor());
                RuntimeScenario.Complete(runtime, PresentationKind.DoorMotion);
            }
            AssertClosedDoorLocked(runtime);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void LivePolicyChangeDuringOpeningOrOutsideViewDoesNotTrapTheCamera(bool finishOpeningFirst)
        {
            var runtime = RuntimeScenario.Create();
            RuntimeScenario.EnterApartment(runtime);
            RuntimeScenario.Accept(runtime.EnterDoorView());
            if (finishOpeningFirst) RuntimeScenario.Complete(runtime, PresentationKind.DoorMotion);
            var pendingToken = runtime.GetView().Cue?.Token;
            runtime.ConfigureDoorLockAfterSupervisor(true);
            Assert.That(runtime.GetView().DoorViewActive, Is.True);
            Assert.That(runtime.GetView().RoomDoorOpen, Is.True);
            Assert.That(runtime.GetView().Cue?.Token, Is.EqualTo(pendingToken));
            if (!finishOpeningFirst) RuntimeScenario.Complete(runtime, PresentationKind.DoorMotion);
            ReturnInside(runtime);
            AssertClosedDoorLocked(runtime);
        }

        [Test]
        public void WorkBetweenPatientsRemainsLockedAndTheNextMorningUnlocksNaturally()
        {
            var runtime = RuntimeScenario.Create();
            runtime.ConfigureDoorLockAfterSupervisor(true);
            RuntimeScenario.OpenTerminal(runtime);
            RuntimeScenario.ReachDisposition(runtime);
            RuntimeScenario.Accept(runtime.SubmitDisposition(runtime.GetView().Game.CurrentEncounterId!, Disposition.Treat));
            RuntimeScenario.Complete(runtime, PresentationKind.Reaction);
            RuntimeScenario.Complete(runtime, PresentationKind.ExitTerminal);
            Assert.That(runtime.GetView().Game.Phase, Is.EqualTo(GamePhase.Work));
            AssertClosedDoorLocked(runtime);
            RuntimeScenario.DriveUntil(runtime, view => view.Game.Day == 2 && view.Game.Phase == GamePhase.MorningInspection && view.Cue == null);
            Assert.That(runtime.LockDoorAfterSupervisor, Is.True);
            Assert.That(runtime.IsDoorLockedAfterSupervisor, Is.False);
            Assert.That(runtime.GetView().AllowedInteractions, Does.Contain(InteractionTarget.Door));
            RuntimeScenario.Accept(runtime.EnterDoorView());
            RuntimeScenario.Complete(runtime, PresentationKind.DoorMotion);
            RuntimeScenario.Complete(runtime, PresentationKind.MorningInspection);
            RuntimeScenario.Complete(runtime, PresentationKind.Briefing);
            Assert.That(runtime.IsDoorLockedAfterSupervisor, Is.True);
            ReturnInside(runtime);
            AssertClosedDoorLocked(runtime);
        }

        [Test]
        public void DayZeroHasNoCompletedSupervisorToLockTheDoor()
        {
            var baseline = PrototypeRules.Create();
            var days = new[] { new DayDefinition(0, 0, Array.Empty<MeetingDefinition>()) }.Concat(baseline.Days);
            var rules = new GameRules("door-policy-day-zero", baseline.Patients, days, baseline.EndingPriority);
            var runtime = new GameRuntime(rules, new PrototypeContent(rules));
            runtime.ConfigureDoorLockAfterSupervisor(true);
            Assert.That(runtime.GetView().Game.Day, Is.Zero);
            Assert.That(runtime.GetView().Game.Phase, Is.EqualTo(GamePhase.Apartment));
            Assert.That(runtime.IsDoorLockedAfterSupervisor, Is.False);
            RuntimeScenario.Accept(runtime.EnterDoorView());
            RuntimeScenario.Complete(runtime, PresentationKind.DoorMotion);
            ReturnInside(runtime);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void ExistingSavesRestoreWithoutPolicyAndAllowTheHostToReapplyIt(bool savedOutside)
        {
            var rules = PrototypeRules.Create();
            var content = new PrototypeContent(rules);
            var original = new GameRuntime(rules, content);
            RuntimeScenario.EnterApartment(original);
            if (savedOutside)
            {
                RuntimeScenario.Accept(original.EnterDoorView());
                RuntimeScenario.Complete(original, PresentationKind.DoorMotion);
            }
            var saved = original.Save();
            var restored = GameRuntime.Restore(saved, rules, content);
            Assert.That(restored.LockDoorAfterSupervisor, Is.False);
            Assert.That(restored.IsDoorLockedAfterSupervisor, Is.False);
            restored.ConfigureDoorLockAfterSupervisor(true);
            Assert.That(restored.Save(), Is.EqualTo(saved));
            if (savedOutside) ReturnInside(restored);
            AssertClosedDoorLocked(restored);
        }

        private static void ReturnInside(GameRuntime runtime)
        {
            RuntimeScenario.Accept(runtime.ExitDoorView());
            RuntimeScenario.Complete(runtime, PresentationKind.DoorViewReturn);
            RuntimeScenario.Complete(runtime, PresentationKind.DoorMotion);
            Assert.That(runtime.GetView().Input, Is.EqualTo(InputContext.World));
            Assert.That(runtime.GetView().RoomDoorOpen, Is.False);
            Assert.That(runtime.GetView().DoorViewActive, Is.False);
        }

        private static void AssertClosedDoorLocked(GameRuntime runtime)
        {
            Assert.That(runtime.IsDoorLockedAfterSupervisor, Is.True);
            Assert.That(runtime.GetView().RoomDoorOpen, Is.False);
            Assert.That(runtime.GetView().AllowedInteractions, Does.Not.Contain(InteractionTarget.Door));
            Assert.That(runtime.GetView().AllowedInteractions, Does.Contain(InteractionTarget.Computer));
            AssertLockedRejection(runtime, runtime.EnterDoorView);
            AssertLockedRejection(runtime, runtime.ToggleRoomDoor);
        }

        private static void AssertLockedRejection(GameRuntime runtime, Func<RuntimeResult> command)
        {
            var saved = runtime.Save();
            var result = command();
            Assert.That(result.Accepted, Is.False);
            Assert.That(result.Reason, Is.EqualTo("DoorLockedAfterSupervisor"));
            Assert.That(result.Events, Is.Empty);
            Assert.That(runtime.Save(), Is.EqualTo(saved));
            Assert.That(runtime.GetView().Cue, Is.Null);
        }

        private static string CoreJson(GameRuntime runtime) => JObject.Parse(runtime.Save())["CoreJson"]!.Value<string>()!;
    }
}
