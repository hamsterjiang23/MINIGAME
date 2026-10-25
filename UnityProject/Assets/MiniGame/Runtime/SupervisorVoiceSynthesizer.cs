using System;
using System.Globalization;
using UnityEngine;

namespace MiniGame.UnityHost
{
    /// <summary>Composes muffled speech fragments into one PCM phrase, with the same duration as its text page.</summary>
    public static class SupervisorVoiceSynthesizer
    {
        public static bool IsSupervisorLine(string sourceId, bool narration = false)
            => !narration && sourceId != null && (sourceId.StartsWith(SupervisorDialogueContent.SourcePrefix, StringComparison.Ordinal)
                || sourceId.StartsWith("supervisor-ending__", StringComparison.Ordinal));

        public static AudioClip CreateClip(SupervisorVoiceProfile profile, string text, float charactersPerSecond)
        {
            if (profile == null || string.IsNullOrWhiteSpace(text) || !profile.TryGetBank(out var bank, out int rate)) return null;
            int[] elements = StringInfo.ParseCombiningCharacters(text);
            float speed = charactersPerSecond > 0 && !float.IsInfinity(charactersPerSecond)
                ? charactersPerSecond : Mathf.Max(1, profile.InstantTextCharactersPerSecond);
            if (float.IsNaN(speed) || float.IsInfinity(speed)) speed = 30;
            // Preserve the original text clock instead of silently speeding up abnormally slow/large input.
            float duration = elements.Length / speed;
            if (duration > 60) return null;
            int frames = Mathf.Max(1, Mathf.RoundToInt(duration * rate));
            var pcm = new float[frames];
            uint random = 2166136261;
            foreach (char c in text) random = (random ^ c) * 16777619;
            if (random == 0) random = 1;
            // Punctuation still contributes to page duration, but no longer opens audible gaps.
            AddMurmur(pcm, bank, rate, 0, frames, ref random);
            float peak = 0;
            for (int i = 0; i < frames; i++) peak = Mathf.Max(peak, Mathf.Abs(pcm[i]));
            float gain = peak > .82f ? .82f / peak : 1;
            int fade = Mathf.Min(frames / 2, Mathf.RoundToInt(.018f * rate));
            for (int i = 0; i < frames; i++)
            {
                float edge = fade == 0 ? 1 : Mathf.Min(1, Mathf.Min(i, frames - 1 - i) / (float)fade);
                pcm[i] *= gain * edge;
            }
            var output = AudioClip.Create("Supervisor murmur (" + elements.Length + " chars)", frames, 1, rate, false);
            output.SetData(pcm, 0);
            return output;
        }

        private static void AddMurmur(float[] output, float[][] bank, int rate, int begin, int end, ref uint random)
        {
            int cursor = begin;
            int edge = Mathf.Max(1, Mathf.RoundToInt(.022f * rate));
            while (cursor < end)
            {
                float[] grain = bank[(int)(Next(ref random) * bank.Length)];
                // Keep contiguous spoken syllables and breath contours rather than repeatedly
                // retriggering a short periodic grain, which sounds like mechanical rumble.
                float pitch = .96f + Next(ref random) * .08f;
                int length = Mathf.Min(end - cursor, Mathf.RoundToInt((.85f + Next(ref random) * .55f) * rate));
                length = Mathf.Max(1, Mathf.Min(length, Mathf.FloorToInt((grain.Length - 2) / pitch)));
                int offset = Mathf.FloorToInt(Next(ref random) * Mathf.Max(0, grain.Length - length * pitch - 2));
                // A short page must begin on speech, not on an interior punctuation gap.
                int probe = Mathf.Min(length, Mathf.RoundToInt(.025f * rate));
                for (int attempt = 0; probe > 0 && attempt < 30; attempt++)
                {
                    float energy = 0;
                    for (int s = 0; s < probe; s++) energy += Mathf.Abs(grain[Mathf.Min(grain.Length - 1, offset + s)]);
                    if (energy / probe > .03f) break;
                    offset = (offset + Mathf.RoundToInt(.035f * rate)) % Mathf.Max(1, Mathf.FloorToInt(grain.Length - length * pitch - 1));
                }
                float gain = .74f + Next(ref random) * .24f;
                for (int j = 0; j < length; j++)
                {
                    float index = offset + j * pitch;
                    int a = Mathf.Min(grain.Length - 2, (int)index);
                    float value = Mathf.Lerp(grain[a], grain[a + 1], index - a);
                    float envelope = Mathf.Min(1, Mathf.Min(j, length - 1 - j) / (float)edge);
                    output[cursor + j] += value * gain * envelope;
                }
                cursor += Mathf.Max(1, length - Mathf.Min(edge, length / 3));
            }
        }

        private static float Next(ref uint value)
        {
            value ^= value << 13; value ^= value >> 17; value ^= value << 5;
            return (value >> 8) / 16777216f;
        }
    }
}
