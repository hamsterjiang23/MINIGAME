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
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace MiniGame.UnityHost.Tests
{
    public sealed class SupervisorDoorLockPlayModeTests
    {
        private MiniGameBootstrap host;
        private MiniGameYarnBridge bridge;
        private DoorViewCamera doorway;
        private HingedDoor door;
        private WorldInteraction doorInteraction;
        private RoomInteractionSettings settings;
        private GameRules rules;
        private PrototypeContent content;
        private readonly List<string> temporarySaves = new List<string>();
        private readonly HashSet<string> inspectionTokens = new HashSet<string>();
        private GameRuntime observedRuntime;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            MiniGameSessionHost.Release();
            yield return SceneManager.LoadSceneAsync(MiniGameSessionHost.CombinedScene);
            yield return null;
            host = Object.FindObjectOfType<MiniGameBootstrap>();
            Assert.That(host, Is.Not.Null);
            bridge = Object.FindObjectOfType<MiniGameYarnBridge>();
            door = Object.FindObjectOfType<HingedDoor>();
            doorway = host.Player.ViewCamera.GetComponent<DoorViewCamera>();
            doorInteraction = SceneObjects<WorldInteraction>().Single(value => value.Target == InteractionTarget.Door);
            host.Effects.PrepareForRuntimeChange(); host.Effects.enabled = false;
            host.Dreams?.PrepareForRuntimeChange();
            if (host.Dreams != null) host.Dreams.enabled = false;
            bridge.enabled = false; host.Player.enabled = false;
            host.Screen.DialogueCharactersPerSecond = 0;
            settings = ScriptableObject.CreateInstance<RoomInteractionSettings>();
            Assert.That(settings.LockDoorAfterSupervisor, Is.True, "New global settings must default to locking only after the conversation.");
            host.ConfigureRoomInteractions(settings);
            rules = new GameRules("supervisor-door-lock-fixture",
                Enumerable.Range(0, 4).Select(day => new PatientDefinition("door-lock-patient-" + day, "测试对象" + day,
                    day, 0, null, PatientPresence.TreatmentEnded)),
                Enumerable.Range(0, 4).Select(day => new DayDefinition(day, 1, new[] {
                    new MeetingDefinition("door-lock-patient-" + day, new[] {
                        new RoundDefinition("door-lock-question-" + day, new[] {
                            new ResponseDefinition("door-lock-answer-" + day, "door-lock-branch-" + day, 0) }) },
                        null, 100, 0, new Dictionary<Disposition,string> { [Disposition.Purify] = "door-lock-purify-" + day }) })),
                new[] { EndingType.PriestAttack, EndingType.LockedInRoom, EndingType.TakenAway });
            content = new PrototypeContent(rules);
            var fixture = new GameRuntime(rules, content, true);
            Accept(fixture.BeginDayZeroOpening());
            AdvanceToNextMorning(fixture, 1);
            Install(fixture);
            yield return null;
            Assert.That(host.Runtime.LockDoorAfterSupervisor, Is.True);
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (observedRuntime != null) observedRuntime.Changed -= ObserveInspection;
            if (bridge != null) bridge.enabled = false;
            if (host != null)
            {
                host.Player.enabled = false;
                host.enabled = false;
                host.Effects.PrepareForRuntimeChange();
            }
            MiniGameSessionHost.Release();
            foreach (string path in temporarySaves) if (File.Exists(path)) File.Delete(path);
            temporarySaves.Clear();
            if (settings != null) Object.Destroy(settings);
            Cursor.lockState = CursorLockMode.None; Cursor.visible = true;
            yield return null;
        }

        [UnityTest, Timeout(65000)]
        public IEnumerator CompletedSupervisorLocksDoorLiveOverrideAndNextMorningRemainUsable()
        {
            Assert.That(host.Runtime.GetView().Game.Phase, Is.EqualTo(GamePhase.MorningInspection));
            Assert.That(host.Runtime.IsDoorLockedAfterSupervisor, Is.False);
            StageAtDoor();
            Assert.That(host.Player.FocusedInteraction, Is.SameAs(doorInteraction));
            Assert.That(host.Player.InteractFocused(), Is.True, "The required first conversation must remain reachable.");
            yield return CompleteSupervisorConversation(1);
            Assert.That(host.Runtime.GetView().Game.Phase, Is.EqualTo(GamePhase.Apartment));
            Assert.That(host.Runtime.IsDoorLockedAfterSupervisor, Is.True);
            Assert.That(host.Runtime.GetView().DoorViewActive, Is.True);
            Assert.That(doorway.TryReturn(), Is.True, "Locking after dialogue must still let the player return inside.");
            yield return DoorClosedInWorld();
            AssertLockedDoorFocus();
            Assert.That(door.TryInteract().Accepted, Is.False, "Physical hinge opening must use the same policy.");
            Assert.That(doorway.TryEnter(), Is.False);
            int conversations = inspectionTokens.Count;
            Assert.That(conversations, Is.EqualTo(1));

            settings.LockDoorAfterSupervisor = false;
            yield return null; yield return null;
            Assert.That(host.Runtime.LockDoorAfterSupervisor, Is.False, "The host must apply a live global setting change.");
            Assert.That(host.Runtime.IsDoorLockedAfterSupervisor, Is.False);
            Assert.That(host.Runtime.GetView().AllowedInteractions, Does.Contain(InteractionTarget.Door));
            StageAtDoor();
            Assert.That(host.Player.InteractFocused(), Is.True);
            yield return Until(() => host.Runtime.GetView().Input == InputContext.DoorView
                && host.Runtime.GetView().Cue == null, "unlocked door view", 7);
            Assert.That(inspectionTokens.Count, Is.EqualTo(conversations), "Reopening after the meeting cannot replay supervisor dialogue.");
            Assert.That(bridge.SupervisorSourceId, Is.Empty);
            settings.LockDoorAfterSupervisor = true;
            yield return null; yield return null;
            Assert.That(host.Runtime.IsDoorLockedAfterSupervisor, Is.True);
            Assert.That(doorway.TryReturn(), Is.True, "Changing the setting outside must not strand the player.");
            yield return DoorClosedInWorld();
            AssertLockedDoorFocus();

            string path = Path.Combine(Application.temporaryCachePath, "supervisor-door-lock-" + Guid.NewGuid().ToString("N") + ".json");
            temporarySaves.Add(path);
            var session = MiniGameSessionHost.Current;
            session.SaveTo(path);
            Assert.That(File.Exists(path), Is.True, session.LastMessage);
            settings.LockDoorAfterSupervisor = false;
            yield return null; yield return null;
            session.LoadFrom(path);
            Assert.That(session.LastMessage, Does.Contain("已恢复"));
            Observe(host.Runtime);
            yield return null;
            Assert.That(host.Runtime, Is.SameAs(session.Runtime));
            Assert.That(host.Runtime.GetView().Game.Day, Is.EqualTo(1));
            Assert.That(host.Runtime.GetView().Game.Phase, Is.EqualTo(GamePhase.Apartment));
            Assert.That(host.Runtime.LockDoorAfterSupervisor, Is.False, "Load must reapply the current global switch, not the saved policy.");
            StageAtDoor();
            Assert.That(host.Player.InteractFocused(), Is.True, "The loaded, globally unlocked door must actually open.");
            yield return Until(() => host.Runtime.GetView().Input == InputContext.DoorView
                && host.Runtime.GetView().Cue == null, "reopened door after load", 7);
            Assert.That(inspectionTokens.Count, Is.EqualTo(conversations));
            Assert.That(doorway.TryReturn(), Is.True);
            yield return DoorClosedInWorld();

            settings.LockDoorAfterSupervisor = true;
            yield return null; yield return null;
            bridge.enabled = false;
            var nextMorning = GameRuntime.Restore(host.Runtime.Save(), rules, content);
            AdvanceToNextMorning(nextMorning, 2);
            Install(nextMorning);
            yield return null;
            Assert.That(host.Runtime.GetView().Game.Day, Is.EqualTo(2));
            Assert.That(host.Runtime.GetView().Game.Phase, Is.EqualTo(GamePhase.MorningInspection));
            Assert.That(host.Runtime.LockDoorAfterSupervisor, Is.True);
            Assert.That(host.Runtime.IsDoorLockedAfterSupervisor, Is.False);
            Assert.That(host.Runtime.GetView().AllowedInteractions, Does.Contain(InteractionTarget.Door));
            StageAtDoor();
            Assert.That(host.Player.InteractFocused(), Is.True, "Yesterday's lock must not block tomorrow's required inspection.");
            string firstLine = SupervisorDialogueContent.SourcePrefix
                + SupervisorDialogueContent.Load().conversations.Single(value => value.day == 2).lines[0].id;
            yield return Until(() => bridge.SupervisorSourceId == firstLine, "second morning supervisor conversation", 7);
            Assert.That(inspectionTokens.Count, Is.EqualTo(conversations + 1));
        }

        private void Install(GameRuntime runtime)
        {
            var session = MiniGameSessionHost.Install(runtime, rules, content, host.DreamProfile);
            host.RestoreFromSession(session);
            host.Screen.SetTransitionLocked(false);
            bridge.enabled = true;
            bridge.PresentCurrent();
            Observe(runtime);
        }

        private void Observe(GameRuntime runtime)
        {
            if (observedRuntime != null) observedRuntime.Changed -= ObserveInspection;
            observedRuntime = runtime;
            runtime.Changed += ObserveInspection;
            ObserveInspection(runtime.GetView());
        }
        private void ObserveInspection(RuntimeView view)
        {
            if (view.Cue?.Kind == PresentationKind.MorningInspection) inspectionTokens.Add(view.Cue.Token);
        }

        private IEnumerator CompleteSupervisorConversation(int day)
        {
            var expected = SupervisorDialogueContent.Load().conversations.Single(value => value.day == day)
                .lines.Select(value => SupervisorDialogueContent.SourcePrefix + value.id).ToArray();
            var seen = new List<string>();
            float deadline = Time.realtimeSinceStartup + 18;
            while (Time.realtimeSinceStartup < deadline)
            {
                yield return null;
                string source = bridge.SupervisorSourceId;
                if (!string.IsNullOrEmpty(source) && !seen.Contains(source)) seen.Add(source);
                if (host.Runtime.GetView().Game.Phase == GamePhase.Apartment
                    && host.Runtime.GetView().Cue == null && !bridge.IsRunning) break;
                host.Screen.AdvanceWorldDialogue();
            }
            Assert.That(seen, Is.EqualTo(expected), bridge.Diagnostics);
            Assert.That(host.Runtime.GetView().Cue, Is.Null, bridge.Diagnostics);
            Assert.That(bridge.IsRunning, Is.False, bridge.Diagnostics);
        }

        private IEnumerator DoorClosedInWorld() => Until(() => !host.Runtime.GetView().DoorViewActive
            && !host.Runtime.GetView().RoomDoorOpen && host.Runtime.GetView().Cue == null && !door.IsMoving
            && host.Runtime.GetView().Input == InputContext.World, "return and door closure", 7);

        private void StageAtDoor()
        {
            var anchor = SceneObjects<Transform>().Single(value => value.name == "Door approach");
            var controller = host.Player.GetComponent<CharacterController>();
            bool wasEnabled = controller.enabled;
            controller.enabled = false;
            host.Player.transform.position = anchor.position;
            controller.enabled = wasEnabled;
            var hitbox = doorInteraction.GetComponent<Collider>() ?? doorInteraction.GetComponentInChildren<Collider>();
            Assert.That(hitbox, Is.Not.Null);
            host.Player.FacePoint(hitbox.bounds.center);
            Physics.SyncTransforms();
            host.Player.RefreshInteractionFocus();
        }

        private void AssertLockedDoorFocus()
        {
            Assert.That(host.Runtime.GetView().AllowedInteractions, Has.No.Member(InteractionTarget.Door));
            StageAtDoor();
            Assert.That(host.Screen.InteractionCursor, Is.EqualTo(InteractionCursorKind.None));
            Assert.That(host.Player.FocusedInteraction.IsHighlighted, Is.False);
            Assert.That(host.Player.InteractPrimaryFocused(), Is.False, "The left-click path must not bypass the global lock.");
            Assert.That(host.Runtime.GetView().RoomDoorOpen, Is.False);
        }

        private T[] SceneObjects<T>() where T : Component => host.gameObject.scene.GetRootGameObjects()
            .SelectMany(value => value.GetComponentsInChildren<T>(true)).ToArray();

        private static IEnumerator Until(Func<bool> condition, string label, float seconds)
        {
            float deadline = Time.realtimeSinceStartup + seconds;
            while (!condition() && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.That(condition(), Is.True, label);
        }

        private static void AdvanceToNextMorning(GameRuntime runtime, int targetDay)
        {
            for (int guard = 0; guard < 100; guard++)
            {
                var view = runtime.GetView();
                if (view.Game.Day == targetDay && view.Game.Phase == GamePhase.MorningInspection) return;
                RuntimeResult result;
                if (view.Cue != null) result = runtime.CompleteCue(view.Cue.Token);
                else if (view.DoorViewActive) result = runtime.ExitDoorView();
                else if (view.TerminalPage == TerminalPage.Archive) result = runtime.LeaveTerminal();
                else if (view.Game.Phase == GamePhase.Evening && view.TerminalOpen) result = runtime.LeaveTerminal();
                else if (view.TerminalPage == TerminalPage.Directory)
                    result = runtime.EnterSelectedPatient(view.Patients.First(value => value.CanReceiveToday).Id);
                else if (view.Input == InputContext.Dialogue)
                    result = view.Game.EncounterStage == EncounterStage.AwaitingDisposition
                        ? runtime.SubmitDisposition(view.Game.CurrentEncounterId, Disposition.Purify)
                        : runtime.SelectResponse(view.Game.CurrentEncounterId, view.Game.CurrentNodeId, view.Game.AvailableResponseIds[0]);
                else if (view.Game.Phase == GamePhase.Evening || view.Game.Phase == GamePhase.AfterDream)
                    result = runtime.TryInteract(InteractionTarget.Bed);
                else if (view.Game.Phase == GamePhase.Apartment || view.Game.Phase == GamePhase.Work)
                    result = runtime.TryInteract(InteractionTarget.Computer);
                else throw new InvalidOperationException("No legal next-morning fixture step: " + view.Game.Phase + "/" + view.Input);
                Accept(result);
            }
            Assert.Fail("Next-morning fixture exceeded its accepted-command bound.");
        }

        private static void Accept(RuntimeResult result) => Assert.That(result.Accepted, Is.True, result.Reason);
    }
}
