using System;
using System.Collections.Generic;
using UnityEngine;

namespace MiniGame.UnityHost
{
    /// <summary>Detects attacks in the recording once; rendering then follows the source's PCM cursor.</summary>
    public sealed class KnockImpactTrack
    {
        public const float ResponseSeconds = .48f;
        private readonly int frequency;
        private readonly int[] impacts;
        public IReadOnlyList<int> ImpactSamples => impacts;

        private KnockImpactTrack(int frequency, int[] impacts)
        { this.frequency = frequency; this.impacts = impacts; }

        public static KnockImpactTrack Analyze(AudioClip clip)
        {
            if (clip == null) return new KnockImpactTrack(1, Array.Empty<int>());
            if (clip.loadState != AudioDataLoadState.Loaded) clip.LoadAudioData();
            if (clip.loadState != AudioDataLoadState.Loaded || clip.loadType == AudioClipLoadType.Streaming)
                return new KnockImpactTrack(clip.frequency, Array.Empty<int>());
            var pcm = new float[clip.samples * clip.channels];
            if (!clip.GetData(pcm, 0)) return new KnockImpactTrack(clip.frequency, Array.Empty<int>());

            // Short RMS windows reject the quiet rattles between the four actual impacts.
            int window = Math.Max(1, clip.frequency / 200);
            var rms = new float[(clip.samples + window - 1) / window];
            float peak = 0;
            for (int block = 0; block < rms.Length; block++)
            {
                int first = block * window * clip.channels;
                int end = Math.Min(pcm.Length, first + window * clip.channels);
                double energy = 0;
                for (int i = first; i < end; i++) energy += pcm[i] * pcm[i];
                rms[block] = (float)Math.Sqrt(energy / (end - first));
                peak = Mathf.Max(peak, rms[block]);
            }
            var hits = new List<int>();
            bool armed = true;
            int last = -clip.frequency;
            if (peak > .0001f)
                for (int block = 0; block < rms.Length; block++)
                {
                    if (rms[block] < peak * .18f) armed = true;
                    if (!armed || rms[block] < peak * .4f || block * window - last < clip.frequency * .14f) continue;
                    // Refine the attack to its sample rather than the edge of the analysis window.
                    int onset = block * window;
                    int first = Math.Max(0, onset - window);
                    int end = Math.Min(clip.samples, onset + window);
                    bool found = false;
                    for (int frame = first; frame < end && !found; frame++)
                        for (int channel = 0; channel < clip.channels; channel++)
                            if (Mathf.Abs(pcm[frame * clip.channels + channel]) >= peak * .25f)
                            { onset = frame; found = true; break; }
                    hits.Add(onset);
                    last = onset;
                    armed = false;
                }
            return new KnockImpactTrack(clip.frequency, hits.ToArray());
        }

        public Vector3 Evaluate(int sample)
        {
            var response = Vector3.zero;
            for (int i = impacts.Length - 1; i >= 0; i--)
            {
                float age = (sample - impacts[i]) / (float)frequency;
                if (age < 0) continue;
                if (age >= ResponseSeconds) break;
                float tail = 1 - age / ResponseSeconds;
                float envelope = Mathf.Exp(-age * 4) * tail * tail;
                float phase = age * 2 * Mathf.PI;
                float side = i % 2 == 0 ? 1 : -1;
                // Each hit adds energy to the still-settling room; independent axes avoid a gentle pendulum motion.
                response += new Vector3(
                    .72f * Mathf.Cos(phase * 7.5f) + .28f * Mathf.Cos(phase * 15.7f),
                    side * .72f * (.65f * Mathf.Cos(phase * 6.2f) + .35f * Mathf.Cos(phase * 13.1f)),
                    side * .34f * (.7f * Mathf.Cos(phase * 5.3f) + .3f * Mathf.Sin(phase * 11.3f))) * envelope;
            }
            return response;
        }
    }
}
