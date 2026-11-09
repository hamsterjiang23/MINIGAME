using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using MiniGame.Core;
using MiniGame.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace MiniGame.UnityHost.Tests
{
    public sealed class MainMenuFlowPlayModeTests
    {
        private MiniGameBootstrap host;
        private string directory, previousDirectory;
        private bool previousSkipStartup, hadBrightness, hadVolume, hadDetail, hadPixelSetup;
        private float previousBrightness, previousVolume;
        private int previousDetail, previousPixelSetup;
        private TransitionTrace transitionTrace;
        private bool observeTransition;
        private static int captureRequestedFrame = -1;
        private MainMenuController Menu => MainMenuController.Current;
        private string ManualPath => Path.Combine(directory, "campaign-dream-save.json");
        private string AutomaticPath => Path.Combine(directory, "campaign-continue.json");

        [UnitySetUp]
        public IEnumerator LoadTitle()
        {
            previousSkipStartup = MainMenuController.SkipStartup;
            previousDirectory = MainMenuController.SaveDirectoryOverride;
            hadBrightness = PlayerPrefs.HasKey(PlayerPreferences.BrightnessKey);
            hadVolume = PlayerPrefs.HasKey(PlayerPreferences.MasterVolumeKey);
            hadDetail = PlayerPrefs.HasKey(PlayerGraphicsSettings.PreferenceKey);
            hadPixelSetup = PlayerPrefs.HasKey(PixelStrengthOnboarding.PreferenceKey);
            previousPixelSetup = PlayerPrefs.GetInt(PixelStrengthOnboarding.PreferenceKey);
            // These regressions begin after the independent first-launch graphics choice.
            PlayerPrefs.SetInt(PixelStrengthOnboarding.PreferenceKey, 1);
            previousBrightness = PlayerPrefs.GetFloat(PlayerPreferences.BrightnessKey, 1);
            previousVolume = PlayerPrefs.GetFloat(PlayerPreferences.MasterVolumeKey, 1);
            previousDetail = PlayerPrefs.GetInt(PlayerGraphicsSettings.PreferenceKey, 2);
            directory = Path.Combine(Path.GetTempPath(), "minigame-menu-flow-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            MainMenuController.SkipStartup = false;
            MainMenuController.SaveDirectoryOverride = directory;
            yield return RebootToTitle();
        }

        [UnityTearDown]
        public IEnumerator Cleanup()
        {
            observeTransition = false;
            WriteTransitionTraceIfRequested();
            StopSceneAdapters();
            if (Menu != null) Object.Destroy(Menu.gameObject);
            MiniGameSessionHost.Release();
            yield return null;
            var oldScene = SceneManager.GetActiveScene();
            var empty = SceneManager.CreateScene("Main menu cleanup " + Guid.NewGuid().ToString("N"));
            SceneManager.SetActiveScene(empty);
            if (oldScene.IsValid() && oldScene.isLoaded && oldScene.name == MiniGameSessionHost.CombinedScene)
                yield return SceneManager.UnloadSceneAsync(oldScene);
            MainMenuController.SkipStartup = previousSkipStartup;
            MainMenuController.SaveDirectoryOverride = previousDirectory;
            RestoreFloat(PlayerPreferences.BrightnessKey, hadBrightness, previousBrightness);
            RestoreFloat(PlayerPreferences.MasterVolumeKey, hadVolume, previousVolume);
            if (hadDetail) PlayerPrefs.SetInt(PlayerGraphicsSettings.PreferenceKey, previousDetail);
            else PlayerPrefs.DeleteKey(PlayerGraphicsSettings.PreferenceKey);
            if (hadPixelSetup) PlayerPrefs.SetInt(PixelStrengthOnboarding.PreferenceKey, previousPixelSetup);
            else PlayerPrefs.DeleteKey(PixelStrengthOnboarding.PreferenceKey);
            PlayerPrefs.Save();
            PlayerPreferences.Reload();
            PlayerGraphicsSettings.Reload();
            if (directory != null && Directory.Exists(directory))
            {
                Assert.That(Path.GetFileName(directory), Does.StartWith("minigame-menu-flow-"));
                Assert.That(Path.GetFullPath(directory).StartsWith(Path.GetFullPath(Path.GetTempPath()).TrimEnd(Path.DirectorySeparatorChar)
                    + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase), Is.True);
                // This fixture only creates these top-level files; never recursively delete shared directories.
                foreach (string file in Directory.GetFiles(directory)) File.Delete(file);
                Directory.Delete(directory);
            }
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            yield return null;
        }

        [UnityTest, Timeout(90000)]
        public IEnumerator AutosaveSlotsAreIndependentAndContinueLoadsManualAfterCompletion()
        {
            yield return StartGame();
            host.Save();
            string manual = File.ReadAllText(ManualPath);
            RestoreDream();
            MiniGameSessionHost.Current.SaveAutomatic();
            string automatic = File.ReadAllText(AutomaticPath);
            Assert.That(automatic, Is.Not.EqualTo(manual));
            Assert.That(File.ReadAllText(ManualPath), Is.EqualTo(manual));
            var previous = host.Runtime;
            Menu.CompleteInspectionAndReturnToMenu();
            Assert.That(Menu.View.ContinueButton.interactable, Is.True);
            Assert.That(File.ReadAllText(AutomaticPath), Is.EqualTo(automatic));
            Menu.Continue();
            yield return WaitForGame();
            Assert.That(host.Runtime, Is.Not.SameAs(previous));
            Assert.That(host.Runtime.GetView().Game.Day, Is.Zero, "Manual checkpoint wins over the newer automatic dream.");
            Menu.ReturnToMenu();
            Assert.That(Menu.HasRetainedGame, Is.True, "The historical completion marker must not lock a restored session.");
        }

        [UnityTest, Timeout(90000)]
        public IEnumerator AutosaveLoadPanelShowsTwoSlotsAndReturnsToPause()
        {
            yield return StartGame();
            host.Save();
            string manual = File.ReadAllText(ManualPath);
            string automatic = File.ReadAllText(AutomaticPath);
            host.Load();
            var panel = host.Screen.GetComponentInChildren<PauseMenuView>(true);
            Assert.That(panel.LoadSlotsVisible, Is.True);
            Assert.That(panel.LoadSlotButtons.Length, Is.EqualTo(3));
            Assert.That(panel.LoadSlotButtons[0].interactable && panel.LoadSlotButtons[1].interactable, Is.True);
            Assert.That(host.Runtime.GetView().Paused, Is.True);
            yield return new WaitForSecondsRealtime(.3f);
            yield return CaptureIfRequested("autosave-load-slots", host.Screen.Canvas);
            yield return null;
            var previous = host.Runtime;
            panel.LoadSlotButtons[1].onClick.Invoke();
            Assert.That(host.Runtime, Is.Not.SameAs(previous));
            Assert.That(host.Runtime.GetView().Paused, Is.True);
            Assert.That(panel.LoadSlotsVisible, Is.False);
            Assert.That(File.ReadAllText(ManualPath), Is.EqualTo(manual));
            Assert.That(File.ReadAllText(AutomaticPath), Is.EqualTo(automatic));
            host.Load();
            yield return null;
            previous = host.Runtime;
            File.WriteAllText(AutomaticPath, "{broken after opening");
            panel.LoadSlotButtons[1].onClick.Invoke();
            Assert.That(host.Runtime, Is.SameAs(previous), "Failed selection preserves the live session.");
            Assert.That(panel.LoadSlotsVisible, Is.True);
            Assert.That(panel.GetComponentsInChildren<Text>(true).Single(text => text.name == "LoadStatus").text,
                Does.Contain("读取失败"));
            File.WriteAllText(AutomaticPath, automatic);
            host.HandleEscape();
            Assert.That(panel.LoadSlotsVisible, Is.False);
            Assert.That(host.Runtime.GetView().Paused, Is.True);
        }

        [UnityTest, Timeout(90000)]
        public IEnumerator AutosaveContinueFallsBackFromBrokenManualAndKeepsFiles()
        {
            yield return StartGame();
            string automatic = File.ReadAllText(AutomaticPath);
            File.WriteAllText(ManualPath, "{broken manual");
            Menu.ReturnToMenu();
            Assert.That(Menu.View.ContinueButton.interactable, Is.True);
            Menu.Continue();
            yield return WaitForGame();
            Assert.That(host.Runtime.GetView().Game.Day, Is.Zero);
            Assert.That(File.ReadAllText(ManualPath), Is.EqualTo("{broken manual"));
            Assert.That(File.ReadAllText(AutomaticPath), Is.EqualTo(automatic));
        }

        [UnityTest, Timeout(90000)]
        public IEnumerator AutosaveDoorKeepsPreInteractionStateAndDetachesOldRuntime()
        {
            yield return StartGame();
            var fixture = Fixture();
            DriveTo(fixture, view => view.Game.Day == 1 && view.Game.Phase == GamePhase.MorningInspection && view.Cue == null);
            host.RestoreCheckpoint(fixture.Save());
            var previous = host.Runtime;
            host.RestoreCheckpoint(fixture.Save());
            var runtime = host.Runtime;
            host.Save();
            string manual = File.ReadAllText(ManualPath);
            string oldAutomatic = File.ReadAllText(AutomaticPath);
            Assert.That(previous.TryInteract(InteractionTarget.Door).Accepted, Is.True);
            Assert.That(File.ReadAllText(AutomaticPath), Is.EqualTo(oldAutomatic), "Old runtime has no disk subscription.");
            string checkpoint = runtime.Save();
            Assert.That(runtime.TryInteract(InteractionTarget.Door).Accepted, Is.True);
            string automatic = File.ReadAllText(AutomaticPath);
            Assert.That(JsonUtility.FromJson<AutomaticEnvelope>(automatic).Runtime, Is.EqualTo(checkpoint));
            Assert.That(File.ReadAllText(ManualPath), Is.EqualTo(manual));
            Assert.That(runtime.TryInteract(InteractionTarget.Door).Accepted, Is.False);
            Assert.That(File.ReadAllText(AutomaticPath), Is.EqualTo(automatic));
            Assert.That(host.TryLoadCampaign(AutomaticPath, out var error), Is.True, error);
            Assert.That(host.Runtime.GetView().Game.Phase, Is.EqualTo(GamePhase.MorningInspection));
            Assert.That(host.Runtime.GetView().Cue, Is.Null);
            Assert.That(File.ReadAllText(AutomaticPath), Is.EqualTo(automatic), "Loading does not autosave.");
        }

        [Serializable] private sealed class AutomaticEnvelope { public string Runtime; }

        [UnityTest, Timeout(90000)]
        public IEnumerator PriestReleasePreservesJudgmentFileAcrossLaterInteractionsAndManualLoad()
        {
            yield return StartGame();
            var fixture = Fixture();
            DriveTo(fixture, view => view.Game.Day == 2 && view.Game.CurrentPatientId == "priest"
                && view.TerminalPage == TerminalPage.Review && view.Cue != null, true);
            host.RestoreCheckpoint(fixture.Save());
            DriveTo(host.Runtime, view => view.Game.EncounterStage == EncounterStage.AwaitingDisposition, true);
            var runtime = host.Runtime;
            string judgment = runtime.Save();
            string automatic = File.ReadAllText(AutomaticPath);
            Assert.That(JsonUtility.FromJson<AutomaticEnvelope>(automatic).Runtime, Is.EqualTo(judgment));
            Assert.That(runtime.SubmitDisposition(runtime.GetView().Game.CurrentEncounterId, Disposition.Release).Accepted, Is.True);
            Assert.That(runtime.IsAutomaticSaveLocked, Is.True);
            host.Save();
            string manual = File.ReadAllText(ManualPath);
            MiniGameSessionHost.Current.SaveAutomatic();
            Assert.That(File.ReadAllText(AutomaticPath), Is.EqualTo(automatic));
            DriveTo(runtime, view => view.Game.Phase == GamePhase.Ending, true);
            Assert.That(runtime.GetView().Game.Ending, Is.EqualTo(EndingType.PriestAttack));
            Assert.That(File.ReadAllText(AutomaticPath), Is.EqualTo(automatic));
            Assert.That(host.TryLoadCampaign(ManualPath, out var error), Is.True, error);
            Assert.That(host.Runtime.IsAutomaticSaveLocked, Is.True);
            MiniGameSessionHost.Current.SaveAutomatic();
            Assert.That(File.ReadAllText(AutomaticPath), Is.EqualTo(automatic));
            Assert.That(host.TryLoadCampaign(AutomaticPath, out error), Is.True, error);
            runtime = host.Runtime;
            runtime.SetPaused(false);
            Assert.That(runtime.IsAutomaticSaveLocked, Is.False);
            Assert.That(runtime.GetView().Game.EncounterStage, Is.EqualTo(EncounterStage.AwaitingDisposition));
            Assert.That(runtime.SubmitDisposition(runtime.GetView().Game.CurrentEncounterId, Disposition.Treat).Accepted, Is.True);
            DriveTo(runtime, view => view.Game.Phase == GamePhase.Evening && view.Cue == null, true);
            Assert.That(runtime.TryInteract(InteractionTarget.Bed).Accepted, Is.True);
            Assert.That(File.ReadAllText(AutomaticPath), Is.Not.EqualTo(automatic));
            Assert.That(File.ReadAllText(ManualPath), Is.EqualTo(manual));
        }

        [UnityTest, Timeout(90000)]
        public IEnumerator AutosaveEmptySlotsDisableLoadingAndKeepReturnNavigation()
        {
            yield return StartGame();
            File.Delete(AutomaticPath);
            host.Load();
            var panel = host.Screen.GetComponentInChildren<PauseMenuView>(true);
            Assert.That(panel.LoadSlotButtons[0].interactable, Is.False);
            Assert.That(panel.LoadSlotButtons[1].interactable, Is.False);
            Assert.That(EventSystem.current.currentSelectedGameObject, Is.EqualTo(panel.LoadSlotButtons[2].gameObject));
            Assert.That(panel.LoadSlotButtons[2].navigation.selectOnDown, Is.SameAs(panel.LoadSlotButtons[2]));
            host.HandleEscape();
            Menu.ReturnToMenu();
            Assert.That(Menu.View.ContinueButton.interactable, Is.False);
        }

        [UnityTest, Timeout(90000)]
        public IEnumerator AutosaveWithoutDreamFlowKeepsFailureNoticeAfterAcceptedDoor()
        {
            yield return StartGame();
            var profile = Object.Instantiate(host.DreamProfile);
            profile.EnableDreamFlow = false;
            try
            {
                host.ConfigureDreams(profile, Resources.Load<PlayerRebellionProfile>("MiniGame/PlayerRebellion"));
                host.StartCampaign();
                Assert.That(MiniGameSessionHost.Current, Is.Null);
                var compiled = EndingCampaignScenario.LoadPackage();
                var fixture = new GameRuntime(compiled.Rules, compiled.Content, true);
                DriveTo(fixture, view => view.Game.Day == 1 && view.Game.Phase == GamePhase.MorningInspection && view.Cue == null);
                host.RestoreCheckpoint(fixture.Save());
                string blockingFile = Path.Combine(directory, "blocked-auto-directory");
                File.WriteAllText(blockingFile, "keep");
                MainMenuController.SaveDirectoryOverride = blockingFile;
                Assert.That(host.Runtime.TryInteract(InteractionTarget.Door).Accepted, Is.True);
                host.Screen.ClearNotification(); // The successful world interaction clears immediate feedback.
                MainMenuController.SaveDirectoryOverride = directory;
                yield return null;
                yield return null;
                Assert.That(host.Screen.GetComponentsInChildren<Text>(true).Single(text => text.name == "Notification").text,
                    Does.Contain("自动保存失败"));
            }
            finally { MainMenuController.SaveDirectoryOverride = directory; Object.Destroy(profile); }
        }

        [UnityTest, Timeout(90000), Category("PixelStrength")]
        public IEnumerator FreshLaunchHasOnlyStartPromptAndStartsOneDayZeroPeriod()
        {
            Assert.That(host.Runtime, Is.Null, "The title must not start a hidden Day0 session.");
            Assert.That(MiniGameSessionHost.Current, Is.Null);
            Assert.That(Menu.View.Page, Is.EqualTo(MainMenuPage.FirstStart));
            Assert.That(VisibleButtonLabels(), Is.EqualTo(new[] { "按任意键开始游戏" }));
            Assert.That(Menu.View.ContinueButton.gameObject.activeInHierarchy, Is.False);
            Assert.That(Menu.View.SettingsButton.gameObject.activeInHierarchy, Is.False);
            Assert.That(MainMenuController.BlocksGameplayInput, Is.True);
            AssertSharedTypography();
            var menuAudio = Menu.GetComponentInChildren<MiniGameAudioService>();
            Assert.That(menuAudio.GetSource(MiniGameAudioChannel.MenuMusic).isPlaying, Is.True);
            int logoSounds = 0;
            menuAudio.SoundPlayed += (channel, sound) => { if (sound == MiniGameSound.LogoGlitch) logoSounds++; };
            Menu.View.SignalTuning.Profile.Logo.BurstFrequency = 5;
            yield return Until(() => logoSounds > 0, "title logo glitch sound", 3);
            yield return CaptureIfRequested("first-start");
            var sameMenu = Menu;
            Menu.View.StartButton.onClick.Invoke();
            Menu.View.StartButton.onClick.Invoke();
            Assert.That(MainMenuController.BlocksGameplayInput, Is.True, "The start frame remains consumed by the menu.");
            yield return WaitForGame();
            Assert.That(Menu, Is.SameAs(sameMenu));
            Assert.That(host.Runtime.GetView().Game.Day, Is.Zero);
            Assert.That(host.Runtime.GetView().Paused, Is.False);
            Assert.That(menuAudio.GetSource(MiniGameAudioChannel.MenuMusic).isPlaying, Is.False,
                "The title track stops before Day0 starts.");
            Assert.That(host.Audio.Service.GetSource(MiniGameAudioChannel.MenuMusic).isPlaying, Is.False,
                "Day0 must not start a second copy of the title track.");
            Assert.That(host.Audio.Service.GetSource(MiniGameAudioChannel.GameplayMusic).isPlaying, Is.True);
            Assert.That(host.Screen.DayZeroOpeningWaiting, Is.False, "The title action also owns the monitor pullback.");
            Assert.That(host.Screen.TerminalDisplay.OpeningProgress, Is.EqualTo(1));
            Assert.That(host.Runtime.GetView().Cue?.Kind, Is.EqualTo(PresentationKind.Introduction));
            yield return WaitForTutorialIntroduction();
            Assert.That(host.Runtime.GetView().Transcript.Count(line => !line.IsPlayer), Is.EqualTo(1));
            Assert.That(File.Exists(AutomaticPath), Is.True, "A new period has an independently persisted continuation seed.");
            Assert.That(File.Exists(ManualPath), Is.False);
            Assert.That(Menu.View.StartButton.gameObject.activeInHierarchy, Is.False);
            Assert.That(EventSystem.current.currentSelectedGameObject, Is.Null,
                "The startup selection cannot submit to the game after the menu releases input.");
        }

        [UnityTest, Timeout(90000)]
        public IEnumerator EscapePauseMenuReturnsToTheSameTutorialLineAfterMonitorPullback()
        {
            yield return StartGame();
            yield return WaitForTutorialIntroduction();
            var runtime = host.Runtime;
            string node = runtime.GetView().Game.CurrentNodeId;
            int transcriptCount = runtime.GetView().Transcript.Count;
            var camera = host.Player.ViewCamera;
            var position = camera.transform.position;
            var rotation = camera.transform.rotation;
            float progress = host.Screen.TerminalDisplay.OpeningProgress;
            Assert.That(runtime.GetView().Transcript.Any(line => line.IsPlayer), Is.False);

            host.HandleEscape();
            Assert.That(runtime.GetView().Paused, Is.True);
            var pause = host.Screen.GetComponentInChildren<PauseMenuView>(true);
            Assert.That(pause.gameObject.activeInHierarchy, Is.True);
            AssertSharedTypography();
            yield return new WaitForSecondsRealtime(.6f);
            CaptureTransitionFrameIfRequested("pause-menu-font");
            yield return null;
            var returnButton = pause.Buttons.Single(button => button.GetComponentsInChildren<Text>(true)
                .Any(text => text.text == "回到主菜单"));
            Assert.That(returnButton, Is.InstanceOf<MiniGameAudioButton>());
            Assert.That(((MiniGameAudioButton)returnButton).ActivateWithAudio(), Is.True);
            Assert.That(Menu.View.Page, Is.EqualTo(MainMenuPage.Menu));
            Assert.That(host.Screen.RequestDayZeroOpening(), Is.False, "A covered opening must reject the menu click.");
            Menu.View.SettingsButton.onClick.Invoke();
            Assert.That(Menu.View.Page, Is.EqualTo(MainMenuPage.Settings));
            AssertSharedTypography();
            host.HandleEscape();
            Assert.That(Menu.View.Page, Is.EqualTo(MainMenuPage.Menu));
            Assert.That(runtime.GetView().Paused, Is.True, "Esc exits settings without resuming Day0.");
            yield return new WaitForSecondsRealtime(.2f);
            Assert.That(host.Runtime, Is.SameAs(runtime));
            Assert.That(runtime.GetView().Game.CurrentNodeId, Is.EqualTo(node));
            Assert.That(host.Screen.DayZeroOpeningWaiting, Is.False);
            Assert.That(host.Screen.TerminalDisplay.OpeningProgress, Is.EqualTo(progress));
            Assert.That(Vector3.Distance(camera.transform.position, position), Is.LessThan(.001f));
            Assert.That(Quaternion.Angle(camera.transform.rotation, rotation), Is.LessThan(.01f));

            host.Save();
            Menu.View.ContinueButton.onClick.Invoke();
            Assert.That(host.Screen.RequestDayZeroOpening(), Is.False, "Continue's frame is still owned by the menu.");
            yield return WaitForGame();
            Assert.That(host.Runtime, Is.Not.SameAs(runtime));
            runtime = host.Runtime;
            Assert.That(host.Screen.DayZeroOpeningWaiting, Is.False);
            Assert.That(runtime.GetView().Transcript.Count, Is.EqualTo(transcriptCount));
            Assert.That(runtime.GetView().Game.CurrentNodeId, Is.EqualTo(node));
            Assert.That(Vector3.Distance(camera.transform.position, position), Is.LessThan(.001f));
            Assert.That(Quaternion.Angle(camera.transform.rotation, rotation), Is.LessThan(.01f));
            yield return null;
            Assert.That(host.Screen.RequestDayZeroOpening(), Is.False, "Returning from the menu cannot replay the opening.");
        }

        [UnityTest, Timeout(90000)]
        public IEnumerator TitleControlsAndMusicFadeBeforeTheMonitorContinuouslyPullsBack()
        {
            var audio = Menu.GetComponentInChildren<MiniGameAudioService>().GetSource(MiniGameAudioChannel.MenuMusic);
            yield return new WaitForSecondsRealtime(.4f);
            Assert.That(host.Runtime, Is.Null, "Title prewarming must not install its prepared seed as an active runtime.");
            Assert.That(MiniGameSessionHost.Current, Is.Null);
            Assert.That(Directory.GetFiles(directory), Is.Empty, "Prewarming may prepare a checkpoint in memory but cannot write it before Start.");
            var lightingPanel = host.Screen.LightingTuning;
            var lightingRows = LightingRows();
            var selectedLight = lightingPanel.SelectedLightPath;
            Assert.That(selectedLight, Is.Not.Empty, "The authored room selects a real light during prewarming.");
            var guidance = Object.FindObjectOfType<BedtimeGuidance>();
            var atmosphere = guidance.Atmosphere;
            Assert.That(atmosphere, Is.Not.Null, "Bedtime resources are prepared without binding a runtime.");
            var atmosphereRenderers = atmosphere.GetComponentsInChildren<MeshRenderer>(true)
                .Where(item => item.name == "BedtimeCeilingHaze" || item.name == "BedtimeGlowParticles").ToArray();
            Assert.That(atmosphereRenderers.Length, Is.EqualTo(2));
            Assert.That(atmosphereRenderers.All(item => !item.enabled), Is.True, "Prewarming cannot reveal bedtime effects on the title.");
            float initialVolume = audio.volume;
            Assert.That(initialVolume, Is.GreaterThan(0));
            var originalHost = host;
            int sceneHandle = host.gameObject.scene.handle;
            var logoRect = Menu.View.LogoScreenRect;
            var logoObject = Menu.View.GetComponentsInChildren<RawImage>(true).Single(item => item.name == "NormLogo");
            BeginTransitionTrace();
            Menu.View.StartButton.onClick.Invoke();
            Assert.That(LightingRows(), Is.EqualTo(lightingRows), "Starting must reuse the prewarmed lighting controls.");
            Assert.That(lightingPanel.SelectedLightPath, Is.EqualTo(selectedLight));
            Assert.That(guidance.Atmosphere, Is.SameAs(atmosphere));
            yield return new WaitForSecondsRealtime(.18f);
            Assert.That(Menu.View.ControlsAlpha, Is.InRange(.01f, .99f), "Controls must fade across several frames.");
            Assert.That(audio.isPlaying, Is.True);
            Assert.That(audio.volume, Is.GreaterThan(0).And.LessThan(initialVolume));
            var gameplayMusic = host.Audio.Service.GetSource(MiniGameAudioChannel.GameplayMusic);
            Assert.That(gameplayMusic.isPlaying && gameplayMusic.volume > 0, Is.True, "The existing gameplay track crossfades in while the title fades out.");
            Assert.That(Vector2.Distance(Menu.View.LogoScreenRect.position, logoRect.position), Is.LessThan(.1f));
            Assert.That(Vector2.Distance(Menu.View.LogoScreenRect.size, logoRect.size), Is.LessThan(.1f));
            Assert.That(host.Screen.TerminalDisplay.OpeningProgress, Is.Zero, "The fixed title holds through the controls fade.");
            Assert.That(host, Is.SameAs(originalHost));
            Assert.That(host.gameObject.scene.handle, Is.EqualTo(sceneHandle), "The opening must reuse its prewarmed scene.");
            Assert.That(MainMenuController.BlocksGameplayInput, Is.True);
            CaptureTransitionFrameIfRequested("transition-controls-midpoint");
            yield return Until(() => Menu.View.ReleaseAlpha > .05f && Menu.View.ReleaseAlpha < .95f,
                "title dissolves over the live monitor", 15);
            host = Object.FindObjectOfType<MiniGameBootstrap>();
            Assert.That(host.Screen.Canvas.enabled, Is.True, "The dissolve must reveal a live frame, not a disabled canvas.");
            Assert.That(host.Runtime.GetView().Paused, Is.True);
            Assert.That(host.Screen.TerminalDisplay.OpeningProgress, Is.GreaterThan(0).And.LessThan(.1f));
            Assert.That(Menu.View.ControlsAlpha, Is.Zero.Within(.001f));
            CaptureTransitionFrameIfRequested("transition-title-dissolve");
            yield return Until(() => host.Screen.TerminalDisplay.OpeningProgress > .1f
                && host.Screen.TerminalDisplay.OpeningProgress < .9f, "automatic monitor pullback", 10);
            float progress = host.Screen.TerminalDisplay.OpeningProgress;
            var camera = host.Screen.GetComponentsInChildren<Camera>(true).Single(item => item.name == "Terminal UI camera");
            float framing = camera.orthographicSize;
            Assert.That(MainMenuController.BlocksGameplayInput, Is.True);
            Assert.That(host.Runtime.GetView().Transcript, Is.Empty);
            Assert.That(Menu.View.GetComponentsInChildren<RawImage>(true).Single(item => item.name == "NormLogo"), Is.SameAs(logoObject));
            CaptureTransitionFrameIfRequested("transition-monitor-pullback");
            yield return new WaitForSecondsRealtime(.15f);
            Assert.That(host.Screen.TerminalDisplay.OpeningProgress, Is.GreaterThan(progress));
            Assert.That(camera.orthographicSize, Is.GreaterThan(framing), "The monitor camera must pull back continuously.");
            yield return WaitForGame();
            Assert.That(Menu.TransitionElapsed, Is.EqualTo(MainMenuController.ControlsFadeSeconds + MainMenuController.MonitorPullbackSeconds).Within(.001f));
            Assert.That(audio.isPlaying, Is.False);
            yield return WaitForTutorialIntroduction();
            Assert.That(host.Runtime.GetView().Transcript.Count(line => !line.IsPlayer), Is.EqualTo(1));
            Assert.That(host.Runtime.GetView().Transcript.Any(line => line.IsPlayer), Is.False);
            CaptureTransitionFrameIfRequested("transition-gameplay-ready");
            observeTransition = false;
        }

        [UnityTest, Timeout(90000)]
        public IEnumerator TitleLightingRebindRetainsSelectionAndInvalidatesRowsWhenLightsChange()
        {
            yield return new WaitForSecondsRealtime(.1f);
            var panel = host.Screen.LightingTuning;
            var lighting = panel.Controller;
            var lights = lighting.GetEditableSceneLights(RoomLightingPeriod.Day);
            Assert.That(lights.Length, Is.GreaterThan(1));
            var selectedPath = lighting.GetHierarchyPath(lights[lights.Length - 1]);
            Assert.That(panel.SelectLight(selectedPath), Is.True);
            yield return null;
            var rows = LightingRows();
            panel.Bind(null, lighting, host.Screen.SignalTuning);
            Assert.That(LightingRows(), Is.EqualTo(rows));
            Assert.That(panel.SelectedLightPath, Is.EqualTo(selectedPath));

            var addedLight = new GameObject("Title lighting cache regression", typeof(Light));
            SceneManager.MoveGameObjectToScene(addedLight, host.gameObject.scene);
            try
            {
                panel.Bind(null, lighting, host.Screen.SignalTuning);
                Assert.That(rows.All(row => !row.gameObject.activeSelf), Is.True,
                    "Changing the editable light list must invalidate its captured selection rows.");
                yield return null;
                Assert.That(LightingRows().Intersect(rows), Is.Empty);
                Assert.That(host.Runtime, Is.Null);
                Assert.That(MiniGameSessionHost.Current, Is.Null);
                Assert.That(Directory.GetFiles(directory), Is.Empty);
            }
            finally { Object.Destroy(addedLight); }
        }

        private Transform[] LightingRows() => host.Screen.GetComponentsInChildren<Transform>(true)
            .Single(item => item.name == "Parameters" && item.parent.name == "Viewport"
                && item.parent.parent.name == "LightingControls").Cast<Transform>().ToArray();

        [UnityTest, Timeout(90000)]
        public IEnumerator ContinueHasNoInitialSelectionAndKeyboardNavigationStillSelectsIt()
        {
            yield return StartGame();
            Menu.ReturnToMenu();
            yield return RebootToTitle();
            Assert.That(Menu.View.Page, Is.EqualTo(MainMenuPage.Menu));
            Assert.That(EventSystem.current.currentSelectedGameObject, Is.Null, "Entering the title must not select Continue.");
            var button = Menu.View.ContinueButton;
            ExecuteEvents.Execute(button.gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.pointerExitHandler);
            yield return new WaitForSecondsRealtime(.12f);
            Assert.That(button.targetGraphic.canvasRenderer.GetColor(), Is.EqualTo(button.colors.normalColor));
            Menu.View.SelectInitialForNavigation(false);
            Assert.That(EventSystem.current.currentSelectedGameObject, Is.SameAs(button.gameObject));
            ExecuteEvents.Execute(button.gameObject,
                new AxisEventData(EventSystem.current) { moveDir = MoveDirection.Right, moveVector = Vector2.right }, ExecuteEvents.moveHandler);
            Assert.That(EventSystem.current.currentSelectedGameObject, Is.SameAs(Menu.View.NewPeriodButton.gameObject));
        }

        [UnityTest, Timeout(90000)]
        public IEnumerator ReturningDuringNativeDayOneDialogueFreezesTypingWithoutAdvancingItsSourceLine()
        {
            yield return StartGame();
            var fixture = Fixture();
            DriveTo(fixture, view => view.Game.Day == 1 && view.Game.Phase == GamePhase.Work
                && view.TerminalOpen && view.Input == InputContext.Dialogue
                && view.Game.EncounterStage == EncounterStage.NormalDialogue && view.Dialogue != null);
            const float typingSpeed = 4;
            host.Screen.DialogueCharactersPerSecond = typingSpeed;
            host.RestoreCheckpoint(fixture.Save());
            var body = host.Screen.GetComponentsInChildren<Text>(true).Single(text => text.name == "TerminalDialogueBody");
            // The current Day1 source starts with a reception choice, not an introduction speech.
            // Let the real Yarn adapter present and submit that choice before checking its answer's typing.
            yield return UntilWithDialogueDiagnostics(() => ActiveReply() != null && !host.Screen.TransitionLocked,
                "native Day1 reception choice", 12, body);
            var reception = ActiveReply();
            Debug.Log(DialogueDiagnostic("before native reception click", body));
            Assert.That(reception, Is.InstanceOf<MiniGameAudioButton>());
            Assert.That(((MiniGameAudioButton)reception).ActivateWithAudio(), Is.True);
            yield return UntilWithDialogueDiagnostics(() => body.text.Length > 0 && !host.Screen.LineIsRevealed
                && !host.Screen.TransitionLocked && host.Runtime.GetView().Transcript.Any(line => !line.IsPlayer && !line.Completed),
                "native Day1 source line begins typing", 12, body);
            var runtime = host.Runtime;
            var before = runtime.GetView();
            var line = before.Transcript.Last(item => !item.IsPlayer && !item.Completed);
            string sourceId = line.SourceNodeId;
            Assert.That(sourceId, Is.Not.Empty, "Use the current package's real Yarn source identity.");
            Assert.That(line.Text.StartsWith(body.text, StringComparison.Ordinal), Is.True);
            string node = before.Game.CurrentNodeId;
            string cue = before.Cue?.Token;
            int transcriptCount = before.Transcript.Count;
            var camera = host.Player.ViewCamera;
            var position = camera.transform.position;
            var rotation = camera.transform.rotation;

            host.Save();
            Menu.ReturnToMenu();
            string pausedText = body.text;
            Assert.That(host.Screen.HandleTerminalKey(KeyCode.Return), Is.False);
            Assert.That(host.Screen.HandleTerminalKey(KeyCode.Alpha1), Is.False);
            yield return new WaitForSecondsRealtime(.2f);
            Assert.That(host.Runtime, Is.SameAs(runtime));
            Assert.That(runtime.GetView().Paused, Is.True);
            Assert.That(body.text, Is.EqualTo(pausedText), "The character-reveal clock freezes behind the main menu.");
            Assert.That(host.Screen.LineIsRevealed, Is.False);
            Assert.That(runtime.GetView().Game.CurrentNodeId, Is.EqualTo(node));
            Assert.That(runtime.GetView().Cue?.Token, Is.EqualTo(cue));
            Assert.That(runtime.GetView().Transcript.Count, Is.EqualTo(transcriptCount));
            Assert.That(runtime.IsDialogueLineComplete(sourceId), Is.False);
            Assert.That(Vector3.Distance(camera.transform.position, position), Is.LessThan(.001f));
            Assert.That(Quaternion.Angle(camera.transform.rotation, rotation), Is.LessThan(.01f));

            Menu.View.ContinueButton.onClick.Invoke();
            Assert.That(host.Screen.HandleTerminalKey(KeyCode.Return), Is.False);
            yield return WaitForGame();
            Assert.That(host.Runtime, Is.Not.SameAs(runtime));
            runtime = host.Runtime;
            Assert.That(runtime.GetView().Game.CurrentNodeId, Is.EqualTo(node));
            if (cue != null) Assert.That(runtime.GetView().Cue?.Token, Is.Not.EqualTo(cue), "Loading invalidates presentation callbacks.");
            else Assert.That(runtime.GetView().Cue, Is.Null);
            Assert.That(runtime.GetView().Transcript.Count, Is.EqualTo(transcriptCount));
            Assert.That(runtime.IsDialogueLineComplete(sourceId), Is.False, "Continue cannot complete a Yarn source line or select a response.");
            Assert.That(line.Text.StartsWith(body.text, StringComparison.Ordinal), Is.True,
                "Loading may replay the current source line, but must not advance it.");
            Assert.That(Vector3.Distance(camera.transform.position, position), Is.LessThan(.001f));
            Assert.That(Quaternion.Angle(camera.transform.rotation, rotation), Is.LessThan(.01f));
        }

        [UnityTest, Timeout(90000)]
        public IEnumerator ReturningDuringDreamFreezesThenReloadsManualCheckpoint()
        {
            yield return StartGame();
            RestoreDream();
            host.Dreams.Advance(host.DreamProfile.BlackScreenSeconds);
            host.Dreams.Advance(2.25f);
            Assert.That(host.Dreams.IsPlaying, Is.True);
            var runtime = host.Runtime;
            var camera = host.Player.ViewCamera;
            var position = camera.transform.position;
            var rotation = camera.transform.rotation;
            host.Save();
            string manual = File.ReadAllText(ManualPath);
            Menu.ReturnToMenu();
            Assert.That(Menu.HasRetainedGame, Is.True);
            Assert.That(Menu.View.Page, Is.EqualTo(MainMenuPage.Menu));
            Assert.That(VisibleButtonLabels(), Is.EqualTo(new[] { "继续检查", "开始新的检查期", "设置" }));
            Assert.That(Menu.View.StartButton.gameObject.activeInHierarchy, Is.False);
            Assert.That(runtime.GetView().Paused, Is.True);
            Assert.That(File.Exists(AutomaticPath), Is.True);
            Assert.That(File.ReadAllText(ManualPath), Is.EqualTo(manual));
            host.HandleEscape();
            Assert.That(Menu.IsVisible, Is.True, "Esc on the main menu must not resume the session.");
            yield return CaptureIfRequested("main-menu");
            Menu.View.SettingsButton.onClick.Invoke();
            Assert.That(Menu.View.Page, Is.EqualTo(MainMenuPage.Settings));
            var settings = Menu.View.Settings;
            AssertSharedTypography();
            settings.BrightnessSlider.value = 1.2f;
            settings.VolumeSlider.value = .35f;
            settings.PixelDetailButtons[0].onClick.Invoke();
            Assert.That(PlayerPreferences.Brightness, Is.EqualTo(1.2f).Within(.001f));
            Assert.That(AudioListener.volume, Is.EqualTo(.35f).Within(.001f));
            Assert.That(PlayerGraphicsSettings.Detail, Is.EqualTo(PixelDetail.High));
            host.Dreams.Advance(20);
            yield return null;
            Assert.That(host.Runtime, Is.SameAs(runtime));
            Assert.That(host.Dreams.PlaybackSeconds, Is.EqualTo(2.25f).Within(.001f));
            Assert.That(Vector3.Distance(camera.transform.position, position), Is.LessThan(.001f));
            Assert.That(Quaternion.Angle(camera.transform.rotation, rotation), Is.LessThan(.01f));
            yield return CaptureIfRequested("settings");
            host.HandleEscape();
            Assert.That(Menu.View.Page, Is.EqualTo(MainMenuPage.Menu));
            Menu.View.ContinueButton.onClick.Invoke();
            Assert.That(MainMenuController.BlocksGameplayInput, Is.True, "Continue also consumes its triggering frame.");
            yield return WaitForGame();
            Assert.That(host.Runtime, Is.Not.SameAs(runtime));
            Assert.That(host.Dreams.PlaybackSeconds, Is.EqualTo(2.25f).Within(.001f));
            host.Dreams.Advance(.25f);
            Assert.That(host.Dreams.PlaybackSeconds, Is.EqualTo(2.5f).Within(.001f));
            Assert.That(File.ReadAllText(ManualPath), Is.EqualTo(manual));
        }

        [UnityTest, Timeout(90000)]
        public IEnumerator HiddenSpaceContinueReloadsManualCheckpointAndResumesItsOcean()
        {
            yield return StartGame();
            Assert.That(host.DreamProfile.UseRoomCubemapEnding, Is.True);
            var fixture = EndingCampaignScenario.CreateHidden(EndingCampaignScenario.LoadPackage(), host.DreamProfile,
                Resources.Load<PlayerRebellionProfile>("MiniGame/PlayerRebellion"));
            Assert.That(fixture.TryInteract(InteractionTarget.Computer).Accepted, Is.True);
            while (fixture.GetView().Cue != null)
                Assert.That(fixture.CompleteCue(fixture.GetView().Cue.Token).Accepted, Is.True);
            Assert.That(fixture.EnterSelectedPatient(GameRuntime.SelfPatientId).Accepted, Is.True);
            while (fixture.GetView().Cue != null)
                Assert.That(fixture.CompleteCue(fixture.GetView().Cue.Token).Accepted, Is.True);
            EndingCampaignScenario.DriveSelfReview(fixture);
            Assert.That(fixture.ConfirmSelfPurification().Accepted, Is.True);
            host.RestoreCheckpoint(fixture.Save());
            yield return Until(() => Object.FindObjectOfType<RoomDreamEnding>() != null
                && Object.FindObjectOfType<RoomDreamEnding>().Ready, "native hidden space ready", 20);
            var room = Object.FindObjectOfType<RoomDreamEnding>();
            var ending = room.Ending;
            ending.UseExternalClock = true;
            ending.SetPaused(true);
            host.Save();
            var previousRuntime = host.Runtime;
            var position = ending.ViewCamera.transform.position;
            Menu.ReturnToMenu();
            Menu.Continue();
            yield return WaitForGame();
            yield return Until(() => Object.FindObjectOfType<RoomDreamEnding>() != null
                && Object.FindObjectOfType<RoomDreamEnding>().Ready, "restored hidden space ready", 20);
            ending = Object.FindObjectOfType<RoomDreamEnding>().Ending;
            Assert.That(host.Runtime, Is.Not.SameAs(previousRuntime));
            Assert.That(host.Runtime.GetView().Game.Ending, Is.EqualTo(EndingType.DreamSea));
            yield return Until(() => ending.Audio.GetSource(MiniGameAudioChannel.Ocean).isPlaying,
                "restored ocean audio", 3);
            Assert.That(host.Screen.Canvas.enabled, Is.False);
            Assert.That(Vector3.Distance(ending.ViewCamera.transform.position, position), Is.LessThan(.001f));
            ending.SetPaused(true);
            Assert.That(ending.Audio.GetSource(MiniGameAudioChannel.Ocean).isPlaying, Is.False);
        }

        [UnityTest, Timeout(90000)]
        public IEnumerator NewPeriodCancelPreservesProgressAndConfirmationReplacesOnlyAutomaticProgress()
        {
            yield return StartGame();
            RestoreDream();
            host.Dreams.Advance(host.DreamProfile.BlackScreenSeconds);
            host.Dreams.Advance(1.25f);
            PlayerPreferences.SetBrightness(.8f);
            PlayerPreferences.SetMasterVolume(.4f);
            PlayerGraphicsSettings.SetDetail(PixelDetail.Medium);
            File.WriteAllText(ManualPath, "retain this manual checkpoint");
            MiniGameSessionHost.Current.SaveAutomatic();
            Menu.ReturnToMenu();
            var previousRuntime = host.Runtime;
            var previousHost = host;
            int sceneHandle = host.gameObject.scene.handle;
            string previousAutomatic = File.ReadAllText(AutomaticPath);
            Menu.View.NewPeriodButton.onClick.Invoke();
            Assert.That(Menu.View.Page, Is.EqualTo(MainMenuPage.ConfirmNew));
            Menu.View.CancelButton.onClick.Invoke();
            Assert.That(Menu.View.Page, Is.EqualTo(MainMenuPage.Menu));
            Assert.That(host.Runtime, Is.SameAs(previousRuntime));
            Assert.That(File.ReadAllText(AutomaticPath), Is.EqualTo(previousAutomatic));
            Menu.View.NewPeriodButton.onClick.Invoke();
            Menu.View.ConfirmButton.onClick.Invoke();
            yield return WaitForGame();
            Assert.That(host.Runtime, Is.Not.SameAs(previousRuntime));
            Assert.That(host, Is.SameAs(previousHost), "A new period resets the existing prewarmed room.");
            Assert.That(host.gameObject.scene.handle, Is.EqualTo(sceneHandle));
            Assert.That(host.Runtime.GetView().Game.Day, Is.Zero);
            Assert.That(File.ReadAllText(AutomaticPath), Is.Not.EqualTo(previousAutomatic));
            Assert.That(File.ReadAllText(ManualPath), Is.EqualTo("retain this manual checkpoint"));
            PlayerPreferences.Reload();
            PlayerGraphicsSettings.Reload();
            Assert.That(PlayerPreferences.Brightness, Is.EqualTo(.8f).Within(.001f));
            Assert.That(PlayerPreferences.MasterVolume, Is.EqualTo(.4f).Within(.001f));
            Assert.That(PlayerGraphicsSettings.Detail, Is.EqualTo(PixelDetail.Medium));
        }

        [UnityTest, Timeout(90000)]
        public IEnumerator CorruptAutomaticProgressDoesNotOverrideValidManualProgress()
        {
            MiniGameBootstrap.WriteNewCampaignContinuation(ManualPath);
            string manual = File.ReadAllText(ManualPath);
            File.WriteAllText(AutomaticPath, "{broken automatic save");
            Menu.ShowStartup();
            Assert.That(Menu.View.Page, Is.EqualTo(MainMenuPage.Menu));
            Assert.That(MainMenuContinueStore.ResolveResumePath(directory), Is.EqualTo(ManualPath));
            Menu.View.ContinueButton.onClick.Invoke();
            yield return WaitForGame();
            Assert.That(host.Runtime, Is.Not.Null);
            Assert.That(host.Runtime.GetView().Game.Day, Is.Zero);
            Assert.That(File.ReadAllText(AutomaticPath), Is.EqualTo("{broken automatic save"));
            Assert.That(File.ReadAllText(ManualPath), Is.EqualTo(manual));
        }

        [UnityTest, Timeout(90000)]
        public IEnumerator RestartedTitleResumesSavedDreamFrameRatherThanStartingDayZero()
        {
            yield return StartGame();
            RestoreDream();
            host.Dreams.Advance(host.DreamProfile.BlackScreenSeconds);
            host.Dreams.Advance(2.25f);
            var previousRuntime = host.Runtime;
            var savedPosition = host.Player.ViewCamera.transform.position;
            MiniGameSessionHost.Current.SaveTo(AutomaticPath);
            Menu.ReturnToMenu();
            yield return RebootToTitle();
            Assert.That(host.Runtime, Is.Null);
            Assert.That(Menu.View.Page, Is.EqualTo(MainMenuPage.Menu));
            Menu.View.ContinueButton.onClick.Invoke();
            host.Dreams.UseExternalClock = true;
            Assert.That(host.Runtime.GetView().Paused, Is.True, "Loading cannot run behind the covered menu.");
            yield return WaitForGame();
            Assert.That(host.Runtime, Is.Not.SameAs(previousRuntime));
            Assert.That(host.Runtime.GetView().Game.Day, Is.EqualTo(1));
            Assert.That(host.Runtime.GetView().Game.Phase, Is.EqualTo(GamePhase.Dream));
            Assert.That(host.Dreams.Phase, Is.EqualTo(DreamPlaybackPhase.Playing));
            Assert.That(host.Dreams.PlaybackSeconds, Is.EqualTo(2.25f).Within(.001f));
            Assert.That(Vector3.Distance(host.Player.ViewCamera.transform.position, savedPosition), Is.LessThan(.001f));
            host.Dreams.Advance(.5f);
            Assert.That(host.Dreams.PlaybackSeconds, Is.EqualTo(2.75f).Within(.001f));
        }

        [UnityTest, Timeout(90000), Category("InteractionWakeGm")]
        public IEnumerator RestartedTitleRestoresAutomaticWakeProgressWithoutAdvancingTheDayAgain()
        {
            yield return StartGame();
            RestoreDream();
            for (int step = 0; step < 4 && host.Dreams.OwnsCamera; step++) host.Dreams.Advance(1000);
            Assert.That(host.Runtime.GetView().Game.Day, Is.EqualTo(2));
            yield return Until(() => host.Effects.CaptureSleepCheckpoint()?.Phase == SleepTransitionPhase.Waking
                && host.Effects.CaptureSleepCheckpoint().WakeElapsed >= .15f, "automatic wake progress", 5);
            MiniGameSessionHost.Current.SaveTo(AutomaticPath);
            Menu.ReturnToMenu();
            float elapsed = host.Effects.CaptureSleepCheckpoint().WakeElapsed;
            Assert.That(elapsed, Is.GreaterThanOrEqualTo(.15f));
            yield return new WaitForSecondsRealtime(.3f);
            Assert.That(host.Effects.CaptureSleepCheckpoint().WakeElapsed, Is.EqualTo(elapsed).Within(.001f));
            yield return RebootToTitle();
            Menu.View.ContinueButton.onClick.Invoke();
            Assert.That(host.Runtime.GetView().Paused, Is.True);
            var loaded = host.Effects.CaptureSleepCheckpoint();
            Assert.That(loaded, Is.Not.Null);
            Assert.That(loaded.Phase, Is.EqualTo(SleepTransitionPhase.Waking));
            Assert.That(loaded.WakeElapsed, Is.EqualTo(elapsed).Within(.001f));
            Assert.That(host.Runtime.GetView().Game.Day, Is.EqualTo(2));
            yield return WaitForGame();
            yield return Until(() => host.Effects.CaptureSleepCheckpoint() == null,
                "one completed wake transition", 5);
            yield return new WaitForSecondsRealtime(.2f);
            Assert.That(host.Runtime.GetView().Game.Day, Is.EqualTo(2));
            Assert.That(host.Effects.CaptureSleepCheckpoint(), Is.Null);
        }

        [UnityTest, Timeout(90000)]
        public IEnumerator FailedAutomaticSavePreservesFilesAndDoesNotBlockGameplay()
        {
            yield return StartGame();
            RestoreDream();
            string automatic = File.ReadAllText(AutomaticPath);
            string blockingFile = Path.Combine(directory, "not-a-directory");
            File.WriteAllText(blockingFile, "preserve this file");
            MainMenuController.SaveDirectoryOverride = blockingFile;
            var runtime = host.Runtime;
            MiniGameSessionHost.Current.SaveAutomatic();
            Assert.That(MiniGameSessionHost.Current.LastMessage, Does.Contain("保存失败"));
            Assert.That(host.Runtime, Is.SameAs(runtime));
            Assert.That(File.ReadAllText(blockingFile), Is.EqualTo("preserve this file"));
            Assert.That(File.ReadAllText(AutomaticPath), Is.EqualTo(automatic));
            MainMenuController.SaveDirectoryOverride = directory;
            yield return null;
        }

        [UnityTest, Timeout(90000), Category("InteractionWakeGm")]
        public IEnumerator LegacyAfterDreamLoadStartsBlackAndWaitsForPausedResumeBeforeWakingOnce()
        {
            yield return StartGame();
            var fixture = Fixture();
            DriveTo(fixture, view => view.Game.Day == 1 && view.Cue?.Kind == PresentationKind.Dream);
            // Recreate the former adapter boundary from legal core commands in a separate session.
            string checkpoint = fixture.Save();
            var session = MiniGameSessionHost.Current;
            var core = SnapshotCodec.Deserialize(JsonUtility.FromJson<LegacyCoreEnvelope>(checkpoint).CoreJson, session.Rules);
            Assert.That(core.Execute(new CompleteDream()).Accepted, Is.True);
            Assert.That(core.GetPlayerView().Phase, Is.EqualTo(GamePhase.AfterDream));
            string encoded = JsonUtility.ToJson(new LegacyCoreEnvelope { CoreJson = SnapshotCodec.Serialize(core) });
            string quotedCore = encoded.Substring(encoded.IndexOf(':') + 1);
            quotedCore = quotedCore.Substring(0, quotedCore.Length - 1);
            checkpoint = ReplaceCheckpointValue(checkpoint, "CoreJson", quotedCore);
            checkpoint = ReplaceCheckpointValue(checkpoint, "CueKind", "null");
            checkpoint = ReplaceCheckpointValue(checkpoint, "DreamFromBed", "false");
            checkpoint = ReplaceCheckpointValue(checkpoint, "Paused", "false");
            string path = Path.Combine(directory, "legacy-after-dream.json");
            File.WriteAllText(path, checkpoint);
            foreach (bool pauseOnLoad in new[] { true, false })
            {
                Assert.That(session.TryLoadFrom(path, out var error, pauseOnLoad), Is.True, error);
                Assert.That(host.Runtime.RestoredFromAfterDream, Is.True);
                var fade = host.Screen.GetComponentsInChildren<Image>(true).Single(image => image.name == "PresentationFade");
                Assert.That(fade.color.a, Is.EqualTo(1), "The first restored frame is black, including an unpaused load.");
                if (pauseOnLoad)
                {
                    Assert.That(host.Runtime.GetView().Game.Day, Is.EqualTo(1));
                    Assert.That(host.Runtime.GetView().Game.Phase, Is.EqualTo(GamePhase.AfterDream));
                    Assert.That(host.Effects.CaptureSleepCheckpoint().Phase, Is.EqualTo(SleepTransitionPhase.WaitingToWake));
                    yield return new WaitForSecondsRealtime(.2f);
                    Assert.That(host.Runtime.GetView().Game.Day, Is.EqualTo(1));
                    Assert.That(fade.color.a, Is.EqualTo(1));
                    host.Runtime.SetPaused(false);
                }
                Assert.That(host.Runtime.GetView().Game.Day, Is.EqualTo(2));
                Assert.That(host.Effects.CaptureSleepCheckpoint().Phase, Is.EqualTo(SleepTransitionPhase.Waking));
                yield return Until(() => host.Effects.CaptureSleepCheckpoint() == null, "legacy wake completion", 5);
                Assert.That(host.Runtime.GetView().Game.Day, Is.EqualTo(2));
                Assert.That(fade.color.a, Is.Zero);
                Assert.That(File.ReadAllText(path), Is.EqualTo(checkpoint), "Migration does not overwrite the manual source file.");
            }
        }

        [Serializable] private sealed class LegacyCoreEnvelope { public string CoreJson; }
        private static string ReplaceCheckpointValue(string json, string key, string value)
            => Regex.Replace(json, "\"" + key + "\"\\s*:\\s*(?:\"(?:\\\\.|[^\"\\\\])*\"|true|false|null)",
                _ => "\"" + key + "\":" + value);

        private IEnumerator StartGame()
        {
            Assert.That(Menu.View.Page, Is.EqualTo(MainMenuPage.FirstStart));
            Menu.View.StartButton.onClick.Invoke();
            yield return WaitForGame();
        }

        private IEnumerator WaitForGame()
        {
            yield return Until(() => Menu != null && !Menu.IsVisible && !MainMenuController.BlocksGameplayInput
                && MiniGameSessionHost.Current != null && !MiniGameSessionHost.Current.IsTransitioning, "menu releases the loaded game", 30);
            host = Object.FindObjectOfType<MiniGameBootstrap>();
            Assert.That(host, Is.Not.Null);
            Assert.That(host.Runtime, Is.Not.Null);
            host.Player.enabled = false;
            host.Screen.DialogueCharactersPerSecond = 0;
        }

        private IEnumerator WaitForTutorialIntroduction() => Until(() => host.Runtime.GetView().Cue?.Kind == PresentationKind.Introduction
            && host.Runtime.GetView().Transcript.Any(line => !line.IsPlayer)
            && host.Screen.GetComponentsInChildren<Button>(true).Any(button => button.name == "Continue"
                && button.gameObject.activeInHierarchy && button.interactable), "native tutorial introduction", 12);

        private IEnumerator RebootToTitle()
        {
            StopSceneAdapters();
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
            Assert.That(host.Runtime, Is.Null, "Reloading a title does not silently load its progress.");
        }

        private void StopSceneAdapters()
        {
            var current = host != null ? host : Object.FindObjectOfType<MiniGameBootstrap>();
            if (current == null) return;
            current.enabled = false;
            if (current.Player != null) current.Player.enabled = false;
            current.Dreams?.PrepareForRuntimeChange();
            current.Effects?.PrepareForRuntimeChange();
        }

        private GameRuntime Fixture()
        {
            var session = MiniGameSessionHost.Current;
            Assert.That(session, Is.Not.Null);
            var runtime = new GameRuntime(session.Rules, session.Content, true);
            if (runtime.UsesEndingFlow) runtime.ConfigureEndings(EndingDialoguePresenter.LoadCatalog());
            return runtime;
        }

        private void RestoreDream()
        {
            var fixture = Fixture();
            DriveTo(fixture, view => view.Game.Day == 1 && view.Cue?.Kind == PresentationKind.Dream);
            host.RestoreCheckpoint(fixture.Save());
            host.Dreams.UseExternalClock = true;
            Assert.That(host.Dreams.Phase, Is.EqualTo(DreamPlaybackPhase.Aligning));
        }

        private string[] VisibleButtonLabels() => Menu.View.GetComponentsInChildren<Button>()
            .Where(button => button.gameObject.activeInHierarchy).Select(button => button.GetComponentInChildren<Text>().text).ToArray();

        private string StatusText() => Menu.View.GetComponentsInChildren<Text>(true)
            .Single(text => text.name == "MainMenuStatus").text;

        private Button ActiveReply() => host.Screen.GetComponentsInChildren<Button>(true)
            .FirstOrDefault(button => button.name.StartsWith("Reply-", StringComparison.Ordinal)
                && button.gameObject.activeInHierarchy && button.interactable);

        private IEnumerator UntilWithDialogueDiagnostics(Func<bool> condition, string label, float seconds, Text body)
        {
            float deadline = Time.realtimeSinceStartup + seconds;
            float nextDiagnostic = 0;
            while (!condition() && Time.realtimeSinceStartup < deadline)
            {
                if (Time.realtimeSinceStartup >= nextDiagnostic)
                {
                    Debug.Log(DialogueDiagnostic(label, body));
                    nextDiagnostic = Time.realtimeSinceStartup + 1;
                }
                yield return null;
            }
            string diagnostic = DialogueDiagnostic(label + (condition() ? " READY" : " TIMEOUT"), body);
            Debug.Log(diagnostic);
            Assert.That(condition(), Is.True, diagnostic);
        }

        private string DialogueDiagnostic(string label, Text body)
        {
            var view = host.Runtime.GetView();
            var recent = view.Transcript.Skip(Math.Max(0, view.Transcript.Count - 3))
                .Select(line => line.SourceNodeId + ":completed=" + line.Completed + ":player=" + line.IsPlayer);
            string shown = body != null ? body.text.Replace("\n", "\\n").Replace("\r", "\\r") : "<missing body>";
            return "MINIGAME_MENU_DIALOGUE " + label + "; day=" + view.Game.Day + "; phase=" + view.Game.Phase
                + "; input=" + view.Input + "; stage=" + view.Game.EncounterStage + "; node=" + view.Game.CurrentNodeId
                + "; cue=" + view.Cue?.Kind + "/" + view.Cue?.Token + "; paused=" + view.Paused
                + "; lineRevealed=" + host.Screen.LineIsRevealed + "; linePages=" + host.Screen.CurrentLinePageCount
                + "; transitionLocked=" + host.Screen.TransitionLocked + "; power=" + host.Screen.PowerPhase
                + "; menu=" + Menu.View.Page + "; body=" + shown + "; transcript=" + view.Transcript.Count
                + "; recent=" + string.Join(",", recent) + "; yarn=" + host.GetComponent<MiniGameYarnBridge>().Diagnostics;
        }

        private void AssertSharedTypography()
        {
            var menuFont = Menu.View.StartButton.GetComponentInChildren<Text>(true).font;
            Assert.That(menuFont, Is.Not.Null);
            Assert.That(menuFont.name, Is.EqualTo("Game UI - " + GameUiTypography.Family),
                "The title must track the game's typography entrypoint, including its installed-font fallback.");
            if (host.Runtime != null)
            {
                var pause = host.Screen.GetComponentInChildren<PauseMenuView>(true);
                Assert.That(pause, Is.Not.Null);
                foreach (var label in pause.GetComponentsInChildren<Text>(true))
                {
                    Assert.That(label, Is.TypeOf<TerminalBitmapText>(), "Pause menu: " + label.name);
                    Assert.That(((TerminalBitmapText)label).ScaleToFontSize, Is.True);
                }
                var gameFont = pause.GetComponentsInChildren<Text>(true).First().font;
                Assert.That(gameFont, Is.Not.Null);
                Assert.That(menuFont.name, Is.EqualTo(gameFont.name));
                Assert.That(menuFont.fontNames, Is.EqualTo(gameFont.fontNames));
            }
            foreach (var label in Menu.View.GetComponentsInChildren<Text>(true))
            {
                Assert.That(label, Is.TypeOf<TerminalBitmapText>(), label.name + " must draw the pixel atlas.");
                Assert.That(((TerminalBitmapText)label).ScaleToFontSize, Is.True);
                Assert.That(label.font, Is.SameAs(menuFont), label.name + " must share the menu owner's game font.");
            }
            foreach (var label in Menu.View.Settings.GetComponentsInChildren<Text>(true))
                Assert.That(label.font, Is.SameAs(menuFont), label.name + " must retain the same font in settings.");
            // Preview cards have separate title/description typography; uniform action rows retain this contract.
            foreach (var button in Menu.View.GetComponentsInChildren<Button>(true)
                .Where(button => !button.transform.IsChildOf(Menu.View.PixelSetup.transform)))
            {
                var label = button.GetComponentsInChildren<Text>(true).Single(text => text.name == "Label");
                Assert.That(label.fontStyle, Is.EqualTo(FontStyle.Normal), button.name + " must use the Continue label's regular weight.");
                Assert.That(label.fontSize, Is.EqualTo(GameUiTypography.ReferenceSize), button.name + " must match the Continue label size.");
            }
            foreach (var label in Menu.View.GetComponentsInChildren<Text>(true)
                .Where(text => text.name == "SettingsTitle" || text.name == "ConfirmTitle"))
            {
                Assert.That(label.fontStyle, Is.EqualTo(FontStyle.Normal), label.name + " must use the Continue label's regular weight.");
                Assert.That(label.fontSize, Is.EqualTo(GameUiTypography.ReferenceSize), label.name + " must match the Continue label size.");
            }
        }

        private static void RestoreFloat(string key, bool existed, float value)
        {
            if (existed) PlayerPrefs.SetFloat(key, value);
            else PlayerPrefs.DeleteKey(key);
        }

        private static IEnumerator Until(Func<bool> condition, string label, float seconds)
        {
            float deadline = Time.realtimeSinceStartup + seconds;
            while (!condition() && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.That(condition(), Is.True, label + " timed out.");
        }

        private static void DriveTo(GameRuntime runtime, Func<RuntimeView, bool> stop, bool maximizeTrust = false)
        {
            var package = maximizeTrust ? EndingCampaignScenario.LoadPackage() : null;
            for (int step = 0; step < 2000; step++)
            {
                var view = runtime.GetView();
                if (stop(view)) return;
                RuntimeResult result;
                if (view.Cue != null) result = runtime.CompleteCue(view.Cue.Token);
                else if (view.TerminalPage == TerminalPage.Archive) result = runtime.LeaveTerminal();
                else if (view.TerminalPage == TerminalPage.Directory)
                {
                    var patient = view.Patients.FirstOrDefault(item => item.CanReceiveToday);
                    Assert.That(patient, Is.Not.Null);
                    result = runtime.EnterSelectedPatient(patient.Id);
                }
                else if (view.Input == InputContext.Dialogue)
                {
                    if (view.Game.EncounterStage == EncounterStage.AwaitingDisposition)
                    {
                        var available = view.Game.AvailableDispositions;
                        var disposition = view.Game.RemainingPurificationQuota > 0 && available.Contains(Disposition.Purify)
                            ? Disposition.Purify : available.Contains(Disposition.Treat) ? Disposition.Treat : available.First();
                        result = runtime.SubmitDisposition(view.Game.CurrentEncounterId, disposition);
                    }
                    else
                    {
                        string response = view.Game.AvailableResponseIds.First();
                        if (package != null)
                        {
                            var meeting = package.Rules.Days.Single(d => d.Day == view.Game.Day).Meetings.Single(m => m.PatientId == view.Game.CurrentPatientId);
                            var round = view.Game.EncounterStage == EncounterStage.SpecialDialogue ? meeting.SpecialRound
                                : meeting.NormalRounds.Single(r => r.NodeId == view.Game.CurrentNodeId);
                            response = round.Responses.OrderByDescending(r => r.TrustDelta).First().Id;
                        }
                        result = runtime.SelectResponse(view.Game.CurrentEncounterId, view.Game.CurrentNodeId, response);
                    }
                }
                else if (view.Game.Phase == GamePhase.Evening || view.Game.Phase == GamePhase.AfterDream)
                    result = runtime.TryInteract(InteractionTarget.Bed);
                else if (view.Game.Phase == GamePhase.MorningInspection) result = runtime.TryInteract(InteractionTarget.Door);
                else if (view.Game.Phase == GamePhase.Apartment || view.Game.Phase == GamePhase.Work)
                    result = runtime.TryInteract(InteractionTarget.Computer);
                else { Assert.Fail("No legal campaign step in " + view.Game.Phase + "/" + view.Input); return; }
                Assert.That(result.Accepted, Is.True, "Rejected campaign command at step " + step);
            }
            Assert.Fail("Campaign driver exceeded 2000 accepted commands.");
        }

        private static void CaptureTransitionFrameIfRequested(string name)
        {
            string output = Environment.GetEnvironmentVariable("MINIGAME_MENU_CAPTURE_DIR");
            if (string.IsNullOrWhiteSpace(output)) return;
            Directory.CreateDirectory(output);
            captureRequestedFrame = Time.frameCount;
            ScreenCapture.CaptureScreenshot(Path.Combine(output, name + "-" + Screen.width + "x" + Screen.height + ".png"));
        }

        [Serializable]
        private sealed class TransitionTrace
        {
            public int width, height;
            public string note = "Editor frame intervals include test assertions and optional asynchronous screenshot requests; captureFrame flags the request frame and following frame.";
            public List<TransitionFrame> frames = new List<TransitionFrame>(4096);
        }

        [Serializable]
        private struct TransitionFrame
        {
            public int frame;
            public float wallSeconds, frameSeconds, clock, controlsAlpha, backdropAlpha, openingProgress, cameraSize;
            public bool menuVisible, captureFrame;
        }

        private void BeginTransitionTrace()
        {
            if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("MINIGAME_MENU_CAPTURE_DIR"))) return;
            transitionTrace = new TransitionTrace { width = Screen.width, height = Screen.height };
            observeTransition = true;
            host.StartCoroutine(ObserveTransitionFrames());
        }

        private IEnumerator ObserveTransitionFrames()
        {
            var camera = host.Screen.GetComponentsInChildren<Camera>(true).Single(item => item.name == "Terminal UI camera");
            float start = Time.realtimeSinceStartup, previous = start;
            while (observeTransition && Menu != null && transitionTrace.frames.Count < 4096)
            {
                float now = Time.realtimeSinceStartup;
                transitionTrace.frames.Add(new TransitionFrame
                {
                    frame = Time.frameCount, wallSeconds = now - start, frameSeconds = now - previous,
                    clock = Menu.TransitionElapsed, controlsAlpha = Menu.View.ControlsAlpha, backdropAlpha = Menu.View.ReleaseAlpha,
                    openingProgress = host.Screen.TerminalDisplay.OpeningProgress, cameraSize = camera.orthographicSize,
                    menuVisible = Menu.IsVisible, captureFrame = Time.frameCount - captureRequestedFrame <= 1
                });
                previous = now;
                yield return null;
            }
        }

        private void WriteTransitionTraceIfRequested()
        {
            if (transitionTrace == null) return;
            string output = Environment.GetEnvironmentVariable("MINIGAME_MENU_CAPTURE_DIR");
            if (!string.IsNullOrWhiteSpace(output))
            {
                Directory.CreateDirectory(output);
                File.WriteAllText(Path.Combine(output, "transition-frames-" + transitionTrace.width + "x" + transitionTrace.height + ".json"),
                    JsonUtility.ToJson(transitionTrace, true));
            }
            transitionTrace = null;
        }

        private IEnumerator CaptureIfRequested(string name, Canvas captureCanvas = null)
        {
            string output = Environment.GetEnvironmentVariable("MINIGAME_MENU_CAPTURE_DIR");
            if (string.IsNullOrWhiteSpace(output)) yield break;
            Directory.CreateDirectory(output);
            var canvas = captureCanvas != null ? captureCanvas : Menu.View.Canvas;
            var originalMode = canvas.renderMode;
            var originalCamera = canvas.worldCamera;
            float originalPlane = canvas.planeDistance;
            var nodes = canvas.GetComponentsInChildren<Transform>(true);
            var layers = nodes.Select(node => node.gameObject.layer).ToArray();
            var cameraRoot = new GameObject("Main menu capture camera", typeof(Camera));
            var camera = cameraRoot.GetComponent<Camera>();
            camera.enabled = false;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.black;
            camera.cullingMask = 1 << 5;
            camera.nearClipPlane = .1f;
            camera.farClipPlane = 10;
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = camera;
            canvas.planeDistance = 1;
            foreach (var node in nodes) node.gameObject.layer = 5;
            try
            {
                foreach (var size in new[] { new Vector2Int(1920, 1080), new Vector2Int(2560, 1440) })
                {
                    var target = new RenderTexture(size.x, size.y, 24, RenderTextureFormat.ARGB32);
                    var image = new Texture2D(size.x, size.y, TextureFormat.RGB24, false);
                    var previous = RenderTexture.active;
                    try
                    {
                        target.Create();
                        camera.targetTexture = target;
                        Canvas.ForceUpdateCanvases();
                        yield return null;
                        Canvas.ForceUpdateCanvases();
                        camera.Render();
                        RenderTexture.active = target;
                        image.ReadPixels(new Rect(0, 0, size.x, size.y), 0, 0);
                        image.Apply();
                        File.WriteAllBytes(Path.Combine(output, name + "-" + size.x + "x" + size.y + ".png"), image.EncodeToPNG());
                    }
                    finally
                    {
                        RenderTexture.active = previous;
                        camera.targetTexture = null;
                        target.Release();
                        Object.Destroy(target);
                        Object.Destroy(image);
                    }
                }
            }
            finally
            {
                canvas.renderMode = originalMode;
                canvas.worldCamera = originalCamera;
                canvas.planeDistance = originalPlane;
                for (int i = 0; i < nodes.Length; i++) nodes[i].gameObject.layer = layers[i];
                Object.Destroy(cameraRoot);
                Canvas.ForceUpdateCanvases();
            }
        }
    }
}
