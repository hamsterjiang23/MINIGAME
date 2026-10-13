using System;
using Cysharp.Threading.Tasks;
using RoboRyanTron.SearchableEnum;
using UnityEngine;
using _Code.Events;
using _Code.Infrastructure.DayNight;
using _Code.Infrastructure.Pause;
using _Code.Infrastructure.Player;
using _Code.Infrastructure.Sound;
using _Code.Infrastructure.StateObjects;
using _Code.Menues.HUD;
using _Code.Menues.HUD.Animations;
using _Scripts.Services.Sound;
using _Scripts.Services.Sound.Service;

namespace _Code.Infrastructure;

public sealed class TheHoleInteractable : AInteractableObject
{
	[SerializeField]
	[SearchableEnum]
	private ESound[] _diggingSounds;

	private IStateObjectController _stateObjectController;

	private IHUDPresenter _hudPresenter;

	private IPlayerService _playerService;

	private IDayNightController _dayNightController;

	private INotAHumanSoundService _soundService;

	private bool _isInteracting;

	public override bool HardConditions => _dayNightController.CurrentTimeOfDay == ETimeOfDay.Day;

	public override bool SoftConditions => _dayNightController.DayActions >= EnergyCost;

	protected override int EnergyCost => 1;

	public event Action HoleReady;

	public void Init(IHUDPresenter hudPresenter, IPauseController pauseController, IStateObjectController stateObjectController, IPlayerService playerService, IDayNightController dayNightController, INotAHumanSoundService soundService)
	{
		_dayNightController = dayNightController;
		_raycastTarget.Init(hudPresenter, pauseController, () => HardConditions);
		_hudPresenter = hudPresenter;
		_stateObjectController = stateObjectController;
		_playerService = playerService;
		_soundService = soundService;
	}

	public override void Interact()
	{
		if (!_isInteracting)
		{
			InteractAsync().Forget();
		}
	}

	private async UniTask InteractAsync()
	{
		_soundService.PlayOneShot(ESoundSource.s1_GameScene_2DSFXSource, _diggingSounds.GetRandomElement());
		_isInteracting = true;
		_hudPresenter.HideHint().Forget();
		_raycastTarget.SetLockedState(isLocked: true);
		_playerService.MakeImmovable();
		await _hudPresenter.FadeIn(0.5f);
		_stateObjectController.IncrementState(EStateObjectType.Ground);
		if (_stateObjectController.GetState(EStateObjectType.Ground) == 3)
		{
			_stateObjectController.SetState(EStateObjectType.HoleInteract, 0);
			HoleReady?.Invoke();
		}
		_dayNightController.ActAll();
		await UniTask.Delay(TimeSpan.FromSeconds(1.0));
		_hudPresenter.PlayAnimation(EHUDAnimation.WatchDirtyHands).Forget();
		await _hudPresenter.FadeOut(0.5f);
		_raycastTarget.SetLockedState(isLocked: false);
		_playerService.MakeMovable();
		_playerService.Signs.Dig();
		await UniTask.DelayFrame(1);
		_isInteracting = false;
	}
}
