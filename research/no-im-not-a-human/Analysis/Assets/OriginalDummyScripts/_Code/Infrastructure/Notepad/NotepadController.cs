using _Code.Infrastructure.Updatable;

namespace _Code.Infrastructure.Notepad
{
	public sealed class NotepadController : INotepadController, IUpdateable
	{
		private NotepadView _notepadView;

		private bool _isOpened;

		public IUpdateable Updatable => null;

		public NotepadController(INotepadViewProvider viewProvider)
		{
		}

		public void Open()
		{
		}

		public void Close()
		{
		}

		public void OnUpdateAction()
		{
		}
	}
}
