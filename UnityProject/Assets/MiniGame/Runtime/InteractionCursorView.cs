using System;
using UnityEngine;
using UnityEngine.UI;

namespace MiniGame.UnityHost
{
    public enum InteractionCursorKind { None, Read, Door, Sleep, Work }

    /// <summary>Uses the approved icon artwork as a mask, tinted with the existing reticle color.</summary>
    internal sealed class InteractionCursorView : IDisposable
    {
        private readonly RawImage image;
        private readonly Material material;
        private readonly Texture2D atlas;
        private readonly Texture2D mouse;
        public InteractionCursorKind Kind { get; private set; }
        public bool Visible => image != null && image.gameObject.activeInHierarchy;

        public InteractionCursorView(RectTransform parent, Color color)
        {
            var root = new GameObject("InteractionCursor", typeof(RectTransform), typeof(RawImage));
            root.transform.SetParent(parent, false);
            image = root.GetComponent<RawImage>();
            image.raycastTarget = false;
            image.color = color;
            image.rectTransform.anchorMin = image.rectTransform.anchorMax = new Vector2(.5f, .5f);
            image.rectTransform.anchoredPosition = Vector2.zero;
            atlas = Resources.Load<Texture2D>("MiniGame/InteractionCursorAtlas");
            mouse = Resources.Load<Texture2D>("MiniGame/InteractionCursorMouse");
            image.texture = atlas;
            var shader = Resources.Load<Shader>("InteractionCursorMask");
            if (atlas == null || mouse == null || shader == null || !shader.isSupported)
                Debug.LogError("Interaction cursor artwork or mask shader is unavailable.");
            else
            {
                material = new Material(shader) { hideFlags = HideFlags.HideAndDontSave };
                image.material = material;
            }
            root.SetActive(false);
        }

        public void SetKind(InteractionCursorKind kind)
        {
            if (Kind == kind) return;
            Kind = kind;
            if (kind == InteractionCursorKind.None) { SetVisible(false); return; }
            image.texture = kind == InteractionCursorKind.Work ? mouse : atlas;
            // Tight bounds from the approved four-icon sheet, in bottom-left pixel coordinates.
            Rect source;
            switch (kind)
            {
                case InteractionCursorKind.Read: source = new Rect(130, 794, 416, 229); break;
                case InteractionCursorKind.Door: source = new Rect(794, 754, 272, 330); break;
                case InteractionCursorKind.Sleep: source = new Rect(159, 195, 354, 297); break;
                default: source = new Rect(0, 0, mouse.width, mouse.height); break;
            }
            if (image.texture == null) return;
            image.uvRect = new Rect(source.x / image.texture.width, source.y / image.texture.height,
                source.width / image.texture.width, source.height / image.texture.height);
            image.rectTransform.sizeDelta = new Vector2(source.width, source.height) * (44 / Mathf.Max(source.width, source.height));
            if (material != null)
            {
                var uv = image.uvRect;
                var size = image.rectTransform.sizeDelta;
                material.SetVector("_AtlasRect", new Vector4(uv.x, uv.y, uv.width, uv.height));
                material.SetVector("_IconSize", new Vector4(size.x, size.y, 0, 0));
                // Keep the approved eye; coarsen the door and Zs, preserve the mouse's authored pixel grid.
                material.SetFloat("_PixelSize", kind == InteractionCursorKind.Read ? 1.5f
                    : kind == InteractionCursorKind.Work ? 44f / Mathf.Max(source.width, source.height) : 2.25f);
            }
        }

        public void SetVisible(bool visible) => image.gameObject.SetActive(visible && Kind != InteractionCursorKind.None);

        public void Dispose()
        {
            if (material != null) UnityEngine.Object.Destroy(material);
            if (image != null) UnityEngine.Object.Destroy(image.gameObject);
        }
    }
}
