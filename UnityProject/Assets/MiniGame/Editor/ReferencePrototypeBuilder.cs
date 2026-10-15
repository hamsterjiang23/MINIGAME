using System;
using System.IO;
using System.Linq;
using MiniGame.Core;
using MiniGame.Presentation;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace MiniGame.UnityHost.Editor
{
    /// <summary>Rebuildable original geometry; no recovered game assets are referenced.</summary>
    public static class ReferencePrototypeBuilder
    {
        public const string ScenePath = "Assets/MiniGame/Scenes/InspectionApartment.unity";
        private static readonly Color Concrete = new Color(.31f, .33f, .3f);
        private static readonly Color Metal = new Color(.14f, .2f, .18f);
        private static string RepositoryRoot => Path.GetFullPath(
            Environment.GetEnvironmentVariable("MINIGAME_REPOSITORY_ROOT") ?? Path.Combine(Application.dataPath, "../.."));

        [MenuItem("MiniGame/Open prototype scene")]
        public static void OpenPrototype()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Leave Play Mode before opening the prototype.");
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            ValidateScene(scene);
            Debug.Log("MINIGAME_MAIN_SCENE_OPENED " + scene.path);
        }

        [MenuItem("MiniGame/Create and validate prototype")]
        public static void CreateAndValidate()
        {
            DialoguePackageImporter.EnsureDefaultPackage();
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Leave Play Mode before regenerating the scene.");
            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));
            Directory.CreateDirectory("Assets/MiniGame/Generated/Materials");
            ConfigureProject();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "InspectionApartment";
            var gray = Material("Concrete", Concrete);
            var dark = Material("Metal", Metal);
            var wood = Material("Desk", new Color(.27f, .22f, .17f));
            var bedMaterial = Material("Bedding", new Color(.36f, .39f, .34f));
            var monitorMaterial = Material("MonitorEmission", new Color(.15f, .34f, .23f), true);
            var root = new GameObject("Original inspection apartment");
            Box(root.transform, "Floor", new Vector3(0, -.12f, 0), new Vector3(8, .24f, 7), gray);
            Box(root.transform, "Ceiling", new Vector3(0, 3.05f, 0), new Vector3(8, .2f, 7), gray);
            Box(root.transform, "North wall", new Vector3(0, 1.5f, 3.5f), new Vector3(8, 3, .18f), gray);
            Box(root.transform, "South wall", new Vector3(0, 1.5f, -3.5f), new Vector3(8, 3, .18f), gray);
            Box(root.transform, "West wall", new Vector3(-4, 1.5f, 0), new Vector3(.18f, 3, 7), gray);
            Box(root.transform, "East wall", new Vector3(4, 1.5f, 0), new Vector3(.18f, 3, 7), gray);
            var door = Box(root.transform, "Morning inspection door", new Vector3(0, 1.22f, 3.35f), new Vector3(1.25f, 2.44f, .2f), dark);
            door.AddComponent<WorldInteraction>().Configure(InteractionTarget.Door, "门 / 晨检", door.GetComponent<Renderer>());
            Box(door.transform, "Door notice", new Vector3(0, .28f, -.6f), new Vector3(.68f, .3f, .06f), bedMaterial, true);
            Box(root.transform, "Desk top", new Vector3(2.7f, .88f, 1.9f), new Vector3(1.9f, .12f, .86f), wood);
            Box(root.transform, "Desk support", new Vector3(2.7f, .42f, 1.9f), new Vector3(1.7f, .84f, .72f), dark);
            var computer = new GameObject("Peace instrument terminal"); computer.transform.SetParent(root.transform);
            computer.transform.position = new Vector3(2.7f, 1.38f, 2.02f);
            var computerScreen = Box(computer.transform, "Monitor", Vector3.zero, new Vector3(1.05f, .68f, .14f), dark, true);
            Box(computer.transform, "Screen emission", new Vector3(0, 0, -.085f), new Vector3(.94f, .56f, .03f), monitorMaterial, true);
            Box(computer.transform, "Stand", new Vector3(0, -.34f, 0), new Vector3(.16f, .24f, .15f), dark, true);
            Box(computer.transform, "Keyboard", new Vector3(0, -.42f, -.34f), new Vector3(.84f, .055f, .24f), dark, true);
            computer.AddComponent<WorldInteraction>().Configure(InteractionTarget.Computer, "和平仪 / 会谈", computerScreen.GetComponent<Renderer>());
            var bed = Box(root.transform, "Bed", new Vector3(-2.7f, .36f, -1.6f), new Vector3(1.55f, .72f, 2.6f), bedMaterial);
            bed.AddComponent<WorldInteraction>().Configure(InteractionTarget.Bed, "床 / 休息与次日", bed.GetComponent<Renderer>());
            Box(root.transform, "Pillow", new Vector3(-2.7f, .78f, -.85f), new Vector3(1.2f, .18f, .54f), gray);
            Box(root.transform, "Bedside cabinet", new Vector3(-1.35f, .5f, -.5f), new Vector3(.62f, 1f, .62f), wood);
            Box(root.transform, "Window frame", new Vector3(-3.85f, 1.65f, 1.1f), new Vector3(.06f, 1.2f, 1.9f), dark);
            Box(root.transform, "Window black glass", new Vector3(-3.81f, 1.65f, 1.1f), new Vector3(.02f, 1.02f, 1.68f), MetalMaterial());
            var lightObject = new GameObject("Apartment ceiling light");
            lightObject.transform.position = new Vector3(0, 2.75f, 0);
            var light = lightObject.AddComponent<Light>(); light.type = LightType.Point;
            light.range = 11f; light.intensity = 1.8f; light.color = new Color(.89f, .91f, .74f); light.shadows = LightShadows.Soft;
            var bedLightObject = new GameObject("Night bed guidance");
            bedLightObject.transform.position = new Vector3(-2.4f, 2.5f, -1.6f);
            var bedLight = bedLightObject.AddComponent<Light>(); bedLight.type = LightType.Spot;
            bedLight.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            bedLight.range = 4f; bedLight.spotAngle = 58f; bedLight.intensity = 1.6f;
            bedLight.color = new Color(.6f, .8f, .65f); bedLight.enabled = false;
            RenderSettings.ambientMode = AmbientMode.Flat; RenderSettings.ambientLight = new Color(.17f, .19f, .15f);
            RenderSettings.fog = true; RenderSettings.fogColor = new Color(.09f, .12f, .1f); RenderSettings.fogDensity = .035f;

            var playerObject = new GameObject("Player"); playerObject.transform.position = new Vector3(0, .03f, 0);
            var controller = playerObject.AddComponent<CharacterController>(); controller.height = 1.75f;
            controller.center = new Vector3(0, .875f, 0); controller.radius = .27f; controller.stepOffset = .2f;
            var cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            cameraObject.tag = "MainCamera"; cameraObject.transform.SetParent(playerObject.transform, false);
            cameraObject.transform.localPosition = new Vector3(0, 1.56f, 0);
            var camera = cameraObject.GetComponent<Camera>(); camera.fieldOfView = 65f; camera.nearClipPlane = .04f;
            camera.backgroundColor = new Color(.09f, .12f, .1f);
            var player = playerObject.AddComponent<ApartmentPlayer>(); player.Configure(camera);
            var terminalPose = new GameObject("Terminal camera destination");
            terminalPose.transform.position = new Vector3(2.7f, 1.38f, 1.26f); terminalPose.transform.rotation = Quaternion.identity;

            var host = new GameObject("MiniGame Bootstrap");
            var screen = host.AddComponent<MiniGameScreen>();
            var effects = host.AddComponent<PresentationEffects>(); effects.Configure(camera, terminalPose.transform, light, bedLight);
            var bridge = host.AddComponent<MiniGameYarnBridge>(); bridge.Configure(GeneratePrototypeYarn.Generate());
            var bootstrap = host.AddComponent<MiniGameBootstrap>(); bootstrap.Configure(screen, player, effects, bridge);
            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            AssetDatabase.SaveAssets();
            ValidateScene(scene);
            ValidateFlow();
            WriteReport("unity-scene-validation.json", "{\"status\":\"passed\",\"unity\":\"" + Application.unityVersion + "\",\"scene\":\"" + ScenePath + "\",\"checks\":[\"single_runtime_host\",\"three_interaction_targets\",\"camera_and_controller\",\"three_day_rule_flow\"],\"scope\":\"Editor scene generation and pure runtime simulation; not visual playability\"}");
            Debug.Log("MINIGAME_SCENE_VALIDATION_PASSED " + ScenePath);
        }

        public static void BuildWindows()
        {
            CreateAndValidate();
            string destination = Path.Combine(RepositoryRoot, "Builds/MiniGame/MiniGame.exe");
            Directory.CreateDirectory(Path.GetDirectoryName(destination));
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { ScenePath }, locationPathName = destination,
                target = BuildTarget.StandaloneWindows64, options = BuildOptions.Development
            });
            WriteReport("unity-windows-build.json", "{\"result\":\"" + report.summary.result + "\",\"errors\":" + report.summary.totalErrors + ",\"warnings\":" + report.summary.totalWarnings + ",\"bytes\":" + report.summary.totalSize + ",\"path\":\"Builds/MiniGame/MiniGame.exe\"}");
            if (report.summary.result != BuildResult.Succeeded) throw new InvalidOperationException("Windows build failed: " + report.summary.result);
            Debug.Log("MINIGAME_WINDOWS_BUILD_PASSED " + destination);
        }

        private static void ConfigureProject()
        {
            PlayerSettings.companyName = "MiniGame Prototype";
            PlayerSettings.productName = "检定员 · 开发原型";
            PlayerSettings.defaultScreenWidth = 1600; PlayerSettings.defaultScreenHeight = 900;
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            PlayerSettings.resizableWindow = true;
            PlayerSettings.runInBackground = true;
            PlayerSettings.SetApiCompatibilityLevel(BuildTargetGroup.Standalone, ApiCompatibilityLevel.NET_Standard_2_0);
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, ScriptingImplementation.Mono2x);
            GraphicsSettings.defaultRenderPipeline = null;
            QualitySettings.renderPipeline = null;
            QualitySettings.vSyncCount = 1;
        }

        private static void ValidateScene(Scene scene)
        {
            var roots = scene.GetRootGameObjects();
            if (roots.SelectMany(root => root.GetComponentsInChildren<MiniGameBootstrap>(true)).Count() != 1) throw new InvalidOperationException("Scene must contain exactly one host.");
            var targets = roots.SelectMany(root => root.GetComponentsInChildren<WorldInteraction>(true)).Select(item => item.Target).OrderBy(item => item).ToArray();
            if (!targets.SequenceEqual(new[] { InteractionTarget.Door, InteractionTarget.Computer, InteractionTarget.Bed }.OrderBy(item => item))) throw new InvalidOperationException("Scene must contain exactly one door, terminal and bed.");
            if (roots.SelectMany(root => root.GetComponentsInChildren<Camera>(true)).Count() != 1) throw new InvalidOperationException("Scene needs one camera.");
            if (roots.SelectMany(root => root.GetComponentsInChildren<CharacterController>(true)).Count() != 1) throw new InvalidOperationException("Scene needs one player controller.");
        }

        private static void ValidateFlow()
        {
            var rules = PrototypeRules.Create();
            var runtime = new GameRuntime(rules, new PrototypeContent(rules));
            int safety = 0;
            while (runtime.GetView().Game.Phase != GamePhase.Ending && safety++ < 1000)
            {
                var view = runtime.GetView();
                RuntimeResult result;
                if (view.Cue != null) result = runtime.CompleteCue(view.Cue.Token);
                else if (view.Input == InputContext.World)
                    result = runtime.TryInteract(view.AllowedInteractions.First());
                else if (view.Dialogue != null && view.Dialogue.Responses.Count > 0)
                    result = runtime.SelectResponse(view.Game.CurrentEncounterId, view.Game.CurrentNodeId, view.Dialogue.Responses[0].Id);
                else if (view.Game.AvailableDispositions.Count > 0)
                    result = runtime.SubmitDisposition(view.Game.CurrentEncounterId, view.Game.RemainingPurificationQuota > 0 ? Disposition.Purify : Disposition.Treat);
                else throw new InvalidOperationException("Prototype flow has no usable action at " + view.Game.Phase);
                if (!result.Accepted) throw new InvalidOperationException("Prototype flow rejected: " + result.Reason);
            }
            if (safety >= 1000 || runtime.GetView().Game.Ending != EndingType.TakenAway) throw new InvalidOperationException("Three-day quota-complete flow did not reach TakenAway.");
            var restored = GameRuntime.Restore(runtime.Save(), rules, new PrototypeContent(rules));
            if (restored.GetView().Game.Ending != EndingType.TakenAway) throw new InvalidOperationException("Ending save failed to restore.");
        }

        private static GameObject Box(Transform parent, string name, Vector3 position, Vector3 scale, Material material, bool local = false)
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube); cube.name = name;
            cube.transform.SetParent(parent, false);
            if (local) cube.transform.localPosition = position; else cube.transform.position = position;
            cube.transform.localScale = scale; cube.GetComponent<Renderer>().sharedMaterial = material;
            return cube;
        }
        private static Material MetalMaterial() => Material("Window", new Color(.02f, .04f, .03f));
        private static Material Material(string name, Color color, bool emission = false)
        {
            string path = "Assets/MiniGame/Generated/Materials/" + name + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null) { material = new Material(Shader.Find("Standard")); AssetDatabase.CreateAsset(material, path); }
            material.color = color;
            if (emission) { material.EnableKeyword("_EMISSION"); material.SetColor("_EmissionColor", color * .65f); }
            return material;
        }
        private static void WriteReport(string name, string contents)
        {
            var directory = Path.Combine(RepositoryRoot, "Artifacts/Unity"); Directory.CreateDirectory(directory);
            File.WriteAllText(Path.Combine(directory, name), contents);
        }
    }
}
