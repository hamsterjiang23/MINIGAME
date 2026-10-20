using UnityEngine;

namespace MiniGame.UnityHost
{
    [CreateAssetMenu(menuName = "MiniGame/画面/房间灯光配置", fileName = "RoomLightingProfile")]
    public sealed class RoomLightingProfile : ScriptableObject
    {
        [Header("吊灯聚光灯")]
        [Tooltip("相对灯罩开口的位置；Y 负值表示向下。")]
        public Vector3 lampOffset = new Vector3(0, -.04f, 0);
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

        public int Revision { get; private set; }
        public void NotifyChanged() { ValidateValues(); Revision++; }
        private void OnValidate() => NotifyChanged();

        private void ValidateValues()
        {
            lampOffset = SafeVector(lampOffset, new Vector3(0, -.04f, 0));
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
        }

        private static float Safe(float value, float min, float max, float fallback) =>
            float.IsNaN(value) || float.IsInfinity(value) ? Mathf.Clamp(fallback, min, max) : Mathf.Clamp(value, min, max);
        private static Vector3 SafeVector(Vector3 value, Vector3 fallback) => new Vector3(
            Safe(value.x, -20, 20, fallback.x), Safe(value.y, -20, 20, fallback.y), Safe(value.z, -20, 20, fallback.z));
        private static Color SafeColor(Color value, Color fallback) => new Color(
            Safe(value.r, 0, 4, fallback.r), Safe(value.g, 0, 4, fallback.g), Safe(value.b, 0, 4, fallback.b), 1);
    }
}
