using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using _Code.Events;
using _Code.Infrastructure.DayNight;
using _Code.Infrastructure.Locations;
using _Code.Infrastructure.Pause;
using _Code.Infrastructure.Player;
using _Code.Menues.HUD;

namespace _Code.Infrastructure
{
	public sealed class HatchInteractable : AInteractableObject
	{
		[SerializeField]
		private bool _isInsideBasement;

		[SerializeField]
		private Collider _collider;

		private readonly Vector3 _startRotation = new Vector3(90f, -90f, -270f);

		private readonly Vector3 _endRotation = new Vector3(-60f, -90f, -270f);

		private readonly float _rotationDuration = 1f;

		private bool _isOpened;

		private ILocationsManager _locationsManager;

		private IDayNightController _dayNightController;

		private bool _isInteracting;

		private IPlayerService _playerService;

		private IHUDPresenter _hudPresenter;

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

		public void Init(ILocationsManager locationsManager, IHUDPresenter hudPresenter, IPauseController pauseController, IDayNightController dayNightController, IPlayerService playerService)
		{
			_hudPresenter = hudPresenter;
			_dayNightController = dayNightController;
			_locationsManager = locationsManager;
			_playerService = playerService;
			_raycastTarget.Init(hudPresenter, pauseController, () => HardConditions);
		}

		public override void Interact()
		{
			InteractAsync().Forget();
		}

		private async UniTask InteractAsync()
		{
			if (_isInteracting)
			{
				return;
			}
			_playerService.MakeImmovable();
			_raycastTarget.SetLockedState(isLocked: true);
			_hudPresenter.HideHint().Forget();
			_isInteracting = true;
			_collider.isTrigger = true;
			if (!_isInsideBasement)
			{
				await (Tween)DOTween.To(() => transform.eulerAngles, (Vector3 x) =>
				{
					transform.eulerAngles = new Vector3(x.x, -90f, -270f);
				}, _endRotation, _rotationDuration);
			}
			await _locationsManager.GoToLocation((!_isInsideBasement) ? ELocation.Basement : ELocation.HouseInterior);
			if (!_isInsideBasement)
			{
				transform.eulerAngles = _startRotation;
			}
			_collider.isTrigger = false;
			_isInteracting = false;
			_raycastTarget.SetLockedState(isLocked: false);
			_playerService.MakeMovable();
		}
	}
}
