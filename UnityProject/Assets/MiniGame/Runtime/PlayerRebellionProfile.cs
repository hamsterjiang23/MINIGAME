using System;
using System.Collections.Generic;
using UnityEngine;

namespace MiniGame.UnityHost
{
    [Serializable]
    public sealed class RebellionChoiceDelta
    {
        public string OptionId;
        [Range(-100, 100)] public int Delta;
    }

    [CreateAssetMenu(menuName = "MiniGame/玩家全局反叛值", fileName = "PlayerRebellionProfile")]
    public sealed class PlayerRebellionProfile : ScriptableObject
    {
        [Range(0, 100), InspectorName("玩家初始反叛值")] public int InitialValue;
        [Range(1, 100), InspectorName("反叛值上限")] public int Maximum = 100;
        [Range(1, 100), InspectorName("梦结局阈值")] public int Threshold = 100;
        [InspectorName("对白选项增减"), Tooltip("未列出的选项增减为 0；独立于各患者信任，只修改玩家全局值。")]
        public List<RebellionChoiceDelta> Choices = new List<RebellionChoiceDelta>();

        public IReadOnlyDictionary<string, int> CreateDeltas()
        {
            var result = new Dictionary<string, int>(StringComparer.Ordinal);
            foreach (var choice in Choices)
            {
                if (choice == null || string.IsNullOrWhiteSpace(choice.OptionId)) continue;
                if (result.ContainsKey(choice.OptionId)) throw new InvalidOperationException("反叛值配置中有重复选项：" + choice.OptionId);
                result.Add(choice.OptionId, choice.Delta);
            }
            return result;
        }

        private void OnValidate()
        {
            Maximum = Mathf.Clamp(Maximum, 1, 100);
            InitialValue = Mathf.Clamp(InitialValue, 0, Maximum);
            Threshold = Mathf.Clamp(Threshold, 1, Maximum);
            if (Choices == null) Choices = new List<RebellionChoiceDelta>();
            foreach (var choice in Choices) if (choice != null) choice.Delta = Mathf.Clamp(choice.Delta, -100, 100);
        }
    }
}
