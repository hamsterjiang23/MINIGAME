using _Code.DialogSystem.Commands;

namespace _Code.DialogSystem
{
	public interface IDialogViewProvider
	{
		DialogView DialogView { get; }

		DialogCommandsInstance CommandsInstance { get; }

		SubtitlesView OverlaySubtitlesView { get; }
	}
}
