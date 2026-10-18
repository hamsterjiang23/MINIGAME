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
            var daylight = new LightingState(host.Player.ViewCamera, guidance.Spotlight);
            yield return CaptureWide(host, "bedtime-before");

            // Advance a separate real runtime through the tutorial. Loading its checkpoint exercises
            // the same public restore path as a player save without editing authoritative state.
            var archived = TutorialArchive();
            host.RestoreCheckpoint(archived.Save());
            yield return null;
            Assert.That(host.Runtime.GetView().Game.Phase, Is.EqualTo(GamePhase.Evening));
            Assert.That(host.Runtime.GetView().TerminalOpen, Is.True);
            Assert.That(guidance.Isolation, Is.LessThan(.001f), "The archive remains readable before returning to the room.");
            yield return new WaitForSecondsRealtime(1.3f);
            var archiveLights = Object.FindObjectsOfType<Light>().Where(light => light != guidance.Spotlight).ToArray();
            var archiveIntensities = archiveLights.Select(light => light.intensity).ToArray();
            Assert.That(host.Runtime.ReturnToDirectory().Accepted, Is.True);
            Assert.That(host.Runtime.LeaveTerminal().Accepted, Is.True);
            yield return null;
            for (int index = 0; index < archiveLights.Length; index++)
                Assert.That(archiveLights[index].intensity, Is.LessThanOrEqualTo(archiveIntensities[index] + .01f),
                    archiveLights[index].name + " must dim continuously from the archive lighting, without a brightness jump.");
            yield return Until(host, () => guidance.Isolation > .15f, "bedroom light transition begins");
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
            var insectPositions = Enumerable.Range(0, guidance.Atmosphere.InsectCount)
                .Select(guidance.Atmosphere.GetInsectPosition).ToArray();
            yield return new WaitForSecondsRealtime(.25f);
            Assert.That(Enumerable.Range(0, guidance.Atmosphere.InsectCount).Count(index =>
                Vector3.Distance(guidance.Atmosphere.GetInsectPosition(index), insectPositions[index]) > .005f), Is.GreaterThan(1),
                "Several insects must actually fly within the lit volume.");
            yield return Capture(host, "bedtime-insects-later");

            yield return StageAt(host, InteractionTarget.Bed);
            Assert.That(host.Player.InteractFocused(), Is.True, "The physical bed ray must remain usable in the darkness.");
            Assert.That(host.Runtime.GetView().Cue.Kind, Is.EqualTo(PresentationKind.Sleep));
            yield return Until(host, () => host.Runtime.GetView().Cue?.Kind == PresentationKind.Dream
                && guidance.BeamStrength < .001f, "sleep fades the beam before dream");
            Assert.That(guidance.Isolation, Is.GreaterThan(.999f), "Sleep must not bring ordinary room lights back.");
            Assert.That(host.Runtime.CompleteCue(host.Runtime.GetView().Cue.Token).Accepted, Is.True);
            yield return Until(host, () => host.Runtime.GetView().Game.Phase == GamePhase.AfterDream
                && guidance.BeamStrength > .999f, "after-dream bed guidance returns");
            AssertNight(host, guidance);
            yield return CaptureWide(host, "bedtime-after-dream");
            yield return StageAt(host, InteractionTarget.Bed);
            Assert.That(host.Player.InteractFocused(), Is.True);
            Assert.That(host.Runtime.GetView().Game.Day, Is.EqualTo(1));
            Assert.That(host.Runtime.GetView().Game.Phase, Is.EqualTo(GamePhase.MorningInspection));
            yield return Until(host, () => !guidance.OwnsLighting && guidance.Isolation < .001f,
                "morning restores the room lighting");
            daylight.AssertRestored(host.Player.ViewCamera);
            Assert.That(guidance.Spotlight.enabled, Is.False);
            yield return CaptureWide(host, "bedtime-restored-morning");
            Debug.Log("MINIGAME_BEDTIME_CYCLE_PASSED archive exit; ceiling spotlight; moving insects; pause; physical bed; dream; morning lighting restore");
        }

        [UnityTest, Timeout(30000)]
        public IEnumerator RestoringPausedBedtimeSnapsTheLightAndBlockedObjectsStaySilent()
        {
            yield return SceneManager.LoadSceneAsync("MainRoomCombined");
            yield return null;
            var host = Object.FindObjectOfType<MiniGameBootstrap>();
            var guidance = Object.FindObjectOfType<BedtimeGuidance>();
            Assert.That(guidance, Is.Not.Null);
            string apartment = host.Runtime.Save();
            var daylight = new LightingState(host.Player.ViewCamera, guidance.Spotlight);
            var evening = TutorialArchive();
            Assert.That(evening.ReturnToDirectory().Accepted, Is.True);
            Assert.That(evening.LeaveTerminal().Accepted, Is.True);
            Assert.That(evening.CompleteCue(evening.GetView().Cue.Token).Accepted, Is.True);
            Assert.That(evening.SetPaused(true).Accepted, Is.True);
            host.RestoreCheckpoint(evening.Save());
            Assert.That(guidance.Isolation, Is.EqualTo(1).Within(.00001f), "A restored bedtime must not flash the daylight room.");
            Assert.That(guidance.BeamStrength, Is.EqualTo(1).Within(.00001f));
            Assert.That(guidance.Atmosphere.InsectCount, Is.GreaterThan(1));
            var positions = Enumerable.Range(0, guidance.Atmosphere.InsectCount)
                .Select(guidance.Atmosphere.GetInsectPosition).ToArray();
            float visualTime = guidance.Atmosphere.VisualTime;
            yield return new WaitForSecondsRealtime(.2f);
            for (int index = 0; index < positions.Length; index++)
                Assert.That(Vector3.Distance(positions[index], guidance.Atmosphere.GetInsectPosition(index)), Is.LessThan(.00001f),
                    "Paused insects must not advance their visual clock.");
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
                Assert.That(Text(host, "InteractionPrompt").text, Is.Empty, "Blocked furniture must not show a 'temporarily unavailable' prompt.");
                Assert.That(host.Player.InteractFocused(), Is.False);
                Assert.That(Text(host, "Notification").text, Is.Empty, "Pressing E on blocked furniture must not add a rejection toast.");
                Assert.That(host.Runtime.GetView().Game.Phase, Is.EqualTo(GamePhase.Evening));
            }
            yield return StageAt(host, InteractionTarget.Bed);
            Assert.That(Text(host, "InteractionPrompt").text, Does.Contain("[ E ]"));
            host.RestoreCheckpoint(apartment);
            Assert.That(guidance.Isolation, Is.Zero);
            Assert.That(guidance.OwnsLighting, Is.False);
            daylight.AssertRestored(host.Player.ViewCamera);
            Debug.Log("MINIGAME_BEDTIME_RESTORE_PASSED paused checkpoint snap; frozen insects; blocked furniture silent; original lighting restored");
        }

        private static GameRuntime TutorialArchive()
        {
            var asset = Resources.Load<DialoguePackageAsset>("MiniGame/ActiveCampaignPackage");
            Assert.That(asset, Is.Not.Null);
            var package = DialoguePackageCompiler.Compile(asset.PackageSource.text);
            var runtime = new GameRuntime(package.Rules, package.Content, useTerminalDirectory: true);
            Assert.That(runtime.TryInteract(InteractionTarget.Computer).Accepted, Is.True);
            Assert.That(runtime.CompleteCue(runtime.GetView().Cue.Token).Accepted, Is.True);
            Assert.That(runtime.EnterSelectedPatient(runtime.GetView().Game.CurrentPatientId).Accepted, Is.True);
            Assert.That(runtime.GetView().Cue.Kind, Is.EqualTo(PresentationKind.Introduction));
            Assert.That(runtime.CompleteCue(runtime.GetView().Cue.Token).Accepted, Is.True);
            Assert.That(runtime.SubmitDisposition(runtime.GetView().Game.CurrentEncounterId, Disposition.Purify).Accepted, Is.True);
            Assert.That(runtime.CompleteCue(runtime.GetView().Cue.Token).Accepted, Is.True);
            Assert.That(runtime.CompleteCue(runtime.GetView().Cue.Token).Accepted, Is.True);
            Assert.That(runtime.GetView().TerminalPage, Is.EqualTo(TerminalPage.Archive));
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

        private static IEnumerator StageAt(MiniGameBootstrap host, InteractionTarget target)
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
            host.Player.FacePoint(interaction.GetComponent<Collider>().bounds.center);
            host.Player.RefreshInteractionFocus();
            Assert.That(host.Player.FocusedInteraction, Is.EqualTo(interaction), "The camera must hit the physical " + target + " object.");
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
            yield return StageAt(host, InteractionTarget.Computer);
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
            private readonly bool[] emptyProperties;
            private readonly Color[] emission;
            private readonly Color[] globalEmission;

            public LightingState(Camera camera, Light spotlight)
            {
                clearFlags = camera.clearFlags;
                background = camera.backgroundColor;
                lights = Object.FindObjectsOfType<Light>().Where(light => light != spotlight).ToArray();
                enabled = lights.Select(light => light.enabled).ToArray();
                intensity = lights.Select(light => light.intensity).ToArray();
                colors = lights.Select(light => light.color).ToArray();
                surfaces = SelfLitSlots();
                emptyProperties = new bool[surfaces.Length];
                emission = new Color[surfaces.Length];
                globalEmission = new Color[surfaces.Length];
                for (int index = 0; index < surfaces.Length; index++)
                {
                    var surface = surfaces[index];
                    var properties = new MaterialPropertyBlock();
                    surface.Renderer.GetPropertyBlock(properties, surface.Slot);
                    emptyProperties[index] = properties.isEmpty;
                    emission[index] = properties.GetColor(surface.Property);
                    surface.Renderer.GetPropertyBlock(properties);
                    globalEmission[index] = properties.GetColor(surface.Property);
                }
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
                    var properties = new MaterialPropertyBlock();
                    surface.Renderer.GetPropertyBlock(properties, surface.Slot);
                    Assert.That(properties.isEmpty, Is.EqualTo(emptyProperties[index]), context);
                    Assert.That(properties.GetColor(surface.Property), Is.EqualTo(emission[index]), context);
                    surface.Renderer.GetPropertyBlock(properties);
                    Assert.That(properties.GetColor(surface.Property), Is.EqualTo(globalEmission[index]), context + " renderer override");
                }
            }
        }
    }
}
