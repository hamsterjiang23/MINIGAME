using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using _Code.Infrastructure.Consumables;
using _Code.Infrastructure.ControlsViewer;
using _Code.Infrastructure.ViewProvider;
using _Code.Menues.HUD.Animations;
using _Code.Player;

namespace _Code.Menues.HUD
{
	public sealed class MockHudPresenter : IHUDPresenter
	{
		private IHUDView _view;

		public event Func<int> GetMaxDayActions;

		public event Func<int> GetDayActions;

		public MockHudPresenter(IViewProvider viewProvider)
		{
			_view = viewProvider.HUDView;
		}

		public void InitActionsCount()
		{
			throw new NotImplementedException();
		}

		public void InitActionsCount(bool isSkipAnim = false)
		{
			throw new NotImplementedException();
		}

		public void SetActionsCountActive(bool hasActionsCount)
		{
			throw new NotImplementedException();
		}

		public UniTask Death(string cause, bool showEndingAfter = false, Camera camera = null)
		{
			throw new NotImplementedException();
		}

		public async UniTask ShowHint(string subject, string action, Transform target, ERaycastHintIcon icon)
		{
			_view.SetHintData(subject, action, target, icon);
			await _view.ShowHint();
		}

		public async UniTask HideHint()
		{
			await _view.HideHint();
		}

		public void AnimateFallAsleep(out float[] randomSleepTimes)
		{
			throw new NotImplementedException();
		}

		public UniTask WakeUp()
		{
			throw new NotImplementedException();
		}

		public UniTask PlayAnimation(EHUDAnimation animation)
		{
			throw new NotImplementedException();
		}

		public async UniTask FadeIn(float appearingTime, CancellationToken token = default(CancellationToken))
		{
			await _view.FadeOverlay(Color.black, appearingTime, Ease.OutSine, token);
		}

		public async UniTask FadeOut(float disappearingTime, CancellationToken token = default(CancellationToken))
		{
			await _view.FadeOverlay(Color.clear, disappearingTime, Ease.InSine, token);
		}

		public void GunShow()
		{
			throw new NotImplementedException();
		}

		public void GunHide()
		{
			throw new NotImplementedException();
		}

		public UniTask ShowGameSaved()
		{
			throw new NotImplementedException();
		}

		public void HideControlsView()
		{
			throw new NotImplementedException();
		}

		public void SetupAndShowControlsView(EControlsList controlsList)
		{
			throw new NotImplementedException();
		}

		public void SetControlsAvailability(EControl control, bool isAvailable)
		{
			throw new NotImplementedException();
		}

		public void SetHintAvailability(bool isAvailable)
		{
			throw new NotImplementedException();
		}

		public void ShowScreamer()
		{
			throw new NotImplementedException();
		}

		public void SetHintFadedState(bool isFaded)
		{
			_view.SetHintFadedState(isFaded);
		}

		public void ShowItemReceivedHint(EConsumable item, int count)
		{
			throw new NotImplementedException();
		}

		public void ShowItemGivenAwayHint(EConsumable item, int count)
		{
			throw new NotImplementedException();
		}

		public void EnableDream()
		{
			throw new NotImplementedException();
		}

		public void DisableDream()
		{
			throw new NotImplementedException();
		}

		public void HideAction()
		{
			throw new NotImplementedException();
		}

		public UniTask ShowGameSavedOnClose()
		{
			throw new NotImplementedException();
		}
	}
}
