using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MiniGame.Core;
using MiniGame.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace MiniGame.UnityHost.Tests
{
    public sealed class TerminalDailyPowerPlayModeTests
    {
        [UnityTest, Timeout(45000)]
        public IEnumerator OneDailyBootKeepsPhysicalScreenLitUntilFinalAutomaticClosingWithSounds()
        {
            var rules = PrototypeRules.Create(); var content = new PrototypeContent(rules);
            var runtime = new GameRuntime(rules, content, true);
            var owner = new GameObject("Daily terminal power fixture");
            var cameraObject = new GameObject("Daily terminal camera", typeof(Camera)); cameraObject.transform.SetParent(owner.transform);
            var pose = new GameObject("Daily terminal pose").transform; pose.SetParent(owner.transform); pose.position = Vector3.forward;
            var surface = GameObject.CreatePrimitive(PrimitiveType.Quad); surface.transform.SetParent(owner.transform);
            var material = new Material(Shader.Find("Standard")); material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", new Color(.38f, .65f, .46f)); surface.GetComponent<Renderer>().sharedMaterial = material;
            var screen = owner.AddComponent<MiniGameScreen>();
            var audio = owner.AddComponent<MiniGameAudioDirector>(); audio.Bind(runtime); screen.BindAudio(audio);
            var effects = owner.AddComponent<PresentationEffects>(); effects.Configure(cameraObject.GetComponent<Camera>(), pose, null, null);
            effects.ConfigureTerminalSurface(surface.GetComponent<Renderer>());
            var played = new List<MiniGameSound>(); audio.Service.SoundPlayed += (_, sound) => played.Add(sound);
            try
            {
                screen.Bind(runtime); screen.UseYarnPresentation(); effects.Bind(runtime, screen);
                AssertEmission(surface.GetComponent<Renderer>(), false);
                runtime.TryInteract(InteractionTarget.Door); Complete(runtime); Complete(runtime);
                runtime.TryInteract(InteractionTarget.Computer);
                yield return null; Assert.That(screen.HandleTerminalKey(KeyCode.Return), Is.True);
                yield return Until(() => runtime.GetView().Cue == null, 6, "daily boot");
                Assert.That(played.Count(s => s == MiniGameSound.TerminalPowerOn), Is.EqualTo(1));
                FinishPatient(runtime);
                yield return Until(() => runtime.GetView().TerminalPage == TerminalPage.Archive, 2, "first patient signal loss");
                var quota = runtime.GetView().Game.RemainingPurificationQuota;
                runtime.LeaveTerminal();
                yield return Until(() => !runtime.GetView().TerminalOpen, 3, "UI exit");
                Assert.That(runtime.GetView().TerminalPowered, Is.True);
                AssertEmission(surface.GetComponent<Renderer>(), true);
                Assert.That(played.Count(s => s == MiniGameSound.TerminalPowerOff), Is.Zero);
                Assert.That(audio.Service.GetSource(MiniGameAudioChannel.ComputerNoise).isPlaying, Is.True);
                runtime.TryInteract(InteractionTarget.Computer);
                yield return Until(() => runtime.GetView().Cue == null, 3, "UI reentry");
                Assert.That(screen.PowerPhase, Is.EqualTo(TerminalPowerPhase.Running));
                Assert.That(runtime.GetView().Game.RemainingPurificationQuota, Is.EqualTo(quota));
                Assert.That(played.Count(s => s == MiniGameSound.TerminalPowerOn), Is.EqualTo(1));
                for (int guard = 0; guard < 10 && runtime.GetView().Game.Phase == GamePhase.Work; guard++)
                {
                    FinishPatient(runtime);
                    if (runtime.GetView().Game.Phase == GamePhase.Work)
                    {
                        yield return Until(() => runtime.GetView().TerminalPage == TerminalPage.Archive, 2, "patient signal loss");
                        runtime.ReturnToDirectory();
                    }
                }
                yield return Until(() => screen.TerminalConsoleText.Contains("Closing session..."), 3, "final closing console");
                Assert.That(runtime.GetView().TerminalShuttingDown, Is.True);
                Assert.That(played.Count(s => s == MiniGameSound.TerminalPowerOff), Is.EqualTo(1));
                Assert.That(screen.TransitionLocked, Is.True); Assert.That(screen.TerminalReveal, Is.EqualTo(1));
                runtime.SetPaused(true); string frozen = screen.TerminalConsoleText;
                yield return new WaitForSecondsRealtime(.15f);
                Assert.That(screen.TerminalConsoleText, Is.EqualTo(frozen)); runtime.SetPaused(false);
                yield return Until(() => !runtime.GetView().TerminalOpen, 8, "final system shutdown");
                Assert.That(runtime.GetView().TerminalPower, Is.EqualTo(TerminalPowerState.Off));
                AssertEmission(surface.GetComponent<Renderer>(), false);
                Assert.That(played.Count(s => s == MiniGameSound.TerminalPowerOn), Is.EqualTo(1));
                Assert.That(played.Count(s => s == MiniGameSound.TerminalPowerOff), Is.EqualTo(1));
                yield return null;
                var reticle = screen.GetComponentsInChildren<UnityEngine.UI.Text>(true).Single(text => text.name == "Reticle");
                Assert.That(runtime.GetView().Input, Is.EqualTo(InputContext.World));
                Assert.That(screen.TransitionLocked, Is.False);
                Assert.That(reticle.gameObject.activeInHierarchy, Is.True, "Night keeps an aiming reference before a bed is focused.");

                Assert.That(runtime.SetPaused(true).Accepted, Is.True);
                yield return null;
                Assert.That(reticle.gameObject.activeInHierarchy, Is.False);
                Assert.That(runtime.SetPaused(false).Accepted, Is.True);
                yield return null;
                Assert.That(reticle.gameObject.activeInHierarchy, Is.True);

                // Exercise the actual player ray and bed action after the daily shutdown.
                var player = owner.AddComponent<ApartmentPlayer>();
                player.Configure(cameraObject.GetComponent<Camera>());
                player.Bind(runtime, screen);
                player.enabled = false;
                var bed = GameObject.CreatePrimitive(PrimitiveType.Cube);
                bed.transform.SetParent(owner.transform);
                bed.transform.position = cameraObject.transform.position + cameraObject.transform.forward * 2;
                var interaction = bed.AddComponent<WorldInteraction>();
                interaction.Configure(InteractionTarget.Bed, "Sleep", bed.GetComponent<Renderer>());
                surface.GetComponent<Collider>().enabled = false;
                Physics.SyncTransforms();
                player.RefreshInteractionFocus();
                Assert.That(player.FocusedInteraction, Is.SameAs(interaction));
                Assert.That(player.InteractFocused(), Is.True, "The night bed action must accept the player's focused interaction.");
                Assert.That(runtime.GetView().Cue.Kind, Is.EqualTo(PresentationKind.Sleep));
            }
            finally { Object.Destroy(owner); Object.Destroy(material); }
            yield return null;
        }

        private static void FinishPatient(GameRuntime runtime)
        {
            Assert.That(runtime.EnterSelectedPatient(runtime.GetView().Game.CurrentPatientId).Accepted, Is.True);
            for (int guard = 0; guard < 30 && runtime.GetView().Game.EncounterStage != EncounterStage.AwaitingDisposition; guard++)
            {
                var view = runtime.GetView();
                if (view.Cue != null) Complete(runtime);
                else { Assert.That(runtime.SelectResponse(view.Game.CurrentEncounterId, view.Game.CurrentNodeId, "neutral").Accepted, Is.True); Complete(runtime); }
            }
            var current = runtime.GetView();
            var disposition = current.Game.CurrentPatientId == PrototypeRules.Child ? Disposition.Purify : Disposition.Treat;
            Assert.That(runtime.SubmitDisposition(current.Game.CurrentEncounterId, disposition).Accepted, Is.True);
            Complete(runtime);
        }

        private static void Complete(GameRuntime runtime) => Assert.That(runtime.CompleteCue(runtime.GetView().Cue.Token).Accepted, Is.True);

        private static void AssertEmission(Renderer surface, bool lit)
        {
            var properties = new MaterialPropertyBlock(); surface.GetPropertyBlock(properties, 0);
            Assert.That(properties.GetColor("_EmissionColor").maxColorComponent, lit ? Is.GreaterThan(.1f) : Is.EqualTo(0));
        }

        private static IEnumerator Until(Func<bool> condition, float seconds, string description)
        {
            float deadline = Time.realtimeSinceStartup + seconds;
            while (!condition() && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.That(condition(), Is.True, description);
        }
    }
}
