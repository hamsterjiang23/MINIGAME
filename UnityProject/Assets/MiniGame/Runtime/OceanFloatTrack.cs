using System;
using UnityEngine;

namespace MiniGame.UnityHost
{
    /// <summary>Smoothed energy of the actual streaming ocean recording, evaluated at its PCM cursor.</summary>
    public sealed class OceanFloatTrack
    {
        [Serializable] private sealed class Envelope
        {
            public string ClipName;
            public int SampleRate, PcmFrames;
            public float EnvelopeHz;
            public float[] Lift;
        }
        private readonly Envelope data;
        private OceanFloatTrack(Envelope value) { data = value; }

        public static OceanFloatTrack Load(AudioClip clip)
        {
            var asset = Resources.Load<TextAsset>("MiniGame/OceanWaveMotion");
            if (asset == null || clip == null) return null;
            var value = JsonUtility.FromJson<Envelope>(asset.text);
            return value != null && value.ClipName == clip.name && value.SampleRate == clip.frequency
                && value.PcmFrames == clip.samples && value.EnvelopeHz > 0 && value.Lift?.Length > 3
                ? new OceanFloatTrack(value) : null;
        }

        public Vector3 Evaluate(int pcmSample)
        {
            float seconds = (pcmSample % data.PcmFrames + data.PcmFrames) % data.PcmFrames / (float)data.SampleRate;
            // Slow heave follows the swell's loudness; small delayed tilts suggest a floating support.
            return new Vector3(Sample(seconds), Sample(seconds - .35f), Sample(seconds - 1.1f));
        }

        private float Sample(float seconds)
        {
            float x = Mathf.Repeat(seconds * data.EnvelopeHz, data.Lift.Length);
            int index = Mathf.FloorToInt(x), count = data.Lift.Length;
            float t = x - index;
            float a = data.Lift[(index + count - 1) % count], b = data.Lift[index];
            float c = data.Lift[(index + 1) % count], d = data.Lift[(index + 2) % count];
            return Mathf.Clamp(.5f * ((2 * b) + (-a + c) * t + (2 * a - 5 * b + 4 * c - d) * t * t
                + (-a + 3 * b - 3 * c + d) * t * t * t), -1, 1);
        }
    }
}
