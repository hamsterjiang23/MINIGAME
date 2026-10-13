using System;
using System.Runtime.CompilerServices;
using Zenject;
using _Code.DialogSystem;
using _Code.Infrastructure.Cursor;
using _Code.Infrastructure.Cutscenes;
using _Code.Infrastructure.DayNight;
using _Code.Infrastructure.Localization;
using _Code.Infrastructure.Locations;
using _Code.Infrastructure.Player;
using _Code.Infrastructure.Rooms;
using _Code.Infrastructure.Updatable;
using _Code.Infrastructure.Windows;
using _Code.Menues.HUD;
using _Code.Player;
using _Scripts.Services.DataModel;

namespace _Code.Infrastructure.Pause
{
	public sealed class PauseController : IUpdateable, IPauseController, ITickable
	{
		private readonly PauseMenuView _view;

		private readonly IPlayerService _playerService;

		private readonly ICursorController _cursorController;

		private readonly IDialogManager _dialogManager;

		private readonly IRoomsManager _roomsManager;

		private readonly ILocalizationManager _localizationManager;

		private readonly IHUDPresenter _hudPresenter;

		private readonly InputHandling _inputHandler;

		private readonly IWindowsManager _windowsManager;

		private readonly ICutscenesManager _cutscenesManager;

		private readonly ILocationsManager _locationsManager;

		[CompilerGenerated]
		private Action<bool> m_PauseStateChanged;

		public IUpdateable Updateable => null;

		public bool IsPaused { get; private set; }

		public event Action<bool> PauseStateChanged
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public PauseController(IPauseMenuViewProvider pauseMenuViewProvider, IPlayerService playerService, ICursorController cursorController, IDialogManager dialogManager, IRoomsManager roomsManager, ILocalizationManager localizationManager, IInputHandlerProvider inputHandlerProvider, IHUDPresenter hudPresenter, IWindowsManager windowsManager, IDataModelService dataModelService, IDayNightController dayNightController, ICutscenesManager cutscenesManager, ILocationsManager locationsManager, WatcherManager watcherManager)
		{
		}

		public void OnUpdateAction()
		{
		}

		public void Tick()
		{
		}

		private void SwitchPause()
		{
		}
	}
}
