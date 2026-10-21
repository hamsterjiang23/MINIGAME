using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace MiniGame.UnityHost.Editor
{
    public static class DreamImageSequencePreviewExporter
    {
        private const int Width = 960, Height = 540, Fps = 24;
        [Serializable] private sealed class Preview
        {
            public int day, imageCount, videoFrames;
            public float duration, postIntensity;
            public string video;
        }
        [Serializable] private sealed class Manifest
        {
            public string scope = "Fixed-step Unity GPU frames, offline audio mixed from current profile; not a real-time performance measurement.";
            public Preview[] previews;
        }

        [MenuItem("MiniGame/梦境/导出每日梦境视频预览")]
        public static void Export()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("请停止播放再导出梦境视频。");
            string repository = Directory.GetParent(Application.dataPath).Parent.FullName;
            string output = Environment.GetEnvironmentVariable("MINIGAME_DREAM_SEQUENCE_EVIDENCE")
                ?? Path.Combine(repository, "Artifacts", "DreamImageSequencePreview");
            Directory.CreateDirectory(output);
            var profile = AssetDatabase.LoadAssetAtPath<DreamSequenceProfile>(DreamSequenceSetup.ProfilePath);
            var entries = new List<Preview>();
            var readback = new Texture2D(Width, Height, TextureFormat.RGB24, false);
            var bytes = new byte[Width * Height * 3];
            var oldTarget = RenderTexture.active;
            try
            {
                using (var stage = new DreamAnimationStage(Width, Height))
                {
                    for (int day = 1; day <= 3; day++)
                    {
                        var settings = profile.GetDay(day);
                        if (settings.Frames == null || settings.Frames.Length != 10)
                            throw new InvalidOperationException("请先导入每晚 10 张梦境画面。");
                        float duration = Mathf.Clamp(settings.Duration, 5, 180);
                        int frames = Mathf.CeilToInt(duration * Fps);
                        string path = Path.Combine(output, "day" + day + "-dream.mp4");
                        string args = "-y -hide_banner -loglevel error -f rawvideo -pixel_format rgb24 -video_size "
                            + Width + "x" + Height + " -framerate " + Fps + " -i pipe:0";
                        var filters = new List<string>();
                        var labels = new StringBuilder();
                        int input = 1;
                        AddAudio(profile.Audio, settings.Music, false, duration, ref input, ref args, filters, labels);
                        AddAudio(profile.Audio, MiniGameSound.Projector, true, duration, ref input, ref args, filters, labels);
                        if (input > 1)
                        {
                            filters.Add(labels + "amix=inputs=" + (input - 1) + ":normalize=0[a]");
                            args += " -filter_complex " + Quote(string.Join(";", filters)) + " -map 0:v -map " + Quote("[a]") + " -c:a aac -b:a 160k";
                        }
                        else args += " -an";
                        args += " -vf " + Quote("vflip,fade=t=in:st=0:d=0.35,fade=t=out:st=" + Number(duration - .45f) + ":d=0.45")
                            + " -t " + Number(duration) + " -c:v libx264 -preset fast -crf 18 -pix_fmt yuv420p -movflags +faststart " + Quote(path);
                        var errors = new StringBuilder();
                        using (var encoder = new Process { StartInfo = new ProcessStartInfo
                        {
                            FileName = Environment.GetEnvironmentVariable("MINIGAME_FFMPEG") ?? "ffmpeg",
                            Arguments = args, UseShellExecute = false, CreateNoWindow = true,
                            RedirectStandardInput = true, RedirectStandardError = true
                        } })
                        {
                            encoder.ErrorDataReceived += (sender, e) => { if (e.Data != null) errors.AppendLine(e.Data); };
                            encoder.Start(); encoder.BeginErrorReadLine();
                            try
                            {
                                for (int frame = 0; frame < frames; frame++)
                                {
                                    stage.Render(settings, frame / (float)Fps);
                                    RenderTexture.active = stage.Texture;
                                    readback.ReadPixels(new Rect(0, 0, Width, Height), 0, 0, false);
                                    readback.GetRawTextureData<byte>().CopyTo(bytes);
                                    encoder.StandardInput.BaseStream.Write(bytes, 0, bytes.Length);
                                    if (frame == Fps * 3 || frame == Fps * 13)
                                        File.WriteAllBytes(Path.Combine(output, "day" + day + "-" + frame / Fps + "s.png"), readback.EncodeToPNG());
                                }
                                encoder.StandardInput.Close();
                                if (!encoder.WaitForExit(30000)) throw new TimeoutException("视频编码超时。");
                                encoder.WaitForExit();
                                if (encoder.ExitCode != 0) throw new InvalidOperationException("视频编码失败：" + errors);
                            }
                            finally { if (!encoder.HasExited) encoder.Kill(); }
                        }
                        entries.Add(new Preview { day = day, imageCount = 10, videoFrames = frames,
                            duration = duration, postIntensity = settings.PostIntensity, video = path });
                        Debug.Log("DREAM_SEQUENCE_VIDEO day=" + day + " frames=" + frames + " path=" + path);
                    }
                }
                File.WriteAllText(Path.Combine(output, "preview-manifest.json"), JsonUtility.ToJson(new Manifest { previews = entries.ToArray() }, true));
            }
            finally { RenderTexture.active = oldTarget; UnityEngine.Object.DestroyImmediate(readback); }
        }

        private static void AddAudio(MiniGameAudioProfile audio, MiniGameSound id, bool ambience, float duration,
            ref int input, ref string args, List<string> filters, StringBuilder labels)
        {
            var entry = audio != null ? audio.GetEntry(id) : null;
            string path = entry?.Clip != null ? AssetDatabase.GetAssetPath(entry.Clip) : null;
            if (string.IsNullOrEmpty(path) || !File.Exists(path)) return;
            float gain = Mathf.Clamp01(audio.MasterVolume) * Mathf.Clamp01(ambience ? audio.AmbienceVolume : audio.MusicVolume) * Mathf.Clamp01(entry.Volume);
            args += " -stream_loop -1 -i " + Quote(Path.GetFullPath(path));
            string label = "a" + input;
            filters.Add("[" + input + ":a]volume=" + Number(gain) + ",afade=t=in:d=1,afade=t=out:st="
                + Number(duration - 1) + ":d=1[" + label + "]");
            labels.Append("[").Append(label).Append("]"); input++;
        }
        private static string Number(float value) => value.ToString("0.####", CultureInfo.InvariantCulture);
        private static string Quote(string value) => "\"" + value.Replace("\"", "\\\"") + "\"";
    }
}
