using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace MiniGame.UnityHost.Editor
{
    /// <summary>Projects only the reference wallpaper onto the interior wall faces; never edits the imported model.</summary>
    public static class WallSurfaceRepairSetup
    {
        private const string ModelPath = "Assets/MiniGame/Art/MainRoom/Combined/MainRoom.fbx";
        private const string GeneratedPath = "Assets/MiniGame/Generated/WallSurfaceRepair";
        private const string PaperGuid = "b223d03b1ea901649a40aa3feb9a4393";
        private const float Inset = .004f, OverlapDepthStep = .001f, MaxDistance = .25f, PlaneTolerance = .0001f;
        private static readonly HashSet<string> WallGuids = new HashSet<string>
        { "436d601d590a14540a9d4d2d009f34fc", "79d1ba8d74fa9864e88442f5ead77834" };
        private static readonly Dictionary<int, SourceIdentity> TemporarySources = new Dictionary<int, SourceIdentity>();

        [Serializable] public sealed class Report
        {
            public string scene, sourceModel = ModelPath, sourceModelSha256Before, sourceModelSha256After;
            public bool persisted, sourceModelUnchanged;
            public float interiorOffsetMetres = Inset;
            public float overlapDepthStepMetres = OverlapDepthStep;
            public Vector3 floorCentre;
            public int repairedRenderers;
            public List<SurfaceReport> surfaces = new List<SurfaceReport>();
        }

        [Serializable] public sealed class SurfaceReport
        {
            public string rendererPath, sourceMeshName, sourceMeshAssetPath, sourceMeshGuid, outputMeshAssetPath;
            public long sourceMeshLocalId;
            public string wallPath, wallMaterialGuid, shadowCasting;
            public Vector3 inwardNormal;
            public float wallPlaneCoordinate, targetPlaneCoordinate, nearestPlaneDistance, overlapArea;
            public float gapBeforeMin, gapBeforeMax, gapAfterMin, gapAfterMax, minimumNormalDot;
            public int vertices, changedVertices, originalTriangles, removedDegenerateTriangles, newlyDegenerateTriangles;
            public int remainingTriangles, flippedTriangles, candidateWalls;
            public int overlapLayer;
            public List<string> overlappingPapers = new List<string>();
            public bool uvPreserved, tangentCoordinatesPreserved, normalsConsistent, tangentsFinite, receiveShadows;
            public bool materialsUnchanged;
        }

        [Serializable] private sealed class SourceIdentity
        {
            public string kind, assetPath, guid, meshName;
            public long localId;
        }

        private sealed class Wall
        {
            public MeshRenderer Renderer;
            public int Axis;
            public float Plane;
            public Vector3 Inward;
            public string MaterialGuid;
        }

        private sealed class Candidate
        {
            public Wall Wall;
            public float Distance, Overlap;
        }

        private sealed class Plan
        {
            public MeshFilter Filter;
            public MeshRenderer Renderer;
            public Mesh Mesh, Assigned;
            public Material[] Materials;
            public int[] Queues;
            public SourceIdentity Source;
            public SurfaceReport Report;
        }

        public static Report Apply(Scene scene, bool persist)
        {
            if (!scene.IsValid() || !scene.isLoaded) throw new InvalidOperationException("A loaded scene is required.");
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Wall repair requires Edit Mode.");
            var renderers = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<MeshRenderer>(true)).ToArray();
            var papers = renderers.Where(renderer => renderer.sharedMaterials.Any(IsPaperMaterial)).ToArray();
            if (papers.Length != 12 || papers.Select(renderer => renderer.name).Distinct().Count() != 12
                || papers.Any(renderer => !ExpectedPaperName(renderer.name)))
                throw new InvalidOperationException("Expected the twelve unique reference wallpaper renderers; found: "
                    + string.Join(", ", papers.Select(renderer => PathOf(renderer.transform))));
            var floors = renderers.Where(renderer => renderer.name == "B_Floor").ToArray();
            if (floors.Length != 1) throw new InvalidOperationException("The unique B_Floor renderer is required.");
            Vector3 floorCentre = floors[0].bounds.center;
            var walls = renderers.Select(renderer => MakeWall(renderer, floorCentre)).Where(wall => wall != null).ToArray();
            if (walls.Length == 0) throw new InvalidOperationException("No eligible interior concrete wall shells were found.");
            var report = new Report { scene = scene.path, persisted = persist, floorCentre = floorCentre,
                sourceModelSha256Before = ModelHash() };
            var plans = new List<Plan>();
            try
            {
                // Resolve every mapping and validate every output before modifying any renderer or saving assets.
                foreach (var paper in papers.OrderBy(renderer => renderer.name, StringComparer.Ordinal)) plans.Add(BuildPlan(paper, walls));
                SeparateOverlappingPapers(plans);
                if (persist)
                {
                    PreflightPersist(plans);
                    EnsureFolder(GeneratedPath);
                    foreach (var plan in plans) Persist(plan);
                }
                foreach (var plan in plans)
                {
                    Mesh previous = plan.Filter.sharedMesh;
                    plan.Filter.sharedMesh = persist ? plan.Assigned : plan.Mesh;
                    if (!persist) TemporarySources[plan.Mesh.GetInstanceID()] = plan.Source;
                    plan.Renderer.shadowCastingMode = ShadowCastingMode.Off;
                    plan.Renderer.receiveShadows = true;
                    plan.Report.shadowCasting = plan.Renderer.shadowCastingMode.ToString();
                    plan.Report.receiveShadows = plan.Renderer.receiveShadows;
                    plan.Report.materialsUnchanged = plan.Renderer.sharedMaterials.SequenceEqual(plan.Materials)
                        && plan.Renderer.sharedMaterials.Select(material => material.renderQueue).SequenceEqual(plan.Queues);
                    if (!plan.Report.materialsUnchanged) throw new InvalidOperationException("Wallpaper material changed unexpectedly.");
                    if (persist)
                    {
                        Record(plan.Filter); Record(plan.Renderer);
                    }
                    if (previous != null && TemporarySources.Remove(previous.GetInstanceID())) Object.DestroyImmediate(previous);
                    report.surfaces.Add(plan.Report);
                }
                if (persist) EditorSceneManager.MarkSceneDirty(scene);
                report.repairedRenderers = plans.Count;
                report.sourceModelSha256After = ModelHash();
                report.sourceModelUnchanged = report.sourceModelSha256Before == report.sourceModelSha256After;
                if (!report.sourceModelUnchanged) throw new InvalidOperationException("Source FBX changed during the repair.");
                return report;
            }
            catch
            {
                foreach (var plan in plans)
                    if (plan.Mesh != null && plan.Filter.sharedMesh != plan.Mesh) Object.DestroyImmediate(plan.Mesh);
                throw;
            }
            finally
            {
                if (persist)
                    foreach (var plan in plans) if (plan.Mesh != null) Object.DestroyImmediate(plan.Mesh);
            }
        }

        [MenuItem("MiniGame/Main Room/Install wallpaper surface repair")]
        public static void Install()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Exit Play Mode before repairing wallpaper.");
            var scene = SceneManager.GetActiveScene();
            if (scene.path != MainRoomPreviewBuilder.CombinedScenePath)
                throw new InvalidOperationException("Open MainRoomCombined before installing the wallpaper repair. The menu does not switch scenes or discard unsaved changes.");
            var report = Apply(scene, true);
            if (!EditorSceneManager.SaveScene(scene)) throw new IOException("Could not save the repaired main room scene.");
            string root = Environment.GetEnvironmentVariable("MINIGAME_REPOSITORY_ROOT");
            if (string.IsNullOrEmpty(root)) root = Path.GetFullPath(Path.Combine(Application.dataPath, "../.."));
            string folder = Path.Combine(root, "Artifacts", "WallLighting"); Directory.CreateDirectory(folder);
            File.WriteAllText(Path.Combine(folder, "wall-surface-repair.json"), JsonUtility.ToJson(report, true));
            Debug.Log("MINIGAME_WALL_SURFACE_REPAIRED " + JsonUtility.ToJson(report));
        }

        public static void InstallFromCommandLine()
        {
            if (!Application.isBatchMode)
                throw new InvalidOperationException("InstallFromCommandLine is available only in Unity batch mode.");
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Exit Play Mode before repairing wallpaper.");
            if (SceneManager.GetActiveScene().path != MainRoomPreviewBuilder.CombinedScenePath)
                EditorSceneManager.OpenScene(MainRoomPreviewBuilder.CombinedScenePath, OpenSceneMode.Single);
            Install();
        }

        private static Plan BuildPlan(MeshRenderer renderer, Wall[] walls)
        {
            if (renderer.sharedMaterials.Length != 1 || !IsPaperMaterial(renderer.sharedMaterial))
                throw new InvalidOperationException("Wallpaper must have one unmodified reference material: " + PathOf(renderer.transform));
            var filter = renderer.GetComponent<MeshFilter>();
            if (filter == null || filter.sharedMesh == null) throw new InvalidOperationException("Missing wallpaper mesh: " + renderer.name);
            SourceIdentity identity = ResolveIdentity(filter.sharedMesh);
            Mesh source = LoadSource(identity);
            if (!source.isReadable) throw new InvalidOperationException("Source wallpaper mesh is not readable: " + source.name);
            var local = source.vertices;
            if (local.Length < 3) throw new InvalidOperationException("Wallpaper source has too few vertices.");
            var world = local.Select(vertex => filter.transform.TransformPoint(vertex)).ToArray();
            if (world.Any(vertex => !Finite(vertex))) throw new InvalidOperationException("Wallpaper contains non-finite positions.");
            Vector3 sum = Vector3.zero;
            for (int sub = 0; sub < source.subMeshCount; sub++)
            {
                if (source.GetTopology(sub) != MeshTopology.Triangles) throw new InvalidOperationException("Wallpaper must contain triangles only.");
                var indices = source.GetIndices(sub);
                for (int i = 0; i < indices.Length; i += 3)
                    sum += Vector3.Cross(world[indices[i + 1]] - world[indices[i]], world[indices[i + 2]] - world[indices[i]]);
            }
            if (sum.sqrMagnitude < 1e-10f) throw new InvalidOperationException("Wallpaper plane normal is ambiguous: " + renderer.name);
            Vector3 normal = sum.normalized;
            int axis = Mathf.Abs(normal.x) >= Mathf.Abs(normal.z) ? 0 : 2;
            if (Mathf.Abs(normal[axis]) < .9f) throw new InvalidOperationException("Wallpaper is not aligned with a vertical wall: " + renderer.name);
            var bounds = new Bounds(world[0], Vector3.zero);
            foreach (Vector3 vertex in world) bounds.Encapsulate(vertex);
            var coordinates = world.Select(vertex => vertex[axis]).OrderBy(value => value).ToArray();
            float median = coordinates[coordinates.Length / 2];
            int tangentAxis = axis == 0 ? 2 : 0;
            var candidates = walls.Where(wall => wall.Axis == axis).Select(wall => new Candidate
            {
                Wall = wall, Distance = Mathf.Abs(median - wall.Plane),
                Overlap = Overlap(bounds.min.y, bounds.max.y, wall.Renderer.bounds.min.y, wall.Renderer.bounds.max.y)
                    * Overlap(bounds.min[tangentAxis], bounds.max[tangentAxis], wall.Renderer.bounds.min[tangentAxis], wall.Renderer.bounds.max[tangentAxis])
            }).Where(candidate => candidate.Distance < MaxDistance && candidate.Overlap > .0001f
                && Mathf.Max(Mathf.Abs(bounds.min[axis] - candidate.Wall.Plane), Mathf.Abs(bounds.max[axis] - candidate.Wall.Plane)) < MaxDistance)
                .OrderBy(candidate => candidate.Distance).ThenByDescending(candidate => candidate.Overlap).ToArray();
            if (candidates.Length == 0) throw new InvalidOperationException("No nearby overlapping wall for " + renderer.name);
            Candidate best = candidates[0];
            var samePlane = candidates.Where(candidate => Mathf.Abs(candidate.Wall.Plane - best.Wall.Plane) <= PlaneTolerance
                && Vector3.Dot(candidate.Wall.Inward, best.Wall.Inward) > .999f).OrderByDescending(candidate => candidate.Overlap).ToArray();
            best = samePlane[0];
            if (samePlane.Length > 1 && Mathf.Abs(samePlane[0].Overlap - samePlane[1].Overlap) < .0001f)
                throw new InvalidOperationException("Equally overlapping wall owners for " + renderer.name);
            if (candidates.Any(candidate => Mathf.Abs(candidate.Wall.Plane - best.Wall.Plane) > PlaneTolerance
                && Mathf.Abs(candidate.Distance - best.Distance) < .001f))
                throw new InvalidOperationException("Two different wall planes are equally near " + renderer.name);
            var detail = new SurfaceReport { rendererPath = PathOf(renderer.transform), sourceMeshName = source.name,
                sourceMeshAssetPath = identity.assetPath, sourceMeshGuid = identity.guid, sourceMeshLocalId = identity.localId,
                wallPath = PathOf(best.Wall.Renderer.transform), wallMaterialGuid = best.Wall.MaterialGuid,
                inwardNormal = best.Wall.Inward, wallPlaneCoordinate = best.Wall.Plane,
                targetPlaneCoordinate = best.Wall.Plane + best.Wall.Inward[axis] * Inset,
                nearestPlaneDistance = best.Distance, overlapArea = best.Overlap, candidateWalls = candidates.Length, vertices = local.Length };
            var projected = (Vector3[])world.Clone();
            detail.gapBeforeMin = float.PositiveInfinity; detail.gapBeforeMax = float.NegativeInfinity;
            for (int i = 0; i < projected.Length; i++)
            {
                float gap = (world[i][axis] - best.Wall.Plane) * best.Wall.Inward[axis];
                detail.gapBeforeMin = Mathf.Min(detail.gapBeforeMin, gap); detail.gapBeforeMax = Mathf.Max(detail.gapBeforeMax, gap);
                projected[i][axis] = detail.targetPlaneCoordinate;
                if (Vector3.SqrMagnitude(projected[i] - world[i]) > 1e-14f) detail.changedVertices++;
            }
            Mesh output = Object.Instantiate(source);
            output.name = renderer.name + " interior wallpaper"; output.hideFlags = HideFlags.HideAndDontSave;
            try
            {
                output.vertices = projected.Select(vertex => filter.transform.InverseTransformPoint(vertex)).ToArray();
                for (int sub = 0; sub < source.subMeshCount; sub++)
                {
                    var indices = source.GetIndices(sub); var kept = new List<int>(indices.Length);
                    detail.originalTriangles += indices.Length / 3;
                    for (int i = 0; i < indices.Length; i += 3)
                    {
                        int a = indices[i], b = indices[i + 1], c = indices[i + 2];
                        Vector3 area = Vector3.Cross(projected[b] - projected[a], projected[c] - projected[a]);
                        if (area.sqrMagnitude <= 1e-14f)
                        {
                            detail.removedDegenerateTriangles++;
                            if (Vector3.Cross(world[b] - world[a], world[c] - world[a]).sqrMagnitude > 1e-14f) detail.newlyDegenerateTriangles++;
                            continue;
                        }
                        if (Vector3.Dot(area, best.Wall.Inward) < 0) { int swap = b; b = c; c = swap; detail.flippedTriangles++; }
                        kept.Add(a); kept.Add(b); kept.Add(c);
                    }
                    output.SetIndices(kept.ToArray(), MeshTopology.Triangles, sub, false);
                    detail.remainingTriangles += kept.Count / 3;
                }
                if (detail.remainingTriangles == 0) throw new InvalidOperationException("Projection removed every triangle of " + renderer.name);
                output.RecalculateBounds(); output.RecalculateNormals();
                // Canonical planar normals also cover vertices orphaned by collapsed folds.
                Vector3 localNormal = filter.transform.localToWorldMatrix.transpose.MultiplyVector(best.Wall.Inward).normalized;
                output.normals = Enumerable.Repeat(localNormal, output.vertexCount).ToArray();
                var uv = new List<Vector4>(); source.GetUVs(0, uv);
                if (uv.Count != output.vertexCount) throw new InvalidOperationException("Wallpaper requires a complete UV0 channel.");
                output.RecalculateTangents();
                detail.uvPreserved = SameUvs(source, output);
                detail.tangentsFinite = output.tangents.Length == output.vertexCount && output.tangents.All(Finite);
                detail.minimumNormalDot = output.normals.Min(value => Vector3.Dot(filter.transform.worldToLocalMatrix.transpose.MultiplyVector(value).normalized, best.Wall.Inward));
                detail.normalsConsistent = detail.minimumNormalDot > .9999f;
                detail.tangentCoordinatesPreserved = true;
                detail.gapAfterMin = float.PositiveInfinity; detail.gapAfterMax = float.NegativeInfinity;
                var actual = output.vertices;
                for (int i = 0; i < actual.Length; i++)
                {
                    Vector3 point = filter.transform.TransformPoint(actual[i]);
                    if (Mathf.Abs(point.y - world[i].y) > .00002f || Mathf.Abs(point[tangentAxis] - world[i][tangentAxis]) > .00002f)
                        detail.tangentCoordinatesPreserved = false;
                    float gap = (point[axis] - best.Wall.Plane) * best.Wall.Inward[axis];
                    detail.gapAfterMin = Mathf.Min(detail.gapAfterMin, gap); detail.gapAfterMax = Mathf.Max(detail.gapAfterMax, gap);
                }
                if (!detail.uvPreserved || !detail.normalsConsistent || !detail.tangentsFinite || !detail.tangentCoordinatesPreserved
                    || Mathf.Abs(detail.gapAfterMin - Inset) > .00002f || Mathf.Abs(detail.gapAfterMax - Inset) > .00002f)
                    throw new InvalidOperationException("Projected wallpaper failed geometry validation: " + renderer.name);
                return new Plan { Filter = filter, Renderer = renderer, Mesh = output, Source = identity, Report = detail,
                    Materials = renderer.sharedMaterials, Queues = renderer.sharedMaterials.Select(material => material.renderQueue).ToArray() };
            }
            catch { Object.DestroyImmediate(output); throw; }
        }

        private static void SeparateOverlappingPapers(IReadOnlyList<Plan> plans)
        {
            // Adjacent source sheets overlap by millimetres. Flattening all of them onto one
            // plane causes depth ties and duplicate ForwardAdd lighting that flickers with view angle.
            var bounds = plans.Select(plan =>
            {
                var points = plan.Mesh.vertices.Select(plan.Filter.transform.TransformPoint).ToArray();
                var result = new Bounds(points[0], Vector3.zero);
                foreach (var point in points) result.Encapsulate(point);
                return result;
            }).ToArray();
            for (int index = 0; index < plans.Count; index++)
            {
                var plan = plans[index];
                int axis = Mathf.Abs(plan.Report.inwardNormal.x) > .9f ? 0 : 2;
                int tangent = axis == 0 ? 2 : 0;
                var occupiedLayers = new HashSet<int>();
                for (int other = 0; other < plans.Count; other++)
                {
                    if (other == index || Vector3.Dot(plan.Report.inwardNormal, plans[other].Report.inwardNormal) < .999f
                        || Mathf.Abs(plan.Report.wallPlaneCoordinate - plans[other].Report.wallPlaneCoordinate) > PlaneTolerance
                        || Overlap(bounds[index].min.y, bounds[index].max.y, bounds[other].min.y, bounds[other].max.y) <= .00001f
                        || Overlap(bounds[index].min[tangent], bounds[index].max[tangent], bounds[other].min[tangent], bounds[other].max[tangent]) <= .00001f)
                        continue;
                    plan.Report.overlappingPapers.Add(plans[other].Report.rendererPath);
                    if (other < index) occupiedLayers.Add(plans[other].Report.overlapLayer);
                }
                int layer = 0;
                while (occupiedLayers.Contains(layer)) layer++;
                plan.Report.overlapLayer = layer;
                float offset = layer * OverlapDepthStep;
                if (offset == 0) continue;
                Vector3 shift = plan.Report.inwardNormal * offset;
                plan.Mesh.vertices = plan.Mesh.vertices.Select(vertex => plan.Filter.transform.InverseTransformPoint(
                    plan.Filter.transform.TransformPoint(vertex) + shift)).ToArray();
                plan.Mesh.RecalculateBounds();
                plan.Report.targetPlaneCoordinate += shift[axis];
                plan.Report.gapAfterMin += offset;
                plan.Report.gapAfterMax += offset;
            }
        }

        private static Wall MakeWall(MeshRenderer renderer, Vector3 floorCentre)
        {
            if (!renderer.name.StartsWith("B_", StringComparison.Ordinal) || renderer.name.IndexOf("wall", StringComparison.OrdinalIgnoreCase) < 0
                || renderer.name.IndexOf("Exterior", StringComparison.OrdinalIgnoreCase) >= 0) return null;
            var filter = renderer.GetComponent<MeshFilter>();
            if (filter == null || filter.sharedMesh == null || AssetDatabase.GetAssetPath(filter.sharedMesh) != ModelPath) return null;
            string guid = renderer.sharedMaterials.Select(MaterialGuid).FirstOrDefault(value => WallGuids.Contains(value));
            if (guid == null) return null;
            var bounds = renderer.bounds;
            int axis = bounds.size.x <= bounds.size.z ? 0 : 2, tangent = axis == 0 ? 2 : 0;
            if (bounds.size.y < .3f || bounds.size[axis] > .35f || bounds.size[tangent] < .1f) return null;
            float sign = floorCentre[axis] >= bounds.center[axis] ? 1 : -1;
            float plane = sign > 0 ? bounds.max[axis] : bounds.min[axis];
            if ((floorCentre[axis] - plane) * sign < .25f) return null;
            return new Wall { Renderer = renderer, Axis = axis, Plane = plane, Inward = Axis(axis) * sign, MaterialGuid = guid };
        }

        private static SourceIdentity ResolveIdentity(Mesh current)
        {
            string path = AssetDatabase.GetAssetPath(current);
            if (path == ModelPath)
            {
                if (!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(current, out string guid, out long id))
                    throw new InvalidOperationException("Could not identify the original wallpaper mesh.");
                return new SourceIdentity { kind = "MiniGameWallSurfaceSource", assetPath = path, guid = guid, localId = id, meshName = current.name };
            }
            if (TemporarySources.TryGetValue(current.GetInstanceID(), out var temporary)) return temporary;
            if (path.StartsWith(GeneratedPath + "/", StringComparison.Ordinal))
            {
                var importer = AssetImporter.GetAtPath(path);
                var identity = importer != null ? JsonUtility.FromJson<SourceIdentity>(importer.userData) : null;
                if (identity != null && identity.kind == "MiniGameWallSurfaceSource" && identity.assetPath == ModelPath) return identity;
            }
            throw new InvalidOperationException("Wallpaper has no reliable original FBX identity: " + current.name + " / " + path);
        }

        private static Mesh LoadSource(SourceIdentity identity)
        {
            if (identity.assetPath != ModelPath || AssetDatabase.AssetPathToGUID(ModelPath) != identity.guid)
                throw new InvalidOperationException("Wallpaper source identity does not refer to the reference FBX.");
            var matches = AssetDatabase.LoadAllAssetsAtPath(ModelPath).OfType<Mesh>().Where(mesh =>
                AssetDatabase.TryGetGUIDAndLocalFileIdentifier(mesh, out string guid, out long id) && guid == identity.guid && id == identity.localId).ToArray();
            if (matches.Length != 1) throw new InvalidOperationException("Could not resolve one original wallpaper mesh: " + identity.meshName);
            return matches[0];
        }

        private static string OutputPath(Plan plan)
        {
            string name = new string(plan.Renderer.name.Select(character => char.IsLetterOrDigit(character) || character == '_' ? character : '_').ToArray());
            return GeneratedPath + "/" + name + "_" + plan.Source.localId + ".asset";
        }

        private static void PreflightPersist(IReadOnlyList<Plan> plans)
        {
            var paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var plan in plans)
            {
                string path = OutputPath(plan);
                if (!paths.Add(path)) throw new InvalidOperationException("Two wallpaper outputs target the same asset: " + path);
                ValidateDestination(path, plan.Source);
            }
        }

        private static Mesh ValidateDestination(string path, SourceIdentity expected)
        {
            Object asset = AssetDatabase.LoadMainAssetAtPath(path);
            string absolute = Path.GetFullPath(Path.Combine(Application.dataPath, "..", path));
            bool fileExists = File.Exists(absolute);
            if (asset == null)
            {
                if (fileExists || Directory.Exists(absolute) || File.Exists(absolute + ".meta")
                    || !string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(path)))
                    throw new InvalidOperationException("Wallpaper output path is occupied by an unimported, invalid or reserved asset: " + path);
                return null;
            }
            if (!(asset is Mesh mesh) || !fileExists)
                throw new InvalidOperationException("Wallpaper output path is occupied by a non-Mesh or missing asset: " + path);
            var importer = AssetImporter.GetAtPath(path);
            if (importer == null || string.IsNullOrWhiteSpace(importer.userData))
                throw new InvalidOperationException("Refusing to overwrite a mesh without wallpaper repair source metadata: " + path);
            SourceIdentity identity;
            try { identity = JsonUtility.FromJson<SourceIdentity>(importer.userData); }
            catch (ArgumentException exception)
            { throw new InvalidOperationException("Invalid wallpaper repair source metadata: " + path, exception); }
            if (identity == null || identity.kind != "MiniGameWallSurfaceSource"
                || identity.assetPath != expected.assetPath || identity.guid != expected.guid || identity.localId != expected.localId)
                throw new InvalidOperationException("Refusing to overwrite a mesh owned by another source: " + path);
            return mesh;
        }

        private static void Persist(Plan plan)
        {
            string path = OutputPath(plan);
            // Recheck immediately before each write, after the complete batch preflight.
            var mesh = ValidateDestination(path, plan.Source);
            if (mesh == null)
            {
                mesh = Object.Instantiate(plan.Mesh); mesh.hideFlags = HideFlags.None;
                AssetDatabase.CreateAsset(mesh, path);
            }
            else { EditorUtility.CopySerialized(plan.Mesh, mesh); mesh.hideFlags = HideFlags.None; EditorUtility.SetDirty(mesh); }
            // CopySerialized can leave an already-rendered Mesh using its old GPU vertex buffer.
            mesh.vertices = plan.Mesh.vertices;
            mesh.RecalculateBounds();
            mesh.UploadMeshData(false);
            AssetDatabase.SaveAssetIfDirty(mesh);
            var importer = AssetImporter.GetAtPath(path);
            if (importer == null) throw new InvalidOperationException("Generated mesh importer is missing: " + path);
            importer.userData = JsonUtility.ToJson(plan.Source); importer.SaveAndReimport();
            plan.Assigned = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (plan.Assigned == null) throw new InvalidOperationException("Generated mesh failed to reload: " + path);
            plan.Report.outputMeshAssetPath = path;
        }

        private static bool SameUvs(Mesh first, Mesh second)
        {
            for (int channel = 0; channel < 8; channel++)
            {
                var before = new List<Vector4>(); var after = new List<Vector4>();
                first.GetUVs(channel, before); second.GetUVs(channel, after);
                if (!before.SequenceEqual(after)) return false;
            }
            return true;
        }

        private static bool ExpectedPaperName(string name) => name == "B_平面"
            || Enumerable.Range(1, 11).Any(index => name == "B_平面." + index.ToString("000"));
        private static bool IsPaperMaterial(Material material) => material != null && MaterialGuid(material) == PaperGuid
            && (material.name == "MainRoom_30_B" || material.name == "MainRoom_30");
        private static string MaterialGuid(Material material) => material == null ? "" : AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(material));
        private static Vector3 Axis(int axis) => axis == 0 ? Vector3.right : Vector3.forward;
        private static float Overlap(float aMin, float aMax, float bMin, float bMax) => Mathf.Max(0, Mathf.Min(aMax, bMax) - Mathf.Max(aMin, bMin));
        private static bool Finite(Vector3 value) => Finite(value.x) && Finite(value.y) && Finite(value.z);
        private static bool Finite(Vector4 value) => Finite(value.x) && Finite(value.y) && Finite(value.z) && Finite(value.w);
        private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
        private static string PathOf(Transform transform) => transform.parent == null ? transform.name : PathOf(transform.parent) + "/" + transform.name;
        private static void Record(Object value)
        {
            EditorUtility.SetDirty(value);
            if (PrefabUtility.IsPartOfPrefabInstance(value)) PrefabUtility.RecordPrefabInstancePropertyModifications(value);
        }
        private static void EnsureFolder(string path)
        {
            string current = "Assets";
            foreach (string part in path.Split('/').Skip(1))
            {
                string next = current + "/" + part;
                if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(current, part);
                current = next;
            }
        }
        private static string ModelHash()
        {
            using var hash = SHA256.Create();
            using var file = File.OpenRead(Path.GetFullPath(Path.Combine(Application.dataPath, "..", ModelPath)));
            return BitConverter.ToString(hash.ComputeHash(file)).Replace("-", "").ToLowerInvariant();
        }
    }
}
