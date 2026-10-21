using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MiniGame.Core;
using MiniGame.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace MiniGame.UnityHost.Tests
{
    public sealed class DreamCubemapPlayModeTests
    {
        private readonly List<Object> owned = new List<Object>();
        private readonly List<string> saveFiles = new List<string>();
        private DreamCubemapRenderer isolated;
        private RoomDreamEnding room;
        private MiniGameBootstrap host;

        [UnityTearDown]
        public IEnumerator Cleanup()
        {
            if (room != null) room.StopAndRestore();
            isolated?.Dispose(); isolated = null;
            if (host != null)
            {
                host.enabled = false;
                host.Player.enabled = false;
                host.Dreams?.PrepareForRuntimeChange();
                host.Effects?.PrepareForRuntimeChange();
            }
            MiniGameSessionHost.Release();
            foreach (var value in owned) if (value != null) Object.Destroy(value);
            owned.Clear();
            foreach (var file in saveFiles) if (File.Exists(file)) File.Delete(file);
            saveFiles.Clear();
            Cursor.lockState = CursorLockMode.None; Cursor.visible = true;
            yield return null;
        }

        [UnityTest]
        public IEnumerator SixFacesPreserveQuadrantOrientationAndRestoreRenderState()
        {
            var profile = Own(ScriptableObject.CreateInstance<DreamSequenceProfile>());
            profile.CubemapFaceSize = 256;
            profile.CubemapSourcePrefab = CreateOrientationSource();
            var viewer = Own(new GameObject("Cubemap orientation viewer", typeof(Camera))).GetComponent<Camera>();
            viewer.enabled = false; viewer.cullingMask = 0;
            viewer.clearFlags = CameraClearFlags.SolidColor; viewer.fieldOfView = 90; viewer.aspect = 1;
            var originalSky = viewer.gameObject.AddComponent<Skybox>();
            originalSky.enabled = false;
            var sentinel = Own(new RenderTexture(16, 16, 0)); sentinel.Create();
            var previousActive = RenderTexture.active;
            var beforeAmbient = RenderSettings.ambientSkyColor;
            var beforeGlobalSky = RenderSettings.skybox;
            isolated = new DreamCubemapRenderer(profile, viewer);
            try
            {
                RenderTexture.active = sentinel;
                isolated.Render(0, BeachEndingPhase.Walking, 0, 0);
                Assert.That(RenderTexture.active, Is.SameAs(sentinel));
                Assert.That(RenderSettings.ambientSkyColor, Is.EqualTo(beforeAmbient));
                Assert.That(RenderSettings.skybox, Is.SameAs(beforeGlobalSky));
                Assert.That(isolated.IsReady, Is.True);
                var first = isolated.Texture;
                isolated.Render(.02f, BeachEndingPhase.Walking, 0, 0);
                Assert.That(isolated.CapturedFrames, Is.EqualTo(1), "Throttle must retain the complete front cube.");
                isolated.Render(.2f, BeachEndingPhase.Walking, 0, 0);
                Assert.That(isolated.CapturedFrames, Is.EqualTo(2));
                Assert.That(isolated.Texture, Is.Not.SameAs(first));
                for (int face = 0; face < Directions.Length; face++)
                {
                    viewer.transform.rotation = FaceRotation(face);
                    var pixels = ReadView(viewer);
                    AssertColor(pixels.GetPixel(42, 85), Color.red, face + " upper left");
                    AssertColor(pixels.GetPixel(85, 85), Color.green, face + " upper right");
                    AssertColor(pixels.GetPixel(42, 42), Color.blue, face + " lower left");
                    AssertColor(pixels.GetPixel(85, 42), Color.yellow, face + " lower right");
                    Object.Destroy(pixels);
                }
                var clone = isolated.SourceRoot.GetComponentInChildren<DreamCubemapSource>();
                Assert.That(clone.GetComponentsInChildren<Camera>(true), Is.Empty);
                Assert.That(clone.GetComponentsInChildren<AudioSource>(true), Is.Empty);
                Assert.That(clone.GetComponentsInChildren<Canvas>(true), Is.Empty);
                Assert.That(clone.GetComponentsInChildren<MonoBehaviour>(true).All(value => value is DreamCubemapSource), Is.True);
                var marker = clone.transform.Find("Face0-TL").GetComponent<Renderer>();
                marker.sharedMaterial.color = Color.magenta;
                isolated.Render(1, BeachEndingPhase.Walking, 0, 0);
                viewer.transform.rotation = FaceRotation(0);
                var changed = ReadView(viewer);
                AssertColor(changed.GetPixel(42, 85), Color.magenta, "The next capture must contain changed source content.");
                Object.Destroy(changed);
                isolated.Dispose();
                Assert.That(viewer.clearFlags, Is.EqualTo(CameraClearFlags.SolidColor));
                Assert.That(viewer.cullingMask, Is.Zero);
                Assert.That(originalSky.enabled, Is.False);
                Assert.That(originalSky.material, Is.Null);
            }
            finally { RenderTexture.active = previousActive; }
            yield return null;
        }

        [UnityTest]
        public IEnumerator StaticWallpaperMatchesDoorwayProjectionAndRevealsPhysicalWallsOffAnchor()
        {
            var profile = Own(ScriptableObject.CreateInstance<DreamSequenceProfile>());
            profile.CubemapFaceSize = 256;
            profile.CubemapSourcePrefab = CreateOrientationSource();
            var viewer = Own(new GameObject("Finite room viewer", typeof(Camera))).GetComponent<Camera>();
            viewer.enabled = false; viewer.cullingMask = 1 << 26;
            viewer.clearFlags = CameraClearFlags.SolidColor; viewer.fieldOfView = 90; viewer.aspect = 1;
            viewer.nearClipPlane = .03f;
            isolated = new DreamCubemapRenderer(profile, viewer, false);
            var parent = Own(new GameObject("Finite room"));
            var center = new Vector3(13.7f, -2.8f, 9.3f);
            var bounds = new Bounds(center, new Vector3(9.7f, 3.12f, 4.42f));
            var doorway = center + new Vector3(-4.1f, .5f, 1.5f);
            using (var surfaces = new DreamCubemapRoomSurface(bounds, parent.transform, 26, doorway))
            {
                surfaces.SetOutline(false);
                isolated.Render(0, BeachEndingPhase.Walking, 0, 0);
                surfaces.Bind(isolated.Texture, 0);
                Assert.That(surfaces.Faces.Length, Is.EqualTo(6));
                Assert.That(viewer.GetComponent<Skybox>(), Is.Null);
                viewer.transform.position = doorway;
                for (int face = 0; face < Directions.Length; face++)
                {
                    viewer.transform.rotation = FaceRotation(face);
                    var pixels = ReadView(viewer);
                    AssertColor(pixels.GetPixel(42, 85), Color.red, face + " room upper left");
                    AssertColor(pixels.GetPixel(85, 85), Color.green, face + " room upper right");
                    AssertColor(pixels.GetPixel(42, 42), Color.blue, face + " room lower left");
                    AssertColor(pixels.GetPixel(85, 42), Color.yellow, face + " room lower right");
                    Object.Destroy(pixels);
                }
                var originalFaceMatrices = surfaces.Faces.Select(face => face.localToWorldMatrix).ToArray();
                viewer.transform.rotation = FaceRotation(0);
                var calibratedImage = ReadView(viewer);
                var ray = new Vector3(-1.35f, 1.35f, 4);
                var wallPoint = RayExitRoom(doorway, ray, bounds);
                var originalScreen = viewer.WorldToViewportPoint(wallPoint);
                viewer.transform.position = doorway + new Vector3(.6f, 0, -.7f);
                var movedImage = ReadView(viewer);
                try
                {
                    var movedScreen = viewer.WorldToViewportPoint(wallPoint);
                    Assert.That(Mathf.Abs(movedScreen.x - originalScreen.x), Is.GreaterThan(.08f));
                    AssertColor(movedImage.GetPixel(Mathf.RoundToInt(movedScreen.x * 128), Mathf.RoundToInt(movedScreen.y * 128)),
                        Color.red, "The same painted red marker follows its fixed physical wall position.");
                    Assert.That(CountChangedPixels(calibratedImage, movedImage, 20), Is.GreaterThan(128),
                        "Leaving the doorway must reveal a changed perspective of the static painted room.");
                }
                finally { Object.Destroy(calibratedImage); Object.Destroy(movedImage); }
                Assert.That((Vector3)surfaces.Material.GetVector("_ProjectionOrigin"), Is.EqualTo(doorway),
                    "The projection origin remains the calibrated doorway eye after the viewer moves.");
                Assert.That(isolated.CapturedFrames, Is.EqualTo(1));
                Assert.That(surfaces.Root.transform.position, Is.EqualTo(center));
                for (int face = 0; face < surfaces.Faces.Length; face++)
                    Assert.That(surfaces.Faces[face].localToWorldMatrix, Is.EqualTo(originalFaceMatrices[face]),
                        "Wallpaper and room geometry remain fixed after leaving the calibration point.");
                Assert.That(viewer.clearFlags, Is.EqualTo(CameraClearFlags.SolidColor));
            }
            yield return null;
        }

        [UnityTest]
        public IEnumerator RoomOutlineRendersAllTwelveEdgesWithoutFaceDiagonalsAndRespectsDepth()
        {
            var viewer = Own(new GameObject("Room outline viewer", typeof(Camera))).GetComponent<Camera>();
            viewer.enabled = false; viewer.cullingMask = 1 << 26;
            viewer.clearFlags = CameraClearFlags.SolidColor;
            viewer.nearClipPlane = .03f; viewer.fieldOfView = 12; viewer.aspect = 1;
            var parent = Own(new GameObject("Outlined room"));
            var center = new Vector3(13.7f, -2.8f, 9.3f);
            var bounds = new Bounds(center, new Vector3(9.7f, 3.12f, 4.42f));
            var cube = Own(new Cubemap(16, TextureFormat.RGBA32, false));
            var bluePixels = Enumerable.Repeat(Color.blue, 16 * 16).ToArray();
            for (int face = 0; face < 6; face++) cube.SetPixels(bluePixels, (CubemapFace)face);
            cube.Apply();
            viewer.transform.position = center;
            using (var surfaces = new DreamCubemapRoomSurface(bounds, parent.transform, 26))
            {
                surfaces.Bind(cube, 0);
                var edgeMidpoints = new List<Vector3>();
                for (int freeAxis = 0; freeAxis < 3; freeAxis++)
                    foreach (float firstSign in new[] { -1f, 1f })
                        foreach (float secondSign in new[] { -1f, 1f })
                        {
                            var offset = Vector3.zero;
                            int firstAxis = (freeAxis + 1) % 3, secondAxis = (freeAxis + 2) % 3;
                            offset[firstAxis] = bounds.extents[firstAxis] * firstSign;
                            offset[secondAxis] = bounds.extents[secondAxis] * secondSign;
                            edgeMidpoints.Add(center + offset);
                        }
                Assert.That(edgeMidpoints.Count, Is.EqualTo(12));
                foreach (var point in edgeMidpoints)
                    AssertRenderedPoint(viewer, point, Color.white, "Physical room edge " + point);
                foreach (var face in surfaces.Faces)
                {
                    var vertices = face.GetComponent<MeshFilter>().sharedMesh.vertices;
                    foreach (float fraction in new[] { .25f, .5f, .75f })
                    {
                        var point = face.transform.TransformPoint(Vector3.Lerp(vertices[0], vertices[2], fraction));
                        AssertRenderedPoint(viewer, point, Color.blue,
                            face.name + " interior mesh diagonal must retain the cubemap at " + fraction);
                    }
                }
                var occludedEdge = edgeMidpoints[0];
                var occluder = Own(GameObject.CreatePrimitive(PrimitiveType.Cube));
                occluder.name = "Foreground outline occluder"; occluder.layer = 26;
                occluder.transform.position = Vector3.Lerp(center, occludedEdge, .5f);
                occluder.transform.localScale = Vector3.one * .3f;
                occluder.GetComponent<Renderer>().sharedMaterial = Own(new Material(Shader.Find("Unlit/Color")) { color = Color.magenta });
                AssertRenderedPoint(viewer, occludedEdge, Color.magenta,
                    "Foreground geometry must occlude the white room outline through normal depth testing.");
            }
            yield return null;
        }

        [UnityTest]
        public IEnumerator BakerSourceTranslationPreservesGeometryAndContinuousLieDownWithYaw()
        {
            var profile = Own(ScriptableObject.CreateInstance<DreamSequenceProfile>());
            profile.CubemapFaceSize = 256;
            profile.CubemapSourcePrefab = CreateOrientationSource();
            profile.CubemapSourcePrefab.GetComponent<DreamCubemapSource>().PillowPose.localPosition = new Vector3(1.1f, -.7f, .6f);
            var viewer = Own(new GameObject("Translated panorama viewer", typeof(Camera))).GetComponent<Camera>();
            viewer.enabled = false; viewer.cullingMask = 0;
            viewer.clearFlags = CameraClearFlags.SolidColor; viewer.fieldOfView = 90; viewer.aspect = 1;
            viewer.nearClipPlane = .03f;
            isolated = new DreamCubemapRenderer(profile, viewer);

            var center = new Vector3(13.7f, -2.8f, 9.3f);
            var source = isolated.SourceRoot.GetComponentInChildren<DreamCubemapSource>();

            foreach (float yaw in new[] { 0f, 90f })
            {
                profile.CubemapYaw = yaw;
                var rotation = Quaternion.Euler(0, yaw, 0);
                for (int face = 0; face < Directions.Length; face++)
                {
                    var viewRotation = rotation * FaceRotation(face);
                    viewer.transform.SetPositionAndRotation(center, viewRotation);
                    isolated.Render(5, BeachEndingPhase.Walking, 0, 0, true, Vector3.zero);

                    int frames = isolated.CapturedFrames;
                    isolated.Render(5.001f, BeachEndingPhase.Walking, 0, 0, false, Vector3.zero);
                    Assert.That(isolated.CapturedFrames, Is.EqualTo(frames), "An unchanged capture pose remains throttled.");
                    var markerWorld = center + viewRotation * new Vector3(-1.35f, 1.35f, 4);
                    var originalScreen = viewer.WorldToViewportPoint(markerWorld);
                    var offset = viewRotation * new Vector3(.9f, .25f, .4f);
                    viewer.transform.position = center + offset;
                    isolated.Render(5, BeachEndingPhase.Walking, 0, 0, false, offset);
                    Assert.That(isolated.CapturedFrames, Is.EqualTo(frames + 1), "Movement must refresh the cube even inside its stationary capture interval.");
                    var expectedCapture = source.StandingPose.position + Quaternion.Euler(0, -yaw, 0) * offset;
                    Assert.That(Vector3.Distance(isolated.CaptureCamera.transform.position, expectedCapture), Is.LessThan(.006f));

                    var movedScreen = viewer.WorldToViewportPoint(markerWorld);
                    Assert.That(Mathf.Abs(movedScreen.x - originalScreen.x), Is.GreaterThan(.08f));
                    var image = ReadView(viewer);
                    try
                    {
                        AssertColor(image.GetPixel(Mathf.RoundToInt(movedScreen.x * 128), Mathf.RoundToInt(movedScreen.y * 128)),
                            Color.red, "Translated geometric marker, yaw " + yaw + ", face " + face);
                        var stale = image.GetPixel(Mathf.RoundToInt(originalScreen.x * 128), Mathf.RoundToInt(originalScreen.y * 128));
                        Assert.That(stale.r - Mathf.Max(stale.g, stale.b), Is.LessThan(.5f),
                            "The red marker must leave its old screen position as the capture camera moves.");
                    }
                    finally { Object.Destroy(image); }
                }
                var explorationOffset = rotation * new Vector3(.7f, .15f, .2f);
                isolated.Render(5, BeachEndingPhase.Walking, 0, 0, false, explorationOffset);
                var walkingCapture = isolated.CaptureCamera.transform.position;
                isolated.Render(5, BeachEndingPhase.Lying, 0, 0, false, explorationOffset);
                Assert.That(Vector3.Distance(isolated.CaptureCamera.transform.position, walkingCapture), Is.LessThan(.006f),
                    "Lying begins at the exact explored capture pose.");
                isolated.Render(5, BeachEndingPhase.Lying, .5f, 0, false, explorationOffset);
                Assert.That(Vector3.Distance(isolated.CaptureCamera.transform.position,
                    Vector3.Lerp(walkingCapture, source.PillowPose.position, .5f)), Is.LessThan(.006f));
                isolated.Render(5, BeachEndingPhase.Lying, 1, 0, false, explorationOffset);
                Assert.That(Vector3.Distance(isolated.CaptureCamera.transform.position, source.PillowPose.position), Is.LessThan(.006f));
                isolated.Render(5, BeachEndingPhase.Walking, 0, 0, false, explorationOffset);
                Assert.That(Vector3.Distance(isolated.CaptureCamera.transform.position, walkingCapture), Is.LessThan(.006f),
                    "Restoring exploration returns to the same source position without depending on elapsed animation time.");
            }
            yield return null;
        }

        [Test]
        public void GeneratedSourceContainsOnlyBeachAndItsProfileKeepsDailyIllustrations()
        {
            var profile = Resources.Load<DreamSequenceProfile>("MiniGame/DreamSequenceDefault");
            Assert.That(profile, Is.Not.Null);
            Assert.That(profile.UseRoomCubemapEnding, Is.True);
            Assert.That(profile.ShowRoomOutline, Is.False);
            Assert.That(profile.CubemapFaceSize, Is.EqualTo(1024));
            Assert.That(profile.CubemapRefreshRate, Is.EqualTo(10));
            Assert.That(profile.CubemapSourcePrefab, Is.Not.Null, "Run DreamCubemapSetup.BuildSource before the regression suite.");
            var source = profile.CubemapSourcePrefab.GetComponent<DreamCubemapSource>();
            Assert.That(source, Is.Not.Null);
            Assert.That(source.StandingPose, Is.Not.Null);
            Assert.That(source.PillowPose.parent, Is.Not.SameAs(source.transform), "The meshless bed pivot preserves the original drift trajectory.");
            Assert.That(source.PillowPose.parent.GetComponentsInChildren<Renderer>(true), Is.Empty);
            Assert.That(source.GetComponentsInChildren<MonoBehaviour>(true).All(value => value is DreamCubemapSource), Is.True);
            Assert.That(source.GetComponentsInChildren<Camera>(true), Is.Empty);
            Assert.That(source.GetComponentsInChildren<AudioSource>(true), Is.Empty);
            Assert.That(source.GetComponentsInChildren<Canvas>(true), Is.Empty);
            Assert.That(source.WaterSurfaces.Length, Is.GreaterThanOrEqualTo(2));
            Assert.That(source.Lights.Length, Is.GreaterThanOrEqualTo(1));
            for (int day = 1; day <= 3; day++) Assert.That(profile.GetDay(day).Artwork, Is.Not.Null);
        }

        [UnityTest]
        public IEnumerator SelectedSkyboxCapturesAtDoorEyeAndKeepsAllSixDirections()
        {
            var profile = Own(ScriptableObject.CreateInstance<DreamSequenceProfile>());
            profile.CubemapFaceSize = 256;
            var cube = Own(new Cubemap(16, TextureFormat.RGBA32, false));
            var colors = new[] { Color.red, Color.green, Color.blue, Color.yellow, Color.magenta, Color.cyan };
            for (int face = 0; face < 6; face++) cube.SetPixels(Enumerable.Repeat(colors[face], 256).ToArray(), (CubemapFace)face);
            cube.Apply();
            var selectedSky = Own(new Material(Shader.Find("Skybox/Cubemap")));
            selectedSky.SetTexture("_Tex", cube);
            var viewer = Own(new GameObject("Selected skybox doorway viewer", typeof(Camera))).GetComponent<Camera>();
            viewer.enabled = false; viewer.cullingMask = 1 << 26;
            viewer.clearFlags = CameraClearFlags.SolidColor; viewer.fieldOfView = 90; viewer.aspect = 1;
            viewer.nearClipPlane = .03f;
            var center = new Vector3(13.7f, -2.8f, 9.3f);
            var doorway = center + new Vector3(-4.1f, .5f, 1.5f);
            var parent = Own(new GameObject("Selected skybox room"));
            isolated = new DreamCubemapRenderer(profile, viewer, selectedSky, doorway);
            isolated.Render(0, BeachEndingPhase.Walking, 0, 0);
            Assert.That(isolated.CaptureCamera.transform.position, Is.EqualTo(doorway));
            Assert.That(isolated.CapturedFrames, Is.EqualTo(1));
            using (var surfaces = new DreamCubemapRoomSurface(new Bounds(center, new Vector3(9.7f, 3.12f, 4.42f)), parent.transform, 26, doorway))
            {
                surfaces.SetOutline(false);
                surfaces.Bind(isolated.Texture, 0);
                viewer.transform.position = doorway;
                int[] faceIndices = { 4, 5, 1, 0, 2, 3 };
                for (int face = 0; face < Directions.Length; face++)
                {
                    viewer.transform.rotation = FaceRotation(face);
                    var image = ReadView(viewer);
                    try { AssertColor(image.GetPixel(64, 64), colors[faceIndices[face]], "Selected skybox direction " + face); }
                    finally { Object.Destroy(image); }
                }
            }
            yield return null;
        }

        [UnityTest, Timeout(90000)]
        public IEnumerator RoomExplorationPauseCheckpointDriftAndCleanupPreserveOriginalRoom()
        {
            MiniGameSessionHost.Release();
            yield return null;
            yield return SceneManager.LoadSceneAsync(MiniGameSessionHost.CombinedScene);
            yield return null;
            host = Object.FindObjectOfType<MiniGameBootstrap>();
            Assert.That(host, Is.Not.Null);
            var originalProfile = host.DreamProfile;
            string originalJson = JsonUtility.ToJson(originalProfile);
            var profile = Own(Object.Instantiate(originalProfile));
            profile.CubemapFaceSize = 256;
            profile.LieDownSeconds = .2f;
            profile.ShowRoomOutline = false;
            host.ConfigureDreams(profile, Resources.Load<PlayerRebellionProfile>("MiniGame/PlayerRebellion"));
            host.StartCampaign();
            host.Runtime.SetPaused(false);
            var renderers = host.gameObject.scene.GetRootGameObjects().SelectMany(value => value.GetComponentsInChildren<Renderer>(true)).ToArray();
            var rendererStates = renderers.Select(value => value.enabled).ToArray();
            var colliders = host.gameObject.scene.GetRootGameObjects().SelectMany(value => value.GetComponentsInChildren<Collider>(true)).ToArray();
            var colliderStates = colliders.Select(value => value.enabled).ToArray();
            var camera = host.Player.ViewCamera;
            var doorwaySpawn = host.gameObject.scene.GetRootGameObjects()
                .SelectMany(value => value.GetComponentsInChildren<Transform>(true)).Single(value => value.name == "Player spawn").position;
            var originalFlags = camera.clearFlags;
            int originalMask = camera.cullingMask;
            var originalSkybox = camera.GetComponent<Skybox>();
            var originalMaterial = originalSkybox != null ? originalSkybox.material : null;
            bool originalSkyEnabled = originalSkybox != null && originalSkybox.enabled;
            room = RoomDreamEnding.Begin(host, MiniGameSessionHost.Current);
            yield return Until(() => room != null && room.Ready, 10);
            var ending = room.Ending;
            ending.UseExternalClock = true;
            Assert.That(room.Cubemap.IsReady, Is.True);
            Assert.That(room.Cubemap.CapturedFrames, Is.EqualTo(1));
            var frozenCube = room.Cubemap.Texture;
            var frozenPixels = Own(ReadCubeFace(frozenCube, CubemapFace.PositiveZ));
            var projectionOrigin = room.PanoramaOrigin;
            Assert.That(Vector2.Distance(new Vector2(room.PlayerPosition.x, room.PlayerPosition.z),
                new Vector2(doorwaySpawn.x, doorwaySpawn.z)), Is.LessThan(.01f), "Exploration begins at the actual Player spawn doorway.");
            Assert.That(Vector3.Distance(projectionOrigin, camera.transform.position), Is.LessThan(.005f));
            Assert.That(room.RoomSurface.Material.GetFloat("_EdgeWidth"), Is.Zero);
            Assert.That(room.RoomSurface.Faces.Length, Is.EqualTo(6));
            Assert.That(room.RoomSurface.Faces.All(value => value.enabled), Is.True);
            Assert.That(camera.clearFlags, Is.EqualTo(CameraClearFlags.SolidColor));
            Assert.That(room.MappedRoomBounds.Contains(camera.transform.position), Is.True);
            Assert.That(ending.Phase, Is.EqualTo(BeachEndingPhase.Walking));
            Assert.That(renderers.Where(value => value.enabled).All(value => value.transform.IsChildOf(ending.Bed)), Is.True,
                "Among original scene renderers, only the physical bed remains visible.");
            Assert.That(colliders.Where((value, index) => colliderStates[index] && value.enabled && !(value is CharacterController)).Any(), Is.True,
                "The hidden floor and room boundary must remain solid.");
            Assert.That(colliders.Where((value, index) => colliderStates[index] && !value.enabled).Any(), Is.True,
                "Hidden furniture must not keep blocking the room.");
            AssertPhysicalBedParallax(room);
            var spawn = room.PlayerPosition;
            var rotation = room.ViewCamera.transform.rotation;
            ending.AdvanceForTests(0, 0);
            var bedPosition = ending.Bed.position;
            var bedRotation = ending.Bed.rotation;
            var eyePosition = camera.transform.position;
            var capturePosition = room.Cubemap.CaptureCamera.transform.position;
            float fixedWorldTime = ending.WorldTime;
            var beforeMovement = ReadRoomBackground(room);
            room.AdvanceExploration(Vector2.up, Vector2.zero, .3f);
            ending.AdvanceForTests(0, 0);
            var afterMovement = ReadRoomBackground(room);
            try
            {
                Assert.That(Vector3.Distance(camera.transform.position, eyePosition), Is.GreaterThan(.02f));
                Assert.That(room.Cubemap.CaptureCamera.transform.position, Is.EqualTo(capturePosition),
                    "WASD must leave the frozen source capture and calibration point unchanged.");
                Assert.That(room.Cubemap.CapturedFrames, Is.EqualTo(1));
                Assert.That(room.PanoramaOrigin, Is.EqualTo(projectionOrigin));
                Assert.That((Vector3)room.RoomSurface.Material.GetVector("_ProjectionOrigin"), Is.EqualTo(projectionOrigin));
                Assert.That(ending.WorldTime, Is.EqualTo(fixedWorldTime));
                Assert.That(CountChangedPixels(beforeMovement, afterMovement, 3), Is.GreaterThan(30),
                    "The fixed painted-room image changes perspective when the viewer leaves the doorway.");
                Assert.That(ending.Bed.position, Is.EqualTo(bedPosition));
                Assert.That(Quaternion.Angle(ending.Bed.rotation, bedRotation), Is.LessThan(.001f),
                    "WASD changes the viewer, never the real bed's world pose.");
            }
            finally { Object.Destroy(beforeMovement); Object.Destroy(afterMovement); }
            room.AdvanceExploration(new Vector2(.5f, -.5f), new Vector2(15, 10), .3f);
            ending.AdvanceForTests(0, 0);
            Assert.That(Vector3.Distance(room.PlayerPosition, spawn), Is.GreaterThan(.02f));
            Assert.That(Quaternion.Angle(rotation, room.ViewCamera.transform.rotation), Is.GreaterThan(1));
            var saved = ending.CaptureCheckpoint();
            var savedCapturePosition = room.Cubemap.CaptureCamera.transform.position;
            Assert.That(saved.HasRoomPose, Is.True);
            for (int step = 0; step < 120; step++) room.AdvanceExploration(new Vector2(1, 1), Vector2.zero, .1f);
            Assert.That(room.ExplorationBounds.Contains(room.PlayerPosition), Is.True);
            ending.RestoreCheckpoint(JsonUtility.FromJson<BeachEndingCheckpoint>(JsonUtility.ToJson(saved)));
            Assert.That(Vector3.Distance(saved.RoomPosition, room.PlayerPosition), Is.LessThan(.005f));
            Assert.That(Quaternion.Angle(Quaternion.Euler(saved.RoomPitch, saved.RoomYaw, 0), room.ViewCamera.transform.rotation), Is.LessThan(.05f));
            Assert.That(Vector3.Distance(room.Cubemap.CaptureCamera.transform.position, savedCapturePosition), Is.LessThan(.006f),
                "A room checkpoint must not recapture or relocate the frozen source.");
            Assert.That(room.Cubemap.CapturedFrames, Is.EqualTo(1));
            ending.SetPaused(true);
            int frames = room.Cubemap.CapturedFrames;
            float worldTime = ending.CaptureCheckpoint().WorldTime;
            room.AdvanceExploration(Vector2.one, Vector2.one, 5);
            ending.AdvanceForTests(0, 5);
            yield return null;
            Assert.That(room.Cubemap.CapturedFrames, Is.EqualTo(frames));
            Assert.That(Vector3.Distance(room.Cubemap.CaptureCamera.transform.position, savedCapturePosition), Is.LessThan(.006f));
            Assert.That(ending.CaptureCheckpoint().WorldTime, Is.EqualTo(worldTime));
            Assert.That(Vector3.Distance(saved.RoomPosition, room.PlayerPosition), Is.LessThan(.005f));
            ending.SetPaused(false);
            ending.RestoreCheckpoint(new BeachEndingCheckpoint { Phase = BeachEndingPhase.Walking, WalkDistance = 999 });
            Assert.That(Vector3.Distance(spawn, room.PlayerPosition), Is.LessThan(.01f), "Legacy saves respawn safely rather than interpreting old beach coordinates as room coordinates.");
            ApproachBed(room);
            Assert.That(ending.CanLieDown, Is.True);
            var nearBed = ending.CaptureCheckpoint();
            var bedParts = ending.Bed.GetComponentsInChildren<Renderer>();
            var bedBounds = bedParts[0].bounds;
            foreach (var part in bedParts) bedBounds.Encapsulate(part.bounds);
            for (int i = 0; i < 80; i++)
            {
                room.AdvanceExploration(Vector2.up, Vector2.zero, .05f);
                var point = room.PlayerPosition;
                bool insideBed = point.x > bedBounds.min.x && point.x < bedBounds.max.x
                    && point.z > bedBounds.min.z && point.z < bedBounds.max.z;
                Assert.That(insideBed, Is.False, "Walking into the bed must not pass through its footprint.");
            }
            ending.RestoreCheckpoint(nearBed);
            Assert.That(ending.TryLieDown(), Is.True);
            Assert.That(ending.TryLieDown(), Is.False);
            var lyingActor = room.PlayerPosition;
            room.AdvanceExploration(Vector2.one, Vector2.one, .25f);
            Assert.That(room.PlayerPosition, Is.EqualTo(lyingActor), "Lying disables exploration input.");
            ending.AdvanceForTests(0, .1f);
            var lying = ending.CaptureCheckpoint();
            var lyingPosition = camera.transform.position;
            ending.AdvanceForTests(0, 3);
            ending.RestoreCheckpoint(lying);
            Assert.That(Vector3.Distance(lyingPosition, camera.transform.position), Is.LessThan(.01f));
            Assert.That(room.Cubemap.CapturedFrames, Is.EqualTo(1), "Lying and checkpoint restore keep the same static wallpaper.");
            ending.AdvanceForTests(0, profile.LieDownSeconds + profile.SubtitleDelay + .2f);
            Assert.That(ending.Phase, Is.EqualTo(BeachEndingPhase.Drifting));
            Assert.That(ending.CurrentSubtitle, Is.Not.Empty);
            var drift = ending.CaptureCheckpoint();
            ending.AdvanceForTests(0, 1);
            ending.RestoreCheckpoint(drift);
            Assert.That(ending.CaptureCheckpoint().DriftElapsed, Is.EqualTo(drift.DriftElapsed));
            var farDrift = ending.CaptureCheckpoint();
            farDrift.DriftElapsed = 600;
            ending.RestoreCheckpoint(farDrift);
            foreach (var part in bedParts)
            {
                Assert.That(room.MappedRoomBounds.SqrDistance(part.bounds.min), Is.LessThan(.0001f));
                Assert.That(room.MappedRoomBounds.SqrDistance(part.bounds.max), Is.LessThan(.0001f));
            }
            Assert.That(room.MappedRoomBounds.Contains(camera.transform.position), Is.True);
            Assert.That(room.Cubemap.CapturedFrames, Is.EqualTo(1));
            Assert.That(room.Cubemap.Texture, Is.SameAs(frozenCube));
            Assert.That(room.Cubemap.CaptureCamera.transform.position, Is.EqualTo(capturePosition));
            Assert.That(room.PanoramaOrigin, Is.EqualTo(projectionOrigin));
            var finalFrozenPixels = ReadCubeFace(room.Cubemap.Texture, CubemapFace.PositiveZ);
            try { Assert.That(CountChangedPixels(frozenPixels, finalFrozenPixels, 0), Is.Zero, "The captured source image remains byte-identical through movement, pause, saves, lying and drift."); }
            finally { Object.Destroy(finalFrozenPixels); }
            room.StopAndRestore(); room = null;
            yield return null;
            for (int i = 0; i < renderers.Length; i++) if (renderers[i] != null) Assert.That(renderers[i].enabled, Is.EqualTo(rendererStates[i]), renderers[i].name);
            for (int i = 0; i < colliders.Length; i++) if (colliders[i] != null) Assert.That(colliders[i].enabled, Is.EqualTo(colliderStates[i]), colliders[i].name);
            Assert.That(camera.clearFlags, Is.EqualTo(originalFlags));
            Assert.That(camera.cullingMask, Is.EqualTo(originalMask));
            if (originalSkybox != null)
            {
                Assert.That(originalSkybox.material, Is.SameAs(originalMaterial));
                Assert.That(originalSkybox.enabled, Is.EqualTo(originalSkyEnabled));
            }
            else Assert.That(camera.GetComponent<Skybox>(), Is.Null);
            Assert.That(Resources.FindObjectsOfTypeAll<DreamCubemapSource>()
                .Where(value => value.gameObject.activeInHierarchy), Is.Empty);
            room = RoomDreamEnding.Begin(host, MiniGameSessionHost.Current);
            yield return Until(() => room != null && room.Ready, 10);
            Assert.That(room.Cubemap.CapturedFrames, Is.EqualTo(1), "Reentry creates exactly one new static capture.");
            Assert.That(room.Cubemap.Texture, Is.Not.SameAs(frozenCube));
            Assert.That(Resources.FindObjectsOfTypeAll<DreamCubemapSource>()
                .Count(value => value.gameObject.activeInHierarchy), Is.EqualTo(profile.RoomPanoramaSkybox != null ? 0 : 1),
                "root=" + room.Cubemap.SourceRoot.activeInHierarchy + " sources="
                + string.Join(";", Resources.FindObjectsOfTypeAll<DreamCubemapSource>().Select(value =>
                    value.name + ":active=" + value.gameObject.activeInHierarchy + ":valid=" + value.gameObject.scene.IsValid()
                    + ":scene=" + value.gameObject.scene.name + ":flags=" + value.hideFlags)));
            Assert.That(JsonUtility.ToJson(originalProfile), Is.EqualTo(originalJson), "Tests must not edit the production asset.");
            // Scene destruction must release capture resources without re-enabling unloading room behaviours.
            yield return SceneManager.LoadSceneAsync("InspectionApartment");
            yield return null;
            Assert.That(Resources.FindObjectsOfTypeAll<DreamCubemapSource>()
                .Where(value => value.gameObject.activeInHierarchy), Is.Empty);
        }

        [UnityTest, Timeout(90000)]
        public IEnumerator CrossSceneLyingAndDriftingLoadsKeepTheAuthoredDoorwayProjectionEye()
        {
            MiniGameSessionHost.Release();
            yield return null;
            yield return SceneManager.LoadSceneAsync(MiniGameSessionHost.CombinedScene);
            yield return null;
            host = Object.FindObjectOfType<MiniGameBootstrap>();
            Assert.That(host, Is.Not.Null);
            var profile = Own(Object.Instantiate(host.DreamProfile));
            profile.CubemapFaceSize = 256;
            profile.LieDownSeconds = 1;
            Assert.That(profile.RoomPanoramaSkybox, Is.Not.Null, "The shipped static room panorama must be configured.");
            var rebellion = Own(Object.Instantiate(Resources.Load<PlayerRebellionProfile>("MiniGame/PlayerRebellion")));
            rebellion.InitialValue = rebellion.Maximum = rebellion.Threshold = 100;
            rebellion.Choices.Clear();
            host.ConfigureDreams(profile, rebellion);
            host.StartCampaign();
            var session = MiniGameSessionHost.Current;
            var doorway = host.gameObject.scene.GetRootGameObjects()
                .SelectMany(value => value.GetComponentsInChildren<Transform>(true)).Single(value => value.name == "Player spawn");
            Assert.That(host.Player.StandingEyeLocalPosition, Is.EqualTo(new Vector3(0, 1.56f, 0)));
            var authoredEye = doorway.position + host.Player.StandingEyeLocalPosition;
            var fixture = new GameRuntime(session.Rules, session.Content, true);
            DriveToSelfReview(fixture);
            host.RestoreCheckpoint(fixture.Save());
            Assert.That(host.Runtime.TryInteract(InteractionTarget.Computer).Accepted, Is.True);
            yield return Until(() => host.Runtime.GetView().Cue == null && !host.Effects.IsTransitioning
                && host.Runtime.GetView().TerminalPage == TerminalPage.SelfReview, 15);
            Assert.That(host.Runtime.ConfirmSelfPurification().Accepted, Is.True);
            yield return Until(() => (room = Object.FindObjectOfType<RoomDreamEnding>()) != null && room.Ready && !session.IsTransitioning, 20);
            Assert.That(session.Runtime.GetView().Game.Ending, Is.EqualTo(EndingType.DreamSea));
            Assert.That(Vector3.Distance(room.PanoramaOrigin, authoredEye), Is.LessThan(.01f));
            var ending = room.Ending;
            ending.UseExternalClock = true;
            ApproachBed(room);
            Assert.That(ending.TryLieDown(), Is.True);
            ending.AdvanceForTests(0, .4f);
            var lying = ending.CaptureCheckpoint();
            Assert.That(lying.Phase, Is.EqualTo(BeachEndingPhase.Lying));
            var lyingFile = SaveTemporaryEnding(session);
            ending.AdvanceForTests(0, profile.LieDownSeconds + profile.SubtitleDelay + .5f);
            var drifting = ending.CaptureCheckpoint();
            Assert.That(drifting.Phase, Is.EqualTo(BeachEndingPhase.Drifting));
            var driftingFile = SaveTemporaryEnding(session);
            var paths = new[] { lyingFile, driftingFile };
            var checkpoints = new[] { lying, drifting };
            for (int index = 0; index < paths.Length; index++)
            {
                // A blank scene has no bootstrap that could install a different campaign.
                session.enabled = false;
                var departingScene = host.gameObject.scene;
                var otherScene = SceneManager.CreateScene("Dream save restore origin " + Guid.NewGuid().ToString("N"));
                SceneManager.SetActiveScene(otherScene);
                yield return SceneManager.UnloadSceneAsync(departingScene);
                room = null; host = null;
                Assert.That(SceneManager.GetActiveScene().name, Is.Not.EqualTo(MiniGameSessionHost.CombinedScene));
                session.LoadFrom(paths[index]);
                Assert.That(session.LastMessage, Does.Contain("已恢复"));
                Assert.That(session.Runtime.SetPaused(true).Accepted, Is.True);
                session.enabled = true;
                yield return Until(() => SceneManager.GetActiveScene().name == MiniGameSessionHost.CombinedScene
                    && (room = Object.FindObjectOfType<RoomDreamEnding>()) != null && room.Ready && !session.IsTransitioning, 20);
                host = Object.FindObjectOfType<MiniGameBootstrap>();
                ending = room.Ending;
                ending.UseExternalClock = true;
                var restored = ending.CaptureCheckpoint();
                Assert.That(restored.Phase, Is.EqualTo(checkpoints[index].Phase));
                Assert.That(restored.PhaseElapsed, Is.EqualTo(checkpoints[index].PhaseElapsed).Within(.0001f));
                Assert.That(restored.DriftElapsed, Is.EqualTo(checkpoints[index].DriftElapsed).Within(.0001f));
                Assert.That(restored.WorldTime, Is.EqualTo(checkpoints[index].WorldTime).Within(.0001f));
                Assert.That(restored.SubtitleIndex, Is.EqualTo(checkpoints[index].SubtitleIndex));
                Assert.That(restored.SubtitleElapsed, Is.EqualTo(checkpoints[index].SubtitleElapsed).Within(.0001f));
                Assert.That(Vector3.Distance(room.PanoramaOrigin, authoredEye), Is.LessThan(.01f),
                    "Loading a pillow camera pose must not redefine the doorway's authored standing eye.");
                Assert.That(Vector3.Distance(room.Cubemap.CaptureCamera.transform.position, authoredEye), Is.LessThan(.01f));
                Assert.That(Vector3.Distance((Vector3)room.RoomSurface.Material.GetVector("_ProjectionOrigin"), authoredEye), Is.LessThan(.01f));
                Assert.That(room.Cubemap.CapturedFrames, Is.EqualTo(1));
            }
        }

        private string SaveTemporaryEnding(MiniGameSessionHost session)
        {
            var path = Path.Combine(Path.GetTempPath(), "minigame-static-room-" + Guid.NewGuid().ToString("N") + ".json");
            saveFiles.Add(path);
            session.SaveTo(path);
            Assert.That(session.LastMessage, Does.Contain("已保存"));
            Assert.That(File.Exists(path), Is.True);
            return path;
        }

        private static void DriveToSelfReview(GameRuntime runtime)
        {
            for (int step = 0; step < 2000; step++)
            {
                var view = runtime.GetView();
                if (view.Game.Phase == GamePhase.SelfReview) return;
                RuntimeResult result;
                if (view.Cue != null) result = runtime.CompleteCue(view.Cue.Token);
                else if (view.TerminalPage == TerminalPage.Archive) result = runtime.LeaveTerminal();
                else if (view.TerminalPage == TerminalPage.Directory)
                    result = runtime.EnterSelectedPatient(view.Patients.First(patient => patient.CanReceiveToday).Id);
                else if (view.Input == InputContext.Dialogue)
                    result = view.Game.EncounterStage == EncounterStage.AwaitingDisposition
                        ? runtime.SubmitDisposition(view.Game.CurrentEncounterId,
                            view.Game.RemainingPurificationQuota > 0 && view.Game.AvailableDispositions.Contains(Disposition.Purify)
                                ? Disposition.Purify : view.Game.AvailableDispositions.Contains(Disposition.Treat)
                                    ? Disposition.Treat : view.Game.AvailableDispositions.First())
                        : runtime.SelectResponse(view.Game.CurrentEncounterId, view.Game.CurrentNodeId, view.Game.AvailableResponseIds.First());
                else if (view.Game.Phase == GamePhase.Evening) result = runtime.BeginNightDream();
                else if (view.Game.Phase == GamePhase.AfterDream) result = runtime.TryInteract(InteractionTarget.Bed);
                else if (view.Game.Phase == GamePhase.MorningInspection) result = runtime.TryInteract(InteractionTarget.Door);
                else if (view.Game.Phase == GamePhase.Apartment || view.Game.Phase == GamePhase.Work)
                    result = runtime.TryInteract(InteractionTarget.Computer);
                else { Assert.Fail("No accepted campaign route from " + view.Game.Phase); return; }
                Assert.That(result.Accepted, Is.True, "Campaign fixture rejected command at step " + step);
            }
            Assert.Fail("Campaign fixture exceeded its 2000-command bound.");
        }

        private static void AssertPhysicalBedParallax(RoomDreamEnding value)
        {
            var camera = value.ViewCamera;
            var originalPose = value.Ending.CaptureCheckpoint();
            float originalAspect = camera.aspect;
            var bedParts = value.Ending.Bed.GetComponentsInChildren<Renderer>();
            var bedBounds = bedParts[0].bounds;
            foreach (var part in bedParts) bedBounds.Encapsulate(part.bounds);
            var directions = new[] { Vector3.right, Vector3.left, Vector3.forward, Vector3.back };
            var limits = value.MappedRoomBounds;
            float[] clearances =
            {
                limits.max.x - bedBounds.max.x, bedBounds.min.x - limits.min.x,
                limits.max.z - bedBounds.max.z, bedBounds.min.z - limits.min.z
            };
            int side = Array.IndexOf(clearances, clearances.Max());
            float extent = side < 2 ? bedBounds.extents.x : bedBounds.extents.z;
            Assert.That(clearances[side], Is.GreaterThan(1.5f), "The real room must provide room to approach the bed.");
            try
            {
                camera.aspect = 1;
                var far = bedBounds.center + directions[side] * (extent + clearances[side] - .6f);
                var near = bedBounds.center + directions[side] * (extent + .65f);
                far.y = near.y = originalPose.RoomPosition.y;
                LookAtBedFrom(value, originalPose, far, bedBounds.center);
                float farDistance = Vector3.Distance(camera.transform.position, bedBounds.center);
                int farBedPixels = CountRenderedBedPixels(camera, bedParts);
                LookAtBedFrom(value, originalPose, near, bedBounds.center);
                float nearDistance = Vector3.Distance(camera.transform.position, bedBounds.center);
                int nearBedPixels = CountRenderedBedPixels(camera, bedParts);
                Assert.That(farDistance - nearDistance, Is.GreaterThan(.5f));
                Assert.That(farBedPixels, Is.GreaterThan(25), "The physical bed must be visible in the far camera render.");
                Assert.That(nearBedPixels, Is.GreaterThan(farBedPixels * 1.3f),
                    "Approaching the real bed must enlarge its rendered silhouette independently of the static wallpaper. "
                    + "Far pixels=" + farBedPixels + ", near pixels=" + nearBedPixels);
            }
            finally
            {
                camera.aspect = originalAspect;
                value.RestorePose(originalPose);
            }
        }

        private static void LookAtBedFrom(RoomDreamEnding value, BeachEndingCheckpoint template, Vector3 position, Vector3 target)
        {
            var pose = JsonUtility.FromJson<BeachEndingCheckpoint>(JsonUtility.ToJson(template));
            pose.RoomPosition = position;
            value.RestorePose(pose);
            var direction = target - value.ViewCamera.transform.position;
            pose.RoomYaw = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            pose.RoomPitch = -Mathf.Atan2(direction.y, new Vector2(direction.x, direction.z).magnitude) * Mathf.Rad2Deg;
            value.RestorePose(pose);
        }

        private static int CountRenderedBedPixels(Camera camera, Renderer[] bedParts)
        {
            var visible = ReadView(camera);
            Texture2D background = null;
            var enabled = bedParts.Select(part => part.enabled).ToArray();
            try
            {
                foreach (var part in bedParts) part.enabled = false;
                background = ReadView(camera);
                var foregroundPixels = visible.GetPixels32();
                var backgroundPixels = background.GetPixels32();
                int count = 0;
                for (int index = 0; index < foregroundPixels.Length; index++)
                {
                    var a = foregroundPixels[index]; var b = backgroundPixels[index];
                    if (Mathf.Max(Mathf.Abs(a.r - b.r), Mathf.Max(Mathf.Abs(a.g - b.g), Mathf.Abs(a.b - b.b))) > 12)
                        count++;
                }
                return count;
            }
            finally
            {
                for (int index = 0; index < bedParts.Length; index++) bedParts[index].enabled = enabled[index];
                Object.Destroy(visible);
                if (background != null) Object.Destroy(background);
            }
        }

        private static Texture2D ReadRoomBackground(RoomDreamEnding value)
        {
            var bedParts = value.Ending.Bed.GetComponentsInChildren<Renderer>();
            var enabled = bedParts.Select(part => part.enabled).ToArray();
            try
            {
                foreach (var part in bedParts) part.enabled = false;
                return ReadView(value.ViewCamera);
            }
            finally
            {
                for (int index = 0; index < bedParts.Length; index++) bedParts[index].enabled = enabled[index];
            }
        }

        private static int CountChangedPixels(Texture2D first, Texture2D second, int threshold)
        {
            var before = first.GetPixels32();
            var after = second.GetPixels32();
            int count = 0;
            for (int index = 0; index < before.Length; index++)
            {
                var a = before[index]; var b = after[index];
                if (Mathf.Max(Mathf.Abs(a.r - b.r), Mathf.Max(Mathf.Abs(a.g - b.g), Mathf.Abs(a.b - b.b))) > threshold)
                    count++;
            }
            return count;
        }

        private static Vector3 RayExitRoom(Vector3 origin, Vector3 direction, Bounds bounds)
        {
            float distance = float.PositiveInfinity;
            for (int axis = 0; axis < 3; axis++)
            {
                if (Mathf.Abs(direction[axis]) < .000001f) continue;
                float boundary = direction[axis] > 0 ? bounds.max[axis] : bounds.min[axis];
                distance = Mathf.Min(distance, (boundary - origin[axis]) / direction[axis]);
            }
            return origin + direction * distance;
        }

        private static Texture2D ReadCubeFace(RenderTexture cube, CubemapFace face)
        {
            var previous = RenderTexture.active;
            try
            {
                Graphics.SetRenderTarget(cube, 0, face);
                var image = new Texture2D(cube.width, cube.height, TextureFormat.RGBA32, false);
                image.ReadPixels(new Rect(0, 0, cube.width, cube.height), 0, 0); image.Apply();
                return image;
            }
            finally { RenderTexture.active = previous; }
        }

        private static void ApproachBed(RoomDreamEnding value)
        {
            for (int i = 0; i < 500 && !value.Ending.CanLieDown; i++)
            {
                var direction = value.Ending.Bed.position - value.PlayerPosition;
                var pose = value.Ending.CaptureCheckpoint();
                pose.RoomYaw = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
                pose.RoomPitch = 0;
                value.RestorePose(pose);
                value.AdvanceExploration(Vector2.up, Vector2.zero, .025f);
            }
        }

        private GameObject CreateOrientationSource()
        {
            var sourceObject = Own(new GameObject("Orientation source"));
            sourceObject.SetActive(false);
            var source = sourceObject.AddComponent<DreamCubemapSource>();
            source.SkyMaterial = Own(new Material(Resources.Load<Shader>("DreamCloud")));
            source.StandingPose = new GameObject("Standing").transform; source.StandingPose.SetParent(sourceObject.transform, false);
            var pivot = new GameObject("Virtual bed pivot").transform; pivot.SetParent(sourceObject.transform, false);
            source.PillowPose = new GameObject("Pillow").transform; source.PillowPose.SetParent(pivot, false);
            var shader = Shader.Find("Unlit/Color"); Assert.That(shader, Is.Not.Null);
            var colors = new[] { Color.red, Color.green, Color.blue, Color.yellow };
            var names = new[] { "TL", "TR", "BL", "BR" };
            for (int face = 0; face < Directions.Length; face++)
                for (int quadrant = 0; quadrant < 4; quadrant++)
                {
                    var marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    marker.name = "Face" + face + "-" + names[quadrant];
                    marker.transform.SetParent(sourceObject.transform, false);
                    var rotation = FaceRotation(face);
                    marker.transform.localPosition = rotation * new Vector3(quadrant % 2 == 0 ? -1.35f : 1.35f,
                        quadrant < 2 ? 1.35f : -1.35f, 4);
                    marker.transform.localRotation = rotation;
                    marker.transform.localScale = new Vector3(.8f, .8f, .03f);
                    marker.GetComponent<Renderer>().sharedMaterial = Own(new Material(shader) { color = colors[quadrant] });
                }
            return sourceObject;
        }

        private static readonly Vector3[] Directions = { Vector3.forward, Vector3.back, Vector3.left, Vector3.right, Vector3.up, Vector3.down };
        private static Quaternion FaceRotation(int face) => Quaternion.LookRotation(Directions[face],
            face == 4 ? Vector3.forward : face == 5 ? Vector3.back : Vector3.up);
        private T Own<T>(T value) where T : Object { owned.Add(value); return value; }
        private static IEnumerator Until(Func<bool> ready, float seconds)
        {
            float deadline = Time.realtimeSinceStartup + seconds;
            while (!ready() && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.That(ready(), Is.True, "Room cubemap ending did not become ready.");
        }
        private static Texture2D ReadView(Camera camera)
        {
            var target = RenderTexture.GetTemporary(128, 128, 24, RenderTextureFormat.ARGB32);
            var previousTarget = camera.targetTexture;
            var previousActive = RenderTexture.active;
            try
            {
                camera.targetTexture = target; camera.Render(); RenderTexture.active = target;
                var image = new Texture2D(128, 128, TextureFormat.RGBA32, false);
                image.ReadPixels(new Rect(0, 0, 128, 128), 0, 0); image.Apply();
                return image;
            }
            finally { camera.targetTexture = previousTarget; RenderTexture.active = previousActive; RenderTexture.ReleaseTemporary(target); }
        }
        private static void AssertRenderedPoint(Camera camera, Vector3 point, Color expected, string label)
        {
            var direction = (point - camera.transform.position).normalized;
            camera.transform.rotation = Quaternion.LookRotation(direction,
                Mathf.Abs(Vector3.Dot(direction, Vector3.up)) > .99f ? Vector3.forward : Vector3.up);
            var image = ReadView(camera);
            try { AssertColor(image.GetPixel(64, 64), expected, label); }
            finally { Object.Destroy(image); }
        }
        private static void AssertColor(Color actual, Color expected, string label)
        {
            Assert.That(Mathf.Abs(actual.r - expected.r), Is.LessThan(.2f), label + " red");
            Assert.That(Mathf.Abs(actual.g - expected.g), Is.LessThan(.2f), label + " green");
            Assert.That(Mathf.Abs(actual.b - expected.b), Is.LessThan(.2f), label + " blue");
        }
    }
}
