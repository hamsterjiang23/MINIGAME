using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MiniGame.Core;
using MiniGame.Presentation;
using MiniGame.Presentation.Authoring;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace MiniGame.UnityHost.Tests
{
    public sealed class DayZeroOpeningPlayModeTests
    {
        private MiniGameBootstrap host;

        [UnitySetUp]
        public IEnumerator LoadFreshCampaign()
        {
            MiniGameSessionHost.Release();
            yield return SceneManager.LoadSceneAsync(MiniGameSessionHost.CombinedScene, LoadSceneMode.Single);
            yield return null;
            host = Object.FindObjectOfType<MiniGameBootstrap>();
            Assert.That(host, Is.Not.Null);
            host.Player.enabled = false;
            host.Screen.DialogueCharactersPerSecond = 0;
            yield return Until(() => host.Screen.DayZeroOpeningWaiting, "fresh NORM opening logo");
        }

        [UnityTearDown]
        public IEnumerator Cleanup()
        {
            if (host != null)
            {
                host.enabled = false;
                host.Dreams?.PrepareForRuntimeChange();
                host.Effects?.PrepareForRuntimeChange();
            }
            MiniGameSessionHost.Release();
            var current = SceneManager.GetActiveScene();
            var empty = SceneManager.CreateScene("Day0 opening cleanup " + Guid.NewGuid().ToString("N"));
            SceneManager.SetActiveScene(empty);
            if (current.IsValid() && current.isLoaded) yield return SceneManager.UnloadSceneAsync(current);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            yield return null;
        }

        [UnityTest, Timeout(15000)]
        public IEnumerator FreshOpeningCachesTheComputerStandingReturnPose()
        {
            var approach = host.gameObject.scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<Transform>(true)).Single(item => item.name == "Computer approach");
            Assert.That(host.Effects.TryGetRoomReturnPose(out var actor, out _, out var eyes, out _, out _), Is.True);
            Assert.That(Vector3.Distance(actor, approach.position), Is.LessThan(.001f),
                "Day0 automatic entry must cache the same computer-side standing point used by later terminal visits.");
            Assert.That(Vector3.Distance(eyes, actor + Vector3.up * 1.56f), Is.LessThan(.001f));
            yield return null;
        }

        [UnityTest, Timeout(45000)]
        public IEnumerator DayZeroShutdownReturnsBesideComputerWithStandingCollision()
        {
            int shutdownTyping = 0, shutdownPower = 0;
            bool typingSourcePlaying = false;
            host.Audio.Service.SoundPlayed += (channel, sound) =>
            {
                if (host.Screen.PowerPhase == TerminalPowerPhase.Shutdown && channel == MiniGameAudioChannel.TerminalTyping)
                {
                    shutdownTyping++;
                    var source = host.Audio.Service.GetSource(channel);
                    typingSourcePlaying |= source.isPlaying && source.enabled && !source.mute && source.volume > 0;
                    Assert.That(source.outputAudioMixerGroup, Is.EqualTo(host.Audio.Profile.UIGroup));
                }
                if (sound == MiniGameSound.TerminalPowerOff) shutdownPower++;
            };
            Assert.That(host.Screen.RequestDayZeroOpening(), Is.True);
            float deadline = Time.realtimeSinceStartup + 35;
            while (host.Runtime.GetView().TerminalOpen && Time.realtimeSinceStartup < deadline)
            {
                if (!host.Screen.TransitionLocked)
                {
                    var buttons = ActiveButtons();
                    Assert.That(buttons.Length, Is.LessThanOrEqualTo(1));
                    if (buttons.Length == 1) buttons[0].onClick.Invoke();
                }
                yield return null;
            }
            Assert.That(host.Runtime.GetView().TerminalOpen, Is.False);
            TestContext.WriteLine($"Day0 shutdown: typing={shutdownTyping}, powerOff={shutdownPower}, sourcePlaying={typingSourcePlaying}");
            Assert.That(shutdownTyping, Is.GreaterThan(0), "Day0 shutdown text must use the daily terminal typing sounds.");
            Assert.That(shutdownPower, Is.EqualTo(1));
            Assert.That(typingSourcePlaying, Is.True, "Day0 typing must reach an enabled, unmuted audio source.");
            var approach = host.gameObject.scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<Transform>(true)).Single(item => item.name == "Computer approach");
            Assert.That(Vector3.Distance(host.Player.transform.position, approach.position), Is.LessThan(.04f));
            Assert.That(Vector3.Distance(host.Player.ViewCamera.transform.position, host.Player.transform.position + Vector3.up * 1.56f),
                Is.LessThan(.04f));
            Assert.That(host.Player.GetComponent<CharacterController>().enabled, Is.True);
            Assert.That(host.Effects.IsSeated, Is.False);
            Assert.That(host.Runtime.GetView().AllowedInteractions, Is.EqualTo(new[] { InteractionTarget.Bed }));
            yield return PrototypePlayModeTests.CaptureIfRequested(host, "day0-return-fixed");
        }

        [UnityTest, Timeout(60000)]
        public IEnumerator NormLogoPullsBackIntoTheTerminalThenOneButtonTutorialAndOneSleepStartDayOne()
        {
            var display = host.Screen.TerminalDisplay;
            var supervisor = host.Screen.GetComponentsInChildren<RawImage>(true).Single(image => image.name == "Day0SupervisorFeed");
            var texture = supervisor.texture;
            Assert.That(texture, Is.SameAs(Resources.Load<Texture2D>("MiniGame/NormLogo")));
            Assert.That(texture, Is.Not.Null);
            Assert.That(supervisor.color, Is.EqualTo(Color.white), "The supplied logo keeps its original colour and alpha.");
            Assert.That(supervisor.raycastTarget, Is.False);
            Assert.That(supervisor.rectTransform.rect.width / supervisor.rectTransform.rect.height,
                Is.EqualTo((float)texture.width / texture.height).Within(.001), "The logo must retain its aspect ratio.");
            Assert.That(Vector3.Distance(supervisor.rectTransform.TransformPoint(supervisor.rectTransform.rect.center),
                ((RectTransform)supervisor.transform.parent).TransformPoint(((RectTransform)supervisor.transform.parent).rect.center)),
                Is.LessThan(.001), "The logo must be centred in the opening terminal view.");
            var viewport = (RectTransform)display.transform;
            var originalSize = viewport.sizeDelta;
            try
            {
                foreach (float aspect in new[] { 4f / 3, 16f / 9, 32f / 9, 9f / 16 })
                {
                    viewport.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 600 * aspect);
                    viewport.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 600);
                    viewport.ForceUpdateRectTransforms(); host.Screen.ReflowTerminal();
                    AssertLogoFitsViewport(supervisor);
                }
            }
            finally { viewport.sizeDelta = originalSize; viewport.ForceUpdateRectTransforms(); host.Screen.ReflowTerminal(); }
            Assert.That(supervisor.gameObject.activeInHierarchy, Is.True);
            Assert.That(host.Screen.GetComponentsInChildren<PatientSurveillanceFeed>(true).Single().gameObject.activeInHierarchy,
                Is.False, "The tutorial feed must not cover the centred opening logo.");
            Assert.That(host.Effects.EntryPhase, Is.EqualTo(TerminalEntryPhase.OpeningMonitor));
            Assert.That(display.OpeningProgress, Is.Zero);
            Assert.That(display.OutputRect.rect.width / display.OutputRect.rect.height,
                Is.EqualTo(host.Screen.Canvas.GetComponent<RectTransform>().rect.width / host.Screen.Canvas.GetComponent<RectTransform>().rect.height).Within(.001));
            Assert.That(host.Screen.TransitionLocked, Is.True);
            Assert.That(ActiveButtons(), Is.Empty);
            Assert.That(host.Runtime.GetView().Transcript, Is.Empty);
            CaptureComposite("day0-opening-monitor");

            Assert.That(host.Screen.RequestDayZeroOpening(), Is.True);
            Assert.That(host.Screen.RequestDayZeroOpening(), Is.False, "The opening click cannot become a second action.");
            Assert.That(host.Screen.HandleTerminalKey(KeyCode.Return), Is.False);
            yield return Until(() => display.OpeningProgress > .35f && display.OpeningProgress < .9f, "pullback midpoint");
            Assert.That(host.Effects.EntryPhase, Is.EqualTo(TerminalEntryPhase.OpeningPullback));
            Assert.That(host.Runtime.GetView().Transcript, Is.Empty);
            Assert.That(host.Screen.TransitionLocked, Is.True);
            Assert.That(supervisor.texture, Is.SameAs(texture));
            CaptureComposite("day0-opening-pullback");
            yield return Until(() => host.Runtime.GetView().Cue?.Kind == PresentationKind.Introduction && Find("Continue") != null, "first tutorial line");
            Assert.That(host.Effects.EntryPhase, Is.EqualTo(TerminalEntryPhase.Active));
            Assert.That(display.OpeningProgress, Is.EqualTo(1));
            Assert.That(display.OutputRect.rect.width / display.OutputRect.rect.height, Is.EqualTo(4f / 3).Within(.001));
            Assert.That(supervisor.gameObject.activeInHierarchy, Is.False, "The opening logo overlay must retire after the pullback.");
            var guide = host.Screen.GetComponentsInChildren<PatientSurveillanceFeed>(true).Single();
            yield return Until(() => guide.IsShowing, "tutorial patient surveillance feed");
            var catalog = Resources.Load<PortraitCatalog>("MiniGame/PortraitCatalog");
            var firstGuide = catalog.Find("D0_01");
            Assert.That(firstGuide, Is.Not.Null, "The new tutorial character must be registered as D0_01.");
            Assert.That(guide.PatientId, Is.EqualTo("tutorial"));
            Assert.That(guide.CurrentSprite, Is.SameAs(firstGuide));
            Assert.That(guide.GetComponent<RawImage>().texture, Is.SameAs(guide.Output));
            Assert.That(guide.RenderedFrames, Is.GreaterThan(0));
            #if UNITY_EDITOR
            string guidePath = UnityEditor.AssetDatabase.GetAssetPath(firstGuide.texture);
            Assert.That(guidePath, Does.Contain("/NamedPortraits/"), "Day0 must use the supplied character PNG, not a legacy portrait.");
            TestContext.WriteLine("Day0 guide D0_01: " + guidePath);
            #endif
            Assert.That(host.Screen.PowerPhase, Is.EqualTo(TerminalPowerPhase.Running));
            CaptureComposite("day0-opening-terminal");
            CaptureComposite("day0-guide-D0_01");

            var completed = new HashSet<string>();
            var sourcePackage = Resources.Load<DialoguePackageAsset>("MiniGame/ActiveCampaignPackage");
            var expectedLines = DialoguePackageCompiler.Compile(sourcePackage.PackageSource.text).SourceNodes
                .Where(node => node.Kind == "line" && node.Headers.TryGetValue("patient", out var patient) && patient == "tutorial")
                .Select(node => node.Id).ToArray();
            int maxDispositionRecords = 0;
            bool submitted = false, sawShutdown = false, sawClosingSession = false, sawSessionClosed = false;
            float deadline = Time.realtimeSinceStartup + 30;
            while (!BedtimeGuidance.IsSleepOnly(host.Runtime.GetView()) || host.Runtime.GetView().TerminalOpen)
            {
                Assert.That(Time.realtimeSinceStartup, Is.LessThan(deadline), "Day0 terminal did not finish.");
                var view = host.Runtime.GetView();
                foreach (var line in view.Transcript.Where(line => !line.IsPlayer && line.Completed)) completed.Add(line.SourceNodeId);
                maxDispositionRecords = Math.Max(maxDispositionRecords, view.Transcript.Count(line => line.SourceNodeId == "disposition:Purify"));
                if (view.Cue?.Kind == PresentationKind.ExitTerminal && view.LeavingTerminal) sawShutdown = true;
                var closingConsole = host.Screen.GetComponentsInChildren<Text>(true).Single(label => label.name == "BootConsole");
                if (closingConsole.gameObject.activeInHierarchy && closingConsole.text.Contains("Closing session..."))
                {
                    if (!sawClosingSession) CaptureComposite("day0-closing-session", closingConsole.text);
                    sawClosingSession = true;
                    Assert.That(host.Screen.TerminalReveal, Is.EqualTo(1));
                    Assert.That(host.Screen.TerminalDisplay.OutputRect.gameObject.activeInHierarchy, Is.True);
                }
                if (closingConsole.gameObject.activeInHierarchy && closingConsole.text.Contains("Session closed.")) sawSessionClosed = true;
                if (!host.Screen.TransitionLocked)
                {
                    var buttons = ActiveButtons();
                    Assert.That(buttons.Length, Is.LessThanOrEqualTo(1), "Day0 must expose at most one actionable terminal button.");
                    if (buttons.Length == 0) { yield return null; continue; } // Yarn may be installing the next source line.
                    if (buttons[0].name == "Disposition-Purify")
                    {
                        Assert.That(submitted, Is.False);
                        Assert.That(view.Game.AvailableDispositions, Is.EqualTo(new[] { Disposition.Purify }));
                        submitted = true;
                        var staleClick = buttons[0].onClick;
                        yield return Click(buttons[0]);
                        staleClick.Invoke();
                    }
                    else
                    {
                        Assert.That(buttons[0].name, Is.EqualTo("Continue"));
                        yield return Click(buttons[0]);
                    }
                }
                yield return null;
            }
            Assert.That(submitted && sawShutdown, Is.True);
            Assert.That(sawClosingSession && sawSessionClosed, Is.True, "Day0 must visibly play the HTML Closing session and Session closed output before leaving the terminal.");
            Assert.That(completed, Is.EquivalentTo(expectedLines), "Every tutorial introduction and reaction source line must complete before shutdown.");
            Assert.That(maxDispositionRecords, Is.EqualTo(1));
            Assert.That(host.Runtime.GetView().Game.RemainingPurificationQuota, Is.Zero);
            Assert.That(host.Runtime.GetView().AllowedInteractions, Is.EqualTo(new[] { InteractionTarget.Bed }));
            Assert.That(host.Runtime.TryInteract(InteractionTarget.Computer).Accepted, Is.False);
            Assert.That(host.Runtime.TryInteract(InteractionTarget.Door).Accepted, Is.False);
            Assert.That(host.Dreams.OwnsCamera, Is.False);
            var computerApproach = host.gameObject.scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<Transform>(true)).Single(item => item.name == "Computer approach");
            Assert.That(Vector3.Distance(host.Player.transform.position, computerApproach.position), Is.LessThan(.04f),
                "Day0 must return beside the computer, like later terminal visits, rather than to the distant opening spawn.");
            Assert.That(Vector3.Distance(host.Player.ViewCamera.transform.position, host.Player.transform.position + Vector3.up * 1.56f),
                Is.LessThan(.04f), "The exit restores the standing eye position with the player.");
            Assert.That(host.Player.GetComponent<CharacterController>().enabled, Is.True);
            var guidance = host.GetComponent<BedtimeGuidance>();
            Assert.That(guidance, Is.Not.Null);
            yield return Until(() => guidance.BeamStrength > .95f && guidance.Isolation >= .9999f, "bedtime guidance and night lighting");
            var lighting = Object.FindObjectsOfType<RoomLightingController>()
                .Single(roomController => roomController.gameObject.scene == host.gameObject.scene);
            var nightBed = lighting.CaptureSceneLight(guidance.Spotlight, RoomLightingPeriod.Night);
            Assert.That(nightBed, Is.Not.Null, "The bed light must remain addressable in the authored night configuration.");
            float nightIntensity = nightBed.intensity * (lighting.HasIndependentProfiles ? lighting.NightProfile.lightMultiplier : 1);
            bool nightBedEnabled = nightBed.dayEnabled && nightIntensity > 0;
            bool visibleAtmosphere = nightBedEnabled && nightIntensity > .001f;
            TestContext.WriteLine("Night bed guidance: configured enabled=" + nightBed.dayEnabled + ", effective intensity=" + nightIntensity);
            Assert.That(guidance.Spotlight.enabled, Is.EqualTo(nightBedEnabled), "The authored night period controls the bed light.");
            Assert.That(guidance.OwnsLighting, Is.True);
            Assert.That(guidance.Atmosphere, Is.Not.Null);
            Assert.That(guidance.Atmosphere.BeamRenderer, Is.Not.Null);
            Assert.That(guidance.Atmosphere.ParticleRenderer, Is.Not.Null);
            Assert.That(guidance.Atmosphere.ParticleCount, Is.GreaterThan(0), "Glow particles remain available when the authored bed light is enabled.");
            Assert.That(guidance.Atmosphere.BeamRenderer.enabled, Is.EqualTo(visibleAtmosphere));
            Assert.That(guidance.Atmosphere.ParticleRenderer.enabled, Is.EqualTo(visibleAtmosphere));
            Assert.That(host.Runtime.GetView().AllowedInteractions, Is.EqualTo(new[] { InteractionTarget.Bed }),
                "Configuring night lighting must retain the bed-only gameplay gate.");
            yield return PrototypePlayModeTests.CaptureIfRequested(host, "day0-bed-only");

            var bed = host.gameObject.scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<WorldInteraction>(true))
                .Single(item => item.Target == InteractionTarget.Bed);
            var approach = host.gameObject.scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<Transform>(true))
                .Single(item => item.name == "Bed approach");
            var controller = host.Player.GetComponent<CharacterController>();
            controller.enabled = false;
            host.Player.transform.position = approach.position;
            controller.enabled = true;
            host.Player.FacePoint(bed.GetComponent<Collider>().bounds.center);
            Physics.SyncTransforms();
            host.Player.RefreshInteractionFocus();
            Assert.That(host.Player.FocusedInteraction, Is.SameAs(bed));
            Assert.That(host.Player.InteractFocused(), Is.True);
            Assert.That(host.Runtime.GetView().Cue?.Kind, Is.EqualTo(PresentationKind.Sleep));
            Assert.That(host.Runtime.GetView().Game.Day, Is.Zero);
            Assert.That(host.Runtime.TryInteract(InteractionTarget.Bed).Accepted, Is.False);
            yield return Until(() => host.Runtime.GetView().Game.Day == 1 && host.Runtime.GetView().Cue == null, "Day1 after a single sleep");
            Assert.That(host.Runtime.GetView().Game.Phase, Is.EqualTo(GamePhase.MorningInspection));
            Assert.That(host.Runtime.GetView().AllowedInteractions, Is.EqualTo(new[] { InteractionTarget.Door }));
            Assert.That(host.Screen.DayZeroOpeningWaiting, Is.False);
            Assert.That(host.Runtime.TryInteract(InteractionTarget.Bed).Accepted, Is.False);
            Debug.Log("MINIGAME_DAY0_OPENING_PASSED monitor pullback; supplied tutorial guide; one active action; " + expectedLines.Length + " Yarn lines; one purification; automatic shutdown; one sleep; Day1 morning");
        }

        [UnityTest, Timeout(30000)]
        public IEnumerator PausedPullbackFreezesAndUnfinishedOpeningRestoresAtItsWaitingStart()
        {
            var runtime = host.Runtime;
            Assert.That(runtime.SetPaused(true).Accepted, Is.True);
            Assert.That(host.Screen.RequestDayZeroOpening(), Is.False);
            Assert.That(runtime.GetView().Transcript, Is.Empty);
            runtime.SetPaused(false);
            yield return null; // The resume input is owned by the pause menu for its frame.
            Assert.That(host.Screen.RequestDayZeroOpening(), Is.True);
            yield return Until(() => host.Screen.TerminalDisplay.OpeningProgress > .2f, "opening pullback");
            runtime.SetPaused(true);
            float progress = host.Screen.TerminalDisplay.OpeningProgress;
            float transition = host.Effects.TerminalTransitionProgress;
            yield return new WaitForSecondsRealtime(.2f);
            Assert.That(host.Screen.TerminalDisplay.OpeningProgress, Is.EqualTo(progress));
            Assert.That(host.Effects.TerminalTransitionProgress, Is.EqualTo(transition));
            string oldToken = runtime.GetView().Cue.Token;
            string save = runtime.Save();
            host.RestoreCheckpoint(save);
            Assert.That(host.Runtime.GetView().Paused, Is.True);
            Assert.That(host.Runtime.CompleteCue(oldToken).Accepted, Is.False);
            yield return Until(() => host.Screen.DayZeroOpeningWaiting, "restored opening waits for a fresh click");
            Assert.That(host.Screen.TerminalDisplay.OpeningProgress, Is.Zero);
            Assert.That(host.Screen.GetComponentsInChildren<RawImage>(true).Single(image => image.name == "Day0SupervisorFeed").texture,
                Is.SameAs(Resources.Load<Texture2D>("MiniGame/NormLogo")), "Restoring an unfinished opening must show the centred logo again.");
            Assert.That(host.Runtime.GetView().Transcript, Is.Empty);
            Assert.That(host.Screen.RequestDayZeroOpening(), Is.False);
            host.Runtime.SetPaused(false);
            yield return null;
            Assert.That(host.Screen.RequestDayZeroOpening(), Is.True);
            Assert.That(host.Screen.RequestDayZeroOpening(), Is.False);
            yield return Until(() => Find("Continue") != null && !host.Screen.TransitionLocked, "restored pullback finishes");
            Assert.That(host.Runtime.GetView().Transcript.Count(line => !line.IsPlayer), Is.EqualTo(1));
            Assert.That(host.Runtime.IsDialogueLineComplete("tutorial__P00_D0_R1_COMMON_001"), Is.False,
                "Neither the opening nor the repeated click may complete the first tutorial line.");
            var guide = host.Screen.GetComponentsInChildren<PatientSurveillanceFeed>(true).Single();
            yield return Until(() => guide.IsShowing, "restored tutorial feed");
            Assert.That(guide.PatientId, Is.EqualTo("tutorial"));
            Assert.That(guide.CurrentSprite, Is.SameAs(Resources.Load<PortraitCatalog>("MiniGame/PortraitCatalog").Find("D0_01")));
            host.Runtime.SetPaused(true);
            yield return null;
            float monitorTime = guide.PresentationTime;
            int monitorFrames = guide.RenderedFrames;
            yield return new WaitForSecondsRealtime(.2f);
            Assert.That(guide.PresentationTime, Is.EqualTo(monitorTime), "Pausing must freeze the new tutorial feed.");
            Assert.That(guide.RenderedFrames, Is.EqualTo(monitorFrames));
            string activeSave = host.Runtime.Save();
            host.RestoreCheckpoint(activeSave);
            yield return Until(() => guide.IsShowing && guide.CurrentSprite != null, "restored active tutorial portrait");
            Assert.That(host.Screen.DayZeroOpeningWaiting, Is.False, "An active tutorial save must not restart the opening logo.");
            Assert.That(guide.CurrentSprite, Is.SameAs(Resources.Load<PortraitCatalog>("MiniGame/PortraitCatalog").Find("D0_01")),
                "A tutorial line without an explicit portrait must restore its registered base portrait.");
        }

        private Button[] ActiveButtons() => host.Screen.GetComponentsInChildren<Button>(false)
            .Where(button => button.IsActive() && button.IsInteractable()).ToArray();
        private void AssertLogoFitsViewport(RawImage logo)
        {
            var camera = host.Screen.GetComponentsInChildren<Camera>(true).Single(item => item.name == "Terminal UI camera");
            var corners = new Vector3[4]; logo.rectTransform.GetWorldCorners(corners);
            foreach (var corner in corners)
            {
                var point = camera.WorldToViewportPoint(corner);
                Assert.That(point.x, Is.InRange(.03f, .97f), "The complete logo must fit horizontally inside the CRT.");
                Assert.That(point.y, Is.InRange(.03f, .97f), "The complete logo must fit vertically inside the CRT.");
            }
        }
        private Button Find(string name) => ActiveButtons().FirstOrDefault(button => button.name == name);

        private IEnumerator Click(Button button)
        {
            yield return null;
            Canvas.ForceUpdateCanvases();
            var pointer = new PointerEventData(EventSystem.current)
            {
                button = PointerEventData.InputButton.Left,
                clickCount = 1,
                position = host.Screen.TerminalDisplay.ScreenPointFor((RectTransform)button.transform)
            };
            var hits = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointer, hits);
            Assert.That(hits, Is.Not.Empty, button.name);
            Assert.That(hits[0].gameObject.GetComponentInParent<Button>(), Is.SameAs(button),
                "Mapped picking must target " + button.name + ": " + string.Join(",", hits.Select(hit => hit.gameObject.name)));
            ExecuteEvents.ExecuteHierarchy(hits[0].gameObject, pointer, ExecuteEvents.pointerDownHandler);
            ExecuteEvents.ExecuteHierarchy(hits[0].gameObject, pointer, ExecuteEvents.pointerUpHandler);
            ExecuteEvents.ExecuteHierarchy(hits[0].gameObject, pointer, ExecuteEvents.pointerClickHandler);
        }

        private static IEnumerator Until(Func<bool> predicate, string label)
        {
            float deadline = Time.realtimeSinceStartup + 12;
            while (!predicate() && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.That(predicate(), Is.True, label);
        }

        private void CaptureComposite(string name, string sparseConsoleText = null)
        {
            var display = host.Screen.TerminalDisplay;
            display.RenderFrame();
            float aspect = display.OutputRect.rect.width / display.OutputRect.rect.height;
            var composite = new RenderTexture(1280, Mathf.RoundToInt(1280 / aspect), 0, RenderTextureFormat.ARGB32);
            composite.Create();
            var before = RenderTexture.active;
            var image = new Texture2D(composite.width, composite.height, TextureFormat.RGBA32, false);
            try
            {
                Graphics.Blit(display.Texture, composite, display.ScreenMaterial);
                RenderTexture.active = composite;
                image.ReadPixels(new Rect(0, 0, composite.width, composite.height), 0, 0);
                image.Apply();
                string directory = Environment.GetEnvironmentVariable("MINIGAME_CAPTURE_DIR");
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                    File.WriteAllBytes(Path.Combine(directory, name + ".png"), image.EncodeToPNG());
                }
                int litPixels = image.GetPixels32().Count(pixel => pixel.r > 45 && pixel.g > 20);
                if (sparseConsoleText != null)
                {
                    // Shutdown deliberately clears the full review screen. Validate its actual
                    // console content instead of applying the full-monitor pixel-area oracle.
                    int visibleCharacters = sparseConsoleText.Count(character => !char.IsWhiteSpace(character));
                    Assert.That(visibleCharacters, Is.GreaterThan(0), "The shutdown capture needs nonblank console output.");
                    TestContext.WriteLine(name + ": lit pixels=" + litPixels + ", console character lower bound=" + visibleCharacters);
                    Assert.That(litPixels, Is.GreaterThanOrEqualTo(visibleCharacters),
                        "The shutdown composite must retain at least one above-background pixel per nonblank console character.");
                }
                else
                    Assert.That(litPixels, Is.GreaterThan(1000),
                        "The native GPU composite must contain a visible monitor, not a black frame.");
            }
            finally
            {
                RenderTexture.active = before;
                Object.Destroy(image);
                composite.Release();
                Object.Destroy(composite);
            }
        }
    }
}
