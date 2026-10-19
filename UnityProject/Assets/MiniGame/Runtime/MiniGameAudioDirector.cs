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
        private GameRuntime runtime;
        private RuntimeView previous;
        private static readonly MiniGameSound[] LoopSounds = { MiniGameSound.StartMenu, MiniGameSound.Gameplay,
            MiniGameSound.Dream0, MiniGameSound.Dream1, MiniGameSound.Dream2, MiniGameSound.Dream3, MiniGameSound.PCNoise, MiniGameSound.Projector };
        private readonly AudioClip[] mappedClips = new AudioClip[LoopSounds.Length];
        private bool menuMusic;
        private bool dreamPlayback;
        private int dreamDay;
        private DreamSequenceProfile dreamProfile;
        private readonly HashSet<string> transcriptKeys = new HashSet<string>();
        public MiniGameAudioService Service => service;
        public MiniGameAudioProfile Profile => profile;

        public void Bind(GameRuntime game)
        {
            if (runtime != null) runtime.Changed -= Present;
            runtime = game;
            menuMusic = false;
            dreamPlayback = false;
            service = GetComponent<MiniGameAudioService>();
            if (profile == null) profile = Resources.Load<MiniGameAudioProfile>("MiniGame/AudioDefault");
            service.Configure(profile);
            previous = game.GetView();
            transcriptKeys.Clear();
            foreach (var entry in previous.Transcript) transcriptKeys.Add(entry.Key);
            RememberClips();
            if (!isActiveAndEnabled) return;
            SynchronizeLoops(previous);
            runtime.Changed += Present;
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
            for (int i = 0; i < LoopSounds.Length; i++)
            {
                if (mappedClips[i] == profile.GetClip(LoopSounds[i])) continue;
                RememberClips();
                if (menuMusic) service.SetLoop(MiniGameAudioChannel.MenuMusic, MiniGameSound.StartMenu, profile.FadeSeconds);
                else SynchronizeLoops(runtime.GetView());
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
            menuMusic = false;
            SynchronizeLoops(view);
            bool reviewing = view.TerminalOpen && view.TerminalPage == TerminalPage.Review;
            if (before != null && !view.Paused)
            {
                bool enteredReview = reviewing && (!before.TerminalOpen || before.TerminalPage != TerminalPage.Review
                    || before.Game.CurrentEncounterId != view.Game.CurrentEncounterId);
                bool openedInspection = view.Inspection != null && !ReferenceEquals(before.Inspection, view.Inspection);
                if (enteredReview || openedInspection)
                    service.PlayOneShot(MiniGameAudioChannel.Scanner, MiniGameSound.Scan);
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

        private void SynchronizeLoops(RuntimeView view)
        {
            if (profile == null) return;
            bool dream = view.Game.Phase == GamePhase.Dream;
            bool ending = view.Game.Phase == GamePhase.Ending;
            bool projecting = dream && (!runtime.UsesDreamFlow || dreamPlayback);
            float fade = profile.FadeSeconds;
            service.SetLoop(MiniGameAudioChannel.MenuMusic, null, fade);
            service.SetLoop(MiniGameAudioChannel.GameplayMusic, dream || ending ? (MiniGameSound?)null : MiniGameSound.Gameplay, fade);
            MiniGameSound? music = !projecting ? null : runtime.UsesDreamFlow
                ? dreamProfile != null ? dreamProfile.GetDay(dreamDay)?.Music : DreamSound(dreamDay - 1)
                : DreamSound(view.Game.Day);
            service.SetLoop(MiniGameAudioChannel.DreamMusic, music, fade);
            service.SetLoop(MiniGameAudioChannel.ComputerNoise, view.TerminalOpen && !dream && !ending ? MiniGameSound.PCNoise : (MiniGameSound?)null, fade);
            service.SetLoop(MiniGameAudioChannel.Projector, projecting ? MiniGameSound.Projector : (MiniGameSound?)null, fade);
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

        // For a future menu host; Combined currently starts directly in the room.
        public void PlayMenuMusic()
        {
            if (!isActiveAndEnabled || service == null || profile == null) return;
            service.StopAll();
            menuMusic = true;
            service.SetLoop(MiniGameAudioChannel.MenuMusic, MiniGameSound.StartMenu, profile.FadeSeconds);
        }

        public void PlayWorldInteraction()
        {
            if (isActiveAndEnabled && runtime != null && !runtime.GetView().Paused)
                service.PlayOneShot(MiniGameAudioChannel.World, MiniGameSound.WorldInteraction);
        }

        public void PlayUIInteraction()
        {
            if (isActiveAndEnabled) service?.PlayOneShot(MiniGameAudioChannel.UI, MiniGameSound.UIInteraction);
        }

        public void PlayDialoguePop()
        {
            if (isActiveAndEnabled && runtime != null && !runtime.GetView().Paused)
                service.PlayOneShot(MiniGameAudioChannel.Dialogue, MiniGameSound.DialoguePop);
        }

        private void OnDisable()
        {
            if (runtime != null) runtime.Changed -= Present;
            if (service != null) service.StopAll();
        }

        private void OnEnable()
        {
            if (runtime == null) return;
            Bind(runtime);
            var projection = GetComponent<DreamSequenceDirector>();
            if (projection != null && projection.OwnsCamera)
                SetDreamPlayback(projection.IsPlaying, projection.ActiveDay, runtime.GetView().Paused);
        }
    }
}
