using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MiniGame.UnityHost
{
    /// <summary>One audio-aware uGUI button with the same feedback for pointer and keyboard focus.</summary>
    public sealed class PauseMenuButton : MiniGameAudioButton
    {
        private Text label, shortcut, number;
        private CanvasGroup frame;
        private Image marker;
        private Color ink;
        private float frameTarget;
        private bool configured;

        public void ConfigureVisuals(Text mainLabel, Text shortcutLabel, CanvasGroup focusFrame, Image selectionMarker, Color textColor, Text commandNumber = null)
        {
            label = mainLabel; shortcut = shortcutLabel; frame = focusFrame; marker = selectionMarker; ink = textColor; number = commandNumber;
            transition = Transition.None;
            configured = true;
            DoStateTransition(currentSelectionState, true);
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.OnPointerEnter(eventData);
            if (IsActive() && IsInteractable() && EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(gameObject, eventData);
        }

        protected override void DoStateTransition(SelectionState state, bool instant)
        {
            if (!configured) return;
            bool disabled = state == SelectionState.Disabled;
            bool focused = state == SelectionState.Highlighted || state == SelectionState.Selected || state == SelectionState.Pressed;
            frameTarget = focused && !disabled ? state == SelectionState.Pressed ? .80f : 1 : 0;
            Color text = focused && !disabled ? TerminalView.Desk : ink;
            label.color = new Color(text.r, text.g, text.b, disabled ? .30f : 1);
            shortcut.color = new Color(text.r, text.g, text.b, disabled ? .20f : focused ? 1 : .62f);
            if (number != null) number.color = new Color(text.r, text.g, text.b, disabled ? .20f : focused ? 1 : .62f);
            if (instant) frame.alpha = frameTarget;
            marker.color = new Color(text.r, text.g, text.b, frameTarget > 0 ? 1 : 0);
        }

        private void Update()
        {
            if (frame != null) frame.alpha = Mathf.MoveTowards(frame.alpha, frameTarget, Time.unscaledDeltaTime / .075f);
        }
    }
}
