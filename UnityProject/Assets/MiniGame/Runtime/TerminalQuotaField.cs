using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

namespace MiniGame.UnityHost
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public sealed class TerminalQuotaField : MonoBehaviour
    {
        private readonly Dictionary<int, Rect> fields = new Dictionary<int, Rect>();
        private Texture2D atlas;
        private RawImage artwork;
        private Text fallbackLabel;
        private int currentValue;
        private Vector2Int fieldSize;

        public void Initialize(TerminalAssetSet assets, Text fallbackLabel)
            => Initialize(assets != null ? assets.QuotaFields : null, assets != null ? assets.QuotaFieldMetrics : null,
                fallbackLabel, new Vector2Int(66, 32));

        public void Initialize(Texture2D texture, TextAsset metrics, Text fallbackLabel, Vector2Int size)
        {
            if (fallbackLabel == null) throw new ArgumentNullException(nameof(fallbackLabel));
            if (this.fallbackLabel != null && this.fallbackLabel != fallbackLabel) this.fallbackLabel.enabled = true;
            this.fallbackLabel = fallbackLabel;
            atlas = texture; fieldSize = size;
            ReadMetrics(metrics);
            var parent = (RectTransform)transform;
            parent.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
            parent.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);
            if (artwork == null)
            {
                var owner = new GameObject("DailyWorkQuotaArtwork", typeof(RectTransform), typeof(RawImage));
                owner.layer = gameObject.layer;
                artwork = owner.GetComponent<RawImage>();
                artwork.rectTransform.SetParent(transform, false);
                artwork.rectTransform.anchorMin = Vector2.zero;
                artwork.rectTransform.anchorMax = Vector2.one;
                artwork.rectTransform.offsetMin = artwork.rectTransform.offsetMax = Vector2.zero;
                artwork.raycastTarget = false;
                artwork.color = Color.white;
            }
            artwork.texture = atlas;
            SetValue(currentValue);
        }

        public void SetValue(int value)
        {
            currentValue = Math.Max(0, value);
            if (fallbackLabel == null) return;
            fallbackLabel.text = currentValue.ToString("D2", CultureInfo.InvariantCulture);
            Rect rectangle = default;
            bool useAtlas = atlas != null && fields.TryGetValue(currentValue, out rectangle);
            if (artwork != null)
            {
                artwork.enabled = useAtlas;
                if (useAtlas) artwork.uvRect = rectangle;
            }
            // Keep the actual number accessible even when the supplied pixels draw it.
            fallbackLabel.enabled = !useAtlas;
        }

        private void ReadMetrics(TextAsset source)
        {
            fields.Clear();
            if (atlas == null || source == null) return;
            QuotaMetrics metrics;
            try { metrics = JsonUtility.FromJson<QuotaMetrics>(source.text); }
            catch (ArgumentException) { return; }
            if (metrics == null || metrics.origin != "top-left" || metrics.fields == null) return;
            var size = metrics.size != null && metrics.size.Length == 2 ? metrics.size : metrics.atlasSize;
            if (size == null || size.Length != 2 || size[0] != atlas.width || size[1] != atlas.height) return;
            foreach (var field in metrics.fields)
            {
                if (field == null || field.value < 0 || field.value > 99 || field.rect == null || field.rect.Length != 4) continue;
                int x = field.rect[0], y = field.rect[1], width = field.rect[2], height = field.rect[3];
                if (x < 0 || y < 0 || width != fieldSize.x || height != fieldSize.y
                    || x > atlas.width - width || y > atlas.height - height) continue;
                fields[field.value] = new Rect((float)x / atlas.width, 1f - (float)(y + height) / atlas.height,
                    (float)width / atlas.width, (float)height / atlas.height);
            }
        }

        [Serializable]
        private sealed class QuotaMetrics
        {
            public string origin = string.Empty;
            public int[] size = Array.Empty<int>();
            public int[] atlasSize = Array.Empty<int>();
            public QuotaEntry[] fields = Array.Empty<QuotaEntry>();
        }

        [Serializable]
        private sealed class QuotaEntry
        {
            public int value = -1;
            public int[] rect = Array.Empty<int>();
        }
    }
}
