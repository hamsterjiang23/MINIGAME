using System;
using System.Collections.Generic;
using System.Linq;
using MiniGame.UnityHost.Editor;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MiniGame.UnityHost.Tests
{
    public sealed class IndependentPeriodLightingTests
    {
        private readonly List<Scene> owned = new List<Scene>();
        private RoomLightingController room;
        private RoomLightingProfile day, night;
        private Light fill;
        private string folder;
        private Scene previous;

        [SetUp]
        public void CreateIndependentRoom()
        {
            previous = SceneManager.GetActiveScene();
            folder = "Assets/__IndependentPeriodTests_" + Guid.NewGuid().ToString("N");
            AssetDatabase.CreateFolder("Assets", folder.Substring(7));
            var untitled = Enumerable.Range(0, SceneManager.sceneCount).Select(SceneManager.GetSceneAt)
                .Where(scene => string.IsNullOrEmpty(scene.path)).ToArray();
            for (int index = 0; index < untitled.Length; index++)
            {
                Assert.That(EditorSceneManager.SaveScene(untitled[index], folder + "/Harness" + index + ".unity"), Is.True);
                owned.Add(untitled[index]);
            }
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive); owned.Add(scene);
            var root = new GameObject("Independent room");
            var camera = new GameObject("Camera", typeof(Camera)); camera.transform.SetParent(root.transform);
            var bed = new GameObject("Bed guide", typeof(Light)).GetComponent<Light>(); bed.transform.SetParent(root.transform);
            bed.enabled = false; bed.intensity = 8; bed.type = LightType.Spot;
            fill = new GameObject("Fill", typeof(Light)).GetComponent<Light>(); fill.transform.SetParent(root.transform);
            fill.intensity = .8f;
            var guidance = root.AddComponent<BedtimeGuidance>();
            guidance.Configure(camera.GetComponent<Camera>(), bed, Array.Empty<Renderer>(), Vector3.zero, 1);
            day = ScriptableObject.CreateInstance<RoomLightingProfile>(); day.name = "Complete day";
            night = ScriptableObject.CreateInstance<RoomLightingProfile>(); night.name = "Complete night";
            AssetDatabase.CreateAsset(day, folder + "/Day.asset"); AssetDatabase.CreateAsset(night, folder + "/Night.asset");
            room = root.AddComponent<RoomLightingController>();
            room.Configure(day, null, null, null, null, null, null, null, 0, Array.Empty<Renderer>(), null, guidance);
            var baseline = new[] {
                new SceneLightSettings { HierarchyPath = room.GetHierarchyPath(fill), intensity = .8f, color = fill.color, range = fill.range },
                new SceneLightSettings { HierarchyPath = room.GetHierarchyPath(bed), dayEnabled = false, intensity = 8, type = LightType.Spot }
            };
            day.SceneLightDefaults = baseline.Select(value => value.Copy()).ToList();
            night.SceneLightDefaults = baseline.Select(value => value.Copy()).ToList();
            room.ConfigureProfiles(day, night);
            AssetDatabase.SaveAssetIfDirty(day); AssetDatabase.SaveAssetIfDirty(night);
            Assert.That(EditorSceneManager.SaveScene(scene, folder + "/Room.unity"), Is.True);
        }

        [TearDown]
        public void Cleanup()
        {
            if (room != null) room.SetPreviewMode(RoomLightingPreviewMode.Automatic);
            if (day != null) Undo.ClearUndo(day); if (night != null) Undo.ClearUndo(night);
            var survivors = Enumerable.Range(0, SceneManager.sceneCount).Select(SceneManager.GetSceneAt)
                .Where(scene => !owned.Any(item => item.handle == scene.handle)).ToArray();
            if (survivors.Length == 0) EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            else SceneManager.SetActiveScene(previous.IsValid() && previous.isLoaded && !owned.Any(item => item.handle == previous.handle) ? previous : survivors[0]);
            foreach (var scene in owned) if (scene.IsValid() && scene.isLoaded) EditorSceneManager.CloseScene(scene, true);
            owned.Clear();
            Assert.That(folder, Does.StartWith("Assets/__IndependentPeriodTests_"));
            Assert.That(AssetDatabase.DeleteAsset(folder), Is.True);
        }

        [Test]
        public void SharedAssetBindingsAreRejectedWithoutExposingDayThroughNight()
        {
            Assert.Throws<ArgumentException>(() => room.ConfigureProfiles(day, day));
            Assert.That(room.GetProfile(RoomLightingPeriod.Night), Is.SameAs(night));
            var serialized = new SerializedObject(room);
            serialized.FindProperty("nightProfile").objectReferenceValue = day;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            Assert.That(room.HasIndependentProfiles, Is.False);
            Assert.That(room.GetProfile(RoomLightingPeriod.Night), Is.Null);
            room.ConfigureProfiles(day, night);
        }

        [Test]
        public void BothPeriodsExposeEveryLightAndNeitherBorrowsTheOthersValues()
        {
            Assert.That(room.GetEditableSceneLights(RoomLightingPeriod.Day), Is.EquivalentTo(room.GetEditableSceneLights(RoomLightingPeriod.Night)));
            Assert.That(ReferenceEquals(day.SceneLightDefaults[0], night.SceneLightDefaults[0]), Is.False);
            night.lightMultiplier = .25f; night.NotifyChanged();
            room.SetPreviewMode(RoomLightingPreviewMode.Night);
            Assert.That(fill.intensity, Is.EqualTo(.2f).Within(.001f));
            day.lightMultiplier = 2; day.NotifyChanged(); room.ApplyFromProfile();
            Assert.That(fill.intensity, Is.EqualTo(.2f).Within(.001f));
            room.SetPreviewMode(RoomLightingPreviewMode.Day);
            Assert.That(fill.intensity, Is.EqualTo(1.6f).Within(.001f));
            Assert.That(night.lightMultiplier, Is.EqualTo(.25f));
        }

        [Test]
        public void SavingDaySceneGeometryDoesNotChangeNightAfterReopening()
        {
            var changed = room.CaptureSceneLight(fill, RoomLightingPeriod.Day);
            changed.intensity = 2.7f; changed.localPosition = new Vector3(2, 1, 3);
            RoomLightingAuthoring.WriteLight(room, fill, changed, RoomLightingPeriod.Day);
            RoomLightingAuthoring.SaveProfile(day); RoomLightingAuthoring.SaveProfile(night);
            var scene = room.gameObject.scene;
            room.SetPreviewMode(RoomLightingPreviewMode.Night);
            Assert.That(EditorSceneManager.SaveScene(scene, folder + "/Room.unity"), Is.True);
            EditorSceneManager.CloseScene(scene, true);
            var reopened = EditorSceneManager.OpenScene(folder + "/Room.unity", OpenSceneMode.Additive);
            owned.Add(reopened); SceneManager.SetActiveScene(reopened);
            room = reopened.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<RoomLightingController>()).Single();
            fill = reopened.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<Light>()).Single(value => value.name == "Fill");
            room.SetPreviewMode(RoomLightingPreviewMode.Night);
            Assert.That(fill.intensity, Is.EqualTo(.8f).Within(.001f));
            Assert.That(fill.transform.localPosition, Is.EqualTo(Vector3.zero));
            room.SetPreviewMode(RoomLightingPreviewMode.Day);
            Assert.That(fill.intensity, Is.EqualTo(2.7f).Within(.001f));
            Assert.That(fill.transform.localPosition, Is.EqualTo(new Vector3(2, 1, 3)));
        }

        [Test]
        public void ImportingACompleteNightFileDoesNotModifyTheDayAsset()
        {
            string dayBefore = JsonUtility.ToJson(day);
            var incoming = UnityEngine.Object.Instantiate(night);
            try
            {
                incoming.lightMultiplier = .5f; incoming.ambientColor = Color.blue; incoming.lampIntensity = 1.7f;
                incoming.SceneLightDefaults[0].intensity = 3;
                string json = RoomLightingPresetIO.ExportJson(incoming);
                Undo.RecordObject(night, "Import independent night");
                Assert.That(RoomLightingPresetIO.TryImportJson(json, night, out string message), Is.True, message);
                RoomLightingAuthoring.ProfileEdited(night);
                Assert.That(JsonUtility.ToJson(day), Is.EqualTo(dayBefore));
                Assert.That(night.lampIntensity, Is.EqualTo(1.7f));
                room.SetPreviewMode(RoomLightingPreviewMode.Night);
                Assert.That(fill.intensity, Is.EqualTo(1.5f).Within(.001f));
            }
            finally { UnityEngine.Object.DestroyImmediate(incoming); }
        }
    }
}
