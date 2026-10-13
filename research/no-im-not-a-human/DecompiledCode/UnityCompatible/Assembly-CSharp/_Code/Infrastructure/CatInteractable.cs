using System;
using Cysharp.Threading.Tasks;
using RoboRyanTron.SearchableEnum;
using UnityEngine;
using _Code.Infrastructure.ActionableObjects;
using _Code.Infrastructure.Sound;
using _Code.Infrastructure._NINAH__Cat;
using _Code.Menues.HUD;
using _Code.Menues.HUD.Animations;
using _Scripts.Services.Sound;
using _Scripts.Services.Sound.Service;

namespace _Code.Infrastructure
{
	public sealed class CatInteractable : AInteractableObject
	{
		[SerializeField]
		private ParticleSystem _particleSystem;

		[SerializeField]
		[SearchableEnum]
		private ESound[] _catPurrSounds;

		[SerializeField]
		[SearchableEnum]
		private ESound[] _catGrabSounds;

		[SerializeField]
		[SearchableEnum]
		private ESound[] _catDropSounds;

		private INotAHumanSoundService _soundService;

		private IHUDPresenter _hudPresenter;

		private IActionableObjectsManager _actionableObjectsManager;

		private ICatController _catController;

		public override bool HardConditions => true;

		public override bool SoftConditions => true;

		public event Action NaperdyshReleased;

		public override void Interact()
		{
		}

		public void Pet()
		{
			_soundService.PlayOneShot(ESoundSource.s1_GameScene_2DSFXSource, _catPurrSounds.GetRandomElement());
			_particleSystem.Play();
			PauseInteracting().Forget();
		}

		private async UniTaskVoid PauseInteracting()
		{
			_hudPresenter.HideHint().Forget();
			_raycastTarget.SetLockedState(isLocked: true);
			await UniTask.Delay(TimeSpan.FromSeconds(2.0));
			_raycastTarget.SetLockedState(isLocked: false);
		}

		public void Init(IHUDPresenter hudPresenter, INotAHumanSoundService soundService, IActionableObjectsManager actionableObjectsManager, ICatController catController)
		{
			_hudPresenter = hudPresenter;
			_soundService = soundService;
			_actionableObjectsManager = actionableObjectsManager;
			_catController = catController;
			_catController.DayStarted -= OnDayStarted;
			_catController.DayStarted += OnDayStarted;
			_catController.NightStarted -= OnNightStarted;
			_catController.NightStarted += OnNightStarted;
		}

		private void OnDayStarted()
		{
			_raycastTarget.SetLockedState(isLocked: false);
		}

		private void OnNightStarted()
		{
			_raycastTarget.SetLockedState(isLocked: true);
		}

		public void Take()
		{
			gameObject.SetActive(value: false);
			_actionableObjectsManager.SetLockedStateForAll(isLocked: true);
			TakeAsync().Forget();
		}

		private async UniTaskVoid TakeAsync()
		{
			float duration = UnityEngine.Random.Range(4f, 7f);
			_soundService.PlayOneShotAsync(ESoundSource.s1_GameScene_2DSFXSource, _catGrabSounds.GetRandomElement());
			await _hudPresenter.PlayAnimation(EHUDAnimation.TakeNaperdysh);
			_hudPresenter.PlayAnimation(EHUDAnimation.HoldNaperdysh).Forget();
			await UniTask.Delay(TimeSpan.FromSeconds(duration));
			_soundService.PlayOneShotAsync(ESoundSource.s1_GameScene_2DSFXSource, _catDropSounds.GetRandomElement());
			await _hudPresenter.PlayAnimation(EHUDAnimation.ReleaseNaperdysh);
			gameObject.SetActive(value: true);
			_catController.PlayAnimation();
			_actionableObjectsManager.SetLockedStateForAll(isLocked: false);
			NaperdyshReleased?.Invoke();
		}

		public void ResetAnim()
		{
			if (gameObject.activeSelf)
			{
				_catController.PlayAnimation();
			}
		}
	}
}
