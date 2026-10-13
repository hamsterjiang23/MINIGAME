using UnityEngine;
using _Code.Utils.UI.ImageAnimating;

namespace _Code.DialogSystem
{
	public sealed class DialogSignViewAnimatedContent : ADialogSignElementView
	{
		[field: SerializeField]
		public AnimatedImage AnimatedContent { get; private set; }
	}
}
