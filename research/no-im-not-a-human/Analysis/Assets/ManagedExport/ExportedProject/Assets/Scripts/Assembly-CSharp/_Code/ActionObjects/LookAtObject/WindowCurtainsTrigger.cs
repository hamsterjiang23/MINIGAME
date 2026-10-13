using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using _Code.Infrastructure.ActionableObjects;
using _Code.Infrastructure.CloseUps;
using _Code.Infrastructure.Cursor;
using _Code.Infrastructure.DayNight;
using _Code.Infrastructure.GameEvents;
using _Code.Infrastructure.Pause;
using _Code.Infrastructure.Player;
using _Code.Infrastructure.Rooms;
using _Code.Infrastructure.Sound;
using _Code.Infrastructure.Windows;
using _Code.Menues.HUD;
using _Code.Player;
using _Code.Utils;
using _Scripts.Services.Sound.Service;

namespace _Code.ActionObjects.LookAtObject
{
	public sealed class WindowCurtainsTrigger : AActionableObjectView
	{
		[SerializeField]
		private Transform _leftCurtain;

		[SerializeField]
		private Transform _rightCurtain;

		[SerializeField]
		private Vector3 _moveVector;

		[SerializeField]
		private float _duration;

		[SerializeField]
		private Material _windowMaterial;

		[SerializeField]
		private WindowView _linkedWindow;

		private Vector3 _leftCurtainStartPos;

		private Vector3 _rightCurtainStartPos;

		private WatcherManager _watcherManager;

		protected override bool CanLeave => _linkedWindow.CanLeave;

		protected override Func<UniTask> ExtraActionIn => async () =>
		{
			_watcherManager.EnterState(EWatcherState.Window);
			_linkedWindow.StartOpen();
			SoundService.Play(_soundSource, ESound.Curtains);
			Vibrator.VibrateForTime(_duration / 2f, 0f, 0.1f).Forget();
			_leftCurtain.transform.DOMove(_leftCurtainStartPos - _moveVector, _duration).SetEase(Ease.InCubic);
			_rightCurtain.transform.DOMove(_rightCurtainStartPos + _moveVector, _duration).SetEase(Ease.InCubic);
			_windowMaterial.DOColor(Color.clear, _duration / 2f).SetEase(Ease.InCirc);
			await UniTask.Delay(TimeSpan.FromSeconds(_duration));
			_linkedWindow.FinishOpen();
		};

		protected override Func<UniTask> ExtraActionOut => async () =>
		{
			_watcherManager.LeaveState(EWatcherState.Window);
			SoundService.Play(_soundSource, ESound.Curtains);
			Vibrator.VibrateForTime(_duration / 2f, 0f, 0.1f).Forget();
			_leftCurtain.transform.DOMove(_leftCurtainStartPos, _duration).SetEase(Ease.OutQuint);
			_rightCurtain.transform.DOMove(_rightCurtainStartPos, _duration).SetEase(Ease.OutQuint);
			await UniTask.Delay(TimeSpan.FromSeconds(_duration / 2f));
			_windowMaterial.DOColor(Color.black, _duration / 2f);
			await UniTask.Delay(TimeSpan.FromSeconds(_duration / 2f));
			_linkedWindow.Close();
		};

		protected override Func<UniTask> ExtraActionInE { get; }

		protected override Func<UniTask> ExtraActionOutE { get; }

		private void Awake()
		{
			_leftCurtainStartPos = _leftCurtain.position;
			_rightCurtainStartPos = _rightCurtain.position;
		}

		public override void Init(IHUDPresenter hudPresenter, IDayNightController dayNightController, IRoomsManager roomsManager, IGameEventsManager gameEventsManager, ICloseUpsController closeUpsController, IPlayerService playerService, ICursorController cursorController, IPauseController pauseController, INotAHumanSoundService soundService, IInputHandlerProvider inputHandlerProvider, WatcherManager watcherManager)
		{
			_watcherManager = watcherManager;
			base.Init(hudPresenter, dayNightController, roomsManager, gameEventsManager, closeUpsController, playerService, cursorController, pauseController, soundService, inputHandlerProvider, watcherManager);
			_linkedWindow.LinkActionableObject(this);
		}

		protected override void ExtraActionBeforeIn()
		{
			base.ExtraActionBeforeIn();
			_linkedWindow.StartOfStartOpening();
		}
	}
}
