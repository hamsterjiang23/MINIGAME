using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace MiniGame.UnityHost
{
    /// <summary>Presentation-only cameras. No patient decisions or session state are stored here.</summary>
    [ExecuteAlways]
    public sealed class PatientSurveillanceFeed : MonoBehaviour
    {
        public const int StageLayer = 31;
        private const string StageName = "Patient surveillance stages";
        private int stageSlot = -1;
        private readonly Dictionary<string, PatientMonitorRoom> rooms = new Dictionary<string, PatientMonitorRoom>();
        private PatientMonitorProfile profile;
        private RawImage image;
        private GameObject stage;
        private Camera stageCamera;
        private SpriteRenderer actor;
        private SpriteRenderer backdrop;
        private Sprite backdropSprite;
        private Material actorMaterial, signalMaterial;
        private RenderTexture source, output;
        private PatientMonitorRoom activeRoom;
        private PatientMonitorProfile.Room settings;
        private string patientId;
        private Sprite sprite;
        private float clock, sinceFrame;
        private bool paused, failed, needsFrame, showing;
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
        public bool IsShowing => image != null && image.isActiveAndEnabled && output != null
            && output.IsCreated() && image.texture == output;
        public event System.Action<bool> VisibilityChanged;

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
            if (image != target || profile != configuration) ReleaseStage();
            image = target;
            profile = configuration;
            failed = false;
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
            if (stage == null || stageSlot < 0 || rooms.Count == 0 || revision != profile.Revision)
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
            CleanupOrphanedEditorStages();
            stage = new GameObject(StageName) { hideFlags = HideFlags.DontSave };
            // Preview and gameplay feeds can coexist. They share the CCTV layer, so place
            // their stages beyond each other's far plane rather than photographing both.
            // Inspect live stages so an Editor script reload cannot reset a static slot pool.
            var occupied = new HashSet<int>();
            // A preview owner can disappear during a reload while its native renderers survive.
            // Reserve the actual generated roots, including roots without a managed feed owner.
            foreach (var other in Resources.FindObjectsOfTypeAll<GameObject>())
                if (other != stage && IsGeneratedStage(other))
                    occupied.Add(Mathf.RoundToInt(other.transform.position.z / 128f));
            stageSlot = 0;
            while (occupied.Contains(stageSlot)) stageSlot++;
            stage.transform.position = new Vector3(6000, -6000, stageSlot * 128f);
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
            if (profile.Backdrop != null)
            {
                backdropSprite = Sprite.Create(profile.Backdrop, new Rect(0, 0, profile.Backdrop.width, profile.Backdrop.height), new Vector2(.5f, .5f), 100);
                var backgroundObject = new GameObject("Original white ward backdrop", typeof(SpriteRenderer));
                backgroundObject.transform.SetParent(stage.transform, false);
                backgroundObject.layer = StageLayer;
                backdrop = backgroundObject.GetComponent<SpriteRenderer>();
                backdrop.sprite = backdropSprite;
                backdrop.sharedMaterial = actorMaterial;
                backdrop.sortingOrder = -1;
                backdrop.shadowCastingMode = ShadowCastingMode.Off;
                backdrop.receiveShadows = false;
            }
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
            if (!rooms.TryGetValue(patientId, out var next)) { image.enabled = false; NotifyVisibility(); return; }
            if (activeRoom != next)
            {
                if (activeRoom != null) activeRoom.gameObject.SetActive(false);
                activeRoom = next;
                activeRoom.gameObject.SetActive(true);
            }
            image.enabled = true;
            actor.sprite = sprite;
            NotifyVisibility();
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
            if (backdrop != null)
            {
                // Flat artwork uses the original ward plate, without perspective skew or bed occlusion.
                activeRoom.gameObject.SetActive(false);
                stageCamera.orthographic = true;
                stageCamera.orthographicSize = 1;
                stageCamera.transform.position = stage.transform.position + new Vector3(0, 0, -10);
                stageCamera.transform.rotation = Quaternion.identity;
                float backgroundScale = Mathf.Max(2 * stageCamera.aspect / backdropSprite.bounds.size.x, 2 / backdropSprite.bounds.size.y);
                backdrop.transform.localPosition = Vector3.forward;
                backdrop.transform.localScale = Vector3.one * backgroundScale;
                actor.transform.rotation = Quaternion.identity;
                var placement = profile.FindPlacement(sprite.name);
                float portraitHeight = height * (placement != null ? Mathf.Clamp(placement.HeightMultiplier, .1f, 2) : 1) * (IsCloseUp ? 1.4f : 1);
                scale = portraitHeight / Mathf.Max(.01f, sprite.bounds.size.y);
                actor.transform.localScale = new Vector3(scale, scale * (1 + breath), scale);
                var bottom = placement != null ? placement.BottomCenter : new Vector2(.42f, .01f);
                float portraitY = IsCloseUp ? .96f - sprite.bounds.max.y * scale * (1 + breath)
                    : bottom.y * 2 - 1 - sprite.bounds.min.y * scale * (1 + breath);
                actor.transform.position = stage.transform.position + new Vector3((bottom.x * 2 - 1) * stageCamera.aspect, portraitY, 0) + settings.ActorOffset;
            }
            signalMaterial.SetFloat("_Clock", clock);
            // Both the ward plate and the person use the original amber CCTV transfer.
            signalMaterial.SetFloat("_OriginalColor", 0);
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
            NotifyVisibility();
        }
        private void NotifyVisibility()
        {
            bool visible = IsShowing;
            if (showing == visible) return;
            showing = visible;
            VisibilityChanged?.Invoke(visible);
        }
        private void OnDisable() => HideFeed();
        public void DisposePresentation() => ReleaseStage();
        private void ReleaseStage()
        {
            bool immediate = !Application.isPlaying;
#if UNITY_EDITOR
            immediate |= stage != null && (!stage.scene.IsValid() || UnityEditor.SceneManagement.EditorSceneManager.IsPreviewScene(stage.scene));
#endif
            if (image != null) { image.enabled = false; image.texture = null; }
            NotifyVisibility();
            if (stageCamera != null) stageCamera.targetTexture = null;
            if (stage != null)
                foreach (var room in stage.GetComponentsInChildren<PatientMonitorRoom>(true)) room.DisposePresentation(immediate);
            if (stage != null) { stage.SetActive(false); Release(stage, immediate); }
            stageSlot = -1;
            if (actorMaterial != null) Release(actorMaterial, immediate);
            if (backdropSprite != null) Release(backdropSprite, immediate);
            if (signalMaterial != null) Release(signalMaterial, immediate);
            if (source != null) { source.Release(); Release(source, immediate); }
            if (output != null) { output.Release(); Release(output, immediate); }
            rooms.Clear();
            stage = null; activeRoom = null; stageCamera = null; actor = null;
            backdrop = null; backdropSprite = null;
            source = null; output = null;
        }
        private static bool IsGeneratedStage(GameObject value) => value != null && value.name == StageName
            && value.transform.parent == null && (value.hideFlags & HideFlags.DontSave) != 0;

        public static int CleanupOrphanedEditorStages()
        {
            int removed = 0;
#if UNITY_EDITOR
            foreach (var root in Resources.FindObjectsOfTypeAll<GameObject>())
                if (IsGeneratedStage(root) && !root.scene.IsValid() && !UnityEditor.EditorUtility.IsPersistent(root))
                {
                    foreach (var room in root.GetComponentsInChildren<PatientMonitorRoom>(true)) room.DisposePresentation(true);
                    DestroyImmediate(root); removed++;
                }
#endif
            return removed;
        }

        private static void Release(Object value, bool immediate)
        {
            if (immediate) DestroyImmediate(value); else Destroy(value);
        }
        private void OnDestroy() => ReleaseStage();
    }
}
