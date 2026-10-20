using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace MiniGame.UnityHost
{
    /// <summary>Presentation-only cameras. No patient decisions or session state are stored here.</summary>
    public sealed class PatientSurveillanceFeed : MonoBehaviour
    {
        public const int StageLayer = 31;
        private readonly Dictionary<string, PatientMonitorRoom> rooms = new Dictionary<string, PatientMonitorRoom>();
        private PatientMonitorProfile profile;
        private RawImage image;
        private GameObject stage;
        private Camera stageCamera;
        private SpriteRenderer actor;
        private Material actorMaterial, signalMaterial;
        private RenderTexture source, output;
        private PatientMonitorRoom activeRoom;
        private PatientMonitorProfile.Room settings;
        private string patientId;
        private Sprite sprite;
        private float clock, sinceFrame;
        private bool paused, failed, needsFrame;
        private int revision;
        public Sprite CurrentSprite => sprite;
        public string PatientId => patientId;
        public RenderTexture Output => output;
        public Camera ViewCamera => stageCamera;
        public PatientMonitorRoom ActiveRoom => activeRoom;
        public int RenderedFrames { get; private set; }
        public float PresentationTime => clock;
        public bool UseExternalClock { get; set; }
        public bool IsCloseUp { get; private set; }

        public void SetCloseUp(bool value)
        {
            if (IsCloseUp == value) return;
            IsCloseUp = value;
            if (!isActiveAndEnabled || !Ready()) return;
            SelectRoom();
            RenderFrame();
        }

        public void Initialize(RawImage target, PatientMonitorProfile configuration)
        {
            image = target;
            profile = configuration;
            image.raycastTarget = false;
            image.enabled = false;
        }

        public void Show(string id, Sprite portrait, bool frozen)
        {
            bool changed = patientId != id || sprite != portrait;
            if (patientId != id) IsCloseUp = false;
            patientId = id;
            sprite = portrait;
            paused = frozen;
            if (!isActiveAndEnabled) return;
            if (!Ready()) { HideFeed(); return; }
            SelectRoom();
            if (changed || needsFrame) RenderFrame();
        }

        private bool Ready()
        {
            if (image == null || profile == null || !profile.Enabled || sprite == null || failed) return false;
            settings = profile.Find(patientId);
            if (settings == null) return false;
            if (stage == null || revision != profile.Revision)
            {
                ReleaseStage();
                Build();
            }
            if (stage != null && !stage.activeSelf) stage.SetActive(true);
            return stage != null && !failed;
        }

        private void Build()
        {
            var roomShader = Resources.Load<Shader>("PatientMonitorRoom");
            var actorShader = Resources.Load<Shader>("PatientMonitorActor");
            var signalShader = Resources.Load<Shader>("PatientMonitorSignal");
            if (roomShader == null || actorShader == null || signalShader == null
                || !roomShader.isSupported || !actorShader.isSupported || !signalShader.isSupported)
            {
                failed = true;
                Debug.LogError("人物监控室 Shader 不可用，回退到原立绘。", this);
                return;
            }
            stage = new GameObject("Patient surveillance stages") { hideFlags = HideFlags.DontSave };
            stage.transform.position = new Vector3(6000, -6000, 0);
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(stage, gameObject.scene);
            int index = 0;
            foreach (var entry in profile.Rooms)
            {
                if (entry == null || string.IsNullOrEmpty(entry.PatientId) || rooms.ContainsKey(entry.PatientId)) continue;
                var room = PatientMonitorRoom.Create(stage.transform, StageLayer, entry.Seed,
                    entry.WallTint, entry.LightTint, Mathf.Clamp01(entry.Grime));
                room.name = entry.Label + " / " + entry.PatientId;
                room.transform.localPosition = Vector3.right * (index++ * 14);
                room.SetOccupied(true);
                room.gameObject.SetActive(false);
                rooms.Add(entry.PatientId, room);
            }
            int width = Mathf.Clamp(profile.Width, 346, 1600), height = Mathf.Clamp(profile.Height, 140, 800);
            source = Texture("Patient surveillance scene", width, height, 24);
            output = Texture("Patient surveillance output", width, height, 0);
            var cameraObject = new GameObject("Patient CCTV camera", typeof(Camera));
            cameraObject.transform.SetParent(stage.transform, false);
            stageCamera = cameraObject.GetComponent<Camera>();
            stageCamera.enabled = false;
            stageCamera.useOcclusionCulling = false;
            #if UNITY_EDITOR
            stageCamera.scene = gameObject.scene;
            #endif
            stageCamera.targetTexture = source;
            stageCamera.clearFlags = CameraClearFlags.SolidColor;
            stageCamera.backgroundColor = new Color(.09f, .12f, .11f);
            stageCamera.cullingMask = 1 << StageLayer;
            stageCamera.nearClipPlane = .04f;
            stageCamera.farClipPlane = 18;
            stageCamera.allowHDR = false;
            stageCamera.allowMSAA = false;
            actorMaterial = new Material(actorShader) { hideFlags = HideFlags.DontSave };
            signalMaterial = new Material(signalShader) { hideFlags = HideFlags.DontSave };
            var actorObject = new GameObject("Seated patient cutout", typeof(SpriteRenderer));
            actorObject.transform.SetParent(stage.transform, false);
            actorObject.layer = StageLayer;
            actor = actorObject.GetComponent<SpriteRenderer>();
            actor.sharedMaterial = actorMaterial;
            actor.shadowCastingMode = ShadowCastingMode.Off;
            actor.receiveShadows = false;
            image.texture = output;
            revision = profile.Revision;
            needsFrame = true;
        }

        private static RenderTexture Texture(string label, int width, int height, int depth)
        {
            var value = new RenderTexture(width, height, depth, RenderTextureFormat.ARGB32)
                { name = label, filterMode = FilterMode.Bilinear, hideFlags = HideFlags.DontSave };
            value.Create();
            return value;
        }

        private void SelectRoom()
        {
            if (!rooms.TryGetValue(patientId, out var next)) { image.enabled = false; return; }
            if (activeRoom != next)
            {
                if (activeRoom != null) activeRoom.gameObject.SetActive(false);
                activeRoom = next;
                activeRoom.gameObject.SetActive(true);
            }
            image.enabled = true;
            actor.sprite = sprite;
        }

        private void LateUpdate()
        {
            if (!UseExternalClock) Advance(Time.unscaledDeltaTime);
        }

        public void Advance(float deltaSeconds)
        {
            if (!isActiveAndEnabled || !Ready()) { HideFeed(); return; }
            SelectRoom();
            if (needsFrame) { RenderFrame(); return; }
            if (paused) return;
            if (float.IsNaN(deltaSeconds) || float.IsInfinity(deltaSeconds)) return;
            float delta = Mathf.Clamp(deltaSeconds, 0, 1);
            clock += delta;
            sinceFrame += delta;
            if (sinceFrame >= 1f / Mathf.Clamp(profile.FramesPerSecond, 1, 30)) RenderFrame();
        }

        private void RenderFrame()
        {
            if (activeRoom == null || sprite == null) return;
            activeRoom.Tick(clock);
            var root = activeRoom.transform;
            stageCamera.transform.position = root.TransformPoint(settings.CameraPosition);
            var forward = root.TransformPoint(settings.CameraTarget) - stageCamera.transform.position;
            if (forward.sqrMagnitude < .001f) forward = Vector3.forward;
            stageCamera.transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
            stageCamera.fieldOfView = Mathf.Clamp(settings.FieldOfView, 15, 75);
            if (IsCloseUp)
            {
                var face = activeRoom.ActorAnchor.position + settings.ActorOffset + Vector3.up * (settings.ActorHeight * .8f);
                stageCamera.transform.position = face + new Vector3(-.06f, .04f, -1.45f);
                stageCamera.transform.rotation = Quaternion.LookRotation(face - stageCamera.transform.position, Vector3.up);
                stageCamera.fieldOfView = 29;
            }
            var display = image.rectTransform.rect;
            stageCamera.aspect = display.width > 0 && display.height > 0 ? display.width / display.height : (float)source.width / source.height;
            actor.transform.position = activeRoom.ActorAnchor.position + settings.ActorOffset;
            // Keep the person upright; pitching a flat cutout toward the ceiling camera
            // pushes its upper body through the bed behind it.
            actor.transform.rotation = Quaternion.Euler(0, stageCamera.transform.eulerAngles.y, 0);
            float height = Mathf.Clamp(settings.ActorHeight, .6f, 2.5f);
            float scale = height / Mathf.Max(.01f, sprite.bounds.size.y);
            float breath = Mathf.Sin(clock * 1.4f) * Mathf.Clamp(profile.IdleMotion, 0, .03f) / height;
            actor.transform.localScale = new Vector3(scale, scale * (1 + breath), scale);
            signalMaterial.SetFloat("_Clock", clock);
            signalMaterial.SetFloat("_Exposure", Mathf.Clamp(profile.Exposure, .25f, 2));
            signalMaterial.SetFloat("_Saturation", Mathf.Clamp01(profile.Saturation));
            signalMaterial.SetFloat("_Noise", Mathf.Clamp(profile.Noise, 0, .2f));
            var previousTarget = RenderTexture.active;
            try
            {
                stageCamera.Render();
                Graphics.Blit(source, output, signalMaterial);
            }
            finally { RenderTexture.active = previousTarget; }
            sinceFrame = 0;
            needsFrame = false;
            RenderedFrames++;
        }

        private void OnEnable()
        {
            if (!Ready()) return;
            stage.SetActive(true);
            SelectRoom();
            RenderFrame();
        }
        private void HideFeed()
        {
            if (image != null) image.enabled = false;
            if (stage != null) stage.SetActive(false);
        }
        private void OnDisable() => HideFeed();
        private void ReleaseStage()
        {
            if (image != null) { image.enabled = false; image.texture = null; }
            if (stageCamera != null) stageCamera.targetTexture = null;
            if (stage != null) { stage.SetActive(false); Release(stage); }
            if (actorMaterial != null) Release(actorMaterial);
            if (signalMaterial != null) Release(signalMaterial);
            if (source != null) { source.Release(); Release(source); }
            if (output != null) { output.Release(); Release(output); }
            rooms.Clear();
            stage = null; activeRoom = null; stageCamera = null; actor = null;
            source = null; output = null;
        }
        private static void Release(Object value)
        {
            if (Application.isPlaying) Destroy(value); else DestroyImmediate(value);
        }
        private void OnDestroy() => ReleaseStage();
    }
}
