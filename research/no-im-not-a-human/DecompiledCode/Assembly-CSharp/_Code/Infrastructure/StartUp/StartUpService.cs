using UnityEngine;
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

namespace _Code.Infrastructure.StartUp;

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
		_updaterService = updaterService;
		_gameEventsManager = gameEventsManager;
		_dataModelService = dataModelService;
		_cursorController = cursorController;
		_watcherManager = watcherManager;
		updaterService.AddUpdatable(notepadController.Updatable);
		updaterService.AddUpdatable(gameEventsManager.Updateable);
		updaterService.AddUpdatable(pauseController.Updateable);
		updaterService.AddUpdatable(playerService.Updateable);
		IUpdateable[] actionableObjectsUpdates = actionableObjectsManager.ActionableObjectsUpdates;
		foreach (IUpdateable updateable in actionableObjectsUpdates)
		{
			updaterService.AddUpdatable(updateable);
		}
		actionableObjectsUpdates = closeUpsController.Updateables;
		foreach (IUpdateable updateable2 in actionableObjectsUpdates)
		{
			updaterService.AddUpdatable(updateable2);
		}
		actionableObjectsUpdates = interactablesManager.Updateables;
		foreach (IUpdateable updateable3 in actionableObjectsUpdates)
		{
			updaterService.AddUpdatable(updateable3);
		}
		_inputHandler = inputHandlerProvider.InputHandler;
		_updaterService.Run();
		PrepareThings();
	}

	private void PrepareThings()
	{
		Material obj = Resources.Load("Window") as Material;
		obj.SetColor("_EmissionColor", new Color(0.008f, 0.008f, 0.008f));
		obj.color = Color.black;
	}

	public void Initialize()
	{
		_dataModelService.GameSaveDataHandler.LoadIfNeeded();
		_cursorController.SetThrough(isThrough: false);
		_watcherManager.EnterState(EWatcherState.World3d);
		_watcherManager.ChangeInput(_inputHandler.CurrentDevice);
		_inputHandler.Init(_cursorController);
		_inputHandler.InputDeviceChanged -= OnInputChanged;
		_inputHandler.InputDeviceChanged += OnInputChanged;
	}

	private void OnInputChanged(EInputDevice device)
	{
		_watcherManager.ChangeInput(device);
	}
}
