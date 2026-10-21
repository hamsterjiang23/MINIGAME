using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace MiniGame.UnityHost.Editor
{
    /// <summary>Temporary Play Mode presentation without story fast-forward or player-save writes.</summary>
    [InitializeOnLoad]
    public static class DreamCubemapPreview
    {
        private const string Pending = "MiniGame.CubemapPreview.Pending";
        private const string Active = "MiniGame.CubemapPreview.Active";
        private const string Setup = "MiniGame.CubemapPreview.SceneSetup";
        private static RoomDreamEnding owner;
        private static double deadline;
        [Serializable] private sealed class SavedScenes { public SceneSetup[] Scenes; }
        public static bool IsActive => SessionState.GetBool(Active, false) || SessionState.GetBool(Pending, false);
        public static bool IsReady => owner != null && owner.Ready && !SessionState.GetBool(Pending, false);

        static DreamCubemapPreview()
        {
            EditorApplication.playModeStateChanged += OnState;
            if (IsActive && EditorApplication.isPlaying) StartWaiting();
            else if (IsActive && !EditorApplication.isPlayingOrWillChangePlaymode)
                EditorApplication.delayCall += RestoreScenes;
        }
        [MenuItem("MiniGame/梦境/预览房间全景结局")]
        public static void Open()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("请先停止当前 Unity 播放，再启动全景预览。");
            var profile = AssetDatabase.LoadAssetAtPath<DreamSequenceProfile>(DreamSequenceSetup.ProfilePath);
            if (profile == null || profile.RoomPanoramaSkybox == null && profile.CubemapSourcePrefab == null)
                throw new InvalidOperationException("请先在“梦境与结局 → 海滩终局”生成全景捕获源。");
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            SessionState.SetString(Setup, JsonUtility.ToJson(new SavedScenes { Scenes = EditorSceneManager.GetSceneManagerSetup() }));
            SessionState.SetBool(Pending, true);
            SessionState.SetBool(Active, true);
            try
            {
                EditorSceneManager.OpenScene(MainRoomPreviewBuilder.CombinedScenePath, OpenSceneMode.Single);
                EditorApplication.ExecuteMenuItem("Window/General/Game");
                EditorApplication.isPlaying = true;
            }
            catch { RestoreScenes(); throw; }
        }
        [MenuItem("MiniGame/梦境/停止房间全景预览")]
        public static void Stop()
        {
            if (!IsActive) return;
            SessionState.SetBool(Pending, false);
            if (EditorApplication.isPlayingOrWillChangePlaymode) EditorApplication.isPlaying = false;
            else RestoreScenes();
        }
        [MenuItem("MiniGame/梦境/预览房间全景结局", true)]
        private static bool CanOpen() => !EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isCompiling;
        [MenuItem("MiniGame/梦境/停止房间全景预览", true)]
        private static bool CanStop() => IsActive;

        private static void OnState(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingPlayMode && IsActive && owner != null)
                owner.StopAndRestore();
            if (state == PlayModeStateChange.EnteredPlayMode && IsActive) StartWaiting();
            if (state == PlayModeStateChange.EnteredEditMode && IsActive) EditorApplication.delayCall += RestoreScenes;
        }
        private static void StartWaiting()
        {
            deadline = EditorApplication.timeSinceStartup + 25;
            EditorApplication.update -= Tick; EditorApplication.update += Tick;
        }
        private static void Tick()
        {
            if (!EditorApplication.isPlaying) return;
            try
            {
                if (owner == null)
                {
                    owner = UnityEngine.Object.FindObjectOfType<RoomDreamEnding>();
                    if (owner == null)
                    {
                        var host = UnityEngine.Object.FindObjectOfType<MiniGameBootstrap>();
                        var session = MiniGameSessionHost.Current;
                        if (host != null && host.Runtime != null && session != null)
                            owner = RoomDreamEnding.Begin(host, session, true);
                    }
                }
                if (owner != null && !string.IsNullOrEmpty(owner.FailureReason)) throw new InvalidOperationException(owner.FailureReason);
                if (owner != null && owner.Ready)
                {
                    SessionState.SetBool(Pending, false); SessionState.SetBool(Active, true);
                    EditorApplication.update -= Tick;
                    Debug.Log("MINIGAME_CUBEMAP_PREVIEW_READY WASD / mouse / E; no player save written");
                }
                else if (EditorApplication.timeSinceStartup > deadline) throw new TimeoutException("房间全景预览启动超时，请检查 Console。");
            }
            catch (Exception exception) { Debug.LogException(exception); Stop(); }
        }
        private static void RestoreScenes()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            EditorApplication.update -= Tick; owner = null;
            string json = SessionState.GetString(Setup, "");
            SessionState.EraseBool(Pending); SessionState.EraseBool(Active); SessionState.EraseString(Setup);
            var saved = string.IsNullOrEmpty(json) ? null : JsonUtility.FromJson<SavedScenes>(json);
            if (saved?.Scenes != null && saved.Scenes.Length > 0 && saved.Scenes.All(scene => !string.IsNullOrEmpty(scene.path)))
                EditorSceneManager.RestoreSceneManagerSetup(saved.Scenes);
        }
    }
}
