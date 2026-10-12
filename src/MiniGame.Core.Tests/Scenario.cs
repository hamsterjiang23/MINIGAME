using System;
using System.Linq;
using MiniGame.Core;
using NUnit.Framework;

namespace MiniGame.Core.Tests
{
    internal static class Scenario
    {
        internal static CommandResult Accept(GameSession session, GameCommand command)
        {
            var result = session.Execute(command);
            Assert.That(result.Accepted, Is.True, $"{command.GetType().Name}: {result.Reason}");
            return result;
        }

        internal static void RejectWithoutMutation(GameSession session, GameCommand? command, RejectionReason? reason = null)
        {
            var before = SnapshotCodec.Serialize(session);
            var result = session.Execute(command);
            Assert.That(result.Accepted, Is.False);
            Assert.That(result.Events, Is.Empty);
            if (reason.HasValue) Assert.That(result.Reason, Is.EqualTo(reason.Value));
            Assert.That(SnapshotCodec.Serialize(session), Is.EqualTo(before));
        }

        internal static void EnterWork(GameSession session)
        {
            Accept(session, new CompleteMorningInspection());
            Accept(session, new CompleteBriefing());
            Accept(session, new Interact(InteractionTarget.Computer));
        }

        internal static void Reply(GameSession session, string response)
        {
            var view = session.GetPlayerView();
            Accept(session, new SelectResponse(view.CurrentEncounterId!, view.CurrentNodeId!, response));
            view = session.GetPlayerView();
            Accept(session, new CompleteResponseBranch(view.CurrentEncounterId!, view.CurrentNodeId!));
        }

        internal static void ReachDisposition(GameSession session, string response = "neutral")
        {
            for (var guard = 0; guard < 20; guard++)
            {
                var view = session.GetPlayerView();
                if (view.EncounterStage == EncounterStage.AwaitingDisposition) return;
                if (view.EncounterStage == EncounterStage.ResponseBranch)
                    Accept(session, new CompleteResponseBranch(view.CurrentEncounterId!, view.CurrentNodeId!));
                else Reply(session, response);
            }
            Assert.Fail("Dialogue did not reach disposition.");
        }

        internal static void NextMorning(GameSession session)
        {
            Assert.That(session.GetPlayerView().Phase, Is.EqualTo(GamePhase.Evening));
            Accept(session, new Interact(InteractionTarget.Bed));
            Accept(session, new CompleteSleep());
            Accept(session, new CompleteDream());
            Accept(session, new Interact(InteractionTarget.Bed));
        }

        internal static Disposition NormalPolicy(int day, string patientId)
        {
            var target = day == 1 ? PrototypeRules.Child : day == 2 ? PrototypeRules.FormerRuler : PrototypeRules.Doctor;
            return patientId == target ? Disposition.Purify : Disposition.Treat;
        }

        internal static void Step(GameSession session, Func<int, string, Disposition> policy, string response = "neutral")
        {
            var view = session.GetPlayerView();
            switch (view.Phase)
            {
                case GamePhase.MorningInspection: Accept(session, new CompleteMorningInspection()); break;
                case GamePhase.Briefing: Accept(session, new CompleteBriefing()); break;
                case GamePhase.Apartment: Accept(session, new Interact(InteractionTarget.Computer)); break;
                case GamePhase.Evening:
                case GamePhase.AfterDream: Accept(session, new Interact(InteractionTarget.Bed)); break;
                case GamePhase.Sleeping: Accept(session, new CompleteSleep()); break;
                case GamePhase.Dream: Accept(session, new CompleteDream()); break;
                case GamePhase.Work:
                    switch (view.EncounterStage)
                    {
                        case EncounterStage.NormalDialogue:
                        case EncounterStage.SpecialDialogue:
                            Accept(session, new SelectResponse(view.CurrentEncounterId!, view.CurrentNodeId!, response)); break;
                        case EncounterStage.ResponseBranch:
                            Accept(session, new CompleteResponseBranch(view.CurrentEncounterId!, view.CurrentNodeId!)); break;
                        case EncounterStage.AwaitingDisposition:
                            Accept(session, new SubmitDisposition(view.CurrentEncounterId!, policy(view.Day, view.CurrentPatientId!))); break;
                        case EncounterStage.Reaction: Accept(session, new CompleteReaction(view.CurrentEncounterId!)); break;
                        default: throw new InvalidOperationException("Unexpected work checkpoint.");
                    }
                    break;
                default: throw new InvalidOperationException("No step is available.");
            }
        }

        internal static void DriveUntil(GameSession session, Func<PlayerView, bool> stop,
            Func<int, string, Disposition>? policy = null, string response = "neutral")
        {
            for (var guard = 0; guard < 1000; guard++)
            {
                if (stop(session.GetPlayerView())) return;
                Step(session, policy ?? NormalPolicy, response);
            }
            Assert.Fail("Scenario exceeded its command limit.");
        }

        internal static GameRules SinglePatient(int initialTrust = 0, int? treatmentLimit = 3,
            PatientPresence outcome = PatientPresence.Deceased, int delay = 0,
            int quota = 0, int days = 3, int normalRounds = 4)
        {
            var patient = new PatientDefinition("fixture-patient", "测试病人", 1, initialTrust, treatmentLimit, outcome, delay);
            return new GameRules("fixture-v1", new[] { patient }, Enumerable.Range(1, days).Select(day =>
                new DayDefinition(day, quota, new[] { PrototypeRules.CreateMeeting(patient.Id, day, normalRounds) })),
                new[] { EndingType.PriestAttack, EndingType.LockedInRoom, EndingType.TakenAway });
        }
    }
}
