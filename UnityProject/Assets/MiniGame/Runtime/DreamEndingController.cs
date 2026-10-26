using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MiniGame.UnityHost
{
    /// <summary>The ending is presentation only; the persistent session owns the actual ending.</summary>
    [DisallowMultipleComponent]
    public sealed class DreamEndingController : MonoBehaviour
    {
        public const float SkyHoldSeconds = SupervisorInterruptSeconds + PostDialogueSeconds;
        public const float SupervisorInterruptSeconds = 7f;
        public const float PostDialogueSeconds = 36f;
        public const float CreditsSeconds = 20f;
        public const float CreditsStartSeconds = PostDialogueSeconds - CreditsSeconds;
        private DreamSupervisorDialogue supervisorDialogue;
        private DreamEndingCredits credits;
        public DreamEndingCredits Credits => credits;
        private int dialoguePresentedFrame = -1;
        public bool SupervisorDialogueActive => UsesBedEndingSequence && state.Phase == BeachEndingPhase.SkyHold
            && state.PhaseElapsed >= SupervisorInterruptSeconds && !state.SupervisorDialogueComplete;
        public int SupervisorDialogueIndex => state.SupervisorDialogueIndex;
        public string SupervisorSubtitle => supervisorDialogue != null ? supervisorDialogue.Text : string.Empty;
        public const float EndingFadeSeconds = 1f;
        [SerializeField] private DreamSequenceProfile profile;
        [SerializeField] private Camera viewCamera;
        [SerializeField] private Transform bed;
        [SerializeField] private Transform pillow;
        [SerializeField] private Transform approach;
        [SerializeField] private Renderer sea;
        private BeachEndingCheckpoint state = new BeachEndingCheckpoint();
        private Vector3 startPosition, walkDirection, bedBasePosition, pillowLocal;
        private Quaternion standingRotation, bedBaseRotation;
        private float pathLength, worldTime;
        private bool initialized, localPaused, audioPaused;
        private int pauseClosedFrame = -1, lastInteractionInputFrame = -1;
        private Material skyMaterial, originalSky;
        private sealed class SurfaceMaterials
        {
            public Renderer Renderer;
            public Material[] Originals, Instances;
        }
        private readonly List<SurfaceMaterials> waterSurfaces = new List<SurfaceMaterials>();
        private MiniGameAudioService audioService;
        private OceanFloatTrack oceanFloatTrack;
        private AudioClip oceanFloatClip;
        private int oceanPoseSample = -1;
        private Canvas canvas;
        private Image endingFade;
        private int opaqueFrame = -1;
        private Text hint, caption;
        private InteractionCursorView interactionCursor;
        private PauseMenuView pauseMenu;
        private Font font;
        private int voiceIndex = -1;
        private RoomDreamEnding roomEnding;
        private bool previewMode, thanksActive;
        public bool IsPreview => previewMode;
        public bool IsRoomCubemapEnding => roomEnding != null;
        public float WorldTime => worldTime;
        public BeachEndingPhase Phase => state.Phase;
        private bool UsesBedEndingSequence => roomEnding != null && (roomEnding.UsesSharedThanks || roomEnding.IsPortalPreview);
        // Keep the initial half-duration turn; double only the subsequent recline/crossing time.
        public float LieDownDuration => Safe(profile.LieDownSeconds, 1.5f, .01f)
            * (roomEnding?.Portal != null ? 1.5f : 1);
        private float LieDownProgress
        {
            get
            {
                float elapsed = state.PhaseElapsed / Safe(profile.LieDownSeconds, 1.5f, .01f);
                return Mathf.Clamp01(roomEnding?.Portal != null && elapsed > .5f
                    ? .5f + (elapsed - .5f) * .5f : elapsed);
            }
        }
        public float EndingFadeOpacity => endingFade != null ? endingFade.color.a : 0;
        public bool IsBedEndingComplete => UsesBedEndingSequence && state.Phase == BeachEndingPhase.FadingOut
            && state.PhaseElapsed >= EndingFadeSeconds;
        public Camera ViewCamera => viewCamera;
        public DreamSequenceProfile Profile => profile;
        public Transform Bed => bed;
        public MiniGameAudioService Audio => audioService;
        public Vector3 OceanFloatPose { get; private set; }
        public bool UseExternalClock { get; set; }
        public bool Paused => MiniGameSessionHost.Current?.Runtime?.GetView().Paused ?? localPaused;
        public string CurrentSubtitle => caption != null ? caption.text : string.Empty;
        public bool BedInteractionFocused => CanLieDown && (roomEnding == null || roomEnding.Portal == null || roomEnding.IsBedFocused());
        public bool CanLieDown => initialized && !Paused && state.Phase == BeachEndingPhase.Walking
            && (roomEnding == null || roomEnding.Ready)
            && (roomEnding != null ? roomEnding.IsNearBed(Safe(profile.BedInteractionDistance, 1.5f, .1f))
                : pathLength - state.WalkDistance <= Safe(profile.BedInteractionDistance, 1.5f, .1f));
        private float MaximumWalkDistance => Mathf.Max(0, pathLength
            - Mathf.Min(.65f, Safe(profile.BedInteractionDistance, 1.5f, .1f) * .8f));

        public void Configure(DreamSequenceProfile settings, Camera camera, Transform bedRoot,
            Transform pillowPose, Transform startingPose, Renderer seaRenderer = null)
        {
            profile = settings;
            viewCamera = camera;
            bed = bedRoot;
            pillow = pillowPose;
            approach = startingPose;
            sea = seaRenderer;
            if (Application.isPlaying) Initialize();
        }

        private void Start() => Initialize();

        public void ConfigureRoom(DreamSequenceProfile settings, Camera camera, Transform bedRoot,
            Transform pillowPose, Transform startingPose, RoomDreamEnding owner)
        {
            roomEnding = owner ?? throw new ArgumentNullException(nameof(owner));
            previewMode = owner.IsPreview;
            Configure(settings, camera, bedRoot, pillowPose, startingPose);
        }

        private void Initialize()
        {
            if (initialized) return;
            var host = MiniGameSessionHost.Current;
            if (host != null && host.DreamProfile != null) profile = host.DreamProfile;
            if (profile == null || viewCamera == null || bed == null || pillow == null || approach == null)
            {
                Debug.LogError("海滩终局缺少配置、相机、床或枕头定位点。", this);
                enabled = false;
                return;
            }
            initialized = true;
            PlayerBrightnessEffect.EnsureFor(viewCamera);
            startPosition = approach.position;
            standingRotation = approach.rotation;
            walkDirection = Vector3.ProjectOnPlane(approach.forward, Vector3.up).normalized;
            if (walkDirection.sqrMagnitude < .5f) walkDirection = Vector3.forward;
            bedBasePosition = bed.position;
            bedBaseRotation = bed.rotation;
            pillowLocal = bed.InverseTransformPoint(pillow.position);
            pathLength = Mathf.Max(0, Vector3.Dot(bed.position - startPosition, walkDirection));
            originalSky = RenderSettings.skybox;
            if (roomEnding == null && originalSky != null)
            {
                skyMaterial = new Material(originalSky);
                RenderSettings.skybox = skyMaterial;
            }
            if (roomEnding == null) CreateWaterMaterials();
            audioService = gameObject.AddComponent<MiniGameAudioService>();
            audioService.Configure(profile.Audio);
            audioService.SetLoop(MiniGameAudioChannel.EndingMusic, MiniGameSound.EndingMusic, 2);
            audioService.SetLoop(MiniGameAudioChannel.Ocean, MiniGameSound.OceanWaves, 2);
            BuildInterface();
            RestoreCheckpoint(host?.EndingCheckpoint ?? new BeachEndingCheckpoint());
        }

        private void Update()
        {
            if (MainMenuController.BlocksGameplayInput) return;
            if (initialized) ApplyPause();
            if (initialized && !UseExternalClock && (roomEnding == null || roomEnding.Ready)
                && Input.GetKeyDown(KeyCode.Escape))
            {
                HandlePauseEscape();
                return;
            }
            // Present a full black frame before the menu takes over, including after a restore.
            if (initialized && !Paused && roomEnding != null && roomEnding.Ready && IsBedEndingComplete
                && opaqueFrame >= 0 && Time.frameCount > opaqueFrame)
            {
                roomEnding.CompleteAtBed();
                return;
            }
            if (!initialized || UseExternalClock || roomEnding != null && !roomEnding.Ready) return;
            if (roomEnding != null && state.Phase == BeachEndingPhase.Walking)
                roomEnding.AdvanceExploration(new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")),
                    new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")), Time.unscaledDeltaTime);
            AdvanceForTests(Input.GetKey(KeyCode.W) ? 1 : 0, Time.unscaledDeltaTime);
            HandleInteractionInput(Input.GetKeyDown(KeyCode.E), Input.GetMouseButtonDown(0));
        }

        private void LateUpdate()
        {
            if (!initialized || audioService == null) return;
            // Use the final camera pose, including the monitor-return entrance and the lie-down motion.
            bool facing = !thanksActive && !Paused && !MainMenuController.BlocksGameplayInput
                && (SupervisorDialogueActive || roomEnding != null && roomEnding.IsFacingPrintedMonitor);
            audioService.SetLoop(MiniGameAudioChannel.ComputerNoise,
                facing ? MiniGameSound.PCNoise : (MiniGameSound?)null, facing ? .08f : 0);
        }

        public bool HandleInteractionInput(bool interactPressed, bool primaryPressed)
        {
            if ((!interactPressed && !primaryPressed) || pauseClosedFrame == Time.frameCount
                || lastInteractionInputFrame == Time.frameCount || MainMenuController.BlocksGameplayInput) return false;
            lastInteractionInputFrame = Time.frameCount;
            if (SupervisorDialogueActive)
            {
                if (Paused || dialoguePresentedFrame == Time.frameCount || !primaryPressed) return false;
                supervisorDialogue.Advance();
                return true;
            }
            if (roomEnding?.Portal != null && !BedInteractionFocused) return false;
            return TryLieDown();
        }

        public void SetPaused(bool value)
        {
            if (Paused && !value) pauseClosedFrame = Time.frameCount;
            localPaused = value;
            MiniGameSessionHost.Current?.Runtime?.SetPaused(value);
            ApplyPause();
        }

        public void HandlePauseEscape()
        {
            if (Paused && pauseMenu != null && (pauseMenu.TryCloseGm()
                || pauseMenu.TryCloseSettings() || pauseMenu.TryCloseManual())) return;
            SetPaused(!Paused);
        }

        public void HideForThanks()
        {
            supervisorDialogue?.Hide();
            credits?.Refresh(0, false);
            thanksActive = true;
            interactionCursor?.SetVisible(false);
            roomEnding?.SetBedFocus(false);
            if (hint != null) hint.gameObject.SetActive(false);
            if (caption != null) caption.gameObject.SetActive(false);
            pauseMenu?.SetVisible(Paused && !MainMenuController.BlocksGameplayInput);
            audioService?.StopAll();
            enabled = false;
        }

        public void RefreshPausePresentation() => ApplyPause();

        /// <summary>Forward is clamped to [0,1]; reverse and lateral input have no route.</summary>
        public void AdvanceForTests(float forward, float dt)
        {
            if (!initialized || roomEnding != null && !roomEnding.Ready) return;
            ApplyPause();
            if (Paused) { ApplyWaterParameters(); RenderRoomCubemap(); return; }
            dt = Finite(dt) ? Mathf.Max(0, dt) : 0;
            if (SupervisorDialogueActive) { supervisorDialogue.Tick(dt); return; }
            // Discard overshoot at the interruption; waiting for a click cannot consume the ending.
            if (UsesBedEndingSequence && !state.SupervisorDialogueComplete)
            {
                float until = state.Phase == BeachEndingPhase.Lying
                    ? Mathf.Max(0, LieDownDuration - state.PhaseElapsed) + SupervisorInterruptSeconds
                    : state.Phase == BeachEndingPhase.SkyHold ? Mathf.Max(0, SupervisorInterruptSeconds - state.PhaseElapsed) : float.PositiveInfinity;
                dt = Mathf.Min(dt, until);
            }
            worldTime += dt;
            if (state.Phase == BeachEndingPhase.Walking)
            {
                float input = roomEnding == null && Finite(forward) ? Mathf.Clamp01(forward) : 0;
                state.WalkDistance = Mathf.Min(MaximumWalkDistance,
                    state.WalkDistance + input * Safe(profile.WalkSpeed, 1.2f, .01f) * dt);
            }
            else if (UsesBedEndingSequence)
            {
                AdvanceBedEnding(dt);
            }
            else if (state.Phase == BeachEndingPhase.Lying)
            {
                state.PhaseElapsed += dt;
                float duration = LieDownDuration;
                if (state.PhaseElapsed >= duration)
                {
                    float remaining = state.PhaseElapsed - duration;
                    state.Phase = BeachEndingPhase.Drifting;
                    state.PhaseElapsed = 0;
                    state.DriftElapsed = remaining;
                    state.SubtitleElapsed = remaining - Safe(profile.SubtitleDelay, 2, 0);
                    AdvanceSubtitles(0);
                }
            }
            else
            {
                state.DriftElapsed += dt;
                AdvanceSubtitles(dt);
            }
            ApplyPose();
            if (SupervisorDialogueActive) BeginSupervisorDialogue();
            RefreshInterface();
            PublishCheckpoint();
        }

        private void AdvanceBedEnding(float dt)
        {
            // Carry overshoot between stages; the supervisor interruption is handled before this clock.
            for (int stage = 0; stage < 3; stage++)
            {
                float duration = state.Phase == BeachEndingPhase.Lying ? LieDownDuration
                    : state.Phase == BeachEndingPhase.SkyHold ? SkyHoldSeconds : EndingFadeSeconds;
                float used = Mathf.Min(dt, Mathf.Max(0, duration - state.PhaseElapsed));
                state.PhaseElapsed += used;
                dt -= used;
                if (state.PhaseElapsed < duration || state.Phase == BeachEndingPhase.FadingOut) break;
                state.Phase = state.Phase == BeachEndingPhase.Lying ? BeachEndingPhase.SkyHold : BeachEndingPhase.FadingOut;
                state.PhaseElapsed = 0;
            }
        }

        private void BeginSupervisorDialogue()
        {
            dialoguePresentedFrame = Time.frameCount;
            supervisorDialogue.Present(state.SupervisorDialogueIndex, () =>
            {
                state.SupervisorDialogueIndex++;
                if (state.SupervisorDialogueIndex < DreamSupervisorDialogue.Lines.Length) BeginSupervisorDialogue();
                else
                {
                    state.SupervisorDialogueComplete = true;
                    supervisorDialogue.Hide();
                    audioService.Stop(MiniGameAudioChannel.ComputerNoise);
                }
                ApplyPause(true);
                PublishCheckpoint();
            });
            audioService.SetLoop(MiniGameAudioChannel.ComputerNoise, MiniGameSound.PCNoise, 0);
            ApplyPause(true);
        }

        public bool TryLieDown()
        {
            if (!CanLieDown) return false;
            state.Phase = BeachEndingPhase.Lying;
            if (roomEnding != null)
            {
                state.LieStartPosition = viewCamera.transform.position;
                state.LieStartRotation = viewCamera.transform.rotation;
            }
            state.PhaseElapsed = 0;
            state.SubtitleIndex = 0;
            state.SubtitleElapsed = -Safe(profile.SubtitleDelay, 2, 0);
            state.SubtitlesComplete = false;
            RefreshInterface();
            PublishCheckpoint();
            return true;
        }

        private void AdvanceSubtitles(float dt)
        {
            if (state.SubtitlesComplete) return;
            state.SubtitleElapsed += dt;
            var lines = profile.EndingLines;
            if (lines == null || lines.Length == 0) { state.SubtitlesComplete = true; return; }
            while (state.SubtitleIndex < lines.Length)
            {
                var line = lines[state.SubtitleIndex];
                float length = line != null && line.Voice != null ? line.Voice.length
                    : Safe(line?.Duration ?? 4, 4, .1f);
                if (state.SubtitleElapsed < length) break;
                state.SubtitleElapsed -= length;
                state.SubtitleIndex++;
                voiceIndex = -1;
            }
            if (state.SubtitleIndex >= lines.Length)
            {
                state.SubtitlesComplete = true;
                audioService.Stop(MiniGameAudioChannel.Voice);
                return;
            }
            var current = lines[state.SubtitleIndex];
            if (state.SubtitleElapsed >= 0 && voiceIndex != state.SubtitleIndex)
            {
                voiceIndex = state.SubtitleIndex;
                if (current?.Voice != null) audioService.PlayVoice(current.Voice, state.SubtitleElapsed, 1);
            }
        }

        private void ApplyPose(int? restoredOceanSample = null)
        {
            bed.position = bedBasePosition;
            bed.rotation = bedBaseRotation;
            var walking = startPosition + walkDirection * state.WalkDistance;
            var lyingRotation = roomEnding?.Portal != null
                ? roomEnding.WallLyingRotation
                : bedBaseRotation * Quaternion.LookRotation(Vector3.up,
                UsesBedEndingSequence ? Vector3.back : Vector3.forward);
            if (state.Phase == BeachEndingPhase.Walking)
            {
                if (roomEnding == null) viewCamera.transform.SetPositionAndRotation(walking, standingRotation);
            }
            else if (state.Phase == BeachEndingPhase.Lying)
            {
                float progress = LieDownProgress;
                if (roomEnding?.Portal != null)
                    roomEnding.ApplyWallLieDown(state.LieStartPosition, state.LieStartRotation, bed.TransformPoint(pillowLocal), progress);
                else
                {
                    float t = Mathf.SmoothStep(0, 1, progress);
                    viewCamera.transform.SetPositionAndRotation(Vector3.Lerp(roomEnding != null ? state.LieStartPosition : walking, bed.TransformPoint(pillowLocal), t),
                        Quaternion.Slerp(roomEnding != null ? state.LieStartRotation : standingRotation, lyingRotation, t));
                }
            }
            else if (UsesBedEndingSequence)
            {
                viewCamera.transform.SetPositionAndRotation(bed.TransformPoint(pillowLocal),
                    lyingRotation);
                ApplyOceanFloat(restoredOceanSample);
            }
            else
            {
                float phase = state.DriftElapsed * Mathf.PI * 2 / Safe(profile.FloatPeriod, 8, .1f);
                float amplitude = Safe(profile.FloatAmplitude, .08f, 0);
                float tilt = Safe(profile.FloatTiltDegrees, 1.5f, 0);
                float drift = Safe(profile.DriftSpeed, .04f, 0);
                bed.position += Vector3.up * (Mathf.Sin(phase) * amplitude)
                    + Vector3.forward * (state.DriftElapsed * drift);
                bed.rotation = bedBaseRotation * Quaternion.Euler(Mathf.Sin(phase * .7f) * tilt, 0, Mathf.Sin(phase) * tilt * .7f);
                roomEnding?.KeepBedInsideRoom();
                viewCamera.transform.SetPositionAndRotation(bed.TransformPoint(pillowLocal),
                    bed.rotation * Quaternion.LookRotation(Vector3.up, Vector3.forward));
            }
            ApplyWaterParameters();
            ApplyEndingFade();
            if (skyMaterial != null)
            {
                skyMaterial.SetFloat("_DreamTime", worldTime);
                skyMaterial.SetFloat("_CloudSpeed", Safe(profile.CloudSpeed, .006f, 0));
            }
            RenderRoomCubemap();
        }

        private void ApplyOceanFloat(int? restoredSample)
        {
            OceanFloatPose = Vector3.zero;
            if (!profile.EnableOceanFloat) return;
            var ocean = audioService?.GetSource(MiniGameAudioChannel.Ocean);
            if (ocean?.clip == null) return;
            if (oceanFloatClip != ocean.clip)
            {
                oceanFloatClip = ocean.clip;
                oceanFloatTrack = OceanFloatTrack.Load(ocean.clip);
            }
            if (oceanFloatTrack == null) return;
            oceanPoseSample = restoredSample ?? ocean.timeSamples;
            float elapsed = state.Phase == BeachEndingPhase.FadingOut ? SkyHoldSeconds + state.PhaseElapsed : state.PhaseElapsed;
            float gain = Mathf.SmoothStep(0, 1, Mathf.Clamp01(elapsed / 1.1f));
            var swell = oceanFloatTrack.Evaluate(oceanPoseSample);
            OceanFloatPose = new Vector3(swell.x * Mathf.Min(.2f, Safe(profile.OceanFloatHeight, .1f, 0)),
                swell.y * Mathf.Min(5, Safe(profile.OceanFloatPitch, 2.6f, 0)),
                swell.z * Mathf.Min(3, Safe(profile.OceanFloatRoll, 1.2f, 0))) * gain;
            viewCamera.transform.position += Vector3.up * OceanFloatPose.x;
            viewCamera.transform.rotation *= Quaternion.Euler(OceanFloatPose.y, 0, OceanFloatPose.z);
        }

        private void ApplyEndingFade()
        {
            if (endingFade == null) return;
            float opacity = UsesBedEndingSequence && state.Phase == BeachEndingPhase.FadingOut
                ? Mathf.Clamp01(state.PhaseElapsed / EndingFadeSeconds) : 0;
            endingFade.color = new Color(0, 0, 0, opacity);
            endingFade.gameObject.SetActive(opacity > 0);
            if (opacity < 1) opaqueFrame = -1;
            else if (opaqueFrame < 0) opaqueFrame = Time.frameCount;
        }

        private void RenderRoomCubemap()
        {
            if (roomEnding?.Cubemap == null) return;
            // Water uses the shared ending clock. The capture eye stays fixed when the player moves.
            if (roomEnding.Cubemap.HasPanoramaWater)
                roomEnding.Cubemap.Render(worldTime, BeachEndingPhase.Walking, 0, 0);
            else if (!roomEnding.Cubemap.IsReady)
                roomEnding.Cubemap.Render(0, BeachEndingPhase.Walking, 0, 0, true);
            roomEnding.UpdateRoomSurface();
            // The wall image stays painted in place. The eye physically crosses it into the exterior.
            roomEnding.RoomSurface?.SetProjectionOrigin(roomEnding.PanoramaOrigin);
            roomEnding.UpdatePortalView(worldTime, state.Phase == BeachEndingPhase.Lying
                ? (float?)LieDownProgress : null);
        }

        private void CreateWaterMaterials()
        {
            var renderers = new List<Renderer>(GetComponentsInChildren<Renderer>(true));
            if (sea != null && !renderers.Contains(sea)) renderers.Add(sea);
            foreach (var renderer in renderers)
            {
                var originals = renderer.sharedMaterials;
                Material[] instances = null;
                for (int i = 0; i < originals.Length; i++)
                {
                    var original = originals[i];
                    if (original == null || original.shader == null || (renderer != sea && original.shader.name != "MiniGame/Dream Sand")) continue;
                    if (instances == null) instances = (Material[])originals.Clone();
                    instances[i] = new Material(original) { name = original.name + " (Dream runtime)", hideFlags = HideFlags.DontSave };
                }
                if (instances == null) continue;
                renderer.sharedMaterials = instances;
                waterSurfaces.Add(new SurfaceMaterials { Renderer = renderer, Originals = originals, Instances = instances });
            }
        }

        private void ApplyWaterParameters()
        {
            foreach (var surface in waterSurfaces)
                for (int i = 0; i < surface.Instances.Length; i++)
                {
                    var material = surface.Instances[i];
                    if (material == null || material == surface.Originals[i]) continue;
                    material.SetFloat("_UseDreamTime", 1);
                    material.SetFloat("_DreamTime", worldTime);
                    material.SetFloat("_WaveHeight", Mathf.Clamp(Safe(profile.SeaWaveHeight, .24f, 0), 0, .6f));
                    material.SetFloat("_WavePeriod", Mathf.Clamp(Safe(profile.SeaWavePeriod, 6, 2), 2, 15));
                    material.SetFloat("_WaveLength", Mathf.Clamp(Safe(profile.SeaWaveLength, 7, 3), 3, 20));
                    material.SetFloat("_ShoreWash", Mathf.Clamp(Safe(profile.ShoreWashDistance, 1.8f, 0), 0, 4));
                    material.SetFloat("_FoamAmount", Mathf.Clamp(Safe(profile.SeaFoamAmount, .85f, 0), 0, 1.5f));
                }
        }

        private void RestoreWaterMaterials()
        {
            foreach (var surface in waterSurfaces)
            {
                if (surface.Renderer != null)
                {
                    var current = surface.Renderer.sharedMaterials;
                    for (int i = 0; i < current.Length && i < surface.Instances.Length; i++)
                        if (current[i] == surface.Instances[i]) current[i] = surface.Originals[i];
                    surface.Renderer.sharedMaterials = current;
                }
                for (int i = 0; i < surface.Instances.Length; i++)
                    if (surface.Instances[i] != null && surface.Instances[i] != surface.Originals[i]) Destroy(surface.Instances[i]);
            }
            waterSurfaces.Clear();
        }

        public BeachEndingCheckpoint CaptureCheckpoint()
        {
            var saved = new BeachEndingCheckpoint
            {
                Phase = state.Phase, WorldTime = worldTime, WalkDistance = state.WalkDistance, PhaseElapsed = state.PhaseElapsed,
                SupervisorDialogueIndex = state.SupervisorDialogueIndex, SupervisorDialogueComplete = state.SupervisorDialogueComplete,
                DriftElapsed = state.DriftElapsed, SubtitleElapsed = state.SubtitleElapsed,
                SubtitleIndex = state.SubtitleIndex, SubtitlesComplete = state.SubtitlesComplete,
                LieStartPosition = state.LieStartPosition, LieStartRotation = state.LieStartRotation,
                HasMusicPosition = audioService?.GetSource(MiniGameAudioChannel.EndingMusic)?.clip != null,
                MusicSeconds = audioService?.GetSource(MiniGameAudioChannel.EndingMusic)?.time ?? 0,
                HasOceanPosition = audioService?.GetSource(MiniGameAudioChannel.Ocean)?.clip != null,
                OceanSample = UsesBedEndingSequence && profile.EnableOceanFloat && oceanPoseSample >= 0
                    && (state.Phase == BeachEndingPhase.SkyHold || state.Phase == BeachEndingPhase.FadingOut)
                    ? oceanPoseSample : audioService?.GetSource(MiniGameAudioChannel.Ocean)?.timeSamples ?? 0
            };
            roomEnding?.CapturePose(saved);
            return saved;
        }

        public void RestoreCheckpoint(BeachEndingCheckpoint value)
        {
            if (!initialized || value == null) return;
            state = new BeachEndingCheckpoint
            {
                Phase = Enum.IsDefined(typeof(BeachEndingPhase), value.Phase) ? value.Phase : BeachEndingPhase.Walking,
                SupervisorDialogueIndex = Mathf.Clamp(value.SupervisorDialogueIndex, 0, DreamSupervisorDialogue.Lines.Length - 1),
                SupervisorDialogueComplete = value.SupervisorDialogueComplete,
                WalkDistance = Mathf.Clamp(Safe(value.WalkDistance, 0, 0), 0, MaximumWalkDistance),
                PhaseElapsed = Safe(value.PhaseElapsed, 0, 0), DriftElapsed = Safe(value.DriftElapsed, 0, 0),
                SubtitleElapsed = Finite(value.SubtitleElapsed) ? value.SubtitleElapsed : 0,
                SubtitleIndex = Mathf.Max(0, value.SubtitleIndex), SubtitlesComplete = value.SubtitlesComplete,
                LieStartPosition = value.LieStartPosition, LieStartRotation = value.LieStartRotation
            };
            opaqueFrame = -1;
            supervisorDialogue.Hide();
            // Older formal saves could contain the preview's drifting phase. Resume at the sky hold.
            if (UsesBedEndingSequence && state.Phase == BeachEndingPhase.Drifting)
            { state.Phase = BeachEndingPhase.SkyHold; state.PhaseElapsed = 0; }
            if (UsesBedEndingSequence && state.Phase != BeachEndingPhase.Walking)
            {
                float duration = state.Phase == BeachEndingPhase.Lying ? LieDownDuration
                    : state.Phase == BeachEndingPhase.SkyHold ? SkyHoldSeconds : EndingFadeSeconds;
                state.PhaseElapsed = Mathf.Min(state.PhaseElapsed, duration);
            }
            if (UsesBedEndingSequence && state.Phase == BeachEndingPhase.SkyHold && !state.SupervisorDialogueComplete)
                state.PhaseElapsed = Mathf.Min(state.PhaseElapsed, SupervisorInterruptSeconds);
            if (roomEnding != null)
            {
                bed.SetPositionAndRotation(bedBasePosition, bedBaseRotation);
                roomEnding.RestorePose(value);
                if (!value.HasRoomPose || !Finite(state.LieStartPosition.x) || !Finite(state.LieStartPosition.y)
                    || !Finite(state.LieStartPosition.z) || !ValidRotation(state.LieStartRotation))
                {
                    state.LieStartPosition = viewCamera.transform.position;
                    state.LieStartRotation = viewCamera.transform.rotation;
                }
                else state.LieStartRotation = Quaternion.Normalize(state.LieStartRotation);
            }
            worldTime = Safe(value.WorldTime, state.DriftElapsed + state.PhaseElapsed, 0);
            var music = audioService.GetSource(MiniGameAudioChannel.EndingMusic);
            if (value.HasMusicPosition && music != null && music.clip != null)
                music.time = Mathf.Clamp(Safe(value.MusicSeconds, 0, 0), 0, Mathf.Max(0, music.clip.length - .01f));
            var ocean = audioService.GetSource(MiniGameAudioChannel.Ocean);
            if (value.HasOceanPosition && ocean?.clip != null)
                ocean.timeSamples = Mathf.Clamp(value.OceanSample, 0, Mathf.Max(0, ocean.clip.samples - 1));
            oceanPoseSample = -1;
            voiceIndex = -1;
            audioService.Stop(MiniGameAudioChannel.Voice);
            if (state.Phase == BeachEndingPhase.Drifting) AdvanceSubtitles(0);
            // Streaming seeks reach the audio thread asynchronously; restore the saved visual pose immediately.
            ApplyPose(value.HasOceanPosition ? (int?)Mathf.Max(0, value.OceanSample) : null);
            RenderRoomCubemap();
            if (SupervisorDialogueActive) BeginSupervisorDialogue();
            ApplyPause(true);
            RefreshInterface();
            PublishCheckpoint();
        }

        private void ApplyPause(bool force = false)
        {
            bool paused = Paused;
            if (force || paused != audioPaused)
            {
                foreach (var channel in new[] { MiniGameAudioChannel.EndingMusic, MiniGameAudioChannel.Ocean, MiniGameAudioChannel.Voice, MiniGameAudioChannel.ComputerNoise })
                    if (paused || SupervisorDialogueActive && (channel == MiniGameAudioChannel.EndingMusic || channel == MiniGameAudioChannel.Ocean))
                        audioService.Pause(channel); else audioService.Resume(channel);
                supervisorDialogue?.SetPaused(paused);
                audioPaused = paused;
            }
            pauseMenu?.SetVisible(paused && !MainMenuController.BlocksGameplayInput);
            RefreshCredits();
            if (hint != null) hint.gameObject.SetActive(!paused && !thanksActive);
            if (caption != null) caption.gameObject.SetActive(!paused && !thanksActive);
            if (paused || thanksActive)
            {
                interactionCursor?.SetVisible(false);
                roomEnding?.SetBedFocus(false);
            }
            Cursor.lockState = paused || thanksActive ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = paused || thanksActive;
        }

        private void PublishCheckpoint()
        {
            var host = MiniGameSessionHost.Current;
            if (host != null && !host.IsTransitioning && host.Runtime != null
                && host.Runtime.GetView().Game.Ending == MiniGame.Core.EndingType.DreamSea)
                host.EndingCheckpoint = CaptureCheckpoint();
        }

        private void RefreshInterface()
        {
            RefreshCredits();
            if (hint != null) hint.text = string.Empty;
            bool focused = BedInteractionFocused && !Paused && !thanksActive;
            interactionCursor?.SetKind(InteractionCursorKind.Sleep);
            interactionCursor?.SetVisible(focused);
            roomEnding?.SetBedFocus(focused);
            if (caption == null) return;
            var lines = profile.EndingLines;
            caption.text = state.Phase == BeachEndingPhase.Drifting && !state.SubtitlesComplete
                && state.SubtitleElapsed >= 0 && lines != null && state.SubtitleIndex < lines.Length
                ? lines[state.SubtitleIndex]?.Text ?? string.Empty : string.Empty;
        }

        private void RefreshCredits()
        {
            if (credits == null) return;
            float elapsed = state.PhaseElapsed - SupervisorInterruptSeconds - CreditsStartSeconds;
            bool visible = UsesBedEndingSequence && state.Phase == BeachEndingPhase.SkyHold
                && state.SupervisorDialogueComplete && elapsed >= 0 && elapsed < CreditsSeconds
                && !Paused && !thanksActive;
            credits.Refresh(Mathf.Clamp01(elapsed / CreditsSeconds), visible);
        }

        private void BuildInterface()
        {
            font = GameUiTypography.CreateFont();
            var go = new GameObject("海滩字幕与暂停", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            go.transform.SetParent(transform, false);
            canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 950;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = .5f;
            hint = Label(go.transform, "提示", new Vector2(.5f, .12f), new Vector2(700, 54), 24);
            interactionCursor = new InteractionCursorView((RectTransform)go.transform, TerminalView.Ink);
            caption = Label(go.transform, "字幕", new Vector2(.5f, .12f), new Vector2(1600, 90), 28);
            supervisorDialogue = gameObject.AddComponent<DreamSupervisorDialogue>();
            supervisorDialogue.Initialize(go.transform, audioService);
            credits = gameObject.AddComponent<DreamEndingCredits>();
            credits.Initialize(go.transform);
            var curtain = new GameObject("Ending fade", typeof(RectTransform), typeof(Image));
            curtain.transform.SetParent(go.transform, false);
            var curtainRect = (RectTransform)curtain.transform;
            curtainRect.anchorMin = Vector2.zero; curtainRect.anchorMax = Vector2.one;
            curtainRect.offsetMin = curtainRect.offsetMax = Vector2.zero;
            endingFade = curtain.GetComponent<Image>();
            endingFade.color = Color.clear;
            endingFade.raycastTarget = false;
            curtain.SetActive(false);
            string[] names = { "继续", "保存进度", "读取存档", "重新开始", "回到主菜单", "退出游戏" };
            Action[] actions = { () => SetPaused(false), SaveFromPause, LoadFromPause, Restart,
                () => MainMenuController.Current?.ReturnToMenu(), Quit };
            pauseMenu = PauseMenuView.Create(go.transform, font, "EndingPauseModal", names,
                new[] { "ESC", "", "", "", "", "" }, actions);
            pauseMenu.SetContext("海上之梦");
            if (!previewMode)
                pauseMenu.ConfigureGmEndings(ending =>
                {
                    var session = MiniGameSessionHost.Current;
                    if (session == null) { pauseMenu.SetStatus("当前没有检查流程。", true); return; }
                    if (!session.TryEnterGmEnding(ending, out string error)) pauseMenu.SetStatus(error, true);
                });
            pauseMenu.SetHint("海浪会在继续后流动。");
            pauseMenu.Buttons[1].interactable = !previewMode && MiniGameSessionHost.Current != null;
            pauseMenu.Buttons[2].interactable = !previewMode && MiniGameSessionHost.Current != null;
            pauseMenu.Buttons[4].interactable = !previewMode && MainMenuController.Current != null;
            if (previewMode)
            {
                pauseMenu.SetContext("海上之梦 · 预览");
                pauseMenu.SetHint("预览不保存进度；停止 Unity 播放可返回编辑。");
                pauseMenu.Buttons[3].interactable = false;
            }
            if (FindObjectOfType<EventSystem>() == null)
            {
                var events = new GameObject("Ending EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
                events.transform.SetParent(transform, false);
            }
            pauseMenu.SetVisible(false);
        }

        private void SaveFromPause()
        {
            if (previewMode) return;
            var session = MiniGameSessionHost.Current;
            if (session == null) return;
            session.Save();
            pauseMenu.SetStatus(session.LastMessage);
        }

        private void LoadFromPause()
        {
            if (previewMode) return;
            var session = MiniGameSessionHost.Current;
            if (session == null) return;
            session.Load();
            if (pauseMenu != null) pauseMenu.SetStatus(session.LastMessage);
        }

        private Text Label(Transform parent, string name, Vector2 anchor, Vector2 size, int fontSize)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text), typeof(Shadow));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = anchor; rect.sizeDelta = size;
            var text = go.GetComponent<Text>();
            text.font = font; text.fontSize = fontSize; text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white; text.raycastTarget = false;
            var shadow = go.GetComponent<Shadow>();
            shadow.effectColor = new Color(0, .05f, .08f, .85f); shadow.effectDistance = new Vector2(1, -2);
            return text;
        }

        private void Restart()
        {
            SetPaused(false);
            if (MiniGameSessionHost.Current != null) MiniGameSessionHost.Current.RestartToCombined();
            else SceneManager.LoadScene("MainRoomCombined");
        }

        private static void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
        private static bool ValidRotation(Quaternion value) => Finite(value.x) && Finite(value.y) && Finite(value.z)
            && Finite(value.w) && value.x * value.x + value.y * value.y + value.z * value.z + value.w * value.w > .0001f;
        private static float Safe(float value, float fallback, float minimum) => Finite(value) ? Mathf.Max(minimum, value) : fallback;

        private void OnDestroy()
        {
            interactionCursor?.Dispose();
            roomEnding?.SetBedFocus(false);
            if (RenderSettings.skybox == skyMaterial) RenderSettings.skybox = originalSky;
            if (skyMaterial != null) Destroy(skyMaterial);
            RestoreWaterMaterials();
            if (font != null) Destroy(font);
        }
    }
}
