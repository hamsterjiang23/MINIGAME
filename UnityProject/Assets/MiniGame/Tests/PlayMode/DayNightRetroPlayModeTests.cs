using System;
using System.Collections;
using System.IO;
using System.Linq;
using MiniGame.Core;
using MiniGame.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace MiniGame.UnityHost.Tests
{
    public sealed class DayNightRetroPlayModeTests
    {
        [UnityTest, Timeout(30000)]
        public IEnumerator PreviewPauseAndCheckpointRestoreShareTheRoomLightingClock()
        {
            MiniGameSessionHost.Release();
            yield return SceneManager.LoadSceneAsync("MainRoomCombined"); yield return null;
            var host = Object.FindObjectOfType<MiniGameBootstrap>(); CampaignWorldFixture.Restore(host);
            var room = Object.FindObjectsOfType<RoomLightingController>().Single(item => item.gameObject.scene == host.gameObject.scene);
            var guide = Object.FindObjectOfType<BedtimeGuidance>();
            var effect = host.Player.ViewCamera.GetComponent<RetroScreenEffect>();
            var sourceDay = room.Profile; var sourceNight = room.NightProfile;
            Assert.That(sourceDay.ScreenStyle, Is.Not.Null);
            Assert.That(sourceNight.ScreenStyle, Is.Not.Null.And.Not.SameAs(sourceDay.ScreenStyle));
            var day = Object.Instantiate(sourceDay); var night = Object.Instantiate(sourceNight);
            day.ScreenStyle = Object.Instantiate(sourceDay.ScreenStyle); night.ScreenStyle = Object.Instantiate(sourceNight.ScreenStyle);
            string sourceDayStyle = JsonUtility.ToJson(sourceDay.ScreenStyle), sourceNightStyle = JsonUtility.ToJson(sourceNight.ScreenStyle);
            try
            {
                room.ConfigureProfiles(day, night); effect.FollowRoomLighting = true;
                room.SetPreviewMode(RoomLightingPreviewMode.Day);
                Assert.That(effect.ActiveProfile, Is.SameAs(day.ScreenStyle));
                yield return Capture(host, "day-default");
                room.SetPreviewMode(RoomLightingPreviewMode.Night);
                Assert.That(effect.ActiveProfile, Is.SameAs(night.ScreenStyle));
                yield return Capture(host, "night-default");
                string dayBefore = JsonUtility.ToJson(day.ScreenStyle);
                night.ScreenStyle.PixelSize = 5; night.ScreenStyle.FilmGrainStrength = 0;
                night.ScreenStyle.ColorGradingEnabled = true; night.ScreenStyle.ShadowExposure = .7f;
                night.ScreenStyle.HighlightExposure = -.6f;
                yield return Capture(host, "night-adjusted-example");
                Assert.That(JsonUtility.ToJson(day.ScreenStyle), Is.EqualTo(dayBefore));
                room.SetPreviewMode(RoomLightingPreviewMode.Automatic);
                var session = MiniGameSessionHost.Current;
                var runtime = new GameRuntime(session.Rules, session.Content, true);
                string morning = runtime.Save();
                Assert.That(runtime.TryInteract(InteractionTarget.Computer).Accepted, Is.True);
                Assert.That(runtime.CompleteCue(runtime.GetView().Cue.Token).Accepted, Is.True);
                Assert.That(runtime.EnterSelectedPatient(runtime.GetView().Game.CurrentPatientId).Accepted, Is.True);
                Assert.That(runtime.CompleteCue(runtime.GetView().Cue.Token).Accepted, Is.True);
                Assert.That(runtime.SubmitDisposition(runtime.GetView().Game.CurrentEncounterId, Disposition.Purify).Accepted, Is.True);
                for (int i = 0; runtime.GetView().Cue != null && i < 5; i++)
                    Assert.That(runtime.CompleteCue(runtime.GetView().Cue.Token).Accepted, Is.True);
                if (runtime.UsesDreamFlow && runtime.GetView().Game.Phase == GamePhase.Evening)
                    Assert.That(runtime.BeginNightDream().Accepted, Is.True);
                Assert.That(new[] { GamePhase.Evening, GamePhase.AfterDream, GamePhase.Dream }, Does.Contain(runtime.GetView().Game.Phase));
                host.RestoreCheckpoint(runtime.Save()); yield return null;
                Assert.That(room.EffectivePeriod, Is.EqualTo(RoomLightingPeriod.Night));
                Assert.That(effect.ActiveProfile, Is.SameAs(night.ScreenStyle));
                Assert.That(host.Runtime.SetPaused(true).Accepted, Is.True);
                float blend = room.NightBlend;
                yield return new WaitForSecondsRealtime(.15f);
                Assert.That(room.NightBlend, Is.EqualTo(blend));
                string paused = host.Runtime.Save();
                host.RestoreCheckpoint(morning); yield return null;
                Assert.That(room.EffectivePeriod, Is.EqualTo(RoomLightingPeriod.Day));
                Assert.That(effect.ActiveProfile, Is.SameAs(day.ScreenStyle));
                Assert.That(room.NightBlend, Is.Zero);
                host.RestoreCheckpoint(paused); yield return null;
                Assert.That(host.Runtime.GetView().Paused, Is.True);
                Assert.That(room.NightBlend, Is.EqualTo(1).Within(.0001f));
                Assert.That(effect.ActiveProfile, Is.SameAs(night.ScreenStyle));
                Debug.Log("DAY_NIGHT_RETRO_PASSED independent styles; preview; dark/highlight changes; pause; checkpoint restore");
            }
            finally
            {
                room.SetPreviewMode(RoomLightingPreviewMode.Automatic); room.ConfigureProfiles(sourceDay, sourceNight);
                Assert.That(JsonUtility.ToJson(sourceDay.ScreenStyle), Is.EqualTo(sourceDayStyle));
                Assert.That(JsonUtility.ToJson(sourceNight.ScreenStyle), Is.EqualTo(sourceNightStyle));
                Object.Destroy(day.ScreenStyle); Object.Destroy(night.ScreenStyle); Object.Destroy(day); Object.Destroy(night);
            }
        }

        private static IEnumerator Capture(MiniGameBootstrap host, string name)
        {
            string folder = Environment.GetEnvironmentVariable("MINIGAME_RETRO_PERIOD_EVIDENCE");
            if (string.IsNullOrEmpty(folder)) yield break;
            yield return null;
            Directory.CreateDirectory(folder);
            var camera = host.Player.ViewCamera;
            var target = RenderTexture.GetTemporary(960, 540, 24);
            var readback = new Texture2D(960, 540, TextureFormat.RGB24, false);
            var old = camera.targetTexture; var active = RenderTexture.active;
            try
            {
                camera.targetTexture = target; camera.Render(); RenderTexture.active = target;
                readback.ReadPixels(new Rect(0, 0, 960, 540), 0, 0); readback.Apply();
                File.WriteAllBytes(Path.Combine(folder, name + ".png"), readback.EncodeToPNG());
            }
            finally
            {
                camera.targetTexture = old; RenderTexture.active = active;
                RenderTexture.ReleaseTemporary(target); Object.Destroy(readback);
            }
        }
    }
}
