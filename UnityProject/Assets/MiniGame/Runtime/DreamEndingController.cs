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
        private Material skyMaterial, originalSky;
        private sealed class SurfaceMaterials
        {
            public Renderer Renderer;
            public Material[] Originals, Instances;
        }
        private readonly List<SurfaceMaterials> waterSurfaces = new List<SurfaceMaterials>();
        private MiniGameAudioService audioService;
        private Canvas canvas;
        private Text hint, caption;
        private GameObject pauseMenu;
        private Font font;
        private int voiceIndex = -1;
        public BeachEndingPhase Phase => state.Phase;
        public Camera ViewCamera => viewCamera;
        public DreamSequenceProfile Profile => profile;
        public Transform Bed => bed;
        public MiniGameAudioService Audio => audioService;
        public bool UseExternalClock { get; set; }
        public bool Paused => MiniGameSessionHost.Current?.Runtime?.GetView().Paused ?? localPaused;
        public string CurrentSubtitle => caption != null ? caption.text : string.Empty;
        public bool CanLieDown => initialized && !Paused && state.Phase == BeachEndingPhase.Walking
            && pathLength - state.WalkDistance <= Safe(profile.BedInteractionDistance, 1.5f, .1f);
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
            startPosition = approach.position;
            standingRotation = approach.rotation;
            walkDirection = Vector3.ProjectOnPlane(approach.forward, Vector3.up).normalized;
            if (walkDirection.sqrMagnitude < .5f) walkDirection = Vector3.forward;
            bedBasePosition = bed.position;
            bedBaseRotation = bed.rotation;
            pillowLocal = bed.InverseTransformPoint(pillow.position);
            pathLength = Mathf.Max(0, Vector3.Dot(bed.position - startPosition, walkDirection));
            originalSky = RenderSettings.skybox;
            if (originalSky != null)
            {
                skyMaterial = new Material(originalSky);
                RenderSettings.skybox = skyMaterial;
            }
            CreateWaterMaterials();
            audioService = gameObject.AddComponent<MiniGameAudioService>();
            audioService.Configure(profile.Audio);
            audioService.SetLoop(MiniGameAudioChannel.EndingMusic, MiniGameSound.EndingMusic, 2);
            audioService.SetLoop(MiniGameAudioChannel.Ocean, MiniGameSound.OceanWaves, 2);
            BuildInterface();
            RestoreCheckpoint(host?.EndingCheckpoint ?? new BeachEndingCheckpoint());
        }

        private void Update()
        {
            if (!initialized || UseExternalClock) return;
            if (Input.GetKeyDown(KeyCode.Escape)) SetPaused(!Paused);
            AdvanceForTests(Input.GetKey(KeyCode.W) ? 1 : 0, Time.unscaledDeltaTime);
            if (Input.GetKeyDown(KeyCode.E)) TryLieDown();
        }

        public void SetPaused(bool value)
        {
            localPaused = value;
            MiniGameSessionHost.Current?.Runtime?.SetPaused(value);
            ApplyPause();
        }

        /// <summary>Forward is clamped to [0,1]; reverse and lateral input have no route.</summary>
        public void AdvanceForTests(float forward, float dt)
        {
            if (!initialized) return;
            ApplyPause();
            if (Paused) { ApplyWaterParameters(); return; }
            dt = Finite(dt) ? Mathf.Max(0, dt) : 0;
            worldTime += dt;
            if (state.Phase == BeachEndingPhase.Walking)
            {
                float input = Finite(forward) ? Mathf.Clamp01(forward) : 0;
                state.WalkDistance = Mathf.Min(MaximumWalkDistance,
                    state.WalkDistance + input * Safe(profile.WalkSpeed, 1.2f, .01f) * dt);
            }
            else if (state.Phase == BeachEndingPhase.Lying)
            {
                state.PhaseElapsed += dt;
                float duration = Safe(profile.LieDownSeconds, 1.5f, .01f);
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
            RefreshInterface();
            PublishCheckpoint();
        }

        public bool TryLieDown()
        {
            if (!CanLieDown) return false;
            state.Phase = BeachEndingPhase.Lying;
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

        private void ApplyPose()
        {
            bed.position = bedBasePosition;
            bed.rotation = bedBaseRotation;
            var walking = startPosition + walkDirection * state.WalkDistance;
            if (state.Phase == BeachEndingPhase.Walking)
            {
                viewCamera.transform.SetPositionAndRotation(walking, standingRotation);
            }
            else if (state.Phase == BeachEndingPhase.Lying)
            {
                float t = Mathf.SmoothStep(0, 1, Mathf.Clamp01(state.PhaseElapsed / Safe(profile.LieDownSeconds, 1.5f, .01f)));
                viewCamera.transform.SetPositionAndRotation(Vector3.Lerp(walking, bed.TransformPoint(pillowLocal), t),
                    Quaternion.Slerp(standingRotation, bedBaseRotation * Quaternion.LookRotation(Vector3.up, Vector3.forward), t));
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
                viewCamera.transform.SetPositionAndRotation(bed.TransformPoint(pillowLocal),
                    bed.rotation * Quaternion.LookRotation(Vector3.up, Vector3.forward));
            }
            ApplyWaterParameters();
            if (skyMaterial != null)
            {
                skyMaterial.SetFloat("_DreamTime", worldTime);
                skyMaterial.SetFloat("_CloudSpeed", Safe(profile.CloudSpeed, .006f, 0));
            }
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

        public BeachEndingCheckpoint CaptureCheckpoint() => new BeachEndingCheckpoint
        {
            Phase = state.Phase, WorldTime = worldTime, WalkDistance = state.WalkDistance, PhaseElapsed = state.PhaseElapsed,
            DriftElapsed = state.DriftElapsed, SubtitleElapsed = state.SubtitleElapsed,
            SubtitleIndex = state.SubtitleIndex, SubtitlesComplete = state.SubtitlesComplete
        };

        public void RestoreCheckpoint(BeachEndingCheckpoint value)
        {
            if (!initialized || value == null) return;
            state = new BeachEndingCheckpoint
            {
                Phase = Enum.IsDefined(typeof(BeachEndingPhase), value.Phase) ? value.Phase : BeachEndingPhase.Walking,
                WalkDistance = Mathf.Clamp(Safe(value.WalkDistance, 0, 0), 0, MaximumWalkDistance),
                PhaseElapsed = Safe(value.PhaseElapsed, 0, 0), DriftElapsed = Safe(value.DriftElapsed, 0, 0),
                SubtitleElapsed = Finite(value.SubtitleElapsed) ? value.SubtitleElapsed : 0,
                SubtitleIndex = Mathf.Max(0, value.SubtitleIndex), SubtitlesComplete = value.SubtitlesComplete
            };
            worldTime = Safe(value.WorldTime, state.DriftElapsed + state.PhaseElapsed, 0);
            voiceIndex = -1;
            audioService.Stop(MiniGameAudioChannel.Voice);
            if (state.Phase == BeachEndingPhase.Drifting) AdvanceSubtitles(0);
            ApplyPose();
            ApplyPause(true);
            RefreshInterface();
            PublishCheckpoint();
        }

        private void ApplyPause(bool force = false)
        {
            bool paused = Paused;
            if (force || paused != audioPaused)
            {
                foreach (var channel in new[] { MiniGameAudioChannel.EndingMusic, MiniGameAudioChannel.Ocean, MiniGameAudioChannel.Voice })
                    if (paused) audioService.Pause(channel); else audioService.Resume(channel);
                audioPaused = paused;
            }
            if (pauseMenu != null) pauseMenu.SetActive(paused);
            Cursor.lockState = paused ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = paused;
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
            if (hint != null) hint.text = state.Phase != BeachEndingPhase.Walking ? string.Empty
                : CanLieDown ? "E  躺下" : "W  走向床";
            if (caption == null) return;
            var lines = profile.EndingLines;
            caption.text = state.Phase == BeachEndingPhase.Drifting && !state.SubtitlesComplete
                && state.SubtitleElapsed >= 0 && lines != null && state.SubtitleIndex < lines.Length
                ? lines[state.SubtitleIndex]?.Text ?? string.Empty : string.Empty;
        }

        private void BuildInterface()
        {
            font = Font.CreateDynamicFontFromOSFont(new[] { "Microsoft YaHei", "SimSun", "Noto Sans CJK SC", "Arial" }, 26);
            var go = new GameObject("海滩字幕与暂停", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            go.transform.SetParent(transform, false);
            canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = .5f;
            hint = Label(go.transform, "提示", new Vector2(.5f, .12f), new Vector2(700, 54), 24);
            caption = Label(go.transform, "字幕", new Vector2(.5f, .12f), new Vector2(1600, 90), 28);
            pauseMenu = new GameObject("暂停", typeof(RectTransform), typeof(Image));
            pauseMenu.transform.SetParent(go.transform, false);
            var rect = (RectTransform)pauseMenu.transform;
            rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = rect.offsetMax = Vector2.zero;
            pauseMenu.GetComponent<Image>().color = new Color(.02f, .07f, .09f, .82f);
            Label(pauseMenu.transform, "标题", new Vector2(.5f, .77f), new Vector2(700, 70), 32).text = "暂停";
            string[] names = { "继续", "保存", "读取", "重新开始", "退出" };
            Action[] actions = { () => SetPaused(false), () => MiniGameSessionHost.Current?.Save(),
                () => MiniGameSessionHost.Current?.Load(), Restart, Quit };
            for (int i = 0; i < names.Length; i++)
            {
                var buttonGo = new GameObject(names[i], typeof(RectTransform), typeof(Image), typeof(Button));
                buttonGo.transform.SetParent(pauseMenu.transform, false);
                var buttonRect = (RectTransform)buttonGo.transform;
                buttonRect.anchorMin = buttonRect.anchorMax = new Vector2(.5f, .65f - i * .09f);
                buttonRect.sizeDelta = new Vector2(320, 64);
                buttonGo.GetComponent<Image>().color = new Color(.14f, .25f, .28f, 1);
                var button = buttonGo.GetComponent<Button>();
                var action = actions[i]; button.onClick.AddListener(() => action());
                button.interactable = (i != 1 && i != 2) || MiniGameSessionHost.Current != null;
                Label(buttonGo.transform, "文字", Vector2.one * .5f, new Vector2(320, 64), 24).text = names[i];
            }
            if (FindObjectOfType<EventSystem>() == null)
            {
                var events = new GameObject("Ending EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
                events.transform.SetParent(transform, false);
            }
            pauseMenu.SetActive(false);
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
        private static float Safe(float value, float fallback, float minimum) => Finite(value) ? Mathf.Max(minimum, value) : fallback;

        private void OnDestroy()
        {
            if (RenderSettings.skybox == skyMaterial) RenderSettings.skybox = originalSky;
            if (skyMaterial != null) Destroy(skyMaterial);
            RestoreWaterMaterials();
            if (font != null) Destroy(font);
        }
    }
}
