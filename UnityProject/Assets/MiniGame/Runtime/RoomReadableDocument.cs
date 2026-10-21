using System;
using UnityEngine;

namespace MiniGame.UnityHost
{
    [CreateAssetMenu(menuName = "MiniGame/房间阅读文档")]
    public sealed class RoomReadableDocument : ScriptableObject
    {
        [Serializable]
        private sealed class DailyText
        {
            [SerializeField, Min(1), InspectorName("天数")] private int day = 1;
            [SerializeField, TextArea(5, 20), InspectorName("当日全文")] private string text = "";

            public int Day => day;
            public string Text => text ?? "";
        }

        [SerializeField, InspectorName("稳定标识")] private string id = "";
        [SerializeField, InspectorName("标题")] private string title = "";
        [SerializeField, InspectorName("交互提示")] private string interactionLabel = "阅读";
        [SerializeField, TextArea(5, 20), InspectorName("全文")] private string text = "";
        [SerializeField, InspectorName("按天切换内容")] private bool changesWithDay;
        [SerializeField, InspectorName("每日内容")]
        [Tooltip("仅在指定天数显示对应内容；没有配置的天数不显示，避免提前透露新闻。")]
        private DailyText[] dailyTexts = Array.Empty<DailyText>();

        public string Id => id;
        public string Title => title;
        public string InteractionLabel => string.IsNullOrWhiteSpace(interactionLabel) ? "阅读" : interactionLabel;

        public string GetText(int day)
        {
            if (!changesWithDay) return text ?? "";
            if (dailyTexts != null)
                foreach (var entry in dailyTexts)
                    if (entry != null && entry.Day == day) return entry.Text;
            return "";
        }
    }
}
