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
        [SerializeField, InspectorName("门外人物上移（米）")] private float verticalOffset = .1f;
        private GameRuntime runtime;
        private ApartmentPlayer player;
        private DoorViewCamera doorway;
        private SpriteRenderer surface;
        private readonly Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();
        private Camera endingCamera;
        private Vector3 returnPosition;
        private Quaternion returnRotation;
        private float returnFieldOfView;

        public SpriteRenderer Surface => surface;
        public bool IsVisible => surface != null && surface.enabled && surface.gameObject.activeInHierarchy;
        public string CurrentPortraitId { get; private set; } = "JD_01";

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
            PlaceAtDoorway();
            SetPortrait("JD_01");
            if (game.IsCampaign && game.GetView().Game.Ending == EndingType.TakenAway)
            {
                SetPortrait("JD_06");
                ShowEndingAtDoorway(player.ViewCamera);
            }

            // A permanent world actor: the closed door occludes him naturally, rather than
            // spawning the portrait when the door opens or a morning dialogue starts.
            surface.enabled = isActiveAndEnabled;
        }

        private void PlaceAtDoorway()
        {
            var outward = Vector3.ProjectOnPlane(doorway.InsidePose.forward, Vector3.up).normalized;
            var position = doorway.InsidePose.position + outward * 1.75f;
            var controller = player.GetComponent<CharacterController>();
            position.y = controller != null && controller.bounds.size.y > .001f ? controller.bounds.min.y
                : controller != null ? player.transform.TransformPoint(controller.center).y
                    - controller.height * Mathf.Abs(player.transform.lossyScale.y) * .5f : player.transform.position.y;
            var effects = GetComponent<PresentationEffects>();
            if (effects != null && effects.TryGetRoomReturnPose(out var standingPosition, out _, out _, out _, out _))
                position.y = standingPosition.y;
            position.y += verticalOffset;
            surface.transform.SetPositionAndRotation(position, Quaternion.LookRotation(outward, Vector3.up));
            surface.transform.localScale = Vector3.one * (1.8f / surface.sprite.bounds.size.y);
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
            string resource = SupervisorDialogueContent.PortraitResource(id);
            if (resource == null) return null;
            var texture = Resources.Load<Texture2D>(resource);
            if (texture == null) return null;
            var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
                new Vector2(.5f, 0), 100, 0, SpriteMeshType.FullRect);
            sprite.name = "Supervisor " + id;
            sprites.Add(id, sprite);
            return sprite;
        }

        public bool SetPortrait(string id)
        {
            var portrait = GetPortrait(id);
            if (surface == null || portrait == null) return false;
            surface.sprite = portrait;
            CurrentPortraitId = id;
            return true;
        }

        public void ShowEndingAtDoorway(Camera camera)
        {
            if (endingCamera != null || camera == null || doorway == null || doorway.InsidePose == null) return;
            endingCamera = camera;
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
            if (surface != null) Destroy(surface.gameObject);
            foreach (var sprite in sprites.Values) if (sprite != null) Destroy(sprite);
            sprites.Clear();
        }
    }
}
