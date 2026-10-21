using System;
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
        public IEnumerator ChoiceKeysDefaultToFirstWrapAndSupportAllFourNumberShortcuts()
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
                Assert.That(selected, Is.EqualTo(new[] { "choice-1", "choice-1" }), "Enter confirms the initial first choice; Space never chooses.");

                screen.HandleTerminalKey(KeyCode.UpArrow);
                screen.HandleTerminalKey(KeyCode.Space);
                Assert.That(selected, Has.Count.EqualTo(2), "Space cannot confirm a response.");
                screen.HandleTerminalKey(KeyCode.Return);
                Assert.That(selected.Last(), Is.EqualTo("choice-4"), "Up from no selection starts at the last choice.");

                screen.ShowResponseButtons(responses, selected.Add);
                screen.HandleTerminalKey(KeyCode.Return);
                Assert.That(selected, Has.Count.EqualTo(4), "Replacing choices selects the first item.");
                screen.HandleTerminalKey(KeyCode.DownArrow);
                screen.HandleTerminalKey(KeyCode.KeypadEnter);
                Assert.That(selected.Last(), Is.EqualTo("choice-2"), "Down from the default first item chooses the second.");

                screen.ShowResponseButtons(responses, selected.Add);
                screen.HandleTerminalKey(KeyCode.UpArrow);
                screen.HandleTerminalKey(KeyCode.DownArrow);
                screen.HandleTerminalKey(KeyCode.Return);
                Assert.That(selected.Last(), Is.EqualTo("choice-1"), "Down wraps from the last choice to the first.");

                screen.ShowResponseButtons(responses, selected.Add);
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
                Assert.That(selected, Has.Count.EqualTo(11), "Each confirmation invokes exactly one response callback.");
            }
            finally { Object.Destroy(owner); }
            yield return null;
        }

        [UnityTest]
        public IEnumerator RemovedFunctionKeysLeavePassiveTranscriptAndDialogueAvailable()
        {
            var runtime = CreateRuntime(); var owner = new GameObject("Passive transcript test");
            var screen = owner.AddComponent<MiniGameScreen>();
            try
            {
                OpenReview(screen, runtime);
                for (int i=0;i<8;i++) screen.SetDialogueLine("对象", "记录"+i+"：审查对话", "passive-"+i,"ML01");
                int advanced=0, quota=runtime.GetView().Game.RemainingPurificationQuota;
                screen.ShowResponseButtons(Responses(),_=>advanced++); yield return null;
                var recordKeys=runtime.GetView().Transcript.Select(entry=>entry.Key).ToArray();
                Assert.That(screen.HandleTerminalKey(KeyCode.F2),Is.False);
                Assert.That(screen.HandleTerminalKey(KeyCode.F3),Is.False);
                Assert.That(screen.HistoryOpen,Is.False);
                Assert.That(screen.TryCloseHistory(),Is.False);
                Assert.That(screen.GetComponentsInChildren<Transform>(true).Any(t=>t.name=="TerminalSettings"||t.name=="HistoryTab"),Is.False);
                var rows=screen.GetComponentsInChildren<Text>(false).Where(t=>t.name.StartsWith("TranscriptText")).ToArray();
                Assert.That(rows,Has.Length.EqualTo(8),"The passive scrollable transcript must retain all saved records.");
                Assert.That(rows.Select(row=>row.text),Is.EqualTo(Enumerable.Range(0,8).Select(i=>"记录"+i+"：审查对话")));
                Assert.That(runtime.GetView().Transcript.Count(entry=>entry.SourceNodeId.StartsWith("passive-",StringComparison.Ordinal)),Is.EqualTo(8));
                Assert.That(runtime.GetView().Transcript.Select(entry=>entry.Key),Is.EqualTo(recordKeys),"Removed function keys must not alter saved transcript entries.");
                var scroll=rows.Last().GetComponentInParent<ScrollRect>();
                Assert.That(scroll,Is.Not.Null);
                Assert.That(scroll.content.rect.height,Is.GreaterThan(scroll.viewport.rect.height));
                screen.HandleTerminalKey(KeyCode.Return); Assert.That(advanced,Is.EqualTo(1));
                Assert.That(runtime.GetView().Game.RemainingPurificationQuota,Is.EqualTo(quota));
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
                screen.SetDialogueLine("对象", lastSpeech, "D1_M_TREAT_T1", "ML03");
                screen.SetDialogueInteractable(true);
                Assert.That(TextNamed(screen, "RecordNode").text, Is.EqualTo("人员复核 / 措施"));
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
