using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace MiniGame.UnityHost
{
    /// <summary>Single capture per opening; UI stays sharp over a frozen, softly defocused scene.</summary>
    public sealed class PauseMenuBackdrop : MonoBehaviour
    {
        private RawImage image;
        private CanvasGroup group;
        private RenderTexture blurred;
        private Material material;
        private float captureStarted;
        private int captureWidth, captureHeight;
        public bool Capturing { get; private set; }

        public void Configure(RectTransform root, CanvasGroup fade)
        {
            group = fade;
            var go = new GameObject("PauseDefocusedScene", typeof(RectTransform), typeof(RawImage));
            go.transform.SetParent(root, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            image = go.GetComponent<RawImage>();
            image.raycastTarget = false;
            image.enabled = false;
            var shader = Resources.Load<Shader>("PauseBackdrop");
            if (shader != null && shader.isSupported)
                material = new Material(shader) { hideFlags = HideFlags.HideAndDontSave };
        }

        public void Capture()
        {
            if (Capturing || Application.isBatchMode || material == null) return;
            captureStarted = Time.realtimeSinceStartup;
            captureWidth = Screen.width; captureHeight = Screen.height;
            StartCoroutine(CaptureFrame());
        }

        private void Update()
        {
            // A hidden Game view may never reach end-of-frame. Keep the modal usable.
            if (Capturing)
            {
                if (Time.realtimeSinceStartup - captureStarted < .5f) return;
                StopAllCoroutines();
                Capturing = false;
            }
            else if (image != null && image.enabled &&
                (captureWidth != Screen.width || captureHeight != Screen.height)) Capture();
        }

        private IEnumerator CaptureFrame()
        {
            Capturing = true;
            group.alpha = 0;
            image.enabled = false;
            yield return new WaitForEndOfFrame();
            RenderTexture full = null, scratch = null;
            try
            {
                int width = Mathf.Max(1, Screen.width), height = Mathf.Max(1, Screen.height);
                int smallWidth = Mathf.Min(960, Mathf.Max(1, width / 2));
                int smallHeight = Mathf.Max(1, Mathf.RoundToInt((float)smallWidth * height / width));
                if (blurred == null || blurred.width != smallWidth || blurred.height != smallHeight)
                {
                    ReleaseTexture();
                    blurred = new RenderTexture(smallWidth, smallHeight, 0, RenderTextureFormat.ARGB32)
                        { name = "Pause scene soft focus", filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
                    blurred.Create();
                }
                full = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32);
                scratch = RenderTexture.GetTemporary(smallWidth, smallHeight, 0, RenderTextureFormat.ARGB32);
                full.filterMode = scratch.filterMode = FilterMode.Bilinear;
                full.wrapMode = scratch.wrapMode = TextureWrapMode.Clamp;
                ScreenCapture.CaptureScreenshotIntoRenderTexture(full);
                material.SetFloat("_Finish", 0);
                Graphics.Blit(full, scratch, material);
                Graphics.Blit(scratch, blurred, material);
                Graphics.Blit(blurred, scratch, material);
                material.SetFloat("_Finish", 1);
                Graphics.Blit(scratch, blurred, material);
                image.texture = blurred;
                image.enabled = true;
            }
            finally
            {
                if (full != null) RenderTexture.ReleaseTemporary(full);
                if (scratch != null) RenderTexture.ReleaseTemporary(scratch);
                Capturing = false;
            }
        }

        private void OnDisable() { StopAllCoroutines(); Capturing = false; if (image != null) image.enabled = false; }
        private void ReleaseTexture()
        {
            if (blurred == null) return;
            blurred.Release(); Destroy(blurred); blurred = null;
        }
        private void OnDestroy() { ReleaseTexture(); if (material != null) Destroy(material); }
    }
}
