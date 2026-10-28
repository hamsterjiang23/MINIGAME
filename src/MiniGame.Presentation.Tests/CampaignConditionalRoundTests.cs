using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using MiniGame.Core;
using MiniGame.Presentation.Authoring;
using Newtonsoft.Json;
using NUnit.Framework;

namespace MiniGame.Presentation.Tests
{
    [TestFixture]
    public sealed class CampaignConditionalRoundTests
    {
        [TestCase(2, 0, false)]
        [TestCase(2, 1, true)]
        [TestCase(3, -1, false)]
        [TestCase(3, 0, true)]
        public void SpecialRoundUsesTrustAfterFinalOrdinaryAnswer(int initialTrust, int ordinaryDelta, bool expectedSpecial)
        {
            var compiled = Compile(Fixture(initialTrust, ordinaryDelta, 2));
            var meeting = compiled.Rules.Days.Single().Meetings.Single();
            Assert.That(meeting.NormalRounds.Select(round => round.NodeId), Is.EqualTo(new[] { "FIRST" }));
            Assert.That(meeting.SpecialRound!.NodeId, Is.EqualTo("SPECIAL_INTRO"));
            Assert.That(meeting.SpecialTrustThreshold, Is.EqualTo(3));
            Assert.That(compiled.Content.GetSegment("FIRST").Lines, Is.Empty, "The first source node is a player choice.");
            Assert.That(compiled.Content.GetSegment("FIRST_OPTION.branch").Lines.Select(line => line.SourceNodeId),
                Is.EqualTo(new[] { "FIRST_LINE" }), "High-trust dialogue must not leak into the ordinary response.");
            Assert.That(compiled.Content.GetSegment("SPECIAL_INTRO").Lines.Select(line => line.SourceNodeId),
                Is.EqualTo(new[] { "SPECIAL_INTRO" }));

            var session = Start(compiled);
            Reply(session, "FIRST_OPTION");
            Assert.That(session.GetPlayerView().EncounterStage,
                Is.EqualTo(expectedSpecial ? EncounterStage.SpecialDialogue : EncounterStage.AwaitingDisposition));
            Assert.That(session.CreateSnapshot().State.Patients.Single().Trust, Is.EqualTo(initialTrust + ordinaryDelta));
            if (expectedSpecial)
            {
                Assert.That(session.GetPlayerView().CurrentNodeId, Is.EqualTo("SPECIAL_INTRO"));
                Reply(session, "SPECIAL_OPTION");
                Assert.That(session.GetPlayerView().EncounterStage, Is.EqualTo(EncounterStage.AwaitingDisposition));
            }
        }

        [TestCase(1, false)]
        [TestCase(2, true)]
        public void ReleaseUsesPerMeetingThresholdAfterSpecialAnswer(int specialDelta, bool expectedRelease)
        {
            var compiled = Compile(Fixture(3, 0, specialDelta));
            var session = Start(compiled);
            Reply(session, "FIRST_OPTION");
            Reply(session, "SPECIAL_OPTION");
            Assert.That(compiled.Rules.Days.Single().Meetings.Single().ReleaseTrustThreshold, Is.EqualTo(5));
            Assert.That(session.GetPlayerView().AvailableDispositions.Contains(Disposition.Release), Is.EqualTo(expectedRelease));
        }

        [Test]
        public void OmittedMeetingReleaseThresholdUsesCampaignDefault()
        {
            var package = Fixture(3, 0, 0);
            package.Campaign!.ReleaseThreshold = 3;
            package.Campaign.Days.Single().Meetings.Single().ReleaseThreshold = null;
            var compiled = Compile(package);
            Assert.That(compiled.Rules.Days.Single().Meetings.Single().ReleaseTrustThreshold, Is.EqualTo(3));
            var session = Start(compiled);
            Reply(session, "FIRST_OPTION");
            Reply(session, "SPECIAL_OPTION");
            Assert.That(session.GetPlayerView().AvailableDispositions, Does.Contain(Disposition.Release));
        }

        [Test]
        public void LargePositiveAndNegativeSourceDeltasArePreservedWithoutClamping()
        {
            var compiled = Compile(Fixture(0, 100, -107));
            var meeting = compiled.Rules.Days.Single().Meetings.Single();
            Assert.That(meeting.NormalRounds.Single().Responses.Single().TrustDelta, Is.EqualTo(100));
            Assert.That(meeting.SpecialRound!.Responses.Single().TrustDelta, Is.EqualTo(-107));
            var session = Start(compiled);
            Reply(session, "FIRST_OPTION");
            Assert.That(session.CreateSnapshot().State.Patients.Single().Trust, Is.EqualTo(100));
            Reply(session, "SPECIAL_OPTION");
            Assert.That(session.CreateSnapshot().State.Patients.Single().Trust, Is.EqualTo(-7));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void PartialSpecialMetadataIsRejected(bool omitEntry)
        {
            var package = Fixture(0, 0, 0);
            var meeting = package.Campaign!.Days.Single().Meetings.Single();
            if (omitEntry) meeting.SpecialRoundStartNode = null;
            else meeting.SpecialTrustThreshold = null;
            AssertFailure(package, "special-metadata");
        }

        [TestCase(int.MaxValue, 1)]
        [TestCase(int.MinValue, -1)]
        public void DispositionEmotionCannotOverflowPersistentDialogueBounds(int trust, int emotion)
        {
            var package = Fixture(trust, 0, 0);
            string source = package.YarnSource.Replace("#id:RELEASE_OPTION #trust:0",
                "#id:RELEASE_OPTION #trust:" + emotion.ToString(CultureInfo.InvariantCulture));
            package.YarnSource = source;
            package.Campaign!.Days.Single().Meetings.Single().YarnSource = source;
            Assert.DoesNotThrow(() => Compile(package));
        }

        [TestCase("MISSING", "special-entry")]
        [TestCase("MEASURE", "special-entry")]
        [TestCase("FIRST", "special-position")]
        public void SpecialEntryMustDescribeTheWholeFinalChoiceGroup(string entry, string error)
        {
            var package = Fixture(0, 0, 0);
            package.Campaign!.Days.Single().Meetings.Single().SpecialRoundStartNode = entry;
            AssertFailure(package, error);
        }

        [Test]
        public void SpecialRoundMayStartDirectlyWithAPlayerChoice()
        {
            var package = Fixture(3, 0, 0);
            var meeting = package.Campaign!.Days.Single().Meetings.Single();
            meeting.SpecialRoundStartNode = "SPECIAL_CHOICE";
            meeting.YarnSource = package.YarnSource = meeting.YarnSource
                .Replace(Node("SPECIAL_INTRO", "line", "R1", "病人: 只对信任者说的话。\n<<jump SPECIAL_CHOICE>>"), "")
                .Replace("<<jump SPECIAL_INTRO>>", "<<jump SPECIAL_CHOICE>>");
            var compiled = Compile(package);
            Assert.That(compiled.Content.GetSegment("SPECIAL_CHOICE").Lines, Is.Empty);
            var session = Start(compiled);
            Reply(session, "FIRST_OPTION");
            Assert.That(session.GetPlayerView().CurrentNodeId, Is.EqualTo("SPECIAL_CHOICE"));
        }

        [Test]
        public void SpecialRoundCannotPrecedeAnotherOrdinaryChoice()
        {
            var package = Fixture(0, 0, 0);
            var meeting = package.Campaign!.Days.Single().Meetings.Single();
            var source = meeting.YarnSource.Replace("<<jump MEASURE>>", "<<jump EXTRA>>") +
                Node("EXTRA", "choice", "R2", "-> 后续 #id:EXTRA_OPTION #trust:0\n    <<jump MEASURE>>");
            meeting.YarnSource = package.YarnSource = source;
            AssertFailure(package, "special-position");
        }

        [TestCase(int.MaxValue, 1)]
        [TestCase(int.MinValue, -1)]
        public void ReachableSpecialResponseOverflowIsRejected(int initialTrust, int specialDelta)
        {
            var package = Fixture(initialTrust, 0, specialDelta);
            package.Campaign!.Days.Single().Meetings.Single().SpecialTrustThreshold = initialTrust;
            AssertFailure(package, "trust-overflow");
        }

        [Test]
        public void UnreachableSpecialResponseCannotOverflowLowTrustPath()
        {
            var compiled = Compile(Fixture(-1, 0, int.MinValue));
            var session = Start(compiled);
            Reply(session, "FIRST_OPTION");
            Assert.That(session.GetPlayerView().EncounterStage, Is.EqualTo(EncounterStage.AwaitingDisposition));
            Assert.That(session.CreateSnapshot().State.Patients.Single().Trust, Is.EqualTo(-1));
        }

        [Test]
        public void ChangedSpecialOrReleaseThresholdInvalidatesContentRevision()
        {
            var package = Fixture(0, 0, 0);
            var original = Compile(package);
            var meeting = package.Campaign!.Days.Single().Meetings.Single();
            meeting.SpecialTrustThreshold = 4;
            var changedSpecial = Compile(package);
            Assert.That(changedSpecial.SourceHash, Is.Not.EqualTo(original.SourceHash));
            Assert.That(changedSpecial.Content.Revision, Is.Not.EqualTo(original.Content.Revision));
            meeting.SpecialTrustThreshold = 3;
            meeting.ReleaseThreshold = 6;
            var changedRelease = Compile(package);
            Assert.That(changedRelease.Rules.Fingerprint, Is.Not.EqualTo(original.Rules.Fingerprint));
            Assert.That(changedRelease.Content.Revision, Is.Not.EqualTo(original.Content.Revision));
        }

        private static void AssertFailure(DialoguePackage package, string code)
        {
            var error = Assert.Throws<DialoguePackageException>(() => Compile(package));
            Assert.That(error!.Diagnostics.Single().Code, Is.EqualTo(code));
        }

        private static CompiledDialoguePackage Compile(DialoguePackage package)
            => DialoguePackageCompiler.Compile(JsonConvert.SerializeObject(package));

        private static GameSession Start(CompiledDialoguePackage compiled)
        {
            var session = new GameSession(compiled.Rules);
            Accept(session, new CompleteMorningInspection());
            Accept(session, new CompleteBriefing());
            Accept(session, new Interact(InteractionTarget.Computer));
            return session;
        }

        private static void Accept(GameSession session, GameCommand command)
        {
            var result = session.Execute(command);
            Assert.That(result.Accepted, Is.True, result.Reason.ToString());
        }

        private static void Reply(GameSession session, string responseId)
        {
            var view = session.GetPlayerView();
            Accept(session, new SelectResponse(view.CurrentEncounterId!, view.CurrentNodeId!, responseId));
            view = session.GetPlayerView();
            Accept(session, new CompleteResponseBranch(view.CurrentEncounterId!, view.CurrentNodeId!));
        }

        private static string Node(string id, string kind, string round, string body)
            => $"title: {id}\nkind: {kind}\nround: {round}\npatient: patient\nday: 1\n---\n{body}\n===\n";

        private static DialoguePackage Fixture(int initialTrust, int ordinaryDelta, int specialDelta)
        {
            var source =
                Node("FIRST", "choice", "R1", "-> 普通问题 #id:FIRST_OPTION #trust:" +
                    ordinaryDelta.ToString(CultureInfo.InvariantCulture) + "\n    <<jump FIRST_LINE>>") +
                Node("FIRST_LINE", "line", "R1", "病人: 普通回答。\n<<jump SPECIAL_INTRO>>") +
                Node("SPECIAL_INTRO", "line", "R1", "病人: 只对信任者说的话。\n<<jump SPECIAL_CHOICE>>") +
                Node("SPECIAL_CHOICE", "choice", "R1", "-> 深入提问 #id:SPECIAL_OPTION #trust:" +
                    specialDelta.ToString(CultureInfo.InvariantCulture) + "\n    <<jump SPECIAL_RESPONSE>>") +
                Node("SPECIAL_RESPONSE", "line", "R1", "病人: 特殊回答。\n<<jump MEASURE>>") +
                Node("MEASURE", "measure", "M", "-> 释放 #id:RELEASE_OPTION #trust:0 #disposition:release\n    <<jump RELEASE>>\n" +
                    "-> 净化 #id:PURIFY_OPTION #trust:0 #disposition:purify\n    <<jump PURIFY>>") +
                Node("RELEASE", "line", "M", "病人: 释放回应。\n<<jump END>>") +
                Node("PURIFY", "line", "M", "病人: 净化回应。\n<<jump END>>");
            return new DialoguePackage
            {
                FormatVersion = 2, PackageId = "conditional-fixture", ContentVersion = "1", YarnSource = source,
                Campaign = new CampaignDefinition
                {
                    ReleaseThreshold = int.MaxValue,
                    Patients = new List<CampaignPatient>
                    {
                        new CampaignPatient
                        {
                            Id = "patient", Name = "病人", FirstDay = 1, InitialTrust = initialTrust,
                            TreatmentLimit = 3, TreatmentOutcome = "Deceased"
                        }
                    },
                    Days = new List<CampaignDay>
                    {
                        new CampaignDay
                        {
                            Day = 1, Quota = 0,
                            Meetings = new List<CampaignMeeting>
                            {
                                new CampaignMeeting
                                {
                                    PatientId = "patient", StartNode = "FIRST", EndNode = "END", YarnSource = source,
                                    SpecialRoundStartNode = "SPECIAL_INTRO", SpecialTrustThreshold = 3, ReleaseThreshold = 5
                                }
                            }
                        }
                    }
                }
            };
        }
    }
}
