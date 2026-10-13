using UnityEngine;
using Zenject;
using _Code.DialogSystem;
using _Code.Infrastructure.ActionableObjects;
using _Code.Infrastructure.CloseUps;
using _Code.Infrastructure.Cutscenes;
using _Code.Infrastructure.DayNight;
using _Code.Infrastructure.Endings.View;
using _Code.Infrastructure.Locations;
using _Code.Infrastructure.Notepad;
using _Code.Infrastructure.Pause;
using _Code.Infrastructure.Player;
using _Code.Infrastructure.Rooms;
using _Code.Infrastructure.Settings;
using _Code.Infrastructure.StateObjects;
using _Code.Infrastructure.TriggerObjects;
using _Code.Infrastructure.Updatable;
using _Code.Infrastructure.ViewProvider;
using _Code.Infrastructure.Windows;
using _Code.Infrastructure._NINAH__Cat;
using _Code.Player;
using _Scripts.Services.Sound.Service;

namespace _Code.Infrastructure
{
	public sealed class GameplaySceneInstaller : MonoInstaller
	{
		[SerializeField]
		private _Code.Infrastructure.ViewProvider.ViewProvider _viewProvider;

		[SerializeField]
		private DayNightControllerViewProvider _dayNightControllerViewProvider;

		[SerializeField]
		private RoomsViewProvider _roomsViewProvider;

		[SerializeField]
		private UpdaterInstanceProvider _updaterInstanceProvider;

		[SerializeField]
		private WindowsViewProvider _windowsViewProvider;

		[SerializeField]
		private ActionableObjectsViewProvider _actionableObjectsViewProvider;

		[SerializeField]
		private CloseUpsViewProvider _closeUpsViewProvider;

		[SerializeField]
		private NotepadViewProvider _notepadViewProvider;

		[SerializeField]
		private DialogViewProvider _dialogViewProvider;

		[SerializeField]
		private PlayerViewProvider _playerViewProvider;

		[SerializeField]
		private PauseMenuViewProvider _pauseMenuViewProvider;

		[SerializeField]
		private RoomDisplayerViewProvider _roomDisplayerViewProvider;

		[SerializeField]
		private LocationsViewProvider _locationsViewProvider;

		[SerializeField]
		private InteractablesViewProvider _interactablesViewProvider;

		[SerializeField]
		private StateObjectsViewProvider _stateObjectViewProvider;

		[SerializeField]
		private SoundServiceInstanceProvider _soundServiceInstanceProvider;

		[SerializeField]
		private EndingViewProvider _endingViewProvider;

		[SerializeField]
		private InputHandlerProvider _inputHandlerProvider;

		[SerializeField]
		private CutscenesDataProvider _cutscenesDataProvider;

		[SerializeField]
		private SettingsInstanceProvider _settingsInstanceProvider;

		[SerializeField]
		private CatViewProvider _catViewProvider;

		[SerializeField]
		private TriggerObjectsProvider _triggerObjectsProvider;

		[SerializeField]
		private ResourceMother _resourceMother;

		public override void InstallBindings()
		{
		}
	}
}
