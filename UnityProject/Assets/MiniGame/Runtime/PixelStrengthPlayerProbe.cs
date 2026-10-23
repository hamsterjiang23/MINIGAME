#if DEVELOPMENT_BUILD || UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MiniGame.UnityHost
{
    /// <summary>Explicit development verification; normal launches never touch preferences or saves.</summary>
    public sealed class PixelStrengthPlayerProbe : MonoBehaviour
    {
        private const string Flag = "--pixel-strength-smoke";
        private static bool requested, preferencesBackedUp, preferencesRestored;
        private static string outputFolder, isolatedSaveDirectory, startupError, originalSaveOverride;
        private static bool originalSkipStartup;
        private static PreferenceBackup preferences;
        internal static string IsolatedSaveDirectory => isolatedSaveDirectory;
        private readonly ProbeReport report = new ProbeReport();
        private MiniGameBootstrap host;
        private MainMenuController menu;
        private float began;
        private bool finished;
        private bool originalRunInBackground;
        private BaseInputModule[] inputModules = Array.Empty<BaseInputModule>();
        private bool[] inputModuleStates = Array.Empty<bool>();
        private int exitCode;
        public bool Completed => finished;
        public bool Passed => finished && exitCode == 0;

        [Serializable] private sealed class PreferenceBackup
        {
            public bool graphicsHadKey, onboardingHadKey;
            public int graphicsValue, onboardingValue;
        }
        [Serializable] private sealed class CaptureRecord
        {
            public string file, page, selection;
            public int width, height, frame, gridCount;
        }
        [Serializable] private sealed class ProbeReport
        {
            public bool passed, preferencesRestored, defaultMedium, provisionalChoiceDidNotSave;
            public bool escapeDidNotSkip, confirmDidNotStartGame, confirmationSurvivesReload, mainMenuTooltips, pauseTooltips;
            public int isolatedInputModules;
            public string environment, unity, graphicsDevice, error, isolatedSaveDirectory;
            public string inputMode = "Programmatic EventSystem pointer events after screen-space GraphicRaycaster hit verification; controller keyboard handler calls. Physical input module temporarily disabled to isolate captures, then restored. Not physical device input.";
            public string captureMode = "ScreenCapture.CaptureScreenshotAsTexture after WaitForEndOfFrame: complete GPU frame including overlay canvases, with native platform orientation.";
            public List<CaptureRecord> captures = new List<CaptureRecord>();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Prepare()
        {
            isolatedSaveDirectory = null;
            requested = Array.IndexOf(Environment.GetCommandLineArgs(), Flag) >= 0;
            if (!requested) return;
            string[] args = Environment.GetCommandLineArgs();
            int index = Array.IndexOf(args, "--pixel-strength-output");
            outputFolder = Path.Combine(Path.GetTempPath(), "MiniGame-pixel-strength-" + Guid.NewGuid().ToString("N"));
            try
            {
                if (index < 0 || index + 1 >= args.Length || !Path.IsPathRooted(args[index + 1]))
                    throw new ArgumentException("--pixel-strength-output requires an absolute output directory.");
                outputFolder = Path.GetFullPath(args[index + 1]);
                PrepareScope();
            }
            catch (Exception error) { startupError = error.ToString(); RestorePreferences(); }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            if (!requested) return;
            var owner = new GameObject("Pixel strength Player verification");
            DontDestroyOnLoad(owner);
            owner.AddComponent<PixelStrengthPlayerProbe>();
        }

#if UNITY_EDITOR
        public static PixelStrengthPlayerProbe StartForEditor(string outputPath)
        {
            if (!Application.isPlaying) throw new InvalidOperationException("Start the probe in Combined Play Mode.");
            var previous = FindObjectOfType<PixelStrengthPlayerProbe>();
            if (previous != null) throw new InvalidOperationException("Exit Play Mode before starting another pixel probe.");
            outputFolder = Path.GetFullPath(outputPath);
            PrepareScope();
            MainMenuController.Current?.ShowStartup();
            var owner = new GameObject("Pixel strength Editor verification");
            DontDestroyOnLoad(owner);
            return owner.AddComponent<PixelStrengthPlayerProbe>();
        }
#endif

        private static void PrepareScope()
        {
            startupError = null;
            preferencesRestored = false;
            Directory.CreateDirectory(outputFolder);
            preferences = new PreferenceBackup
            {
                graphicsHadKey = PlayerPrefs.HasKey(PlayerGraphicsSettings.PreferenceKey),
                graphicsValue = PlayerPrefs.GetInt(PlayerGraphicsSettings.PreferenceKey),
                onboardingHadKey = PlayerPrefs.HasKey(PixelStrengthOnboarding.PreferenceKey),
                onboardingValue = PlayerPrefs.GetInt(PixelStrengthOnboarding.PreferenceKey)
            };
            File.WriteAllText(Path.Combine(outputFolder, "preferences-backup.json"), JsonUtility.ToJson(preferences, true));
            preferencesBackedUp = true;
            originalSaveOverride = MainMenuController.SaveDirectoryOverride;
            originalSkipStartup = MainMenuController.SkipStartup;
            isolatedSaveDirectory = Path.Combine(outputFolder, "isolated-saves-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(isolatedSaveDirectory);
            MainMenuController.SaveDirectoryOverride = isolatedSaveDirectory;
            MainMenuController.SkipStartup = false;
            PlayerPrefs.DeleteKey(PlayerGraphicsSettings.PreferenceKey);
            PlayerPrefs.DeleteKey(PixelStrengthOnboarding.PreferenceKey);
            PlayerPrefs.Save();
            PlayerGraphicsSettings.Reload();
        }

        private IEnumerator Start()
        {
            began = Time.realtimeSinceStartup;
            report.environment = Application.isEditor ? "Unity Editor Combined Play Mode" : "Windows Player Combined";
            report.unity = Application.unityVersion;
            report.graphicsDevice = SystemInfo.graphicsDeviceType + " / " + SystemInfo.graphicsDeviceName;
            report.isolatedSaveDirectory = isolatedSaveDirectory;
            var routine = Run();
            while (!finished)
            {
                bool more;
                object current = null;
                try { more = routine.MoveNext(); if (more) current = routine.Current; }
                catch (Exception error) { Finish(false, error.ToString()); yield break; }
                if (!more) { Finish(true, null); yield break; }
                yield return current;
            }
        }

        private void Awake()
        {
            originalRunInBackground = Application.runInBackground;
            Application.runInBackground = true;
        }

        private void Update()
        {
            if (!finished && Time.realtimeSinceStartup - began > 120)
                Finish(false, "The 120-second pixel UI probe deadline expired. A visible Game View is required for end-of-frame Editor captures.");
        }

        private IEnumerator Run()
        {
            Require(startupError == null, startupError);
            yield return null;
            yield return null;
            host = FindObjectsOfType<MiniGameBootstrap>().SingleOrDefault(item => item.gameObject.scene.name == "MainRoomCombined");
            Require(host != null, "Expected the formal MainRoomCombined host.");
            menu = MainMenuController.Current;
            Require(menu != null && menu.View.Page == MainMenuPage.PixelSetup, "A clean no-save launch must show pixel setup first.");
            // currentInputModule can still be null before the EventSystem's first activation.
            inputModules = FindObjectsOfType<BaseInputModule>(true);
            inputModuleStates = inputModules.Select(module => module.enabled).ToArray();
            report.isolatedInputModules = inputModules.Length;
            Require(inputModules.Length > 0, "The real scene's UI input module is missing.");
            foreach (var module in inputModules)
            {
                module.enabled = false;
            }
            // EventSystem.Update still calls its cached current module even after that module is disabled.
            // Re-enable the existing systems to clear that cache while preserving raycast/focus APIs.
            foreach (var events in FindObjectsOfType<EventSystem>())
            {
                if (!events.enabled) continue;
                events.enabled = false;
                events.enabled = true;
                Require(events.currentInputModule == null, "Physical input module cache was not cleared.");
            }
            Require(MainMenuController.SaveDirectoryOverride == isolatedSaveDirectory, "Probe save isolation was lost.");
            Require(PlayerGraphicsSettings.Detail == PixelDetail.Medium && menu.View.PixelSetup.SelectedDetail == PixelDetail.Medium,
                "The absent-preference default and initial selection must both be Medium.");
            report.defaultMedium = true;
            Require(menu.View.PixelSetup.PreviewImages.All(item => item.texture != null && item.texture.width == 1280 && item.texture.height == 720),
                "All three actual-render preview assets must be loaded at 1280x720.");
            Require(menu.HandlePixelSetupKey(KeyCode.Escape) && menu.View.Page == MainMenuPage.PixelSetup,
                "Escape must be consumed without skipping first setup.");
            report.escapeDidNotSkip = true;
            yield return new WaitForEndOfFrame();
            Capture("onboarding-default-medium");
            PixelDetail[] details = { PixelDetail.High, PixelDetail.Medium, PixelDetail.Low };
            for (int i = 0; i < details.Length; i++)
            {
                Click(menu.View.PixelSetup.ChoiceButtons[i]);
                Require(menu.View.PixelSetup.SelectedDetail == details[i], "Preview click selected the wrong preset.");
                Require(!PlayerPrefs.HasKey(PlayerGraphicsSettings.PreferenceKey) && !PixelStrengthOnboarding.Completed,
                    "Provisional preview selection must not save a preference or completion marker.");
                yield return null;
                yield return new WaitForEndOfFrame();
                Capture("onboarding-" + details[i].ToString().ToLowerInvariant());
            }
            report.provisionalChoiceDidNotSave = true;
            Require(menu.HandlePixelSetupKey(KeyCode.LeftArrow) && menu.View.PixelSetup.SelectedDetail == PixelDetail.Medium,
                "Keyboard selection must move Low to Medium.");
            ExecuteEvents.Execute(menu.View.PixelSetup.ConfirmButton.gameObject,
                new BaseEventData(EventSystem.current), ExecuteEvents.submitHandler);
            Require(menu.View.Page == MainMenuPage.PixelSetup,
                "The Unity submit path must defer confirmation to the controller's single keyboard handler.");
            Require(menu.HandlePixelSetupKey(KeyCode.Return), "Enter was not consumed by setup.");
            menu.StartFirstInspection();
            Require(menu.View.Page == MainMenuPage.FirstStart && PixelStrengthOnboarding.Completed
                && PlayerGraphicsSettings.Detail == PixelDetail.Medium, "Confirmation must persist Medium and show the original first-start page.");
            Require(!Directory.EnumerateFiles(isolatedSaveDirectory).Any(), "Pixel setup must not create a campaign save.");
            report.confirmDidNotStartGame = true;
            PlayerGraphicsSettings.Reload();
            menu.ShowStartup();
            Require(menu.View.Page == MainMenuPage.FirstStart && !PixelStrengthOnboarding.ShouldShow(isolatedSaveDirectory),
                "Confirmed setup must stay dismissed even with no campaign save.");
            report.confirmationSurvivesReload = true;
            yield return null;
            yield return new WaitForEndOfFrame();
            Capture("original-first-start-after-confirmation");

            menu.BackToMenu();
            yield return null;
            yield return new WaitForEndOfFrame();
            Click(menu.View.SettingsButton);
            Require(menu.View.Page == MainMenuPage.Settings, "Main-menu settings did not open through its real button.");
            yield return null;
            yield return new WaitForEndOfFrame();
            for (int i = 0; i < details.Length; i++)
            {
                VerifyHover(menu.View.Settings, i);
                yield return null;
                yield return new WaitForEndOfFrame();
                Capture("main-menu-grid-" + details[i].ToString().ToLowerInvariant(), menu.View.Settings.PixelLegend);
                VerifyExitAndFocus(menu.View.Settings, i);
            }
            report.mainMenuTooltips = true;
            yield return new WaitForEndOfFrame();
            Capture("main-menu-no-hover");
            Click(menu.View.Settings.PixelDetailButtons[0]);
            Require(PlayerGraphicsSettings.Detail == PixelDetail.High, "Settings click did not persist the actual High preset.");
            Require(!menu.View.Settings.PixelLegend.gameObject.activeSelf, "Selection without hover must not show the grid.");
            PlayerGraphicsSettings.Reload();
            Require(PlayerGraphicsSettings.Detail == PixelDetail.High && PixelStrengthOnboarding.Completed,
                "Settings reload must preserve High without clearing setup confirmation.");
            menu.View.Settings.Hide();
            menu.View.Show(MainMenuPage.Hidden, false);
            host.Screen.SetMainMenuCovered(false);
            host.StartCampaign();
            Require(Path.GetDirectoryName(Path.GetFullPath(host.SavePath)) == isolatedSaveDirectory,
                "The formal campaign must use the probe's isolated save directory.");
            Require(host.Runtime.SetPaused(true).Accepted, "The formal runtime rejected pause.");
            yield return null;
            yield return null;
            yield return new WaitForEndOfFrame();
            var pause = host.Screen.GetComponentInChildren<PauseMenuView>(true);
            Require(pause != null, "The formal scene pause panel is missing.");
            var settingsButton = pause.Buttons.Single(item => item.transform.Find("Label").GetComponent<Text>().text == "游戏设置");
            Click(settingsButton);
            Require(pause.SettingsVisible, "Pause settings did not open through its real button.");
            yield return null;
            yield return new WaitForEndOfFrame();
            for (int i = 0; i < details.Length; i++)
            {
                VerifyHover(pause.SharedSettings, i);
                yield return null;
                yield return new WaitForEndOfFrame();
                Capture("pause-grid-" + details[i].ToString().ToLowerInvariant(), pause.SharedSettings.PixelLegend);
                VerifyExitAndFocus(pause.SharedSettings, i);
            }
            report.pauseTooltips = true;
            yield return new WaitForEndOfFrame();
            Capture("pause-no-hover");
            Require(host.Runtime.GetView().Paused, "Tooltip inspection must not resume the campaign.");
        }

        private static void VerifyHover(PlayerSettingsView settings, int index)
        {
            EventSystem.current.SetSelectedGameObject(settings.BrightnessSlider.gameObject);
            var before = PlayerGraphicsSettings.Detail;
            var button = settings.PixelDetailButtons[index];
            var pointer = Hit(button);
            ExecuteEvents.Execute(button.gameObject, pointer, ExecuteEvents.pointerEnterHandler);
            int expected = index == 0 ? 2 : index == 1 ? 4 : 8;
            Require(settings.PixelLegend.gameObject.activeSelf && settings.PixelLegend.GridCount == expected
                && settings.PixelLegend.GridRect.rect.size == new Vector2(128, 128), "Hover grid must have the expected cells and common 128x128 extent.");
            Require(PlayerGraphicsSettings.Detail == before, "Hover must not change the active pixel setting.");
            Require(settings.PixelLegend.GetComponentsInChildren<Graphic>(true).All(item => !item.raycastTarget),
                "Tooltip graphics must not intercept any pointer input.");
            Hit(button);
        }

        private static void VerifyExitAndFocus(PlayerSettingsView settings, int index)
        {
            var button = settings.PixelDetailButtons[index];
            ExecuteEvents.Execute(button.gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.pointerExitHandler);
            Require(!settings.PixelLegend.gameObject.activeSelf, "Pointer exit must hide the grid.");
            EventSystem.current.SetSelectedGameObject(settings.BrightnessSlider.gameObject);
            EventSystem.current.SetSelectedGameObject(button.gameObject);
            Require(!settings.PixelLegend.gameObject.activeSelf, "Keyboard focus without hover must not show the grid.");
            EventSystem.current.SetSelectedGameObject(settings.BrightnessSlider.gameObject);
            Require(!settings.PixelLegend.gameObject.activeSelf, "Moving keyboard focus away must hide the grid.");
        }

        private static PointerEventData Hit(Button button)
        {
            Require(button != null && button.isActiveAndEnabled && button.IsInteractable(), "The requested UI button is not interactive.");
            Canvas.ForceUpdateCanvases();
            var rect = (RectTransform)button.transform;
            var canvas = button.GetComponentInParent<Canvas>();
            var camera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
            var pointer = new PointerEventData(EventSystem.current)
            {
                button = PointerEventData.InputButton.Left,
                position = RectTransformUtility.WorldToScreenPoint(camera, rect.TransformPoint(rect.rect.center))
            };
            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointer, results);
            Require(results.Count > 0 && ExecuteEvents.GetEventHandler<IPointerClickHandler>(results[0].gameObject) == button.gameObject,
                "The actual screen-coordinate raycast did not hit " + button.name + ".");
            pointer.pointerCurrentRaycast = results[0];
            pointer.pointerPressRaycast = results[0];
            return pointer;
        }

        private static void Click(Button button)
        {
            var pointer = Hit(button);
            ExecuteEvents.Execute(button.gameObject, pointer, ExecuteEvents.pointerDownHandler);
            ExecuteEvents.Execute(button.gameObject, pointer, ExecuteEvents.pointerUpHandler);
            ExecuteEvents.Execute(button.gameObject, pointer, ExecuteEvents.pointerClickHandler);
        }

        private void Capture(string name, PixelStrengthTooltip legend = null)
        {
            int width = Screen.width, height = Screen.height;
            Require(width > 0 && height > 0, "The actual rendered screen has no extent.");
            var texture = ScreenCapture.CaptureScreenshotAsTexture();
            Require(texture != null && texture.width == width && texture.height == height,
                "Full-frame capture did not preserve the rendered screen extent.");
            try
            {
                Require(texture.GetPixels32().Any(pixel => pixel.r > 32 || pixel.g > 32 || pixel.b > 32),
                    "Captured frame is black; a visible rendered Player window is required for visual acceptance.");
                string path = Path.Combine(outputFolder, name + ".png");
                File.WriteAllBytes(path, texture.EncodeToPNG());
                report.captures.Add(new CaptureRecord
                {
                    file = path, width = width, height = height, frame = Time.frameCount,
                    page = menu.View.Page.ToString(), selection = menu.View.PixelSetup.SelectedDetail.ToString(),
                    gridCount = legend != null ? legend.GridCount : 0
                });
            }
            finally
            {
                Destroy(texture);
            }
        }

        private void Finish(bool passed, string error)
        {
            if (finished) return;
            finished = true;
            RestoreInputModules();
            Application.runInBackground = originalRunInBackground;
            exitCode = passed ? 0 : 1;
            RestorePreferences();
            report.preferencesRestored = preferencesRestored;
            report.passed = passed && preferencesRestored;
            if (!report.passed) exitCode = 1;
            report.error = error;
            Directory.CreateDirectory(outputFolder);
            File.WriteAllText(Path.Combine(outputFolder, "report.json"), JsonUtility.ToJson(report, true));
            Debug.Log("MINIGAME_PIXEL_STRENGTH_" + (report.passed ? "PASSED " : "FAILED ") + JsonUtility.ToJson(report));
            if (!Application.isEditor) Application.Quit(exitCode);
        }

        private static void RestorePreferences()
        {
            if (!preferencesBackedUp || preferencesRestored) return;
            if (preferences.graphicsHadKey) PlayerPrefs.SetInt(PlayerGraphicsSettings.PreferenceKey, preferences.graphicsValue);
            else PlayerPrefs.DeleteKey(PlayerGraphicsSettings.PreferenceKey);
            if (preferences.onboardingHadKey) PlayerPrefs.SetInt(PixelStrengthOnboarding.PreferenceKey, preferences.onboardingValue);
            else PlayerPrefs.DeleteKey(PixelStrengthOnboarding.PreferenceKey);
            PlayerPrefs.Save();
            PlayerGraphicsSettings.Reload();
            preferencesRestored = true;
        }

        private void OnApplicationQuit() => RestorePreferences();
        private void RestoreInputModules()
        {
            for (int i = 0; i < inputModules.Length; i++)
                if (inputModules[i] != null) inputModules[i].enabled = inputModuleStates[i];
        }
        private void OnDestroy()
        {
            RestoreInputModules();
            RestorePreferences();
            Application.runInBackground = originalRunInBackground;
            if (MainMenuController.SaveDirectoryOverride == isolatedSaveDirectory)
                MainMenuController.SaveDirectoryOverride = originalSaveOverride;
            MainMenuController.SkipStartup = originalSkipStartup;
            isolatedSaveDirectory = null;
        }
        private static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }
    }
}
#endif
