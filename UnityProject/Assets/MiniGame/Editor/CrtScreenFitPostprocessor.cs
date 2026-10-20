using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace MiniGame.UnityHost.Editor
{
    /// <summary>Conforms the existing phosphor mesh without replacing the model, renderer or scene references.</summary>
    public sealed class CrtScreenFitPostprocessor : AssetPostprocessor
    {
        internal const string ModelPath = "Assets/MiniGame/Art/MainRoom/Combined/MainRoom.fbx";
        internal const string DataPath = "Assets/MiniGame/Resources/MiniGame/CrtScreenFit.json";
        public override uint GetVersion() => 2;

        private void OnPreprocessModel()
        {
            if (assetPath == ModelPath) ((ModelImporter)assetImporter).isReadable = true;
        }

        [Serializable] private sealed class Frame { public Vector3 origin, right, up, depth; }
        [Serializable] private sealed class Range { public Vector3 min, max; }
        [Serializable] private sealed class Fit
        {
            public float[] vertices, normals, uv;
            public int[] triangles;
            public Frame sourceFrame;
            public Range originalMonitorWorldBounds;
        }

        private void OnPostprocessModel(GameObject model)
        {
            if (assetPath != ModelPath) return;
            context.DependsOnSourceAsset(DataPath);
            string path = Path.Combine(Application.dataPath, "MiniGame/Resources/MiniGame/CrtScreenFit.json");
            if (!File.Exists(path)) throw new InvalidOperationException("Missing calibrated CRT surface: " + DataPath);
            Apply(model, JsonUtility.FromJson<Fit>(File.ReadAllText(path)));
        }

        private static Matrix4x4 Basis(Frame frame)
        {
            var result = Matrix4x4.identity;
            result.SetColumn(0, (Vector4)(frame.right - frame.origin));
            result.SetColumn(1, (Vector4)(frame.up - frame.origin));
            result.SetColumn(2, (Vector4)(frame.depth - frame.origin));
            result.SetColumn(3, new Vector4(frame.origin.x, frame.origin.y, frame.origin.z, 1));
            return result;
        }

        private static Bounds TransformBounds(Bounds bounds, Matrix4x4 matrix)
        {
            var result = new Bounds(matrix.MultiplyPoint3x4(bounds.center), Vector3.zero);
            for (int x = -1; x <= 1; x += 2) for (int y = -1; y <= 1; y += 2) for (int z = -1; z <= 1; z += 2)
                result.Encapsulate(matrix.MultiplyPoint3x4(bounds.center + Vector3.Scale(bounds.extents, new Vector3(x, y, z))));
            return result;
        }

        private static void Apply(GameObject model, Fit fit)
        {
            if (fit?.sourceFrame == null || fit.originalMonitorWorldBounds == null || fit.vertices == null
                || fit.vertices.Length % 3 != 0 || fit.normals?.Length != fit.vertices.Length
                || fit.uv?.Length != fit.vertices.Length / 3 * 2 || fit.triangles == null || fit.triangles.Length % 3 != 0)
                throw new InvalidDataException("Invalid calibrated CRT mesh data.");
            var nodes = model.GetComponentsInChildren<Transform>(true);
            Vector3 Marker(string name) => nodes.Single(node => node.name == "Unity_" + name).position;
            var target = new Frame { origin = Marker("ReferenceOrigin"), right = Marker("ReferenceRight"),
                up = Marker("ReferenceUp"), depth = Marker("ReferenceDepth") };
            Matrix4x4 world = Basis(target) * Basis(fit.sourceFrame).inverse;
            var monitor = model.GetComponentsInChildren<MeshRenderer>(true)
                .Single(renderer => renderer.name.StartsWith("CRT_Monitor_Tripo", StringComparison.Ordinal));
            var screen = model.GetComponentsInChildren<MeshFilter>(true)
                .Single(filter => filter.name.StartsWith("CRT screen emission", StringComparison.Ordinal));
            var expected = new Bounds(world.MultiplyPoint3x4(fit.originalMonitorWorldBounds.min), Vector3.zero);
            for (int x = 0; x < 2; x++) for (int y = 0; y < 2; y++) for (int z = 0; z < 2; z++)
                expected.Encapsulate(world.MultiplyPoint3x4(new Vector3(
                    x == 0 ? fit.originalMonitorWorldBounds.min.x : fit.originalMonitorWorldBounds.max.x,
                    y == 0 ? fit.originalMonitorWorldBounds.min.y : fit.originalMonitorWorldBounds.max.y,
                    z == 0 ? fit.originalMonitorWorldBounds.min.z : fit.originalMonitorWorldBounds.max.z)));
            Bounds actual = TransformBounds(monitor.GetComponent<MeshFilter>().sharedMesh.bounds, monitor.transform.localToWorldMatrix);
            float error = Vector3.Distance(expected.center, actual.center) + Vector3.Distance(expected.size, actual.size);
            if (error > .003f) throw new InvalidDataException("CRT calibration no longer matches the actual monitor: " + error + "m");

            Matrix4x4 local = screen.transform.worldToLocalMatrix * world;
            Matrix4x4 normalMap = local.inverse.transpose;
            var vertices = new Vector3[fit.vertices.Length / 3];
            var normals = new Vector3[vertices.Length];
            var uv = new Vector2[vertices.Length];
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = local.MultiplyPoint3x4(new Vector3(fit.vertices[i * 3], fit.vertices[i * 3 + 1], fit.vertices[i * 3 + 2]));
                normals[i] = normalMap.MultiplyVector(new Vector3(fit.normals[i * 3], fit.normals[i * 3 + 1], fit.normals[i * 3 + 2])).normalized;
                uv[i] = new Vector2(fit.uv[i * 2], fit.uv[i * 2 + 1]);
            }
            var triangles = (int[])fit.triangles.Clone();
            for (int i = 0; i < triangles.Length; i += 3)
            {
                int a = triangles[i], b = triangles[i + 1], c = triangles[i + 2];
                if ((uint)a >= vertices.Length || (uint)b >= vertices.Length || (uint)c >= vertices.Length)
                    throw new InvalidDataException("CRT triangle index outside calibrated vertices.");
                if (Vector3.Dot(Vector3.Cross(vertices[b] - vertices[a], vertices[c] - vertices[a]), normals[a] + normals[b] + normals[c]) < 0)
                { triangles[i + 1] = c; triangles[i + 2] = b; }
            }
            // Edit the imported subasset in place so Unity preserves its file ID and every scene binding.
            var mesh = screen.sharedMesh;
            mesh.Clear();
            mesh.indexFormat = vertices.Length > 65535 ? IndexFormat.UInt32 : IndexFormat.UInt16;
            mesh.vertices = vertices; mesh.normals = normals; mesh.uv = uv; mesh.triangles = triangles;
            mesh.RecalculateBounds(); mesh.RecalculateTangents();
            if (mesh.vertexCount != vertices.Length) throw new InvalidOperationException("CRT import did not retain the calibrated geometry.");
            Debug.Log("MINIGAME_CRT_SURFACE_FITTED vertices=" + vertices.Length + " triangles=" + triangles.Length / 3 + " monitorBoundsError=" + error);
        }
    }
}
