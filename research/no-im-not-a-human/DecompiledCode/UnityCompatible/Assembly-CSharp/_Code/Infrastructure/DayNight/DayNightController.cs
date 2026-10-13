using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using _Code.Characters;
using _Code.Characters.DialogSystem;
using _Code.DialogSystem;
using _Code.Events;
using _Code.Infrastructure.Consumables;
using _Code.Infrastructure.Cursor;
using _Code.Infrastructure.DataModel.Models.GameSave;
using _Code.Infrastructure.Endings;
using _Code.Infrastructure.Endings.Data;
using _Code.Infrastructure.GameEvents;
using _Code.Infrastructure.Sound;
using _Code.Infrastructure.StateObjects;
using _Code.Infrastructure.Windows;
using _Code.Infrastructure._NINAH__Cat;
using _Code.Infrastructure._NINAH__CommonView;
using _Code.Infrastructure._NINAH__DayNight;
using _Code.Infrastructure._NINAH__Dream;
using _Code.Infrastructure._NINAH__Effects;
using _Code.Infrastructure._NINAH__Rooms;
using _Code.Menues.HUD;
using _Code.Player;
using _Code.Rooms;
using _Code.Utils;
using _Scripts.Services.DataModel;
using _Scripts.Services.Sound;
using _Scripts.Services.Sound.Service;

namespace _Code.Infrastructure.DayNight
{
	public sealed class DayNightController : ASavableClass<DayNightSaveData>, IDayNightController, IDisposable
	{
		private DayNightSaveData _saveData = new DayNightSaveData();

		private readonly IGameEventsManager _gameEventsManager;

		private readonly ICharactersManager _charactersManager;

		private readonly IHUDPresenter _hudPresenter;

		private readonly IDayNightControllerViewProvider _viewProvider;

		private readonly IDialogManager _dialogManager;

		private readonly IWindowsViewProvider _windowsViewProvider;

		private readonly INotAHumanSoundService _soundService;

		private readonly IStateObjectController _stateObjectController;

		private readonly DayNightControllerSOData _data;

		private bool _wasSomebodyMurdered;

		private IDataModelService _dataModelService;

		private readonly InputHandling _inputHandler;

		private readonly ICursorController _cursorController;

		private readonly IConsumablesController _consumablesController;

		private readonly ICommonViewProvider _commonViewProvider;

		private readonly IDreamController _dreamController;

		private readonly IEffectsController _effectsController;

		private readonly EndingsSOData _endingSoData;

		private readonly ICatController _catController;

		public int MaxDayActions => _data.MaxDayActionsByDays[Day] + _saveData.ExtraEnergySlots - (_saveData.EnergyFinesByDays.ContainsKey(Day) ? _saveData.EnergyFinesByDays[Day] : 0);

		public int DayActions => _saveData.DayActions;

		public int Day => _saveData.Day;

		public bool IsEndingDay => Day == _data.LastDay;

		public ETimeOfDay CurrentTimeOfDay => _saveData.TimeOfDay;

		public float LastChange => _saveData.LastChange;

		public bool CanLeaveRooms { get; private set; } = true;

		public event Action WatchedTV;

		public event Action<ETimeOfDay> Changed;

		public event Action<List<ChangePoseData>> PosesChanged;

		public event Action<int> DayChanged;

		public event Action BodyEaterAppeared;

		public event Action BodyEaterDisappeared;

		public event Action WentToBed;

		public event Action WokeUp;

		public DayNightController(IDayNightControllerSODataProvider dataProvider, IGameEventsManager gameEventsManager, ICharactersManager charactersManager, IHUDPresenter hudPresenter, IDayNightControllerViewProvider viewProvider, IDialogManager dialogManager, IWindowsViewProvider windowsViewProvider, INotAHumanSoundService soundService, IDataModelService dataModelService, IStateObjectController stateObjectController, ICursorController cursorController, IInputHandlerProvider inputHandlerProvider, IConsumablesController consumablesController, ICommonViewProvider commonViewProvider, IDreamController dreamController, IEffectsController effectsController, IEndingSODataProvider endingSoDataProvider, ICatController catController)
		{
			_data = dataProvider.DayNightControllerData;
			_gameEventsManager = gameEventsManager;
			_charactersManager = charactersManager;
			_hudPresenter = hudPresenter;
			_viewProvider = viewProvider;
			_dialogManager = dialogManager;
			_windowsViewProvider = windowsViewProvider;
			_soundService = soundService;
			_stateObjectController = stateObjectController;
			_cursorController = cursorController;
			_dataModelService = dataModelService;
			_inputHandler = inputHandlerProvider.InputHandler;
			_consumablesController = consumablesController;
			_commonViewProvider = commonViewProvider;
			_dreamController = dreamController;
			_effectsController = effectsController;
			_endingSoData = endingSoDataProvider.EndingSOData;
			_catController = catController;
			Init();
			dataModelService.GameSaveDataHandler.SavedAll -= OnSave;
			dataModelService.GameSaveDataHandler.SavedAll += OnSave;
			LinkSaveDataKey(this, _saveData, dataModelService.GameSaveDataHandler);
		}

		private void Init()
		{
			_gameEventsManager.AnyEventCompleted -= UpdateTimeOfDayStartedOrDialogEndedTime;
			_dialogManager.EnergyConsumed -= Act;
			_dialogManager.FedCat -= OnFedCat;
			_dialogManager.CouldOrderCourier -= OnCouldOrderCourier;
			_dialogManager.OrderedCourier -= OnOrderedCourier;
			_dialogManager.GotEnergy -= OnGotEnergy;
			_dialogManager.IntroSkipped -= OnIntroSkipped;
			_gameEventsManager.AnyEventCompleted += UpdateTimeOfDayStartedOrDialogEndedTime;
			_dialogManager.EnergyConsumed += Act;
			_dialogManager.FedCat += OnFedCat;
			_dialogManager.CouldOrderCourier += OnCouldOrderCourier;
			_dialogManager.OrderedCourier += OnOrderedCourier;
			_dialogManager.GotEnergy += OnGotEnergy;
			_dialogManager.IntroSkipped += OnIntroSkipped;
			_gameEventsManager.GetBaseDayNightData += () => (day: Day, timeOfDay: CurrentTimeOfDay, timeFromLastChange: LastChange - _saveData.AccumulatedTime);
			_charactersManager.GetBaseDayNightData += () => (Day: Day, CurrentTimeOfDay: CurrentTimeOfDay, LastChange: LastChange - _saveData.AccumulatedTime);
			_hudPresenter.GetDayActions += () => DayActions;
			_hudPresenter.GetMaxDayActions += () => MaxDayActions;
			_saveData.DayActions = MaxDayActions;
			if (Day == 0)
			{
				_gameEventsManager.GenerateDingDongEventsForDayAsync(0);
			}
			_gameEventsManager.InitializeGetDay(() => Day);
			_consumablesController.InitializeGetDay(() => Day);
			_dialogManager.InitializeGetDay(() => Day);
			InitDevUtils().Forget();
		}

		private void OnIntroSkipped()
		{
			_saveData.Day = 1;
			_saveData.TimeOfDay = ETimeOfDay.Night;
			WindowView[] windowsViews = _windowsViewProvider.WindowsViews;
			for (int i = 0; i < windowsViews.Length; i++)
			{
				windowsViews[i].InitImages(Day);
			}
			windowsViews = _windowsViewProvider.WindowsViews;
			for (int i = 0; i < windowsViews.Length; i++)
			{
				windowsViews[i].RenewDialogs();
			}
			_gameEventsManager.GenerateDingDongEventsForSkip();
			DayChanged?.Invoke(Day);
		}

		private int OnGotEnergy()
		{
			return DayActions;
		}

		private void OnOrderedCourier(EConsumable consumable, int count)
		{
			_saveData.LastCourierOrderedDay = Day;
			_gameEventsManager.GenerateCharacterDingDongEventsForDay(Day, ECharacterType.Courier);
		}

		private bool OnCouldOrderCourier()
		{
			return _saveData.LastCourierOrderedDay + 2 < Day;
		}

		private void OnFedCat()
		{
			_saveData.ImpostersKillingBlocked = true;
			_soundService.PlayOneShot(ESoundSource.s1_GameScene_2DSFXSource, (UnityEngine.Random.Range(0, 2) == 0) ? ESound.Characters_cat_feed_01 : ESound.Characters_cat_feed_02);
		}

		private async UniTaskVoid InitDevUtils()
		{
		}

		public async UniTaskVoid SetDayForEnding(EEnding ending)
		{
			await _dataModelService.GameSaveDataHandler.SaveAllReserve();
			switch (ending)
			{
			case EEnding.Baby:
				_saveData.Day = _data.BabyEndingDay;
				InitNewNight();
				break;
			case EEnding.Killer:
				_saveData.Day = _data.KillerEndingDay;
				_saveData.TimeOfDay = ETimeOfDay.Night;
				InitNewNight();
				break;
			case EEnding.FEMA:
				_saveData.Day = _data.FemaEndingDay;
				_saveData.TimeOfDay = ETimeOfDay.Night;
				InitNewNight();
				break;
			case EEnding.TheDeath:
			case EEnding.Cult:
				break;
			}
		}

		public void AddEnergySlot()
		{
			_saveData.CoffeeDays.Add(Day);
			_saveData.ExtraEnergySlots++;
			_hudPresenter.InitActionsCount();
		}

		public void AddEnergy()
		{
			if (_saveData.DayActions < MaxDayActions)
			{
				_saveData.DayActions++;
				_hudPresenter.InitActionsCount();
			}
		}

		public void RefillEnergy()
		{
			_saveData.DayActions = MaxDayActions;
			_hudPresenter.InitActionsCount();
		}

		public void RemoveExtraEnergySlotsForTomorrow(int energyCount)
		{
			if (_saveData.EnergyFinesByDays.ContainsKey(Day + 1))
			{
				_saveData.EnergyFinesByDays[Day + 1] += energyCount;
			}
			else
			{
				_saveData.EnergyFinesByDays.Add(Day + 1, energyCount);
			}
		}

		public void Change()
		{
			if (!CanLeaveRooms)
			{
				return;
			}
			bool flag = false;
			if (!HasCompletedDaytimeGoal() && !flag)
			{
				if (!_dialogManager.IsOpenedSubtitle)
				{
					EInfoMessageType messageType = ((CurrentTimeOfDay == ETimeOfDay.Day) ? EInfoMessageType.CantSleepDay : EInfoMessageType.CantSleepNight);
					_dialogManager.ShowSubtitlePopup(messageType);
				}
				return;
			}
			Vibrator.VibrateForTime(0.1f, 0.1f, 0.2f).Forget();
			WentToBed?.Invoke();
			_saveData.TimeOfDay++;
			if (_saveData.TimeOfDay > ETimeOfDay.Night)
			{
				_saveData.TimeOfDay = ETimeOfDay.Day;
			}
			_dialogManager.UpdateTalksCount();
			ChangeDaytimeVisual().Forget();
			_saveData.LastChange = Time.timeSinceLevelLoad + _saveData.AccumulatedTime;
		}

		private void InitNewNight(bool isFromSaveData = false)
		{
			WindowView[] windowsViews = _windowsViewProvider.WindowsViews;
			for (int i = 0; i < windowsViews.Length; i++)
			{
				windowsViews[i].InitImages(Day);
			}
			windowsViews = _windowsViewProvider.WindowsViews;
			for (int i = 0; i < windowsViews.Length; i++)
			{
				windowsViews[i].RenewDialogs();
			}
			if (!isFromSaveData)
			{
				if (_saveData.IsReadyToRunBodyeater)
				{
					_saveData.IsReadyToRunBodyeater = false;
					_saveData.IsNeedToRepairAfterBodyeater = true;
					_charactersManager.KillEveryone();
					BodyEaterAppeared?.Invoke();
					_gameEventsManager.DisableSuperForDay(Day);
				}
				_gameEventsManager.GenerateDingDongEventsForDayAsync(Day);
			}
		}

		private void InitNewDay(bool isFromSaveData = false)
		{
			if (isFromSaveData)
			{
				return;
			}
			_saveData.TimeOfDay = ETimeOfDay.Day;
			_saveData.Day++;
			_charactersManager.RunMorningOfSuicides();
			PosesChanged?.Invoke(_saveData.ChangePosesTomorrow);
			_saveData.ChangePosesTomorrow.Clear();
			if (_charactersManager.ImpostersCount > 1 && _charactersManager.GetPossibleCharactersToKillCount() > 0 && _charactersManager.InnocentsCount > 0 && !_saveData.ImpostersKillingBlocked)
			{
				_wasSomebodyMurdered = true;
			}
			_saveData.ImpostersKillingBlocked = false;
			int item = -1;
			foreach (int coffeeDay in _saveData.CoffeeDays)
			{
				if (Day >= coffeeDay + _data.CoffeeBoostDays)
				{
					item = coffeeDay;
					_saveData.ExtraEnergySlots--;
					break;
				}
			}
			_saveData.CoffeeDays.Remove(item);
			_saveData.DayActions = MaxDayActions;
			DayChanged?.Invoke(Day);
			_gameEventsManager.BeginNewDay();
			_charactersManager.BeginNewDay(Day);
			_dialogManager.RefreshSignChecks();
			_consumablesController.BeginNewDay();
		}

		public bool HasCompletedDaytimeGoal()
		{
			ETimeOfDay currentTimeOfDay = CurrentTimeOfDay;
			return currentTimeOfDay switch
			{
				ETimeOfDay.Day => DayActions <= 0, 
				ETimeOfDay.Night => _gameEventsManager.AreAllDingDongsCompletedThisNight(Day), 
			};
		}

		private async UniTaskVoid ChangeDaytimeVisual()
		{
			CanLeaveRooms = false;
			_hudPresenter.AnimateFallAsleep(out var randomSleepTimes);
			float num = randomSleepTimes.Sum() + randomSleepTimes[0] / 2f + randomSleepTimes[1] / 2f;
			if (CurrentTimeOfDay == ETimeOfDay.Day)
			{
				_soundService.DisableNightTheme(num);
			}
			else
			{
				_soundService.DisableDayTheme(num);
			}
			await UniTask.Delay(TimeSpan.FromSeconds(num));
			if (_dreamController.NeedToShowDream)
			{
				await _dreamController.ShowDream();
				_soundService.Stop(ESoundSource.s1_GameScene_2DSFXSourceExtra);
				_soundService.Stop(ESoundSource.s1_GameScene_2DSFXSourceExtra2);
			}
			await UniTask.DelayFrame(1);
			if (CurrentTimeOfDay == ETimeOfDay.Night)
			{
				InitNewNight();
			}
			else
			{
				if (Day == 13)
				{
					await _dataModelService.GameSaveDataHandler.SaveAllReserve();
				}
				InitNewDay();
			}
			await UniTask.DelayFrame(1);
			Changed?.Invoke(CurrentTimeOfDay);
			_gameEventsManager.CheckOtherEvents();
			_catController.ChangePosition(CurrentTimeOfDay);
			_effectsController.DisableSmoke3D();
			if (Day == _data.LastDay)
			{
				_dialogManager.ShowSubtitlePopup(EInfoMessageType.INeedToNailUpWindows, 4f);
				await UniTask.Delay(TimeSpan.FromSeconds(4.0));
			}
			if (_wasSomebodyMurdered && Day != _data.LastDay)
			{
				_charactersManager.KillRandom();
				_dialogManager.ShowSubtitlePopup(EInfoMessageType.SomebodyWasMurdered);
				await UniTask.Delay(TimeSpan.FromSeconds(2.5));
				_wasSomebodyMurdered = false;
				if (_charactersManager.KilledByImpostersCount % _endingSoData.BodyEaterCount == 0)
				{
					_saveData.IsReadyToRunBodyeater = true;
					_dreamController.WantToShowDream(EDream.BodyEater);
				}
			}
			if (CurrentTimeOfDay == ETimeOfDay.Day && _saveData.IsNeedToRepairAfterBodyeater)
			{
				_saveData.IsNeedToRepairAfterBodyeater = false;
				BodyEaterDisappeared?.Invoke();
				_dialogManager.ShowSubtitlePopup(EInfoMessageType.BodyEaterVisit);
				await UniTask.Delay(TimeSpan.FromSeconds(2.5));
			}
			SelectMusic();
			UpdateVisualThingForSelectedTimeOfDay();
			if (CurrentTimeOfDay == ETimeOfDay.Day && Day < _data.LastDay)
			{
				WatchMorningTV().Forget();
			}
			if (CurrentTimeOfDay == ETimeOfDay.Day)
			{
				_stateObjectController.CheckChangesForDay(Day);
			}
			await _hudPresenter.WakeUp();
			WokeUp?.Invoke();
			CanLeaveRooms = true;
		}

		private void UpdateVisualThingForSelectedTimeOfDay()
		{
			Debug.Log("Current time of day + " + CurrentTimeOfDay);
			VisualsByTimeOfDay visualsByTimeOfDay = _data.VolumeProfiles.First((VisualsByTimeOfDay x) => x.TimeOfDay == CurrentTimeOfDay);
			_effectsController.ChangeVolumeProfile(visualsByTimeOfDay.VolumeProfile);
			GameObject[] lights = _viewProvider.Lights;
			for (int num = 0; num < lights.Length; num++)
			{
				lights[num].SetActive(visualsByTimeOfDay.HasLight);
			}
			_data.WindowMaterial.color = ((CurrentTimeOfDay == ETimeOfDay.Day) ? Color.white : Color.black);
			if (CurrentTimeOfDay == ETimeOfDay.Day)
			{
				_data.WindowMaterial.SetColor("_EmissionColor", Color.white);
			}
			else
			{
				_data.WindowMaterial.SetColor("_EmissionColor", Color.black);
			}
			_hudPresenter.SetActionsCountActive(visualsByTimeOfDay.HasActionsCount);
			if (!IsEndingDay)
			{
				_hudPresenter.InitActionsCount(isSkipAnim: true);
			}
			else
			{
				_hudPresenter.HideAction();
			}
			ChangeLightBeamMaterials();
		}

		private void SelectMusic()
		{
			if (CurrentTimeOfDay == ETimeOfDay.Night)
			{
				_soundService.EnableNightTheme(Day, 1f);
				return;
			}
			_soundService.DisableNightTheme(0f);
			_soundService.EnableDayTheme(Day, 1f);
		}

		private void ChangeLightBeamMaterials()
		{
			Material material = ((CurrentTimeOfDay == ETimeOfDay.Day) ? _data.LightBeamDayMaterial : _data.LightBeamNightMaterial);
			MeshRenderer[] lightBeamsRenderers = _viewProvider.LightBeamsRenderers;
			for (int i = 0; i < lightBeamsRenderers.Length; i++)
			{
				lightBeamsRenderers[i].material = material;
			}
		}

		private async UniTaskVoid WatchMorningTV()
		{
			await UniTask.WaitUntil(() => !_dialogManager.IsOpened);
			WatchedTV?.Invoke();
		}

		public void Act()
		{
			_saveData.DayActions--;
			_hudPresenter.InitActionsCount();
		}

		public void ActAll()
		{
			_saveData.DayActions = 0;
			_hudPresenter.InitActionsCount();
		}

		public void UpdateTimeOfDayStartedOrDialogEndedTime()
		{
			_saveData.LastChange = Time.timeSinceLevelLoad + _saveData.AccumulatedTime;
		}

		protected override void OnSaveDataLoad(IGameSaveDataHandler saver)
		{
			_saveData = saver.LoadKey(this);
			SelectMusic();
			UpdateVisualThingForSelectedTimeOfDay();
			if (CurrentTimeOfDay == ETimeOfDay.Night)
			{
				InitNewNight(isFromSaveData: true);
			}
			else
			{
				InitNewDay(isFromSaveData: true);
			}
			if (Day == _saveData.LastCourierOrderedDay && CurrentTimeOfDay == ETimeOfDay.Day)
			{
				_gameEventsManager.GenerateCharacterDingDongEventsForDay(Day, ECharacterType.Courier);
			}
			if (_saveData.IsNeedToRepairAfterBodyeater)
			{
				BodyEaterAppeared?.Invoke();
			}
			LinkSaveDataKey(this, _saveData, _dataModelService.GameSaveDataHandler);
		}

		private void OnSave(bool isReserve)
		{
			_saveData.AccumulatedTime += Time.timeSinceLevelLoad;
		}

		public void AddChangePosTomorrow(ECharacterType character, ERoomPeopleState pose)
		{
			_saveData.ChangePosesTomorrow.Add(new ChangePoseData(character, pose));
		}
	}
}
