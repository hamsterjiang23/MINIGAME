using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace MiniGame.UnityHost.Tests
{
    public sealed class PlayerPreferencesPlayModeTests
    {
        private bool hadBrightness, hadVolume, hadDetail;
        private float originalBrightness, originalVolume, listenerVolume;
        private int originalDetail;

        [SetUp]
        public void SavePreferences()
        {
            hadBrightness = PlayerPrefs.HasKey(PlayerPreferences.BrightnessKey);
            hadVolume = PlayerPrefs.HasKey(PlayerPreferences.MasterVolumeKey);
            hadDetail = PlayerPrefs.HasKey(PlayerGraphicsSettings.PreferenceKey);
            originalBrightness = PlayerPrefs.GetFloat(PlayerPreferences.BrightnessKey);
            originalVolume = PlayerPrefs.GetFloat(PlayerPreferences.MasterVolumeKey);
            originalDetail = PlayerPrefs.GetInt(PlayerGraphicsSettings.PreferenceKey);
            listenerVolume = AudioListener.volume;
            PlayerPrefs.DeleteKey(PlayerPreferences.BrightnessKey);
            PlayerPrefs.DeleteKey(PlayerPreferences.MasterVolumeKey);
            PlayerPrefs.DeleteKey(PlayerGraphicsSettings.PreferenceKey);
            PlayerPreferences.Reload(); PlayerGraphicsSettings.Reload();
        }

        [TearDown]
        public void RestorePreferences()
        {
            if (hadBrightness) PlayerPrefs.SetFloat(PlayerPreferences.BrightnessKey, originalBrightness);
            else PlayerPrefs.DeleteKey(PlayerPreferences.BrightnessKey);
            if (hadVolume) PlayerPrefs.SetFloat(PlayerPreferences.MasterVolumeKey, originalVolume);
            else PlayerPrefs.DeleteKey(PlayerPreferences.MasterVolumeKey);
            if (hadDetail) PlayerPrefs.SetInt(PlayerGraphicsSettings.PreferenceKey, originalDetail);
            else PlayerPrefs.DeleteKey(PlayerGraphicsSettings.PreferenceKey);
            PlayerPrefs.Save(); PlayerPreferences.Reload(); PlayerGraphicsSettings.Reload();
            AudioListener.volume = listenerVolume;
        }

        [Test]
        public void DefaultsAndPersistedSettingsControlTheGlobalListenerWithoutASaveGame()
        {
            Assert.That(PlayerPreferences.Brightness, Is.EqualTo(1));
            Assert.That(PlayerPreferences.MasterVolume, Is.EqualTo(1));
            Assert.That(PlayerGraphicsSettings.Detail, Is.EqualTo(PixelDetail.Low));
            PlayerPreferences.SetBrightness(1.25f);
            PlayerPreferences.SetMasterVolume(0);
            Assert.That(AudioListener.volume, Is.Zero);
            AudioListener.volume = 1;
            PlayerPreferences.Reload();
            Assert.That(PlayerPreferences.Brightness, Is.EqualTo(1.25f));
            Assert.That(PlayerPreferences.MasterVolume, Is.Zero);
            Assert.That(AudioListener.volume, Is.Zero, "Reload must apply saved volume, including before the first scene.");
            PlayerPreferences.SetMasterVolume(.4f);
            Assert.That(AudioListener.volume, Is.EqualTo(.4f));
            Assert.That(PlayerPrefs.GetFloat(PlayerPreferences.MasterVolumeKey), Is.EqualTo(.4f));
            Assert.That(PlayerGraphicsSettings.Detail, Is.EqualTo(PixelDetail.Low));
        }

        [Test]
        public void OutOfRangeAndInvalidPreferencesHaveSafeFiniteBounds()
        {
            PlayerPreferences.SetBrightness(-3);
            Assert.That(PlayerPreferences.Brightness, Is.EqualTo(.5f));
            PlayerPreferences.SetBrightness(9);
            Assert.That(PlayerPreferences.Brightness, Is.EqualTo(1.5f));
            PlayerPreferences.SetMasterVolume(9);
            Assert.That(PlayerPreferences.MasterVolume, Is.EqualTo(1));
            PlayerPreferences.SetMasterVolume(-3);
            Assert.That(PlayerPreferences.MasterVolume, Is.Zero);
            PlayerPreferences.SetBrightness(float.NaN);
            PlayerPreferences.SetMasterVolume(float.PositiveInfinity);
            Assert.That(PlayerPreferences.Brightness, Is.EqualTo(1));
            Assert.That(AudioListener.volume, Is.EqualTo(1));
            PlayerPrefs.SetFloat(PlayerPreferences.BrightnessKey, 9);
            PlayerPrefs.SetFloat(PlayerPreferences.MasterVolumeKey, -9);
            PlayerPreferences.Reload();
            Assert.That(PlayerPreferences.Brightness, Is.EqualTo(1.5f));
            Assert.That(AudioListener.volume, Is.Zero);
        }

        [UnityTest]
        public IEnumerator SharedSettingsRefreshBothMenusAndHiddenControlsCannotChangePreferences()
        {
            var canvas = new GameObject("Shared player settings test", typeof(RectTransform), typeof(Canvas));
            GameObject events = EventSystem.current == null ? new GameObject("Settings event system", typeof(EventSystem)) : null;
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            int returned = 0;
            var main = PlayerSettingsView.Create(canvas.transform, font, () => returned++);
            var pause = PauseMenuView.Create(canvas.transform, font, "Pause settings test",
                new[] { "继续游戏" }, new[] { "Esc" }, new System.Action[] { () => { } });
            try
            {
                main.Show("返回主菜单");
                pause.SetVisible(true); pause.ShowSettings();
                Assert.That(pause.SharedSettings, Is.Not.Null);
                Assert.That(pause.PixelDetailButtons, Is.SameAs(pause.SharedSettings.PixelDetailButtons));
                main.BrightnessSlider.value = 1.35f;
                main.VolumeSlider.value = .25f;
                Assert.That(pause.SharedSettings.BrightnessSlider.value, Is.EqualTo(1.35f).Within(.001));
                Assert.That(pause.SharedSettings.VolumeSlider.value, Is.EqualTo(.25f));
                Assert.That(AudioListener.volume, Is.EqualTo(.25f));
                pause.PixelDetailButtons[0].onClick.Invoke();
                yield return null;
                Assert.That(main.PixelDetailButtons[0].transform.Find("Label").GetComponent<Text>().text, Does.Contain("已选择"));
                Assert.That(PlayerGraphicsSettings.PixelSize, Is.EqualTo(8));
                pause.SharedSettings.ReturnButton.onClick.Invoke();
                Assert.That(pause.SettingsVisible, Is.False);
                Assert.That(pause.transform.Find("PauseColumn").gameObject.activeSelf, Is.True);
                main.ReturnButton.onClick.Invoke();
                Assert.That(returned, Is.EqualTo(1));
                main.Hide();
                main.BrightnessSlider.value = .5f;
                main.VolumeSlider.value = 1;
                main.PixelDetailButtons[2].onClick.Invoke();
                Assert.That(PlayerPreferences.Brightness, Is.EqualTo(1.35f).Within(.001));
                Assert.That(PlayerPreferences.MasterVolume, Is.EqualTo(.25f));
                Assert.That(PlayerGraphicsSettings.PixelSize, Is.EqualTo(8));
                main.Show("返回主菜单");
                Assert.That(main.BrightnessSlider.value, Is.EqualTo(1.35f).Within(.001));
                Assert.That(main.ReturnButton.transform.Find("Label").GetComponent<Text>().text, Is.EqualTo("返回主菜单"));
            }
            finally { Object.Destroy(canvas); if (events != null) Object.Destroy(events); }
            yield return null;
        }

        [UnityTest]
        public IEnumerator BrightnessMultipliesTheFinalImageAndPreservesBlackWhenRetroIsBypassed()
        {
            var go = new GameObject("Player brightness GPU test", typeof(Camera), typeof(RetroScreenEffect));
            go.GetComponent<Camera>().enabled = false;
            go.GetComponent<RetroScreenEffect>().PresentationBypass = true;
            var effect = PlayerBrightnessEffect.EnsureFor(go.GetComponent<Camera>());
            Assert.That(PlayerBrightnessEffect.EnsureFor(go.GetComponent<Camera>()), Is.SameAs(effect));
            Assert.That(go.GetComponents<PlayerBrightnessEffect>().Length, Is.EqualTo(1));
            var source = RenderTexture.GetTemporary(8, 8, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            var output = RenderTexture.GetTemporary(8, 8, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            var input = new Texture2D(8, 8, TextureFormat.RGBAFloat, false, true);
            var readback = new Texture2D(8, 8, TextureFormat.RGBAFloat, false, true);
            var active = RenderTexture.active;
            try
            {
                var colors = new Color[64];
                for (int i = 0; i < colors.Length; i++) colors[i] = i % 8 < 4 ? new Color(0, 0, 0, .6f) : new Color(.2f, .3f, .4f, .6f);
                input.SetPixels(colors); input.Apply(); Graphics.Blit(input, source);
                foreach (float value in new[] { .5f, 1, 1.5f })
                {
                    PlayerPreferences.SetBrightness(value); effect.Render(source, output);
                    RenderTexture.active = output; readback.ReadPixels(new Rect(0, 0, 8, 8), 0, 0); readback.Apply();
                    var pixels = readback.GetPixels();
                    for (int i = 0; i < colors.Length; i++)
                    {
                        Assert.That(pixels[i].r, Is.EqualTo(colors[i].r * value).Within(.002));
                        Assert.That(pixels[i].g, Is.EqualTo(colors[i].g * value).Within(.002));
                        Assert.That(pixels[i].b, Is.EqualTo(colors[i].b * value).Within(.002));
                        Assert.That(pixels[i].a, Is.EqualTo(colors[i].a).Within(.002));
                    }
                }
            }
            finally
            {
                RenderTexture.active = active;
                RenderTexture.ReleaseTemporary(source); RenderTexture.ReleaseTemporary(output);
                Object.Destroy(input); Object.Destroy(readback); Object.Destroy(go);
            }
            yield return null;
        }
    }
}
