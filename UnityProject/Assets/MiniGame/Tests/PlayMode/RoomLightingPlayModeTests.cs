using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using MiniGame.Core;
using MiniGame.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace MiniGame.UnityHost.Tests
{
    public sealed class RoomLightingPlayModeTests
    {
        private MiniGameBootstrap host;
        private RoomLightingController controller;
        private RoomLightingProfile original, originalNight, settings, nightSettings;
        private string originalJson, originalNightJson;
        private bool previousSkipStartup;

        [UnitySetUp]
        public IEnumerator LoadRoom()
        {
            previousSkipStartup = MainMenuController.SkipStartup;
            MainMenuController.SkipStartup = true;
            MiniGameSessionHost.Release();
            yield return null;
            yield return SceneManager.LoadSceneAsync(MiniGameSessionHost.CombinedScene, LoadSceneMode.Single);
            yield return null;
            host = Object.FindObjectOfType<MiniGameBootstrap>();
            controller = Object.FindObjectOfType<RoomLightingController>();
            Assert.That(host, Is.Not.Null);
            Assert.That(controller, Is.Not.Null, "Combined must have the installed room lighting controller.");
            Assert.That(controller.Profile, Is.Not.Null);
            Assert.That(host.Runtime, Is.Not.Null, "Lighting integration tests start the real gameplay adapters.");
            host.Player.enabled = false;
            original = controller.Profile;
            originalNight = controller.GetProfile(RoomLightingPeriod.Night);
            Assert.That(originalNight, Is.Not.Null);
            originalJson = JsonUtility.ToJson(original);
            originalNightJson = JsonUtility.ToJson(originalNight);
        }

        [UnityTearDown]
        public IEnumerator ReleaseRoom()
        {
            MainMenuController.SkipStartup = previousSkipStartup;
            if (controller != null && original != null)
            {
                controller.ConfigureProfiles(original, originalNight);
                controller.PreviewUnbanded = false;
                controller.ApplyNow();
            }
            if (host != null)
            {
                host.enabled = false;
                host.Player.enabled = false;
                host.Dreams?.PrepareForRuntimeChange();
                host.Effects?.PrepareForRuntimeChange();
            }
            MiniGameSessionHost.Release();
            yield return null;
            var room = SceneManager.GetActiveScene();
            var empty = SceneManager.CreateScene("Room lighting cleanup " + Guid.NewGuid().ToString("N"));
            SceneManager.SetActiveScene(empty);
            if (room.IsValid() && room.isLoaded && room.name == MiniGameSessionHost.CombinedScene)
                yield return SceneManager.UnloadSceneAsync(room);
            if (settings != null) Object.Destroy(settings);
            if (nightSettings != null) Object.Destroy(nightSettings);
            if (original != null && originalJson != null)
                Assert.That(JsonUtility.ToJson(original), Is.EqualTo(originalJson), "Tests must not edit the shared designer asset.");
            if (originalNight != null && originalNightJson != null)
                Assert.That(JsonUtility.ToJson(originalNight), Is.EqualTo(originalNightJson), "Tests must not edit the shared night asset.");
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        [UnityTest]
        public IEnumerator CombinedUsesADownwardLampAndShadowReceivingFloorWithoutAnOptionalRugBinding()
        {
            var lamp = Field<Renderer>("lamp");
            var floor = Field<Renderer>("floor");
            var light = controller.MainLight;
            Assert.That(light.type, Is.EqualTo(LightType.Spot));
            Assert.That(Vector3.Dot(light.transform.forward, Vector3.down), Is.GreaterThan(.999f));
            Assert.That(light.transform.position.y, Is.LessThan(lamp.bounds.min.y - .004f),
                "The emitter must stay below the shade instead of casting its floating ellipse onto the wall.");
            Assert.That(light.shadows, Is.EqualTo(LightShadows.Soft));
            Assert.That(light.renderMode, Is.EqualTo(LightRenderMode.ForcePixel));
            var cookie = light.cookie as Texture2D;
            Assert.That(cookie, Is.Not.Null, "Built-in uses a radial cookie for the configurable inner cone.");
            Assert.That(cookie.GetPixel(cookie.width / 2, cookie.height / 2).a, Is.GreaterThan(.99f));
            Assert.That(cookie.GetPixel(0, 0).a, Is.LessThan(.01f));

            var furniture = host.gameObject.scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<Renderer>(true))
                .Where(item => item.name.StartsWith("B_", StringComparison.Ordinal)
                    && (item.name.IndexOf("Desk", StringComparison.OrdinalIgnoreCase) >= 0
                        || item.name.IndexOf("Table", StringComparison.OrdinalIgnoreCase) >= 0
                        || item.name.IndexOf("Chair", StringComparison.OrdinalIgnoreCase) >= 0
                        || item.name.IndexOf("Bed", StringComparison.OrdinalIgnoreCase) >= 0)).ToArray();
            Assert.That(furniture, Is.Not.Empty, "Verify actual furniture, not only the light component.");
            foreach (var item in furniture)
                Assert.That(item.shadowCastingMode, Is.EqualTo(ShadowCastingMode.On), item.name + " must retain real furniture occlusion.");
            Assert.That(floor.receiveShadows, Is.True);
            Assert.That(controller.Rug, Is.Null, "The authored Combined scene does not bind an optional rug.");
            yield return null;
        }

        [UnityTest]
        public IEnumerator LiveProfileChangesAndUnbandedPreviewDoNotMutateTheSharedAsset()
        {
            CloneSettings();
            settings.lightMultiplier = 1;
            var lampSettings = RegisteredLampSettings();
            lampSettings.intensity = 2.35f;
            lampSettings.innerSpotAngle = 45;
            lampSettings.spotAngle = 105;
            settings.lightingBandsEnabled = true;
            settings.lightingBands = 6;
            settings.NotifyChanged();
            yield return null;
            Assert.That(controller.MainLight.intensity, Is.EqualTo(2.35f).Within(.001f));
            Assert.That(controller.MainLight.spotAngle, Is.EqualTo(105).Within(.001f));
            var floor = Field<Renderer>("floor");
            var block = new MaterialPropertyBlock();
            floor.GetPropertyBlock(block, 0);
            Assert.That(block.GetFloat("_LightingBandsEnabled"), Is.EqualTo(1));
            Assert.That(block.GetFloat("_LightingBands"), Is.EqualTo(6));
            string configured = JsonUtility.ToJson(settings);
            controller.PreviewUnbanded = true;
            yield return null;
            floor.GetPropertyBlock(block, 0);
            Assert.That(block.GetFloat("_LightingBandsEnabled"), Is.Zero);
            Assert.That(JsonUtility.ToJson(settings), Is.EqualTo(configured), "Comparison is transient, not a profile edit.");
            controller.PreviewUnbanded = false;
            yield return null;
            floor.GetPropertyBlock(block, 0);
            Assert.That(block.GetFloat("_LightingBandsEnabled"), Is.EqualTo(1));
            Assert.That(JsonUtility.ToJson(original), Is.EqualTo(originalJson));
        }

        [UnityTest, Timeout(30000)]
        public IEnumerator EditingTheProfileAtBedtimeKeepsTheRoomDarkAndUpdatesTheNextMorning()
        {
            CloneSettings();
            var session = MiniGameSessionHost.Current;
            Assert.That(session, Is.Not.Null);
            var fixture = new GameRuntime(session.Rules, session.Content, true);
            if (fixture.UsesEndingFlow) fixture.ConfigureEndings(EndingDialoguePresenter.LoadCatalog());
            DriveToAfterDream(fixture);
            host.RestoreCheckpoint(fixture.Save());
            var guidance = Object.FindObjectOfType<BedtimeGuidance>();
            Assert.That(guidance.Isolation, Is.EqualTo(1).Within(.0001f));
            Assert.That(guidance.OwnsLighting, Is.True);
            Assert.That(controller.MainLight.enabled, Is.False);
            settings.lightMultiplier = 1;
            RegisteredLampSettings().intensity = 2.4f;
            settings.ambientIntensity = 1;
            settings.ambientColor = new Color(.04f, .035f, .025f);
            settings.lightingBandsEnabled = true;
            settings.lightingBands = 5;
            controller.PreviewUnbanded = true;
            settings.NotifyChanged();
            yield return null;
            Assert.That(controller.MainLight.enabled, Is.False, "Live editing cannot flash the ordinary lamp during sleep guidance.");
            Assert.That(controller.MainLight.intensity, Is.Zero.Within(.001f));
            Assert.That(RenderSettings.ambientIntensity, Is.Zero.Within(.001f));
            Assert.That(RenderSettings.ambientLight.maxColorComponent, Is.Zero.Within(.001f));
            Assert.That(guidance.Spotlight.enabled, Is.True);

            Assert.That(host.Runtime.TryInteract(InteractionTarget.Bed).Accepted, Is.True);
            float sleepDeadline = Time.realtimeSinceStartup + 6;
            while (host.Runtime.GetView().Game.Day == 0 && Time.realtimeSinceStartup < sleepDeadline) yield return null;
            Assert.That(host.Runtime.GetView().Game.Day, Is.EqualTo(1));
            float deadline = Time.realtimeSinceStartup + 4;
            while (guidance.OwnsLighting && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.That(guidance.OwnsLighting, Is.False);
            Assert.That(controller.MainLight.enabled, Is.True);
            Assert.That(controller.MainLight.intensity, Is.EqualTo(2.4f).Within(.001f), "Morning restores the edited baseline.");
            Assert.That(RenderSettings.ambientLight.r, Is.EqualTo(settings.ambientColor.r).Within(.001f));
            Assert.That(RenderSettings.ambientLight.g, Is.EqualTo(settings.ambientColor.g).Within(.001f));
            Assert.That(RenderSettings.ambientLight.b, Is.EqualTo(settings.ambientColor.b).Within(.001f));
            AssertBands(Field<Renderer>("lamp"), 5, false);
            AssertBands(Field<Renderer>("floor"), 5, false);
        }

        [UnityTest]
        public IEnumerator TerminalGlowUpdatesMaterialSlotsWithoutDiscardingTheirLightingBands()
        {
            CloneSettings();
            settings.lightingBandsEnabled = true;
            settings.lightingBands = 7;
            settings.NotifyChanged();
            yield return null;
            var terminal = TerminalSurface();
            // The authored CRT is not a roomSurfaces entry. Seed its per-slot properties to
            // exercise the glow writer's preservation contract independently of that wiring.
            var initial = new MaterialPropertyBlock();
            for (int slot = 0; slot < terminal.sharedMaterials.Length; slot++)
            {
                terminal.GetPropertyBlock(initial, slot);
                initial.SetFloat("_LightingBands", 7);
                initial.SetFloat("_LightingBandsEnabled", 1);
                terminal.SetPropertyBlock(initial, slot);
            }
            AssertBands(terminal, 7, true);
            var daylight = terminal.sharedMaterial.GetColor("_EmissionColor");
            Assert.That(daylight.maxColorComponent, Is.GreaterThan(.01f), "The CRT must have a nonzero daylight emission to exercise dimming.");
            var setGlow = typeof(PresentationEffects).GetMethod("SetSurfaceGlow", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(setGlow, Is.Not.Null);
            try
            {
                setGlow.Invoke(host.Effects, new object[] { .18f });
                var block = new MaterialPropertyBlock();
                for (int slot = 0; slot < terminal.sharedMaterials.Length; slot++)
                {
                    terminal.GetPropertyBlock(block, slot);
                    var actual = block.GetColor("_EmissionColor");
                    Assert.That(actual.r, Is.EqualTo(daylight.r * .18f).Within(.0001f), "CRT slot " + slot + " red emission");
                    Assert.That(actual.g, Is.EqualTo(daylight.g * .18f).Within(.0001f), "CRT slot " + slot + " green emission");
                    Assert.That(actual.b, Is.EqualTo(daylight.b * .18f).Within(.0001f), "CRT slot " + slot + " blue emission");
                }
                AssertBands(terminal, 7, true);
            }
            finally { setGlow.Invoke(host.Effects, new object[] { 1f }); }
        }

        private void CloneSettings()
        {
            settings = Object.Instantiate(original);
            nightSettings = Object.Instantiate(originalNight);
            controller.ConfigureProfiles(settings, nightSettings);
        }

        private SceneLightSettings RegisteredLampSettings() => settings.SceneLights.Single(item =>
            item != null && item.HierarchyPath == controller.GetHierarchyPath(controller.MainLight));

        private T Field<T>(string name) where T : Object => (T)typeof(RoomLightingController)
            .GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(controller);

        private Renderer TerminalSurface() => (Renderer)typeof(PresentationEffects)
            .GetField("terminalSurface", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(host.Effects);

        private static void AssertBands(Renderer renderer, int bands, bool enabled)
        {
            var block = new MaterialPropertyBlock();
            int checkedSlots = 0;
            for (int slot = 0; slot < renderer.sharedMaterials.Length; slot++)
            {
                if (!renderer.sharedMaterials[slot].HasProperty("_LightingBands")) continue;
                renderer.GetPropertyBlock(block, slot);
                Assert.That(block.GetFloat("_LightingBands"), Is.EqualTo(bands), renderer.name + " slot " + slot + " must retain the live band count.");
                Assert.That(block.GetFloat("_LightingBandsEnabled"), Is.EqualTo(enabled ? 1 : 0), renderer.name + " slot " + slot + " must retain the transient preview.");
                checkedSlots++;
            }
            Assert.That(checkedSlots, Is.GreaterThan(0), renderer.name + " must expose room lighting slots.");
        }

        private static void DriveToAfterDream(GameRuntime runtime)
        {
            for (int step = 0; step < 120; step++)
            {
                var view = runtime.GetView();
                if (view.Game.Day == 0 && view.Game.Phase == GamePhase.AfterDream) return;
                RuntimeResult result;
                if (view.Cue != null) result = runtime.CompleteCue(view.Cue.Token);
                else if (view.TerminalPage == TerminalPage.Archive) result = runtime.LeaveTerminal();
                else if (view.TerminalPage == TerminalPage.Directory)
                    result = runtime.EnterSelectedPatient(view.Patients.First(item => item.CanReceiveToday).Id);
                else if (view.Input == InputContext.Dialogue)
                    result = view.Game.EncounterStage == EncounterStage.AwaitingDisposition
                        ? runtime.SubmitDisposition(view.Game.CurrentEncounterId, Disposition.Purify)
                        : runtime.SelectResponse(view.Game.CurrentEncounterId, view.Game.CurrentNodeId, view.Game.AvailableResponseIds.First());
                else if (view.Game.Phase == GamePhase.Evening) result = runtime.BeginNightDream();
                else if (view.Game.Phase == GamePhase.Apartment) result = runtime.TryInteract(InteractionTarget.Computer);
                else { Assert.Fail("Unexpected tutorial stage: " + view.Game.Phase + "/" + view.Input); return; }
                Assert.That(result.Accepted, Is.True, "The lighting fixture must use accepted campaign commands.");
            }
            Assert.Fail("Tutorial did not reach the after-dream bed stage within 120 commands.");
        }
    }
}
