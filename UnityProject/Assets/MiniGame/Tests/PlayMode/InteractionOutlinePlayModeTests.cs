using System;
using System.Collections;
using System.Collections.Generic;
using MiniGame.Core;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace MiniGame.UnityHost.Tests
{
    public sealed class InteractionOutlinePlayModeTests
    {
        private const int TestLayer = 30;
        private readonly List<Object> cleanup = new List<Object>();
        private Camera camera;
        private RenderTexture target;
        private Texture2D pixels;
        private GameObject subject;
        private Renderer surface;
        private Material originalMaterial;
        private WorldInteraction interaction;
        private RenderTexture previousActive;

        [SetUp]
        public void SetUp()
        {
            previousActive = RenderTexture.active;
            var origin = new Vector3(10000, 10000, 10000);
            var cameraObject = Track(new GameObject("Outline test camera", typeof(Camera)));
            camera = cameraObject.GetComponent<Camera>();
            camera.enabled = false;
            camera.transform.position = origin + new Vector3(0, 0, -3);
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.black;
            camera.orthographic = true;
            camera.orthographicSize = 1.5f;
            camera.cullingMask = 1 << TestLayer;
            camera.renderingPath = RenderingPath.Forward;
            camera.allowHDR = false;
            camera.allowMSAA = false;
            target = Track(new RenderTexture(256, 256, 24, RenderTextureFormat.ARGB32));
            target.Create();
            camera.targetTexture = target;
            pixels = Track(new Texture2D(256, 256, TextureFormat.RGBA32, false));

            subject = Track(GameObject.CreatePrimitive(PrimitiveType.Cube));
            subject.name = "Focused interaction mesh";
            subject.layer = TestLayer;
            subject.transform.position = origin;
            surface = subject.GetComponent<Renderer>();
            var shader = Shader.Find("Unlit/Color");
            Assert.That(shader, Is.Not.Null);
            originalMaterial = Track(new Material(shader) { color = new Color(.15f, .2f, .25f) });
            surface.sharedMaterial = originalMaterial;
            var properties = new MaterialPropertyBlock();
            properties.SetColor("_Color", new Color(.3f, .15f, .2f));
            properties.SetFloat("_UnrelatedProperty", 19);
            surface.SetPropertyBlock(properties);
            interaction = subject.AddComponent<WorldInteraction>();
            interaction.Configure(InteractionTarget.Computer, "Computer", surface);
        }

        [TearDown]
        public void TearDown()
        {
            if (interaction != null) interaction.Highlight(false);
            if (camera != null) camera.targetTexture = null;
            RenderTexture.active = previousActive;
            for (int index = cleanup.Count - 1; index >= 0; index--)
                if (cleanup[index] != null) Object.DestroyImmediate(cleanup[index]);
            cleanup.Clear();
        }

        [TestCase(InteractionTarget.Door)]
        [TestCase(InteractionTarget.Bed)]
        public void DoorAndBedNeverChangeAppearance(InteractionTarget targetKind)
        {
            interaction.Configure(targetKind, "No highlight", surface);
            var before = Capture();
            interaction.Highlight(true);
            Assert.That(interaction.IsHighlighted, Is.False);
            Assert.That(Capture(), Is.EqualTo(before), "Door/bed focus must not tint or outline any pixel.");
            AssertSourceUnchanged();
            Assert.That(camera.GetCommandBuffers(CameraEvent.BeforeForwardAlpha), Is.Empty);
        }

        [Test]
        public void DistancesAcceptDesignerValuesAndRejectNonFiniteInput()
        {
            Assert.That(interaction.InteractionDistance, Is.EqualTo(3.2f));
            interaction.InteractionDistance = 8;
            Assert.That(interaction.InteractionDistance, Is.EqualTo(8));
            interaction.InteractionDistance = float.NaN;
            Assert.That(interaction.InteractionDistance, Is.EqualTo(3.2f));
            interaction.InteractionDistance = float.PositiveInfinity;
            Assert.That(interaction.InteractionDistance, Is.EqualTo(3.2f));
            interaction.InteractionDistance = -10;
            Assert.That(interaction.InteractionDistance, Is.EqualTo(.1f));
            var poster = subject.AddComponent<ReadableProp>();
            poster.Configure("poster", "caption", null, float.NaN, float.PositiveInfinity);
            Assert.That(poster.InteractionDistance, Is.EqualTo(2));
            Assert.That(poster.CaptionSeconds, Is.EqualTo(4));
            poster.InteractionDistance = 7;
            Assert.That(poster.InteractionDistance, Is.EqualTo(7));
            poster.InteractionDistance = -1;
            Assert.That(poster.InteractionDistance, Is.EqualTo(.1f));
        }

        [UnityTest]
        public IEnumerator MultiMaterialOutlineIsWhiteKeepsInteriorAndReleasesOnDisable()
        {
            var filter = subject.GetComponent<MeshFilter>();
            var mesh = Track(Object.Instantiate(filter.sharedMesh));
            var triangles = mesh.triangles;
            var first = new int[triangles.Length / 2];
            var second = new int[first.Length];
            Array.Copy(triangles, first, first.Length);
            Array.Copy(triangles, first.Length, second, 0, second.Length);
            mesh.subMeshCount = 2;
            mesh.SetTriangles(first, 0);
            mesh.SetTriangles(second, 1);
            filter.sharedMesh = mesh;
            surface.sharedMaterials = new[] { originalMaterial, originalMaterial };
            subject.transform.rotation = Quaternion.Euler(14, 26, 0);
            var before = Capture();
            interaction.Highlight(true);
            yield return null;
            Assert.That(interaction.IsHighlighted, Is.True);
            var highlighted = Capture();
            int white = 0;
            for (int index = 0; index < before.Length; index++)
            {
                if (before[index].r > 5 || before[index].g > 5 || before[index].b > 5)
                    Assert.That(highlighted[index], Is.EqualTo(before[index]), "Focus must preserve the material inside the silhouette.");
                else if (highlighted[index].r > 245 && highlighted[index].g > 245 && highlighted[index].b > 245) white++;
            }
            Assert.That(white, Is.GreaterThan(80), "The GPU must render a visible white exterior border.");
            AssertSourceUnchanged();
            Assert.That(camera.GetCommandBuffers(CameraEvent.BeforeForwardAlpha).Length, Is.EqualTo(1));
            interaction.enabled = false;
            Assert.That(interaction.IsHighlighted, Is.False);
            Assert.That(camera.GetCommandBuffers(CameraEvent.BeforeForwardAlpha), Is.Empty);
            Assert.That(Capture(), Is.EqualTo(before));
            interaction.enabled = true;
            interaction.Highlight(true);
            Capture();
            Object.Destroy(interaction);
            yield return null;
            Assert.That(camera.GetCommandBuffers(CameraEvent.BeforeForwardAlpha), Is.Empty);
            Assert.That(Capture(), Is.EqualTo(before));
        }

        [UnityTest]
        public IEnumerator OutlineDoesNotRenderThroughOpaqueOccluderOrForDisabledSurface()
        {
            var occluder = Track(GameObject.CreatePrimitive(PrimitiveType.Cube));
            occluder.layer = TestLayer;
            occluder.transform.position = subject.transform.position + new Vector3(0, 0, -1);
            occluder.transform.localScale = new Vector3(2, 2, .1f);
            occluder.GetComponent<Renderer>().sharedMaterial = originalMaterial;
            var before = Capture();
            interaction.Highlight(true);
            yield return null;
            Assert.That(Capture(), Is.EqualTo(before), "Opaque geometry must hide the complete outline behind it.");
            occluder.SetActive(false);
            surface.enabled = false;
            var hidden = Capture();
            foreach (var color in hidden)
                Assert.That(Mathf.Max(color.r, color.g, color.b), Is.LessThan(2), "A disabled renderer must not leave a floating outline.");
        }

        [UnityTest]
        public IEnumerator CoarseRetroKeepsWhiteOutlineContinuousAcrossPixelGridOffsetsAndBypasses()
        {
            interaction.Highlight(true);
            var nativeOutline = Capture();
            var effect = AddCoarseRetro();
            string settings = JsonUtility.ToJson(effect.Profile);
            yield return null;
            var origin = subject.transform.position;
            float worldPerPixel = 2 * camera.orthographicSize / camera.pixelHeight;
            foreach (int offset in new[] { 0, 3, 6, 7 })
            {
                subject.transform.position = origin + new Vector3(offset * worldPerPixel, offset * worldPerPixel, 0);
                interaction.Highlight(false);
                var baseline = Capture();
                interaction.Highlight(true);
                AssertWhitePerimeter(baseline, Capture(), "Computer offset " + offset);
            }
            subject.transform.position = origin;
            Assert.That(JsonUtility.ToJson(effect.Profile), Is.EqualTo(settings), "Outline adaptation must not modify the designer's profile.");
            effect.PreviewOriginal = true;
            Assert.That(Capture(), Is.EqualTo(nativeOutline), "Original preview must restore the native 3-pixel outline.");
            effect.PreviewOriginal = false;
            effect.enabled = false;
            Assert.That(Capture(), Is.EqualTo(nativeOutline));
            effect.enabled = true;
            effect.Profile.Strength = 0;
            Assert.That(Capture(), Is.EqualTo(nativeOutline));
            effect.Profile.Strength = 1;
            effect.Profile.EffectEnabled = false;
            Assert.That(Capture(), Is.EqualTo(nativeOutline));
        }

        [UnityTest]
        public IEnumerator CoarseRetroKeepsPosterBorderContinuousAndRestoresNativeWidth()
        {
            var quad = Track(GameObject.CreatePrimitive(PrimitiveType.Quad));
            quad.SetActive(false);
            subject.GetComponent<MeshFilter>().sharedMesh = quad.GetComponent<MeshFilter>().sharedMesh;
            var poster = subject.AddComponent<ReadableProp>();
            poster.Configure("poster", "caption", surface);
            poster.Highlight(true);
            var nativeBorder = Capture();
            var effect = AddCoarseRetro();
            yield return null;
            var origin = subject.transform.position;
            float worldPerPixel = 2 * camera.orthographicSize / camera.pixelHeight;
            foreach (int offset in new[] { 0, 3, 6, 7 })
            {
                subject.transform.position = origin + new Vector3(offset * worldPerPixel, offset * worldPerPixel, 0);
                poster.Highlight(false);
                var baseline = Capture();
                poster.Highlight(true);
                AssertWhitePerimeter(baseline, Capture(), "Poster offset " + offset);
            }
            subject.transform.position = origin;
            effect.PreviewOriginal = true;
            Assert.That(Capture(), Is.EqualTo(nativeBorder), "Original preview must restore the 0.006-metre border.");
            AssertSourceUnchanged();
        }

        [UnityTest]
        public IEnumerator PosterBorderIsWhiteAndDisablingOrRemovingComponentClearsIt()
        {
            var poster = subject.AddComponent<ReadableProp>();
            poster.Configure("poster", "caption", surface);
            poster.Highlight(true);
            var line = subject.GetComponentInChildren<LineRenderer>();
            Assert.That(line, Is.Not.Null);
            Assert.That(line.sharedMaterial.GetColor("_Color"), Is.EqualTo(Color.white));
            Assert.That(poster.IsHighlighted, Is.True);
            AssertSourceUnchanged();
            poster.enabled = false;
            Assert.That(poster.IsHighlighted, Is.False);
            poster.enabled = true;
            poster.Highlight(true);
            Object.Destroy(poster);
            yield return null;
            yield return null;
            Assert.That(subject.GetComponentsInChildren<LineRenderer>(), Is.Empty);
        }

        private void AssertSourceUnchanged()
        {
            Assert.That(surface.sharedMaterial, Is.SameAs(originalMaterial));
            Assert.That(originalMaterial.color, Is.EqualTo(new Color(.15f, .2f, .25f)));
            var properties = new MaterialPropertyBlock();
            surface.GetPropertyBlock(properties);
            Assert.That(properties.GetColor("_Color"), Is.EqualTo(new Color(.3f, .15f, .2f)));
            Assert.That(properties.GetFloat("_UnrelatedProperty"), Is.EqualTo(19));
        }

        private Color32[] Capture()
        {
            camera.Render();
            RenderTexture.active = target;
            pixels.ReadPixels(new Rect(0, 0, target.width, target.height), 0, 0);
            pixels.Apply();
            return pixels.GetPixels32();
        }

        private RetroScreenEffect AddCoarseRetro()
        {
            var profile = Track(ScriptableObject.CreateInstance<RetroScreenProfile>());
            profile.PixelSize = 8;
            profile.Strength = 1;
            profile.Levels = 23;
            var shader = Resources.Load<Shader>("RetroScreen");
            Assert.That(shader, Is.Not.Null);
            Assert.That(shader.isSupported, Is.True);
            var effect = camera.gameObject.AddComponent<RetroScreenEffect>();
            effect.Configure(profile, shader);
            return effect;
        }

        private void AssertWhitePerimeter(Color32[] baseline, Color32[] highlighted, string label)
        {
            int width = target.width, height = target.height;
            int minX = width, maxX = -1, minY = height, maxY = -1;
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                {
                    var color = baseline[y * width + x];
                    if (color.r <= 5 && color.g <= 5 && color.b <= 5) continue;
                    minX = Mathf.Min(minX, x); maxX = Mathf.Max(maxX, x);
                    minY = Mathf.Min(minY, y); maxY = Mathf.Max(maxY, y);
                }
            Assert.That(maxX - minX, Is.GreaterThan(32), label + " must have a visible test subject.");
            // Check every row/column along all four edges after the real post-process,
            // excluding only corner blocks where the mesh silhouette changes direction.
            for (int y = minY + 8; y <= maxY - 8; y++)
            {
                bool left = false, right = false;
                for (int x = 0; x < minX; x++) left |= IsWhite(highlighted[y * width + x]);
                for (int x = maxX + 1; x < width; x++) right |= IsWhite(highlighted[y * width + x]);
                Assert.That(left && right, Is.True, label + " has a broken side border at row " + y);
            }
            for (int x = minX + 8; x <= maxX - 8; x++)
            {
                bool bottom = false, top = false;
                for (int y = 0; y < minY; y++) bottom |= IsWhite(highlighted[y * width + x]);
                for (int y = maxY + 1; y < height; y++) top |= IsWhite(highlighted[y * width + x]);
                Assert.That(bottom && top, Is.True, label + " has a broken top/bottom border at column " + x);
            }
        }

        private static bool IsWhite(Color32 color) => color.r > 245 && color.g > 245 && color.b > 245;

        private T Track<T>(T value) where T : Object
        {
            cleanup.Add(value);
            return value;
        }
    }
}
