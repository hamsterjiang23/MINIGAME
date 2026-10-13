using System;
using System.IO;
using System.Linq;
using MiniGame.Core;
using MiniGame.Presentation;
using NUnit.Framework;

namespace MiniGame.Presentation.Tests
{
    [TestFixture]
    public sealed class RuntimeFileSaveStoreTests
    {
        [Test]
        public void SaveReplacementRestoresReactionWithoutRepeatingDispositionAndLeavesNoTemporaryFiles()
        {
            WithDirectory(directory =>
            {
                var rules = PrototypeRules.Create();
                var content = new PrototypeContent(rules);
                var runtime = new GameRuntime(rules, content);
                var store = new RuntimeFileSaveStore(Path.Combine(directory, "nested", "session.json"));
                store.Save(runtime);
                Assert.That(store.Load(rules, content).Save(), Is.EqualTo(runtime.Save()));
                RuntimeScenario.OpenTerminal(runtime);
                RuntimeScenario.ReachDisposition(runtime);
                var encounter = runtime.GetView().Game.CurrentEncounterId!;
                RuntimeScenario.Accept(runtime.SubmitDisposition(encounter, Disposition.Purify));
                var oldToken = runtime.GetView().Cue!.Token;
                store.Save(runtime);
                var restored = store.Load(rules, content);
                Assert.That(restored.Save(), Is.EqualTo(runtime.Save()));
                RuntimeScenario.RejectWithoutMutation(restored, () => restored.CompleteCue(oldToken));
                RuntimeScenario.RejectWithoutMutation(restored, () => restored.SubmitDisposition(encounter, Disposition.Purify));
                var completion = RuntimeScenario.Complete(restored, PresentationKind.Reaction);
                Assert.That(completion.Events.Any(item => item.Kind == GameEventKind.DispositionCommitted), Is.False);
                Assert.That(restored.GetView().Game.RemainingPurificationQuota, Is.Zero);
                Assert.That(Directory.GetFiles(Path.GetDirectoryName(store.FilePath)!).Select(Path.GetFileName),
                    Is.EqualTo(new[] { "session.json" }));
                var bytes = File.ReadAllBytes(store.FilePath);
                Assert.That(bytes.Take(3), Is.Not.EqualTo(new byte[] { 0xEF, 0xBB, 0xBF }));
            });
        }

        [Test]
        public void MissingSaveReportsFileNotFoundWithoutCreatingAnUnrequestedNewGame()
        {
            WithDirectory(directory =>
            {
                var rules = PrototypeRules.Create();
                var store = new RuntimeFileSaveStore(Path.Combine(directory, "missing.json"));
                Assert.Throws<FileNotFoundException>(() => store.Load(rules, new PrototypeContent(rules)));
                Assert.That(File.Exists(store.FilePath), Is.False);
            });
        }

        [Test]
        public void CorruptFileReportsSnapshotValidationFailure()
        {
            WithDirectory(directory =>
            {
                var rules = PrototypeRules.Create();
                var store = new RuntimeFileSaveStore(Path.Combine(directory, "corrupt.json"));
                File.WriteAllText(store.FilePath, "{}");
                Assert.Throws<SnapshotValidationException>(() => store.Load(rules, new PrototypeContent(rules)));
                Assert.That(File.ReadAllText(store.FilePath), Is.EqualTo("{}"));
            });
        }

        [Test]
        [Platform("Win")]
        public void FailedReplacementOfALockedFilePreservesThePreviousCheckpoint()
        {
            WithDirectory(directory =>
            {
                var runtime = RuntimeScenario.Create();
                var store = new RuntimeFileSaveStore(Path.Combine(directory, "session.json"));
                store.Save(runtime);
                var previous = File.ReadAllText(store.FilePath);
                RuntimeScenario.OpenTerminal(runtime);
                using (var locked = new FileStream(store.FilePath, FileMode.Open, FileAccess.Read, FileShare.None))
                    Assert.Throws<IOException>(() => store.Save(runtime));
                Assert.That(File.ReadAllText(store.FilePath), Is.EqualTo(previous));
                Assert.That(Directory.GetFiles(directory).Select(Path.GetFileName), Is.EqualTo(new[] { "session.json" }));
            });
        }

        private static void WithDirectory(Action<string> test)
        {
            var directory = Path.Combine(Path.GetTempPath(), "minigame-runtime-save-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            try { test(directory); }
            finally { Directory.Delete(directory, true); }
        }
    }
}
