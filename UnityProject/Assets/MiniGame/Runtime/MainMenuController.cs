using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using MiniGame.Presentation;
using UnityEngine;
using UnityEngine.EventSystems;
using MiniGame.Core;

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
        private int pixelSetupConfirmedFrame = -1;
        public const float ControlsFadeSeconds = .45f;
        public const float MonitorPullbackSeconds = 2.2f;
        public float TransitionElapsed { get; private set; }
        private MiniGameAudioService transitionAudio;
        private DreamEndingController transitionEnding;
        private readonly List<MiniGameAudioChannel> transitionChannels = new List<MiniGameAudioChannel>(2);
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
                () => Click(BackToMenu), () => Click(ConfirmPixelSetup));
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
            View.Show(PixelStrengthOnboarding.ShouldShow(SaveDirectory) ? MainMenuPage.PixelSetup
                : MainMenuContinueStore.ResolveResumePath(SaveDirectory) == null && !MainMenuContinueStore.HasCompletedInspection(SaveDirectory)
                    ? MainMenuPage.FirstStart : MainMenuPage.Menu, HasResumeFile());
            StartMenuMusic();
        }

        public void ConfirmPixelSetup()
        {
            if (releasing || View.Page != MainMenuPage.PixelSetup) return;
            try { PixelStrengthOnboarding.Confirm(View.PixelSetup.SelectedDetail); }
            catch (Exception exception) { View.SetStatus("设置保存失败：" + exception.Message, true); return; }
            pixelSetupConfirmedFrame = Time.frameCount;
            View.Show(MainMenuPage.FirstStart, false);
        }

        public bool HandlePixelSetupKey(KeyCode key)
        {
            if (releasing || View.Page != MainMenuPage.PixelSetup) return false;
            if (key == KeyCode.LeftArrow || key == KeyCode.UpArrow) View.PixelSetup.MoveSelection(-1);
            else if (key == KeyCode.RightArrow || key == KeyCode.DownArrow || key == KeyCode.Tab) View.PixelSetup.MoveSelection(1);
            else if (key == KeyCode.Return || key == KeyCode.KeypadEnter) Click(ConfirmPixelSetup);
            else if (key != KeyCode.Escape) return false;
            return true;
        }

        private IEnumerator PrewarmTitle()
        {
            yield return null;
            if (owner == null || releasing) yield break;
            try { owner.PrewarmTitleAssets(); }
            catch (Exception exception) { View.SetStatus(exception.Message, true); }
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
            if (View.Page != MainMenuPage.FirstStart || releasing || pixelSetupConfirmedFrame == Time.frameCount) return;
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
            if (owner == null) { View.SetStatus("当前场景不能开始检查期。", true); return; }
            releasing = true;
            if (!owner.TryPrepareNewCampaign(ContinuePath, out var error))
            { releasing = false; View.SetStatus(error, true); return; }
            completedInspection = false; retained = true;
            CoverGame(true);
            SuspendGameAudio();
            StartCoroutine(ReleaseInputAndResume());
            owner.FinishOpeningPreparation("ready_for_menu_release");
        }

        public void CancelNewInspection()
        {
            if (View.Page == MainMenuPage.ConfirmNew && !releasing) BackToMenu();
        }

        public void CompleteInspectionAndReturnToMenu(bool playEndNote = false)
        {
            if (releasing || Runtime == null) return;
            bool firstCompletion = !completedInspection;
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
            if (playEndNote && firstCompletion)
                menuAudio.PlayOneShot(MiniGameAudioChannel.EndNote, MiniGameSound.EndNote);
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
            releasing = true;
            TransitionElapsed = 0;
            yield return null;
            while (Input.anyKey || MiniGameSessionHost.Current != null && MiniGameSessionHost.Current.IsTransitioning) yield return null;
            var screen = owner != null ? owner.Screen : null;
            bool opening = screen != null && screen.DayZeroOpeningWaiting;
            var display = screen != null ? screen.TerminalDisplay : null;
            bool pullBack = !opening && display != null && Runtime != null && Runtime.GetView().TerminalOpen
                && !screen.TransitionLocked;
            bool monitor = opening || pullBack;
            float previousOpening = display != null ? display.OpeningProgress : 1;
            RectTransform projectedLogo = null;
            Rect titleLogo = View.LogoScreenRect, projectedStart = titleLogo;
            if (monitor)
            {
                if (opening) owner.Effects.BeginMenuOpeningPullback();
                projectedLogo = screen.PrepareTitleLogoProjection(opening);
                UnityEngine.Canvas.ForceUpdateCanvases();
                display.BeginTitlePullback(titleLogo, projectedLogo);
                projectedStart = display.ScreenRectFor(projectedLogo);
            }
            screen?.RevealForMainMenuTransition();
            display?.RenderFrame();
            // No loading, compilation or new render targets below this point: one clock owns the visible handoff.
            BeginMenuRelease();
            float duration = ControlsFadeSeconds + (monitor ? MonitorPullbackSeconds : .2f);
            while (TransitionElapsed < duration)
            {
                TransitionElapsed = Mathf.Min(duration, TransitionElapsed + Time.unscaledDeltaTime);
                float blend = Mathf.Clamp01(TransitionElapsed / ControlsFadeSeconds);
                menuAudio.SetMenuMusicTransitionGain(MiniGameAudioChannel.MenuMusic, Mathf.Cos(blend * Mathf.PI * .5f));
                foreach (var channel in transitionChannels)
                    transitionAudio?.SetMenuMusicTransitionGain(channel, Mathf.Sin(blend * Mathf.PI * .5f));
                if (blend >= 1) menuAudio.Stop(MiniGameAudioChannel.MenuMusic);
                float progress = Mathf.Clamp01((TransitionElapsed - ControlsFadeSeconds) / MonitorPullbackSeconds);
                if (monitor && TransitionElapsed > ControlsFadeSeconds)
                {
                    if (opening) owner.Effects.ApplyMenuOpeningPullback(progress);
                    else display.SetOpeningProgress(Mathf.SmoothStep(0, previousOpening, progress));
                    var projected = display.ScreenRectFor(projectedLogo);
                    float correction = 1 - Mathf.SmoothStep(0, 1, progress);
                    projected.position += (titleLogo.position - projectedStart.position) * correction;
                    projected.size += (titleLogo.size - projectedStart.size) * correction;
                    View.SetProjectedLogoRect(projected);
                    if (!opening) View.SetLogoAlpha(1 - Mathf.SmoothStep(0, 1, (progress - .7f) / .3f));
                }
                View.SetReleaseProgress(TransitionElapsed, monitor);
                yield return null;
            }
            menuAudio.Stop(MiniGameAudioChannel.MenuMusic);
            menuAudio.EndMenuMusicTransition(MiniGameAudioChannel.MenuMusic);
            if (monitor) display.EndTitlePullback(opening ? 1 : previousOpening);
            EventSystem.current?.SetSelectedGameObject(null);
            // Only the completed presentation releases input; its initiating key cannot enter the dialogue.
            while (Input.anyKey) yield return null;
            View.Show(MainMenuPage.Hidden, false);
            ResumeGameAudio();
            CoverGame(false);
            Runtime?.SetPaused(false);
            transitionEnding?.RefreshPausePresentation();
            EndIncomingMusicTransition();
            if (opening) owner.Effects.CompleteMenuOpeningPullback();
            screen?.FinishTitleLogoProjection();
            retained = true;
            releasing = false;
        }

        private void BeginMenuRelease()
        {
            releasing = true;
            View.BeginRelease();
            menuAudio.BeginMenuMusicTransition(MiniGameAudioChannel.MenuMusic);
            menuAudio.SetMenuMusicTransitionGain(MiniGameAudioChannel.MenuMusic, 1);
            transitionEnding = FindObjectOfType<DreamEndingController>();
            if (transitionEnding != null && transitionEnding.isActiveAndEnabled && transitionEnding.Audio != null
                && transitionEnding.Audio.isActiveAndEnabled)
            {
                transitionAudio = transitionEnding.Audio;
                BeginIncomingMusicChannel(MiniGameAudioChannel.EndingMusic);
                BeginIncomingMusicChannel(MiniGameAudioChannel.Ocean);
            }
            else
            {
                transitionEnding = null;
                transitionAudio = owner != null ? owner.Audio?.Service : null;
                BeginIncomingMusicChannel(Runtime != null && Runtime.GetView().Game.Phase == GamePhase.Dream
                    ? MiniGameAudioChannel.DreamMusic : MiniGameAudioChannel.GameplayMusic);
                if (transitionChannels.Count == 0) BeginIncomingMusicChannel(MiniGameAudioChannel.Ocean);
            }
            menuAudio.Stop(MiniGameAudioChannel.LogoGlitch, .2f);
        }

        private void BeginIncomingMusicChannel(MiniGameAudioChannel channel)
        {
            if (transitionAudio == null || !transitionAudio.isActiveAndEnabled) return;
            var source = transitionAudio.GetSource(channel);
            if (source == null || !source.enabled || source.clip == null) return;
            transitionChannels.Add(channel);
            transitionAudio.BeginMenuMusicTransition(channel);
            transitionAudio.SetMenuMusicTransitionGain(channel, 0);
        }

        private void EndIncomingMusicTransition()
        {
            foreach (var channel in transitionChannels) transitionAudio?.EndMenuMusicTransition(channel);
            transitionChannels.Clear();
            transitionAudio = null;
            transitionEnding = null;
        }

        private void ResumeGameAudio()
        {
            foreach (var service in suspended) if (service != null) service.SetMenuSuspended(false);
            suspended.Clear();
        }

        private void SuspendGameAudio()
        {
            foreach (var service in FindObjectsOfType<MiniGameAudioService>(true))
                if (service != menuAudio)
                {
                    service.SetMenuSuspended(true);
                    if (!suspended.Contains(service)) suspended.Add(service);
                }
        }

        private void CoverGame(bool value)
        {
            if (owner != null) owner.Screen.SetMainMenuCovered(value);
            Cursor.lockState = CursorLockMode.None; Cursor.visible = true;
        }

#if UNITY_EDITOR
        public bool TryDismissForEditorDebug()
        {
            StopAllCoroutines();
            EndIncomingMusicTransition();
            owner?.Screen.TerminalDisplay?.EndTitlePullback();
            owner?.Screen.FinishTitleLogoProjection();
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
            StartCoroutine(PrewarmTitle());
        }

        private bool HasResumeFile() => !completedInspection && MainMenuContinueStore.ResolveResumePath(SaveDirectory) != null;
        private void Update()
        {
            if (!IsVisible || releasing || pixelSetupConfirmedFrame == Time.frameCount) return;
            if (View.Page == MainMenuPage.PixelSetup)
            {
                foreach (var key in new[] { KeyCode.LeftArrow, KeyCode.UpArrow, KeyCode.RightArrow,
                    KeyCode.DownArrow, KeyCode.Tab, KeyCode.Return, KeyCode.KeypadEnter, KeyCode.Escape })
                    if (Input.GetKeyDown(key)) { HandlePixelSetupKey(key); return; }
                return;
            }
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
            EndIncomingMusicTransition();
            foreach (var service in suspended) if (service != null) service.SetMenuSuspended(false);
            if (Current == this) Current = null;
        }
    }
}
