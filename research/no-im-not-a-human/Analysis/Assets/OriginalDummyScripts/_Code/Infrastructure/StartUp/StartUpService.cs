using Zenject;
using _Code.Infrastructure.ActionableObjects;
using _Code.Infrastructure.CloseUps;
using _Code.Infrastructure.Cursor;
using _Code.Infrastructure.GameEvents;
using _Code.Infrastructure.Notepad;
using _Code.Infrastructure.Pause;
using _Code.Infrastructure.Player;
using _Code.Infrastructure.Updatable;
using _Code.Player;
using _Scripts.Services.DataModel;

namespace _Code.Infrastructure.StartUp
{
	public sealed class StartUpService : IStartUpService, IInitializable
	{
		private readonly IUpdaterService _updaterService;

		private readonly IGameEventsManager _gameEventsManager;

		private readonly IDataModelService _dataModelService;

		private readonly ICursorController _cursorController;

		private readonly WatcherManager _watcherManager;

		private readonly InputHandling _inputHandler;

		public StartUpService(IUpdaterService updaterService, IActionableObjectsManager actionableObjectsManager, IGameEventsManager gameEventsManager, ICloseUpsController closeUpsController, INotepadController notepadController, IPauseController pauseController, IPlayerService playerService, IInteractablesManager interactablesManager, IDataModelService dataModelService, ICursorController cursorController, WatcherManager watcherManager, IInputHandlerProvider inputHandlerProvider)
		{
		}

		private void PrepareThings()
		{
		}

		public void Initialize()
		{
		}

		private void OnInputChanged(EInputDevice device)
		{
		}
	}
}
