using System;
using System.Collections;
using System.IO;
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
    public sealed class SocialAdaptationPlayModeTests
    {
        [UnityTest]
        public IEnumerator TutorialOpeningImmediatelyUsesTheMaximumAgitationReading()
        {
            var asset = Resources.Load<DialoguePackageAsset>("MiniGame/ActiveCampaignPackage");
            var package = MiniGame.Presentation.Authoring.DialoguePackageCompiler.Compile(asset.PackageSource.text);
            var runtime = new GameRuntime(package.Rules, package.Content, true);
            var owner = new GameObject("Day0 maximum agitation");
            var screen = owner.AddComponent<MiniGameScreen>();
            try
            {
                screen.Bind(runtime); screen.UseYarnPresentation();
                Assert.That(runtime.BeginDayZeroOpening().Accepted, Is.True);
                Assert.That(runtime.CompleteCue(runtime.GetView().Cue.Token).Accepted, Is.True);
                var intro = runtime.GetView();
                Assert.That(intro.Cue.Kind, Is.EqualTo(PresentationKind.Introduction));
                foreach (var line in package.Content.GetSegment(intro.Game.CurrentNodeId).Lines.Take(1))
                    screen.SetDialogueLine(line.Speaker, line.Text, line.SourceNodeId, line.PortraitId);
                yield return null; yield return null;
                var wave = screen.GetComponentInChildren<TerminalAdaptationWaveform>(true);
                Assert.That(wave.HasReading, Is.True);
                Assert.That(wave.DisplayCoordinate, Is.Zero);
                Assert.That(wave.SignalParameters.Agitation, Is.EqualTo(1));
                Assert.That(wave.SignalParameters.Amplitude, Is.EqualTo(2.45).Within(.000001));
                Capture(screen.TerminalDisplay, "day0-maximum-agitation");
                runtime = GameRuntime.Restore(runtime.Save(), package.Rules, package.Content);
                screen.Bind(runtime);
                yield return null;
                Assert.That(wave.HasReading, Is.True);
                Assert.That(wave.DisplayCoordinate, Is.Zero);
                Assert.That(runtime.CompleteCue(runtime.GetView().Cue.Token).Accepted, Is.True);
                Assert.That(runtime.GetView().Game.AvailableDispositions, Is.EqualTo(new[] { Disposition.Purify }));
            }
            finally { Object.Destroy(owner); }
            yield return null;
        }

        [UnityTest, Timeout(60000)]
        public IEnumerator RealRepliesDriveTheInstrumentWhileAnswersPickingAndPauseRemainUsable()
        {
            var rules = PrototypeRules.Create();
            var runtime = new GameRuntime(rules, new PrototypeContent(rules), true);
            var owner = new GameObject("Social adaptation integration");
            var screen = owner.AddComponent<MiniGameScreen>();
            try
            {
                screen.Bind(runtime); screen.UseYarnPresentation();
                runtime.TryInteract(InteractionTarget.Door);
                runtime.CompleteCue(runtime.GetView().Cue.Token);
                runtime.CompleteCue(runtime.GetView().Cue.Token);
                runtime.TryInteract(InteractionTarget.Computer);
                runtime.CompleteCue(runtime.GetView().Cue.Token);
                runtime.EnterSelectedPatient(PrototypeRules.FormerRuler);
                while (runtime.GetView().Cue != null) runtime.CompleteCue(runtime.GetView().Cue.Token);
                screen.SetDialogueLine("对象", "我已经回答过你的问题。你还想知道什么？", "adaptation-review", PortraitCatalog.BasePortrait(PrototypeRules.FormerRuler));
                int selected = 0;
                screen.ShowResponseButtons(new[] {
                    new ResponseContent("one", "请继续说。"), new ResponseContent("two", "你还记得什么？"),
                    new ResponseContent("three", "我们慢慢谈。"), new ResponseContent("four", "我明白了。") }, _ => selected++);
                yield return null; yield return null;
                var wave = screen.GetComponentInChildren<TerminalAdaptationWaveform>(true);
                Assert.That(wave, Is.Not.Null);
                Assert.That(wave.HasReading, Is.True);
                Assert.That(wave.SignalParameters.Amplitude, Is.GreaterThan(1));
                var labels = screen.GetComponentsInChildren<Text>(true);
                Assert.That(labels.Single(t => t.name == "SocialAdaptationHeading").text, Is.EqualTo("社会适应度"));
                Assert.That(labels.Any(t => t.name == "SocialAdaptationStatus" || t.name == "SocialAdaptationBandLabel"
                    || t.name == "CommandStatus"), Is.False, "Redundant labels must be removed from the terminal.");
                Assert.That(labels.Single(t => t.name == "DialogueHeading").text, Is.EqualTo("对话"));
                Assert.That(labels.Where(t => t.name.StartsWith("SocialAdaptation")).All(t => !t.text.Any(char.IsDigit)), Is.True);
                Assert.That(labels.Single(t => t.name == "AnswerHeading").gameObject.activeInHierarchy, Is.True);
                var replies = screen.GetComponentsInChildren<Button>().Where(b => b.name.StartsWith("Reply-")).ToArray();
                Assert.That(replies.Length, Is.EqualTo(4));
                Assert.That(replies.All(button => button.interactable), Is.True);
                Assert.That(screen.HandleTerminalKey(KeyCode.Return), Is.True);
                Assert.That(selected, Is.EqualTo(1));
                Assert.That(wave.raycastTarget, Is.False);
                foreach (var label in labels.Where(t => t.name.StartsWith("SocialAdaptation")))
                {
                    var bitmap = label as TerminalBitmapText;
                    Assert.That(bitmap, Is.Not.Null);
                    foreach (char character in label.text.Where(c => !char.IsWhiteSpace(c)))
                        Assert.That(bitmap.ArkTypeface.TryGet(character, label.fontSize, out _), Is.True, "Missing instrument glyph " + character);
                }
                // Use the existing CRT-mapped screen coordinates for the fourth real answer button.
                var hits = new System.Collections.Generic.List<UnityEngine.EventSystems.RaycastResult>();
                var display = screen.TerminalDisplay;
                var pointer = new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current)
                    { position = display.ScreenPointFor((RectTransform)replies[3].transform) };
                UnityEngine.EventSystems.EventSystem.current.RaycastAll(pointer, hits);
                Assert.That(hits.Any(hit => hit.gameObject.GetComponentInParent<Button>() == replies[3]), Is.True);
                Capture(display, "social-adaptation-low");
                var geometry = wave.canvasRenderer.GetMesh();
                Assert.That(geometry.vertexCount, Is.GreaterThan(100));
                AssertWaveformPixels(display);
                yield return CaptureAnimation(display, "low");
                double clock = wave.AnimationClock;
                runtime.SetPaused(true); yield return null;
                double frozen = wave.AnimationClock;
                yield return null; yield return null;
                Assert.That(wave.AnimationClock, Is.EqualTo(frozen));
                runtime.SetPaused(false); yield return null;
                Assert.That(wave.AnimationClock, Is.GreaterThanOrEqualTo(clock));
                screen.HideDialogueActions();
                // Accepted real responses change the core reading; the view never mutates snapshots.
                for (int step = 0; step < 30; step++)
                {
                    var view = runtime.GetView();
                    if (view.Game.EncounterStage == EncounterStage.AwaitingDisposition) break;
                    if (view.Cue != null) Assert.That(runtime.CompleteCue(view.Cue.Token).Accepted, Is.True);
                    else Assert.That(runtime.SelectResponse(view.Game.CurrentEncounterId, view.Game.CurrentNodeId, "empathize").Accepted, Is.True);
                }
                Assert.That(runtime.GetView().Game.AvailableDispositions, Does.Contain(Disposition.Release));
                yield return new WaitForSecondsRealtime(1.25f);
                Assert.That(wave.HasReading, Is.True);
                Assert.That(wave.SignalParameters.Amplitude, Is.LessThanOrEqualTo(1));
                Capture(display, "social-adaptation-passed");
                yield return CaptureAnimation(display, "passed");
                Assert.That(screen.GetComponentsInChildren<Button>(true).Single(b => b.name == "Disposition-Release").interactable, Is.True);
            }
            finally { Object.Destroy(owner); }
            yield return null;
        }

        [UnityTest, Timeout(60000)]
        public IEnumerator PriestReleaseUpdatesTheLiveInstrumentDuringItsReaction()
        {
            var asset = Resources.Load<DialoguePackageAsset>("MiniGame/ActiveCampaignPackage");
            Assert.That(asset, Is.Not.Null);
            var package = MiniGame.Presentation.Authoring.DialoguePackageCompiler.Compile(asset.PackageSource.text);
            var meeting = package.Rules.Days.Single(d => d.Day == 2).Meetings.Single(m => m.PatientId == "priest");
            var patient = new PatientDefinition("priest", "乔纳", 1, 0, 3, PatientPresence.Deceased);
            var rules = new GameRules("priest-reaction-instrument", new[] { patient },
                new[] { new DayDefinition(1, 0, new[] { meeting }) }, package.Rules.EndingPriority);
            var runtime = new GameRuntime(rules, package.Content, true);
            var owner = new GameObject("Priest operation reaction");
            var screen = owner.AddComponent<MiniGameScreen>();
            try
            {
                screen.Bind(runtime); screen.UseYarnPresentation();
                runtime.TryInteract(InteractionTarget.Door);
                runtime.CompleteCue(runtime.GetView().Cue.Token);
                runtime.CompleteCue(runtime.GetView().Cue.Token);
                runtime.TryInteract(InteractionTarget.Computer);
                for (int step = 0; step < 100; step++)
                {
                    var view = runtime.GetView();
                    if (view.Game.EncounterStage == EncounterStage.AwaitingDisposition) break;
                    if (view.Cue != null) Assert.That(runtime.CompleteCue(view.Cue.Token).Accepted, Is.True);
                    else if (view.TerminalPage == TerminalPage.Directory)
                        Assert.That(runtime.EnterSelectedPatient("priest").Accepted, Is.True);
                    else
                    {
                        var round = view.Game.EncounterStage == EncounterStage.SpecialDialogue ? meeting.SpecialRound
                            : meeting.NormalRounds.Single(r => r.NodeId == view.Game.CurrentNodeId);
                        Assert.That(runtime.SelectResponse(view.Game.CurrentEncounterId, view.Game.CurrentNodeId,
                            round.Responses.OrderByDescending(r => r.TrustDelta).First().Id).Accepted, Is.True);
                    }
                }
                yield return new WaitForSecondsRealtime(.4f);
                var wave = screen.GetComponentInChildren<TerminalAdaptationWaveform>(true);
                Assert.That(wave.HasReading, Is.True);
                Assert.That(wave.SignalParameters.Amplitude, Is.LessThanOrEqualTo(1));
                Capture(screen.TerminalDisplay, "priest-before-release");
                double before = runtime.GetView().Game.SocialAdaptation.RelativeReleaseMargin;
                int dialogueTrust = PersistentTrust(runtime, rules, "priest");
                Assert.That(runtime.SubmitDisposition(runtime.GetView().Game.CurrentEncounterId, Disposition.Release).Accepted, Is.True);
                foreach (var line in package.Content.GetSegment(runtime.GetView().Game.CurrentNodeId).Lines)
                    runtime.RecordDialogueLine(line.Speaker, line.Text, line.SourceNodeId, line.PortraitId, line.IsNarration);
                yield return null; yield return null;
                Assert.That(runtime.GetView().Game.EncounterStage, Is.EqualTo(EncounterStage.Reaction));
                Assert.That(runtime.GetView().Game.SocialAdaptation.RelativeReleaseMargin,
                    Is.EqualTo(before - 200d / meeting.ReleaseTrustThreshold).Within(.000001));
                Assert.That(PersistentTrust(runtime, rules, "priest"),
                    Is.EqualTo(dialogueTrust), "Release emotion must not alter persistent dialogue trust.");
                Assert.That(wave.HasReading, Is.True);
                Assert.That(wave.SignalParameters.Amplitude, Is.GreaterThan(1));
                Capture(screen.TerminalDisplay, "priest-after-release");
            }
            finally { Object.Destroy(owner); }
            yield return null;
        }

        [Serializable]
        private sealed class CoreCheckpoint { public string CoreJson = ""; }

        private static int PersistentTrust(GameRuntime runtime, GameRules rules, string patientId)
        {
            var core = SnapshotCodec.Deserialize(JsonUtility.FromJson<CoreCheckpoint>(runtime.Save()).CoreJson, rules);
            return core.CreateSnapshot().State.Patients.Single(p => p.Id == patientId).Trust;
        }

        private static IEnumerator CaptureAnimation(TerminalDisplay display, string state)
        {
            string directory = Environment.GetEnvironmentVariable("MINIGAME_CAPTURE_DIR");
            if (string.IsNullOrEmpty(directory) || Environment.GetEnvironmentVariable("MINIGAME_CAPTURE_ANIMATION") != "1") yield break;
            for (int index = 0; index < 60; index++)
            {
                yield return new WaitForSecondsRealtime(1f / 30);
                Capture(display, state + "/frame-" + index.ToString("D3"));
            }
        }
        private static void Capture(TerminalDisplay display, string name)
        {
            string directory = Environment.GetEnvironmentVariable("MINIGAME_CAPTURE_DIR");
            if (string.IsNullOrEmpty(directory)) return;
            string path = Path.Combine(directory, name + ".png"); Directory.CreateDirectory(Path.GetDirectoryName(path));
            display.RenderFrame(); var previous = RenderTexture.active;
            var image = new Texture2D(800, 600, TextureFormat.RGB24, false);
            try
            {
                RenderTexture.active = display.Texture;
                image.ReadPixels(new Rect(0, 0, 800, 600), 0, 0); image.Apply();
                File.WriteAllBytes(path, image.EncodeToPNG());
            }
            finally { RenderTexture.active = previous; Object.Destroy(image); }
        }

        private static void AssertWaveformPixels(TerminalDisplay display)
        {
            display.RenderFrame(); var previous = RenderTexture.active;
            var image = new Texture2D(248, 44, TextureFormat.RGB24, false);
            try
            {
                RenderTexture.active = display.Texture;
                image.ReadPixels(new Rect(530, 600 - 158, 248, 44), 0, 0); image.Apply();
                Assert.That(image.GetPixels32().Count(pixel => pixel.r > 90 && pixel.g > 40), Is.GreaterThan(100),
                    "The waveform must contribute visible pixels on the actual 800x600 terminal render target.");
            }
            finally { RenderTexture.active = previous; Object.Destroy(image); }
        }
    }
}
