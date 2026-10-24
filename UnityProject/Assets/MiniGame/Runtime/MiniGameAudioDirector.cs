using System.Collections.Generic;
using MiniGame.Core;
using MiniGame.Presentation;
using UnityEngine;

namespace MiniGame.UnityHost
{
    /// <summary>Presentation-only audio; GameRuntime remains the authority for game state.</summary>
    [DisallowMultipleComponent, RequireComponent(typeof(MiniGameAudioService))]
    public sealed class MiniGameAudioDirector : MonoBehaviour
    {
        [SerializeField] private MiniGameAudioProfile profile;
        private MiniGameAudioService service;
        private MiniGameAudioService menuService;
        private GameRuntime runtime;
        private RuntimeView previous;
        private static readonly MiniGameSound[] LoopSounds = { MiniGameSound.StartMenu, MiniGameSound.Gameplay,
            MiniGameSound.Dream0, MiniGameSound.Dream1, MiniGameSound.Dream2, MiniGameSound.Dream3, MiniGameSound.PCNoise, MiniGameSound.Projector };
        private readonly AudioClip[] mappedClips = new AudioClip[LoopSounds.Length];
        private bool menuMusic;
        private float knockRemaining;
        private bool knockPlaying;
        private bool dreamPlayback;
        private int dreamDay;
        private DreamSequenceProfile dreamProfile;
        private readonly HashSet<string> transcriptKeys = new HashSet<string>();
        private readonly HashSet<int> supervisorScanDays = new HashSet<int>();
        private readonly List<HingedDoor> doors = new List<HingedDoor>();
        public MiniGameAudioService Service => service;
        public MiniGameAudioProfile Profile => profile;

        public void Bind(GameRuntime game)
        {
            StopMenuMusic();
            UnsubscribeDoors();
            if (runtime != null) runtime.Changed -= Present;
            if (runtime != game) supervisorScanDays.Clear();
            runtime = game;
            menuMusic = false;
            dreamPlayback = false;
            service = GetComponent<MiniGameAudioService>();
            if (profile == null) profile = Resources.Load<MiniGameAudioProfile>("MiniGame/AudioDefault");
            service.Configure(profile);
            ConfigureKnockPosition();
            knockRemaining = 0;
            knockPlaying = false;
            previous = game.GetView();
            transcriptKeys.Clear();
            foreach (var entry in previous.Transcript) transcriptKeys.Add(entry.Key);
            RememberClips();
            if (!isActiveAndEnabled) return;
            SynchronizeLoops(previous);
            runtime.Changed += Present;
            foreach (var root in gameObject.scene.GetRootGameObjects())
                foreach (var door in root.GetComponentsInChildren<HingedDoor>(true))
                {
                    doors.Add(door);
                    door.MotionStarted += PlayDoorMotion;
                }
        }

        private void ConfigureKnockPosition()
        {
            var source = service.GetSource(MiniGameAudioChannel.DoorKnock);
            source.spatialBlend = 1;
            source.rolloffMode = AudioRolloffMode.Linear;
            source.minDistance = .8f;
            source.maxDistance = 10;
            foreach (var root in gameObject.scene.GetRootGameObjects())
                foreach (var interaction in root.GetComponentsInChildren<WorldInteraction>(true))
                    if (interaction.Target == InteractionTarget.Door)
                    {
                        var collider = interaction.GetComponent<Collider>();
                        source.transform.position = collider != null ? collider.bounds.center : interaction.transform.position;
                        return;
                    }
        }

        private void PlayDoorMotion(HingedDoor door, bool opening)
        {
            if (isActiveAndEnabled && runtime != null && door.IsBoundTo(runtime) && !runtime.GetView().Paused)
                service.PlayOneShot(MiniGameAudioChannel.Door, opening ? MiniGameSound.DoorOpen : MiniGameSound.DoorShut);
        }

        private void UnsubscribeDoors()
        {
            foreach (var door in doors) if (door != null) door.MotionStarted -= PlayDoorMotion;
            doors.Clear();
        }

        public void Configure(MiniGameAudioProfile value)
        {
            profile = value;
            if (runtime != null) Bind(runtime);
        }

        public void ConfigureDreams(DreamSequenceProfile value) => dreamProfile = value;

        public void SetDreamPlayback(bool active, int day, bool paused)
        {
            if (!isActiveAndEnabled || service == null || runtime == null || !runtime.UsesDreamFlow) return;
            bool changed = dreamPlayback != active || dreamDay != day;
            dreamPlayback = active;
            dreamDay = day;
            if (changed) SynchronizeLoops(runtime.GetView());
            SetDreamPaused(paused);
        }

        private void SetDreamPaused(bool paused)
        {
            if (service == null) return;
            if (paused)
            {
                service.Pause(MiniGameAudioChannel.DreamMusic);
                service.Pause(MiniGameAudioChannel.Projector);
            }
            else
            {
                service.Resume(MiniGameAudioChannel.DreamMusic);
                service.Resume(MiniGameAudioChannel.Projector);
            }
        }

        private void LateUpdate()
        {
            if (profile == null || runtime == null) return;
            UpdateKnocking(runtime.GetView(), Time.unscaledDeltaTime);
            for (int i = 0; i < LoopSounds.Length; i++)
            {
                if (mappedClips[i] == profile.GetClip(LoopSounds[i])) continue;
                RememberClips();
                if (menuMusic && menuService != null) menuService.SetLoop(MiniGameAudioChannel.MenuMusic, MiniGameSound.StartMenu, profile.FadeSeconds);
                SynchronizeLoops(runtime.GetView());
                break;
            }
        }

        private void RememberClips()
        {
            for (int i = 0; i < LoopSounds.Length; i++) mappedClips[i] = profile != null ? profile.GetClip(LoopSounds[i]) : null;
        }

        private void Start()
        {
            // A serialized service can receive Bootstrap.Bind before its first OnEnable.
            // Start runs after all scene components are enabled, so initial loops are now safe.
            if (runtime != null) SynchronizeLoops(runtime.GetView());
        }

        private void Present(RuntimeView view)
        {
            var before = previous;
            previous = view;
            SynchronizeLoops(view);
            UpdateKnocking(view, 0);
            if (view.Paused)
            {
                service.Pause(MiniGameAudioChannel.TerminalPower);
                service.Pause(MiniGameAudioChannel.Door);
                service.Pause(MiniGameAudioChannel.TerminalTyping);
                service.Pause(MiniGameAudioChannel.LogoGlitch);
                service.Pause(MiniGameAudioChannel.Scanner);
            }
            else
            {
                service.Resume(MiniGameAudioChannel.TerminalPower);
                service.Resume(MiniGameAudioChannel.Door);
                service.Resume(MiniGameAudioChannel.LogoGlitch);
                service.Resume(MiniGameAudioChannel.Scanner);
                service.Resume(MiniGameAudioChannel.TerminalTyping);
            }
            bool reviewing = view.TerminalOpen && view.TerminalPage == TerminalPage.Review;
            if (before != null && !view.Paused)
            {
                if (reviewing && view.Game.EncounterStage == EncounterStage.AwaitingDisposition
                    && (before.Game.EncounterStage != EncounterStage.AwaitingDisposition
                        || before.Game.CurrentEncounterId != view.Game.CurrentEncounterId))
                    service.PlayOneShot(MiniGameAudioChannel.Lamp, MiniGameSound.LightBeep);
            }
            bool newLine = false;
            foreach (var entry in view.Transcript)
                if (transcriptKeys.Add(entry.Key) && !entry.IsPlayer && entry.EncounterId == view.Game.CurrentEncounterId)
                    newLine = true;
            if (reviewing && newLine && !view.Paused) PlayDialoguePop();
        }

        private void UpdateKnocking(RuntimeView view, float deltaTime)
        {
            // The recording already contains all four knocks; repeat the whole clip.
            bool waiting = (view.Game.Phase == GamePhase.MorningInspection || view.EndingStage == EndingStage.AwaitingDoor)
                && !view.RoomDoorOpen && view.Cue == null;
            if (!waiting)
            {
                service.Stop(MiniGameAudioChannel.DoorKnock);
                knockRemaining = 0;
                knockPlaying = false;
                return;
            }
            if (view.Paused || menuMusic)
            {
                service.Pause(MiniGameAudioChannel.DoorKnock);
                return;
            }
            service.Resume(MiniGameAudioChannel.DoorKnock);
            if (knockPlaying)
            {
                if (service.GetSource(MiniGameAudioChannel.DoorKnock).isPlaying) return;
                knockPlaying = false;
                knockRemaining = .5f;
                return;
            }
            knockRemaining = Mathf.Max(0, knockRemaining - deltaTime);
            if (knockRemaining > 0) return;
            var clip = profile != null ? profile.GetClip(MiniGameSound.DoorKnock) : null;
            if (clip != null && service.PlayRecording(MiniGameAudioChannel.DoorKnock, MiniGameSound.DoorKnock))
                knockPlaying = true;
        }

        private void SynchronizeLoops(RuntimeView view)
        {
            if (profile == null) return;
            bool dream = view.Game.Phase == GamePhase.Dream;
            bool ending = view.Game.Phase == GamePhase.Ending;
            bool projecting = dream && (!runtime.UsesDreamFlow || dreamPlayback);
            float fade = profile.FadeSeconds;
            service.Stop(MiniGameAudioChannel.MenuMusic);
            service.SetLoop(MiniGameAudioChannel.GameplayMusic, dream || ending ? (MiniGameSound?)null : MiniGameSound.Gameplay, fade);
            MiniGameSound? music = !projecting ? null : runtime.UsesDreamFlow
                ? dreamProfile != null ? dreamProfile.GetDay(dreamDay)?.Music : DreamSound(dreamDay - 1)
                : DreamSound(view.Game.Day);
            service.SetLoop(MiniGameAudioChannel.DreamMusic, music, fade);
            service.SetLoop(MiniGameAudioChannel.ComputerNoise, view.TerminalPowered && !dream && !ending ? MiniGameSound.PCNoise : (MiniGameSound?)null, fade);
            if (projecting)
                service.SetLoopWithIntro(MiniGameAudioChannel.Projector, MiniGameSound.ProjectorOpen, MiniGameSound.Projector, fade);
            else service.Stop(MiniGameAudioChannel.Projector, fade);
            if (runtime.UsesDreamFlow) SetDreamPaused(view.Paused);
        }

        private static MiniGameSound? DreamSound(int day)
        {
            switch (day)
            {
                case 0: return MiniGameSound.Dream0;
                case 1: return MiniGameSound.Dream1;
                case 2: return MiniGameSound.Dream2;
                default: return null;
            }
        }

        // A menu uses its own sources so returning to the room retains all playheads.
        public void PlayMenuMusic()
        {
            if (!isActiveAndEnabled || service == null || profile == null) return;
            if (menuService == null)
            {
                var child = new GameObject("Menu Audio");
                child.transform.SetParent(transform, false);
                menuService = child.AddComponent<MiniGameAudioService>();
                menuService.Configure(profile);
            }
            else if (menuService.Profile != profile) menuService.Configure(profile);
            service.SetMenuSuspended(true);
            menuMusic = true;
            menuService.SetLoop(MiniGameAudioChannel.MenuMusic, MiniGameSound.StartMenu, profile.FadeSeconds);
        }

        public void StopMenuMusic()
        {
            if (!menuMusic) return;
            menuMusic = false;
            if (menuService != null) menuService.StopAll();
            if (service != null) service.SetMenuSuspended(false);
        }

        public void EndMenuMusic()
        {
            StopMenuMusic();
            if (isActiveAndEnabled && service != null && runtime != null) SynchronizeLoops(runtime.GetView());
        }

        public void PlayWorldInteraction()
        {
            if (isActiveAndEnabled && runtime != null && !runtime.GetView().Paused)
                service.PlayOneShot(MiniGameAudioChannel.World, MiniGameSound.WorldInteraction);
        }

        public void PresentSupervisorPortrait(GameRuntime game, string portraitId)
        {
            if (!isActiveAndEnabled || runtime == null || runtime != game || service == null || portraitId != "JD_04") return;
            var view = runtime.GetView();
            if (view.Paused || (view.Cue?.Kind != PresentationKind.MorningInspection && view.Cue?.Kind != PresentationKind.Briefing)
                || supervisorScanDays.Contains(view.Game.Day)) return;
            if (service.PlayOneShot(MiniGameAudioChannel.Scanner, MiniGameSound.Scan))
                supervisorScanDays.Add(view.Game.Day);
        }

        public int[] CaptureSupervisorScanDays(GameRuntime game)
        {
            if (runtime != game) return System.Array.Empty<int>();
            var days = new int[supervisorScanDays.Count];
            supervisorScanDays.CopyTo(days);
            System.Array.Sort(days);
            return days;
        }

        public void RestoreSupervisorScanDays(int[] days)
        {
            supervisorScanDays.Clear();
            if (runtime == null || days == null) return;
            int currentDay = runtime.GetView().Game.Day;
            foreach (int day in days) if (day >= 0 && day <= currentDay) supervisorScanDays.Add(day);
        }

        public void PlayUIInteraction()
        {
            if (isActiveAndEnabled)
                (menuMusic ? menuService : service)?.PlayOneShot(MiniGameAudioChannel.UI, MiniGameSound.UIInteraction);
        }

        public void PlayDialoguePop()
        {
            if (isActiveAndEnabled && runtime != null && !runtime.GetView().Paused)
                service.PlayOneShot(MiniGameAudioChannel.Dialogue, MiniGameSound.DialoguePop);
        }

        public void PlayTerminalPowerOn() => PlayTerminalPower(MiniGameSound.TerminalPowerOn);
        public void PlayLogoGlitch()
        {
            if (isActiveAndEnabled && runtime != null && !runtime.GetView().Paused)
                service?.PlayOneShot(MiniGameAudioChannel.LogoGlitch, MiniGameSound.LogoGlitch);
        }

        public void StopLogoGlitch() => service?.Stop(MiniGameAudioChannel.LogoGlitch);
        public void PlayTerminalPowerOff() => PlayTerminalPower(MiniGameSound.TerminalPowerOff);

        public void PlayTerminalTyping()
        {
            if (isActiveAndEnabled && runtime != null && !runtime.GetView().Paused)
                service?.PlayTerminalTyping();
        }

        private void PlayTerminalPower(MiniGameSound sound)
        {
            if (isActiveAndEnabled && runtime != null && !runtime.GetView().Paused)
                service?.PlayOneShot(MiniGameAudioChannel.TerminalPower, sound);
        }

        private void OnDisable()
        {
            UnsubscribeDoors();
            if (runtime != null) runtime.Changed -= Present;
            if (service != null)
            {
                service.StopAll();
                service.SetMenuSuspended(false);
            }
            if (menuService != null) menuService.StopAll();
            menuMusic = false;
        }

        private void OnEnable()
        {
            if (runtime == null) return;
            Bind(runtime);
            var projection = GetComponent<DreamSequenceDirector>();
            if (projection != null && projection.OwnsCamera)
                SetDreamPlayback(projection.MusicActive, projection.ActiveDay, runtime.GetView().Paused);
        }
    }
}
