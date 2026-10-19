using System;
using System.IO;
using System.Linq;
using MiniGame.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Yarn.Unity;

namespace MiniGame.UnityHost.Editor
{
    /// <summary>Connects the imported room to the existing authoritative runtime.</summary>
    public static class MainRoomGameplayBuilder
    {
        public const string ScenePath = MainRoomPreviewBuilder.CombinedScenePath;
        private const string GameplayRootName = "MainRoom Gameplay";

        [Serializable]
        private sealed class SetupReport
        {
            public string scene, unity, scope, frontClosureRepair;
            public int worldInteractions, colliders, exteriorConcreteRenderers;
            public Vector3 roomOrigin, sourceRight, sourceDepth, playerSpawn, screenCenter, screenSize, terminalCamera;
            public float terminalCameraDistance;
            public string[] checks;
        }

        private readonly struct RoomFrame
        {
            public readonly Vector3 Origin, Right, Depth;
            public RoomFrame(GameObject room)
            {
                Depth = (Marker(room, "WindowDetailTarget") - Marker(room, "WindowDetailPosition")) / 4.8f;
                Right = (Marker(room, "FillLight") - Marker(room, "CeilingLight")
                    + Depth * 1.95f + Vector3.up * .31f) / .95f;
                if (Mathf.Abs(Depth.magnitude - 1f) > .01f || Mathf.Abs(Right.magnitude - 1f) > .01f
                    || Mathf.Abs(Vector3.Dot(Depth, Right)) > .01f || Mathf.Abs(Right.y) > .01f)
                    throw new InvalidOperationException("Room markers no longer describe the expected metre-scale horizontal frame.");
                Origin = Marker(room, "WindowDetailPosition") - Right * 1.1f - Depth * 1.9f - Vector3.up * 1.85f;
            }
            // Coordinates are the authored Blender room coordinates: x, depth, height.
            public Vector3 Point(float x, float depth, float height) => Origin + Right * x + Depth * depth + Vector3.up * height;
        }

        [MenuItem("MiniGame/Main Room/Setup gameplay")]
        public static void Setup() => TrySetup();

        private static bool TrySetup()
        {
            RequireEditMode();
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return false;
            if (!File.Exists(ScenePath))
                throw new FileNotFoundException("Import the combined room with MiniGame/Room Art/Import and open combined room first.", ScenePath);
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            ConfigureOpenScene();
            return true;
        }

        [MenuItem("MiniGame/Main Room/Setup and play")]
        public static void SetupAndPlay()
        {
            if (TrySetup()) StartPlay();
        }

        [MenuItem("MiniGame/Main Room/Open and play")]
        public static void OpenAndPlay()
        {
            RequireEditMode();
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            if (!HasGameplay(scene)) ConfigureOpenScene();
            else DialoguePackageImporter.EnsureDefaultPackage();
            StartPlay();
        }

        private static void StartPlay()
        {
            EditorApplication.ExecuteMenuItem("Window/General/Game");
            EditorApplication.isPlaying = true;
        }

        private static void RequireEditMode()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Leave Play Mode before configuring the main room.");
        }

        private static bool HasGameplay(Scene scene) => scene.GetRootGameObjects()
            .SelectMany(root => root.GetComponentsInChildren<MiniGameBootstrap>(true)).Count() == 1;

        // Also called after art regeneration, so reimporting cannot silently remove gameplay.
        public static void ConfigureOpenScene()
        {
            RequireEditMode();
            var scene = SceneManager.GetActiveScene();
            if (scene.path != ScenePath) throw new InvalidOperationException("Open MainRoomCombined before configuring its gameplay.");
            var room = scene.GetRootGameObjects().Single(root => root.name == "MainRoomCombined");
            if (room.GetComponentsInChildren<Transform>(true).Any(item => item.name == "Unity_ReferenceOrigin"))
            {
                ReferenceRoomGameplayBuilder.Configure(room);
                return;
            }
            var frame = new RoomFrame(room);
            var retroProfile = RetroScreenSetup.FindProfile(scene);
            var screenSurface = Surface(room, "CRT screen emission");
            var monitor = Surface(room, "CRT_Monitor_Tripo");
            var floor = Surface(room, "Floor substrate");
            var bed = Surface(room, "B_SM_mattress_1");
            if (Vector3.Distance(screenSurface.bounds.center, frame.Point(-2.311f, -1.1f, 1.004f)) > .04f)
                throw new InvalidOperationException("The imported CRT has moved; inspect its placement before regenerating gameplay anchors.");
            if (Mathf.Abs(floor.bounds.size.y - .1f) > .02f || Mathf.Abs(floor.bounds.max.y - frame.Origin.y) > .02f)
                throw new InvalidOperationException("The imported floor does not match the room marker frame.");

            var package = DialoguePackageImporter.EnsureDefaultPackage();
            var legacyYarn = AssetDatabase.LoadAssetAtPath<YarnProject>(GeneratePrototypeYarn.ProjectPath);
            if (legacyYarn == null || legacyYarn.Program == null) legacyYarn = GeneratePrototypeYarn.Generate();
            foreach (var old in scene.GetRootGameObjects().Where(root => root.name == GameplayRootName).ToArray())
                UnityEngine.Object.DestroyImmediate(old);
            foreach (var preview in scene.GetRootGameObjects().Where(root => root.name == "Main Camera" && root.GetComponent<Camera>() != null).ToArray())
                UnityEngine.Object.DestroyImmediate(preview);
            if (scene.GetRootGameObjects().Any(root => root.GetComponentInChildren<MiniGameBootstrap>(true) != null))
                throw new InvalidOperationException("Another gameplay host already exists; refusing to install a second authority.");

            var gameplay = new GameObject(GameplayRootName);
            var collisionRoot = Child(gameplay.transform, "Room collision");
            foreach (var surface in room.GetComponentsInChildren<MeshRenderer>(true))
            {
                if (IsRoomShell(surface.name)) BoundsCollider(collisionRoot.transform, surface.name, surface.bounds, .08f);
                else if (IsSolidFurniture(surface.name)) BoundsCollider(collisionRoot.transform, surface.name, surface.bounds, .03f);
            }

            // The combined GLB joined these objects after losing their local translations.
            // Reconstruct the closure from the original room recipe, without altering source art.
            var closure = Surface(room, "Front closure");
            closure.enabled = false;
            PrefabUtility.RecordPrefabInstancePropertyModifications(closure);
            var closureRoot = Child(gameplay.transform, "Entrance closure restored from source layout");
            var plaster = ClosureMaterial(closure, "Plaster");
            var timber = ClosureMaterial(closure, "Timber");
            var paint = ClosureMaterial(closure, "Paint");
            var hardware = ClosureMaterial(closure, "Hardware");
            SourceBox(closureRoot.transform, frame, "Front left wall", new Vector3(-1.85f, -3.47f, 1.5f), new Vector3(1.9f, .12f, 3f), plaster);
            SourceBox(closureRoot.transform, frame, "Front right wall", new Vector3(1.45f, -3.47f, 1.5f), new Vector3(2.7f, .12f, 3f), plaster);
            SourceBox(closureRoot.transform, frame, "Front above door", new Vector3(-.4f, -3.47f, 2.65f), new Vector3(1f, .12f, .7f), plaster);
            var door = SourceBox(closureRoot.transform, frame, "Entrance door", new Vector3(-.4f, -3.45f, 1.12f), new Vector3(.92f, .06f, 2.24f), timber);
            SourceBox(closureRoot.transform, frame, "Entrance frame left", new Vector3(-.91f, -3.4f, 1.15f), new Vector3(.07f, .12f, 2.3f), paint);
            SourceBox(closureRoot.transform, frame, "Entrance frame right", new Vector3(.11f, -3.4f, 1.15f), new Vector3(.07f, .12f, 2.3f), paint);
            SourceBox(closureRoot.transform, frame, "Entrance frame top", new Vector3(-.4f, -3.4f, 2.3f), new Vector3(1.09f, .12f, .07f), paint);
            var handle = SourceBox(door.transform, frame, "Entrance handle", new Vector3(-.09f, -3.39f, 1.1f), new Vector3(.12f, .05f, .025f), hardware);
            // SourceBox accepts metre dimensions; preserve the handle when parented below a scaled cube.
            handle.transform.SetParent(closureRoot.transform, true);
            UnityEngine.Object.DestroyImmediate(handle.GetComponent<Collider>());
            door.AddComponent<WorldInteraction>().Configure(InteractionTarget.Door, "门 / 接受晨检", door.GetComponent<Renderer>());

            var interactionRoot = Child(gameplay.transform, "World interaction targets");
            var terminalBounds = monitor.bounds;
            terminalBounds.Encapsulate(screenSurface.bounds);
            terminalBounds.Expand(.035f);
            var terminal = BoundsCollider(interactionRoot.transform, "Computer interaction", terminalBounds, .04f);
            terminal.gameObject.AddComponent<WorldInteraction>().Configure(InteractionTarget.Computer, "和平仪 / 打开终端", screenSurface);
            var bedBounds = bed.bounds;
            bedBounds.Expand(new Vector3(.04f, .08f, .04f));
            var bedHit = BoundsCollider(interactionRoot.transform, "Bed interaction", bedBounds, .04f);
            bedHit.gameObject.AddComponent<WorldInteraction>().Configure(InteractionTarget.Bed, "床 / 休息", bed);

            var spawn = frame.Point(.15f, -1.65f, .055f);
            var anchors = Child(gameplay.transform, "Gameplay anchors");
            Anchor(anchors.transform, "Player spawn", spawn);
            Anchor(anchors.transform, "Door approach", frame.Point(-.4f, -2.4f, .055f));
            Anchor(anchors.transform, "Computer approach", frame.Point(-.78f, -1.1f, .055f));
            Anchor(anchors.transform, "Bed approach", frame.Point(-1.4f, 1.08f, .055f));
            Physics.SyncTransforms();
            RequireWalkable(spawn, "player spawn");
            RequireWalkable(frame.Point(-.4f, -2.4f, .055f), "door approach");
            RequireWalkable(frame.Point(-.78f, -1.1f, .055f), "computer approach");
            RequireWalkable(frame.Point(-1.4f, 1.08f, .055f), "bed approach");
            RequireRay(frame.Point(-.4f, -2.4f, 1.56f), door.GetComponent<Renderer>().bounds.center, InteractionTarget.Door);
            RequireRay(frame.Point(-.78f, -1.1f, 1.56f), screenSurface.bounds.center, InteractionTarget.Computer);
            RequireRay(frame.Point(-1.4f, 1.08f, 1.56f), bed.bounds.center, InteractionTarget.Bed);

            var playerObject = Child(gameplay.transform, "Player");
            playerObject.transform.position = spawn;
            playerObject.transform.rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(screenSurface.bounds.center - spawn, Vector3.up));
            var controller = playerObject.AddComponent<CharacterController>();
            controller.height = 1.75f; controller.center = new Vector3(0f, .875f, 0f);
            controller.radius = .23f; controller.stepOffset = .18f; controller.skinWidth = .025f;
            var cameraObject = Child(playerObject.transform, "Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.localPosition = new Vector3(0f, 1.56f, 0f);
            var camera = cameraObject.AddComponent<Camera>();
            cameraObject.AddComponent<AudioListener>();
            camera.fieldOfView = 63f; camera.nearClipPlane = .025f; camera.farClipPlane = 40f;
            camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color(.045f, .05f, .045f);
            camera.allowHDR = true;
            RetroScreenSetup.Attach(camera, retroProfile);
            var player = playerObject.AddComponent<ApartmentPlayer>(); player.Configure(camera);

            var terminalPose = Child(gameplay.transform, "Terminal camera destination");
            terminalPose.transform.position = screenSurface.bounds.center + frame.Right * .62f;
            terminalPose.transform.rotation = Quaternion.LookRotation(-frame.Right, Vector3.up);
            var seatedEye = Child(gameplay.transform, "Terminal seated eye");
            seatedEye.transform.position = screenSurface.bounds.center + frame.Right * 1.05f + Vector3.up * .18f;
            seatedEye.transform.rotation = Quaternion.LookRotation(screenSurface.bounds.center - seatedEye.transform.position, Vector3.up);
            var alignment = Child(gameplay.transform, "Terminal screen alignment");
            alignment.transform.SetPositionAndRotation(terminalPose.transform.position, terminalPose.transform.rotation);
            var bedLight = Child(gameplay.transform, "Night bed guidance").AddComponent<Light>();
            bedLight.transform.position = new Vector3(bed.bounds.center.x, frame.Origin.y + 2.94f, bed.bounds.center.z);
            bedLight.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            bedLight.type = LightType.Spot; bedLight.range = 3.6f; bedLight.spotAngle = 56f; bedLight.innerSpotAngle = 36f;
            bedLight.intensity = 8f; bedLight.color = new Color(.94f, .93f, .79f); bedLight.enabled = false;
            bedLight.shadows = LightShadows.Soft; bedLight.shadowBias = .015f; bedLight.shadowNormalBias = .08f;
            bedLight.renderMode = LightRenderMode.ForcePixel;
            int bedLayer = EnsureBedtimeLayer();
            var bedRenderers = room.GetComponentsInChildren<Renderer>(true).Where(renderer =>
                renderer.name == "B_SM_bed_1" || renderer.name == "B_SM_mattress_1" || renderer.name == "B_SM_pillow_1"
                || renderer.name == "B_SM_rug_2").ToArray();
            if (bedRenderers.Length != 4) throw new InvalidOperationException("The bed, mattress, pillow and blanket must all be available for the sleep spotlight.");
            foreach (var renderer in bedRenderers)
            {
                renderer.gameObject.layer = bedLayer;
                PrefabUtility.RecordPrefabInstancePropertyModifications(renderer.gameObject);
            }
            bedLight.cullingMask = 1 << bedLayer;
            var ceilingLight = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<Light>(true))
                .Single(light => light.name == "CeilingLight");
            var host = Child(gameplay.transform, "MiniGame Bootstrap");
            host.SetActive(false);
            var ui = host.AddComponent<MiniGameScreen>();
            var effects = host.AddComponent<PresentationEffects>();
            effects.Configure(camera, terminalPose.transform, ceilingLight, bedLight);
            effects.ConfigureTerminalSurface(screenSurface);
            effects.ConfigureSeatedEntry(player, seatedEye.transform, alignment.transform, screenSurface);
            var bedtime = host.AddComponent<BedtimeGuidance>();
            bedtime.Configure(camera, bedLight, bedRenderers, new Vector3(bed.bounds.center.x, bed.bounds.max.y, bed.bounds.center.z), 1.12f);
            effects.ConfigureBedtime(bedtime);
            var bridge = host.AddComponent<MiniGameYarnBridge>(); bridge.Configure(legacyYarn);
            var bootstrap = host.AddComponent<MiniGameBootstrap>(); bootstrap.Configure(ui, player, effects, bridge);
            var serializedHost = new SerializedObject(bootstrap);
            serializedHost.FindProperty("useTerminalDemo").boolValue = true;
            serializedHost.FindProperty("dialoguePackage").objectReferenceValue = package;
            serializedHost.ApplyModifiedPropertiesWithoutUndo();
            host.SetActive(true);

            var exterior = room.GetComponentsInChildren<MeshRenderer>(true).Where(renderer => renderer.name.StartsWith("B_ExteriorConcrete_", StringComparison.Ordinal)).ToArray();
            if (exterior.Length != 2 || exterior.Any(renderer => !renderer.enabled))
                throw new InvalidOperationException("Both original exterior concrete wall and ledge must remain visible.");
            var targets = gameplay.GetComponentsInChildren<WorldInteraction>(true);
            if (!targets.Select(target => target.Target).OrderBy(target => target)
                .SequenceEqual(new[] { InteractionTarget.Door, InteractionTarget.Computer, InteractionTarget.Bed }.OrderBy(target => target)))
                throw new InvalidOperationException("The room must contain one door, one computer and one bed interaction.");
            if (scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<Camera>(true)).Count() != 1)
                throw new InvalidOperationException("The playable scene must have one camera.");

            EditorBuildSettings.scenes = new[]
                {
                    new EditorBuildSettingsScene(ScenePath, true),
                    new EditorBuildSettingsScene(ReferencePrototypeBuilder.ScenePath, true)
                }
                .Concat(EditorBuildSettings.scenes.Where(entry => entry.path != ScenePath && entry.path != ReferencePrototypeBuilder.ScenePath)).ToArray();
            EditorSceneManager.MarkSceneDirty(scene);
            CampaignBuilder.EnableCampaignInOpenScene();
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            var report = new SetupReport
            {
                scene = ScenePath, unity = Application.unityVersion,
                scope = "Editor wiring, marker calibration, collision and line-of-sight probes; runtime and visual acceptance are separate checks",
                frontClosureRepair = "Restored separate closure pieces from Art/MainRoom/scripts/build_room.py; malformed joined preview mesh remains hidden",
                worldInteractions = targets.Length, colliders = gameplay.GetComponentsInChildren<Collider>(true).Length,
                exteriorConcreteRenderers = exterior.Length, roomOrigin = frame.Origin, sourceRight = frame.Right, sourceDepth = frame.Depth,
                playerSpawn = spawn, screenCenter = screenSurface.bounds.center, screenSize = screenSurface.bounds.size,
                terminalCamera = terminalPose.transform.position, terminalCameraDistance = Vector3.Distance(terminalPose.transform.position, screenSurface.bounds.center),
                checks = new[] { "imported metre scale", "CRT marker alignment", "floor top alignment", "four walkable positions", "three unobstructed interaction rays", "single runtime host", "single camera", "native Yarn package", "exterior wall and ledge" }
            };
            var artifactDirectory = Path.GetFullPath(Path.Combine(Application.dataPath, "../../Artifacts/Unity/MainRoomCombined"));
            Directory.CreateDirectory(artifactDirectory);
            File.WriteAllText(Path.Combine(artifactDirectory, "gameplay-setup.json"), JsonUtility.ToJson(report, true));
            Selection.activeGameObject = playerObject;
            Debug.Log("MINIGAME_MAIN_ROOM_GAMEPLAY_READY " + JsonUtility.ToJson(report));
        }

        private static int EnsureBedtimeLayer()
        {
            const int index = 28;
            const string name = "BedtimeLit";
            var settings = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            var layer = settings.FindProperty("layers").GetArrayElementAtIndex(index);
            if (!string.IsNullOrEmpty(layer.stringValue) && layer.stringValue != name)
                throw new InvalidOperationException("Layer28 is already in use; choose a free named layer for the bedtime light.");
            layer.stringValue = name;
            settings.ApplyModifiedPropertiesWithoutUndo();
            return index;
        }

        private static Vector3 Marker(GameObject room, string name) => room.GetComponentsInChildren<Transform>(true)
            .Single(transform => transform.name == "Unity_" + name).position;
        private static MeshRenderer Surface(GameObject room, string prefix) => room.GetComponentsInChildren<MeshRenderer>(true)
            .Single(renderer => renderer.name.StartsWith(prefix, StringComparison.Ordinal));
        private static bool IsRoomShell(string name) => name.StartsWith("Floor substrate", StringComparison.Ordinal)
            || name.StartsWith("Ceiling.", StringComparison.Ordinal) || name.StartsWith("Left wallpaper", StringComparison.Ordinal)
            || name.StartsWith("Right wallpaper", StringComparison.Ordinal) || name.StartsWith("Back left", StringComparison.Ordinal)
            || name.StartsWith("Back right", StringComparison.Ordinal) || name.StartsWith("Above window", StringComparison.Ordinal)
            || name.StartsWith("Below window", StringComparison.Ordinal);
        private static bool IsSolidFurniture(string name) => name == "B_obj_01" || name == "B_HT01"
            || name == "B_SM_CupboardChunk" || name == "B_SM_MovingBoxes" || name == "B_SM_SingleBox";

        private static GameObject Child(Transform parent, string name)
        {
            var child = new GameObject(name); child.transform.SetParent(parent, false); return child;
        }
        private static void Anchor(Transform parent, string name, Vector3 position) => Child(parent, name).transform.position = position;
        private static BoxCollider BoundsCollider(Transform parent, string name, Bounds bounds, float minimumThickness)
        {
            var child = Child(parent, name + " collider");
            child.transform.position = bounds.center;
            var collider = child.AddComponent<BoxCollider>();
            collider.size = new Vector3(Mathf.Max(bounds.size.x, minimumThickness), Mathf.Max(bounds.size.y, minimumThickness), Mathf.Max(bounds.size.z, minimumThickness));
            return collider;
        }
        private static Material ClosureMaterial(Renderer closure, string label) => closure.sharedMaterials
            .Single(material => material.name.EndsWith("_" + label, StringComparison.Ordinal));
        private static GameObject SourceBox(Transform parent, RoomFrame frame, string name, Vector3 sourcePosition, Vector3 sourceSize, Material material)
        {
            var box = GameObject.CreatePrimitive(PrimitiveType.Cube); box.name = name;
            box.transform.position = frame.Point(sourcePosition.x, sourcePosition.y, sourcePosition.z);
            box.transform.rotation = Quaternion.LookRotation(frame.Depth, Vector3.up);
            box.transform.localScale = new Vector3(sourceSize.x, sourceSize.z, sourceSize.y);
            box.transform.SetParent(parent, true);
            box.GetComponent<Renderer>().sharedMaterial = material;
            return box;
        }
        private static void RequireWalkable(Vector3 feet, string name)
        {
            if (Physics.CheckCapsule(feet + Vector3.up * .25f, feet + Vector3.up * 1.5f, .23f, ~0, QueryTriggerInteraction.Ignore))
                throw new InvalidOperationException("A solid collider blocks the " + name + ".");
        }
        private static void RequireRay(Vector3 origin, Vector3 target, InteractionTarget expected)
        {
            var direction = target - origin;
            if (direction.magnitude > 3.2f || !Physics.Raycast(origin, direction.normalized, out var hit, 3.2f)
                || hit.collider.GetComponentInParent<WorldInteraction>()?.Target != expected)
                throw new InvalidOperationException("The " + expected + " interaction is blocked from its standing approach.");
        }
    }
}
