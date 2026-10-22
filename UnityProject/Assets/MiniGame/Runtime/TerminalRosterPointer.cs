using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MiniGame.UnityHost
{
    public sealed class TerminalRosterPointer : MonoBehaviour, IPointerEnterHandler, IPointerMoveHandler
    {
        public Action Hovered;

        public void OnPointerEnter(PointerEventData eventData) => Hovered?.Invoke();
        public void OnPointerMove(PointerEventData eventData)
        { if (eventData.delta.sqrMagnitude > 0) Hovered?.Invoke(); }
    }
}
