using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MiniGame.UnityHost.Editor;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace MiniGame.UnityHost.Tests
{
    public sealed class DayNightLightingToolsTests
    {
        private Scene previous;
        private Object previousSelection;
        private readonly List<Scene> owned = new List<Scene>();
        private string folder;
        private RoomLightingProfile profile;
        private RoomLightingController controller;
        private Light light;

        [SetUp]
        public void CreateRoom()
        {
            previous = SceneManager.GetActiveScene(); previousSelection = Selection.activeObject;
            folder = "Assets/__DayNightLightingTests_" + Guid.NewGuid().ToString("N");
            AssetDatabase.CreateFolder("Assets", folder.Substring("Assets/".Length));
            profile = ScriptableObject.CreateInstance<RoomLightingProfile>();
            AssetDatabase.CreateAsset(profile, folder + "/Lighting.asset");
            var untitled = Enumerable.Range(0, SceneManager.sceneCount).Select(SceneManager.GetSceneAt)
                .Where(scene => string.IsNullOrEmpty(scene.path)).ToArray();
            for (int index = 0; index < untitled.Length; index++)
            {
                Assert.That(EditorSceneManager.SaveScene(untitled[index], folder + "/Harness" + index + ".unity"), Is.True);
                owned.Add(untitled[index]);
            }
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            owned.Add(scene);
            RenderSettings.fog = true;
            var root = new GameObject("Day night test room");
            var camera = new GameObject("Room camera", typeof(Camera)); camera.transform.SetParent(root.transform);
            var bed = new GameObject("Bed spotlight", typeof(Light)); bed.transform.SetParent(root.transform);
            var guidance = root.AddComponent<BedtimeGuidance>();
            guidance.Configure(camera.GetComponent<Camera>(), bed.GetComponent<Light>(), Array.Empty<Renderer>(), Vector3.zero, 1);
            light = new GameObject("Supplemental light", typeof(Light)).GetComponent<Light>();
            light.transform.SetParent(root.transform); light.intensity = .8f; light.range = 4;
            controller = root.AddComponent<RoomLightingController>();
            controller.Configure(profile, null, null, null, null, null, null, null, 0, Array.Empty<Renderer>(), null, guidance);
            Assert.That(EditorSceneManager.SaveScene(scene, folder + "/Room.unity"), Is.True);
            Selection.activeObject = root;
        }

        [TearDown]
        public void Cleanup()
        {
            if (controller != null)
            {
                RoomLightingAuthoring.ClearEditingPeriod(controller);
                controller.SetPreviewMode(RoomLightingPreviewMode.Automatic);
            }
            if (profile != null) Undo.ClearUndo(profile);
            Selection.activeObject = previousSelection;
            var survivors = Enumerable.Range(0, SceneManager.sceneCount).Select(SceneManager.GetSceneAt)
                .Where(scene => !owned.Any(item => item.handle == scene.handle)).ToArray();
            if (survivors.Length == 0) EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            else SceneManager.SetActiveScene(previous.IsValid() && previous.isLoaded && !owned.Any(item => item.handle == previous.handle)
                ? previous : survivors[0]);
            foreach (var scene in owned) if (scene.IsValid() && scene.isLoaded) EditorSceneManager.CloseScene(scene, true);
            owned.Clear();
            Assert.That(folder, Does.StartWith("Assets/__DayNightLightingTests_"));
            Assert.That(AssetDatabase.DeleteAsset(folder), Is.True);
        }

        [Test]
        public void SelectedRoomResolvesEvenWhenAnotherAdditiveSceneIsActive()
        {
            var other = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            owned.Add(other); SceneManager.SetActiveScene(other);
            Selection.activeGameObject = light.gameObject;
            Assert.That(RoomLightingAuthoring.FindController(), Is.SameAs(controller));
            Assert.That(RoomLightingAuthoring.FindControllerForObject(light.transform), Is.SameAs(controller));
            Assert.That(RoomLightingAuthoring.GetEditableLights(controller), Does.Contain(light));
        }

        [Test]
        public void SavingAnInactiveNightRoomPreservesItsDayFogAfterReload()
        {
            var roomScene = controller.gameObject.scene;
            string path = folder + "/Room.unity";
            controller.SetPreviewMode(RoomLightingPreviewMode.Night);
            Assert.That(RenderSettings.fog, Is.False);
            var other = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            owned.Add(other); SceneManager.SetActiveScene(other);
            Assert.That(EditorSceneManager.SaveScene(roomScene, path), Is.True);
            Assert.That(SceneManager.GetActiveScene().handle, Is.EqualTo(other.handle), "Saving another room must preserve the editor's active scene.");
            EditorSceneManager.CloseScene(roomScene, true);
            var reopened = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
            owned.Add(reopened); SceneManager.SetActiveScene(reopened);
            Assert.That(RenderSettings.fog, Is.True, "The room's day environment must not be replaced by its temporary night preview.");
        }

        [Test]
        public void SavingNightPreviewDoesNotBakeNightIntoUncoveredDayLights()
        {
            string path = folder + "/Room.unity";
            var roomScene = controller.gameObject.scene;
            controller.SetPreviewMode(RoomLightingPreviewMode.Night);
            Assert.That(light.intensity, Is.Zero.Within(.001f));
            Assert.That(EditorSceneManager.SaveScene(roomScene, path), Is.True);
            Assert.That(controller.PreviewMode, Is.EqualTo(RoomLightingPreviewMode.Night), "Saving must restore the working preview.");
            EditorSceneManager.CloseScene(roomScene, true);
            var reopened = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
            owned.Add(reopened); SceneManager.SetActiveScene(reopened);
            controller = reopened.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<RoomLightingController>()).Single();
            light = reopened.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<Light>())
                .Single(item => item.name == "Supplemental light");
            controller.SetPreviewMode(RoomLightingPreviewMode.Automatic);
            Assert.That(light.enabled, Is.True);
            Assert.That(light.intensity, Is.EqualTo(.8f).Within(.001f), "Night preview must not replace an uncovered scene light's saved daylight.");
        }

        [UnityTest]
        public IEnumerator InactiveRoomPreviewDoesNotOverwriteAnotherScenesEnvironment()
        {
            var roomScene = controller.gameObject.scene;
            var other = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            owned.Add(other); SceneManager.SetActiveScene(other);
            var ambient = new Color(.1f, .2f, .3f);
            RenderSettings.ambientLight = ambient;
            profile.night.ambientColor = Color.red; profile.NotifyChanged();
            controller.SetPreviewMode(RoomLightingPreviewMode.Night);
            Assert.That(RenderSettings.ambientLight, Is.EqualTo(ambient));
            SceneManager.SetActiveScene(roomScene);
            yield return null; // ExecuteAlways observes Editor active-scene changes on the next update.
            Assert.That(RenderSettings.ambientLight, Is.EqualTo(Color.red));
        }

        [Test]
        public void SeparatePeriodsPreviewAndFirstNightUndoRestoreTheOriginalDaylight()
        {
            var night = controller.CaptureSceneLight(light, RoomLightingPeriod.Night);
            night.dayEnabled = true; night.intensity = 2.7f; night.color = Color.blue;
            night.localPosition = new Vector3(1, 2, 3); night.shadowStrength = .45f;
            Undo.IncrementCurrentGroup();
            RoomLightingAuthoring.WriteLight(controller, light, night, RoomLightingPeriod.Night);
            Undo.FlushUndoRecordObjects();
            Assert.That(profile.SceneLights, Is.Empty);
            Assert.That(profile.night.SceneLights.Count, Is.EqualTo(1));
            controller.SetPreviewMode(RoomLightingPreviewMode.Night);
            Assert.That(light.intensity, Is.EqualTo(2.7f).Within(.001f));
            Assert.That(light.transform.localPosition, Is.EqualTo(night.localPosition));
            Undo.PerformUndo();
            Assert.That(profile.night.SceneLights, Is.Empty);
            Assert.That(light.intensity, Is.Zero.Within(.001f));
            Undo.PerformRedo();
            Assert.That(light.intensity, Is.EqualTo(2.7f).Within(.001f));
            controller.SetPreviewMode(RoomLightingPreviewMode.Day);
            Assert.That(light.intensity, Is.EqualTo(.8f).Within(.001f));
            Assert.That(light.transform.localPosition, Is.EqualTo(Vector3.zero));
            controller.SetPreviewMode(RoomLightingPreviewMode.Automatic);
            Assert.That(light.intensity, Is.EqualTo(.8f).Within(.001f));
        }

        [UnityTest]
        public IEnumerator NativeInspectorChangesSaveNightWithoutAWindowOrCurrentLightSelection()
        {
            RoomLightingAuthoring.SetEditingPeriod(controller, RoomLightingPeriod.Night);
            Selection.activeObject = controller.gameObject;
            Undo.IncrementCurrentGroup(); Undo.RecordObject(light, "Locked inspector light edit");
            light.intensity = 3.2f;
            Undo.FlushUndoRecordObjects();
            yield return null;
            Assert.That(profile.night.SceneLights.Count, Is.EqualTo(1),
                "The global authoring callback must follow the modified Light rather than the current Selection.");
            Assert.That(profile.night.SceneLights.Single().intensity, Is.EqualTo(3.2f).Within(.001f));
            Assert.That(profile.SceneLights, Is.Empty);
            RoomLightingAuthoring.SaveProfile(profile);
            var saved = AssetDatabase.LoadAssetAtPath<RoomLightingProfile>(folder + "/Lighting.asset");
            Assert.That(saved.night.SceneLights.Single().intensity, Is.EqualTo(3.2f).Within(.001f));
        }
    }
}
