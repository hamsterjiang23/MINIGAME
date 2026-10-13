using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using RoboRyanTron.SearchableEnum;
using UnityEngine;
using _Code.Events;
using _Code.Infrastructure.CloseUps;
using _Code.Infrastructure.DayNight;
using _Code.Infrastructure.Pause;
using _Code.Infrastructure.Sound;
using _Code.Menues.HUD;
using _Code.Utils.Attributes.MinMaxRange;
using _Scripts.Services.Sound;
using _Scripts.Services.Sound.Service;

namespace _Code.Infrastructure
{
	public sealed class RadioInteractable : AInteractableObject
	{
		[SerializeField]
		private GameObject _radio3dObject;

		[SerializeField]
		[MinMaxRange(0f, 60f)]
		private Vector2 _randomSoundTimeRange;

		[SerializeField]
		[SearchableEnum]
		private ESound[] _randomSounds;

		private ICloseUpsController _closeUpsController;

		private IHUDPresenter _hudPresenter;

		private IDayNightController _dayNightController;

		private INotAHumanSoundService _soundService;

		private float _randomSoundTime;

		private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

		public override bool HardConditions => _dayNightController.CurrentTimeOfDay == ETimeOfDay.Day;

		public override bool SoftConditions => true;

		public event Action RadioTaken;

		public event Action RadioRemoved;

		public void Init(ICloseUpsController closeUpsController, IHUDPresenter hudPresenter, IPauseController pauseController, IDayNightController dayNightController, INotAHumanSoundService soundService)
		{
			_dayNightController = dayNightController;
			_closeUpsController = closeUpsController;
			_hudPresenter = hudPresenter;
			_soundService = soundService;
			_raycastTarget.Init(hudPresenter, pauseController, () => HardConditions);
			_closeUpsController.Radio.Left -= OnRadioClosed;
			_closeUpsController.Radio.Left += OnRadioClosed;
			_closeUpsController.Radio.Entered -= OnRadioOpened;
			_closeUpsController.Radio.Entered += OnRadioOpened;
			_dayNightController.Changed -= OnDayTimeChanged;
			_dayNightController.Changed += OnDayTimeChanged;
		}

		private void OnDayTimeChanged(ETimeOfDay timeOfDay)
		{
			if (timeOfDay == ETimeOfDay.Day)
			{
				_randomSoundTime = UnityEngine.Random.Range(_randomSoundTimeRange.x, _randomSoundTimeRange.y);
				RunRandomSoundCheck().Forget();
			}
		}

		private async UniTask RunRandomSoundCheck()
		{
			await UniTask.Delay(TimeSpan.FromSeconds(_randomSoundTime), ignoreTimeScale: false, PlayerLoopTiming.Update, _cancellationTokenSource.Token);
			if (_dayNightController.CurrentTimeOfDay != ETimeOfDay.Night && !_closeUpsController.Radio.HasUsedToday)
			{
				AttractAttention();
				_randomSoundTime = UnityEngine.Random.Range(_randomSoundTimeRange.x, _randomSoundTimeRange.y);
				if (_dayNightController.CurrentTimeOfDay == ETimeOfDay.Day && !_closeUpsController.Radio.HasUsedToday)
				{
					RunRandomSoundCheck().Forget();
				}
			}
		}

		public override void Interact()
		{
			_hudPresenter.HideHint();
			_closeUpsController.Radio.Show().Forget();
		}

		private void OnRadioOpened()
		{
			RadioTaken?.Invoke();
			_radio3dObject.SetActive(value: false);
		}

		private void OnRadioClosed()
		{
			RadioRemoved?.Invoke();
			_radio3dObject.SetActive(value: true);
		}

		private void AttractAttention()
		{
			_soundService.Play(ESoundSource.s1_GameScene_Radio, _randomSounds.GetRandomElement());
			_radio3dObject.transform.DOShakeRotation(1f, 10f);
		}
	}
}
