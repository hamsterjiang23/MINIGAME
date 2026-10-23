using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MiniGame.Core;
using MiniGame.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace MiniGame.UnityHost.Tests
{
    [Category("TerminalV0824")]
    public sealed class NormPurificationKpiPlayModeTests
    {
        [UnityTest]
        public IEnumerator DirectoryAndReviewShowAuthoritativeProgressAfterDispositionRestoreAndANewDay()
        {
            var rules = Rules();
            var content = new KpiContent();
            var runtime = new GameRuntime(rules, content, true);
            var owner = new GameObject("NORM purification KPI regression");
            var screen = owner.AddComponent<MiniGameScreen>();
            try
            {
                screen.Bind(runtime); screen.UseYarnPresentation();
                OpenDirectory(runtime);
                AssertKpi(screen, "DirectoryPurificationRequirement", "净化要求：0/1");
                Assert.That(runtime.EnterSelectedPatient("first").Accepted, Is.True);
                Complete(runtime);
                AssertReviewRemaining(screen, runtime, 1);
                string encounter = runtime.GetView().Game.CurrentEncounterId;
                Assert.That(runtime.SubmitDisposition(encounter, Disposition.Purify).Accepted, Is.True);
                AssertReviewRemaining(screen, runtime, 0);
                Assert.That(runtime.SubmitDisposition(encounter, Disposition.Purify).Accepted, Is.False);
                AssertReviewRemaining(screen, runtime, 0);

                runtime = GameRuntime.Restore(runtime.Save(), rules, content);
                screen.Bind(runtime);
                AssertReviewRemaining(screen, runtime, 0);
                Complete(runtime); Complete(runtime);
                Assert.That(runtime.ReturnToDirectory().Accepted, Is.True);
                AssertKpi(screen, "DirectoryPurificationRequirement", "净化要求：1/1");
                Assert.That(runtime.EnterSelectedPatient("second").Accepted, Is.True);
                Complete(runtime);
                Assert.That(runtime.SubmitDisposition(runtime.GetView().Game.CurrentEncounterId, Disposition.Treat).Accepted, Is.True);
                AssertReviewRemaining(screen, runtime, 0);
                Complete(runtime); Complete(runtime); Complete(runtime);
                Assert.That(runtime.TryInteract(InteractionTarget.Bed).Accepted, Is.True);
                Assert.That(runtime.GetView().Cue.Kind, Is.EqualTo(PresentationKind.Sleep));
                Complete(runtime);
                Assert.That(runtime.GetView().Cue.Kind, Is.EqualTo(PresentationKind.Dream));
                Complete(runtime);
                Assert.That(runtime.GetView().Game.Day, Is.EqualTo(2));
                OpenDirectory(runtime);
                AssertKpi(screen, "DirectoryPurificationRequirement", "净化要求：0/2");
                Assert.That(runtime.EnterSelectedPatient("second").Accepted, Is.True);
                Complete(runtime);
                AssertReviewRemaining(screen, runtime, 2);
            }
            finally { Object.Destroy(owner); }
            yield return null;
        }

        private static void AssertKpi(MiniGameScreen screen, string name, string expected)
        {
            var label = screen.GetComponentsInChildren<Text>(true).Single(text => text.name == name);
            Assert.That(label.gameObject.activeInHierarchy, Is.True);
            Assert.That(label.text, Is.EqualTo(expected));
            Assert.That(label.preferredWidth, Is.LessThanOrEqualTo(label.rectTransform.rect.width));
        }

        private static void AssertReviewRemaining(MiniGameScreen screen, GameRuntime runtime, int expected)
        {
            int remaining = Math.Max(0, runtime.GetView().Game.RemainingPurificationQuota);
            Assert.That(remaining, Is.EqualTo(expected));
            var label = screen.GetComponentsInChildren<Text>(true).Single(text => text.name == "DailyWorkIndicatorRemaining");
            Assert.That(label.text, Is.EqualTo(remaining.ToString("00")));
            Assert.That(label.enabled && label.gameObject.activeInHierarchy, Is.False,
                "The authoritative value stays readable without drawing over the supplied quota artwork.");
            var artwork = screen.GetComponentsInChildren<RawImage>(true).Single(image => image.name == "DailyWorkQuotaArtwork");
            Assert.That(artwork.enabled && artwork.gameObject.activeInHierarchy, Is.True);
            Assert.That(artwork.texture, Is.SameAs(Resources.Load<TerminalAssetSet>("MiniGame/TerminalAssets").QuotaFields));
            Assert.That(artwork.uvRect.x, Is.EqualTo(remaining % 10 / 10f).Within(.00001f));
            Assert.That(artwork.uvRect.y, Is.EqualTo(1 - (remaining / 10 + 1) / 10f).Within(.00001f));
            Assert.That(artwork.uvRect.width, Is.EqualTo(.1f).Within(.00001f));
            Assert.That(artwork.uvRect.height, Is.EqualTo(.1f).Within(.00001f));
        }

        private static void Complete(GameRuntime runtime) =>
            Assert.That(runtime.CompleteCue(runtime.GetView().Cue.Token).Accepted, Is.True);

        private static void OpenDirectory(GameRuntime runtime)
        {
            Assert.That(runtime.TryInteract(InteractionTarget.Door).Accepted, Is.True);
            Complete(runtime); Complete(runtime);
            Assert.That(runtime.TryInteract(InteractionTarget.Computer).Accepted, Is.True);
            Complete(runtime);
        }

        private static GameRules Rules()
        {
            var first = new PatientDefinition("first", "小孩", 1, 0, null, PatientPresence.TreatmentEnded);
            var second = new PatientDefinition("second", "医生", 1, 0, null, PatientPresence.TreatmentEnded);
            MeetingDefinition Meeting(string id, int day) => new MeetingDefinition(id, Array.Empty<RoundDefinition>(), null, int.MaxValue, 0,
                new Dictionary<Disposition, string> { [Disposition.Purify] = day + "." + id + ".purify", [Disposition.Treat] = day + "." + id + ".treat" },
                day + "." + id + ".intro");
            return new GameRules("norm-kpi-fixture", new[] { first, second },
                new[] { new DayDefinition(1, 1, new[] { Meeting(first.Id, 1), Meeting(second.Id, 1) }),
                    new DayDefinition(2, 2, new[] { Meeting(second.Id, 2) }) },
                new[] { EndingType.PriestAttack, EndingType.LockedInRoom, EndingType.TakenAway })
                .WithDreamFlow(0, 100, 100, new Dictionary<string, int>());
        }

        private sealed class KpiContent : IGameContent
        {
            public string Revision => "norm-kpi-fixture";
            public DialogueContent GetDialogue(PlayerView view) => new DialogueContent(view.CurrentNodeId, "对象",
                view.EncounterStage == EncounterStage.Reaction ? "处置已登记。" : "会谈开始。", Array.Empty<ResponseContent>());
            public string GetPresentation(PresentationKind kind, PlayerView view) => kind.ToString();
            public InspectionContent GetInspection(string patientId, int day, InspectionKind kind) => new InspectionContent(kind, "观察", "记录");
        }
    }
}
