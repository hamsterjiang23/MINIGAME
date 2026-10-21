using System.Collections;
using System.Linq;
using MiniGame.Core;
using MiniGame.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace MiniGame.UnityHost.Tests
{
    public sealed class NormWorldPresentationPlayModeTests
    {
        [UnityTest]
        public IEnumerator MorningUsesSupervisorAndAWhiteFrameWithoutDeveloperLabelsOrWorldInstructions()
        {
            var rules = PrototypeRules.Create();
            var runtime = new GameRuntime(rules, new PrototypeContent(rules), true);
            var owner = new GameObject("NORM morning presentation regression");
            var screen = owner.AddComponent<MiniGameScreen>();
            try
            {
                screen.Bind(runtime);
                Assert.That(runtime.TryInteract(InteractionTarget.Door).Accepted, Is.True);
                screen.SetDialogueText("终端", "【开发占位】门外扫描灯亮起。");
                screen.ShowCueText("【开发占位】门外扫描灯亮起。", () => { });
                yield return null;
                Assert.That(Resources.Load<Texture2D>("MiniGame/SupervisorPortrait"), Is.Not.Null);
                Assert.That(screen.GetComponentsInChildren<RawImage>(true).Any(image => image.name == "SupervisorFigure"), Is.False,
                    "The supervisor is a world actor outside the door, never an indoor HUD overlay.");
                var body = screen.GetComponentsInChildren<Text>(true).Single(text => text.name == "DialogueBody");
                Assert.That(body.text, Is.EqualTo("门外扫描灯亮起。"));
                Assert.That(body.color.r, Is.GreaterThan(body.color.g));
                Assert.That(body.color.g, Is.GreaterThan(body.color.b));
                Assert.That(screen.GetComponentsInChildren<Text>(true).Single(text => text.name == "Speaker").text, Is.EqualTo("监督员"));
                var frame = body.transform.parent.GetComponentsInChildren<Image>(true).Where(image => image.name.StartsWith("NarrativeFrame")).ToArray();
                Assert.That(frame.Length, Is.EqualTo(4));
                Assert.That(frame.All(image => image.color == Color.white && !image.raycastTarget), Is.True);
                Assert.That(screen.GetComponentsInChildren<Text>(true).Single(text => text.name == "WorldInstructions").gameObject.activeSelf, Is.False);
                Assert.That(screen.GetComponentsInChildren<Transform>(true).Any(node => node.name == "FocusUnderline" || node.name == "DialogueAccent"), Is.False);
                runtime.CompleteCue(runtime.GetView().Cue.Token);
                runtime.CompleteCue(runtime.GetView().Cue.Token);
                yield return null;
            }
            finally { Object.Destroy(owner); }
            yield return null;
        }

        [UnityTest, Timeout(20000)]
        public IEnumerator RealRoomManualKeepsPauseUntilTheSecondEscapeAndMorningCanBeRead()
        {
            yield return SceneManager.LoadSceneAsync("MainRoomCombined");
            yield return null;
            var host = Object.FindObjectOfType<MiniGameBootstrap>();
            var supervisor = host.GetComponent<SupervisorDoorwayView>();
            Assert.That(supervisor, Is.Not.Null);
            Assert.That(supervisor.IsVisible, Is.True, "The actual room must already contain the supervisor before opening its door.");
            var effects = host.GetComponent<PresentationEffects>();
            if (effects.TryGetRoomReturnPose(out var standingPosition, out _, out _, out _, out _))
                Assert.That(supervisor.Surface.bounds.min.y, Is.EqualTo(standingPosition.y + .1f).Within(.001f),
                    "An opening seated capsule must not lower the permanently placed supervisor.");
            host.StartTerminalDemo(); host.Player.enabled = false; host.Screen.DialogueCharactersPerSecond = 0;
            var outsidePosition = supervisor.Surface.transform.position;
            Assert.That(host.Runtime.GetView().RoomDoorOpen, Is.False);
            Assert.That(supervisor.IsVisible, Is.True);
            host.Runtime.SetPaused(true);
            var menu = host.Screen.GetComponentInChildren<PauseMenuView>(true);
            menu.ShowManual();
            Assert.That(host.Screen.PauseManualVisible, Is.True);
            string before = host.Runtime.Save();
            host.HandleEscape();
            Assert.That(host.Screen.PauseManualVisible, Is.False);
            Assert.That(host.Runtime.GetView().Paused, Is.True);
            Assert.That(host.Runtime.Save(), Is.EqualTo(before));
            host.HandleEscape();
            Assert.That(host.Runtime.GetView().Paused, Is.False);
            var interaction = Object.FindObjectsOfType<WorldInteraction>().Single(item => item.Target == InteractionTarget.Door);
            var controller = host.Player.GetComponent<CharacterController>(); controller.enabled = false;
            host.Player.transform.position = GameObject.Find("Door approach").transform.position;
            controller.enabled = true; host.Player.FacePoint(interaction.GetComponent<Collider>().bounds.center);
            Physics.SyncTransforms(); host.Player.RefreshInteractionFocus();
            yield return PrototypePlayModeTests.CaptureIfRequested(host, "supervisor-already-outside-closed-door");
            Assert.That(supervisor.IsVisible, Is.True);
            Assert.That(host.Player.InteractFocused(), Is.True);
            float deadline = Time.realtimeSinceStartup + 8;
            while (host.Runtime.GetView().Cue?.Kind != PresentationKind.MorningInspection && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.That(host.Runtime.GetView().Cue?.Kind, Is.EqualTo(PresentationKind.MorningInspection));
            Assert.That(Vector3.Distance(supervisor.Surface.transform.position, outsidePosition), Is.LessThan(.001f),
                "The actual door opening must reveal the existing actor without spawning or repositioning him.");
            yield return new WaitForSecondsRealtime(.25f);
            yield return PrototypePlayModeTests.CaptureIfRequested(host, "supervisor-morning");
            host.Runtime.SetPaused(true); menu.ShowManual();
            yield return new WaitForSecondsRealtime(.2f);
            yield return PrototypePlayModeTests.CaptureIfRequested(host, "pause-operation-manual");
            host.HandleEscape();
            yield return new WaitForSecondsRealtime(.2f);
            yield return PrototypePlayModeTests.CaptureIfRequested(host, "pause-amber-menu");
            host.HandleEscape();
        }
    }
}
