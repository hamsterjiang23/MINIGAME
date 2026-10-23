using UnityEngine.EventSystems;

namespace MiniGame.UnityHost
{
    /// <summary>The title controller is the sole owner of keyboard confirmation.</summary>
    public sealed class PixelStrengthSelectionButton : MiniGameAudioButton
    {
        public override void OnSubmit(BaseEventData eventData) => eventData?.Use();
    }
}
