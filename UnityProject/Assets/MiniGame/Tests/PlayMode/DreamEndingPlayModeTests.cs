using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace MiniGame.UnityHost.Tests
{
    public sealed class DreamEndingPlayModeTests
    {
        private GameObject root;
        private DreamSequenceProfile profile;
        private MiniGameAudioProfile audio;
        private DreamEndingController ending;
        private AudioClip voice;
        private Renderer seaSurface, sandSurface;
        private Material originalSea, originalSand;
        private BeachEndingCheckpoint previousCheckpoint;
        private bool previousPaused;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            var host = MiniGameSessionHost.Current;
            previousCheckpoint = host?.EndingCheckpoint;
            previousPaused = host?.Runtime?.GetView().Paused ?? false;
            host?.Runtime?.SetPaused(false);
            profile = ScriptableObject.CreateInstance<DreamSequenceProfile>();
            audio = ScriptableObject.CreateInstance<MiniGameAudioProfile>();
            profile.Audio = audio;
            root = new GameObject("Isolated ending regression");
            var camera = new GameObject("Camera", typeof(Camera)).GetComponent<Camera>();
            camera.transform.SetParent(root.transform, false);
            camera.enabled = false;
            var bed = new GameObject("Bed").transform;
            bed.SetParent(root.transform, false); bed.position = new Vector3(0, 0, 5);
            var pillow = new GameObject("Pillow").transform;
            pillow.SetParent(bed, false); pillow.localPosition = new Vector3(0, 1, .7f);
            var start = new GameObject("Start").transform;
            start.SetParent(root.transform, false); start.position = new Vector3(0, 1.7f, 0);
            var seaShader = Resources.Load<Shader>("DreamSea");
            var sandShader = Resources.Load<Shader>("DreamSeaSand");
            Assert.That(seaShader, Is.Not.Null); Assert.That(sandShader, Is.Not.Null);
            originalSea = new Material(seaShader);
            originalSand = new Material(sandShader);
            originalSea.SetFloat("_DreamTime", -17);
            originalSand.SetFloat("_DreamTime", -23);
            seaSurface = new GameObject("Sea", typeof(MeshFilter), typeof(MeshRenderer)).GetComponent<Renderer>();
            sandSurface = new GameObject("Sand", typeof(MeshFilter), typeof(MeshRenderer)).GetComponent<Renderer>();
            seaSurface.transform.SetParent(root.transform, false); seaSurface.sharedMaterial = originalSea;
            sandSurface.transform.SetParent(root.transform, false); sandSurface.sharedMaterial = originalSand;
            ending = root.AddComponent<DreamEndingController>();
            ending.Configure(profile, camera, bed, pillow, start, seaSurface);
            ending.enabled = false; // Exercise the production tick without physical keyboard input.
            ending.RestoreCheckpoint(new BeachEndingCheckpoint());
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (MiniGameSessionHost.Current != null)
            {
                MiniGameSessionHost.Current.EndingCheckpoint = previousCheckpoint;
                MiniGameSessionHost.Current.Runtime?.SetPaused(previousPaused);
            }
            Object.Destroy(root); Object.Destroy(profile); Object.Destroy(audio);
            Object.Destroy(originalSea); Object.Destroy(originalSand);
            if (voice != null) Object.Destroy(voice);
            Cursor.lockState = CursorLockMode.None; Cursor.visible = true;
            yield return null;
        }

        [Test]
        public void WOnlyMovesForwardAndCannotOvershootBed()
        {
            var start = ending.ViewCamera.transform.position;
            var rotation = ending.ViewCamera.transform.rotation;
            Assert.That(ending.TryLieDown(), Is.False);
            ending.AdvanceForTests(-1, 1);
            Assert.That(ending.ViewCamera.transform.position, Is.EqualTo(start));
            ending.AdvanceForTests(1, 1);
            Assert.That(ending.ViewCamera.transform.position.z, Is.EqualTo(start.z + 1.2f).Within(.001));
            Assert.That(ending.ViewCamera.transform.position.x, Is.EqualTo(start.x));
            Assert.That(ending.ViewCamera.transform.rotation, Is.EqualTo(rotation));
            ending.AdvanceForTests(1, 500);
            Assert.That(ending.ViewCamera.transform.position.z, Is.LessThan(ending.Bed.position.z));
            Assert.That(ending.CanLieDown, Is.True);
        }

        [Test]
        public void LyingLocksUpwardAndCameraFollowsFloatingBed()
        {
            ending.AdvanceForTests(1, 10);
            Assert.That(ending.TryLieDown(), Is.True);
            Assert.That(ending.TryLieDown(), Is.False, "The bed command is accepted only once.");
            ending.AdvanceForTests(0, profile.LieDownSeconds);
            Assert.That(ending.Phase, Is.EqualTo(BeachEndingPhase.Drifting));
            Assert.That(Vector3.Dot(ending.ViewCamera.transform.forward, Vector3.up), Is.GreaterThan(.999));
            var local = ending.Bed.InverseTransformPoint(ending.ViewCamera.transform.position);
            var before = ending.Bed.position;
            ending.AdvanceForTests(1, profile.FloatPeriod / 4);
            Assert.That(ending.Bed.position.y, Is.EqualTo(before.y + profile.FloatAmplitude).Within(.001));
            Assert.That(ending.Bed.position.z, Is.GreaterThan(before.z));
            Assert.That(Vector3.Distance(local, ending.Bed.InverseTransformPoint(ending.ViewCamera.transform.position)), Is.LessThan(.001));
        }

        [Test]
        public void PauseFreezesPresentationAndSubtitlesThenResumeContinues()
        {
            ending.AdvanceForTests(1, 10); ending.TryLieDown();
            ending.AdvanceForTests(0, profile.LieDownSeconds + profile.SubtitleDelay + 1);
            Assert.That(ending.CurrentSubtitle, Is.EqualTo(profile.EndingLines[0].Text));
            var before = ending.CaptureCheckpoint();
            var position = ending.ViewCamera.transform.position;
            ending.SetPaused(true);
            ending.AdvanceForTests(1, 60);
            Assert.That(ending.CaptureCheckpoint().DriftElapsed, Is.EqualTo(before.DriftElapsed));
            Assert.That(ending.ViewCamera.transform.position, Is.EqualTo(position));
            Assert.That(ending.TryLieDown(), Is.False);
            ending.SetPaused(false); ending.AdvanceForTests(0, 1);
            Assert.That(ending.CaptureCheckpoint().DriftElapsed, Is.EqualTo(before.DriftElapsed + 1).Within(.001));
        }

        [UnityTest]
        public IEnumerator RestoringSubtitleSeeksVoiceAndCompletedLinesNeverRestart()
        {
            voice = AudioClip.Create("Test voice", 44100 * 4, 1, 44100, false);
            profile.EndingLines[2].Voice = voice;
            ending.RestoreCheckpoint(new BeachEndingCheckpoint { Phase = BeachEndingPhase.Drifting,
                DriftElapsed = 14, SubtitleIndex = 2, SubtitleElapsed = 1.2f, WalkDistance = 4.35f });
            Assert.That(ending.CurrentSubtitle, Is.EqualTo(profile.EndingLines[2].Text));
            var source = ending.Audio.GetSource(MiniGameAudioChannel.Voice);
            Assert.That(source.clip, Is.EqualTo(voice));
            Assert.That(source.time, Is.GreaterThanOrEqualTo(1.19f), "Load resumes the line instead of replaying from the beginning.");
            ending.AdvanceForTests(0, 50);
            Assert.That(ending.CaptureCheckpoint().SubtitlesComplete, Is.True);
            Assert.That(ending.CurrentSubtitle, Is.Empty);
            var complete = ending.CaptureCheckpoint();
            ending.RestoreCheckpoint(complete);
            ending.AdvanceForTests(0, 100);
            Assert.That(ending.CurrentSubtitle, Is.Empty);
            Assert.That(source.isPlaying, Is.False);
            yield return null;
        }

        [Test]
        public void LyingCheckpointRestoresExactCameraPoseAndRejectsInvalidNumbers()
        {
            ending.AdvanceForTests(1, 10); ending.TryLieDown(); ending.AdvanceForTests(0, .6f);
            var saved = ending.CaptureCheckpoint();
            var position = ending.ViewCamera.transform.position;
            var rotation = ending.ViewCamera.transform.rotation;
            ending.AdvanceForTests(0, 5); ending.RestoreCheckpoint(saved);
            Assert.That(Vector3.Distance(ending.ViewCamera.transform.position, position), Is.LessThan(.001));
            Assert.That(Quaternion.Angle(ending.ViewCamera.transform.rotation, rotation), Is.LessThan(.01));
            ending.RestoreCheckpoint(new BeachEndingCheckpoint { WalkDistance = float.NaN, PhaseElapsed = float.PositiveInfinity });
            ending.AdvanceForTests(float.NaN, float.NaN);
            Assert.That(float.IsNaN(ending.ViewCamera.transform.position.z), Is.False);
        }

        [UnityTest]
        public IEnumerator SeaAndWetSandShareLiveParametersPausedClockAndRestoredCheckpoint()
        {
            profile.SeaWaveHeight = .34f; profile.SeaWavePeriod = 4;
            profile.SeaWaveLength = 11; profile.ShoreWashDistance = 2.2f; profile.SeaFoamAmount = 1.1f;
            ending.AdvanceForTests(0, .4f);
            var seaInstance = seaSurface.sharedMaterial;
            var sandInstance = sandSurface.sharedMaterial;
            Assert.That(seaInstance, Is.Not.SameAs(originalSea));
            Assert.That(sandInstance, Is.Not.SameAs(originalSand));
            foreach (var material in new[] { seaInstance, sandInstance })
            {
                Assert.That(material.HasProperty("_WaveHeight"), Is.True, material.shader.name);
                Assert.That(material.GetFloat("_WaveHeight"), Is.EqualTo(.34f).Within(.0001));
                Assert.That(material.GetFloat("_WavePeriod"), Is.EqualTo(4));
                Assert.That(material.GetFloat("_WaveLength"), Is.EqualTo(11));
                Assert.That(material.GetFloat("_ShoreWash"), Is.EqualTo(2.2f).Within(.0001));
                Assert.That(material.GetFloat("_FoamAmount"), Is.EqualTo(1.1f).Within(.0001));
                Assert.That(material.GetFloat("_UseDreamTime"), Is.EqualTo(1));
                Assert.That(material.GetFloat("_DreamTime"), Is.EqualTo(.4f).Within(.0001));
            }
            var saved = ending.CaptureCheckpoint();
            ending.AdvanceForTests(0, 5);
            ending.SetPaused(true);
            ending.RestoreCheckpoint(saved);
            profile.SeaWaveHeight = .17f;
            ending.AdvanceForTests(0, 100);
            foreach (var material in new[] { seaInstance, sandInstance })
            {
                Assert.That(material.GetFloat("_DreamTime"), Is.EqualTo(saved.WorldTime).Within(.0001));
                Assert.That(material.GetFloat("_WaveHeight"), Is.EqualTo(.17f).Within(.0001), "Parameter edits apply during pause without advancing time.");
            }
            Assert.That(originalSea.GetFloat("_DreamTime"), Is.EqualTo(-17));
            Assert.That(originalSand.GetFloat("_DreamTime"), Is.EqualTo(-23));
            ending.SetPaused(false);
            ending.AdvanceForTests(0, .25f);
            Assert.That(seaInstance.GetFloat("_DreamTime"), Is.EqualTo(saved.WorldTime + .25f).Within(.0001));
            Assert.That(sandInstance.GetFloat("_DreamTime"), Is.EqualTo(saved.WorldTime + .25f).Within(.0001));
            Object.Destroy(ending);
            yield return null;
            Assert.That(seaSurface.sharedMaterial, Is.SameAs(originalSea));
            Assert.That(sandSurface.sharedMaterial, Is.SameAs(originalSand));
            yield return null;
            Assert.That(seaInstance == null, Is.True);
            Assert.That(sandInstance == null, Is.True);
        }
    }
}
