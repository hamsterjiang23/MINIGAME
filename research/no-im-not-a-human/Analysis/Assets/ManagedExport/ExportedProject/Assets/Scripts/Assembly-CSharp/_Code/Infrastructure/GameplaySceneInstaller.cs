using UnityEngine;
using Zenject;
using _Code.Characters;
using _Code.DialogSystem;
using _Code.Infrastructure.ActionableObjects;
using _Code.Infrastructure.CloseUps;
using _Code.Infrastructure.CloseUps.Views;
using _Code.Infrastructure.Consumables;
using _Code.Infrastructure.Cutscenes;
using _Code.Infrastructure.DayNight;
using _Code.Infrastructure.Endings.Data;
using _Code.Infrastructure.Endings.Gameplay;
using _Code.Infrastructure.Endings.View;
using _Code.Infrastructure.GameEvents;
using _Code.Infrastructure.Localization;
using _Code.Infrastructure.Locations;
using _Code.Infrastructure.Notepad;
using _Code.Infrastructure.OtherGameData;
using _Code.Infrastructure.Pause;
using _Code.Infrastructure.Player;
using _Code.Infrastructure.Randomization;
using _Code.Infrastructure.Rooms;
using _Code.Infrastructure.Settings;
using _Code.Infrastructure.Sound;
using _Code.Infrastructure.StartUp;
using _Code.Infrastructure.StateObjects;
using _Code.Infrastructure.TriggerObjects;
using _Code.Infrastructure.UI;
using _Code.Infrastructure.Updatable;
using _Code.Infrastructure.ViewProvider;
using _Code.Infrastructure.Windows;
using _Code.Infrastructure._NINAH__Cat;
using _Code.Infrastructure._NINAH__CommonView;
using _Code.Infrastructure._NINAH__Dream;
using _Code.Infrastructure._NINAH__Effects;
using _Code.Infrastructure._NINAH__RandomSoundEmitters;
using _Code.Menues.HUD;
using _Code.Player;
using _Code.Utils.CustomYarnReading;
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
			base.Container.Bind<IViewProvider>().FromComponentInHierarchy().AsCached();
			base.Container.Bind<IDayNightControllerViewProvider>().FromComponentInHierarchy().AsCached();
			base.Container.Bind<IRoomsViewProvider>().FromComponentInHierarchy().AsCached();
			base.Container.Bind<IUpdaterInstanceProvider>().FromComponentInHierarchy().AsCached();
			base.Container.Bind<IWindowsViewProvider>().FromComponentInHierarchy().AsCached();
			base.Container.Bind<IActionableObjectsViewProvider>().FromComponentInHierarchy().AsCached();
			base.Container.Bind<ICloseUpsViewProvider>().FromComponentInHierarchy().AsCached();
			base.Container.Bind<INotepadViewProvider>().FromComponentInHierarchy().AsCached();
			base.Container.Bind<IDialogViewProvider>().FromComponentInHierarchy().AsCached();
			base.Container.Bind<IPlayerViewProvider>().FromComponentInHierarchy().AsCached();
			base.Container.Bind<IPauseMenuViewProvider>().FromComponentInHierarchy().AsCached();
			base.Container.Bind<IRoomDisplayerViewProvider>().FromComponentInHierarchy().AsCached();
			base.Container.Bind<ILocationsViewProvider>().FromComponentInHierarchy().AsCached();
			base.Container.Bind<IInteractablesViewProvider>().FromComponentInHierarchy().AsCached();
			base.Container.Bind<IStateObjectViewProvider>().FromComponentInHierarchy().AsCached();
			base.Container.Bind<ISoundServiceInstanceProvider>().FromComponentInHierarchy().AsCached();
			base.Container.Bind<IEndingViewProvider>().FromComponentInHierarchy().AsCached();
			base.Container.Bind<ICustomYarnReaderProvider>().FromComponentInHierarchy().AsCached();
			base.Container.Bind<IInputHandlerProvider>().FromComponentInHierarchy().AsCached();
			base.Container.Bind<ISettingsInstanceProvider>().FromComponentInHierarchy().AsCached();
			base.Container.Bind<ICatViewProvider>().FromComponentInHierarchy().AsCached();
			base.Container.Bind<ITriggerObjectsProvider>().FromComponentInHierarchy().AsCached();
			base.Container.Bind<ICutscenesDataProvider>().FromComponentInHierarchy().AsSingle();
			base.Container.Bind<ICommonViewProvider>().FromComponentInHierarchy().AsSingle();
			base.Container.Bind<IEffectsViewProvider>().FromComponentInHierarchy().AsSingle();
			base.Container.Bind<IDayNightControllerSODataProvider>().FromInstance(_resourceMother).AsSingle();
			base.Container.Bind<IGameEventsManagerSODataProvider>().FromInstance(_resourceMother).AsSingle();
			base.Container.Bind<IOtherGameSODataProvider>().FromInstance(_resourceMother).AsSingle();
			base.Container.Bind<ICharactersSODataProvider>().FromInstance(_resourceMother).AsSingle();
			base.Container.Bind<IRandomGenerationSettingsSODataProvider>().FromInstance(_resourceMother).AsSingle();
			base.Container.Bind<IWindowsSODataProvider>().FromInstance(_resourceMother).AsSingle();
			base.Container.Bind<IEndingDataProvider>().FromInstance(_resourceMother).AsSingle();
			base.Container.Bind<IEndingSODataProvider>().FromInstance(_resourceMother).AsSingle();
			base.Container.Bind<IRandomSoundsEmitterSODataViewProvider>().FromInstance(_resourceMother).AsSingle();
			base.Container.Bind<IMusicSOData>().FromInstance(_resourceMother).AsSingle();
			base.Container.BindInterfacesAndSelfTo<LocalizationManager>().AsSingle().NonLazy();
			base.Container.BindInterfacesAndSelfTo<NotAHumanSoundService>().AsSingle().NonLazy();
			base.Container.BindInterfacesAndSelfTo<GameplayRandomizer>().AsSingle().NonLazy();
			base.Container.BindInterfacesAndSelfTo<CharactersManager>().AsSingle().NonLazy();
			base.Container.BindInterfacesAndSelfTo<DialogManager>().AsSingle().NonLazy();
			base.Container.BindInterfacesAndSelfTo<DayNightController>().AsSingle().NonLazy();
			base.Container.BindInterfacesAndSelfTo<RoomsManager>().AsSingle().NonLazy();
			base.Container.BindInterfacesAndSelfTo<GameEventsManager>().AsSingle().NonLazy();
			base.Container.BindInterfacesAndSelfTo<HUDPresenter>().AsSingle().NonLazy();
			base.Container.BindInterfacesAndSelfTo<UpdaterService>().AsSingle().NonLazy();
			base.Container.BindInterfacesAndSelfTo<WindowsManager>().AsSingle().NonLazy();
			base.Container.BindInterfacesAndSelfTo<ConsumablesController>().AsSingle().NonLazy();
			base.Container.BindInterfacesAndSelfTo<CloseUpsController>().AsSingle().NonLazy();
			base.Container.BindInterfacesAndSelfTo<ActionableObjectsManager>().AsSingle().NonLazy();
			base.Container.BindInterfacesAndSelfTo<NotepadController>().AsSingle().NonLazy();
			base.Container.BindInterfacesAndSelfTo<PlayerService>().AsSingle().NonLazy();
			base.Container.BindInterfacesAndSelfTo<PauseController>().AsSingle().NonLazy();
			base.Container.BindInterfacesAndSelfTo<LocationsManager>().AsSingle().NonLazy();
			base.Container.BindInterfacesAndSelfTo<InteractablesManager>().AsSingle().NonLazy();
			base.Container.BindInterfacesAndSelfTo<StateObjectController>().AsSingle().NonLazy();
			base.Container.BindInterfacesAndSelfTo<UIInitializer>().AsSingle().NonLazy();
			base.Container.BindInterfacesAndSelfTo<EndingShower>().AsSingle().NonLazy();
			base.Container.BindInterfacesAndSelfTo<GameplayEndingManager>().AsSingle().NonLazy();
			base.Container.BindInterfacesAndSelfTo<CutscenesManager>().AsSingle().NonLazy();
			base.Container.BindInterfacesAndSelfTo<SettingsManager>().AsSingle().NonLazy();
			base.Container.BindInterfacesAndSelfTo<CatController>().AsSingle().NonLazy();
			base.Container.BindInterfacesAndSelfTo<TriggerObjectsController>().AsSingle().NonLazy();
			base.Container.BindInterfacesAndSelfTo<DreamController>().AsSingle().NonLazy();
			base.Container.BindInterfacesAndSelfTo<EffectsController>().AsSingle().NonLazy();
			base.Container.BindInterfacesAndSelfTo<RandomSoundsEmitterController>().AsSingle().NonLazy();
			base.Container.BindInterfacesAndSelfTo<StartUpService>().AsSingle().NonLazy();
		}
	}
}
