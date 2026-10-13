using System;
using System.Collections.Generic;
using System.Linq;
using MiniGame.Core;
using MiniGame.Presentation;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace MiniGame.Presentation.Tests
{
    [TestFixture]
    public sealed class RuntimeEndingTests
    {
        [TestCase(EndingType.TakenAway, 4)]
        [TestCase(EndingType.LockedInRoom, 4)]
        [TestCase(EndingType.PriestAttack, 3)]
        public void RoomDialogueAndNightLifecyclePreserveAllThreeCoreEndings(EndingType expectedEnding, int expectedDay)
        {
            var runtime = RuntimeScenario.Create();
            var encountered = new HashSet<string>(StringComparer.Ordinal);
            Disposition Policy(int day, string id)
            {
                if (expectedEnding == EndingType.PriestAttack && id == PrototypeRules.Priest) return Disposition.Release;
                if (expectedEnding == EndingType.LockedInRoom && day == 1) return Disposition.Treat;
                return RuntimeScenario.NormalPolicy(day, id);
            }
            for (var guard = 0; guard < 1000 && runtime.GetView().Game.Phase != GamePhase.Ending; guard++)
            {
                var id = runtime.GetView().Game.CurrentPatientId;
                if (id != null) encountered.Add(id);
                RuntimeScenario.Step(runtime, Policy, expectedEnding == EndingType.PriestAttack ? "empathize" : "neutral");
            }
            var view = runtime.GetView();
            Assert.That(view.Game.Phase, Is.EqualTo(GamePhase.Ending));
            Assert.That(view.Game.Ending, Is.EqualTo(expectedEnding));
            Assert.That(view.Game.Day, Is.EqualTo(expectedDay));
            Assert.That(encountered.Count, Is.EqualTo(5));
            Assert.That(view.Patients.Count, Is.EqualTo(5));
            Assert.That(view.Input, Is.EqualTo(InputContext.Ending));
            Assert.That(view.AllowedInteractions, Is.Empty);
            var core = JObject.Parse(JObject.Parse(runtime.Save())["CoreJson"]!.Value<string>()!);
            Assert.That(core["State"]!["Days"]!.Count(), Is.EqualTo(expectedEnding == EndingType.PriestAttack ? 2 : 3));
            if (expectedEnding == EndingType.LockedInRoom)
                Assert.That(core["State"]!["Days"]![0]!["QuotaMet"]!.Value<bool>(), Is.False);
            RuntimeScenario.RejectWithoutMutation(runtime, () => runtime.TryInteract(InteractionTarget.Bed));
        }

        [TestCase(Disposition.Treat, true)]
        [TestCase(Disposition.Purify, false)]
        [TestCase(Disposition.Release, false)]
        public void TreatmentReturnsThePatientTomorrowWhilePurificationAndReleaseRemoveThem(Disposition disposition, bool returns)
        {
            var runtime = RuntimeScenario.Create();
            Disposition Policy(int day, string id)
                => day == 1 && id == PrototypeRules.Child ? disposition : Disposition.Treat;
            RuntimeScenario.DriveUntil(runtime, view => view.Game.Day == 2 && view.Game.Phase == GamePhase.Apartment,
                Policy, disposition == Disposition.Release ? "empathize" : "neutral");
            var child = runtime.GetView().Patients.Single(patient => patient.Id == PrototypeRules.Child);
            Assert.That(child.Presence, Is.EqualTo(disposition == Disposition.Treat ? PatientPresence.Active
                : disposition == Disposition.Purify ? PatientPresence.Purified : PatientPresence.Released));
            Assert.That(child.TreatmentCount, Is.EqualTo(disposition == Disposition.Treat ? 1 : 0));
            var dayTwoPatients = new HashSet<string>(StringComparer.Ordinal);
            for (var guard = 0; guard < 500 && runtime.GetView().Game.Phase != GamePhase.Evening; guard++)
            {
                var view = runtime.GetView();
                if (view.Game.CurrentPatientId != null) dayTwoPatients.Add(view.Game.CurrentPatientId);
                RuntimeScenario.Step(runtime, Policy, "neutral");
            }
            Assert.That(runtime.GetView().Game.Phase, Is.EqualTo(GamePhase.Evening));
            Assert.That(dayTwoPatients.Contains(PrototypeRules.Child), Is.EqualTo(returns));
            Assert.That(dayTwoPatients.Contains(PrototypeRules.Priest), Is.True);
        }

        [Test]
        public void DawnScheduledAttackRunsOnlyOnceWhenResumingAcrossTheNightBoundary()
        {
            var rules = PrototypeRules.Create();
            var content = new PrototypeContent(rules);
            var runtime = new GameRuntime(rules, content);
            Disposition Policy(int day, string id) => id == PrototypeRules.Priest ? Disposition.Release : RuntimeScenario.NormalPolicy(day, id);
            RuntimeScenario.DriveUntil(runtime, view => view.Game.Day == 2 && view.Game.Phase == GamePhase.AfterDream, Policy, "empathize");
            var restored = GameRuntime.Restore(runtime.Save(), rules, content);
            var dawn = RuntimeScenario.Accept(restored.TryInteract(InteractionTarget.Bed));
            Assert.That(dawn.Events.Count(item => item.Kind == GameEventKind.ScheduledEventExecuted), Is.EqualTo(1));
            Assert.That(dawn.Events.Count(item => item.Kind == GameEventKind.GameEnded), Is.EqualTo(1));
            var finished = GameRuntime.Restore(restored.Save(), rules, content);
            RuntimeScenario.RejectWithoutMutation(finished, () => finished.TryInteract(InteractionTarget.Bed));
            var core = JObject.Parse(JObject.Parse(finished.Save())["CoreJson"]!.Value<string>()!);
            Assert.That(core["State"]!["ExecutedEventIds"]!.Count(), Is.EqualTo(1));
            Assert.That(core["State"]!["PendingEvents"], Is.Empty);
        }
    }
}
