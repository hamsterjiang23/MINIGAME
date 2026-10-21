using System;
using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace MiniGame.UnityHost.Tests
{
    public sealed class BedtimeAtmospherePlayModeTests
    {
        [UnityTest, Timeout(15000)]
        public IEnumerator GlowCloudDriftsInsideTheConeFreezesAndReleasesReboundResources()
        {
            var fixture = new ParticleFixture(512);
            try
            {
                var atmosphere = fixture.Atmosphere;
                atmosphere.SetVisualState(1, 0, true);
                Assert.That(atmosphere.VisualTime, Is.Zero);
                Assert.That(atmosphere.ParticleCount, Is.GreaterThan(1));
                Assert.That(atmosphere.BeamRenderer.enabled, Is.True, "The particle change must keep the beam available.");
                Assert.That(atmosphere.ParticleRenderer.sharedMaterial.shader.name, Is.EqualTo("MiniGame/Bedtime/GlowParticles"));
                var mesh = atmosphere.ParticleRenderer.GetComponent<MeshFilter>().sharedMesh;
                Assert.That(mesh.vertexCount, Is.EqualTo(atmosphere.ParticleCount * 4), "Each point uses one quad, without body, wing or leg geometry.");
                Assert.That(mesh.triangles.Length, Is.EqualTo(atmosphere.ParticleCount * 6));
                for (int index = 0; index < atmosphere.ParticleCount; index++)
                    Assert.That(mesh.triangles.Skip(index * 6).Take(6).All(vertex => vertex >= index * 4 && vertex < (index + 1) * 4), Is.True);
                AssertInsideCone(fixture);
                var positions = Positions(atmosphere);
                var first = fixture.Capture("bedtime-particles-time-0");
                Assert.That(first.Count(pixel => Brightness(pixel) > 2), Is.GreaterThan(atmosphere.ParticleCount),
                    "The actual particle layer must produce visible RGB, independently of its additive alpha.");

                atmosphere.SetVisualState(1, .75f, false);
                Assert.That(atmosphere.VisualTime, Is.EqualTo(.75f).Within(.00001f));
                var distances = Positions(atmosphere).Select((position, index) => Vector3.Distance(position, positions[index])).ToArray();
                Assert.That(distances.Count(distance => distance > .0005f), Is.GreaterThan(atmosphere.ParticleCount / 2));
                Assert.That(distances.Max(), Is.LessThan(ParticleFixture.Radius * .2f), "Less than a second should produce slow drifting, not a flight across the cone.");
                AssertInsideCone(fixture);
                var moved = fixture.Capture("bedtime-particles-time-075");
                Assert.That(ChangedPixels(first, moved), Is.GreaterThan(10), "Manual time must change the actual rendered point positions.");
                var pausedPositions = Positions(atmosphere);
                float pausedTime = atmosphere.VisualTime;
                atmosphere.SetVisualState(1, 10, true);
                yield return new WaitForSecondsRealtime(.06f);
                Assert.That(atmosphere.VisualTime, Is.EqualTo(pausedTime));
                AssertPositionsEqual(atmosphere, pausedPositions);
                Assert.That(MaxRgbDifference(moved, fixture.Capture()), Is.LessThanOrEqualTo(1),
                    "Pause must freeze the visible particles across real Unity frames.");

                foreach (float delta in new[] { 2f, 20f, 120f })
                {
                    atmosphere.SetVisualState(1, delta, false);
                    AssertInsideCone(fixture);
                }
                float hiddenTime = atmosphere.VisualTime;
                atmosphere.SetVisualState(0, 30, false);
                Assert.That(atmosphere.VisualTime, Is.EqualTo(hiddenTime), "Hidden atmosphere cannot consume the owner's visual clock.");
                Assert.That(atmosphere.BeamRenderer.enabled || atmosphere.ParticleRenderer.enabled, Is.False);
                Assert.That(fixture.Capture().All(pixel => Brightness(pixel) <= 1), Is.True);

                var oldParticle = atmosphere.ParticleRenderer;
                var oldBeam = atmosphere.BeamRenderer;
                var oldParticleMesh = oldParticle.GetComponent<MeshFilter>().sharedMesh;
                var oldBeamMesh = oldBeam.GetComponent<MeshFilter>().sharedMesh;
                var oldParticleMaterial = oldParticle.sharedMaterial;
                var oldBeamMaterial = oldBeam.sharedMaterial;
                fixture.Emitter.position += new Vector3(.25f, .15f, 0);
                atmosphere.Initialize(fixture.Emitter, fixture.BedCenter, ParticleFixture.Radius);
                atmosphere.BeamRenderer.gameObject.layer = 29;
                Assert.That(atmosphere.VisualTime, Is.Zero);
                Assert.That(atmosphere.ParticleRenderer, Is.Not.SameAs(oldParticle));
                atmosphere.SetVisualState(1, 0, true);
                AssertInsideCone(fixture);
                yield return null;
                yield return null;
                Assert.That(oldParticle == null && oldBeam == null && oldParticleMesh == null && oldBeamMesh == null
                    && oldParticleMaterial == null && oldBeamMaterial == null, Is.True, "Rebinding must release all previous runtime render resources.");
                Assert.That(atmosphere.GetComponentsInChildren<MeshRenderer>().Length, Is.EqualTo(2));

                var finalMesh = atmosphere.ParticleRenderer.GetComponent<MeshFilter>().sharedMesh;
                var finalMaterial = atmosphere.ParticleRenderer.sharedMaterial;
                fixture.Dispose();
                yield return null;
                yield return null;
                Assert.That(finalMesh == null && finalMaterial == null, Is.True, "Destroying the owner must release its new runtime mesh and material.");
            }
            finally { fixture.Dispose(); }
        }

        [Test]
        public void SingleGlowPointRendersABrightCentreSoftHaloAndNoRectangularEdge()
        {
            using (var fixture = new ParticleFixture(128))
            {
                var material = fixture.ShowSinglePoint();
                var pixels = fixture.Capture("bedtime-particle-soft-edge");
                int size = fixture.Size;
                float centre = Brightness(pixels[(size / 2) * size + size / 2]);
                var halo = pixels.Where((pixel, index) =>
                {
                    var point = new Vector2(index % size - size * .5f, index / size - size * .5f);
                    return point.magnitude >= size * .18f && point.magnitude <= size * .25f;
                }).Select(pixel => (float)Brightness(pixel)).ToArray();
                Assert.That(centre, Is.GreaterThan(20), "An isolated point must have an actual luminous centre.");
                Assert.That(halo.Average(), Is.GreaterThan(1), "The point needs a faint visible halo rather than a hard dot.");
                Assert.That(halo.Average(), Is.LessThan(centre * .4f));
                Assert.That(pixels.Count(pixel => Brightness(pixel) > 1 && Brightness(pixel) < centre * .5f), Is.GreaterThan(100),
                    "Soft falloff must occupy intermediate RGB values, not a binary silhouette.");
                for (int y = 0; y < 10; y++)
                    for (int x = 0; x < 10; x++)
                        Assert.That(Brightness(pixels[y * size + x]), Is.LessThanOrEqualTo(1), "Quad corners outside the point must remain black.");
                material.SetFloat("_Strength", 0);
                Assert.That(fixture.Capture().All(pixel => Brightness(pixel) <= 1), Is.True);
            }
        }

        private static Vector3[] Positions(BedtimeAtmosphere atmosphere) => Enumerable.Range(0, atmosphere.ParticleCount)
            .Select(atmosphere.GetParticlePosition).ToArray();

        private static void AssertPositionsEqual(BedtimeAtmosphere atmosphere, Vector3[] expected)
        {
            for (int index = 0; index < expected.Length; index++)
                Assert.That(Vector3.Distance(atmosphere.GetParticlePosition(index), expected[index]), Is.LessThan(.00001f));
        }

        private static void AssertInsideCone(ParticleFixture fixture)
        {
            var offset = fixture.BedCenter - fixture.Emitter.position;
            float height = offset.magnitude;
            var axis = offset.normalized;
            foreach (var position in Positions(fixture.Atmosphere))
            {
                var relative = position - fixture.Emitter.position;
                float along = Vector3.Dot(relative, axis);
                Assert.That(along, Is.InRange(0, height));
                float radial = (relative - axis * along).magnitude;
                Assert.That(radial, Is.LessThanOrEqualTo(ParticleFixture.Radius * along / height + .0002f));
            }
        }

        private static int Brightness(Color32 color) => Mathf.Max(color.r, Mathf.Max(color.g, color.b));
        private static int ChangedPixels(Color32[] first, Color32[] second) => first.Where((pixel, index) =>
            Mathf.Abs(pixel.r - second[index].r) + Mathf.Abs(pixel.g - second[index].g) + Mathf.Abs(pixel.b - second[index].b) > 3).Count();

        private static int MaxRgbDifference(Color32[] first, Color32[] second)
        {
            int maximum = 0;
            for (int index = 0; index < first.Length; index++)
            {
                maximum = Mathf.Max(maximum, Mathf.Abs(first[index].r - second[index].r));
                maximum = Mathf.Max(maximum, Mathf.Abs(first[index].g - second[index].g));
                maximum = Mathf.Max(maximum, Mathf.Abs(first[index].b - second[index].b));
            }
            return maximum;
        }

        private sealed class ParticleFixture : IDisposable
        {
            public const float Radius = 1.15f;
            public readonly int Size;
            public readonly BedtimeAtmosphere Atmosphere;
            public readonly Transform Emitter;
            public Vector3 BedCenter => root.transform.position + Vector3.up * .4f;
            private readonly GameObject root;
            private readonly Camera camera;
            private readonly RenderTexture target;
            private readonly Texture2D readback;
            private Mesh pointMesh;
            private Material pointMaterial;
            private bool disposed;

            public ParticleFixture(int size)
            {
                Size = size;
                var shader = Resources.Load<Shader>("BedtimeInsect");
                Assert.That(shader != null && shader.isSupported, Is.True, "The compatible resource path must resolve a supported glow-particle shader.");
                root = new GameObject("Bedtime particle GPU fixture"); root.layer = 30;
                root.transform.position = new Vector3(100, 100, 100);
                Emitter = new GameObject("Ceiling emitter").transform; Emitter.SetParent(root.transform, false);
                Emitter.localPosition = new Vector3(0, 2.5f, 0);
                Atmosphere = root.AddComponent<BedtimeAtmosphere>();
                Atmosphere.Initialize(Emitter, BedCenter, Radius);
                Atmosphere.BeamRenderer.gameObject.layer = 29;
                var cameraObject = new GameObject("Particle-only camera", typeof(Camera)); cameraObject.transform.SetParent(root.transform, false);
                cameraObject.transform.localPosition = new Vector3(0, 1.3f, -3);
                camera = cameraObject.GetComponent<Camera>(); camera.enabled = false;
                camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = Color.black;
                camera.orthographic = true; camera.orthographicSize = 1.2f; camera.aspect = 1;
                camera.nearClipPlane = .1f; camera.farClipPlane = 10; camera.cullingMask = 1 << 30;
                camera.allowHDR = false; camera.allowMSAA = false;
                target = new RenderTexture(size, size, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear) { antiAliasing = 1 };
                target.Create(); camera.targetTexture = target;
                readback = new Texture2D(size, size, TextureFormat.RGBA32, false, true);
            }

            public Material ShowSinglePoint()
            {
                Atmosphere.BeamRenderer.enabled = false; Atmosphere.ParticleRenderer.enabled = false;
                camera.orthographicSize = 1;
                var point = new GameObject("One glow quad", typeof(MeshFilter), typeof(MeshRenderer));
                point.layer = 30; point.transform.SetParent(root.transform, false); point.transform.localPosition = Vector3.up * 1.3f;
                pointMesh = new Mesh { vertices = new[] { new Vector3(-.9f, -.9f, 0), new Vector3(.9f, -.9f, 0), new Vector3(.9f, .9f, 0), new Vector3(-.9f, .9f, 0) },
                    uv = new[] { new Vector2(-1, -1), new Vector2(1, -1), new Vector2(1, 1), new Vector2(-1, 1) },
                    colors = Enumerable.Repeat(new Color(1, .95f, .84f, .4f), 4).ToArray(), triangles = new[] { 0, 1, 2, 0, 2, 3 } };
                pointMesh.RecalculateBounds(); point.GetComponent<MeshFilter>().sharedMesh = pointMesh;
                pointMaterial = new Material(Resources.Load<Shader>("BedtimeInsect")); pointMaterial.SetFloat("_Strength", 1);
                point.GetComponent<MeshRenderer>().sharedMaterial = pointMaterial;
                return pointMaterial;
            }

            public Color32[] Capture(string name = null)
            {
                var previous = RenderTexture.active;
                try
                {
                    camera.Render(); RenderTexture.active = target;
                    readback.ReadPixels(new Rect(0, 0, Size, Size), 0, 0); readback.Apply();
                    if (name != null)
                    {
                        string directory = CaptureDirectory(); Directory.CreateDirectory(directory);
                        string path = Path.Combine(directory, name + ".png");
                        File.WriteAllBytes(path, readback.EncodeToPNG()); Debug.Log("MINIGAME_PARTICLE_CAPTURE " + path);
                    }
                    return readback.GetPixels32();
                }
                finally { RenderTexture.active = previous; }
            }

            private static string CaptureDirectory()
            {
                string capture = Environment.GetEnvironmentVariable("MINIGAME_CAPTURE_DIR");
                if (!string.IsNullOrEmpty(capture)) return Path.GetFullPath(capture);
                string repository = Environment.GetEnvironmentVariable("MINIGAME_REPO_ROOT");
                if (!string.IsNullOrEmpty(repository)) return Path.Combine(Path.GetFullPath(repository), "Artifacts", "BedtimeParticles", "screenshots");
                var directory = new DirectoryInfo(Application.dataPath);
                while (directory != null && !File.Exists(Path.Combine(directory.FullName, "src", "MiniGame.sln"))) directory = directory.Parent;
                if (directory == null) throw new InvalidOperationException("Set MINIGAME_CAPTURE_DIR or MINIGAME_REPO_ROOT for particle captures.");
                return Path.Combine(directory.FullName, "Artifacts", "BedtimeParticles", "screenshots");
            }

            public void Dispose()
            {
                if (disposed) return; disposed = true;
                camera.targetTexture = null; target.Release();
                Object.Destroy(root); Object.Destroy(readback); Object.Destroy(target);
                if (pointMesh != null) Object.Destroy(pointMesh);
                if (pointMaterial != null) Object.Destroy(pointMaterial);
            }
        }
    }
}
