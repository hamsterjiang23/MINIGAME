using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MiniGame.UnityHost.Editor
{
    /// <summary>A continuous U-shaped casing and reveal, clear of the source wall's opening planes.</summary>
    public static class ReferenceDoorFrameBuilder
    {
        public const float ClearWidth = 1.45f;
        public const float ClearHeight = 2.475f;
        public const float LeafWidth = 1.43f;
        public const float LeafHeight = 2.45f;
        public const float HingeOutset = .055f;
        private const string MeshPath = "Assets/MiniGame/Generated/ReferenceRoom/Entrance frame.asset";

        public static void Build(Transform frame, Material material)
        {
            float inner = ClearWidth * .5f, outer = inner + .105f;
            float innerTop = ClearHeight, outerTop = innerTop + .13f;
            // Bury the casing feet into the floor instead of sharing the source wall's floor caps.
            const float front = -.135f, back = .135f, bottom = -.01f;
            var vertices = new List<Vector3>();
            var normals = new List<Vector3>();
            var uv = new List<Vector2>();
            var triangles = new List<int>();
            // The posts end at the header: front/back regions share edges, never an area.
            foreach (float z in new[] { front, back })
            {
                Vector3 normal = z < 0 ? Vector3.back : Vector3.forward;
                Quad(vertices, normals, uv, triangles, new Vector3(-outer, bottom, z), new Vector3(-inner, bottom, z),
                    new Vector3(-inner, innerTop, z), new Vector3(-outer, innerTop, z), normal);
                Quad(vertices, normals, uv, triangles, new Vector3(inner, bottom, z), new Vector3(outer, bottom, z),
                    new Vector3(outer, innerTop, z), new Vector3(inner, innerTop, z), normal);
                Quad(vertices, normals, uv, triangles, new Vector3(-outer, innerTop, z), new Vector3(outer, innerTop, z),
                    new Vector3(outer, outerTop, z), new Vector3(-outer, outerTop, z), normal);
            }
            foreach (float side in new[] { -1f, 1f })
            {
                float x = side * inner;
                Quad(vertices, normals, uv, triangles, new Vector3(x, bottom, front), new Vector3(x, bottom, back),
                    new Vector3(x, innerTop, back), new Vector3(x, innerTop, front), Vector3.right * -side);
                x = side * outer;
                Quad(vertices, normals, uv, triangles, new Vector3(x, bottom, front), new Vector3(x, bottom, back),
                    new Vector3(x, outerTop, back), new Vector3(x, outerTop, front), Vector3.right * side);
                float low = side < 0 ? -outer : inner, high = side < 0 ? -inner : outer;
                Quad(vertices, normals, uv, triangles, new Vector3(low, bottom, front), new Vector3(high, bottom, front),
                    new Vector3(high, bottom, back), new Vector3(low, bottom, back), Vector3.down);
            }
            Quad(vertices, normals, uv, triangles, new Vector3(-inner, innerTop, front), new Vector3(inner, innerTop, front),
                new Vector3(inner, innerTop, back), new Vector3(-inner, innerTop, back), Vector3.down);
            Quad(vertices, normals, uv, triangles, new Vector3(-outer, outerTop, front), new Vector3(outer, outerTop, front),
                new Vector3(outer, outerTop, back), new Vector3(-outer, outerTop, back), Vector3.up);
            var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(MeshPath);
            if (mesh == null) { mesh = new Mesh { name = "Entrance frame continuous casing" }; AssetDatabase.CreateAsset(mesh, MeshPath); }
            mesh.Clear(); mesh.SetVertices(vertices); mesh.SetNormals(normals); mesh.SetUVs(0, uv); mesh.SetTriangles(triangles, 0); mesh.RecalculateBounds();
            EditorUtility.SetDirty(mesh);
            frame.gameObject.AddComponent<MeshFilter>().sharedMesh = mesh;
            frame.gameObject.AddComponent<MeshRenderer>().sharedMaterial = material;
            frame.gameObject.AddComponent<MeshCollider>().sharedMesh = mesh;
        }

        private static void Quad(List<Vector3> vertices, List<Vector3> normals, List<Vector2> uv, List<int> triangles,
            Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector3 normal)
        {
            if (Vector3.Dot(Vector3.Cross(b - a, c - a), normal) < 0) { var swap = b; b = d; d = swap; }
            int start = vertices.Count;
            vertices.AddRange(new[] { a, b, c, d });
            normals.AddRange(new[] { normal, normal, normal, normal });
            uv.AddRange(new[] { Vector2.zero, Vector2.right, Vector2.one, Vector2.up });
            triangles.AddRange(new[] { start, start + 1, start + 2, start, start + 2, start + 3 });
        }
    }
}
