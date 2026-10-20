using UnityEngine;
using UnityEngine.UI;

namespace MiniGame.UnityHost
{
    /// <summary>Projects the hit bounds too, then rejects the empty corners of the trapezoid.</summary>
    public sealed class PauseMenuHitArea : Image
    {
        private RectTransform column, row;

        public void Configure(RectTransform menuColumn, RectTransform menuRow)
        {
            column = menuColumn; row = menuRow; color = Color.clear;
            var corners = new Vector3[4];
            row.GetWorldCorners(corners);
            Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
            Vector2 max = new Vector2(float.MinValue, float.MinValue);
            foreach (var corner in corners)
            {
                Vector2 projected = PauseMenuPerspective.Project(column.InverseTransformPoint(corner));
                Vector2 local = row.InverseTransformPoint(column.TransformPoint(projected));
                min = Vector2.Min(min, local); max = Vector2.Max(max, local);
            }
            rectTransform.anchorMin = rectTransform.anchorMax = row.pivot;
            rectTransform.pivot = Vector2.zero;
            rectTransform.anchoredPosition = min;
            rectTransform.sizeDelta = max - min;
        }

        public override bool Raycast(Vector2 screenPoint, Camera eventCamera)
        {
            if (!base.Raycast(screenPoint, eventCamera) || column == null || row == null) return false;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(column, screenPoint, eventCamera, out var point)) return false;
            Vector2 original = PauseMenuPerspective.Unproject(point);
            return row.rect.Contains(row.InverseTransformPoint(column.TransformPoint(original)));
        }
    }
}
