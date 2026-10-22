using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MiniGame.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace MiniGame.UnityHost.Tests
{
    public sealed class PropNarrationPlayModeTests
    {
        [UnityTest]
        public IEnumerator ReadingOwnsInputPagesAndPauseWithoutChangingGameState()
        {
            yield return SceneManager.LoadSceneAsync("MainRoomCombined");
            yield return null;
            var host = Stage();
            var prop = Object.FindObjectsOfType<ReadableProp>().Single(item => item.Id == "poster-beach");
            prop.Configure(prop.Id, "这里是我的故乡。\n但我从没去过海边。", prop.Surface);
            string checkpoint = host.Runtime.Save();
            int originalMask = host.Player.ViewCamera.cullingMask;
            Assert.That(host.Player.InteractPrimaryFocused(), Is.True);
            Assert.That(host.Screen.AdvancePropNarration(), Is.False, "Opening click must not consume the first page.");
            Assert.That(host.Screen.PropNarration.Dismiss(), Is.False, "The opening frame must not also dismiss the reading.");
            host.HandleEscape();
            Assert.That(host.Runtime.GetView().Paused, Is.False, "The reading consumes opening-frame Escape without pausing.");
            Assert.That(host.Screen.PropNarration.IsOpen, Is.True);
            Assert.That(host.Player.InteractFocused(), Is.False);
            Assert.That(host.Player.InteractPrimaryFocused(), Is.False);
            yield return new WaitForSecondsRealtime(.3f);
            var narration = host.Screen.PropNarration;
            Assert.That(narration.GetComponentInChildren<Canvas>().renderMode, Is.EqualTo(RenderMode.WorldSpace));
            var label = NarrationText(narration);
            Assert.That(label.text, Is.EqualTo("这里是我的故乡。"));
            Assert.That(label.color, Is.EqualTo(Color.white), "Narration must use white ink on its black background.");
            Assert.That(label.fontSize, Is.GreaterThanOrEqualTo(48), "Interaction text must keep its enlarged reading size.");
            Assert.That(label.fontStyle, Is.EqualTo(FontStyle.Bold), "Interaction text must use bold white lettering.");
            AssertContinueReadingHint(narration, true);
            AssertTextFitsPanel(narration);
            var edges = narration.GetComponentsInChildren<Image>(true).Where(image => image.name.StartsWith("Frame ")).ToArray();
            Assert.That(edges, Is.Empty, "Reading panels must have no border geometry.");
            var background = narration.GetComponentsInChildren<Image>(true).Single(image => image.name == "Reading background");
            Assert.That(background.color, Is.EqualTo(Color.black), "Reading panels must use a fully opaque black background.");
            Assert.That(background.raycastTarget, Is.False, "The background must not consume reading input.");
            Assert.That(background.rectTransform.rect.size, Is.EqualTo(ReadingPanel(narration).rect.size),
                "The black background must cover the complete reading panel.");
            Assert.That(narration.GetComponentsInChildren<Transform>(true).Any(item => item.name == "Fading rule" || item.name == "Reading controls"), Is.False);
            var textPoint = host.Player.ViewCamera.WorldToViewportPoint(narration.WorldPosition);
            Assert.That(textPoint.z, Is.GreaterThan(.1f));
            AssertPanelBesideProp(host, prop.Surface);
            Assert.That(narration.Opacity, Is.EqualTo(1).Within(.001f), "The settled reading panel must be opaque.");
            yield return PrototypePlayModeTests.CaptureIfRequested(host, "poster-borderless-black-reading");
            Assert.That(host.Runtime.SetPaused(true).Accepted, Is.True);
            Assert.That(host.Screen.AdvancePropNarration(), Is.False);
            Assert.That(narration.IsOpen, Is.True);
            Assert.That(narration.Visible, Is.False);
            yield return new WaitForSecondsRealtime(.3f);
            Assert.That(host.Runtime.SetPaused(false).Accepted, Is.True);
            Assert.That(host.Screen.AdvancePropNarration(), Is.False);
            yield return null;
            Assert.That(host.Screen.AdvancePropNarration(), Is.True);
            Assert.That(host.Screen.CurrentRoomCaption, Is.EqualTo("但我从没去过海边。"));
            AssertContinueReadingHint(narration, false);
            Assert.That(host.Screen.AdvancePropNarration(), Is.False, "One input cannot skip two pages.");
            Assert.That(narration.Dismiss(), Is.False, "A click shared with dismissal must advance only once.");
            yield return new WaitForSecondsRealtime(.3f);
            Assert.That(host.Screen.AdvancePropNarration(), Is.True);
            Assert.That(host.Screen.AdvancePropNarration(), Is.False, "Closing cannot accept another page advance.");
            Assert.That(narration.Dismiss(), Is.False, "Closing cannot accept another dismissal.");
            host.HandleEscape();
            Assert.That(host.Runtime.GetView().Paused, Is.False, "Escape during closing belongs to the reading, not the pause menu.");
            Assert.That(host.Player.InteractPrimaryFocused(), Is.False);
            yield return new WaitForSecondsRealtime(.35f);
            yield return null;
            Assert.That(narration.IsOpen, Is.False);
            Assert.That(host.Player.ViewCamera.cullingMask, Is.EqualTo(originalMask));
            Assert.That(host.Runtime.GetView().Input, Is.EqualTo(InputContext.World));
            Assert.That(host.Runtime.Save(), Is.EqualTo(checkpoint), "Reading must not consume rules, rewards or inventory.");
        }

        [UnityTest]
        public IEnumerator ReadingPanelKeepsSmallRightMarginAndFollowsCameraYawAndPitch()
        {
            yield return SceneManager.LoadSceneAsync("MainRoomCombined");
            yield return null;
            var host = Stage();
            var prop = Object.FindObjectsOfType<ReadableProp>().Single(item => item.Id == "poster-beach");
            var camera = host.Player.ViewCamera;
            string checkpoint = host.Runtime.Save();
            Assert.That(host.Player.InteractPrimaryFocused(), Is.True);
            yield return new WaitForSecondsRealtime(.3f);
            var narration = host.Screen.PropNarration;
            string caption = narration.CurrentText;
            int pageCount = narration.PageCount;
            AssertPanelBesideProp(host, prop.Surface);
            var pinnedBounds = ProjectPanel(camera, ReadingPanel(narration));
            var initialRotation = camera.transform.rotation;
            foreach (var turn in new[] { new Vector2(-12, 8), new Vector2(10, -7) })
            {
                var direction = initialRotation * Quaternion.Euler(turn.y, turn.x, 0) * Vector3.forward;
                host.Player.FacePoint(camera.transform.position + direction * 4);
                // Coroutines resume before LateUpdate; allow one complete camera-follow frame.
                yield return null;
                yield return null;
                Assert.That(Quaternion.Angle(camera.transform.rotation, initialRotation), Is.GreaterThan(5));
                AssertPanelBoundsEqual(camera, narration, pinnedBounds);
                Assert.That(narration.Visible, Is.True, "A later camera adjustment cannot hide an already placed reading page.");
                AssertTextFitsPanel(narration);
                Assert.That(narration.CurrentText, Is.EqualTo(caption), "Looking around must not advance the reading.");
                Assert.That(narration.PageCount, Is.EqualTo(pageCount));
            }
            host.Player.FacePoint(camera.transform.position + initialRotation * Vector3.forward * 4);
            yield return null;
            yield return null;
            AssertPanelBesideProp(host, prop.Surface);
            AssertPanelBoundsEqual(camera, narration, pinnedBounds);
            yield return PrototypePlayModeTests.CaptureIfRequested(host, "poster-following-camera-right-edge");
            Assert.That(host.Runtime.Save(), Is.EqualTo(checkpoint), "Camera-follow reading must preserve the session.");
        }

        [UnityTest]
        public IEnumerator ContinuousCameraTurnsKeepReadingViewportBoundsStableAndVisible()
        {
            yield return SceneManager.LoadSceneAsync("MainRoomCombined");
            yield return null;
            var host = Stage();
            var prop = Object.FindObjectsOfType<ReadableProp>().Single(item => item.Id == "poster-beach");
            var originalSurface = prop.Surface;
            string originalCaption = prop.Caption;
            var camera = host.Player.ViewCamera;
            host.Player.FacePoint(camera.transform.position + Vector3.forward * 4);
            var initialRotation = camera.transform.rotation;
            var obstruction = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obstruction.name = "Continuous reading camera test surface";
            try
            {
                float worldHeight = 4 * Mathf.Tan(camera.fieldOfView * .5f * Mathf.Deg2Rad);
                float worldWidth = worldHeight * camera.aspect;
                obstruction.transform.position = camera.ViewportToWorldPoint(new Vector3(.4f, .5f, 2));
                obstruction.transform.localScale = new Vector3(worldWidth * .24f, worldHeight * .8f, .01f);
                prop.Configure(prop.Id, "海报里的小镇是我的故乡...但我从没去过海边", obstruction.GetComponent<Renderer>());
                Physics.SyncTransforms();
                Assert.That(host.Screen.ShowPropNarration(prop, camera), Is.True);
                yield return new WaitForSecondsRealtime(.3f);
                var narration = host.Screen.PropNarration;
                var panel = ReadingPanel(narration);
                var initialBounds = ProjectPanel(camera, panel);
                string caption = narration.CurrentText;
                int pageCount = narration.PageCount;
                var samples = new List<CameraReadingSample>();
                const int steps = 96;
                for (int index = 0; index <= steps; index++)
                {
                    float progress = index / (float)steps;
                    float yaw = Mathf.Sin(progress * Mathf.PI * 2) * 32;
                    float pitch = Mathf.Sin(progress * Mathf.PI * 4) * 14;
                    var direction = initialRotation * Quaternion.Euler(pitch, yaw, 0) * Vector3.forward;
                    host.Player.FacePoint(camera.transform.position + direction * 4);
                    yield return null;
                    yield return null;
                    var corners = new Vector3[4];
                    panel.GetWorldCorners(corners);
                    var projected = corners.Select(camera.WorldToViewportPoint).ToArray();
                    var bounds = Rect.MinMaxRect(projected.Min(point => point.x), projected.Min(point => point.y),
                        projected.Max(point => point.x), projected.Max(point => point.y));
                    samples.Add(new CameraReadingSample
                    {
                        frame = index, yaw = yaw, pitch = pitch, visible = narration.Visible,
                        right = bounds.xMax, centerY = bounds.center.y, width = bounds.width, height = bounds.height,
                        pageUnchanged = narration.CurrentText == caption && narration.PageCount == pageCount
                    });
                }
                var report = new CameraReadingReport
                {
                    samples = samples.ToArray(),
                    hiddenFrames = samples.Count(sample => !sample.visible),
                    maxRightChange = samples.Max(sample => Mathf.Abs(sample.right - initialBounds.xMax)),
                    maxCenterYChange = samples.Max(sample => Mathf.Abs(sample.centerY - initialBounds.center.y)),
                    maxWidthChange = samples.Max(sample => Mathf.Abs(sample.width - initialBounds.width)),
                    maxHeightChange = samples.Max(sample => Mathf.Abs(sample.height - initialBounds.height))
                };
                string captureDirectory = System.Environment.GetEnvironmentVariable("MINIGAME_CAPTURE_DIR");
                string directory = string.IsNullOrEmpty(captureDirectory)
                    ? System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath, "../../Artifacts/ReadingStability20261021"))
                    : System.IO.Path.GetDirectoryName(System.IO.Path.GetFullPath(captureDirectory));
                System.IO.Directory.CreateDirectory(directory);
                string reportPath = System.IO.Path.Combine(directory, "fixed-metrics.json");
                System.IO.File.WriteAllText(reportPath, JsonUtility.ToJson(report, true));
                Debug.Log($"Reading camera bounds: right={report.maxRightChange:F5}, centerY={report.maxCenterYChange:F5}, "
                    + $"width={report.maxWidthChange:F5}, height={report.maxHeightChange:F5}, hidden={report.hiddenFrames}; {reportPath}");
                Assert.That(samples.All(sample => sample.pageUnchanged), Is.True, "Camera motion must preserve the same reading page.");
                Assert.That(report.hiddenFrames, Is.Zero, "Projection overlap must never hide an open reading page.");
                Assert.That(report.maxRightChange, Is.LessThan(.002f), "The right margin must follow the camera during every frame.");
                Assert.That(report.maxCenterYChange, Is.LessThan(.002f), "A page must not jump vertically while the camera turns.");
                Assert.That(report.maxWidthChange, Is.LessThan(.002f), "A page must not shrink or expand while the camera turns.");
                Assert.That(report.maxHeightChange, Is.LessThan(.002f), "A page must keep its viewport height while the camera turns.");
            }
            finally
            {
                prop.Configure(prop.Id, originalCaption, originalSurface);
                Object.Destroy(obstruction);
            }
        }

        [System.Serializable]
        private sealed class CameraReadingSample
        {
            public int frame;
            public float yaw, pitch, right, centerY, width, height;
            public bool visible, pageUnchanged;
        }

        [System.Serializable]
        private sealed class CameraReadingReport
        {
            public CameraReadingSample[] samples;
            public int hiddenFrames;
            public float maxRightChange, maxCenterYChange, maxWidthChange, maxHeightChange;
        }

        [UnityTest]
        public IEnumerator PinnedReadingStaysVisibleThroughObstructionAndCanFinish()
        {
            yield return SceneManager.LoadSceneAsync("MainRoomCombined");
            yield return null;
            var host = Stage();
            var prop = Object.FindObjectsOfType<ReadableProp>().Single(item => item.Id == "poster-beach");
            var originalSurface = prop.Surface;
            string originalCaption = prop.Caption;
            prop.Configure(prop.Id, "第一段留在海边。\n第二段不会因遮挡而丢失。", originalSurface);
            Assert.That(host.Player.InteractPrimaryFocused(), Is.True);
            yield return new WaitForSecondsRealtime(.3f);
            Assert.That(host.Screen.AdvancePropNarration(), Is.True);
            yield return new WaitForSecondsRealtime(.3f);
            var narration = host.Screen.PropNarration;
            string secondPage = narration.CurrentText;
            Assert.That(secondPage, Is.EqualTo("第二段不会因遮挡而丢失。"));
            int pageCount = narration.PageCount;
            var camera = host.Player.ViewCamera;
            var pinnedBounds = ProjectPanel(camera, ReadingPanel(narration));
            var obstruction = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obstruction.name = "Reading placement test surface";
            try
            {
                host.Player.FacePoint(camera.transform.position + Vector3.forward * 4);
                float worldHeight = 4 * Mathf.Tan(camera.fieldOfView * .5f * Mathf.Deg2Rad);
                float worldWidth = worldHeight * camera.aspect;
                obstruction.transform.position = camera.ViewportToWorldPoint(new Vector3(.9f, .5f, 2));
                obstruction.transform.localScale = new Vector3(worldWidth * .24f, worldHeight * .3f, .01f);
                var surface = obstruction.GetComponent<Renderer>();
                prop.Configure(prop.Id, prop.Caption, surface);
                Physics.SyncTransforms();
                yield return null;
                yield return null;
                Assert.That(narration.Visible, Is.True, "An existing page must remain visible when its source enters the pinned rectangle.");
                AssertPanelBoundsEqual(camera, narration, pinnedBounds);
                Assert.That(narration.CurrentText, Is.EqualTo(secondPage));

                var initialRotation = camera.transform.rotation;
                foreach (var turn in new[] { new Vector2(-6, 4), new Vector2(4, -3) })
                {
                    var direction = initialRotation * Quaternion.Euler(turn.y, turn.x, 0) * Vector3.forward;
                    host.Player.FacePoint(camera.transform.position + direction * 4);
                    yield return null;
                    yield return null;
                    Assert.That(narration.Visible, Is.True, "Projection overlap during a camera adjustment cannot hide the reading.");
                    AssertPanelBoundsEqual(camera, narration, pinnedBounds);
                    Assert.That(narration.CurrentText, Is.EqualTo(secondPage), "Camera adjustments must preserve the page.");
                    Assert.That(narration.PageCount, Is.EqualTo(pageCount));
                }

                obstruction.transform.position = camera.ViewportToWorldPoint(new Vector3(.5f, .5f, 2));
                obstruction.transform.localScale = new Vector3(worldWidth * 1.2f, worldHeight * 1.2f, .01f);
                Physics.SyncTransforms();
                Assert.That(narration.CurrentText, Is.EqualTo(secondPage));
                yield return null;
                yield return null;
                Assert.That(narration.IsOpen, Is.True, "No free screen area must not dismiss the reading.");
                Assert.That(narration.Visible, Is.True, "No free screen area must not hide or lock an existing page.");
                Assert.That(narration.CurrentText, Is.EqualTo(secondPage));
                Assert.That(narration.PageCount, Is.EqualTo(pageCount));
                AssertPanelBoundsEqual(camera, narration, pinnedBounds);
                AssertTextFitsPanel(narration);

                Assert.That(host.Screen.AdvancePropNarration(), Is.True, "A visible page can finish while its source covers the viewport.");
                Assert.That(host.Screen.AdvancePropNarration(), Is.False);
                Assert.That(narration.Dismiss(), Is.False);
                yield return new WaitForSecondsRealtime(.35f);
                Assert.That(narration.IsOpen, Is.False);
                Assert.That(host.Runtime.GetView().Input, Is.EqualTo(InputContext.World));

                obstruction.transform.position = camera.ViewportToWorldPoint(new Vector3(.25f, .5f, 2));
                obstruction.transform.localScale = new Vector3(worldWidth * .1f, worldHeight * .3f, .01f);
                Physics.SyncTransforms();
                yield return null;
                yield return null;
                Assert.That(host.Screen.ShowPropNarration(prop, camera), Is.True, "Closing an obstructed page must release input for another reading.");
                yield return new WaitForSecondsRealtime(.3f);
                AssertPanelBesideProp(host, surface);
                Assert.That(narration.CurrentText, Is.EqualTo("第一段留在海边。"));
                Assert.That(narration.PageCount, Is.EqualTo(pageCount));
            }
            finally
            {
                prop.Configure(prop.Id, originalCaption, originalSurface);
                Object.Destroy(obstruction);
            }
        }

        [UnityTest]
        public IEnumerator InitiallyRightSidePropChoosesClearVerticalDockOnlyOnce()
        {
            yield return SceneManager.LoadSceneAsync("MainRoomCombined");
            yield return null;
            var host = Stage();
            var prop = Object.FindObjectsOfType<ReadableProp>().Single(item => item.Id == "poster-beach");
            var originalSurface = prop.Surface;
            string originalCaption = prop.Caption;
            var camera = host.Player.ViewCamera;
            host.Player.FacePoint(camera.transform.position + Vector3.forward * 4);
            var obstruction = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obstruction.name = "Initial reading dock test surface";
            try
            {
                float worldHeight = 4 * Mathf.Tan(camera.fieldOfView * .5f * Mathf.Deg2Rad);
                float worldWidth = worldHeight * camera.aspect;
                obstruction.transform.position = camera.ViewportToWorldPoint(new Vector3(.9f, .5f, 2));
                obstruction.transform.localScale = new Vector3(worldWidth * .24f, worldHeight * .3f, .01f);
                var surface = obstruction.GetComponent<Renderer>();
                prop.Configure(prop.Id, "海报里的小镇是我的故乡...但我从没去过海边", surface);
                Physics.SyncTransforms();
                Assert.That(host.Screen.ShowPropNarration(prop, camera), Is.True);
                yield return new WaitForSecondsRealtime(.3f);
                var narration = host.Screen.PropNarration;
                var pinnedBounds = ProjectPanel(camera, ReadingPanel(narration));
                var sourceBounds = ProjectBounds(camera, surface.bounds);
                AssertPanelBesideProp(host, surface);
                Assert.That(sourceBounds.xMax, Is.GreaterThan(1));
                Assert.That(pinnedBounds.yMin >= sourceBounds.yMax || pinnedBounds.yMax <= sourceBounds.yMin, Is.True,
                    "An initially obstructed right edge must choose clear space above or below before displaying the page.");
                yield return PrototypePlayModeTests.CaptureIfRequested(host, "poster-initial-vertical-dock");

                obstruction.transform.position = camera.ViewportToWorldPoint(new Vector3(.3f, .5f, 2));
                Physics.SyncTransforms();
                yield return null;
                yield return null;
                AssertPanelBesideProp(host, surface);
                AssertPanelBoundsEqual(camera, narration, pinnedBounds);
                Assert.That(narration.CurrentText, Is.EqualTo("海报里的小镇是我的故乡...但我从没去过海边"));
            }
            finally
            {
                prop.Configure(prop.Id, originalCaption, originalSurface);
                Object.Destroy(obstruction);
            }
        }

        [UnityTest]
        public IEnumerator OversizedSourceKeepsFallbackReadableAndDismissRestoresWorldInput()
        {
            yield return SceneManager.LoadSceneAsync("MainRoomCombined");
            yield return null;
            var host = Stage();
            var prop = Object.FindObjectsOfType<ReadableProp>().Single(item => item.Id == "poster-beach");
            var originalSurface = prop.Surface;
            string originalCaption = prop.Caption;
            string checkpoint = host.Runtime.Save();
            var camera = host.Player.ViewCamera;
            var obstruction = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obstruction.name = "Reading fallback test surface";
            try
            {
                float height = 4 * Mathf.Tan(camera.fieldOfView * .5f * Mathf.Deg2Rad);
                obstruction.transform.position = camera.ViewportToWorldPoint(new Vector3(.5f, .5f, 2));
                obstruction.transform.localScale = new Vector3(height * camera.aspect * 1.4f, height * 1.4f, .01f);
                prop.Configure(prop.Id, "视野里没有空位，也必须能读完并离开。", obstruction.GetComponent<Renderer>());
                Physics.SyncTransforms();
                Assert.That(host.Screen.ShowPropNarration(prop, camera), Is.True);
                Assert.That(host.Screen.AdvancePropNarration(), Is.False);
                Assert.That(host.Screen.PropNarration.Dismiss(), Is.False, "Opening and dismissal cannot share an input frame.");
                host.HandleEscape();
                Assert.That(host.Runtime.GetView().Paused, Is.False);
                var position = host.Player.transform.position;
                var rotation = camera.transform.rotation;
                host.Player.enabled = true;
                yield return new WaitForSecondsRealtime(.3f);
                var narration = host.Screen.PropNarration;
                Assert.That(narration.Visible, Is.True, "The fallback must be visible even when the source fills the screen.");
                Assert.That(narration.CurrentText, Is.EqualTo("视野里没有空位，也必须能读完并离开。"));
                Assert.That(narration.Opacity, Is.EqualTo(1).Within(.001f));
                Assert.That(host.Screen.PropNarrationBlocksWorld, Is.True);
                Debug.Log("Reading cursor state: frame=" + Time.frameCount + " focused=" + Application.isFocused
                    + " input=" + host.Runtime.GetView().Input + " propBlocks=" + host.Screen.PropNarrationBlocksWorld
                    + " playerEnabled=" + host.Player.isActiveAndEnabled + " visible=" + Cursor.visible + " lock=" + Cursor.lockState);
                Assert.That(Cursor.visible, Is.True);
                Assert.That(Cursor.lockState, Is.EqualTo(CursorLockMode.None));
                Assert.That(Vector3.Distance(host.Player.transform.position, position), Is.LessThan(.001f));
                Assert.That(Quaternion.Angle(camera.transform.rotation, rotation), Is.LessThan(.01f));
                var bounds = ProjectPanel(camera, ReadingPanel(narration));
                Assert.That(bounds.xMin, Is.GreaterThanOrEqualTo(-.001f));
                Assert.That(bounds.xMax, Is.LessThanOrEqualTo(1.001f));
                Assert.That(bounds.yMin, Is.GreaterThanOrEqualTo(-.001f));
                Assert.That(bounds.yMax, Is.LessThanOrEqualTo(1.001f));
                AssertTextFitsPanel(narration);
                Assert.That(host.Player.InteractFocused(), Is.False);
                Assert.That(host.Player.InteractPrimaryFocused(), Is.False);
                Assert.That(narration.Dismiss(), Is.True);
                Assert.That(narration.Dismiss(), Is.False, "Only the first dismissal may enter closing.");
                Assert.That(host.Screen.AdvancePropNarration(), Is.False);
                host.HandleEscape();
                Assert.That(host.Runtime.GetView().Paused, Is.False, "Repeated closing input must not open pause.");
                float deadline = Time.realtimeSinceStartup + 2;
                while (narration.IsOpen && Time.realtimeSinceStartup < deadline) yield return null;
                Assert.That(narration.IsOpen, Is.False);
                host.HandleEscape();
                Assert.That(host.Runtime.GetView().Paused, Is.False, "The final closing frame still consumes Escape.");
                Assert.That(host.Screen.ShowPropNarration(prop, camera), Is.False, "The closing frame cannot immediately reopen the same reading.");
                yield return null;
                yield return null;
                var restored = host.Runtime.GetView();
                bool sceneChanging = MiniGameSessionHost.Current != null && MiniGameSessionHost.Current.IsTransitioning;
                bool dreamOwnsCamera = host.Dreams != null && host.Dreams.OwnsCamera;
                Debug.Log("Restored world cursor state: frame=" + Time.frameCount + " focused=" + Application.isFocused
                    + " input=" + restored.Input + " paused=" + restored.Paused + " propBlocks=" + host.Screen.PropNarrationBlocksWorld
                    + " playerEnabled=" + host.Player.isActiveAndEnabled + " cameraEnabled=" + camera.isActiveAndEnabled
                    + " sceneChanging=" + sceneChanging + " dreamOwnsCamera=" + dreamOwnsCamera
                    + " signalBlocks=" + host.Screen.SignalTuningBlocksInput + " visible=" + Cursor.visible + " lock=" + Cursor.lockState);
                Assert.That(restored.Input, Is.EqualTo(InputContext.World));
                Assert.That(restored.Paused, Is.False);
                Assert.That(host.Screen.PropNarrationBlocksWorld, Is.False);
                Assert.That(host.Player.isActiveAndEnabled, Is.True, "The actual player Update must be enabled to restore world input.");
                Assert.That(camera.isActiveAndEnabled, Is.True);
                Assert.That(sceneChanging || dreamOwnsCamera || host.Screen.SignalTuningBlocksInput, Is.False,
                    "No earlier camera or transition owner may bypass the player's world-input branch.");
                Assert.That(Cursor.visible, Is.False);
                if (Application.isFocused) Assert.That(Cursor.lockState, Is.EqualTo(CursorLockMode.Locked));
                else Debug.Log("Physical OS pointer-lock verification requires application focus; world input and cursor visibility are still asserted.");
                host.HandleEscape();
                Assert.That(host.Runtime.GetView().Paused, Is.True, "After reading closes, a fresh Escape can open pause again.");
                host.HandleEscape();
                Assert.That(host.Runtime.GetView().Paused, Is.False);
                yield return null;
                host.Player.enabled = false;
                Assert.That(host.Screen.ShowPropNarration(prop, camera), Is.True, "A later world input can open the prop again.");
                Assert.That(host.Runtime.Save(), Is.EqualTo(checkpoint));
            }
            finally
            {
                host.Player.enabled = false;
                host.Screen.PropNarration?.CloseImmediately();
                prop.Configure(prop.Id, originalCaption, originalSurface);
                Object.Destroy(obstruction);
            }
        }

        [UnityTest]
        public IEnumerator RestoreDisableAndTargetRemovalReleaseNarrationAndCameraOwnership()
        {
            yield return SceneManager.LoadSceneAsync("MainRoomCombined");
            yield return null;
            var host = Stage();
            string checkpoint = host.Runtime.Save();
            var camera = host.Player.ViewCamera;
            int originalMask = camera.cullingMask;
            Assert.That(host.Player.InteractPrimaryFocused(), Is.True);
            yield return null;
            host.RestoreCheckpoint(checkpoint);
            Assert.That(host.Screen.PropNarration.IsOpen, Is.False);
            Assert.That(camera.cullingMask, Is.EqualTo(originalMask));
            yield return null;
            host.Player.enabled = false;
            Assert.That(host.Player.InteractPrimaryFocused(), Is.True);
            yield return null;
            host.Screen.enabled = false;
            Assert.That(host.Screen.PropNarration.IsOpen, Is.False);
            host.Screen.enabled = true;
            yield return null;
            Assert.That(host.Player.InteractPrimaryFocused(), Is.True);
            camera.cullingMask &= ~(1 << 4);
            var prop = Object.FindObjectsOfType<ReadableProp>().Single(item => item.Id == "poster-beach");
            prop.gameObject.SetActive(false);
            yield return null;
            yield return null;
            Assert.That(host.Screen.PropNarration.IsOpen, Is.False);
            Assert.That(camera.cullingMask, Is.EqualTo(originalMask & ~(1 << 4)), "Cleanup preserves other systems' mask changes.");
            Assert.That(camera.GetComponent<PropNarrationComposite>().enabled, Is.False);
        }

        private static MiniGameBootstrap Stage()
        {
            var host = Object.FindObjectOfType<MiniGameBootstrap>();
            CampaignWorldFixture.Restore(host);
            host.Player.enabled = false;
            var controller = host.Player.GetComponent<CharacterController>();
            controller.enabled = false;
            host.Player.transform.position = GameObject.Find("Poster beach approach").transform.position;
            controller.enabled = true;
            var prop = Object.FindObjectsOfType<ReadableProp>().Single(item => item.Id == "poster-beach");
            host.Player.FacePoint(prop.Surface.bounds.center);
            Physics.SyncTransforms(); host.Player.RefreshInteractionFocus();
            return host;
        }

        private static RectTransform ReadingPanel(PropNarrationView narration) => narration
            .GetComponentsInChildren<RectTransform>(true).Single(item => item.name == "Spatial prop narration");

        private static Text NarrationText(PropNarrationView narration) => narration
            .GetComponentsInChildren<Text>(true).Single(item => item.name == "Narration");

        private static void AssertContinueReadingHint(PropNarrationView narration, bool expectedVisible)
        {
            var hint = narration.GetComponentsInChildren<Text>(true).Single(item => item.name == "Continue reading");
            Assert.That(hint.gameObject.activeInHierarchy, Is.EqualTo(expectedVisible),
                "Only pages with more text remaining must invite the player to continue reading.");
            if (expectedVisible)
            {
                Assert.That(hint.text, Is.EqualTo("点击继续阅读"));
                Assert.That(hint.color, Is.EqualTo(Color.white));
            }
        }

        private static void AssertPanelBesideProp(MiniGameBootstrap host, Renderer surface)
        {
            var narration = host.Screen.PropNarration;
            var camera = host.Player.ViewCamera;
            Assert.That(narration.Visible, Is.True, "A readable gap is available beside or above the prop.");
            var panel = ReadingPanel(narration);
            Assert.That(Quaternion.Angle(panel.rotation, camera.transform.rotation), Is.LessThan(.1f),
                "The reading plane must face the current camera direction.");
            var corners = new Vector3[4];
            panel.GetWorldCorners(corners);
            Assert.That(camera.WorldToViewportPoint(corners[2]).x, Is.EqualTo(.985f).Within(.001f), "Top-right panel corner must keep the requested small viewport margin.");
            Assert.That(camera.WorldToViewportPoint(corners[3]).x, Is.EqualTo(.985f).Within(.001f), "Bottom-right panel corner must keep the requested small viewport margin.");
            var frameRect = ProjectPanel(camera, panel);
            Assert.That(frameRect.xMin, Is.GreaterThanOrEqualTo(-.001f));
            Assert.That(frameRect.yMin, Is.GreaterThanOrEqualTo(-.001f));
            Assert.That(frameRect.yMax, Is.LessThanOrEqualTo(1.001f));
            Assert.That(frameRect.Overlaps(ProjectBounds(camera, surface.bounds)), Is.False,
                "The full reading frame must leave the interaction surface unobstructed.");
        }

        private static Rect ProjectPanel(Camera camera, RectTransform panel)
        {
            var corners = new Vector3[4];
            panel.GetWorldCorners(corners);
            return ProjectPoints(camera, corners);
        }

        private static void AssertPanelBoundsEqual(Camera camera, PropNarrationView narration, Rect expected)
        {
            var actual = ProjectPanel(camera, ReadingPanel(narration));
            Assert.That(actual.xMin, Is.EqualTo(expected.xMin).Within(.002f), "The page's viewport left edge must stay pinned.");
            Assert.That(actual.xMax, Is.EqualTo(expected.xMax).Within(.002f), "The page's right margin must stay pinned.");
            Assert.That(actual.yMin, Is.EqualTo(expected.yMin).Within(.002f), "The page's viewport bottom edge must stay pinned.");
            Assert.That(actual.yMax, Is.EqualTo(expected.yMax).Within(.002f), "The page's viewport top edge must stay pinned.");
        }

        private static void AssertTextFitsPanel(PropNarrationView narration)
        {
            Canvas.ForceUpdateCanvases();
            var panel = ReadingPanel(narration);
            var label = NarrationText(narration);
            label.rectTransform.ForceUpdateRectTransforms();
            var corners = new Vector3[4];
            label.rectTransform.GetWorldCorners(corners);
            foreach (var corner in corners)
            {
                var local = panel.InverseTransformPoint(corner);
                Assert.That(local.x, Is.InRange(panel.rect.xMin - .1f, panel.rect.xMax + .1f),
                    "The complete text rectangle must stay inside the black panel horizontally.");
                Assert.That(local.y, Is.InRange(panel.rect.yMin - .1f, panel.rect.yMax + .1f),
                    "The complete text rectangle must stay inside the black panel vertically.");
            }
            using var generator = new TextGenerator();
            Assert.That(generator.Populate(label.text, label.GetGenerationSettings(label.rectTransform.rect.size)), Is.True);
            Assert.That(generator.characterCountVisible, Is.GreaterThanOrEqualTo(label.text.Length),
                "Every character on the current page must fit in the actual rendered text area.");
        }

        private static Rect ProjectBounds(Camera camera, Bounds bounds)
        {
            var corners = new Vector3[8];
            for (int index = 0; index < corners.Length; index++)
                corners[index] = bounds.center + Vector3.Scale(bounds.extents, new Vector3(
                    (index & 1) == 0 ? -1 : 1, (index & 2) == 0 ? -1 : 1, (index & 4) == 0 ? -1 : 1));
            return ProjectPoints(camera, corners);
        }

        private static Rect ProjectPoints(Camera camera, Vector3[] points)
        {
            var projected = points.Select(camera.WorldToViewportPoint).ToArray();
            Assert.That(projected.All(point => point.z > camera.nearClipPlane), Is.True,
                "This placement test keeps the entire geometry in front of the camera.");
            return Rect.MinMaxRect(projected.Min(point => point.x), projected.Min(point => point.y),
                projected.Max(point => point.x), projected.Max(point => point.y));
        }

        [UnityTest]
        public IEnumerator LongParagraphRemainsCompleteAcrossReadablePages()
        {
            yield return SceneManager.LoadSceneAsync("MainRoomCombined");
            yield return null;
            var host = Stage();
            var prop = Object.FindObjectsOfType<ReadableProp>().Single(item => item.Id == "poster-beach");
            string text = string.Concat(Enumerable.Repeat("海风吹过门外，记忆仍在远方。", 20));
            prop.Configure(prop.Id, text, prop.Surface);
            string checkpoint = host.Runtime.Save();
            Assert.That(host.Player.InteractPrimaryFocused(), Is.True);
            Assert.That(host.Screen.PropNarration.PageCount, Is.GreaterThan(1));
            var read = new List<string>();
            for (int index = 0; index < host.Screen.PropNarration.PageCount; index++)
            {
                yield return new WaitForSecondsRealtime(.22f);
                AssertTextFitsPanel(host.Screen.PropNarration);
                AssertContinueReadingHint(host.Screen.PropNarration, index + 1 < host.Screen.PropNarration.PageCount);
                read.Add(host.Screen.CurrentRoomCaption);
                Assert.That(host.Screen.AdvancePropNarration(), Is.True);
            }
            Assert.That(string.Concat(read), Is.EqualTo(text), "Pagination must never silently discard a sentence.");
            yield return new WaitForSecondsRealtime(.35f);
            Assert.That(host.Screen.PropNarration.IsOpen, Is.False);
            Assert.That(host.Runtime.Save(), Is.EqualTo(checkpoint), "Reading every page must preserve the game session.");
        }
    }
}
