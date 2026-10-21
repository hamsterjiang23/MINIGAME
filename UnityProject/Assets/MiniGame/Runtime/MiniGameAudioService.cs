using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace MiniGame.UnityHost
{
    public enum MiniGameAudioChannel
    {
        MenuMusic = 0, GameplayMusic = 1, DreamMusic = 2, ComputerNoise = 3, Projector = 4,
        World = 5, UI = 6, Dialogue = 7, Scanner = 8, Lamp = 9, EndingMusic = 10, Ocean = 11, Voice = 12, TerminalPower = 13,
        TerminalTyping = 14, Door = 15
    }

    /// <summary>Scene-owned named sources, sound-bank lookup and interruptible fades.</summary>
    [DisallowMultipleComponent]
    public sealed class MiniGameAudioService : MonoBehaviour
    {
        private sealed class Channel
        {
            public AudioSource Source;
            public MiniGameSound? Sound;
            public bool LoopRequested;
            public bool Paused;
            public float Gain, StartGain, TargetGain, Elapsed, Duration;
        }

        private readonly Dictionary<MiniGameAudioChannel, Channel> channels = new Dictionary<MiniGameAudioChannel, Channel>();
        private readonly Dictionary<string, float> mixerValues = new Dictionary<string, float>();
        private static readonly MiniGameSound[] TypingSounds = { MiniGameSound.TerminalType1, MiniGameSound.TerminalType2,
            MiniGameSound.TerminalType3, MiniGameSound.TerminalType4 };
        private readonly MiniGameSound[] typingPool = new MiniGameSound[TypingSounds.Length];
        private readonly System.Random typingRandom = new System.Random();
        private MiniGameAudioProfile profile;
        private AudioMixer appliedMixer;
        public MiniGameAudioProfile Profile => profile;
        public event Action<MiniGameAudioChannel, MiniGameSound> SoundPlayed;

        public void Configure(MiniGameAudioProfile value)
        {
            StopAll();
            profile = value;
            mixerValues.Clear();
            foreach (MiniGameAudioChannel id in Enum.GetValues(typeof(MiniGameAudioChannel)))
            {
                if (channels.ContainsKey(id)) continue;
                var child = new GameObject("Audio-" + id);
                child.transform.SetParent(transform, false);
                var source = child.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.spatialBlend = 0;
                source.dopplerLevel = 0;
                channels.Add(id, new Channel { Source = source });
            }
        }

        public AudioSource GetSource(MiniGameAudioChannel channel)
            => channels.TryGetValue(channel, out var state) ? state.Source : null;

        public void SetLoop(MiniGameAudioChannel channel, MiniGameSound? sound, float fadeSeconds)
        {
            if (!isActiveAndEnabled) return;
            if (!channels.TryGetValue(channel, out var state)) return;
            var entry = sound.HasValue && profile != null ? profile.GetEntry(sound.Value) : null;
            if (entry == null || entry.Clip == null) { Stop(channel, fadeSeconds); return; }
            if (state.LoopRequested && state.Sound == sound && state.Source.clip == entry.Clip) return;

            bool sameClip = state.Source.clip == entry.Clip && state.Sound == sound;
            state.Sound = sound;
            state.LoopRequested = true;
            if (!sameClip || (!state.Source.isPlaying && !state.Paused))
            {
                state.Source.Stop();
                state.Paused = false;
                state.Source.clip = entry.Clip;
                state.Source.loop = true;
                state.Source.pitch = 1;
                state.Gain = 0;
                Route(channel, state.Source);
                state.Source.volume = 0;
                state.Source.Play();
                SoundPlayed?.Invoke(channel, sound.Value);
            }
            Fade(state, 1, fadeSeconds);
        }

        public bool PlayOneShot(MiniGameAudioChannel channel, MiniGameSound sound)
        {
            if (!isActiveAndEnabled || profile == null || !channels.TryGetValue(channel, out var state)) return false;
            var entry = profile.GetEntry(sound);
            if (entry == null || entry.Clip == null || state.LoopRequested) return false;
            Route(channel, state.Source);
            state.Source.pitch = 1;
            state.Source.loop = false;
            state.Source.volume = 1;
            state.Source.PlayOneShot(entry.Clip, Unit(entry.Volume));
            SoundPlayed?.Invoke(channel, sound);
            return true;
        }

        public bool PlayTerminalTyping()
        {
            if (!isActiveAndEnabled || profile == null) return false;
            int count = 0;
            foreach (var sound in TypingSounds)
                if (profile.GetClip(sound) != null) typingPool[count++] = sound;
            return count > 0 && PlayOneShot(MiniGameAudioChannel.TerminalTyping, typingPool[typingRandom.Next(count)]);
        }

        public void Stop(MiniGameAudioChannel channel, float fadeSeconds = 0)
        {
            if (!channels.TryGetValue(channel, out var state)) return;
            if (!state.Sound.HasValue) { state.Source.Stop(); state.Paused = false; return; }
            if (!state.LoopRequested && state.TargetGain == 0 && fadeSeconds > 0) return;
            state.LoopRequested = false;
            Fade(state, 0, fadeSeconds);
        }

        public void Pause(MiniGameAudioChannel channel)
        {
            if (!channels.TryGetValue(channel, out var state)) return;
            state.Paused = true;
            state.Source.Pause();
        }

        public void PlayVoice(AudioClip clip, float startSeconds = 0, float volume = 1)
        {
            if (!isActiveAndEnabled || !channels.TryGetValue(MiniGameAudioChannel.Voice, out var state)) return;
            Stop(MiniGameAudioChannel.Voice);
            if (clip == null || startSeconds >= clip.length) return;
            Route(MiniGameAudioChannel.Voice, state.Source);
            state.Source.clip = clip;
            state.Source.loop = false;
            state.Source.volume = Unit(volume);
            state.Source.time = Mathf.Clamp(startSeconds, 0, Mathf.Max(0, clip.length - .01f));
            state.Source.Play();
        }

        public void Resume(MiniGameAudioChannel channel)
        {
            if (!channels.TryGetValue(channel, out var state) || !state.Paused) return;
            state.Paused = false;
            state.Source.UnPause();
        }

        public void StopAll()
        {
            foreach (var state in channels.Values)
            {
                state.Source.Stop();
                state.Source.clip = null;
                state.Source.loop = false;
                state.Sound = null;
                state.LoopRequested = state.Paused = false;
                state.Gain = state.StartGain = state.TargetGain = state.Duration = 0;
            }
        }

        private void Update()
        {
            if (profile == null) return;
            // SetFloat is deliberately deferred until Update, after Awake/OnEnable.
            if (appliedMixer != profile.Mixer) { appliedMixer = profile.Mixer; mixerValues.Clear(); }
            ApplyVolume("MasterVolume", profile.MasterVolume);
            ApplyVolume("MusicVolume", profile.MusicVolume);
            ApplyVolume("AmbienceVolume", profile.AmbienceVolume);
            ApplyVolume("SfxVolume", profile.SfxVolume);
            ApplyVolume("UIVolume", profile.UIVolume);
            foreach (var pair in channels)
            {
                var state = pair.Value;
                Route(pair.Key, state.Source);
                if (!state.Sound.HasValue) continue;
                if (state.LoopRequested)
                {
                    var clip = profile.GetClip(state.Sound.Value);
                    if (clip != state.Source.clip)
                    {
                        SetLoop(pair.Key, state.Sound, profile.FadeSeconds);
                        continue;
                    }
                }
                if (!state.Paused && state.Duration > 0)
                {
                    state.Elapsed += Time.unscaledDeltaTime;
                    float t = Mathf.Clamp01(state.Elapsed / state.Duration);
                    float eased = state.TargetGain > state.StartGain ? t * t : 1 - (1 - t) * (1 - t);
                    state.Gain = Mathf.Lerp(state.StartGain, state.TargetGain, eased);
                    if (t >= 1) { state.Duration = 0; FinishFade(state); }
                }
                if (state.Sound.HasValue)
                    state.Source.volume = state.Gain * Unit(profile.GetEntry(state.Sound.Value)?.Volume ?? 0);
            }
        }

        private static void Fade(Channel state, float target, float seconds)
        {
            state.StartGain = state.Gain;
            state.TargetGain = target;
            state.Elapsed = 0;
            state.Duration = float.IsNaN(seconds) || float.IsInfinity(seconds) ? 0 : Mathf.Max(0, seconds);
            if (state.Duration == 0) { state.Gain = target; FinishFade(state); }
        }

        private static void FinishFade(Channel state)
        {
            if (state.TargetGain != 0 || state.LoopRequested) return;
            state.Source.Stop();
            state.Source.clip = null;
            state.Sound = null;
            state.Paused = false;
        }

        private void Route(MiniGameAudioChannel id, AudioSource source)
        {
            if (profile == null) return;
            source.outputAudioMixerGroup = id == MiniGameAudioChannel.MenuMusic || id == MiniGameAudioChannel.GameplayMusic
                || id == MiniGameAudioChannel.DreamMusic || id == MiniGameAudioChannel.EndingMusic ? profile.MusicGroup
                : id == MiniGameAudioChannel.ComputerNoise || id == MiniGameAudioChannel.Projector || id == MiniGameAudioChannel.Ocean ? profile.AmbienceGroup
                : id == MiniGameAudioChannel.UI || id == MiniGameAudioChannel.Dialogue || id == MiniGameAudioChannel.Voice
                    || id == MiniGameAudioChannel.TerminalTyping ? profile.UIGroup : profile.SfxGroup;
        }

        private void ApplyVolume(string parameter, float value)
        {
            if (profile.Mixer == null) return;
            float level = Unit(value);
            if (mixerValues.TryGetValue(parameter, out var previous) && previous == level) return;
            float db = level <= .0001f ? -80 : Mathf.Log10(level) * 20;
            if (profile.Mixer.SetFloat(parameter, db)) mixerValues[parameter] = level;
        }

        private static float Unit(float value) => float.IsNaN(value) || float.IsInfinity(value) ? 0 : Mathf.Clamp01(value);
        private void OnDisable() => StopAll();
    }
}
