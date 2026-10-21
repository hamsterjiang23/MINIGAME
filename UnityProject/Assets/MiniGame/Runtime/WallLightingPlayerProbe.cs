#if UNITY_EDITOR || DEVELOPMENT_BUILD || MINIGAME_FEEDBACK_PROBE
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace MiniGame.UnityHost
{
    /// <summary>Opt-in observer. The accompanying terminal smoke owns every gameplay command and process exit.</summary>
    [DefaultExecutionOrder(1000)]
    public sealed class WallLightingPlayerProbe : MonoBehaviour
    {
        [Serializable] private sealed class Report
        {
            public string result = "RUNNING", detail, unity, graphics, dayPhase, nightPhase;
            public string captureMode = "Actual player ViewCamera.Render with its live RetroScreenEffect; temporary room-centre poses, restored in finally. World image only, no UI or manual-play acceptance claim.";
            public bool passed, developmentBuild, dayObserved, nightObserved, profileMatchesResource, profileValuesPassed;
            public bool wallpaperPassed, dayLightsPassed, nightLightsPassed, cameraPoseRestored = true, cameraTargetRestored = true;
            public bool playerPoseUnchanged = true, runtimeUnchangedByCaptures = true, bedSpotlightEnabled;
            public int wallpaperCount, targetFrameRate;
            public float seconds, isolation, bedSpotlightIntensity;
            public Vector3 captureEye;
            public ProfileRecord profile;
            public LightRecord[] supplementaryLights;
            public WallpaperRecord[] wallpapers;
            public List<CaptureRecord> captures = new List<CaptureRecord>();
        }
        [Serializable] private sealed class ProfileRecord
        {
            public string name;
            public bool effectEnabled, fullscreenQuantization, colorGradingEnabled, previewOriginal, presentationBypass;
            public int levels;
            public float strength, gammaBefore, gammaAfter, postExposure, contrast, saturation, grain;
        }
        [Serializable] private sealed class LightRecord
        {
            public string name, path, type, bakeType, renderMode;
            public bool dayEnabled, dayActive, nightEnabled, nightActive, nightObserved, isBaked;
            public float dayIntensity, nightIntensity;
        }
        [Serializable] private sealed class WallpaperRecord
        {
            public string renderer, mesh, shadowCasting;
            public int vertices, subMeshes, normals = -1, uv = -1;
            public bool generatedMeshName, receiveShadows, cpuReadable, normalsInspected, normalsFinite;
        }
        [Serializable] private sealed class CaptureRecord
        {
            public string file, phase;
            public Vector3 eye, forward;
            public float verticalFieldOfView, aspect, meanLuminance;
            public int sampledColors, sampledNonBlack, sampledPixels;
            public bool retroEnabled;
        }

        private readonly Report report = new Report();
        private MiniGameBootstrap host;
        private BedtimeGuidance guidance;
        private Camera camera;
        private RetroScreenEffect retro;
        private Light[] supplementary;
        private Vector3 rearDirection;
        private string folder;
        private float began;
        private bool finished;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            if (Environment.GetCommandLineArgs().Contains("--wall-lighting-smoke"))
                new GameObject("Wall lighting Player observer").AddComponent<WallLightingPlayerProbe>();
        }

        private void Start()
        {
            began = Time.realtimeSinceStartup;
            string root = Environment.GetEnvironmentVariable("MINIGAME_REPOSITORY_ROOT");
            folder = Path.Combine(string.IsNullOrEmpty(root) ? Application.persistentDataPath : root, "Artifacts", "WallLighting", "Player");
            Directory.CreateDirectory(folder);
            try
            {
                host = FindObjectOfType<MiniGameBootstrap>();
                Require(host != null && host.Player != null && host.Runtime != null, "A running MainRoomCombined host is required.");
                camera = host.Player.ViewCamera;
                guidance = host.GetComponent<BedtimeGuidance>();
                Require(camera != null && guidance != null && guidance.Spotlight != null, "Player camera or bedtime lighting is missing.");
                retro = camera.GetComponent<RetroScreenEffect>();
                Require(retro != null && retro.Profile != null, "The actual player camera has no retro profile.");
                var scene = host.gameObject.scene;
                var renderers = scene.GetRootGameObjects().SelectMany(go => go.GetComponentsInChildren<MeshRenderer>(true)).ToArray();
                var floor = renderers.SingleOrDefault(item => item.name == "B_Floor");
                Require(floor != null, "The unique B_Floor renderer is missing.");
                report.captureEye = new Vector3(floor.bounds.center.x, floor.bounds.max.y + 1.65f, floor.bounds.center.z);
                var bed = renderers.SingleOrDefault(item => item.name == "B_SM_mattress_1");
                var desk = renderers.FirstOrDefault(item => item.name.StartsWith("CRT_Monitor_Tripo", StringComparison.Ordinal));
                Require(bed != null && desk != null, "The desk/bed capture targets are missing.");
                rearDirection = Vector3.ProjectOnPlane((bed.bounds.center + desk.bounds.center) * .5f - report.captureEye, Vector3.up).normalized;
                Require(rearDirection.sqrMagnitude > .5f, "The desk/bed capture direction is degenerate.");
                var lights = scene.GetRootGameObjects().SelectMany(go => go.GetComponentsInChildren<Light>(true)).ToArray();
                supplementary = Enumerable.Range(1, 5).Select(index => lights.SingleOrDefault(light => light.name == "MonitorLight (" + index + ")")).ToArray();
                Require(supplementary.All(light => light != null), "All five numbered MonitorLight supplements must exist.");
                report.supplementaryLights = supplementary.Select(light => new LightRecord { name = light.name,
                    path = PathOf(light.transform), type = light.type.ToString(), bakeType = light.bakingOutput.lightmapBakeType.ToString(), renderMode = light.renderMode.ToString(),
                    isBaked = light.bakingOutput.isBaked }).ToArray();
                report.wallpapers = renderers.Where(IsWallpaper).Select(DescribeWallpaper).ToArray();
                report.wallpaperCount = report.wallpapers.Length;
                report.wallpaperPassed = report.wallpaperCount == 12 && report.wallpapers.All(item => item.generatedMeshName
                    && item.shadowCasting == ShadowCastingMode.Off.ToString() && item.receiveShadows);
                Require(report.wallpaperPassed, "Twelve generated wallpaper meshes must have casting Off and receiveShadows True.");
            }
            catch (Exception exception) { Finish(false, exception.ToString()); }
        }

        private void LateUpdate()
        {
            if (finished) return;
            try
            {
                Require(Time.realtimeSinceStartup - began < 90, "Wall lighting observation timed out before the daily night state.");
                var view = host.Runtime.GetView();
                if (!report.dayObserved && !view.Paused && guidance.Isolation < .001f)
                {
                    ObserveDay();
                    CapturePair("day");
                    report.dayPhase = view.Game.Phase.ToString(); report.dayObserved = true;
                }
                if (!report.dayObserved || view.Paused || view.TerminalOpen || guidance.Isolation < .9999f
                    || !guidance.Spotlight.enabled || !guidance.Spotlight.gameObject.activeInHierarchy) return;
                ObserveNight();
                CapturePair("night");
                report.nightPhase = view.Game.Phase.ToString(); report.nightObserved = true;
                Finish(true, "Live Player profile, twelve generated wallpaper meshes, five realtime day supplements and their bedtime shutdown observed; paired real-camera room captures saved.");
            }
            catch (Exception exception) { Finish(false, exception.ToString()); }
        }

        private void ObserveDay()
        {
            var profile = retro.Profile;
            report.profileMatchesResource = profile == Resources.Load<RetroScreenProfile>("RoomLightingReferenceRetro");
            report.profile = new ProfileRecord { name = profile.name, effectEnabled = profile.EffectEnabled,
                fullscreenQuantization = profile.FullscreenQuantization, colorGradingEnabled = profile.ColorGradingEnabled,
                previewOriginal = retro.PreviewOriginal, presentationBypass = retro.PresentationBypass, levels = profile.Levels,
                strength = profile.Strength, gammaBefore = profile.GammaBefore, gammaAfter = profile.GammaAfter,
                postExposure = profile.PostExposure, contrast = profile.Contrast, saturation = profile.Saturation, grain = profile.FilmGrainStrength };
            report.profileValuesPassed = report.profileMatchesResource && profile.EffectEnabled && profile.FullscreenQuantization
                && profile.ColorGradingEnabled && profile.Levels == 16 && Near(profile.GammaBefore, .906f) && Near(profile.GammaAfter, 1.023f)
                && Near(profile.PostExposure, .28f) && Near(profile.Contrast, -4.3f) && Near(profile.Saturation, 6.8f) && Near(profile.FilmGrainStrength, .05f);
            Require(report.profileValuesPassed, "The live RoomLightingReferenceRetro profile does not match the requested values.");
            for (int index = 0; index < supplementary.Length; index++)
            {
                var light = supplementary[index]; var item = report.supplementaryLights[index];
                item.dayEnabled = light.enabled; item.dayActive = light.gameObject.activeInHierarchy; item.dayIntensity = light.intensity;
            }
            report.dayLightsPassed = supplementary.All(light => light.bakingOutput.lightmapBakeType == LightmapBakeType.Realtime && !light.bakingOutput.isBaked
                && light.renderMode == LightRenderMode.ForcePixel && light.enabled && light.gameObject.activeInHierarchy && light.intensity > 0);
            Require(report.dayLightsPassed, "Each numbered MonitorLight must be realtime, per-pixel, active and lit in daytime.");
        }

        private void ObserveNight()
        {
            for (int index = 0; index < supplementary.Length; index++)
            {
                var light = supplementary[index]; var item = report.supplementaryLights[index];
                item.nightEnabled = light.enabled; item.nightActive = light.gameObject.activeInHierarchy;
                item.nightIntensity = light.intensity; item.nightObserved = true;
            }
            report.nightLightsPassed = supplementary.All(light => !light.enabled && Mathf.Abs(light.intensity) <= .001f);
            report.isolation = guidance.Isolation;
            report.bedSpotlightEnabled = guidance.Spotlight.enabled && guidance.Spotlight.gameObject.activeInHierarchy;
            report.bedSpotlightIntensity = guidance.Spotlight.intensity;
            Require(report.nightLightsPassed, "A numbered MonitorLight remains lit after bedtime isolation.");
            Require(report.bedSpotlightEnabled && report.bedSpotlightIntensity > .01f, "The existing bed guidance spotlight must remain lit.");
        }

        private void CapturePair(string phase)
        {
            string checkpoint = host.Runtime.Save();
            Vector3 playerPosition = host.Player.transform.position;
            Quaternion playerRotation = host.Player.transform.rotation;
            CaptureRoom(phase, "positive-z", Vector3.forward);
            CaptureRoom(phase, "desk-bed-rear", rearDirection);
            report.playerPoseUnchanged &= Vector3.SqrMagnitude(playerPosition - host.Player.transform.position) < 1e-12f
                && Quaternion.Angle(playerRotation, host.Player.transform.rotation) < .001f;
            report.runtimeUnchangedByCaptures &= checkpoint == host.Runtime.Save();
            Require(report.playerPoseUnchanged && report.runtimeUnchangedByCaptures, "Capturing must not change the player or gameplay checkpoint.");
        }

        private void CaptureRoom(string phase, string view, Vector3 forward)
        {
            Require(retro.enabled && retro.Profile.EffectEnabled && !retro.PreviewOriginal && !retro.PresentationBypass,
                "The room capture must use the player's enabled, non-bypassed retro effect.");
            var localPosition = camera.transform.localPosition;
            var localRotation = camera.transform.localRotation;
            var previousTarget = camera.targetTexture;
            var active = RenderTexture.active;
            RenderTexture target = null;
            Texture2D pixels = null;
            try
            {
                target = RenderTexture.GetTemporary(1280, 720, 24, RenderTextureFormat.ARGB32);
                pixels = new Texture2D(1280, 720, TextureFormat.RGB24, false);
                camera.transform.SetPositionAndRotation(report.captureEye, Quaternion.LookRotation(forward, Vector3.up));
                camera.targetTexture = target; camera.Render(); RenderTexture.active = target;
                pixels.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0, false); pixels.Apply(false, false);
                var record = new CaptureRecord { file = phase + "-" + view + ".png", phase = phase, eye = camera.transform.position,
                    forward = camera.transform.forward, verticalFieldOfView = camera.fieldOfView, aspect = camera.aspect, retroEnabled = true };
                var colors = new HashSet<int>(); float luminance = 0;
                var values = pixels.GetPixels32();
                for (int index = 0; index < values.Length; index += 37)
                {
                    var color = values[index]; colors.Add((color.r << 16) | (color.g << 8) | color.b);
                    if (Mathf.Max(color.r, color.g, color.b) > 4) record.sampledNonBlack++;
                    luminance += (.2126f * color.r + .7152f * color.g + .0722f * color.b) / 255;
                    record.sampledPixels++;
                }
                record.sampledColors = colors.Count; record.meanLuminance = luminance / Mathf.Max(1, record.sampledPixels);
                File.WriteAllBytes(Path.Combine(folder, record.file), pixels.EncodeToPNG()); report.captures.Add(record);
                if (phase == "day") Require(record.sampledColors > 8 && record.sampledNonBlack > 100, "The daytime native room capture is empty or uniform.");
            }
            finally
            {
                camera.transform.localPosition = localPosition; camera.transform.localRotation = localRotation;
                camera.targetTexture = previousTarget; RenderTexture.active = active;
                report.cameraPoseRestored &= Vector3.SqrMagnitude(camera.transform.localPosition - localPosition) < 1e-12f
                    && Quaternion.Angle(camera.transform.localRotation, localRotation) < .001f;
                report.cameraTargetRestored &= camera.targetTexture == previousTarget;
                if (target != null) RenderTexture.ReleaseTemporary(target);
                if (pixels != null) Destroy(pixels);
            }
            Require(report.cameraPoseRestored && report.cameraTargetRestored, "The actual player camera must be restored after each capture.");
        }

        private static WallpaperRecord DescribeWallpaper(MeshRenderer renderer)
        {
            var mesh = renderer.GetComponent<MeshFilter>()?.sharedMesh;
            var record = new WallpaperRecord { renderer = PathOf(renderer.transform), mesh = mesh == null ? "<null>" : mesh.name,
                shadowCasting = renderer.shadowCastingMode.ToString(), receiveShadows = renderer.receiveShadows };
            if (mesh == null) return record;
            string prefix = new string(renderer.name.Select(character => char.IsLetterOrDigit(character) || character == '_' ? character : '_').ToArray()) + "_";
            record.generatedMeshName = mesh.name == renderer.name + " interior wallpaper"
                || mesh.name.StartsWith(prefix, StringComparison.Ordinal) && long.TryParse(mesh.name.Substring(prefix.Length), out _);
            record.vertices = mesh.vertexCount; record.subMeshes = mesh.subMeshCount; record.cpuReadable = mesh.isReadable;
            if (mesh.isReadable)
            {
                var normals = mesh.normals; record.normals = normals.Length; record.uv = mesh.uv.Length;
                record.normalsInspected = true; record.normalsFinite = normals.All(Finite);
            }
            return record;
        }
        private static bool IsWallpaper(MeshRenderer renderer) => renderer.sharedMaterials.Any(material => material != null
            && (material.name == "MainRoom_30_B" || material.name == "MainRoom_30"));
        private static bool Finite(Vector3 value) => !(float.IsNaN(value.x) || float.IsNaN(value.y) || float.IsNaN(value.z)
            || float.IsInfinity(value.x) || float.IsInfinity(value.y) || float.IsInfinity(value.z));
        private static bool Near(float actual, float expected) => Mathf.Abs(actual - expected) <= .0005f;
        private static string PathOf(Transform transform)
        { string path = transform.name; for (var parent = transform.parent; parent != null; parent = parent.parent) path = parent.name + "/" + path; return path; }
        private static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
        private void Finish(bool passed, string detail)
        {
            if (finished) return;
            finished = true; report.passed = passed; report.result = passed ? "PASSED" : "FAILED"; report.detail = detail;
            report.seconds = Time.realtimeSinceStartup - began; report.unity = Application.unityVersion;
            report.graphics = SystemInfo.graphicsDeviceType.ToString(); report.developmentBuild = Debug.isDebugBuild;
            report.targetFrameRate = Application.targetFrameRate;
            File.WriteAllText(Path.Combine(folder, "verification.json"), JsonUtility.ToJson(report, true));
            if (passed) Debug.Log("MINIGAME_WALL_LIGHTING_PASSED " + detail);
            else Debug.LogError("MINIGAME_WALL_LIGHTING_FAILED " + detail);
        }
        private void OnApplicationQuit() { if (!finished && !string.IsNullOrEmpty(folder)) Finish(false, "Player exited before both daytime and bedtime observations completed."); }
    }
}
#endif
