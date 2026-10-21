using System;
using System.Collections.Generic;

namespace MiniGame.Presentation
{
    public enum TerminalPowerPhase { Boot, Loading, Running, Shutdown, Safe, Collapse, Off }

    /// <summary>Deterministic presentation timing from the v0.8.12 reference. No game-state effects.</summary>
    public sealed class TerminalPowerFrame
    {
        public TerminalPowerPhase Phase { get; }
        public string ConsoleText { get; }
        public bool Cursor { get; }
        public int LoadedRegions { get; }
        public float Collapse { get; }
        public TerminalPowerFrame(TerminalPowerPhase phase, string text = "", bool cursor = false, int loaded = 0, float collapse = 0)
        { Phase = phase; ConsoleText = text; Cursor = cursor; LoadedRegions = loaded; Collapse = collapse; }
    }

    public static class TerminalPowerTimeline
    {
        public const float BootDuration = 7.5f;
        public const float LoadingDuration = 1.48f;
        public const float ShutdownDuration = 3.885f;
        private static readonly float[] ShutdownStops = { .375f, .875f, 1.25f, 1.625f, 1.75f, 2.25f, 2.5f, 3.125f, ShutdownDuration };
        private static readonly float[] LoadTimes = { .12f, .28f, .44f, .50f, .56f, .62f, .68f, .80f, .96f, 1.02f, 1.08f, 1.14f, 1.20f, 1.36f };
        private static readonly int[] Counts = { 4, 10, 16, 21, 28, 32, 36, 43, 48, 50, 56, 64 };
        private static readonly float[] Bursts = { .125f, .125f, .250f, .125f, .625f, .125f, .125f, .125f, .375f, .125f, .125f, .500f };
        private const string Prefix = "PMS console 3.14\n\nSelf-test\n";
        private const string Closing = "Closing session...", Closed = "Session closed.", Halted = "System halted.\n\n>>>";
        private const string Ready = "PMS / 3.14\n\nConsole ready.\n";
        private static readonly string[] TestOutputs = Array.ConvertAll(Counts, count => Prefix + "\n" + Tests(count));
        private static readonly string BootPrompt = TestOutputs[TestOutputs.Length - 1] + "\n\nTests completed.\n\n>>>";
        private static readonly string BootCommand = BootPrompt + " BOOT";

        public static TerminalPowerFrame Boot(float elapsed)
        {
            float t = Sanitize(elapsed);
            if (t >= BootDuration + LoadingDuration) return new TerminalPowerFrame(TerminalPowerPhase.Running);
            if (t >= BootDuration)
            {
                int loaded = 0;
                foreach (float at in LoadTimes) if (t - BootDuration + .00001f >= at) loaded++;
                return new TerminalPowerFrame(TerminalPowerPhase.Loading, loaded: loaded);
            }
            bool blink = ((int)(t / .5f) & 1) == 0;
            if (t < .625f) return Console("", blink);
            if (t < 1.125f) return Console(Reveal("", Prefix, t - .625f, .4f), false);
            float boundary = 1.125f;
            string previous = Prefix;
            for (int i = 0; i < Counts.Length; i++)
            {
                string target = TestOutputs[i];
                if (t < boundary + Bursts[i]) return Console(Reveal(previous, target, t - boundary, Bursts[i]), false);
                boundary += Bursts[i]; previous = target;
            }
            if (t < 5.125f) return Console(Reveal(previous, BootPrompt, t - boundary, .625f), blink);
            if (t < 5.875f) return Console(Reveal(BootPrompt, BootCommand, t - 5.125f, .375f), false);
            if (t < 6.250f) return Console("", false);
            return Console(Reveal("", Ready, t - 6.250f, .5f), blink);
        }

        public static TerminalPowerFrame Shutdown(float elapsed)
        {
            float t = Sanitize(elapsed);
            if (t >= ShutdownDuration) return new TerminalPowerFrame(TerminalPowerPhase.Off);
            if (t >= 3.125f) return new TerminalPowerFrame(TerminalPowerPhase.Collapse, Halted, true, collapse: (t - 3.125f) / .760f);
            if (t >= 2.5f) return new TerminalPowerFrame(TerminalPowerPhase.Safe, Halted, true);
            bool blink = ((int)(t / .5f) & 1) == 0;
            string text = t < .375f ? "" : t < 1.25f ? Reveal("", Closing, t - .375f, .5f)
                : t < 1.75f ? Reveal("", Closed, t - 1.25f, .375f)
                : Reveal(Closed, Closed + "\n\n" + Halted, t - 1.75f, .5f);
            return new TerminalPowerFrame(TerminalPowerPhase.Shutdown, text, t >= 1.75f && blink);
        }

        /// <summary>Never spend a delayed frame's catch-up time on output that has not been shown yet.</summary>
        public static float AdvanceShutdown(float elapsed, float delta)
        {
            float t = Sanitize(elapsed);
            if (t >= ShutdownDuration) return ShutdownDuration;
            float next = t + Sanitize(delta);
            foreach (float boundary in ShutdownStops)
                if (t < boundary) return Math.Min(next, boundary);
            return ShutdownDuration;
        }

        private static TerminalPowerFrame Console(string text, bool cursor) => new TerminalPowerFrame(TerminalPowerPhase.Boot, text, cursor);
        private static float Sanitize(float value) => float.IsNaN(value) || value < 0 ? 0 : value;
        private static string Reveal(string previous, string target, float elapsed, float duration)
        {
            int added = target.Length - previous.Length;
            // ASCII console output: one first character, then evenly spaced additions and a full-text hold.
            // Clamp before converting to int so a completed interval cannot overflow its character count.
            float progress = Math.Min(1, Math.Max(0, elapsed / duration));
            int visible = Math.Min(added, 1 + (int)Math.Floor(progress * added));
            return target.Substring(0, previous.Length + visible);
        }
        private static string Tests(int count)
        {
            var lines = new List<string>();
            for (int i = 0; i < count; i++)
            {
                if (i % 16 == 0) lines.Add("");
                lines[lines.Count - 1] += (66 - i).ToString("00") + "..";
            }
            return string.Join("\n", lines);
        }
    }
}
