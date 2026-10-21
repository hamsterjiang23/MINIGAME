using System;
using System.Collections;
using MiniGame.Core;
using MiniGame.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace MiniGame.UnityHost.Tests
{
    public sealed class RoomLightingAuthoringPlayModeTests
    {
        private Scene previousScene, ownedScene;
        private GameObject owner;
        private RoomLightingProfile source, settings;
        private string sourceJson;

        [UnityTearDown]
        public IEnumerator Cleanup()
        {
            if (owner != null)
            {
                var guidance = owner.GetComponent<BedtimeGuidance>();
                if (guidance != null) guidance.enabled = false;
                Object.Destroy(owner);
            }
            if (settings != null) Object.Destroy(settings);
            yield return null;
            if (previousScene.IsValid() && previousScene.isLoaded) SceneManager.SetActiveScene(previousScene);
            else SceneManager.SetActiveScene(SceneManager.CreateScene("Lighting authoring cleanup"));
            if (ownedScene.IsValid() && ownedScene.isLoaded) yield return SceneManager.UnloadSceneAsync(ownedScene);
            if (source != null) Assert.That(JsonUtility.ToJson(source), Is.EqualTo(sourceJson), "The shared designer asset must not be edited by the test.");
        }

        [UnityTest, Timeout(15000)]
        public IEnumerator EmptyListPreservesSceneLightsAndNightEditsRestoreTheNewDaylightWithoutAFlash()
        {
            previousScene = SceneManager.GetActiveScene();
            ownedScene = SceneManager.CreateScene("Lighting authoring " + Guid.NewGuid().ToString("N"));
            SceneManager.SetActiveScene(ownedScene);
            owner = new GameObject("Lighting authoring fixture");
            source = Resources.Load<RoomLightingProfile>("RoomLightingDefault");
            Assert.That(source, Is.Not.Null);
            sourceJson = JsonUtility.ToJson(source);
            settings = Object.Instantiate(source); settings.SceneLights.Clear(); settings.night = new RoomNightLightingSettings(); settings.NotifyChanged();
            var cameraObject = new GameObject("Camera", typeof(Camera)); cameraObject.transform.SetParent(owner.transform);
            var camera = cameraObject.GetComponent<Camera>(); camera.enabled = false;
            var bedObject = new GameObject("Bed guide", typeof(Light)); bedObject.transform.SetParent(owner.transform);
            var bed = bedObject.GetComponent<Light>(); bed.type = LightType.Spot;
            var extraObject = new GameObject("Supplemental lamp", typeof(Light)); extraObject.transform.SetParent(owner.transform);
            var extra = extraObject.GetComponent<Light>(); extra.intensity = .8f; extra.range = 3.5f;
            extra.color = new Color(.8f, .7f, .5f); extra.transform.localPosition = new Vector3(1, 2, 3);
            var guidance = owner.AddComponent<BedtimeGuidance>();
            guidance.Configure(camera, bed, Array.Empty<Renderer>(), Vector3.zero, 1);
            var controller = owner.AddComponent<RoomLightingController>();
            controller.Configure(settings, null, null, null, null, null, null, null, 0, Array.Empty<Renderer>(), null, guidance);
            controller.ApplyFromProfile();
            yield return null;
            Assert.That(settings.SceneLights, Is.Empty, "Opening an old profile must not author new light overrides.");
            Assert.That(extra.enabled, Is.True);
            Assert.That(extra.intensity, Is.EqualTo(.8f).Within(.0001f));
            Assert.That(extra.range, Is.EqualTo(3.5f).Within(.0001f));
            Assert.That(extra.transform.localPosition, Is.EqualTo(new Vector3(1, 2, 3)));
            Assert.That(settings.lampEulerAngles, Is.EqualTo(new Vector3(90, 0, 0)), "The legacy main lamp starts facing down.");

            var rules = PrototypeRules.Create();
            var runtime = new GameRuntime(rules, new PrototypeContent(rules));
            DriveToEvening(runtime);
            guidance.Bind(runtime);
            Assert.That(guidance.OwnsLighting, Is.True);
            Assert.That(extra.enabled, Is.False);
            Assert.That(extra.intensity, Is.Zero.Within(.0001f));
            var edited = controller.CaptureSceneLight(extra);
            Assert.That(edited.intensity, Is.EqualTo(.8f).Within(.0001f), "Editing at night must begin with the retained daylight, not the currently dark light.");
            edited.intensity = 3.1f; edited.range = 6.5f; edited.color = new Color(.95f, .63f, .2f);
            edited.localPosition = new Vector3(2, 1.5f, 1); edited.dayEnabled = true;
            settings.SceneLights.Add(edited); settings.NotifyChanged(); controller.ApplyFromProfile();
            Assert.That(extra.enabled, Is.False, "Applying the daylight baseline must not briefly enable the light.");
            Assert.That(extra.intensity, Is.Zero.Within(.0001f));
            yield return null; yield return null;
            Assert.That(extra.enabled, Is.False);
            Assert.That(extra.intensity, Is.Zero.Within(.0001f));
            Assert.That(guidance.TryGetDaylightLight(extra, out bool enabled, out float intensity, out Color color), Is.True);
            Assert.That(enabled, Is.True); Assert.That(intensity, Is.EqualTo(3.1f).Within(.0001f));
            Assert.That(color, Is.EqualTo(edited.color));

            Assert.That(runtime.TryInteract(InteractionTarget.Bed).Accepted, Is.True);
            Complete(runtime); Complete(runtime);
            Assert.That(runtime.TryInteract(InteractionTarget.Bed).Accepted, Is.True);
            Assert.That(runtime.GetView().Game.Day, Is.EqualTo(2));
            float deadline = Time.realtimeSinceStartup + 4;
            while (guidance.OwnsLighting && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.That(guidance.OwnsLighting, Is.False);
            Assert.That(extra.enabled, Is.True);
            Assert.That(extra.intensity, Is.EqualTo(3.1f).Within(.0001f));
            Assert.That(extra.color, Is.EqualTo(edited.color));
            Assert.That(extra.range, Is.EqualTo(6.5f).Within(.0001f));
            Assert.That(extra.transform.localPosition, Is.EqualTo(edited.localPosition));
        }

        private static void Complete(GameRuntime runtime) => Assert.That(runtime.CompleteCue(runtime.GetView().Cue.Token).Accepted, Is.True);
        private static void DriveToEvening(GameRuntime runtime)
        {
            for (int step = 0; step < 500; step++)
            {
                var view = runtime.GetView();
                if (view.Game.Phase == GamePhase.Evening && view.Cue == null && !view.TerminalOpen) return;
                RuntimeResult result;
                if (view.Cue != null) result = runtime.CompleteCue(view.Cue.Token);
                else if (view.Input == InputContext.Dialogue)
                    result = view.Game.EncounterStage == EncounterStage.AwaitingDisposition
                        ? runtime.SubmitDisposition(view.Game.CurrentEncounterId, Disposition.Treat)
                        : runtime.SelectResponse(view.Game.CurrentEncounterId, view.Game.CurrentNodeId, "neutral");
                else result = runtime.TryInteract(view.Game.Phase == GamePhase.MorningInspection ? InteractionTarget.Door : InteractionTarget.Computer);
                Assert.That(result.Accepted, Is.True, "The lighting fixture must advance through accepted game commands: " + result.Reason);
            }
            Assert.Fail("The lighting fixture did not reach its first evening.");
        }
    }
}
