using System;
using UnityEngine;

namespace MiniGame.UnityHost
{
    [Serializable]
    public sealed class TerminalSignalParameters
    {
        [InspectorName("开启效果")] public bool Enabled = true;
        [InspectorName("整体强度"), Range(0, 1)] public float Intensity = .8f;
        [InspectorName("横向撕裂"), Range(0, .12f)] public float Tear = .025f;
        [InspectorName("色彩分离"), Range(0, .03f)] public float RgbSplit = .004f;
        [InspectorName("噪点"), Range(0, .25f)] public float Noise = .025f;
        [InspectorName("扫描线强度"), Range(0, .8f)] public float ScanlineStrength = .22f;
        [InspectorName("扫描线数量"), Range(20, 600)] public float ScanlineDensity = 100;
        [InspectorName("扫描线速度（条距/秒）"), Range(0, 10)] public float ScanlineSpeed = .65f;
        [InspectorName("滚动干扰带"), Range(0, .15f)] public float RollStrength = .012f;
        [InspectorName("干扰带速度（屏/秒）"), Range(0, 3)] public float RollSpeed = .18f;
        [InspectorName("噪声刷新（Hz）"), Range(1, 30)] public float Rate = 12;
        [InspectorName("故障频率（次/秒）"), Range(.1f, 10)] public float BurstFrequency = .65f;
        [InspectorName("故障时长（秒）"), Range(.02f, .8f)] public float BurstDuration = .2f;

        public static TerminalSignalParameters LogoDefaults() => new TerminalSignalParameters
        {
            Intensity = .95f, Tear = .06f, RgbSplit = .012f, Noise = .04f,
            ScanlineStrength = .12f, ScanlineDensity = 55, ScanlineSpeed = 1.2f,
            RollStrength = 0, RollSpeed = .3f, Rate = 18,
            BurstFrequency = .8f, BurstDuration = .26f
        };

        public void Sanitize()
        {
            Intensity = Limit(Intensity, 0, 1); Tear = Limit(Tear, 0, .12f);
            RgbSplit = Limit(RgbSplit, 0, .03f); Noise = Limit(Noise, 0, .25f);
            ScanlineStrength = Limit(ScanlineStrength, 0, .8f);
            ScanlineDensity = Limit(ScanlineDensity, 20, 600);
            ScanlineSpeed = Limit(ScanlineSpeed, 0, 10);
            RollStrength = Limit(RollStrength, 0, .15f); RollSpeed = Limit(RollSpeed, 0, 3);
            Rate = Limit(Rate, 1, 30); BurstFrequency = Limit(BurstFrequency, .1f, 10);
            BurstDuration = Limit(BurstDuration, .02f, .8f);
        }

        private static float Limit(float value, float min, float max) =>
            float.IsNaN(value) || float.IsInfinity(value) ? min : Mathf.Clamp(value, min, max);

        public void Apply(Material material, float clock, bool logo)
        {
            material.SetFloat("_Clock", clock);
            material.SetFloat("_LogoMode", logo ? 1 : 0);
            material.SetFloat("_Intensity", Enabled ? Intensity : 0);
            material.SetFloat("_Tear", Tear);
            material.SetFloat("_RgbSplit", RgbSplit);
            material.SetFloat("_Noise", Noise);
            material.SetFloat("_ScanlineStrength", ScanlineStrength);
            material.SetFloat("_ScanlineDensity", ScanlineDensity);
            material.SetFloat("_ScanlineSpeed", ScanlineSpeed);
            material.SetFloat("_RollStrength", RollStrength);
            material.SetFloat("_RollSpeed", RollSpeed);
            material.SetFloat("_Rate", Rate);
            material.SetFloat("_BurstFrequency", BurstFrequency);
            material.SetFloat("_BurstDuration", BurstDuration);
        }
    }

    [CreateAssetMenu(menuName = "MiniGame/标志与监控扰动配置")]
    public sealed class TerminalSignalProfile : ScriptableObject
    {
        [InspectorName("Norm 标志")] public TerminalSignalParameters Logo = TerminalSignalParameters.LogoDefaults();
        [InspectorName("监控画面")] public TerminalSignalParameters Monitor = new TerminalSignalParameters();

        public void Sanitize()
        {
            if (Logo == null) Logo = TerminalSignalParameters.LogoDefaults();
            if (Monitor == null) Monitor = new TerminalSignalParameters();
            Logo.Sanitize(); Monitor.Sanitize();
        }
        private void OnValidate() => Sanitize();
    }
}
