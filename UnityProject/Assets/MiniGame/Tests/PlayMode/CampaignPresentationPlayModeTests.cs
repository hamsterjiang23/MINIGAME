using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using MiniGame.Core;
using MiniGame.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace MiniGame.UnityHost.Tests
{
    public sealed class CampaignPresentationPlayModeTests
    {
        [UnityTest]
        public IEnumerator LongUnicodeDialogueWrapsAsOneLineWithoutExtraPageClicks()
        {
            var owner = new GameObject("Campaign complete dialogue test");
            var screen = owner.AddComponent<MiniGameScreen>();
            try
            {
                var runtime = OpenTutorial(screen);
                Canvas.ForceUpdateCanvases();
                screen.ReflowTerminal();
                var text = string.Concat(Enumerable.Repeat("\u8fd9\u662f\u5b8c\u6574\u7684\u4e00\u53e5\u8bdd e\u0301 \U0001F642 ", 10));
                int completed = 0;
                screen.PresentDialogueLine("Patient", text, "whole-line", "D0_01", false, () =>
                {
                    completed++;
                    runtime.CompleteDialogueLine("whole-line");
                });
                var label = TextNamed(screen, "TerminalDialogueBody");
                Assert.That(screen.CurrentLinePageCount, Is.EqualTo(1));
                Assert.That(label.text, Is.Empty);
                Assert.That(runtime.GetView().Transcript.Single().Text, Is.EqualTo(text));
                Assert.That(screen.HandleTerminalKey(KeyCode.Return), Is.True);
                Assert.That(screen.LineIsRevealed, Is.True);
                Assert.That(label.text, Is.EqualTo(text), "One reveal shows the entire line, including its former second page.");
                Assert.That(completed, Is.Zero);
                yield return null;
                Assert.That(TextNamed(screen, "TranscriptText").text, Is.EqualTo(text));
                Assert.That(TextNamed(screen, "TranscriptText").horizontalOverflow, Is.EqualTo(HorizontalWrapMode.Wrap));
                Assert.That(screen.HandleTerminalKey(KeyCode.Return), Is.True);
                Assert.That(completed, Is.EqualTo(1), "The next click completes this line rather than displaying another fragment.");
                Assert.That(runtime.IsDialogueLineComplete("whole-line"), Is.True);
                screen.HandleTerminalKey(KeyCode.Return);
                screen.HandleTerminalKey(KeyCode.Space);
                Assert.That(completed, Is.EqualTo(1));
            }
            finally { Object.Destroy(owner); }
            yield return null;
        }
        [UnityTest]
        public IEnumerator HistoryAndPauseFreezeTypingAndBlockContinueUntilTheyClose()
        {
            var owner = new GameObject("Campaign Typing Input Gate Test");
            var screen = owner.AddComponent<MiniGameScreen>();
            try
            {
                var runtime = OpenTutorial(screen);
                int completed = 0;
                screen.PresentDialogueLine("对象", new string('字', 180), "paused-line", "D0_01", false, () => completed++);
                var label = TextNamed(screen, "TerminalDialogueBody");
                ButtonNamed(screen, "HistoryTab").onClick.Invoke();
                string beforeHistory = label.text;
                Assert.That(screen.HandleTerminalKey(KeyCode.Return), Is.False);
                Assert.That(screen.HandleTerminalKey(KeyCode.Space), Is.False);
                yield return new WaitForSecondsRealtime(.2f);
                Assert.That(label.text, Is.EqualTo(beforeHistory), "Viewing the log freezes the current page's typing.");
                Assert.That(completed, Is.Zero);
                Assert.That(screen.TryCloseHistory(), Is.True);
                Assert.That(runtime.SetPaused(true).Accepted, Is.True);
                string beforePause = label.text;
                Assert.That(screen.HandleTerminalKey(KeyCode.Return), Is.False);
                yield return new WaitForSecondsRealtime(.2f);
                Assert.That(label.text, Is.EqualTo(beforePause), "Paused gameplay must not reveal more characters.");
                Assert.That(completed, Is.Zero);
                Assert.That(runtime.SetPaused(false).Accepted, Is.True);
                yield return new WaitForSecondsRealtime(.2f);
                Assert.That(label.text.Length, Is.GreaterThan(beforePause.Length), "Typing resumes when both input gates are open.");
                Assert.That(completed, Is.Zero);
                Assert.That(screen.HandleTerminalKey(KeyCode.Return), Is.True);
                Assert.That(screen.LineIsRevealed, Is.True);
            }
            finally { Object.Destroy(owner); }
            yield return null;
        }

        [UnityTest]
        public IEnumerator NarrationKeepsActorSpeechAndRealPortraitWhileEmptyPortraitIdsInheritTheirState()
        {
            var owner = new GameObject("Campaign Portrait And Conclusion Test");
            var screen = owner.AddComponent<MiniGameScreen>();
            try
            {
                var catalog = Resources.Load<PortraitCatalog>("MiniGame/PortraitCatalog");
                Assert.That(catalog, Is.Not.Null, "Campaign setup must import the portrait catalog before acceptance tests run.");
                Assert.That(catalog.Find("D0_01"), Is.Not.Null);
                Assert.That(catalog.Find("D0_02"), Is.Not.Null);
                var runtime = OpenTutorial(screen);
                screen.DialogueCharactersPerSecond = 0;
                screen.PresentDialogueLine("对象", "我记得这堵墙。", "last-actor-line", "D0_01", false, () => { });
                var actor = TextNamed(screen, "TerminalDialogueBody");
                var portrait = screen.GetComponentsInChildren<Image>(true).Single(image => image.name == "CurrentPortrait");
                Assert.That(portrait.sprite, Is.SameAs(catalog.Find("D0_01")));
                Assert.That(portrait.gameObject.activeInHierarchy, Is.True);
                screen.PresentDialogueLine("对象", "它一直都在那里。", "inherited-portrait-line", "", false, () => { });
                string lastActorSpeech = actor.text;
                Assert.That(portrait.sprite, Is.SameAs(catalog.Find("D0_01")), "An empty field retains the active actor's current state.");
                int narrationCompleted = 0;
                screen.PresentDialogueLine("结案记录", "该对象的档案已终止。", "conclusion-line", "", true, () => narrationCompleted++);
                var conclusion = TextNamed(screen, "ConclusionBody");
                Assert.That(conclusion.gameObject.activeInHierarchy, Is.True);
                Assert.That(conclusion.text, Is.EqualTo("该对象的档案已终止。"));
                Assert.That(actor.text, Is.EqualTo(lastActorSpeech));
                Assert.That(portrait.sprite, Is.SameAs(catalog.Find("D0_01")));
                Assert.That(runtime.GetView().Transcript.Last().IsNarration, Is.True);
                Assert.That(screen.HandleTerminalKey(KeyCode.Return), Is.True);
                Assert.That(narrationCompleted, Is.EqualTo(1));
                screen.PresentDialogueLine("对象", "最后的反应。", "next-actor-line", "D0_02", false, () => { });
                Assert.That(conclusion.gameObject.activeSelf, Is.False);
                Assert.That(portrait.sprite, Is.SameAs(catalog.Find("D0_02")));
                Assert.That(actor.text, Is.EqualTo("最后的反应。"));
                screen.PresentDialogueLine("对象", "这句继承刚才的状态。", "nonbase-inherited-line", "", false, () => { });
                Assert.That(portrait.sprite, Is.SameAs(catalog.Find("D0_02")));
                screen.Bind(runtime);
                Assert.That(portrait.sprite, Is.SameAs(catalog.Find("D0_02")),
                    "Rebinding a line with an empty portrait ID must retain the latest explicit portrait instead of returning to D0_01.");
                Assert.That(actor.text, Is.EqualTo("这句继承刚才的状态。"));

                foreach (var states in new[] { new[] { "ED04", "ED05" }, new[] { "AD03", "AD05" } })
                {
                    Assert.That(catalog.Find(states[0]), Is.Not.Null);
                    Assert.That(catalog.Find(states[1]), Is.Not.Null);
                    screen.PresentDialogueLine("对象", "这是对象最后的发言。", states[0] + "-actor", states[0], false, () => { });
                    string speechBeforeConclusion = actor.text;
                    Assert.That(portrait.sprite, Is.SameAs(catalog.Find(states[0])));
                    screen.PresentDialogueLine("结案记录", "档案终止。", states[1] + "-narration", states[1], true, () => { });
                    Assert.That(portrait.sprite, Is.SameAs(catalog.Find(states[1])),
                        "A conclusion with an explicit portrait must show its authored final character state.");
                    Assert.That(conclusion.text, Is.EqualTo("档案终止。"));
                    Assert.That(actor.text, Is.EqualTo(speechBeforeConclusion), "Changing art for narration must preserve the last NPC speech.");
                    screen.Bind(runtime);
                    Assert.That(portrait.sprite, Is.SameAs(catalog.Find(states[1])), "The final portrait state must also survive rebinding.");
                    Assert.That(actor.text, Is.EqualTo(speechBeforeConclusion));
                }
            }
            finally { Object.Destroy(owner); }
            yield return null;
        }

        private static GameRuntime OpenTutorial(MiniGameScreen screen)
        {
            var patient = new PatientDefinition("tutorial", "无名对象", 0, 0, null, PatientPresence.TreatmentEnded);
            var meeting = new MeetingDefinition(patient.Id, Array.Empty<RoundDefinition>(), null, int.MaxValue, 3,
                new Dictionary<Disposition, string> { [Disposition.Purify] = "tutorial.purify" }, "tutorial.introduction",
                new Dictionary<Disposition, int> { [Disposition.Purify] = -1 });
            var rules = new GameRules("campaign-ui-fixture", new[] { patient },
                new[] { new DayDefinition(0, 1, new[] { meeting }), new DayDefinition(1, 0, Array.Empty<MeetingDefinition>()) },
                new[] { EndingType.PriestAttack, EndingType.LockedInRoom, EndingType.TakenAway });
            var runtime = new GameRuntime(rules, new TutorialContent(), true);
            screen.Bind(runtime);
            screen.UseYarnPresentation();
            Assert.That(runtime.TryInteract(InteractionTarget.Computer).Accepted, Is.True);
            Assert.That(runtime.CompleteCue(runtime.GetView().Cue.Token).Accepted, Is.True);
            Assert.That(runtime.EnterSelectedPatient(patient.Id).Accepted, Is.True);
            Assert.That(runtime.GetView().Cue.Kind, Is.EqualTo(PresentationKind.Introduction));
            return runtime;
        }

        private static Text TextNamed(MiniGameScreen screen, string name)
            => screen.GetComponentsInChildren<Text>(true).Single(text => text.name == name);

        private static Button ButtonNamed(MiniGameScreen screen, string name)
            => screen.GetComponentsInChildren<Button>(true).Single(button => button.name == name);

        private sealed class TutorialContent : IGameContent
        {
            public string Revision => "campaign-ui-fixture-v2";
            public DialogueContent GetDialogue(PlayerView view) => new DialogueContent(view.CurrentNodeId, "对象", "开场对白", Array.Empty<ResponseContent>());
            public string GetPresentation(PresentationKind kind, PlayerView view) => kind.ToString();
            public InspectionContent GetInspection(string patientId, int day, InspectionKind kind) => new InspectionContent(kind, "观察", "记录");
        }
    }
}
