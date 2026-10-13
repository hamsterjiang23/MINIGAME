using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using _Code.Characters;
using _Code.DialogSystem;
using _Code.Events;
using _Code.Infrastructure.CloseUps;
using _Code.Infrastructure.Cursor;
using _Code.Infrastructure.DayNight;
using _Code.Infrastructure.Endings.Gameplay;
using _Code.Infrastructure.GameEvents;
using _Code.Infrastructure.OtherGameData;
using _Code.Infrastructure.Rooms;
using _Code.Infrastructure.Sound;
using _Code.Infrastructure.Sound.Settings;
using _Code.Infrastructure.StateObjects;
using _Code.Infrastructure._NINAH__Rooms;
using _Code.Menues.HUD;
using _Scripts.Services.DataModel;
using _Scripts.Services.Sound;
using _Scripts.Services.Sound.Service;

namespace _Code.Rooms
{
	public sealed class Bedroom : ARoom
	{
		private bool _isMorningTV;

		private IGameplayEndingManager _gameplayEndingManager;

		private IDataModelService _dataModelService;

		private ICharactersManager _charactersManager;

		private readonly IDayNightController _dayNightController;

		private readonly IDialogManager _dialogManager;

		private RoomsSaveData _saveData;

		private float _lastTVDialogEndedTime;

		private const float X_DIALOG_POSITION = -575f;

		public override ERoom RoomType => ERoom.Bedroom;

		private BedroomView BedroomView => (BedroomView)View;

		public bool CanWatchTV
		{
			get
			{
				if (DayNightController.Day > 0 && _saveData.WatchTVTimesToday <= OtherGameSoDataProvider.OtherGameData.TvReportageCountByDay[DayNightController.Day])
				{
					return DayNightController.CurrentTimeOfDay == ETimeOfDay.Day;
				}
				return false;
			}
		}

		public event Action BabyActivated;

		public Bedroom(IDayNightController dayNightController, IOtherGameSODataProvider otherGameSoDataProvider, IDialogManager dialogManager, IGameEventsManager gameEventsManager, ICloseUpsController closeUpsController, ICursorController cursorController, INotAHumanSoundService soundService, IGameplayEndingManager gameplayEndingManager, IHUDPresenter hudPresenter, ICharactersManager charactersManager, IDataModelService dataModelService, IStateObjectController stateObjectController)
			: base(dayNightController, otherGameSoDataProvider, dialogManager, gameEventsManager, closeUpsController, cursorController, soundService, gameplayEndingManager, hudPresenter, charactersManager, dataModelService, stateObjectController)
		{
			AwaitViewInitThenInit().Forget();
			_gameplayEndingManager = gameplayEndingManager;
			_dayNightController = dayNightController;
			_dialogManager = dialogManager;
		}

		public void InitModules(IDataModelService dataModelService, ICharactersManager charactersManager, RoomsSaveData saveData)
		{
			_dataModelService = dataModelService;
			_charactersManager = charactersManager;
			_saveData = saveData;
			_charactersManager.CharacterAdded -= OnCharacterAdded2;
			_charactersManager.CharacterAdded += OnCharacterAdded2;
		}

		private async UniTask AwaitViewInitThenInit()
		{
			await UniTask.WaitUntil(() => BedroomView != null);
			BedroomView.Init(StartTVDialog, _gameplayEndingManager, _dialogManager, _saveData);
			BedroomView.BabyActivated -= OnBabyActivated;
			BedroomView.BabyActivated += OnBabyActivated;
			_charactersManager.CharacterAdded -= OnCharacterAdded;
			_charactersManager.CharacterAdded += OnCharacterAdded;
		}

		private void OnCharacterAdded2(ECharacterType character, bool isFromSave)
		{
			if (isFromSave)
			{
				OnCharacterAddedFromLoadAsync(character).Forget();
			}
		}

		private async UniTaskVoid OnCharacterAddedFromLoadAsync(ECharacterType character)
		{
			await UniTask.WaitUntil(() => BedroomView != null);
			if (_dayNightController.CurrentTimeOfDay == ETimeOfDay.Night && character == ECharacterType.Sexy)
			{
				BedroomView.DisableSexyButton();
			}
		}

		private void OnCharacterAdded(ECharacterType character, bool isFromSave)
		{
			if (!isFromSave && _dayNightController.CurrentTimeOfDay == ETimeOfDay.Night && character == ECharacterType.Sexy)
			{
				BedroomView.DisableSexyButton();
			}
		}

		private void OnBabyActivated()
		{
			BabyActivated?.Invoke();
		}

		private void StartTVDialog(CharacterRoomObjectView character, bool ignoreIsActiveCondition = false)
		{
			if (!(Time.timeSinceLevelLoad < _lastTVDialogEndedTime + 0.2f) && (CanWatchTV || _isMorningTV) && !DayNightController.IsEndingDay && (ignoreIsActiveCondition || character.IsButtonActive))
			{
				View.SetDialogCompanion(character);
				_isMorningTV = false;
				int watchTVTimesToday = _saveData.WatchTVTimesToday;
				string text = ((_saveData.WatchTVTimesToday == 0) ? "necessary" : ("extra" + watchTVTimesToday));
				_saveData.WatchTVTimesToday++;
				string text2 = GetDialogName(character.Data) + text;
				_dataModelService.MetaPrefsDataHandler.WatchTV(text2);
				DialogManager.RunDialog(character.Data, text2, EDialogOverlayType.TV, View.LinkedCamera, new DialogViewData(-575f));
				DialogManager.AddActionForNextDialogEnded(() =>
				{
					_lastTVDialogEndedTime = Time.timeSinceLevelLoad;
					SoundService.FadeOut(ESoundSource.s1_GameScene_2DMusicSourceExtra, 0.2f, Ease.OutSine);
				});
				SoundService.FadeIn(ESoundSource.s1_GameScene_2DMusicSourceExtra, ESound.Tv, 0.3f, Ease.Unset, new SoundSettingCycling());
				CallDialogStarted();
				if (_saveData.WatchTVTimesToday > OtherGameSoDataProvider.OtherGameData.TvReportageCountByDay[DayNightController.Day])
				{
					((BedroomView)View).SetTVActive(isActive: false);
				}
			}
		}

		private new string GetDialogName(CharacterSOData character)
		{
			return "tv_" + DayNightController.Day + "_";
		}

		public void WatchMorningTV()
		{
			_saveData.WatchTVTimesToday = 0;
			_isMorningTV = true;
			SetTVActive(isActive: true);
			StartTVDialog(View.CharacterViews[0], ignoreIsActiveCondition: true);
			SetTVActive(isActive: false);
			if (OtherGameSoDataProvider.OtherGameData.TvReportageCountByDay[DayNightController.Day] > 0 && DayNightController.Day != 1)
			{
				SetTVActive(isActive: true);
			}
		}

		public void DisableTvIfExtra()
		{
			if (_saveData.WatchTVTimesToday <= OtherGameSoDataProvider.OtherGameData.TvReportageCountByDay[DayNightController.Day])
			{
				((BedroomView)View).SetTVActive(isActive: false);
			}
		}

		public void GrowUpBelly()
		{
			BedroomView.GrowUpBelly();
		}

		public void SetTVActive(bool isActive)
		{
			((BedroomView)View).SetTVActive(isActive);
		}

		public async UniTask OnLoad(RoomsSaveData saveData)
		{
			await UniTask.WaitUntil(() => DayNightController != null);
			_saveData = saveData;
			if (CanWatchTV)
			{
				SetTVActive(isActive: true);
			}
		}
	}
}
