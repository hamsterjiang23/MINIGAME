using System;
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
        public DreamPlaybackCheckpoint DreamCheckpoint { get; private set; }
        public bool PendingCombinedRestore { get; private set; }
        public bool IsTransitioning { get; private set; }
        public string SavePath => Path.Combine(Application.persistentDataPath, "campaign-dream-save.json");
        public string LastMessage { get; private set; }

        public static MiniGameSessionHost Install(GameRuntime runtime, GameRules rules, IGameContent content, DreamSequenceProfile profile)
        {
            if (Current == null)
            {
                var owner = new GameObject("MiniGame Persistent Session");
                Current = owner.AddComponent<MiniGameSessionHost>();
                DontDestroyOnLoad(owner);
            }
            var host = Current;
            host.Runtime = runtime; host.Rules = rules; host.Content = content; host.DreamProfile = profile;
            host.DreamCheckpoint = null; host.EndingCheckpoint = null;
            host.PendingCombinedRestore = false;
            return host;
        }

        public static void Release()
        {
            if (Current == null) return;
            var previous = Current;
            Current = null;
            Destroy(previous.gameObject);
        }

        private void LateUpdate()
        {
            if (Runtime == null || IsTransitioning) return;
            var view = Runtime.GetView();
            if (view.Game.Phase == GamePhase.Ending && view.Game.Ending == EndingType.DreamSea
                && SceneManager.GetActiveScene().name != BeachScene) TransitionTo(BeachScene);
        }

        private void TransitionTo(string sceneName)
        {
            if (IsTransitioning) return;
            IsTransitioning = true;
            var room = FindObjectOfType<MiniGameBootstrap>();
            if (room != null) room.Screen.SetTransitionLocked(true);
            var load = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
            if (load == null) { IsTransitioning = false; throw new InvalidOperationException("无法加载场景：" + sceneName); }
            load.completed += _ => { if (this != null) IsTransitioning = false; };
        }

        public void Save() => SaveTo(SavePath);
        public void SaveTo(string path)
        {
            try
            {
                if (Runtime == null || IsTransitioning) throw new InvalidOperationException("请等待场景切换结束后保存。");
                var dream = FindObjectOfType<DreamSequenceDirector>();
                var beach = FindObjectOfType<DreamEndingController>();
                DreamCheckpoint = dream != null ? dream.CaptureCheckpoint() : null;
                if (beach != null) EndingCheckpoint = beach.CaptureCheckpoint();
                var saved = new DreamSessionSave
                {
                    FormatVersion = 1, Runtime = Runtime.Save(), Dream = DreamCheckpoint,
                    Ending = Runtime.GetView().Game.Ending == EndingType.DreamSea ? EndingCheckpoint : null
                };
                var player = FindObjectOfType<ApartmentPlayer>();
                if (player != null)
                {
                    saved.HasRoomPose = true;
                    saved.PlayerPosition = player.transform.position;
                    saved.PlayerRotation = player.transform.rotation;
                    saved.CameraPosition = player.ViewCamera.transform.position;
                    saved.CameraRotation = player.ViewCamera.transform.rotation;
                    saved.FieldOfView = player.ViewCamera.fieldOfView;
                    var roomHost = FindObjectOfType<MiniGameBootstrap>();
                    if (roomHost != null && roomHost.Effects.TryGetRoomReturnPose(out var actorPosition, out var actorRotation,
                        out var cameraPosition, out var cameraRotation, out var fov))
                    {
                        saved.PlayerPosition = actorPosition; saved.PlayerRotation = actorRotation;
                        saved.CameraPosition = cameraPosition; saved.CameraRotation = cameraRotation; saved.FieldOfView = fov;
                    }
                    var door = player.ViewCamera.GetComponent<DoorViewCamera>();
                    if (door != null && door.HasRoomPose)
                    { saved.CameraPosition = door.ReturnPosition; saved.CameraRotation = door.ReturnRotation; saved.FieldOfView = door.ReturnFieldOfView; }
                    if (DreamCheckpoint != null && DreamCheckpoint.Phase != DreamPlaybackPhase.None)
                    { saved.CameraPosition = DreamCheckpoint.CameraPosition; saved.CameraRotation = DreamCheckpoint.CameraRotation; saved.FieldOfView = DreamCheckpoint.FieldOfView; }
                }
                WriteAtomically(Path.GetFullPath(path), JsonUtility.ToJson(saved, true));
                LastMessage = "梦境流程检查点已保存。";
            }
            catch (Exception exception) { LastMessage = "保存失败：" + exception.Message; Debug.LogWarning(LastMessage); }
        }

        public void Load() => LoadFrom(SavePath);
        public void LoadFrom(string path)
        {
            try
            {
                if (IsTransitioning) throw new InvalidOperationException("请等待场景切换结束后读取。");
                var envelope = JsonUtility.FromJson<DreamSessionSave>(File.ReadAllText(path, Encoding.UTF8));
                if (envelope == null || envelope.FormatVersion != 1 || string.IsNullOrEmpty(envelope.Runtime))
                    throw new InvalidDataException("存档版本不兼容；旧存档文件已保留，请开始新的梦境流程。");
                var restored = GameRuntime.Restore(envelope.Runtime, Rules, Content);
                Runtime = restored;
                // JsonUtility materializes null nested serializable classes as default objects.
                // Core phase, rather than the presence of a visual DTO, decides which state is meaningful.
                var phase = restored.GetView().Game;
                DreamCheckpoint = phase.Phase == GamePhase.Dream && envelope.Dream != null
                    && envelope.Dream.Day == phase.Day && envelope.Dream.Phase != DreamPlaybackPhase.None
                    && Enum.IsDefined(typeof(DreamPlaybackPhase), envelope.Dream.Phase) ? envelope.Dream : null;
                EndingCheckpoint = restored.GetView().Game.Ending == EndingType.DreamSea ? envelope.Ending : null;
                pendingRoomPose = envelope;
                bool beach = restored.GetView().Game.Ending == EndingType.DreamSea;
                if (beach)
                {
                    PendingCombinedRestore = false;
                    var controller = FindObjectOfType<DreamEndingController>();
                    if (controller != null) controller.RestoreCheckpoint(EndingCheckpoint ?? new BeachEndingCheckpoint());
                    else TransitionTo(BeachScene);
                }
                else
                {
                    PendingCombinedRestore = true;
                    var bootstrap = FindObjectOfType<MiniGameBootstrap>();
                    if (bootstrap != null) bootstrap.RestoreFromSession(this);
                    else TransitionTo(CombinedScene);
                }
                LastMessage = "已恢复检查点；没有重复结算反叛值或净化。";
            }
            catch (Exception exception) { LastMessage = "读取失败：" + exception.Message; Debug.LogWarning(LastMessage); }
        }

        private DreamSessionSave pendingRoomPose;
        public void RestoreRoomPose(ApartmentPlayer player)
        {
            if (pendingRoomPose != null && pendingRoomPose.HasRoomPose)
            {
                player.ViewCamera.GetComponent<DoorViewCamera>()?.DiscardRoomPoseCapture();
                var controller = player.GetComponent<CharacterController>();
                bool wasEnabled = controller.enabled;
                controller.enabled = false;
                player.transform.SetPositionAndRotation(pendingRoomPose.PlayerPosition, pendingRoomPose.PlayerRotation);
                if (pendingRoomPose.FieldOfView > 1 && pendingRoomPose.FieldOfView < 180)
                {
                    player.ViewCamera.transform.SetPositionAndRotation(pendingRoomPose.CameraPosition, pendingRoomPose.CameraRotation);
                    player.ViewCamera.fieldOfView = pendingRoomPose.FieldOfView;
                    player.FacePoint(pendingRoomPose.CameraPosition + pendingRoomPose.CameraRotation * Vector3.forward);
                }
                controller.enabled = wasEnabled;
            }
            pendingRoomPose = null;
            PendingCombinedRestore = false;
        }

        public void RestartToCombined()
        {
            if (IsTransitioning) return;
            Release();
            SceneManager.LoadSceneAsync(CombinedScene, LoadSceneMode.Single);
        }

        private void OnDestroy() { if (Current == this) Current = null; }

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
            public BeachEndingCheckpoint Ending;
            public bool HasRoomPose;
            public Vector3 PlayerPosition;
            public Quaternion PlayerRotation;
            public Vector3 CameraPosition;
            public Quaternion CameraRotation;
            public float FieldOfView;
        }
    }
}
