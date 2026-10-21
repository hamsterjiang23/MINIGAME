using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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
    public sealed class TerminalDailyWorkIndicatorPlayModeTests
    {
        [UnityTest]
        public IEnumerator ReviewRemainingFollowsDispositionRestoreAndNextDayWithoutReplacingProgress()
        {
            var rules = Rules();
            var content = new DailyWorkContent();
            var runtime = new GameRuntime(rules, content, true);
            var owner = new GameObject("Terminal daily work indicator regression");
            var screen = owner.AddComponent<MiniGameScreen>();
            try
            {
                screen.Bind(runtime);
                screen.UseYarnPresentation();
                OpenDirectory(runtime);
                Assert.That(Label(screen, "DailyWorkIndicatorRemaining").gameObject.activeInHierarchy, Is.False);
                Assert.That(Label(screen, "PurificationQuota").text, Is.EqualTo("01"));

                var patients = new[] { "treated", "released", "purified", "excess" };
                var dispositions = new[] { Disposition.Treat, Disposition.Release, Disposition.Purify, Disposition.Purify };
                for (int i = 0; i < patients.Length; i++)
                {
                    Assert.That(runtime.EnterSelectedPatient(patients[i]).Accepted, Is.True);
                    Complete(runtime);
                    AssertRemaining(screen, runtime, i < 3 ? "01" : "00");
                    if (i == 0)
                    {
                        AssertIndicatorLayout(screen);
                        yield return CaptureIfRequested(screen, "daily-work-indicator-review-01");
                    }

                    string encounter = runtime.GetView().Game.CurrentEncounterId;
                    Assert.That(runtime.SubmitDisposition(encounter, dispositions[i]).Accepted, Is.True);
                    string expected = i < 2 ? "01" : "00";
                    AssertRemaining(screen, runtime, expected);
                    Assert.That(runtime.SubmitDisposition(encounter, dispositions[i]).Accepted, Is.False);
                    AssertRemaining(screen, runtime, expected);
                    Assert.That(runtime.GetView().Game.PurificationsToday, Is.EqualTo(Math.Max(0, i - 1)));
                    Assert.That(Label(screen, "ReviewPurificationRequirement").text,
                        Is.EqualTo("净化要求：" + Math.Max(0, i - 1) + "/1"));

                    if (i == 2)
                    {
                        runtime = GameRuntime.Restore(runtime.Save(), rules, content);
                        screen.Bind(runtime);
                        AssertRemaining(screen, runtime, "00");
                        Assert.That(runtime.SubmitDisposition(encounter, Disposition.Purify).Accepted, Is.False);
                        Assert.That(runtime.GetView().Game.PurificationsToday, Is.EqualTo(1));
                        AssertRemaining(screen, runtime, "00");
                        yield return CaptureIfRequested(screen, "daily-work-indicator-purified-00");
                    }

                    Complete(runtime);
                    Complete(runtime);
                    if (i < patients.Length - 1)
                    {
                        Assert.That(runtime.ReturnToDirectory().Accepted, Is.True);
                        Assert.That(Label(screen, "PurificationQuota").text, Is.EqualTo(expected));
                    }
                    else Complete(runtime);
                }

                Assert.That(runtime.TryInteract(InteractionTarget.Bed).Accepted, Is.True);
                Assert.That(runtime.GetView().Cue.Kind, Is.EqualTo(PresentationKind.Sleep));
                Complete(runtime);
                Assert.That(runtime.GetView().Cue.Kind, Is.EqualTo(PresentationKind.Dream));
                Complete(runtime);
                Assert.That(runtime.GetView().Game.Day, Is.EqualTo(2));
                OpenDirectory(runtime);
                Assert.That(Label(screen, "PurificationQuota").text, Is.EqualTo("12"));
                Assert.That(runtime.EnterSelectedPatient("treated").Accepted, Is.True);
                Complete(runtime);
                AssertRemaining(screen, runtime, "12");
                Assert.That(Label(screen, "ReviewPurificationRequirement").text, Is.EqualTo("净化要求：0/12"));
                AssertIndicatorLayout(screen);
            }
            finally { Object.Destroy(owner); }
            yield return null;
        }

        private static void AssertRemaining(MiniGameScreen screen, GameRuntime runtime, string expected)
        {
            var remaining = Label(screen, "DailyWorkIndicatorRemaining");
            Assert.That(remaining.gameObject.activeInHierarchy, Is.True);
            Assert.That(remaining.text, Is.EqualTo(expected));
            Assert.That(remaining.text, Is.EqualTo(runtime.GetView().Game.RemainingPurificationQuota.ToString("00")));
            Assert.That(runtime.GetView().Game.RemainingPurificationQuota, Is.GreaterThanOrEqualTo(0));
            Assert.That(remaining.preferredWidth, Is.LessThanOrEqualTo(remaining.rectTransform.rect.width));
        }

        private static void AssertIndicatorLayout(MiniGameScreen screen)
        {
            var label = Label(screen, "DailyWorkIndicatorLabel");
            var remaining = Label(screen, "DailyWorkIndicatorRemaining");
            var status = Label(screen, "CommandStatus");
            var heading = Label(screen, "DispositionHeading");
            Assert.That(label.text, Is.EqualTo("当日指标 / 净化剩余"));
            Assert.That(label.gameObject.activeInHierarchy, Is.True);
            Assert.That(label.transform.parent.name, Is.EqualTo("PersonnelReview"));
            Assert.That(remaining.transform.parent, Is.EqualTo(label.transform.parent));
            Assert.That(remaining.alignment, Is.EqualTo(TextAnchor.UpperRight));
            Assert.That(remaining.fontSize, Is.GreaterThan(label.fontSize));
            Assert.That(label.preferredWidth, Is.LessThanOrEqualTo(label.rectTransform.rect.width));

            var parent = label.transform.parent;
            var labelBounds = RectTransformUtility.CalculateRelativeRectTransformBounds(parent, label.transform);
            var remainingBounds = RectTransformUtility.CalculateRelativeRectTransformBounds(parent, remaining.transform);
            var statusBounds = RectTransformUtility.CalculateRelativeRectTransformBounds(parent, status.transform);
            var headingBounds = RectTransformUtility.CalculateRelativeRectTransformBounds(parent, heading.transform);
            Assert.That(labelBounds.max.y, Is.LessThanOrEqualTo(statusBounds.min.y));
            Assert.That(remainingBounds.max.y, Is.LessThanOrEqualTo(statusBounds.min.y));
            Assert.That(labelBounds.min.y, Is.GreaterThanOrEqualTo(headingBounds.max.y));
            Assert.That(remainingBounds.min.y, Is.GreaterThanOrEqualTo(headingBounds.max.y));
            Assert.That(labelBounds.max.x, Is.LessThanOrEqualTo(remainingBounds.min.x));
            Assert.That(remainingBounds.max.x, Is.EqualTo(778).Within(.01f));
        }

        private static Text Label(MiniGameScreen screen, string name) =>
            screen.GetComponentsInChildren<Text>(true).Single(text => text.name == name);

        private static IEnumerator CaptureIfRequested(MiniGameScreen screen, string name)
        {
            string directory = Environment.GetEnvironmentVariable("MINIGAME_CAPTURE_DIR");
            if (string.IsNullOrEmpty(directory)) yield break;
            Directory.CreateDirectory(directory);

            var canvas = screen.Canvas;
            var previousMode = canvas.renderMode;
            var previousCamera = canvas.worldCamera;
            float previousDistance = canvas.planeDistance;
            var previousActive = RenderTexture.active;
            var cameraObject = new GameObject("Daily work indicator capture camera", typeof(Camera));
            var camera = cameraObject.GetComponent<Camera>();
            var target = new RenderTexture(1600, 1200, 24);
            Texture2D pixels = null;
            try
            {
                camera.enabled = false;
                camera.transform.position = new Vector3(0, 0, -100);
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = Color.black;
                camera.orthographic = true;
                camera.orthographicSize = 5;
                camera.nearClipPlane = .01f;
                camera.farClipPlane = 100;
                camera.cullingMask = 1 << canvas.gameObject.layer;
                target.Create();
                camera.targetTexture = target;
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = camera;
                canvas.planeDistance = .02f;
                for (int frame = 0; frame < 3; frame++)
                {
                    Canvas.ForceUpdateCanvases();
                    screen.ReflowTerminal();
                    yield return null;
                }
                Canvas.ForceUpdateCanvases();
                screen.ReflowTerminal();
                Canvas.ForceUpdateCanvases();
                camera.Render();
                RenderTexture.active = target;
                pixels = new Texture2D(1600, 1200, TextureFormat.RGBA32, false);
                pixels.ReadPixels(new Rect(0, 0, 1600, 1200), 0, 0);
                pixels.Apply();
                File.WriteAllBytes(Path.Combine(directory, name + ".png"), pixels.EncodeToPNG());
            }
            finally
            {
                RenderTexture.active = previousActive;
                canvas.renderMode = previousMode;
                canvas.worldCamera = previousCamera;
                canvas.planeDistance = previousDistance;
                camera.targetTexture = null;
                target.Release();
                Object.Destroy(target);
                if (pixels != null) Object.Destroy(pixels);
                Object.Destroy(cameraObject);
                screen.ReflowTerminal();
            }
        }

        private static void Complete(GameRuntime runtime) =>
            Assert.That(runtime.CompleteCue(runtime.GetView().Cue.Token).Accepted, Is.True);

        private static void OpenDirectory(GameRuntime runtime)
        {
            Assert.That(runtime.TryInteract(InteractionTarget.Door).Accepted, Is.True);
            Complete(runtime);
            Complete(runtime);
            Assert.That(runtime.TryInteract(InteractionTarget.Computer).Accepted, Is.True);
            Complete(runtime);
        }

        private static GameRules Rules()
        {
            var ids = new[] { "treated", "released", "purified", "excess" };
            var patients = ids.Select(id => new PatientDefinition(id, id, 1, 0, null, PatientPresence.TreatmentEnded));
            MeetingDefinition Meeting(string id, int day) => new MeetingDefinition(id,
                Array.Empty<RoundDefinition>(), null, int.MaxValue, 0,
                new Dictionary<Disposition, string>
                {
                    [Disposition.Treat] = day + "." + id + ".treat",
                    [Disposition.Release] = day + "." + id + ".release",
                    [Disposition.Purify] = day + "." + id + ".purify"
                }, day + "." + id + ".intro");
            return new GameRules("daily-work-indicator-fixture", patients,
                new[]
                {
                    new DayDefinition(1, 1, ids.Select(id => Meeting(id, 1))),
                    new DayDefinition(2, 12, new[] { Meeting("treated", 2) })
                }, new[] { EndingType.PriestAttack, EndingType.LockedInRoom, EndingType.TakenAway })
                .WithDreamFlow(0, 100, 100, new Dictionary<string, int>());
        }

        private sealed class DailyWorkContent : IGameContent
        {
            public string Revision => "daily-work-indicator-fixture";
            public DialogueContent GetDialogue(PlayerView view) => new DialogueContent(view.CurrentNodeId, "对象",
                view.EncounterStage == EncounterStage.Reaction ? "处置已登记。" : "会谈开始。", Array.Empty<ResponseContent>());
            public string GetPresentation(PresentationKind kind, PlayerView view) => kind.ToString();
            public InspectionContent GetInspection(string patientId, int day, InspectionKind kind) =>
                new InspectionContent(kind, "观察", "记录");
        }
    }
}
