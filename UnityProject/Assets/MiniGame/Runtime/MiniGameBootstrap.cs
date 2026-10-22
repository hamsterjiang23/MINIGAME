using System;
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
        { enableDreamSequence = true; dreamProfile = profile; rebellionProfile = rebellion; }
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
            else if (MainMenu != null && MainMenu.TryTakeNewInspection(out var continuePath))
            {
                bool loaded = TryLoadCampaign(continuePath, out var error);
                MainMenu.FinishNewInspection(loaded, error);
            }
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
            if (view.Paused) { if (!screen.TryClosePauseSettings() && !screen.TryClosePauseManual()) Runtime.SetPaused(false); }
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
            if (installSession)
            {
                if (runtime.UsesDreamFlow) MiniGameSessionHost.Install(runtime, rules, content, dreamProfile);
                else MiniGameSessionHost.Release();
            }
            Audio = GetComponent<MiniGameAudioDirector>();
            if (Audio == null) Audio = gameObject.AddComponent<MiniGameAudioDirector>();
            Audio.ConfigureDreams(dreamProfile);
            Audio.Bind(runtime);
            if (MiniGameSessionHost.Current != null && MiniGameSessionHost.Current.Runtime == runtime)
                Audio.RestoreSupervisorScanDays(MiniGameSessionHost.Current.SupervisorScanDays);
            screen.BindAudio(Audio);
            screen.Bind(runtime);
            var roomLighting = GetComponent<RoomLightingController>();
            if (roomLighting == null)
                foreach (var candidate in FindObjectsOfType<RoomLightingController>())
                    if (candidate.gameObject.scene == gameObject.scene) { roomLighting = candidate; break; }
            screen.BindLighting(runtime, roomLighting);
            player.Bind(runtime, screen);
            var retro = player.ViewCamera.GetComponent<RetroScreenEffect>();
            if (retro != null) retro.UsePlayerPixelSettings = true;
            PlayerBrightnessEffect.EnsureFor(player.ViewCamera);
            effects.Bind(runtime, screen);
            var monitor = GetComponent<SelfReviewMonitor>();
            if (monitor == null && runtime.UsesDreamFlow) monitor = gameObject.AddComponent<SelfReviewMonitor>();
            if (monitor != null)
            {
                monitor.Configure(player); monitor.Bind(runtime); screen.BindSelfMonitor(monitor.Output);
            }
            foreach (var door in FindObjectsOfType<HingedDoor>())
                if (door.gameObject.scene == gameObject.scene) door.Bind(runtime);
            if (doorView != null) doorView.Bind(runtime);
            var projection = player.ViewCamera.GetComponent<ReferenceCameraProjection>();
            if (projection != null) projection.Bind(runtime);
            var supervisor = GetComponent<SupervisorDoorwayView>();
            if (supervisor == null) supervisor = gameObject.AddComponent<SupervisorDoorwayView>();
            supervisor.Bind(runtime, player, doorView);
            if (Dreams != null) Dreams.Bind(runtime, screen, player, Audio, dreamProfile);
            screen.UseYarnPresentation();
            yarn.Bind(runtime, screen, supervisor);
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
            effects.RestoreSleepCheckpoint(session.SleepCheckpoint);
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
            var compiled = DialoguePackageCompiler.Compile(selected.PackageSource.text);
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
