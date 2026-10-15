using System;
using System.IO;
using System.Linq;
using MiniGame.Core;
using MiniGame.Presentation.Authoring;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace MiniGame.Presentation.Tests
{
    public sealed class TerminalDirectoryRuntimeTests
    {
        [Test]
        public void LegacyRepeatedResponseIdsRetainOneEntryPerActualRound()
        {
            var rules = PrototypeRules.Create();
            var content = new PrototypeContent(rules);
            var runtime = new GameRuntime(rules, content);
            runtime.TryInteract(InteractionTarget.Door); Complete(runtime); Complete(runtime);
            runtime.TryInteract(InteractionTarget.Computer); Complete(runtime);
            for (var round = 0; round < 2; round++)
            {
                var view = runtime.GetView();
                Assert.That(runtime.SelectResponse(view.Game.CurrentEncounterId!, view.Game.CurrentNodeId!, "neutral").Accepted, Is.True);
                Complete(runtime);
            }
            Assert.That(runtime.GetView().Transcript, Has.Count.EqualTo(2));
            Assert.That(runtime.GetView().Transcript.Select(entry => entry.CoreNodeId).Distinct().Count(), Is.EqualTo(2));
            runtime = GameRuntime.Restore(runtime.Save(), rules, content);
            Assert.That(runtime.GetView().Transcript, Has.Count.EqualTo(2));
        }

        [Test]
        public void DirectoryEntryAndEarlyLeaveRestoreWithoutStartingOrDuplicatingDialogue()
        {
            var package = Package();
            var runtime = Open(package);
            Assert.That(runtime.GetView().TerminalPage, Is.EqualTo(TerminalPage.Directory));
            Assert.That(runtime.GetView().Input, Is.EqualTo(InputContext.TerminalDirectory));
            Assert.That(runtime.GetView().Dialogue, Is.Null);
            Assert.That(runtime.EnterSelectedPatient("unconfigured").Accepted, Is.False);
            var restored = GameRuntime.Restore(runtime.Save(), package.Rules, package.Content);
            Assert.That(restored.GetView().TerminalPage, Is.EqualTo(TerminalPage.Directory));
            Assert.That(restored.LeaveTerminal().Accepted, Is.True);
            restored = GameRuntime.Restore(restored.Save(), package.Rules, package.Content);
            Assert.That(restored.GetView().LeavingTerminal, Is.True);
            Complete(restored);
            Assert.That(restored.GetView().TerminalOpen, Is.False);
            Assert.That(restored.TryInteract(InteractionTarget.Computer).Accepted, Is.True);
            Complete(restored);
            Assert.That(restored.EnterSelectedPatient("child").Accepted, Is.True);
            Assert.That(restored.GetView().Dialogue!.NodeId, Is.EqualTo(package.Package.Profile.StartNode));
            Assert.That(restored.GetView().Game.RemainingPurificationQuota, Is.EqualTo(5));
        }

        [Test]
        public void TranscriptPersistsActualReplyAndDeduplicatesReplayedPresentation()
        {
            var package = Package();
            var runtime = Open(package);
            runtime.EnterSelectedPatient("child");
            RecordFirstLine(runtime, package);
            RecordFirstLine(runtime, package);
            Assert.That(runtime.GetView().Transcript, Has.Count.EqualTo(1));
            var view = runtime.GetView();
            var response = view.Dialogue!.Responses[0];
            Assert.That(runtime.SelectResponse(view.Game.CurrentEncounterId!, view.Game.CurrentNodeId!, response.Id).Accepted, Is.True);
            RecordFirstLine(runtime, package);
            var expected = runtime.GetView().Transcript.Select(entry => entry.Key).ToArray();
            runtime = GameRuntime.Restore(runtime.Save(), package.Rules, package.Content);
            RecordFirstLine(runtime, package);
            Assert.That(runtime.GetView().Transcript.Select(entry => entry.Key), Is.EqualTo(expected));
            Assert.That(runtime.GetView().Transcript.Single(entry => entry.IsPlayer).Text, Is.EqualTo(response.Text));
            Assert.That(runtime.SelectResponse(view.Game.CurrentEncounterId!, view.Game.CurrentNodeId!, response.Id).Accepted, Is.False);
            Complete(runtime);
            Assert.That(runtime.GetView().Dialogue!.NodeId, Is.Not.EqualTo(view.Game.CurrentNodeId));
        }

        [TestCase(Disposition.Treat, 5)]
        [TestCase(Disposition.Purify, 4)]
        [TestCase(Disposition.Release, 5)]
        public void ReactionArchiveAndReturnNavigationSurviveRestoreWithoutRepeatingDisposition(Disposition disposition, int remaining)
        {
            var package = Package();
            var runtime = Open(package);
            runtime.EnterSelectedPatient("child");
            while (runtime.GetView().Game.EncounterStage != EncounterStage.AwaitingDisposition)
            {
                var view = runtime.GetView();
                var round = package.Rules.Days[0].Meetings[0].NormalRounds.Single(item => item.NodeId == view.Game.CurrentNodeId);
                var response = round.Responses.OrderByDescending(item => item.TrustDelta).First();
                Assert.That(runtime.SelectResponse(view.Game.CurrentEncounterId!, round.NodeId, response.Id).Accepted, Is.True);
                Complete(runtime);
            }
            var encounter = runtime.GetView().Game.CurrentEncounterId!;
            Assert.That(runtime.SubmitDisposition(encounter, disposition).Accepted, Is.True);
            Assert.That(runtime.SubmitDisposition(encounter, disposition).Accepted, Is.False);
            RecordFirstLine(runtime, package);
            runtime = GameRuntime.Restore(runtime.Save(), package.Rules, package.Content);
            Assert.That(runtime.GetView().Game.RemainingPurificationQuota, Is.EqualTo(remaining));
            Complete(runtime); // Reaction commits its forward-only completion and starts signal loss.
            runtime = GameRuntime.Restore(runtime.Save(), package.Rules, package.Content);
            Complete(runtime);
            Assert.That(runtime.GetView().TerminalPage, Is.EqualTo(TerminalPage.Archive));
            Assert.That(runtime.GetView().ArchiveDirectoryVisible, Is.False);
            Assert.That(runtime.GetView().TerminalOpen, Is.True);
            Assert.That(runtime.ReturnToDirectory().Accepted, Is.True);
            runtime = GameRuntime.Restore(runtime.Save(), package.Rules, package.Content);
            Assert.That(runtime.GetView().ArchiveDirectoryVisible, Is.True);
            Assert.That(runtime.EnterSelectedPatient("child").Accepted, Is.False);
            Assert.That(runtime.GetView().Game.RemainingPurificationQuota, Is.EqualTo(remaining));
            Assert.That(runtime.GetView().Transcript.Count(entry => entry.SourceNodeId.StartsWith("disposition:", StringComparison.Ordinal)), Is.EqualTo(1));
            Assert.That(runtime.LeaveTerminal().Accepted, Is.True);
            runtime = GameRuntime.Restore(runtime.Save(), package.Rules, package.Content);
            Complete(runtime);
            Assert.That(runtime.GetView().TerminalPage, Is.EqualTo(TerminalPage.Closed));
            Assert.That(runtime.GetView().AllowedInteractions, Is.EqualTo(new[] { InteractionTarget.Bed }));
        }

        [Test]
        public void PauseAndWrongPageDoNotAddHistoryOrLeaveAnActiveEncounter()
        {
            var package = Package();
            var runtime = Open(package);
            runtime.RecordDialogueLine("米洛", "not presented", "D1_R1_OPEN1", "ML01");
            Assert.That(runtime.GetView().Transcript, Is.Empty);
            runtime.EnterSelectedPatient("child");
            Assert.That(runtime.LeaveTerminal().Accepted, Is.False);
            Assert.That(runtime.ReturnToDirectory().Accepted, Is.False);
            runtime.SetPaused(true);
            RecordFirstLine(runtime, package);
            Assert.That(runtime.GetView().Transcript, Is.Empty);
            runtime.SetPaused(false);
            RecordFirstLine(runtime, package);
            Assert.That(runtime.GetView().Transcript, Has.Count.EqualTo(1));
        }

        [Test]
        public void RestoreRejectsFabricatedRepliesAndInvalidArchiveNavigation()
        {
            var package = Package();
            var runtime = Open(package);
            runtime.EnterSelectedPatient("child");
            RecordFirstLine(runtime, package);
            var json = JObject.Parse(runtime.Save());
            json["Transcript"]![0]!["IsPlayer"] = true;
            json["Transcript"]![0]!["Key"] = runtime.GetView().Game.CurrentEncounterId + ":player:D1_R1_OPEN1";
            Assert.Throws<SnapshotValidationException>(() => GameRuntime.Restore(json.ToString(), package.Rules, package.Content));
            json = JObject.Parse(runtime.Save());
            json["ArchiveDirectoryVisible"] = true;
            Assert.Throws<SnapshotValidationException>(() => GameRuntime.Restore(json.ToString(), package.Rules, package.Content));
        }

        private static GameRuntime Open(CompiledDialoguePackage package)
        {
            var runtime = new GameRuntime(package.Rules, package.Content, useTerminalDirectory: true);
            Assert.That(runtime.TryInteract(InteractionTarget.Door).Accepted, Is.True);
            Complete(runtime); Complete(runtime);
            Assert.That(runtime.TryInteract(InteractionTarget.Computer).Accepted, Is.True);
            Complete(runtime);
            return runtime;
        }

        private static void RecordFirstLine(GameRuntime runtime, CompiledDialoguePackage package)
        {
            var view = runtime.GetView();
            var line = package.Content.GetSegment(view.Game.CurrentNodeId!).Lines.First();
            runtime.RecordDialogueLine(line.Speaker, line.Text, line.SourceNodeId, line.PortraitId);
        }

        private static void Complete(GameRuntime runtime)
            => Assert.That(runtime.CompleteCue(runtime.GetView().Cue!.Token).Accepted, Is.True);

        private static CompiledDialoguePackage Package()
        {
            var directory = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
            while (directory != null)
            {
                var path = Path.Combine(directory.FullName, "UnityProject", "Assets", "MiniGame", "Content", "TerminalDemo", "terminal-day1.minigame.json");
                if (File.Exists(path)) return DialoguePackageCompiler.Compile(File.ReadAllText(path));
                directory = directory.Parent;
            }
            throw new FileNotFoundException("The checked-in canonical terminal package fixture is missing.");
        }
    }
}
