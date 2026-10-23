using System;
using UnityEngine;
using UnityEngine.Audio;

namespace MiniGame.UnityHost
{
    public enum MiniGameSound
    {
        [InspectorName("主菜单音乐")] StartMenu = 0,
        [InspectorName("房间音乐")] Gameplay = 1,
        [InspectorName("Day1 鲸尸海滩")] Dream0 = 2,
        [InspectorName("Day2 倒悬花海")] Dream1 = 3,
        [InspectorName("备用：枯花田")] Dream2 = 4,
        [InspectorName("电脑底噪")] PCNoise = 5,
        [InspectorName("放映机")] Projector = 6,
        [InspectorName("扫描")] Scan = 7,
        [InspectorName("检定仪亮灯")] LightBeep = 8,
        [InspectorName("场景交互")] WorldInteraction = 9,
        [InspectorName("界面交互")] UIInteraction = 10,
        [InspectorName("对话弹出")] DialoguePop = 11,
        [InspectorName("Day3 受缚待审")] Dream3 = 12,
        [InspectorName("海滩终局音乐")] EndingMusic = 13,
        [InspectorName("海浪环境声")] OceanWaves = 14,
        [InspectorName("电脑开机")] TerminalPowerOn = 15,
        [InspectorName("电脑关机")] TerminalPowerOff = 16,
        [InspectorName("终端打字 1")] TerminalType1 = 17,
        [InspectorName("终端打字 2")] TerminalType2 = 18,
        [InspectorName("终端打字 3")] TerminalType3 = 19,
        [InspectorName("终端打字 4")] TerminalType4 = 20,
        [InspectorName("开门")] DoorOpen = 21,
        [InspectorName("关门")] DoorShut = 22,
        [InspectorName("放映机启动（单次）")] ProjectorOpen = 23,
        [InspectorName("主菜单标志闪烁")] LogoGlitch = 24,
        [InspectorName("监督员敲门")] DoorKnock = 25,
        [InspectorName("结局结束提示")] EndNote = 26
    }

    [Serializable]
    public sealed class MiniGameSoundEntry
    {
        [InspectorName("用途")] public MiniGameSound Id;
        [InspectorName("音频片段"), Tooltip("保留为空时，此用途不播放声音；不会自动借用其他片段。")]
        public AudioClip Clip;
        [Range(0, 2), InspectorName("片段音量"), Tooltip("此片段的独立音量，再乘以所属通道和总音量。")]
        public float Volume = 1f;
    }

    [CreateAssetMenu(menuName = "MiniGame/音频配置", fileName = "AudioProfile")]
    public sealed class MiniGameAudioProfile : ScriptableObject
    {
        [Header("音量"), Range(0, 1), InspectorName("总音量")] public float MasterVolume = 1f;
        [Range(0, 1), InspectorName("音乐音量")] public float MusicVolume = .6f;
        [Range(0, 1), InspectorName("环境音量")] public float AmbienceVolume = .45f;
        [Range(0, 1), InspectorName("场景音效音量")] public float SfxVolume = .7f;
        [Range(0, 1), InspectorName("界面音效音量")] public float UIVolume = .65f;
        [Min(0), InspectorName("淡入淡出时长（秒）"), Tooltip("音乐与环境循环切换的过渡时间；0 表示立即切换。")]
        public float FadeSeconds = 1f;

        [Header("混音路由"), InspectorName("混音器"), Tooltip("运行时统一控制各通道音量；默认配置使用 MiniGame 混音器。")]
        public AudioMixer Mixer;
        [InspectorName("音乐通道")] public AudioMixerGroup MusicGroup;
        [InspectorName("环境通道")] public AudioMixerGroup AmbienceGroup;
        [InspectorName("场景音效通道")] public AudioMixerGroup SfxGroup;
        [InspectorName("界面音效通道")] public AudioMixerGroup UIGroup;

        [Header("声音条目"), Tooltip("按稳定用途编号查找片段；留空即不播放，不自动借用其他音频。")]
        public MiniGameSoundEntry[] Sounds = Array.Empty<MiniGameSoundEntry>();

        public MiniGameSoundEntry GetEntry(MiniGameSound id)
        {
            if (Sounds == null) return null;
            foreach (var entry in Sounds)
                if (entry != null && entry.Id == id) return entry;
            return null;
        }

        public AudioClip GetClip(MiniGameSound id) => GetEntry(id)?.Clip;

        private void OnValidate()
        {
            MasterVolume = Volume(MasterVolume, 1);
            MusicVolume = Volume(MusicVolume, .6f);
            AmbienceVolume = Volume(AmbienceVolume, .45f);
            SfxVolume = Volume(SfxVolume, .7f);
            UIVolume = Volume(UIVolume, .65f);
            FadeSeconds = float.IsNaN(FadeSeconds) || float.IsInfinity(FadeSeconds) ? 1 : Mathf.Max(0, FadeSeconds);
            if (Sounds == null) Sounds = Array.Empty<MiniGameSoundEntry>();
            foreach (var entry in Sounds)
                if (entry != null) entry.Volume = float.IsNaN(entry.Volume) || float.IsInfinity(entry.Volume)
                    ? 1 : Mathf.Clamp(entry.Volume, 0, 2);
        }

        private static float Volume(float value, float fallback) => float.IsNaN(value) || float.IsInfinity(value)
            ? fallback : Mathf.Clamp01(value);
    }
}
