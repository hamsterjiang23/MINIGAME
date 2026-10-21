using System;
using System.Collections.Generic;
using UnityEngine;

namespace MiniGame.UnityHost
{
    public enum RoomLightingPeriod { Day, Night }
    public enum RoomLightingPreviewMode { Automatic, Day, Night }

    [Serializable]
    public sealed class SceneLightSettings
    {
        public string HierarchyPath = "";
        public bool dayEnabled = true;
        public Vector3 localPosition;
        public Vector3 localEuler;
        [ColorUsage(false, true)] public Color color = Color.white;
        [Range(0, 100)] public float intensity = 1;
        [Range(.01f, 100)] public float range = 10;
        public LightType type = LightType.Point;
        [Range(1, 179)] public float spotAngle = 30;
        [Range(0, 179)] public float innerSpotAngle = 21.8f;
        public LightShadows shadows = LightShadows.None;
        [Range(0, 1)] public float shadowStrength = 1;
        [Range(0, 2)] public float shadowBias = .05f;
        [Range(0, 3)] public float shadowNormalBias = .4f;

        public SceneLightSettings Copy() => (SceneLightSettings)MemberwiseClone();

        internal void ValidateValues()
        {
            HierarchyPath ??= "";
            localPosition = new Vector3(RoomLightingProfile.Safe(localPosition.x, -1000, 1000, 0),
                RoomLightingProfile.Safe(localPosition.y, -1000, 1000, 0), RoomLightingProfile.Safe(localPosition.z, -1000, 1000, 0));
            localEuler = RoomLightingProfile.SafeEuler(localEuler, Vector3.zero);
            color = RoomLightingProfile.SafeColor(color, Color.white);
            intensity = RoomLightingProfile.Safe(intensity, 0, 100, 1);
            range = RoomLightingProfile.Safe(range, .01f, 100, 10);
            spotAngle = RoomLightingProfile.Safe(spotAngle, 1, 179, 30);
            innerSpotAngle = RoomLightingProfile.Safe(innerSpotAngle, 0, spotAngle, 21.8f);
            shadowBias = RoomLightingProfile.Safe(shadowBias, 0, 2, .05f);
            shadowStrength = RoomLightingProfile.Safe(shadowStrength, 0, 1, 1);
            shadowNormalBias = RoomLightingProfile.Safe(shadowNormalBias, 0, 3, .4f);
            if (type != LightType.Point && type != LightType.Spot && type != LightType.Directional) type = LightType.Point;
            if (shadows != LightShadows.None && shadows != LightShadows.Hard && shadows != LightShadows.Soft) shadows = LightShadows.None;
        }
    }

    [Serializable]
    public sealed class RoomNightLightingSettings
    {
        public Color ambientColor = Color.black;
        [Range(0, 1)] public float reflectionIntensity;
        [Range(0, 4)] public float lightMultiplier;
        [Range(0, 4)] public float emissionMultiplier;
        [Range(.01f, 10)] public float fadeSeconds = 1.1f;
        public List<SceneLightSettings> SceneLights = new List<SceneLightSettings>();

        internal void ValidateValues()
        {
            ambientColor = RoomLightingProfile.SafeColor(ambientColor, Color.black);
            reflectionIntensity = RoomLightingProfile.Safe(reflectionIntensity, 0, 1, 0);
            lightMultiplier = RoomLightingProfile.Safe(lightMultiplier, 0, 4, 0);
            emissionMultiplier = RoomLightingProfile.Safe(emissionMultiplier, 0, 4, 0);
            fadeSeconds = RoomLightingProfile.Safe(fadeSeconds, .01f, 10, 1.1f);
            SceneLights ??= new List<SceneLightSettings>();
            foreach (var light in SceneLights) light?.ValidateValues();
        }
    }

    [CreateAssetMenu(menuName = "MiniGame/画面/房间灯光配置", fileName = "RoomLightingProfile")]
    public sealed class RoomLightingProfile : ScriptableObject
    {
        [Header("当前时段整体控制")]
        [Range(0, 4)] public float lightMultiplier = 1;
        [Range(0, 4)] public float emissionMultiplier = 1;
        [Range(.01f, 10)] public float fadeSeconds = 1.1f;
        [Header("吊灯聚光灯")]
        [Tooltip("相对灯罩开口的位置；Y 负值表示向下。")]
        public Vector3 lampOffset = new Vector3(0, -.04f, 0);
        public Vector3 lampEulerAngles = new Vector3(90, 0, 0);
        public Color lampColor = new Color(1, .84f, .62f);
        [Range(0, 8)] public float lampIntensity = 4.8f;
        [Range(1, 12)] public float lampRange = 7;
        [Range(20, 170)] public float outerAngle = 125;
        [Range(0, 165)] public float innerAngle = 70;
        [Range(0, 1)] public float shadowStrength = 1;
        [Range(0, .1f)] public float shadowBias = .015f;
        [Range(0, 1)] public float shadowNormalBias = .08f;
        [Range(0, 4)] public float bulbEmission = 1.4f;
        [Header("环境与窗边")]
        public Color ambientColor = new Color(.22f, .205f, .155f);
        [Range(0, 1)] public float reflectionIntensity = .15f;
        public Color windowColor = new Color(.62f, .72f, .8f);
        [Range(0, 2)] public float windowIntensity = .18f;
        [Range(1, 8)] public float windowRange = 4.5f;
        [Range(0, 1)] public float monitorIntensity = .12f;
        [Header("可选照明分层（默认关闭）")]
        public bool lightingBandsEnabled;
        [Range(2, 32)] public int lightingBands = 8;
        [Range(0, 1)] public float lightingBandsStrength = .4f;
        [Range(0, 1)] public float lightingDither;
        [Header("木地板")]
        [Range(.25f, 3)] public float floorGain = 1.65f;
        [Range(.25f, 2)] public float floorContrast = 1.08f;
        [Range(0, 1)] public float floorSmoothness = .13f;
        [Header("地面地毯")]
        public bool rugVisible = true;
        [Tooltip("相对地板中心的 X/Z 偏移；Y 为离地高度。")]
        public Vector3 rugOffset = new Vector3(1.09f, .008f, -.23f);
        public Vector2 rugSize = new Vector2(3.4f, 2);
        [Range(-180, 180)] public float rugYaw;
        public Color rugTint = new Color(.82f, .74f, .60f);

        [Header("场景补光（未添加的灯保持场景原值）")]
        public List<SceneLightSettings> SceneLights = new List<SceneLightSettings>();
        [HideInInspector] public List<SceneLightSettings> SceneLightDefaults = new List<SceneLightSettings>();
        [HideInInspector]
        public RoomNightLightingSettings night = new RoomNightLightingSettings();

        public int Revision { get; private set; }
        public void NotifyChanged() { ValidateValues(); Revision++; }
        private void OnValidate() => NotifyChanged();

        private void ValidateValues()
        {
            lightMultiplier = Safe(lightMultiplier, 0, 4, 1);
            emissionMultiplier = Safe(emissionMultiplier, 0, 4, 1);
            fadeSeconds = Safe(fadeSeconds, .01f, 10, 1.1f);
            lampOffset = SafeVector(lampOffset, new Vector3(0, -.04f, 0));
            lampEulerAngles = SafeEuler(lampEulerAngles, new Vector3(90, 0, 0));
            // Never place the effective light back inside the shade.
            lampOffset.y = Mathf.Min(-.005f, lampOffset.y);
            lampIntensity = Safe(lampIntensity, 0, 8, 4.8f); lampRange = Safe(lampRange, 1, 12, 7);
            outerAngle = Safe(outerAngle, 20, 170, 125); innerAngle = Safe(innerAngle, 0, outerAngle - 1, 70);
            shadowStrength = Safe(shadowStrength, 0, 1, 1); shadowBias = Safe(shadowBias, 0, .1f, .015f);
            shadowNormalBias = Safe(shadowNormalBias, 0, 1, .08f); bulbEmission = Safe(bulbEmission, 0, 4, 1.4f);
            windowIntensity = Safe(windowIntensity, 0, 2, .18f); windowRange = Safe(windowRange, 1, 8, 4.5f);
            monitorIntensity = Safe(monitorIntensity, 0, 1, .12f); reflectionIntensity = Safe(reflectionIntensity, 0, 1, .15f);
            lightingBands = Mathf.Clamp(lightingBands, 2, 32); lightingDither = Safe(lightingDither, 0, 1, 0);
            lightingBandsStrength = Safe(lightingBandsStrength, 0, 1, .4f);
            floorGain = Safe(floorGain, .25f, 3, 1.65f); floorContrast = Safe(floorContrast, .25f, 2, 1.08f);
            floorSmoothness = Safe(floorSmoothness, 0, 1, .13f);
            rugOffset = SafeVector(rugOffset, new Vector3(1.09f, .008f, -.23f)); rugOffset.y = Mathf.Max(.008f, rugOffset.y);
            rugSize = new Vector2(Safe(rugSize.x, .2f, 8, 3.4f), Safe(rugSize.y, .2f, 6, 2));
            rugYaw = Safe(rugYaw, -180, 180, 0);
            lampColor = SafeColor(lampColor, new Color(1, .84f, .62f));
            ambientColor = SafeColor(ambientColor, new Color(.22f, .205f, .155f));
            windowColor = SafeColor(windowColor, new Color(.62f, .72f, .8f)); rugTint = SafeColor(rugTint, Color.white);
            SceneLights ??= new List<SceneLightSettings>();
            foreach (var light in SceneLights) light?.ValidateValues();
            SceneLightDefaults ??= new List<SceneLightSettings>();
            foreach (var light in SceneLightDefaults) light?.ValidateValues();
            night ??= new RoomNightLightingSettings();
            night.ValidateValues();
        }

        internal static float Safe(float value, float min, float max, float fallback) =>
            float.IsNaN(value) || float.IsInfinity(value) ? Mathf.Clamp(fallback, min, max) : Mathf.Clamp(value, min, max);
        private static Vector3 SafeVector(Vector3 value, Vector3 fallback) => new Vector3(
            Safe(value.x, -20, 20, fallback.x), Safe(value.y, -20, 20, fallback.y), Safe(value.z, -20, 20, fallback.z));
        internal static Vector3 SafeEuler(Vector3 value, Vector3 fallback) => new Vector3(
            Angle(value.x, fallback.x), Angle(value.y, fallback.y), Angle(value.z, fallback.z));
        private static float Angle(float value, float fallback) => Mathf.Repeat(Safe(value, -360000, 360000, fallback) + 180, 360) - 180;
        internal static Color SafeColor(Color value, Color fallback) => new Color(
            Safe(value.r, 0, 4, fallback.r), Safe(value.g, 0, 4, fallback.g), Safe(value.b, 0, 4, fallback.b), 1);
    }
}
