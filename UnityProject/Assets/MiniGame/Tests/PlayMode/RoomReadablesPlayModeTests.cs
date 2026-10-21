using System;
using System.Collections;
using System.Collections.Generic;
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
    public sealed class RoomReadablesPlayModeTests
    {
        [UnityTest, Timeout(45000)]
        public IEnumerator ThreePhysicalReadablesUseLeftClickAndKeepFullTextWithoutSaveSideEffects()
        {
            yield return SceneManager.LoadSceneAsync("MainRoomCombined");
            yield return null;
            var host = Object.FindObjectOfType<MiniGameBootstrap>();
            CampaignWorldFixture.Restore(host);
            var rules = Rules();
            var runtime = new GameRuntime(rules, new ReadableContent(), true);
            FinishMorning(runtime);
            Bind(host, runtime);
            try
            {
                foreach (string id in new[] { "family-letter", "daily-newspaper", "employment-notice" })
                {
                    var prop = Stage(host, id);
                    Assert.That(prop.Document, Is.Not.Null, id + " must reference its authored document.");
                    Assert.That(prop.Surface.enabled && prop.Surface.gameObject.activeInHierarchy, Is.True,
                        id + " must have an actual visible scene surface.");
                    Assert.That(prop.Surface.sharedMaterial, Is.Not.Null);
                    Assert.That(prop.GetComponentsInChildren<Collider>().Any(collider => collider.enabled && !collider.isTrigger), Is.True,
                        id + " must be physically targetable by the existing world raycast.");
                    Assert.That(host.Player.FocusedReadable, Is.EqualTo(prop), id + " must be reachable from its authored approach.");
                    var prompt = host.Screen.GetComponentsInChildren<Text>(true).Single(text => text.name == "InteractionPrompt");
                    Assert.That(prompt.text, Does.Contain("鼠标左键"));
                    Assert.That(prompt.text, Does.Contain(prop.InteractionLabel));
                    string checkpoint = runtime.Save();
                    Assert.That(host.Player.InteractFocused(), Is.False, "The E interaction must not open a readable document.");
                    Assert.That(host.Player.InteractSecondaryFocused(), Is.True, id + " must open with the left-click interaction.");
                    if (id == "family-letter")
                        Assert.That(host.Screen.PropNarration.PageCount, Is.GreaterThan(1), "The complete family letter must remain readable across pages.");
                    yield return ReadAll(host, prop.CaptionForDay(1), id + "-black-reading");
                    Assert.That(runtime.Save(), Is.EqualTo(checkpoint), id + " must not alter quota, disposition, trust, inventory or saved progress.");
                }
            }
            finally { Bind(host, host.Runtime); }
        }

        [UnityTest, Timeout(45000)]
        public IEnumerator NewspaperUsesAuthoritativeDayAcrossSleepAndRestoringAnEarlierDay()
        {
            yield return SceneManager.LoadSceneAsync("MainRoomCombined");
            yield return null;
            var host = Object.FindObjectOfType<MiniGameBootstrap>();
            CampaignWorldFixture.Restore(host);
            var rules = Rules();
            var content = new ReadableContent();
            var runtime = new GameRuntime(rules, content, true);
            FinishMorning(runtime);
            Bind(host, runtime);
            string dayOneCheckpoint = runtime.Save();
            try
            {
                for (int day = 1; day <= 3; day++)
                {
                    Assert.That(runtime.GetView().Game.Day, Is.EqualTo(day));
                    var newspaper = Stage(host, "daily-newspaper");
                    Assert.That(host.Player.FocusedReadable, Is.EqualTo(newspaper));
                    string checkpoint = runtime.Save();
                    Assert.That(host.Player.InteractSecondaryFocused(), Is.True);
                    yield return ReadAll(host, newspaper.CaptionForDay(day), "daily-newspaper-day-" + day);
                    Assert.That(runtime.Save(), Is.EqualTo(checkpoint), "Reading the daily news must preserve the authoritative session.");
                    if (day < 3) AdvanceOneDay(runtime);
                }

                var latest = Stage(host, "daily-newspaper");
                Assert.That(host.Player.InteractSecondaryFocused(), Is.True);
                yield return new WaitForSecondsRealtime(.3f);
                string latestPage = host.Screen.CurrentRoomCaption;
                runtime = GameRuntime.Restore(dayOneCheckpoint, rules, content);
                Bind(host, runtime);
                Assert.That(host.Screen.PropNarration.IsOpen, Is.False, "Restoring must discard the open Day3 reading presentation.");
                Assert.That(runtime.GetView().Game.Day, Is.EqualTo(1));
                yield return null;
                Stage(host, "daily-newspaper");
                Assert.That(host.Player.InteractSecondaryFocused(), Is.True);
                Assert.That(host.Screen.CurrentRoomCaption, Is.Not.EqualTo(latestPage), "A restored Day1 must not reuse the cached Day3 news.");
                yield return ReadAll(host, latest.CaptionForDay(1), null);
                Assert.That(runtime.Save(), Is.EqualTo(dayOneCheckpoint));
            }
            finally { Bind(host, host.Runtime); }
        }

        [UnityTest]
        public IEnumerator DayZeroHidesUnpublishedNewsWhileStaticDocumentsStayReadable()
        {
            yield return SceneManager.LoadSceneAsync("MainRoomCombined");
            yield return null;
            var host = Object.FindObjectOfType<MiniGameBootstrap>();
            CampaignWorldFixture.Restore(host);
            Assert.That(host.Runtime.GetView().Game.Day, Is.EqualTo(0));
            string checkpoint = host.Runtime.Save();
            var newspaper = Stage(host, "daily-newspaper");
            Assert.That(host.Player.FocusedReadable, Is.Null, "Day0 must not expose an unpublished news interaction.");
            Assert.That(newspaper.IsHighlighted, Is.False);
            Assert.That(host.Player.InteractSecondaryFocused(), Is.False);
            Assert.That(host.Screen.ShowPropNarration(newspaper, host.Player.ViewCamera), Is.False,
                "The screen adapter must also reject news when the current day has no issue.");
            foreach (string id in new[] { "family-letter", "employment-notice" })
            {
                var prop = Stage(host, id);
                Assert.That(host.Player.FocusedReadable, Is.EqualTo(prop));
                Assert.That(host.Player.InteractSecondaryFocused(), Is.True);
                yield return ReadAll(host, prop.CaptionForDay(0), null);
            }
            Assert.That(host.Runtime.Save(), Is.EqualTo(checkpoint));
        }

        private static void Bind(MiniGameBootstrap host, GameRuntime runtime)
        {
            host.Screen.Bind(runtime);
            host.Screen.UseYarnPresentation();
            host.Player.Bind(runtime, host.Screen);
        }

        private static ReadableProp Stage(MiniGameBootstrap host, string id)
        {
            var prop = Object.FindObjectsOfType<ReadableProp>().Single(item => item.Id == id);
            string approach = id == "family-letter" ? "Family letter approach"
                : id == "daily-newspaper" ? "Newspaper approach" : "Employment notice approach";
            var anchor = GameObject.Find(approach);
            Assert.That(anchor, Is.Not.Null, "The scene must provide a collision-safe approach: " + approach);
            host.Player.enabled = false;
            var controller = host.Player.GetComponent<CharacterController>();
            controller.enabled = false;
            host.Player.transform.position = anchor.transform.position;
            controller.enabled = true;
            host.Player.FacePoint(prop.Surface.bounds.center);
            Physics.SyncTransforms();
            host.Player.RefreshInteractionFocus();
            return prop;
        }

        private static IEnumerator ReadAll(MiniGameBootstrap host, string expected, string capture)
        {
            var narration = host.Screen.PropNarration;
            int count = narration.PageCount;
            Assert.That(count, Is.GreaterThan(0));
            var pages = new List<string>();
            for (int page = 0; page < count; page++)
            {
                yield return new WaitForSecondsRealtime(.3f);
                Assert.That(narration.Visible, Is.True, "The reading must remain visible without covering its source object.");
                AssertReadingPresentation(narration);
                pages.Add(narration.CurrentText);
                if (page == 0 && capture != null)
                    yield return PrototypePlayModeTests.CaptureIfRequested(host, capture);
                Assert.That(host.Screen.AdvancePropNarration(), Is.True);
            }
            string normalized = string.Concat(expected.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(paragraph => paragraph.Trim()).Where(paragraph => paragraph.Length > 0));
            Assert.That(string.Concat(pages), Is.EqualTo(normalized), "Reading must preserve every authored character through the last page.");
            yield return new WaitForSecondsRealtime(.35f);
            yield return null;
            Assert.That(narration.IsOpen, Is.False);
        }

        private static void AssertReadingPresentation(PropNarrationView narration)
        {
            Canvas.ForceUpdateCanvases();
            var label = narration.GetComponentInChildren<Text>(true);
            Assert.That(label.color, Is.EqualTo(Color.white));
            Assert.That(label.fontStyle, Is.EqualTo(FontStyle.Bold));
            Assert.That(label.fontSize, Is.GreaterThanOrEqualTo(48));
            var graphics = narration.GetComponentsInChildren<Image>(true);
            Assert.That(graphics.Where(graphic => graphic.name.StartsWith("Frame ", StringComparison.Ordinal)), Is.Empty);
            var backdrop = graphics.Single(graphic => graphic.name == "Reading background");
            Assert.That(backdrop.color, Is.EqualTo(Color.black));
            Assert.That(narration.Opacity, Is.EqualTo(1).Within(.001f));
            label.rectTransform.ForceUpdateRectTransforms();
            using var generator = new TextGenerator();
            Assert.That(generator.Populate(label.text, label.GetGenerationSettings(label.rectTransform.rect.size)), Is.True);
            Assert.That(generator.characterCountVisible, Is.GreaterThanOrEqualTo(label.text.Length),
                "The actual rendered text rectangle must contain the complete current page.");
            var panel = backdrop.rectTransform;
            var corners = new Vector3[4];
            panel.GetWorldCorners(corners);
            Assert.That(narration.ViewCamera.WorldToViewportPoint(corners[2]).x, Is.EqualTo(.985f).Within(.001f));
            Assert.That(Quaternion.Angle(panel.rotation, narration.ViewCamera.transform.rotation), Is.LessThan(.1f));
        }

        private static void FinishMorning(GameRuntime runtime)
        {
            Assert.That(runtime.TryInteract(InteractionTarget.Door).Accepted, Is.True);
            Complete(runtime, PresentationKind.MorningInspection);
            Complete(runtime, PresentationKind.Briefing);
            Assert.That(runtime.GetView().Input, Is.EqualTo(InputContext.World));
        }

        private static void AdvanceOneDay(GameRuntime runtime)
        {
            int day = runtime.GetView().Game.Day;
            Assert.That(runtime.TryInteract(InteractionTarget.Computer).Accepted, Is.True);
            Complete(runtime, PresentationKind.EnterTerminal);
            Assert.That(runtime.EnterSelectedPatient(runtime.GetView().Game.CurrentPatientId).Accepted, Is.True);
            Complete(runtime, PresentationKind.Introduction);
            Assert.That(runtime.SubmitDisposition(runtime.GetView().Game.CurrentEncounterId, Disposition.Purify).Accepted, Is.True);
            Complete(runtime, PresentationKind.Reaction);
            Complete(runtime, PresentationKind.ExitTerminal);
            Complete(runtime, PresentationKind.ExitTerminal);
            Assert.That(runtime.TryInteract(InteractionTarget.Bed).Accepted, Is.True);
            Complete(runtime, PresentationKind.Sleep);
            Complete(runtime, PresentationKind.Dream);
            Assert.That(runtime.GetView().Game.Phase, Is.EqualTo(GamePhase.AfterDream),
                "The legacy fixture finishes the dream before the bed advances the calendar.");
            Assert.That(runtime.TryInteract(InteractionTarget.Bed).Accepted, Is.True);
            Assert.That(runtime.GetView().Game.Day, Is.EqualTo(day + 1));
            FinishMorning(runtime);
        }

        private static void Complete(GameRuntime runtime, PresentationKind expected)
        {
            var cue = runtime.GetView().Cue;
            Assert.That(cue, Is.Not.Null);
            Assert.That(cue.Kind, Is.EqualTo(expected));
            Assert.That(runtime.CompleteCue(cue.Token).Accepted, Is.True);
        }

        private static GameRules Rules()
        {
            var patients = Enumerable.Range(1, 3).Select(day => new PatientDefinition("news-fixture-" + day,
                "对象" + day, day, 0, null, PatientPresence.TreatmentEnded));
            var days = Enumerable.Range(1, 3).Select(day => new DayDefinition(day, 1, new[]
            {
                new MeetingDefinition("news-fixture-" + day, Array.Empty<RoundDefinition>(), null, int.MaxValue, 0,
                    new Dictionary<Disposition, string> { [Disposition.Purify] = "news-fixture-" + day + "-purify" },
                    "news-fixture-" + day + "-intro")
            }));
            return new GameRules("room-readables-day-fixture", patients, days,
                new[] { EndingType.PriestAttack, EndingType.LockedInRoom, EndingType.TakenAway });
        }

        private sealed class ReadableContent : IGameContent
        {
            public string Revision => "room-readables-day-fixture";
            public DialogueContent GetDialogue(PlayerView view) => new DialogueContent(view.CurrentNodeId,
                "对象", "会谈内容", Array.Empty<ResponseContent>());
            public string GetPresentation(PresentationKind kind, PlayerView view) => kind.ToString();
            public InspectionContent GetInspection(string patientId, int day, InspectionKind kind) =>
                new InspectionContent(kind, "观察", "记录");
        }
    }
}
