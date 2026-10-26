using System;
using System.Collections;
using System.IO;
using System.Text;
using MiniGame.Core;
using MiniGame.Presentation;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MiniGame.UnityHost
{
    /// <summary>One campaign runtime survives the room-to-beach scene handoff.</summary>
    public sealed class MiniGameSessionHost : MonoBehaviour
    {
        public const string CombinedScene = "MainRoomCombined";
        public const string BeachScene = "DreamEndingBeach";
        public static MiniGameSessionHost Current { get; private set; }
        public GameRuntime Runtime { get; private set; }
        public GameRules Rules { get; private set; }
        public IGameContent Content { get; private set; }
        public DreamSequenceProfile DreamProfile { get; private set; }
        public BeachEndingCheckpoint EndingCheckpoint { get; set; }
        public RoomEndingTransitionCheckpoint RoomEndingTransition { get; set; }
        public EndingThanksCheckpoint ThanksCheckpoint { get; private set; }
        public DreamPlaybackCheckpoint DreamCheckpoint { get; private set; }
        public SleepTransitionCheckpoint SleepCheckpoint { get; private set; }
        public SupervisorDialogueCheckpoint SupervisorCheckpoint { get; private set; }
        public int[] SupervisorScanDays { get; private set; } = Array.Empty<int>();
        public bool PendingCombinedRestore { get; private set; }
        public bool IsTransitioning { get; private set; }
        public string SavePath => Path.Combine(MiniGameBootstrap.SaveDirectory, "campaign-dream-save.json");
        public string LastMessage { get; private set; }
        private RoomDreamEnding roomEnding;
        private bool roomEndingFailed;
        private bool hiddenThanksReady;
        private EndingThanksView thanks;
        private int flowGeneration;
        private bool UsesRoomEnding => DreamProfile != null && DreamProfile.UseRoomCubemapEnding;

        public static MiniGameSessionHost Install(GameRuntime runtime, GameRules rules, IGameContent content, DreamSequenceProfile profile)
        {
            if (Current == null)
            {
                var owner = new GameObject("MiniGame Persistent Session");
                Current = owner.AddComponent<MiniGameSessionHost>();
                DontDestroyOnLoad(owner);
            }
            var host = Current;
            host.flowGeneration++;
            host.StopAllCoroutines();
            host.IsTransitioning = false;
            host.StopRoomEnding();
            host.Runtime = runtime; host.Rules = rules; host.Content = content; host.DreamProfile = profile;
            host.DreamCheckpoint = null; host.EndingCheckpoint = null; host.SleepCheckpoint = null;
            host.RoomEndingTransition = null; host.ThanksCheckpoint = null;
            host.pendingEndingFadePose = null;
            if (host.thanks != null) Destroy(host.thanks.gameObject);
            host.thanks = null;
            host.SupervisorCheckpoint = null;
            host.SupervisorScanDays = Array.Empty<int>();
            host.PendingCombinedRestore = false;
            host.roomEndingFailed = false;
            return host;
        }

        public static void Release()
        {
            if (Current == null) return;
            var previous = Current;
            Current = null;
            previous.StopRoomEnding();
            Destroy(previous.gameObject);
        }

        private void LateUpdate()
        {
            if (Runtime == null || IsTransitioning) return;
            var view = Runtime.GetView();
            if (roomEnding != null && !string.IsNullOrEmpty(roomEnding.FailureReason)) roomEndingFailed = true;
            if (roomEnding != null && roomEnding.Ready && view.EndingStage == EndingStage.Transition && !view.Paused)
            { roomEndingFailed = false; Runtime.MarkEndingSpaceReady(); view = Runtime.GetView(); }
            if (Runtime.UsesEndingFlow && view.EndingStage == EndingStage.Thanks)
            {
                // Keep the serialized stage for compatible checkpoints, but end immediately without a final page.
                if (!view.Paused) CompleteEnding();
                return;
            }
            if (Runtime.UsesEndingFlow && view.EndingStage != EndingStage.Transition && view.EndingStage != EndingStage.Space) return;
            if (view.Game.Phase != GamePhase.Ending || view.Game.Ending != EndingType.DreamSea) return;
            if (UsesRoomEnding)
            {
                if (roomEndingFailed || roomEnding != null) return;
                EnsureRoomEnding();
            }
            else if (SceneManager.GetActiveScene().name != BeachScene) TransitionTo(BeachScene);
        }

        private void EnsureRoomEnding()
        {
            if (SceneManager.GetActiveScene().name != CombinedScene)
            {
                PendingCombinedRestore = true;
                TransitionTo(CombinedScene);
                return;
            }
            var room = FindObjectOfType<MiniGameBootstrap>();
            if (room != null) StartCoroutine(BeginRoomEnding(room));
        }

        private IEnumerator BeginRoomEnding(MiniGameBootstrap room)
        {
            int generation = flowGeneration;
            var runtime = Runtime;
            IsTransitioning = true;
            try { roomEnding = RoomDreamEnding.Begin(room, this); roomEnding.BedCompleted += OnHiddenBed; IsTransitioning = false; }
            catch (Exception exception)
            {
                IsTransitioning = false; roomEndingFailed = true;
                LastMessage = "隐藏梦结局未能开始：" + exception.Message;
                Debug.LogException(exception);
                yield break;
            }
            while (generation == flowGeneration && roomEnding != null && !roomEnding.Ready && !hiddenThanksReady
                && string.IsNullOrEmpty(roomEnding.FailureReason)) yield return null;
            if (generation != flowGeneration || Runtime != runtime) yield break;
            IsTransitioning = false;
            if (roomEnding == null || !string.IsNullOrEmpty(roomEnding.FailureReason))
            {
                roomEndingFailed = true;
                LastMessage = roomEnding != null ? roomEnding.FailureReason : "隐藏梦结局已中止。";
            }
            else if (Runtime.UsesEndingFlow && Runtime.EndingStage == EndingStage.Thanks && !hiddenThanksReady)
            {
                roomEnding.RestoreThanksPresentation();
                hiddenThanksReady = true;
            }
            else if (Runtime.UsesEndingFlow && Runtime.EndingStage == EndingStage.Transition)
            {
                while (generation == flowGeneration && Runtime.GetView().Paused) yield return null;
                if (generation != flowGeneration || Runtime != runtime) yield break;
                Runtime.MarkEndingSpaceReady();
            }
        }

        private void OnHiddenBed()
        {
            if (Runtime != null && Runtime.CompleteHiddenBed().Accepted)
            {
                hiddenThanksReady = true;
                CompleteEnding();
            }
        }

        private void CompleteEnding()
        {
            if (Runtime == null || !Runtime.MarkEndingCompleted().Accepted) return;
            ThanksCheckpoint = new EndingThanksCheckpoint { Phase = EndingThanksPhase.Completed };
            FindObjectOfType<MiniGameBootstrap>()?.MainMenu?.CompleteInspectionAndReturnToMenu(true);
            if (thanks != null) Destroy(thanks.gameObject);
            thanks = null;
        }

        public bool RetryRoomEnding()
        {
            if (!roomEndingFailed || Runtime == null
                || Runtime.EndingStage != EndingStage.Transition && (Runtime.EndingStage != EndingStage.Thanks || hiddenThanksReady)) return false;
            if (roomEnding != null)
            {
                bool retry = roomEnding.RetryInitialization();
                if (retry) roomEndingFailed = false;
                return retry;
            }
            StopRoomEnding(); RoomEndingTransition = null; roomEndingFailed = false;
            return true;
        }

        private void StopRoomEnding(bool restoreRoom = true)
        {
            var owner = roomEnding != null ? roomEnding : FindObjectOfType<RoomDreamEnding>();
            roomEnding = null;
            hiddenThanksReady = false;
            if (owner == null) return;
            owner.BedCompleted -= OnHiddenBed;
            if (restoreRoom) owner.StopAndRestore();
            else owner.CleanupForDestruction();
        }

        private void TransitionTo(string sceneName)
        {
            if (IsTransitioning) return;
            IsTransitioning = true;
            var room = FindObjectOfType<MiniGameBootstrap>();
            if (room != null) room.Screen.SetTransitionLocked(true);
            var load = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
            if (load == null) { IsTransitioning = false; throw new InvalidOperationException("无法加载场景：" + sceneName); }
            int generation = flowGeneration;
            load.completed += _ => { if (this != null && generation == flowGeneration) IsTransitioning = false; };
        }

        public bool TryEnterGmEnding(EndingType ending, out string error)
        {
            error = null;
            if (Runtime == null || !Runtime.UsesDreamFlow || !Runtime.GetView().Paused
                || MainMenuController.BlocksGameplayInput || IsTransitioning && roomEnding == null)
            {
                error = LastMessage = "请在游戏内暂停菜单选择 GM 结局。";
                return false;
            }
            GameRuntime replacement;
            try
            {
                // Old presenters retain their old runtime until rebinding; delayed callbacks cannot advance this flow.
                replacement = GameRuntime.Restore(Runtime.Save(), Rules, Content, Runtime.Endings, pauseOnLoad: true);
                if (!replacement.EnterGmEndingFromPause(ending).Accepted)
                {
                    error = LastMessage = "当前无法进入所选 GM 结局。";
                    return false;
                }
            }
            catch (Exception exception)
            {
                error = LastMessage = "GM 结局准备失败：" + exception.Message;
                return false;
            }

            flowGeneration++;
            StopAllCoroutines();
            IsTransitioning = false;
            StopRoomEnding();
            foreach (var controller in FindObjectsOfType<DreamEndingController>())
            {
                controller.enabled = false;
                controller.Audio?.StopAll();
                controller.gameObject.SetActive(false);
            }
            if (thanks != null) Destroy(thanks.gameObject);
            thanks = null;
            DreamCheckpoint = null; EndingCheckpoint = null; SleepCheckpoint = null;
            RoomEndingTransition = null; ThanksCheckpoint = null; SupervisorCheckpoint = null;
            pendingRoomPose = null; pendingEndingFadePose = null;
            roomEndingFailed = hiddenThanksReady = false;
            Runtime = replacement;
            PendingCombinedRestore = true;
            var room = FindObjectOfType<MiniGameBootstrap>();
            if (room != null) room.RestoreFromSession(this);
            else TransitionTo(CombinedScene);
            LastMessage = "已进入所选 GM 结局。";
            return true;
        }

        public void Save() => SaveTo(SavePath);
        public void SaveTo(string path) => TrySaveTo(path, out _);

        public bool TrySaveTo(string path, out string error)
        {
            error = null;
            try
            {
                if (Runtime == null || IsTransitioning && roomEnding == null) throw new InvalidOperationException("请等待场景切换结束后保存。");
                var dream = FindObjectOfType<DreamSequenceDirector>();
                var beach = FindObjectOfType<DreamEndingController>();
                DreamCheckpoint = dream != null ? dream.CaptureCheckpoint() : null;
                SleepCheckpoint = FindObjectOfType<PresentationEffects>()?.CaptureSleepCheckpoint();
                if (beach != null) EndingCheckpoint = beach.CaptureCheckpoint();
                RoomEndingTransition = roomEnding != null ? roomEnding.CaptureTransitionCheckpoint() : RoomEndingTransition;
                ThanksCheckpoint = thanks != null ? thanks.CaptureCheckpoint() : ThanksCheckpoint;
                var saved = new DreamSessionSave
                {
                    FormatVersion = 1, Runtime = Runtime.Save(), Dream = DreamCheckpoint, Sleep = SleepCheckpoint,
                    HasSleepCheckpoint = SleepCheckpoint != null,
                    RoomEndingTransition = RoomEndingTransition, HasRoomEndingTransition = RoomEndingTransition != null,
                    Thanks = ThanksCheckpoint, HasThanks = ThanksCheckpoint != null,
                    Ending = Runtime.GetView().Game.Ending == EndingType.DreamSea ? EndingCheckpoint : null
                };
                SupervisorCheckpoint = FindObjectOfType<MiniGameYarnBridge>()?.CaptureSupervisorCheckpoint();
                saved.Supervisor = SupervisorCheckpoint;
                var audio = FindObjectOfType<MiniGameBootstrap>()?.Audio;
                if (audio != null) SupervisorScanDays = audio.CaptureSupervisorScanDays(Runtime);
                saved.SupervisorScanDays = SupervisorScanDays;
                var player = FindObjectOfType<ApartmentPlayer>();
                if (player != null)
                {
                    bool visibleEndingFade = UsesRoomEnding && Runtime.EndingStage == EndingStage.Transition
                        && (roomEnding != null ? roomEnding.TransitionPhase : RoomEndingTransition?.Phase ?? RoomEndingTransitionPhase.FadingOut)
                            == RoomEndingTransitionPhase.FadingOut;
                    saved.HasRoomPose = true;
                    saved.PlayerPosition = player.transform.position;
                    saved.PlayerRotation = player.transform.rotation;
                    saved.CameraPosition = player.ViewCamera.transform.position;
                    saved.CameraRotation = player.ViewCamera.transform.rotation;
                    saved.FieldOfView = player.ViewCamera.fieldOfView;
                    var roomHost = FindObjectOfType<MiniGameBootstrap>();
                    if (!visibleEndingFade && roomHost != null && roomHost.Effects.TryGetRoomReturnPose(out var actorPosition, out var actorRotation,
                        out var cameraPosition, out var cameraRotation, out var fov))
                    {
                        saved.PlayerPosition = actorPosition; saved.PlayerRotation = actorRotation;
                        saved.CameraPosition = cameraPosition; saved.CameraRotation = cameraRotation; saved.FieldOfView = fov;
                    }
                    var door = player.ViewCamera.GetComponent<DoorViewCamera>();
                    if (!visibleEndingFade && door != null && door.HasRoomPose)
                    { saved.CameraPosition = door.ReturnPosition; saved.CameraRotation = door.ReturnRotation; saved.FieldOfView = door.ReturnFieldOfView; }
                    var supervisor = FindObjectOfType<SupervisorDoorwayView>();
                    if (!visibleEndingFade && supervisor != null && supervisor.TryGetRoomReturnPose(out var returnPosition, out var returnRotation, out var returnFov))
                    { saved.CameraPosition = returnPosition; saved.CameraRotation = returnRotation; saved.FieldOfView = returnFov; }
                    if (!visibleEndingFade && DreamCheckpoint != null && DreamCheckpoint.Phase != DreamPlaybackPhase.None)
                    { saved.CameraPosition = DreamCheckpoint.CameraPosition; saved.CameraRotation = DreamCheckpoint.CameraRotation; saved.FieldOfView = DreamCheckpoint.FieldOfView; }
                }
                WriteAtomically(Path.GetFullPath(path), JsonUtility.ToJson(saved, true));
                LastMessage = "存档已记录。";
                return true;
            }
            catch (Exception exception)
            {
                error = LastMessage = "保存失败：" + exception.Message;
                Debug.LogWarning(LastMessage);
                return false;
            }
        }

        public void Load() => LoadFrom(SavePath);
        public void LoadFrom(string path) => TryLoadFrom(path, out _);

        public bool TryLoadFrom(string path, out string error, bool pauseOnLoad = false)
        {
            error = null;
            try
            {
                if (IsTransitioning && roomEnding == null) throw new InvalidOperationException("请等待场景切换结束后读取。");
                var json = File.ReadAllText(path, Encoding.UTF8);
                var envelope = JsonUtility.FromJson<DreamSessionSave>(json);
                // Pre-dream manual saves contain the strict runtime checkpoint directly.
                // Let the authoritative parser validate it before changing this session.
                if (envelope != null && envelope.FormatVersion == 0 && string.IsNullOrEmpty(envelope.Runtime))
                    envelope = new DreamSessionSave { FormatVersion = 1, Runtime = json };
                if (envelope == null || envelope.FormatVersion != 1 || string.IsNullOrEmpty(envelope.Runtime))
                    throw new InvalidDataException("存档版本不兼容；旧存档文件已保留，请开始新的梦境流程。");
                var restored = GameRuntime.Restore(envelope.Runtime, Rules, Content,
                    Rules.DreamFlowEnabled ? EndingDialoguePresenter.LoadCatalog() : null, pauseOnLoad);
                if (restored.EndingStage == EndingStage.Completed)
                    throw new InvalidDataException("本次检查期已结束，请开始新的检查期。存档文件已保留。");
                ValidateVisualCheckpoint(envelope, restored.GetView());
                flowGeneration++;
                StopAllCoroutines(); IsTransitioning = false;
                StopRoomEnding();
                if (thanks != null) Destroy(thanks.gameObject);
                thanks = null;
                Runtime = restored;
                SupervisorCheckpoint = envelope.Supervisor;
                SupervisorScanDays = envelope.SupervisorScanDays ?? Array.Empty<int>();
                SleepCheckpoint = envelope.HasSleepCheckpoint ? envelope.Sleep : null;
                RoomEndingTransition = envelope.HasRoomEndingTransition ? envelope.RoomEndingTransition : null;
                ThanksCheckpoint = envelope.HasThanks ? envelope.Thanks : null;
                roomEndingFailed = false;
                // JsonUtility materializes null nested serializable classes as default objects.
                // Core phase, rather than the presence of a visual DTO, decides which state is meaningful.
                var phase = restored.GetView().Game;
                DreamCheckpoint = phase.Phase == GamePhase.Dream && envelope.Dream != null
                    && envelope.Dream.Day == phase.Day && envelope.Dream.Phase != DreamPlaybackPhase.None
                    && Enum.IsDefined(typeof(DreamPlaybackPhase), envelope.Dream.Phase) ? envelope.Dream : null;
                EndingCheckpoint = restored.GetView().Game.Ending == EndingType.DreamSea ? envelope.Ending : null;
                bool beach = restored.GetView().Game.Ending == EndingType.DreamSea
                    && (!restored.UsesEndingFlow || restored.EndingStage == EndingStage.Transition || restored.EndingStage == EndingStage.Space);
                // Before full black the original apartment is still visible; later phases restore their own room pose.
                bool visibleApartmentFade = restored.EndingStage == EndingStage.Transition
                    && (RoomEndingTransition == null || RoomEndingTransition.Phase == RoomEndingTransitionPhase.FadingOut);
                pendingRoomPose = beach && UsesRoomEnding && !visibleApartmentFade ? null : envelope;
                pendingEndingFadePose = beach && UsesRoomEnding && visibleApartmentFade ? envelope : null;
                if (beach)
                {
                    if (UsesRoomEnding)
                    {
                        if (SceneManager.GetActiveScene().name != CombinedScene)
                        {
                            PendingCombinedRestore = true;
                            TransitionTo(CombinedScene);
                        }
                        else
                        {
                            PendingCombinedRestore = false;
                            var room = FindObjectOfType<MiniGameBootstrap>();
                            if (room != null) { room.RestoreFromSession(this); StartCoroutine(BeginRoomEnding(room)); }
                        }
                    }
                    else
                    {
                        PendingCombinedRestore = false;
                        var controller = FindObjectOfType<DreamEndingController>();
                        if (controller != null) controller.RestoreCheckpoint(EndingCheckpoint ?? new BeachEndingCheckpoint());
                        else TransitionTo(BeachScene);
                    }
                }
                else
                {
                    StopRoomEnding();
                    PendingCombinedRestore = true;
                    var bootstrap = FindObjectOfType<MiniGameBootstrap>();
                    if (bootstrap != null) bootstrap.RestoreFromSession(this);
                    else TransitionTo(CombinedScene);
                }
                LastMessage = "存档已读取。";
                return true;
            }
            catch (Exception exception)
            {
                error = LastMessage = "读取失败：" + exception.Message;
                Debug.LogWarning(LastMessage);
                return false;
            }
        }

        private static void ValidateVisualCheckpoint(DreamSessionSave saved, RuntimeView view)
        {
            if (saved.HasRoomEndingTransition && (saved.RoomEndingTransition == null || !saved.RoomEndingTransition.IsValid()
                || view.Game.Ending != EndingType.DreamSea))
                throw new InvalidDataException("隐藏空间转场检查点无效。");
            if (saved.HasThanks && (saved.Thanks == null || !saved.Thanks.IsValid()
                || view.EndingStage != EndingStage.Thanks && view.EndingStage != EndingStage.Completed
                || (saved.Thanks.Phase == EndingThanksPhase.Completed) != (view.EndingStage == EndingStage.Completed)))
                throw new InvalidDataException("感谢页检查点无效。");
            if (saved.HasSleepCheckpoint && (saved.Sleep == null || saved.Sleep.ForRestoredView(view) == null))
                throw new InvalidDataException("睡眠检查点与当前日期或演出不匹配。");
            var beach = saved.Ending;
            if (beach != null && beach.HasMusicPosition && (!Finite(beach.MusicSeconds) || beach.MusicSeconds < 0))
                throw new InvalidDataException("海滩音乐检查点无效。");
            if (beach != null && beach.HasOceanPosition && beach.OceanSample < 0)
                throw new InvalidDataException("海浪播放位置无效。");
            if (beach != null && (beach.Phase == BeachEndingPhase.SkyHold || beach.Phase == BeachEndingPhase.FadingOut)
                && (view.Game.Ending != EndingType.DreamSea || view.EndingStage != EndingStage.Space
                    || !beach.HasRoomPose || !Finite(beach.RoomPosition) || !Finite(beach.RoomYaw) || !Finite(beach.RoomPitch)
                    || !Finite(beach.LieStartPosition) || !Finite(beach.LieStartRotation)
                    || !Finite(beach.PhaseElapsed) || beach.PhaseElapsed < 0
                    || beach.PhaseElapsed > (beach.Phase == BeachEndingPhase.SkyHold
                        ? DreamEndingController.SkyHoldSeconds : DreamEndingController.EndingFadeSeconds)))
                throw new InvalidDataException("海滩收尾检查点无效。");
            if (saved.HasRoomPose && (!Finite(saved.PlayerPosition) || !Finite(saved.CameraPosition)
                || !Finite(saved.PlayerRotation) || !Finite(saved.CameraRotation)
                || !Finite(saved.FieldOfView) || saved.FieldOfView <= 1 || saved.FieldOfView >= 180))
                throw new InvalidDataException("存档镜头位置无效。");
            var dream = saved.Dream;
            if (dream != null && dream.Phase != DreamPlaybackPhase.None
                && (view.Game.Phase != GamePhase.Dream || dream.Day != view.Game.Day
                    || !Enum.IsDefined(typeof(DreamPlaybackPhase), dream.Phase)
                    || !Finite(dream.Elapsed) || dream.Elapsed < 0 || !Finite(dream.PlaybackSeconds) || dream.PlaybackSeconds < 0
                    || !Finite(dream.CameraPosition) || !Finite(dream.CameraRotation)
                    || !Finite(dream.FieldOfView) || dream.FieldOfView <= 1 || dream.FieldOfView >= 180))
                throw new InvalidDataException("梦境检查点无效或与当前日期不匹配。");
        }

        private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
        private static bool Finite(Vector3 value) => Finite(value.x) && Finite(value.y) && Finite(value.z);
        private static bool Finite(Quaternion value) => Finite(value.x) && Finite(value.y) && Finite(value.z) && Finite(value.w)
            && value.x * value.x + value.y * value.y + value.z * value.z + value.w * value.w > .01f;

        private DreamSessionSave pendingRoomPose;
        private DreamSessionSave pendingEndingFadePose;
        public void RestoreRoomPose(ApartmentPlayer player)
        {
            if (pendingRoomPose != null && pendingRoomPose.HasRoomPose)
            {
                player.ViewCamera.GetComponent<DoorViewCamera>()?.DiscardRoomPoseCapture();
                ApplyRoomPose(player, pendingRoomPose, pendingEndingFadePose == null);
            }
            pendingRoomPose = null;
            PendingCombinedRestore = false;
        }

        internal void RestoreEndingFadePose(ApartmentPlayer player)
        {
            // Runtime binding applies the ordinary lens; restore the visible ending lens before the first rendered fade frame.
            if (pendingEndingFadePose != null && pendingEndingFadePose.HasRoomPose)
                ApplyRoomPose(player, pendingEndingFadePose, false);
            pendingEndingFadePose = null;
        }

        private static void ApplyRoomPose(ApartmentPlayer player, DreamSessionSave saved, bool synchronizeLook)
        {
            var controller = player.GetComponent<CharacterController>();
            bool wasEnabled = controller.enabled;
            controller.enabled = false;
            player.transform.SetPositionAndRotation(saved.PlayerPosition, saved.PlayerRotation);
            player.ViewCamera.transform.SetPositionAndRotation(saved.CameraPosition, saved.CameraRotation);
            player.ViewCamera.fieldOfView = saved.FieldOfView;
            if (synchronizeLook) player.FacePoint(saved.CameraPosition + saved.CameraRotation * Vector3.forward);
            controller.enabled = wasEnabled;
        }

        public void RestartToCombined()
        {
            if (IsTransitioning) return;
            Release();
            SceneManager.LoadSceneAsync(CombinedScene, LoadSceneMode.Single);
        }

        private void OnDestroy() { StopRoomEnding(false); if (thanks != null) Destroy(thanks.gameObject); if (Current == this) Current = null; }

        private static void WriteAtomically(string path, string json)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            string temporary = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
            try
            {
                var bytes = new UTF8Encoding(false).GetBytes(json);
                using (var stream = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                { stream.Write(bytes, 0, bytes.Length); stream.Flush(true); }
                if (File.Exists(path)) File.Replace(temporary, path, null);
                else File.Move(temporary, path);
            }
            finally { if (File.Exists(temporary)) File.Delete(temporary); }
        }

        [Serializable]
        private sealed class DreamSessionSave
        {
            public int FormatVersion;
            public string Runtime;
            public DreamPlaybackCheckpoint Dream;
            public SleepTransitionCheckpoint Sleep;
            public bool HasSleepCheckpoint;
            public BeachEndingCheckpoint Ending;
            public RoomEndingTransitionCheckpoint RoomEndingTransition;
            public bool HasRoomEndingTransition;
            public EndingThanksCheckpoint Thanks;
            public bool HasThanks;
            public SupervisorDialogueCheckpoint Supervisor;
            public int[] SupervisorScanDays;
            public bool HasRoomPose;
            public Vector3 PlayerPosition;
            public Quaternion PlayerRotation;
            public Vector3 CameraPosition;
            public Quaternion CameraRotation;
            public float FieldOfView;
        }
    }
}
