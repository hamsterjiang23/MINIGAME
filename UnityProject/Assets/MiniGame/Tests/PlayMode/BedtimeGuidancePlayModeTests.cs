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
    public sealed class BedtimeGuidancePlayModeTests
    {
        [UnityTest, Timeout(90000)]
        public IEnumerator TerminalDepartureRevealsOnlyTheBedAndSleepRestoresTheMorningRoom()
        {
            yield return SceneManager.LoadSceneAsync("MainRoomCombined");
            yield return null;
            var host = Object.FindObjectOfType<MiniGameBootstrap>();
            var guidance = Object.FindObjectOfType<BedtimeGuidance>();
            Assert.That(host.CampaignActive, Is.True);
            Assert.That(guidance, Is.Not.Null);
            host.RestoreCheckpoint(CreateCampaign(host.Runtime.UsesDreamFlow).Save());
            yield return null;
            var daylight = new LightingState(host.Player.ViewCamera, guidance.Spotlight);
            yield return CaptureWide(host, "bedtime-before");

            // Advance a separate real runtime through the tutorial. Loading its checkpoint exercises
            // the same public restore path as a player save without editing authoritative state.
            var archived = TutorialSignalLoss(host.Runtime.UsesDreamFlow);
            host.RestoreCheckpoint(archived.Save());
            yield return null;
            Assert.That(host.Runtime.GetView().Game.Phase, Is.EqualTo(GamePhase.Evening));
            Assert.That(host.Runtime.GetView().TerminalOpen, Is.True);
            Assert.That(guidance.Isolation, Is.LessThan(.001f), "Patient signal loss precedes the automatic terminal departure.");
            var archiveLights = Object.FindObjectsOfType<Light>().Where(light => light != guidance.Spotlight).ToArray();
            var archiveIntensities = archiveLights.Select(light => light.intensity).ToArray();
            yield return Until(host, () => guidance.Isolation > .15f, "automatic terminal departure begins the bedroom transition");
            for (int index = 0; index < archiveLights.Length; index++)
                Assert.That(archiveLights[index].intensity, Is.LessThanOrEqualTo(archiveIntensities[index] + .01f),
                    archiveLights[index].name + " must dim continuously from the archive lighting, without a brightness jump.");
            Assert.That(host.Runtime.SetPaused(true).Accepted, Is.True);
            float pausedIsolation = guidance.Isolation;
            float pausedBeam = guidance.BeamStrength;
            yield return new WaitForSecondsRealtime(.15f);
            Assert.That(guidance.Isolation, Is.EqualTo(pausedIsolation).Within(.00001f));
            Assert.That(guidance.BeamStrength, Is.EqualTo(pausedBeam).Within(.00001f));
            Assert.That(host.Runtime.SetPaused(false).Accepted, Is.True);
            yield return Until(host, () => host.Runtime.GetView().Input == InputContext.World
                && guidance.Isolation > .999f && guidance.BeamStrength > .999f, "room isolates around the bed");
            AssertNight(host, guidance);
            yield return CaptureWide(host, "bedtime-spotlight");
            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("MINIGAME_CAPTURE_DIR")))
            {
                for (int frame = 0; frame < 16; frame++)
                {
                    yield return Capture(host, "bedtime-motion-" + frame.ToString("D2"));
                    yield return new WaitForSecondsRealtime(.1f);
                }
            }
            var particlePositions = Enumerable.Range(0, guidance.Atmosphere.ParticleCount)
                .Select(guidance.Atmosphere.GetParticlePosition).ToArray();
            yield return new WaitForSecondsRealtime(.5f);
            Assert.That(Enumerable.Range(0, guidance.Atmosphere.ParticleCount).Count(index =>
                Vector3.Distance(guidance.Atmosphere.GetParticlePosition(index), particlePositions[index]) > .0005f), Is.GreaterThan(1),
                "Several glow particles must measurably drift within the lit volume.");
            yield return Capture(host, "bedtime-particles-later");

            yield return StageAt(host, InteractionTarget.Bed);
            Assert.That(host.Player.InteractFocused(), Is.True, "The physical bed ray must remain usable in the darkness.");
            Assert.That(host.Runtime.GetView().Cue.Kind, Is.EqualTo(PresentationKind.Sleep));
            bool dreamObserved = false;
            Action<RuntimeView> observe = view => dreamObserved |= view.Cue?.Kind == PresentationKind.Dream;
            host.Runtime.Changed += observe;
            yield return new WaitForSecondsRealtime(.2f);
            Assert.That(host.Screen.GetComponentsInChildren<Image>(true).Single(image => image.name == "PresentationFade").color.a,
                Is.GreaterThan(0), "The single Day0 sleep plays its actual fade.");
            Assert.That(host.Runtime.GetView().Game.Day, Is.Zero, "The day advances after the sleep fade.");
            Assert.That(guidance.Isolation, Is.GreaterThan(.999f), "Sleep must not bring ordinary room lights back.");
            yield return Until(host, () => host.Runtime.GetView().Game.Day == 1, "one sleep completes Day0");
            host.Runtime.Changed -= observe;
            Assert.That(dreamObserved, Is.False, "Day0 has no dream video or second bed interaction.");
            Assert.That(host.Runtime.GetView().Game.Day, Is.EqualTo(1));
            Assert.That(host.Runtime.GetView().Game.Phase, Is.EqualTo(GamePhase.MorningInspection));
            yield return Until(host, () => !guidance.OwnsLighting && guidance.Isolation < .001f,
                "morning restores the room lighting");
            daylight.AssertRestored(host.Player.ViewCamera);
            Assert.That(guidance.Spotlight.enabled, Is.False);
            yield return CaptureWide(host, "bedtime-restored-morning");
            Debug.Log("MINIGAME_BEDTIME_CYCLE_PASSED automatic terminal exit; ceiling spotlight; moving particles; pause; physical bed trigger; one sleep; morning lighting restore");
        }

        [UnityTest, Timeout(30000)]
        public IEnumerator RestoringPausedBedtimeKeepsUnavailableObjectFeedback()
        {
            yield return SceneManager.LoadSceneAsync("MainRoomCombined");
            yield return null;
            var host = Object.FindObjectOfType<MiniGameBootstrap>();
            var guidance = Object.FindObjectOfType<BedtimeGuidance>();
            Assert.That(guidance, Is.Not.Null);
            host.RestoreCheckpoint(CreateCampaign(host.Runtime.UsesDreamFlow).Save());
            yield return null;
            string apartment = host.Runtime.Save();
            var daylight = new LightingState(host.Player.ViewCamera, guidance.Spotlight);
            var evening = TutorialSignalLoss(host.Runtime.UsesDreamFlow);
            for (int step = 0; evening.GetView().Cue != null && step < 4; step++)
                Assert.That(evening.CompleteCue(evening.GetView().Cue.Token).Accepted, Is.True);
            if (evening.UsesDreamFlow && evening.GetView().Game.Phase == GamePhase.Evening)
                Assert.That(evening.BeginNightDream().Accepted, Is.True);
            Assert.That(evening.SetPaused(true).Accepted, Is.True);
            host.RestoreCheckpoint(evening.Save());
            Assert.That(guidance.Isolation, Is.EqualTo(1).Within(.00001f), "A restored bedtime must not flash the daylight room.");
            Assert.That(guidance.BeamStrength, Is.EqualTo(1).Within(.00001f));
            Assert.That(guidance.Atmosphere.ParticleCount, Is.GreaterThan(1));
            var positions = Enumerable.Range(0, guidance.Atmosphere.ParticleCount)
                .Select(guidance.Atmosphere.GetParticlePosition).ToArray();
            float visualTime = guidance.Atmosphere.VisualTime;
            yield return new WaitForSecondsRealtime(.2f);
            for (int index = 0; index < positions.Length; index++)
                Assert.That(Vector3.Distance(positions[index], guidance.Atmosphere.GetParticlePosition(index)), Is.LessThan(.00001f),
                    "Paused particles must not advance their visual clock.");
            Assert.That(guidance.Atmosphere.VisualTime, Is.EqualTo(visualTime));
            Assert.That(host.Runtime.SetPaused(false).Accepted, Is.True);
            yield return null;
            AssertNight(host, guidance);
            Assert.That(Text(host, "WorldInstructions").gameObject.activeInHierarchy, Is.False,
                "The spotlight replaces the list of unavailable world interactions.");
            foreach (var target in new[] { InteractionTarget.Computer, InteractionTarget.Door })
            {
                yield return StageAt(host, target);
                host.Screen.ClearNotification();
                Assert.That(Text(host, "InteractionPrompt").text, Is.Not.Empty, "Unavailable objects retain their normal focus feedback at night.");
                var before = host.Runtime.Save();
                Assert.That(host.Player.InteractFocused(), Is.False);
                Assert.That(Text(host, "Notification").text, Is.Not.Empty, "Pressing E explains why the object is unavailable.");
                Assert.That(host.Runtime.Save(), Is.EqualTo(before), "Feedback cannot reopen the terminal or door or advance the day.");
                Assert.That(BedtimeGuidance.IsSleepOnly(host.Runtime.GetView()), Is.True);
            }
            yield return StageAt(host, InteractionTarget.Bed);
            Assert.That(Text(host, "InteractionPrompt").text, Does.Contain("[ E ]"));
            host.RestoreCheckpoint(apartment);
            Assert.That(guidance.Isolation, Is.Zero);
            Assert.That(guidance.OwnsLighting, Is.False);
            daylight.AssertRestored(host.Player.ViewCamera);
            Debug.Log("MINIGAME_BEDTIME_RESTORE_PASSED paused checkpoint snap; frozen particles; unavailable furniture feedback; original lighting restored");
        }

        private static GameRuntime CreateCampaign(bool dreamFlow)
        {
            var asset = Resources.Load<DialoguePackageAsset>("MiniGame/ActiveCampaignPackage");
            Assert.That(asset, Is.Not.Null);
            var package = DialoguePackageCompiler.Compile(asset.PackageSource.text);
            var rules = package.Rules;
            if (dreamFlow)
            {
                var rebellion = Resources.Load<PlayerRebellionProfile>("MiniGame/PlayerRebellion");
                Assert.That(rebellion, Is.Not.Null);
                rules = rules.WithDreamFlow(rebellion.InitialValue, rebellion.Maximum, rebellion.Threshold, rebellion.CreateDeltas());
            }
            return new GameRuntime(rules, package.Content, useTerminalDirectory: true);
        }

        private static GameRuntime TutorialSignalLoss(bool dreamFlow)
        {
            var runtime = CreateCampaign(dreamFlow);
            Assert.That(runtime.TryInteract(InteractionTarget.Computer).Accepted, Is.True);
            Assert.That(runtime.CompleteCue(runtime.GetView().Cue.Token).Accepted, Is.True);
            Assert.That(runtime.EnterSelectedPatient(runtime.GetView().Game.CurrentPatientId).Accepted, Is.True);
            Assert.That(runtime.GetView().Cue.Kind, Is.EqualTo(PresentationKind.Introduction));
            Assert.That(runtime.CompleteCue(runtime.GetView().Cue.Token).Accepted, Is.True);
            Assert.That(runtime.SubmitDisposition(runtime.GetView().Game.CurrentEncounterId, Disposition.Purify).Accepted, Is.True);
            Assert.That(runtime.CompleteCue(runtime.GetView().Cue.Token).Accepted, Is.True);
            Assert.That(runtime.GetView().Cue.Kind, Is.EqualTo(PresentationKind.ExitTerminal));
            Assert.That(runtime.GetView().LeavingTerminal, Is.False, "Patient signal loss precedes whole-terminal shutdown.");
            Assert.That(runtime.GetView().Game.Phase, Is.EqualTo(GamePhase.Evening));
            return runtime;
        }

        private static void AssertNight(MiniGameBootstrap host, BedtimeGuidance guidance)
        {
            Assert.That(host.Runtime.GetView().AllowedInteractions, Is.EqualTo(new[] { InteractionTarget.Bed }),
                "Visual guidance must preserve the core's bed-only interaction contract.");
            Assert.That(guidance.OwnsLighting, Is.True);
            Assert.That(RenderSettings.ambientIntensity, Is.Zero);
            Assert.That(RenderSettings.reflectionIntensity, Is.Zero);
            Assert.That(RenderSettings.ambientLight.maxColorComponent, Is.LessThan(.001f));
            Assert.That(host.Player.ViewCamera.clearFlags, Is.EqualTo(CameraClearFlags.SolidColor));
            Assert.That(host.Player.ViewCamera.backgroundColor.maxColorComponent, Is.LessThan(.001f));
            Assert.That(Object.FindObjectsOfType<Light>().Where(light => light != guidance.Spotlight)
                .All(light => !light.enabled || light.intensity < .001f), Is.True, "Other room lights must not dilute the bed isolation.");
            foreach (var surface in SelfLitSlots().Where(surface => surface.Renderer.enabled))
            {
                var properties = new MaterialPropertyBlock();
                surface.Renderer.GetPropertyBlock(properties, surface.Slot);
                string context = surface.Renderer.name + " material " + surface.Slot;
                Assert.That(properties.HasProperty(surface.Property), Is.True, context + " needs an override for its self-lit material.");
                Assert.That(properties.GetColor(surface.Property).maxColorComponent, Is.LessThan(.001f),
                    context + " must not glow outside the bed spotlight.");
            }
            Assert.That(guidance.Spotlight.enabled, Is.True);
            Assert.That(guidance.Spotlight.type, Is.EqualTo(LightType.Spot));
            Assert.That(guidance.Spotlight.intensity, Is.GreaterThan(0));
            Assert.That(Vector3.Dot(guidance.Spotlight.transform.forward, Vector3.down), Is.GreaterThan(.99f));
            Assert.That(guidance.BedRenderers, Is.Not.Empty);
            var bedBounds = Object.FindObjectsOfType<WorldInteraction>()
                .Single(item => item.Target == InteractionTarget.Bed).GetComponent<Collider>().bounds;
            Assert.That(guidance.Spotlight.transform.position.y - bedBounds.max.y, Is.GreaterThan(1f));
            Assert.That(Vector2.Distance(new Vector2(guidance.Spotlight.transform.position.x, guidance.Spotlight.transform.position.z),
                new Vector2(bedBounds.center.x, bedBounds.center.z)), Is.LessThan(.5f));
            Assert.That(guidance.BedRenderers.All(renderer => guidance.Spotlight.cullingMask == 1 << renderer.gameObject.layer), Is.True,
                "The beam must illuminate only the bed's dedicated rendering layer.");
        }

        private static IEnumerator StageAt(MiniGameBootstrap host, InteractionTarget target, bool expectHit = true)
        {
            var marker = GameObject.Find(target + " approach");
            Assert.That(marker, Is.Not.Null);
            var controller = host.Player.GetComponent<CharacterController>();
            controller.enabled = false;
            host.Player.transform.position = marker.transform.position;
            controller.enabled = true;
            Physics.SyncTransforms();
            yield return null;
            var interaction = Object.FindObjectsOfType<WorldInteraction>().Single(item => item.Target == target);
            var collider = interaction.GetComponent<Collider>();
            var center = collider is BoxCollider box ? box.transform.TransformPoint(box.center) : interaction.Surface.bounds.center;
            host.Player.FacePoint(center);
            host.Player.RefreshInteractionFocus();
            if (expectHit)
                Assert.That(host.Player.FocusedInteraction, Is.EqualTo(interaction), "The camera must hit the physical " + target + " object.");
            else
                Assert.That(host.Player.FocusedInteraction, Is.Not.EqualTo(interaction), "The disabled " + target + " collider must not remain focusable.");
        }

        private static IEnumerator Until(MiniGameBootstrap host, Func<bool> condition, string expectation)
        {
            float deadline = Time.realtimeSinceStartup + 10f;
            while (!condition() && Time.realtimeSinceStartup < deadline) yield return null;
            var view = host.Runtime.GetView();
            Assert.That(condition(), Is.True, expectation + ": phase=" + view.Game.Phase + " input=" + view.Input + " cue=" + view.Cue?.Kind);
        }

        private static Text Text(MiniGameBootstrap host, string name) => host.Screen.GetComponentsInChildren<Text>(true).Single(text => text.name == name);
        private static IEnumerator Capture(MiniGameBootstrap host, string name) => PrototypePlayModeTests.CaptureIfRequested(host, name);
        private static IEnumerator CaptureWide(MiniGameBootstrap host, string name)
        {
            var computer = Object.FindObjectsOfType<WorldInteraction>().Single(item => item.Target == InteractionTarget.Computer);
            yield return StageAt(host, InteractionTarget.Computer, expectHit: computer.GetComponent<Collider>().enabled);
            var bed = Object.FindObjectsOfType<WorldInteraction>().Single(item => item.Target == InteractionTarget.Bed);
            host.Player.FacePoint(bed.GetComponent<Collider>().bounds.center + Vector3.up * .9f);
            host.Player.RefreshInteractionFocus();
            yield return Capture(host, name);
        }

        private static SurfaceSlot[] SelfLitSlots() => Object.FindObjectsOfType<Renderer>()
            .SelectMany(renderer => renderer.sharedMaterials.Select((material, slot) => new SurfaceSlot(renderer, slot, material)))
            .Where(surface => surface.Property != null).ToArray();

        private sealed class SurfaceSlot
        {
            public readonly Renderer Renderer;
            public readonly int Slot;
            public readonly string Property;
            public SurfaceSlot(Renderer renderer, int slot, Material material)
            {
                Renderer = renderer; Slot = slot;
                Property = material == null ? null : material.shader != null && material.shader.name == "MiniGame/Room Window Glass"
                    ? "_Color" : material.HasProperty("_EmissionColor") && material.GetColor("_EmissionColor").maxColorComponent > .001f
                        ? "_EmissionColor" : null;
            }
        }

        private sealed class LightingState
        {
            private readonly float ambientIntensity = RenderSettings.ambientIntensity;
            private readonly float reflectionIntensity = RenderSettings.reflectionIntensity;
            private readonly Color ambientLight = RenderSettings.ambientLight;
            private readonly UnityEngine.Rendering.AmbientMode ambientMode = RenderSettings.ambientMode;
            private readonly Material skybox = RenderSettings.skybox;
            private readonly CameraClearFlags clearFlags;
            private readonly Color background;
            private readonly Light[] lights;
            private readonly bool[] enabled;
            private readonly float[] intensity;
            private readonly Color[] colors;
            private readonly SurfaceSlot[] surfaces;
            private readonly Color[] emission;

            public LightingState(Camera camera, Light spotlight)
            {
                clearFlags = camera.clearFlags;
                background = camera.backgroundColor;
                lights = Object.FindObjectsOfType<Light>().Where(light => light != spotlight).ToArray();
                enabled = lights.Select(light => light.enabled).ToArray();
                intensity = lights.Select(light => light.intensity).ToArray();
                colors = lights.Select(light => light.color).ToArray();
                surfaces = SelfLitSlots();
                emission = new Color[surfaces.Length];
                for (int index = 0; index < surfaces.Length; index++)
                    emission[index] = EffectiveColor(surfaces[index]);
            }

            public void AssertRestored(Camera camera)
            {
                Assert.That(RenderSettings.ambientIntensity, Is.EqualTo(ambientIntensity).Within(.001f));
                Assert.That(RenderSettings.reflectionIntensity, Is.EqualTo(reflectionIntensity).Within(.001f));
                Assert.That(RenderSettings.ambientLight, Is.EqualTo(ambientLight));
                Assert.That(RenderSettings.ambientMode, Is.EqualTo(ambientMode));
                Assert.That(RenderSettings.skybox, Is.EqualTo(skybox));
                Assert.That(camera.clearFlags, Is.EqualTo(clearFlags));
                Assert.That(camera.backgroundColor, Is.EqualTo(background));
                for (int index = 0; index < lights.Length; index++)
                {
                    Assert.That(lights[index].enabled, Is.EqualTo(enabled[index]), lights[index].name);
                    Assert.That(lights[index].intensity, Is.EqualTo(intensity[index]).Within(.001f), lights[index].name);
                    Assert.That(lights[index].color, Is.EqualTo(colors[index]), lights[index].name);
                }
                for (int index = 0; index < surfaces.Length; index++)
                {
                    var surface = surfaces[index];
                    string context = surface.Renderer.name + " material " + surface.Slot;
                    Assert.That(EffectiveColor(surface), Is.EqualTo(emission[index]), context + " effective emission/glass colour");
                }
            }

            private static Color EffectiveColor(SurfaceSlot surface)
            {
                var properties = new MaterialPropertyBlock();
                surface.Renderer.GetPropertyBlock(properties, surface.Slot);
                if (properties.HasProperty(surface.Property)) return properties.GetColor(surface.Property);
                surface.Renderer.GetPropertyBlock(properties);
                if (properties.HasProperty(surface.Property)) return properties.GetColor(surface.Property);
                return surface.Renderer.sharedMaterials[surface.Slot].GetColor(surface.Property);
            }
        }
    }
}
