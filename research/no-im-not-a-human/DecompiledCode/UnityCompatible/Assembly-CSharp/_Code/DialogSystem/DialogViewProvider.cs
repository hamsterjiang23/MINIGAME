using UnityEngine;
using _Code.DialogSystem.Commands;

namespace _Code.DialogSystem
{
	public sealed class DialogViewProvider : MonoBehaviour, IDialogViewProvider
	{
		[field: SerializeField]
		public DialogView DialogView { get; private set; }

		[field: SerializeField]
		public DialogCommandsInstance CommandsInstance { get; private set; }

		[field: SerializeField]
		public SubtitlesView OverlaySubtitlesView { get; private set; }
	}
}
