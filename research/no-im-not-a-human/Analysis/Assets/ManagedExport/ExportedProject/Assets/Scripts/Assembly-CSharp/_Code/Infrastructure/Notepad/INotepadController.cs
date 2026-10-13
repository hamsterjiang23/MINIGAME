using _Code.Infrastructure.Updatable;

namespace _Code.Infrastructure.Notepad
{
	public interface INotepadController
	{
		IUpdateable Updatable { get; }

		void Open();

		void Close();
	}
}
