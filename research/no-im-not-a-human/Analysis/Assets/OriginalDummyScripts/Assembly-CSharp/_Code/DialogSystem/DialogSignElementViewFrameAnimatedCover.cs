using UnityEngine;
using _Code.Utils.UI.ImageAnimating;

namespace _Code.DialogSystem;

public class DialogSignElementViewFrameAnimatedCover : ADialogSignElementView
{
	[field: SerializeField]
	public AnimatedImage AnimatedCover { get; private set; }
}
