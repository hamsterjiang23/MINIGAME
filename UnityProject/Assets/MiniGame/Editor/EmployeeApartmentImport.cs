using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using MiniGame.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace MiniGame.UnityHost.Editor
{
    /// <summary>Replaces room art without rebuilding the corridor or the gameplay authority.</summary>
    public static class EmployeeApartmentImport
    {
        private const string Art = "Assets/MiniGame/Art/EmployeeApartment/";
        private static readonly string[] ProtectedRoots = { "Reference exterior corridor", "Entrance hinge", "Entrance frame",
            "Door camera alignment", "Door camera outside" };
        private static readonly string[] DoorWalls = { "B_ReferenceDoorWallLowerSide", "B_ReferenceDoorWallUpperSide", "B_ReferenceDoorWallLintel" };

        [Serializable] private sealed class Report
        {
            public string scene, corridorBefore, corridorAfter;
            public int replaced, added, disabled, missingMaterials;
            public string[] excludedOverlappingPosters, blockedAnchors;
            public bool corridorPreserved;
        }

        [MenuItem("MiniGame/Room Art/Replace room with Employee Apartment 1.0 (keep corridor)")]
        public static void ApplyInOpenScene()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Exit Play Mode before replacing room art.");
            var scene = SceneManager.GetActiveScene();
            if (scene.path != MainRoomGameplayBuilder.ScenePath) throw new InvalidOperationException("Open MainRoomCombined first.");
            var room = scene.GetRootGameObjects().Single(value => value.name == "MainRoomCombined");
            string modelPath = Art + "MainRoom.fbx";
            AssetDatabase.ImportAsset(modelPath, ImportAssetOptions.ForceSynchronousImport);
            var importer = (ModelImporter)AssetImporter.GetAtPath(modelPath);
            importer.importAnimation = false;
            importer.importBlendShapes = false;
            importer.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
            importer.SaveAndReimport();
            var materials = MainRoomPreviewBuilder.ImportMaterialAssets(Art);
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
            if (model == null) throw new InvalidOperationException("Employee Apartment FBX was not imported.");

            var report = new Report { scene = scene.path, corridorBefore = ProtectedSignature(scene) };
            Undo.IncrementCurrentGroup();
            int undoGroup = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Replace employee apartment room art");
            foreach (var root in scene.GetRootGameObjects().Where(value => !ProtectedRoots.Contains(value.name)))
                Undo.RegisterFullObjectHierarchyUndo(root, "Replace room art");
            var incoming = (GameObject)PrefabUtility.InstantiatePrefab(model, scene);
            try
            {
                incoming.transform.SetParent(room.transform, false);
                var existing = room.GetComponentsInChildren<MeshRenderer>(true)
                    .Where(value => !value.transform.IsChildOf(incoming.transform)).ToDictionary(value => value.name);
                var oldScreenMaterial = existing["CRT screen emission"].sharedMaterial;
                var seen = new HashSet<string>();
                foreach (var source in incoming.GetComponentsInChildren<MeshRenderer>(true))
                {
                    seen.Add(source.name);
                    if (!existing.TryGetValue(source.name, out var target))
                    {
                        var item = new GameObject(source.name, typeof(MeshFilter), typeof(MeshRenderer));
                        Undo.RegisterCreatedObjectUndo(item, "Add apartment art");
                        item.transform.SetParent(room.transform, false);
                        target = item.GetComponent<MeshRenderer>();
                        report.added++;
                    }
                    else report.replaced++;
                    target.transform.SetPositionAndRotation(source.transform.position, source.transform.rotation);
                    var scale = source.transform.lossyScale;
                    var parentScale = target.transform.parent.lossyScale;
                    target.transform.localScale = new Vector3(scale.x / parentScale.x, scale.y / parentScale.y, scale.z / parentScale.z);
                    target.GetComponent<MeshFilter>().sharedMesh = source.GetComponent<MeshFilter>().sharedMesh;
                    target.sharedMaterials = source.name == "CRT screen emission" ? new[] { oldScreenMaterial }
                        : source.sharedMaterials.Select(value => materials.TryGetValue(value.name, out var material)
                            ? material : throw new InvalidOperationException("Unmapped apartment material: " + value.name)).ToArray();
                    target.gameObject.SetActive(true);
                    target.enabled = true;
                    foreach (var collider in target.GetComponents<MeshCollider>())
                    {
                        collider.sharedMesh = target.GetComponent<MeshFilter>().sharedMesh;
                        collider.enabled = true;
                    }
                    if (!existing.ContainsKey(source.name) && SolidProp(target))
                        target.gameObject.AddComponent<MeshCollider>().sharedMesh = target.GetComponent<MeshFilter>().sharedMesh;
                    Record(target.gameObject);
                }
                foreach (var old in existing.Values.Where(value => !seen.Contains(value.name) && !DoorWalls.Contains(value.name)))
                {
                    old.enabled = false;
                    foreach (var collider in old.GetComponents<Collider>()) collider.enabled = false;
                    Record(old.gameObject);
                    report.disabled++;
                }
                Object.DestroyImmediate(incoming);
                incoming = null;
                var roomRenderers = room.GetComponentsInChildren<MeshRenderer>(true).ToDictionary(value => value.name);
                foreach (var name in DoorWalls)
                {
                    roomRenderers[name].sharedMaterials = roomRenderers["B_Wall__3_"].sharedMaterials;
                    Record(roomRenderers[name].gameObject);
                }
                UpdateCollision(scene, roomRenderers);
                UpdateTerminal(scene, room, roomRenderers);
                UpdateLighting(scene, roomRenderers);
                report.excludedOverlappingPosters = RemovePosterOverlaps(scene, roomRenderers.Values);
                Physics.SyncTransforms();
                report.blockedAnchors = BlockedAnchors(scene);
                if (report.blockedAnchors.Length != 0)
                    throw new InvalidOperationException("Apartment blocks gameplay anchors: " + string.Join("; ", report.blockedAnchors));
                report.missingMaterials = roomRenderers.Values.Where(value => value.enabled)
                    .Sum(value => value.sharedMaterials.Count(material => material == null || material.shader == null || !material.shader.isSupported));
                if (report.missingMaterials != 0) throw new InvalidOperationException("Apartment has missing or unsupported materials.");
                report.corridorAfter = ProtectedSignature(scene);
                report.corridorPreserved = report.corridorBefore == report.corridorAfter;
                if (!report.corridorPreserved) throw new InvalidOperationException("Protected corridor or doorway changed.");
                AssetDatabase.SaveAssets();
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                Directory.CreateDirectory("../Artifacts/EmployeeApartmentImport");
                File.WriteAllText("../Artifacts/EmployeeApartmentImport/unity-import.json", JsonUtility.ToJson(report, true));
                Undo.CollapseUndoOperations(undoGroup);
                Selection.activeGameObject = room;
                Debug.Log("EMPLOYEE_APARTMENT_IMPORTED " + JsonUtility.ToJson(report));
            }
            catch
            {
                if (incoming != null) Object.DestroyImmediate(incoming);
                Undo.RevertAllDownToGroup(undoGroup);
                throw;
            }
        }

        private static T[] Components<T>(Scene scene) where T : Component
            => scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<T>(true)).ToArray();

        private static bool SolidProp(Renderer renderer)
        {
            var size = renderer.bounds.size;
            return Mathf.Min(size.x, size.y, size.z) > .06f && size.y > .25f
                && !renderer.name.StartsWith("B_ExteriorConcrete_", StringComparison.Ordinal);
        }

        private static void UpdateCollision(Scene scene, Dictionary<string, MeshRenderer> room)
        {
            var collisionRoot = Components<Transform>(scene).Single(value => value.name == "Room collision");
            foreach (var collider in collisionRoot.GetComponentsInChildren<BoxCollider>(true))
            {
                string name = collider.name.Replace(" collider", "");
                if (!room.TryGetValue(name, out var surface)) continue;
                collider.enabled = surface.enabled;
                collider.transform.position = surface.bounds.center;
                collider.transform.rotation = Quaternion.identity;
                collider.transform.localScale = Vector3.one;
                collider.center = Vector3.zero;
                collider.size = Vector3.Max(surface.bounds.size, Vector3.one * .025f);
            }
        }

        private static void UpdateTerminal(Scene scene, GameObject room, Dictionary<string, MeshRenderer> renderers)
        {
            var screen = renderers["CRT screen emission"];
            var monitor = renderers["CRT_Monitor_Tripo"];
            var targets = Components<WorldInteraction>(scene);
            var computer = targets.Single(value => value.Target == InteractionTarget.Computer);
            var bounds = monitor.bounds;
            bounds.Encapsulate(screen.bounds); bounds.Expand(.02f);
            var collider = computer.GetComponent<BoxCollider>();
            collider.transform.position = bounds.center; collider.center = Vector3.zero; collider.size = bounds.size;
            computer.Configure(computer.Target, computer.Label, monitor, computer.InteractionDistance);
            var transforms = Components<Transform>(scene);
            // Exported screen normals and full-range UVs describe the new monitor face.
            var mesh = screen.GetComponent<MeshFilter>().sharedMesh;
            var normal = screen.transform.TransformDirection(mesh.normals.Aggregate(Vector3.zero, (sum, item) => sum + item).normalized);
            if (normal.sqrMagnitude < .5f) throw new InvalidOperationException("New CRT has no usable outward normal.");
            var towardRoom = transforms.Single(value => value.name == "Computer approach").position - screen.bounds.center;
            if (Vector3.Dot(normal, towardRoom) < 0) normal = -normal;
            var rotation = Quaternion.LookRotation(-normal, Vector3.up);
            foreach (var name in new[] { "Terminal screen alignment", "Terminal camera destination" })
                transforms.Single(value => value.name == name).SetPositionAndRotation(screen.bounds.center + normal * .62f, rotation);
            var seated = transforms.Single(value => value.name == "Terminal seated eye");
            seated.SetPositionAndRotation(screen.bounds.center + normal * 1.05f + Vector3.up * .18f,
                Quaternion.LookRotation(screen.bounds.center - (screen.bounds.center + normal * 1.05f + Vector3.up * .18f)));
            room.transform.Find("Unity_ScreenCentre").position = screen.bounds.center;
            Components<PresentationEffects>(scene).Single().ConfigureTerminalSurface(screen);
        }

        private static void UpdateLighting(Scene scene, Dictionary<string, MeshRenderer> room)
        {
            var lighting = Components<RoomLightingController>(scene).Single();
            if (lighting.Rug != null) lighting.Rug.gameObject.SetActive(false);
            var serialized = new SerializedObject(lighting);
            // This model contains an authored rug; the generated unit quad must not cover it or rescale it.
            serialized.FindProperty("rug").objectReferenceValue = null;
            var surfaces = room.Values.Where(value => value.enabled && value.name != "CRT screen emission").Cast<Renderer>().ToList();
            foreach (var surface in Components<MeshRenderer>(scene).Where(value => value.name.StartsWith("Solid B_", StringComparison.Ordinal)))
            {
                if (room.TryGetValue(surface.name.Substring("Solid ".Length), out var original)) surface.sharedMaterials = original.sharedMaterials;
                surfaces.Add(surface);
            }
            var array = serialized.FindProperty("roomSurfaces");
            array.arraySize = surfaces.Count;
            for (int i = 0; i < surfaces.Count; i++) array.GetArrayElementAtIndex(i).objectReferenceValue = surfaces[i];
            serialized.ApplyModifiedPropertiesWithoutUndo();
            lighting.ApplyNow();
        }

        private static string[] RemovePosterOverlaps(Scene scene, IEnumerable<MeshRenderer> room)
        {
            var readables = Components<ReadableProp>(scene).SelectMany(value => value.GetComponentsInChildren<Renderer>(true)).ToArray();
            var removed = new List<string>();
            foreach (var poster in room.Where(value => Regex.IsMatch(value.name, @"^B_平面\.0(?:1[5-9]|2[0-6])$")))
            {
                var bounds = poster.bounds; bounds.Expand(.06f);
                if (!readables.Any(readable => bounds.Intersects(readable.bounds))) continue;
                poster.enabled = false;
                Record(poster.gameObject);
                removed.Add(poster.name);
            }
            return removed.ToArray();
        }

        private static string[] BlockedAnchors(Scene scene)
        {
            var player = Components<ApartmentPlayer>(scene).Single();
            var controller = player.GetComponent<CharacterController>();
            bool enabled = controller.enabled; controller.enabled = false;
            try
            {
                return Components<Transform>(scene).Where(value => new[] { "Player spawn", "Computer approach", "Bed approach", "Door approach" }.Contains(value.name))
                    .SelectMany(anchor => Physics.OverlapCapsule(anchor.position + Vector3.up * .25f, anchor.position + Vector3.up * 1.5f,
                        .23f, ~0, QueryTriggerInteraction.Ignore).Select(collider => anchor.name + ": " + collider.name)).ToArray();
            }
            finally { controller.enabled = enabled; }
        }

        private static string ProtectedSignature(Scene scene)
        {
            var objects = Components<Transform>(scene).Where(value => ProtectedRoots.Contains(value.name))
                .SelectMany(value => value.GetComponentsInChildren<Component>(true)).OrderBy(value => value.GetInstanceID());
            string text = string.Join("\n", objects.Select(value => EditorJsonUtility.ToJson(value)));
            using var sha = SHA256.Create();
            return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(text))).Replace("-", "").ToLowerInvariant();
        }

        private static void Record(GameObject item)
        {
            foreach (var component in item.GetComponents<Component>())
            {
                EditorUtility.SetDirty(component);
                PrefabUtility.RecordPrefabInstancePropertyModifications(component);
            }
            PrefabUtility.RecordPrefabInstancePropertyModifications(item);
        }
    }
}
