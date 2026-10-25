using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace MiniGame.UnityHost.Editor
{
    public static class SupervisorVoicePreview
    {
        [MenuItem("MiniGame/音频/导出监督员配音试听")]
        public static void ExportActualLines()
        {
            var profile = Resources.Load<SupervisorVoiceProfile>(SupervisorVoiceProfile.ResourcePath);
            var lines = SupervisorDialogueContent.Load().conversations.SelectMany(day => day.lines).ToArray();
            string directory = Environment.GetEnvironmentVariable("MINIGAME_SUPERVISOR_VOICE_PREVIEW");
            if (string.IsNullOrEmpty(directory)) directory = Path.GetFullPath(Path.Combine(Application.dataPath, "../../Artifacts/Audio/SupervisorVoice"));
            Directory.CreateDirectory(directory);
            var samples = new List<float>();
            var report = new StringBuilder("# 监督员配音试听\n\n当前默认出字速度：30 字/秒。片段连续拼接，不添加句间空白。\n\n");
            int rate = 24000;
            foreach (var line in new[] { lines[0], lines[1], lines[2], lines.OrderByDescending(item => item.text.Length).First(), lines.Last() })
            {
                var clip = SupervisorVoiceSynthesizer.CreateClip(profile, line.text, 30);
                if (clip == null) throw new InvalidOperationException("Supervisor voice resource bank is unavailable.");
                var pcm = new float[clip.samples]; clip.GetData(pcm, 0); rate = clip.frequency;
                WriteWav(Path.Combine(directory, line.id + ".wav"), pcm, rate);
                report.AppendLine($"- {line.id}：{line.text}（{StringInfo.ParseCombiningCharacters(line.text).Length} 字，{clip.length:F3} 秒）");
                samples.AddRange(pcm);
                UnityEngine.Object.DestroyImmediate(clip);
            }
            WriteWav(Path.Combine(directory, "supervisor-audition.wav"), samples.ToArray(), rate);
            File.WriteAllText(Path.Combine(directory, "audition.md"), report.ToString(), new UTF8Encoding(false));
            Debug.Log("Supervisor voice preview exported: " + directory);
        }

        private static void WriteWav(string path, float[] pcm, int rate)
        {
            using var writer = new BinaryWriter(File.Create(path));
            writer.Write(Encoding.ASCII.GetBytes("RIFF")); writer.Write(36 + pcm.Length * 2);
            writer.Write(Encoding.ASCII.GetBytes("WAVEfmt ")); writer.Write(16); writer.Write((short)1);
            writer.Write((short)1); writer.Write(rate); writer.Write(rate * 2); writer.Write((short)2); writer.Write((short)16);
            writer.Write(Encoding.ASCII.GetBytes("data")); writer.Write(pcm.Length * 2);
            foreach (float value in pcm) writer.Write((short)Mathf.RoundToInt(Mathf.Clamp(value, -1, 1) * 32767));
        }
    }
}
