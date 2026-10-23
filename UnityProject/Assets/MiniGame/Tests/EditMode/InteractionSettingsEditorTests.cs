using System;
using System.Linq;
using MiniGame.Core;
using MiniGame.UnityHost.Editor;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MiniGame.UnityHost.Tests
{
    [Category("InteractionWakeGm")]
    public sealed class InteractionSettingsEditorTests
    {
        private Scene scene;
        private Scene otherScene;
        private Scene previousScene;
        private string path;
        private string otherPath;
        private string renamedPath;
        private string harnessPath;

        [SetUp]
        public void SetUp()
        {
            previousScene = SceneManager.GetActiveScene();
            path = "Assets/InteractionSettingsTest-" + Guid.NewGuid().ToString("N") + ".unity";
            otherPath = path.Replace(".unity", "-other.unity");
            renamedPath = path.Replace(".unity", "-renamed.unity");
            if (string.IsNullOrEmpty(previousScene.path))
            {
                harnessPath = path.Replace(".unity", "-harness.unity");
                Assert.That(EditorSceneManager.SaveScene(previousScene, harnessPath), Is.True);
            }
            scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            Assert.That(EditorSceneManager.SaveScene(scene, path), Is.True);
            otherScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            Assert.That(EditorSceneManager.SaveScene(otherScene, otherPath), Is.True);
        }

        [TearDown]
        public void TearDown()
        {
            if (scene.IsValid() && scene.isLoaded) EditorSceneManager.CloseScene(scene, true);
            if (otherScene.IsValid() && otherScene.isLoaded) EditorSceneManager.CloseScene(otherScene, true);
            if (previousScene.IsValid() && previousScene.isLoaded) SceneManager.SetActiveScene(previousScene);
            if (!string.IsNullOrEmpty(path)) AssetDatabase.DeleteAsset(path);
            if (!string.IsNullOrEmpty(otherPath)) AssetDatabase.DeleteAsset(otherPath);
            if (!string.IsNullOrEmpty(renamedPath)) AssetDatabase.DeleteAsset(renamedPath);
            if (!string.IsNullOrEmpty(harnessPath)) AssetDatabase.DeleteAsset(harnessPath);
            harnessPath = null;
        }

        [Test]
        public void DiscoveryIncludesInactiveTargetsAndExcludesOtherScenes()
        {
            var door = Create<WorldInteraction>(scene, "Door");
            door.Configure(InteractionTarget.Door, "Door", null);
            var poster = Create<ReadableProp>(scene, "Poster");
            poster.gameObject.SetActive(false);
            Create<WorldInteraction>(otherScene, "Unrelated scene target");
            Assert.That(InteractionSettingsWindow.GetTargets(scene), Is.EquivalentTo(new Component[] { door, poster }));
            Assert.That(InteractionSettingsWindow.GetTargets(default), Is.Empty);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void DistanceEditsSupportUndoRedoValidationAndSceneReload(bool readable)
        {
            Component target = readable ? (Component)Create<ReadableProp>(scene, "Target") : Create<WorldInteraction>(scene, "Target");
            float initial = readable ? 2 : 3.2f;
            Assert.That(EditorSceneManager.SaveScene(scene, path), Is.True);
            Assert.That(scene.isDirty, Is.False);
            Undo.IncrementCurrentGroup();
            using (var serialized = new SerializedObject(target))
            {
                serialized.Update();
                serialized.FindProperty("interactionDistance").floatValue = 5.75f;
                Assert.That(InteractionSettingsWindow.ApplyDistance(serialized, scene), Is.True);
            }
            Assert.That(Distance(target), Is.EqualTo(5.75f));
            Assert.That(scene.isDirty, Is.True);
            Undo.FlushUndoRecordObjects();
            Undo.PerformUndo();
            Assert.That(Distance(target), Is.EqualTo(initial));
            Undo.PerformRedo();
            Assert.That(Distance(target), Is.EqualTo(5.75f));
            Assert.That(EditorSceneManager.SaveScene(scene, path), Is.True);
            Assert.That(EditorSceneManager.CloseScene(scene, true), Is.True);
            scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
            target = InteractionSettingsWindow.GetTargets(scene).Single();
            Assert.That(Distance(target), Is.EqualTo(5.75f), "Saved values must come from the component's existing serialized field.");
            foreach (float invalid in new[] { float.NaN, float.PositiveInfinity, -1 })
            {
                using (var serialized = new SerializedObject(target))
                {
                    serialized.Update();
                    serialized.FindProperty("interactionDistance").floatValue = invalid;
                    Assert.That(InteractionSettingsWindow.ApplyDistance(serialized, scene), Is.True);
                    serialized.Update();
                    Assert.That(serialized.FindProperty("interactionDistance").floatValue,
                        Is.EqualTo(invalid < 0 ? .1f : initial), "OnValidate must sanitize the persisted field as well as its getter.");
                }
            }
        }

        [Test]
        public void EditingRejectsOtherScenesAndPreviewScenes()
        {
            var target = Create<WorldInteraction>(otherScene, "Other scene");
            using (var serialized = new SerializedObject(target))
            {
                serialized.FindProperty("interactionDistance").floatValue = 8;
                Assert.That(InteractionSettingsWindow.ApplyDistance(serialized, scene), Is.False);
                Assert.That(target.InteractionDistance, Is.EqualTo(3.2f));
            }
            var preview = EditorSceneManager.NewPreviewScene();
            try
            {
                Create<WorldInteraction>(preview, "Preview target");
                Assert.That(InteractionSettingsWindow.GetTargets(preview), Is.Empty);
                Assert.That(InteractionSettingsWindow.CanEditScene(preview), Is.False);
            }
            finally { EditorSceneManager.ClosePreviewScene(preview); }
        }

        [Test]
        public void SaveAsInvalidatesCachedSceneEligibilityForItsOriginalPath()
        {
            var cached = scene;
            Assert.That(InteractionSettingsWindow.ResolveLoadedScene(path), Is.EqualTo(cached));
            Assert.That(EditorSceneManager.SaveScene(scene, renamedPath), Is.True);
            Assert.That(cached.IsValid() && cached.isLoaded, Is.True, "Save As retains the cached scene handle.");
            Assert.That(cached.path, Is.EqualTo(renamedPath));
            Assert.That(InteractionSettingsWindow.ResolveLoadedScene(path).IsValid(), Is.False,
                "A still-loaded cached scene must not stay eligible after Save As changes its path.");
            Assert.That(InteractionSettingsWindow.ResolveLoadedScene(renamedPath), Is.EqualTo(cached));
            Assert.That(InteractionSettingsWindow.ResolveLoadedScene(string.Empty).IsValid(), Is.False);
        }

        private static float Distance(Component target) => target is ReadableProp readable
            ? readable.InteractionDistance : ((WorldInteraction)target).InteractionDistance;

        private static T Create<T>(Scene destination, string name) where T : Component
        {
            var gameObject = new GameObject(name);
            SceneManager.MoveGameObjectToScene(gameObject, destination);
            return gameObject.AddComponent<T>();
        }
    }
}
