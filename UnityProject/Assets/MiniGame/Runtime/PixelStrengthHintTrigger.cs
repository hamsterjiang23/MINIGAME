using UnityEngine;
using UnityEngine.EventSystems;

namespace MiniGame.UnityHost
{
    /// <summary>Only hovering shows the legend; selection never keeps it open.</summary>
    public sealed class PixelStrengthHintTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private PixelStrengthTooltip tooltip;
        private PixelDetail detail;
        private Vector2 position;

        public void Initialize(PixelStrengthTooltip value, PixelDetail pixelDetail, Vector2 anchoredPosition)
        {
            tooltip = value;
            detail = pixelDetail;
            position = anchoredPosition;
        }

        public void OnPointerEnter(PointerEventData eventData) => Show();
        public void OnPointerExit(PointerEventData eventData) => tooltip?.Hide(gameObject);
        private void OnDisable() => tooltip?.Hide(gameObject);
        private void Show()
        {
            if (isActiveAndEnabled) tooltip?.Show(gameObject, detail, position);
        }
    }
}
