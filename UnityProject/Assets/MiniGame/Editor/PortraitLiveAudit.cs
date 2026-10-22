using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace MiniGame.UnityHost.Editor
{
    public static class PortraitLiveAudit
    {
        [MenuItem("MiniGame/人物素材/导出当前监控诊断")]
        public static void Capture()
        {
            string root = Directory.GetParent(Application.dataPath).Parent.FullName;
            string folder = Path.Combine(root, "Artifacts", "PortraitRemap20261022", "live");
            Directory.CreateDirectory(folder);
            var feeds = Resources.FindObjectsOfTypeAll<PatientSurveillanceFeed>();
            var sprites = Resources.FindObjectsOfTypeAll<SpriteRenderer>();
            var report = new
            {
                playing = Application.isPlaying,
                feeds = feeds.Select(feed => new
                {
                    id = feed.GetInstanceID(), scene = feed.gameObject.scene.name, patient = feed.PatientId,
                    sprite = feed.CurrentSprite != null ? feed.CurrentSprite.name : null,
                    enabled = feed.isActiveAndEnabled, showing = feed.IsShowing,
                    stage = Position(Field<GameObject>(feed, "stage")?.transform),
                    camera = Position(feed.ViewCamera != null ? feed.ViewCamera.transform : null),
                    cameraScene = feed.ViewCamera != null ? feed.ViewCamera.scene.name : null,
                    frameCount = feed.RenderedFrames,
                    visibleSprites = feed.ViewCamera == null ? Array.Empty<int>() : sprites.Where(sprite => sprite.enabled
                        && sprite.gameObject.activeInHierarchy && (feed.ViewCamera.cullingMask & 1 << sprite.gameObject.layer) != 0
                        && GeometryUtility.TestPlanesAABB(GeometryUtility.CalculateFrustumPlanes(feed.ViewCamera), sprite.bounds))
                        .Select(sprite => sprite.GetInstanceID()).ToArray()
                }).ToArray(),
                sprites = sprites.Select(sprite => new
                {
                    id = sprite.GetInstanceID(), sprite.name, scene = sprite.gameObject.scene.name,
                    sceneHandle = sprite.gameObject.scene.handle, enabled = sprite.enabled, active = sprite.gameObject.activeInHierarchy,
                    position = Position(sprite.transform), layer = sprite.gameObject.layer,
                    spriteName = sprite.sprite != null ? sprite.sprite.name : null,
                    asset = AssetDatabase.GetAssetPath(sprite.sprite), bounds = sprite.bounds.ToString(),
                    parent = sprite.transform.parent != null ? sprite.transform.parent.name : null
                }).ToArray(),
                images = Resources.FindObjectsOfTypeAll<Image>().Where(value => value.name == "CurrentPortrait" || value.name == "PreviousPortrait")
                    .Select(value => new { value.name, scene = value.gameObject.scene.name, enabled = value.isActiveAndEnabled,
                        sprite = value.sprite != null ? value.sprite.name : null, alpha = value.color.a }).ToArray()
            };
            File.WriteAllText(Path.Combine(folder, "state.json"), JsonConvert.SerializeObject(report, Formatting.Indented));
            foreach (var feed in feeds) Save(feed.Output, Path.Combine(folder, "feed-" + feed.GetInstanceID() + ".png"));
            foreach (var display in Resources.FindObjectsOfTypeAll<TerminalDisplay>())
                Save(display.Texture, Path.Combine(folder, "terminal-" + display.GetInstanceID() + ".png"));
            Debug.Log("PORTRAIT_LIVE_AUDIT " + folder);
        }

        private static T Field<T>(object value, string name) where T : class
            => value.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(value) as T;
        private static string Position(Transform value) => value == null ? null : value.position.ToString("F3");
        private static void Save(RenderTexture target, string path)
        {
            if (target == null || !target.IsCreated()) return;
            var previous = RenderTexture.active;
            var pixels = new Texture2D(target.width, target.height, TextureFormat.RGB24, false);
            try
            {
                RenderTexture.active = target;
                pixels.ReadPixels(new Rect(0, 0, target.width, target.height), 0, 0); pixels.Apply();
                File.WriteAllBytes(path, pixels.EncodeToPNG());
            }
            finally { RenderTexture.active = previous; UnityEngine.Object.DestroyImmediate(pixels); }
        }
    }
}
