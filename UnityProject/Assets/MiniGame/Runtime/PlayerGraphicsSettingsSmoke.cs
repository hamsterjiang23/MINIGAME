#if UNITY_EDITOR || DEVELOPMENT_BUILD || MINIGAME_FEEDBACK_PROBE
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using MiniGame.Presentation;
using UnityEngine;

namespace MiniGame.UnityHost
{
    /// <summary>Opt-in release-Player checks. Normal launches never change a preference or auto-open a menu.</summary>
    public sealed class PlayerGraphicsSettingsSmoke : MonoBehaviour
    {
        [Serializable] private sealed class Choice
        {
            public string label, capture;
            public int selectedPixels;
            public float shaderPixels, shaderVirtualResolution;
        }
        [Serializable] private sealed class Report
        {
            public bool passed, originalHadKey, restartLoaded, sourceProfilesUnchanged, pausedAfterReturn;
            public int originalValue, initialPixels;
            public string error, unity, graphics;
            public Choice[] choices;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            var args = Environment.GetCommandLineArgs();
            if (args.Contains("--pixel-settings-smoke") || args.Contains("--pixel-settings-reload-smoke"))
                new GameObject("Player graphics settings verification").AddComponent<PlayerGraphicsSettingsSmoke>();
        }

        private IEnumerator Start()
        {
            yield return null; yield return null;
            string folder = Environment.GetEnvironmentVariable("MINIGAME_PIXEL_SETTINGS_EVIDENCE");
            if (string.IsNullOrEmpty(folder)) { Debug.LogError("Pixel settings evidence directory missing."); Application.Quit(1); yield break; }
            Directory.CreateDirectory(folder);
            string firstReport = Path.Combine(folder, "settings-player.json");
            bool reload = Environment.GetCommandLineArgs().Contains("--pixel-settings-reload-smoke");
            var report = new Report { unity = Application.unityVersion, graphics = SystemInfo.graphicsDeviceType.ToString(),
                originalHadKey = PlayerPrefs.HasKey(PlayerGraphicsSettings.PreferenceKey),
                originalValue = PlayerPrefs.GetInt(PlayerGraphicsSettings.PreferenceKey), initialPixels = PlayerGraphicsSettings.PixelSize };
            Report original = report;
            if (reload && File.Exists(firstReport)) original = JsonUtility.FromJson<Report>(File.ReadAllText(firstReport));
            try
            {
                var host = FindObjectOfType<MiniGameBootstrap>();
                Require(host != null && host.Player != null, "Actual game host/camera missing.");
                var effect = host.Player.ViewCamera.GetComponent<RetroScreenEffect>();
                Require(effect != null && effect.UsePlayerPixelSettings, "Actual camera did not enable player preferences.");
                if (reload)
                {
                    Require(original.passed && report.initialPixels == 8 && effect.EffectivePixelSize == 8, "A new process did not restore the saved high/8 choice.");
                    report.restartLoaded = true; report.passed = true;
                }
                else
                {
                    var session = MiniGameSessionHost.Current;
                    host.RestoreCheckpoint(new GameRuntime(session.Rules, session.Content, true).Save());
                    var lighting = FindObjectsOfType<RoomLightingController>().Single(item => item.gameObject.scene == host.gameObject.scene);
                    string day = JsonUtility.ToJson(lighting.Profile.ScreenStyle), night = JsonUtility.ToJson(lighting.NightProfile.ScreenStyle);
                    Require(host.Runtime.SetPaused(true).Accepted, "Pause command rejected.");
                    var menu = host.Screen.GetComponentInChildren<PauseMenuView>(true);
                    var entry = menu.Buttons.Single(button => button.transform.Find("Label").GetComponent<UnityEngine.UI.Text>().text == "游戏设置");
                    entry.onClick.Invoke(); Require(menu.SettingsVisible, "The actual settings menu did not open.");
                    report.choices = new Choice[3];
                    int[] sizes = { 8, 4, 2 }; string[] labels = { "高", "中", "低" };
                    for (int i = 0; i < 3; i++)
                    {
                        menu.PixelDetailButtons[i].onClick.Invoke();
                        string name = "pixel-" + sizes[i] + ".png";
                        Capture(host.Player.ViewCamera, Path.Combine(folder, name));
                        var material = (Material)typeof(RetroScreenEffect).GetField("material", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(effect);
                        Require(material != null, "The actual camera did not execute the pixel shader.");
                        var choice = new Choice { label = labels[i], capture = name, selectedPixels = effect.EffectivePixelSize,
                            shaderPixels = material.GetFloat("_PixelSize"), shaderVirtualResolution = material.GetFloat("_UseVirtualResolution") };
                        Require(choice.selectedPixels == sizes[i] && choice.shaderPixels == sizes[i] && choice.shaderVirtualResolution == 0,
                            "Player choice did not reach actual render uniforms.");
                        report.choices[i] = choice;
                    }
                    report.sourceProfilesUnchanged = JsonUtility.ToJson(lighting.Profile.ScreenStyle) == day && JsonUtility.ToJson(lighting.NightProfile.ScreenStyle) == night;
                    Require(report.sourceProfilesUnchanged, "Player setting changed an authored style.");
                    host.HandleEscape(); report.pausedAfterReturn = host.Runtime.GetView().Paused && !menu.SettingsVisible;
                    Require(report.pausedAfterReturn, "First Escape must return to pause without resuming gameplay.");
                    // Leave high/8 for the next process; that second run restores the player's prior preference.
                    PlayerGraphicsSettings.SetDetail(PixelDetail.High);
                    report.passed = true;
                }
            }
            catch (Exception exception) { report.error = exception.ToString(); }
            finally
            {
                if (reload || !report.passed)
                {
                    if (original.originalHadKey) PlayerPrefs.SetInt(PlayerGraphicsSettings.PreferenceKey, original.originalValue);
                    else PlayerPrefs.DeleteKey(PlayerGraphicsSettings.PreferenceKey);
                    PlayerPrefs.Save(); PlayerGraphicsSettings.Reload();
                }
                File.WriteAllText(Path.Combine(folder, reload ? "settings-reload-player.json" : "settings-player.json"), JsonUtility.ToJson(report, true));
            }
            Debug.Log("MINIGAME_PIXEL_SETTINGS_" + (report.passed ? "PASSED" : "FAILED") + " " + JsonUtility.ToJson(report));
            Application.Quit(report.passed ? 0 : 1);
        }

        private static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
        private static void Capture(Camera camera, string path)
        {
            var target = RenderTexture.GetTemporary(960, 540, 24);
            var texture = new Texture2D(960, 540, TextureFormat.RGB24, false);
            var old = camera.targetTexture; var active = RenderTexture.active;
            try
            {
                camera.targetTexture = target; camera.Render(); RenderTexture.active = target;
                texture.ReadPixels(new Rect(0, 0, 960, 540), 0, 0); texture.Apply(); File.WriteAllBytes(path, texture.EncodeToPNG());
            }
            finally
            {
                camera.targetTexture = old; RenderTexture.active = active; RenderTexture.ReleaseTemporary(target); Destroy(texture);
            }
        }
    }
}
#endif
