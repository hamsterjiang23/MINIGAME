using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using RoboRyanTron.SearchableEnum;
using SABI;
using UnityEngine;
using UnityEngine.Localization;
using _Code.Infrastructure.CloseUps;
using _Code.Infrastructure.Consumables;
using _Code.Infrastructure.DayNight;
using _Code.Infrastructure.Pause;
using _Code.Infrastructure.Player;
using _Code.Infrastructure.Sound;
using _Code.Menues.HUD;
using _Code.Menues.HUD.Animations;
using _Scripts.Services.DataModel;
using _Scripts.Services.Sound;
using _Scripts.Services.Sound.Service;

namespace _Code.Infrastructure
{
	public sealed class SaveInteractable : AInteractableObject
	{
		[SerializeField]
		private GameObject _cinemachineCam;

		[SerializeField]
		private GameObject[] _saveObject3D;

		[SerializeField]
		private Transform _movingThing;

		[SerializeField]
		private LocalizedString _localizedString;

		[SerializeField]
		[SearchableEnum]
		private ESound[] _kombuchaSounds;

		private INotAHumanSoundService _soundService;

		private IHUDPresenter _hudPresenter;

		private IDataModelService _dataModelService;

		private IConsumablesController _consumablesController;

		private IPlayerService _playerService;

		private ICloseUpsController _closeUpsController;

		private int _actualCount;

		private Vector3 _movingThingStartPosition;

		private bool _isInteracting;

		private IDayNightController _dayNightController;

		public override bool HardConditions
		{
			get
			{
				if (_consumablesController.Count(EConsumable.Kombucha) > 0)
				{
					return _dayNightController.Day < 14;
				}
				return false;
			}
		}

		public override bool SoftConditions => true;

		public void Init(IHUDPresenter hudPresenter, IPauseController pauseController, IDataModelService dataModelService, IConsumablesController consumablesController, IPlayerService playerService, ICloseUpsController closeUpsController, IDayNightController dayNightController, INotAHumanSoundService soundService)
		{
			_hudPresenter = hudPresenter;
			_dataModelService = dataModelService;
			_consumablesController = consumablesController;
			_closeUpsController = closeUpsController;
			_playerService = playerService;
			_dayNightController = dayNightController;
			_raycastTarget.Init(hudPresenter, pauseController, () => HardConditions);
			UpdateConsumableCount();
			_consumablesController.UpdatedCount -= OnUpdatedConsumablesCount;
			_consumablesController.UpdatedCount += OnUpdatedConsumablesCount;
			_soundService = soundService;
			_raycastTarget.SetLockedState(_consumablesController.Count(EConsumable.Kombucha) == 0);
		}

		private void OnDestroy()
		{
			_consumablesController.UpdatedCount -= OnUpdatedConsumablesCount;
		}

		private void OnUpdatedConsumablesCount(EConsumable consumable, int count)
		{
			if (consumable == EConsumable.Kombucha)
			{
				_actualCount = count;
				GameObject[] saveObject3D = _saveObject3D;
				for (int i = 0; i < saveObject3D.Length; i++)
				{
					saveObject3D[i].SetActive(count > 0);
				}
				if (count > 0 && _raycastTarget.IsLocked)
				{
					_raycastTarget.SetLockedState(isLocked: false);
				}
				UpdateConsumableCount();
			}
		}

		private void UpdateConsumableCount()
		{
			_localizedString.Arguments = new List<object> { _consumablesController.Count(EConsumable.Kombucha) };
			_localizedString.RefreshString();
			_raycastTarget.SetSubjectLocalizedString(_localizedString);
		}

		public override void Interact()
		{
			if (!_isInteracting && !_closeUpsController.Consumable.IsAnimating)
			{
				_cinemachineCam?.SetActive(value: true);
				_raycastTarget.SetLockedState(isLocked: true);
				_closeUpsController.Consumable.SetupConsumable(EConsumable.Kombucha);
				_closeUpsController.Consumable.SetOnUseAction(() =>
				{
					InteractAsync().Forget();
				});
				_closeUpsController.Consumable.SetOnCloseAction(OnClose);
				_closeUpsController.Consumable.Show().Forget();
			}
		}

		private void OnClose()
		{
			_cinemachineCam?.SetActive(value: false);
			_raycastTarget.SetLockedState(isLocked: false);
		}

		private async UniTaskVoid InteractAsync()
		{
			_isInteracting = true;
			_dataModelService.GameSaveDataHandler.SaveAll();
			_playerService.MakeImmovable();
			_hudPresenter.HideHint().Forget();
			_raycastTarget.SetLockedState(isLocked: true);
			_raycastTarget.SetLockedState(isLocked: true);
			_saveObject3D.ForEach((GameObject x) =>
			{
				x.gameObject.SetActive(value: false);
			});
			UniTask drinkAnim = _hudPresenter.PlayAnimation(EHUDAnimation.DrinkKombucha);
			_soundService.PlayOneShot(ESoundSource.s1_GameScene_2DSFXSource, _kombuchaSounds.GetRandomElement());
			await UniTask.Delay(TimeSpan.FromSeconds(0.6600000262260437));
			_hudPresenter.ShowGameSaved().Forget();
			await _hudPresenter.FadeIn(0.66f);
			UpdateConsumableCount();
			await drinkAnim;
			await UniTask.Delay(TimeSpan.FromSeconds(0.6600000262260437));
			_saveObject3D.ForEach((GameObject x) =>
			{
				x.gameObject.SetActive(_actualCount > 0);
			});
			await _hudPresenter.FadeOut(0.66f);
			if (_actualCount > 0)
			{
				_raycastTarget.SetLockedState(isLocked: false);
			}
			_playerService.MakeMovable();
			await UniTask.DelayFrame(1);
			_raycastTarget.SetLockedState(isLocked: false);
			_isInteracting = false;
		}

		private void Start()
		{
			_movingThingStartPosition = _movingThing.position;
		}

		private void Update()
		{
			_movingThing.position = _movingThingStartPosition + Vector3.up * (Mathf.Sin(Time.time * 0.33f) * 0.01f);
		}

		public override void OnLoad()
		{
			UpdateConsumableCount();
		}
	}
}
