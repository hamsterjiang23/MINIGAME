using System;
using System.IO;
using System.Linq;
using MiniGame.Core;
using MiniGame.Presentation;
using MiniGame.Presentation.Authoring;
using MiniGame.UnityHost.Editor;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MiniGame.UnityHost.Tests
{
    public sealed class CampaignDebugEditorTests
    {
        private GameRules rules;
        private IGameContent content;

        [SetUp]
        public void SetUp()
        {
            var package = Resources.Load<DialoguePackageAsset>("MiniGame/ActiveCampaignPackage");
            var compiled = DialoguePackageCompiler.Compile(package.PackageSource.text);
            var rebellion = Resources.Load<PlayerRebellionProfile>("MiniGame/PlayerRebellion");
            rules = compiled.Rules.WithDreamFlow(rebellion.InitialValue, rebellion.Maximum,
                rebellion.Threshold, rebellion.CreateDeltas());
            content = compiled.Content;
        }

        [TestCase(0), TestCase(1), TestCase(2), TestCase(3)]
        public void EverySupportedStageRestoresTheRealCampaignAndAcceptsItsNextAction(int day)
        {
            foreach (var point in CampaignDebugCheckpoint.PointsForDay(day))
            {
                string checkpoint = CampaignDebugCheckpoint.Create(rules, content, day, point);
                var runtime = GameRuntime.Restore(checkpoint, rules, content);
                var view = runtime.GetView();
                Assert.That(CampaignDebugCheckpoint.Matches(view, day, point), Is.True, day + "/" + point);
                Assert.That(view.Paused, Is.False);
                RuntimeResult result;
                if (view.Cue != null) result = runtime.CompleteCue(view.Cue.Token);
                else if (point == CampaignDebugPoint.Morning) result = runtime.TryInteract(InteractionTarget.Door);
                else if (point == CampaignDebugPoint.Terminal)
                    result = runtime.EnterSelectedPatient(view.Patients.First(patient => patient.CanReceiveToday).Id);
                else if (point == CampaignDebugPoint.Bedtime || point == CampaignDebugPoint.AfterDream && view.Game.Phase != GamePhase.SelfReview)
                    result = runtime.TryInteract(InteractionTarget.Bed);
                else result = runtime.TryInteract(InteractionTarget.Computer);
                Assert.That(result.Accepted, Is.True, day + "/" + point + ": " + result.Reason);
            }
        }

        [Test]
        public void DayZeroKeepsOpeningAndFirstSleepButHasNoOrdinaryDreamTarget()
        {
            var points = CampaignDebugCheckpoint.PointsForDay(0);
            Assert.That(points, Has.Member(CampaignDebugPoint.Opening));
            Assert.That(points, Has.Member(CampaignDebugPoint.Shutdown));
            Assert.That(points, Has.Member(CampaignDebugPoint.Sleep));
            Assert.That(points, Has.No.Member(CampaignDebugPoint.Dream));
            Assert.Throws<ArgumentException>(() => CampaignDebugCheckpoint.Create(rules, content, 0, CampaignDebugPoint.Dream));
            var sleep = GameRuntime.Restore(CampaignDebugCheckpoint.Create(rules, content, 0, CampaignDebugPoint.Sleep), rules, content);
            Assert.That(sleep.CompleteCue(sleep.GetView().Cue.Token).Accepted, Is.True);
            Assert.That(sleep.GetView().Game.Day, Is.EqualTo(1));
            Assert.That(sleep.GetView().Game.Phase, Is.EqualTo(GamePhase.MorningInspection));
        }

        [TestCase(-1), TestCase(4), TestCase(100)]
        public void UnknownDatesAreRejected(int day)
            => Assert.Throws<ArgumentException>(() => CampaignDebugCheckpoint.Create(rules, content, day, CampaignDebugPoint.Room));

        [Test]
        public void DebugSaveRoutingUsesTheSeparateDirectoryAndRestoresTheNormalPaths()
        {
            string previous = EditorCampaignDebugSession.SaveDirectory;
            var root = new GameObject("Debug save path test");
            try
            {
                var host = root.AddComponent<MiniGameBootstrap>();
                var session = root.AddComponent<MiniGameSessionHost>();
                EditorCampaignDebugSession.SaveDirectory = null;
                string normalHost = host.SavePath, normalSession = session.SavePath;
                string isolated = Path.Combine(Application.temporaryCachePath, "CampaignDayDebugTest");
                EditorCampaignDebugSession.SaveDirectory = isolated;
                Assert.That(Path.GetFullPath(Path.GetDirectoryName(host.SavePath)), Is.EqualTo(Path.GetFullPath(isolated)));
                Assert.That(Path.GetFullPath(Path.GetDirectoryName(session.SavePath)), Is.EqualTo(Path.GetFullPath(isolated)));
                EditorCampaignDebugSession.SaveDirectory = null;
                Assert.That(host.SavePath, Is.EqualTo(normalHost));
                Assert.That(session.SavePath, Is.EqualTo(normalSession));
            }
            finally { EditorCampaignDebugSession.SaveDirectory = previous; Object.DestroyImmediate(root); }
        }
    }
}
