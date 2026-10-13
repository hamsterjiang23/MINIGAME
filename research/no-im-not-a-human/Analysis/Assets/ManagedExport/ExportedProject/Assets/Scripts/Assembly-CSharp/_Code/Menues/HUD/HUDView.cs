using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using _Code.Infrastructure.Consumables;
using _Code.Infrastructure.ControlsViewer;
using _Code.Menues.HUD.Animations;
using _Code.Player;
using _Code.Utils.UI.ImageAnimating;
using _Scripts.Services.Sound.Service;

namespace _Code.Menues.HUD
{
	public sealed class HUDView : MonoBehaviour, IHUDView
	{
		[SerializeField]
		private HUDAnimationData[] _animationsData;

		[SerializeField]
		private ActionsView _actions;

		[SerializeField]
		private OpenRoomView _openRoomView;

		[SerializeField]
		private ScreenResolutionBoxesAdjuster _screenResolutionBoxesAdjuster;

		[SerializeField]
		private AnimatedImage _animationLayer;

		[SerializeField]
		private CanvasGroup _gameSaved;

		[SerializeField]
		private ControlsListView _controlsList;

		[SerializeField]
		private ScreamersView _screamersView;

		[SerializeField]
		private ItemHintView _itemHintView;

		[SerializeField]
		private RawImage _dreamImage;

		private bool _isBreakingAnimation;

		private bool _isHintAvailable = true;

		private InputHandling _inputHandler;

		private EControlsList _lastControlsList;

		[field: SerializeField]
		public Image ColorOverlay { get; private set; }

		[field: SerializeField]
		public Gun Gun { get; private set; }

		[field: SerializeField]
		public Ending Ending { get; private set; }

		[field: SerializeField]
		public Camera PlayerCamera { get; private set; }

		public event Action OnShootAnimationCompleted;

		public void Init(IInputHandlerProvider inputHandlerProvider, INotAHumanSoundService soundService)
		{
			_inputHandler = inputHandlerProvider.InputHandler;
			_screenResolutionBoxesAdjuster.AdjustBars();
			_controlsList.InitModules(inputHandlerProvider);
			_screamersView.Init(soundService);
			Gun.InitModules(soundService);
			inputHandlerProvider.InputHandler.InputDeviceChanged -= OnInputChanged;
			inputHandlerProvider.InputHandler.InputDeviceChanged += OnInputChanged;
		}

		private void OnInputChanged(EInputDevice input)
		{
			_openRoomView.UpdateIcon(_inputHandler.CurrentDevice, _inputHandler.GetGaypadType());
			UpdateControlsList();
		}

		public void SetActionsCount(int count, bool isSkipAnim)
		{
			_actions.SetCount(count, isSkipAnim);
		}

		public void SetFilledActionsCount(int count)
		{
			_actions.SetFilledCount(count);
		}

		public void SetActionsActiveState(bool isActive)
		{
			_actions.gameObject.SetActive(isActive);
		}

		public async UniTask FadeOverlay(Color color, float time, Ease ease, CancellationToken cancellationToken)
		{
			ColorOverlay.raycastTarget = true;
			await ColorOverlay.DOColor(color, time).SetEase(ease).WithCancellation(cancellationToken);
			ColorOverlay.raycastTarget = false;
		}

		public void SetHintData(string subject, string action, Transform target, ERaycastHintIcon icon)
		{
			_openRoomView.SetSubjectName(subject).SetActionName(action).SetTarget(target)
				.SetIcon(icon);
		}

		public async UniTask ShowHint()
		{
			if (_isHintAvailable)
			{
				await _openRoomView.Show();
			}
		}

		public async UniTask HideHint()
		{
			await _openRoomView.Hide();
		}

		public async UniTask AnimateFallAsleep(float[] randomSleepAlphas, float[] randomSleepTimes)
		{
			Sequence sequence = DOTween.Sequence();
			sequence.Append(ColorOverlay.DOColor(new Color(0f, 0f, 0f, randomSleepAlphas[0]), randomSleepTimes[0])).SetEase(Ease.OutSine);
			sequence.Append(ColorOverlay.DOColor(new Color(0f, 0f, 0f, randomSleepAlphas[0] / 2f), randomSleepTimes[0] / 2f)).SetEase(Ease.InSine);
			sequence.Append(ColorOverlay.DOColor(new Color(0f, 0f, 0f, randomSleepAlphas[1]), randomSleepTimes[1])).SetEase(Ease.OutSine);
			sequence.Append(ColorOverlay.DOColor(new Color(0f, 0f, 0f, randomSleepAlphas[1] / 2f), randomSleepTimes[1] / 2f)).SetEase(Ease.InSine);
			sequence.Append(ColorOverlay.DOColor(new Color(0f, 0f, 0f, 1f), randomSleepTimes[2])).SetEase(Ease.OutSine);
			await (Tween)sequence;
		}

		public async UniTask WakeUp()
		{
			await (Tween)ColorOverlay.DOColor(new Color(0f, 0f, 0f, 0f), 1f).SetEase(Ease.InBounce);
		}

		public async UniTask PlayAnimation(EHUDAnimation animationType)
		{
			HUDAnimationData hUDAnimationData = _animationsData.FirstOrDefault((HUDAnimationData x) => x.AnimationType == animationType);
			if (hUDAnimationData != null)
			{
				_animationLayer.SetData(hUDAnimationData.AnimationData);
				_animationLayer.StartAnimationOrSetFirstFrame();
				await UniTask.Delay(TimeSpan.FromSeconds(hUDAnimationData.AnimationData.Duration));
			}
		}

		public void AnimateShootOverlay()
		{
			Sequence s = DOTween.Sequence();
			s.Append(ColorOverlay.DOColor(Color.white, 0.2f).SetEase(Ease.InCubic));
			s.Append(ColorOverlay.DOColor(Color.black, 1f).SetEase(Ease.OutSine));
			s.AppendCallback(() =>
			{
				OnShootAnimationCompleted?.Invoke();
			});
			s.Append(ColorOverlay.DOColor(Color.clear, 0.5f).SetEase(Ease.OutSine));
		}

		public async UniTask ShowGameSaved()
		{
			_gameSaved.gameObject.SetActive(value: true);
			await (Tween)_gameSaved.DOFade(1f, 0.66f).SetEase(Ease.InCubic);
			await UniTask.Delay(TimeSpan.FromSeconds(0.6600000262260437));
			await (Tween)_gameSaved.DOFade(0f, 0.66f).SetEase(Ease.OutCubic);
			_gameSaved.gameObject.SetActive(value: false);
		}

		public async UniTask ShowGameSavedOnClose()
		{
			_gameSaved.gameObject.SetActive(value: true);
			await (Tween)_gameSaved.DOFade(1f, 0.66f).SetEase(Ease.InCubic);
			await UniTask.Delay(TimeSpan.FromSeconds(0.6600000262260437));
		}

		public async UniTask HideControlsView()
		{
			_lastControlsList = EControlsList.None;
			await _controlsList.ClearControls();
		}

		public async UniTaskVoid SetupAndShowControlsView(EControlsList controlsList)
		{
			if (_controlsList.ActiveCount != 0)
			{
				await HideControlsView();
			}
			_lastControlsList = controlsList;
			switch (controlsList)
			{
			case EControlsList.InRoom:
				_controlsList.AddControl(EControl.ExitRoom);
				break;
			case EControlsList.InFridge:
				_controlsList.AddControl(EControl.CloseFridge);
				break;
			case EControlsList.InRadio:
				_controlsList.AddControl(EControl.PutRadio);
				_controlsList.AddControl(EControl.ShowTutor);
				_controlsList.AddControl(EControl.SpeedUp);
				if (_inputHandler.CurrentDevice == EInputDevice.Gaypad)
				{
					_controlsList.AddControl(EControl.SwitchToAM);
					_controlsList.AddControl(EControl.SwitchToFM);
					_controlsList.AddControl(EControl.RotateHandle);
				}
				break;
			case EControlsList.InPhone:
				_controlsList.AddControl(EControl.HangPhone);
				break;
			case EControlsList.InUsualCloseUp:
				_controlsList.AddControl(EControl.Leave);
				break;
			case EControlsList.RunZone:
				_controlsList.AddControl(EControl.Run);
				break;
			case EControlsList.InDialog:
				if (_inputHandler.CurrentDevice == EInputDevice.Gaypad)
				{
					_controlsList.AddControl(EControl.Listen);
					_controlsList.AddControl(EControl.Answer);
				}
				break;
			}
		}

		public void UpdateControlsList()
		{
			SetupAndShowControlsView(_lastControlsList).Forget();
		}

		public void SetControlsAvailability(EControl control, bool isAvailable)
		{
			_controlsList.SetControlAvailability(control, isAvailable);
		}

		public void SetHintAvailability(bool isAvailable)
		{
			if (!isAvailable)
			{
				HideHint().Forget();
			}
			_isHintAvailable = isAvailable;
		}

		public void ShowScreamer()
		{
			_screamersView.ShowScreamer().Forget();
		}

		public void SetHintFadedState(bool isFaded)
		{
			_openRoomView.SetFadedState(isFaded);
		}

		public EControlsList GetLastControlsList()
		{
			return _lastControlsList;
		}

		public void ShowItemReceivedHint(EConsumable item, int count)
		{
			_itemHintView.ShowItemReceivedHint(item, count);
		}

		public void ShowItemGivenAwayHint(EConsumable item, int count)
		{
			_itemHintView.ShowItemGivenAwayHint(item, count);
		}

		public void EnableDream()
		{
			_dreamImage.gameObject.SetActive(value: true);
		}

		public void DisableDream()
		{
			_dreamImage.gameObject.SetActive(value: false);
		}
	}
}
