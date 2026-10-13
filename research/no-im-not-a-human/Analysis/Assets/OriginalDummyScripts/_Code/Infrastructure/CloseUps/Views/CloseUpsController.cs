using System;
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
		private CloseUpSaveData _saveData;

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

		public FridgeCloseUpView Fridge => null;

		public PhoneCloseUpView Phone => null;

		public RadioCloseUpView Radio => null;

		public MushroomlistCloseUpView Mushroomlist => null;

		public ConsumableCloseUpView Consumable => null;

		private ACloseUpView[] CloseUpViews => null;

		public bool IsAnyCloseUpActive { get; private set; }

		public int FemaCallsCount => 0;

		public IUpdateable[] Updateables => null;

		public CloseUpsController(ICloseUpsViewProvider closeUpsViewProvider, IHUDPresenter hudPresenter, IDayNightController dayNightController, IPlayerService playerService, ICursorController cursorController, IOtherGameSODataProvider otherGameSoDataProvider, IInputHandlerProvider inputHandlerProvider, IConsumablesController consumablesController, IDataModelService dataModelService, INotAHumanSoundService soundService, ICustomYarnReaderProvider customYarnReaderProvider, IDialogManager dialogManager, IStateObjectController stateObjectController, IRoomDisplayerViewProvider roomDisplayerViewProvider, IGameplayEndingManager gameplayEndingManager, ICharactersManager charactersManager, WatcherManager watcherManager, IDreamController dreamController)
		{
		}

		private void OnWaveFound(int day)
		{
		}

		private void OnDayChanged(int day)
		{
		}

		private void Init()
		{
		}

		private void OnCloseUpEntered()
		{
		}

		private void OnCloseUpLeft()
		{
		}

		public void UnlockPhoneSubscriber(EPhoneSubscriber subscriber)
		{
		}

		public string GetPhoneNumber(EPhoneSubscriber subscriber)
		{
			return null;
		}

		protected override void OnSaveDataLoad(IGameSaveDataHandler saver)
		{
		}
	}
}
