using System;
using UnityEngine;

namespace MiniGame.UnityHost
{
    /// <summary>Final player-camera brightness adjustment, independent of the cinematic retro bypass.</summary>
    [RequireComponent(typeof(Camera)), DisallowMultipleComponent]
    public sealed class PlayerBrightnessEffect : MonoBehaviour
    {
        private Material material;

        public static PlayerBrightnessEffect EnsureFor(Camera camera)
        {
            if (camera == null) throw new ArgumentNullException(nameof(camera));
            var effect = camera.GetComponent<PlayerBrightnessEffect>();
            return effect != null ? effect : camera.gameObject.AddComponent<PlayerBrightnessEffect>();
        }

        private void OnRenderImage(RenderTexture source, RenderTexture destination) => Render(source, destination);

        public void Render(RenderTexture source, RenderTexture destination)
        {
            float brightness = PlayerPreferences.Brightness;
            if (!enabled || Mathf.Approximately(brightness, 1))
            {
                Graphics.Blit(source, destination);
                return;
            }
            if (material == null)
            {
                var shader = Resources.Load<Shader>("PlayerBrightness");
                if (shader == null || !shader.isSupported) { Graphics.Blit(source, destination); return; }
                material = new Material(shader) { hideFlags = HideFlags.HideAndDontSave };
            }
            material.SetFloat("_Brightness", brightness);
            Graphics.Blit(source, destination, material);
        }

        private void OnDisable() => ReleaseMaterial();
        private void OnDestroy() => ReleaseMaterial();
        private void ReleaseMaterial()
        {
            if (material == null) return;
            if (Application.isPlaying) Destroy(material); else DestroyImmediate(material);
            material = null;
        }
    }
}
