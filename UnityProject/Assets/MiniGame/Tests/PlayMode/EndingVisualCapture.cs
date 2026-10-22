using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using MiniGame.Presentation;
using NUnit.Framework;
using UnityEngine;

namespace MiniGame.UnityHost.Tests
{
    /// <summary>Opt-in captures of the real Game view, including its live overlay canvases.</summary>
    internal static class EndingVisualCapture
    {
        private const string EnvironmentKey = "MINIGAME_ENDING_CAPTURE_DIR";
        private static readonly List<CaptureRecord> pending = new List<CaptureRecord>();
        private static string requestedDirectory, runDirectory;
        internal static bool Enabled => !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(EnvironmentKey));

        internal static void Request(string name, GameRuntime runtime)
        {
            string requested = Environment.GetEnvironmentVariable(EnvironmentKey);
            if (string.IsNullOrWhiteSpace(requested)) return;
            if (requested != requestedDirectory)
            {
                requestedDirectory = requested;
                runDirectory = Path.Combine(Path.GetFullPath(requested),
                    DateTime.UtcNow.ToString("yyyyMMddTHHmmssfffffffZ") + "-" + Guid.NewGuid().ToString("N").Substring(0, 8));
            }
            Directory.CreateDirectory(runDirectory);
            foreach (char value in name)
                if (!char.IsLetterOrDigit(value) && value != '-' && value != '_')
                    throw new ArgumentException("Capture labels must be plain file names.", nameof(name));
            var view = runtime.GetView();
            var room = UnityEngine.Object.FindObjectOfType<RoomDreamEnding>();
            var thanks = UnityEngine.Object.FindObjectOfType<EndingThanksView>();
            var record = new CaptureRecord
            {
                Label = name, File = Path.Combine(runDirectory, name + "-frame" + Time.frameCount.ToString("D6") + ".png"),
                Test = TestContext.CurrentContext.Test.Name, RequestedFrame = Time.frameCount,
                Realtime = Time.realtimeSinceStartup, Day = view.Game.Day, GamePhase = view.Game.Phase.ToString(),
                Ending = view.Game.Ending.ToString(), EndingStage = view.EndingStage.ToString(),
                TerminalPage = view.TerminalPage.ToString(), TerminalOpen = view.TerminalOpen,
                TerminalPower = view.TerminalPower.ToString(), Paused = view.Paused,
                Line = runtime.CurrentEndingLine?.SourceId ?? string.Empty,
                Portrait = runtime.CurrentEndingLine?.PortraitId ?? string.Empty,
                Transition = room != null ? room.TransitionPhase.ToString() : string.Empty,
                TransitionElapsed = room != null ? room.TransitionElapsed : 0,
                ThanksOpacity = thanks != null ? thanks.Opacity : 0
            };
            // Unity captures the presented framebuffer at the end of this frame. No replacement camera,
            // render texture, canvas mode, resolution or scene material is introduced for this evidence.
            ScreenCapture.CaptureScreenshot(record.File);
            pending.Add(record);
        }

        internal static IEnumerator Capture(string name, GameRuntime runtime)
        {
            if (!Enabled) yield break;
            Request(name, runtime);
            yield return null;
        }

        internal static IEnumerator Flush()
        {
            if (pending.Count == 0) yield break;
            float deadline = Time.realtimeSinceStartup + 15;
            while (pending.Count != 0)
            {
                for (int index = pending.Count - 1; index >= 0; index--)
                {
                    var record = pending[index];
                    if (!TryReadCompletePng(record.File, out var bytes)) continue;
                    record.Width = ReadBigEndian(bytes, 16);
                    record.Height = ReadBigEndian(bytes, 20);
                    Assert.That(record.Width, Is.GreaterThan(0), "Game view capture width: " + record.File);
                    Assert.That(record.Height, Is.GreaterThan(0), "Game view capture height: " + record.File);
                    using (var hash = SHA256.Create())
                        record.Sha256 = BitConverter.ToString(hash.ComputeHash(bytes)).Replace("-", string.Empty).ToLowerInvariant();
                    File.AppendAllText(Path.Combine(Path.GetDirectoryName(record.File), "manifest.jsonl"),
                        JsonUtility.ToJson(record) + Environment.NewLine);
                    Debug.Log("MINIGAME_ENDING_CAPTURE " + record.Label + " " + record.File);
                    pending.RemoveAt(index);
                }
                if (pending.Count == 0) yield break;
                Assert.That(Time.realtimeSinceStartup, Is.LessThan(deadline),
                    "Actual Game view screenshot was not written. Keep the Game view rendering: " + pending[0].File);
                yield return null;
            }
        }

        private static bool TryReadCompletePng(string path, out byte[] bytes)
        {
            bytes = null;
            if (!File.Exists(path)) return false;
            try { bytes = File.ReadAllBytes(path); }
            catch (IOException) { return false; }
            return bytes.Length >= 32 && bytes[0] == 137 && bytes[1] == 80 && bytes[2] == 78 && bytes[3] == 71
                && bytes[bytes.Length - 8] == 73 && bytes[bytes.Length - 7] == 69
                && bytes[bytes.Length - 6] == 78 && bytes[bytes.Length - 5] == 68;
        }

        private static int ReadBigEndian(byte[] bytes, int offset) => bytes[offset] << 24 | bytes[offset + 1] << 16
            | bytes[offset + 2] << 8 | bytes[offset + 3];

        [Serializable]
        private sealed class CaptureRecord
        {
            public string Label, File, Test, GamePhase, Ending, EndingStage, TerminalPage, TerminalPower;
            public string Line, Portrait, Transition, Sha256;
            public int RequestedFrame, Day, Width, Height;
            public bool TerminalOpen, Paused;
            public float Realtime, TransitionElapsed, ThanksOpacity;
        }
    }
}
