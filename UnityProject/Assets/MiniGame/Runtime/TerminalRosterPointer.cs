using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MiniGame.UnityHost
{
    public sealed class TerminalRosterPointer : MonoBehaviour, IPointerClickHandler
    {
        public Action DoubleClicked;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left && eventData.clickCount == 2)
                DoubleClicked?.Invoke();
        }
    }
}
