using System;
using UnityEngine;

namespace MiniGame.UnityHost
{
    [CreateAssetMenu(menuName = "MiniGame/监督员配音", fileName = "SupervisorVoiceDefault")]
    public sealed class SupervisorVoiceProfile : ScriptableObject
    {
        public const string ResourcePath = "MiniGame/SupervisorVoice/Default";
        [InspectorName("启用监督员配音")] public bool VoiceEnabled = true;
        [Range(0, 2), InspectorName("配音音量")] public float Volume = 1.152f;
        [Min(1), InspectorName("瞬间显示文字时的配音字速")] public float InstantTextCharactersPerSecond = 30;
        [InspectorName("模糊低沉的语言素材")] public AudioClip[] Grains = Array.Empty<AudioClip>();

        [NonSerialized] private float[][] samples;
        [NonSerialized] private int frequency;

        internal bool TryGetBank(out float[][] bank, out int sampleRate)
        {
            bank = null; sampleRate = 0;
            if (!VoiceEnabled || Grains == null || Grains.Length == 0) return false;
            if (samples == null)
            {
                var loaded = new float[Grains.Length][];
                int rate = Grains[0] != null ? Grains[0].frequency : 0;
                for (int i = 0; i < Grains.Length; i++)
                {
                    var clip = Grains[i];
                    if (clip == null || clip.samples < 2 || clip.channels != 1 || clip.frequency != rate) return false;
                    loaded[i] = new float[clip.samples];
                    if (!clip.GetData(loaded[i], 0)) return false;
                }
                samples = loaded; frequency = rate;
            }
            bank = samples; sampleRate = frequency;
            return true;
        }

        private void OnValidate() => samples = null;
    }
}
