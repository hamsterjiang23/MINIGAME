using UnityEngine;
using UnityEngine.UI;

namespace MiniGame.UnityHost
{
    /// <summary>Credits are derived from the saved ending clock, including pause and load.</summary>
    public sealed class DreamEndingCredits : MonoBehaviour
    {
        private static readonly string[][] Groups = {
            new[] { "策划", "马步庭", "时晨洋" },
            new[] { "美术", "刘静怡", "刘皓喆", "赵耀" },
            new[] { "程序", "李海江", "黄炜轩" },
            new[] { "发行", "胡雅淳", "吴芷娴", "黎念祺" },
            new[] { "运营", "陈瑶瑶", "朱睿雯", "时子彧", "黄郑志一" }
        };
        private RectTransform viewport, content;
        private float contentHeight;
        public bool IsVisible => viewport != null && viewport.gameObject.activeSelf;
        public float Progress { get; private set; }
        public Vector2 ScrollPosition => content != null ? content.anchoredPosition : Vector2.zero;

        public void Initialize(Transform parent)
        {
            var root = new GameObject("Dream ending credits", typeof(RectTransform), typeof(Image), typeof(RectMask2D));
            root.transform.SetParent(parent, false);
            viewport = root.GetComponent<RectTransform>();
            viewport.anchorMin = Vector2.zero; viewport.anchorMax = Vector2.one;
            viewport.offsetMin = viewport.offsetMax = Vector2.zero;
            var backdrop = root.GetComponent<Image>();
            backdrop.color = new Color(0, 0, 0, .18f);
            backdrop.raycastTarget = false;
            var roll = new GameObject("Credits roll", typeof(RectTransform));
            roll.transform.SetParent(root.transform, false);
            content = roll.GetComponent<RectTransform>();
            content.anchorMin = content.anchorMax = new Vector2(.5f, 0);
            content.pivot = new Vector2(.5f, 1);
            float offset = 0;
            AddLine("制作人员", 48, ref offset, 120);
            foreach (var group in Groups)
            {
                AddLine(group[0], 24, ref offset, 52);
                for (int i = 1; i < group.Length; i++) AddLine(group[i], 36, ref offset, 56);
                offset += 56;
            }
            contentHeight = offset - 56;
            content.sizeDelta = new Vector2(720, contentHeight);
            Refresh(0, false);
        }

        private void AddLine(string value, int size, ref float offset, float pitch)
        {
            var row = new GameObject(value, typeof(RectTransform), typeof(TerminalBitmapText), typeof(Shadow));
            row.transform.SetParent(content, false);
            var label = row.GetComponent<TerminalBitmapText>();
            var rect = label.rectTransform;
            rect.anchorMin = rect.anchorMax = new Vector2(.5f, 1);
            rect.pivot = new Vector2(.5f, 1);
            rect.anchoredPosition = new Vector2(0, -offset);
            rect.sizeDelta = new Vector2(720, size + 12);
            label.ScaleToFontSize = true;
            label.fontSize = size;
            label.LinePitch = size + 12;
            label.text = value;
            label.color = new Color32(255, 207, 104, 255);
            label.alignment = TextAnchor.UpperCenter;
            label.horizontalOverflow = HorizontalWrapMode.Overflow;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            label.raycastTarget = false;
            label.supportRichText = false;
            var shadow = row.GetComponent<Shadow>();
            shadow.effectColor = new Color(0, 0, 0, .85f);
            shadow.effectDistance = new Vector2(2, -2);
            offset += pitch;
        }

        public void Refresh(float progress, bool visible)
        {
            Progress = Mathf.Clamp01(progress);
            if (viewport == null) return;
            viewport.gameObject.SetActive(visible);
            // Every line enters below the picture and leaves above it within the allotted credits interval.
            float y = Mathf.Lerp(-36, viewport.rect.height + contentHeight + 36, Progress);
            content.anchoredPosition = new Vector2(0, Mathf.Round(y / 2) * 2);
        }
    }
}
