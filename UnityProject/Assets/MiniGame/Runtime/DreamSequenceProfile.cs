using System;
using UnityEngine;

namespace MiniGame.UnityHost
{
    [Serializable]
    public sealed class DreamDaySettings
    {
        [InspectorName("日期")] public int Day;
        [Range(5, 180), InspectorName("播放时长（秒）")] public float Duration = 20;
        [InspectorName("梦境画面")] public Texture2D Artwork;
        [InspectorName("顺序画面（按编号）")] public Texture2D[] Frames = Array.Empty<Texture2D>();
        [Range(0, 2), InspectorName("画面叠化时长（秒）")] public float CrossfadeSeconds = .6f;
        [InspectorName("梦境音乐")] public MiniGameSound Music;
        [Range(0, 2), InspectorName("运动强度")] public float MotionIntensity = 1;
        [Range(0, 2), InspectorName("梦境后处理强度")] public float PostIntensity = 1;
    }

    [Serializable]
    public sealed class EndingSubtitle
    {
        [TextArea(1, 3), InspectorName("字幕")] public string Text;
        [InspectorName("配音文件")] public AudioClip Voice;
        [Range(1, 30), InspectorName("无配音时显示秒数")] public float Duration = 4;
    }

    [CreateAssetMenu(menuName = "MiniGame/梦境与结局配置", fileName = "DreamSequenceProfile")]
    public sealed class DreamSequenceProfile : ScriptableObject
    {
        [Header("墙面梦境"), InspectorName("启用正式梦境流程")] public bool EnableDreamFlow = true;
        [Min(.1f), InspectorName("镜头定位时间（秒）")] public float AlignSeconds = 1.2f;
        [Min(.1f), InspectorName("镜头返回时间（秒）")] public float ReturnSeconds = 1;
        [Range(0, .5f), InspectorName("投影边缘留白（米）")] public float ProjectionInset = .15f;
        [InspectorName("每日演出")] public DreamDaySettings[] Days =
        {
            new DreamDaySettings { Day = 1, Music = MiniGameSound.Dream0 },
            new DreamDaySettings { Day = 2, Music = MiniGameSound.Dream1 },
            new DreamDaySettings { Day = 3, Music = MiniGameSound.Dream3 }
        };
        [Header("海滩终局"), InspectorName("使用门口固定全景布景")] public bool UseRoomCubemapEnding = true;
        [InspectorName("门口固定天空盒")] public Material RoomPanoramaSkybox;
        [InspectorName("显示房间白色边框")] public bool ShowRoomOutline;
        [InspectorName("全景捕获源")] public GameObject CubemapSourcePrefab;
        [InspectorName("全景每面分辨率")] public int CubemapFaceSize = 1024;
        [Range(1, 30), InspectorName("全景每秒更新次数")] public float CubemapRefreshRate = 10;
        [Range(-180, 180), InspectorName("全景水平方向（度）")] public float CubemapYaw;
        [Range(.1f, 4), InspectorName("前进速度（米/秒）")] public float WalkSpeed = 1.2f;
        [Range(.5f, 4), InspectorName("床交互距离（米）")] public float BedInteractionDistance = 1.5f;
        [Range(.2f, 5), InspectorName("躺下时间（秒）")] public float LieDownSeconds = 1.5f;
        [Range(0, .3f), InspectorName("床起伏幅度（米）")] public float FloatAmplitude = .08f;
        [Range(2, 30), InspectorName("床起伏周期（秒）")] public float FloatPeriod = 8;
        [Range(0, 5), InspectorName("床摇摆角度")] public float FloatTiltDegrees = 1.5f;
        [Range(0, .2f), InspectorName("漂流速度（米/秒）")] public float DriftSpeed = .04f;
        [Range(0, .03f), InspectorName("流云速度")] public float CloudSpeed = .006f;
        [Header("海浪与岸边"), Range(0, .6f), InspectorName("海浪高度（米）"), Tooltip("控制海面的实际起伏；0 表示平静水面。")]
        public float SeaWaveHeight = .24f;
        [Range(2, 15), InspectorName("海浪周期（秒）"), Tooltip("数值越小，海浪推进越快。")]
        public float SeaWavePeriod = 6;
        [Range(3, 20), InspectorName("海浪长度（米）"), Tooltip("控制相邻波峰的间隔。")]
        public float SeaWaveLength = 7;
        [Range(0, 4), InspectorName("岸边冲刷距离（米）"), Tooltip("海水与湿沙边缘往复推进的距离。")]
        public float ShoreWashDistance = 1.8f;
        [Range(0, 1.5f), InspectorName("浪花泡沫强度"), Tooltip("调整破浪白沫和岸边泡沫的可见程度。")]
        public float SeaFoamAmount = .85f;
        [Range(0, 15), InspectorName("躺稳后字幕延迟（秒）")] public float SubtitleDelay = 2;
        [InspectorName("音频配置")] public MiniGameAudioProfile Audio;
        [InspectorName("终局字幕与配音")] public EndingSubtitle[] EndingLines =
        {
            new EndingSubtitle { Text = "这就是新来的审查员？" },
            new EndingSubtitle { Text = "他看起来已经彻底停止工作了" },
            new EndingSubtitle { Text = "这种蛀虫……" },
            new EndingSubtitle { Text = "败类" },
            new EndingSubtitle { Text = "来吧，我们把他送到下面去" }
        };

        public DreamDaySettings GetDay(int day)
        {
            if (Days != null) foreach (var value in Days) if (value != null && value.Day == day) return value;
            return null;
        }

        private void OnValidate()
        {
            AlignSeconds = Valid(AlignSeconds, .1f, 10, 1.2f);
            ReturnSeconds = Valid(ReturnSeconds, .1f, 10, 1);
            ProjectionInset = Valid(ProjectionInset, 0, .5f, .15f);
            CubemapFaceSize = Mathf.ClosestPowerOfTwo(Mathf.Clamp(CubemapFaceSize, 256, 2048));
            CubemapRefreshRate = Valid(CubemapRefreshRate, 1, 30, 10);
            CubemapYaw = Valid(CubemapYaw, -180, 180, 0);
            WalkSpeed = Valid(WalkSpeed, .1f, 4, 1.2f);
            BedInteractionDistance = Valid(BedInteractionDistance, .5f, 4, 1.5f);
            LieDownSeconds = Valid(LieDownSeconds, .2f, 5, 1.5f);
            FloatAmplitude = Valid(FloatAmplitude, 0, .3f, .08f);
            FloatPeriod = Valid(FloatPeriod, 2, 30, 8);
            FloatTiltDegrees = Valid(FloatTiltDegrees, 0, 5, 1.5f);
            DriftSpeed = Valid(DriftSpeed, 0, .2f, .04f);
            CloudSpeed = Valid(CloudSpeed, 0, .03f, .006f);
            SeaWaveHeight = Valid(SeaWaveHeight, 0, .6f, .24f);
            SeaWavePeriod = Valid(SeaWavePeriod, 2, 15, 6);
            SeaWaveLength = Valid(SeaWaveLength, 3, 20, 7);
            ShoreWashDistance = Valid(ShoreWashDistance, 0, 4, 1.8f);
            SeaFoamAmount = Valid(SeaFoamAmount, 0, 1.5f, .85f);
            SubtitleDelay = Valid(SubtitleDelay, 0, 15, 2);
            if (Days != null) foreach (var day in Days) if (day != null)
            {
                day.Duration = Valid(day.Duration, 5, 180, 20);
                day.MotionIntensity = Valid(day.MotionIntensity, 0, 2, 1);
                day.CrossfadeSeconds = Valid(day.CrossfadeSeconds, 0, 2, .6f);
                day.PostIntensity = Valid(day.PostIntensity, 0, 2, 1);
            }
            if (EndingLines != null) foreach (var line in EndingLines) if (line != null)
                line.Duration = Valid(line.Duration, 1, 30, 4);
        }

        private static float Valid(float value, float min, float max, float fallback)
            => float.IsNaN(value) || float.IsInfinity(value) ? fallback : Mathf.Clamp(value, min, max);
    }

    public enum DreamPlaybackPhase { None, Aligning, Playing, Returning }
    [Serializable]
    public sealed class DreamPlaybackCheckpoint
    {
        public DreamPlaybackPhase Phase;
        public int Day;
        public float Elapsed;
        public float PlaybackSeconds;
        public Vector3 CameraPosition;
        public Quaternion CameraRotation = Quaternion.identity;
        public float FieldOfView;
    }

    public enum BeachEndingPhase { Walking, Lying, Drifting }
    [Serializable]
    public sealed class BeachEndingCheckpoint
    {
        public BeachEndingPhase Phase;
        public float WorldTime;
        public float WalkDistance;
        public float PhaseElapsed;
        public float DriftElapsed;
        public float SubtitleElapsed;
        public int SubtitleIndex;
        public bool SubtitlesComplete;
        public bool HasRoomPose;
        public Vector3 RoomPosition;
        public float RoomYaw;
        public float RoomPitch;
        public Vector3 LieStartPosition;
        public Quaternion LieStartRotation = Quaternion.identity;
    }
}
