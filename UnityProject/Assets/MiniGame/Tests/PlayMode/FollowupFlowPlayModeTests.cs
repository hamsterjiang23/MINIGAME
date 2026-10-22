using System;
using System.Collections;
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
    public sealed class FollowupFlowPlayModeTests
    {
        private MiniGameBootstrap host;
        private bool previousSkipStartup;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            previousSkipStartup = MainMenuController.SkipStartup;
            MainMenuController.SkipStartup = true;
            MiniGameSessionHost.Release();
            yield return SceneManager.LoadSceneAsync(MiniGameSessionHost.CombinedScene);
            yield return null;
            host = Object.FindObjectOfType<MiniGameBootstrap>();
            host.MainMenu.TryDismissForEditorDebug();
            if (host.Runtime == null) host.StartCampaign();
            host.Player.enabled = false;
            host.Screen.DialogueCharactersPerSecond = 0;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            MainMenuController.SkipStartup = previousSkipStartup;
            if (host != null)
            {
                host.enabled = false;
                host.Player.enabled = false;
                host.Dreams?.PrepareForRuntimeChange();
                host.Effects?.PrepareForRuntimeChange();
            }
            MiniGameSessionHost.Release();
            if (MainMenuController.Current != null) Object.Destroy(MainMenuController.Current.gameObject);
            yield return null;
        }

        [UnityTest]
        public IEnumerator DailyPatientCardsFollowArrivalDaysAndStableOrder()
        {
            var runtime = Fixture();
            var expected = new[]
            {
                new[] { "former-ruler", "battlefield-doctor", "child" },
                new[] { "former-ruler", "battlefield-doctor", "priest" },
                new[] { "former-ruler", "battlefield-doctor", "diplomat" }
            };
            for (int day = 1; day <= 3; day++)
            {
                int targetDay = day;
                DriveTo(runtime, view => view.Game.Day == targetDay && view.TerminalPage == TerminalPage.Directory && view.Cue == null);
                Assert.That(runtime.GetView().Patients.Select(patient => patient.Id), Is.EqualTo(expected[day - 1]));
                host.RestoreCheckpoint(runtime.Save());
                yield return null;
                var labels = host.Screen.GetComponentsInChildren<Button>(true)
                    .Where(button => button.name.StartsWith("Roster-", StringComparison.Ordinal) && button.gameObject.activeInHierarchy)
                    .OrderBy(button => int.Parse(button.name.Substring("Roster-".Length)))
                    .Select(button => string.Join(" ", button.GetComponentsInChildren<Text>(true).Select(text => text.text))).ToArray();
                Assert.That(labels.Length, Is.EqualTo(expected[day - 1].Length));
                var names = runtime.GetView().Patients.Select(patient => patient.Name).ToArray();
                for (int index = 0; index < names.Length; index++) Assert.That(labels[index], Does.Contain(names[index]));
                for (int index = 0; index < names.Length; index++)
                    if (runtime.GetView().Patients[index].TreatmentCount > 0)
                        Assert.That(labels[index], Does.Contain("治疗" + runtime.GetView().Patients[index].TreatmentCount + "次"));
                foreach (var row in host.Screen.GetComponentsInChildren<Button>(true)
                    .Where(button => button.name.StartsWith("Roster-", StringComparison.Ordinal) && button.gameObject.activeInHierarchy))
                {
                    int index = int.Parse(row.name.Substring("Roster-".Length));
                    var patient = runtime.GetView().Patients[index];
                    Assert.That(row.transform.Find("Stage").GetComponent<Text>().text,
                        Is.EqualTo(patient.TreatmentCount == 0 ? "未处理" : "治疗" + patient.TreatmentCount + "次"));
                    Assert.That(row.transform.Find("Status").GetComponent<Text>().text, Is.EqualTo("待审"),
                        "Prior-day treatment must not mark today's review as complete.");
                    Assert.That(row.transform.Find("Unit").GetComponent<Text>().text, Is.EqualTo(PortraitCatalog.Unit(patient.Id)));
                }
            }
        }

        [UnityTest]
        public IEnumerator DayZeroShutdownRestoresOnlyTheBedSpotlight()
        {
            var runtime = Fixture();
            DriveTo(runtime, view => view.Game.Day == 0 && view.Game.Phase == GamePhase.Evening && !view.TerminalOpen && view.Cue == null);
            host.RestoreCheckpoint(runtime.Save());
            yield return null;
            var guidance = host.GetComponent<BedtimeGuidance>();
            Assert.That(guidance.Isolation, Is.EqualTo(1));
            Assert.That(guidance.Spotlight.enabled, Is.True);
            Assert.That(guidance.Spotlight.intensity, Is.GreaterThan(0));
            Assert.That(RenderSettings.ambientIntensity, Is.Zero);
            Assert.That(RenderSettings.reflectionIntensity, Is.Zero);
            Assert.That(RenderSettings.ambientLight.maxColorComponent, Is.LessThan(.001f));
            Assert.That(host.gameObject.scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<Light>(true))
                .Where(light => light != guidance.Spotlight).All(light => !light.isActiveAndEnabled || light.intensity < .001f), Is.True);
            Assert.That(runtime.GetView().AllowedInteractions, Is.EqualTo(new[] { InteractionTarget.Bed }));
        }

        [UnityTest, Timeout(30000)]
        public IEnumerator EscapeLeavesPatientSelectionButPausesPatientDialogue()
        {
            var runtime = Fixture();
            DriveTo(runtime, view => view.Game.Day == 1 && view.TerminalPage == TerminalPage.Directory && view.Cue == null);
            host.RestoreCheckpoint(runtime.Save());
            yield return null;
            host.HandleEscape();
            Assert.That(host.Runtime.GetView().Cue?.Kind, Is.EqualTo(PresentationKind.ExitTerminal));
            Assert.That(host.Runtime.GetView().Paused, Is.False);
            yield return Until(() => !host.Runtime.GetView().TerminalOpen && host.Runtime.GetView().Cue == null);
            Assert.That(host.Runtime.GetView().Input, Is.EqualTo(InputContext.World));
            Assert.That(host.Runtime.TryInteract(InteractionTarget.Computer).Accepted, Is.True);
            yield return Until(() => host.Runtime.GetView().TerminalPage == TerminalPage.Directory && host.Runtime.GetView().Cue == null);
            Assert.That(host.Runtime.EnterSelectedPatient("former-ruler").Accepted, Is.True);
            yield return Until(() => host.Runtime.GetView().Input == InputContext.Dialogue);
            host.HandleEscape();
            Assert.That(host.Runtime.GetView().Paused, Is.True);
            Assert.That(host.Runtime.GetView().TerminalOpen, Is.True);
            Assert.That(host.Runtime.GetView().TerminalPage, Is.EqualTo(TerminalPage.Review));
            host.HandleEscape();
            Assert.That(host.Runtime.GetView().Paused, Is.False);
        }

        [UnityTest]
        public IEnumerator DreamPlaysMusicOverFourPointFiveBlackSecondsBeforeFifteenSecondsOfFilm()
        {
            var runtime = Fixture();
            DriveTo(runtime, view => view.Game.Day == 1 && view.Cue?.Kind == PresentationKind.Dream);
            host.RestoreCheckpoint(runtime.Save());
            host.Dreams.UseExternalClock = true;
            yield return null;
            var fade = host.Screen.GetComponentsInChildren<Image>(true).Single(image => image.name == "PresentationFade");
            var projection = host.transform.Find("Dream wall projection").GetComponent<Renderer>();
            var music = host.Audio.Service.GetSource(MiniGameAudioChannel.DreamMusic);
            Assert.That(music.isPlaying, Is.True);
            Assert.That(music.clip, Is.SameAs(host.Audio.Profile.GetClip(host.DreamProfile.GetDay(1).Music)));
            Assert.That(fade.color.a, Is.EqualTo(1));
            Assert.That(projection.enabled, Is.False);
            host.Dreams.Advance(4.49f);
            Assert.That(host.Dreams.Phase, Is.EqualTo(DreamPlaybackPhase.Aligning));
            Assert.That(projection.enabled, Is.False);
            host.Dreams.Advance(.02f);
            Assert.That(host.Dreams.Phase, Is.EqualTo(DreamPlaybackPhase.Playing));
            Assert.That(host.Dreams.PlaybackSeconds, Is.Zero);
            Assert.That(fade.color.a, Is.Zero);
            host.Dreams.Advance(14.9f);
            Assert.That(host.Dreams.Phase, Is.EqualTo(DreamPlaybackPhase.Playing));
            host.Dreams.Advance(.11f);
            Assert.That(host.Dreams.PlaybackSeconds, Is.EqualTo(15));
            Assert.That(host.Dreams.Phase, Is.EqualTo(DreamPlaybackPhase.Returning));
        }

        [UnityTest]
        public IEnumerator DoorKnockingHasLowerMeasuredOutputAtTheFarEndOfTheRoom()
        {
            var runtime = Fixture();
            DriveTo(runtime, view => view.Game.Day == 1 && view.Game.Phase == GamePhase.MorningInspection && view.Cue == null);
            host.RestoreCheckpoint(runtime.Save());
            yield return null;
            var source = host.Audio.Service.GetSource(MiniGameAudioChannel.DoorKnock);
            var door = host.gameObject.scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<WorldInteraction>(true))
                .Single(interaction => interaction.Target == InteractionTarget.Door);
            Assert.That(Vector3.Distance(source.transform.position, door.GetComponent<Collider>().bounds.center), Is.LessThan(.01f));
            Assert.That(source.spatialBlend, Is.EqualTo(1));
            Assert.That(source.rolloffMode, Is.EqualTo(AudioRolloffMode.Linear));
            var camera = host.Player.ViewCamera;
            var original = camera.transform.position;
            host.Audio.enabled = false;
            host.Audio.Service.StopAll();
            host.Audio.Service.SetLoop(MiniGameAudioChannel.DoorKnock, MiniGameSound.DoorKnock, 0);
            float nearPeak = 0, farPeak = 0;
            var samples = new float[1024];
            try
            {
                for (int position = 0; position < 2; position++)
                {
                    camera.transform.position = source.transform.position + Vector3.right * (position == 0 ? .5f : 9.5f);
                    yield return new WaitForSecondsRealtime(.25f);
                    float deadline = Time.realtimeSinceStartup + .8f;
                    while (Time.realtimeSinceStartup < deadline)
                    {
                        AudioListener.GetOutputData(samples, 0);
                        float peak = samples.Max(value => Mathf.Abs(value));
                        if (position == 0) nearPeak = Mathf.Max(nearPeak, peak); else farPeak = Mathf.Max(farPeak, peak);
                        yield return null;
                    }
                }
                string measurementPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath,
                    "../../Artifacts/InteractionRepair20261022/spatial-audio-measurement.json"));
                System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(measurementPath));
                System.IO.File.WriteAllText(measurementPath,
                    System.FormattableString.Invariant($"{{\"nearDistance\":0.5,\"farDistance\":9.5,\"nearPeak\":{nearPeak},\"farPeak\":{farPeak}}}"));
                Assert.That(nearPeak, Is.GreaterThan(.0001f), "The near-door recording must contain actual output samples.");
                Assert.That(farPeak, Is.LessThan(nearPeak * .3f), "Distance must attenuate the audible output.");
            }
            finally { host.Audio.Service.StopAll(); camera.transform.position = original; }
        }

        private static GameRuntime Fixture()
        {
            var session = MiniGameSessionHost.Current;
            var runtime = new GameRuntime(session.Rules, session.Content, true);
            if (runtime.UsesEndingFlow) runtime.ConfigureEndings(EndingDialoguePresenter.LoadCatalog());
            return runtime;
        }

        private static IEnumerator Until(Func<bool> condition)
        {
            float deadline = Time.realtimeSinceStartup + 10;
            while (!condition() && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.That(condition(), Is.True, "Presentation did not reach the requested state.");
        }

        private static void DriveTo(GameRuntime runtime, Func<RuntimeView, bool> stop)
        {
            for (int step = 0; step < 3000; step++)
            {
                var view = runtime.GetView();
                if (stop(view)) return;
                RuntimeResult result;
                if (view.Cue != null) result = runtime.CompleteCue(view.Cue.Token);
                else if (view.TerminalPage == TerminalPage.Archive) result = runtime.LeaveTerminal();
                else if (view.TerminalPage == TerminalPage.Directory)
                    result = runtime.EnterSelectedPatient(view.Patients.First(patient => patient.CanReceiveToday).Id);
                else if (view.Input == InputContext.Dialogue)
                {
                    var actions = view.Game.AvailableDispositions;
                    // Meet the real daily quota so the roster test can reach the following morning.
                    // Vera and Edwin remain in treatment; the departed child/priest must disappear.
                    bool quotaPatient = view.Game.CurrentPatientId == "child" || view.Game.CurrentPatientId == "priest";
                    result = view.Game.EncounterStage == EncounterStage.AwaitingDisposition
                        ? runtime.SubmitDisposition(view.Game.CurrentEncounterId,
                            quotaPatient && view.Game.RemainingPurificationQuota > 0 && actions.Contains(Disposition.Purify)
                                ? Disposition.Purify : actions.Contains(Disposition.Treat) ? Disposition.Treat : actions.First())
                        : runtime.SelectResponse(view.Game.CurrentEncounterId, view.Game.CurrentNodeId, view.Game.AvailableResponseIds.First());
                }
                else if (view.Game.Phase == GamePhase.Evening || view.Game.Phase == GamePhase.AfterDream)
                    result = runtime.TryInteract(InteractionTarget.Bed);
                else if (view.Game.Phase == GamePhase.MorningInspection) result = runtime.TryInteract(InteractionTarget.Door);
                else result = runtime.TryInteract(InteractionTarget.Computer);
                Assert.That(result.Accepted, Is.True, result.Reason);
            }
            Assert.Fail("Campaign driver exceeded its bound.");
        }
    }
}
