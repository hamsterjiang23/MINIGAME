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
        public GameRuntime Runtime { get; private set; }
        public MiniGameScreen Screen => screen;
        public ApartmentPlayer Player => player;
        public PresentationEffects Effects => effects;
        public MiniGameAudioDirector Audio { get; private set; }
        public DreamSequenceDirector Dreams { get; private set; }
        public DreamSequenceProfile DreamProfile => dreamProfile;
        public bool TerminalDemoActive => terminalDemoActive;
        public bool CampaignActive => campaignActive;
        public string SavePath => Runtime != null && Runtime.UsesDreamFlow && MiniGameSessionHost.Current != null
            ? MiniGameSessionHost.Current.SavePath : Path.Combine(Application.persistentDataPath,
            campaignActive ? "campaign-save.json" : terminalDemoActive ? "terminal-demo-save.json" : "prototype-save.json");

        public void Configure(MiniGameScreen ui, ApartmentPlayer worldPlayer, PresentationEffects presentation, MiniGameYarnBridge bridge)
        { screen = ui; player = worldPlayer; effects = presentation; yarn = bridge; }
        public void ConfigureDreams(DreamSequenceProfile profile, PlayerRebellionProfile rebellion)
        { enableDreamSequence = true; dreamProfile = profile; rebellionProfile = rebellion; }

        private void Awake()
        {
            legacyProject = yarn.Project;
            if (MiniGameSessionHost.Current != null && MiniGameSessionHost.Current.PendingCombinedRestore)
                RestoreFromSession(MiniGameSessionHost.Current);
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
            screen.SaveRequested += Save;
            screen.LoadRequested += Load;
            screen.RestartRequested += Restart;
            Debug.Log("MINIGAME_PLAYER_READY unity=" + Application.unityVersion + " phase=" + Runtime.GetView().Game.Phase
                + " profile=" + (campaignActive ? "campaign" : terminalDemoActive ? "terminal-demo" : "legacy-three-day") + " rules=" + rules.RulesId);
        }

        private void OnDestroy()
        {
            if (screen == null) return;
            screen.SaveRequested -= Save;
            screen.LoadRequested -= Load;
            screen.RestartRequested -= Restart;
        }

        private void Update()
        {
            if (Runtime == null || (MiniGameSessionHost.Current != null && MiniGameSessionHost.Current.IsTransitioning)) return;
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                var view = Runtime.GetView();
                if (view.Paused) Runtime.SetPaused(false);
                else if (screen.TryClosePropNarration()) { }
                else if (screen.TryCloseHistory()) { }
                else if (view.Inspection != null) Runtime.CloseInspection();
                else Runtime.SetPaused(true);
            }
            if (Input.GetKeyDown(KeyCode.F5)) Save();
            if (Input.GetKeyDown(KeyCode.F9)) Load();
            if (MiniGameSessionHost.Current != null && MiniGameSessionHost.Current.IsTransitioning) return;
            var state = Runtime.GetView();
            if (Runtime.UsesDreamFlow && state.Game.Phase == GamePhase.Evening && !state.TerminalOpen
                && state.Cue == null && !state.Paused && !effects.IsTransitioning)
                Runtime.BeginNightDream();
        }

        private void InstallRuntime(GameRuntime runtime, bool installSession = true, bool prepared = false)
        {
            Dreams = GetComponent<DreamSequenceDirector>();
            if (!prepared && Dreams != null) Dreams.PrepareForRuntimeChange();
            var doorView = player.ViewCamera.GetComponent<DoorViewCamera>();
            if (!prepared && doorView != null) doorView.PrepareForRuntimeChange(runtime);
            Runtime = runtime;
            if (installSession)
            {
                if (runtime.UsesDreamFlow) MiniGameSessionHost.Install(runtime, rules, content, dreamProfile);
                else MiniGameSessionHost.Release();
            }
            Audio = GetComponent<MiniGameAudioDirector>();
            if (Audio == null) Audio = gameObject.AddComponent<MiniGameAudioDirector>();
            Audio.ConfigureDreams(dreamProfile);
            Audio.Bind(runtime);
            screen.BindAudio(Audio);
            screen.Bind(runtime);
            player.Bind(runtime, screen);
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
            if (Dreams != null) Dreams.Bind(runtime, screen, player, Audio, dreamProfile);
            screen.UseYarnPresentation();
            yarn.Bind(runtime, screen);
        }

        public void RestoreFromSession(MiniGameSessionHost session)
        {
            rules = session.Rules; content = session.Content; dreamProfile = session.DreamProfile;
            campaignActive = terminalDemoActive = true;
            var package = Resources.Load<DialoguePackageAsset>("MiniGame/ActiveCampaignPackage");
            yarn.Configure(package.Project);
            GetComponent<DreamSequenceDirector>()?.PrepareForRuntimeChange();
            player.ViewCamera.GetComponent<DoorViewCamera>()?.PrepareForRuntimeChange(session.Runtime);
            effects.PrepareForRuntimeChange();
            session.RestoreRoomPose(player);
            InstallRuntime(session.Runtime, false, true);
            if (Dreams != null && session.DreamCheckpoint != null) Dreams.RestoreCheckpoint(session.DreamCheckpoint);
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
            InstallRuntime(new GameRuntime(rules, content, useTerminalDirectory: true));
            if (campaignActive) Runtime.BeginDayZeroOpening();
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
            if (Runtime.UsesDreamFlow && MiniGameSessionHost.Current != null) MiniGameSessionHost.Current.RestartToCombined();
            else
            {
                InstallRuntime(new GameRuntime(rules, content, terminalDemoActive));
                if (campaignActive) Runtime.BeginDayZeroOpening();
            }
        }
        public void RestoreCheckpoint(string json) => InstallRuntime(GameRuntime.Restore(json, rules, content));

        public void StartCampaign()
        {
            var package = Resources.Load<DialoguePackageAsset>("MiniGame/ActiveCampaignPackage");
            if (package == null) throw new InvalidOperationException("Import the full campaign with MiniGame/Campaign/Import and activate first.");
            StartTerminalDemo(package);
            if (!campaignActive) throw new InvalidOperationException("The campaign resource must contain a validated V2 package.");
        }
    }
}
