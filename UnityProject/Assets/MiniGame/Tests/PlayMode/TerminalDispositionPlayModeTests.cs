using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MiniGame.Core;
using MiniGame.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace MiniGame.UnityHost.Tests
{
    [Category("TerminalV0824")]
    public sealed class TerminalDispositionPlayModeTests
    {
        [UnityTest]
        public IEnumerator AuthoredStatesAndFortyFourPixelHitAreasMatchAllThreeCrtModes()
        {
            using (var fixture = new Fixture())
            {
                fixture.FinishDialogue();
                yield return null;
                var screen = fixture.Screen;
                var display = screen.TerminalDisplay;
                var answers = screen.GetComponentsInChildren<RectTransform>(true).Single(rect => rect.name == "AnswerContents");
                Assert.That(answers.anchoredPosition, Is.EqualTo(new Vector2(530, -392)));
                string before = fixture.Runtime.Save();
                for (int mode = 0; mode < 3; mode++)
                {
                    display.SetMode(mode);
                    display.RenderFrame();
                    for (int index = 0; index < fixture.Buttons.Length; index++)
                    {
                        var button = fixture.Buttons[index];
                        var rect = (RectTransform)button.transform;
                        var artwork = Artwork(button);
                        Assert.That(rect.anchoredPosition, Is.EqualTo(new Vector2(530, -(200 + 48 * index))));
                        Assert.That(rect.rect.size, Is.EqualTo(new Vector2(248, 44)));
                        Assert.That(artwork.rectTransform.rect.size, Is.EqualTo(new Vector2(248, 48)));
                        Assert.That(artwork.raycastTarget, Is.False, "Artwork includes a four-pixel gap outside the clickable row.");
                        Assert.That(button.GetComponentsInChildren<Text>(true).All(label => !label.enabled || !label.gameObject.activeInHierarchy), Is.True);
                        foreach (float x in new[] { 531f, 654f, 777f })
                        foreach (float y in new[] { 1f, 22f, 43f })
                            Assert.That(Pick(display, new Vector2(x, 200 + index * 48 + y)).FirstOrDefault().gameObject,
                                Is.SameAs(button.gameObject), "Mode " + mode + ", row " + index + ", point " + x + "," + y);
                        foreach (float y in new[] { 44.5f, 46f, 47.5f })
                        {
                            var hits = Pick(display, new Vector2(654, 200 + index * 48 + y));
                            Assert.That(hits.Any(hit => hit.gameObject.GetComponentInParent<Button>()?.name.StartsWith("Disposition-", StringComparison.Ordinal) == true), Is.False,
                                "The last four artwork pixels must not submit a neighboring command in mode " + mode);
                            if (hits.Count > 0) ExecuteEvents.Execute(hits[0].gameObject, Pointer(), ExecuteEvents.pointerClickHandler);
                            Assert.That(fixture.Runtime.Save(), Is.EqualTo(before));
                        }
                        var pointer = Pointer();
                        ExecuteEvents.Execute(button.gameObject, pointer, ExecuteEvents.pointerEnterHandler);
                        Assert.That(artwork.texture, Is.SameAs(fixture.States[index].Selected));
                        for (int other = 0; other < fixture.Buttons.Length; other++)
                            if (other != index) Assert.That(Artwork(fixture.Buttons[other]).texture, Is.SameAs(fixture.States[other].Normal));
                        ExecuteEvents.Execute(button.gameObject, pointer, ExecuteEvents.pointerDownHandler);
                        Assert.That(artwork.texture, Is.SameAs(fixture.States[index].Pressed));
                        Assert.That(fixture.Runtime.Save(), Is.EqualTo(before), "Press feedback alone cannot submit a disposition.");
                        ExecuteEvents.Execute(button.gameObject, pointer, ExecuteEvents.pointerUpHandler);
                        ExecuteEvents.Execute(button.gameObject, pointer, ExecuteEvents.pointerExitHandler);
                        Assert.That(artwork.texture, Is.SameAs(fixture.States[index].Selected));
                    }
                }
                Assert.That(screen.HandleTerminalKey(KeyCode.DownArrow), Is.True);
                Assert.That(Artwork(fixture.Buttons[0]).texture, Is.SameAs(fixture.States[0].Selected));
                Assert.That(screen.HandleTerminalKey(KeyCode.UpArrow), Is.True);
                Assert.That(Artwork(fixture.Buttons[2]).texture, Is.SameAs(fixture.States[2].Selected));
                foreach (var key in new[] { KeyCode.Return, KeyCode.KeypadEnter, KeyCode.Space })
                {
                    Assert.That(screen.HandleTerminalKey(key), Is.True);
                    Assert.That(fixture.Runtime.Save(), Is.EqualTo(before));
                }
                ExecuteEvents.Execute(fixture.Buttons[2].gameObject, new BaseEventData(EventSystem.current), ExecuteEvents.submitHandler);
                Assert.That(fixture.Runtime.Save(), Is.EqualTo(before));
            }
            yield return null;
        }

        [UnityTest]
        public IEnumerator DialoguePauseHistoryAndTransitionBlockCommandsAndOnlyALeftClickCommitsOnce()
        {
            using (var fixture = new Fixture())
            {
                yield return null;
                AssertDisabled(fixture);
                string before = fixture.Runtime.Save();
                foreach (var button in fixture.Buttons) Click(button);
                Assert.That(fixture.Runtime.Save(), Is.EqualTo(before), "Conversation must finish before a command becomes executable.");
                fixture.FinishDialogue();
                var purify = fixture.Buttons[2];
                Assert.That(fixture.Runtime.SetPaused(true).Accepted, Is.True);
                AssertDisabled(fixture);
                before = fixture.Runtime.Save(); Click(purify);
                Assert.That(fixture.Runtime.Save(), Is.EqualTo(before));
                Assert.That(fixture.Screen.HandleTerminalKey(KeyCode.Return), Is.False);
                Assert.That(fixture.Runtime.SetPaused(false).Accepted, Is.True);
                yield return null;

                var view = typeof(MiniGameScreen).GetField("terminalView", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(fixture.Screen);
                Assert.That(view.GetType().GetMethod("OpenHistory").Invoke(view, null), Is.EqualTo(true));
                AssertDisabled(fixture);
                before = fixture.Runtime.Save(); Click(purify);
                Assert.That(fixture.Runtime.Save(), Is.EqualTo(before));
                Assert.That(fixture.Screen.TryCloseHistory(), Is.True);
                yield return null;
                Assert.That(purify.IsInteractable(), Is.True);
                fixture.Screen.SetTransitionLocked(true);
                AssertDisabled(fixture);
                Click(purify);
                Assert.That(fixture.Runtime.Save(), Is.EqualTo(before));
                fixture.Screen.SetTransitionLocked(false);
                yield return null;
                ExecuteEvents.Execute(purify.gameObject, Pointer(PointerEventData.InputButton.Right), ExecuteEvents.pointerClickHandler);
                Assert.That(fixture.Runtime.Save(), Is.EqualTo(before));
                int remaining = fixture.Runtime.GetView().Game.RemainingPurificationQuota;
                Click(purify);
                Assert.That(fixture.Runtime.GetView().PresentedDisposition, Is.EqualTo(Disposition.Purify));
                Assert.That(fixture.Runtime.GetView().Game.RemainingPurificationQuota, Is.EqualTo(remaining - 1));
                string committed = fixture.Runtime.Save();
                foreach (var button in fixture.Buttons) Click(button);
                Assert.That(fixture.Runtime.Save(), Is.EqualTo(committed), "Repeated or different command clicks cannot commit twice.");
                AssertDisabled(fixture);
            }
            yield return null;
        }

        private static void AssertDisabled(Fixture fixture)
        {
            for (int index = 0; index < fixture.Buttons.Length; index++)
            {
                Assert.That(fixture.Buttons[index].IsInteractable(), Is.False);
                Assert.That(Artwork(fixture.Buttons[index]).texture, Is.SameAs(fixture.States[index].Disabled));
            }
        }

        private static RawImage Artwork(Button button) => button.transform.Find("CommandArtwork").GetComponent<RawImage>();
        private static PointerEventData Pointer(PointerEventData.InputButton button = PointerEventData.InputButton.Left)
            => new PointerEventData(EventSystem.current) { button = button, clickCount = 1 };
        private static void Click(Button button) => ExecuteEvents.Execute(button.gameObject, Pointer(), ExecuteEvents.pointerClickHandler);

        private static List<RaycastResult> Pick(TerminalDisplay display, Vector2 source)
        {
            Vector2 wanted = new Vector2(source.x / 800, 1 - source.y / 600), uv = wanted;
            for (int index = 0; index < 12; index++) uv += wanted - TerminalDisplay.DisplayToSource(uv, display.Mode);
            var rect = display.OutputRect;
            var local = new Vector3(rect.rect.xMin + uv.x * rect.rect.width, rect.rect.yMin + uv.y * rect.rect.height, 0);
            var pointer = Pointer(); pointer.position = RectTransformUtility.WorldToScreenPoint(null, rect.TransformPoint(local));
            var hits = new List<RaycastResult>(); EventSystem.current.RaycastAll(pointer, hits); return hits;
        }

        private sealed class Fixture : IDisposable
        {
            private readonly GameObject owner = new GameObject("Disposition command artwork fixture");
            public readonly GameRuntime Runtime;
            public readonly MiniGameScreen Screen;
            public readonly Button[] Buttons;
            public readonly TerminalCommandArtwork[] States;

            public Fixture()
            {
                var patient = new PatientDefinition("command-patient", "测试对象", 1, 0, null, PatientPresence.TreatmentEnded);
                var meeting = new MeetingDefinition(patient.Id,
                    new[] { new RoundDefinition("question", new[] { new ResponseDefinition("answer", "reply", 0) }) },
                    null, int.MaxValue, 0, new Dictionary<Disposition, string>
                    { [Disposition.Release] = "released", [Disposition.Treat] = "treated", [Disposition.Purify] = "purified" });
                var rules = new GameRules("command-artwork-fixture", new[] { patient }, new[] { new DayDefinition(1, 1, new[] { meeting }) },
                    new[] { EndingType.PriestAttack, EndingType.LockedInRoom, EndingType.TakenAway });
                Runtime = new GameRuntime(rules, new PrototypeContent(rules), true);
                Screen = owner.AddComponent<MiniGameScreen>(); Screen.Bind(Runtime); Screen.UseYarnPresentation();
                Assert.That(Runtime.TryInteract(InteractionTarget.Door).Accepted, Is.True); Complete(); Complete();
                Assert.That(Runtime.TryInteract(InteractionTarget.Computer).Accepted, Is.True); Complete();
                Assert.That(Runtime.EnterSelectedPatient(patient.Id).Accepted, Is.True);
                Buttons = new[] { Disposition.Release, Disposition.Treat, Disposition.Purify }
                    .Select(kind => Screen.GetComponentsInChildren<Button>(true).Single(button => button.name == "Disposition-" + kind)).ToArray();
                var assets = Resources.Load<TerminalAssetSet>("MiniGame/TerminalAssets");
                States = new[] { assets.ReleaseCommand, assets.TreatmentCommand, assets.PurificationCommand };
            }

            public void FinishDialogue()
            {
                var view = Runtime.GetView();
                Assert.That(Runtime.SelectResponse(view.Game.CurrentEncounterId, view.Game.CurrentNodeId, "answer").Accepted, Is.True);
                Complete();
                Assert.That(Runtime.GetView().Game.EncounterStage, Is.EqualTo(EncounterStage.AwaitingDisposition));
                Assert.That(Runtime.GetView().Game.AvailableDispositions, Is.EquivalentTo(new[] { Disposition.Release, Disposition.Treat, Disposition.Purify }));
            }

            private void Complete() => Assert.That(Runtime.CompleteCue(Runtime.GetView().Cue.Token).Accepted, Is.True);
            public void Dispose() => Object.Destroy(owner);
        }
    }
}
