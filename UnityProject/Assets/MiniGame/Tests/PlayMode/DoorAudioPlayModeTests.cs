using System;
using System.Collections;
using System.Collections.Generic;
using MiniGame.Core;
using MiniGame.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace MiniGame.UnityHost.Tests
{
    public sealed class DoorAudioPlayModeTests
    {
        [UnityTest]
        public IEnumerator KnockRepeatsWholeClipWithHalfSecondGapPausesAndStopsWhenDoorOpens()
        {
            var root = new GameObject("Knock fixture");
            var profile = Object.Instantiate(Resources.Load<MiniGameAudioProfile>("MiniGame/AudioDefault"));
            var clip = AudioClip.Create("Short four-knock recording", 4410, 1, 44100, false);
            try
            {
                Assert.That(profile.GetClip(MiniGameSound.DoorKnock).name, Is.EqualTo("NewDoorKnock"));
                profile.GetEntry(MiniGameSound.DoorKnock).Clip = clip;
                var director = root.AddComponent<MiniGameAudioDirector>();
                director.Configure(profile);
                var rules = PrototypeRules.Create();
                var runtime = new GameRuntime(rules, new PrototypeContent(rules));
                director.Bind(runtime);
                var starts = new List<float>();
                director.Service.SoundPlayed += (channel, sound) =>
                { if (sound == MiniGameSound.DoorKnock) starts.Add(Time.realtimeSinceStartup); };
                yield return null;
                Assert.That(starts.Count, Is.EqualTo(1));
                yield return new WaitForSecondsRealtime(.25f);
                runtime.SetPaused(true);
                yield return new WaitForSecondsRealtime(.7f);
                Assert.That(starts.Count, Is.EqualTo(1), "Pause must freeze the inter-recording wait.");
                runtime.SetPaused(false);
                yield return new WaitForSecondsRealtime(.2f);
                Assert.That(starts.Count, Is.EqualTo(1), "The remaining gap must survive resume.");
                yield return Until(() => starts.Count == 2);
                Assert.That(starts[1] - starts[0], Is.GreaterThanOrEqualTo(clip.length + .5f + .65f));
                Assert.That(runtime.ToggleRoomDoor().Accepted, Is.True);
                yield return null;
                Assert.That(director.Service.GetSource(MiniGameAudioChannel.DoorKnock).isPlaying, Is.False);
                yield return new WaitForSecondsRealtime(.7f);
                Assert.That(starts.Count, Is.EqualTo(2));
            }
            finally
            {
                Object.Destroy(root);
                Object.Destroy(profile);
                Object.Destroy(clip);
            }
        }

        [UnityTest]
        public IEnumerator ActualDoorMotionPlaysMappedClipsOnceAndRejectedOrRestoredDoorsStaySilent()
        {
            var scene = SceneManager.CreateScene("Door audio " + Guid.NewGuid().ToString("N"));
            var root = new GameObject("Door audio fixture");
            SceneManager.MoveGameObjectToScene(root, scene);
            var profile = Object.Instantiate(Resources.Load<MiniGameAudioProfile>("MiniGame/AudioDefault"));
            try
            {
                Assert.That(profile.GetClip(MiniGameSound.DoorOpen).name, Is.EqualTo("DoorOpen"));
                Assert.That(profile.GetClip(MiniGameSound.DoorShut).name, Is.EqualTo("NewDoorShut"));
                Assert.That(profile.GetClip(MiniGameSound.DoorOpen).loadState, Is.EqualTo(AudioDataLoadState.Loaded));
                var pivot = new GameObject("Door hinge");
                pivot.transform.SetParent(root.transform, false);
                var door = pivot.AddComponent<HingedDoor>();
                door.Configure(pivot.transform, 60, .3f);
                var director = root.AddComponent<MiniGameAudioDirector>();
                director.Configure(profile);
                var rules = PrototypeRules.Create();
                var content = new PrototypeContent(rules);
                var runtime = new GameRuntime(rules, content);
                Assert.That(runtime.TryInteract(InteractionTarget.Door).Accepted, Is.True);
                Assert.That(runtime.CompleteCue(runtime.GetView().Cue.Token).Accepted, Is.True);
                Assert.That(runtime.CompleteCue(runtime.GetView().Cue.Token).Accepted, Is.True);
                director.Bind(runtime);
                door.Bind(runtime);
                var played = new List<MiniGameSound>();
                director.Service.SoundPlayed += (channel, sound) =>
                { if (channel == MiniGameAudioChannel.Door) played.Add(sound); };
                yield return null;
                Assert.That(played, Is.Empty, "Initial binding must not make the door sound.");

                Assert.That(door.TryInteract().Accepted, Is.True);
                yield return null;
                Assert.That(door.IsMoving, Is.True);
                Assert.That(played, Is.EqualTo(new[] { MiniGameSound.DoorOpen }));
                Assert.That(director.Service.GetSource(MiniGameAudioChannel.Door).isPlaying, Is.True);
                Assert.That(director.Service.GetSource(MiniGameAudioChannel.Door).outputAudioMixerGroup, Is.SameAs(profile.SfxGroup));
                runtime.SetPaused(true);
                float angle = door.CurrentAngle;
                yield return null; yield return null;
                Assert.That(door.CurrentAngle, Is.EqualTo(angle));
                Assert.That(played.Count, Is.EqualTo(1));
                runtime.SetPaused(false);
                yield return Until(() => !door.IsMoving && runtime.GetView().Cue == null);
                Assert.That(played.Count, Is.EqualTo(1), "Resume must not replay opening.");
                Assert.That(door.TryInteract().Accepted, Is.True);
                yield return null;
                Assert.That(played, Is.EqualTo(new[] { MiniGameSound.DoorOpen, MiniGameSound.DoorShut }));
                yield return Until(() => !door.IsMoving && runtime.GetView().Cue == null);

                runtime.ConfigureDoorLockAfterSupervisor(true);
                Assert.That(door.TryInteract().Accepted, Is.False);
                yield return null;
                Assert.That(played.Count, Is.EqualTo(2), "A locked-door attempt must not play an opening clip.");

                runtime.ConfigureDoorLockAfterSupervisor(false);
                Assert.That(runtime.EnterDoorView().Accepted, Is.True);
                var restored = GameRuntime.Restore(runtime.Save(), rules, content);
                door.Bind(restored); director.Bind(restored);
                yield return null; yield return null;
                Assert.That(played.Count, Is.EqualTo(2), "Restoring an open or pending-opening door must not replay its sound.");
            }
            finally
            {
                Object.Destroy(root);
                Object.Destroy(profile);
            }
            yield return SceneManager.UnloadSceneAsync(scene);
        }

        private static IEnumerator Until(Func<bool> ready)
        {
            float end = Time.realtimeSinceStartup + 3;
            while (!ready() && Time.realtimeSinceStartup < end) yield return null;
            Assert.That(ready(), Is.True, "Door movement did not finish.");
        }
    }
}
