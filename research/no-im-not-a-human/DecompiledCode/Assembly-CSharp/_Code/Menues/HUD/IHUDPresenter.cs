using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using _Code.Infrastructure.Consumables;
using _Code.Infrastructure.ControlsViewer;
using _Code.Menues.HUD.Animations;
using _Code.Player;

namespace _Code.Menues.HUD;

public interface IHUDPresenter
{
	event Func<int> GetMaxDayActions;

	event Func<int> GetDayActions;

	void InitActionsCount(bool isSkipAnim = false);

	void SetActionsCountActive(bool hasActionsCount);

	UniTask Death(string cause, bool showEndingAfter = false, Camera camera = null);

	UniTask ShowHint(string subject, string action, Transform target, ERaycastHintIcon icon);

	UniTask HideHint();

	void AnimateFallAsleep(out float[] randomSleepTimes);

	UniTask WakeUp();

	UniTask PlayAnimation(EHUDAnimation animation);

	UniTask FadeIn(float appearingTime, CancellationToken token = default(CancellationToken));

	UniTask FadeOut(float disappearingTime, CancellationToken token = default(CancellationToken));

	void GunShow();

	void GunHide();

	UniTask ShowGameSaved();

	void HideControlsView();

	void SetupAndShowControlsView(EControlsList controlsList);

	void SetControlsAvailability(EControl control, bool isAvailable);

	void SetHintAvailability(bool isAvailable);

	void ShowScreamer();

	void SetHintFadedState(bool isFaded);

	void ShowItemReceivedHint(EConsumable item, int count);

	void ShowItemGivenAwayHint(EConsumable item, int count);

	void EnableDream();

	void DisableDream();

	void HideAction();

	UniTask ShowGameSavedOnClose();
}
