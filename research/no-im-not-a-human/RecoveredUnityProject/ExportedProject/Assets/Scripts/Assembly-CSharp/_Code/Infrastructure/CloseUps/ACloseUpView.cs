using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using _Code.Infrastructure.Cursor;
using _Code.Infrastructure.Player;
using _Code.Infrastructure.Updatable;
using _Code.Menues.HUD;
using _Code.Player;
using _Scripts.Services.Sound.Service;

namespace _Code.Infrastructure.CloseUps
{
	public abstract class ACloseUpView : MonoBehaviour, ICloseUpView, IUpdateable
	{
		private const float APPEARING_TIME = 0.5f;

		protected IHUDPresenter HUDPresenter;

		protected ICursorController CursorController;

		protected IPlayerService PlayerService;

		protected INotAHumanSoundService SoundService;

		protected InputHandling InputHandler;

		protected bool IsHoldToClose;

		protected bool HasTutor;

		private bool _isHolding;

		private bool _isChangingState;

		private float _holdProgress;

		protected virtual bool IsUseFade => true;

		public bool IsEntered { get; private set; }

		protected virtual float HoldProgressTarget { get; } = 1f;

		protected virtual bool CanHold { get; set; } = true;

		protected bool CanLeave { get; set; } = true;

		public abstract IUpdateable[] Updateables { get; }

		public event Action Entered;

		public event Action Left;

		protected event Action<float> HoldProgressChanged;

		protected event Action TutorSwitched;

		protected event Action TutorLeft;

		public virtual async UniTask Show()
		{
			if (!_isChangingState)
			{
				OnStartShow();
				Entered?.Invoke();
				_isChangingState = true;
				HUDPresenter.SetHintAvailability(isAvailable: false);
				InputHandler.SetIsInUIState(isInUi: true);
				if (IsUseFade)
				{
					await HUDPresenter.FadeIn(0.25f);
				}
				gameObject.SetActive(value: true);
				if (IsUseFade)
				{
					await HUDPresenter.FadeOut(0.25f);
				}
				IsEntered = true;
				_isChangingState = false;
				OnShown();
			}
		}

		public virtual async UniTask Hide()
		{
			if (!_isChangingState && CanLeave)
			{
				await UniTask.DelayFrame(1);
				OnStartHide();
				_isChangingState = true;
				InputHandler.SetIsInUIState(isInUi: false);
				if (IsUseFade)
				{
					await HUDPresenter.FadeIn(0.25f);
				}
				gameObject.SetActive(value: false);
				if (IsUseFade)
				{
					await HUDPresenter.FadeOut(0.25f);
				}
				IsEntered = false;
				HUDPresenter.SetHintAvailability(isAvailable: true);
				_isChangingState = false;
				Left?.Invoke();
				OnHidden();
			}
		}

		protected virtual void OnStartShow()
		{
		}

		protected virtual void OnShown()
		{
		}

		protected virtual void OnStartHide()
		{
		}

		protected virtual void OnHidden()
		{
		}

		public virtual void Init()
		{
		}

		public void InitModules(IHUDPresenter hudPresenter, IPlayerService playerService, ICursorController cursorController, IInputHandlerProvider inputHandlerProvider, INotAHumanSoundService soundService)
		{
			HUDPresenter = hudPresenter;
			PlayerService = playerService;
			CursorController = cursorController;
			SoundService = soundService;
			InputHandler = inputHandlerProvider.InputHandler;
		}

		public virtual void OnUpdateAction()
		{
			if (!IsHoldToClose)
			{
				if (InputHandler.UIExitTriggered && IsEntered && !_isChangingState)
				{
					Hide().Forget();
				}
			}
			else if (CanHold)
			{
				if (InputHandler.UIExitTriggered && IsEntered && !_isChangingState && !_isHolding)
				{
					_isHolding = true;
				}
				if (_isHolding)
				{
					_holdProgress += Time.deltaTime;
					HoldProgressChanged?.Invoke(_holdProgress / HoldProgressTarget);
				}
				if (InputHandler.UIExitReleased && IsEntered && !_isChangingState && _isHolding)
				{
					_isHolding = false;
					_holdProgress = 0f;
					HoldProgressChanged?.Invoke(_holdProgress / HoldProgressTarget);
				}
				if (_holdProgress >= HoldProgressTarget)
				{
					_isHolding = false;
					_holdProgress = 0f;
					Hide().Forget();
				}
			}
			if (HasTutor)
			{
				if (InputHandler.UITutorTriggered)
				{
					TutorSwitched?.Invoke();
				}
				if (InputHandler.UIExitTriggered)
				{
					TutorLeft?.Invoke();
				}
			}
		}
	}
}
