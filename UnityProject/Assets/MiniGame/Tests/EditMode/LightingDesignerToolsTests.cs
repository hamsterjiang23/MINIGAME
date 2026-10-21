using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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
    public sealed class LightingDesignerToolsTests
    {
        private const string FolderKey = "MiniGame.LightingDesignerToolsTests.Folder";
        private const string PreviousSceneKey = "MiniGame.LightingDesignerToolsTests.PreviousScene";
        private const string SourceJsonKey = "MiniGame.LightingDesignerToolsTests.SourceJson";
        private const string PlayPhaseKey = "MiniGame.LightingDesignerToolsTests.PlayPhase";
        private const string LightPathKey = "MiniGame.LightingDesignerToolsTests.LightPath";
        private const string Prefix = "Assets/__LightingDesignerToolsTests_";

        private sealed class Fixture
        {
            public Scene Scene;
            public GameObject Root;
            public RoomLightingController Controller;
            public RoomLightingProfile Profile;
            public Light[] Extra, Managed;
            public Light Bed;
        }

        [UnityTearDown]
        public IEnumerator Cleanup()
        {
            if (EditorApplication.isPlaying) yield return new ExitPlayMode();
            CleanupFixture();
        }

        private static void CleanupFixture()
        {
            string folder = SessionState.GetString(FolderKey, "");
            if (string.IsNullOrEmpty(folder)) return;
            RequireOwnedFolder(folder);
            var scenes = Enumerable.Range(0, SceneManager.sceneCount).Select(SceneManager.GetSceneAt).ToArray();
            var owned = scenes.Where(scene => scene.path.StartsWith(folder + "/", StringComparison.Ordinal)).ToArray();
            var survivors = scenes.Where(scene => !owned.Any(item => item.handle == scene.handle)).ToArray();
            if (survivors.Length == 0) EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            else
            {
                SceneManager.SetActiveScene(survivors[0]);
                foreach (var scene in owned) if (scene.IsValid() && scene.isLoaded) EditorSceneManager.CloseScene(scene, true);
                var previous = SceneManager.GetSceneByPath(SessionState.GetString(PreviousSceneKey, ""));
                if (previous.IsValid() && previous.isLoaded) SceneManager.SetActiveScene(previous);
            }
            var source = Resources.Load<RoomLightingProfile>("RoomLightingDefault");
            string original = SessionState.GetString(SourceJsonKey, "");
            bool sourceUnchanged = source != null && JsonUtility.ToJson(source) == original;
            var asset = AssetDatabase.LoadAssetAtPath<RoomLightingProfile>(folder + "/Profile.asset");
            if (asset != null) Undo.ClearUndo(asset);
            bool deleted = AssetDatabase.DeleteAsset(folder);
            SessionState.EraseString(FolderKey); SessionState.EraseString(PreviousSceneKey); SessionState.EraseString(SourceJsonKey);
            SessionState.EraseString(LightPathKey); SessionState.EraseInt(PlayPhaseKey);
            Assert.That(deleted, Is.True, "Only the fixture's validated temporary asset directory is removed.");
            Assert.That(sourceUnchanged, Is.True, "Designer tests must leave the actual shared profile unchanged.");
        }

        [Test]
        public void SceneViewCameraHasTheExistingRetroImageEffectEnabled()
        {
            Assert.That(Attribute.GetCustomAttribute(typeof(RetroScreenEffect), typeof(ImageEffectAllowedInSceneView)), Is.Not.Null,
                "Scene View should use the same supported retro image effect without relying on an invisible batch-mode GUI.");
        }

        [Test]
        public void FiveSupplementalLightsUseUnambiguousScenePathsAndProtectManagedLights()
        {
            var fixture = CreateFixture();
            Assert.That(RoomLightingAuthoring.FindController(), Is.SameAs(fixture.Controller));
            Assert.That(RoomLightingAuthoring.GetEditableLights(fixture.Controller), Is.EquivalentTo(fixture.Extra));
            Assert.That(fixture.Profile.SceneLights, Is.Empty);
            var paths = fixture.Extra.Select(fixture.Controller.GetHierarchyPath).ToArray();
            Assert.That(paths.Distinct().Count(), Is.EqualTo(5), "Repeated and slash-containing hierarchy names must retain separate identities.");
            for (int index = 0; index < fixture.Extra.Length; index++)
            {
                Assert.That(RoomLightingAuthoring.ResolveLight(fixture.Controller, paths[index]), Is.SameAs(fixture.Extra[index]));
                var read = RoomLightingAuthoring.ReadLight(fixture.Controller, fixture.Extra[index]);
                float before = fixture.Extra[index].intensity;
                read.intensity = 99;
                Assert.That(fixture.Extra[index].intensity, Is.EqualTo(before), "Reading returns a detached value without authoring an override.");
            }
            Assert.That(fixture.Profile.SceneLights, Is.Empty);
            foreach (var light in fixture.Managed.Concat(new[] { fixture.Bed }))
            {
                Assert.That(fixture.Controller.IsEditableSceneLight(light), Is.False);
                Assert.That(fixture.Controller.CaptureSceneLight(light), Is.Null);
                Assert.Throws<ArgumentException>(() => RoomLightingAuthoring.ReadLight(fixture.Controller, light));
            }
            var other = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            var foreignRoot = new GameObject("Fixture root");
            var foreignGroup = new GameObject("Group/West#"); foreignGroup.transform.SetParent(foreignRoot.transform);
            var foreign = LightObject("Lamp", foreignGroup.transform);
            Assert.That(foreign.gameObject.scene.handle, Is.EqualTo(other.handle));
            Assert.That(fixture.Controller.GetHierarchyPath(foreign), Is.Empty);
            Assert.That(fixture.Controller.IsEditableSceneLight(foreign), Is.False);
            Assert.That(RoomLightingAuthoring.ResolveLight(fixture.Controller, paths[0]), Is.SameAs(fixture.Extra[0]),
                "An identical hierarchy in the active foreign scene must not redirect resolution.");
            Assert.Throws<ArgumentException>(() => RoomLightingAuthoring.WriteLight(fixture.Controller, foreign, new SceneLightSettings()));
            SceneManager.SetActiveScene(fixture.Scene);
            Assert.That(EditorSceneManager.SaveScene(other, Folder + "/Other.unity"), Is.True);
            RoomLightingAuthoring.SelectLight(fixture.Controller, fixture.Extra[2]);
            Assert.That(Selection.activeGameObject, Is.SameAs(fixture.Extra[2].gameObject));
            Assert.That(fixture.Profile.SceneLights, Is.Empty);
        }

        [Test]
        public void FirstOverrideImmediatelyUpdatesTheLightAndUndoRedoRestoresBothAssetAndScene()
        {
            var fixture = CreateFixture(); var light = fixture.Extra[0];
            var original = RoomLightingAuthoring.ReadLight(fixture.Controller, light);
            var edited = Edited(original);
            Undo.IncrementCurrentGroup();
            RoomLightingAuthoring.WriteLight(fixture.Controller, light, edited);
            Undo.FlushUndoRecordObjects();
            Assert.That(fixture.Profile.SceneLights.Count, Is.EqualTo(1));
            AssertLight(light, edited);
            Undo.PerformUndo();
            Assert.That(fixture.Profile.SceneLights, Is.Empty, "Undoing the first edit must remove its asset override.");
            AssertLight(light, original);
            Undo.PerformRedo();
            Assert.That(fixture.Profile.SceneLights.Count, Is.EqualTo(1));
            AssertLight(light, edited);
        }

        [Test]
        public void ConsumedMouseUpEndsEachSceneHandleUndoGroupIndependently()
        {
            var fixture = CreateFixture(); var light = fixture.Extra[0];
            float original = light.intensity;
            var window = ScriptableObject.CreateInstance<RoomLightingWindow>();
            try
            {
                int previousGroup = -1;
                foreach (float intensity in new[] { 1.4f, 2.8f })
                {
                    window.BeginSceneHandleInteraction();
                    Assert.That(window.SceneHandleUndoGroup, Is.GreaterThanOrEqualTo(0));
                    Assert.That(window.SceneHandleUndoGroup, Is.Not.EqualTo(previousGroup));
                    previousGroup = window.SceneHandleUndoGroup;
                    var value = RoomLightingAuthoring.ReadLight(fixture.Controller, light); value.intensity = intensity;
                    RoomLightingAuthoring.WriteLight(fixture.Controller, light, value);
                    Undo.FlushUndoRecordObjects();
                    var mouseUp = new Event { type = EventType.MouseUp };
                    EventType raw = mouseUp.rawType; mouseUp.Use();
                    Assert.That(mouseUp.type, Is.EqualTo(EventType.Used));
                    window.CompleteSceneHandleEvent(raw);
                    Assert.That(window.SceneHandleUndoGroup, Is.EqualTo(-1));
                }
                Undo.PerformUndo();
                Assert.That(light.intensity, Is.EqualTo(1.4f).Within(.0001f));
                Assert.That(fixture.Profile.SceneLights.Single().intensity, Is.EqualTo(1.4f).Within(.0001f));
                Undo.PerformUndo();
                Assert.That(light.intensity, Is.EqualTo(original).Within(.0001f));
                Assert.That(fixture.Profile.SceneLights, Is.Empty);
            }
            finally { Object.DestroyImmediate(window); }
        }

        [Test]
        public void SavingTheTemporaryAssetReappliesItsValuesAfterARealSceneReload()
        {
            var fixture = CreateFixture(); var light = fixture.Extra[4];
            string path = fixture.Controller.GetHierarchyPath(light);
            var edited = Edited(RoomLightingAuthoring.ReadLight(fixture.Controller, light));
            edited.HierarchyPath = "caller must not redirect this edit";
            RoomLightingAuthoring.WriteLight(fixture.Controller, light, edited);
            RoomLightingAuthoring.SaveProfile(fixture.Profile);
            Assert.That(EditorUtility.IsDirty(fixture.Profile), Is.False);
            string scenePath = fixture.Scene.path;
            SceneManager.SetActiveScene(SceneManager.GetSceneByPath(Folder + "/Harness.unity").IsValid()
                ? SceneManager.GetSceneByPath(Folder + "/Harness.unity") : fixture.Scene);
            Assert.That(EditorSceneManager.CloseScene(fixture.Scene, true), Is.True);
            AssetDatabase.ImportAsset(Folder + "/Profile.asset", ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
            var reopened = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
            SceneManager.SetActiveScene(reopened);
            var controller = RoomLightingAuthoring.FindController();
            var stored = AssetDatabase.LoadAssetAtPath<RoomLightingProfile>(Folder + "/Profile.asset");
            Assert.That(controller.Profile, Is.SameAs(stored));
            Assert.That(stored.SceneLights.Single().HierarchyPath, Is.EqualTo(path), "The service chooses the actual complete path.");
            controller.ApplyFromProfile();
            AssertLight(RoomLightingAuthoring.ResolveLight(controller, path), edited);
            var transient = Object.Instantiate(stored);
            string storedJson = JsonUtility.ToJson(stored);
            try
            {
                int revision = transient.Revision; transient.lampIntensity = 6.2f;
                Assert.DoesNotThrow(() => RoomLightingProfileEditor.ApplyInspectorChanges(transient));
                Assert.That(transient.Revision, Is.GreaterThan(revision));
                Assert.That(JsonUtility.ToJson(stored), Is.EqualTo(storedJson), "Inspecting a nonpersistent copy cannot write the asset.");
            }
            finally { Object.DestroyImmediate(transient); }
        }

        [UnityTest, Timeout(90000)]
        public IEnumerator PlayModeWritesAndProfileInspectorEditsSurviveReturningToEditMode()
        {
            if (!EditorApplication.isPlaying && SessionState.GetInt(PlayPhaseKey, 0) == 0)
            {
                CreateFixture();
                SessionState.SetInt(PlayPhaseKey, 1);
                ObserveFixture("before-enter-play");
                yield return new EnterPlayMode();
            }
            if (EditorApplication.isPlaying)
            {
                EditFixtureDuringPlayMode();
                yield return new ExitPlayMode();
            }
            Assert.That(EditorApplication.isPlaying, Is.False);
            Assert.That(SessionState.GetInt(PlayPhaseKey, 0), Is.EqualTo(2));
            ObserveFixture("after-exit-before-import", true);
            AssetDatabase.ImportAsset(Folder + "/Profile.asset", ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
            yield return null; yield return null;
            AssertSavedFixtureAfterPlayMode();
        }

        // Only SessionState crosses domain reloads; closures and Unity object locals stay in synchronous helpers.
        private static void EditFixtureDuringPlayMode()
        {
            ObserveFixture("after-enter-play");
            bool wasPaused = EditorApplication.isPaused;
            try
            {
                EditorApplication.isPaused = true;
                Assert.That(EditorApplication.isPaused, Is.True);
                var controller = FixtureController();
                Assert.That(controller.Profile, Is.Not.Null, "The playing fixture must still reference the temporary persistent profile.");
                var light = RoomLightingAuthoring.GetEditableLights(controller).Single(item => item.name == "Rim");
                string path = controller.GetHierarchyPath(light);
                var edited = Edited(RoomLightingAuthoring.ReadLight(controller, light));
                RoomLightingAuthoring.WriteLight(controller, light, edited);
                AssertLight(light, edited);
                Assert.That(EditorUtility.IsPersistent(controller.Profile), Is.True);
                Assert.That(AssetDatabase.GetAssetPath(controller.Profile), Is.EqualTo(Folder + "/Profile.asset"));
                using (var serialized = new SerializedObject(controller.Profile))
                {
                    serialized.FindProperty("lampIntensity").floatValue = 3.35f;
                    serialized.FindProperty("lampEulerAngles").vector3Value = new Vector3(55, 10, 5);
                    serialized.ApplyModifiedProperties();
                }
                RoomLightingProfileEditor.ApplyInspectorChanges(controller.Profile);
                Assert.That(controller.Profile.lampIntensity, Is.EqualTo(3.35f).Within(.0001f));
                SessionState.SetString(LightPathKey, path);
            }
            finally { EditorApplication.isPaused = wasPaused; }
            SessionState.SetInt(PlayPhaseKey, 2);
            ObserveFixture("before-exit-play");
        }

        private static void AssertSavedFixtureAfterPlayMode()
        {
            ObserveFixture("after-import");
            var restored = FixtureController();
            string savedPath = SessionState.GetString(LightPathKey, "");
            Assert.That(savedPath, Is.Not.Empty, "The selected light path must survive the play-mode transition in SessionState.");
            var saved = AssetDatabase.LoadAssetAtPath<RoomLightingProfile>(Folder + "/Profile.asset");
            Assert.That(saved, Is.Not.Null, "The temporary profile must load after forced reimport; see the before-import raw asset snapshot.");
            Assert.That(saved.SceneLights, Is.Not.Null, "The reimported asset must retain its serialized SceneLights list.");
            var entries = saved.SceneLights.Where(item => item != null && item.HierarchyPath == savedPath).ToArray();
            Assert.That(entries.Length, Is.EqualTo(1), "The reimported asset must contain exactly one override for the selected light path.");
            var entry = entries[0];
            Assert.That(entry.intensity, Is.EqualTo(2.25f).Within(.0001f), "Scene-light edits must be saved automatically without SaveProfile.");
            Assert.That(saved.lampIntensity, Is.EqualTo(3.35f).Within(.0001f), "Base profile Inspector changes must also survive automatic save on exit.");
            Assert.That(saved.lampEulerAngles, Is.EqualTo(new Vector3(55, 10, 5)));
            Assert.That(restored.Profile, Is.SameAs(saved));
            AssertLight(RoomLightingAuthoring.ResolveLight(restored, savedPath), entry);
        }

        private static string Folder => SessionState.GetString(FolderKey, "");
        private static Fixture CreateFixture()
        {
            Assert.That(Folder, Is.Empty, "A prior fixture must be cleaned before creating another one.");
            string folder = Prefix + Guid.NewGuid().ToString("N");
            AssetDatabase.CreateFolder("Assets", folder.Substring("Assets/".Length));
            SessionState.SetString(FolderKey, folder);
            var previous = SceneManager.GetActiveScene(); SessionState.SetString(PreviousSceneKey, previous.path);
            var unsaved = Enumerable.Range(0, SceneManager.sceneCount).Select(SceneManager.GetSceneAt)
                .Where(scene => string.IsNullOrEmpty(scene.path)).ToArray();
            for (int index = 0; index < unsaved.Length; index++)
                Assert.That(EditorSceneManager.SaveScene(unsaved[index], folder + (index == 0 ? "/Harness.unity" : "/Harness" + index + ".unity")), Is.True);
            var source = Resources.Load<RoomLightingProfile>("RoomLightingDefault");
            Assert.That(source, Is.Not.Null); SessionState.SetString(SourceJsonKey, JsonUtility.ToJson(source));
            var profile = Object.Instantiate(source); profile.name = "Temporary designer profile";
            profile.SceneLights = new List<SceneLightSettings>(); profile.NotifyChanged();
            AssetDatabase.CreateAsset(profile, folder + "/Profile.asset");
            var fixture = new Fixture { Scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive), Profile = profile };
            SceneManager.SetActiveScene(fixture.Scene);
            fixture.Root = new GameObject("Fixture root");
            fixture.Managed = new[] { "Ceiling", "Window", "Fill", "Exterior", "Monitor" }.Select(name => LightObject(name, fixture.Root.transform)).ToArray();
            fixture.Bed = LightObject("Bed guide", fixture.Root.transform); fixture.Bed.type = LightType.Spot;
            var cameraObject = new GameObject("Fixture camera", typeof(Camera)); cameraObject.transform.SetParent(fixture.Root.transform);
            cameraObject.GetComponent<Camera>().enabled = false;
            var guidance = fixture.Root.AddComponent<BedtimeGuidance>();
            guidance.Configure(cameraObject.GetComponent<Camera>(), fixture.Bed, Array.Empty<Renderer>(), Vector3.zero, 1);
            var first = new GameObject("Group/West#"); first.transform.SetParent(fixture.Root.transform);
            var second = new GameObject("Group/West#"); second.transform.SetParent(fixture.Root.transform);
            fixture.Extra = new[] { LightObject("Lamp", first.transform), LightObject("Lamp", first.transform),
                LightObject("Lamp", second.transform), LightObject("Supplemental fill", fixture.Root.transform), LightObject("Rim", fixture.Root.transform) };
            fixture.Controller = fixture.Root.AddComponent<RoomLightingController>();
            fixture.Controller.Configure(profile, fixture.Managed[0], fixture.Managed[1], fixture.Managed[2], fixture.Managed[3], fixture.Managed[4],
                null, null, 0, Array.Empty<Renderer>(), null, guidance);
            Assert.That(EditorSceneManager.SaveScene(fixture.Scene, folder + "/Fixture.unity"), Is.True);
            AssetDatabase.SaveAssetIfDirty(profile);
            return fixture;
        }
        private static Light LightObject(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(Light)); go.transform.SetParent(parent, false);
            var light = go.GetComponent<Light>(); light.intensity = .7f; light.range = 4;
            return light;
        }
        private static RoomLightingController FixtureController()
        {
            Assert.That(Folder, Is.Not.Empty, "The fixture directory must survive in SessionState.");
            var controllers = Resources.FindObjectsOfTypeAll<RoomLightingController>()
                .Where(controller => controller != null && controller.gameObject.scene.IsValid() && controller.gameObject.scene.isLoaded
                    && controller.Profile != null && AssetDatabase.GetAssetPath(controller.Profile) == Folder + "/Profile.asset").ToArray();
            Assert.That(controllers.Length, Is.EqualTo(1), "Exactly one loaded fixture controller must reference the temporary asset; see fixture observations.");
            Assert.That(controllers[0], Is.Not.Null);
            Assert.That(controllers[0].Profile, Is.Not.Null);
            return controllers[0];
        }
        private static void ObserveFixture(string stage, bool preserveRawAsset = false)
        {
            string folder = Folder;
            Assert.That(folder, Is.Not.Empty, stage + ": the fixture directory is absent from SessionState.");
            RequireOwnedFolder(folder);
            string assetPath = folder + "/Profile.asset";
            string absolute = Path.GetFullPath(Path.Combine(Application.dataPath, "..", assetPath));
            bool exists = File.Exists(absolute);
            var loaded = AssetDatabase.LoadAssetAtPath<RoomLightingProfile>(assetPath);
            var scenes = Enumerable.Range(0, SceneManager.sceneCount).Select(SceneManager.GetSceneAt)
                .Select(scene => scene.path + "|loaded=" + scene.isLoaded + "|dirty=" + scene.isDirty).ToArray();
            var controllers = Resources.FindObjectsOfTypeAll<RoomLightingController>()
                .Where(controller => controller != null && controller.gameObject.scene.IsValid() && controller.gameObject.scene.isLoaded)
                .Select(controller =>
                {
                    var profile = controller.Profile;
                    string identity = controller.gameObject.scene.path + "/" + controller.name + "|profile="
                        + (profile == null ? "<null>" : AssetDatabase.GetAssetPath(profile));
                    if (profile == null) return identity;
                    var lights = profile.SceneLights == null ? new[] { "<null-list>" } : profile.SceneLights.Select(entry =>
                    {
                        if (entry == null) return "<null-entry>";
                        var actual = controller.ResolveSceneLight(entry.HierarchyPath);
                        return entry.HierarchyPath + "|storedPos=" + entry.localPosition.ToString("F4")
                            + "|actualPos=" + (actual == null ? "<unresolved>" : actual.transform.localPosition.ToString("F4"));
                    }).ToArray();
                    return identity + "|revision=" + profile.Revision + "|lights=[" + string.Join(";", lights) + "]";
                }).ToArray();
            string observation = "[LightingDesignerToolsTests] stage=" + stage + " playing=" + EditorApplication.isPlaying
                + " paused=" + EditorApplication.isPaused + " phase=" + SessionState.GetInt(PlayPhaseKey, 0) + " folder=" + folder + " fileExists=" + exists
                + " loaded=" + (loaded != null) + " entries=" + (loaded == null || loaded.SceneLights == null ? -1 : loaded.SceneLights.Count)
                + " loadedRevision=" + (loaded == null ? -1 : loaded.Revision)
                + " lightPath=" + SessionState.GetString(LightPathKey, "") + " scenes=[" + string.Join(";", scenes)
                + "] controllers=[" + string.Join(";", controllers) + "]";
            Debug.Log(observation);
            if (preserveRawAsset && exists)
            {
                var directory = new DirectoryInfo(Path.GetFullPath(Path.Combine(Application.dataPath, "..")));
                while (directory != null && !(Directory.Exists(Path.Combine(directory.FullName, "UnityProject"))
                    && File.Exists(Path.Combine(directory.FullName, "src", "MiniGame.sln")))) directory = directory.Parent;
                Assert.That(directory, Is.Not.Null, "The raw diagnostic asset must be written into this repository's Artifacts directory.");
                string artifact = Path.Combine(directory.FullName, "Artifacts", "LightingTools");
                Directory.CreateDirectory(artifact);
                File.Copy(absolute, Path.Combine(artifact, "failed-profile-before-import.asset"), true);
                File.WriteAllText(Path.Combine(artifact, "failed-profile-before-import-state.txt"), observation);
            }
            Assert.That(exists, Is.True, stage + ": the temporary Profile.asset is missing on disk.");
            Assert.That(loaded, Is.Not.Null, stage + ": LoadAssetAtPath did not return the temporary RoomLightingProfile.");
            Assert.That(loaded.SceneLights, Is.Not.Null, stage + ": the temporary profile has a null SceneLights list.");
        }
        private static SceneLightSettings Edited(SceneLightSettings original)
        {
            var value = original.Copy(); value.localPosition = new Vector3(4, 5, 6); value.localEuler = new Vector3(20, 30, 40);
            value.dayEnabled = true; value.color = new Color(.6f, .4f, .2f); value.intensity = 2.25f; value.range = 6.75f;
            value.type = LightType.Spot; value.spotAngle = 80; value.innerSpotAngle = 35;
            value.shadows = LightShadows.Soft; value.shadowBias = .07f; value.shadowNormalBias = .25f;
            return value;
        }
        private static void AssertLight(Light light, SceneLightSettings settings)
        {
            Assert.That(light, Is.Not.Null); Assert.That(light.enabled, Is.EqualTo(settings.dayEnabled));
            Assert.That(light.transform.localPosition, Is.EqualTo(settings.localPosition));
            Assert.That(Quaternion.Angle(light.transform.localRotation, Quaternion.Euler(settings.localEuler)), Is.LessThan(.01f));
            Assert.That(light.intensity, Is.EqualTo(settings.intensity).Within(.0001f));
            Assert.That(light.range, Is.EqualTo(settings.range).Within(.0001f)); Assert.That(light.color, Is.EqualTo(settings.color));
            Assert.That(light.type, Is.EqualTo(settings.type)); Assert.That(light.shadows, Is.EqualTo(settings.shadows));
            Assert.That(light.spotAngle, Is.EqualTo(settings.spotAngle).Within(.0001f));
            Assert.That(light.innerSpotAngle, Is.EqualTo(settings.innerSpotAngle).Within(.0001f));
            Assert.That(light.shadowBias, Is.EqualTo(settings.shadowBias).Within(.0001f));
            Assert.That(light.shadowNormalBias, Is.EqualTo(settings.shadowNormalBias).Within(.0001f));
        }
        private static void RequireOwnedFolder(string folder)
        {
            if (!folder.StartsWith(Prefix, StringComparison.Ordinal) || !Guid.TryParseExact(folder.Substring(Prefix.Length), "N", out _))
                throw new InvalidOperationException("Refusing to clean an unowned lighting test directory: " + folder);
            string absolute = Path.GetFullPath(Path.Combine(Application.dataPath, "..", folder));
            if (!absolute.StartsWith(Path.GetFullPath(Application.dataPath) + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Lighting test directory escaped this project's Assets folder.");
        }
    }
}
