using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    public sealed class NormExistingRegressionPlayModeTests
    {
        [UnityTest]
        public IEnumerator CompletedBitmapSpeechShowsItsFirstLineWhenReplyCommandsAppear()
        {
            var owner = new GameObject("NORM completed speech regression");
            var screen = owner.AddComponent<MiniGameScreen>();
            try
            {
                var runtime = OpenTutorial(screen);
                const string source = "仪器亮红灯，只能说明人害怕、愤怒，或者悲伤。\n它怎么证明谁会伤人？";
                screen.PresentDialogueLine("对象", source, "norm-long-speech", "D0_01", false,
                    () => runtime.CompleteDialogueLine("norm-long-speech"));
                Assert.That(screen.CurrentLinePageCount, Is.GreaterThan(1));
                int count = screen.CurrentLinePageCount;
                for (int page = 0; page < count; page++)
                {
                    Assert.That(screen.HandleTerminalKey(KeyCode.Return), Is.True);
                    Assert.That(screen.HandleTerminalKey(KeyCode.Return), Is.True);
                }
                screen.ShowResponseButtons(new[] { new ResponseContent("answer", "检测结论就是依据。") }, _ => { });
                yield return null;
                Canvas.ForceUpdateCanvases();
                var text = screen.GetComponentsInChildren<TerminalBitmapText>(true)
                    .Single(item => item.name == "TranscriptText");
                Assert.That(runtime.IsDialogueLineComplete("norm-long-speech"), Is.True);
                Assert.That(text.text, Is.EqualTo(source), "Opening the reply commands must retain both source paragraphs.");
                Assert.That(text.rectTransform.rect.height, Is.GreaterThanOrEqualTo(text.preferredHeight));
                Assert.That(screen.GetComponentsInChildren<Button>().Any(button =>
                    button.name == "Reply-answer" && button.IsInteractable()), Is.True);
            }
            finally { Object.Destroy(owner); }
            yield return null;
        }

        [UnityTest]
        public IEnumerator WheelReadingOffsetSurvivesAnAppendedTypingRowAndCanReturnToLatest()
        {
            var owner = new GameObject("NORM growing transcript regression");
            var screen = owner.AddComponent<MiniGameScreen>();
            try
            {
                var runtime = OpenTutorial(screen);
                const string paragraph = "第一行记录应当保留。\n第二行记录可以通过滚轮查阅。";
                for (int index = 0; index < 12; index++)
                {
                    string source = "norm-history-" + index;
                    runtime.RecordDialogueLine("对象", paragraph, source, "D0_01");
                    runtime.CompleteDialogueLine(source);
                }
                yield return null;
                Canvas.ForceUpdateCanvases();
                var scroll = screen.GetComponentsInChildren<ScrollRect>(true)
                    .Single(item => item.name == "TranscriptPanel");
                Scroll(scroll, 3);
                Assert.That(scroll.content.anchoredPosition.y, Is.GreaterThan(0));
                Assert.That(scroll.verticalNormalizedPosition, Is.GreaterThan(.01f));
                float offset = scroll.content.anchoredPosition.y;
                screen.PresentDialogueLine("对象", new string('字', 80), "norm-typing-row", "D0_01", false, () => { });
                yield return new WaitForSecondsRealtime(.2f);
                Assert.That(scroll.content.anchoredPosition.y, Is.EqualTo(offset).Within(.2f),
                    "An appended speech row and typing updates must not pull the reader away from the same record.");

                Scroll(scroll, -1000);
                Assert.That(scroll.verticalNormalizedPosition, Is.EqualTo(0).Within(.001f));
                runtime.CompleteDialogueLine("norm-typing-row");
                yield return null;
                Assert.That(scroll.verticalNormalizedPosition, Is.EqualTo(0).Within(.001f),
                    "Following the latest record resumes when the reader scrolls back to the bottom.");
            }
            finally { Object.Destroy(owner); }
            yield return null;
        }

        private static void Scroll(ScrollRect scroll, float amount) => ExecuteEvents.ExecuteHierarchy(scroll.gameObject,
            new PointerEventData(EventSystem.current) { scrollDelta = Vector2.up * amount }, ExecuteEvents.scrollHandler);

        private static GameRuntime OpenTutorial(MiniGameScreen screen)
        {
            var patient = new PatientDefinition("tutorial", "无名对象", 0, 0, null, PatientPresence.TreatmentEnded);
            var meeting = new MeetingDefinition(patient.Id, Array.Empty<RoundDefinition>(), null, int.MaxValue, 3,
                new Dictionary<Disposition, string> { [Disposition.Purify] = "tutorial.purify" }, "tutorial.introduction",
                new Dictionary<Disposition, int> { [Disposition.Purify] = -1 });
            var rules = new GameRules("norm-regression-fixture", new[] { patient },
                new[] { new DayDefinition(0, 1, new[] { meeting }), new DayDefinition(1, 0, Array.Empty<MeetingDefinition>()) },
                new[] { EndingType.PriestAttack, EndingType.LockedInRoom, EndingType.TakenAway });
            var runtime = new GameRuntime(rules, new TutorialContent(), true);
            screen.Bind(runtime);
            screen.UseYarnPresentation();
            Assert.That(runtime.TryInteract(InteractionTarget.Computer).Accepted, Is.True);
            Assert.That(runtime.CompleteCue(runtime.GetView().Cue.Token).Accepted, Is.True);
            Assert.That(runtime.EnterSelectedPatient(patient.Id).Accepted, Is.True);
            return runtime;
        }

        private sealed class TutorialContent : IGameContent
        {
            public string Revision => "norm-regression-fixture";
            public DialogueContent GetDialogue(PlayerView view) => new DialogueContent(view.CurrentNodeId, "对象", "开场对白", Array.Empty<ResponseContent>());
            public string GetPresentation(PresentationKind kind, PlayerView view) => kind.ToString();
            public InspectionContent GetInspection(string patientId, int day, InspectionKind kind) => new InspectionContent(kind, "观察", "记录");
        }
    }
}
