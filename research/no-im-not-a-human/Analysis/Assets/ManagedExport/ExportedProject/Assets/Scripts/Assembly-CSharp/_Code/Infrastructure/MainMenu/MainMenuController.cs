using _Code.Infrastructure.Cursor;
using _Code.Infrastructure.Endings.View;
using _Code.Player;
using _Code.Utils.CustomYarnReading;
using _Scripts.Services.DataModel;
using _Scripts.Services.Sound.Service;

namespace _Code.Infrastructure.MainMenu
{
	public sealed class MainMenuController : IMainMenuController
	{
		private readonly MainMenuView _view;

		public MainMenuController(IMainMenuViewProvider viewProvider, IDataModelService dataModelService, IInputHandlerProvider inputHandlerProvider, ICursorController cursorController, IEndingDataProvider endingDataProvider, INotAHumanSoundService soundService, ICustomYarnReaderProvider customYarnReaderProvider, ICharactersSODataProvider charactersSoData, WatcherManager watcherManager)
		{
			_view = viewProvider.MainMenuView;
			_view.Init(dataModelService, inputHandlerProvider, cursorController, endingDataProvider, soundService, customYarnReaderProvider, charactersSoData, watcherManager);
		}
	}
}
