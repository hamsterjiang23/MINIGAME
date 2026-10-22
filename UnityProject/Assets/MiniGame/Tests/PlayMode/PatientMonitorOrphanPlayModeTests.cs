using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif
using Object = UnityEngine.Object;

namespace MiniGame.UnityHost.Tests
{
    public sealed class PatientMonitorOrphanPlayModeTests
    {
        [UnityTest, Timeout(30000)]
        public IEnumerator OwnerlessRulerStageCannotAppearInANewTutorialMonitor()
        {
            var catalog = Resources.Load<PortraitCatalog>("MiniGame/PortraitCatalog");
            var profile = FrozenProfile();
            Assert.That(catalog, Is.Not.Null);
            var tutorial = catalog.Find("D0_01");
            var ruler = catalog.Find("VV04");
            Assert.That(tutorial, Is.Not.Null);
            Assert.That(ruler, Is.Not.Null);
            GameObject baselineOwner = null, orphanOwner = null, newOwner = null, orphanStage = null;
            Material orphanMaterial = null;
            Sprite orphanBackdrop = null;
            try
            {
                var baselineFeed = CreateFeed("Tutorial without an orphan", profile, out baselineOwner);
                baselineFeed.Show("tutorial", tutorial, true);
                var baseline = ReadPixels(baselineFeed.Output);
                Assert.That(baseline.Count(pixel => pixel.r > 20 || pixel.g > 20 || pixel.b > 20),
                    Is.GreaterThan(1000), "The baseline must be a real rendered frame.");
                Capture(baselineFeed.Output, "orphan-tutorial-baseline");
                Object.DestroyImmediate(baselineOwner);
                baselineOwner = null;
                yield return null; // Finish normal game-scene cleanup before reusing its slot.

                var abandoned = CreateFeed("Owner that loses its preview stage", profile, out orphanOwner);
                abandoned.Show("former-ruler", ruler, true);
                orphanStage = abandoned.ViewCamera.transform.parent.gameObject;
                var orphanActor = orphanStage.GetComponentsInChildren<SpriteRenderer>(true)
                    .Single(renderer => renderer.sprite == ruler);
                orphanActor.sortingOrder = 10;
                orphanMaterial = orphanActor.sharedMaterial;
                orphanBackdrop = GetField<Sprite>(abandoned, "backdropSprite");

                // Reproduce a script-reload orphan without depending on a real domain reload:
                // its rendered objects survive, but no feed retains ownership of the root.
                SetField(abandoned, "stage", null);
                SetField(abandoned, "actorMaterial", null);
                SetField(abandoned, "backdropSprite", null);
                Object.DestroyImmediate(orphanOwner);
                orphanOwner = null;
                Assert.That(orphanStage.activeInHierarchy, Is.True);
                Assert.That(orphanActor.sprite, Is.SameAs(ruler));
                Assert.That(Resources.FindObjectsOfTypeAll<PatientSurveillanceFeed>()
                    .Any(feed => GetField<GameObject>(feed, "stage") == orphanStage), Is.False,
                    "The stale stage must have no surviving feed owner.");

                var current = CreateFeed("Tutorial beside an ownerless ruler", profile, out newOwner);
                current.Show("tutorial", tutorial, true);
                Capture(current.Output, "orphan-tutorial-isolated");
                Assert.That(ReadPixels(current.Output), Is.EqualTo(baseline),
                    "An ownerless VV04 stage must not add any pixels to the new D0_01 monitor.");
                Assert.That(GeometryUtility.TestPlanesAABB(
                    GeometryUtility.CalculateFrustumPlanes(current.ViewCamera), orphanActor.bounds), Is.False,
                    "The new camera's frustum must exclude the orphan, including its transparent portrait.");

                // Prove this is not a vacuous camera/scene test: deliberately put the stale
                // root into the new camera's stage and require visible contamination.
                var isolatedSource = ReadPixels(current.ViewCamera.targetTexture);
                var orphanPosition = orphanStage.transform.position;
                try
                {
                    orphanStage.transform.position = current.ViewCamera.transform.parent.position;
                    current.ViewCamera.Render();
                    var contaminated = ReadPixels(current.ViewCamera.targetTexture);
                    Capture(current.ViewCamera.targetTexture, "orphan-positive-contamination-control");
                    Assert.That(isolatedSource.Zip(contaminated, (a, b) => a.Equals(b) ? 0 : 1).Sum(),
                        Is.GreaterThan(100), "The same orphan must visibly overlap when deliberately co-located.");
                }
                finally { orphanStage.transform.position = orphanPosition; }
            }
            finally
            {
                Object.DestroyImmediate(baselineOwner);
                Object.DestroyImmediate(orphanOwner);
                Object.DestroyImmediate(newOwner);
                Object.DestroyImmediate(orphanStage);
                Object.DestroyImmediate(orphanMaterial);
                Object.DestroyImmediate(orphanBackdrop);
                Object.DestroyImmediate(profile);
            }
            yield return null;
        }

#if UNITY_EDITOR
        [UnityTest, Timeout(30000)]
        public IEnumerator DestroyingAPreviewOwnerImmediatelyReleasesItsStageAndPortraits()
        {
            var catalog = Resources.Load<PortraitCatalog>("MiniGame/PortraitCatalog");
            var profile = FrozenProfile();
            Assert.That(catalog, Is.Not.Null);
            var previewScene = EditorSceneManager.NewPreviewScene();
            GameObject owner = null, stage = null;
            try
            {
                var feed = CreateFeed("Preview owner cleanup regression", profile, out owner, previewScene);
                var portrait = catalog.Find("VV04");
                var backdrop = profile.Backdrop;
                feed.Show("former-ruler", portrait, true);
                Assert.That(feed.Output, Is.Not.Null);
                stage = feed.ViewCamera.transform.parent.gameObject;
                Assert.That(EditorSceneManager.IsPreviewScene(stage.scene), Is.True);
                var rootIds = stage.GetComponentsInChildren<Transform>(true)
                    .Select(value => value.gameObject.GetInstanceID()).ToArray();
                var rendererIds = stage.GetComponentsInChildren<SpriteRenderer>(true)
                    .Select(value => value.GetInstanceID()).ToArray();
                Assert.That(rendererIds.Length, Is.GreaterThanOrEqualTo(2));
                var ownedMeshes = stage.GetComponentsInChildren<MeshFilter>(true)
                    .Select(value => value.sharedMesh)
                    .Where(value => value != null && !UnityEditor.EditorUtility.IsPersistent(value))
                    .Distinct().ToArray();
                var ownedMaterials = stage.GetComponentsInChildren<Renderer>(true)
                    .SelectMany(value => value.sharedMaterials)
                    .Where(value => value != null && !UnityEditor.EditorUtility.IsPersistent(value))
                    .Distinct().ToArray();
                Assert.That(ownedMeshes.Length, Is.GreaterThan(0), "The preview must own generated room meshes.");
                Assert.That(ownedMaterials.Length, Is.GreaterThan(0), "The preview must own generated materials.");

                Object.DestroyImmediate(owner);
                owner = null;
                Assert.That(stage == null, Is.True,
                    "Preview objects have no normal scene frame lifecycle; cleanup must not defer Destroy.");
                Assert.That(Resources.FindObjectsOfTypeAll<GameObject>()
                    .Any(value => rootIds.Contains(value.GetInstanceID())), Is.False,
                    "No generated preview hierarchy may remain immediately after its owner is destroyed.");
                Assert.That(Resources.FindObjectsOfTypeAll<SpriteRenderer>()
                    .Any(value => rendererIds.Contains(value.GetInstanceID())), Is.False);
                Assert.That(ownedMeshes.All(value => value == null), Is.True,
                    "Preview disposal must immediately release every generated room mesh.");
                Assert.That(ownedMaterials.All(value => value == null), Is.True,
                    "Preview disposal must immediately release room and portrait materials.");
                Assert.That(portrait != null && portrait.texture != null, Is.True,
                    "Disposing a preview must preserve its original catalog portrait and texture.");
                Assert.That(backdrop != null, Is.True,
                    "Disposing a preview must preserve its original ward backdrop texture.");
                EditorSceneManager.ClosePreviewScene(previewScene);
                Assert.That(Resources.FindObjectsOfTypeAll<SpriteRenderer>()
                    .Any(value => rendererIds.Contains(value.GetInstanceID())), Is.False,
                    "Closing the preview must not leave scene-handle-zero portrait renderers behind.");
            }
            finally
            {
                Object.DestroyImmediate(owner);
                Object.DestroyImmediate(stage);
                if (previewScene.IsValid()) EditorSceneManager.ClosePreviewScene(previewScene);
                Object.DestroyImmediate(profile);
            }
            yield return null;
        }
#endif

        private static PatientMonitorProfile FrozenProfile()
        {
            var template = Resources.Load<PatientMonitorProfile>("MiniGame/PatientMonitorDefault");
            Assert.That(template, Is.Not.Null);
            var result = Object.Instantiate(template);
            result.Enabled = true;
            result.Noise = 0;
            result.IdleMotion = 0;
            result.Width = 640;
            result.Height = 360;
            Assert.That(result.Backdrop, Is.Not.Null);
            return result;
        }

        private static PatientSurveillanceFeed CreateFeed(string name, PatientMonitorProfile profile,
            out GameObject owner, Scene scene = default)
        {
            owner = new GameObject(name, typeof(RectTransform), typeof(RawImage));
            if (scene.IsValid()) SceneManager.MoveGameObjectToScene(owner, scene);
            ((RectTransform)owner.transform).sizeDelta = new Vector2(464, 261);
            var feed = owner.AddComponent<PatientSurveillanceFeed>();
            feed.UseExternalClock = true;
            feed.Initialize(owner.GetComponent<RawImage>(), profile);
            return feed;
        }

        private static T GetField<T>(PatientSurveillanceFeed owner, string name) where T : Object =>
            (T)Field(name).GetValue(owner);

        private static void SetField(PatientSurveillanceFeed owner, string name, Object value) =>
            Field(name).SetValue(owner, value);

        private static FieldInfo Field(string name)
        {
            var result = typeof(PatientSurveillanceFeed).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(result, Is.Not.Null, "The orphan fixture requires the actual stage ownership field.");
            return result;
        }

        private static Color32[] ReadPixels(RenderTexture source)
        {
            var image = Read(source);
            try { return image.GetPixels32(); }
            finally { Object.DestroyImmediate(image); }
        }

        private static Texture2D Read(RenderTexture source)
        {
            Assert.That(source, Is.Not.Null);
            Assert.That(source.IsCreated(), Is.True);
            var previous = RenderTexture.active;
            var image = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
            try
            {
                RenderTexture.active = source;
                image.ReadPixels(new Rect(0, 0, source.width, source.height), 0, 0);
                image.Apply();
                return image;
            }
            finally { RenderTexture.active = previous; }
        }

        private static void Capture(RenderTexture source, string name)
        {
            string folder = Environment.GetEnvironmentVariable("MINIGAME_PORTRAIT_CAPTURE_DIR");
            if (string.IsNullOrEmpty(folder)) return;
            Directory.CreateDirectory(folder);
            var image = Read(source);
            try { File.WriteAllBytes(Path.Combine(folder, name + ".png"), image.EncodeToPNG()); }
            finally { Object.DestroyImmediate(image); }
        }
    }
}
