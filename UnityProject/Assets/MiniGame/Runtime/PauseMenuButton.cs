using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MiniGame.UnityHost
{
    /// <summary>One audio-aware uGUI button with the same feedback for pointer and keyboard focus.</summary>
    public sealed class PauseMenuButton : MiniGameAudioButton
    {
        private Text label, shortcut;
        private CanvasGroup frame;
        private Image marker;
        private Color ink;
        private float frameTarget;
        private bool configured;

        public void ConfigureVisuals(Text mainLabel, Text shortcutLabel, CanvasGroup focusFrame, Image selectionMarker, Color textColor)
        {
            label = mainLabel; shortcut = shortcutLabel; frame = focusFrame; marker = selectionMarker; ink = textColor;
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
            frameTarget = focused && !disabled ? state == SelectionState.Pressed ? .74f : 1 : 0;
            label.color = new Color(ink.r, ink.g, ink.b, disabled ? .30f : focused ? 1 : .83f);
            shortcut.color = new Color(ink.r, ink.g, ink.b, disabled ? .20f : focused ? .72f : .40f);
            if (instant) frame.alpha = frameTarget;
            marker.color = new Color(.48f, .11f, .09f, frameTarget > 0 ? .95f : 0);
        }

        private void Update()
        {
            if (frame != null) frame.alpha = Mathf.MoveTowards(frame.alpha, frameTarget, Time.unscaledDeltaTime / .075f);
        }
    }
}
