using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MiniGame.Core;
using MiniGame.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace MiniGame.UnityHost.Tests
{
    public sealed class TerminalHtmlParityTests
    {
        [UnityTest]
        public IEnumerator ChoiceKeysRequireSelectionWrapAndSupportAllFourNumberShortcuts()
        {
            var runtime = CreateRuntime();
            var owner = new GameObject("HTML Choice Input Test");
            var screen = owner.AddComponent<MiniGameScreen>();
            try
            {
                OpenReview(screen, runtime);
                var selected = new List<string>();
                var responses = Responses();
                screen.ShowResponseButtons(responses, selected.Add);
                screen.HandleTerminalKey(KeyCode.Return);
                screen.HandleTerminalKey(KeyCode.KeypadEnter);
                screen.HandleTerminalKey(KeyCode.Space);
                Assert.That(selected, Is.Empty, "A new choice has no selection; Enter and Space cannot silently choose it.");

                screen.HandleTerminalKey(KeyCode.UpArrow);
                screen.HandleTerminalKey(KeyCode.Space);
                Assert.That(selected, Is.Empty, "Space cannot commit even when a choice has been highlighted.");
                screen.HandleTerminalKey(KeyCode.Return);
                Assert.That(selected.Last(), Is.EqualTo("choice-4"), "Up from no selection starts at the last choice.");

                screen.ShowResponseButtons(responses, selected.Add);
                screen.HandleTerminalKey(KeyCode.Return);
                Assert.That(selected, Has.Count.EqualTo(1), "Replacing the choices clears the previous selection.");
                screen.HandleTerminalKey(KeyCode.DownArrow);
                screen.HandleTerminalKey(KeyCode.KeypadEnter);
                Assert.That(selected.Last(), Is.EqualTo("choice-1"), "Down from no selection starts at the first choice.");

                screen.ShowResponseButtons(responses, selected.Add);
                screen.HandleTerminalKey(KeyCode.UpArrow);
                screen.HandleTerminalKey(KeyCode.DownArrow);
                screen.HandleTerminalKey(KeyCode.Return);
                Assert.That(selected.Last(), Is.EqualTo("choice-1"), "Down wraps from the last choice to the first.");

                screen.ShowResponseButtons(responses, selected.Add);
                screen.HandleTerminalKey(KeyCode.DownArrow);
                screen.HandleTerminalKey(KeyCode.UpArrow);
                screen.HandleTerminalKey(KeyCode.Return);
                Assert.That(selected.Last(), Is.EqualTo("choice-4"), "Up wraps from the first choice to the last.");

                for (int index = 0; index < responses.Length; index++)
                {
                    screen.ShowResponseButtons(responses, selected.Add);
                    int before = selected.Count;
                    screen.HandleTerminalKey((KeyCode)((int)KeyCode.Alpha1 + index));
                    Assert.That(selected.Count, Is.EqualTo(before + 1));
                    Assert.That(selected.Last(), Is.EqualTo(responses[index].Id));
                }
                Assert.That(selected, Has.Count.EqualTo(8), "Each confirmation invokes exactly one response callback.");
            }
            finally { Object.Destroy(owner); }
            yield return null;
        }

        [UnityTest]
        public IEnumerator HistoryShowsFiveClippedRowsAndBlocksDialogueKeyboardInput()
        {
            var runtime = CreateRuntime();
            var owner = new GameObject("HTML History Test");
            var screen = owner.AddComponent<MiniGameScreen>();
            try
            {
                OpenReview(screen, runtime);
                for (int index = 0; index < 8; index++)
                    screen.SetDialogueLine("对象", "记录" + index + "：" + new string('长', 80) + "\n第二行",
                        "html-history-" + index, "ML01");
                int advanced = 0;
                screen.ShowResponseButtons(Responses(), _ => advanced++);
                ButtonNamed(screen, "HistoryTab").onClick.Invoke();
                yield return null;
                Canvas.ForceUpdateCanvases();

                var rows = screen.GetComponentsInChildren<Text>(false)
                    .Where(text => text.name == "TranscriptText" || Enumerable.Range(1, 4).Any(index => text.name == "TranscriptText" + index))
                    .OrderBy(text => text.name).ToArray();
                Assert.That(rows, Has.Length.EqualTo(5));
                Assert.That(rows.Select(row => row.GetComponentInParent<RectMask2D>()).All(mask => mask != null), Is.True,
                    "Long transcript lines need a clipping viewport for every fixed row.");
                Assert.That(rows.Select(row => row.GetComponentInParent<RectMask2D>()).Distinct().Count(), Is.EqualTo(5),
                    "Each row must clip independently so a long line cannot consume the following row.");
                for (int index = 0; index < rows.Length; index++)
                {
                    Assert.That(rows[index].text, Does.Contain("记录" + (index + 3)));
                    Assert.That(rows[index].text, Does.Not.Contain("\n"));
                    Assert.That(rows[index].horizontalOverflow, Is.EqualTo(HorizontalWrapMode.Overflow));
                    Assert.That(rows[index].verticalOverflow, Is.EqualTo(VerticalWrapMode.Truncate));
                }
                Assert.That(ObjectNamed(screen, "HistoryThumb").activeInHierarchy, Is.True);
                var beforeRows = rows.Select(row => row.text).ToArray();
                int quota = runtime.GetView().Game.RemainingPurificationQuota;
                foreach (var key in new[] { KeyCode.Return, KeyCode.KeypadEnter, KeyCode.Space, KeyCode.Alpha1,
                    KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4, KeyCode.UpArrow, KeyCode.DownArrow })
                {
                    screen.HandleTerminalKey(key);
                    Assert.That(rows.Select(row => row.text), Is.EqualTo(beforeRows), "History content changed for " + key);
                }
                Assert.That(advanced, Is.Zero);
                Assert.That(runtime.GetView().Game.RemainingPurificationQuota, Is.EqualTo(quota));
                Assert.That(screen.TryCloseHistory(), Is.True);
                screen.HandleTerminalKey(KeyCode.Alpha2);
                Assert.That(advanced, Is.EqualTo(1));

                screen.ShowCueText("系统提示", () => advanced++);
                ButtonNamed(screen, "HistoryTab").onClick.Invoke();
                screen.HandleTerminalKey(KeyCode.Return);
                screen.HandleTerminalKey(KeyCode.Space);
                Assert.That(advanced, Is.EqualTo(1), "History also blocks line continuation.");
                Assert.That(screen.TryCloseHistory(), Is.True);
                screen.HandleTerminalKey(KeyCode.Space);
                Assert.That(advanced, Is.EqualTo(2), "Space still advances a line after history closes.");
            }
            finally { Object.Destroy(owner); }
            yield return null;
        }

        [UnityTest]
        public IEnumerator DispositionKeepsLastSpeechAndLocalSignalLossThenReturnsToDirectoryBeforeRoomExit()
        {
            var runtime = CreateRuntime();
            var owner = new GameObject("HTML Archive Flow Test");
            var screen = owner.AddComponent<MiniGameScreen>();
            try
            {
                OpenReview(screen, runtime);
                string patientId = runtime.GetView().Game.CurrentPatientId;
                string lastSpeech = string.Empty;
                for (int step = 0; step < 32 && runtime.GetView().Game.EncounterStage != EncounterStage.AwaitingDisposition; step++)
                {
                    var view = runtime.GetView();
                    if (view.Cue != null)
                    {
                        Assert.That(view.Cue.Kind, Is.EqualTo(PresentationKind.ResponseBranch));
                        lastSpeech = "对象回应 " + step;
                        screen.SetDialogueLine("对象", lastSpeech, "html-reaction-" + step, "ML02");
                        Assert.That(runtime.CompleteCue(view.Cue.Token).Accepted, Is.True);
                    }
                    else
                    {
                        Assert.That(view.Dialogue, Is.Not.Null);
                        lastSpeech = view.Dialogue.Text;
                        screen.SetDialogueLine(view.Dialogue.Speaker, lastSpeech, "html-line-" + step, "ML01");
                        Assert.That(runtime.SelectResponse(view.Game.CurrentEncounterId, view.Game.CurrentNodeId,
                            view.Dialogue.Responses[0].Id).Accepted, Is.True);
                    }
                }
                var dispositionView = runtime.GetView();
                Assert.That(dispositionView.Game.EncounterStage, Is.EqualTo(EncounterStage.AwaitingDisposition),
                    "The real core conversation must reach disposition within the bounded loop.");
                Assert.That(TextNamed(screen, "TerminalDialogueBody").text, Is.EqualTo(lastSpeech));
                screen.SetDialogueText("系统", "请选择处置");
                screen.ShowCueText("处置记录准备中", null);
                Assert.That(TextNamed(screen, "TerminalDialogueBody").text, Is.EqualTo(lastSpeech));
                Assert.That(runtime.SubmitDisposition(dispositionView.Game.CurrentEncounterId,
                    dispositionView.Game.AvailableDispositions.First()).Accepted, Is.True);
                Assert.That(runtime.GetView().Cue.Kind, Is.EqualTo(PresentationKind.Reaction));
                lastSpeech = "我知道了。这是我的最后一句话。";
                screen.SetDialogueLine("对象", lastSpeech, "html-final-speech", "ML03");
                screen.SetGlitch(.55f);
                Assert.That(ObjectNamed(screen, "GeneratedSignalLoss").activeSelf, Is.False);
                Assert.That(ObjectNamed(screen, "MonitorSignalLoss").activeSelf, Is.False,
                    "Signal loss starts after the actor finishes the reaction.");
                Assert.That(runtime.CompleteCue(runtime.GetView().Cue.Token).Accepted, Is.True);
                Assert.That(runtime.GetView().Cue.Kind, Is.EqualTo(PresentationKind.ExitTerminal));
                Assert.That(runtime.GetView().LeavingTerminal, Is.False);
                screen.SetDialogueText("系统", "对象已归档");
                screen.SetGlitch(.55f);
                Assert.That(ObjectNamed(screen, "GeneratedSignalLoss").activeSelf, Is.False);
                Assert.That(ObjectNamed(screen, "MonitorSignalLoss").activeInHierarchy, Is.True);
                Assert.That(TextNamed(screen, "TerminalDialogueBody").text, Is.EqualTo(lastSpeech));

                Assert.That(runtime.CompleteCue(runtime.GetView().Cue.Token).Accepted, Is.True);
                screen.SetGlitch(0);
                Assert.That(runtime.GetView().TerminalPage, Is.EqualTo(TerminalPage.Archive));
                Assert.That(ObjectNamed(screen, "SignalLostMessage").activeInHierarchy, Is.True);
                Assert.That(TextNamed(screen, "TerminalDialogueBody").text, Is.EqualTo(lastSpeech));
                var checkpoint = runtime.Save();
                var rules = PrototypeRules.Create();
                runtime = GameRuntime.Restore(checkpoint, rules, new PrototypeContent(rules));
                Assert.That(runtime.GetView().Transcript.Last(entry => !entry.IsPlayer).Text, Is.EqualTo(lastSpeech));
                screen.Bind(runtime);
                Assert.That(runtime.GetView().TerminalPage, Is.EqualTo(TerminalPage.Archive));
                Assert.That(TextNamed(screen, "TerminalDialogueBody").text, Is.EqualTo(lastSpeech),
                    "Rebinding an archive checkpoint restores the last actor speech from its transcript.");
                Assert.That(ObjectNamed(screen, "SignalLostMessage").activeInHierarchy, Is.True);
                screen.HandleTerminalKey(KeyCode.Return);
                Assert.That(runtime.GetView().TerminalPage, Is.EqualTo(TerminalPage.Directory));
                Assert.That(runtime.GetView().TerminalOpen, Is.True, "Archive completion returns to the terminal directory, not the room.");
                Assert.That(ObjectNamed(screen, "ObjectDirectory").activeInHierarchy, Is.True);
                Assert.That(ButtonNamed(screen, "EnterReview").interactable, Is.True, "The next queued patient is now available.");
                Assert.That(runtime.GetView().Game.CurrentPatientId, Is.Not.EqualTo(patientId));
                Assert.That(runtime.EnterSelectedPatient(patientId).Accepted, Is.False);
                ButtonNamed(screen, "LeaveTerminal").onClick.Invoke();
                Assert.That(runtime.GetView().LeavingTerminal, Is.True, "The explicit directory exit still requests the room transition.");
                Assert.That(runtime.GetView().Cue.Kind, Is.EqualTo(PresentationKind.ExitTerminal));
                Assert.That(runtime.CompleteCue(runtime.GetView().Cue.Token).Accepted, Is.True);
                Assert.That(runtime.GetView().TerminalOpen, Is.False);
            }
            finally { Object.Destroy(owner); }
            yield return null;
        }

        private static GameRuntime CreateRuntime()
        {
            var rules = PrototypeRules.Create();
            return new GameRuntime(rules, new PrototypeContent(rules), useTerminalDirectory: true);
        }

        private static void OpenReview(MiniGameScreen screen, GameRuntime runtime)
        {
            screen.Bind(runtime);
            screen.UseYarnPresentation();
            Assert.That(runtime.TryInteract(InteractionTarget.Door).Accepted, Is.True);
            Assert.That(runtime.CompleteCue(runtime.GetView().Cue.Token).Accepted, Is.True);
            Assert.That(runtime.CompleteCue(runtime.GetView().Cue.Token).Accepted, Is.True);
            Assert.That(runtime.TryInteract(InteractionTarget.Computer).Accepted, Is.True);
            if (runtime.GetView().Cue != null)
                Assert.That(runtime.CompleteCue(runtime.GetView().Cue.Token).Accepted, Is.True);
            Assert.That(runtime.EnterSelectedPatient(runtime.GetView().Game.CurrentPatientId).Accepted, Is.True);
            Assert.That(runtime.GetView().TerminalPage, Is.EqualTo(TerminalPage.Review));
        }

        private static ResponseContent[] Responses() => Enumerable.Range(1, 4)
            .Select(index => new ResponseContent("choice-" + index, "选项 " + index)).ToArray();

        private static Button ButtonNamed(MiniGameScreen screen, string name) =>
            screen.GetComponentsInChildren<Button>(true).Single(button => button.name == name);

        private static Text TextNamed(MiniGameScreen screen, string name) =>
            screen.GetComponentsInChildren<Text>(true).Single(text => text.name == name);

        private static GameObject ObjectNamed(MiniGameScreen screen, string name) =>
            screen.GetComponentsInChildren<Transform>(true).Single(item => item.name == name).gameObject;
    }
}
