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
        private Material material;
        private RectTransform logical;
        private float zoom;
        private float openingProgress = 1;
        private bool fitted;
        private Vector2 fittedViewport;
        private float fittedZoom, fittedOpening;
        private int layerHierarchyCount = -1;
        public float OpeningProgress => openingProgress;
        public int Mode { get; private set; } = 1;
        public RenderTexture Texture => texture;
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
            content.SetParent(rect, false); content.anchorMin = content.anchorMax = content.pivot = new Vector2(.5f, .5f); content.anchoredPosition = Vector2.zero;
            var cameraObject = new GameObject("Terminal UI camera", typeof(Camera)); cameraObject.transform.SetParent(sourceRoot.transform, false);
            cameraObject.transform.localPosition = new Vector3(0, 0, -1000);
            sourceCamera = cameraObject.GetComponent<Camera>(); sourceCamera.enabled = false; sourceCamera.orthographic = true;
            sourceCamera.orthographicSize = 300; sourceCamera.aspect = 4f / 3; sourceCamera.nearClipPlane = 1; sourceCamera.farClipPlane = 2000;
            sourceCamera.clearFlags = CameraClearFlags.SolidColor; sourceCamera.backgroundColor = TerminalView.Desk; sourceCamera.cullingMask = 1 << SourceLayer;
            texture = new RenderTexture(1600, 1200, 24, RenderTextureFormat.ARGB32) { name = "Terminal native UI 800x600 @2x", filterMode = FilterMode.Bilinear, hideFlags = HideFlags.DontSave };
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
        public void SetZoom(float value) { zoom = Mathf.Max(0, value); Fit(); }
        internal void InvalidateSourceLayers() => layerHierarchyCount = -1;
        /// <summary>Pulls back from the native 464x261 monitor to the complete terminal.</summary>
        public void SetOpeningProgress(float value) { openingProgress = Mathf.Clamp01(value); Fit(); }
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
            sourceCamera.orthographicSize = Mathf.Lerp(Mathf.Min(130.5f, 232 / sourceCamera.aspect), 300, openingProgress);
            sourceCamera.transform.localPosition = Vector3.Lerp(new Vector3(-138, 77.5f, -1000), new Vector3(0, 0, -1000), openingProgress);
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
        private void LateUpdate() { Fit(); RenderFrame(); }
        public void RenderFrame()
        {
            if (sourceCamera == null || sourceRoot == null || !sourceRoot.activeSelf) return;
            int hierarchyCount = sourceRoot.transform.hierarchyCount;
            if (hierarchyCount != layerHierarchyCount)
            {
                foreach (Transform child in sourceRoot.GetComponentsInChildren<Transform>(true)) child.gameObject.layer = SourceLayer;
                layerHierarchyCount = hierarchyCount;
            }
            // World-space canvases maintain layout and font atlases before this manually rendered camera.
            Canvas.ForceUpdateCanvases();
            var previous = RenderTexture.active;
            try { sourceCamera.Render(); }
            finally { RenderTexture.active = previous; }
        }
        private void OnEnable() { if (sourceRoot != null) sourceRoot.SetActive(true); }
        private void OnDisable() { if (sourceRoot != null) sourceRoot.SetActive(false); }
        private void OnDestroy()
        {
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
