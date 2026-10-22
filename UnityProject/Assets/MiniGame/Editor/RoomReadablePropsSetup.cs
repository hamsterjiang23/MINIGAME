using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace MiniGame.UnityHost.Editor
{
    /// <summary>Adds three original, readable paper props without rebuilding the imported room.</summary>
    public static class RoomReadablePropsSetup
    {
        public const string RootName = "Room readable documents";
        private const string Generated = "Assets/MiniGame/Generated/RoomReadables";

        private readonly struct Frame
        {
            public readonly Vector3 Origin, Right, Depth;
            public Frame(Transform[] nodes)
            {
                Origin = nodes.Single(item => item.name == "Unity_ReferenceOrigin").position;
                Right = nodes.Single(item => item.name == "Unity_ReferenceRight").position - Origin;
                Depth = nodes.Single(item => item.name == "Unity_ReferenceDepth").position - Origin;
                if (Mathf.Abs(Right.magnitude - 1) > .001f || Mathf.Abs(Depth.magnitude - 1) > .001f
                    || Mathf.Abs(Vector3.Dot(Right, Depth)) > .001f || Mathf.Abs(Right.y) > .001f || Mathf.Abs(Depth.y) > .001f)
                    throw new InvalidOperationException("The room must retain its horizontal metre-scale reference frame.");
            }

            public Vector3 Point(float x, float depth, float height) => Origin + Right * x + Depth * depth + Vector3.up * height;
            public float X(Vector3 point) => Vector3.Dot(point - Origin, Right);
            public float D(Vector3 point) => Vector3.Dot(point - Origin, Depth);
        }

        [MenuItem("MiniGame/Main Room/Install readable letter, newspaper and notice")]
        public static void InstallInOpenScene()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Exit Play Mode before installing the room documents.");
            var scene = SceneManager.GetActiveScene();
            if (scene.path != MainRoomGameplayBuilder.ScenePath)
                throw new InvalidOperationException("Open MainRoomCombined before installing room documents.");
            var nodes = scene.GetRootGameObjects().SelectMany(item => item.GetComponentsInChildren<Transform>(true)).ToArray();
            var frame = new Frame(nodes);
            var desk = nodes.Single(item => item.name == "B_obj_01").GetComponent<Renderer>();
            var monitor = nodes.Single(item => item.name.StartsWith("CRT_Monitor_Tripo", StringComparison.Ordinal)).GetComponent<Renderer>();
            var oldPosters = nodes.Select(item => item.GetComponent<ReadableProp>()).Where(item => item != null
                && (item.Id == "poster-beach" || item.Id == "poster-instrument")).ToArray();
            if (desk == null || monitor == null || oldPosters.Length != 2)
                throw new InvalidOperationException("The desk, CRT and both original readable posters must be present.");
            var letter = Document("FamilyLetter");
            var news = Document("DailyNewspaper");
            var notice = Document("EmploymentNotice");
            var lighting = nodes.Select(item => item.GetComponent<RoomLightingController>()).Single(item => item != null);
            var lightingObject = new SerializedObject(lighting);
            if (lightingObject.FindProperty("roomSurfaces") == null)
                throw new InvalidOperationException("The room lighting controller no longer exposes its surface bindings.");
            var cutout = Shader.Find("MiniGame/Room Readable Cutout");
            if (cutout == null) throw new InvalidOperationException("The room document cutout shader is unavailable.");
            Directory.CreateDirectory(Generated);
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            var letterInk = Material("Family letter print", cutout, Color.white, PrintedTexture("family-letter"));
            var newspaperInk = Material("Newspaper print", cutout, Color.white, PrintedTexture("daily-newspaper"));
            newspaperInk.SetFloat("_AlbedoGain", 1.2f);
            newspaperInk.SetFloat("_ReadableFill", .035f);
            var noticeInk = Material("Employment notice print", cutout, Color.white, PrintedTexture("employment-notice"));
            // The rear and sides use the same alpha silhouette so transparent holes never reveal an opaque backing rectangle.
            var paperEdge = Material("Letter thin edge", cutout, new Color(.72f, .72f, .72f), letterInk.mainTexture);
            var rackEdge = Material("Newspaper rack thin edge", cutout, new Color(.8f, .8f, .8f), newspaperInk.mainTexture, .05f);
            rackEdge.SetFloat("_ReadableFill", .025f);
            var frameEdge = Material("Notice frame thin edge", cutout, new Color(.62f, .62f, .62f), noticeInk.mainTexture);

            var root = new GameObject(RootName + " (installing)").transform;
            SceneManager.MoveGameObjectToScene(root.gameObject, scene);
            try
            {
                // Work from the existing geometry rather than an assumed imported world origin.
                float deskX = frame.X(desk.bounds.center);
                float deskD = frame.D(desk.bounds.center);
                float wallD = oldPosters.Average(item => frame.D(item.transform.position));
                float deskTop = desk.bounds.max.y - frame.Origin.y;
                // This front corner clears the keyboard, cigarette box and cup in the authored desk layout.
                var letterPose = frame.Point(deskX + .60f, deskD + .22f, deskTop + .002f);
                var family = CreateLetter(root, letterPose, Quaternion.LookRotation(Vector3.down, -frame.Depth)
                    * Quaternion.Euler(0, 0, -9), letterInk, paperEdge, letter);
                var wallRotation = oldPosters[0].transform.rotation;
                var inward = wallRotation * Vector3.back;
                var newspaper = CreateNewspaper(root, frame.Point(deskX - 1.55f, wallD, 1.47f), wallRotation, newspaperInk, rackEdge, news);
                var certificate = CreateNotice(root, frame.Point(deskX + .55f, wallD, 1.87f), wallRotation, noticeInk, frameEdge, notice);
                // Move the two desk-side standing points past the chair's left edge.
                AddApproach(root, "Family letter approach", family.Surface.bounds.center, inward, frame.Origin.y, frame.Right * .25f);
                AddApproach(root, "Newspaper approach", newspaper.Surface.bounds.center, inward, frame.Origin.y);
                AddApproach(root, "Employment notice approach", certificate.Surface.bounds.center, inward, frame.Origin.y, frame.Right * .30f);
                foreach (var previous in scene.GetRootGameObjects().Where(item => item.name == RootName).ToArray())
                    UnityEngine.Object.DestroyImmediate(previous);
                root.name = RootName;
                AppendLightingSurfaces(lightingObject, root);
                AssetDatabase.SaveAssets();
                Physics.SyncTransforms();
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                Debug.Log("MINIGAME_ROOM_READABLES_READY props=3 originalPosters=2 root=" + RootName);
            }
            catch
            {
                if (root != null) UnityEngine.Object.DestroyImmediate(root.gameObject);
                throw;
            }
        }

        private static RoomReadableDocument Document(string resource)
        {
            var document = Resources.Load<RoomReadableDocument>("MiniGame/Readables/" + resource);
            if (document == null) throw new InvalidOperationException("Missing room document resource: " + resource);
            return document;
        }

        private static void AppendLightingSurfaces(SerializedObject lighting, Transform root)
        {
            lighting.Update();
            var property = lighting.FindProperty("roomSurfaces");
            var surfaces = new List<Renderer>();
            for (int index = 0; index < property.arraySize; index++)
            {
                var renderer = property.GetArrayElementAtIndex(index).objectReferenceValue as Renderer;
                if (renderer != null && !surfaces.Contains(renderer)) surfaces.Add(renderer);
            }
            foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
                if (!surfaces.Contains(renderer)) surfaces.Add(renderer);
            property.arraySize = surfaces.Count;
            for (int index = 0; index < surfaces.Count; index++)
                property.GetArrayElementAtIndex(index).objectReferenceValue = surfaces[index];
            lighting.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(lighting.targetObject);
        }

        private static ReadableProp CreateLetter(Transform parent, Vector3 position, Quaternion rotation,
            Material print, Material edge, RoomReadableDocument document)
        {
            var root = Pose(parent, "Family letter - opened envelope", position, rotation);
            return Silhouette(root, "Opened family letter", .34f, .002f, new[]
            {
                new Vector2(750, 48), new Vector2(784, 48), new Vector2(1082, 273), new Vector2(1159, 272),
                new Vector2(1159, 338), new Vector2(1279, 432), new Vector2(1279, 951),
                new Vector2(256, 951), new Vector2(256, 432), new Vector2(374, 338),
                new Vector2(374, 272), new Vector2(450, 273)
            }, new Vector2(320, 650), print, edge, document);
        }

        private static ReadableProp CreateNewspaper(Transform parent, Vector3 position, Quaternion rotation,
            Material print, Material edge, RoomReadableDocument document)
        {
            var root = Pose(parent, "Daily newspaper - wall rack", position, rotation);
            return Silhouette(root, "Daily newspaper and rack", .46f, .02f, new[]
            {
                new Vector2(173, 48), new Vector2(1052, 48), new Vector2(1052, 237),
                new Vector2(1153, 237), new Vector2(1153, 1198), new Vector2(73, 1198),
                new Vector2(73, 237), new Vector2(173, 237)
            }, new Vector2(130, 600), print, edge, document);
        }

        private static ReadableProp CreateNotice(Transform parent, Vector3 position, Quaternion rotation,
            Material print, Material edge, RoomReadableDocument document)
        {
            var root = Pose(parent, "Employment notice - framed", position, rotation);
            return Silhouette(root, "Framed employment notice", .43f, .018f, new[]
            {
                new Vector2(35, 99), new Vector2(990, 99), new Vector2(990, 1422), new Vector2(35, 1422)
            }, new Vector2(60, 750), print, edge, document);
        }

        private static ReadableProp Silhouette(Transform root, string name, float width, float thickness,
            Vector2[] pixels, Vector2 edgePixel, Material print, Material edge, RoomReadableDocument document)
        {
            var texture = print.mainTexture;
            var minimum = pixels.Aggregate(Vector2.Min); var maximum = pixels.Aggregate(Vector2.Max);
            var center = (minimum + maximum) * .5f;
            float scale = width / (maximum.x - minimum.x);
            var points = pixels.Select(point => new Vector2(point.x - center.x, center.y - point.y) * scale).ToArray();
            var uv = pixels.Select(point => new Vector2(point.x / texture.width, 1 - point.y / texture.height)).ToArray();
            // Sample a pixel centre well inside the opaque paper/metal/frame; silhouette-border UVs are transparent.
            var edgeUv = new Vector2((edgePixel.x + .5f) / texture.width, 1 - (edgePixel.y + .5f) / texture.height);
            var triangles = Triangulate(points);
            var face = PersistMesh(name + " front", points.Select(point => new Vector3(point.x, point.y, 0)).ToArray(),
                points.Select(_ => Vector3.back).ToArray(), uv, triangles);
            var body = Extrusion(name + " thin body", points, uv, edgeUv, thickness, triangles, false);
            var collision = Extrusion(name + " collision", points, uv, edgeUv, thickness, triangles, true);
            var surface = MeshRenderer(root, name, face, print);
            MeshRenderer(root, name + " edge and back", body, edge);
            // These stationary, concave document shapes retain their silhouette instead of filling the empty corners with a box.
            var collider = root.gameObject.AddComponent<MeshCollider>(); collider.sharedMesh = collision;
            var readable = root.gameObject.AddComponent<ReadableProp>();
            readable.Configure(document.Id, "", surface, 2f);
            readable.ConfigureDocument(document);
            return readable;
        }

        private static Transform Pose(Transform parent, string name, Vector3 position, Quaternion rotation)
        {
            var root = new GameObject(name).transform; root.SetParent(parent, false); root.SetPositionAndRotation(position, rotation); return root;
        }

        private static void AddApproach(Transform parent, string name, Vector3 target, Vector3 inward, float floor, Vector3 lateral = default)
        {
            var position = target + inward * 1.1f + lateral; position.y = floor + .055f;
            Pose(parent, name, position, Quaternion.LookRotation(Vector3.ProjectOnPlane(target - position, Vector3.up), Vector3.up));
        }

        private static Renderer MeshRenderer(Transform parent, string name, Mesh mesh, Material material)
        {
            var item = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer)); item.transform.SetParent(parent, false);
            item.GetComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = item.GetComponent<MeshRenderer>(); renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.TwoSided;
            return renderer;
        }

        private static Mesh Extrusion(string name, Vector2[] points, Vector2[] sourceUv, Vector2 edgeUv, float thickness, int[] front, bool includeFront)
        {
            var vertices = new List<Vector3>(); var triangles = new List<int>();
            var uv = new List<Vector2>(); uv.AddRange(sourceUv); uv.AddRange(sourceUv);
            vertices.AddRange(points.Select(point => new Vector3(point.x, point.y, 0)));
            vertices.AddRange(points.Select(point => new Vector3(point.x, point.y, thickness)));
            if (includeFront) triangles.AddRange(front);
            for (int i = 0; i < front.Length; i += 3)
            {
                triangles.Add(front[i] + points.Length); triangles.Add(front[i + 2] + points.Length); triangles.Add(front[i + 1] + points.Length);
            }
            float area = SignedArea(points);
            for (int i = 0; i < points.Length; i++)
            {
                int next = (i + 1) % points.Length, start = vertices.Count;
                vertices.Add(new Vector3(points[i].x, points[i].y, 0)); vertices.Add(new Vector3(points[next].x, points[next].y, 0));
                vertices.Add(new Vector3(points[next].x, points[next].y, thickness)); vertices.Add(new Vector3(points[i].x, points[i].y, thickness));
                uv.AddRange(new[] { edgeUv, edgeUv, edgeUv, edgeUv });
                if (area > 0) triangles.AddRange(new[] { start, start + 1, start + 2, start, start + 2, start + 3 });
                else triangles.AddRange(new[] { start, start + 2, start + 1, start, start + 3, start + 2 });
            }
            return PersistMesh(name, vertices.ToArray(), null, uv.ToArray(), triangles.ToArray());
        }

        private static Mesh PersistMesh(string name, Vector3[] vertices, Vector3[] normals, Vector2[] uv, int[] triangles)
        {
            var path = Generated + "/" + name + ".asset";
            var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (mesh == null) { mesh = new Mesh { name = name }; AssetDatabase.CreateAsset(mesh, path); }
            mesh.Clear();
            mesh.vertices = vertices; mesh.triangles = triangles;
            if (normals != null) mesh.normals = normals; else mesh.RecalculateNormals();
            if (uv != null) { mesh.uv = uv; mesh.RecalculateTangents(); }
            mesh.RecalculateBounds(); EditorUtility.SetDirty(mesh); return mesh;
        }

        private static float SignedArea(Vector2[] points)
        {
            float area = 0;
            for (int i = 0; i < points.Length; i++)
            {
                var next = points[(i + 1) % points.Length]; area += points[i].x * next.y - next.x * points[i].y;
            }
            return area * .5f;
        }

        private static float Cross(Vector2 a, Vector2 b, Vector2 c) => (b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x);

        private static int[] Triangulate(Vector2[] points)
        {
            var remaining = Enumerable.Range(0, points.Length).ToList(); var triangles = new List<int>();
            float direction = Mathf.Sign(SignedArea(points)); const float epsilon = 1e-8f;
            while (remaining.Count > 2)
            {
                bool found = false;
                for (int i = 0; i < remaining.Count; i++)
                {
                    int a = remaining[(i + remaining.Count - 1) % remaining.Count], b = remaining[i], c = remaining[(i + 1) % remaining.Count];
                    if (Cross(points[a], points[b], points[c]) * direction <= epsilon) continue;
                    bool contains = remaining.Any(point => point != a && point != b && point != c
                        && Cross(points[a], points[b], points[point]) * direction >= -epsilon
                        && Cross(points[b], points[c], points[point]) * direction >= -epsilon
                        && Cross(points[c], points[a], points[point]) * direction >= -epsilon);
                    if (contains) continue;
                    triangles.Add(a); triangles.Add(direction < 0 ? b : c); triangles.Add(direction < 0 ? c : b);
                    remaining.RemoveAt(i); found = true; break;
                }
                if (!found) throw new InvalidOperationException("The authored paper silhouette is not a simple polygon.");
            }
            return triangles.ToArray();
        }

        private static Material Material(string name, Shader shader, Color color, Texture texture, float metallic = 0)
        {
            string path = Generated + "/" + name + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null) { material = new Material(shader) { name = name }; AssetDatabase.CreateAsset(material, path); }
            material.shader = shader;
            material.color = color; material.mainTexture = texture; material.SetFloat("_Metallic", metallic);
            material.SetFloat("_Cutoff", .5f);
            material.SetFloat("_Glossiness", .08f); material.SetFloat("_Cull", (float)CullMode.Off); material.SetColor("_EmissionColor", Color.black);
            EditorUtility.SetDirty(material); return material;
        }

        private static Texture2D PrintedTexture(string name)
        {
            string source = Path.GetFullPath(Path.Combine(Application.dataPath, "../../Art/RoomReadables/Textures/", name + ".png"));
            if (!File.Exists(source)) throw new FileNotFoundException("Restore the authored paper texture before installing room documents.", source);
            string path = Generated + "/" + name + ".png";
            File.Copy(source, path, true);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.sRGBTexture = true; importer.alphaSource = TextureImporterAlphaSource.FromInput;
            importer.alphaIsTransparency = true;
            importer.npotScale = TextureImporterNPOTScale.None; importer.maxTextureSize = 2048;
            importer.mipmapEnabled = true; importer.filterMode = FilterMode.Bilinear;
            importer.wrapMode = TextureWrapMode.Clamp; importer.anisoLevel = 4;
            importer.textureCompression = TextureImporterCompression.Uncompressed; importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

    }
}
