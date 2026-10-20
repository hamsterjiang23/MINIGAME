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
    public static class ReferenceRoomGameplayBuilder
    {
        private const string Generated = "Assets/MiniGame/Generated/ReferenceRoom";
        private const float Width = 9.70078f, Depth = 4.41176f, Height = 3.12066f;
        private static Vector3 Marker(GameObject room, string name) => room.GetComponentsInChildren<Transform>(true)
            .Single(item => item.name == "Unity_" + name).position;

        public static void Configure(GameObject room)
        {
            var scene = SceneManager.GetActiveScene();
            var retroProfile = RetroScreenSetup.FindProfile(scene);
            foreach (var old in scene.GetRootGameObjects().Where(item => item.name == "MainRoom Gameplay"
                || item.name == "Main Camera" && item.GetComponent<Camera>() != null).ToArray())
                UnityEngine.Object.DestroyImmediate(old);
            if (UnityEngine.Object.FindObjectsOfType<MiniGameBootstrap>().Length > 0)
                throw new InvalidOperationException("Another gameplay authority is already installed.");
            Directory.CreateDirectory(Generated);
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            var origin = Marker(room, "ReferenceOrigin");
            var right = Marker(room, "ReferenceRight") - origin;
            var depth = Marker(room, "ReferenceDepth") - origin;
            if (Mathf.Abs(right.magnitude - 1) > .001f || Mathf.Abs(depth.magnitude - 1) > .001f
                || Mathf.Abs(Vector3.Dot(right, depth)) > .001f)
                throw new InvalidOperationException("Reference axes must remain orthogonal metres.");
            var root = new GameObject("MainRoom Gameplay").transform;
            var solids = Child(root, "Room collision");
            var renderers = room.GetComponentsInChildren<MeshRenderer>(true);
            var floor = renderers.Single(item => item.name == "B_Floor");
            var ceiling = renderers.Single(item => item.name == "B_Awal");
            BoxBounds(solids, "Reference floor", floor.bounds, true);
            BoxBounds(solids, "Reference ceiling", ceiling.bounds, false);
            foreach (var surface in renderers)
            {
                if (surface.name.StartsWith("B_Wall__", StringComparison.Ordinal)
                    || surface.name == "B_Awall2" || surface.name.StartsWith("B_ReferenceDoorWall", StringComparison.Ordinal))
                {
                    var collider = surface.gameObject.GetComponent<MeshCollider>();
                    if (collider == null) collider = surface.gameObject.AddComponent<MeshCollider>();
                    collider.sharedMesh = surface.GetComponent<MeshFilter>().sharedMesh;
                }
                else if (surface.name == "B_obj_01" || surface.name.StartsWith("B_HT01", StringComparison.Ordinal)
                    || surface.name == "B_SM_CupboardChunk" || surface.name == "B_SM_MovingBoxes" || surface.name == "B_SM_SingleBox")
                    BoundsBox(solids, surface.name, surface.bounds);
            }
            var bed = renderers.Single(item => item.name == "B_SM_mattress_1");
            var screen = renderers.Single(item => item.name.StartsWith("CRT screen emission", StringComparison.Ordinal));
            var monitor = renderers.Single(item => item.name.StartsWith("CRT_Monitor_Tripo", StringComparison.Ordinal));
            var targets = Child(root, "World interaction targets");
            var computerBounds = monitor.bounds; computerBounds.Encapsulate(screen.bounds); computerBounds.Expand(.02f);
            var computer = BoundsBox(targets, "Computer interaction", computerBounds);
            computer.AddComponent<WorldInteraction>().Configure(InteractionTarget.Computer, "和平仪 / 打开终端", monitor);
            var bedBounds = bed.bounds; bedBounds.Expand(new Vector3(.04f, .08f, .04f));
            BoundsBox(targets, "Bed interaction", bedBounds).AddComponent<WorldInteraction>()
                .Configure(InteractionTarget.Bed, "床 / 休息", bed);
            var wood = Material("Reference door wood", new Color(.78f, .68f, .52f));
            wood.mainTexture = floor.sharedMaterial.mainTexture;
            var paint = Material("Reference door paint", new Color(.63f, .66f, .58f));
            var metal = Material("Reference door fittings", new Color(.25f, .28f, .26f));
            var doorCentre = Marker(room, "DoorCentre");
            var doorwayFloor = doorCentre - Vector3.up * 1.25f;
            var hinge = Child(root, "Entrance hinge");
            hinge.position = doorwayFloor - depth * (ReferenceDoorFrameBuilder.ClearWidth * .5f) + right * ReferenceDoorFrameBuilder.HingeOutset;
            hinge.rotation = Quaternion.LookRotation(-right, Vector3.up);
            var leaf = Cube(hinge, "Entrance door", new Vector3(.725f, 1.24f, ReferenceDoorFrameBuilder.HingeOutset),
                new Vector3(ReferenceDoorFrameBuilder.LeafWidth, ReferenceDoorFrameBuilder.LeafHeight, .08f), wood);
            leaf.AddComponent<WorldInteraction>().Configure(InteractionTarget.Door, "门", leaf.GetComponent<Renderer>());
            var handle = Cube(hinge, "Entrance handle", new Vector3(1.27f, 1.12f, ReferenceDoorFrameBuilder.HingeOutset + .075f), new Vector3(.16f, .035f, .045f), metal);
            UnityEngine.Object.DestroyImmediate(handle.GetComponent<Collider>());
            var door = hinge.gameObject.AddComponent<HingedDoor>(); door.Configure(hinge);
            var frame = Child(root, "Entrance frame"); frame.position = doorwayFloor; frame.rotation = Quaternion.LookRotation(right, Vector3.up);
            ReferenceDoorFrameBuilder.Build(frame, paint);
            ReferenceCorridorBuilder.Build(root, doorwayFloor, right, depth, Height);
            var anchors = Child(root, "Gameplay anchors");
            Anchor(anchors, "Player spawn", Marker(room, "Spawn"));
            Anchor(anchors, "Door approach", Marker(room, "DoorApproach"));
            Anchor(anchors, "Computer approach", Marker(room, "ComputerApproach"));
            Anchor(anchors, "Bed approach", Marker(room, "BedApproach"));
            var beach = Poster(root, room, "Beach", "poster-beach", "beach", "这里是我的故乡…但我从没去过海边", depth);
            var instrument = Poster(root, room, "Device", "poster-instrument", "instrument", "新一代手持仪器引领我们走向未来！", depth);
            Anchor(anchors, "Poster beach approach", Marker(room, "BedApproach"));
            Anchor(anchors, "Poster device approach", instrument.position + depth * 1.25f + Vector3.up * (origin.y + .055f - instrument.position.y));
            var playerObject = Child(root, "Player"); playerObject.position = Marker(room, "Spawn");
            playerObject.rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(screen.bounds.center - playerObject.position, Vector3.up));
            var controller = playerObject.gameObject.AddComponent<CharacterController>();
            controller.height = 1.75f; controller.center = new Vector3(0, .875f, 0); controller.radius = .23f;
            controller.stepOffset = .18f; controller.skinWidth = .025f;
            var eye = Child(playerObject, "Main Camera"); eye.localPosition = new Vector3(0, 1.56f, 0); eye.tag = "MainCamera";
            var camera = eye.gameObject.AddComponent<Camera>(); eye.gameObject.AddComponent<AudioListener>();
            camera.nearClipPlane = .025f; camera.farClipPlane = 45; camera.allowHDR = true;
            RetroScreenSetup.Attach(camera, retroProfile);
            camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color(.045f, .05f, .045f);
            eye.gameObject.AddComponent<ReferenceCameraProjection>().Configure(61.9275f);
            var player = playerObject.gameObject.AddComponent<ApartmentPlayer>(); player.Configure(camera);
            CombinedInteractionSetup.ConfigureDoorCamera(camera, player, door);
            var destination = Child(root, "Terminal camera destination");
            destination.position = Marker(room, "TerminalAlign"); destination.rotation = Quaternion.LookRotation(-depth, Vector3.up);
            var alignment = Child(root, "Terminal screen alignment"); alignment.SetPositionAndRotation(destination.position, destination.rotation);
            var seated = Child(root, "Terminal seated eye"); seated.position = Marker(room, "SeatEye"); seated.LookAt(screen.bounds.center);
            var bedLight = Child(root, "Night bed guidance").gameObject.AddComponent<Light>();
            bedLight.transform.position = new Vector3(bed.bounds.center.x, origin.y + Height - .18f, bed.bounds.center.z);
            bedLight.transform.rotation = Quaternion.Euler(90, 0, 0);
            bedLight.type = LightType.Spot; bedLight.spotAngle = 56; bedLight.innerSpotAngle = 36;
            bedLight.range = 3.6f; bedLight.intensity = 8; bedLight.color = new Color(.94f, .93f, .79f);
            bedLight.enabled = false; bedLight.shadows = LightShadows.Soft; bedLight.renderMode = LightRenderMode.ForcePixel;
            int layer = EnsureBedLayer(); bedLight.cullingMask = 1 << layer;
            var bedRenderers = renderers.Where(item => new[] { "B_SM_bed_1", "B_SM_mattress_1", "B_SM_pillow_1", "B_SM_rug_2" }.Contains(item.name)).ToArray();
            if (bedRenderers.Length != 4) throw new InvalidOperationException("Four reference bed surfaces are required.");
            foreach (var renderer in bedRenderers) { renderer.gameObject.layer = layer; PrefabUtility.RecordPrefabInstancePropertyModifications(renderer.gameObject); }
            var host = Child(root, "MiniGame Bootstrap").gameObject; host.SetActive(false);
            var ui = host.AddComponent<MiniGameScreen>(); var effects = host.AddComponent<PresentationEffects>();
            var ceilingLight = UnityEngine.Object.FindObjectsOfType<Light>().Single(item => item.name == "CeilingLight");
            effects.Configure(camera, destination, ceilingLight, bedLight); effects.ConfigureSeatedEntry(player, seated, alignment, screen);
            var bedtime = host.AddComponent<BedtimeGuidance>();
            bedtime.Configure(camera, bedLight, bedRenderers, new Vector3(bed.bounds.center.x, bed.bounds.max.y, bed.bounds.center.z), 1.12f);
            effects.ConfigureBedtime(bedtime);
            var legacy = AssetDatabase.LoadAssetAtPath<YarnProject>(GeneratePrototypeYarn.ProjectPath);
            var bridge = host.AddComponent<MiniGameYarnBridge>(); bridge.Configure(legacy);
            var bootstrap = host.AddComponent<MiniGameBootstrap>(); bootstrap.Configure(ui, player, effects, bridge);
            host.AddComponent<MiniGameAudioDirector>().Configure(MiniGameAudioSetup.EnsureAssets());
            var serialized = new SerializedObject(bootstrap); serialized.FindProperty("useCampaign").boolValue = true; serialized.ApplyModifiedPropertiesWithoutUndo();
            host.SetActive(true);
            Physics.SyncTransforms();
            controller.enabled = false;
            foreach (var name in new[] { "Player spawn", "Door approach", "Computer approach", "Bed approach", "Poster beach approach", "Poster device approach" })
            {
                var feet = anchors.Find(name).position;
                var blocked = Physics.OverlapCapsule(feet + Vector3.up * .25f, feet + Vector3.up * 1.5f, .23f, ~0, QueryTriggerInteraction.Ignore);
                if (blocked.Length > 0) throw new InvalidOperationException("Reference standing anchor blocked: " + name + " by " + string.Join(",", blocked.Select(item => item.name)));
            }
            controller.enabled = true;
            if (renderers.Count(item => item.name.StartsWith("B_ExteriorConcrete_", StringComparison.Ordinal)) != 2)
                throw new InvalidOperationException("Both original exterior concrete objects must be retained.");
            RoomLightingSetup.EnsureInScene(scene);
            AssetDatabase.SaveAssets(); EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene, MainRoomGameplayBuilder.ScenePath);
            var report = new { width = Width, depth = Depth, height = Height, horizontalFov = 61.9275f, frame133 = true,
                doorWidth = 1.5, doorHeight = 2.5, posters = 2, exteriorConcrete = 2, source = "Art/MainRoom2/minigame员工宿舍.blend" };
            var path = Path.GetFullPath(Path.Combine(Application.dataPath, "../../Artifacts/ReferenceRoom")); Directory.CreateDirectory(path);
            File.WriteAllText(Path.Combine(path, "setup.json"), Newtonsoft.Json.JsonConvert.SerializeObject(report, Newtonsoft.Json.Formatting.Indented));
            Debug.Log("MINIGAME_REFERENCE_ROOM_READY net=" + Width + "x" + Depth + "x" + Height);
        }

        private static Transform Poster(Transform parent, GameObject room, string marker, string id, string file, string caption, Vector3 depth)
        {
            const string folder = "Assets/MiniGame/Art/Interactions"; Directory.CreateDirectory(folder);
            var path = folder + "/" + file + ".png";
            File.Copy(Path.GetFullPath(Path.Combine(Application.dataPath, "../../Art/Interactions/Posters/" + file + ".png")), path, true);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path); importer.mipmapEnabled = false; importer.maxTextureSize = 2048; importer.SaveAndReimport();
            var material = Material("Poster " + file, Color.white); material.mainTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(path); EditorUtility.SetDirty(material);
            var poster = GameObject.CreatePrimitive(PrimitiveType.Quad); poster.name = "Readable poster " + file; poster.transform.SetParent(parent, false);
            poster.transform.position = Marker(room, "Poster" + marker); poster.transform.rotation = Quaternion.LookRotation(-depth, Vector3.up);
            poster.transform.localScale = new Vector3(.66f, .88f, 1);
            UnityEngine.Object.DestroyImmediate(poster.GetComponent<Collider>());
            var collider = poster.AddComponent<BoxCollider>(); collider.size = new Vector3(1, 1, .02f);
            poster.GetComponent<Renderer>().sharedMaterial = material;
            poster.AddComponent<ReadableProp>().Configure(id, caption, poster.GetComponent<Renderer>());
            return poster.transform;
        }
        private static Transform Child(Transform parent, string name) { var item = new GameObject(name).transform; item.SetParent(parent, false); return item; }
        private static void Anchor(Transform parent, string name, Vector3 position) => Child(parent, name).position = position;
        private static GameObject Cube(Transform parent, string name, Vector3 position, Vector3 scale, Material material)
        { var item = GameObject.CreatePrimitive(PrimitiveType.Cube); item.name = name; item.transform.SetParent(parent, false); item.transform.localPosition = position; item.transform.localScale = scale; item.GetComponent<Renderer>().sharedMaterial = material; return item; }
        private static Material Material(string name, Color color)
        {
            var path = Generated + "/" + name + ".mat"; var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null) { material = new Material(Shader.Find("MiniGame/Room Art Standard")); AssetDatabase.CreateAsset(material, path); }
            material.color = color; material.SetFloat("_Glossiness", .15f); EditorUtility.SetDirty(material); return material;
        }
        private static GameObject BoundsBox(Transform parent, string name, Bounds bounds)
        { var item = Child(parent, name + " collider"); item.position = bounds.center; item.gameObject.AddComponent<BoxCollider>().size = Vector3.Max(bounds.size, Vector3.one * .025f); return item.gameObject; }
        private static void BoxBounds(Transform parent, string name, Bounds bounds, bool floor)
        { bounds.center += Vector3.up * (floor ? -.05f : .05f); bounds.size = new Vector3(bounds.size.x, .1f, bounds.size.z); BoundsBox(parent, name, bounds); }
        private static int EnsureBedLayer()
        {
            var settings = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            var property = settings.FindProperty("layers").GetArrayElementAtIndex(28);
            if (!string.IsNullOrEmpty(property.stringValue) && property.stringValue != "BedtimeLit") throw new InvalidOperationException("Layer28 is occupied.");
            property.stringValue = "BedtimeLit"; settings.ApplyModifiedPropertiesWithoutUndo(); return 28;
        }
    }
}
