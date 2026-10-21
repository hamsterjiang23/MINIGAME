using System.Collections.Generic;
using UnityEngine;

namespace MiniGame.UnityHost
{
    /// <summary>One terminal page shares the HTML canvas's single pointer highlight.</summary>
    internal sealed class TerminalHighlightGroup : MonoBehaviour
    {
        private readonly List<TerminalButtonVisual> buttons = new List<TerminalButtonVisual>();
        private TerminalButtonVisual pointer;

        internal bool HasPointer => pointer != null && pointer.IsActive() && pointer.IsInteractable();
        internal bool IsPointer(TerminalButtonVisual button) => HasPointer && pointer == button;

        internal void Register(TerminalButtonVisual button)
        {
            if (!buttons.Contains(button)) buttons.Add(button);
        }

        internal void Unregister(TerminalButtonVisual button)
        {
            buttons.Remove(button);
            ReleasePointer(button);
        }

        internal void SetPointer(TerminalButtonVisual button)
        {
            if (pointer == button) return;
            pointer?.ClearPointerState();
            pointer = button;
            PaintAll();
        }

        internal void ReleasePointer(TerminalButtonVisual button)
        {
            if (pointer != button) return;
            pointer = null;
            PaintAll();
        }

        /// <summary>Arrow-key navigation takes priority until the next pointer enter.</summary>
        public static void ClearPointerHighlight(Transform scope)
        {
            if (scope == null) return;
            var parent = scope.GetComponentInParent<TerminalHighlightGroup>();
            if (parent != null) parent.ClearPointer();
            foreach (var group in scope.GetComponentsInChildren<TerminalHighlightGroup>(true))
                if (group != parent) group.ClearPointer();
        }

        private void OnDisable() => ClearPointer();

        private void ClearPointer()
        {
            pointer?.ClearPointerState();
            pointer = null;
            PaintAll();
        }

        private void PaintAll()
        {
            foreach (var button in buttons) if (button != null) button.Paint();
        }
    }
}
