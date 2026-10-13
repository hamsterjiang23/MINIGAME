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

namespace _Code.Infrastructure;

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
		Container.Bind<IViewProvider>().FromComponentInHierarchy().AsCached();
		Container.Bind<IDayNightControllerViewProvider>().FromComponentInHierarchy().AsCached();
		Container.Bind<IRoomsViewProvider>().FromComponentInHierarchy().AsCached();
		Container.Bind<IUpdaterInstanceProvider>().FromComponentInHierarchy().AsCached();
		Container.Bind<IWindowsViewProvider>().FromComponentInHierarchy().AsCached();
		Container.Bind<IActionableObjectsViewProvider>().FromComponentInHierarchy().AsCached();
		Container.Bind<ICloseUpsViewProvider>().FromComponentInHierarchy().AsCached();
		Container.Bind<INotepadViewProvider>().FromComponentInHierarchy().AsCached();
		Container.Bind<IDialogViewProvider>().FromComponentInHierarchy().AsCached();
		Container.Bind<IPlayerViewProvider>().FromComponentInHierarchy().AsCached();
		Container.Bind<IPauseMenuViewProvider>().FromComponentInHierarchy().AsCached();
		Container.Bind<IRoomDisplayerViewProvider>().FromComponentInHierarchy().AsCached();
		Container.Bind<ILocationsViewProvider>().FromComponentInHierarchy().AsCached();
		Container.Bind<IInteractablesViewProvider>().FromComponentInHierarchy().AsCached();
		Container.Bind<IStateObjectViewProvider>().FromComponentInHierarchy().AsCached();
		Container.Bind<ISoundServiceInstanceProvider>().FromComponentInHierarchy().AsCached();
		Container.Bind<IEndingViewProvider>().FromComponentInHierarchy().AsCached();
		Container.Bind<ICustomYarnReaderProvider>().FromComponentInHierarchy().AsCached();
		Container.Bind<IInputHandlerProvider>().FromComponentInHierarchy().AsCached();
		Container.Bind<ISettingsInstanceProvider>().FromComponentInHierarchy().AsCached();
		Container.Bind<ICatViewProvider>().FromComponentInHierarchy().AsCached();
		Container.Bind<ITriggerObjectsProvider>().FromComponentInHierarchy().AsCached();
		Container.Bind<ICutscenesDataProvider>().FromComponentInHierarchy().AsSingle();
		Container.Bind<ICommonViewProvider>().FromComponentInHierarchy().AsSingle();
		Container.Bind<IEffectsViewProvider>().FromComponentInHierarchy().AsSingle();
		Container.Bind<IDayNightControllerSODataProvider>().FromInstance(_resourceMother).AsSingle();
		Container.Bind<IGameEventsManagerSODataProvider>().FromInstance(_resourceMother).AsSingle();
		Container.Bind<IOtherGameSODataProvider>().FromInstance(_resourceMother).AsSingle();
		Container.Bind<ICharactersSODataProvider>().FromInstance(_resourceMother).AsSingle();
		Container.Bind<IRandomGenerationSettingsSODataProvider>().FromInstance(_resourceMother).AsSingle();
		Container.Bind<IWindowsSODataProvider>().FromInstance(_resourceMother).AsSingle();
		Container.Bind<IEndingDataProvider>().FromInstance(_resourceMother).AsSingle();
		Container.Bind<IEndingSODataProvider>().FromInstance(_resourceMother).AsSingle();
		Container.Bind<IRandomSoundsEmitterSODataViewProvider>().FromInstance(_resourceMother).AsSingle();
		Container.Bind<IMusicSOData>().FromInstance(_resourceMother).AsSingle();
		Container.BindInterfacesAndSelfTo<LocalizationManager>().AsSingle().NonLazy();
		Container.BindInterfacesAndSelfTo<NotAHumanSoundService>().AsSingle().NonLazy();
		Container.BindInterfacesAndSelfTo<GameplayRandomizer>().AsSingle().NonLazy();
		Container.BindInterfacesAndSelfTo<CharactersManager>().AsSingle().NonLazy();
		Container.BindInterfacesAndSelfTo<DialogManager>().AsSingle().NonLazy();
		Container.BindInterfacesAndSelfTo<DayNightController>().AsSingle().NonLazy();
		Container.BindInterfacesAndSelfTo<RoomsManager>().AsSingle().NonLazy();
		Container.BindInterfacesAndSelfTo<GameEventsManager>().AsSingle().NonLazy();
		Container.BindInterfacesAndSelfTo<HUDPresenter>().AsSingle().NonLazy();
		Container.BindInterfacesAndSelfTo<UpdaterService>().AsSingle().NonLazy();
		Container.BindInterfacesAndSelfTo<WindowsManager>().AsSingle().NonLazy();
		Container.BindInterfacesAndSelfTo<ConsumablesController>().AsSingle().NonLazy();
		Container.BindInterfacesAndSelfTo<CloseUpsController>().AsSingle().NonLazy();
		Container.BindInterfacesAndSelfTo<ActionableObjectsManager>().AsSingle().NonLazy();
		Container.BindInterfacesAndSelfTo<NotepadController>().AsSingle().NonLazy();
		Container.BindInterfacesAndSelfTo<PlayerService>().AsSingle().NonLazy();
		Container.BindInterfacesAndSelfTo<PauseController>().AsSingle().NonLazy();
		Container.BindInterfacesAndSelfTo<LocationsManager>().AsSingle().NonLazy();
		Container.BindInterfacesAndSelfTo<InteractablesManager>().AsSingle().NonLazy();
		Container.BindInterfacesAndSelfTo<StateObjectController>().AsSingle().NonLazy();
		Container.BindInterfacesAndSelfTo<UIInitializer>().AsSingle().NonLazy();
		Container.BindInterfacesAndSelfTo<EndingShower>().AsSingle().NonLazy();
		Container.BindInterfacesAndSelfTo<GameplayEndingManager>().AsSingle().NonLazy();
		Container.BindInterfacesAndSelfTo<CutscenesManager>().AsSingle().NonLazy();
		Container.BindInterfacesAndSelfTo<SettingsManager>().AsSingle().NonLazy();
		Container.BindInterfacesAndSelfTo<CatController>().AsSingle().NonLazy();
		Container.BindInterfacesAndSelfTo<TriggerObjectsController>().AsSingle().NonLazy();
		Container.BindInterfacesAndSelfTo<DreamController>().AsSingle().NonLazy();
		Container.BindInterfacesAndSelfTo<EffectsController>().AsSingle().NonLazy();
		Container.BindInterfacesAndSelfTo<RandomSoundsEmitterController>().AsSingle().NonLazy();
		Container.BindInterfacesAndSelfTo<StartUpService>().AsSingle().NonLazy();
	}
}
