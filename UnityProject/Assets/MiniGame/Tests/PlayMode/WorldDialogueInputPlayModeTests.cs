using System.Collections;
using System.Linq;
using MiniGame.Core;
using MiniGame.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace MiniGame.UnityHost.Tests
{
    public sealed class WorldDialogueInputPlayModeTests
    {
        private MiniGameBootstrap host;

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            MiniGameSessionHost.Release();
            yield return null;
        }

        [UnityTest, Timeout(30000)]
        public IEnumerator SupervisorPointerKeyboardAndBackgroundShareRevealAndAdvanceGuard()
        {
            yield return OpenMorning(.01f);
            var screen = host.Screen;
            var body = WorldBody();
            Assert.That(screen.LineIsRevealed, Is.False, "The real morning Yarn line must still be typing.");
            string checkpoint = host.Runtime.Save();
            Assert.That(screen.AdvanceWorldDialogue(), Is.True);
            Assert.That(screen.LineIsRevealed, Is.True, "The first background click reveals the current page.");
            string firstLine = body.text;
            Assert.That(firstLine, Is.Not.Empty);
            Assert.That(host.Runtime.Save(), Is.EqualTo(checkpoint), "Revealing text must not complete the morning cue.");
            Assert.That(screen.AdvanceWorldDialogue(), Is.False);
            Assert.That(screen.HandleTerminalKey(KeyCode.Space), Is.False, "Space must share the background-click frame gate.");
            Click(WorldContinue());
            Assert.That(body.text, Is.EqualTo(firstLine), "A same-frame native button click cannot also consume the revealed line.");
            Assert.That(host.Runtime.Save(), Is.EqualTo(checkpoint));

            Assert.That(host.Runtime.SetPaused(true).Accepted, Is.True);
            string paused = host.Runtime.Save();
            Assert.That(screen.AdvanceWorldDialogue(), Is.False);
            Assert.That(screen.HandleTerminalKey(KeyCode.Space), Is.False);
            Click(WorldContinue());
            Assert.That(host.Runtime.Save(), Is.EqualTo(paused));
            yield return null;
            Assert.That(host.Runtime.SetPaused(false).Accepted, Is.True);
            Assert.That(screen.AdvanceWorldDialogue(), Is.False, "The resume input cannot also advance a world line.");
            yield return null;
            screen.SetTransitionLocked(true);
            string locked = host.Runtime.Save();
            try
            {
                Assert.That(screen.AdvanceWorldDialogue(), Is.False);
                Assert.That(screen.HandleTerminalKey(KeyCode.Space), Is.False);
                Click(WorldContinue());
                Assert.That(host.Runtime.Save(), Is.EqualTo(locked));
            }
            finally { screen.SetTransitionLocked(false); }
            yield return null;

            Click(WorldContinue());
            Assert.That(screen.AdvanceWorldDialogue(), Is.False, "The button and background cannot complete two lines in one frame.");
            float deadline = Time.realtimeSinceStartup + 5;
            while ((screen.CurrentLinePageCount == 0 || screen.LineIsRevealed || body.text == firstLine)
                && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.That(screen.CurrentLinePageCount, Is.GreaterThan(0), host.GetComponent<MiniGameYarnBridge>().Diagnostics);
            Assert.That(screen.LineIsRevealed, Is.False, "The next real supervisor line must begin with unrevealed text.");
            yield return null;
            Assert.That(screen.HandleTerminalKey(KeyCode.Space), Is.True);
            Assert.That(screen.LineIsRevealed, Is.True);
            Assert.That(body.text, Is.Not.EqualTo(firstLine), "The native button must have completed the prior line exactly once.");
            string nextRevealed = host.Runtime.Save();
            Assert.That(screen.AdvanceWorldDialogue(), Is.False);
            Click(WorldContinue());
            Assert.That(host.Runtime.Save(), Is.EqualTo(nextRevealed));

            // The real door session supplies the context; a completion counter isolates the new-line input boundary.
            yield return null;
            int completions = 0;
            screen.ShowCueText("监督员：请保持原地。", () => completions++);
            Assert.That(screen.AdvanceWorldDialogue(), Is.False, "A newly presented line cannot consume its opening frame.");
            Assert.That(screen.HandleTerminalKey(KeyCode.Space), Is.False);
            Click(WorldContinue());
            Assert.That(completions, Is.Zero);
            yield return null;
            Assert.That(screen.AdvanceWorldDialogue(), Is.True);
            Assert.That(completions, Is.EqualTo(1));
            Assert.That(screen.AdvanceWorldDialogue(), Is.False);
            Assert.That(completions, Is.EqualTo(1));
        }

        [UnityTest, Timeout(45000)]
        public IEnumerator WorldContinueFinishesTheDoorConversationAndCannotOperateComputerDialogue()
        {
            yield return OpenMorning(0);
            float deadline = Time.realtimeSinceStartup + 10;
            while (host.Runtime.GetView().Cue != null && Time.realtimeSinceStartup < deadline)
            {
                var kind = host.Runtime.GetView().Cue.Kind;
                Assert.That(kind == PresentationKind.MorningInspection || kind == PresentationKind.Briefing, Is.True);
                host.Screen.AdvanceWorldDialogue();
                yield return null;
            }
            Assert.That(host.Runtime.GetView().Cue, Is.Null, host.GetComponent<MiniGameYarnBridge>().Diagnostics);
            Assert.That(host.Runtime.GetView().Input, Is.EqualTo(InputContext.DoorView));
            Assert.That(host.Screen.AdvanceWorldDialogue(), Is.False);
            Assert.That(host.Player.InteractFocused(), Is.True, "The actual doorway interaction returns the player to the room.");
            deadline = Time.realtimeSinceStartup + 7;
            while (host.Runtime.GetView().Input != InputContext.World && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.That(host.Runtime.GetView().Input, Is.EqualTo(InputContext.World));
            Assert.That(host.Screen.AdvanceWorldDialogue(), Is.False);

            StageAt(InteractionTarget.Computer, "Computer approach");
            Assert.That(host.Player.InteractFocused(), Is.True);
            Assert.That(host.Screen.AdvanceWorldDialogue(), Is.False, "Entering the terminal cannot use world dialogue input.");
            deadline = Time.realtimeSinceStartup + 12;
            while (host.Runtime.GetView().Input != InputContext.TerminalDirectory && Time.realtimeSinceStartup < deadline)
            {
                Assert.That(host.Screen.AdvanceWorldDialogue(), Is.False);
                if (host.Screen.PowerPhase == TerminalPowerPhase.Boot) host.Screen.HandleTerminalKey(KeyCode.Return);
                yield return null;
            }
            Assert.That(host.Runtime.GetView().Input, Is.EqualTo(InputContext.TerminalDirectory));
            Assert.That(host.Runtime.GetView().TerminalOpen, Is.True);
            string directory = host.Runtime.Save();
            Assert.That(host.Screen.AdvanceWorldDialogue(), Is.False);
            Assert.That(host.Runtime.Save(), Is.EqualTo(directory));
            Click(host.Screen.GetComponentsInChildren<Button>(true)
                .Single(button => button.name == "EnterReview" && button.gameObject.activeInHierarchy && button.interactable));
            yield return null;
            Assert.That(host.Runtime.GetView().TerminalOpen, Is.True);
            string review = host.Runtime.Save();
            Assert.That(host.Screen.AdvanceWorldDialogue(), Is.False, "A computer character's dialogue keeps its own input path.");
            Assert.That(host.Runtime.Save(), Is.EqualTo(review));
        }

        private IEnumerator OpenMorning(float charactersPerSecond)
        {
            MiniGameSessionHost.Release();
            yield return SceneManager.LoadSceneAsync("MainRoomCombined");
            yield return null;
            host = Object.FindObjectOfType<MiniGameBootstrap>();
            host.MainMenu.TryDismissForEditorDebug();
            host.StartCampaign();
            RestoreFirstMorning(host);
            host.Player.enabled = false;
            host.Screen.DialogueCharactersPerSecond = charactersPerSecond;
            Assert.That(host.Screen.AdvanceWorldDialogue(), Is.False);
            StageAt(InteractionTarget.Door, "Door approach");
            Assert.That(host.Player.InteractFocused(), Is.True);
            Assert.That(host.Screen.AdvanceWorldDialogue(), Is.False, "The door-opening input must not consume supervisor text.");
            float deadline = Time.realtimeSinceStartup + 8;
            while ((host.Runtime.GetView().Cue?.Kind != PresentationKind.MorningInspection
                || host.Screen.CurrentLinePageCount == 0 || host.Screen.TransitionLocked
                || !WorldContinue().gameObject.activeInHierarchy || !WorldContinue().interactable)
                && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.That(host.Runtime.GetView().Cue?.Kind, Is.EqualTo(PresentationKind.MorningInspection));
            Assert.That(host.Screen.CurrentLinePageCount, Is.GreaterThan(0), host.GetComponent<MiniGameYarnBridge>().Diagnostics);
            Assert.That(host.Screen.TransitionLocked, Is.False);
            Assert.That(WorldContinue().gameObject.activeInHierarchy && WorldContinue().interactable, Is.True);
            Assert.That(host.GetComponent<SupervisorDoorwayView>().IsVisible, Is.True);
            yield return null;
        }

        private static void RestoreFirstMorning(MiniGameBootstrap host)
        {
            var session = MiniGameSessionHost.Current;
            Assert.That(session, Is.Not.Null, "The combined scene must provide the real Campaign rules and content.");
            var runtime = new GameRuntime(session.Rules, session.Content, useTerminalDirectory: true);
            runtime.ConfigureEndings(host.Runtime.Endings);
            Assert.That(runtime.IsCampaign, Is.True);
            Assert.That(runtime.GetView().Game.Day, Is.Zero);
            Assert.That(runtime.BeginDayZeroOpening().Accepted, Is.True);
            for (int step = 0; step < 64; step++)
            {
                var view = runtime.GetView();
                if (view.Game.Day == 1 && view.Game.Phase == GamePhase.MorningInspection
                    && view.Cue == null && !view.TerminalOpen)
                {
                    host.RestoreCheckpoint(runtime.Save());
                    Assert.That(host.Runtime.GetView().Game.Day, Is.EqualTo(1));
                    Assert.That(host.Runtime.GetView().Game.Phase, Is.EqualTo(GamePhase.MorningInspection));
                    return;
                }
                Assert.That(view.Game.Day, Is.Zero, "Fixture preparation must stop before consuming the first morning.");
                RuntimeResult result;
                if (view.Cue != null) result = runtime.CompleteCue(view.Cue.Token);
                else if (view.TerminalOpen && view.Game.EncounterStage == EncounterStage.AwaitingDisposition)
                {
                    Assert.That(view.Game.AvailableDispositions, Does.Contain(Disposition.Purify));
                    result = runtime.SubmitDisposition(view.Game.CurrentEncounterId, Disposition.Purify);
                }
                else if (view.Dialogue != null && view.Dialogue.Responses.Count > 0)
                    result = runtime.SelectResponse(view.Game.CurrentEncounterId, view.Game.CurrentNodeId, view.Dialogue.Responses[0].Id);
                else if (view.AllowedInteractions.Contains(InteractionTarget.Bed)) result = runtime.TryInteract(InteractionTarget.Bed);
                else
                {
                    Assert.Fail("Unsupported Campaign preparation state: " + view.Game.Phase + "/" + view.Input + "/" + view.TerminalPage);
                    return;
                }
                Assert.That(result.Accepted, Is.True, result.Reason);
            }
            Assert.Fail("The actual Day0 tutorial did not reach the first morning within its bounded command sequence.");
        }

        private void StageAt(InteractionTarget target, string approach)
        {
            var interaction = Object.FindObjectsOfType<WorldInteraction>().Single(item => item.Target == target);
            var capsule = host.Player.GetComponent<CharacterController>();
            capsule.enabled = false;
            host.Player.transform.position = GameObject.Find(approach).transform.position;
            capsule.enabled = true;
            host.Player.FacePoint(interaction.GetComponent<Collider>().bounds.center);
            Physics.SyncTransforms();
            host.Player.RefreshInteractionFocus();
        }

        private Button WorldContinue() => host.Screen.GetComponentsInChildren<Button>(true)
            .Single(button => button.name == "Continue" && button.transform.parent.name == "Conversation");

        private Text WorldBody() => host.Screen.GetComponentsInChildren<Text>(true).Single(text => text.name == "DialogueBody");

        private static void Click(Button button)
        {
            Assert.That(EventSystem.current, Is.Not.Null);
            Canvas.ForceUpdateCanvases();
            var rectangle = (RectTransform)button.transform;
            var camera = button.GetComponentInParent<Canvas>().worldCamera;
            var point = RectTransformUtility.WorldToScreenPoint(camera, rectangle.TransformPoint(rectangle.rect.center));
            var pointer = new PointerEventData(EventSystem.current) { button = PointerEventData.InputButton.Left, position = point };
            ExecuteEvents.Execute(button.gameObject, pointer, ExecuteEvents.pointerDownHandler);
            ExecuteEvents.Execute(button.gameObject, pointer, ExecuteEvents.pointerUpHandler);
            ExecuteEvents.Execute(button.gameObject, pointer, ExecuteEvents.pointerClickHandler);
        }
    }
}
