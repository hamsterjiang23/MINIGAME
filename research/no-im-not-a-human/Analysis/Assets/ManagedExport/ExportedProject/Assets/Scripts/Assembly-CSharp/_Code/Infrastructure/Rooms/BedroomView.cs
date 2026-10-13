using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Video;
using _Code.Characters.DialogSystem;
using _Code.DialogSystem;
using _Code.Events;
using _Code.Infrastructure.DayNight;
using _Code.Infrastructure.Endings;
using _Code.Infrastructure.Endings.Gameplay;
using _Code.Infrastructure._NINAH__Rooms;
using _Code.Rooms;
using _Code.Utils.UI.ImageAnimating;

namespace _Code.Infrastructure.Rooms
{
	public sealed class BedroomView : ARoomView
	{
		[SerializeField]
		private CharacterRoomObjectView _tv;

		[SerializeField]
		private UIButton _tvUiButton;

		[SerializeField]
		private CharacterRoomObjectView _sexy;

		[SerializeField]
		private ObjectRoomObjectView _bed;

		[SerializeField]
		private VideoPlayer _videoPlayer;

		[SerializeField]
		private GameObject _screen;

		[SerializeField]
		private AnimatedImage _babyCurtain;

		[SerializeField]
		private UIButton _babyCurtainButton;

		[SerializeField]
		private UIButton _mushroomlistButton;

		[SerializeField]
		private UIButton _clock;

		private IGameplayEndingManager _gameplayEndingManager;

		private IDayNightController _dayNightController;

		private IDialogManager _dialogManager;

		private RoomsSaveData _saveData;

		public event Action BabyActivated;

		public void Init(Action<CharacterRoomObjectView, bool> tvClickedAction, IGameplayEndingManager gameplayEndingManager, IDialogManager dialogManager, RoomsSaveData roomsSaveData)
		{
			_saveData = roomsSaveData;
			_gameplayEndingManager = gameplayEndingManager;
			_dialogManager = dialogManager;
			_tv.DialogStarted -= tvClickedAction;
			_tv.DialogStarted += tvClickedAction;
			DayNightController.Changed -= OnTimeOfDayChanged;
			DayNightController.Changed += OnTimeOfDayChanged;
			DayNightController.WentToBed -= OnWentToBed;
			DayNightController.WentToBed += OnWentToBed;
			DayNightController.WokeUp -= OnWokeUp;
			DayNightController.WokeUp += OnWokeUp;
			_mushroomlistButton.SetAction(() =>
			{
				DisableObjects();
				CloseUpsController.Mushroomlist.Show().Forget();
				CloseUpsController.Mushroomlist.Closed += OnListClosed;
			});
			_clock.SetAction(() =>
			{
				_dialogManager.ShowSubtitlePopup(EInfoMessageType.Clocks);
				((GameplayEndingMushroom)_gameplayEndingManager.GetEnding(EEnding.Mushroom)).WatchClock();
			});
		}

		private void OnListClosed()
		{
			CloseUpsController.Mushroomlist.Closed -= OnListClosed;
			EnableObjects();
		}

		private void OnWentToBed()
		{
			UIButton[] uiButtons = _uiButtons;
			for (int i = 0; i < uiButtons.Length; i++)
			{
				uiButtons[i].IsActive = false;
			}
		}

		private void OnWokeUp()
		{
			UIButton[] uiButtons = _uiButtons;
			for (int i = 0; i < uiButtons.Length; i++)
			{
				uiButtons[i].IsActive = true;
			}
		}

		private void OnTimeOfDayChanged(ETimeOfDay timeOfDay)
		{
			if (_sexy.gameObject.activeSelf)
			{
				UpdateSexyState();
			}
			if (timeOfDay == ETimeOfDay.Day)
			{
				_sexy.EnableButton();
			}
			else
			{
				_sexy.DisableButton();
			}
		}

		public void ActivateBaby()
		{
			SteamAchievements.Unlock("SPEC_SEXY_PREG");
			_babyCurtain.gameObject.SetActive(value: true);
			_babyCurtain.StartAnimationOrSetFirstFrame();
			_babyCurtainButton.SetAction(() =>
			{
				_babyCurtain.gameObject.SetActive(value: false);
				((GameplayEndingBaby)_gameplayEndingManager.GetEnding(EEnding.Baby)).FindBaby(ERoom.Bedroom);
			});
		}

		public void GrowUpBelly()
		{
			_saveData.BellyProgress++;
			UpdateSexyState();
			if (_saveData.BellyProgress == 4)
			{
				BabyActivated?.Invoke();
				ActivateBaby();
				_sexy.SelectState(ERoomPeopleState.Corpse);
			}
		}

		private void UpdateSexyState()
		{
			int num = ((DayNightController.CurrentTimeOfDay != ETimeOfDay.Day) ? _saveData.BellyProgress : ((_saveData.BellyProgress >= 4) ? _saveData.BellyProgress : 0));
			CharacterRoomObjectView sexy = _sexy;
			sexy.SelectState(num switch
			{
				0 => ERoomPeopleState.Lying, 
				1 => ERoomPeopleState.Pregnant1, 
				2 => ERoomPeopleState.Pregnant2, 
				3 => ERoomPeopleState.Pregnant3, 
				_ => ERoomPeopleState.Corpse, 
			});
		}

		public void SetTVActive(bool isActive)
		{
			_tvUiButton.IsActive = isActive;
		}

		public void DisableSexyButton()
		{
			_sexy.DisableButton();
		}

		public async UniTaskVoid OnLoad(RoomsSaveData saveData)
		{
			_saveData = saveData;
			await UniTask.WaitUntil(() => DayNightController != null);
			if (saveData.BellyProgress > 0)
			{
				UpdateSexyState();
			}
			else if (DayNightController.CurrentTimeOfDay == ETimeOfDay.Day)
			{
				_sexy.EnableButton();
			}
			if (_saveData.BellyProgress < 4)
			{
				return;
			}
			await UniTask.WaitUntil(() => _gameplayEndingManager != null);
			if (!((GameplayEndingBaby)_gameplayEndingManager.GetEnding(EEnding.Baby)).IsBedroomBabyFound)
			{
				SteamAchievements.Unlock("SPEC_SEXY_PREG");
				_babyCurtain.gameObject.SetActive(value: true);
				_babyCurtain.StartAnimationOrSetFirstFrame();
				_babyCurtainButton.SetAction(() =>
				{
					_babyCurtain.gameObject.SetActive(value: false);
					((GameplayEndingBaby)_gameplayEndingManager.GetEnding(EEnding.Baby)).FindBaby(ERoom.Bedroom);
				});
			}
		}
	}
}
