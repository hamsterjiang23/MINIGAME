using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace MiniGame.UnityHost.Editor
{
    /// <summary>Installs the supplied distant buildings without rebuilding the room or gameplay.</summary>
    public static class WindowExteriorSetup
    {
        private const string Art = "Assets/MiniGame/Art/WindowExterior/";
        private const string RootName = "Window exterior - Employee Apartment 1.0";
        private const string Evidence = "../Artifacts/WindowExterior20261023/";
        private static readonly string[] OldBackdropNames =
        {
            "B_Wall__3_.003", "B_Wall__3_.012", "B_Wall__3_.013",
            "B_立方体.003", "B_立方体.016", "B_立方体.017", "B_立方体.019",
            "B_立方体.020", "B_立方体.021", "B_立方体.022", "B_立方体.024",
            "B_立方体.025", "B_立方体.026", "B_立方体.027", "B_立方体.028"
        };

        [Serializable] private sealed class Report
        {
            public string scene, root;
            public string[] hiddenBackdrop;
            public int exteriorRenderers, missingMaterials;
            public long triangles;
            public float cameraFarBefore, cameraFarAfter;
            public Vector3 exteriorMin, exteriorMax;
            public bool protectedComponentsUnchanged;
        }

        [MenuItem("MiniGame/Room Art/Replace window exterior with Employee Apartment 1.0")]
        public static void ApplyInOpenScene()
        {
            var scene = SceneManager.GetActiveScene();
            if (EditorApplication.isPlayingOrWillChangePlaymode || scene.path != MainRoomGameplayBuilder.ScenePath)
                throw new InvalidOperationException("Open MainRoomCombined in Edit Mode before replacing its window exterior.");
            var room = scene.GetRootGameObjects().Single(value => value.name == "MainRoomCombined");
            var camera = scene.GetRootGameObjects().SelectMany(value => value.GetComponentsInChildren<Camera>(true))
                .Single(value => value.CompareTag("MainCamera"));
            var oldBackdrop = OldBackdropNames.Select(name => room.GetComponentsInChildren<MeshRenderer>(true)
                .Single(value => value.name == name)).ToArray();
            if (oldBackdrop.Any(value => value.bounds.min.x < 2.4f))
                throw new InvalidOperationException("An expected backdrop object overlaps the protected room.");
            Directory.CreateDirectory(Evidence);
            if (!File.Exists(Evidence + "before-window.png")) CaptureViews("before");

            AssetDatabase.ImportAsset(Art + "MainRoom.fbx", ImportAssetOptions.ForceSynchronousImport);
            var importer = (ModelImporter)AssetImporter.GetAtPath(Art + "MainRoom.fbx");
            importer.importAnimation = false;
            importer.importBlendShapes = false;
            importer.importCameras = false;
            importer.importLights = false;
            importer.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
            importer.SaveAndReimport();
            var materials = MainRoomPreviewBuilder.ImportMaterialAssets(Art);
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(Art + "MainRoom.fbx");
            if (model == null || model.GetComponentsInChildren<MeshRenderer>(true).Length != 8)
                throw new InvalidOperationException("Expected exactly seven exterior buildings and one ground mesh.");

            var previous = scene.GetRootGameObjects().SingleOrDefault(value => value.name == RootName);
            var protectedObjects = scene.GetRootGameObjects().Where(value => value != previous)
                .SelectMany(value => value.GetComponentsInChildren<Component>(true))
                .Where(value => value != null && value != camera && !oldBackdrop.Contains(value as MeshRenderer))
                .ToDictionary(value => value, value => EditorJsonUtility.ToJson(value));
            var report = new Report { scene = scene.path, root = RootName, hiddenBackdrop = OldBackdropNames,
                cameraFarBefore = camera.farClipPlane };
            Undo.IncrementCurrentGroup();
            int group = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Replace window exterior only");
            GameObject incoming = null;
            try
            {
                incoming = (GameObject)PrefabUtility.InstantiatePrefab(model, scene);
                Undo.RegisterCreatedObjectUndo(incoming, "Add window exterior");
                incoming.name = RootName;
                var renderers = incoming.GetComponentsInChildren<MeshRenderer>(true);
                foreach (var renderer in renderers)
                {
                    renderer.sharedMaterials = renderer.sharedMaterials.Select(material => material != null
                        && materials.TryGetValue(material.name, out var mapped) ? mapped
                        : throw new InvalidOperationException("Unmapped exterior material.")).ToArray();
                    // The exterior receives the existing ambient light but cannot change room shadows.
                    renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    PrefabUtility.RecordPrefabInstancePropertyModifications(renderer);
                    var mesh = renderer.GetComponent<MeshFilter>().sharedMesh;
                    report.triangles += (long)mesh.GetIndexCount(0) / 3;
                    for (int submesh = 1; submesh < mesh.subMeshCount; submesh++)
                        report.triangles += (long)mesh.GetIndexCount(submesh) / 3;
                }
                var bounds = renderers[0].bounds;
                foreach (var renderer in renderers) bounds.Encapsulate(renderer.bounds);
                if (renderers.Where(value => value.bounds.size.y > 1).Any(value => value.bounds.min.x < 20))
                    throw new InvalidOperationException("Imported buildings do not match the source exterior placement.");
                foreach (var renderer in oldBackdrop)
                {
                    Undo.RecordObject(renderer, "Hide old window backdrop");
                    renderer.enabled = false;
                    PrefabUtility.RecordPrefabInstancePropertyModifications(renderer);
                }
                Undo.RecordObject(camera, "Extend distant exterior visibility");
                camera.farClipPlane = Mathf.Max(camera.farClipPlane, 300);
                PrefabUtility.RecordPrefabInstancePropertyModifications(camera);
                report.protectedComponentsUnchanged = protectedObjects.All(pair => pair.Key != null
                    && EditorJsonUtility.ToJson(pair.Key) == pair.Value);
                if (!report.protectedComponentsUnchanged)
                    throw new InvalidOperationException("An existing protected room, corridor, light or gameplay component changed.");
                if (previous != null) Undo.DestroyObjectImmediate(previous);
                report.exteriorRenderers = renderers.Length;
                report.exteriorMin = bounds.min; report.exteriorMax = bounds.max;
                report.cameraFarAfter = camera.farClipPlane;
                report.missingMaterials = renderers.Sum(value => value.sharedMaterials.Count(material => material == null
                    || material.shader == null || !material.shader.isSupported));
                if (report.missingMaterials != 0) throw new InvalidOperationException("Exterior material validation failed.");
                AssetDatabase.SaveAssets();
                File.WriteAllText(Evidence + "unity-import.json", JsonUtility.ToJson(report, true));
                CaptureViews("after");
                EditorSceneManager.MarkSceneDirty(scene);
                if (!EditorSceneManager.SaveScene(scene))
                    throw new IOException("Unity could not save the window exterior scene.");
                Undo.CollapseUndoOperations(group);
                Debug.Log("WINDOW_EXTERIOR_INSTALLED " + JsonUtility.ToJson(report));
            }
            catch
            {
                Undo.RevertAllDownToGroup(group);
                throw;
            }
        }

        public static void CaptureViews(string prefix)
        {
            Directory.CreateDirectory(Evidence);
            var main = Object.FindObjectsOfType<Camera>(true).Single(value => value.CompareTag("MainCamera"));
            var temporary = new GameObject("Temporary window verification camera") { hideFlags = HideFlags.HideAndDontSave };
            var camera = temporary.AddComponent<Camera>();
            camera.CopyFrom(main); camera.enabled = false;
            try
            {
                camera.transform.SetPositionAndRotation(new Vector3(.4f, 1.15f, -1.55f), Quaternion.LookRotation(Vector3.right));
                Capture(camera, Evidence + prefix + "-window.png");
                camera.transform.SetPositionAndRotation(new Vector3(-1.7f, .95f, -1.95f),
                    Quaternion.LookRotation(new Vector3(2.15f, 1.15f, -1.55f) - new Vector3(-1.7f, .95f, -1.95f)));
                Capture(camera, Evidence + prefix + "-room.png");
            }
            finally { Object.DestroyImmediate(temporary); }
        }

        private static void Capture(Camera camera, string path)
        {
            var previous = RenderTexture.active;
            var target = RenderTexture.GetTemporary(1280, 720, 24, RenderTextureFormat.ARGB32);
            Texture2D image = null;
            try
            {
                camera.targetTexture = target; camera.Render(); RenderTexture.active = target;
                image = new Texture2D(target.width, target.height, TextureFormat.RGB24, false);
                image.ReadPixels(new Rect(0, 0, target.width, target.height), 0, 0); image.Apply();
                File.WriteAllBytes(path, image.EncodeToPNG());
            }
            finally
            {
                camera.targetTexture = null; RenderTexture.active = previous;
                RenderTexture.ReleaseTemporary(target);
                if (image != null) Object.DestroyImmediate(image);
            }
        }
    }
}
