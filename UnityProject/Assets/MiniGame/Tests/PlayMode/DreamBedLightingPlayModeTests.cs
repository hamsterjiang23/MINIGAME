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
using Object = UnityEngine.Object;

namespace MiniGame.UnityHost.Tests
{
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

        [UnitySetUp]
        public IEnumerator OpenRoom()
        {
            MiniGameSessionHost.Release();
            yield return SceneManager.LoadSceneAsync("MainRoomCombined");
            yield return null;
            host = Object.FindObjectOfType<MiniGameBootstrap>();
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
        public IEnumerator EveryNightRestoresOnlyTheBedWhilePausedAndPreservesLightingPreviews()
        {
            var fixture = CreateCampaign();
            for (int currentDay = 1; currentDay <= 3; currentDay++)
            {
                DriveTo(fixture, view => view.Game.Day == currentDay && view.Game.Phase == GamePhase.AfterDream);
                Assert.That(fixture.SetPaused(true).Accepted, Is.True);
                var restored = BindLightingRuntime(fixture);
                AssertBedOnly();
                string saved = restored.Save();
                float visualTime = guidance.Atmosphere.VisualTime;
                yield return new WaitForSecondsRealtime(.1f);
                AssertBedOnly();
                Assert.That(guidance.Atmosphere.VisualTime, Is.EqualTo(visualTime));
                guidance.enabled = false; guidance.enabled = true;
                AssertBedOnly();

                controller.SetPreviewMode(RoomLightingPreviewMode.Night);
                AssertOrdinaryNight();
                controller.SetPreviewMode(RoomLightingPreviewMode.Day);
                Assert.That(RenderSettings.ambientLight, Is.EqualTo(day.AmbientLightColor));
                Assert.That(guidance.Spotlight.enabled, Is.False);
                controller.SetPreviewMode(RoomLightingPreviewMode.Automatic);
                AssertBedOnly();
                Assert.That(restored.Save(), Is.EqualTo(saved), "Light previews and pause restoration must not wake the player.");
                Assert.That(fixture.SetPaused(false).Accepted, Is.True);
            }
        }

        [UnityTest, Timeout(30000)]
        public IEnumerator ReturningRevealWorksBeforeDreamCompletionAndRebindClearsItsTransientOverride()
        {
            var fixture = CreateCampaign();
            DriveTo(fixture, view => view.Game.Day == 1 && view.Game.Phase == GamePhase.Dream);
            Assert.That(fixture.SetPaused(true).Accepted, Is.True);
            fixture = BindLightingRuntime(fixture);
            AssertDarkRoom();
            Assert.That(guidance.Spotlight.enabled, Is.False);
            Assert.That(guidance.Atmosphere.BeamRenderer.enabled, Is.False);
            string checkpoint = fixture.Save();
            guidance.SetDreamBedReveal(true);
            AssertBedOnly();
            Assert.That(fixture.GetView().Game.Phase, Is.EqualTo(GamePhase.Dream));
            Assert.That(fixture.Save(), Is.EqualTo(checkpoint), "The reveal must not consume the dream cue or wake the player.");
            yield return new WaitForSecondsRealtime(.1f);
            AssertBedOnly();
            guidance.SetDreamBedReveal(false);
            AssertDarkRoom();
            Assert.That(guidance.Spotlight.enabled, Is.False);
            Assert.That(guidance.Atmosphere.BeamRenderer.enabled, Is.False);

            guidance.SetDreamBedReveal(true);
            BindLightingRuntime(GameRuntime.Restore(checkpoint, rules, content));
            AssertDarkRoom();
            Assert.That(guidance.Spotlight.enabled, Is.False, "A new Dream binding must wait for its own returning cue.");
            guidance.SetDreamBedReveal(true);
            AssertBedOnly();
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

        private void AssertOrdinaryNight()
        {
            Assert.That(RenderSettings.ambientLight, Is.EqualTo(night.AmbientLightColor));
            Assert.That(RenderSettings.reflectionIntensity, Is.EqualTo(night.reflectionIntensity));
            Assert.That(ordinaryLight.enabled, Is.True);
            Assert.That(ordinaryLight.intensity, Is.EqualTo(.7f).Within(.001f));
            Assert.That(guidance.Spotlight.enabled, Is.False, "Ordinary night retains the authored disabled bed light.");
            Assert.That(guidance.Atmosphere.BeamRenderer.enabled, Is.False);
        }

        private void AssertBedOnly()
        {
            AssertDarkRoom();
            Assert.That(guidance.Spotlight.enabled, Is.True);
            Assert.That(guidance.Spotlight.type, Is.EqualTo(LightType.Spot));
            Assert.That(guidance.Spotlight.intensity, Is.EqualTo(guidance.DefaultSpotIntensity).Within(.001f));
            Assert.That(guidance.Atmosphere.BeamRenderer.enabled, Is.True);
            Assert.That(guidance.Atmosphere.ParticleRenderer.enabled, Is.True);
            Assert.That(guidance.BedRenderers.All(renderer => guidance.Spotlight.cullingMask == 1 << renderer.gameObject.layer), Is.True);
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

        private GameRuntime CreateCampaign() => new GameRuntime(rules, content, useTerminalDirectory: true);

        private static void DriveTo(GameRuntime runtime, Func<RuntimeView, bool> stop)
        {
            for (int step = 0; step < 2000; step++)
            {
                var view = runtime.GetView();
                if (stop(view)) return;
                RuntimeResult result;
                if (view.Cue != null) result = runtime.CompleteCue(view.Cue.Token);
                else if (view.TerminalPage == TerminalPage.Archive) result = runtime.LeaveTerminal();
                else if (view.TerminalPage == TerminalPage.Directory)
                    result = runtime.EnterSelectedPatient(view.Patients.First(item => item.CanReceiveToday).Id);
                else if (view.Input == InputContext.Dialogue)
                {
                    if (view.Game.EncounterStage == EncounterStage.AwaitingDisposition)
                    {
                        var available = view.Game.AvailableDispositions;
                        var disposition = view.Game.RemainingPurificationQuota > 0 && available.Contains(Disposition.Purify) ? Disposition.Purify
                            : available.Contains(Disposition.Treat) ? Disposition.Treat : available.First();
                        result = runtime.SubmitDisposition(view.Game.CurrentEncounterId, disposition);
                    }
                    else result = runtime.SelectResponse(view.Game.CurrentEncounterId, view.Game.CurrentNodeId, view.Game.AvailableResponseIds.First());
                }
                else if (view.Game.Phase == GamePhase.Evening) result = runtime.BeginNightDream();
                else if (view.Game.Phase == GamePhase.AfterDream) result = runtime.TryInteract(InteractionTarget.Bed);
                else if (view.Game.Phase == GamePhase.MorningInspection) result = runtime.TryInteract(InteractionTarget.Door);
                else if (view.Game.Phase == GamePhase.Apartment || view.Game.Phase == GamePhase.Work) result = runtime.TryInteract(InteractionTarget.Computer);
                else { Assert.Fail("No legal fixture step in " + view.Game.Phase + "/" + view.Input); return; }
                Assert.That(result.Accepted, Is.True, "Rejected real campaign command: " + view.Game.Phase + "/" + view.Input);
            }
            Assert.Fail("Real campaign driver exceeded its 2000-command bound.");
        }
    }
}
