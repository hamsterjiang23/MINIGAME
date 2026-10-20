using UnityEngine;
using UnityEngine.UI;

namespace MiniGame.UnityHost
{
    /// <summary>One vanishing point for all menu glyphs, rules and focus frames.</summary>
    public sealed class PauseMenuPerspective : BaseMeshEffect
    {
        private RectTransform column;

        public void Configure(RectTransform value) { column = value; graphic.SetVerticesDirty(); }

        public static Vector2 Project(Vector2 point)
        {
            float depth = 1 + point.x * .00032f;
            return new Vector2(point.x / depth, (point.y + point.x * .045f) / depth);
        }

        public static Vector2 Unproject(Vector2 point)
        {
            float depth = 1 / (1 - point.x * .00032f);
            float x = point.x * depth;
            return new Vector2(x, point.y * depth - x * .045f);
        }

        public override void ModifyMesh(VertexHelper vertices)
        {
            if (!IsActive() || column == null) return;
            var vertex = new UIVertex();
            for (int i = 0; i < vertices.currentVertCount; i++)
            {
                vertices.PopulateUIVertex(ref vertex, i);
                Vector3 local = column.InverseTransformPoint(transform.TransformPoint(vertex.position));
                Vector2 projected = Project(local);
                vertex.position = transform.InverseTransformPoint(column.TransformPoint(new Vector3(projected.x, projected.y, local.z)));
                vertices.SetUIVertex(vertex, i);
            }
        }
    }
}
