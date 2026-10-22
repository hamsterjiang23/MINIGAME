#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using UnityEngine;

namespace MiniGame.UnityHost
{
    // Opt-in verification for a real Windows Player; inactive in normal launches and omitted in release builds.
    public sealed class RoomLightingPlayerProbe : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void StartIfRequested()
        {
            if (Array.IndexOf(Environment.GetCommandLineArgs(), "--room-lighting-probe") < 0) return;
            new GameObject("Room lighting Player verification").AddComponent<RoomLightingPlayerProbe>();
        }

        private IEnumerator Start()
        {
            yield return null;
            yield return null;
            string path = Path.Combine(Application.persistentDataPath, "RoomLightingProbe");
            var args = Environment.GetCommandLineArgs();
            int directory = Array.IndexOf(args, "--room-lighting-probe-dir");
            if (directory >= 0 && directory + 1 < args.Length) path = args[directory + 1];
            Directory.CreateDirectory(path);
            var result = new Result { unity = Application.unityVersion, graphics = SystemInfo.graphicsDeviceType.ToString() };
            try
            {
                var room = FindObjectOfType<RoomLightingController>();
                if (room == null) throw new InvalidOperationException("Room lighting controller missing from Player scene.");
                if (room.MainLight.type != LightType.Spot || Vector3.Dot(room.MainLight.transform.forward, Vector3.down) < .999f)
                    throw new InvalidOperationException("Player lamp is not a downward spotlight.");
                if (room.Rug == null || room.Rug.sharedMaterial == null || !room.Rug.sharedMaterial.shader.isSupported)
                    throw new InvalidOperationException("Rug or supported room shader missing from Player.");
                var host = FindObjectOfType<MiniGameBootstrap>();
                host.Player.enabled = false;
                var camera = host.Player.ViewCamera;
                camera.transform.position = new Vector3(-7.1f, .89f, -2.9f);
                camera.transform.LookAt(new Vector3(-.1f, .35f, -.6f));
                camera.fieldOfView = 58.2f;
                var target = RenderTexture.GetTemporary(1280, 720, 24);
                var previous = RenderTexture.active;
                var oldTarget = camera.targetTexture;
                var image = new Texture2D(1280, 720, TextureFormat.RGB24, false);
                try
                {
                    camera.targetTexture = target; camera.Render(); RenderTexture.active = target;
                    image.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0); image.Apply();
                    File.WriteAllBytes(Path.Combine(path, "player-720.png"), image.EncodeToPNG());
                    var originalPixels = image.GetPixels32();
                    var effect = camera.GetComponent<RetroScreenEffect>();
                    var originalProfile = effect.Profile;
                    bool followedLighting = effect.FollowRoomLighting;
                    var preview = Instantiate(originalProfile);
                    try
                    {
                        preview.ResetToReferenceStyle(); effect.Profile = preview;
                        camera.Render(); RenderTexture.active = target;
                        image.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0); image.Apply();
                        var gradedPixels = image.GetPixels32();
                        double difference = 0;
                        for (int index = 0; index < originalPixels.Length; index++)
                            difference += Math.Abs(originalPixels[index].r - gradedPixels[index].r)
                                + Math.Abs(originalPixels[index].g - gradedPixels[index].g)
                                + Math.Abs(originalPixels[index].b - gradedPixels[index].b);
                        result.gradingDifference = (float)(difference / (originalPixels.Length * 3.0 * 255));
                        result.gradingChangesPixels = result.gradingDifference > .0005f;
                        if (!result.gradingChangesPixels) throw new InvalidOperationException("Player grading did not change rendered pixels.");
                        File.WriteAllBytes(Path.Combine(path, "player-grade-720.png"), image.EncodeToPNG());
                    }
                    finally { effect.Profile = originalProfile; effect.FollowRoomLighting = followedLighting; Destroy(preview); }
                }
                finally
                {
                    camera.targetTexture = oldTarget; RenderTexture.active = previous;
                    RenderTexture.ReleaseTemporary(target); Destroy(image);
                }
                result.passed = true; result.shader = room.Rug.sharedMaterial.shader.name;
                result.lightPosition = room.MainLight.transform.position;
                result.cookie = room.MainLight.cookie != null;
                result.materialBanding = room.Profile.lightingBandsEnabled;
                result.fullscreenBanding = camera.GetComponent<RetroScreenEffect>().Profile.FullscreenQuantization;
            }
            catch (Exception exception) { result.error = exception.ToString(); }
            File.WriteAllText(Path.Combine(path, "player-verification.json"), JsonUtility.ToJson(result, true));
            Debug.Log("MINIGAME_ROOM_LIGHTING_PLAYER " + JsonUtility.ToJson(result));
            Application.Quit(result.passed ? 0 : 1);
        }

        [Serializable]
        private sealed class Result
        {
            public string unity, graphics, shader, error;
            public bool passed, cookie, materialBanding, fullscreenBanding;
            public bool gradingChangesPixels;
            public float gradingDifference;
            public Vector3 lightPosition;
        }
    }
}
#endif
