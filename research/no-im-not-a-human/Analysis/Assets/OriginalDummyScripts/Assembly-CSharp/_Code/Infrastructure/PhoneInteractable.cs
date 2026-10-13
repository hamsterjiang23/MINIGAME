using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using _Code.Events;
using _Code.Infrastructure.CloseUps;
using _Code.Infrastructure.DayNight;
using _Code.Infrastructure.Pause;
using _Code.Menues.HUD;

namespace _Code.Infrastructure;

public sealed class PhoneInteractable : AInteractableObject
{
	[SerializeField]
	private GameObject _receiver;

	private ICloseUpsController _closeUpsController;

	private IHUDPresenter _hudPresenter;

	private IDayNightController _dayNightController;

	public override bool HardConditions
	{
		get
		{
			if (_dayNightController.CurrentTimeOfDay == ETimeOfDay.Day)
			{
				return _dayNightController.Day < 14;
			}
			return false;
		}
	}

	public override bool SoftConditions => true;

	public event Action Entered;

	public event Action Exited;

	public void Init(ICloseUpsController closeUpsController, IHUDPresenter hudPresenter, IPauseController pauseController, IDayNightController dayNightController)
	{
		_dayNightController = dayNightController;
		_closeUpsController = closeUpsController;
		_hudPresenter = hudPresenter;
		_raycastTarget.Init(hudPresenter, pauseController, () => HardConditions);
		_closeUpsController.Phone.Left -= ClosePhone;
		_closeUpsController.Phone.Left += ClosePhone;
	}

	private void ClosePhone()
	{
		_raycastTarget.SetLockedState(isLocked: false);
		_receiver.SetActive(value: true);
		Exited?.Invoke();
	}

	public override void Interact()
	{
		if (!_closeUpsController.Phone.IsAnimating)
		{
			_receiver.SetActive(value: false);
			_hudPresenter.HideHint();
			_raycastTarget.SetLockedState(isLocked: true);
			_closeUpsController.Phone.Show().Forget();
			Entered?.Invoke();
		}
	}
}
