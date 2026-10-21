using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace MiniGame.UnityHost
{
    /// <summary>Focused, camera-following reading with a visible page throughout the interaction. Never changes the game session.</summary>
    public sealed class PropNarrationView : MonoBehaviour
    {
        private const int Layer = 31;
        private const float FadeSeconds = .24f;
        private const float MaximumViewportWidth = .37f, MinimumViewportWidth = .24f, SourceGap = .015f;
        private const float RightViewportMargin = .015f;
        private const float MaximumTextWidth = 560, MaximumTextHeight = 360, TextPadding = 22;
        private const float ContinueHintHeight = 40;
        private Camera viewCamera, textCamera;
        private PropNarrationComposite composite;
        private RectTransform panel;
        private CanvasGroup group;
        private Text body, continueHint;
        private Material textMaterial, compositeMaterial;
        private ReadableProp prop;
        private string[] pages = Array.Empty<string>();
        private int page, previousLayerBit, openedFrame;
        private int lastAcceptedFrame = -1, blockedInputFrame = -1;
        private float opacity, settleTime;
        private bool closing, paused, hasSpace, placementChosen;
        private float viewportY, placementAspect;
        private UnityEngine.Rect readingViewport;

        public bool IsOpen { get; private set; }
        public bool Visible => IsOpen && !paused && hasSpace;
        public bool BlocksWorldInput => IsOpen || ClosedFrame == Time.frameCount;
        public int ClosedFrame { get; private set; } = -1;
        public string CurrentText => IsOpen && page < pages.Length ? pages[page] : string.Empty;
        public Vector3 WorldPosition => panel != null ? panel.position : Vector3.zero;
        public float Opacity => opacity;
        public int PageCount => pages.Length;
        public Camera ViewCamera => viewCamera;

        public bool Show(ReadableProp source, Camera camera, Font font, int day = 0)
        {
            if (IsOpen || ClosedFrame == Time.frameCount || blockedInputFrame == Time.frameCount
                || source == null || !source.isActiveAndEnabled || source.Surface == null || camera == null) return false;
            var nextPages = source.CaptionForDay(day).Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
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
            panel.sizeDelta = new Vector2(MaximumTextWidth, MaximumTextHeight);
            SetContinueHint(false);
            viewportY = Mathf.Clamp(camera.WorldToViewportPoint(source.Surface.bounds.center).y, .30f, .76f);
            body.rectTransform.ForceUpdateRectTransforms();
            pages = nextPages.SelectMany(paragraph => DialogueTextPlayback.Paginate(body, paragraph)).ToArray();
            panel.gameObject.SetActive(true);
            SetPage();
            source.Highlight(false);
            return true;
        }

        public bool Advance()
        {
            if (IsOpen && !paused) UpdatePlacement();
            if (!Visible || !CanAcceptInput() || settleTime < .18f) return false;
            lastAcceptedFrame = Time.frameCount;
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
            if (!CanAcceptInput()) return false;
            lastAcceptedFrame = Time.frameCount;
            closing = true;
            return true;
        }

        private bool CanAcceptInput() => IsOpen && !paused && !closing && Time.frameCount != openedFrame
            && Time.frameCount != lastAcceptedFrame && Time.frameCount != blockedInputFrame && Time.frameCount != ClosedFrame;

        public void SetPaused(bool value)
        {
            if (paused == value) return;
            paused = value;
            blockedInputFrame = Time.frameCount;
            if (panel != null) panel.gameObject.SetActive(IsOpen && !paused);
        }

        public void CloseImmediately()
        {
            if (IsOpen) ClosedFrame = blockedInputFrame = Time.frameCount;
            IsOpen = false; closing = false; hasSpace = false; placementChosen = false; opacity = 0;
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
            opacity = Mathf.MoveTowards(opacity, closing ? 0 : 1f, Time.unscaledDeltaTime / FadeSeconds);
            group.alpha = opacity;
            if (closing && opacity <= 0) CloseImmediately();
        }

        private void LateUpdate()
        {
            if (IsOpen && !paused) UpdatePlacement();
        }

        private void UpdatePlacement()
        {
            if (panel == null || viewCamera == null || prop == null || prop.Surface == null) return;
            if (!placementChosen || !Mathf.Approximately(placementAspect, viewCamera.aspect))
            {
                bool sourceOnScreen = TryGetSourceViewport(out var occupied);
                ChoosePlacement(sourceOnScreen, occupied);
            }
            // A chosen reading page stays visible even if its source subsequently crosses that rectangle.
            hasSpace = placementChosen;
            group.alpha = hasSpace ? opacity : 0;

            float depth = Mathf.Max(.6f, viewCamera.nearClipPlane + .1f);
            float visibleHeight = viewCamera.orthographic ? viewCamera.orthographicSize * 2
                : 2 * depth * Mathf.Tan(viewCamera.fieldOfView * .5f * Mathf.Deg2Rad);
            var anchor = viewCamera.ViewportToWorldPoint(new Vector3(readingViewport.center.x, readingViewport.center.y, depth));
            panel.SetPositionAndRotation(anchor, viewCamera.transform.rotation);
            panel.localScale = Vector3.one * (visibleHeight * viewCamera.aspect * readingViewport.width / panel.rect.width);
        }

        private void ChoosePlacement(bool sourceOnScreen, UnityEngine.Rect occupied)
        {
            float ratio = viewCamera.aspect * panel.rect.height / panel.rect.width;
            float width = Mathf.Min(MaximumViewportWidth * panel.rect.width / MaximumTextWidth, 1f / ratio);
            float minimumWidth = MinimumViewportWidth * panel.rect.width / MaximumTextWidth;
            float height = width * ratio;
            float y = Mathf.Clamp(viewportY, height * .5f, 1 - height * .5f);
            // Retain a valid camera-relative pose even when no readable gap exists on opening.
            readingViewport = new UnityEngine.Rect(1 - RightViewportMargin - width, y - height * .5f, width, height);
            placementChosen = true;
            placementAspect = viewCamera.aspect;
            if (sourceOnScreen)
            {
                float sideWidth = Mathf.Min(width, 1 - RightViewportMargin - occupied.xMax);
                if (sideWidth >= minimumWidth)
                {
                    width = sideWidth; height = width * ratio;
                    y = Mathf.Clamp(viewportY, height * .5f, 1 - height * .5f);
                }
                else
                {
                    float aboveWidth = Mathf.Min(width, Mathf.Clamp01(1 - occupied.yMax) / ratio);
                    float belowWidth = Mathf.Min(width, Mathf.Clamp01(occupied.yMin) / ratio);
                    bool above = aboveWidth > belowWidth || (Mathf.Approximately(aboveWidth, belowWidth) && viewportY >= occupied.center.y);
                    width = above ? aboveWidth : belowWidth;
                    height = width * ratio;
                    placementChosen = width >= minimumWidth;
                    y = above ? occupied.yMax + height * .5f : occupied.yMin - height * .5f;
                }
            }
            if (placementChosen)
                readingViewport = new UnityEngine.Rect(1 - RightViewportMargin - width, y - height * .5f, width, height);
            else
                // No side/upper/lower gap fits: keep the existing right-side rectangle instead of locking input behind invisible text.
                placementChosen = true;
        }

        private bool TryGetSourceViewport(out UnityEngine.Rect occupied)
        {
            var bounds = prop.Surface.bounds;
            var minimum = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
            var maximum = new Vector2(float.NegativeInfinity, float.NegativeInfinity);
            bool inFront = false, behind = false;
            for (int corner = 0; corner < 8; corner++)
            {
                var world = bounds.center + Vector3.Scale(bounds.extents, new Vector3(
                    (corner & 1) == 0 ? -1 : 1, (corner & 2) == 0 ? -1 : 1, (corner & 4) == 0 ? -1 : 1));
                var point = viewCamera.WorldToViewportPoint(world);
                if (point.z <= viewCamera.nearClipPlane) { behind = true; continue; }
                inFront = true;
                minimum = Vector2.Min(minimum, point); maximum = Vector2.Max(maximum, point);
            }
            occupied = new UnityEngine.Rect(0, 0, 1, 1);
            if (!inFront) return false;
            if (behind) return true;
            occupied = UnityEngine.Rect.MinMaxRect(minimum.x - SourceGap, minimum.y - SourceGap,
                maximum.x + SourceGap, maximum.y + SourceGap);
            return occupied.xMax > 0 && occupied.xMin < 1 && occupied.yMax > 0 && occupied.yMin < 1;
        }

        private void SetPage()
        {
            bool morePages = page + 1 < pages.Length;
            float hintHeight = morePages ? ContinueHintHeight : 0;
            SetContinueHint(morePages);
            panel.sizeDelta = new Vector2(MaximumTextWidth, MaximumTextHeight + hintHeight);
            body.text = pages[page];
            body.rectTransform.ForceUpdateRectTransforms();
            float preferredWidth = body.preferredWidth;
            // Balance short paragraphs so a final punctuation mark does not occupy a row by itself.
            using (var generator = new TextGenerator())
            {
                var settings = body.GetGenerationSettings(body.rectTransform.rect.size);
                settings.verticalOverflow = VerticalWrapMode.Overflow;
                generator.Populate(body.text, settings);
                if (generator.lineCount >= 2 && generator.lineCount <= 3)
                    preferredWidth = Mathf.Min(preferredWidth, preferredWidth / generator.lineCount + body.fontSize);
            }
            if (morePages) preferredWidth = Mathf.Max(preferredWidth, continueHint.preferredWidth);
            panel.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,
                Mathf.Clamp(preferredWidth + TextPadding * 2, TextPadding * 2 + body.fontSize, MaximumTextWidth));
            body.rectTransform.ForceUpdateRectTransforms();
            panel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,
                Mathf.Clamp(body.preferredHeight + TextPadding * 2 + hintHeight,
                    TextPadding * 2 + body.fontSize + hintHeight, MaximumTextHeight + hintHeight));
            placementChosen = false;
            UpdatePlacement();
        }

        private void SetContinueHint(bool visible)
        {
            continueHint.gameObject.SetActive(visible);
            body.rectTransform.offsetMin = new Vector2(TextPadding, TextPadding + (visible ? ContinueHintHeight : 0));
        }

        private void Build(Font font, Shader shader, Shader compositor)
        {
            textMaterial = new Material(shader) { name = "Soft spatial narration", hideFlags = HideFlags.HideAndDontSave };
            compositeMaterial = new Material(compositor) { hideFlags = HideFlags.HideAndDontSave };
            var cameraObject = new GameObject("Prop narration rendering", typeof(Camera));
            cameraObject.transform.SetParent(transform, false);
            textCamera = cameraObject.GetComponent<Camera>(); textCamera.enabled = false;
            var root = new GameObject("Spatial prop narration", typeof(RectTransform), typeof(Canvas), typeof(CanvasGroup));
            root.layer = Layer; root.transform.SetParent(transform, false);
            panel = root.GetComponent<RectTransform>(); panel.sizeDelta = new Vector2(MaximumTextWidth, MaximumTextHeight);
            var canvas = root.GetComponent<Canvas>(); canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = textCamera;
            group = root.GetComponent<CanvasGroup>(); group.interactable = false; group.blocksRaycasts = false;
            var backdrop = Rect("Reading background", panel, Vector2.zero, Vector2.one).gameObject.AddComponent<Image>();
            backdrop.color = Color.black; backdrop.raycastTarget = false;
            body = MakeText("Narration", font, 48, Vector2.zero, Vector2.one);
            body.color = Color.white;
            body.fontStyle = FontStyle.Bold;
            body.rectTransform.offsetMin = Vector2.one * TextPadding;
            body.rectTransform.offsetMax = -Vector2.one * TextPadding;
            body.alignment = TextAnchor.MiddleLeft; body.lineSpacing = 1.18f;
            body.resizeTextForBestFit = false;
            continueHint = MakeText("Continue reading", font, 22, Vector2.zero, new Vector2(1, 0));
            continueHint.text = "点击继续阅读";
            continueHint.color = Color.white;
            continueHint.alignment = TextAnchor.MiddleRight;
            continueHint.rectTransform.offsetMin = new Vector2(TextPadding, TextPadding);
            continueHint.rectTransform.offsetMax = new Vector2(-TextPadding, TextPadding + ContinueHintHeight);
            continueHint.gameObject.SetActive(false);
            root.SetActive(false);
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
            if (IsOpen && !paused) UpdatePlacement();
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
            if (compositeMaterial != null) Destroy(compositeMaterial);
        }
    }
}
