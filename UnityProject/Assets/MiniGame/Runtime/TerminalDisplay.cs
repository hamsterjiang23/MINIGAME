using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MiniGame.UnityHost
{
    /// <summary>Renders native UI through one CRT composite. Picking uses the same UV warp as the shader.</summary>
    [DefaultExecutionOrder(100)]
    public sealed class TerminalDisplay : MonoBehaviour
    {
        private const int SourceLayer = 27;
        private RectTransform viewport;
        private GameObject sourceRoot;
        private Canvas sourceCanvas;
        private Camera sourceCamera;
        private RawImage output;
        private RenderTexture texture;
        private RenderTexture frozenOutput;
        private Material material;
        private RectTransform logical;
        private float zoom;
        private float openingProgress = 1;
        private bool fitted;
        private Vector2 fittedViewport;
        private float fittedZoom, fittedOpening;
        private int layerHierarchyCount = -1;
        private bool titlePullback;
        private Vector3 titleCameraPosition;
        private float titleCameraSize;
        private readonly Vector3[] projectedCorners = new Vector3[4];
        public float OpeningProgress => openingProgress;
        public int Mode { get; private set; } = 1;
        public RenderTexture Texture => frozenOutput != null ? frozenOutput : texture;
        public bool OutputFrozen => frozenOutput != null;
        public int RenderedFrameCount { get; private set; }
        public Material ScreenMaterial => material;
        public RectTransform OutputRect => output != null ? output.rectTransform : null;
        internal TerminalSourceRaycaster SourceRaycaster { get; private set; }

        public void Initialize(RectTransform content)
        {
            viewport = (RectTransform)transform; logical = content;
            sourceRoot = new GameObject("Terminal native UI source", typeof(RectTransform), typeof(Canvas), typeof(TerminalSourceRaycaster));
            sourceRoot.hideFlags = HideFlags.DontSave;
            sourceRoot.transform.SetParent(GetComponentInParent<MiniGameScreen>().transform, false);
            sourceRoot.transform.SetPositionAndRotation(new Vector3(20000, 20000, 0), Quaternion.identity);
            sourceRoot.transform.localScale = Vector3.one;
            var rect = (RectTransform)sourceRoot.transform; rect.sizeDelta = new Vector2(800, 600);
            sourceCanvas = sourceRoot.GetComponent<Canvas>(); sourceCanvas.renderMode = RenderMode.WorldSpace;
            sourceCanvas.pixelPerfect = true;
            content.SetParent(rect, false); content.anchorMin = content.anchorMax = content.pivot = new Vector2(.5f, .5f); content.anchoredPosition = Vector2.zero;
            var cameraObject = new GameObject("Terminal UI camera", typeof(Camera)); cameraObject.transform.SetParent(sourceRoot.transform, false);
            cameraObject.transform.localPosition = new Vector3(0, 0, -1000);
            sourceCamera = cameraObject.GetComponent<Camera>(); sourceCamera.enabled = false; sourceCamera.orthographic = true;
            sourceCamera.orthographicSize = 300; sourceCamera.aspect = 4f / 3; sourceCamera.nearClipPlane = 1; sourceCamera.farClipPlane = 2000;
            sourceCamera.clearFlags = CameraClearFlags.SolidColor; sourceCamera.backgroundColor = TerminalView.Desk; sourceCamera.cullingMask = 1 << SourceLayer;
            texture = new RenderTexture(800, 600, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear)
                { name = "Terminal HTML UI 800x600", filterMode = FilterMode.Point, antiAliasing = 1, hideFlags = HideFlags.DontSave };
            texture.Create(); sourceCamera.targetTexture = texture; sourceCanvas.worldCamera = sourceCamera;
            SourceRaycaster = sourceRoot.GetComponent<TerminalSourceRaycaster>(); SourceRaycaster.ignoreReversedGraphics = false;
            var outputObject = new GameObject("Amber CRT display", typeof(RectTransform), typeof(RawImage), typeof(TerminalDisplayRaycaster)); outputObject.transform.SetParent(transform, false);
            output = outputObject.GetComponent<RawImage>(); output.texture = texture; output.raycastTarget = false;
            var shader = Resources.Load<TerminalAssetSet>("MiniGame/TerminalAssets")?.DisplayShader;
            if (shader == null || !shader.isSupported) throw new System.InvalidOperationException("TerminalComposite shader is unavailable.");
            material = new Material(shader) { hideFlags = HideFlags.HideAndDontSave }; output.material = material;
            outputObject.GetComponent<TerminalDisplayRaycaster>().Display = this;
            SetMode(1);
            Fit(); RenderFrame();
        }
        public void SetMode(int value) { Mode = Mathf.Clamp(value, 0, 2); if (material != null) material.SetFloat("_Mode", Mode); }
        public void SetOutputVisible(bool visible) { if (output != null) output.gameObject.SetActive(visible); }
        public void FreezeOutput()
        {
            if (OutputFrozen || texture == null) return;
            RenderFrame();
            frozenOutput = new RenderTexture(texture.descriptor) { name = "Self review handoff frame", filterMode = texture.filterMode };
            frozenOutput.Create();
            Graphics.Blit(texture, frozenOutput);
            output.texture = frozenOutput;
        }
        public void ReleaseFrozenOutput()
        {
            if (frozenOutput == null) return;
            if (output != null) output.texture = texture;
            frozenOutput.Release(); Destroy(frozenOutput); frozenOutput = null;
        }
        public void SetZoom(float value) { zoom = Mathf.Max(0, value); Fit(); }
        internal void InvalidateSourceLayers() => layerHierarchyCount = -1;
        /// <summary>Pulls back from the native 464x261 monitor to the complete terminal.</summary>
        public void SetOpeningProgress(float value) { openingProgress = Mathf.Clamp01(value); Fit(); }
        public void BeginTitlePullback(Rect logoScreenRect, RectTransform nativeLogo)
        {
            titlePullback = true; openingProgress = 0; fitted = false;
            float aspect = Mathf.Max(.01f, viewport.rect.width / Mathf.Max(1, viewport.rect.height));
            nativeLogo.GetWorldCorners(projectedCorners);
            float logoHeight = Vector3.Distance(projectedCorners[0], projectedCorners[1]);
            titleCameraSize = logoHeight * Screen.height / Mathf.Max(1, logoScreenRect.height) * .5f;
            var center = sourceRoot.transform.InverseTransformPoint(nativeLogo.TransformPoint(nativeLogo.rect.center));
            var desired = new Vector2(logoScreenRect.center.x / Screen.width, logoScreenRect.center.y / Screen.height);
            titleCameraPosition = new Vector3(center.x - (desired.x - .5f) * titleCameraSize * 2 * aspect,
                center.y - (desired.y - .5f) * titleCameraSize * 2, -1000);
            // Match the existing title through the CRT's same inverse UV mapping; do not move the title to a second logo.
            for (int i = 0; i < 8; i++)
            {
                fitted = false; Fit();
                var projected = ScreenRectFor(nativeLogo);
                titleCameraSize *= projected.height / Mathf.Max(1, logoScreenRect.height);
                titleCameraPosition.x += (projected.center.x - logoScreenRect.center.x) / Screen.width * titleCameraSize * 2 * aspect;
                titleCameraPosition.y += (projected.center.y - logoScreenRect.center.y) / Screen.height * titleCameraSize * 2;
            }
            fitted = false; Fit();
        }
        public void EndTitlePullback(float restoredProgress = 1)
        {
            titlePullback = false; fitted = false; SetOpeningProgress(restoredProgress);
        }
        public void SetCollapse(float progress) { if (material != null) material.SetFloat("_Collapse", progress); }
        public void Fit()
        {
            if (output == null) return;
            Vector2 size = viewport.rect.size;
            if (fitted && size == fittedViewport && zoom == fittedZoom && openingProgress == fittedOpening) return;
            fitted = true; fittedViewport = size; fittedZoom = zoom; fittedOpening = openingProgress;
            float fit = Mathf.Max(.01f, Mathf.Min(size.x / 800, size.y / 600));
            float scale = zoom <= 0 ? fit : Mathf.Min(fit, zoom);
            var rect = output.rectTransform; rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(.5f, .5f); rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = Vector2.Lerp(viewport.rect.size, new Vector2(800, 600) * scale, openingProgress);
            sourceCamera.aspect = rect.sizeDelta.x / Mathf.Max(1, rect.sizeDelta.y);
            sourceCamera.orthographicSize = Mathf.Lerp(titlePullback ? titleCameraSize : Mathf.Min(130.5f, 232 / sourceCamera.aspect), 300, openingProgress);
            sourceCamera.transform.localPosition = Vector3.Lerp(titlePullback ? titleCameraPosition : new Vector3(-138, 77.5f, -1000), new Vector3(0, 0, -1000), openingProgress);
        }
        public static Vector2 DisplayToSource(Vector2 uv, int mode)
        {
            if (mode == 0) return uv;
            Vector2 q = uv * 2 - Vector2.one; float warp = (mode == 2 ? 1.012f : 1.006f) * (1 + (mode == 2 ? .018f : .008f) * q.sqrMagnitude);
            return (q * warp + Vector2.one) * .5f;
        }
        public Vector2 ScreenPointFor(RectTransform sourceRect)
        {
            if (!sourceRect.IsChildOf(logical))
            {
                var canvas = sourceRect.GetComponentInParent<Canvas>();
                return RectTransformUtility.WorldToScreenPoint(canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera, sourceRect.TransformPoint(sourceRect.rect.center));
            }
            Vector3 center = sourceCamera.WorldToViewportPoint(sourceRect.TransformPoint(sourceRect.rect.center));
            Vector2 wanted = new Vector2(center.x, center.y), uv = wanted;
            for (int i = 0; i < 12; i++) uv += wanted - DisplayToSource(uv, Mode);
            var r = output.rectTransform;
            return RectTransformUtility.WorldToScreenPoint(null, r.TransformPoint(new Vector3(r.rect.xMin + uv.x * r.rect.width, r.rect.yMin + uv.y * r.rect.height, 0)));
        }
        public Rect ScreenRectFor(RectTransform sourceRect)
        {
            sourceRect.GetWorldCorners(projectedCorners);
            var minimum = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
            var maximum = new Vector2(float.NegativeInfinity, float.NegativeInfinity);
            var rect = output.rectTransform;
            foreach (var corner in projectedCorners)
            {
                var projected = sourceCamera.WorldToViewportPoint(corner);
                Vector2 wanted = projected, uv = wanted;
                for (int i = 0; i < 12; i++) uv += wanted - DisplayToSource(uv, Mode);
                var point = RectTransformUtility.WorldToScreenPoint(null,
                    rect.TransformPoint(new Vector3(rect.rect.xMin + uv.x * rect.rect.width, rect.rect.yMin + uv.y * rect.rect.height, 0)));
                minimum = Vector2.Min(minimum, point); maximum = Vector2.Max(maximum, point);
            }
            return new Rect(minimum, maximum - minimum);
        }
        private void LateUpdate() { Fit(); RenderFrame(); }
        public void RenderFrame()
        {
            if (OutputFrozen || sourceCamera == null || sourceRoot == null || !sourceRoot.activeSelf) return;
            int hierarchyCount = sourceRoot.transform.hierarchyCount;
            if (hierarchyCount != layerHierarchyCount)
            {
                foreach (Transform child in sourceRoot.GetComponentsInChildren<Transform>(true)) child.gameObject.layer = SourceLayer;
                layerHierarchyCount = hierarchyCount;
            }
            // World-space canvases maintain layout and font atlases before this manually rendered camera.
            Canvas.ForceUpdateCanvases();
            var previous = RenderTexture.active;
            try { sourceCamera.Render(); RenderedFrameCount++; }
            finally { RenderTexture.active = previous; }
        }
        private void OnEnable() { if (sourceRoot != null) sourceRoot.SetActive(true); }
        private void OnDisable() { ReleaseFrozenOutput(); if (sourceRoot != null) sourceRoot.SetActive(false); }
        private void OnDestroy()
        {
            ReleaseFrozenOutput();
            if (sourceCamera != null) sourceCamera.targetTexture = null;
            if (output != null) output.texture = null;
            if (sourceRoot != null) Destroy(sourceRoot);
            if (texture != null) { texture.Release(); Destroy(texture); }
            if (material != null) Destroy(material);
        }
    }

    // Only the visible display participates in global picking. The source is queried with mapped pixels.
    internal sealed class TerminalSourceRaycaster : GraphicRaycaster
    {
        public override void Raycast(PointerEventData eventData, List<RaycastResult> results) { }
        public void RaycastMapped(PointerEventData eventData, List<RaycastResult> results) => base.Raycast(eventData, results);
    }

    internal sealed class TerminalDisplayRaycaster : BaseRaycaster
    {
        public TerminalDisplay Display;
        private readonly List<RaycastResult> mapped = new List<RaycastResult>();
        public override Camera eventCamera => GetComponentInParent<Canvas>()?.worldCamera;
        public override int sortOrderPriority => GetComponentInParent<Canvas>()?.sortingOrder ?? 10;
        public override int renderOrderPriority => GetComponentInParent<Canvas>()?.rootCanvas.renderOrder ?? 0;
        public override void Raycast(PointerEventData data, List<RaycastResult> results)
        {
            if (Display == null || Display.SourceRaycaster == null || !Display.isActiveAndEnabled) return;
            if (GetComponentInParent<MiniGameScreen>()?.TerminalPointerAllowed != true) return;
            var groups = GetComponentsInParent<CanvasGroup>();
            foreach (var group in groups) if (!group.blocksRaycasts || group.alpha < .999f) return;
            var rect = Display.OutputRect;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, data.position, eventCamera, out var local)) return;
            Vector2 uv = new Vector2((local.x - rect.rect.xMin) / rect.rect.width, (local.y - rect.rect.yMin) / rect.rect.height);
            if (uv.x < 0 || uv.x > 1 || uv.y < 0 || uv.y > 1) return;
            uv = TerminalDisplay.DisplayToSource(uv, Display.Mode);
            if (uv.x < 0 || uv.x > 1 || uv.y < 0 || uv.y > 1) return;
            var original = data.position;
            try
            {
                data.position = new Vector2(uv.x * Display.Texture.width, uv.y * Display.Texture.height);
                mapped.Clear(); Display.SourceRaycaster.RaycastMapped(data, mapped);
                foreach (var hit in mapped)
                {
                    var result = hit; result.module = this; result.screenPosition = original; result.distance = 0; result.sortingOrder = sortOrderPriority; result.index = results.Count;
                    results.Add(result);
                }
            }
            finally { data.position = original; }
        }
    }
}
