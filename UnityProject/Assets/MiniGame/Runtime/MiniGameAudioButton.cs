using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MiniGame.UnityHost
{
    /// <summary>Plays one UI sound on an accepted activation, independently of replaceable click listeners.</summary>
    public class MiniGameAudioButton : Button
    {
        private int pendingManualFrame = -1;
        private int pendingSubmitFrame = -1;

        public override void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left || !IsActive() || !IsInteractable()) return;
            PlayActivationSound();
            base.OnPointerClick(eventData);
        }

        public override void OnSubmit(BaseEventData eventData)
        {
            if (!IsActive() || !IsInteractable()) return;
            // uGUI submit and the screen's raw keyboard handler can deliver the same key in either order.
            if (pendingManualFrame == Time.frameCount)
            {
                pendingManualFrame = -1;
                eventData?.Use();
                return;
            }
            pendingSubmitFrame = Time.frameCount;
            PlayActivationSound();
            base.OnSubmit(eventData);
        }

        public bool ActivateWithAudio()
        {
            if (!IsActive() || !IsInteractable()) return false;
            if (pendingSubmitFrame == Time.frameCount)
            {
                pendingSubmitFrame = -1;
                return true;
            }
            pendingManualFrame = Time.frameCount;
            PlayActivationSound();
            onClick.Invoke();
            return true;
        }

        internal static bool Activate(Button button)
            => button is MiniGameAudioButton audioButton && audioButton.ActivateWithAudio();

        private void PlayActivationSound()
            => GetComponentInParent<MiniGameScreen>()?.Audio?.PlayUIInteraction();
    }
}
