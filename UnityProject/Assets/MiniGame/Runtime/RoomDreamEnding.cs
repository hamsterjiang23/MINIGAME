using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MiniGame.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MiniGame.UnityHost
{
    /// <summary>Temporarily lends the room's actor, camera and original bed to the hidden ending.</summary>
    public sealed class RoomDreamEnding : MonoBehaviour
    {
        private const int BedLayer = 26;
        private readonly List<EnabledState<MonoBehaviour>> behaviours = new List<EnabledState<MonoBehaviour>>();
        private readonly List<EnabledState<Canvas>> canvases = new List<EnabledState<Canvas>>();
        private readonly List<EnabledState<Renderer>> renderers = new List<EnabledState<Renderer>>();
        private readonly List<EnabledState<Collider>> colliders = new List<EnabledState<Collider>>();
        private readonly List<EnabledState<Light>> lights = new List<EnabledState<Light>>();
        private readonly List<EnabledState<Camera>> cameras = new List<EnabledState<Camera>>();
        private readonly List<TransformState> moved = new List<TransformState>();
        private readonly Dictionary<GameObject, int> layers = new Dictionary<GameObject, int>();
        private readonly List<AudioState> audio = new List<AudioState>();
        private MiniGameBootstrap room;
        private MiniGameSessionHost session;
        private ApartmentPlayer player;
        private CharacterController character;
        private Transform bed, pillow, standing;
        private Renderer[] bedRenderers;
        private Vector3 savedActorPosition, savedCameraPosition, safeSpawn, cameraLocalPosition;
        private Quaternion savedActorRotation, savedCameraRotation;
        private float floorY, yaw, pitch, savedFieldOfView, savedNear, savedFar;
        private int savedCameraMask;
        private CameraClearFlags savedCameraFlags;
        private bool characterWasEnabled, cameraSaved, roomDisabled, restored;
        private CursorLockMode savedCursorLock;
        private bool savedCursorVisible;
        private RetroScreenEffect retro;
        private bool savedBypass;
        private bool savedRetroEnabled, savedRetroOriginal;
        private RetroScreenProfile savedRetroProfile;
        private LightingState lighting;
        private Image fade;
        private RoomEndingTransitionClock transition;
        private Font failureFont;
        private Text failureLabel;
        private bool bedCompleted, resumingExploration;
        private Vector3 panoramaOrigin;
        private float bedFocusDistance = 3.2f;
        private InteractionFocusOutline bedFocusOutline;

        public event Action BedCompleted;
        public bool IsTransitioning => !restored && !Ready && !bedCompleted;
        public RoomEndingTransitionPhase TransitionPhase => transition?.State.Phase ?? RoomEndingTransitionPhase.FadingOut;
        public float TransitionElapsed => transition?.State.Elapsed ?? 0;

        public bool Ready { get; private set; }
        public bool IsPreview { get; private set; }
        public bool IsPortalPreview { get; private set; }
        public bool UsesSharedThanks => !IsPreview && session != null && session.Runtime != null && session.Runtime.UsesEndingFlow;
        public string FailureReason { get; private set; }
        public DreamEndingController Ending { get; private set; }
        public DreamCubemapRenderer Cubemap { get; private set; }
        public DreamCubemapRoomSurface RoomSurface { get; private set; }
        public DreamWallPortal Portal { get; private set; }
        public Bounds MappedRoomBounds { get; private set; }
        public Bounds ExplorationBounds { get; private set; }
        public Vector3 PlayerPosition => player != null ? player.transform.position : Vector3.zero;
        public Vector3 PanoramaOrigin => panoramaOrigin;
        public Camera ViewCamera { get; private set; }

        public static RoomDreamEnding Begin(MiniGameBootstrap room, MiniGameSessionHost session, bool isPreview = false,
            bool wallPortalPreview = false)
        {
            if (room == null || room.Player == null || room.Player.ViewCamera == null)
                throw new InvalidOperationException("房间梦结局缺少玩家或相机。");
            if (session == null || session.Runtime == null || session.DreamProfile == null)
                throw new InvalidOperationException("房间梦结局缺少有效会话或梦境配置。");
            var existing = room.gameObject.scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<RoomDreamEnding>(true))
                .FirstOrDefault(value => !value.restored);
            if (existing != null) return existing;
            var root = new GameObject("Room Hidden Dream Ending");
            SceneManager.MoveGameObjectToScene(root, room.gameObject.scene);
            var owner = root.AddComponent<RoomDreamEnding>();
            owner.room = room;
            owner.IsPreview = isPreview;
            owner.IsPortalPreview = isPreview && wallPortalPreview;
            owner.session = session;
            owner.player = room.Player;
            owner.ViewCamera = room.Player.ViewCamera;
            if (!isPreview) session.RestoreEndingFadePose(room.Player);
            owner.savedCursorLock = Cursor.lockState;
            owner.savedCursorVisible = Cursor.visible;
            owner.transition = new RoomEndingTransitionClock(isPreview ? null : session.RoomEndingTransition);
            owner.resumingExploration = owner.TransitionPhase == RoomEndingTransitionPhase.Exploring;
            owner.CreateFade();
            owner.StartCoroutine(owner.Enter());
            return owner;
        }

        private IEnumerator Enter()
        {
            if (TransitionPhase == RoomEndingTransitionPhase.Failed)
            {
                ShowFailure("隐藏空间尚未就绪。按 R 重试。");
                yield break;
            }
            while (TransitionPhase == RoomEndingTransitionPhase.FadingOut)
            {
                AdvanceTransition(false);
                yield return null;
            }
            // Always cover reconstruction, including a save restored halfway through the entry camera motion.
            fade.color = Color.black;
            // A saved bed presentation is rebuilt synchronously before its first rendered frame.
            if (!resumingExploration) yield return null;
            try
            {
                PrepareRoom();
                var sky = session.DreamProfile.RoomPanoramaSkybox;
                if (Portal != null && sky == null) sky = ViewCamera.GetComponent<Skybox>().material;
                Cubemap = sky != null
                    ? new DreamCubemapRenderer(session.DreamProfile, ViewCamera, sky, PanoramaOrigin)
                    : new DreamCubemapRenderer(session.DreamProfile, ViewCamera, false);
                RoomSurface = new DreamCubemapRoomSurface(MappedRoomBounds, transform, BedLayer, PanoramaOrigin);
                if (Portal != null)
                {
                    Portal.CaptureHeadWallpaper(standing.position);
                    RoomSurface.BindHead(Portal.HeadTexture, Portal.HeadProjectionOrigin);
                }
                ViewCamera.clearFlags = Portal == null ? CameraClearFlags.SolidColor : CameraClearFlags.Skybox;
                Ending = gameObject.AddComponent<DreamEndingController>();
                Ending.ConfigureRoom(session.DreamProfile, ViewCamera, bed, pillow, standing, this);
                if (!Cubemap.IsReady)
                    throw new InvalidOperationException("海滩全景尚未完成首幅六面捕获。");
                PrepareEntryPose();
            }
            catch (Exception exception)
            {
                FailureReason = "隐藏梦结局未能开始：" + exception.Message;
                Debug.LogException(exception, this);
                transition.Fail();
                PublishTransition();
                ShowFailure(FailureReason + "\n按 R 重试");
                yield break;
            }
            while (TransitionPhase != RoomEndingTransitionPhase.Exploring)
            {
                AdvanceTransition(true);
                ApplyEntryPose();
                yield return null;
            }
            if (restored) yield break;
            Ready = true;
            // The free-look controller takes ownership only after the camera reaches its exact target.
            if (!resumingExploration)
                PlacePose(safeSpawn, HeadingToBed(safeSpawn), 8);
            if (fade != null) fade.gameObject.SetActive(false);
            PublishTransition();
        }

        private void AdvanceTransition(bool sceneReady)
        {
            bool paused = session.Runtime.GetView().Paused || MainMenuController.BlocksGameplayInput;
            transition.Advance(Time.unscaledDeltaTime, sceneReady, paused);
            if (fade != null) fade.color = new Color(0, 0, 0, transition.Opacity);
            PublishTransition();
        }

        private void PrepareEntryPose()
        {
            var state = transition.State;
            if (state.Phase == RoomEndingTransitionPhase.Exploring) return;
            bool valid = state.HasEntryPose && Finite(state.EntryStartPosition) && Finite(state.EntryTargetPosition)
                && ValidRotation(state.EntryStartRotation) && ValidRotation(state.EntryTargetRotation);
            if (!valid)
            {
                state.HasEntryPose = true;
                state.MonitorReturn = Portal != null;
                state.EntryTargetPosition = standing.position;
                state.EntryTargetRotation = standing.rotation;
                if (Portal != null)
                {
                    float distance = Mathf.Max(ViewCamera.nearClipPlane * 1.5f,
                        Portal.PrintedScreenHeight * .5f / Mathf.Tan(24 * Mathf.Deg2Rad) * .82f);
                    state.EntryStartPosition = Portal.PrintedScreenCenter - Portal.PrintedScreenNormal * distance;
                    state.EntryStartRotation = Quaternion.LookRotation(Portal.PrintedScreenNormal, Vector3.up);
                }
                else
                {
                    var start = AvoidBed(Clamp(safeSpawn + Vector3.ProjectOnPlane(standing.forward, Vector3.up).normalized * .35f));
                    state.EntryStartPosition = standing.position + start - safeSpawn;
                    state.EntryStartRotation = Quaternion.Euler(0, -12, 0) * standing.rotation;
                }
            }
            ApplyEntryPose();
        }

        private void ApplyEntryPose()
        {
            var state = transition.State;
            if (!state.HasEntryPose || ViewCamera == null || state.Phase == RoomEndingTransitionPhase.Exploring && resumingExploration) return;
            if (state.MonitorReturn)
            {
                float progress = state.Phase == RoomEndingTransitionPhase.Exploring ? 1
                    : state.Phase == RoomEndingTransitionPhase.Entering ? state.Elapsed / RoomEndingTransitionClock.EntrySeconds : 0;
                TerminalReturnMotion.Apply(ViewCamera, state.EntryStartPosition, state.EntryStartRotation, 48,
                    state.EntryTargetPosition, state.EntryTargetRotation, 62, progress);
                return;
            }
            float t = state.Phase == RoomEndingTransitionPhase.Exploring ? 1 : state.Phase == RoomEndingTransitionPhase.Entering
                ? Mathf.SmoothStep(0, 1, Mathf.Clamp01(state.Elapsed / RoomEndingTransitionClock.EntrySeconds)) : 0;
            ViewCamera.transform.SetPositionAndRotation(Vector3.Lerp(state.EntryStartPosition, state.EntryTargetPosition, t),
                Quaternion.Slerp(state.EntryStartRotation, state.EntryTargetRotation, t));
        }

        public RoomEndingTransitionCheckpoint CaptureTransitionCheckpoint() => transition?.State.Copy();
        private void PublishTransition()
        {
            if (!IsPreview && session != null) session.RoomEndingTransition = CaptureTransitionCheckpoint();
        }

        public bool CompleteAtBed()
        {
            if (!UsesSharedThanks || !Ready || bedCompleted || session.Runtime.GetView().Paused
                || session.Runtime.EndingStage != MiniGame.Presentation.EndingStage.Space
                || Ending == null || !Ending.IsBedEndingComplete) return false;
            RestoreThanksPresentation();
            BedCompleted?.Invoke();
            return true;
        }

        public void RestoreThanksPresentation()
        {
            if (restored) return;
            bedCompleted = true;
            Ready = false;
            Ending?.HideForThanks();
        }

        private void Update()
        {
            if (restored || Ready || bedCompleted || MainMenuController.BlocksGameplayInput) return;
            if (room != null && !room.enabled && Input.GetKeyDown(KeyCode.Escape))
            {
                if (Ending != null) Ending.HandlePauseEscape();
                else session.Runtime.SetPaused(!session.Runtime.GetView().Paused);
            }
            if (TransitionPhase == RoomEndingTransitionPhase.Failed && Input.GetKeyDown(KeyCode.R)) RetryInitialization();
        }

        private void ShowFailure(string message)
        {
            FailureReason = message;
            fade.color = Color.black;
            if (failureLabel == null)
            {
                var root = new GameObject("Hidden ending failure", typeof(RectTransform), typeof(Text));
                root.transform.SetParent(fade.transform.parent, false);
                var rect = (RectTransform)root.transform;
                rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f);
                rect.sizeDelta = new Vector2(1200, 240);
                failureLabel = root.GetComponent<Text>();
                failureFont = GameUiTypography.CreateFont();
                failureLabel.font = failureFont; failureLabel.fontSize = 24;
                failureLabel.alignment = TextAnchor.MiddleCenter;
            }
            failureLabel.text = message;
            failureLabel.gameObject.SetActive(true);
        }

        public bool RetryInitialization()
        {
            if (restored || TransitionPhase != RoomEndingTransitionPhase.Failed || session.Runtime.GetView().Paused) return false;
            StartCoroutine(RetryUnderBlack());
            return true;
        }

        private IEnumerator RetryUnderBlack()
        {
            if (resumingExploration)
                transition = new RoomEndingTransitionClock(new RoomEndingTransitionCheckpoint { Phase = RoomEndingTransitionPhase.Exploring });
            else transition.Retry();
            RestoreRoomState();
            foreach (Transform child in transform)
                if (child != fade.transform.parent) Destroy(child.gameObject);
            if (Ending != null)
            {
                if (Ending.Audio != null) Destroy(Ending.Audio);
                Destroy(Ending);
            }
            Ending = null; Cubemap = null; RoomSurface = null; Portal = null;
            behaviours.Clear(); canvases.Clear(); renderers.Clear(); colliders.Clear(); lights.Clear(); cameras.Clear();
            moved.Clear(); layers.Clear(); audio.Clear();
            cameraSaved = roomDisabled = false;
            failureLabel.gameObject.SetActive(false);
            FailureReason = null;
            // Delayed destruction finishes beneath the still-opaque transition canvas.
            yield return null;
            yield return Enter();
        }

        private void PrepareRoom()
        {
            character = player.GetComponent<CharacterController>();
            if (character == null) throw new InvalidOperationException("房间玩家缺少 CharacterController。");
            characterWasEnabled = character.enabled;
            room.Dreams?.PrepareForRuntimeChange();
            ViewCamera.GetComponent<DoorViewCamera>()?.PrepareForRuntimeChange(session.Runtime);
            room.Effects?.PrepareForRuntimeChange();
            savedActorPosition = player.transform.position;
            savedActorRotation = player.transform.rotation;
            savedCameraPosition = ViewCamera.transform.position;
            savedCameraRotation = ViewCamera.transform.rotation;
            savedFieldOfView = ViewCamera.fieldOfView;
            savedNear = ViewCamera.nearClipPlane;
            savedFar = ViewCamera.farClipPlane;
            savedCameraMask = ViewCamera.cullingMask;
            savedCameraFlags = ViewCamera.clearFlags;
            cameraLocalPosition = player.StandingEyeLocalPosition;
            floorY = savedActorPosition.y;
            cameraSaved = true;
            retro = ViewCamera.GetComponent<RetroScreenEffect>();
            if (retro != null)
            {
                savedBypass = retro.PresentationBypass;
                savedRetroEnabled = retro.enabled;
                savedRetroOriginal = retro.PreviewOriginal;
                savedRetroProfile = retro.PresentationProfile;
                retro.PreviewOriginal = false;
                retro.PresentationBypass = true;
            }

            foreach (var roomAudio in RoomComponents<AudioSource>()) audio.Add(new AudioState(roomAudio));
            // OnDisable restores any ordinary dream, bedside isolation and focus effects first.
            foreach (var behaviour in RoomComponents<MonoBehaviour>())
            {
                if (behaviour == null || behaviour is EventSystem || behaviour is BaseInputModule
                    || behaviour is MiniGameSessionHost || behaviour is RetroScreenEffect || behaviour is PlayerBrightnessEffect) continue;
                behaviours.Add(new EnabledState<MonoBehaviour>(behaviour, behaviour.enabled));
                behaviour.enabled = false;
            }
            roomDisabled = true;
            foreach (var value in audio) value.Silence();
            lighting = new LightingState();

            var allRenderers = RoomComponents<Renderer>();
            string[] names = { "B_SM_bed_1", "B_SM_mattress_1", "B_SM_pillow_1" };
            bedRenderers = names.Select(name => allRenderers.SingleOrDefault(value => value.name == name)).ToArray();
            if (bedRenderers.Any(value => value == null))
                throw new InvalidOperationException("房间原床缺少床架、床垫或枕头网格。");
            var allColliders = RoomComponents<Collider>();
            var doorwaySpawn = RoomComponents<Transform>().FirstOrDefault(value => value.name == "Player spawn");
            if (doorwaySpawn == null) throw new InvalidOperationException("固定全景布景缺少门口 Player spawn 定位点。");
            floorY = doorwaySpawn.position.y;
            panoramaOrigin = doorwaySpawn.position + cameraLocalPosition;
            var floor = allColliders.FirstOrDefault(value => value.name == "Reference floor collider");
            Bounds floorBounds;
            if (floor != null) floorBounds = floor.bounds;
            else
            {
                var surface = allRenderers.FirstOrDefault(value => value.name == "B_Floor" || value.name == "Floor substrate");
                if (surface == null) throw new InvalidOperationException("房间梦结局找不到原房间地面边界。");
                floorBounds = surface.bounds;
            }
            if (floorBounds.size.x <= character.radius * 2 || floorBounds.size.z <= character.radius * 2)
                throw new InvalidOperationException("房间梦结局的可行走地面边界无效。");
            ExplorationBounds = new Bounds(new Vector3(floorBounds.center.x, floorY, floorBounds.center.z),
                new Vector3(floorBounds.size.x, character.height, floorBounds.size.z));
            var ceiling = allColliders.FirstOrDefault(value => value.name == "Reference ceiling collider");
            float bottom = floorBounds.max.y;
            float top = ceiling != null ? ceiling.bounds.min.y : bottom + 3.12f;
            if (top - bottom < character.height)
                throw new InvalidOperationException("房间全景的地板与天花板高度无效。");
            MappedRoomBounds = new Bounds(new Vector3(floorBounds.center.x, (bottom + top) * .5f, floorBounds.center.z),
                new Vector3(floorBounds.size.x, top - bottom, floorBounds.size.z));
            var bedInteraction = RoomComponents<WorldInteraction>().FirstOrDefault(value => value.Target == InteractionTarget.Bed);
            bedFocusDistance = bedInteraction != null ? bedInteraction.InteractionDistance : 3.2f;
            var bedColliders = allColliders.Where(value => bedRenderers.Any(part => value.transform == part.transform
                    || value.transform.IsChildOf(part.transform))
                || bedInteraction != null && (value.transform == bedInteraction.transform
                    || value.transform.IsChildOf(bedInteraction.transform))).ToArray();

            foreach (var value in allRenderers)
            {
                renderers.Add(new EnabledState<Renderer>(value, value.enabled));
                value.enabled = bedRenderers.Contains(value);
            }
            foreach (var value in RoomComponents<Canvas>())
            { canvases.Add(new EnabledState<Canvas>(value, value.enabled)); value.enabled = false; }
            foreach (var value in RoomComponents<Light>())
            { lights.Add(new EnabledState<Light>(value, value.enabled)); value.enabled = false; }
            foreach (var value in RoomComponents<Camera>())
            { cameras.Add(new EnabledState<Camera>(value, value.enabled)); value.enabled = value == ViewCamera; }
            foreach (var value in allColliders)
            {
                if (value == character) continue;
                colliders.Add(new EnabledState<Collider>(value, value.enabled));
                value.enabled = value == floor || bedColliders.Contains(value);
            }

            var bedBounds = BedBounds();
            bed = new GameObject("Original Room Bed Pivot").transform;
            bed.SetParent(transform, false);
            bed.position = new Vector3(bedBounds.center.x, bedBounds.min.y, bedBounds.center.z);
            var head = Vector3.ProjectOnPlane(bedRenderers[2].bounds.center - bed.position, Vector3.up);
            bed.rotation = head.sqrMagnitude > .001f ? Quaternion.LookRotation(head, Vector3.up) : Quaternion.identity;
            foreach (var part in bedRenderers) MoveWithBed(part.transform);
            foreach (var collider in bedColliders)
                if (!collider.transform.IsChildOf(bed)) MoveWithBed(collider.transform);
            // The daytime interaction collider may be a trigger; the dream needs a solid bed body.
            var bedObstacle = new GameObject("Dream bed solid collider");
            bedObstacle.transform.position = bedBounds.center;
            bedObstacle.transform.SetParent(bed, true);
            bedObstacle.layer = BedLayer;
            bedObstacle.AddComponent<BoxCollider>().size = bedBounds.size;
            pillow = new GameObject("Room Pillow Eye Position").transform;
            pillow.SetParent(bed, false);
            var pillowBounds = bedRenderers[2].bounds;
            pillow.position = new Vector3(pillowBounds.center.x, pillowBounds.max.y + .12f, pillowBounds.center.z);
            pillow.rotation = Quaternion.LookRotation(Vector3.up, bed.forward);
            if (UsesSharedThanks || IsPortalPreview)
                Portal = new DreamWallPortal(session.DreamProfile, ViewCamera, transform,
                    MappedRoomBounds, bed, bedRenderers, PanoramaOrigin);
            var bedLight = new GameObject("Dream Bed Light", typeof(Light));
            bedLight.transform.SetParent(transform, false);
            bedLight.transform.rotation = Quaternion.Euler(52, -32, 0);
            var light = bedLight.GetComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(1, .97f, .9f);
            light.intensity = .9f;
            light.shadows = LightShadows.None;
            light.cullingMask = (1 << BedLayer) | (Portal != null
                ? 1 << DreamWallPortal.HeadCaptureLayer : 0);
            ViewCamera.cullingMask = 1 << BedLayer;
            ViewCamera.nearClipPlane = .03f;
            ViewCamera.farClipPlane = 1200;
            ViewCamera.fieldOfView = 62;
            var source = session.DreamProfile.CubemapSourcePrefab != null
                ? session.DreamProfile.CubemapSourcePrefab.GetComponent<DreamCubemapSource>() : null;
            if (source != null) lighting.Apply(source);

            safeSpawn = AvoidBed(Clamp(Portal != null
                ? Portal.PrintedScreenCenter - Portal.PrintedScreenNormal * 1.05f : doorwaySpawn.position));
            standing = new GameObject("Room Dream Standing Pose").transform;
            standing.SetParent(transform, false);
            PlacePose(safeSpawn, HeadingToBed(safeSpawn), 8);
            standing.SetPositionAndRotation(ViewCamera.transform.position, ViewCamera.transform.rotation);
            Physics.SyncTransforms();
        }

        private T[] RoomComponents<T>() where T : Component => room.gameObject.scene.GetRootGameObjects()
            .SelectMany(root => root.GetComponentsInChildren<T>(true))
            .Where(value => value != null && !value.transform.IsChildOf(transform)).ToArray();

        private void MoveWithBed(Transform value)
        {
            moved.Add(new TransformState(value));
            foreach (var child in value.GetComponentsInChildren<Transform>(true))
            {
                if (!layers.ContainsKey(child.gameObject)) layers.Add(child.gameObject, child.gameObject.layer);
                child.gameObject.layer = BedLayer;
            }
            value.SetParent(bed, true);
        }

        public void AdvanceExploration(Vector2 movement, Vector2 look, float dt)
        {
            if (!Ready || restored || character == null || player == null || session == null
                || session.Runtime.GetView().Paused || Ending != null && Ending.Phase != BeachEndingPhase.Walking
                || !Finite(dt) || dt < 0) return;
            if (!Finite(movement.x) || !Finite(movement.y)) movement = Vector2.zero;
            if (!Finite(look.x) || !Finite(look.y)) look = Vector2.zero;
            yaw = Mathf.Repeat(yaw + look.x * PlayerRuntimeSettings.DefaultMouseSensitivity, 360);
            pitch = Mathf.Clamp(pitch - look.y * PlayerRuntimeSettings.DefaultMouseSensitivity, -75, 75);
            player.transform.rotation = Quaternion.Euler(0, yaw, 0);
            ViewCamera.transform.localRotation = Quaternion.Euler(pitch, 0, 0);
            if (movement.sqrMagnitude > 1) movement.Normalize();
            float speed = session.DreamProfile.WalkSpeed;
            if (!Finite(speed)) speed = 1.2f;
            var displacement = (player.transform.right * movement.x + player.transform.forward * movement.y)
                * Mathf.Clamp(speed, .1f, 4) * Mathf.Min(dt, .25f);
            var target = AvoidBed(Clamp(player.transform.position + displacement));
            if (!character.enabled) character.enabled = true;
            character.Move(target - player.transform.position);
            var corrected = AvoidBed(Clamp(player.transform.position));
            if ((corrected - player.transform.position).sqrMagnitude > .000001f)
            {
                character.enabled = false;
                player.transform.position = corrected;
                character.enabled = true;
            }
        }

        public bool IsNearBed(float distance)
        {
            if (restored || bedRenderers == null || player == null || !Finite(distance)) return false;
            var bounds = BedBounds();
            var point = player.transform.position;
            var closest = bounds.ClosestPoint(point);
            point.y = closest.y = 0;
            return Vector3.Distance(point, closest) <= Mathf.Max(.1f, distance);
        }

        public bool IsBedFocused()
        {
            return Ready && ViewCamera != null && bed != null
                && Physics.Raycast(ViewCamera.transform.position, ViewCamera.transform.forward, out var hit,
                    bedFocusDistance, 1 << BedLayer, QueryTriggerInteraction.Ignore)
                && hit.collider.transform.IsChildOf(bed);
        }

        public void SetBedFocus(bool focused)
        {
            if (!focused)
            {
                bedFocusOutline?.Dispose(); bedFocusOutline = null;
            }
            else if (bedFocusOutline == null && bedRenderers != null)
                bedFocusOutline = InteractionFocusOutline.Create(bedRenderers[0], bedRenderers);
        }

        private const float SkyRevealStart = .74f, SkyRevealEnd = .94f;
        public const float SkyRevealPitch = 82;

        public Quaternion WallLyingRotation => Quaternion.LookRotation(Vector3.up, Portal.WallNormal);

        public void ApplyWallLieDown(Vector3 start, Quaternion startRotation, Vector3 target, float progress)
        {
            // Turn in place before reclining. Keep the pillow behind the head throughout the descent,
            // including an approach from either side of the bed, instead of diving face-first into it.
            float turn = EaseWallMotion(Mathf.InverseLerp(0, .5f, progress));
            float lie = EaseWallMotion(Mathf.InverseLerp(.5f, .8f, progress));
            var intoPillow = Vector3.ProjectOnPlane(target - start, Vector3.up);
            if (intoPillow.sqrMagnitude < .0001f) intoPillow = Portal.WallNormal;
            intoPillow.Normalize();
            var awayRotation = Quaternion.LookRotation(-intoPillow, Vector3.up);
            // Settle onto the bed's long axis while reclining, so side approaches end in the same upright pose.
            var settledDirection = Vector3.Slerp(intoPillow, Portal.WallNormal, lie);
            var reclineRotation = Quaternion.LookRotation(-settledDirection, Vector3.up) * Quaternion.Euler(-90 * lie, 0, 0);
            var startAngles = startRotation.eulerAngles;
            var turnRotation = Quaternion.Euler(Mathf.LerpAngle(startAngles.x, 0, turn),
                Mathf.LerpAngle(startAngles.y, awayRotation.eulerAngles.y, turn), 0);
            // Stay just inside the pillow wall until the sky-only dissolve has finished. Crossing
            // earlier reveals the exterior computer pile while the player can still see the horizon.
            var wallFront = target - Portal.WallNormal * (Portal.Wall.GetDistanceToPoint(target) + .12f);
            wallFront.y = start.y; // Keep the bed rails below the sky view until the dissolve is complete.
            var insidePosition = Vector3.Lerp(start, wallFront, lie * lie);
            float crossing = EaseWallMotion(Mathf.InverseLerp(SkyRevealEnd, 1, progress));
            var position = Vector3.Lerp(insidePosition, target, crossing);
            position.y = Mathf.Lerp(start.y, target.y, crossing * crossing);
            ViewCamera.transform.SetPositionAndRotation(progress <= .5f ? start : position,
                progress <= .5f ? turnRotation : reclineRotation);
        }

        private static float EaseWallMotion(float t) => t * t * t * (t * (t * 6 - 15) + 10);

        public void CapturePose(BeachEndingCheckpoint checkpoint)
        {
            if (checkpoint == null || restored || player == null) return;
            checkpoint.HasRoomPose = true;
            checkpoint.RoomPosition = player.transform.position;
            checkpoint.RoomYaw = yaw;
            checkpoint.RoomPitch = pitch;
        }

        public void RestorePose(BeachEndingCheckpoint checkpoint)
        {
            if (restored || !cameraSaved) return;
            bool valid = checkpoint != null && checkpoint.HasRoomPose && Finite(checkpoint.RoomPosition)
                && Finite(checkpoint.RoomYaw) && Finite(checkpoint.RoomPitch);
            var position = valid ? AvoidBed(Clamp(checkpoint.RoomPosition)) : safeSpawn;
            PlacePose(position, valid ? checkpoint.RoomYaw : HeadingToBed(position), valid ? checkpoint.RoomPitch : 0);
        }

        private void PlacePose(Vector3 position, float heading, float elevation)
        {
            character.enabled = false;
            yaw = Mathf.Repeat(heading, 360);
            pitch = Mathf.Clamp(elevation, -75, 75);
            player.transform.SetPositionAndRotation(position, Quaternion.Euler(0, yaw, 0));
            ViewCamera.transform.localPosition = cameraLocalPosition;
            ViewCamera.transform.localRotation = Quaternion.Euler(pitch, 0, 0);
            character.enabled = true;
        }

        private Bounds BedBounds()
        {
            var result = bedRenderers[0].bounds;
            for (int i = 1; i < bedRenderers.Length; i++) result.Encapsulate(bedRenderers[i].bounds);
            return result;
        }

        public void UpdateRoomSurface()
        {
            if (retro != null)
            {
                retro.enabled = true;
                retro.PresentationProfile = session.DreamProfile.EndingRetro;
                retro.PresentationBypass = !session.DreamProfile.EnableEndingRetro;
            }
            if (Cubemap != null && Cubemap.IsReady)
            {
                RoomSurface?.Bind(Cubemap.Texture, session.DreamProfile.CubemapYaw);
                RoomSurface?.SetOutline(session.DreamProfile.ShowRoomOutline);
            }
        }

        public bool IsFacingPrintedMonitor
        {
            get
            {
                if (restored || Portal == null || ViewCamera == null || Portal.IsOutside
                    || TransitionPhase != RoomEndingTransitionPhase.Entering && TransitionPhase != RoomEndingTransitionPhase.Exploring)
                    return false;
                var toScreen = Portal.PrintedScreenCenter - ViewCamera.transform.position;
                // A small central viewing cone allows natural free look without requiring pixel-perfect aim.
                return Vector3.Dot(toScreen, Portal.PrintedScreenNormal) > 0
                    && Vector3.Dot(ViewCamera.transform.forward, toScreen.normalized) >= Mathf.Cos(15 * Mathf.Deg2Rad);
            }
        }

        public void UpdatePortalView(float time, float? lieProgress = null)
        {
            if (Portal == null) return;
            float reveal = Portal.IsOutside ? 1 : 0;
            if (!Portal.IsOutside && lieProgress.HasValue
                && Vector3.Dot(ViewCamera.transform.forward, Vector3.up) >= Mathf.Sin(SkyRevealPitch * Mathf.Deg2Rad))
                reveal = EaseWallMotion(Mathf.InverseLerp(SkyRevealStart, SkyRevealEnd, lieProgress.Value));
            Portal.UpdateView(time, reveal);
            RoomSurface?.SetOpacity(1 - reveal);
        }

        public void KeepBedInsideRoom()
        {
            // Clamp the rotated bed's actual footprint, retaining its existing bob and tilt.
            var bounds = BedBounds();
            var min = MappedRoomBounds.min;
            var max = MappedRoomBounds.max;
            float dx = Mathf.Max(0, min.x - bounds.min.x) - Mathf.Max(0, bounds.max.x - max.x);
            float dz = Mathf.Max(0, min.z - bounds.min.z) - Mathf.Max(0, bounds.max.z - max.z);
            float dy = Mathf.Max(0, min.y - bounds.min.y) - Mathf.Max(0, bounds.max.y - max.y);
            bed.position += new Vector3(dx, dy, dz);
        }

        private float HeadingToBed(Vector3 position)
        {
            var direction = BedBounds().center - position;
            direction.y = 0;
            return direction.sqrMagnitude > .001f ? Quaternion.LookRotation(direction).eulerAngles.y : 0;
        }

        private Vector3 Clamp(Vector3 value)
        {
            float inset = character.radius + character.skinWidth;
            return new Vector3(Mathf.Clamp(value.x, ExplorationBounds.min.x + inset, ExplorationBounds.max.x - inset),
                floorY, Mathf.Clamp(value.z, ExplorationBounds.min.z + inset, ExplorationBounds.max.z - inset));
        }

        private Vector3 AvoidBed(Vector3 value)
        {
            var bounds = BedBounds();
            bounds.Expand(new Vector3(character.radius * 2 + .08f, 0, character.radius * 2 + .08f));
            if (value.x <= bounds.min.x || value.x >= bounds.max.x || value.z <= bounds.min.z || value.z >= bounds.max.z) return value;
            var candidates = new[] { new Vector3(bounds.min.x - .01f, floorY, value.z), new Vector3(bounds.max.x + .01f, floorY, value.z),
                new Vector3(value.x, floorY, bounds.min.z - .01f), new Vector3(value.x, floorY, bounds.max.z + .01f) };
            var available = candidates.Select(Clamp).Where(candidate => candidate.x <= bounds.min.x || candidate.x >= bounds.max.x
                || candidate.z <= bounds.min.z || candidate.z >= bounds.max.z).OrderBy(candidate => (candidate - value).sqrMagnitude).ToArray();
            if (available.Length == 0) throw new InvalidOperationException("原房间没有可安全放置玩家的床边空间。");
            return available[0];
        }

        public void StopAndRestore()
        {
            if (restored) return;
            restored = true;
            Ready = false;
            StopAllCoroutines();
            RestoreRoomState();
            gameObject.SetActive(false);
            if (Application.isPlaying) Destroy(gameObject); else DestroyImmediate(gameObject);
        }

        private void RestoreRoomState()
        {
            SetBedFocus(false);
            if (Ending != null) { Ending.enabled = false; Ending.Audio?.StopAll(); }
            Cubemap?.Dispose();
            RoomSurface?.Dispose();
            Portal?.Dispose();
            if (character != null) character.enabled = false;
            foreach (var value in moved.OrderBy(item => item.Sibling)) value.Restore();
            foreach (var value in layers) if (value.Key != null) value.Key.layer = value.Value;
            foreach (var value in renderers) if (value.Value != null) value.Value.enabled = value.Enabled;
            foreach (var value in colliders) if (value.Value != null) value.Value.enabled = value.Enabled;
            foreach (var value in lights) if (value.Value != null) value.Value.enabled = value.Enabled;
            foreach (var value in cameras) if (value.Value != null) value.Value.enabled = value.Enabled;
            lighting?.Restore();
            if (cameraSaved && player != null && ViewCamera != null)
            {
                player.transform.SetPositionAndRotation(savedActorPosition, savedActorRotation);
                ViewCamera.transform.SetPositionAndRotation(savedCameraPosition, savedCameraRotation);
                ViewCamera.fieldOfView = savedFieldOfView;
                ViewCamera.nearClipPlane = savedNear;
                ViewCamera.farClipPlane = savedFar;
                ViewCamera.cullingMask = savedCameraMask;
                ViewCamera.clearFlags = savedCameraFlags;
            }
            RestoreRetro();
            if (roomDisabled)
            {
                // Audio directors can start loops in OnEnable; their sources must be usable first.
                foreach (var value in audio) value.EnableForRestore();
                foreach (var value in behaviours) if (value.Value != null) value.Value.enabled = value.Enabled;
                foreach (var value in canvases) if (value.Value != null) value.Value.enabled = value.Enabled;
                foreach (var value in audio) value.Restore();
            }
            if (character != null) character.enabled = characterWasEnabled;
            Cursor.lockState = savedCursorLock;
            Cursor.visible = savedCursorVisible;
            Physics.SyncTransforms();
        }

        private void CreateFade()
        {
            var overlay = new GameObject("Dream transition", typeof(Canvas));
            overlay.transform.SetParent(transform, false);
            var canvas = overlay.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 900;
            var panel = new GameObject("Black", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(overlay.transform, false);
            var rect = (RectTransform)panel.transform;
            rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            fade = panel.GetComponent<Image>();
            fade.color = new Color(0, 0, 0, TransitionPhase == RoomEndingTransitionPhase.FadingOut ? transition.Opacity : 1);
            fade.raycastTarget = true;
        }

        public void CleanupForDestruction()
        {
            if (restored) return;
            restored = true;
            Ready = false;
            StopAllCoroutines();
            SetBedFocus(false);
            if (Ending != null) { Ending.enabled = false; Ending.Audio?.StopAll(); }
            Cubemap?.Dispose();
            RoomSurface?.Dispose();
            Portal?.Dispose();
            lighting?.Restore();
            RestoreRetro();
        }

        private void RestoreRetro()
        {
            if (retro == null) return;
            retro.PresentationProfile = savedRetroProfile;
            retro.PresentationBypass = savedBypass;
            retro.enabled = savedRetroEnabled;
            retro.PreviewOriginal = savedRetroOriginal;
        }

        private void OnDestroy() { CleanupForDestruction(); if (failureFont != null) Destroy(failureFont); }
        private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
        private static bool Finite(Vector3 value) => Finite(value.x) && Finite(value.y) && Finite(value.z);
        private static bool ValidRotation(Quaternion value) => Finite(value.x) && Finite(value.y) && Finite(value.z)
            && Finite(value.w) && value.x * value.x + value.y * value.y + value.z * value.z + value.w * value.w > .0001f;

        private readonly struct EnabledState<T> where T : UnityEngine.Object
        {
            public readonly T Value;
            public readonly bool Enabled;
            public EnabledState(T value, bool enabled) { Value = value; Enabled = enabled; }
        }

        private sealed class TransformState
        {
            private readonly Transform value, parent;
            private readonly Vector3 position, scale;
            private readonly Quaternion rotation;
            public readonly int Sibling;
            public TransformState(Transform value)
            {
                this.value = value; parent = value.parent; position = value.localPosition;
                rotation = value.localRotation; scale = value.localScale; Sibling = value.GetSiblingIndex();
            }
            public void Restore()
            {
                if (value == null) return;
                value.SetParent(parent, false); value.SetSiblingIndex(Sibling);
                value.localPosition = position; value.localRotation = rotation; value.localScale = scale;
            }
        }

        private sealed class AudioState
        {
            private readonly AudioSource source;
            private readonly bool enabled, playing;
            private readonly int sample;
            private readonly AudioClip clip;
            public AudioState(AudioSource source)
            {
                this.source = source; enabled = source.enabled; playing = source.isPlaying;
                clip = source.clip; sample = clip != null ? source.timeSamples : 0;
            }
            public void Silence() { if (source != null) source.enabled = false; }
            public void EnableForRestore() { if (source != null) source.enabled = enabled; }
            public void Restore()
            {
                if (source == null) return;
                source.enabled = enabled;
                if (enabled && playing && clip != null && source.gameObject.activeInHierarchy)
                {
                    source.clip = clip; source.timeSamples = Mathf.Clamp(sample, 0, Mathf.Max(0, clip.samples - 1));
                    source.Play();
                }
            }
        }

        private sealed class LightingState
        {
            private readonly AmbientMode mode = RenderSettings.ambientMode;
            private readonly Color sky = RenderSettings.ambientSkyColor, equator = RenderSettings.ambientEquatorColor,
                ground = RenderSettings.ambientGroundColor;
            private readonly float intensity = RenderSettings.ambientIntensity;
            private readonly bool fog = RenderSettings.fog;
            private readonly SphericalHarmonicsL2 probe = RenderSettings.ambientProbe;
            public void Apply(DreamCubemapSource source)
            {
                RenderSettings.ambientMode = AmbientMode.Trilight;
                RenderSettings.ambientSkyColor = source.AmbientSkyColor;
                RenderSettings.ambientEquatorColor = source.AmbientEquatorColor;
                RenderSettings.ambientGroundColor = source.AmbientGroundColor;
                RenderSettings.ambientIntensity = source.AmbientIntensity;
                RenderSettings.fog = false;
            }
            public void Restore()
            {
                RenderSettings.ambientMode = mode; RenderSettings.ambientSkyColor = sky;
                RenderSettings.ambientEquatorColor = equator; RenderSettings.ambientGroundColor = ground;
                RenderSettings.ambientIntensity = intensity; RenderSettings.ambientProbe = probe; RenderSettings.fog = fog;
            }
        }
    }
}
