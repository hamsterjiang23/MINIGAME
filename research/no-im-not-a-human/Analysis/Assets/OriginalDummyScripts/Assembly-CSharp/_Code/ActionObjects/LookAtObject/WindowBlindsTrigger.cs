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

namespace _Code.ActionObjects.LookAtObject;

public sealed class WindowBlindsTrigger : AActionableObjectView
{
	[SerializeField]
	private Transform[] _lines;

	[SerializeField]
	private Vector3 _rotateVector;

	[SerializeField]
	private float _duration;

	[SerializeField]
	private Material _windowMaterial;

	[SerializeField]
	private Material _blindsMaterial;

	[SerializeField]
	private float _blindsAlphaOnLookingIn;

	[SerializeField]
	private WindowView _linkedWindow;

	private Color _baseBlindsColor;

	private Vector3 _baseAngle;

	private WatcherManager _watcherManager;

	protected override bool CanLeave => _linkedWindow.CanLeave;

	protected override Func<UniTask> ExtraActionIn => async () =>
	{
		_watcherManager.EnterState(EWatcherState.Window);
		_linkedWindow.StartOpen();
		SoundService.Play(_soundSource, ESound.Blinds);
		_windowMaterial.DOColor(Color.clear, _duration / 2f);
		_blindsMaterial.DOColor(new Color(_baseBlindsColor.r, _baseBlindsColor.g, _baseBlindsColor.b, _blindsAlphaOnLookingIn), _duration / 2f);
		Transform[] lines = _lines;
		foreach (Transform obj in lines)
		{
			obj.DOLocalRotate(obj.rotation.eulerAngles + _rotateVector, _duration);
		}
		Vibrator.VibrateForTime(_duration / 2f, 0f, 0.1f).Forget();
		await UniTask.Delay(TimeSpan.FromSeconds(_duration));
		_linkedWindow.FinishOpen();
	};

	protected override Func<UniTask> ExtraActionOut => async () =>
	{
		_watcherManager.LeaveState(EWatcherState.Window);
		SoundService.Play(_soundSource, ESound.Blinds);
		_windowMaterial.DOColor(Color.black, _duration / 2f);
		_blindsMaterial.DOColor(_baseBlindsColor, _duration / 2f);
		Transform[] lines = _lines;
		foreach (Transform obj in lines)
		{
			obj.DOLocalRotate(obj.rotation.eulerAngles + _rotateVector, _duration);
		}
		Vibrator.VibrateForTime(_duration / 2f, 0f, 0.1f).Forget();
		await UniTask.Delay(TimeSpan.FromSeconds(_duration));
		_linkedWindow.Close();
	};

	protected override Func<UniTask> ExtraActionInE => async () =>
	{
		_linkedWindow.StartOpen();
		SoundService.Play(_soundSource, ESound.Drift);
		_windowMaterial.DOColor(Color.clear, _duration / 2f);
		_blindsMaterial.DOColor(new Color(_baseBlindsColor.r, _baseBlindsColor.g, _baseBlindsColor.b, _blindsAlphaOnLookingIn), _duration / 2f);
		Transform[] lines = _lines;
		foreach (Transform obj in lines)
		{
			obj.DOLocalRotate(obj.rotation.eulerAngles + new Vector3(0f, 180f, 0f), _duration);
		}
		await UniTask.Delay(TimeSpan.FromSeconds(_duration));
	};

	protected override Func<UniTask> ExtraActionOutE => async () =>
	{
		SoundService.Play(_soundSource, ESound.Drift);
		_windowMaterial.DOColor(Color.black, _duration / 2f);
		_blindsMaterial.DOColor(_baseBlindsColor, _duration / 2f);
		Transform[] lines = _lines;
		for (int i = 0; i < lines.Length; i++)
		{
			lines[i].DOLocalRotate(_baseAngle, _duration);
		}
		await UniTask.Delay(TimeSpan.FromSeconds(_duration));
		_linkedWindow.Close();
	};

	private void Awake()
	{
		_baseAngle = _lines[0].eulerAngles;
		_baseBlindsColor = _blindsMaterial.color;
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
