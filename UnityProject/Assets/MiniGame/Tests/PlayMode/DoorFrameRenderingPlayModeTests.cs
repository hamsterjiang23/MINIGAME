using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MiniGame.Presentation;
using NUnit.Framework;
using Unity.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace MiniGame.UnityHost.Tests
{
    public sealed class DoorFrameRenderingPlayModeTests
    {
        private const float PlaneTolerance = .0001f;
        private const float OverlapAreaTolerance = .000001f;

        [UnityTest, Timeout(15000)]
        public IEnumerator ClosedDoorJambAndThresholdSealTheLightGapsWithoutFillingTheOpening()
        {
            yield return SceneManager.LoadSceneAsync("MainRoomCombined");
            yield return null;
            var frame = GameObject.Find("Entrance frame").transform;
            var solid = frame.GetComponent<MeshCollider>();
            Physics.SyncTransforms();
            foreach (var point in new[] { new Vector2(-.72f, 1.2f), new Vector2(.72f, 1.2f),
                new Vector2(0, 2.47f), new Vector2(0, .008f) })
            {
                var ray = new Ray(frame.TransformPoint(new Vector3(point.x, point.y, -.25f)), frame.forward);
                Assert.That(solid.Raycast(ray, out _, .5f), Is.True,
                    "The solid door stop must occlude the closed leaf gap at " + point);
            }
            Assert.That(solid.Raycast(new Ray(frame.TransformPoint(new Vector3(0, 1.2f, -.25f)), frame.forward), out _, .5f),
                Is.False, "Door stops must retain the actual doorway opening.");
        }

        [UnityTest, Timeout(90000)]
        public IEnumerator DoorFrameHasNoPositiveAreaCoplanarOverlapWithItselfOrEntranceWalls()
        {
            yield return SceneManager.LoadSceneAsync("MainRoomCombined");
            yield return null;
            var frame = GameObject.Find("Entrance frame");
            Assert.That(frame, Is.Not.Null);
            var frameTriangles = Triangles(frame.GetComponentsInChildren<MeshFilter>());
            var wallFilters = Object.FindObjectsOfType<MeshFilter>().Where(filter =>
                filter.name.StartsWith("B_ReferenceDoorWall", StringComparison.Ordinal)
                || filter.name.StartsWith("Entrance wall upper", StringComparison.Ordinal)
                || filter.name.StartsWith("Entrance wall skirt", StringComparison.Ordinal)
                || filter.name == "Entrance lintel").ToArray();
            Assert.That(wallFilters, Is.Not.Empty, "The audit must include the real imported reveals and corridor entrance walls.");
            var wallTriangles = Triangles(wallFilters);
            Assert.That(frameTriangles, Is.Not.Empty);
            Assert.That(wallTriangles, Is.Not.Empty);
            var overlaps = new List<string>();
            for (int first = 0; first < frameTriangles.Count; first++)
            {
                for (int second = first + 1; second < frameTriangles.Count; second++)
                    AddOverlap(frameTriangles[first], frameTriangles[second], overlaps);
                foreach (var wall in wallTriangles) AddOverlap(frameTriangles[first], wall, overlaps);
            }
            Debug.Log("MINIGAME_DOOR_FRAME_COPLANAR_AUDIT frameTriangles=" + frameTriangles.Count
                + " wallTriangles=" + wallTriangles.Count + " overlaps=" + overlaps.Count
                + (overlaps.Count == 0 ? string.Empty : "\n" + string.Join("\n", overlaps)));
            Assert.That(overlaps, Is.Empty, "Coplanar triangles with a shared visible area can alternate depth ownership as the viewpoint changes:\n"
                + string.Join("\n", overlaps));
        }

        [UnityTest, Timeout(90000)]
        public IEnumerator OpenDoorKeepsFrameFixedAndClearAcrossConfiguredAnglesAndRoomSideViews()
        {
            yield return SceneManager.LoadSceneAsync("MainRoomCombined");
            yield return null;
            var host = Object.FindObjectOfType<MiniGameBootstrap>();
            CampaignWorldFixture.Restore(host);
            var doorCamera = host.Player.ViewCamera.GetComponent<DoorViewCamera>();
            if (doorCamera != null) doorCamera.enabled = false; // Keep this geometry fixture on manual hinge movement.
            var door = Object.FindObjectOfType<HingedDoor>();
            var frame = GameObject.Find("Entrance frame");
            Assert.That(host, Is.Not.Null);
            Assert.That(host.Runtime.GetView().Game.Day, Is.Zero, "This regression must exercise the ordinary Day0 door without a morning cue.");
            Assert.That(door, Is.Not.Null);
            Assert.That(frame, Is.Not.Null);
            var leaf = door.GetComponentsInChildren<BoxCollider>().Single(collider => collider.name == "Entrance door");
            var frameColliders = frame.GetComponentsInChildren<Collider>();
            Assert.That(frameColliders, Is.Not.Empty, "The visible frame must retain physical collision.");
            var filters = frame.GetComponentsInChildren<MeshFilter>();
            var stableVertices = WorldVertices(filters);
            var stableMatrix = frame.transform.localToWorldMatrix;
            AssertLeafClear(leaf, frameColliders, 0);

            foreach (float angle in new[] { 30f, 60f, 90f })
            {
                door.OpenAngle = angle;
                Assert.That(door.TryInteract().Accepted, Is.True, "A real runtime command must start the configured door opening.");
                float deadline = Time.realtimeSinceStartup + 4;
                do
                {
                    yield return null;
                    AssertFrameFixed(frame.transform, stableMatrix, filters, stableVertices);
                    AssertLeafClear(leaf, frameColliders, door.CurrentAngle);
                } while ((door.IsMoving || host.Runtime.GetView().Cue?.Kind == PresentationKind.DoorMotion)
                    && Time.realtimeSinceStartup < deadline);
                Assert.That(door.IsMoving, Is.False);
                Assert.That(host.Runtime.GetView().Cue, Is.Null);
                Assert.That(door.CurrentAngle, Is.EqualTo(angle).Within(.001f));
                AssertLeafClear(leaf, frameColliders, angle);
                if (Mathf.Approximately(angle, 60))
                    yield return CaptureRoomSideViews(host, frame.transform, stableMatrix, filters, stableVertices);
                Assert.That(door.TryInteract().Accepted, Is.True);
                deadline = Time.realtimeSinceStartup + 4;
                do
                {
                    yield return null;
                    AssertFrameFixed(frame.transform, stableMatrix, filters, stableVertices);
                    AssertLeafClear(leaf, frameColliders, door.CurrentAngle);
                } while ((door.IsMoving || host.Runtime.GetView().Cue?.Kind == PresentationKind.DoorMotion)
                    && Time.realtimeSinceStartup < deadline);
                Assert.That(door.CurrentAngle, Is.Zero.Within(.001f));
                Assert.That(host.Runtime.GetView().Cue, Is.Null);
            }
            door.OpenAngle = 60;
            Debug.Log("MINIGAME_DOOR_FRAME_MOTION_PASSED stationary world vertices; no leaf/frame penetration through 0/30/60/90-degree opening and closing; eight room-side views");
        }

        private static IEnumerator CaptureRoomSideViews(MiniGameBootstrap host, Transform frame, Matrix4x4 matrix,
            MeshFilter[] filters, Vector3[] stableVertices)
        {
            var approach = GameObject.Find("Door approach");
            var marker = GameObject.Find("Unity_DoorCentre");
            Assert.That(approach, Is.Not.Null);
            Assert.That(marker, Is.Not.Null);
            var inward = Vector3.ProjectOnPlane(approach.transform.position - marker.transform.position, Vector3.up).normalized;
            var lateral = Vector3.Cross(Vector3.up, inward).normalized;
            var positions = new[]
            {
                new Vector2(.7f, 0), new Vector2(1.2f, -.4f), new Vector2(1.2f, .4f),
                new Vector2(2f, -.4f), new Vector2(2f, .4f), new Vector2(2.8f, 0),
                new Vector2(.7f, -.4f), new Vector2(.7f, .4f)
            };
            var names = new[] { "centre-close", "left", "right", "left-far", "right-far", "centre-far", "grazing-left", "grazing-right" };
            var controller = host.Player.GetComponent<CharacterController>();
            for (int index = 0; index < positions.Length; index++)
            {
                controller.enabled = false;
                var position = marker.transform.position + inward * positions[index].x + lateral * positions[index].y;
                position.y = approach.transform.position.y;
                host.Player.transform.position = position;
                controller.enabled = true;
                Physics.SyncTransforms();
                yield return null;
                host.Player.FacePoint(marker.transform.position + Vector3.up * (index == 6 ? -.65f : .15f));
                yield return null;
                AssertFrameFixed(frame, matrix, filters, stableVertices);
                yield return PrototypePlayModeTests.CaptureIfRequested(host, "door-frame-" + names[index]);
            }
        }

        private static void AssertLeafClear(BoxCollider leaf, Collider[] frameColliders, float angle)
        {
            Physics.SyncTransforms();
            foreach (var frame in frameColliders)
            {
                bool overlaps = Physics.ComputePenetration(leaf, leaf.transform.position, leaf.transform.rotation,
                    frame, frame.transform.position, frame.transform.rotation, out _, out float depth);
                Assert.That(overlaps && depth > PlaneTolerance, Is.False,
                    "Door leaf intersects " + frame.name + " at angle " + angle.ToString("F3") + " by " + depth.ToString("F6") + " metres.");
            }
        }

        private static void AssertFrameFixed(Transform frame, Matrix4x4 matrix, MeshFilter[] filters, Vector3[] expected)
        {
            for (int entry = 0; entry < 16; entry++)
                Assert.That(frame.localToWorldMatrix[entry], Is.EqualTo(matrix[entry]).Within(.000001f), "The frame must not inherit leaf rotation.");
            var actual = WorldVertices(filters);
            Assert.That(actual.Length, Is.EqualTo(expected.Length));
            for (int index = 0; index < expected.Length; index++)
                Assert.That(Vector3.Distance(actual[index], expected[index]), Is.LessThan(.000001f), "Frame vertex " + index + " moved with the door.");
        }

        private static Vector3[] WorldVertices(IEnumerable<MeshFilter> filters) => filters.SelectMany(filter =>
            ReadMesh(filter.sharedMesh).Vertices.Select(vertex => filter.transform.TransformPoint(vertex))).ToArray();

        private static List<Triangle> Triangles(IEnumerable<MeshFilter> filters)
        {
            var result = new List<Triangle>();
            foreach (var filter in filters)
            {
                if (filter.sharedMesh == null) continue;
                var data = ReadMesh(filter.sharedMesh);
                for (int index = 0; index + 2 < data.Indices.Length; index += 3)
                {
                    var triangle = new Triangle(filter.name + "/triangle-" + index / 3,
                        filter.transform.TransformPoint(data.Vertices[data.Indices[index]]),
                        filter.transform.TransformPoint(data.Vertices[data.Indices[index + 1]]),
                        filter.transform.TransformPoint(data.Vertices[data.Indices[index + 2]]));
                    if (triangle.Normal.sqrMagnitude > .9f) result.Add(triangle);
                }
            }
            return result;
        }

        private static (Vector3[] Vertices, int[] Indices) ReadMesh(Mesh mesh)
        {
#if UNITY_EDITOR
            using (var meshes = UnityEditor.MeshUtility.AcquireReadOnlyMeshData(mesh))
#else
            using (var meshes = Mesh.AcquireReadOnlyMeshData(mesh))
#endif
            {
                var data = meshes[0];
                using (var vertices = new NativeArray<Vector3>(data.vertexCount, Allocator.Temp))
                {
                    data.GetVertices(vertices);
                    var indices = new List<int>();
                    for (int submesh = 0; submesh < data.subMeshCount; submesh++)
                    {
                        var descriptor = data.GetSubMesh(submesh);
                        if (descriptor.topology != MeshTopology.Triangles) continue;
                        using (var submeshIndices = new NativeArray<int>(descriptor.indexCount, Allocator.Temp))
                        {
                            data.GetIndices(submeshIndices, submesh);
                            indices.AddRange(submeshIndices.ToArray());
                        }
                    }
                    return (vertices.ToArray(), indices.ToArray());
                }
            }
        }

        private static void AddOverlap(Triangle first, Triangle second, List<string> overlaps)
        {
            if (Mathf.Abs(Vector3.Dot(first.Normal, second.Normal)) < .99999f) return;
            if (Mathf.Abs(Vector3.Dot(second.A - first.A, first.Normal)) > PlaneTolerance
                || Mathf.Abs(Vector3.Dot(second.B - first.A, first.Normal)) > PlaneTolerance
                || Mathf.Abs(Vector3.Dot(second.C - first.A, first.Normal)) > PlaneTolerance) return;
            int axis = DominantAxis(first.Normal);
            var clip = new[] { Project(second.A, axis), Project(second.B, axis), Project(second.C, axis) };
            var polygon = new List<Vector2> { Project(first.A, axis), Project(first.B, axis), Project(first.C, axis) };
            float orientation = Mathf.Sign(Cross(clip[1] - clip[0], clip[2] - clip[0]));
            for (int edge = 0; edge < 3 && polygon.Count > 0; edge++)
            {
                Vector2 start = clip[edge], end = clip[(edge + 1) % 3];
                var output = new List<Vector2>();
                Vector2 previous = polygon[polygon.Count - 1];
                float previousSide = orientation * Cross(end - start, previous - start);
                foreach (var current in polygon)
                {
                    float currentSide = orientation * Cross(end - start, current - start);
                    bool previousInside = previousSide >= -0.0000001f;
                    bool currentInside = currentSide >= -0.0000001f;
                    if (previousInside != currentInside)
                        output.Add(Vector2.LerpUnclamped(previous, current, previousSide / (previousSide - currentSide)));
                    if (currentInside) output.Add(current);
                    previous = current;
                    previousSide = currentSide;
                }
                polygon = output;
            }
            float area = 0;
            for (int index = 0; index < polygon.Count; index++) area += Cross(polygon[index], polygon[(index + 1) % polygon.Count]);
            area = Mathf.Abs(area) * .5f;
            if (area > OverlapAreaTolerance)
                overlaps.Add(first.Label + " <-> " + second.Label + " projectedArea=" + area.ToString("F7") + "m2");
        }

        private static int DominantAxis(Vector3 normal) => Mathf.Abs(normal.x) > Mathf.Abs(normal.y)
            ? (Mathf.Abs(normal.x) > Mathf.Abs(normal.z) ? 0 : 2) : (Mathf.Abs(normal.y) > Mathf.Abs(normal.z) ? 1 : 2);
        private static Vector2 Project(Vector3 point, int axis) => axis == 0 ? new Vector2(point.y, point.z)
            : axis == 1 ? new Vector2(point.x, point.z) : new Vector2(point.x, point.y);
        private static float Cross(Vector2 first, Vector2 second) => first.x * second.y - first.y * second.x;

        private readonly struct Triangle
        {
            public readonly string Label;
            public readonly Vector3 A, B, C, Normal;
            public Triangle(string label, Vector3 a, Vector3 b, Vector3 c)
            {
                Label = label; A = a; B = b; C = c;
                Normal = Vector3.Cross(b - a, c - a).normalized;
            }
        }
    }
}
