using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using MiniGame.Presentation;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace MiniGame.UnityHost
{
    /// <summary>Owns front-end input and suspension; the retained runtime owns all story state.</summary>
    public sealed class MainMenuController : MonoBehaviour
    {
        public static MainMenuController Current { get; private set; }
        // Explicit direct-play entry for existing automation/development captures; normal launches show the title.
        public static bool SkipStartup { get; set; }
        public static string SaveDirectoryOverride { get; set; }
        public static bool BlocksGameplayInput => Current != null && (Current.IsVisible || Current.releasing);
        public MainMenuView View { get; private set; }
        public bool IsVisible => View != null && View.Page != MainMenuPage.Hidden;
        public bool HasRetainedGame => !completedInspection && retained && Runtime != null;
        private static string SaveDirectory => SaveDirectoryOverride ?? MiniGameBootstrap.SaveDirectory;
        public string ContinuePath => Path.Combine(SaveDirectory, "campaign-continue.json");
        private MiniGameBootstrap owner;
        private MiniGameAudioService menuAudio;
        private readonly List<MiniGameAudioService> suspended = new List<MiniGameAudioService>();
        private bool retained, releasing, completedInspection;
        private const float MenuFadeSeconds = 1.05f;
        private string pendingNewInspection;
        private GameRuntime Runtime => owner != null && owner.Runtime != null ? owner.Runtime : MiniGameSessionHost.Current?.Runtime;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Current = null;
            SaveDirectoryOverride = null;
            SkipStartup = Array.IndexOf(Environment.GetCommandLineArgs(), "-minigameDirectPlay") >= 0;
        }

        public static MainMenuController Attach(MiniGameBootstrap host)
        {
            if (Current == null)
            {
                var root = new GameObject("NORM main menu", typeof(MainMenuController));
                Current = root.GetComponent<MainMenuController>();
                DontDestroyOnLoad(root);
                Current.Build();
            }
            Current.owner = host;
            if (FindObjectOfType<EventSystem>() == null)
                new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            host.Screen.SetMainMenuCovered(Current.IsVisible || Current.releasing);
            PlayerBrightnessEffect.EnsureFor(host.Player.ViewCamera);
            return Current;
        }

        private void Build()
        {
            View = MainMenuView.Create(transform,
                () => Click(StartFirstInspection), () => Click(Continue), () => Click(RequestNewInspection),
                () => Click(ShowSettings), () => Click(ConfirmNewInspection), () => Click(CancelNewInspection),
                () => Click(BackToMenu));
            var audioRoot = new GameObject("Main menu audio", typeof(MiniGameAudioService));
            audioRoot.transform.SetParent(transform, false);
            menuAudio = audioRoot.GetComponent<MiniGameAudioService>();
            menuAudio.Configure(Resources.Load<MiniGameAudioProfile>("MiniGame/AudioDefault"));
            View.SignalTuning.LogoGlitch += () =>
            {
                if (IsVisible && !releasing) menuAudio.PlayOneShot(MiniGameAudioChannel.LogoGlitch, MiniGameSound.LogoGlitch);
            };
            View.Show(MainMenuPage.Hidden, false);
        }

        private void Click(Action action)
        {
            if (releasing) return;
            menuAudio.PlayOneShot(MiniGameAudioChannel.UI, MiniGameSound.UIInteraction);
            action();
        }

        public void ShowStartup()
        {
            retained = false;
            CoverGame(true);
            View.Show(MainMenuContinueStore.ResolveResumePath(SaveDirectory) == null && !MainMenuContinueStore.HasCompletedInspection(SaveDirectory)
                ? MainMenuPage.FirstStart : MainMenuPage.Menu, HasResumeFile());
            StartMenuMusic();
        }

        public void ReturnToMenu()
        {
            if (releasing || Runtime == null || MiniGameSessionHost.Current != null && MiniGameSessionHost.Current.IsTransitioning) return;
            if (completedInspection || MainMenuContinueStore.HasCompletedInspection(SaveDirectory))
            { CompleteInspectionAndReturnToMenu(); return; }
            CoverGame(true);
            Runtime.SetPaused(true);
            retained = true;
            SuspendGameAudio();
            string error = null;
            if (MiniGameSessionHost.Current != null) MiniGameSessionHost.Current.TrySaveTo(ContinuePath, out error);
            else
            {
                try { new RuntimeFileSaveStore(ContinuePath).Save(Runtime); }
                catch (Exception exception) { error = "保存失败：" + exception.Message; }
            }
            View.Show(MainMenuPage.Menu, true, error ?? string.Empty);
            StartMenuMusic();
        }

        public void Continue()
        {
            if (releasing || View.Page != MainMenuPage.Menu) return;
            if (completedInspection || MainMenuContinueStore.HasCompletedInspection(SaveDirectory))
            { View.SetStatus("本次检查期已结束，请开始新的检查期。", true); return; }
            if (!HasRetainedGame)
            {
                string path = MainMenuContinueStore.ResolveResumePath(SaveDirectory);
                if (path == null) { View.SetStatus("没有可继续的检查进度。", true); return; }
                if (owner == null) { View.SetStatus("当前场景不能读取检查进度。", true); return; }
                if (!owner.TryLoadCampaign(path, out var error)) { View.SetStatus(error, true); return; }
                Runtime?.SetPaused(true);
                retained = true;
                CoverGame(true);
                SuspendGameAudio();
            }
            StartCoroutine(ReleaseInputAndResume());
        }

        public void StartFirstInspection()
        {
            if (View.Page != MainMenuPage.FirstStart || releasing) return;
            BeginNewInspection();
        }

        public void RequestNewInspection()
        {
            if (releasing) return;
            if (!IsVisible && Runtime != null)
            {
                ReturnToMenu();
            }
            View.Show(MainMenuPage.ConfirmNew, HasRetainedGame || HasResumeFile());
        }

        public void ConfirmNewInspection()
        {
            if (View.Page != MainMenuPage.ConfirmNew || releasing) return;
            BeginNewInspection();
        }

        private void BeginNewInspection()
        {
            try
            {
                // Publish the new active period before unloading the old one. A write failure leaves both intact.
                MiniGameBootstrap.WriteNewCampaignContinuation(ContinuePath);
                MainMenuContinueStore.StartNewInspection(SaveDirectory);
                completedInspection = false;
                pendingNewInspection = ContinuePath;
                BeginMenuRelease();
                retained = false;
                StartCoroutine(LoadNewInspectionAfterControls());
            }
            catch (Exception exception)
            {
                releasing = false; pendingNewInspection = null;
                View.Show(MainMenuPage.Menu, HasResumeFile());
                StartMenuMusic();
                View.SetStatus("新检查期未开始：" + exception.Message, true);
            }
        }

        private IEnumerator LoadNewInspectionAfterControls()
        {
            // Let the visible button fade finish before scene loading can stall a frame.
            yield return new WaitForSecondsRealtime(.45f);
            try
            {
                MiniGameSessionHost.Release();
                SceneManager.LoadSceneAsync(MiniGameSessionHost.CombinedScene, LoadSceneMode.Single);
            }
            catch (Exception exception)
            {
                releasing = false; pendingNewInspection = null;
                View.Show(MainMenuPage.Menu, HasResumeFile(), "新检查期未开始：" + exception.Message);
                StartMenuMusic();
            }
        }

        public bool TryTakeNewInspection(out string path)
        {
            path = pendingNewInspection;
            pendingNewInspection = null;
            return path != null;
        }

        public void FinishNewInspection(bool success, string error)
        {
            retained = success;
            if (!success) { releasing = false; View.Show(MainMenuPage.Menu, HasResumeFile(), error); StartMenuMusic(); return; }
            Runtime.SetPaused(true);
            CoverGame(true);
            SuspendGameAudio();
            StartCoroutine(ReleaseInputAndResume());
        }

        public void CancelNewInspection()
        {
            if (View.Page == MainMenuPage.ConfirmNew && !releasing) BackToMenu();
        }

        public void CompleteInspectionAndReturnToMenu()
        {
            if (releasing || Runtime == null) return;
            CoverGame(true);
            Runtime.SetPaused(true);
            retained = false;
            SuspendGameAudio();
            completedInspection = true;
            string error = null;
            try { MainMenuContinueStore.MarkCompleted(SaveDirectory); }
            catch (Exception exception) { error = "完成记录保存失败：" + exception.Message; }
            if (MiniGameSessionHost.Current != null
                && !MiniGameSessionHost.Current.TrySaveTo(ContinuePath, out var saveError)) error = saveError;
            View.Show(MainMenuPage.Menu, false, error ?? string.Empty);
            StartMenuMusic();
        }

        public void ShowSettings()
        {
            if (View.Page == MainMenuPage.Menu && !releasing) View.Show(MainMenuPage.Settings, HasRetainedGame || HasResumeFile());
        }

        public void BackToMenu()
        {
            if (!releasing) View.Show(MainMenuPage.Menu, HasRetainedGame || HasResumeFile());
        }

        public void HandleEscape()
        {
            if (releasing || !IsVisible) return;
            if (View.Page == MainMenuPage.Settings || View.Page == MainMenuPage.ConfirmNew) BackToMenu();
        }

        private IEnumerator ReleaseInputAndResume()
        {
            BeginMenuRelease();
            yield return null;
            while (Input.anyKey || MiniGameSessionHost.Current != null && MiniGameSessionHost.Current.IsTransitioning) yield return null;
            var screen = owner != null ? owner.Screen : null;
            bool opening = screen != null && screen.DayZeroOpeningWaiting;
            var display = screen != null ? screen.TerminalDisplay : null;
            bool pullBack = !opening && display != null && Runtime != null && Runtime.GetView().TerminalOpen
                && !screen.TransitionLocked;
            float previousOpening = display != null ? display.OpeningProgress : 1;
            if (pullBack) display.SetOpeningProgress(0);
            screen?.RevealForMainMenuTransition();
            float elapsed = 0;
            while (elapsed < MenuFadeSeconds)
            {
                elapsed += Time.unscaledDeltaTime;
                View.SetReleaseProgress(elapsed, opening || pullBack);
                yield return null;
            }
            menuAudio.StopAll();
            EventSystem.current?.SetSelectedGameObject(null);
            View.Show(MainMenuPage.Hidden, false);
            if (opening)
            {
                ResumeGameAudio();
                Runtime?.SetPaused(false);
                // The title action owns the opening. No second click or synthetic gameplay input is needed.
                if (screen.RequestDayZeroOpeningFromMainMenu())
                    while (owner != null && screen.TransitionLocked) yield return null;
            }
            else if (pullBack)
            {
                const float duration = 2.2f;
                elapsed = 0;
                while (elapsed < duration)
                {
                    elapsed += Time.unscaledDeltaTime;
                    display.SetOpeningProgress(Mathf.Lerp(0, previousOpening, Mathf.SmoothStep(0, 1, elapsed / duration)));
                    yield return null;
                }
                display.SetOpeningProgress(previousOpening);
            }
            // Only the completed presentation releases input; its initiating key cannot enter the dialogue.
            while (Input.anyKey) yield return null;
            ResumeGameAudio();
            CoverGame(false);
            Runtime?.SetPaused(false);
            retained = true;
            releasing = false;
        }

        private void BeginMenuRelease()
        {
            if (releasing) return;
            releasing = true;
            View.BeginRelease();
            menuAudio.Stop(MiniGameAudioChannel.MenuMusic, MenuFadeSeconds);
            menuAudio.Stop(MiniGameAudioChannel.LogoGlitch, .2f);
        }

        private void ResumeGameAudio()
        {
            foreach (var service in suspended) if (service != null) service.SetMenuSuspended(false);
            suspended.Clear();
        }

        private void SuspendGameAudio()
        {
            foreach (var service in FindObjectsOfType<MiniGameAudioService>(true))
                if (service != menuAudio && !suspended.Contains(service))
                { service.SetMenuSuspended(true); suspended.Add(service); }
        }

        private void CoverGame(bool value)
        {
            if (owner != null) owner.Screen.SetMainMenuCovered(value);
            Cursor.lockState = CursorLockMode.None; Cursor.visible = true;
        }

#if UNITY_EDITOR
        public bool TryDismissForEditorDebug()
        {
            // A new-inspection scene load must finish before its old owner can be replaced.
            if (pendingNewInspection != null) return false;
            StopAllCoroutines();
            releasing = false;
            foreach (var service in suspended) if (service != null) service.SetMenuSuspended(false);
            suspended.Clear();
            menuAudio.StopAll();
            View.Show(MainMenuPage.Hidden, false);
            CoverGame(false);
            return true;
        }
#endif

        private void StartMenuMusic()
        {
            menuAudio.SetLoop(MiniGameAudioChannel.MenuMusic, MiniGameSound.StartMenu, .3f);
            Cursor.lockState = CursorLockMode.None; Cursor.visible = true;
        }

        private bool HasResumeFile() => !completedInspection && MainMenuContinueStore.ResolveResumePath(SaveDirectory) != null;
        private void Update()
        {
            if (!IsVisible || releasing) return;
            if (View.Page == MainMenuPage.FirstStart && Input.anyKeyDown) StartFirstInspection();
            else if (Input.GetKeyDown(KeyCode.Escape)) HandleEscape();
            else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.UpArrow)) View.SelectInitialForNavigation(true);
            else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.Tab)
                || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) View.SelectInitialForNavigation(false);
        }

        private void LateUpdate()
        {
            if (BlocksGameplayInput) { Cursor.lockState = CursorLockMode.None; Cursor.visible = true; }
        }

        private void OnDestroy()
        {
            foreach (var service in suspended) if (service != null) service.SetMenuSuspended(false);
            if (Current == this) Current = null;
        }
    }
}
