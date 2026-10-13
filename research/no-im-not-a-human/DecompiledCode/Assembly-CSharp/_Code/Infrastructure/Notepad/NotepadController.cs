using _Code.Infrastructure.Updatable;

namespace _Code.Infrastructure.Notepad;

public sealed class NotepadController : INotepadController, IUpdateable
{
	private NotepadView _notepadView;

	private bool _isOpened;

	public IUpdateable Updatable => this;

	public NotepadController(INotepadViewProvider viewProvider)
	{
		_notepadView = viewProvider.NotepadView;
	}

	public void Open()
	{
		_notepadView.Open();
	}

	public void Close()
	{
		_notepadView.Close();
	}

	public void OnUpdateAction()
	{
	}
}
