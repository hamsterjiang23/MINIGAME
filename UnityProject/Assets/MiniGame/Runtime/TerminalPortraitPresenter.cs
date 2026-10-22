using UnityEngine;
using UnityEngine.UI;

namespace MiniGame.UnityHost
{
    internal sealed class TerminalPortraitPresenter : MonoBehaviour
    {
        private Image previous;
        private Image current;
        private float blend = 1;
        private bool visible = true;
        public bool Paused { get; set; }

        public void Initialize(Image before, Image after) { previous = before; current = after; }
        public void SetVisible(bool value) { visible = value; Paint(); }
        public void Show(Sprite sprite)
        {
            if (current.sprite == sprite) return;
            previous.sprite = current.sprite;
            previous.enabled = previous.sprite != null;
            current.sprite = sprite;
            current.enabled = sprite != null;
            blend = previous.sprite == null ? 1 : 0;
            Paint();
        }
        private void Update()
        {
            if (Paused || blend >= 1 || current == null) return;
            blend = Mathf.Min(1, blend + Time.unscaledDeltaTime / .12f);
            Paint();
        }
        private void Paint()
        {
            previous.color = new Color(1, 1, 1, 1 - blend);
            current.color = new Color(1, 1, 1, blend);
            previous.enabled = visible && previous.sprite != null && blend < 1;
            current.enabled = visible && current.sprite != null;
        }
    }
}
