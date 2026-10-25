using MiniGame.Presentation;
using MiniGame.Core;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace MiniGame.UnityHost
{
    [DisallowMultipleComponent]
    public sealed class SupervisorDoorwayView : MonoBehaviour
    {
        [SerializeField, Min(.01f), InspectorName("监督员距门内对齐点（米）")] private float forwardDistance = 1.5f;
        [SerializeField, InspectorName("监督员左右偏移（米，右为正）")] private float horizontalOffset = -.22f;
        [SerializeField, InspectorName("门外人物上移（米）")] private float verticalOffset = .25f;
        private GameRuntime runtime;
        private ApartmentPlayer player;
        private DoorViewCamera doorway;
        private SpriteRenderer surface;
        private readonly Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();
        private Camera endingCamera;
        private Vector3 returnPosition;
        private Quaternion returnRotation;
        private float returnFieldOfView;
        [SerializeField, Min(0), InspectorName("开关门时最小向左避让（米）")] private float doorAvoidanceDistance = .8f;
        [SerializeField, Min(0), InspectorName("门与人物安全间距（米）")] private float doorClearanceMargin = .08f;
        [SerializeField, Min(.01f), InspectorName("开门后回位时间（秒）")] private float doorReturnSeconds = .6f;
        [SerializeField, Min(.01f), InspectorName("向左避让时间（秒）")] private float doorAvoidanceSeconds = .45f;
        private Vector3 doorwayPosition, priestPosition, doorwayRight;
        private float safeAvoidanceDistance;
        private float avoidance, slideFrom, slideTo, slideElapsed;
        private HingedDoor clearanceDoor;

        public SpriteRenderer Surface => surface;
        public bool IsVisible => surface != null && surface.enabled && surface.gameObject.activeInHierarchy;
        public string CurrentPortraitId { get; private set; } = "JD_01";
        public bool IsReadyForDialogue => endingCamera != null || (IsVisible && doorway != null
            && doorway.Phase == DoorCameraPhase.Viewing && !ShouldAvoidDoor() && avoidance == 0);

        public void Bind(GameRuntime game, ApartmentPlayer player, DoorViewCamera doorway)
        {
            PrepareForRuntimeChange();
            runtime = game;
            this.player = player;
            this.doorway = doorway;
            if (surface != null) surface.enabled = false;
            if (game == null || player == null || doorway == null || doorway.InsidePose == null) return;
            EnsureSurface();
            if (surface == null) return;
            clearanceDoor = doorway.Door;
            SetPortrait("JD_01");
            PlaceAtDoorway();
            if (clearanceDoor != null) clearanceDoor.MotionClearance = HasDoorClearance;
            if (game.IsCampaign && game.GetView().Game.Phase == GamePhase.Ending)
            {
                SetPortrait(game.GetView().Game.Ending == EndingType.PriestAttack ? "PE01" : "JD_06");
                if (!game.UsesEndingFlow) ShowEndingAtDoorway(player.ViewCamera);
            }

            // A permanent world actor: the closed door occludes him naturally, rather than
            // spawning the portrait when the door opens or a morning dialogue starts.
            surface.enabled = isActiveAndEnabled;
        }

        private void PlaceAtDoorway()
        {
            var outward = Vector3.ProjectOnPlane(doorway.InsidePose.forward, Vector3.up).normalized;
            doorwayRight = Vector3.ProjectOnPlane(doorway.InsidePose.right, Vector3.up).normalized;
            var position = doorway.InsidePose.position;
            var controller = player.GetComponent<CharacterController>();
            position.y = controller != null && controller.bounds.size.y > .001f ? controller.bounds.min.y
                : controller != null ? player.transform.TransformPoint(controller.center).y
                    - controller.height * Mathf.Abs(player.transform.lossyScale.y) * .5f : player.transform.position.y;
            var effects = GetComponent<PresentationEffects>();
            if (effects != null && effects.TryGetRoomReturnPose(out var standingPosition, out _, out _, out _, out _))
                position.y = standingPosition.y;
            // The priest keeps its previously authored ending position.
            priestPosition = position + outward * 1.75f + Vector3.up * .1f - doorwayRight * .4f;
            position += outward * forwardDistance + doorwayRight * horizontalOffset + Vector3.up * verticalOffset;
            surface.transform.SetPositionAndRotation(position, Quaternion.LookRotation(outward, Vector3.up));
            surface.transform.localScale = Vector3.one * (1.8f / surface.sprite.bounds.size.y);
            doorwayPosition = position;
            RefreshDoorClearance();
            avoidance = ShouldAvoidDoor() ? safeAvoidanceDistance : 0;
            slideFrom = slideTo = avoidance;
            slideElapsed = 0;
            ApplyDoorOffset();
        }

        private bool ShouldAvoidDoor() => endingCamera == null && doorway != null && doorway.Door != null
            && (doorway.Door.IsMoving || !doorway.Door.IsOpen || doorway.Phase == DoorCameraPhase.Returning);

        private void LateUpdate()
        {
            if (runtime == null || surface == null || doorway == null || runtime.GetView().Paused) return;
            var view = runtime.GetView();
            if (runtime.UsesEndingFlow && view.EndingStage == EndingStage.AwaitingDoor)
                SetPortrait(view.Game.Ending == EndingType.PriestAttack ? "PE01" : "JD_06");
            float target = ShouldAvoidDoor() ? safeAvoidanceDistance : 0;
            if (!Mathf.Approximately(slideTo, target))
            { slideFrom = avoidance; slideTo = target; slideElapsed = 0; }
            slideElapsed += Time.unscaledDeltaTime;
            float duration = target > 0 ? doorAvoidanceSeconds : doorReturnSeconds;
            float progress = Mathf.Clamp01(slideElapsed / Mathf.Max(.01f, duration));
            avoidance = Mathf.Lerp(slideFrom, slideTo, Mathf.SmoothStep(0, 1, progress));
            ApplyDoorOffset();
        }

        private bool HasDoorClearance() => !isActiveAndEnabled || surface == null
            || !ShouldAvoidDoor() || avoidance >= safeAvoidanceDistance - .001f;

        private bool IsPriest => CurrentPortraitId == "PE01" || CurrentPortraitId == "PE02";

        private void RefreshDoorClearance()
        {
            safeAvoidanceDistance = Mathf.Max(0, doorAvoidanceDistance);
            if (clearanceDoor == null || surface == null || doorwayRight.sqrMagnitude < .5f) return;
            var restingPosition = IsPriest ? priestPosition : doorwayPosition;
            var bounds = surface.bounds;
            // Use the complete portrait rectangle, including its transparent margins.
            float rightEdge = Vector3.Dot(restingPosition + bounds.center - surface.transform.position, doorwayRight)
                + Mathf.Abs(doorwayRight.x) * bounds.extents.x + Mathf.Abs(doorwayRight.z) * bounds.extents.z;
            float leftEdge = Vector3.Dot(clearanceDoor.SweepCenter, doorwayRight) - clearanceDoor.HorizontalSweepRadius;
            safeAvoidanceDistance = Mathf.Max(safeAvoidanceDistance, rightEdge - leftEdge + Mathf.Max(0, doorClearanceMargin));
        }

        private void ApplyDoorOffset()
        {
            if (surface != null) surface.transform.position = (IsPriest ? priestPosition : doorwayPosition) - doorwayRight * avoidance;
        }

        private void EnsureSurface()
        {
            if (surface != null) return;
            var sprite = GetPortrait("JD_01");
            if (sprite == null) return;
            var actor = new GameObject("Supervisor outside doorway", typeof(SpriteRenderer));
            actor.layer = 0;
            SceneManager.MoveGameObjectToScene(actor, gameObject.scene);
            surface = actor.GetComponent<SpriteRenderer>();
            surface.sprite = sprite;
            surface.enabled = false;
            surface.shadowCastingMode = ShadowCastingMode.Off;
            surface.receiveShadows = false;
        }

        private Sprite GetPortrait(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            if (sprites.TryGetValue(id, out var found)) return found;
            if (id == "PE01" || id == "PE02") return Resources.Load<PortraitCatalog>("MiniGame/PortraitCatalog")?.Find(id);
            string resource = SupervisorDialogueContent.PortraitResource(id);
            if (resource == null) return null;
            var texture = Resources.Load<Texture2D>(resource);
            if (texture == null) return null;
            var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
                PortraitPivot(id), 100, 0, SpriteMeshType.FullRect);
            sprite.name = "Supervisor " + id;
            sprites.Add(id, sprite);
            return sprite;
        }

        // Original PNGs have different transparent margins. Register their feet rather than
        // their canvases (alpha > 16, bottom 10% centroid); see the supervisor asset guide.
        private static Vector2 PortraitPivot(string id)
        {
            switch (id)
            {
                case "JD_01": return new Vector2(406.99f / 728, 31f / 1296);
                case "JD_02": return new Vector2(323.99f / 632, 52f / 1327);
                case "JD_03": return new Vector2(322.99f / 654, 5f / 1254);
                case "JD_04": return new Vector2(409.77f / 755, 18f / 1279);
                case "JD_05": return new Vector2(362.06f / 703, 4f / 1254);
                case "JD_06": return new Vector2(343.29f / 682, 42f / 1315);
                default: return new Vector2(.5f, 0);
            }
        }

        public bool SetPortrait(string id)
        {
            if (id != null && id.Length == 4 && id.StartsWith("JD", System.StringComparison.Ordinal)) id = "JD_" + id.Substring(2);
            var portrait = GetPortrait(id);
            if (surface == null || portrait == null) return false;
            surface.sprite = portrait;
            // Keep one reference pixel scale per actor across expressions and checkpoint rebinds.
            var reference = id == "PE01" || id == "PE02" ? GetPortrait("PE01") : GetPortrait("JD_01");
            surface.transform.localScale = Vector3.one * (1.8f / reference.bounds.size.y);
            CurrentPortraitId = id;
            ApplyDoorOffset();
            RefreshDoorClearance();
            return true;
        }

        public void ShowEndingAtDoorway(Camera camera)
        {
            if (endingCamera != null || camera == null || doorway == null || doorway.InsidePose == null) return;
            endingCamera = camera;
            avoidance = 0;
            ApplyDoorOffset();
            returnPosition = camera.transform.position; returnRotation = camera.transform.rotation; returnFieldOfView = camera.fieldOfView;
            // The ending cuts to the hall side of the closed door; the actor stays outside.
            var outward = Vector3.ProjectOnPlane(doorway.InsidePose.forward, Vector3.up).normalized;
            camera.transform.SetPositionAndRotation(doorway.InsidePose.position + outward * .95f, doorway.InsidePose.rotation);
            camera.fieldOfView = 70;
        }

        public void ShowEndingAtDoorway() => ShowEndingAtDoorway(player != null ? player.ViewCamera : null);

        public bool TryGetRoomReturnPose(out Vector3 position, out Quaternion rotation, out float fieldOfView)
        {
            position = returnPosition; rotation = returnRotation; fieldOfView = returnFieldOfView;
            return endingCamera != null;
        }

        public void PrepareForRuntimeChange()
        {
            if (clearanceDoor != null && clearanceDoor.MotionClearance == HasDoorClearance)
                clearanceDoor.MotionClearance = null;
            clearanceDoor = null;
            if (endingCamera == null) return;
            endingCamera.transform.SetPositionAndRotation(returnPosition, returnRotation);
            endingCamera.fieldOfView = returnFieldOfView;
            endingCamera = null;
        }

        private void OnDisable()
        {
            PrepareForRuntimeChange();
            if (surface != null) surface.enabled = false;
        }

        private void OnEnable()
        {
            if (runtime != null) Bind(runtime, player, doorway);
        }

        private void OnDestroy()
        {
            PrepareForRuntimeChange();
            if (surface != null) Destroy(surface.gameObject);
            foreach (var sprite in sprites.Values) if (sprite != null) Destroy(sprite);
            sprites.Clear();
        }
    }
}
