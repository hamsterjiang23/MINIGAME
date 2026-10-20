using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace MiniGame.UnityHost
{
    /// <summary>Presentation-only, object-anchored reading. Never changes the game session.</summary>
    public sealed class PropNarrationView : MonoBehaviour
    {
        private const int Layer = 31;
        private const float FadeSeconds = .24f;
        private Camera viewCamera, textCamera;
        private PropNarrationComposite composite;
        private RectTransform panel;
        private CanvasGroup group;
        private Text body;
        private Material textMaterial, backdropMaterial, compositeMaterial;
        private ReadableProp prop;
        private string[] pages = Array.Empty<string>();
        private int page, previousLayerBit, openedFrame;
        private float opacity, settleTime;
        private bool closing, paused;
        private Vector3 anchorOffset;

        public bool IsOpen { get; private set; }
        public bool Visible => IsOpen && !paused;
        public bool BlocksWorldInput => IsOpen || ClosedFrame == Time.frameCount;
        public int ClosedFrame { get; private set; } = -1;
        public string CurrentText => IsOpen && page < pages.Length ? pages[page] : string.Empty;
        public Vector3 WorldPosition => panel != null ? panel.position : Vector3.zero;
        public float Opacity => opacity;
        public int PageCount => pages.Length;
        public Camera ViewCamera => viewCamera;

        public bool Show(ReadableProp source, Camera camera, Font font)
        {
            if (IsOpen || source == null || !source.isActiveAndEnabled || source.Surface == null || camera == null) return false;
            var nextPages = source.Caption.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(line => line.Trim()).Where(line => line.Length > 0).ToArray();
            if (nextPages.Length == 0) return false;
            var shader = Resources.Load<Shader>("PropNarration");
            var compositor = Resources.Load<Shader>("PropNarrationComposite");
            if (shader == null || !shader.isSupported || compositor == null || !compositor.isSupported) return false;
            if (panel == null) Build(font, shader, compositor);
            viewCamera = camera;
            prop = source;
            pages = nextPages; page = 0;
            previousLayerBit = camera.cullingMask & (1 << Layer);
            camera.cullingMask &= ~(1 << Layer);
            composite = camera.GetComponent<PropNarrationComposite>();
            if (composite == null) composite = camera.gameObject.AddComponent<PropNarrationComposite>();
            composite.Owner = this; composite.enabled = true;
            IsOpen = true; closing = false; paused = false; opacity = 0; settleTime = 0;
            openedFrame = Time.frameCount;
            group.alpha = 0;
            PlaceBesideProp();
            body.rectTransform.offsetMin = body.rectTransform.offsetMax = Vector2.zero;
            body.rectTransform.ForceUpdateRectTransforms();
            BalanceShortParagraphs(nextPages);
            pages = nextPages.SelectMany(paragraph => DialogueTextPlayback.Paginate(body, paragraph)).ToArray();
            panel.gameObject.SetActive(true);
            SetPage();
            source.Highlight(false);
            return true;
        }

        public bool Advance()
        {
            if (!IsOpen || paused || closing || Time.frameCount == openedFrame || settleTime < .18f) return false;
            if (page + 1 < pages.Length)
            {
                page++;
                openedFrame = Time.frameCount; settleTime = 0;
                opacity = Mathf.Min(opacity, .22f);
                SetPage();
            }
            else closing = true;
            return true;
        }

        public bool Dismiss()
        {
            if (!IsOpen || paused) return false;
            closing = true;
            return true;
        }

        public void SetPaused(bool value)
        {
            paused = value;
            if (panel != null) panel.gameObject.SetActive(IsOpen && !paused);
        }

        public void CloseImmediately()
        {
            if (IsOpen) ClosedFrame = Time.frameCount;
            IsOpen = false; closing = false; opacity = 0;
            if (panel != null) panel.gameObject.SetActive(false);
            if (viewCamera != null)
                viewCamera.cullingMask = (viewCamera.cullingMask & ~(1 << Layer)) | previousLayerBit;
            if (composite != null && composite.Owner == this) { composite.Owner = null; composite.enabled = false; }
            prop = null; viewCamera = null; pages = Array.Empty<string>();
        }

        private void Update()
        {
            if (!IsOpen) return;
            if (prop == null || !prop.isActiveAndEnabled || prop.Surface == null || !prop.Surface.enabled
                || !prop.Surface.gameObject.activeInHierarchy || viewCamera == null || !viewCamera.isActiveAndEnabled)
            { CloseImmediately(); return; }
            if (paused) return;
            settleTime += Time.unscaledDeltaTime;
            opacity = Mathf.MoveTowards(opacity, closing ? 0 : .86f, Time.unscaledDeltaTime / FadeSeconds);
            group.alpha = opacity;
            panel.position = prop.Surface.bounds.center + anchorOffset;
            if (closing && opacity <= 0) CloseImmediately();
        }

        private void PlaceBesideProp()
        {
            var bounds = prop.Surface.bounds;
            var center = viewCamera.WorldToViewportPoint(bounds.center);
            float depth = Mathf.Max(.35f, center.z - .16f);
            float visibleHeight = 2 * depth * Mathf.Tan(viewCamera.fieldOfView * .5f * Mathf.Deg2Rad);
            float visibleWidth = visibleHeight * viewCamera.aspect;
            float width = Mathf.Min(.96f, visibleWidth * .37f);
            float posterHalfWidth = Mathf.Abs(viewCamera.transform.right.x) * bounds.extents.x
                + Mathf.Abs(viewCamera.transform.right.y) * bounds.extents.y
                + Mathf.Abs(viewCamera.transform.right.z) * bounds.extents.z;
            float side = center.x <= .5f ? 1 : -1;
            float x = center.x + side * (posterHalfWidth + width * .5f + .075f) / visibleWidth;
            float halfViewportWidth = width / visibleWidth * .5f;
            x = Mathf.Clamp(x, .045f + halfViewportWidth, .955f - halfViewportWidth);
            var anchor = viewCamera.ViewportToWorldPoint(new Vector3(x, Mathf.Clamp(center.y, .30f, .76f), depth));
            panel.SetPositionAndRotation(anchor, viewCamera.transform.rotation);
            // Slight obliqueness gives the text an actual spatial plane while preserving legibility.
            panel.Rotate(0, -side * 7, 0, Space.Self);
            panel.localScale = Vector3.one * (width / 560f);
            anchorOffset = anchor - bounds.center;
        }

        private void SetPage()
        {
            body.text = pages[page];
        }

        private void BalanceShortParagraphs(string[] paragraphs)
        {
            float maximum = body.rectTransform.rect.width;
            var settings = body.GetGenerationSettings(Vector2.zero);
            using var generator = new TextGenerator();
            float longest = paragraphs.Max(text => generator.GetPreferredWidth(text, settings) / body.pixelsPerUnit);
            if (longest <= maximum || longest > maximum * 3) return;
            int lines = Mathf.CeilToInt(longest / maximum);
            float width = Mathf.Min(maximum, longest / lines + body.fontSize);
            body.rectTransform.offsetMax = new Vector2(width - maximum, 0);
            body.rectTransform.ForceUpdateRectTransforms();
        }

        private void Build(Font font, Shader shader, Shader compositor)
        {
            textMaterial = new Material(shader) { name = "Soft spatial narration", hideFlags = HideFlags.HideAndDontSave };
            backdropMaterial = new Material(shader) { name = "Feathered narration shadow", hideFlags = HideFlags.HideAndDontSave };
            backdropMaterial.SetFloat("_Backdrop", 1);
            compositeMaterial = new Material(compositor) { hideFlags = HideFlags.HideAndDontSave };
            var cameraObject = new GameObject("Prop narration rendering", typeof(Camera));
            cameraObject.transform.SetParent(transform, false);
            textCamera = cameraObject.GetComponent<Camera>(); textCamera.enabled = false;
            var root = new GameObject("Spatial prop narration", typeof(RectTransform), typeof(Canvas), typeof(CanvasGroup));
            root.layer = Layer; root.transform.SetParent(transform, false);
            panel = root.GetComponent<RectTransform>(); panel.sizeDelta = new Vector2(560, 230);
            var canvas = root.GetComponent<Canvas>(); canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = textCamera;
            group = root.GetComponent<CanvasGroup>(); group.interactable = false; group.blocksRaycasts = false;
            var backdrop = Rect("Atmosphere", panel, new Vector2(-.15f, -.18f), new Vector2(1.15f, 1.18f)).gameObject.AddComponent<Image>();
            backdrop.material = backdropMaterial; backdrop.color = new Color(.035f, .035f, .021f, .42f); backdrop.raycastTarget = false;
            body = MakeText("Narration", font, 34, new Vector2(.06f, .10f), new Vector2(.94f, .90f));
            body.color = new Color(.87f, .87f, .71f, 1);
            body.alignment = TextAnchor.MiddleLeft; body.lineSpacing = 1.24f;
            body.resizeTextForBestFit = false;
            FrameEdge("Frame top", Vector2.up, Vector2.one, new Vector2(0, -2), Vector2.zero);
            FrameEdge("Frame bottom", Vector2.zero, Vector2.right, Vector2.zero, new Vector2(0, 2));
            FrameEdge("Frame left", Vector2.zero, Vector2.up, Vector2.zero, new Vector2(2, 0));
            FrameEdge("Frame right", Vector2.right, Vector2.one, new Vector2(-2, 0), Vector2.zero);
            root.SetActive(false);
        }

        private void FrameEdge(string name, Vector2 min, Vector2 max, Vector2 offsetMin, Vector2 offsetMax)
        {
            var edge = Rect(name, panel, min, max);
            edge.offsetMin = offsetMin; edge.offsetMax = offsetMax;
            var image = edge.gameObject.AddComponent<Image>();
            image.color = Color.white; image.raycastTarget = false;
        }

        private Text MakeText(string name, Font font, int size, Vector2 min, Vector2 max)
        {
            var label = Rect(name, panel, min, max).gameObject.AddComponent<Text>();
            label.font = font; label.fontSize = size; label.material = textMaterial;
            label.raycastTarget = false; label.supportRichText = false;
            label.horizontalOverflow = HorizontalWrapMode.Wrap; label.verticalOverflow = VerticalWrapMode.Truncate;
            return label;
        }

        private static RectTransform Rect(string name, Transform parent, Vector2 min, Vector2 max)
        {
            var item = new GameObject(name, typeof(RectTransform)); item.layer = Layer;
            item.transform.SetParent(parent, false);
            var rect = (RectTransform)item.transform; rect.anchorMin = min; rect.anchorMax = max;
            rect.offsetMin = rect.offsetMax = Vector2.zero; return rect;
        }

        // Render the world-space lettering after the room's coarse retro filter, including camera captures.
        public void Composite(RenderTexture source, RenderTexture destination)
        {
            if (!Visible || textCamera == null || viewCamera == null)
            { Graphics.Blit(source, destination); return; }
            var target = RenderTexture.GetTemporary(source.width, source.height, 24, RenderTextureFormat.ARGB32);
            var previous = RenderTexture.active;
            try
            {
                textCamera.CopyFrom(viewCamera);
                textCamera.enabled = false; textCamera.cullingMask = 1 << Layer;
                textCamera.clearFlags = CameraClearFlags.SolidColor; textCamera.backgroundColor = Color.clear;
                textCamera.allowHDR = false; textCamera.allowMSAA = false; textCamera.depthTextureMode = DepthTextureMode.None;
                textCamera.transform.SetPositionAndRotation(viewCamera.transform.position, viewCamera.transform.rotation);
                textCamera.targetTexture = target;
                textCamera.Render();
                compositeMaterial.SetTexture("_OverlayTex", target);
                Graphics.Blit(source, destination, compositeMaterial);
            }
            finally
            {
                textCamera.targetTexture = null; RenderTexture.active = previous;
                RenderTexture.ReleaseTemporary(target);
            }
        }

        private void OnDisable() => CloseImmediately();
        private void OnDestroy()
        {
            CloseImmediately();
            if (textMaterial != null) Destroy(textMaterial);
            if (backdropMaterial != null) Destroy(backdropMaterial);
            if (compositeMaterial != null) Destroy(compositeMaterial);
        }
    }
}
