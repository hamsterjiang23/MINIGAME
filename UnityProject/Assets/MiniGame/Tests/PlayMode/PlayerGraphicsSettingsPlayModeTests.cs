using System.Collections;
using System.Linq;
using MiniGame.Core;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace MiniGame.UnityHost.Tests
{
    public sealed class PlayerGraphicsSettingsPlayModeTests
    {
        private bool hadPreference;
        private int originalPreference;

        [SetUp]
        public void SavePreference()
        {
            hadPreference = PlayerPrefs.HasKey(PlayerGraphicsSettings.PreferenceKey);
            originalPreference = PlayerPrefs.GetInt(PlayerGraphicsSettings.PreferenceKey);
            PlayerPrefs.DeleteKey(PlayerGraphicsSettings.PreferenceKey); PlayerGraphicsSettings.Reload();
        }

        [TearDown]
        public void RestorePreference()
        {
            if (hadPreference) PlayerPrefs.SetInt(PlayerGraphicsSettings.PreferenceKey, originalPreference);
            else PlayerPrefs.DeleteKey(PlayerGraphicsSettings.PreferenceKey);
            PlayerPrefs.Save(); PlayerGraphicsSettings.Reload();
        }

        [Test]
        public void DefaultIsLowTwoAndEachChoicePersistsWithoutAStorySave()
        {
            Assert.That(PlayerGraphicsSettings.Detail, Is.EqualTo(PixelDetail.Low));
            Assert.That(PlayerGraphicsSettings.PixelSize, Is.EqualTo(2));
            foreach (var detail in new[] { PixelDetail.High, PixelDetail.Medium, PixelDetail.Low })
            {
                PlayerGraphicsSettings.SetDetail(detail); PlayerGraphicsSettings.Reload();
                Assert.That(PlayerGraphicsSettings.Detail, Is.EqualTo(detail));
                Assert.That(PlayerPrefs.GetInt(PlayerGraphicsSettings.PreferenceKey), Is.EqualTo((int)detail));
            }
            PlayerPrefs.SetInt(PlayerGraphicsSettings.PreferenceKey, 999); PlayerGraphicsSettings.Reload();
            Assert.That(PlayerGraphicsSettings.PixelSize, Is.EqualTo(2));
        }

        [UnityTest]
        public IEnumerator ShaderRendersEightFourTwoBlocksAndNeverEditsTheAuthoredProfile()
        {
            var camera = new GameObject("Player pixel settings GPU test", typeof(Camera));
            camera.GetComponent<Camera>().enabled = false;
            var effect = camera.AddComponent<RetroScreenEffect>();
            var profile = ScriptableObject.CreateInstance<RetroScreenProfile>(); profile.ResetToNaturalDefaults();
            profile.UseVirtualResolution = true; profile.VirtualHeight = 540; profile.PixelSize = 3;
            effect.Configure(profile, Resources.Load<Shader>("RetroScreen")); effect.UsePlayerPixelSettings = true;
            string before = JsonUtility.ToJson(profile);
            try
            {
                Color[] last = null;
                foreach (var detail in new[] { PixelDetail.High, PixelDetail.Medium, PixelDetail.Low })
                {
                    PlayerGraphicsSettings.SetDetail(detail);
                    var pixels = Render(effect);
                    int size = (int)detail;
                    Assert.That(effect.EffectivePixelSize, Is.EqualTo(size));
                    for (int y = 0; y < 32; y++) for (int x = 0; x < 32; x++)
                        Assert.That(Vector4.Distance(pixels[y * 32 + x], pixels[y / size * size * 32 + x / size * size]),
                            Is.LessThan(.00001f), "Every selected pixel block must sample one source texel.");
                    Assert.That(pixels[0], Is.Not.EqualTo(pixels[size]));
                    if (last != null) Assert.That(pixels, Is.Not.EqualTo(last));
                    last = pixels;
                }
                Assert.That(JsonUtility.ToJson(profile), Is.EqualTo(before));
                effect.PresentationProfile = profile;
                Assert.That(effect.PlayerPixelSettingsActive, Is.False, "Dream presentation profiles retain their own sampling.");
                Assert.That(effect.EffectivePixelSize, Is.EqualTo(3));
                effect.PresentationProfile = null;
                effect.PreviewOriginal = true;
                Assert.That(Render(effect)[0], Is.Not.EqualTo(Render(effect)[1]));
            }
            finally { Object.Destroy(camera); Object.Destroy(profile); }
            yield return null;
        }

        [UnityTest, Timeout(30000)]
        public IEnumerator ActualPauseSettingsClicksApplyImmediatelyAndEscapeReturnsBeforeResuming()
        {
            MiniGameSessionHost.Release();
            yield return SceneManager.LoadSceneAsync("MainRoomCombined"); yield return null;
            var host = Object.FindObjectOfType<MiniGameBootstrap>(); CampaignWorldFixture.Restore(host);
            var view = host.Screen.GetComponentInChildren<PauseMenuView>(true);
            var effect = host.Player.ViewCamera.GetComponent<RetroScreenEffect>();
            var lighting = Object.FindObjectsOfType<RoomLightingController>().Single(item => item.gameObject.scene == host.gameObject.scene);
            string day = JsonUtility.ToJson(lighting.Profile.ScreenStyle), night = JsonUtility.ToJson(lighting.NightProfile.ScreenStyle);
            Assert.That(effect.UsePlayerPixelSettings, Is.True);
            Assert.That(effect.EffectivePixelSize, Is.EqualTo(2));
            Assert.That(host.Runtime.SetPaused(true).Accepted, Is.True); yield return null;
            var entry = view.Buttons.Single(button => button.transform.Find("Label").GetComponent<Text>().text == "游戏设置");
            EventSystem.current.SetSelectedGameObject(entry.gameObject); entry.onClick.Invoke();
            Assert.That(view.SettingsVisible, Is.True);
            Assert.That(view.transform.Find("PauseColumn").gameObject.activeSelf, Is.False);
            Assert.That(view.transform.Find("PauseDefocusedScene").GetComponent<RawImage>().enabled, Is.False);
            Assert.That(host.Screen.PauseSubmenuVisible, Is.True);
            string game = host.Runtime.Save();
            foreach (var pair in new[] { (0, 8), (1, 4), (2, 2) })
            {
                var button = view.PixelDetailButtons[pair.Item1];
                button.onClick.Invoke(); yield return null;
                Assert.That(effect.EffectivePixelSize, Is.EqualTo(pair.Item2));
                Assert.That(button.transform.Find("Label").GetComponent<Text>().text, Does.Contain("已选择"));
                lighting.SetPreviewMode(RoomLightingPreviewMode.Night);
                Assert.That(effect.EffectivePixelSize, Is.EqualTo(pair.Item2));
                lighting.SetPreviewMode(RoomLightingPreviewMode.Day);
            }
            foreach (var button in view.Buttons) button.onClick.Invoke();
            Assert.That(host.Runtime.Save(), Is.EqualTo(game), "Hidden pause actions cannot save, resume or replace the story.");
            Assert.That(JsonUtility.ToJson(lighting.Profile.ScreenStyle), Is.EqualTo(day));
            Assert.That(JsonUtility.ToJson(lighting.NightProfile.ScreenStyle), Is.EqualTo(night));
            host.HandleEscape();
            Assert.That(view.SettingsVisible, Is.False); Assert.That(host.Runtime.GetView().Paused, Is.True);
            Assert.That(EventSystem.current.currentSelectedGameObject, Is.SameAs(entry.gameObject));
            yield return null; host.HandleEscape(); Assert.That(host.Runtime.GetView().Paused, Is.False);
            PlayerGraphicsSettings.SetDetail(PixelDetail.High);
            yield return SceneManager.LoadSceneAsync("MainRoomCombined"); yield return null;
            host = Object.FindObjectOfType<MiniGameBootstrap>();
            Assert.That(host.Player.ViewCamera.GetComponent<RetroScreenEffect>().EffectivePixelSize, Is.EqualTo(8));
        }

        private static Color[] Render(RetroScreenEffect effect)
        {
            var source = RenderTexture.GetTemporary(32, 32, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            var target = RenderTexture.GetTemporary(32, 32, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            var input = new Texture2D(32, 32, TextureFormat.RGBAFloat, false, true);
            var readback = new Texture2D(32, 32, TextureFormat.RGBAFloat, false, true);
            var active = RenderTexture.active;
            try
            {
                var colors = new Color[32 * 32];
                for (int y = 0; y < 32; y++) for (int x = 0; x < 32; x++) colors[y * 32 + x] = new Color(x / 32f, y / 32f, .25f, 1);
                input.SetPixels(colors); input.Apply(); Graphics.Blit(input, source); effect.Render(source, target);
                RenderTexture.active = target; readback.ReadPixels(new Rect(0, 0, 32, 32), 0, 0); readback.Apply();
                return readback.GetPixels();
            }
            finally
            {
                RenderTexture.active = active; Object.DestroyImmediate(input); Object.DestroyImmediate(readback);
                RenderTexture.ReleaseTemporary(source); RenderTexture.ReleaseTemporary(target);
            }
        }
    }
}
