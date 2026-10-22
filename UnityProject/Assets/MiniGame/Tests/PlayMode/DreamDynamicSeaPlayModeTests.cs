using System;
using System.Collections;
using System.Collections.Generic;
using MiniGame.Core;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace MiniGame.UnityHost.Tests
{
    public sealed class DreamDynamicSeaPlayModeTests
    {
        private readonly List<Object> owned = new List<Object>();
        private RoomDreamEnding room;
        private MiniGameBootstrap host;

        [UnityTearDown]
        public IEnumerator Cleanup()
        {
            if (room != null) room.StopAndRestore();
            if (host != null)
            {
                host.enabled = false; host.Player.enabled = false;
                host.Dreams?.PrepareForRuntimeChange(); host.Effects?.PrepareForRuntimeChange();
            }
            MiniGameSessionHost.Release();
            foreach (var value in owned) if (value != null) Object.Destroy(value);
            owned.Clear();
            Cursor.lockState = CursorLockMode.None; Cursor.visible = true;
            yield return null;
        }

        [UnityTest]
        public IEnumerator StaticPanoramaDoesNotRequireTheOptionalSeaSource()
        {
            var profile = Own(Object.Instantiate(Resources.Load<DreamSequenceProfile>("MiniGame/DreamSequenceDefault")));
            profile.CubemapFaceSize = 256;
            profile.AnimateRoomWater = false;
            profile.CubemapSourcePrefab = null;
            var viewer = Own(new GameObject("Static panorama without bake source", typeof(Camera))).GetComponent<Camera>();
            viewer.enabled = false; viewer.cullingMask = 0;
            using (var capture = new DreamCubemapRenderer(profile, viewer, profile.RoomPanoramaSkybox, Vector3.zero))
            {
                capture.Render(0, BeachEndingPhase.Walking, 0, 0, true);
                Assert.That(capture.IsReady, Is.True);
                Assert.That(capture.HasPanoramaWater, Is.False);
            }
            yield return null;
        }

        [UnityTest]
        public IEnumerator AuthoredLegacySeaMovesWithoutMovingCaptureAndSupportsLivePausedEdits()
        {
            var profile = Own(Object.Instantiate(Resources.Load<DreamSequenceProfile>("MiniGame/DreamSequenceDefault")));
            profile.CubemapFaceSize = 256;
            Assert.That(profile.PanoramaWaterMask, Is.Not.Null);
            var viewer = Own(new GameObject("Dynamic sea test eye", typeof(Camera))).GetComponent<Camera>();
            viewer.enabled = false; viewer.cullingMask = 0;
            var origin = new Vector3(600, 30, 600);
            viewer.transform.position = origin;
            using (var capture = new DreamCubemapRenderer(profile, viewer, profile.RoomPanoramaSkybox, origin))
            {
                capture.Render(0, BeachEndingPhase.Walking, 0, 0, true);
                var first = Own(Read(capture.Texture));
                capture.Render(2.1f, BeachEndingPhase.Walking, 0, 0, true);
                var second = Own(Read(capture.Texture));
                Assert.That(Changed(first, second), Is.GreaterThan(128), "Legacy sea geometry and foam must move on the real GPU.");
                Assert.That(capture.PanoramaWaterMaterial.shader.name, Is.EqualTo("MiniGame/Dream Sea"));
                int frames = capture.CapturedFrames;
                capture.Render(2.1f, BeachEndingPhase.Walking, 0, 0);
                Assert.That(capture.CapturedFrames, Is.EqualTo(frames), "A frozen clock must not animate the water.");
                profile.SeaWaveHeight = .48f;
                capture.Render(2.1f, BeachEndingPhase.Walking, 0, 0);
                Assert.That(capture.CapturedFrames, Is.EqualTo(frames + 1), "An authored parameter edit refreshes even with a frozen clock.");
                Assert.That(capture.PanoramaWaterMaterial.GetFloat("_WaveHeight"), Is.EqualTo(.48f).Within(.0001f));
                viewer.transform.position += new Vector3(2, .5f, 1);
                capture.Render(3, BeachEndingPhase.Walking, 0, 0);
                Assert.That(capture.CaptureCamera.transform.position, Is.EqualTo(origin), "Walking must retain the doorway projection eye.");
                profile.AnimateRoomWater = false;
                capture.Render(3, BeachEndingPhase.Walking, 0, 0);
                frames = capture.CapturedFrames;
                capture.Render(20, BeachEndingPhase.Walking, 0, 0);
                Assert.That(capture.CapturedFrames, Is.EqualTo(frames), "Turning waves off restores a static panorama.");
            }
            yield return null;
        }

        [UnityTest, Timeout(90000)]
        public IEnumerator CombinedDreamUsesIndependentRetroAndRestoresRoomAfterPauseAndLiveTuning()
        {
            MiniGameSessionHost.Release();
            yield return null;
            yield return SceneManager.LoadSceneAsync(MiniGameSessionHost.CombinedScene);
            yield return null;
            host = Object.FindObjectOfType<MiniGameBootstrap>();
            Assert.That(host, Is.Not.Null);
            var profile = Own(Object.Instantiate(host.DreamProfile));
            profile.CubemapFaceSize = 256;
            host.ConfigureDreams(profile, Resources.Load<PlayerRebellionProfile>("MiniGame/PlayerRebellion"));
            host.StartCampaign(); host.Runtime.SetPaused(false);
            var retro = host.Player.ViewCamera.GetComponent<RetroScreenEffect>();
            Assert.That(retro, Is.Not.Null);
            var previousProfile = retro.Profile;
            bool follow = retro.FollowRoomLighting;
            var outer = Own(ScriptableObject.CreateInstance<RetroScreenProfile>());
            retro.PresentationProfile = outer; retro.PresentationBypass = true;
            retro.enabled = false; retro.PreviewOriginal = true;
            room = RoomDreamEnding.Begin(host, MiniGameSessionHost.Current, true);
            float deadline = Time.realtimeSinceStartup + 15;
            while (room != null && !room.Ready && string.IsNullOrEmpty(room.FailureReason) && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.That(room.Ready, Is.True, room.FailureReason);
            var ending = room.Ending; ending.UseExternalClock = true;
            Assert.That(retro.PresentationProfile, Is.SameAs(profile.EndingRetro));
            Assert.That(retro.PresentationBypass, Is.False);
            Assert.That(retro.PreviewOriginal, Is.False);
            Assert.That(retro.enabled, Is.True);
            ending.AdvanceForTests(0, 1);
            ending.SetPaused(true);
            float time = ending.WorldTime;
            int frames = room.Cubemap.CapturedFrames;
            ending.AdvanceForTests(0, 10);
            Assert.That(ending.WorldTime, Is.EqualTo(time));
            Assert.That(room.Cubemap.CapturedFrames, Is.EqualTo(frames));
            profile.SeaWaveHeight = .33f;
            ending.AdvanceForTests(0, 0);
            Assert.That(room.Cubemap.PanoramaWaterMaterial.GetFloat("_WaveHeight"), Is.EqualTo(.33f).Within(.0001f));
            Assert.That(ending.WorldTime, Is.EqualTo(time));
            profile.EnableEndingRetro = false; ending.AdvanceForTests(0, 0);
            Assert.That(retro.PresentationBypass, Is.True);
            profile.EnableEndingRetro = true; ending.AdvanceForTests(0, 0);
            Assert.That(retro.PresentationBypass, Is.False);
            room.StopAndRestore();
            Assert.That(retro.PresentationProfile, Is.SameAs(outer));
            Assert.That(retro.Profile, Is.SameAs(previousProfile));
            Assert.That(retro.FollowRoomLighting, Is.EqualTo(follow));
            Assert.That(retro.PresentationBypass, Is.True);
            Assert.That(retro.enabled, Is.False);
            Assert.That(retro.PreviewOriginal, Is.True);
        }

        private T Own<T>(T value) where T : Object { owned.Add(value); return value; }
        private static Texture2D Read(RenderTexture cube)
        {
            var previous = RenderTexture.active;
            try
            {
                Graphics.SetRenderTarget(cube, 0, CubemapFace.PositiveZ);
                var image = new Texture2D(cube.width, cube.height, TextureFormat.RGB24, false);
                image.ReadPixels(new Rect(0, 0, cube.width, cube.height), 0, 0); image.Apply();
                return image;
            }
            finally { RenderTexture.active = previous; }
        }
        private static int Changed(Texture2D first, Texture2D second)
        {
            var a = first.GetPixels32(); var b = second.GetPixels32(); int count = 0;
            for (int i = 0; i < a.Length; i++)
                if (Math.Abs(a[i].r - b[i].r) + Math.Abs(a[i].g - b[i].g) + Math.Abs(a[i].b - b[i].b) > 12) count++;
            return count;
        }
    }
}
