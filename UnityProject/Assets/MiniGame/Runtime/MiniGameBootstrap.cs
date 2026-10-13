using System;
using System.IO;
using MiniGame.Core;
using MiniGame.Presentation;
using UnityEngine;

namespace MiniGame.UnityHost
{
    /// <summary>The only Unity owner of a runtime. Widgets never keep a second rule state.</summary>
    public sealed class MiniGameBootstrap : MonoBehaviour
    {
        [SerializeField] private MiniGameScreen screen;
        [SerializeField] private ApartmentPlayer player;
        [SerializeField] private PresentationEffects effects;
        [SerializeField] private MiniGameYarnBridge yarn;
        private GameRules rules;
        private IGameContent content;
        public GameRuntime Runtime { get; private set; }
        public MiniGameScreen Screen => screen;
        public ApartmentPlayer Player => player;
        public PresentationEffects Effects => effects;
        public string SavePath => Path.Combine(Application.persistentDataPath, "prototype-save.json");

        public void Configure(MiniGameScreen ui, ApartmentPlayer worldPlayer, PresentationEffects presentation, MiniGameYarnBridge bridge)
        { screen = ui; player = worldPlayer; effects = presentation; yarn = bridge; }

        private void Awake()
        {
            rules = PrototypeRules.Create();
            content = new PrototypeContent(rules);
            InstallRuntime(new GameRuntime(rules, content));
            screen.SaveRequested += Save;
            screen.LoadRequested += Load;
            screen.RestartRequested += Restart;
            Debug.Log("MINIGAME_PLAYER_READY unity=" + Application.unityVersion + " phase=" + Runtime.GetView().Game.Phase);
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
            if (Runtime == null) return;
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                var view = Runtime.GetView();
                if (view.Paused) Runtime.SetPaused(false);
                else if (view.Inspection != null) Runtime.CloseInspection();
                else Runtime.SetPaused(true);
            }
            if (Input.GetKeyDown(KeyCode.F5)) Save();
            if (Input.GetKeyDown(KeyCode.F9)) Load();
        }

        private void InstallRuntime(GameRuntime runtime)
        {
            Runtime = runtime;
            screen.Bind(runtime);
            player.Bind(runtime, screen);
            effects.Bind(runtime, screen);
            screen.UseYarnPresentation();
            yarn.Bind(runtime, screen);
        }

        public void Save()
        {
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
            try
            {
                var restored = new RuntimeFileSaveStore(SavePath).Load(rules, content);
                InstallRuntime(restored);
                screen.Notify("检查点已恢复；未重复结算回答或处置。", false);
            }
            catch (Exception exception)
            { screen.Notify("恢复失败：" + exception.Message, true); Debug.LogWarning(exception.Message); }
        }

        public void Restart() => InstallRuntime(new GameRuntime(rules, content));
        public void RestoreCheckpoint(string json) => InstallRuntime(GameRuntime.Restore(json, rules, content));
    }
}
