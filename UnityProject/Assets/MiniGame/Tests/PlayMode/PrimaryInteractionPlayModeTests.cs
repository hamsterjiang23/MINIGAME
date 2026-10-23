using System.Collections;
using System.Linq;
using MiniGame.Core;
using MiniGame.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace MiniGame.UnityHost.Tests
{
    [Category("InteractionWakeGm")]
    public sealed class PrimaryInteractionPlayModeTests
    {
        [UnityTest]
        public IEnumerator OnePrimaryActionReadsOpensWorksAndSleepsWithoutBypassingGates()
        {
            var ui = new GameObject("Primary input UI");
            var playerRoot = new GameObject("Primary input player", typeof(CharacterController));
            var cameraRoot = new GameObject("Primary input camera", typeof(Camera));
            var readable = GameObject.CreatePrimitive(PrimitiveType.Quad);
            var furniture = GameObject.CreatePrimitive(PrimitiveType.Cube);
            try
            {
                var rules = new GameRules("primary-input-fixture",
                    new[] { new PatientDefinition("fixture", "测试", 1, 0, null, PatientPresence.TreatmentEnded) },
                    new[] { new DayDefinition(1, 1, new[] { PrototypeRules.CreateMeeting("fixture", 1, 1, 100, 0) }) },
                    new[] { EndingType.PriestAttack, EndingType.LockedInRoom, EndingType.TakenAway });
                var runtime = new GameRuntime(rules, new PrototypeContent(rules));
                var screen = ui.AddComponent<MiniGameScreen>(); screen.Bind(runtime);
                playerRoot.transform.position = new Vector3(10000, 10000, 10000);
                cameraRoot.transform.SetParent(playerRoot.transform, false);
                cameraRoot.transform.localPosition = new Vector3(0, 1.6f, 0);
                cameraRoot.GetComponent<Camera>().enabled = false;
                var player = playerRoot.AddComponent<ApartmentPlayer>();
                player.enabled = false; playerRoot.GetComponent<CharacterController>().enabled = false;
                player.Configure(cameraRoot.GetComponent<Camera>()); player.Bind(runtime, screen);
                var surface = furniture.GetComponent<Renderer>();
                var interaction = furniture.AddComponent<WorldInteraction>();
                readable.transform.position = furniture.transform.position = cameraRoot.transform.position + Vector3.forward * 2;
                furniture.SetActive(false);
                var prop = readable.AddComponent<ReadableProp>();
                prop.Configure("click-fixture", "左键阅读测试。", readable.GetComponent<Renderer>(), 3);
                Physics.SyncTransforms(); player.RefreshInteractionFocus();
                yield return null;
                Assert.That(screen.InteractionCursor, Is.EqualTo(InteractionCursorKind.Read));
                Assert.That(prop.IsHighlighted, Is.True, "The existing readable outline must remain available.");
                string checkpoint = runtime.Save();
                Assert.That(player.HandleInteractionInput(true, false), Is.True, "E uses the same readable action.");
                Assert.That(screen.PropNarrationBlocksWorld, Is.True);
                Assert.That(player.InteractPrimaryFocused(), Is.False, "Opening input cannot operate through the reading page.");
                Assert.That(runtime.Save(), Is.EqualTo(checkpoint));
                screen.PropNarration.CloseImmediately(); readable.SetActive(false);
                Assert.That(player.HandleInteractionInput(false, true), Is.False, "A second input in the same frame cannot operate another target.");
                yield return null;

                void Focus(InteractionTarget target)
                {
                    furniture.SetActive(true); interaction.Configure(target, "fixture", surface, 3);
                    Physics.SyncTransforms(); player.RefreshInteractionFocus();
                }
                void CompleteCues()
                {
                    int steps = 0;
                    while (runtime.GetView().Cue != null && steps++ < 12)
                        Assert.That(runtime.CompleteCue(runtime.GetView().Cue.Token).Accepted, Is.True);
                    Assert.That(steps, Is.LessThan(12));
                }

                Focus(InteractionTarget.Door);
                Assert.That(screen.InteractionCursor, Is.EqualTo(InteractionCursorKind.Door));
                Assert.That(interaction.IsHighlighted, Is.True);
                runtime.SetPaused(true);
                Assert.That(player.InteractPrimaryFocused(), Is.False, "Pause must block the unified primary input.");
                runtime.SetPaused(false);
                Assert.That(player.HandleInteractionInput(true, true), Is.False, "The pause closing input cannot reach the world.");
                yield return null;
                Focus(InteractionTarget.Door);
                interaction.InteractionDistance = .5f; player.RefreshInteractionFocus();
                Assert.That(screen.InteractionCursor, Is.EqualTo(InteractionCursorKind.None));
                Assert.That(player.InteractPrimaryFocused(), Is.False, "Left click must still respect each object's distance.");
                Focus(InteractionTarget.Door);
                Assert.That(player.HandleInteractionInput(true, true), Is.True, "Simultaneous E and left click submit one action.");
                Assert.That(runtime.GetView().Cue.Kind, Is.EqualTo(PresentationKind.MorningInspection));
                Assert.That(player.InteractPrimaryFocused(), Is.False, "Repeated click cannot interrupt the current cue.");
                CompleteCues();
                yield return null;

                Focus(InteractionTarget.Bed);
                Assert.That(screen.InteractionCursor, Is.EqualTo(InteractionCursorKind.None));
                Assert.That(player.InteractPrimaryFocused(), Is.False, "The new input cannot skip unfinished work.");
                Focus(InteractionTarget.Computer);
                Assert.That(screen.InteractionCursor, Is.EqualTo(InteractionCursorKind.Work));
                Assert.That(player.HandleInteractionInput(false, true), Is.True, "Left click still uses the computer action.");
                CompleteCues();
                int turns = 0;
                while (runtime.GetView().Game.EncounterStage != EncounterStage.AwaitingDisposition && turns++ < 12)
                {
                    var view = runtime.GetView();
                    Assert.That(view.Dialogue, Is.Not.Null);
                    Assert.That(runtime.SelectResponse(view.Game.CurrentEncounterId, view.Game.CurrentNodeId,
                        view.Dialogue.Responses.First().Id).Accepted, Is.True);
                    CompleteCues();
                }
                Assert.That(turns, Is.LessThan(12));
                Assert.That(runtime.SubmitDisposition(runtime.GetView().Game.CurrentEncounterId, Disposition.Purify).Accepted, Is.True);
                CompleteCues();
                Assert.That(runtime.GetView().Game.Phase, Is.EqualTo(GamePhase.Evening));
                furniture.SetActive(false);
                player.RefreshInteractionFocus();
                yield return null;
                var reticle = screen.GetComponentsInChildren<UnityEngine.UI.Text>(true).Single(text => text.name == "Reticle");
                Assert.That(reticle.gameObject.activeInHierarchy, Is.True, "Night exploration keeps its dot even away from the bed.");
                runtime.SetPaused(true); yield return null;
                Assert.That(reticle.gameObject.activeInHierarchy, Is.False);
                runtime.SetPaused(false); yield return null;
                screen.SetTransitionLocked(true); yield return null;
                Assert.That(reticle.gameObject.activeInHierarchy, Is.False);
                screen.SetTransitionLocked(false); yield return null;
                Focus(InteractionTarget.Bed);
                Assert.That(screen.InteractionCursor, Is.EqualTo(InteractionCursorKind.Sleep));
                Assert.That(player.InteractPrimaryFocused(), Is.True);
                Assert.That(runtime.GetView().Cue.Kind, Is.EqualTo(PresentationKind.Sleep));
            }
            finally
            {
                Object.Destroy(readable); Object.Destroy(furniture);
                Object.Destroy(playerRoot); Object.Destroy(ui);
            }
            yield return null;
        }
    }
}
