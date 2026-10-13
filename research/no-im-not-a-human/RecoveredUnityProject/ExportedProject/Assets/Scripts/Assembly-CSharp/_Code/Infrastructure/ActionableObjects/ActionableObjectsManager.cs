using _Code.DialogSystem;
using _Code.Infrastructure.CloseUps;
using _Code.Infrastructure.Cursor;
using _Code.Infrastructure.Cutscenes;
using _Code.Infrastructure.DayNight;
using _Code.Infrastructure.GameEvents;
using _Code.Infrastructure.Pause;
using _Code.Infrastructure.Player;
using _Code.Infrastructure.Rooms;
using _Code.Infrastructure.Updatable;
using _Code.Menues.HUD;
using _Code.Player;
using _Scripts.Services.Sound.Service;

namespace _Code.Infrastructure.ActionableObjects
{
	public sealed class ActionableObjectsManager : IActionableObjectsManager
	{
		private readonly IActionableObjectView[] _actionableObjects;

		private readonly IHUDPresenter _hudPresenter;

		private readonly IDayNightController _dayNightController;

		private readonly IRoomsManager _roomsManager;

		private readonly IGameEventsManager _gameEventsManager;

		private readonly ICloseUpsController _closeUpsController;

		private readonly IPlayerService _playerService;

		private readonly ICursorController _cursorController;

		private readonly IPauseController _pauseController;

		private readonly INotAHumanSoundService _soundService;

		private readonly IInputHandlerProvider _inputHandlerProvider;

		private readonly IDialogManager _dialogManager;

		private readonly ICutscenesManager _cutscenesManager;

		private readonly WatcherManager _watcherManager;

		public IUpdateable[] ActionableObjectsUpdates => (AActionableObjectView[])_actionableObjects;

		public ActionableObjectsManager(IActionableObjectsViewProvider actionableObjectsViewProvider, IHUDPresenter hudPresenter, IDayNightController dayNightController, IGameEventsManager gameEventsManager, IRoomsManager roomsManager, ICloseUpsController closeUpsController, IPlayerService playerService, ICursorController cursorController, IPauseController pauseController, INotAHumanSoundService soundService, IInputHandlerProvider inputHandlerProvider, IDialogManager dialogManager, ICutscenesManager cutscenesManager, WatcherManager watcherManager)
		{
			_hudPresenter = hudPresenter;
			_dayNightController = dayNightController;
			_gameEventsManager = gameEventsManager;
			_roomsManager = roomsManager;
			_closeUpsController = closeUpsController;
			_playerService = playerService;
			IActionableObjectView[] actionableObjectViews = actionableObjectsViewProvider.ActionableObjectViews;
			_actionableObjects = actionableObjectViews;
			_cursorController = cursorController;
			_pauseController = pauseController;
			_soundService = soundService;
			_inputHandlerProvider = inputHandlerProvider;
			_dialogManager = dialogManager;
			_cutscenesManager = cutscenesManager;
			_watcherManager = watcherManager;
			Init();
		}

		public void ForceLeave()
		{
			IActionableObjectView[] actionableObjects = _actionableObjects;
			for (int i = 0; i < actionableObjects.Length; i++)
			{
				actionableObjects[i].TryLeave();
			}
		}

		public void SetLockedStateForAll(bool isLocked)
		{
			IActionableObjectView[] actionableObjects = _actionableObjects;
			for (int i = 0; i < actionableObjects.Length; i++)
			{
				actionableObjects[i].SetLockedState(isLocked);
			}
		}

		private void Init()
		{
			IActionableObjectView[] actionableObjects = _actionableObjects;
			foreach (IActionableObjectView obj in actionableObjects)
			{
				obj.Init(_hudPresenter, _dayNightController, _roomsManager, _gameEventsManager, _closeUpsController, _playerService, _cursorController, _pauseController, _soundService, _inputHandlerProvider, _watcherManager);
				obj.InitModules(_dialogManager);
			}
			_dialogManager.CutsceneTriggered -= ForceLeave;
			_dialogManager.CutsceneTriggered += ForceLeave;
			_cutscenesManager.CutsceneStarted -= LockObjects;
			_cutscenesManager.CutsceneStarted += LockObjects;
		}

		private void LockObjects(ECutscene obj)
		{
			IActionableObjectView[] actionableObjects = _actionableObjects;
			for (int i = 0; i < actionableObjects.Length; i++)
			{
				actionableObjects[i].SetLockedState(isLocked: true);
			}
		}
	}
}
