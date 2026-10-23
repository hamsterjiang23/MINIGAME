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
    [Category("InteractionWakeGm")]
    public sealed class PauseMenuPlayModeTests
    {
        private readonly List<EventSystem> suspendedEventSystems = new List<EventSystem>();
        private readonly int[] activations = new int[3];
        private GameObject canvasObject, eventObject;
        private EventSystem events, previousEvents;
        private GameObject previousSelection;
        private PauseMenuView view;
        private Button underlying;
        private float previousTimeScale;
        private int underlyingActivations;

        [UnitySetUp]
        public IEnumerator CreateIsolatedPauseMenu()
        {
            previousTimeScale = Time.timeScale;
            previousEvents = EventSystem.current;
            previousSelection = previousEvents != null ? previousEvents.currentSelectedGameObject : null;
            suspendedEventSystems.Clear();
            foreach (var existing in Object.FindObjectsOfType<EventSystem>())
            {
                if (!existing.isActiveAndEnabled) continue;
                suspendedEventSystems.Add(existing);
                existing.enabled = false;
            }

            eventObject = new GameObject("Pause test events", typeof(EventSystem));
            events = eventObject.GetComponent<EventSystem>();
            EventSystem.current = events;
            canvasObject = new GameObject("Pause test canvas", typeof(RectTransform), typeof(Canvas), typeof(GraphicRaycaster));
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = short.MaxValue;

            var buttonObject = new GameObject("Underlying action", typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(canvasObject.transform, false);
            var rect = (RectTransform)buttonObject.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            underlying = buttonObject.GetComponent<Button>();
            underlying.targetGraphic = buttonObject.GetComponent<Image>();
            underlyingActivations = 0;
            underlying.onClick.AddListener(() => underlyingActivations++);
            Array.Clear(activations, 0, activations.Length);
            view = PauseMenuView.Create(canvasObject.transform, Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"),
                "Pause regression", new[] { "Continue", "Unavailable", "Settings" }, new[] { "Esc", "", "Enter" },
                new Action[] { () => activations[0]++, () => activations[1]++, () => activations[2]++ });
            events.SetSelectedGameObject(underlying.gameObject);
            yield return null;
            Canvas.ForceUpdateCanvases();
        }

        [UnityTest]
        public IEnumerator HiddenTitleRequiresFourConsecutiveClicksAndResetsAcrossMenuActions()
        {
            int entered = 0;
            view.ConfigureHiddenEndingShortcut(() => entered++);
            Time.timeScale = 0;
            view.SetVisible(true);
            yield return new WaitForSecondsRealtime(.8f);
            var title = view.transform.Find("PauseColumn/PauseTitle").GetComponent<Button>();
            var rect = (RectTransform)title.transform;
            var point = RectTransformUtility.WorldToScreenPoint(null, rect.TransformPoint(rect.rect.center));
            var hits = new List<RaycastResult>();
            var pointer = new PointerEventData(events) { position = point, button = PointerEventData.InputButton.Left };
            events.RaycastAll(pointer, hits);
            Assert.That(hits[0].gameObject, Is.EqualTo(title.gameObject));
            for (int i = 0; i < 3; i++)
            {
                ExecuteEvents.Execute(title.gameObject, pointer, ExecuteEvents.pointerClickHandler);
                ExecuteEvents.Execute(title.gameObject, pointer, ExecuteEvents.pointerClickHandler);
                yield return null;
            }
            Assert.That(entered, Is.Zero, "Same-frame duplicate callbacks must not count twice.");
            yield return new WaitForSecondsRealtime(PauseMenuView.HiddenEndingClickInterval + .1f);
            title.onClick.Invoke();
            yield return null;
            Assert.That(entered, Is.Zero, "A long gap restarts the sequence.");
            view.Buttons[0].onClick.Invoke();
            for (int i = 0; i < 3; i++) { yield return null; title.onClick.Invoke(); }
            Assert.That(entered, Is.Zero, "Other menu actions reset the sequence.");
            view.ShowManual();
            yield return null;
            title.onClick.Invoke();
            Assert.That(entered, Is.Zero, "Hidden column cannot trigger from a submenu.");
            view.TryCloseManual();
            for (int i = 0; i < 3; i++) { yield return null; title.onClick.Invoke(); }
            Assert.That(entered, Is.Zero);
            view.SetVisible(false);
            view.SetVisible(true);
            for (int i = 0; i < 3; i++) { yield return null; title.onClick.Invoke(); }
            Assert.That(entered, Is.Zero, "Closing the pause menu resets partial progress.");
            yield return null;
            ExecuteEvents.Execute(title.gameObject, pointer, ExecuteEvents.pointerClickHandler);
            Assert.That(entered, Is.EqualTo(1));
            for (int i = 0; i < 8; i++) { yield return null; title.onClick.Invoke(); }
            Assert.That(entered, Is.EqualTo(1), "The shortcut fires once per open menu.");
            Assert.That(underlyingActivations, Is.Zero);
        }

        [UnityTest]
        public IEnumerator GmChordRequiresAllKeysAndConsumesOnlySuccessfulOpening()
        {
            view.SetVisible(true);
            yield return null;
            // Drive key states within this frame so hardware input cannot interfere between assertions.
            GmChord(true, true, true, true);
            Assert.That(view.GmVisible, Is.False, "Unbound menus, including previews, cannot open GM.");
            var selected = new List<EndingType>();
            view.ConfigureGmEndings(selected.Add);
            foreach (int missing in Enumerable.Range(0, 4))
            {
                var keys = new[] { true, true, true, true };
                keys[missing] = false;
                GmChord(keys[0], keys[1], keys[2], keys[3]);
                Assert.That(view.GmVisible, Is.False, "All four keys must be held together.");
            }
            GmChord(true, true, true, true);
            Assert.That(view.GmVisible, Is.True);
            Assert.That(events.currentSelectedGameObject, Is.SameAs(view.GmEndingButtons[0].gameObject));
            view.Buttons[0].onClick.Invoke();
            Assert.That(activations[0], Is.Zero, "GM must block root menu callbacks.");
            var endings = new[] { EndingType.TakenAway, EndingType.LockedInRoom, EndingType.PriestAttack, EndingType.DreamSea };
            for (int i = 0; i < endings.Length; i++)
            {
                view.GmEndingButtons[i].onClick.Invoke();
                view.GmEndingButtons[i].onClick.Invoke();
            }
            Assert.That(selected, Is.EqualTo(endings), "Each GM row dispatches its ending once per input frame.");
            Assert.That(view.TryCloseGm(), Is.True);
            Assert.That(view.GmVisible, Is.False);
            GmChord(true, true, true, true);
            Assert.That(view.GmVisible, Is.False, "ESC must not reopen GM until the chord is released.");
            foreach (bool settings in new[] { false, true })
            {
                GmChord(false, false, false, false);
                if (settings) view.ShowSettings(); else view.ShowManual();
                GmChord(true, true, true, true);
                Assert.That(view.GmVisible, Is.False, "Settings and manual pages do not accept the chord.");
                if (settings) view.TryCloseSettings(); else view.TryCloseManual();
                GmChord(true, true, true, true);
                Assert.That(view.GmVisible, Is.True, "Four keys held on the eligible root must open GM, even if first observed in a submenu.");
                view.TryCloseGm();
            }
            GmChord(false, false, false, false);
            GmChord(true, true, true, true);
            Assert.That(view.GmVisible, Is.True);
            view.SetVisible(false);
            Assert.That(view.GmVisible, Is.False);
            GmChord(false, false, false, false);
            GmChord(true, true, true, true);
            Assert.That(view.GmVisible, Is.False, "A hidden pause menu cannot open GM.");
            Assert.That(underlyingActivations, Is.Zero);
        }

        private void GmChord(bool n, bool o, bool r, bool m)
            => typeof(PauseMenuView).GetMethod("UpdateGmChord", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(view, new object[] { n, o, r, m });

        [UnityTest]
        public IEnumerator GmChordHeldBeforePauseOrBindingOpensWhenRootBecomesEligible()
        {
            view.ConfigureGmEndings(_ => { });
            GmChord(true, true, true, true);
            Assert.That(view.GmVisible, Is.False);
            Time.timeScale = 0;
            view.SetVisible(true);
            GmChord(true, true, true, true);
            Assert.That(view.GmVisible, Is.True, "Opening pause must not require a new key-down edge.");
            view.TryCloseGm();
            GmChord(true, true, true, true);
            Assert.That(view.GmVisible, Is.False, "Returning from GM while still holding the chord must stay on pause.");
            GmChord(false, true, true, true);
            view.ConfigureGmEndings(null);
            GmChord(true, true, true, true);
            Assert.That(view.GmVisible, Is.False);
            view.ConfigureGmEndings(_ => { });
            GmChord(true, true, true, true);
            Assert.That(view.GmVisible, Is.True, "An unbound menu must not consume a chord that never opened GM.");
            yield return null;
        }

        [UnityTest]
        public IEnumerator GmChordDoesNotAcceptSequentialLettersOrThreeHeldKeys()
        {
            view.ConfigureGmEndings(_ => { });
            view.SetVisible(true);
            for (int key = 0; key < 4; key++)
            {
                var keys = new bool[4]; keys[key] = true;
                GmChord(keys[0], keys[1], keys[2], keys[3]);
                Assert.That(view.GmVisible, Is.False, "Typing NORM separately must never open GM.");
                GmChord(false, false, false, false);
            }
            GmChord(true, true, true, false);
            Assert.That(view.GmVisible, Is.False);
            GmChord(true, true, true, true);
            Assert.That(view.GmVisible, Is.True);
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator RestoreOnlyFixtureOwnedState()
        {
            Time.timeScale = previousTimeScale;
            if (view != null) view.SetVisible(false);
            if (canvasObject != null) Object.Destroy(canvasObject);
            if (eventObject != null) Object.Destroy(eventObject);
            yield return null;
            foreach (var existing in suspendedEventSystems)
                if (existing != null) existing.enabled = true;
            suspendedEventSystems.Clear();
            if (previousEvents != null && previousEvents.isActiveAndEnabled)
            {
                EventSystem.current = previousEvents;
                previousEvents.SetSelectedGameObject(previousSelection != null && previousSelection.activeInHierarchy
                    ? previousSelection : null);
            }
        }

        [UnityTest, Timeout(15000)]
        public IEnumerator SeparateBlackStripsKeepWhiteLetteringAndMatchingPointerTargetsAcrossTheirWidth()
        {
            view.SetVisible(true);
            yield return WaitForFade(true);
            Canvas.ForceUpdateCanvases();
            var column = (RectTransform)view.transform.Find("PauseColumn");
            foreach (var button in view.Buttons)
            {
                var row = (RectTransform)button.transform;
                foreach (float x in new[] { .08f, .5f, .92f })
                {
                    var local = new Vector2(Mathf.Lerp(row.rect.xMin, row.rect.xMax, x), row.rect.center.y);
                    var screen = RectTransformUtility.WorldToScreenPoint(null, row.TransformPoint(local));
                    var hit = FirstHit(screen);
                    Assert.That(hit.GetComponentInParent<PauseMenuButton>(), Is.SameAs(button),
                        "Each black strip and its pointer target must agree across the complete row.");
                }
            }
            Assert.That(column.GetComponentsInChildren<PauseMenuPerspective>(true), Is.Empty,
                "The pause lettering must stay flat and aligned with its pointer target.");
            var selectedFill = view.Buttons[0].transform.Find("FocusFrame").GetComponent<Image>();
            Assert.That(selectedFill.color, Is.EqualTo(Color.white));
            Assert.That(selectedFill.sprite, Is.Not.Null);
            Assert.That(view.Buttons[0].transform.Find("Label").GetComponent<Text>().color, Is.EqualTo(Color.white),
                "Selection is a white outline; selected text must not become dark or invert.");
            var unselectedLabel = view.Buttons[2].transform.Find("Label").GetComponent<Text>();
            Assert.That(unselectedLabel.color, Is.EqualTo(Color.white));
            Assert.That(unselectedLabel, Is.TypeOf<TerminalBitmapText>(), "Keep the target project's shared bitmap typeface.");
            Assert.That(unselectedLabel.material.shader.name, Is.EqualTo("MiniGame/Pause Lettering"));
            foreach (var button in view.Buttons)
            {
                Assert.That(button.GetComponent<Image>().sprite, Is.Not.Null);
                Assert.That(button.transform.Find("CommandNumber"), Is.Null);
            }
            var first = (RectTransform)view.Buttons[0].transform;
            var next = (RectTransform)view.Buttons[1].transform;
            float gapY = (first.anchoredPosition.y - first.rect.height + next.anchoredPosition.y) * .5f;
            var gap = RectTransformUtility.WorldToScreenPoint(null, column.TransformPoint(new Vector2(first.rect.width * .5f, column.rect.yMax + gapY)));
            Assert.That(FirstHit(gap), Is.SameAs(view.gameObject), "The visible gaps between black strips must block world clicks without activating a command.");
            Assert.That(view.transform.Find("PauseColumn/PauseSessionState"), Is.Null);
            Assert.That(view.transform.Find("PauseColumn/PauseTitleRule"), Is.Null);
            Assert.That(view.transform.Find("PauseColumn/PauseContext"), Is.Null);
            Assert.That(view.transform.Find("PauseColumn/PauseHint"), Is.Null);
            Assert.That(view.GetComponentsInChildren<Text>(false).Any(text => text.name == "PauseStatus"), Is.False);
        }

        [UnityTest, Timeout(15000)]
        public IEnumerator OperationManualKeepsTheGamePausedAndBlocksMainMenuCallbacksUntilReturn()
        {
            view.ShowManual();
            Assert.That(view.ManualVisible, Is.False, "A hidden pause menu cannot open a detached manual.");
            Time.timeScale = 0;
            view.SetVisible(true);
            yield return WaitForFade(true);
            events.SetSelectedGameObject(view.Buttons[2].gameObject);
            view.ShowManual();
            Assert.That(view.ManualVisible, Is.True);
            Assert.That(view.transform.Find("PauseColumn").gameObject.activeSelf, Is.False);
            var manual = view.transform.Find("PauseOperationManual");
            var close = manual.Find("ManualReturn").GetComponent<Button>();
            Assert.That(events.currentSelectedGameObject, Is.SameAs(close.gameObject));
            Canvas.ForceUpdateCanvases();
            yield return null;

            foreach (var action in view.Buttons)
            {
                Submit(action);
                action.onClick.Invoke(); // Owner callbacks must also reject an already queued action from the hidden column.
            }
            Assert.That(activations, Is.All.EqualTo(0), "Reading controls cannot continue, save, load or invoke another pause action.");
            Assert.That(Time.timeScale, Is.Zero);
            Assert.That(view.GetComponent<CanvasGroup>().blocksRaycasts, Is.True);
            var center = RectTransformUtility.WorldToScreenPoint(null, ((RectTransform)manual).TransformPoint(Vector3.zero));
            Assert.That(FirstHit(center).transform.IsChildOf(manual), Is.True,
                "Clicks over the manual must be captured by its own panel, never by the world or hidden menu.");

            Submit(close);
            Assert.That(view.ManualVisible, Is.False);
            Assert.That(Time.timeScale, Is.Zero, "Returning from the manual must retain the pause; Continue owns resuming.");
            Assert.That(events.currentSelectedGameObject, Is.SameAs(view.Buttons[2].gameObject));
            Assert.That(view.TryCloseManual(), Is.False, "A second close request must not resume or perform another action.");
            yield return null;
            Submit(view.Buttons[2]);
            Assert.That(activations[2], Is.EqualTo(1), "The original menu action must still work after returning from its manual.");
        }

        [UnityTest, Timeout(15000)]
        public IEnumerator ManualListsLiveControlsAndResetsWhenThePauseCloses()
        {
            view.SetVisible(true);
            view.ShowManual();
            var manual = view.transform.Find("PauseOperationManual");
            string text = string.Join("\n", manual.GetComponentsInChildren<Text>(true).Select(label => label.text));
            foreach (string binding in new[] { "WASD", "鼠标左键", "Enter", "空格", "1–4", "Esc", "F5", "F9" })
                Assert.That(text, Does.Contain(binding));
            Assert.That(text, Does.Not.Contain("F2"));
            Assert.That(text, Does.Not.Contain("F3"));
            Assert.That(text, Does.Not.Contain("鼠标右键"), "The manual must describe the current left-click readable-prop binding.");
            Assert.That(manual.GetComponentsInChildren<Button>(true), Has.Length.EqualTo(1),
                "The manual has one return action and no gameplay or settings controls.");
            view.SetVisible(false);
            Assert.That(view.ManualVisible, Is.False);
            Assert.That(events.currentSelectedGameObject, Is.SameAs(underlying.gameObject));
            yield return WaitForFade(false);
            view.SetVisible(true);
            Assert.That(view.ManualVisible, Is.False);
            Assert.That(view.transform.Find("PauseColumn").gameObject.activeSelf, Is.True);
            Assert.That(events.currentSelectedGameObject, Is.SameAs(view.Buttons[0].gameObject));
        }

        [UnityTest, Timeout(15000)]
        public IEnumerator FullscreenModalInterceptsClicksAndFadesWhileTimeIsStopped()
        {
            Time.timeScale = 0;
            // The right side has no menu rows and the backdrop fades to transparent there.
            // It must still intercept clicks across the whole viewport.
            var point = new Vector2(Screen.width * .97f, Screen.height * .5f);
            Assert.That(FirstHit(point), Is.SameAs(underlying.gameObject));
            view.SetVisible(true);
            var group = view.GetComponent<CanvasGroup>();
            Assert.That(group.interactable, Is.True, "Opening must immediately enable modal input.");
            Assert.That(group.blocksRaycasts, Is.True, "Opening must immediately enable the modal hit surface.");
            Canvas.ForceUpdateCanvases();
            var rootImage = view.GetComponent<Image>();
            Assert.That(rootImage, Is.Not.Null);
            // GraphicRaycaster ignores depth -1 until Unity's native Canvas draw registers a newly enabled graphic.
            float depthDeadline = Time.realtimeSinceStartup + 3;
            while (rootImage.depth < 0 && Time.realtimeSinceStartup < depthDeadline)
                yield return null;
            Assert.That(rootImage.depth, Is.GreaterThanOrEqualTo(0),
                "The modal graphic must acquire a Canvas depth within the bounded wait. " + DescribeModalHit(FirstHit(point), point));
            var hit = FirstHit(point);
            Assert.That(hit.transform.IsChildOf(view.transform), Is.True,
                "The registered modal must intercept the next viewport click while game time is stopped. " + DescribeModalHit(hit, point));
            Click(hit, PointerEventData.InputButton.Left, point);
            Assert.That(underlyingActivations, Is.Zero);
            yield return WaitForFade(true);
            Assert.That(Time.timeScale, Is.Zero);
            Assert.That(FirstHit(point).transform.IsChildOf(view.transform), Is.True);

            view.SetVisible(false);
            Canvas.ForceUpdateCanvases();
            Assert.That(FirstHit(point), Is.SameAs(underlying.gameObject), "Closing must release the modal hit surface.");
            yield return WaitForFade(false);
            Click(FirstHit(point), PointerEventData.InputButton.Left, point);
            Assert.That(underlyingActivations, Is.EqualTo(1));
        }

        [UnityTest, Timeout(15000)]
        public IEnumerator KeyboardFocusSkipsDisabledRowsSurvivesUpdatesAndRestoresPreviousSelection()
        {
            view.Buttons[0].interactable = false;
            view.SetVisible(true);
            Assert.That(events.currentSelectedGameObject, Is.SameAs(view.Buttons[1].gameObject), "Opening must choose the first enabled action.");
            view.SetVisible(false);
            Assert.That(events.currentSelectedGameObject, Is.SameAs(underlying.gameObject));
            yield return WaitForFade(false);

            view.Buttons[0].interactable = true;
            view.Buttons[1].interactable = false;
            view.SetVisible(true);
            Assert.That(events.currentSelectedGameObject, Is.SameAs(view.Buttons[0].gameObject), "Continue is the default when available.");
            ExecuteEvents.Execute(events.currentSelectedGameObject,
                new AxisEventData(events) { moveDir = MoveDirection.Down, moveVector = Vector2.down }, ExecuteEvents.moveHandler);
            Assert.That(events.currentSelectedGameObject, Is.SameAs(view.Buttons[2].gameObject));
            for (int frame = 0; frame < 3; frame++)
            {
                view.SetVisible(true);
                view.SetContext("Day 2 / paused / update " + frame);
                yield return null;
                Assert.That(events.currentSelectedGameObject, Is.SameAs(view.Buttons[2].gameObject), "Refreshing context must not steal keyboard selection.");
            }
            view.SetVisible(false);
            Assert.That(events.currentSelectedGameObject, Is.SameAs(underlying.gameObject));
            yield return WaitForFade(false);

            view.SetVisible(true);
            underlying.interactable = false;
            view.SetVisible(false);
            Assert.That(events.currentSelectedGameObject, Is.Null, "A previous action that became unavailable must not regain focus.");
        }

        [UnityTest, Timeout(15000)]
        public IEnumerator DisabledActionsStaySilentAndPointerSubmitPairsInvokeEachActionOnce()
        {
            view.Buttons[1].interactable = false;
            view.SetVisible(true);
            yield return null;
            Click(view.Buttons[1].gameObject, PointerEventData.InputButton.Left, Vector2.zero);
            Submit(view.Buttons[1]);
            Assert.That(((PauseMenuButton)view.Buttons[1]).ActivateWithAudio(), Is.False);
            Assert.That(activations[1], Is.Zero);

            var action = (PauseMenuButton)view.Buttons[0];
            Click(action.gameObject, PointerEventData.InputButton.Left, Vector2.zero);
            Submit(action); // A second event route in the same frame must not duplicate the action.
            Assert.That(activations[0], Is.EqualTo(1));
            yield return null;
            Submit(action);
            Assert.That(action.ActivateWithAudio(), Is.True);
            Assert.That(activations[0], Is.EqualTo(2), "Native submit plus the owner's keyboard path is one accepted action.");
            yield return null;
            Assert.That(action.ActivateWithAudio(), Is.True);
            Submit(action);
            Assert.That(activations[0], Is.EqualTo(3), "The reverse native/manual order must also invoke once.");
            yield return null;
            Click(action.gameObject, PointerEventData.InputButton.Right, Vector2.zero);
            Assert.That(activations[0], Is.EqualTo(3));
            Click(view.Buttons[2].gameObject, PointerEventData.InputButton.Left, Vector2.zero);
            Assert.That(activations[2], Is.EqualTo(1), "Each row must retain its own callback.");

            view.SetVisible(false);
            yield return null;
            Click(action.gameObject, PointerEventData.InputButton.Left, Vector2.zero);
            Submit(action);
            Assert.That(action.ActivateWithAudio(), Is.False);
            Assert.That(activations[0], Is.EqualTo(3), "A closing menu cannot accept another action.");
        }

        [UnityTest, Timeout(15000)]
        public IEnumerator ResumeSubmitCannotAlsoChooseAResponseOrAdvanceDialogueInTheSameFrame()
        {
            var rules = PrototypeRules.Create();
            var content = new PrototypeContent(rules);
            var runtime = new GameRuntime(rules, content, useTerminalDirectory: true);
            var screenObject = new GameObject("Pause resume terminal regression");
            var screen = screenObject.AddComponent<MiniGameScreen>();
            try
            {
                screen.Bind(runtime);
                screen.enabled = false; // Drive the real keyboard entrypoint explicitly, without hardware input.
                Assert.That(runtime.TryInteract(InteractionTarget.Door).Accepted, Is.True);
                for (int step = 0; step < 4 && runtime.GetView().Cue != null; step++)
                    Assert.That(runtime.CompleteCue(runtime.GetView().Cue.Token).Accepted, Is.True);
                Assert.That(runtime.GetView().Game.Phase, Is.EqualTo(GamePhase.Apartment));
                Assert.That(runtime.TryInteract(InteractionTarget.Computer).Accepted, Is.True);
                if (runtime.GetView().Cue != null)
                    Assert.That(runtime.CompleteCue(runtime.GetView().Cue.Token).Accepted, Is.True);
                Assert.That(runtime.EnterSelectedPatient(runtime.GetView().Game.CurrentPatientId).Accepted, Is.True);
                Assert.That(runtime.GetView().Game.EncounterStage, Is.EqualTo(EncounterStage.NormalDialogue));
                Assert.That(screen.GetComponentsInChildren<Button>(false).Any(button =>
                    button.name.StartsWith("Reply-", StringComparison.Ordinal) && button.IsInteractable()), Is.True);
                Assert.That(screen.WorldInputBlockedThisFrame, Is.True,
                    "The synchronous fixture still shares the door-to-room input handoff frame.");
                Assert.That(screen.HandleTerminalKey(KeyCode.DownArrow), Is.False,
                    "The input that returns to the world cannot also select a terminal response in that frame.");
                yield return null;
                Assert.That(screen.HandleTerminalKey(KeyCode.DownArrow), Is.True, "Select a real underlying response before pausing.");

                var menu = screen.GetComponentInChildren<PauseMenuView>(true);
                Assert.That(menu, Is.Not.Null);
                foreach (var nextKey in new[] { KeyCode.Return, KeyCode.Space })
                {
                    if (nextKey == KeyCode.Space)
                    {
                        Assert.That(runtime.GetView().Cue?.Kind, Is.EqualTo(PresentationKind.ResponseBranch));
                        Assert.That(screen.GetComponentsInChildren<Button>(false).Any(button =>
                            button.name == "Continue" && button.IsInteractable()), Is.True,
                            "The second pause must cover an actual enabled dialogue Continue action.");
                    }
                    var before = runtime.GetView();
                    var checkpoint = runtime.Save();
                    Assert.That(runtime.SetPaused(true).Accepted, Is.True);
                    Assert.That(events.currentSelectedGameObject, Is.SameAs(menu.Buttons[0].gameObject));

                    Submit(menu.Buttons[0]); // Native uGUI Submit synchronously invokes the real resume callback.
                    Assert.That(runtime.GetView().Paused, Is.False);
                    Assert.That(screen.HandleTerminalKey(KeyCode.Return), Is.False,
                        "The resume key must not also confirm the selected terminal response in the same frame.");
                    Assert.That(screen.HandleTerminalKey(KeyCode.Space), Is.False,
                        "The resume key must not also advance the underlying dialogue in the same frame.");
                    Assert.That(runtime.GetView().Cue?.Token, Is.EqualTo(before.Cue?.Token));
                    Assert.That(runtime.Save(), Is.EqualTo(checkpoint),
                        "Resuming must leave the core state, presentation cue and transcript unchanged.");

                    yield return null;
                    Assert.That(screen.HandleTerminalKey(nextKey), Is.True,
                        "A deliberate confirm on the next frame must work; the input guard cannot remain latched.");
                    Assert.That(runtime.GetView().Game.EncounterStage, Is.Not.EqualTo(before.Game.EncounterStage));
                    yield return null;
                }

                // Rebinding an unpaused checkpoint from the pause menu closes the same modal.
                Assert.That(screen.HandleTerminalKey(KeyCode.DownArrow), Is.True);
                Assert.That(screen.HandleTerminalKey(KeyCode.Return), Is.True);
                Assert.That(runtime.GetView().Cue?.Kind, Is.EqualTo(PresentationKind.ResponseBranch));
                var restored = GameRuntime.Restore(runtime.Save(), rules, content);
                var restoredCheckpoint = restored.Save();
                var restoredCueToken = restored.GetView().Cue.Token;
                Assert.That(runtime.SetPaused(true).Accepted, Is.True);
                Assert.That(runtime.GetView().Paused, Is.True);

                screen.Bind(restored);
                Assert.That(restored.GetView().Paused, Is.False);
                Assert.That(screen.GetComponentsInChildren<Button>(false).Any(button =>
                    button.name == "Continue" && button.IsInteractable()), Is.True,
                    "The rebound runtime must expose a real Continue action, so the guard is what prevents advancement.");
                Assert.That(screen.HandleTerminalKey(KeyCode.Return), Is.False,
                    "The key that loads an unpaused checkpoint cannot also confirm its dialogue in the same frame.");
                Assert.That(screen.HandleTerminalKey(KeyCode.Space), Is.False);
                Assert.That(restored.GetView().Cue?.Token, Is.EqualTo(restoredCueToken));
                Assert.That(restored.Save(), Is.EqualTo(restoredCheckpoint));
                yield return null;
                Assert.That(screen.HandleTerminalKey(KeyCode.Space), Is.True,
                    "Rebinding must release the input guard on the following frame.");
                Assert.That(restored.GetView().Game.EncounterStage, Is.EqualTo(EncounterStage.NormalDialogue));
                Assert.That(restored.GetView().Cue, Is.Null);
            }
            finally
            {
                Object.Destroy(screenObject);
            }
            yield return null;
        }

        private GameObject FirstHit(Vector2 position)
        {
            var hits = new List<RaycastResult>();
            events.RaycastAll(new PointerEventData(events) { position = position }, hits);
            Assert.That(hits, Is.Not.Empty, "The fixture must contain a full-screen raycast target.");
            return hits[0].gameObject;
        }

        private void Click(GameObject target, PointerEventData.InputButton button, Vector2 position)
            => ExecuteEvents.ExecuteHierarchy(target, new PointerEventData(events) { button = button, position = position, clickCount = 1 },
                ExecuteEvents.pointerClickHandler);

        private void Submit(Button button)
            => ExecuteEvents.Execute(button.gameObject, new BaseEventData(events), ExecuteEvents.submitHandler);

        private string DescribeModalHit(GameObject hit, Vector2 point)
        {
            var rect = (RectTransform)view.transform;
            var corners = new Vector3[4];
            rect.GetWorldCorners(corners);
            var group = view.GetComponent<CanvasGroup>();
            var rootImage = view.GetComponent<Image>();
            var canvas = canvasObject.GetComponent<Canvas>();
            var hitCanvas = hit.GetComponentInParent<Canvas>();
            var hits = new List<RaycastResult>();
            events.RaycastAll(new PointerEventData(events) { position = point }, hits);
            var hitDescriptions = hits.ConvertAll(result => result.gameObject.name
                + "(depth=" + result.depth + ", order=" + result.sortingOrder + ")");
            string imageState = rootImage == null ? "missing" :
                "active=" + rootImage.isActiveAndEnabled
                + ", cull=" + rootImage.canvasRenderer.cull
                + ", absoluteDepth=" + rootImage.canvasRenderer.absoluteDepth
                + ", raycastTarget=" + rootImage.raycastTarget
                + ", alphaHitTestMinimumThreshold=" + rootImage.alphaHitTestMinimumThreshold
                + ", canvas=" + (rootImage.canvas != null ? rootImage.canvas.name : "null");
            return "hit=" + hit.name
                + "; hitEqualsRoot=" + (hit == view.gameObject)
                + "; hitIsChildOfRoot=" + hit.transform.IsChildOf(view.transform)
                + "; rootIsChildOfSelf=" + view.transform.IsChildOf(view.transform)
                + "; rootActive=" + view.gameObject.activeInHierarchy
                + "; screen=" + Screen.width + "x" + Screen.height + ", point=" + point
                + "; rootRect=" + rect.rect
                + "; worldCorners=[" + string.Join(", ", Array.ConvertAll(corners, corner => corner.ToString("F3"))) + "]"
                + "; group(alpha=" + group.alpha + ", interactable=" + group.interactable + ", blocksRaycasts=" + group.blocksRaycasts + ")"
                + "; rootImage(" + imageState + ")"
                + "; canvas(mode=" + canvas.renderMode + ", sortingLayer=" + canvas.sortingLayerID
                + ", sortingOrder=" + canvas.sortingOrder + ", overrideSorting=" + canvas.overrideSorting + ")"
                + "; hitCanvas=" + (hitCanvas != null ? hitCanvas.name + ", sortingOrder=" + hitCanvas.sortingOrder : "null")
                + "; allHits=[" + string.Join(", ", hitDescriptions) + "]";
        }

        private IEnumerator WaitForFade(bool showing)
        {
            var group = view.GetComponent<CanvasGroup>();
            float deadline = Time.realtimeSinceStartup + 3;
            while ((showing ? group.alpha < .999f : view.gameObject.activeSelf) && Time.realtimeSinceStartup < deadline)
                yield return null;
            if (showing) Assert.That(group.alpha, Is.GreaterThanOrEqualTo(.999f), "Fade-in must advance without scaled game time.");
            else Assert.That(view.gameObject.activeSelf, Is.False, "Fade-out must finish and disable the menu.");
        }
    }
}
