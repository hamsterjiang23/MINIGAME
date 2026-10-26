using System;
using System.IO;
using System.Linq;
using MiniGame.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MiniGame.UnityHost.Editor
{
    public sealed class CampaignDebugWindow : EditorWindow
    {
        [SerializeField] private int day;
        [SerializeField] private CampaignDebugPoint point;
        [SerializeField] private bool pauseOnArrival;

        [MenuItem("MiniGame/调试/日期与阶段跳转")]
        public static void Open() => GetWindow<CampaignDebugWindow>("日期与阶段调试").Show();
        private void OnEnable() { minSize = new Vector2(400, 325); EditorApplication.update += Repaint; }
        private void OnDisable() => EditorApplication.update -= Repaint;

        private void OnGUI()
        {
            EditorGUILayout.LabelField("正式流程 · 日期跳转", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            using (new EditorGUI.DisabledScope(CampaignDebugJump.IsBusy || EditorApplication.isCompiling))
            {
                day = EditorGUILayout.Popup("目标日期", day, new[] { "第零天 · 教学", "第一天", "第二天", "第三天" });
                var points = CampaignDebugCheckpoint.PointsForDay(day);
                int selected = Array.IndexOf(points, point);
                if (selected < 0) selected = 0;
                selected = EditorGUILayout.Popup("进入阶段", selected, points.Select(value => Label(value, day)).ToArray());
                point = points[selected];
                pauseOnArrival = EditorGUILayout.Toggle("进入后暂停", pauseOnArrival);
                EditorGUILayout.Space();
                if (GUILayout.Button(EditorApplication.isPlaying ? "跳转到所选日期与阶段" : "开始播放并跳转", GUILayout.Height(32)))
                    CampaignDebugJump.Request(day, point, pauseOnArrival);
            }
            EditorGUILayout.HelpBox("跳转会替换当前播放进度。前置流程使用正式命令自动推进：优先治疗，回答选择反叛增量最低项。调试存档单独保存。", MessageType.Info);
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("隐藏结局 · 海滩梦境", EditorStyles.boldLabel);
            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUI.DisabledScope(EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling || CampaignDebugJump.IsBusy))
                    if (GUILayout.Button("预览隐藏结局")) StartHiddenEndingPreview();
                using (new EditorGUI.DisabledScope(!DreamCubemapPreview.IsActive))
                    if (GUILayout.Button("停止隐藏结局预览")) DreamCubemapPreview.Stop();
            }
            EditorGUILayout.HelpBox("停止当前播放后可启动独立预览。WASD 移动，鼠标查看，E 躺下；结束预览后恢复原场景。", MessageType.None);
            if (day == 0) EditorGUILayout.HelpBox("第零天包含开场教学、关机和第一次睡觉；没有普通梦境，睡眠后进入第一天。", MessageType.None);
            var host = UnityEngine.Object.FindObjectOfType<MiniGameBootstrap>();
            if (EditorApplication.isPlaying && host != null && host.Runtime != null)
            {
                var current = host.Runtime.GetView();
                EditorGUILayout.LabelField("当前进度", $"第 {current.Game.Day} 天 / {current.Game.Phase}");
                EditorGUILayout.LabelField("当前演出", current.Cue?.Kind.ToString() ?? "自由操作");
            }
            if (!string.IsNullOrEmpty(CampaignDebugJump.Status))
                EditorGUILayout.HelpBox(CampaignDebugJump.Status, CampaignDebugJump.Failed ? MessageType.Error : MessageType.None);
            if (!string.IsNullOrEmpty(EditorCampaignDebugSession.SaveDirectory))
            {
                EditorGUILayout.LabelField("调试存档", EditorCampaignDebugSession.SaveDirectory, EditorStyles.wordWrappedMiniLabel);
                if (GUILayout.Button("打开调试存档目录")) EditorUtility.RevealInFinder(EditorCampaignDebugSession.SaveDirectory);
            }
        }

        public static void StartHiddenEndingPreview() => DreamCubemapPreview.Open();

        internal static string Label(CampaignDebugPoint value, int day)
        {
            switch (value)
            {
                case CampaignDebugPoint.Opening: return "NORM 开场与教学";
                case CampaignDebugPoint.Morning: return "晨检前 · 房间与房门";
                case CampaignDebugPoint.Room: return day == 0 ? "教学前 · 自由查看房间" : "晨检后 · 白天房间";
                case CampaignDebugPoint.Terminal: return day == 0 ? "教学电脑 · 病人目录" : "工作电脑 · 病人目录";
                case CampaignDebugPoint.Shutdown: return "关机演出 · 英文跳字";
                case CampaignDebugPoint.Bedtime: return day == 0 ? "教学结束 · 睡前房间" : "工作结束 · 夜间房间";
                case CampaignDebugPoint.Sleep: return day == 0 ? "第一次睡觉 · 前往第一天" : "上床 · 入梦演出";
                case CampaignDebugPoint.Dream: return "当晚梦境 · 动画播放";
                case CampaignDebugPoint.AfterDream: return "梦醒 / 自审入口";
                default: return value.ToString();
            }
        }
    }

    [InitializeOnLoad]
    public static class CampaignDebugJump
    {
        private const string PendingKey = "MiniGame.CampaignDebug.Pending";
        private const string ActiveKey = EditorCampaignDebugSession.ActiveStateKey;
        private const string DirectoryKey = EditorCampaignDebugSession.DirectoryStateKey;
        private const string StatusKey = "MiniGame.CampaignDebug.Status";
        private const string RestartKey = "MiniGame.CampaignDebug.RestartAfterReload";
        private static bool loading, waitingForDream;
        private static double deadline;
        private static MiniGameBootstrap waitingHost;
        public static bool IsBusy => !string.IsNullOrEmpty(SessionState.GetString(PendingKey, ""));
        public static string Status => SessionState.GetString(StatusKey, "");
        public static bool Failed { get; private set; }

        [Serializable] private sealed class JumpRequest
        {
            public int day;
            public CampaignDebugPoint point;
            public bool pause;
            public int recoveryAttempts;
        }

        static CampaignDebugJump()
        {
            EditorApplication.playModeStateChanged += OnPlayMode;
            EditorApplication.update += Tick;
            if (SessionState.GetBool(ActiveKey, false) && EditorApplication.isPlaying)
            {
                EditorCampaignDebugSession.SaveDirectory = SessionState.GetString(DirectoryKey, "");
                MainMenuController.SaveDirectoryOverride = EditorCampaignDebugSession.SaveDirectory;
            }
            deadline = EditorApplication.timeSinceStartup + 45;
        }

        public static void Request(int day, CampaignDebugPoint point, bool pauseOnArrival = false)
        {
            if (EditorApplication.isCompiling || IsBusy) throw new InvalidOperationException("请等待编译或上次日期跳转完成。");
            if (day < 0 || day > 3 || !CampaignDebugCheckpoint.PointsForDay(day).Contains(point))
                throw new ArgumentOutOfRangeException(nameof(day), "请选择第零天至第三天的可用阶段。");
            if (!EditorApplication.isPlaying && SceneManager.GetActiveScene().path != MainRoomGameplayBuilder.ScenePath)
            {
                if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
                EditorSceneManager.OpenScene(MainRoomGameplayBuilder.ScenePath, OpenSceneMode.Single);
            }
            Failed = false; loading = waitingForDream = false; waitingHost = null;
            SessionState.EraseBool(RestartKey);
            SessionState.SetString(PendingKey, JsonUtility.ToJson(new JumpRequest { day = day, point = point, pause = pauseOnArrival }));
            SessionState.SetString(StatusKey, "准备第 " + day + " 天 / " + CampaignDebugWindow.Label(point, day));
            deadline = EditorApplication.timeSinceStartup + 45;
            EditorApplication.isPaused = false;
            EditorApplication.ExecuteMenuItem("Window/General/Game");
            if (!EditorApplication.isPlaying) EditorApplication.isPlaying = true;
        }

        private static void OnPlayMode(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode) deadline = EditorApplication.timeSinceStartup + 45;
            if (state != PlayModeStateChange.EnteredEditMode) return;
            if (SessionState.GetBool(RestartKey, false) && IsBusy)
            {
                SessionState.EraseBool(RestartKey);
                loading = waitingForDream = false; waitingHost = null;
                EditorApplication.delayCall += () => { if (IsBusy) EditorApplication.isPlaying = true; };
                return;
            }
            SessionState.EraseBool(RestartKey);
            SessionState.EraseString(PendingKey); SessionState.EraseBool(ActiveKey); SessionState.EraseString(DirectoryKey);
            EditorCampaignDebugSession.SaveDirectory = null;
            MainMenuController.SaveDirectoryOverride = null;
            loading = waitingForDream = false; waitingHost = null;
        }

        private static void Tick()
        {
            if (!IsBusy || !EditorApplication.isPlaying || EditorApplication.isCompiling) return;
            try
            {
                if (EditorApplication.timeSinceStartup > deadline) throw new TimeoutException("日期跳转超时，请检查 Console 编译与运行错误。");
                var request = JsonUtility.FromJson<JumpRequest>(SessionState.GetString(PendingKey, ""));
                if (waitingForDream)
                {
                    if (waitingHost == null) throw new InvalidOperationException("梦境预览宿主已卸载。");
                    if (!waitingHost.Dreams.IsPlaying) return;
                    waitingHost.Runtime.SetPaused(true);
                    Finish(request); return;
                }
                BeginSaveScope();
                var host = UnityEngine.Object.FindObjectOfType<MiniGameBootstrap>();
                // Script reload preserves scene components, but not the runtime or constructed UI.
                // Re-enter Play Mode once so Awake can rebuild them before restoring a checkpoint.
                bool staleScreen = host != null && host.Screen != null
                    && host.Screen.transform.Find("Prototype UI") != null && host.Screen.TerminalDisplay == null;
                if (staleScreen || host != null && host.Runtime == null && (host.MainMenu == null || host.MainMenu.View == null))
                {
                    if (request.recoveryAttempts > 0) throw new InvalidOperationException("重新播放后场景仍未初始化，请检查 Console。");
                    request.recoveryAttempts++;
                    SessionState.SetString(PendingKey, JsonUtility.ToJson(request));
                    SessionState.SetBool(RestartKey, true);
                    SessionState.SetString(StatusKey, "脚本重载后重新初始化场景……");
                    EditorApplication.isPlaying = false;
                    return;
                }
                if (MiniGameSessionHost.Current != null && MiniGameSessionHost.Current.IsTransitioning) return;
                if (MainMenuController.Current != null && !MainMenuController.Current.TryDismissForEditorDebug()) return;
                if (SceneManager.GetActiveScene().path != MainRoomGameplayBuilder.ScenePath)
                {
                    if (!loading)
                    {
                        MainMenuController.SkipStartup = true;
                        MiniGameSessionHost.Release();
                        SceneManager.LoadSceneAsync(MiniGameSessionHost.CombinedScene, LoadSceneMode.Single);
                        loading = true;
                    }
                    return;
                }
                if (host == null || host.Screen == null || host.Player == null) return;
                Apply(host, request);
            }
            catch (Exception exception)
            {
                Failed = true;
                SessionState.SetString(StatusKey, "跳转失败：" + exception.Message);
                SessionState.EraseString(PendingKey);
                SessionState.EraseBool(RestartKey);
                Debug.LogException(exception);
            }
        }

        private static void BeginSaveScope()
        {
            string directory = SessionState.GetString(DirectoryKey, "");
            if (string.IsNullOrEmpty(directory)) directory = Path.GetFullPath(Path.Combine(Application.temporaryCachePath, "CampaignDayDebug"));
            Directory.CreateDirectory(directory);
            SessionState.SetString(DirectoryKey, directory); SessionState.SetBool(ActiveKey, true);
            EditorCampaignDebugSession.SaveDirectory = directory;
            MainMenuController.SaveDirectoryOverride = directory;
        }

        private static void Apply(MiniGameBootstrap host, JumpRequest request)
        {
            host.StartCampaign();
            var session = MiniGameSessionHost.Current;
            if (session == null) throw new InvalidOperationException("正式场景需要启用梦境与持续会话配置。");
            string checkpoint = CampaignDebugCheckpoint.Create(session.Rules, session.Content, request.day, request.point,
                host.Runtime.Endings);
            host.Dreams?.PrepareForRuntimeChange();
            host.Effects.PrepareForRuntimeChange();
            PlacePlayer(host, request.point);
            host.Screen.SetMainMenuCovered(false);
            host.RestoreCheckpoint(checkpoint);
            foreach (var audio in host.gameObject.scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<MiniGameAudioService>(true)))
                audio.SetMenuSuspended(false);
            if (request.pause && request.point == CampaignDebugPoint.Dream)
            {
                waitingForDream = true; waitingHost = host;
                SessionState.SetString(StatusKey, "等待梦境首帧后暂停……");
                return;
            }
            if (request.pause) host.Runtime.SetPaused(true);
            Finish(request);
        }

        private static void PlacePlayer(MiniGameBootstrap host, CampaignDebugPoint point)
        {
            bool terminal = point == CampaignDebugPoint.Terminal || point == CampaignDebugPoint.Shutdown || point == CampaignDebugPoint.Opening;
            bool bedtime = point == CampaignDebugPoint.Bedtime || point == CampaignDebugPoint.Sleep
                || point == CampaignDebugPoint.Dream || point == CampaignDebugPoint.AfterDream;
            string anchorName = terminal ? "Computer approach" : bedtime ? "Bed approach" : "Player spawn";
            var transforms = host.gameObject.scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<Transform>(true)).ToArray();
            var anchor = transforms.Single(value => value.name == anchorName);
            var target = host.gameObject.scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<WorldInteraction>(true))
                .Single(value => value.Target == (bedtime ? InteractionTarget.Bed : point == CampaignDebugPoint.Morning ? InteractionTarget.Door : InteractionTarget.Computer));
            var controller = host.Player.GetComponent<CharacterController>();
            bool enabled = controller.enabled; controller.enabled = false;
            host.Player.transform.position = anchor.position;
            host.Player.ViewCamera.transform.localPosition = host.Player.StandingEyeLocalPosition;
            controller.enabled = enabled;
            host.Player.FacePoint(target.GetComponent<Collider>().bounds.center);
            Physics.SyncTransforms();
        }

        private static void Finish(JumpRequest request)
        {
            SessionState.SetString(StatusKey, "已进入第 " + request.day + " 天 / " + CampaignDebugWindow.Label(request.point, request.day)
                + (request.pause ? "（已暂停，Esc 继续）" : ""));
            SessionState.EraseString(PendingKey);
            loading = waitingForDream = false; waitingHost = null;
            Debug.Log("MINIGAME_DAY_DEBUG_READY day=" + request.day + " point=" + request.point);
        }
    }
}
