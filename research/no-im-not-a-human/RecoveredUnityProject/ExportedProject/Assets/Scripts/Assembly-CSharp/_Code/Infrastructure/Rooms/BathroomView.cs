using Cysharp.Threading.Tasks;
using UnityEngine;
using _Code.Characters;
using _Code.DialogSystem;
using _Code.Infrastructure.DayNight;
using _Code.Infrastructure.Endings;
using _Code.Infrastructure.Endings.Gameplay;
using _Code.Infrastructure._NINAH__Rooms;
using _Code.Rooms;

namespace _Code.Infrastructure.Rooms
{
	public sealed class BathroomView : ARoomView
	{
		[SerializeField]
		private CharacterSOData _donorForPlayerSigns;

		[SerializeField]
		private UIButton _sink;

		[SerializeField]
		private UIButton _baby;

		private IGameplayEndingManager _gameplayEndingManager;

		private IDialogManager _dialogManager;

		private IDayNightController _dayNightController;

		private bool _isStarted;

		private bool _isTalkingWithMirror;

		public void Init(IGameplayEndingManager endingManager, IDialogManager dialogManager, IDayNightController dayNightController)
		{
			_gameplayEndingManager = endingManager;
			_dialogManager = dialogManager;
			_dayNightController = dayNightController;
			dayNightController.DayChanged -= OnDayChanged;
			dayNightController.DayChanged += OnDayChanged;
			_sink.IsActive = false;
			_sink.SetAction(SelfCheck);
		}

		private void OnDayChanged(int day)
		{
			if (day == 2)
			{
				_sink.IsActive = true;
			}
		}

		public void ActivateBaby()
		{
			_baby.gameObject.SetActive(value: true);
			_baby.SetAction(() =>
			{
				((GameplayEndingBaby)_gameplayEndingManager.GetEnding(EEnding.Baby)).FindBaby(ERoom.Bathroom);
				_baby.gameObject.SetActive(value: false);
			});
		}

		private void SelfCheck()
		{
			_isTalkingWithMirror = true;
			_sink.IsActive = false;
			_dialogManager.RunDialog(_donorForPlayerSigns, "selfcheck", EDialogOverlayType.None, LinkedCamera);
			_dialogManager.DialogEnded += OnDialogEnded;
			HasDialogNow = true;
			DisableObjects();
		}

		protected override void OnDialogShowed()
		{
			if (!_isTalkingWithMirror)
			{
				base.OnDialogShowed();
			}
		}

		protected override void OnDialogHidden(bool isEndedDialog, bool isEndedSubtitle)
		{
			if (!_isTalkingWithMirror)
			{
				base.OnDialogHidden(isEndedDialog, isEndedSubtitle);
			}
		}

		private void OnDialogEnded(bool endedDialog, bool endedSubtitle)
		{
			HasDialogNow = false;
			_dialogManager.DialogEnded -= OnDialogEnded;
			_sink.IsActive = true;
			_isTalkingWithMirror = false;
			EnableObjects();
		}

		public async UniTaskVoid LoadSink()
		{
			await UniTask.WaitUntil(() => _dayNightController != null);
			if (_dayNightController.Day >= 2)
			{
				_sink.IsActive = true;
			}
		}

		public void OnLoad(RoomsSaveData saveData)
		{
			if (saveData.BellyProgress >= 4)
			{
				OnLoadAsync().Forget();
			}
		}

		private async UniTaskVoid OnLoadAsync()
		{
			await UniTask.WaitUntil(() => _gameplayEndingManager != null);
			if (!((GameplayEndingBaby)_gameplayEndingManager.GetEnding(EEnding.Baby)).IsBathroomBabyFound)
			{
				_baby.gameObject.SetActive(value: true);
				_baby.SetAction(() =>
				{
					((GameplayEndingBaby)_gameplayEndingManager.GetEnding(EEnding.Baby)).FindBaby(ERoom.Bathroom);
					_baby.gameObject.SetActive(value: false);
				});
			}
		}
	}
}
