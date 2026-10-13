using System;
using Zenject;
using _Code.DialogSystem;
using _Code.Infrastructure.ActionableObjects;
using _Code.Infrastructure.CloseUps;
using _Code.Infrastructure.Consumables;
using _Code.Infrastructure.Cursor;
using _Code.Infrastructure.Cutscenes;
using _Code.Infrastructure.DataModel.Models.GameSave;
using _Code.Infrastructure.DayNight;
using _Code.Infrastructure.Endings;
using _Code.Infrastructure.Endings.Gameplay;
using _Code.Infrastructure.Locations;
using _Code.Infrastructure.Pause;
using _Code.Infrastructure.Player;
using _Code.Infrastructure.StateObjects;
using _Code.Infrastructure.Updatable;
using _Code.Infrastructure._NINAH__Cat;
using _Code.Infrastructure._NINAH__InteractableObjects.Objects;
using _Code.Menues.HUD;
using _Code.Player;
using _Scripts.Services.DataModel;
using _Scripts.Services.Sound.Service;

namespace _Code.Infrastructure;

public sealed class InteractablesManager : ASavableClass<InteractablesSaveData>, IInteractablesManager, IDisposable, ITickable
{
	private InteractablesSaveData _saveData = new InteractablesSaveData();

	private readonly HatchInteractable _hatchHouse;

	private readonly HatchInteractable _hatchBasement;

	private readonly PhoneInteractable _phone;

	private readonly RadioInteractable _radio;

	private readonly CigaretteInteractable _cigarettes;

	private readonly EndingLaunchInteractable _peepholeEndingInteractable;

	private readonly SaveInteractable _saveInteractable;

	private readonly MushroomInteractable _mushroomInteractable;

	private readonly TheHoleInteractable _holeInteractable;

	private readonly CatInteractable _catInteractable;

	private readonly WindowBoardsInteractable[] _windowBoardsInteractables;

	private readonly DialogInteractable[] _dialogInteractables;

	private readonly ZoomInteractable[] _zoomInteractables;

	private readonly CalendarInteractable _calendarInteractable;

	private AInteractableObject[] _interactables;

	private readonly IGameplayEndingManager _gameplayEndingManager;

	private IDataModelService _dataModelService;

	public IUpdateable[] Updateables => new IUpdateable[0];

	public InteractablesManager(IInteractablesViewProvider viewProvider, ILocationsManager locationsManager, IHUDPresenter hudPresenter, IPauseController pauseController, ICloseUpsController closeUpsController, INotAHumanSoundService soundService, IInputHandlerProvider inputHandlerProvider, IPlayerService playerService, IGameplayEndingManager gameplayEndingManager, IDialogManager dialogManager, IDataModelService dataModelService, IConsumablesController consumablesController, IDayNightController dayNightController, IStateObjectController stateObjectController, ICursorController cursorController, IActionableObjectsManager actionableObjectsManager, ICatController catController, ICutscenesManager cutscenesManager)
	{
		_hatchHouse = viewProvider.HatchHouse;
		_hatchBasement = viewProvider.HatchBasement;
		_phone = viewProvider.Phone;
		_radio = viewProvider.Radio;
		_cigarettes = viewProvider.Cigarettes;
		_windowBoardsInteractables = viewProvider.WindowBoards;
		_peepholeEndingInteractable = viewProvider.PeepholeEnding;
		_saveInteractable = viewProvider.SaveInteractable;
		_mushroomInteractable = viewProvider.Mushroom;
		_holeInteractable = viewProvider.TheHole;
		_catInteractable = viewProvider.Cat;
		_dialogInteractables = viewProvider.Dialogs;
		_zoomInteractables = viewProvider.ZoomInteractables;
		_calendarInteractable = viewProvider.CalendarInteractable;
		_gameplayEndingManager = gameplayEndingManager;
		_dataModelService = dataModelService;
		_interactables = new AInteractableObject[18]
		{
			_hatchHouse,
			_hatchBasement,
			_phone,
			_radio,
			_cigarettes,
			_windowBoardsInteractables[0],
			_windowBoardsInteractables[1],
			_windowBoardsInteractables[2],
			_peepholeEndingInteractable,
			_saveInteractable,
			_mushroomInteractable,
			_holeInteractable,
			_catInteractable,
			_dialogInteractables[0],
			_dialogInteractables[1],
			_dialogInteractables[2],
			_dialogInteractables[3],
			_calendarInteractable
		};
		AInteractableObject[] interactables = _interactables;
		for (int i = 0; i < interactables.Length; i++)
		{
			interactables[i].PreInit(inputHandlerProvider);
		}
		_hatchHouse.Init(locationsManager, hudPresenter, pauseController, dayNightController, playerService);
		_hatchBasement.Init(locationsManager, hudPresenter, pauseController, dayNightController, playerService);
		_phone.Init(closeUpsController, hudPresenter, pauseController, dayNightController);
		_radio.Init(closeUpsController, hudPresenter, pauseController, dayNightController, soundService);
		_cigarettes.Init(hudPresenter, pauseController, consumablesController, dayNightController, closeUpsController, playerService, soundService);
		_peepholeEndingInteractable.Init(hudPresenter, pauseController, playerService, gameplayEndingManager);
		_saveInteractable.Init(hudPresenter, pauseController, dataModelService, consumablesController, playerService, closeUpsController, dayNightController, soundService);
		_mushroomInteractable.Init(consumablesController, dialogManager, hudPresenter, pauseController, soundService, stateObjectController);
		_holeInteractable.Init(hudPresenter, pauseController, stateObjectController, playerService, dayNightController, soundService);
		_catInteractable.Init(hudPresenter, soundService, actionableObjectsManager, catController);
		_calendarInteractable.Init(hudPresenter, pauseController, dayNightController, inputHandlerProvider);
		WindowBoardsInteractable[] windowBoardsInteractables = _windowBoardsInteractables;
		for (int i = 0; i < windowBoardsInteractables.Length; i++)
		{
			windowBoardsInteractables[i].Init(hudPresenter, pauseController, soundService, playerService, gameplayEndingManager, dialogManager);
		}
		for (int j = 0; j < _dialogInteractables.Length; j++)
		{
			_dialogInteractables[j].Init(j, hudPresenter, pauseController, dialogManager, _saveData, playerService, inputHandlerProvider, cursorController, dayNightController);
			_dialogInteractables[j].DialogStarted -= OnDialogStarted;
			_dialogInteractables[j].DialogStarted += OnDialogStarted;
			_dialogInteractables[j].DialogEnded -= OnDialogEnded;
			_dialogInteractables[j].DialogEnded += OnDialogEnded;
		}
		ZoomInteractable[] zoomInteractables = _zoomInteractables;
		for (int i = 0; i < zoomInteractables.Length; i++)
		{
			zoomInteractables[i].Init(hudPresenter, pauseController, inputHandlerProvider);
		}
		gameplayEndingManager.EndingTriggered -= OnEndingTriggered;
		gameplayEndingManager.EndingTriggered += OnEndingTriggered;
		dialogManager.CatPet -= OnCatPet;
		dialogManager.CatPet += OnCatPet;
		dialogManager.CatTaken -= OnCatTaken;
		dialogManager.CatTaken += OnCatTaken;
		cutscenesManager.CutsceneStarted -= OnCutsceneStarted;
		cutscenesManager.CutsceneStarted += OnCutsceneStarted;
		locationsManager.LocationChanged -= OnLocationChanged;
		locationsManager.LocationChanged += OnLocationChanged;
		_radio.RadioTaken -= OnRadioTaken;
		_radio.RadioTaken += OnRadioTaken;
		_radio.RadioRemoved -= OnRadioRemoved;
		_radio.RadioRemoved += OnRadioRemoved;
		_catInteractable.NaperdyshReleased -= OnNeperdyshReleased;
		_catInteractable.NaperdyshReleased += OnNeperdyshReleased;
		_holeInteractable.HoleReady -= OnHoleReady;
		_holeInteractable.HoleReady += OnHoleReady;
		LinkSaveDataKey(this, _saveData, dataModelService.GameSaveDataHandler);
	}

	private void OnLocationChanged(ELocation location)
	{
		if (location == ELocation.HouseInterior)
		{
			_catInteractable.ResetAnim();
		}
	}

	private void OnHoleReady()
	{
		((GameplayEndingDeath)_gameplayEndingManager.GetEnding(EEnding.TheDeath)).CompleteCondition(2);
	}

	private void OnDialogStarted()
	{
		DisableAll();
	}

	private void OnDialogEnded()
	{
		EnableAll();
	}

	private void OnCutsceneStarted(ECutscene cutscene)
	{
		if (cutscene == ECutscene.Fek3d)
		{
			DisableAll();
		}
	}

	private void EnableAll()
	{
		AInteractableObject[] interactables = _interactables;
		for (int i = 0; i < interactables.Length; i++)
		{
			interactables[i].Enable();
		}
	}

	private void DisableAll()
	{
		AInteractableObject[] interactables = _interactables;
		for (int i = 0; i < interactables.Length; i++)
		{
			interactables[i].Disable();
		}
	}

	private void OnNeperdyshReleased()
	{
		EnableAll();
	}

	private void OnRadioRemoved()
	{
		EnableAll();
	}

	private void OnRadioTaken()
	{
		DisableAll();
	}

	private void OnCatTaken()
	{
		_catInteractable.Take();
		DisableAll();
	}

	private void OnCatPet()
	{
		_catInteractable.Pet();
	}

	private void OnEndingTriggered(EEnding ending)
	{
		if (ending == EEnding.Baby)
		{
			_peepholeEndingInteractable.SetEnding(ending);
		}
	}

	protected override void OnSaveDataLoad(IGameSaveDataHandler saver)
	{
		_saveData = saver.LoadKey(this);
		AInteractableObject[] interactables = _interactables;
		for (int i = 0; i < interactables.Length; i++)
		{
			interactables[i].OnLoad();
		}
		LinkSaveDataKey(this, _saveData, _dataModelService.GameSaveDataHandler);
	}

	public void Tick()
	{
		AInteractableObject[] interactables = _interactables;
		for (int i = 0; i < interactables.Length; i++)
		{
			interactables[i].OnUpdate();
		}
	}
}
