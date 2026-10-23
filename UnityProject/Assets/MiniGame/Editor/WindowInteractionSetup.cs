using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MiniGame.UnityHost.Editor
{
    public static class WindowInteractionSetup
    {
        public const string RootName = "Window observation";
        private const string Generated = "Assets/MiniGame/Generated/WindowObservation";

        [MenuItem("MiniGame/Main Room/Install window observation")]
        public static void InstallInOpenScene()
        {
            var scene = SceneManager.GetActiveScene();
            if (EditorApplication.isPlayingOrWillChangePlaymode || scene.path != MainRoomGameplayBuilder.ScenePath)
                throw new InvalidOperationException("Open MainRoomCombined in Edit Mode before installing window interaction.");
            ConfigureOpenScene();
            AssetDatabase.SaveAssets();
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene)) throw new IOException("Could not save window interaction.");
        }

        public static void ConfigureOpenScene()
        {
            var scene = SceneManager.GetActiveScene();
            var room = scene.GetRootGameObjects().Single(value => value.name == "MainRoomCombined");
            var glass = room.GetComponentsInChildren<MeshRenderer>(true).Single(value => value.name == "B_立方体.012");
            var document = Resources.Load<RoomReadableDocument>("MiniGame/Readables/WindowObservation");
            var shader = Shader.Find("MiniGame/Room Window Glass");
            if (document == null || shader == null) throw new InvalidOperationException("Missing window text or transparent shader.");
            Directory.CreateDirectory(Generated);
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            var material = AssetDatabase.LoadAssetAtPath<Material>(Generated + "/InvisibleWindow.mat");
            if (material == null)
            {
                material = new Material(shader) { name = "Invisible window interaction surface" };
                material.SetColor("_Color", Color.clear);
                AssetDatabase.CreateAsset(material, Generated + "/InvisibleWindow.mat");
            }
            var previous = scene.GetRootGameObjects().SingleOrDefault(value => value.name == RootName);
            var incoming = GameObject.CreatePrimitive(PrimitiveType.Quad);
            incoming.name = RootName + " (installing)";
            SceneManager.MoveGameObjectToScene(incoming, scene);
            try
            {
                // An XY quad supports the shared readable outline; the original glass is a thin X-axis cube.
                var bounds = glass.bounds;
                incoming.transform.SetPositionAndRotation(new Vector3(bounds.min.x - .025f, bounds.center.y, bounds.center.z),
                    Quaternion.LookRotation(Vector3.right, Vector3.up));
                incoming.transform.localScale = new Vector3(bounds.size.z, bounds.size.y, 1);
                var surface = incoming.GetComponent<MeshRenderer>();
                surface.sharedMaterial = material;
                surface.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                surface.receiveShadows = false;
                var prop = incoming.AddComponent<ReadableProp>();
                prop.Configure("window-observation", "", surface, 3.5f);
                prop.ConfigureDocument(document);
                if (previous != null) UnityEngine.Object.DestroyImmediate(previous);
                incoming.name = RootName;
                Physics.SyncTransforms();
                Debug.Log("WINDOW_OBSERVATION_READY " + document.GetText(0));
            }
            catch
            {
                UnityEngine.Object.DestroyImmediate(incoming);
                throw;
            }
        }
    }
}
