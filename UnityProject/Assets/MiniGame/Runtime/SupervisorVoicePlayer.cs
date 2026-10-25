using UnityEngine;

namespace MiniGame.UnityHost
{
    /// <summary>Owns only supervisor voice. Text follows its PCM cursor; existing music keeps its own source.</summary>
    [DisallowMultipleComponent]
    public sealed class SupervisorVoicePlayer : MonoBehaviour
    {
        private const MiniGameAudioChannel Channel = MiniGameAudioChannel.SupervisorVoice;
        private MiniGameAudioService service;
        private SupervisorVoiceProfile profile;
        private AudioClip phrase;
        private bool paused, started;
        private float progress;
        public AudioClip CurrentClip => phrase;

        public void Initialize(MiniGameAudioService audio, SupervisorVoiceProfile settings = null)
        {
            Stop();
            service = audio;
            profile = settings != null ? settings : Resources.Load<SupervisorVoiceProfile>(SupervisorVoiceProfile.ResourcePath);
        }

        public void BeginPage(string text, float charactersPerSecond)
        {
            Stop();
            if (!isActiveAndEnabled || service == null || !service.isActiveAndEnabled) return;
            phrase = SupervisorVoiceSynthesizer.CreateClip(profile, text, charactersPerSecond);
            if (phrase == null) return;
            float volume = float.IsNaN(profile.Volume) || float.IsInfinity(profile.Volume) ? 0 : Mathf.Clamp(profile.Volume, 0, 2);
            // AudioSource.volume stops at 1. Apply boosts to the owned PCM instead of losing them to that clamp.
            if (volume > 1)
            {
                var pcm = new float[phrase.samples];
                phrase.GetData(pcm, 0);
                for (int i = 0; i < pcm.Length; i++) pcm[i] *= volume;
                phrase.SetData(pcm, 0);
            }
            service.PlayVoice(phrase, 0, Mathf.Min(1, volume), Channel);
            if (paused) service.Pause(Channel);
            started = service.GetSource(Channel)?.isPlaying == true;
        }

        public bool TryGetProgress(out float normalized)
        {
            normalized = progress;
            var source = service != null ? service.GetSource(Channel) : null;
            if (phrase == null || source == null || source.clip != phrase || !service.isActiveAndEnabled) return false;
            if (!paused && !service.MenuSuspended)
            {
                if (source.isPlaying)
                {
                    started = true;
                    progress = Mathf.Max(progress, source.timeSamples / (float)phrase.samples);
                }
                else if (started) progress = 1; // Unity resets timeSamples to zero at natural completion.
            }
            normalized = progress;
            return true;
        }

        public void SetPaused(bool value)
        {
            if (paused == value) return;
            paused = value;
            if (service == null || phrase == null) return;
            if (paused) service.Pause(Channel);
            else service.Resume(Channel);
        }

        public void Stop()
        {
            if (service != null && phrase != null)
            {
                var source = service.GetSource(Channel);
                service.Stop(Channel);
                if (source != null && source.clip == phrase) source.clip = null;
            }
            if (phrase != null) Destroy(phrase);
            phrase = null; started = false; progress = 0;
        }

        private void OnDisable() => SetPaused(true);
        private void OnDestroy() => Stop();
    }
}
