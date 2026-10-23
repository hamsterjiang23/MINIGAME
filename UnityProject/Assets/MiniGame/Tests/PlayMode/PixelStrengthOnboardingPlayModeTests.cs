using System;
using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace MiniGame.UnityHost.Tests
{
    [Category("PixelStrength")]
    public sealed class PixelStrengthOnboardingPolicyTests
    {
        private PixelStrengthPreferenceScope preferences;
        private string directory;

        [SetUp]
        public void SetUp()
        {
            preferences = new PixelStrengthPreferenceScope();
            directory = Path.Combine(Path.GetTempPath(), "minigame-pixel-policy-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
        }

        [TearDown]
        public void TearDown()
        {
            preferences.Dispose();
            foreach (var file in Directory.GetFiles(directory)) File.Delete(file);
            Directory.Delete(directory, false);
        }

        [Test]
        public void EmptyInstallNeedsConfirmationAndOrdinarySettingsDoNotCompleteIt()
        {
            Assert.That(PixelStrengthOnboarding.Completed, Is.False);
            Assert.That(PixelStrengthOnboarding.ShouldShow(directory), Is.True);
            PlayerGraphicsSettings.SetDetail(PixelDetail.Low);
            PlayerGraphicsSettings.Reload();
            Assert.That(PixelStrengthOnboarding.Completed, Is.False);
            Assert.That(PixelStrengthOnboarding.ShouldShow(directory), Is.True);
            Assert.That(Directory.GetFiles(directory), Is.Empty);
        }

        [TestCase("campaign-continue.json")]
        [TestCase("campaign-dream-save.json")]
        [TestCase("campaign-save.json")]
        [TestCase("campaign-completed.json")]
        public void EveryFormalProgressFileSkipsTheChoiceWithoutModifyingIt(string name)
        {
            var path = Path.Combine(directory, name);
            const string contents = "existing progress remains authoritative even when invalid";
            File.WriteAllText(path, contents);
            Assert.That(PixelStrengthOnboarding.ShouldShow(directory), Is.False);
            Assert.That(PixelStrengthOnboarding.Completed, Is.False, "Detection is not a confirmation.");
            Assert.That(File.ReadAllText(path), Is.EqualTo(contents));
        }

        [Test]
        public void UnrelatedFilesDoNotSkipAndConfirmedChoicePersistsWithoutAnyStorySave()
        {
            File.WriteAllText(Path.Combine(directory, "unrelated.json"), "{}");
            Assert.That(PixelStrengthOnboarding.ShouldShow(directory), Is.True);
            PixelStrengthOnboarding.Confirm(PixelDetail.High);
            PlayerGraphicsSettings.Reload();
            Assert.That(PixelStrengthOnboarding.Completed, Is.True);
            Assert.That(PlayerPrefs.GetInt(PixelStrengthOnboarding.PreferenceKey), Is.EqualTo(1));
            Assert.That(PlayerGraphicsSettings.Detail, Is.EqualTo(PixelDetail.High));
            Assert.That(MainMenuContinueStore.ResolveResumePath(directory), Is.Null);
            Assert.That(PixelStrengthOnboarding.ShouldShow(directory), Is.False);
            PlayerGraphicsSettings.SetDetail(PixelDetail.Low);
            Assert.That(PixelStrengthOnboarding.ShouldShow(directory), Is.False,
                "Subsequent settings changes must not reopen the first-launch choice.");
        }
    }

    [Category("PixelStrength")]
    public sealed class PixelStrengthOnboardingPlayModeTests
    {
        private PixelStrengthPreferenceScope preferences;
        private string directory, previousDirectory;
        private bool previousSkipStartup;
        private MiniGameBootstrap host;
        private MainMenuController Menu => MainMenuController.Current;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            preferences = new PixelStrengthPreferenceScope();
            previousDirectory = MainMenuController.SaveDirectoryOverride;
            previousSkipStartup = MainMenuController.SkipStartup;
            directory = Path.Combine(Path.GetTempPath(), "minigame-pixel-menu-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            MainMenuController.SaveDirectoryOverride = directory;
            MainMenuController.SkipStartup = false;
            yield return Reboot();
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            StopAdapters();
            if (Menu != null) Object.Destroy(Menu.gameObject);
            MiniGameSessionHost.Release();
            yield return null;
            var oldScene = SceneManager.GetActiveScene();
            var empty = SceneManager.CreateScene("Pixel choice cleanup " + Guid.NewGuid().ToString("N"));
            SceneManager.SetActiveScene(empty);
            if (oldScene.IsValid() && oldScene.isLoaded && oldScene.name == MiniGameSessionHost.CombinedScene)
                yield return SceneManager.UnloadSceneAsync(oldScene);
            MainMenuController.SaveDirectoryOverride = previousDirectory;
            MainMenuController.SkipStartup = previousSkipStartup;
            preferences.Dispose();
            foreach (var file in Directory.GetFiles(directory)) File.Delete(file);
            Directory.Delete(directory, false);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        [UnityTest, Timeout(60000)]
        public IEnumerator NewInstallPreviewsWithoutStartingOrSavingThenConfirmsOnlyToTitle()
        {
            Assert.That(Menu.View.Page, Is.EqualTo(MainMenuPage.PixelSetup));
            Assert.That(host.Runtime, Is.Null);
            Assert.That(MiniGameSessionHost.Current, Is.Null);
            Assert.That(MainMenuController.BlocksGameplayInput, Is.True);
            var choice = Menu.View.PixelSetup;
            Assert.That(choice.SelectedDetail, Is.EqualTo(PixelDetail.Medium));
            Assert.That(choice.ChoiceButtons.Length, Is.EqualTo(3));
            Assert.That(choice.PreviewImages.Length, Is.EqualTo(3));
            Assert.That(choice.PreviewImages.All(image => image.texture != null), Is.True);
            Assert.That(choice.PreviewImages.Select(image => image.texture).Distinct().Count(), Is.EqualTo(3));
            Assert.That(choice.PreviewImages.Select(image => new Vector2Int(image.texture.width, image.texture.height)).Distinct().Count(), Is.EqualTo(1));
            Assert.That(Menu.HandlePixelSetupKey(KeyCode.LeftArrow), Is.True);
            Assert.That(choice.SelectedDetail, Is.EqualTo(PixelDetail.High));
            Assert.That(Menu.HandlePixelSetupKey(KeyCode.RightArrow), Is.True);
            Assert.That(choice.SelectedDetail, Is.EqualTo(PixelDetail.Medium));
            Click(choice.ChoiceButtons[2]);
            Assert.That(choice.SelectedDetail, Is.EqualTo(PixelDetail.Low));
            Assert.That(PlayerGraphicsSettings.Detail, Is.EqualTo(PixelDetail.Medium));
            Assert.That(PlayerPrefs.HasKey(PlayerGraphicsSettings.PreferenceKey), Is.False);
            Assert.That(PixelStrengthOnboarding.Completed, Is.False);
            Assert.That(Menu.HandlePixelSetupKey(KeyCode.Escape), Is.True);
            Assert.That(Menu.View.Page, Is.EqualTo(MainMenuPage.PixelSetup));
            Assert.That(Directory.GetFiles(directory), Is.Empty);

            // uGUI Submit must not duplicate the controller's keyboard confirmation.
            ExecuteEvents.Execute(choice.ConfirmButton.gameObject, new BaseEventData(EventSystem.current), ExecuteEvents.submitHandler);
            Assert.That(Menu.View.Page, Is.EqualTo(MainMenuPage.PixelSetup));
            Assert.That(Menu.HandlePixelSetupKey(KeyCode.Return), Is.True);
            Assert.That(Menu.View.Page, Is.EqualTo(MainMenuPage.FirstStart));
            Assert.That(PlayerGraphicsSettings.Detail, Is.EqualTo(PixelDetail.Low));
            Assert.That(PixelStrengthOnboarding.Completed, Is.True);
            Assert.That(host.Runtime, Is.Null, "Confirming the graphics choice must not begin a campaign.");
            Menu.ConfirmPixelSetup();
            Menu.StartFirstInspection();
            Assert.That(host.Runtime, Is.Null, "The confirmation frame cannot also activate the title.");
            yield return null;
            Assert.That(host.Runtime, Is.Null);
            Assert.That(Directory.GetFiles(directory), Is.Empty);
            yield return Reboot();
            Assert.That(Menu.View.Page, Is.EqualTo(MainMenuPage.FirstStart));
            Assert.That(PlayerGraphicsSettings.Detail, Is.EqualTo(PixelDetail.Low));
        }

        [UnityTest, Timeout(60000)]
        public IEnumerator UnconfirmedRebootRepeatsTheChoiceAndPreservesAnExistingLowPreference()
        {
            PlayerGraphicsSettings.SetDetail(PixelDetail.Low);
            yield return Reboot();
            Assert.That(Menu.View.Page, Is.EqualTo(MainMenuPage.PixelSetup));
            Assert.That(Menu.View.PixelSetup.SelectedDetail, Is.EqualTo(PixelDetail.Low));
            Click(Menu.View.PixelSetup.ChoiceButtons[0]);
            Assert.That(Menu.View.PixelSetup.SelectedDetail, Is.EqualTo(PixelDetail.High));
            yield return Reboot();
            Assert.That(Menu.View.Page, Is.EqualTo(MainMenuPage.PixelSetup));
            Assert.That(Menu.View.PixelSetup.SelectedDetail, Is.EqualTo(PixelDetail.Low));
            Assert.That(PlayerGraphicsSettings.Detail, Is.EqualTo(PixelDetail.Low));
            Assert.That(PixelStrengthOnboarding.Completed, Is.False);
        }

        [UnityTest, Timeout(60000)]
        public IEnumerator ExistingProgressAndCompletedInspectionOpenTheMenuAndItsSharedLegend()
        {
            string saved = Path.Combine(directory, "campaign-save.json");
            File.WriteAllText(saved, "existing save; opening the menu does not load it");
            yield return Reboot();
            Assert.That(Menu.View.Page, Is.EqualTo(MainMenuPage.Menu));
            Assert.That(host.Runtime, Is.Null);
            Click(Menu.View.SettingsButton);
            Assert.That(Menu.View.Page, Is.EqualTo(MainMenuPage.Settings));
            PixelStrengthSettingsAssertions.AssertLegendInteractions(Menu.View.Settings);
            Menu.HandleEscape();
            Assert.That(Menu.View.Page, Is.EqualTo(MainMenuPage.Menu));
            File.Delete(saved);
            MainMenuContinueStore.MarkCompleted(directory);
            yield return Reboot();
            Assert.That(Menu.View.Page, Is.EqualTo(MainMenuPage.Menu));
            Assert.That(Menu.View.ContinueButton.interactable, Is.False);
            Assert.That(PixelStrengthOnboarding.Completed, Is.False);
        }

        [UnityTest, Timeout(90000)]
        public IEnumerator ReturningFromAnActiveGameAndRequestingNewPeriodNeverOpenSetup()
        {
            Click(Menu.View.PixelSetup.ConfirmButton);
            yield return null;
            Click(Menu.View.StartButton);
            float deadline = Time.realtimeSinceStartup + 35;
            while (MainMenuController.BlocksGameplayInput && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.That(MainMenuController.BlocksGameplayInput, Is.False);
            Assert.That(host.Runtime, Is.Not.Null);
            PlayerPrefs.DeleteKey(PixelStrengthOnboarding.PreferenceKey);
            Menu.ReturnToMenu();
            Assert.That(Menu.View.Page, Is.EqualTo(MainMenuPage.Menu));
            Assert.That(PixelStrengthOnboarding.Completed, Is.False);
            Menu.RequestNewInspection();
            Assert.That(Menu.View.Page, Is.EqualTo(MainMenuPage.ConfirmNew));
            Menu.CancelNewInspection();
            Assert.That(Menu.View.Page, Is.EqualTo(MainMenuPage.Menu));
        }

        private IEnumerator Reboot()
        {
            StopAdapters();
            if (Menu != null) Object.Destroy(Menu.gameObject);
            MiniGameSessionHost.Release();
            yield return null;
            yield return SceneManager.LoadSceneAsync(MiniGameSessionHost.CombinedScene, LoadSceneMode.Single);
            yield return null;
            host = Object.FindObjectOfType<MiniGameBootstrap>();
            Assert.That(host, Is.Not.Null);
            Assert.That(Menu, Is.Not.Null);
            host.Player.enabled = false;
            host.Screen.DialogueCharactersPerSecond = 0;
        }

        private void StopAdapters()
        {
            var current = host != null ? host : Object.FindObjectOfType<MiniGameBootstrap>();
            if (current == null) return;
            current.enabled = false;
            if (current.Player != null) current.Player.enabled = false;
            current.Dreams?.PrepareForRuntimeChange();
            current.Effects?.PrepareForRuntimeChange();
        }

        private static void Click(Button button) => ExecuteEvents.Execute(button.gameObject,
            new PointerEventData(EventSystem.current) { button = PointerEventData.InputButton.Left }, ExecuteEvents.pointerClickHandler);
    }

    internal static class PixelStrengthSettingsAssertions
    {
        public static void AssertLegendInteractions(PlayerSettingsView view)
        {
            Assert.That(view.GetComponentsInChildren<Text>().Single(text => text.name == "PixelDetailHeading").text,
                Is.EqualTo("像素风强度"));
            var events = EventSystem.current;
            var legend = view.PixelLegend;
            var original = PlayerGraphicsSettings.Detail;
            bool hadPreference = PlayerPrefs.HasKey(PlayerGraphicsSettings.PreferenceKey);
            int originalSaved = PlayerPrefs.GetInt(PlayerGraphicsSettings.PreferenceKey);
            events.SetSelectedGameObject(view.BrightnessSlider.gameObject);
            for (int i = 0; i < 3; i++)
            {
                var button = view.PixelDetailButtons[i];
                var pointer = new PointerEventData(events);
                ExecuteEvents.Execute(button.gameObject, pointer, ExecuteEvents.pointerEnterHandler);
                Assert.That(legend.gameObject.activeSelf, Is.True);
                Assert.That(legend.Owner, Is.SameAs(button.gameObject));
                int gridCount = new[] { 2, 4, 8 }[i];
                Assert.That(legend.GridCount, Is.EqualTo(gridCount));
                Assert.That(legend.GridRect.rect.size, Is.EqualTo(new Vector2(128, 128)));
                Assert.That(legend.GridLabel.text, Does.Contain(gridCount + "×" + gridCount));
                Assert.That(legend.GridRect.GetComponent<Image>().color, Is.EqualTo((Color)new Color32(255, 207, 104, 255)));
                var lines = legend.GridRect.GetComponentsInChildren<Image>()
                    .Where(image => image.name.StartsWith("Vertical", StringComparison.Ordinal)
                        || image.name.StartsWith("Horizontal", StringComparison.Ordinal)).ToArray();
                Assert.That(lines.Length, Is.EqualTo(2 * (gridCount - 1)));
                Assert.That(lines.All(line => line.color == Color.black), Is.True);
                Assert.That(legend.GetComponentsInChildren<Graphic>(true).All(graphic => !graphic.raycastTarget), Is.True);
                Assert.That(legend.GetComponent<CanvasGroup>().blocksRaycasts, Is.False);
                Assert.That(PlayerGraphicsSettings.Detail, Is.EqualTo(original));
                ExecuteEvents.Execute(button.gameObject, pointer, ExecuteEvents.pointerExitHandler);
                Assert.That(legend.gameObject.activeSelf, Is.False, "Pointer exit hides the legend even if hover selected the option.");
                events.SetSelectedGameObject(view.BrightnessSlider.gameObject);
                events.SetSelectedGameObject(button.gameObject);
                Assert.That(legend.gameObject.activeSelf, Is.False,
                    "Keyboard focus alone must not open a mouse-hover legend.");
                events.SetSelectedGameObject(view.BrightnessSlider.gameObject);
                Assert.That(legend.gameObject.activeSelf, Is.False);
            }
            Assert.That(PlayerGraphicsSettings.Detail, Is.EqualTo(original));
            Assert.That(PlayerPrefs.HasKey(PlayerGraphicsSettings.PreferenceKey), Is.EqualTo(hadPreference));
            Assert.That(PlayerPrefs.GetInt(PlayerGraphicsSettings.PreferenceKey), Is.EqualTo(originalSaved));
        }
    }

    internal sealed class PixelStrengthPreferenceScope : IDisposable
    {
        private readonly bool hadDetail = PlayerPrefs.HasKey(PlayerGraphicsSettings.PreferenceKey);
        private readonly int originalDetail = PlayerPrefs.GetInt(PlayerGraphicsSettings.PreferenceKey);
        private readonly bool hadCompleted = PlayerPrefs.HasKey(PixelStrengthOnboarding.PreferenceKey);
        private readonly int originalCompleted = PlayerPrefs.GetInt(PixelStrengthOnboarding.PreferenceKey);

        public PixelStrengthPreferenceScope()
        {
            PlayerPrefs.DeleteKey(PlayerGraphicsSettings.PreferenceKey);
            PlayerPrefs.DeleteKey(PixelStrengthOnboarding.PreferenceKey);
            PlayerGraphicsSettings.Reload();
        }

        public void Dispose()
        {
            Restore(PlayerGraphicsSettings.PreferenceKey, hadDetail, originalDetail);
            Restore(PixelStrengthOnboarding.PreferenceKey, hadCompleted, originalCompleted);
            PlayerPrefs.Save();
            PlayerGraphicsSettings.Reload();
        }

        private static void Restore(string key, bool existed, int value)
        {
            if (existed) PlayerPrefs.SetInt(key, value);
            else PlayerPrefs.DeleteKey(key);
        }
    }
}
