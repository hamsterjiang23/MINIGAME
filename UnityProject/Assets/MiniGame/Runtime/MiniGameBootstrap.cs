using System;
using System.Collections.Generic;
using System.IO;
using MiniGame.Core;
using MiniGame.Presentation;
using MiniGame.Presentation.Authoring;
using UnityEngine;
using Yarn.Unity;

namespace MiniGame.UnityHost
{
    /// <summary>The only Unity owner of a runtime. Widgets never keep a second rule state.</summary>
    public sealed class MiniGameBootstrap : MonoBehaviour
    {
        [SerializeField] private MiniGameScreen screen;
        [SerializeField] private ApartmentPlayer player;
        [SerializeField] private PresentationEffects effects;
        [SerializeField] private MiniGameYarnBridge yarn;
        [SerializeField] private bool useTerminalDemo = true;
        [SerializeField] private bool useCampaign;
        [SerializeField] private DialoguePackageAsset dialoguePackage;
        [SerializeField] private bool enableDreamSequence;
        [SerializeField] private DreamSequenceProfile dreamProfile;
        [SerializeField] private PlayerRebellionProfile rebellionProfile;
        private GameRules rules;
        private IGameContent content;
        private YarnProject legacyProject;
        private bool terminalDemoActive;
        private bool campaignActive;
        private RoomInteractionSettings interactionSettings;
        private DialoguePackageAsset warmedPackageAsset;
        private TextAsset warmedPackageText;
        private string warmedPackageHash;
        private CompiledDialoguePackage warmedPackage;
        private PreparedCampaign preparedCampaign;
        private bool titlePresentationPrewarmed;
        private sealed class PreparedCampaign
        {
            public GameRuntime Runtime;
            public GameRules Rules;
            public CompiledDialoguePackage Package;
            public string Checkpoint, Settings;
        }
        private OpeningPreparationTrace openingPreparation;
        internal string OpeningPreparationTimingJson => openingPreparation != null ? JsonUtility.ToJson(openingPreparation, true) : null;

        [Serializable] private sealed class OpeningPreparationTrace
        {
            public string result;
            public double totalMilliseconds;
            public List<OpeningPreparationStep> steps = new List<OpeningPreparationStep>(32);
            private readonly System.Diagnostics.Stopwatch watch = System.Diagnostics.Stopwatch.StartNew();
            private double previous;
            public void Mark(string stage)
            {
                if (!watch.IsRunning) return;
                double now = watch.Elapsed.TotalMilliseconds;
                steps.Add(new OpeningPreparationStep { stage = stage, milliseconds = now - previous, elapsedMilliseconds = now });
                previous = totalMilliseconds = now;
            }
            public void Finish(string value) { Mark("menu_ready"); result = value; watch.Stop(); }
        }

        [Serializable] private struct OpeningPreparationStep
        {
            public string stage;
            public double milliseconds, elapsedMilliseconds;
        }

        internal void MarkOpeningPreparation(string stage) => openingPreparation?.Mark(stage);
        internal void FinishOpeningPreparation(string result) => openingPreparation?.Finish(result);
        public GameRuntime Runtime { get; private set; }
        public MiniGameScreen Screen => screen;
        public ApartmentPlayer Player => player;
        public PresentationEffects Effects => effects;
        public MiniGameAudioDirector Audio { get; private set; }
        public DreamSequenceDirector Dreams { get; private set; }
        public DreamSequenceProfile DreamProfile => dreamProfile;
        public MainMenuController MainMenu { get; private set; }
        public bool TerminalDemoActive => terminalDemoActive;
        public bool CampaignActive => campaignActive;
        public string SavePath => Runtime != null && Runtime.UsesDreamFlow && MiniGameSessionHost.Current != null
            ? MiniGameSessionHost.Current.SavePath : Path.Combine(SaveDirectory,
            campaignActive ? "campaign-save.json" : terminalDemoActive ? "terminal-demo-save.json" : "prototype-save.json");

        internal static string SaveDirectory
        {
            get
            {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
                if (!string.IsNullOrEmpty(TerminalDossierPlayerProbe.IsolatedSaveDirectory))
                    return TerminalDossierPlayerProbe.IsolatedSaveDirectory;
                if (!string.IsNullOrEmpty(PixelStrengthPlayerProbe.IsolatedSaveDirectory))
                    return PixelStrengthPlayerProbe.IsolatedSaveDirectory;
                if (!string.IsNullOrEmpty(TerminalCommandPlayerProbe.IsolatedSaveDirectory))
                    return TerminalCommandPlayerProbe.IsolatedSaveDirectory;
#endif
#if UNITY_EDITOR
                return EditorCampaignDebugSession.SaveDirectory ?? Application.persistentDataPath;
#else
                return Application.persistentDataPath;
#endif
            }
        }

        public void Configure(MiniGameScreen ui, ApartmentPlayer worldPlayer, PresentationEffects presentation, MiniGameYarnBridge bridge)
        { screen = ui; player = worldPlayer; effects = presentation; yarn = bridge; }
        public void ConfigureDreams(DreamSequenceProfile profile, PlayerRebellionProfile rebellion)
        { enableDreamSequence = true; dreamProfile = profile; rebellionProfile = rebellion; preparedCampaign = null; }
        public void ConfigureRoomInteractions(RoomInteractionSettings settings)
        {
            interactionSettings = settings;
            ApplyRoomInteractionSettings();
        }

        private void ApplyRoomInteractionSettings()
        {
            if (Runtime == null) return;
            if (interactionSettings == null)
                interactionSettings = Resources.Load<RoomInteractionSettings>(RoomInteractionSettings.ResourcePath);
            Runtime.ConfigureDoorLockAfterSupervisor(interactionSettings == null || interactionSettings.LockDoorAfterSupervisor);
        }

        private void Awake()
        {
            legacyProject = yarn.Project;
            screen.SaveRequested += Save;
            screen.LoadRequested += Load;
            screen.RestartRequested += Restart;
            screen.MainMenuRequested += ReturnToMainMenu;
            if (useCampaign) MainMenu = MainMenuController.Attach(this);
            if (MiniGameSessionHost.Current != null && MiniGameSessionHost.Current.PendingCombinedRestore)
                RestoreFromSession(MiniGameSessionHost.Current);
            else if (useCampaign && !MainMenuController.SkipStartup)
            {
                MainMenu.ShowStartup();
                return;
            }
            else if (useCampaign) StartCampaign();
            else if (useTerminalDemo)
            {
                try { StartTerminalDemo(); }
                catch (Exception exception)
                {
                    Debug.LogWarning("Terminal demo unavailable: " + exception.Message);
                    StartLegacyPrototype();
                    screen.Notify("终端内容包未就绪，请使用 MiniGame 菜单导入；当前为原三天开发配置。", true);
                }
            }
            else StartLegacyPrototype();
            if (Runtime != null) Debug.Log("MINIGAME_PLAYER_READY unity=" + Application.unityVersion + " phase=" + Runtime.GetView().Game.Phase
                + " profile=" + (campaignActive ? "campaign" : terminalDemoActive ? "terminal-demo" : "legacy-three-day") + " rules=" + rules.RulesId);
        }

        private void OnDestroy()
        {
            if (screen == null) return;
            screen.SaveRequested -= Save;
            screen.LoadRequested -= Load;
            screen.RestartRequested -= Restart;
            screen.MainMenuRequested -= ReturnToMainMenu;
        }

        private void ReturnToMainMenu() => MainMenu?.ReturnToMenu();

        private void Update()
        {
            if (MainMenuController.BlocksGameplayInput) return;
            if (Runtime == null || (MiniGameSessionHost.Current != null && MiniGameSessionHost.Current.IsTransitioning)) return;
            ApplyRoomInteractionSettings();
            if (screen != null && screen.PresentationToolsBlockInput) return;
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                HandleEscape(); return;
            }
            if (!screen.PauseSubmenuVisible && Input.GetKeyDown(KeyCode.F5)) Save();
            if (!screen.PauseSubmenuVisible && Input.GetKeyDown(KeyCode.F9)) Load();
            if (MiniGameSessionHost.Current != null && MiniGameSessionHost.Current.IsTransitioning) return;
        }

        public void HandleEscape()
        {
            if (MainMenu != null && MainMenu.IsVisible) { MainMenu.HandleEscape(); return; }
            if (MainMenuController.BlocksGameplayInput) return;
            if (Runtime == null || screen.PresentationToolsBlockInput
                || MiniGameSessionHost.Current != null && MiniGameSessionHost.Current.IsTransitioning) return;
            var view = Runtime.GetView();
            if (view.Paused) { if (!screen.TryClosePauseGm() && !screen.TryClosePauseSettings() && !screen.TryClosePauseManual()) Runtime.SetPaused(false); }
            else if (screen.TryClosePropNarration()) { }
            else if (screen.TryCloseHistory()) { }
            else if (view.Inspection != null) Runtime.CloseInspection();
            else if (view.Input == InputContext.TerminalDirectory && view.TerminalPage == TerminalPage.Directory && !view.IsSelfReview)
                Runtime.LeaveTerminal();
            else Runtime.SetPaused(true);
        }

        private void InstallRuntime(GameRuntime runtime, bool installSession = true, bool prepared = false)
        {
            GetComponent<SupervisorDoorwayView>()?.PrepareForRuntimeChange();
            Dreams = GetComponent<DreamSequenceDirector>();
            if (!prepared && Dreams != null) Dreams.PrepareForRuntimeChange();
            var doorView = player.ViewCamera.GetComponent<DoorViewCamera>();
            if (!prepared && doorView != null) doorView.PrepareForRuntimeChange(runtime);
            Runtime = runtime;
            ApplyRoomInteractionSettings();
            MarkOpeningPreparation("install.prepare_previous_adapters_and_interactions");
            if (installSession)
            {
                if (runtime.UsesDreamFlow) MiniGameSessionHost.Install(runtime, rules, content, dreamProfile);
                else MiniGameSessionHost.Release();
            }
            MarkOpeningPreparation("install.session");
            Audio = GetComponent<MiniGameAudioDirector>();
            if (Audio == null) Audio = gameObject.AddComponent<MiniGameAudioDirector>();
            Audio.ConfigureDreams(dreamProfile);
            Audio.Bind(runtime);
            if (MiniGameSessionHost.Current != null && MiniGameSessionHost.Current.Runtime == runtime)
                Audio.RestoreSupervisorScanDays(MiniGameSessionHost.Current.SupervisorScanDays);
            MarkOpeningPreparation("install.audio");
            screen.BindAudio(Audio);
            screen.Bind(runtime);
            MarkOpeningPreparation("install.screen");
            var roomLighting = GetComponent<RoomLightingController>();
            if (roomLighting == null)
                foreach (var candidate in FindObjectsOfType<RoomLightingController>())
                    if (candidate.gameObject.scene == gameObject.scene) { roomLighting = candidate; break; }
            MarkOpeningPreparation("install.lighting_lookup");
            Action<string> preparationMarker = openingPreparation != null ? MarkOpeningPreparation : (Action<string>)null;
            screen.BindLighting(runtime, roomLighting, preparationMarker);
            MarkOpeningPreparation("install.lighting_panel");
            player.Bind(runtime, screen);
            MarkOpeningPreparation("install.player");
            var retro = player.ViewCamera.GetComponent<RetroScreenEffect>();
            if (retro != null) retro.UsePlayerPixelSettings = true;
            PlayerBrightnessEffect.EnsureFor(player.ViewCamera);
            MarkOpeningPreparation("install.lighting_player_brightness");
            effects.Bind(runtime, screen, preparationMarker);
            MarkOpeningPreparation("install.effects");
            var monitor = GetComponent<SelfReviewMonitor>();
            if (monitor == null && runtime.UsesDreamFlow) monitor = gameObject.AddComponent<SelfReviewMonitor>();
            if (monitor != null)
            {
                monitor.Configure(player); monitor.Bind(runtime); screen.BindSelfMonitor(monitor.Output);
            }
            MarkOpeningPreparation("install.self_monitor");
            foreach (var door in FindObjectsOfType<HingedDoor>())
                if (door.gameObject.scene == gameObject.scene) door.Bind(runtime);
            if (doorView != null) doorView.Bind(runtime);
            var projection = player.ViewCamera.GetComponent<ReferenceCameraProjection>();
            if (projection != null) projection.Bind(runtime);
            MarkOpeningPreparation("install.doors_projection");
            var supervisor = GetComponent<SupervisorDoorwayView>();
            if (supervisor == null) supervisor = gameObject.AddComponent<SupervisorDoorwayView>();
            supervisor.Bind(runtime, player, doorView);
            MarkOpeningPreparation("install.supervisor");
            if (Dreams != null) Dreams.Bind(runtime, screen, player, Audio, dreamProfile);
            MarkOpeningPreparation("install.dreams");
            screen.UseYarnPresentation();
            yarn.Bind(runtime, screen, supervisor);
            MarkOpeningPreparation("install.yarn_binding");
        }

        public void RestoreFromSession(MiniGameSessionHost session)
        {
            rules = session.Rules; content = session.Content; dreamProfile = session.DreamProfile;
            campaignActive = terminalDemoActive = true;
            var package = Resources.Load<DialoguePackageAsset>("MiniGame/ActiveCampaignPackage");
            GetComponent<SupervisorDoorwayView>()?.PrepareForRuntimeChange();
            yarn.Configure(package.Project);
            GetComponent<DreamSequenceDirector>()?.PrepareForRuntimeChange();
            player.ViewCamera.GetComponent<DoorViewCamera>()?.PrepareForRuntimeChange(session.Runtime);
            effects.PrepareForRuntimeChange();
            session.RestoreRoomPose(player);
            InstallRuntime(session.Runtime, false, true);
            yarn.RestoreSupervisorCheckpoint(session.SupervisorCheckpoint);
            if (Dreams != null && session.DreamCheckpoint != null) Dreams.RestoreCheckpoint(session.DreamCheckpoint);
            if (!effects.RestoreSleepCheckpoint(session.SleepCheckpoint) && session.Runtime.RestoredFromAfterDream)
                effects.RestoreLegacyDreamWake();
        }

        public void EnterGmEnding(EndingType ending)
        {
            var session = MiniGameSessionHost.Current;
            string error = null;
            if (session == null || !session.TryEnterGmEnding(ending, out error))
                screen.Notify(session == null ? "当前流程不支持 GM 结局。" : error, true);
        }

        public void Save()
        {
            if (Runtime.UsesDreamFlow && MiniGameSessionHost.Current != null)
            {
                MiniGameSessionHost.Current.Save();
                screen.Notify(MiniGameSessionHost.Current.LastMessage, false);
                return;
            }
            try
            {
                new RuntimeFileSaveStore(SavePath).Save(Runtime);
                screen.Notify("检查点已保存。F9 或暂停菜单恢复。", false);
            }
            catch (Exception exception)
            { screen.Notify("保存失败：" + exception.Message, true); Debug.LogException(exception); }
        }

        public void Load()
        {
            if (Runtime.UsesDreamFlow && MiniGameSessionHost.Current != null)
            {
                MiniGameSessionHost.Current.Load();
                screen.Notify(MiniGameSessionHost.Current.LastMessage, false);
                return;
            }
            try
            {
                var restored = new RuntimeFileSaveStore(SavePath).Load(rules, content);
                InstallRuntime(restored);
                screen.Notify("检查点已恢复；未重复结算回答或处置。", false);
            }
            catch (Exception exception)
            { screen.Notify("恢复失败：" + exception.Message, true); Debug.LogWarning(exception.Message); }
        }

        public void StartTerminalDemo(DialoguePackageAsset package = null)
        {
            PrepareContent(package);
            InstallRuntime(new GameRuntime(rules, content, useTerminalDirectory: true));
            if (campaignActive) Runtime.BeginDayZeroOpening();
        }

        private void PrepareContent(DialoguePackageAsset package)
        {
            var selected = package != null ? package : dialoguePackage != null ? dialoguePackage
                : Resources.Load<DialoguePackageAsset>("MiniGame/ActiveDialoguePackage");
            if (selected == null || selected.PackageSource == null || selected.Project == null)
                throw new InvalidOperationException("No validated terminal dialogue package is active.");
            var compiled = CompilePreparedPackage(selected);
            if (compiled.SourceHash != selected.SourceHash || selected.Project.Program == null)
                throw new InvalidOperationException("The active dialogue source and compiled Yarn project do not match.");
            rules = compiled.Rules; content = compiled.Content; terminalDemoActive = true;
            campaignActive = compiled.Package.FormatVersion == 2;
            if (campaignActive && enableDreamSequence && dreamProfile != null && dreamProfile.EnableDreamFlow)
            {
                if (rebellionProfile == null) rebellionProfile = Resources.Load<PlayerRebellionProfile>("MiniGame/PlayerRebellion");
                if (rebellionProfile == null) throw new InvalidOperationException("缺少玩家反叛值配置，请在梦境与结局面板完成场景接入。");
                rules = rules.WithDreamFlow(rebellionProfile.InitialValue, rebellionProfile.Maximum,
                    rebellionProfile.Threshold, rebellionProfile.CreateDeltas());
            }
            yarn.Configure(selected.Project);
        }

        private CompiledDialoguePackage CompilePreparedPackage(DialoguePackageAsset package)
        {
            if (warmedPackage == null || warmedPackageAsset != package || warmedPackageText != package.PackageSource
                || warmedPackageHash != package.SourceHash)
            {
                warmedPackage = DialoguePackageCompiler.Compile(package.PackageSource.text);
                warmedPackageAsset = package; warmedPackageText = package.PackageSource; warmedPackageHash = package.SourceHash;
                preparedCampaign = null;
                titlePresentationPrewarmed = false;
            }
            return warmedPackage;
        }

        public void PrewarmTitleAssets()
        {
            var package = Resources.Load<DialoguePackageAsset>("MiniGame/ActiveCampaignPackage");
            if (package == null || package.PackageSource == null || package.Project == null)
                throw new InvalidOperationException("缺少正式对话配置。");
            if (CompilePreparedPackage(package).SourceHash != package.SourceHash || package.Project.Program == null)
                throw new InvalidOperationException("对话源与编译结果不一致。");
            PrepareCampaignSeed(warmedPackage);
            if (Runtime == null && !titlePresentationPrewarmed)
            {
                // These objects have no active runtime, session, dialogue task or file output while the title is visible.
                yarn.Configure(package.Project);
                yarn.PrewarmPresentation();
                var lighting = GetComponent<RoomLightingController>();
                if (lighting == null)
                    foreach (var candidate in FindObjectsOfType<RoomLightingController>())
                        if (candidate.gameObject.scene == gameObject.scene) { lighting = candidate; break; }
                screen.BindLighting(null, lighting);
                effects.PrewarmPresentation();
                screen.PrewarmTitlePresentation(true, preparedCampaign.Runtime.GetView());
                titlePresentationPrewarmed = true;
            }
        }

        private void PrepareCampaignSeed(CompiledDialoguePackage compiled)
        {
            bool dreams = enableDreamSequence && dreamProfile != null && dreamProfile.EnableDreamFlow;
            if (dreams && rebellionProfile == null) rebellionProfile = Resources.Load<PlayerRebellionProfile>("MiniGame/PlayerRebellion");
            if (dreams && rebellionProfile == null) throw new InvalidOperationException("缺少玩家反叛值配置。");
            string settings = dreams ? JsonUtility.ToJson(rebellionProfile) : "no-dream-flow";
            if (preparedCampaign != null && preparedCampaign.Package == compiled && preparedCampaign.Settings == settings) return;
            var nextRules = dreams ? compiled.Rules.WithDreamFlow(rebellionProfile.InitialValue, rebellionProfile.Maximum,
                rebellionProfile.Threshold, rebellionProfile.CreateDeltas()) : compiled.Rules;
            var next = new GameRuntime(nextRules, compiled.Content, true);
            if (next.UsesEndingFlow) next.ConfigureEndings(EndingDialoguePresenter.LoadCatalog());
            next.BeginDayZeroOpening(); next.SetPaused(true);
            preparedCampaign = new PreparedCampaign
                { Runtime = next, Rules = nextRules, Package = compiled, Checkpoint = next.Save(), Settings = settings };
        }

        public bool TryPrepareNewCampaign(string continuePath, out string error)
        {
            error = null;
            openingPreparation = !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("MINIGAME_OPENING_PROBE_LABEL"))
                && Array.IndexOf(Environment.GetCommandLineArgs(), "--opening-motion-probe") >= 0 ? new OpeningPreparationTrace() : null;
            try
            {
                PrewarmTitleAssets();
                MarkOpeningPreparation("title_asset_and_render_prewarm");
                var package = Resources.Load<DialoguePackageAsset>("MiniGame/ActiveCampaignPackage");
                var prepared = preparedCampaign;
                var compiled = prepared.Package;
                var next = prepared.Runtime;
                MarkOpeningPreparation("reuse_prepared_content_and_seed");
                // Save before replacing the retained period. No scene load is needed to reset its adapters.
                new RuntimeFileSaveStore(continuePath).SaveSerialized(prepared.Checkpoint);
                MainMenuContinueStore.StartNewInspection(Path.GetDirectoryName(continuePath));
                preparedCampaign = null;
                MarkOpeningPreparation("prepared_seed_atomic_save");
                MiniGameSessionHost.Release();
                effects.PrepareForRuntimeChange();
                MarkOpeningPreparation("release_previous_session");
                rules = prepared.Rules; content = compiled.Content; campaignActive = terminalDemoActive = true;
                yarn.Configure(package.Project);
                MarkOpeningPreparation("yarn_configuration");
                InstallRuntime(next);
                screen.PrewarmTitlePresentation(true);
                MarkOpeningPreparation("bound_tutorial_render_prewarm");
                yarn.PrewarmPresentation();
                MarkOpeningPreparation("yarn_runner_prewarm");
                return true;
            }
            catch (Exception exception)
            { FinishOpeningPreparation("failed"); error = "新检查期未开始：" + exception.Message; return false; }
        }

        public bool TryLoadCampaign(string path, out string error)
        {
            error = null;
            try
            {
                PrepareContent(Resources.Load<DialoguePackageAsset>("MiniGame/ActiveCampaignPackage"));
                var session = MiniGameSessionHost.Install(new GameRuntime(rules, content, true), rules, content, dreamProfile);
                if (session.TryLoadFrom(path, out error, pauseOnLoad: true)) return true;
                MiniGameSessionHost.Release();
                return false;
            }
            catch (Exception exception) { error = "读取失败：" + exception.Message; return false; }
        }

        public static void WriteNewCampaignContinuation(string path)
        {
            var package = Resources.Load<DialoguePackageAsset>("MiniGame/ActiveCampaignPackage");
            if (package == null || package.PackageSource == null || package.Project == null)
                throw new InvalidOperationException("缺少正式对话配置。");
            var compiled = DialoguePackageCompiler.Compile(package.PackageSource.text);
            if (compiled.SourceHash != package.SourceHash || package.Project.Program == null)
                throw new InvalidOperationException("对话源与编译结果不一致。");
            var rules = compiled.Rules;
            var dream = Resources.Load<DreamSequenceProfile>("MiniGame/DreamSequenceDefault");
            if (dream != null && dream.EnableDreamFlow)
            {
                var rebellion = Resources.Load<PlayerRebellionProfile>("MiniGame/PlayerRebellion");
                if (rebellion == null) throw new InvalidOperationException("缺少反叛值配置。");
                rules = rules.WithDreamFlow(rebellion.InitialValue, rebellion.Maximum, rebellion.Threshold, rebellion.CreateDeltas());
            }
            var seed = new GameRuntime(rules, compiled.Content, true);
            if (seed.UsesEndingFlow) seed.ConfigureEndings(EndingDialoguePresenter.LoadCatalog());
            seed.BeginDayZeroOpening();
            seed.SetPaused(true);
            new RuntimeFileSaveStore(path).Save(seed);
        }

        public void StartLegacyPrototype()
        {
            if (legacyProject == null) legacyProject = yarn.Project;
            rules = PrototypeRules.Create(); content = new PrototypeContent(rules); terminalDemoActive = false;
            campaignActive = false;
            if (legacyProject != null) yarn.Configure(legacyProject);
            InstallRuntime(new GameRuntime(rules, content));
        }

        public void Restart()
        {
            if (MainMenu != null) { MainMenu.RequestNewInspection(); return; }
            if (Runtime.UsesDreamFlow && MiniGameSessionHost.Current != null) MiniGameSessionHost.Current.RestartToCombined();
            else
            {
                InstallRuntime(new GameRuntime(rules, content, terminalDemoActive));
                if (campaignActive) Runtime.BeginDayZeroOpening();
            }
        }
        public void RestoreCheckpoint(string json) => InstallRuntime(GameRuntime.Restore(json, rules, content,
            rules.DreamFlowEnabled ? EndingDialoguePresenter.LoadCatalog() : null));

        public void StartCampaign()
        {
            var package = Resources.Load<DialoguePackageAsset>("MiniGame/ActiveCampaignPackage");
            if (package == null) throw new InvalidOperationException("Import the full campaign with MiniGame/Campaign/Import and activate first.");
            StartTerminalDemo(package);
            if (!campaignActive) throw new InvalidOperationException("The campaign resource must contain a validated V2 package.");
        }
    }
}
