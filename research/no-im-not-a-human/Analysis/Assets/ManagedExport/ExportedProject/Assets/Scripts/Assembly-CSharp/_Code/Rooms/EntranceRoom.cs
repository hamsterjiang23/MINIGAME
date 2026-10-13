using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using Zenject;
using _Code.Characters;
using _Code.DialogSystem;
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
using _Other.SimpleBalancer;
using _Scripts.Services.DataModel;
using _Scripts.Services.Sound;
using _Scripts.Services.Sound.Service;

namespace _Code.Rooms
{
	public sealed class EntranceRoom : ARoom, ITickable
	{
		private readonly IDialogManager _dialogManager;

		private bool _isSomeoneKnockingNow;

		private ESound _knockSound;

		private float _lastKnockTime;

		private float _knockDelay;

		private KnockTimeBalanceEnvironment _environment = new KnockTimeBalanceEnvironment();

		private bool _hasWatched;

		private IDataModelService _dataModelService;

		private ECharacterType _currentCharacter;

		private ICharactersManager _charactersManager;

		private float _noiseLevel;

		public override ERoom RoomType => ERoom.Entrance;

		public EntranceRoom(IDayNightController dayNightController, IOtherGameSODataProvider otherGameSoDataProvider, IDialogManager dialogManager, IGameEventsManager gameEventsManager, ICloseUpsController closeUpsController, ICursorController cursorController, INotAHumanSoundService soundService, IGameplayEndingManager gameplayEndingManager, IHUDPresenter hudPresenter, ICharactersManager charactersManager, IDataModelService dataModelService, IStateObjectController stateObjectController)
			: base(dayNightController, otherGameSoDataProvider, dialogManager, gameEventsManager, closeUpsController, cursorController, soundService, gameplayEndingManager, hudPresenter, charactersManager, dataModelService, stateObjectController)
		{
			DayNightController.DayChanged -= OnDayChanged;
			DayNightController.DayChanged += OnDayChanged;
		}

		private void OnDayChanged(int day)
		{
			UpdatePictureForDay(day);
		}

		public void UpdatePictureForDay(int day)
		{
			if (day == 6)
			{
				((EntranceRoomView)View).BurnBackground();
			}
			if (day >= 7)
			{
				((EntranceRoomView)View).DestroyedBackground();
			}
		}

		public void InitModules(IDataModelService dataModelService, ICharactersManager charactersManager)
		{
			_dataModelService = dataModelService;
			_charactersManager = charactersManager;
		}

		private new string GetDialogName(CharacterSOData character)
		{
			if (character.CharacterType == ECharacterType.Courier)
			{
				return "entrance_" + character.CharacterType.ToString().ToLower() + "_" + 1;
			}
			if (character.IsPreset)
			{
				return "entrance_" + character.CharacterType.ToString().ToLower() + "_" + DayNightController.Day;
			}
			return "entrance_" + character.CharacterType.ToString().ToLower();
		}

		public void ShowFullSizeCharacter(CharacterSOData character, string conditionName)
		{
			_currentCharacter = character.CharacterType;
			_knockSound = character.KnockSound;
			_isSomeoneKnockingNow = true;
			_environment = new KnockTimeBalanceEnvironment();
			_environment.AddRandomSetting(EKnockTimeBalanceEnvironment.KnockTime, new BalanceRandomFloatSetting((min: OtherGameSoDataProvider.OtherGameData.StartKnockTimeMin, max: OtherGameSoDataProvider.OtherGameData.StartKnockTimeMax), (min: OtherGameSoDataProvider.OtherGameData.EndKnockTimeMin, max: OtherGameSoDataProvider.OtherGameData.EndKnockTimeMax))).SetMaxTime(OtherGameSoDataProvider.OtherGameData.KnockTimeProgressDuration).SetStartTime(Time.timeSinceLevelLoad);
			DialogManager.DialogStarted -= OnDialogStarted;
			DialogManager.DialogStarted += OnDialogStarted;
			DialogManager.DialogEnded -= OnDialogEnded2;
			DialogManager.DialogEnded += OnDialogEnded2;
			View.Blink(() =>
			{
				DialogManager.AddOnDialogEndedAction(() =>
				{
					GameEventsManager.CompleteEvent(conditionName);
				});
				string dialogName = GetDialogName(character);
				DialogManager.RunDialog(character, dialogName, EDialogOverlayType.Peephole, View.LinkedCamera);
			});
		}

		public void CharacerWillCumSoon()
		{
			base.SuspendedExit = true;
		}

		private void OnDialogStarted()
		{
			_dataModelService.MetaPrefsDataHandler.MeetCharacter(_currentCharacter);
			DialogManager.DialogStarted -= OnDialogStarted;
			_isSomeoneKnockingNow = false;
			_environment = null;
			ESound entranceTheme = _charactersManager.GetCharacter(_currentCharacter).EntranceTheme;
			if (entranceTheme != ESound.None)
			{
				_noiseLevel = SoundService.GetVolume(ESoundSource.s1_GameScene_2DMusicSourceNoise);
				SoundService.FadeVolume(ESoundSource.s1_GameScene_2DMusicSourceNoise, 0f, 1.5f, Ease.InQuad);
				SoundService.FadeVolume(ESoundSource.s1_GameScene_2DMusicSourceNight, 0f, 1.5f, Ease.InQuad);
				SoundService.FadeIn(ESoundSource.s1_GameScene_2DMusicSourceDay, entranceTheme, 1.5f, Ease.InQuad, new SoundSettingCycling());
				DialogManager.DialogEnded += OnDialogEnded;
			}
		}

		private void OnDialogEnded(bool endedDialog, bool endedSubtitle)
		{
			DialogManager.DialogEnded -= OnDialogEnded;
			SoundService.FadeVolume(ESoundSource.s1_GameScene_2DMusicSourceNight, 1f, 1.5f, Ease.OutQuad);
			SoundService.FadeVolume(ESoundSource.s1_GameScene_2DMusicSourceNoise, _noiseLevel, 1.5f, Ease.OutQuad);
			SoundService.FadeOut(ESoundSource.s1_GameScene_2DMusicSourceDay, 1.5f, Ease.OutQuad);
		}

		private void OnDialogEnded2(bool endedDialog, bool endedSubtitle)
		{
			DialogManager.DialogEnded -= OnDialogEnded2;
			base.SuspendedExit = false;
		}

		public void Tick()
		{
			if (_isSomeoneKnockingNow && _lastKnockTime + _knockDelay < Time.time)
			{
				_lastKnockTime = Time.time;
				_knockDelay = _environment.GetRandomValue(EKnockTimeBalanceEnvironment.KnockTime);
				SoundService.PlayOneShot(ESoundSource.s1_GameScene_KnockSource, _knockSound);
			}
		}

		public async UniTaskVoid OnLoad()
		{
			await UniTask.WaitUntil(() => DayNightController != null);
			UpdatePictureForDay(DayNightController.Day);
		}

		protected override void EnterInner()
		{
			base.SuspendedExit = false;
			if (DayNightController.Day == 6)
			{
				SteamAchievements.Unlock("CITY_VIEW");
			}
		}
	}
}
