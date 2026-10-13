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
using _Code.Infrastructure.Sound.Settings;
using _Code.Menues.HUD;
using _Code.Player;
using _Code.Rooms;
using _Code.Utils;
using _Scripts.Services.Sound.Service;

public sealed class DoorTrigger : AActionableObjectView
{
	[SerializeField]
	private Transform _doorPivot;

	[SerializeField]
	private float _openAngle;

	[SerializeField]
	private float _slightOpenAngle = 10f;

	[SerializeField]
	private float _duration;

	[SerializeField]
	private ERoom _linkedRoom;

	[SerializeField]
	private Collider _doorCollider;

	[SerializeField]
	[Range(0.1f, 5f)]
	private float _closeSpeedModifier;

	private ESound[] _openSounds = new ESound[4]
	{
		ESound.House_Doors_Open_01,
		ESound.House_Doors_Open_02,
		ESound.House_Doors_Open_03,
		ESound.House_Doors_Open_04
	};

	private ESound[] _closeSounds = new ESound[4]
	{
		ESound.House_Doors_Close_01,
		ESound.House_Doors_Close_02,
		ESound.House_Doors_Close_03,
		ESound.House_Doors_Close_04
	};

	private float volumeMultiplier = 0.5f;

	private IRoom _room;

	private Vector3 _startRotation;

	private bool _isStarted;

	private WatcherManager _watcherManager;

	private IRoom Room
	{
		get
		{
			if (_room == null)
			{
				_room = RoomsManager.GetRoom(_linkedRoom);
			}
			return _room;
		}
	}

	protected override bool CanLeave
	{
		get
		{
			if (!Room.HasDialogNow && DayNightController.CanLeaveRooms)
			{
				return !CloseUpsController.IsAnyCloseUpActive;
			}
			return false;
		}
	}

	protected override Func<UniTask> ExtraActionIn => async () =>
	{
		SoundService.Play(_soundSource, _openSounds.GetRandomElementWithoutRepeating());
		SoundService.FadeVolume(_soundSource, volumeMultiplier, 0f).Forget();
		Room.Enter();
		Vibrator.VibrateForTime(_duration / 2f, 0.003f, 0f).Forget();
		await (Tween)_doorPivot.DORotate(new Vector3(_startRotation.x, _startRotation.y + _openAngle, _startRotation.z), _duration).SetEase(Ease.OutCubic);
		PlayerService.IsMouseEnabled = true;
		_watcherManager.EnterState(EWatcherState.Room);
		InputHandler.SetGamepadCursorActiveState(active: true);
	};

	protected override Func<UniTask> ExtraActionOut => async () =>
	{
		SoundService.Play(_soundSource, _closeSounds.GetRandomElementWithoutRepeating());
		SoundService.FadeVolume(_soundSource, volumeMultiplier, 0f).Forget();
		PlayerService.IsMouseEnabled = false;
		_watcherManager.LeaveState(EWatcherState.Room);
		InputHandler.SetGamepadCursorActiveState(active: false);
		Vibrator.VibrateForTime(_duration / 2f, 0.003f, 0f).Forget();
		await (Tween)_doorPivot.DORotate(_startRotation, _duration / _closeSpeedModifier).SetEase(Ease.InSine);
		Room.Leave();
	};

	protected override Func<UniTask> ExtraActionInE => async () =>
	{
		_doorCollider.enabled = false;
		SoundService.Play(_soundSource, ESound.Drift, new SoundSettingRandomPitch(0.1f));
		Room.Enter();
		await (Tween)_doorPivot.DORotate(new Vector3(_startRotation.x + 360f, _startRotation.y + _openAngle, _startRotation.z), _duration, RotateMode.FastBeyond360).SetEase(Ease.InSine);
		PlayerService.IsMouseEnabled = true;
		_doorCollider.enabled = true;
	};

	protected override Func<UniTask> ExtraActionOutE => async () =>
	{
		SoundService.Play(_soundSource, ESound.Drift, new SoundSettingRandomPitch(0.1f));
		PlayerService.IsMouseEnabled = false;
		await (Tween)_doorPivot.DORotate(_startRotation + new Vector3(360f, 0f, 0f), _duration / _closeSpeedModifier, RotateMode.FastBeyond360).SetEase(Ease.InSine);
		Room.Leave();
	};

	private void Awake()
	{
		_startRotation = _doorPivot.rotation.eulerAngles;
	}

	public override void Init(IHUDPresenter hudPresenter, IDayNightController dayNightController, IRoomsManager roomsManager, IGameEventsManager gameEventsManager, ICloseUpsController closeUpsController, IPlayerService playerService, ICursorController cursorController, IPauseController pauseController, INotAHumanSoundService soundService, IInputHandlerProvider inputHandlerProvider, WatcherManager watcherManager)
	{
		_watcherManager = watcherManager;
		base.Init(hudPresenter, dayNightController, roomsManager, gameEventsManager, closeUpsController, playerService, cursorController, pauseController, soundService, inputHandlerProvider, watcherManager);
		RoomsManager.LinkActionableToRoom(_linkedRoom, this);
	}

	protected override void ExtraActionBeforeOut()
	{
		Room.DisableObjects();
	}

	protected override void ExtraActionBeforeIn()
	{
		Room.EnableObjects();
	}

	private void Start()
	{
		_isStarted = true;
	}

	public void SlightlyOpenDoor()
	{
		SlightlyOpenDoorAsync().Forget();
	}

	private async UniTaskVoid SlightlyOpenDoorAsync()
	{
		await UniTask.WaitUntil(() => _isStarted);
		if (!IsLooking)
		{
			_doorCollider.transform.eulerAngles = new Vector3(_startRotation.x, _startRotation.y + _slightOpenAngle, _startRotation.z);
		}
	}
}
