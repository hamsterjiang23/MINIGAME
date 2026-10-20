using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace MiniGame.UnityHost.Editor
{
    public static class CrtScreenFitValidation
    {
        [Serializable] private sealed class Report
        {
            public string result, unity;
            public int vertices, triangles;
            public Bounds screenBounds, monitorBounds;
            public float depthMillimetres;
            public string[] captures;
        }
        public static void ValidateAndCapture()
        {
            string root = Environment.GetEnvironmentVariable("MINIGAME_REPOSITORY_ROOT");
            if (string.IsNullOrEmpty(root)) throw new InvalidOperationException("Set MINIGAME_REPOSITORY_ROOT.");
            string folder = Path.Combine(root, "Artifacts", "CrtScreenFit"); Directory.CreateDirectory(folder);
            AssetDatabase.ImportAsset(CrtScreenFitPostprocessor.ModelPath, ImportAssetOptions.ForceUpdate);
            EditorSceneManager.OpenScene(MainRoomGameplayBuilder.ScenePath);
            var renderers = UnityEngine.Object.FindObjectsOfType<MeshRenderer>();
            var screen = renderers.Single(renderer => renderer.name.StartsWith("CRT screen emission", StringComparison.Ordinal));
            var monitor = renderers.Single(renderer => renderer.name.StartsWith("CRT_Monitor_Tripo", StringComparison.Ordinal));
            var mesh = screen.GetComponent<MeshFilter>().sharedMesh;
            if (mesh == null) throw new InvalidOperationException("CRT renderer lost its imported mesh reference.");
            if (mesh.vertexCount < 3000 || screen.bounds.size.z < .02f)
                throw new InvalidOperationException("CRT glass still uses the flat placeholder: " + mesh.vertexCount + " / " + screen.bounds);
            var camera = new GameObject("CRT surface validation camera").AddComponent<Camera>();
            camera.enabled = false;
            camera.nearClipPlane = .01f;
            var markers = screen.transform.root.GetComponentsInChildren<Transform>(true);
            Vector3 origin = markers.Single(node => node.name == "Unity_ReferenceOrigin").position;
            Vector3 right = (markers.Single(node => node.name == "Unity_ReferenceRight").position - origin).normalized;
            Vector3 front = -(markers.Single(node => node.name == "Unity_ReferenceDepth").position - origin).normalized;
            Vector3 center = screen.bounds.center;
            string[] names = { "front.png", "left.png", "right.png" };
            Vector3[] positions = { center + front * .7f + Vector3.up * .03f,
                center + front * .58f + right * .5f + Vector3.up * .18f,
                center + front * .58f - right * .5f + Vector3.up * .18f };
            camera.fieldOfView = 45;
            for (int i = 0; i < names.Length; i++)
            {
                camera.transform.position = positions[i]; camera.transform.LookAt(center);
                var previous = camera.targetTexture; var active = RenderTexture.active;
                var target = RenderTexture.GetTemporary(1280, 960, 24, RenderTextureFormat.ARGB32);
                var pixels = new Texture2D(1280, 960, TextureFormat.RGBA32, false);
                try
                {
                    camera.targetTexture = target; camera.Render(); RenderTexture.active = target;
                    pixels.ReadPixels(new Rect(0, 0, 1280, 960), 0, 0); pixels.Apply();
                    File.WriteAllBytes(Path.Combine(folder, names[i]), pixels.EncodeToPNG());
                }
                finally { camera.targetTexture = previous; RenderTexture.active = active; RenderTexture.ReleaseTemporary(target); UnityEngine.Object.DestroyImmediate(pixels); }
            }
            var report = new Report { result = "passed", unity = Application.unityVersion, vertices = mesh.vertexCount,
                triangles = (int)mesh.GetIndexCount(0) / 3, screenBounds = screen.bounds, monitorBounds = monitor.bounds,
                depthMillimetres = screen.bounds.size.z * 1000, captures = names };
            File.WriteAllText(Path.Combine(folder, "unity-geometry.json"), JsonUtility.ToJson(report, true));
            Debug.Log("MINIGAME_CRT_FIT_VALIDATED " + report.vertices + " vertices / " + report.depthMillimetres + "mm curvature");
        }
    }
}
