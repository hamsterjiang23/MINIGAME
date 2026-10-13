using System;
using System.Linq;
using _Code.Characters;
using _Code.DialogSystem;
using _Code.Infrastructure.CloseUps.Views.Phone;
using _Code.Infrastructure.CloseUps.Views.Radio;
using _Code.Infrastructure.Consumables;
using _Code.Infrastructure.Cursor;
using _Code.Infrastructure.DataModel.Models.GameSave;
using _Code.Infrastructure.DayNight;
using _Code.Infrastructure.Endings.Gameplay;
using _Code.Infrastructure.OtherGameData;
using _Code.Infrastructure.Player;
using _Code.Infrastructure.Rooms;
using _Code.Infrastructure.StateObjects;
using _Code.Infrastructure.Updatable;
using _Code.Infrastructure._NINAH__CloseUps;
using _Code.Infrastructure._NINAH__CloseUps.Views.Consumables;
using _Code.Infrastructure._NINAH__CloseUps.Views.Mushroomlist;
using _Code.Infrastructure._NINAH__Dream;
using _Code.Menues.HUD;
using _Code.Player;
using _Code.Utils.CustomYarnReading;
using _Scripts.Services.DataModel;
using _Scripts.Services.Sound.Service;

namespace _Code.Infrastructure.CloseUps.Views
{
	public sealed class CloseUpsController : ASavableClass<CloseUpSaveData>, ICloseUpsController, IDisposable
	{
		private CloseUpSaveData _saveData = new CloseUpSaveData();

		private readonly FridgeCloseUpView _fridgeCloseUpView;

		private readonly PhoneCloseUpView _phoneCloseUpView;

		private readonly RadioCloseUpView _radioCloseUpView;

		private readonly MushroomlistCloseUpView _mushroomlistCloseUpView;

		private readonly ConsumableCloseUpView _consumableCloseUpView;

		private readonly IHUDPresenter _hudPresenter;

		private readonly IDayNightController _dayNightController;

		private readonly IPlayerService _playerService;

		private readonly ICursorController _cursorController;

		private readonly IOtherGameSODataProvider _otherGameSoDataProvider;

		private readonly IInputHandlerProvider _inputHandlerProvider;

		private readonly IConsumablesController _consumablesController;

		private readonly INotAHumanSoundService _soundService;

		private readonly ICustomYarnReaderProvider _customYarnReaderProvider;

		private readonly IDialogManager _dialogManager;

		private readonly IStateObjectController _stateObjectController;

		private readonly IGameplayEndingManager _gameplayEndingManager;

		private readonly IRoomDisplayerViewProvider _roomDisplayerViewProvider;

		private readonly IDataModelService _dataModelService;

		private readonly ICharactersManager _charactersManager;

		private readonly WatcherManager _watcherManager;

		private readonly IDreamController _dreamController;

		public FridgeCloseUpView Fridge => _fridgeCloseUpView;

		public PhoneCloseUpView Phone => _phoneCloseUpView;

		public RadioCloseUpView Radio => _radioCloseUpView;

		public MushroomlistCloseUpView Mushroomlist => _mushroomlistCloseUpView;

		public ConsumableCloseUpView Consumable => _consumableCloseUpView;

		private ACloseUpView[] CloseUpViews => new ACloseUpView[4] { _fridgeCloseUpView, _phoneCloseUpView, _radioCloseUpView, _mushroomlistCloseUpView };

		public bool IsAnyCloseUpActive { get; private set; }

		public int FemaCallsCount => _saveData.FemaCalls;

		public IUpdateable[] Updateables => CloseUpViews.SelectMany((ACloseUpView x) => x.Updateables).ToArray();

		public CloseUpsController(ICloseUpsViewProvider closeUpsViewProvider, IHUDPresenter hudPresenter, IDayNightController dayNightController, IPlayerService playerService, ICursorController cursorController, IOtherGameSODataProvider otherGameSoDataProvider, IInputHandlerProvider inputHandlerProvider, IConsumablesController consumablesController, IDataModelService dataModelService, INotAHumanSoundService soundService, ICustomYarnReaderProvider customYarnReaderProvider, IDialogManager dialogManager, IStateObjectController stateObjectController, IRoomDisplayerViewProvider roomDisplayerViewProvider, IGameplayEndingManager gameplayEndingManager, ICharactersManager charactersManager, WatcherManager watcherManager, IDreamController dreamController)
		{
			_hudPresenter = hudPresenter;
			_dayNightController = dayNightController;
			_playerService = playerService;
			_cursorController = cursorController;
			_otherGameSoDataProvider = otherGameSoDataProvider;
			_inputHandlerProvider = inputHandlerProvider;
			_consumablesController = consumablesController;
			_soundService = soundService;
			_customYarnReaderProvider = customYarnReaderProvider;
			_dialogManager = dialogManager;
			_stateObjectController = stateObjectController;
			_gameplayEndingManager = gameplayEndingManager;
			_roomDisplayerViewProvider = roomDisplayerViewProvider;
			_dataModelService = dataModelService;
			_charactersManager = charactersManager;
			_watcherManager = watcherManager;
			_dreamController = dreamController;
			_fridgeCloseUpView = closeUpsViewProvider.FridgeCloseUpView;
			_phoneCloseUpView = closeUpsViewProvider.PhoneCloseUpView;
			_radioCloseUpView = closeUpsViewProvider.RadioCloseUpView;
			_mushroomlistCloseUpView = closeUpsViewProvider.MushroomlistCloseUpView;
			_consumableCloseUpView = closeUpsViewProvider.ConsumableCloseUpView;
			_dialogManager.UnlockedPhoneSubscriber -= UnlockPhoneSubscriber;
			_dialogManager.UnlockedPhoneSubscriber += UnlockPhoneSubscriber;
			_dialogManager.GotPhoneNumber -= GetPhoneNumber;
			_dialogManager.GotPhoneNumber += GetPhoneNumber;
			_dayNightController.DayChanged -= OnDayChanged;
			_dayNightController.DayChanged += OnDayChanged;
			_radioCloseUpView.RadioWaveFound -= OnWaveFound;
			_radioCloseUpView.RadioWaveFound += OnWaveFound;
			Init();
			LinkSaveDataKey(this, _saveData, dataModelService.GameSaveDataHandler);
		}

		private void OnWaveFound(int day)
		{
			if (day == 3)
			{
				UnlockPhoneSubscriber(EPhoneSubscriber.Psychics);
			}
			if (day == 5)
			{
				UnlockPhoneSubscriber(EPhoneSubscriber.PhoneRoulette);
			}
		}

		private void OnDayChanged(int day)
		{
			_saveData.DrunkBeerToday = false;
		}

		private void Init()
		{
			_fridgeCloseUpView.InitModules(_hudPresenter, _dayNightController, _consumablesController, _stateObjectController, _gameplayEndingManager, _saveData, _dialogManager, _roomDisplayerViewProvider, _cursorController, _inputHandlerProvider, _playerService, _dataModelService, _dreamController);
			_radioCloseUpView.InitModules(_otherGameSoDataProvider, _customYarnReaderProvider, _saveData, _dayNightController, _dialogManager, _inputHandlerProvider, _dataModelService, _watcherManager);
			_phoneCloseUpView.InitModules(_dialogManager, _dayNightController, _saveData, _otherGameSoDataProvider.OtherGameData, _cursorController, _charactersManager, _inputHandlerProvider.InputHandler, _dataModelService, _watcherManager);
			_consumableCloseUpView.InitModules(_consumablesController, _hudPresenter, _inputHandlerProvider, _cursorController, _dataModelService, _watcherManager);
			ACloseUpView[] closeUpViews = CloseUpViews;
			foreach (ACloseUpView obj in closeUpViews)
			{
				obj.Init();
				obj.InitModules(_hudPresenter, _playerService, _cursorController, _inputHandlerProvider, _soundService);
				obj.Entered -= OnCloseUpEntered;
				obj.Entered += OnCloseUpEntered;
				obj.Left -= OnCloseUpLeft;
				obj.Left += OnCloseUpLeft;
			}
			_gameplayEndingManager.InitGetFemaCallsFunc(() => FemaCallsCount);
		}

		private void OnCloseUpEntered()
		{
			IsAnyCloseUpActive = true;
		}

		private void OnCloseUpLeft()
		{
			IsAnyCloseUpActive = false;
		}

		public void UnlockPhoneSubscriber(EPhoneSubscriber subscriber)
		{
			_phoneCloseUpView.UnlockPhoneSubscriber(subscriber);
		}

		public string GetPhoneNumber(EPhoneSubscriber subscriber)
		{
			return _phoneCloseUpView.GetPhoneNumber(subscriber);
		}

		protected override void OnSaveDataLoad(IGameSaveDataHandler saver)
		{
			_saveData = saver.LoadKey(this);
			_fridgeCloseUpView.Refill();
			_fridgeCloseUpView.ReinitSaveData(_saveData);
			_radioCloseUpView.ReinitSaveData(_saveData);
			_phoneCloseUpView.ReinitSaveData(_saveData);
			_phoneCloseUpView.RestorePhoneNumbers(_saveData.UnlockedPhoneSubscribers);
			LinkSaveDataKey(this, _saveData, _dataModelService.GameSaveDataHandler);
		}
	}
}
