using System;
using System.Collections;
using System.Linq;
using MiniGame.Core;
using MiniGame.Presentation;
using MiniGame.Presentation.Authoring;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace MiniGame.UnityHost.Tests
{
    [Category("InteractionWakeGm")]
    public sealed class DreamBedLightingPlayModeTests
    {
        private MiniGameBootstrap host;
        private BedtimeGuidance guidance;
        private RoomLightingController controller;
        private RoomLightingProfile sourceDay, sourceNight, day, night;
        private string sourceDayJson, sourceNightJson;
        private Light ordinaryLight;
        private Renderer terminalScreen;
        private GameRules rules;
        private IGameContent content;
        private EndingDialogueCatalog endings;
        private bool previousSkipStartup;

        [UnitySetUp]
        public IEnumerator OpenRoom()
        {
            previousSkipStartup = MainMenuController.SkipStartup;
            MainMenuController.SkipStartup = true;
            MiniGameSessionHost.Release();
            yield return null;
            yield return SceneManager.LoadSceneAsync(MiniGameSessionHost.CombinedScene, LoadSceneMode.Single);
            yield return null;
            host = Object.FindObjectOfType<MiniGameBootstrap>();
            Assert.That(host, Is.Not.Null);
            Assert.That(host.Runtime, Is.Not.Null, "Lighting tests require the campaign, not the startup menu.");
            Assert.That(host.Dreams, Is.Not.Null, "The production Combined dream binding must be installed.");
            endings = host.Runtime.Endings;
            Assert.That(endings, Is.Not.Null, "Fixture checkpoints must use the production ending content revision.");
            guidance = host.GetComponent<BedtimeGuidance>();
            controller = Object.FindObjectsOfType<RoomLightingController>()
                .Single(item => item.gameObject.scene == host.gameObject.scene);
            terminalScreen = Object.FindObjectsOfType<Renderer>()
                .Single(item => item.gameObject.scene == host.gameObject.scene && item.name == "CRT screen emission");
            // This fixture owns only lighting and its runtime clock; film/camera integration is tested separately.
            host.Dreams.PrepareForRuntimeChange();
            host.Dreams.enabled = false;
            host.Effects.PrepareForRuntimeChange();
            host.Effects.enabled = false;
            host.Player.enabled = false;
            host.enabled = false;
            sourceDay = controller.Profile; sourceNight = controller.NightProfile;
            Assert.That(controller.HasIndependentProfiles, Is.True);
            sourceDayJson = JsonUtility.ToJson(sourceDay); sourceNightJson = JsonUtility.ToJson(sourceNight);
            day = Object.Instantiate(sourceDay); night = Object.Instantiate(sourceNight);
            day.ambientColor = new Color(.3f, .25f, .2f); day.ambientIntensity = 1;
            night.ambientColor = new Color(.08f, .06f, .04f); night.ambientIntensity = 1;
            night.reflectionIntensity = .2f; night.emissionMultiplier = .5f; night.lightMultiplier = 1;
            day.fadeSeconds = night.fadeSeconds = .05f;
            controller.ConfigureProfiles(day, night);
            ordinaryLight = controller.WindowLight;
            Assert.That(ordinaryLight, Is.Not.Null);
            SetLight(night, ordinaryLight, RoomLightingPeriod.Night, true, .7f);
            SetLight(night, guidance.Spotlight, RoomLightingPeriod.Night, false, .1f);
            SetLight(day, guidance.Spotlight, RoomLightingPeriod.Day, false, .1f);
            day.NotifyChanged(); night.NotifyChanged(); controller.ApplyFromProfile();
            var asset = Resources.Load<DialoguePackageAsset>("MiniGame/ActiveCampaignPackage");
            var package = DialoguePackageCompiler.Compile(asset.PackageSource.text);
            var rebellion = Resources.Load<PlayerRebellionProfile>("MiniGame/PlayerRebellion");
            rules = package.Rules.WithDreamFlow(rebellion.InitialValue, rebellion.Maximum, rebellion.Threshold, rebellion.CreateDeltas());
            content = package.Content;
        }

        [UnityTearDown]
        public IEnumerator CloseRoom()
        {
            MainMenuController.SkipStartup = previousSkipStartup;
            if (controller != null)
            {
                controller.SetPreviewMode(RoomLightingPreviewMode.Automatic);
                controller.ConfigureProfiles(sourceDay, sourceNight);
            }
            if (sourceDay != null) Assert.That(JsonUtility.ToJson(sourceDay), Is.EqualTo(sourceDayJson));
            if (sourceNight != null) Assert.That(JsonUtility.ToJson(sourceNight), Is.EqualTo(sourceNightJson));
            MiniGameSessionHost.Release();
            var current = SceneManager.GetActiveScene();
            var empty = SceneManager.CreateScene("Dream bed lighting cleanup " + Guid.NewGuid().ToString("N"));
            SceneManager.SetActiveScene(empty);
            if (current.IsValid() && current.isLoaded) yield return SceneManager.UnloadSceneAsync(current);
            if (day != null) Object.Destroy(day);
            if (night != null) Object.Destroy(night);
        }

        [UnityTest, Timeout(60000)]
        public IEnumerator EveryDreamRestoresWithNoBedEffectsWhilePausedAndPreservesLightingPreviews()
        {
            var fixture = CreateCampaign();
            for (int currentDay = 1; currentDay <= 3; currentDay++)
            {
                DriveTo(fixture, view => view.Game.Day == currentDay && view.Game.Phase == GamePhase.Dream);
                Assert.That(fixture.SetPaused(true).Accepted, Is.True);
                var restored = BindLightingRuntime(fixture);
                AssertDarkRoom(); AssertNoBedEffects();
                string saved = restored.Save();
                float visualTime = guidance.Atmosphere.VisualTime;
                yield return new WaitForSecondsRealtime(.1f);
                AssertDarkRoom(); AssertNoBedEffects();
                Assert.That(guidance.Atmosphere.VisualTime, Is.EqualTo(visualTime));
                guidance.enabled = false; guidance.enabled = true;
                AssertDarkRoom(); AssertNoBedEffects();

                controller.SetPreviewMode(RoomLightingPreviewMode.Night);
                AssertOrdinaryNight();
                controller.SetPreviewMode(RoomLightingPreviewMode.Day);
                Assert.That(RenderSettings.ambientLight, Is.EqualTo(day.AmbientLightColor));
                Assert.That(guidance.Spotlight.enabled, Is.False);
                controller.SetPreviewMode(RoomLightingPreviewMode.Automatic);
                AssertDarkRoom(); AssertNoBedEffects();
                Assert.That(restored.Save(), Is.EqualTo(saved), "Light previews and pause restoration must not wake the player.");
                Assert.That(fixture.SetPaused(false).Accepted, Is.True);
            }
        }

        [UnityTest, Timeout(30000)]
        public IEnumerator OrdinaryDreamReturnNeverEnablesBedEffectsIncludingPausedRebind()
        {
            var fixture = CreateCampaign();
            DriveTo(fixture, view => view.Game.Day == 1 && view.Game.Phase == GamePhase.Dream);
            Assert.That(fixture.SetPaused(true).Accepted, Is.True);
            fixture = BindLightingRuntime(fixture);
            AssertDarkRoom();
            Assert.That(guidance.Spotlight.enabled, Is.False);
            Assert.That(guidance.Atmosphere.BeamRenderer.enabled, Is.False);
            string checkpoint = fixture.Save();
            guidance.SetDreamReturnReveal(true);
            AssertDarkRoom(); AssertNoBedEffects();
            Assert.That(fixture.GetView().Game.Phase, Is.EqualTo(GamePhase.Dream));
            Assert.That(fixture.Save(), Is.EqualTo(checkpoint), "The reveal must not consume the dream cue or wake the player.");
            yield return new WaitForSecondsRealtime(.1f);
            AssertDarkRoom(); AssertNoBedEffects();
            guidance.SetDreamReturnReveal(false);
            AssertDarkRoom();
            Assert.That(guidance.Spotlight.enabled, Is.False);
            Assert.That(guidance.Atmosphere.BeamRenderer.enabled, Is.False);

            guidance.SetDreamReturnReveal(true);
            BindLightingRuntime(GameRuntime.Restore(checkpoint, rules, content, endings));
            AssertDarkRoom();
            Assert.That(guidance.Spotlight.enabled, Is.False, "A new Dream binding must wait for its own returning cue.");
            guidance.SetDreamReturnReveal(true);
            AssertDarkRoom(); AssertNoBedEffects();
            BindLightingRuntime(CreateCampaign());
            Assert.That(RenderSettings.ambientLight, Is.EqualTo(day.AmbientLightColor));
            Assert.That(guidance.Spotlight.enabled, Is.False);
            Assert.That(guidance.Atmosphere.BeamRenderer.enabled, Is.False);
        }

        [UnityTest, Timeout(30000)]
        public IEnumerator BeforeTheFirstSleepEachEveningUsesTheOrdinaryNightProfile()
        {
            var fixture = CreateCampaign();
            for (int currentDay = 1; currentDay <= 3; currentDay++)
            {
                DriveTo(fixture, view => view.Game.Day == currentDay && view.Game.Phase == GamePhase.Evening
                    && !view.TerminalOpen && view.Cue == null);
                fixture = BindLightingRuntime(fixture);
                AssertOrdinaryNight();
                Assert.That(fixture.GetView().AllowedInteractions, Is.EqualTo(new[] { InteractionTarget.Bed }));
                yield return null;
            }
        }

        [UnityTest, Timeout(60000)]
        public IEnumerator FinalDreamReturnsDirectlyToTerminalAcrossEveryRevealFrameAndPausedRestore()
        {
            var fixture = CreateCampaign();
            EndingCampaignScenario.Drive(fixture, rules, EndingCampaignRoute.Hidden,
                view => view.Game.Day == 3 && view.Game.Phase == GamePhase.Dream);
            Assert.That(fixture.GetView().Game.DreamReturnPhase, Is.EqualTo(GamePhase.SelfReview));
            BindLightingRuntime(fixture);
            host.Dreams.UseExternalClock = true;
            host.Dreams.enabled = true;
            var profile = host.DreamProfile;
            host.Dreams.Advance(profile.BlackScreenSeconds);
            host.Dreams.Advance(profile.GetDay(3).Duration);
            Assert.That(host.Dreams.Phase, Is.EqualTo(DreamPlaybackPhase.Returning));
            while (host.Dreams.Phase == DreamPlaybackPhase.Returning)
            {
                host.Dreams.Advance(profile.ReturnSeconds / 20f);
                AssertNoBedEffects();
                yield return null;
            }
            Assert.That(host.Dreams.Phase, Is.EqualTo(DreamPlaybackPhase.BlackHold));
            AssertTerminalOnly();
            Assert.That(FadeOpacity(), Is.EqualTo(1));
            Assert.That(host.Runtime.GetView().Game.Phase, Is.EqualTo(GamePhase.Dream));
            Assert.That(Vector3.Dot(host.Player.ViewCamera.transform.forward,
                (terminalScreen.bounds.center - host.Player.ViewCamera.transform.position).normalized), Is.GreaterThan(.99f));

            foreach (var restorePhase in new[] { DreamPlaybackPhase.BlackHold, DreamPlaybackPhase.Revealing })
            {
                Assert.That(host.Dreams.Phase, Is.EqualTo(restorePhase));
                Assert.That(host.Runtime.SetPaused(true).Accepted, Is.True);
                var checkpoint = host.Dreams.CaptureCheckpoint();
                string saved = host.Runtime.Save();
                float fade = FadeOpacity();
                var rotation = host.Player.ViewCamera.transform.rotation;
                BindLightingRuntime(GameRuntime.Restore(saved, rules, content, endings));
                host.Dreams.RestoreCheckpoint(checkpoint);
                AssertTerminalOnly();
                Assert.That(FadeOpacity(), Is.EqualTo(fade).Within(.0001f));
                Assert.That(Quaternion.Angle(host.Player.ViewCamera.transform.rotation, rotation), Is.LessThan(.01f));
                host.Dreams.Advance(60);
                yield return null;
                AssertTerminalOnly();
                Assert.That(host.Dreams.CaptureCheckpoint().Elapsed, Is.EqualTo(checkpoint.Elapsed));
                Assert.That(host.Runtime.Save(), Is.EqualTo(saved), "The return preview must not commit core state while paused.");
                Assert.That(host.Runtime.SetPaused(false).Accepted, Is.True);
                if (restorePhase == DreamPlaybackPhase.BlackHold)
                {
                    host.Dreams.Advance(profile.WakeBlackSeconds);
                    host.Dreams.Advance(profile.ReturnSeconds * .4f);
                    Assert.That(FadeOpacity(), Is.InRange(.1f, .9f));
                }
            }

            for (int frame = 0; frame < 30 && host.Dreams.OwnsCamera; frame++)
            {
                host.Dreams.Advance(profile.ReturnSeconds / 20f);
                AssertTerminalOnly();
                yield return null;
            }
            Assert.That(host.Dreams.OwnsCamera, Is.False);
            Assert.That(host.Runtime.GetView().Game.Phase, Is.EqualTo(GamePhase.SelfReview));
            Assert.That(host.Runtime.GetView().AllowedInteractions, Is.EqualTo(new[] { InteractionTarget.Computer }));
            Assert.That(host.Runtime.GetView().TerminalPower, Is.EqualTo(TerminalPowerState.Off));
            Assert.That(FadeOpacity(), Is.Zero);
        }

        private float FadeOpacity() => host.Screen.GetComponentsInChildren<Image>(true)
            .Single(image => image.name == "PresentationFade").color.a;

        private void AssertNoBedEffects()
        {
            Assert.That(guidance.Spotlight.enabled, Is.False, "A dream return must never reveal the bed.");
            Assert.That(guidance.Atmosphere.BeamRenderer.enabled, Is.False);
            Assert.That(guidance.Atmosphere.ParticleRenderer.enabled, Is.False);
        }

        private void AssertTerminalOnly()
        {
            AssertNoBedEffects();
            var properties = new MaterialPropertyBlock();
            terminalScreen.GetPropertyBlock(properties, 0);
            Assert.That(properties.GetFloat("_CrtPower"), Is.EqualTo(.55f).Within(.001f));
            Assert.That(properties.GetColor("_EmissionColor").maxColorComponent, Is.GreaterThan(.01f));
            Assert.That(properties.GetFloat("_LiveDisplay"), Is.Zero);
            Assert.That(RenderSettings.ambientIntensity, Is.Zero);
            Assert.That(RenderSettings.ambientLight.maxColorComponent, Is.LessThan(.001f));
            Assert.That(RenderSettings.reflectionIntensity, Is.Zero);
            // The guidance owns this DontSave runtime child; global object search omits hidden transient objects.
            var terminalGlow = guidance.GetComponentsInChildren<Light>(true)
                .Single(light => light.name == "Self-review terminal glow");
            Assert.That(terminalGlow.enabled, Is.True);
            foreach (var light in guidance.gameObject.scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<Light>(true)).Where(light => light != terminalGlow))
            {
                string diagnostic = light.name + " parent=" + light.transform.parent?.name
                    + " position=" + light.transform.position + " enabled=" + light.enabled
                    + " active=" + light.gameObject.activeInHierarchy + " intensity=" + light.intensity
                    + " range=" + light.range + " cullingMask=" + light.cullingMask;
                if (light.GetComponentInParent<PatientMonitorRoom>() != null)
                {
                    // Offscreen patient rooms share the scene, but cannot illuminate the apartment.
                    Assert.That(light.type, Is.EqualTo(LightType.Spot), diagnostic);
                    Assert.That(light.cullingMask, Is.EqualTo(1 << PatientSurveillanceFeed.StageLayer), diagnostic);
                    Assert.That(Vector3.Distance(light.transform.position, host.Player.ViewCamera.transform.position),
                        Is.GreaterThan(light.range), diagnostic);
                    continue;
                }
                Assert.That(!light.isActiveAndEnabled || light.intensity < .001f, Is.True, diagnostic + " must stay dark.");
            }
        }

        private void AssertOrdinaryNight()
        {
            Assert.That(RenderSettings.ambientLight, Is.EqualTo(night.AmbientLightColor));
            Assert.That(RenderSettings.reflectionIntensity, Is.EqualTo(night.reflectionIntensity));
            Assert.That(ordinaryLight.enabled, Is.True);
            Assert.That(ordinaryLight.intensity, Is.EqualTo(.7f).Within(.001f));
            Assert.That(guidance.Spotlight.enabled, Is.False, "Ordinary night retains the authored disabled bed light.");
            Assert.That(guidance.Atmosphere.BeamRenderer.enabled, Is.False);
        }

        private void AssertDarkRoom()
        {
            Assert.That(host.Runtime.GetView().TerminalPowered, Is.False);
            var terminalProperties = new MaterialPropertyBlock();
            terminalScreen.GetPropertyBlock(terminalProperties, 0);
            Assert.That(terminalProperties.HasProperty("_CrtPower"), Is.True);
            Assert.That(terminalProperties.GetFloat("_CrtPower"), Is.LessThan(.001f),
                "The production presentation binding must switch off the physical CRT, including live-texture emission.");
            Assert.That(terminalProperties.GetFloat("_LiveDisplay"), Is.LessThan(.001f));
            Assert.That(RenderSettings.ambientIntensity, Is.Zero);
            Assert.That(RenderSettings.reflectionIntensity, Is.Zero);
            foreach (var color in new[] { RenderSettings.ambientLight, RenderSettings.ambientSkyColor,
                RenderSettings.ambientEquatorColor, RenderSettings.ambientGroundColor })
                Assert.That(color.maxColorComponent, Is.LessThan(.001f));
            Assert.That(RenderSettings.skybox, Is.Null);
            Assert.That(RenderSettings.fog, Is.False);
            Assert.That(host.Player.ViewCamera.clearFlags, Is.EqualTo(CameraClearFlags.SolidColor));
            Assert.That(host.Player.ViewCamera.backgroundColor.maxColorComponent, Is.LessThan(.001f));
            foreach (var light in Object.FindObjectsOfType<Light>(true).Where(light =>
                light.gameObject.scene == guidance.gameObject.scene && light != guidance.Spotlight))
                Assert.That(!light.enabled || light.intensity < .001f, Is.True, light.name + " must stay dark.");
            foreach (var renderer in Object.FindObjectsOfType<Renderer>().Where(renderer =>
                renderer.enabled && renderer.gameObject.scene == guidance.gameObject.scene))
                for (int slot = 0; slot < renderer.sharedMaterials.Length; slot++)
                {
                    var material = renderer.sharedMaterials[slot];
                    if (material == null) continue;
                    string property = material.shader != null && material.shader.name == "MiniGame/Room Window Glass" ? "_Color"
                        : material.HasProperty("_EmissionColor") ? "_EmissionColor" : null;
                    if (property == null) continue;
                    var block = new MaterialPropertyBlock(); renderer.GetPropertyBlock(block, slot);
                    if (!block.HasProperty(property)) renderer.GetPropertyBlock(block);
                    var color = block.HasProperty(property) ? block.GetColor(property) : material.GetColor(property);
                    Assert.That(color.maxColorComponent, Is.LessThan(.001f), renderer.name + " " + property);
                }
        }

        private void SetLight(RoomLightingProfile profile, Light light, RoomLightingPeriod period, bool enabled, float intensity)
        {
            string path = controller.GetHierarchyPath(light);
            var settings = controller.CaptureSceneLight(light, period);
            settings.dayEnabled = enabled; settings.intensity = intensity;
            profile.SceneLights.RemoveAll(item => item.HierarchyPath == path);
            profile.SceneLights.Add(settings);
        }

        private GameRuntime BindLightingRuntime(GameRuntime fixture)
        {
            // CRT power/live texture and room lighting have separate presenters. Restore both from the
            // same checkpoint before stopping automatic transitions; binding guidance alone leaves Day0's screen lit.
            host.RestoreCheckpoint(fixture.Save());
            host.Effects.PrepareForRuntimeChange();
            return host.Runtime;
        }

        private GameRuntime CreateCampaign()
        {
            var runtime = new GameRuntime(rules, content, useTerminalDirectory: true);
            runtime.ConfigureEndings(endings);
            return runtime;
        }

        private void DriveTo(GameRuntime runtime, Func<RuntimeView, bool> stop)
            => EndingCampaignScenario.Drive(runtime, rules, EndingCampaignRoute.Normal, stop);
    }
}
