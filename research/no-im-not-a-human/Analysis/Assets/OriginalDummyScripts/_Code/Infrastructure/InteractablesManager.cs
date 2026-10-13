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

namespace _Code.Infrastructure
{
	public sealed class InteractablesManager : ASavableClass<InteractablesSaveData>, IInteractablesManager, IDisposable, ITickable
	{
		private InteractablesSaveData _saveData;

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

		public IUpdateable[] Updateables => null;

		public InteractablesManager(IInteractablesViewProvider viewProvider, ILocationsManager locationsManager, IHUDPresenter hudPresenter, IPauseController pauseController, ICloseUpsController closeUpsController, INotAHumanSoundService soundService, IInputHandlerProvider inputHandlerProvider, IPlayerService playerService, IGameplayEndingManager gameplayEndingManager, IDialogManager dialogManager, IDataModelService dataModelService, IConsumablesController consumablesController, IDayNightController dayNightController, IStateObjectController stateObjectController, ICursorController cursorController, IActionableObjectsManager actionableObjectsManager, ICatController catController, ICutscenesManager cutscenesManager)
		{
		}

		private void OnLocationChanged(ELocation location)
		{
		}

		private void OnHoleReady()
		{
		}

		private void OnDialogStarted()
		{
		}

		private void OnDialogEnded()
		{
		}

		private void OnCutsceneStarted(ECutscene cutscene)
		{
		}

		private void EnableAll()
		{
		}

		private void DisableAll()
		{
		}

		private void OnNeperdyshReleased()
		{
		}

		private void OnRadioRemoved()
		{
		}

		private void OnRadioTaken()
		{
		}

		private void OnCatTaken()
		{
		}

		private void OnCatPet()
		{
		}

		private void OnEndingTriggered(EEnding ending)
		{
		}

		protected override void OnSaveDataLoad(IGameSaveDataHandler saver)
		{
		}

		public void Tick()
		{
		}
	}
}
