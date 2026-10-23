using System.Collections;
using System.Linq;
using MiniGame.Core;
using MiniGame.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace MiniGame.UnityHost.Tests
{
    public sealed class TerminalDossierPlayModeTests
    {
        [UnityTest]
        public IEnumerator TutorialUsesExistingPortraitWhenTheDossierSpriteIsUnassigned()
        {
            var rules = new GameRules("tutorial-dossier", new[] {
                new PatientDefinition("tutorial", "无名男子", 1, 0, null, PatientPresence.TreatmentEnded) },
                new[] { new DayDefinition(1, 1, new[] { PrototypeRules.CreateMeeting("tutorial", 1) }) },
                new[] { EndingType.PriestAttack, EndingType.LockedInRoom, EndingType.TakenAway });
            var owner = new GameObject("Tutorial dossier regression");
            try
            {
                var screen = owner.AddComponent<MiniGameScreen>();
                screen.Bind(new GameRuntime(rules, new PrototypeContent(rules), true));
                yield return null;
                var portrait = screen.GetComponentsInChildren<Image>(true).Single(item => item.name == "DirectoryPortrait");
                var expected = Resources.Load<PortraitCatalog>("MiniGame/PortraitCatalog").Find(PortraitCatalog.BasePortrait("tutorial"));
                Assert.That(expected, Is.Not.Null);
                Assert.That(portrait.sprite, Is.SameAs(expected));
                Assert.That(portrait.enabled, Is.True);
            }
            finally { Object.Destroy(owner); }
        }

        [UnityTest]
        public IEnumerator WheelAndArrowsExposeTheWholeSummaryWithoutChangingTheEncounter()
        {
            var rules = new GameRules("dossier-scroll", new[] {
                new PatientDefinition("priest", "乔纳", 1, 0, null, PatientPresence.TreatmentEnded),
                new PatientDefinition("child", "米洛", 1, 0, null, PatientPresence.TreatmentEnded) },
                new[] { new DayDefinition(1, 1, new[] { PrototypeRules.CreateMeeting("priest", 1), PrototypeRules.CreateMeeting("child", 1) }) },
                new[] { EndingType.PriestAttack, EndingType.LockedInRoom, EndingType.TakenAway });
            var runtime = new GameRuntime(rules, new PrototypeContent(rules), true);
            var owner = new GameObject("Dossier scroll regression");
            var screen = owner.AddComponent<MiniGameScreen>();
            try
            {
                screen.Bind(runtime); screen.UseYarnPresentation();
                Assert.That(runtime.TryInteract(InteractionTarget.Door).Accepted, Is.True);
                while (runtime.GetView().Cue != null) Assert.That(runtime.CompleteCue(runtime.GetView().Cue.Token).Accepted, Is.True);
                Assert.That(runtime.TryInteract(InteractionTarget.Computer).Accepted, Is.True);
                while (runtime.GetView().Cue != null) Assert.That(runtime.CompleteCue(runtime.GetView().Cue.Token).Accepted, Is.True);
                yield return null;
                var summary = screen.GetComponentInChildren<TerminalDossierSummary>(true);
                var label = summary.GetComponentsInChildren<Text>(true).Single(text => text.name == "PreviewIntroduction");
                var down = summary.GetComponentsInChildren<Button>(true).Single(button => button.name == "SummaryScrollDown");
                var up = summary.GetComponentsInChildren<Button>(true).Single(button => button.name == "SummaryScrollUp");
                string before = runtime.Save();
                Assert.That(summary.LineCount, Is.GreaterThan(2));
                Assert.That(summary.FirstLine, Is.Zero);
                Assert.That(summary.FullText, Is.EqualTo(Resources.Load<TerminalDossierCatalog>("MiniGame/TerminalDossiers").Find("priest").summary));
                var wheel = new PointerEventData(EventSystem.current) { scrollDelta = new Vector2(0, -1) };
                ExecuteEvents.Execute(summary.gameObject, wheel, ExecuteEvents.scrollHandler);
                Assert.That(summary.FirstLine, Is.EqualTo(1));
                Assert.That(label.text, Does.EndWith("强烈向往净化。"));
                for (int i = 0; i < 5; i++) ExecuteEvents.Execute(summary.gameObject, wheel, ExecuteEvents.scrollHandler);
                Assert.That(summary.FirstLine, Is.EqualTo(summary.MaximumFirstLine));
                Assert.That(down.interactable, Is.False);
                Assert.That(up.interactable, Is.True);
                ExecuteEvents.Execute(up.gameObject, new PointerEventData(EventSystem.current) { button = PointerEventData.InputButton.Left }, ExecuteEvents.pointerClickHandler);
                Assert.That(summary.FirstLine, Is.Zero);
                Assert.That(runtime.Save(), Is.EqualTo(before));
                Assert.That(screen.HandleTerminalKey(KeyCode.DownArrow), Is.True);
                Assert.That(summary.MaximumFirstLine, Is.Zero);
                Assert.That(up.gameObject.activeSelf || down.gameObject.activeSelf, Is.False);
                Assert.That(screen.HandleTerminalKey(KeyCode.UpArrow), Is.True);
                Assert.That(summary.FirstLine, Is.Zero);
                summary.Scroll(1);
                runtime.SetPaused(true);
                summary.Scroll(-1);
                Assert.That(summary.FirstLine, Is.EqualTo(1), "Pause must block summary input.");
                runtime.SetPaused(false);
                Assert.That(summary.FirstLine, Is.EqualTo(1), "Refreshing the same patient must not jump back to the first line.");
                Assert.That(runtime.Save(), Is.EqualTo(before));
                screen.Bind(runtime); screen.UseYarnPresentation();
                Assert.That(summary.FirstLine, Is.Zero, "Rebinding a saved directory must reset the summary.");
                Assert.That(runtime.Save(), Is.EqualTo(before));
                summary.Scroll(1);
                Assert.That(runtime.LeaveTerminal().Accepted, Is.True);
                while (runtime.GetView().Cue != null) Assert.That(runtime.CompleteCue(runtime.GetView().Cue.Token).Accepted, Is.True);
                Assert.That(runtime.TryInteract(InteractionTarget.Computer).Accepted, Is.True);
                while (runtime.GetView().Cue != null) Assert.That(runtime.CompleteCue(runtime.GetView().Cue.Token).Accepted, Is.True);
                Assert.That(summary.FirstLine, Is.Zero);
            }
            finally { Object.Destroy(owner); }
            yield return null;
        }

        [UnityTest]
        public IEnumerator DisabledSummaryIgnoresWheelAndButtonInput()
        {
            var owner = new GameObject("Disabled summary", typeof(RectTransform));
            var labelObject = new GameObject("Summary", typeof(RectTransform), typeof(TerminalBitmapText));
            labelObject.transform.SetParent(owner.transform, false);
            try
            {
                var summary = owner.AddComponent<TerminalDossierSummary>();
                summary.Initialize(labelObject.GetComponent<Text>(), Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"));
                summary.SetContent("long", new string('长', 60));
                summary.SetInputEnabled(false);
                ExecuteEvents.Execute(owner, new PointerEventData(EventSystem.current) { scrollDelta = Vector2.down }, ExecuteEvents.scrollHandler);
                summary.Scroll(1);
                Assert.That(summary.FirstLine, Is.Zero);
                summary.SetInputEnabled(true); summary.Scroll(1);
                Assert.That(summary.FirstLine, Is.EqualTo(1));
                summary.SetContent("other", "短摘要");
                Assert.That(summary.FirstLine, Is.Zero);
            }
            finally { Object.Destroy(owner); }
            yield return null;
        }
    }
}
