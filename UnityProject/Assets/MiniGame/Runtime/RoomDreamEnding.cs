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
        private const float FadeSeconds = .35f;
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
        private LightingState lighting;
        private Image fade;

        public bool Ready { get; private set; }
        public bool IsPreview { get; private set; }
        public string FailureReason { get; private set; }
        public DreamEndingController Ending { get; private set; }
        public DreamCubemapRenderer Cubemap { get; private set; }
        public DreamCubemapRoomSurface RoomSurface { get; private set; }
        public Bounds MappedRoomBounds { get; private set; }
        public Bounds ExplorationBounds { get; private set; }
        public Vector3 PlayerPosition => player != null ? player.transform.position : Vector3.zero;
        public Vector3 PanoramaOrigin => standing != null ? standing.position : Vector3.zero;
        public Camera ViewCamera { get; private set; }

        public static RoomDreamEnding Begin(MiniGameBootstrap room, MiniGameSessionHost session, bool isPreview = false)
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
            owner.session = session;
            owner.player = room.Player;
            owner.ViewCamera = room.Player.ViewCamera;
            owner.savedCursorLock = Cursor.lockState;
            owner.savedCursorVisible = Cursor.visible;
            owner.CreateFade();
            owner.StartCoroutine(owner.Enter());
            return owner;
        }

        private IEnumerator Enter()
        {
            yield return Fade(0, 1);
            try
            {
                PrepareRoom();
                Cubemap = session.DreamProfile.RoomPanoramaSkybox != null
                    ? new DreamCubemapRenderer(session.DreamProfile, ViewCamera, session.DreamProfile.RoomPanoramaSkybox, PanoramaOrigin)
                    : new DreamCubemapRenderer(session.DreamProfile, ViewCamera, false);
                RoomSurface = new DreamCubemapRoomSurface(MappedRoomBounds, transform, BedLayer, PanoramaOrigin);
                ViewCamera.clearFlags = CameraClearFlags.SolidColor;
                Ending = gameObject.AddComponent<DreamEndingController>();
                Ending.ConfigureRoom(session.DreamProfile, ViewCamera, bed, pillow, standing, this);
                if (!Cubemap.IsReady)
                    throw new InvalidOperationException("海滩全景尚未完成首幅六面捕获。");
            }
            catch (Exception exception)
            {
                FailureReason = "隐藏梦结局未能开始：" + exception.Message;
                Debug.LogException(exception, this);
                StopAndRestore();
                if (room != null && room.Screen != null) room.Screen.Notify(FailureReason, true);
                yield break;
            }
            yield return Fade(1, 0);
            if (restored) yield break;
            Ready = true;
            if (fade != null) fade.gameObject.SetActive(false);
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
            if (retro != null) { savedBypass = retro.PresentationBypass; retro.PresentationBypass = true; }

            foreach (var roomAudio in RoomComponents<AudioSource>()) audio.Add(new AudioState(roomAudio));
            // OnDisable restores any ordinary dream, bedside isolation and focus effects first.
            foreach (var behaviour in RoomComponents<MonoBehaviour>())
            {
                if (behaviour == null || behaviour is EventSystem || behaviour is BaseInputModule
                    || behaviour is MiniGameSessionHost || behaviour is RetroScreenEffect) continue;
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
            pillow = new GameObject("Room Pillow Eye Position").transform;
            pillow.SetParent(bed, false);
            var pillowBounds = bedRenderers[2].bounds;
            pillow.position = new Vector3(pillowBounds.center.x, pillowBounds.max.y + .12f, pillowBounds.center.z);
            pillow.rotation = Quaternion.LookRotation(Vector3.up, bed.forward);
            var bedLight = new GameObject("Dream Bed Light", typeof(Light));
            bedLight.transform.SetParent(transform, false);
            bedLight.transform.rotation = Quaternion.Euler(52, -32, 0);
            var light = bedLight.GetComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(1, .97f, .9f);
            light.intensity = .9f;
            light.shadows = LightShadows.None;
            light.cullingMask = 1 << BedLayer;
            ViewCamera.cullingMask = 1 << BedLayer;
            ViewCamera.nearClipPlane = .03f;
            ViewCamera.farClipPlane = 1200;
            ViewCamera.fieldOfView = 62;
            var source = session.DreamProfile.CubemapSourcePrefab != null
                ? session.DreamProfile.CubemapSourcePrefab.GetComponent<DreamCubemapSource>() : null;
            if (source != null) lighting.Apply(source);

            safeSpawn = AvoidBed(Clamp(doorwaySpawn.position));
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
            if (Cubemap != null && Cubemap.IsReady)
            {
                RoomSurface?.Bind(Cubemap.Texture, session.DreamProfile.CubemapYaw);
                RoomSurface?.SetOutline(session.DreamProfile.ShowRoomOutline);
            }
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
            if (Ending != null) { Ending.enabled = false; Ending.Audio?.StopAll(); }
            Cubemap?.Dispose();
            RoomSurface?.Dispose();
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
                if (retro != null) retro.PresentationBypass = savedBypass;
            }
            if (roomDisabled)
            {
                foreach (var value in behaviours) if (value.Value != null) value.Value.enabled = value.Enabled;
                foreach (var value in canvases) if (value.Value != null) value.Value.enabled = value.Enabled;
                foreach (var value in audio) value.Restore();
            }
            if (character != null) character.enabled = characterWasEnabled;
            Cursor.lockState = savedCursorLock;
            Cursor.visible = savedCursorVisible;
            Physics.SyncTransforms();
            gameObject.SetActive(false);
            if (Application.isPlaying) Destroy(gameObject); else DestroyImmediate(gameObject);
        }

        private void CreateFade()
        {
            var overlay = new GameObject("Dream transition", typeof(Canvas));
            overlay.transform.SetParent(transform, false);
            var canvas = overlay.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = short.MaxValue;
            var panel = new GameObject("Black", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(overlay.transform, false);
            var rect = (RectTransform)panel.transform;
            rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            fade = panel.GetComponent<Image>();
            fade.color = Color.clear;
            fade.raycastTarget = false;
        }

        private IEnumerator Fade(float from, float to)
        {
            float elapsed = 0;
            while (elapsed < FadeSeconds)
            {
                if (fade == null) yield break;
                fade.color = new Color(0, 0, 0, Mathf.Lerp(from, to, elapsed / FadeSeconds));
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
            if (fade != null) fade.color = new Color(0, 0, 0, to);
        }

        public void CleanupForDestruction()
        {
            if (restored) return;
            restored = true;
            Ready = false;
            StopAllCoroutines();
            if (Ending != null) { Ending.enabled = false; Ending.Audio?.StopAll(); }
            Cubemap?.Dispose();
            RoomSurface?.Dispose();
            lighting?.Restore();
        }

        private void OnDestroy() => CleanupForDestruction();
        private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
        private static bool Finite(Vector3 value) => Finite(value.x) && Finite(value.y) && Finite(value.z);

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
