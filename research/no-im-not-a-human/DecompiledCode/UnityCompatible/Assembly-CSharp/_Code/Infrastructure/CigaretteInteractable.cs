using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Localization;
using _Code.Events;
using _Code.Infrastructure.CloseUps;
using _Code.Infrastructure.Consumables;
using _Code.Infrastructure.DayNight;
using _Code.Infrastructure.Pause;
using _Code.Infrastructure.Player;
using _Code.Infrastructure.Sound;
using _Code.Menues.HUD;
using _Code.Menues.HUD.Animations;
using _Scripts.Services.Sound;
using _Scripts.Services.Sound.Service;

namespace _Code.Infrastructure
{
	public sealed class CigaretteInteractable : AInteractableObject
	{
		[SerializeField]
		private GameObject _cinemachineCam;

		[SerializeField]
		private GameObject _cigarettes;

		[SerializeField]
		private LocalizedString _localizedStringCigarettes;

		private INotAHumanSoundService _soundService;

		private IHUDPresenter _hudPresenter;

		private IConsumablesController _consumablesController;

		private IDayNightController _dayNightController;

		private ICloseUpsController _closeUpsController;

		private IPlayerService _playerService;

		private bool _isStarted;

		private bool _isInteracting;

		private const float SMOKE_DELAY = 2.5f;

		public override bool HardConditions
		{
			get
			{
				if (_consumablesController.Count(EConsumable.Cigarette) > 0 && _dayNightController.CurrentTimeOfDay == ETimeOfDay.Day)
				{
					return _dayNightController.Day < 14;
				}
				return false;
			}
		}

		public override bool SoftConditions => true;

		public void Init(IHUDPresenter hudPresenter, IPauseController pauseController, IConsumablesController consumablesController, IDayNightController dayNightController, ICloseUpsController closeUpsController, IPlayerService playerService, INotAHumanSoundService soundService)
		{
			_hudPresenter = hudPresenter;
			_consumablesController = consumablesController;
			_dayNightController = dayNightController;
			_closeUpsController = closeUpsController;
			_playerService = playerService;
			_soundService = soundService;
			_raycastTarget.Init(hudPresenter, pauseController, () => HardConditions);
			UpdateString();
			_consumablesController.UpdatedCount -= OnUpdatedConsumables;
			_consumablesController.UpdatedCount += OnUpdatedConsumables;
		}

		private void UpdateString()
		{
			_localizedStringCigarettes.Arguments = new List<object> { _consumablesController.Count(EConsumable.Cigarette) };
			_localizedStringCigarettes.RefreshString();
			_raycastTarget.SetSubjectLocalizedString(_localizedStringCigarettes);
		}

		private void OnDestroy()
		{
			_consumablesController.UpdatedCount -= OnUpdatedConsumables;
		}

		private void OnUpdatedConsumables(EConsumable consumable, int count)
		{
			if (consumable == EConsumable.Cigarette)
			{
				_localizedStringCigarettes.Arguments = new List<object> { _consumablesController.Count(EConsumable.Cigarette) };
				_localizedStringCigarettes.RefreshString();
				_cigarettes.SetActive(count > 0);
			}
		}

		public override void Interact()
		{
			if (!_isInteracting && !_closeUpsController.Consumable.IsAnimating)
			{
				_cinemachineCam?.SetActive(value: true);
				_raycastTarget.SetLockedState(isLocked: true);
				_closeUpsController.Consumable.SetupConsumable(EConsumable.Cigarette);
				_closeUpsController.Consumable.SetOnUseAction(OnUse);
				_closeUpsController.Consumable.SetOnCloseAction(OnClose);
				_closeUpsController.Consumable.Show().Forget();
			}
		}

		private void OnClose()
		{
			_cinemachineCam?.SetActive(value: false);
			_raycastTarget.SetLockedState(isLocked: false);
		}

		private void OnUse()
		{
			_isInteracting = true;
			_hudPresenter.HideHint();
			_hudPresenter.PlayAnimation(EHUDAnimation.SmokeCigarette);
			_soundService.PlayOneShot(ESoundSource.s1_GameScene_2DSFXSource, ESound.Consum_cigarette_2);
			_localizedStringCigarettes.RefreshString();
			_localizedStringCigarettes.Arguments = new List<object> { _consumablesController.Count(EConsumable.Cigarette) };
			_dayNightController.RefillEnergy();
			_dayNightController.RemoveExtraEnergySlotsForTomorrow(1);
			_playerService.Signs.Smoke();
			_raycastTarget.SetLockedState(isLocked: true);
			EnableAfterPause(2.5f).Forget();
		}

		private async UniTaskVoid EnableAfterPause(float smokeDelay)
		{
			await UniTask.Delay(TimeSpan.FromSeconds(smokeDelay));
			_raycastTarget.SetLockedState(_consumablesController.Count(EConsumable.Cigarette) <= 0);
			_isInteracting = false;
		}

		public override void OnLoad()
		{
			UpdateString();
		}
	}
}
