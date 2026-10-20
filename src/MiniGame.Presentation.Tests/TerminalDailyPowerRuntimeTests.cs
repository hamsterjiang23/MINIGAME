using System.Linq;
using MiniGame.Core;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace MiniGame.Presentation.Tests
{
    public sealed class TerminalDailyPowerRuntimeTests
    {
        [Test]
        public void LeavingAfterOnePatientKeepsPowerAndReentryDoesNotBootAgain()
        {
            var rules = PrototypeRules.Create(); var content = new PrototypeContent(rules);
            var runtime = new GameRuntime(rules, content, true);
            Assert.That(runtime.GetView().TerminalPower, Is.EqualTo(TerminalPowerState.Off));
            RuntimeScenario.EnterApartment(runtime);
            RuntimeScenario.Accept(runtime.TryInteract(InteractionTarget.Computer));
            Assert.That(runtime.GetView().TerminalBootRequired, Is.True);
            runtime = GameRuntime.Restore(runtime.Save(), rules, content);
            Assert.That(runtime.GetView().TerminalBootRequired, Is.True);
            RuntimeScenario.Complete(runtime, PresentationKind.EnterTerminal);
            Assert.That(runtime.GetView().TerminalPower, Is.EqualTo(TerminalPowerState.Running));
            FinishPatient(runtime);
            Assert.That(runtime.GetView().TerminalPower, Is.EqualTo(TerminalPowerState.Running));
            RuntimeScenario.Complete(runtime, PresentationKind.ExitTerminal);
            var quota = runtime.GetView().Game.RemainingPurificationQuota;
            var completed = runtime.GetView().Patients.Where(p => p.HasMetToday).Select(p => p.Id).ToArray();
            Assert.That(completed, Has.Length.EqualTo(1));
            RuntimeScenario.Accept(runtime.LeaveTerminal());
            Assert.That(runtime.GetView().TerminalShuttingDown, Is.False);
            RuntimeScenario.Complete(runtime, PresentationKind.ExitTerminal);
            Assert.That(runtime.GetView().TerminalOpen, Is.False);
            Assert.That(runtime.GetView().TerminalPowered, Is.True);
            runtime = GameRuntime.Restore(runtime.Save(), rules, content);
            Assert.That(runtime.GetView().TerminalPowered, Is.True);
            RuntimeScenario.Accept(runtime.TryInteract(InteractionTarget.Computer));
            Assert.That(runtime.GetView().TerminalBootRequired, Is.False);
            RuntimeScenario.Complete(runtime, PresentationKind.EnterTerminal);
            Assert.That(runtime.GetView().Game.RemainingPurificationQuota, Is.EqualTo(quota));
            Assert.That(runtime.GetView().Patients.Where(p => p.HasMetToday).Select(p => p.Id), Is.EqualTo(completed));
        }

        [Test]
        public void OnlyFinalCompletedReactionShutsDownAndNextDayStartsOff()
        {
            var rules = PrototypeRules.Create(); var content = new PrototypeContent(rules);
            var runtime = new GameRuntime(rules, content, true);
            RuntimeScenario.EnterApartment(runtime);
            RuntimeScenario.Accept(runtime.TryInteract(InteractionTarget.Computer));
            RuntimeScenario.Complete(runtime, PresentationKind.EnterTerminal);
            for (int guard = 0; guard < 10; guard++)
            {
                FinishPatient(runtime);
                Assert.That(runtime.GetView().TerminalShuttingDown, Is.False, "Patient signal loss is not whole-system shutdown");
                if (runtime.GetView().Game.Phase == GamePhase.Evening) break;
                RuntimeScenario.Complete(runtime, PresentationKind.ExitTerminal);
                RuntimeScenario.Accept(runtime.ReturnToDirectory());
            }
            Assert.That(runtime.GetView().Game.Phase, Is.EqualTo(GamePhase.Evening));
            Assert.That(runtime.GetView().TerminalPowered, Is.True);
            RuntimeScenario.Complete(runtime, PresentationKind.ExitTerminal);
            Assert.That(runtime.GetView().TerminalShuttingDown, Is.True);
            var quota = runtime.GetView().Game.RemainingPurificationQuota;
            var oldToken = runtime.GetView().Cue!.Token;
            RuntimeScenario.Accept(runtime.SetPaused(true));
            runtime = GameRuntime.Restore(runtime.Save(), rules, content);
            Assert.That(runtime.GetView().TerminalShuttingDown, Is.True);
            RuntimeScenario.RejectWithoutMutation(runtime, () => runtime.CompleteCue(runtime.GetView().Cue!.Token));
            RuntimeScenario.Accept(runtime.SetPaused(false));
            RuntimeScenario.RejectWithoutMutation(runtime, () => runtime.CompleteCue(oldToken));
            RuntimeScenario.Complete(runtime, PresentationKind.ExitTerminal);
            Assert.That(runtime.GetView().TerminalPower, Is.EqualTo(TerminalPowerState.Off));
            Assert.That(runtime.GetView().Game.RemainingPurificationQuota, Is.EqualTo(quota));
            RuntimeScenario.RejectWithoutMutation(runtime, () => runtime.LeaveTerminal());
            RuntimeScenario.DriveUntil(runtime, v => v.Game.Day == 2);
            Assert.That(runtime.GetView().TerminalPower, Is.EqualTo(TerminalPowerState.Off));
            RuntimeScenario.EnterApartment(runtime);
            RuntimeScenario.Accept(runtime.TryInteract(InteractionTarget.Computer));
            Assert.That(runtime.GetView().TerminalBootRequired, Is.True);
        }

        [Test]
        public void PowerCheckpointRejectsWrongDayAndOffOpenUiAndMigratesLegacyLitRoom()
        {
            var rules = PrototypeRules.Create(); var content = new PrototypeContent(rules);
            var runtime = new GameRuntime(rules, content, true);
            RuntimeScenario.EnterApartment(runtime);
            RuntimeScenario.Accept(runtime.TryInteract(InteractionTarget.Computer));
            var json = JObject.Parse(runtime.Save());
            json["TerminalPowerDay"] = 99;
            Assert.Throws<SnapshotValidationException>(() => GameRuntime.Restore(json.ToString(), rules, content));
            json = JObject.Parse(runtime.Save()); json["TerminalPower"] = "Off";
            Assert.Throws<SnapshotValidationException>(() => GameRuntime.Restore(json.ToString(), rules, content));
            RuntimeScenario.Complete(runtime, PresentationKind.EnterTerminal);
            RuntimeScenario.Accept(runtime.LeaveTerminal());
            RuntimeScenario.Complete(runtime, PresentationKind.ExitTerminal);
            json = JObject.Parse(runtime.Save());
            json.Remove("TerminalPowerDay"); json.Remove("TerminalPower");
            runtime = GameRuntime.Restore(json.ToString(), rules, content);
            Assert.That(runtime.GetView().TerminalPowered, Is.True);
            Assert.That(runtime.GetView().TerminalOpen, Is.False);
        }

        private static void FinishPatient(GameRuntime runtime)
        {
            RuntimeScenario.Accept(runtime.EnterSelectedPatient(runtime.GetView().Game.CurrentPatientId!));
            if (runtime.GetView().Cue?.Kind == PresentationKind.Introduction)
                RuntimeScenario.Complete(runtime, PresentationKind.Introduction);
            RuntimeScenario.ReachDisposition(runtime);
            var view = runtime.GetView();
            RuntimeScenario.Accept(runtime.SubmitDisposition(view.Game.CurrentEncounterId!,
                RuntimeScenario.NormalPolicy(view.Game.Day, view.Game.CurrentPatientId!)));
            Assert.That(runtime.GetView().TerminalShuttingDown, Is.False);
            RuntimeScenario.Complete(runtime, PresentationKind.Reaction);
        }
    }
}
