using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace MiniGame.UnityHost.Tests
{
    public sealed class RetroScreenPlayModeTests
    {
        [UnityTest]
        public IEnumerator MainRoomUsesProfileAndKeepsOverlayUiOutsideTheEffect()
        {
            yield return SceneManager.LoadSceneAsync("MainRoomCombined");
            yield return null;
            var host = Object.FindObjectOfType<MiniGameBootstrap>();
            var effect = host.Player.ViewCamera.GetComponent<RetroScreenEffect>();
            Assert.That(effect, Is.Not.Null);
            Assert.That(effect.Profile, Is.SameAs(Resources.Load<RetroScreenProfile>("RetroScreenDefault")));
            Assert.That(host.Screen.Canvas.renderMode, Is.EqualTo(RenderMode.ScreenSpaceOverlay));
            string profileBefore = JsonUtility.ToJson(effect.Profile);
            effect.PreviewOriginal = true;
            yield return null;
            Assert.That(JsonUtility.ToJson(effect.Profile), Is.EqualTo(profileBefore));
            effect.enabled = false;
            Assert.That(effect.PreviewOriginal, Is.False);
            effect.enabled = true;
            yield return SceneManager.LoadSceneAsync("MainRoomCombined");
            yield return null;
            effect = Object.FindObjectOfType<RetroScreenEffect>();
            Assert.That(effect.PreviewOriginal, Is.False);
            Assert.That(JsonUtility.ToJson(effect.Profile), Is.EqualTo(profileBefore));
        }

        [UnityTest]
        public IEnumerator LiveProfileChangesReachTheCameraAndNeutralSettingsMatchOriginal()
        {
            yield return SceneManager.LoadSceneAsync("MainRoomCombined");
            yield return null;
            var effect = Object.FindObjectOfType<RetroScreenEffect>();
            var camera = effect.GetComponent<Camera>();
            var originalProfile = effect.Profile;
            var settings = Object.Instantiate(originalProfile);
            effect.Profile = settings;
            var target = RenderTexture.GetTemporary(257, 145, 24, RenderTextureFormat.ARGB32);
            var texture = new Texture2D(257, 145, TextureFormat.RGBA32, false);
            var oldTarget = camera.targetTexture;
            var oldActive = RenderTexture.active;
            try
            {
                camera.targetTexture = target;
                effect.PreviewOriginal = true;
                var before = Capture(camera, target, texture);
                effect.PreviewOriginal = false;
                settings.Strength = 1; settings.PixelSize = 8; settings.Levels = 4;
                var stylized = Capture(camera, target, texture);
                Assert.That(Difference(before, stylized), Is.GreaterThan(.0001f), "The actual camera must execute the post-process shader.");
                settings.Strength = 0;
                var neutral = Capture(camera, target, texture);
                Assert.That(Difference(before, neutral), Is.LessThan(.00001f), "Zero strength must preserve the complete camera image.");
            }
            finally
            {
                effect.PreviewOriginal = false; effect.Profile = originalProfile;
                camera.targetTexture = oldTarget; RenderTexture.active = oldActive;
                Object.Destroy(settings); Object.Destroy(texture); RenderTexture.ReleaseTemporary(target);
            }
        }

        private static Color32[] Capture(Camera camera, RenderTexture target, Texture2D texture)
        {
            camera.Render(); RenderTexture.active = target;
            texture.ReadPixels(new Rect(0, 0, target.width, target.height), 0, 0); texture.Apply();
            return texture.GetPixels32();
        }

        private static float Difference(Color32[] first, Color32[] second)
        {
            double difference = 0;
            for (int i = 0; i < first.Length; i++)
                difference += Mathf.Abs(first[i].r - second[i].r) + Mathf.Abs(first[i].g - second[i].g) + Mathf.Abs(first[i].b - second[i].b);
            return (float)(difference / (first.Length * 3 * 255));
        }
    }
}
