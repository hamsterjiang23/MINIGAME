using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using MiniGame.UnityHost.Editor;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace MiniGame.UnityHost.Tests
{
    public sealed class WallSurfaceRepairTests
    {
        private const string ScenePath = "Assets/MiniGame/Scenes/MainRoomCombined.unity";
        private const string ModelPath = "Assets/MiniGame/Art/MainRoom/Combined/MainRoom.fbx";
        private const string GeneratedPath = "Assets/MiniGame/Generated/WallSurfaceRepair";

        [Test]
        public void PreviewRepairsAllTwelveActualSurfacesWithoutChangingOtherMeshesCollidersOrMaterials()
        {
            WithCombined(scene =>
            {
                var renderers = Components<MeshRenderer>(scene).ToDictionary(renderer => PathOf(renderer.transform));
                var untouchedMeshes = Components<MeshFilter>(scene).Where(filter => !IsPaperName(filter.name))
                    .ToDictionary(filter => filter, filter => filter.sharedMesh);
                var colliderStates = Components<Collider>(scene).ToDictionary(collider => collider, collider => EditorJsonUtility.ToJson(collider));
                var paperMaterials = renderers.Values.Where(renderer => IsPaperName(renderer.name))
                    .ToDictionary(renderer => renderer, renderer => renderer.sharedMaterials);
                var materialStates = paperMaterials.Values.SelectMany(materials => materials).Distinct()
                    .ToDictionary(material => material, material => EditorJsonUtility.ToJson(material));
                string modelBefore = FileHash(ModelPath), sceneBefore = FileHash(ScenePath);
                var generatedBefore = GeneratedFiles();
                var report = WallSurfaceRepairSetup.Apply(scene, false);
                Assert.That(report.persisted, Is.False);
                Assert.That(report.repairedRenderers, Is.EqualTo(12));
                Assert.That(report.surfaces.Count, Is.EqualTo(12));
                Assert.That(report.sourceModelUnchanged, Is.True);
                Assert.That(report.sourceModelSha256Before, Is.EqualTo(modelBefore));
                Assert.That(report.sourceModelSha256After, Is.EqualTo(modelBefore));

                Vector3 floorCentre = renderers.Values.Single(renderer => renderer.name == "B_Floor").bounds.center;
                foreach (var surface in report.surfaces)
                {
                    var renderer = renderers[surface.rendererPath];
                    var wall = renderers[surface.wallPath];
                    var filter = renderer.GetComponent<MeshFilter>();
                    var mesh = filter.sharedMesh;
                    Assert.That(wall.name, Is.EqualTo(ExpectedWall(renderer.name)), renderer.name);
                    Assert.That(AssetDatabase.GetAssetPath(mesh), Is.Empty, "Preview output must remain a temporary mesh.");
                    Assert.That(surface.sourceMeshAssetPath, Is.EqualTo(ModelPath));
                    Assert.That(surface.sourceMeshGuid, Is.EqualTo(AssetDatabase.AssetPathToGUID(ModelPath)));
                    Mesh source = OriginalMesh(surface.sourceMeshLocalId);
                    Assert.That(source.name, Is.EqualTo(surface.sourceMeshName));
                    Assert.That(mesh.vertexCount, Is.EqualTo(source.vertexCount));
                    Assert.That(surface.uvPreserved && surface.normalsConsistent && surface.tangentsFinite, Is.True);
                    Assert.That(surface.tangentCoordinatesPreserved && surface.materialsUnchanged, Is.True);
                    Assert.That(renderer.sharedMaterials, Is.EqualTo(paperMaterials[renderer]));
                    Assert.That(renderer.shadowCastingMode, Is.EqualTo(ShadowCastingMode.Off));
                    Assert.That(renderer.receiveShadows, Is.True);
                    AssertUvsEqual(source, mesh);

                    int axis = Mathf.Abs(surface.inwardNormal.x) > .9f ? 0 : 2, tangent = axis == 0 ? 2 : 0;
                    Vector3 inward = axis == 0 ? Vector3.right : Vector3.forward;
                    inward *= floorCentre[axis] > wall.bounds.center[axis] ? 1 : -1;
                    float plane = inward[axis] > 0 ? wall.bounds.max[axis] : wall.bounds.min[axis];
                    Assert.That(Vector3.Dot(surface.inwardNormal, inward), Is.GreaterThan(.9999f));
                    var vertices = mesh.vertices; var originalVertices = source.vertices; var normals = mesh.normals;
                    Assert.That(normals.Length, Is.EqualTo(vertices.Length));
                    for (int index = 0; index < vertices.Length; index++)
                    {
                        Vector3 point = filter.transform.TransformPoint(vertices[index]);
                        Vector3 original = filter.transform.TransformPoint(originalVertices[index]);
                        Assert.That((point[axis] - plane) * inward[axis],
                            Is.EqualTo(report.interiorOffsetMetres + surface.overlapLayer * report.overlapDepthStepMetres).Within(.00002f), renderer.name);
                        Assert.That(point.y, Is.EqualTo(original.y).Within(.00002f));
                        Assert.That(point[tangent], Is.EqualTo(original[tangent]).Within(.00002f));
                        Vector3 worldNormal = filter.transform.worldToLocalMatrix.transpose.MultiplyVector(normals[index]).normalized;
                        Assert.That(Vector3.Dot(worldNormal, inward), Is.GreaterThan(.9999f));
                    }
                    int triangles = 0;
                    for (int sub = 0; sub < mesh.subMeshCount; sub++)
                    {
                        var indices = mesh.GetIndices(sub); triangles += indices.Length / 3;
                        for (int index = 0; index < indices.Length; index += 3)
                        {
                            Vector3 a = filter.transform.TransformPoint(vertices[indices[index]]);
                            Vector3 b = filter.transform.TransformPoint(vertices[indices[index + 1]]);
                            Vector3 c = filter.transform.TransformPoint(vertices[indices[index + 2]]);
                            Assert.That(Vector3.Dot(Vector3.Cross(b - a, c - a), inward), Is.GreaterThan(0),
                                "Every retained triangle must have nonzero area and face the room.");
                        }
                    }
                    Assert.That(triangles, Is.EqualTo(surface.remainingTriangles));
                    Assert.That(surface.originalTriangles - surface.remainingTriangles, Is.EqualTo(surface.removedDegenerateTriangles));
                }
                foreach (var pair in untouchedMeshes) Assert.That(pair.Key.sharedMesh, Is.SameAs(pair.Value), pair.Key.name);
                Assert.That(Components<Collider>(scene), Is.EquivalentTo(colliderStates.Keys));
                foreach (var pair in colliderStates) Assert.That(EditorJsonUtility.ToJson(pair.Key), Is.EqualTo(pair.Value), pair.Key.name);
                foreach (var pair in materialStates) Assert.That(EditorJsonUtility.ToJson(pair.Key), Is.EqualTo(pair.Value), pair.Key.name);
                Assert.That(FileHash(ModelPath), Is.EqualTo(modelBefore));
                Assert.That(FileHash(ScenePath), Is.EqualTo(sceneBefore));
                Assert.That(GeneratedFiles(), Is.EquivalentTo(generatedBefore));
            });
        }

        [Test]
        public void SavedOverlappingWallpapersHaveDistinctDepth()
        {
            WithCombined(scene =>
            {
                var papers = Components<MeshRenderer>(scene).Where(renderer => IsPaperName(renderer.name)).ToArray();
                int overlaps = 0;
                foreach (var paper in papers)
                {
                    var mesh = paper.GetComponent<MeshFilter>().sharedMesh;
                    Assert.That(AssetDatabase.GetAssetPath(mesh), Does.StartWith(GeneratedPath + "/"));
                    Vector3 normal = paper.transform.worldToLocalMatrix.transpose.MultiplyVector(mesh.normals[0]).normalized;
                    int axis = Mathf.Abs(normal.x) > .9f ? 0 : 2, tangent = axis == 0 ? 2 : 0;
                    float plane = paper.transform.TransformPoint(mesh.vertices[0])[axis];
                    foreach (var other in papers.Where(other => string.CompareOrdinal(other.name, paper.name) > 0))
                    {
                        var otherMesh = other.GetComponent<MeshFilter>().sharedMesh;
                        var otherNormal = other.transform.worldToLocalMatrix.transpose.MultiplyVector(otherMesh.normals[0]).normalized;
                        float otherPlane = other.transform.TransformPoint(otherMesh.vertices[0])[axis];
                        if (Vector3.Dot(normal, otherNormal) < .999f || Mathf.Abs(plane - otherPlane) > .02f
                            || Mathf.Min(paper.bounds.max.y, other.bounds.max.y) - Mathf.Max(paper.bounds.min.y, other.bounds.min.y) <= .00001f
                            || Mathf.Min(paper.bounds.max[tangent], other.bounds.max[tangent]) - Mathf.Max(paper.bounds.min[tangent], other.bounds.min[tangent]) <= .00001f)
                            continue;
                        overlaps++;
                        Assert.That(Mathf.Abs(plane - otherPlane), Is.GreaterThan(.0005f),
                            paper.name + " and " + other.name + " must not compete at the same depth in their overlap.");
                    }
                }
                Assert.That(overlaps, Is.GreaterThan(0), "The actual source wallpaper must exercise overlapping neighbouring sheets.");
            });
        }

        [Test]
        public void RepeatedPreviewKeepsIdenticalGeometryAndOriginalFbxIdentityWithoutWritingAssets()
        {
            WithCombined(scene =>
            {
                string sceneBefore = FileHash(ScenePath), modelBefore = FileHash(ModelPath);
                var generatedBefore = GeneratedFiles();
                var first = WallSurfaceRepairSetup.Apply(scene, false);
                var renderers = Components<MeshRenderer>(scene).ToDictionary(renderer => PathOf(renderer.transform));
                var vertices = first.surfaces.ToDictionary(surface => surface.rendererPath,
                    surface => renderers[surface.rendererPath].GetComponent<MeshFilter>().sharedMesh.vertices);
                var normals = first.surfaces.ToDictionary(surface => surface.rendererPath,
                    surface => renderers[surface.rendererPath].GetComponent<MeshFilter>().sharedMesh.normals);
                var tangents = first.surfaces.ToDictionary(surface => surface.rendererPath,
                    surface => renderers[surface.rendererPath].GetComponent<MeshFilter>().sharedMesh.tangents);
                var triangles = first.surfaces.ToDictionary(surface => surface.rendererPath,
                    surface => renderers[surface.rendererPath].GetComponent<MeshFilter>().sharedMesh.triangles);
                var second = WallSurfaceRepairSetup.Apply(scene, false);
                Assert.That(second.surfaces.Count, Is.EqualTo(12));
                foreach (var surface in second.surfaces)
                {
                    var previous = first.surfaces.Single(item => item.rendererPath == surface.rendererPath);
                    var mesh = renderers[surface.rendererPath].GetComponent<MeshFilter>().sharedMesh;
                    Assert.That(mesh.vertices, Is.EqualTo(vertices[surface.rendererPath]), surface.rendererPath);
                    Assert.That(mesh.normals, Is.EqualTo(normals[surface.rendererPath]));
                    Assert.That(mesh.tangents, Is.EqualTo(tangents[surface.rendererPath]));
                    Assert.That(mesh.triangles, Is.EqualTo(triangles[surface.rendererPath]));
                    Assert.That(surface.sourceMeshGuid, Is.EqualTo(previous.sourceMeshGuid));
                    Assert.That(surface.sourceMeshLocalId, Is.EqualTo(previous.sourceMeshLocalId));
                    Assert.That(surface.sourceMeshAssetPath, Is.EqualTo(ModelPath));
                    Assert.That(surface.wallPath, Is.EqualTo(previous.wallPath));
                    Assert.That(surface.gapBeforeMin, Is.EqualTo(previous.gapBeforeMin));
                    Assert.That(surface.gapBeforeMax, Is.EqualTo(previous.gapBeforeMax));
                    Assert.That(surface.gapAfterMin, Is.EqualTo(previous.gapAfterMin));
                    Assert.That(surface.gapAfterMax, Is.EqualTo(previous.gapAfterMax));
                    Assert.That(surface.uvPreserved && surface.normalsConsistent, Is.True);
                    AssertUvsEqual(OriginalMesh(surface.sourceMeshLocalId), mesh);
                }
                Assert.That(FileHash(ModelPath), Is.EqualTo(modelBefore));
                Assert.That(FileHash(ScenePath), Is.EqualTo(sceneBefore));
                Assert.That(GeneratedFiles(), Is.EquivalentTo(generatedBefore));
            });
        }

        [Test]
        public void InstallFromAnUnrelatedDirtyScenePreservesTheSceneAndUnsavedObjects()
        {
            Scene previous = SceneManager.GetActiveScene();
            Scene scene = previous;
            bool createdScene = false;
            if (scene.path == ScenePath)
            {
                var alternatives = Enumerable.Range(0, SceneManager.sceneCount).Select(SceneManager.GetSceneAt)
                    .Where(candidate => candidate.isLoaded && candidate.path != ScenePath).ToArray();
                if (alternatives.Length > 0) scene = alternatives[0];
                else
                {
                    scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
                    createdScene = true;
                }
            }
            // Reuse NUnit's untitled scene; creating another additive scene would require saving it first.
            SceneManager.SetActiveScene(scene);
            var marker = new GameObject("Unsaved user edit");
            SceneManager.MoveGameObjectToScene(marker, scene);
            marker.transform.position = new Vector3(12, 34, 56);
            EditorSceneManager.MarkSceneDirty(scene);
            int sceneCount = SceneManager.sceneCount, handle = scene.handle;
            string initialPath = scene.path;
            try
            {
                var exception = Assert.Throws<InvalidOperationException>(() => WallSurfaceRepairSetup.Install());
                Assert.That(exception.Message, Does.Contain("Open MainRoomCombined"));
                Assert.That(SceneManager.GetActiveScene().handle, Is.EqualTo(handle));
                Assert.That(SceneManager.sceneCount, Is.EqualTo(sceneCount));
                Assert.That(scene.isLoaded && scene.isDirty, Is.True);
                Assert.That(scene.path, Is.EqualTo(initialPath), "The scene must not be saved or replaced.");
                Assert.That(marker, Is.Not.Null);
                Assert.That(marker.scene.handle, Is.EqualTo(handle));
                Assert.That(marker.transform.position, Is.EqualTo(new Vector3(12, 34, 56)));
                Assert.That(scene.GetRootGameObjects(), Does.Contain(marker));
            }
            finally
            {
                if (marker != null) Object.DestroyImmediate(marker);
                if (previous.IsValid() && previous.isLoaded) SceneManager.SetActiveScene(previous);
                if (createdScene && scene.IsValid() && scene.isLoaded) EditorSceneManager.CloseScene(scene, true);
            }
        }

        private static void WithCombined(Action<Scene> check)
        {
            Scene previous = SceneManager.GetActiveScene();
            Scene scene = SceneManager.GetSceneByPath(ScenePath), safety = default;
            bool previouslyLoaded = scene.IsValid() && scene.isLoaded;
            if (previouslyLoaded && scene.isDirty)
                throw new InvalidOperationException("Run wall repair tests with a clean MainRoomCombined in the isolated project.");
            if (!previouslyLoaded) scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            var initialMeshes = new HashSet<Mesh>(Components<MeshFilter>(scene).Select(filter => filter.sharedMesh));
            var temporary = new HashSet<Mesh>();
            try
            {
                SceneManager.SetActiveScene(scene);
                check(scene);
            }
            finally
            {
                foreach (var filter in Components<MeshFilter>(scene))
                    if (filter.sharedMesh != null && !initialMeshes.Contains(filter.sharedMesh) && !AssetDatabase.Contains(filter.sharedMesh))
                        temporary.Add(filter.sharedMesh);
                if (SceneManager.sceneCount == 1) safety = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
                if (scene.IsValid() && scene.isLoaded) EditorSceneManager.CloseScene(scene, true);
                foreach (var mesh in temporary) if (mesh != null && !AssetDatabase.Contains(mesh)) Object.DestroyImmediate(mesh);
                Scene restored = previouslyLoaded ? EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive) : default;
                if (previouslyLoaded && previous.handle == scene.handle) SceneManager.SetActiveScene(restored);
                else if (previous.IsValid() && previous.isLoaded) SceneManager.SetActiveScene(previous);
                if (safety.IsValid() && safety.isLoaded && SceneManager.sceneCount > 1) EditorSceneManager.CloseScene(safety, true);
            }
        }

        private static T[] Components<T>(Scene scene) where T : Component => scene.GetRootGameObjects()
            .SelectMany(root => root.GetComponentsInChildren<T>(true)).ToArray();
        private static string PathOf(Transform transform) => transform.parent == null ? transform.name : PathOf(transform.parent) + "/" + transform.name;
        private static bool IsPaperName(string name) => name == "B_平面"
            || Enumerable.Range(1, 11).Any(index => name == "B_平面." + index.ToString("000"));
        private static string ExpectedWall(string paper)
        {
            int index = paper == "B_平面" ? 0 : int.Parse(paper.Substring("B_平面.".Length));
            return index >= 3 && index <= 6 ? "B_Wall__3_.001" : index == 7 || index == 8 || index == 11 ? "B_Wall__2_" : "B_Wall__3_";
        }
        private static Mesh OriginalMesh(long localId) => AssetDatabase.LoadAllAssetsAtPath(ModelPath).OfType<Mesh>().Single(mesh =>
            AssetDatabase.TryGetGUIDAndLocalFileIdentifier(mesh, out string guid, out long id) && id == localId);
        private static void AssertUvsEqual(Mesh source, Mesh actual)
        {
            for (int channel = 0; channel < 8; channel++)
            {
                var before = new List<Vector4>(); var after = new List<Vector4>();
                source.GetUVs(channel, before); actual.GetUVs(channel, after);
                Assert.That(after, Is.EqualTo(before), "UV channel " + channel + " must retain the original values.");
            }
        }
        private static string Absolute(string path) => Path.GetFullPath(Path.Combine(Application.dataPath, "..", path));
        private static string FileHash(string path)
        {
            using var hash = SHA256.Create(); using var file = File.OpenRead(Absolute(path));
            return BitConverter.ToString(hash.ComputeHash(file)).Replace("-", "").ToLowerInvariant();
        }
        private static Dictionary<string, string> GeneratedFiles() => Directory.Exists(Absolute(GeneratedPath))
            ? Directory.EnumerateFiles(Absolute(GeneratedPath), "*", SearchOption.AllDirectories)
                .ToDictionary(path => path, path => FileHash(path))
            : new Dictionary<string, string>();
    }
}
