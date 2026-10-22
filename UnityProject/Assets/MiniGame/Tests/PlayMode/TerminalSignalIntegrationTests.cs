using System;
using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace MiniGame.UnityHost.Tests
{
    public sealed class TerminalSignalIntegrationTests
    {
        private MiniGameBootstrap host;

        [UnitySetUp]
        public IEnumerator LoadOpening()
        {
            MiniGameSessionHost.Release();
            yield return SceneManager.LoadSceneAsync(MiniGameSessionHost.CombinedScene, LoadSceneMode.Single);
            yield return null;
            host = Object.FindObjectOfType<MiniGameBootstrap>();
            Assert.That(host, Is.Not.Null);
            host.Player.enabled = false;
            yield return Until(() => host.Screen.DayZeroOpeningWaiting);
        }

        [UnityTearDown]
        public IEnumerator Cleanup()
        {
            if (host != null)
            {
                host.Screen.SignalTuning.SetPanelOpen(false);
                host.enabled = false;
                host.Dreams?.PrepareForRuntimeChange();
                host.Effects?.PrepareForRuntimeChange();
            }
            MiniGameSessionHost.Release();
            var scene = SceneManager.GetActiveScene();
            var empty = SceneManager.CreateScene("Signal cleanup " + Guid.NewGuid().ToString("N"));
            SceneManager.SetActiveScene(empty);
            if (scene.IsValid() && scene.isLoaded) yield return SceneManager.UnloadSceneAsync(scene);
            Cursor.lockState = CursorLockMode.None; Cursor.visible = true;
            yield return null;
        }

        [UnityTest, Timeout(30000)]
        public IEnumerator OpeningLogoPlaysOneSoundPerBurstAndStopsAfterOpening()
        {
            var controls = host.Screen.SignalTuning;
            controls.Profile.Logo.Enabled = true;
            controls.Profile.Logo.Intensity = 1;
            controls.Profile.Logo.BurstFrequency = 2;
            controls.Profile.Logo.BurstDuration = .15f;
            int sounds = 0;
            host.Audio.Service.SoundPlayed += (channel, sound) =>
            {
                if (sound != MiniGameSound.LogoGlitch) return;
                Assert.That(channel, Is.EqualTo(MiniGameAudioChannel.LogoGlitch));
                Assert.That(controls.Profile.Logo.LogoBurstIndex(controls.Clock), Is.GreaterThanOrEqualTo(0));
                sounds++;
            };
            Assert.That(host.Audio.Profile.GetClip(MiniGameSound.LogoGlitch), Is.Not.Null);
            yield return Until(() => sounds > 0);
            controls.FreezePreview = true;
            int frozenCount = sounds;
            for (int index = 0; index < 5; index++) { controls.ApplySettings(); yield return null; }
            yield return new WaitForSecondsRealtime(.6f);
            Assert.That(sounds, Is.EqualTo(frozenCount), "A frozen burst must not replay its sound.");
            controls.FreezePreview = false;
            host.Runtime.SetPaused(true);
            yield return new WaitForSecondsRealtime(.6f);
            Assert.That(sounds, Is.EqualTo(frozenCount));
            host.Runtime.SetPaused(false);
            yield return Until(() => sounds > frozenCount);
            int resumedCount = sounds;
            yield return new WaitForSecondsRealtime(.52f);
            Assert.That(sounds - resumedCount, Is.EqualTo(1), "One burst must produce one sound, not one per frame.");
            Assert.That(host.Screen.RequestDayZeroOpening(), Is.True);
            yield return Until(() => !host.Screen.GetComponentsInChildren<TerminalSignalEffect>(true).Any(effect => effect.LogoMode));
            int finishedCount = sounds;
            yield return new WaitForSecondsRealtime(.6f);
            Assert.That(sounds, Is.EqualTo(finishedCount));
            Assert.That(host.Audio.Service.GetSource(MiniGameAudioChannel.LogoGlitch).isPlaying, Is.False);
        }

        [UnityTest, Timeout(30000)]
        public IEnumerator PanelSlidersEditIndependentGroupsAndBlockOpeningInput()
        {
            var controls = host.Screen.SignalTuning;
            var defaults = Resources.Load<TerminalSignalProfile>("MiniGame/TerminalSignalDefault");
            Assert.That(defaults, Is.Not.Null);
            float originalDefault = defaults.Logo.Intensity;
            Assert.That(controls.Profile, Is.Not.SameAs(defaults));
            var effects = host.Screen.GetComponentsInChildren<TerminalSignalEffect>(true);
            Assert.That(effects.Length, Is.EqualTo(3));
            var logo = effects.Single(effect => effect.name == "Day0SupervisorFeed");
            Assert.That(logo.LogoMode, Is.True);
            Assert.That(effects.All(effect => effect.EffectMaterial != null && effect.EffectMaterial.shader.isSupported), Is.True);

            controls.SetPanelOpen(true);
            Assert.That(controls.Panel.gameObject.activeInHierarchy, Is.True);
            Assert.That(controls.Panel.transform.GetSiblingIndex(), Is.EqualTo(controls.Panel.transform.parent.childCount - 1));
            var intensity = controls.Panel.GetComponentsInChildren<Slider>(true)
                .Single(slider => slider.transform.parent.name == "总强度");
            intensity.value = .37f;
            Assert.That(controls.Profile.Logo.Intensity, Is.EqualTo(.37f).Within(.001));
            Assert.That(logo.EffectMaterial.GetFloat("_Intensity"), Is.EqualTo(.37f).Within(.001));
            Assert.That(defaults.Logo.Intensity, Is.EqualTo(originalDefault));
            Assert.That(host.Screen.RequestDayZeroOpening(), Is.False);
            Assert.That(host.Screen.HandleTerminalKey(KeyCode.Return), Is.False);
            Assert.That(host.Screen.DayZeroOpeningWaiting, Is.True);
            Assert.That(host.Runtime.GetView().Transcript, Is.Empty);

            controls.Panel.GetComponentsInChildren<Button>(true).Single(button => button.name == "MonitorTab").onClick.Invoke();
            intensity.value = .28f;
            Assert.That(controls.Profile.Monitor.Intensity, Is.EqualTo(.28f).Within(.001));
            Assert.That(logo.EffectMaterial.GetFloat("_Intensity"), Is.EqualTo(.37f).Within(.001));
            foreach (var monitor in effects.Where(effect => effect != logo))
                Assert.That(monitor.EffectMaterial.GetFloat("_Intensity"), Is.EqualTo(.28f).Within(.001));

            controls.SetPanelOpen(false);
            Assert.That(controls.Panel.gameObject.activeSelf, Is.False);
            Assert.That(host.Screen.RequestDayZeroOpening(), Is.False, "Closing a control must consume the same-frame click.");
            yield return null;
            Assert.That(host.Screen.RequestDayZeroOpening(), Is.True);
            controls.SetPanelOpen(true);
            yield return Until(() => !logo.LogoMode && host.Screen.GetComponentsInChildren<Button>()
                .Any(button => button.name == "Continue" && button.gameObject.activeInHierarchy));
            Assert.That(logo.EffectMaterial.GetFloat("_Intensity"), Is.EqualTo(.28f).Within(.001));
            Assert.That(logo.GetComponent<RawImage>().texture, Is.SameAs(Resources.Load<Texture2D>("MiniGame/Day0Supervisor")));
            Assert.That(host.Screen.HandleTerminalKey(KeyCode.Return), Is.False);
            controls.SetPanelOpen(false);
            yield return null;
            Assert.That(host.Screen.HandleTerminalKey(KeyCode.Return), Is.True,
                "Dialogue presented during tuning must remain usable after the panel closes.");
        }

        [UnityTest, Timeout(30000)]
        public IEnumerator PauseAndPreviewFreezeStopTheClockWhileLiveEditsStillApply()
        {
            var controls = host.Screen.SignalTuning;
            float before = controls.Clock;
            yield return null;
            yield return null;
            Assert.That(controls.Clock, Is.GreaterThan(before));
            controls.FreezePreview = true;
            float frozen = controls.Clock;
            yield return null;
            yield return null;
            Assert.That(controls.Clock, Is.EqualTo(frozen));
            controls.FreezePreview = false;
            host.Runtime.SetPaused(true);
            frozen = controls.Clock;
            yield return null;
            yield return null;
            Assert.That(controls.Clock, Is.EqualTo(frozen));
            controls.Profile.Logo.Enabled = false;
            controls.ApplySettings();
            var logo = host.Screen.GetComponentsInChildren<TerminalSignalEffect>(true).Single(effect => effect.LogoMode);
            Assert.That(logo.EffectMaterial.GetFloat("_Intensity"), Is.Zero);
            host.Runtime.SetPaused(false);
            yield return null;
            yield return null;
            Assert.That(controls.Clock, Is.GreaterThan(frozen));
        }

        private static IEnumerator Until(Func<bool> condition)
        {
            float deadline = Time.realtimeSinceStartup + 15;
            while (!condition() && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.That(condition(), Is.True, "Expected presentation state did not arrive.");
        }
    }
}
