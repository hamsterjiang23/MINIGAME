using UnityEngine;

namespace MiniGame.UnityHost
{
    [CreateAssetMenu(menuName = "MiniGame/对话文字扰动")]
    public sealed class TerminalDialogueTextSettings : ScriptableObject
    {
        [InspectorName("启用对话文字扰动")] public bool Enabled = true;
        [InspectorName("整体强度"), Range(0, 1)] public float Intensity = .65f;
        [InspectorName("随机采样频率（Hz）"), Range(1, 20)] public float Rate = 8;
        [InspectorName("突发概率"), Range(0, 1)] public float BurstChance = .12f;
        [InspectorName("每次持续（秒）"), Range(.01f, .2f)] public float BurstSeconds = .055f;
        [InspectorName("受影响字形比例"), Range(0, 1)] public float GlyphFraction = .08f;
        [InspectorName("横向错位（像素）"), Range(0, 3)] public float HorizontalPixels;
        [InspectorName("上下错位（像素）"), Range(0, 4)] public float VerticalPixels = 2.5f;
        [InspectorName("亮度波动"), Range(0, .3f)] public float BrightnessDip = .12f;
        public int Revision { get; private set; }
        private void OnValidate() => Revision++;
    }
}
