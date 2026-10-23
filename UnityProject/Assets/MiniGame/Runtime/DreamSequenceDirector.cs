using MiniGame.Presentation;
using MiniGame.Core;
using UnityEngine;

namespace MiniGame.UnityHost
{
    [DisallowMultipleComponent]
    public sealed class DreamSequenceDirector : MonoBehaviour
    {
        [SerializeField] private Renderer wall;
        [SerializeField] private Transform viewingPose;
        [SerializeField] private Renderer projectionSurface;
        [SerializeField] private Camera viewCamera;
        [SerializeField] private DreamSequenceProfile profile;
        private GameRuntime runtime;
        private MiniGameScreen screen;
        private ApartmentPlayer player;
        private MiniGameAudioDirector audio;
        private BedtimeGuidance bedtime;
        private DreamAnimationStage stage;
        private Material projectionMaterial;
        private Material[] originalMaterials;
        private bool originalProjectionEnabled;
        private bool preview, hasRoomPose;
        private string activeToken;
        private float elapsed, roomFieldOfView, viewingFieldOfView;
        private Vector3 roomPosition;
        private Quaternion roomRotation;
        private bool lastAudioPlaying, lastAudioPaused;
        private RetroScreenEffect retro;
        private bool capturedRetro, previousRetroBypass;
        private int lastAudioDay = -1;
#if UNITY_EDITOR
        private double editorClock;
#endif

        public DreamPlaybackPhase Phase { get; private set; }
        public bool OwnsCamera => hasRoomPose && Phase != DreamPlaybackPhase.None;
        public bool IsPlaying => Phase == DreamPlaybackPhase.Playing;
        public bool MusicActive => Phase == DreamPlaybackPhase.Aligning || IsPlaying;
        public bool IsPreview => preview;
        public bool PreviewPaused { get; set; }
        public bool UseExternalClock { get; set; }
        public int ActiveDay { get; private set; }
        public float PlaybackSeconds { get; private set; }
        public RenderTexture StageTexture => stage?.Texture;

        public void Configure(Renderer projectionWall, Transform pose, Renderer surface, Camera camera, DreamSequenceProfile settings = null)
        {
            StopInternal();
            wall = projectionWall; viewingPose = pose; projectionSurface = surface; viewCamera = camera;
            if (settings != null) profile = settings;
            if (projectionSurface != null) projectionSurface.enabled = false;
        }

        public void Bind(GameRuntime game, MiniGameScreen ui, ApartmentPlayer owner, MiniGameAudioDirector sound, DreamSequenceProfile settings)
        {
            PrepareForRuntimeChange();
            runtime = game; screen = ui; player = owner; audio = sound; profile = settings;
            bedtime = GetComponent<BedtimeGuidance>();
            lastAudioDay = -1;
            if (runtime == null || !isActiveAndEnabled) return;
            runtime.Changed += OnViewChanged;
            OnViewChanged(runtime.GetView());
        }

        public void PrepareForRuntimeChange()
        {
            if (runtime != null) runtime.Changed -= OnViewChanged;
            StopInternal();
        }

        private void OnViewChanged(RuntimeView view)
        {
            if (preview || !isActiveAndEnabled) return;
            bool dream = runtime.UsesDreamFlow && view.Cue?.Kind == PresentationKind.Dream;
            if (!dream)
            {
                if (OwnsCamera) StopInternal();
                return;
            }
            if (activeToken == view.Cue.Token) { UpdateAudio(MusicActive, view.Paused); return; }
            StartDay(view.Game.Day, view.Cue.Token, false);
        }

        public bool PreviewDay(int day)
        {
            if (OwnsCamera && !preview) return false;
            bool started = StartDay(day, null, true);
#if UNITY_EDITOR
            if (started && !Application.isPlaying)
            {
                editorClock = UnityEditor.EditorApplication.timeSinceStartup;
                UnityEditor.EditorApplication.update += EditorTick;
                UnityEditor.SceneManagement.EditorSceneManager.sceneSaving += BeforeSceneSave;
                UnityEditor.EditorApplication.playModeStateChanged += BeforePlayModeChange;
            }
#endif
            return started;
        }

        public void StopPreview() { if (preview) StopInternal(); }

        private bool StartDay(int day, string token, bool isPreview)
        {
            if (profile == null || profile.GetDay(day) == null || wall == null || viewingPose == null
                || projectionSurface == null || viewCamera == null || !isActiveAndEnabled) return false;
            StopInternal();
            roomPosition = viewCamera.transform.position;
            roomRotation = viewCamera.transform.rotation;
            roomFieldOfView = viewCamera.fieldOfView;
            hasRoomPose = true;
            retro = viewCamera.GetComponent<RetroScreenEffect>();
            capturedRetro = retro != null;
            previousRetroBypass = capturedRetro && retro.PresentationBypass;
            ActiveDay = day; activeToken = token; preview = isPreview; PreviewPaused = false;
            elapsed = 0; PlaybackSeconds = 0;
            originalMaterials = projectionSurface.sharedMaterials;
            originalProjectionEnabled = projectionSurface.enabled;
            stage = new DreamAnimationStage();
            var shader = Resources.Load<Shader>("DreamProjectionSurface");
            if (shader == null || !shader.isSupported)
            {
                StopInternal();
                Debug.LogError("DreamProjectionSurface shader is unavailable.", this);
                return false;
            }
            projectionMaterial = new Material(shader) { name = "Wall dream projection", hideFlags = HideFlags.HideAndDontSave };
            projectionMaterial.mainTexture = stage.Texture;
            projectionSurface.sharedMaterial = projectionMaterial;
            projectionSurface.enabled = false;
            viewingFieldOfView = FramedFieldOfView();
            Phase = DreamPlaybackPhase.Aligning;
            if (screen != null) { screen.SetTransitionLocked(true); screen.SetInteractionCursor(InteractionCursorKind.None, false); screen.SetFade(1); screen.SetGlitch(0); }
            RenderStage();
            UpdateAudio(true, runtime != null && !preview && runtime.GetView().Paused);
            return true;
        }

        private void LateUpdate()
        {
            if (Application.isPlaying && !UseExternalClock) Advance(Time.unscaledDeltaTime);
        }

        public void Advance(float deltaTime)
        {
            if (!OwnsCamera || stage == null) return;
            bool paused = PreviewPaused || (!preview && runtime != null && runtime.GetView().Paused);
            UpdateAudio(MusicActive, paused);
            if (paused) return;
            float delta = Finite(deltaTime) ? Mathf.Max(0, deltaTime) : 0;
            elapsed += delta;
            switch (Phase)
            {
                case DreamPlaybackPhase.Aligning:
                    ApplyAlignment();
                    if (elapsed >= BlackDuration)
                    {
                        ApplyViewingPose();
                        Phase = DreamPlaybackPhase.Playing;
                        SetRetroBypass(true);
                        elapsed = 0;
                        projectionSurface.enabled = true;
                        screen?.SetFade(0);
                        RenderStage();
                        UpdateAudio(true, false);
                    }
                    break;
                case DreamPlaybackPhase.Playing:
                    ApplyViewingPose();
                    PlaybackSeconds = Mathf.Min(PlaybackDuration, PlaybackSeconds + delta);
                    RenderStage();
                    if (PlaybackSeconds >= PlaybackDuration)
                    {
                        Phase = DreamPlaybackPhase.Returning;
                        elapsed = 0;
                        UpdateAudio(false, false);
                    }
                    break;
                case DreamPlaybackPhase.Returning:
                    ApplyViewingPose();
                    ApplyReturnFade(Smooth(elapsed / ReturnDuration));
                    if (elapsed >= ReturnDuration)
                    {
                        if (ReturnsToMorning)
                        {
                            CompleteIntoWake();
                            break;
                        }
                        Phase = DreamPlaybackPhase.BlackHold;
                        elapsed = 0;
                        projectionSurface.enabled = false;
                        SetRetroBypass(false);
                        ApplyReturn();
                        bedtime?.SetDreamReturnReveal(true);
                    }
                    break;
                case DreamPlaybackPhase.BlackHold:
                    ApplyReturn();
                    screen?.SetFade(1);
                    if (elapsed >= WakeBlackDuration) { Phase = DreamPlaybackPhase.Revealing; elapsed = 0; }
                    break;
                case DreamPlaybackPhase.Revealing:
                    ApplyReturn();
                    screen?.SetFade(1 - Smooth(elapsed / ReturnDuration));
                    if (elapsed >= ReturnDuration)
                    {
                        string token = activeToken;
                        bool complete = !preview && runtime != null && runtime.GetView().Cue?.Token == token;
                        StopInternal(complete);
                        if (complete) runtime.CompleteCue(token);
                        bedtime?.SetDreamReturnReveal(false);
                    }
                    break;
            }
        }

        private void RenderStage()
        {
            var day = profile.GetDay(ActiveDay);
            stage.Render(day, PlaybackSeconds);
            if (projectionMaterial != null)
                projectionMaterial.SetFloat("_Opacity", 1);
        }

        private float FramedFieldOfView()
        {
            var bounds = projectionSurface.localBounds;
            var center = projectionSurface.transform.TransformPoint(bounds.center);
            float halfWidth = 0, halfHeight = 0;
            for (int x = -1; x <= 1; x += 2)
                for (int y = -1; y <= 1; y += 2)
                    for (int z = -1; z <= 1; z += 2)
                    {
                        var corner = projectionSurface.transform.TransformPoint(bounds.center + Vector3.Scale(bounds.extents, new Vector3(x, y, z))) - center;
                        halfWidth = Mathf.Max(halfWidth, Mathf.Abs(Vector3.Dot(corner, viewingPose.right)));
                        halfHeight = Mathf.Max(halfHeight, Mathf.Abs(Vector3.Dot(corner, viewingPose.up)));
                    }
            float distance = Mathf.Max(.1f, Vector3.Dot(center - viewingPose.position, viewingPose.forward));
            float extent = Mathf.Max(halfHeight, halfWidth / Mathf.Max(.1f, viewCamera.aspect));
            return Mathf.Clamp(2 * Mathf.Atan(extent * 1.06f / distance) * Mathf.Rad2Deg, 15, 110);
        }

        private void ApplyAlignment()
        {
            float p = Smooth(elapsed / Mathf.Min(AlignDuration, BlackDuration));
            viewCamera.transform.SetPositionAndRotation(Vector3.Lerp(roomPosition, viewingPose.position, p), Quaternion.Slerp(roomRotation, viewingPose.rotation, p));
            viewCamera.fieldOfView = Mathf.Lerp(roomFieldOfView, viewingFieldOfView, p);
        }
        private void ApplyViewingPose()
        {
            viewCamera.transform.SetPositionAndRotation(viewingPose.position, viewingPose.rotation);
            viewCamera.fieldOfView = viewingFieldOfView;
        }
        private void ApplyReturn()
        {
            viewCamera.transform.SetPositionAndRotation(roomPosition, ReturnRotation);
            viewCamera.fieldOfView = roomFieldOfView;
        }

        private void ApplyReturnFade(float opacity)
        {
            screen?.SetFade(opacity);
            if (projectionMaterial != null) projectionMaterial.SetFloat("_Opacity", 1 - opacity);
        }

        private bool ReturnsToMorning => !preview && runtime != null
            && runtime.GetView().Game.DreamReturnPhase == GamePhase.AfterDream;

        private void CompleteIntoWake()
        {
            string token = activeToken;
            bool complete = runtime.GetView().Cue?.Token == token;
            // Release the projection before Changed starts the wake camera. Keep this handoff fully black.
            StopInternal(preserveBlack: true);
            if (complete) runtime.CompleteCue(token);
        }

        public DreamPlaybackCheckpoint CaptureCheckpoint() => new DreamPlaybackCheckpoint
        {
            Phase = preview ? DreamPlaybackPhase.None : Phase, Day = ActiveDay,
            Elapsed = elapsed, PlaybackSeconds = PlaybackSeconds,
            CameraPosition = roomPosition, CameraRotation = roomRotation, FieldOfView = roomFieldOfView
        };

        public void RestoreCheckpoint(DreamPlaybackCheckpoint data)
        {
            if (data == null || data.Phase == DreamPlaybackPhase.None || !OwnsCamera || preview || runtime == null
                || runtime.GetView().Cue?.Kind != PresentationKind.Dream || data.Day != ActiveDay) return;
            if (!Finite(data.CameraPosition.x) || !Finite(data.CameraPosition.y) || !Finite(data.CameraPosition.z)
                || !Finite(data.CameraRotation.x) || !Finite(data.CameraRotation.y) || !Finite(data.CameraRotation.z)
                || !Finite(data.CameraRotation.w)) return;
            roomPosition = data.CameraPosition;
            roomRotation = Quaternion.Normalize(data.CameraRotation);
            roomFieldOfView = Finite(data.FieldOfView) && data.FieldOfView > 0 ? Mathf.Clamp(data.FieldOfView, 1, 179) : roomFieldOfView;
            PlaybackSeconds = Finite(data.PlaybackSeconds) ? Mathf.Clamp(data.PlaybackSeconds, 0, PlaybackDuration) : 0;
            Phase = data.Phase;
            elapsed = Finite(data.Elapsed) ? Mathf.Max(0, data.Elapsed) : 0;
            // Old saves could be standing in the room waiting to reveal a second bed interaction.
            // Resume them at the black handoff, leaving paused saves uncommitted until Advance resumes.
            if (ReturnsToMorning && (Phase == DreamPlaybackPhase.BlackHold || Phase == DreamPlaybackPhase.Revealing))
            { Phase = DreamPlaybackPhase.Returning; elapsed = ReturnDuration; }
            if (Phase == DreamPlaybackPhase.Aligning) { elapsed = Mathf.Min(elapsed, BlackDuration); ApplyAlignment(); }
            else if (Phase == DreamPlaybackPhase.Playing) ApplyViewingPose();
            else if (Phase == DreamPlaybackPhase.Returning) { elapsed = Mathf.Min(elapsed, ReturnDuration); ApplyViewingPose(); }
            else if (Phase == DreamPlaybackPhase.BlackHold) { elapsed = Mathf.Min(elapsed, WakeBlackDuration); ApplyReturn(); }
            else if (Phase == DreamPlaybackPhase.Revealing) { elapsed = Mathf.Min(elapsed, ReturnDuration); ApplyReturn(); }
            else { StopInternal(); return; }
            projectionSurface.enabled = IsPlaying || Phase == DreamPlaybackPhase.Returning;
            screen?.SetFade(Phase == DreamPlaybackPhase.Aligning || Phase == DreamPlaybackPhase.BlackHold ? 1
                : Phase == DreamPlaybackPhase.Revealing ? 1 - Smooth(elapsed / ReturnDuration) : 0);
            bedtime?.SetDreamReturnReveal(Phase == DreamPlaybackPhase.BlackHold || Phase == DreamPlaybackPhase.Revealing);
            SetRetroBypass(IsPlaying || Phase == DreamPlaybackPhase.Returning);
            RenderStage();
            if (Phase == DreamPlaybackPhase.Returning) ApplyReturnFade(Smooth(elapsed / ReturnDuration));
            UpdateAudio(MusicActive, runtime.GetView().Paused);
        }

        private void UpdateAudio(bool playing, bool paused)
        {
            if (audio == null || (lastAudioPlaying == playing && lastAudioPaused == paused && lastAudioDay == ActiveDay)) return;
            audio.SetDreamPlayback(playing, ActiveDay, paused);
            lastAudioPlaying = playing; lastAudioPaused = paused; lastAudioDay = ActiveDay;
        }

        private void StopInternal(bool faceReturnTarget = false, bool preserveBlack = false)
        {
            SetRetroBypass(false);
            capturedRetro = false;
            retro = null;
#if UNITY_EDITOR
            UnityEditor.EditorApplication.update -= EditorTick;
            UnityEditor.SceneManagement.EditorSceneManager.sceneSaving -= BeforeSceneSave;
            UnityEditor.EditorApplication.playModeStateChanged -= BeforePlayModeChange;
#endif
            UpdateAudio(false, false);
            if (hasRoomPose && viewCamera != null)
            {
                Quaternion rotation = faceReturnTarget ? ReturnRotation : roomRotation;
                viewCamera.transform.SetPositionAndRotation(roomPosition, rotation);
                viewCamera.fieldOfView = roomFieldOfView;
                // Release ownership before synchronizing the first-person look angles.
                Phase = DreamPlaybackPhase.None;
                if (player != null) player.FacePoint(roomPosition + rotation * Vector3.forward);
            }
            if (projectionSurface != null && originalMaterials != null)
            {
                projectionSurface.sharedMaterials = originalMaterials;
                projectionSurface.enabled = originalProjectionEnabled;
            }
            originalMaterials = null;
            stage?.Dispose(); stage = null;
            if (projectionMaterial != null)
            {
                if (Application.isPlaying) Destroy(projectionMaterial); else DestroyImmediate(projectionMaterial);
            }
            projectionMaterial = null;
            if (hasRoomPose && screen != null) { screen.SetTransitionLocked(preserveBlack); screen.SetFade(preserveBlack ? 1 : 0); }
            if (!faceReturnTarget) bedtime?.SetDreamReturnReveal(false);
            hasRoomPose = false; preview = false; PreviewPaused = false;
            Phase = DreamPlaybackPhase.None; activeToken = null;
        }

        private void SetRetroBypass(bool playing)
        {
            if (capturedRetro && retro != null) retro.PresentationBypass = previousRetroBypass || playing;
        }

        private float AlignDuration => SafeDuration(profile.AlignSeconds, 1.2f);
        private float BlackDuration => SafeDuration(profile.BlackScreenSeconds, 4.5f);
        private float ReturnDuration => SafeDuration(profile.ReturnSeconds, 1);
        private float WakeBlackDuration => SafeDuration(profile.WakeBlackSeconds, 1);
        private float PlaybackDuration => SafeDuration(profile.GetDay(ActiveDay).Duration, 10);
        private Quaternion ReturnRotation => !preview && bedtime != null && (bedtime.DreamReturnFocusPoint - roomPosition).sqrMagnitude > .0001f
            ? Quaternion.LookRotation(bedtime.DreamReturnFocusPoint - roomPosition, Vector3.up) : roomRotation;
        private static float SafeDuration(float value, float fallback) => Finite(value) ? Mathf.Max(.01f, value) : fallback;
        private static float Smooth(float value) { float p = Mathf.Clamp01(value); return p * p * (3 - 2 * p); }
        private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
        private void OnDisable() => PrepareForRuntimeChange();
        private void OnDestroy() => PrepareForRuntimeChange();
        private void OnEnable() { if (runtime != null && profile != null) Bind(runtime, screen, player, audio, profile); }
#if UNITY_EDITOR
        private void EditorTick()
        {
            if (Application.isPlaying || !preview) return;
            double now = UnityEditor.EditorApplication.timeSinceStartup;
            Advance((float)(now - editorClock)); editorClock = now;
            UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
        }
        private void BeforeSceneSave(UnityEngine.SceneManagement.Scene scene, string path) => StopPreview();
        private void BeforePlayModeChange(UnityEditor.PlayModeStateChange state) => StopPreview();
#endif
    }
}
