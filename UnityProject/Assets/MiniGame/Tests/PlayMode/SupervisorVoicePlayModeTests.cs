using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using MiniGame.Core;
using MiniGame.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace MiniGame.UnityHost.Tests
{
    public sealed class SupervisorVoicePlayModeTests
    {
        private GameObject root;
        private MiniGameAudioService service;
        private SupervisorVoicePlayer voice;
        private SupervisorVoiceProfile settings;

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (root != null) Object.Destroy(root);
            if (settings != null) Object.Destroy(settings);
            MiniGameSessionHost.Release();
            yield return null;
        }

        [Test]
        public void EveryAuthoredSupervisorLineHasExactDurationAndFiniteUnclippedPcm()
        {
            var profile = Resources.Load<SupervisorVoiceProfile>(SupervisorVoiceProfile.ResourcePath);
            Assert.That(profile, Is.Not.Null);
            Assert.That(profile.Grains.Length, Is.EqualTo(10));
            var lines = SupervisorDialogueContent.Load().conversations.SelectMany(day => day.lines).ToArray();
            foreach (var line in lines)
            {
                var clip = SupervisorVoiceSynthesizer.CreateClip(profile, line.text, 30);
                try
                {
                    Assert.That(clip, Is.Not.Null, line.id);
                    int count = StringInfo.ParseCombiningCharacters(line.text).Length;
                    Assert.That(clip.samples, Is.EqualTo(Mathf.RoundToInt(count / 30f * clip.frequency)), line.id);
                    var pcm = new float[clip.samples]; Assert.That(clip.GetData(pcm, 0), Is.True);
                    Assert.That(pcm.All(value => !float.IsNaN(value) && !float.IsInfinity(value) && Mathf.Abs(value) <= .821f), Is.True, line.id);
                    Assert.That(pcm[0], Is.Zero); Assert.That(pcm[pcm.Length - 1], Is.Zero);
                    if (line.text.Any(char.IsLetterOrDigit)) Assert.That(pcm.Sum(value => (double)value * value) / pcm.Length, Is.GreaterThan(.0001), line.id);
                }
                finally { if (clip != null) Object.Destroy(clip); }
            }
            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("MINIGAME_SUPERVISOR_VOICE_PREVIEW")))
            {
                var exporter = AppDomain.CurrentDomain.GetAssemblies().Select(assembly =>
                    assembly.GetType("MiniGame.UnityHost.Editor.SupervisorVoicePreview")).First(type => type != null);
                exporter.GetMethod("ExportActualLines").Invoke(null, null);
            }
        }

        [Test]
        public void VoiceIsDeterministicAndLimitedToSupervisorSpeech()
        {
            var profile = Resources.Load<SupervisorVoiceProfile>(SupervisorVoiceProfile.ResourcePath);
            var a = SupervisorVoiceSynthesizer.CreateClip(profile, "请保持原地，不要回头。", 30);
            var b = SupervisorVoiceSynthesizer.CreateClip(profile, "请保持原地，不要回头。", 30);
            var first = new float[a.samples]; var second = new float[b.samples];
            a.GetData(first, 0); b.GetData(second, 0);
            Assert.That(second, Is.EqualTo(first), "Restoring the same line must not randomly change its voice.");
            Object.Destroy(a); Object.Destroy(b);
            Assert.That(SupervisorVoiceSynthesizer.IsSupervisorLine("supervisor__P02_D1_R1_COMMON_001"), Is.True);
            Assert.That(SupervisorVoiceSynthesizer.IsSupervisorLine("supervisor-ending__P02_D4_R1_COMMON_001"), Is.True);
            foreach (string id in new[] { "priest-ending__001", "self-review__001", "tutorial__P00_D0_R1_COMMON_001", "child__001", "" })
                Assert.That(SupervisorVoiceSynthesizer.IsSupervisorLine(id), Is.False, id);
            Assert.That(SupervisorVoiceSynthesizer.IsSupervisorLine("supervisor__001", true), Is.False);
            Assert.That(SupervisorVoiceSynthesizer.CreateClip(profile, "  ", 30), Is.Null);
            var hesitation = SupervisorVoiceSynthesizer.CreateClip(profile, "……", 30);
            var pcm = new float[hesitation.samples]; hesitation.GetData(pcm, 0);
            Assert.That(pcm.Any(value => Mathf.Abs(value) > .01f), Is.True, "The requested dense texture continues across ellipses.");
            Object.Destroy(hesitation);
        }

        [Test]
        public void DenseVoiceDoesNotLeavePunctuationOrBreathGaps()
        {
            var profile = Resources.Load<SupervisorVoiceProfile>(SupervisorVoiceProfile.ResourcePath);
            var clip = SupervisorVoiceSynthesizer.CreateClip(profile, "保持原地，……继续检查。……不要停下，……保持原地。", 8);
            try
            {
                var pcm = new float[clip.samples]; clip.GetData(pcm, 0);
                int window = clip.frequency / 100;
                int quiet = 0, longest = 0;
                for (int start = window * 2; start + window < pcm.Length - window * 2; start += window)
                {
                    double energy = 0;
                    for (int i = start; i < start + window; i++) energy += (double)pcm[i] * pcm[i];
                    quiet = Math.Sqrt(energy / window) < .004 ? quiet + 1 : 0;
                    longest = Math.Max(longest, quiet);
                }
                Assert.That(longest, Is.LessThanOrEqualTo(2), "Continuous speech must not contain audible blank gaps longer than 20 ms.");
            }
            finally { Object.Destroy(clip); }
        }

        [UnityTest]
        public IEnumerator PauseResumeMenuAndNaturalCompletionUseAudioCursorWithoutChangingMusic()
        {
            CreateAudioHost();
            service.SetLoop(MiniGameAudioChannel.GameplayMusic, MiniGameSound.Gameplay, 0);
            var music = service.GetSource(MiniGameAudioChannel.GameplayMusic);
            var musicClip = music.clip;
            voice.BeginPage("请保持原地，不要回头。请保持原地，不要回头。", 8);
            var source = service.GetSource(MiniGameAudioChannel.SupervisorVoice);
            Assert.That(source.isPlaying, Is.True);
            var raw = SupervisorVoiceSynthesizer.CreateClip(Resources.Load<SupervisorVoiceProfile>(SupervisorVoiceProfile.ResourcePath),
                "请保持原地，不要回头。请保持原地，不要回头。", 8);
            var dry = new float[raw.samples]; var amplified = new float[voice.CurrentClip.samples];
            raw.GetData(dry, 0); voice.CurrentClip.GetData(amplified, 0);
            double dryPower = dry.Sum(value => (double)value * value);
            double outputPower = amplified.Sum(value => (double)value * value) * source.volume * source.volume;
            Assert.That(Math.Sqrt(outputPower / dryPower) / 1.44, Is.EqualTo(.8).Within(.001),
                "The shipped voice must be 80% of the previous 1.44 gain, even though AudioSource.volume is capped at one.");
            Assert.That(amplified.Max(value => Mathf.Abs(value)), Is.LessThan(1));
            Object.Destroy(raw);
            yield return new WaitForSecondsRealtime(.15f);
            Assert.That(voice.TryGetProgress(out float before), Is.True); Assert.That(before, Is.GreaterThan(0));
            voice.SetPaused(true);
            int cursor = source.timeSamples;
            yield return new WaitForSecondsRealtime(.15f);
            Assert.That(source.timeSamples, Is.EqualTo(cursor));
            voice.TryGetProgress(out float held); Assert.That(held, Is.EqualTo(before));
            Assert.That(music.isPlaying && music.clip == musicClip, Is.True);
            voice.SetPaused(false);
            yield return new WaitForSecondsRealtime(.1f);
            Assert.That(source.timeSamples, Is.GreaterThan(cursor));
            service.SetMenuSuspended(true);
            cursor = source.timeSamples;
            yield return new WaitForSecondsRealtime(.15f);
            Assert.That(source.timeSamples, Is.EqualTo(cursor));
            voice.BeginPage("你就是新来的审查员？", 30);
            Assert.That(source.isPlaying, Is.False, "A new line under the menu must remain pending.");
            service.SetMenuSuspended(false);
            float deadline = Time.realtimeSinceStartup + 3;
            do { yield return null; voice.TryGetProgress(out held); } while (held < 1 && Time.realtimeSinceStartup < deadline);
            Assert.That(held, Is.EqualTo(1), "Natural completion must reveal the final character even when Unity resets its sample cursor.");
            Assert.That(music.isPlaying && music.clip == musicClip, Is.True);
            voice.Stop();
            Assert.That(source.clip, Is.Null);
            Assert.That(music.isPlaying && music.clip == musicClip, Is.True);
        }

        [UnityTest]
        public IEnumerator ActualMorningVoiceTracksTextPausesAndStopsOnSkipAndRebind()
        {
            MiniGameSessionHost.Release();
            yield return SceneManager.LoadSceneAsync("MainRoomCombined");
            yield return null;
            var host = Object.FindObjectOfType<MiniGameBootstrap>();
            host.MainMenu.TryDismissForEditorDebug(); host.StartCampaign();
            RestoreFirstMorning(host);
            host.Player.enabled = false; host.Screen.DialogueCharactersPerSecond = 5;
            Assert.That(host.Player.ViewCamera.GetComponent<DoorViewCamera>().TryEnter(), Is.True);
            var source = host.Audio.Service.GetSource(MiniGameAudioChannel.SupervisorVoice);
            voice = host.GetComponent<SupervisorVoicePlayer>();
            float deadline = Time.realtimeSinceStartup + 10;
            while ((voice.CurrentClip == null || !source.isPlaying || host.Screen.TransitionLocked)
                && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.That(voice.CurrentClip, Is.Not.Null, host.GetComponent<MiniGameYarnBridge>().Diagnostics);
            var body = host.Screen.GetComponentsInChildren<Text>(true).Single(text => text.name == "DialogueBody");
            var firstLine = SupervisorDialogueContent.Load().conversations[0].lines[0].text;
            Assert.That(voice.CurrentClip.samples, Is.EqualTo(Mathf.RoundToInt(firstLine.Length / 5f * source.clip.frequency)));
            source.timeSamples = source.clip.samples / 2;
            yield return null;
            Assert.That(body.text.Length, Is.EqualTo(Mathf.FloorToInt(firstLine.Length * source.timeSamples / (float)source.clip.samples)).Within(1));
            Assert.That(host.Runtime.SetPaused(true).Accepted, Is.True);
            int cursor = source.timeSamples; string displayed = body.text;
            yield return new WaitForSecondsRealtime(.15f);
            Assert.That(source.timeSamples, Is.EqualTo(cursor)); Assert.That(body.text, Is.EqualTo(displayed));
            Assert.That(host.Runtime.SetPaused(false).Accepted, Is.True);
            yield return null; yield return null;
            host.Screen.SetMainMenuCovered(true); cursor = source.timeSamples;
            yield return new WaitForSecondsRealtime(.12f);
            Assert.That(source.timeSamples, Is.EqualTo(cursor));
            host.Screen.SetMainMenuCovered(false); yield return null; yield return null;
            Assert.That(host.Screen.AdvanceWorldDialogue(), Is.True);
            Assert.That(host.Screen.LineIsRevealed, Is.True); Assert.That(source.isPlaying, Is.False);
            Assert.That(voice.CurrentClip, Is.Null); Assert.That(body.text, Is.EqualTo(firstLine));
            yield return null;
            Assert.That(host.Screen.AdvanceWorldDialogue(), Is.True);
            deadline = Time.realtimeSinceStartup + 3;
            while (voice.CurrentClip == null && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.That(voice.CurrentClip, Is.Not.Null, "Next real Yarn line starts a fresh phrase.");
            host.Screen.Bind(host.Runtime);
            Assert.That(voice.CurrentClip, Is.Null); Assert.That(source.isPlaying, Is.False);
            host.GetComponent<MiniGameYarnBridge>().enabled = false;
            host.Screen.PresentDialogueLine("监督员", "请跟我走。", "supervisor-ending__P02_D4_R1_COMMON_001", "JD_01", false, () => { });
            Assert.That(voice.CurrentClip, Is.Not.Null, "The authored ending presenter uses the same voice path.");
            host.Screen.PresentDialogueLine("神父", "请跟我走。", "priest-ending__001", "", false, () => { });
            Assert.That(voice.CurrentClip, Is.Null, "Priest dialogue must stop supervisor voice without starting another one.");
            host.Screen.DialogueCharactersPerSecond = 30;
            string longLine = string.Concat(Enumerable.Repeat("请保持原地，不要回头。", 60));
            int completeCount = 0;
            host.Screen.PresentDialogueLine("监督员", longLine, "supervisor__pagination-fixture", "JD_01", false, () => completeCount++);
            Assert.That(host.Screen.CurrentLinePageCount, Is.GreaterThan(1));
            var pageOneClip = voice.CurrentClip;
            yield return null;
            Assert.That(host.Screen.AdvanceWorldDialogue(), Is.True); // reveal first page
            Assert.That(voice.CurrentClip, Is.Null);
            yield return null;
            Assert.That(host.Screen.AdvanceWorldDialogue(), Is.True); // begin second page
            Assert.That(voice.CurrentClip, Is.Not.Null);
            Assert.That(voice.CurrentClip, Is.Not.SameAs(pageOneClip));
            Assert.That(completeCount, Is.Zero, "Turning a page must not complete the logical line.");
            host.Screen.HideDialogueActions(); Assert.That(source.isPlaying, Is.False);
        }

        [UnityTest]
        public IEnumerator MissingOrDisabledVoiceFallsBackWithoutLeakingClips()
        {
            CreateAudioHost();
            settings = Object.Instantiate(Resources.Load<SupervisorVoiceProfile>(SupervisorVoiceProfile.ResourcePath));
            settings.VoiceEnabled = false; voice.Initialize(service, settings);
            voice.BeginPage("请保持原地。", 30);
            Assert.That(voice.CurrentClip, Is.Null); Assert.That(voice.TryGetProgress(out _), Is.False);
            settings.VoiceEnabled = true; settings.Grains = Array.Empty<AudioClip>();
            voice.BeginPage("请保持原地。", 30); Assert.That(voice.CurrentClip, Is.Null);
            voice.Initialize(service); voice.BeginPage("请保持原地。", 2);
            Object.Destroy(voice); yield return null;
            Assert.That(service.GetSource(MiniGameAudioChannel.SupervisorVoice).clip, Is.Null);
        }

        private void CreateAudioHost()
        {
            root = new GameObject("Supervisor voice regression");
            if (Object.FindObjectOfType<AudioListener>() == null) root.AddComponent<AudioListener>();
            service = root.AddComponent<MiniGameAudioService>();
            service.Configure(Resources.Load<MiniGameAudioProfile>("MiniGame/AudioDefault"));
            voice = root.AddComponent<SupervisorVoicePlayer>(); voice.Initialize(service);
        }

        private static void RestoreFirstMorning(MiniGameBootstrap host)
        {
            var session = MiniGameSessionHost.Current;
            var runtime = new GameRuntime(session.Rules, session.Content, useTerminalDirectory: true);
            runtime.ConfigureEndings(host.Runtime.Endings);
            Assert.That(runtime.BeginDayZeroOpening().Accepted, Is.True);
            for (int step = 0; step < 64; step++)
            {
                var view = runtime.GetView();
                if (view.Game.Day == 1 && view.Game.Phase == GamePhase.MorningInspection && view.Cue == null && !view.TerminalOpen)
                { host.RestoreCheckpoint(runtime.Save()); return; }
                RuntimeResult result;
                if (view.Cue != null) result = runtime.CompleteCue(view.Cue.Token);
                else if (view.TerminalOpen && view.Game.EncounterStage == EncounterStage.AwaitingDisposition)
                    result = runtime.SubmitDisposition(view.Game.CurrentEncounterId, Disposition.Purify);
                else if (view.Dialogue != null && view.Dialogue.Responses.Count > 0)
                    result = runtime.SelectResponse(view.Game.CurrentEncounterId, view.Game.CurrentNodeId, view.Dialogue.Responses[0].Id);
                else if (view.AllowedInteractions.Contains(InteractionTarget.Bed)) result = runtime.TryInteract(InteractionTarget.Bed);
                else { Assert.Fail("Unsupported morning preparation state."); return; }
                Assert.That(result.Accepted, Is.True, result.Reason);
            }
            Assert.Fail("Morning preparation did not complete.");
        }
    }
}
