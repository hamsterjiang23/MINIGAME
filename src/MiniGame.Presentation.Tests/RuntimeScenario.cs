using System;
using MiniGame.Core;
using MiniGame.Presentation;
using NUnit.Framework;

namespace MiniGame.Presentation.Tests
{
    internal static class RuntimeScenario
    {
        internal static GameRuntime Create()
        {
            var rules = PrototypeRules.Create();
            return new GameRuntime(rules, new PrototypeContent(rules));
        }

        internal static RuntimeResult Accept(RuntimeResult result)
        {
            Assert.That(result.Accepted, Is.True, result.Reason);
            return result;
        }

        internal static void RejectWithoutMutation(GameRuntime runtime, Func<RuntimeResult> action)
        {
            var before = runtime.Save();
            var beforeCue = runtime.GetView().Cue;
            var result = action();
            Assert.That(result.Accepted, Is.False);
            Assert.That(result.Reason, Is.Not.Empty);
            Assert.That(result.Events, Is.Empty);
            Assert.That(runtime.Save(), Is.EqualTo(before));
            Assert.That(runtime.GetView().Cue?.Token, Is.EqualTo(beforeCue?.Token));
        }

        internal static RuntimeResult Complete(GameRuntime runtime, PresentationKind expected)
        {
            var cue = runtime.GetView().Cue;
            Assert.That(cue, Is.Not.Null);
            Assert.That(cue!.Kind, Is.EqualTo(expected));
            return Accept(runtime.CompleteCue(cue.Token));
        }

        internal static void EnterApartment(GameRuntime runtime)
        {
            Accept(runtime.TryInteract(InteractionTarget.Door));
            Complete(runtime, PresentationKind.MorningInspection);
            Complete(runtime, PresentationKind.Briefing);
            Assert.That(runtime.GetView().Game.Phase, Is.EqualTo(GamePhase.Apartment));
        }

        internal static void OpenTerminal(GameRuntime runtime)
        {
            if (runtime.GetView().Game.Phase == GamePhase.MorningInspection) EnterApartment(runtime);
            Accept(runtime.TryInteract(InteractionTarget.Computer));
            Complete(runtime, PresentationKind.EnterTerminal);
            Assert.That(runtime.GetView().Input, Is.EqualTo(InputContext.Dialogue));
        }

        internal static void Reply(GameRuntime runtime, string response)
        {
            var game = runtime.GetView().Game;
            Accept(runtime.SelectResponse(game.CurrentEncounterId!, game.CurrentNodeId!, response));
            Complete(runtime, PresentationKind.ResponseBranch);
        }

        internal static void ReachDisposition(GameRuntime runtime, string response = "neutral")
        {
            for (var guard = 0; guard < 20; guard++)
            {
                if (runtime.GetView().Game.EncounterStage == EncounterStage.AwaitingDisposition) return;
                Reply(runtime, response);
            }
            Assert.Fail("Dialogue did not reach its disposition checkpoint.");
        }

        internal static Disposition NormalPolicy(int day, string patientId)
        {
            var target = day == 1 ? PrototypeRules.Child : day == 2 ? PrototypeRules.FormerRuler : PrototypeRules.Doctor;
            return patientId == target ? Disposition.Purify : Disposition.Treat;
        }

        internal static RuntimeResult Step(GameRuntime runtime, Func<int, string, Disposition> policy, string response)
        {
            var view = runtime.GetView();
            if (view.Cue != null) return Accept(runtime.CompleteCue(view.Cue.Token));
            switch (view.Game.Phase)
            {
                case GamePhase.MorningInspection:
                    return Accept(runtime.TryInteract(InteractionTarget.Door));
                case GamePhase.Apartment:
                    return Accept(runtime.TryInteract(InteractionTarget.Computer));
                case GamePhase.Evening:
                case GamePhase.AfterDream:
                    return Accept(runtime.TryInteract(InteractionTarget.Bed));
                case GamePhase.Work:
                    if (!view.TerminalOpen) return Accept(runtime.TryInteract(InteractionTarget.Computer));
                    if (view.Game.EncounterStage == EncounterStage.AwaitingDisposition)
                        return Accept(runtime.SubmitDisposition(view.Game.CurrentEncounterId!,
                            policy(view.Game.Day, view.Game.CurrentPatientId!)));
                    return Accept(runtime.SelectResponse(view.Game.CurrentEncounterId!, view.Game.CurrentNodeId!, response));
                default:
                    throw new InvalidOperationException($"No host action is available in {view.Game.Phase}.");
            }
        }

        internal static void DriveUntil(GameRuntime runtime, Func<RuntimeView, bool> stop,
            Func<int, string, Disposition>? policy = null, string response = "neutral")
        {
            for (var guard = 0; guard < 1000; guard++)
            {
                if (stop(runtime.GetView())) return;
                Step(runtime, policy ?? NormalPolicy, response);
            }
            Assert.Fail("Runtime scenario exceeded its command limit.");
        }
    }
}
