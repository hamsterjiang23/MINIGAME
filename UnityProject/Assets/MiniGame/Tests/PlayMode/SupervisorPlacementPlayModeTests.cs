using System;
using System.Collections;
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
    public sealed class SupervisorPlacementPlayModeTests
    {
        private GameObject probe;
        private MiniGameBootstrap host;
        private SupervisorDoorwayView supervisor;
        private BoxCollider portraitBox, doorBox;
        private int checkedFrames, capturedFrames;
        private float nextCapture;
        private string directory;
        private RenderTexture target;
        private Texture2D pixels;
        private RoomInteractionSettings interactionSettings;

        [UnityTest, Timeout(45000)]
        public IEnumerator RealDoorNeverIntersectsPortraitDuringRepeatedOpeningAndClosing()
        {
            MiniGameSessionHost.Release();
            yield return SceneManager.LoadSceneAsync("MainRoomCombined");
            yield return null;
            host = Object.FindObjectOfType<MiniGameBootstrap>();
            host.Player.ViewCamera.aspect = 1600f / 900;
            host.StartTerminalDemo();
            // This fixture repeats the physical motion after morning inspection is complete.
            interactionSettings = ScriptableObject.CreateInstance<RoomInteractionSettings>();
            interactionSettings.LockDoorAfterSupervisor = false;
            host.ConfigureRoomInteractions(interactionSettings);
            host.MainMenu.TryDismissForEditorDebug();
            host.Screen.SetMainMenuCovered(false);
            Object.FindObjectOfType<MiniGameYarnBridge>().enabled = false;
            host.Player.enabled = false;
            var controller = host.Player.GetComponent<CharacterController>();
            controller.enabled = false;
            host.Player.transform.position = GameObject.Find("Door approach").transform.position;
            controller.enabled = true;
            var interaction = Object.FindObjectsOfType<WorldInteraction>().Single(item => item.Target == InteractionTarget.Door);
            host.Player.FacePoint(interaction.GetComponent<Collider>().bounds.center);
            Physics.SyncTransforms();
            var doorway = host.Player.ViewCamera.GetComponent<DoorViewCamera>();
            var door = interaction.GetComponentInParent<HingedDoor>();
            door.AnimationSeconds = 1.2f;
            doorBox = interaction.GetComponent<BoxCollider>();
            supervisor = host.GetComponent<SupervisorDoorwayView>();
            probe = new GameObject("Portrait clearance probe", typeof(BoxCollider));
            portraitBox = probe.GetComponent<BoxCollider>();
            portraitBox.isTrigger = true;
            directory = Environment.GetEnvironmentVariable("MINIGAME_PLACEMENT_CAPTURE_DIR");
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
                target = new RenderTexture(1600, 900, 24, RenderTextureFormat.ARGB32);
                target.Create(); pixels = new Texture2D(1600, 900, TextureFormat.RGB24, false);
                // Record at full resolution regardless of the machine's pixel-effect preference.
                var retro = host.Player.ViewCamera.GetComponent<RetroScreenEffect>();
                if (retro != null) retro.enabled = false;
            }
            var safePosition = supervisor.Surface.transform.position;
            for (int cycle = 0; cycle < 2; cycle++)
            {
                Assert.That(doorway.TryEnter(), Is.True);
                float deadline = Time.realtimeSinceStartup + 10;
                while (!supervisor.IsReadyForDialogue && Time.realtimeSinceStartup < deadline)
                { yield return null; CheckAndCapture(); }
                Assert.That(supervisor.IsReadyForDialogue, Is.True);
                var restingPosition = supervisor.Surface.transform.position;
                if (cycle == 0) Capture("supervisor-position.png");
                for (int portrait = 1; portrait <= 6; portrait++)
                {
                    Assert.That(supervisor.SetPortrait("JD_" + portrait.ToString("00")), Is.True);
                    yield return null; CheckAndCapture();
                    Assert.That(Vector3.Distance(restingPosition, supervisor.Surface.transform.position), Is.LessThan(.0001f));
                }
                Assert.That(supervisor.SetPortrait("JD_01"), Is.True);
                if (cycle == 0)
                {
                    Assert.That(host.Runtime.GetView().Cue?.Kind, Is.EqualTo(PresentationKind.MorningInspection));
                    Assert.That(host.Runtime.CompleteCue(host.Runtime.GetView().Cue.Token).Accepted, Is.True);
                    Assert.That(host.Runtime.CompleteCue(host.Runtime.GetView().Cue.Token).Accepted, Is.True);
                }
                Assert.That(doorway.TryReturn(), Is.True);
                // The physical door must wait while the close-position portrait slides out of its sweep.
                float openAngle = door.CurrentAngle;
                yield return null;
                Assert.That(door.CurrentAngle, Is.EqualTo(openAngle).Within(.001f));
                if (cycle == 0)
                {
                    host.Runtime.SetPaused(true);
                    var paused = supervisor.Surface.transform.position;
                    yield return new WaitForSecondsRealtime(.15f);
                    Assert.That(Vector3.Distance(paused, supervisor.Surface.transform.position), Is.LessThan(.0001f));
                    Assert.That(door.CurrentAngle, Is.EqualTo(openAngle).Within(.001f));
                    host.Runtime.SetPaused(false);
                }
                deadline = Time.realtimeSinceStartup + 10;
                while ((host.Runtime.GetView().Input != InputContext.World || door.IsMoving) && Time.realtimeSinceStartup < deadline)
                { yield return null; CheckAndCapture(); }
                Assert.That(host.Runtime.GetView().Input, Is.EqualTo(InputContext.World));
                Assert.That(door.IsMoving, Is.False);
                Assert.That(Vector3.Distance(safePosition, supervisor.Surface.transform.position), Is.LessThan(.001f));
            }
            Assert.That(checkedFrames, Is.GreaterThan(30));
            if (!string.IsNullOrEmpty(directory)) File.WriteAllText(Path.Combine(directory, "verification.json"),
                "{\"result\":\"passed\",\"cycles\":2,\"portraitVariants\":6,\"checkedFrames\":" + checkedFrames
                + ",\"capturedFrames\":" + capturedFrames + ",\"portraitRectangleThicknessMetres\":0.02}");
        }

        private void CheckAndCapture()
        {
            var surface = supervisor.Surface;
            portraitBox.size = new Vector3(surface.sprite.bounds.size.x * surface.transform.lossyScale.x,
                surface.sprite.bounds.size.y * surface.transform.lossyScale.y, .02f);
            probe.transform.SetPositionAndRotation(surface.transform.TransformPoint(surface.sprite.bounds.center), surface.transform.rotation);
            bool intersects = Physics.ComputePenetration(portraitBox, probe.transform.position, probe.transform.rotation,
                doorBox, doorBox.transform.position, doorBox.transform.rotation, out _, out float depth);
            Assert.That(intersects, Is.False, "The actual door intersects the full portrait rectangle at frame " + checkedFrames + ", depth=" + depth);
            checkedFrames++;
            if (!string.IsNullOrEmpty(directory) && Time.realtimeSinceStartup >= nextCapture)
            {
                Capture("frame-" + (capturedFrames++).ToString("0000") + ".png");
                nextCapture = Time.realtimeSinceStartup + .1f;
            }
        }

        private void Capture(string name)
        {
            if (target == null) return;
            var camera = host.Player.ViewCamera;
            var oldTarget = camera.targetTexture;
            var oldActive = RenderTexture.active;
            try
            {
                camera.targetTexture = target; camera.Render(); RenderTexture.active = target;
                pixels.ReadPixels(new Rect(0, 0, 1600, 900), 0, 0); pixels.Apply();
                File.WriteAllBytes(Path.Combine(directory, name), pixels.EncodeToPNG());
            }
            finally { camera.targetTexture = oldTarget; RenderTexture.active = oldActive; }
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (target != null) { target.Release(); Object.Destroy(target); }
            if (pixels != null) Object.Destroy(pixels);
            if (probe != null) Object.Destroy(probe);
            if (interactionSettings != null) Object.Destroy(interactionSettings);
            MiniGameSessionHost.Release();
            yield return null;
        }
    }
}
