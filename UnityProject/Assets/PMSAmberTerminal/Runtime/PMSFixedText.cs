using UnityEngine;
using UnityEngine.UI;

namespace PMSAmber
{
    /// <summary>Native uGUI text graphic with the prototype's fixed Chinese/Latin cell widths.</summary>
    [AddComponentMenu(""), RequireComponent(typeof(CanvasRenderer))]
    public sealed class PMSFixedText : MaskableGraphic
    {
        public Font Typeface;
        [TextArea] public string Content = "";
        public int PixelSize = 14;
        public int LinePitch = 20;
        public bool Wrap;
        public override Texture mainTexture => Typeface && Typeface.material ? Typeface.material.mainTexture : Texture2D.whiteTexture;
        protected override void OnEnable() { base.OnEnable(); Font.textureRebuilt += Rebuilt; }
        protected override void OnDisable() { Font.textureRebuilt -= Rebuilt; base.OnDisable(); }
        private void Rebuilt(Font font) { if (font == Typeface) { SetVerticesDirty(); SetMaterialDirty(); } }
        public static float Cell(char c, int size) => c > 255 ? size : Mathf.Ceil(size * .6f);
        public static float Width(string text, int size)
        {
            float result = 0; foreach (char c in text ?? "") result += Cell(c, size); return result;
        }
        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear(); if (!Typeface || string.IsNullOrEmpty(Content)) return;
            Typeface.RequestCharactersInTexture(Content, PixelSize, FontStyle.Normal);
            float x = 0, baseline = -PixelSize, max = rectTransform.rect.width;
            foreach (char ch in Content)
            {
                if (ch == '\r') continue;
                float cell = Cell(ch, PixelSize);
                if (ch == '\n' || (Wrap && x + cell > max && x > 0))
                { x = 0; baseline -= LinePitch; if (ch == '\n') continue; }
                CharacterInfo c;
                if (Typeface.GetCharacterInfo(ch, out c, PixelSize, FontStyle.Normal))
                {
                    int i = mesh.currentVertCount;
                    mesh.AddVert(new Vector3(x + c.minX, baseline + c.minY), color, c.uvBottomLeft);
                    mesh.AddVert(new Vector3(x + c.minX, baseline + c.maxY), color, c.uvTopLeft);
                    mesh.AddVert(new Vector3(x + c.maxX, baseline + c.maxY), color, c.uvTopRight);
                    mesh.AddVert(new Vector3(x + c.maxX, baseline + c.minY), color, c.uvBottomRight);
                    mesh.AddTriangle(i, i + 1, i + 2); mesh.AddTriangle(i, i + 2, i + 3);
                }
                x += cell;
            }
        }
    }
}
