using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using _Code.Infrastructure.Consumables;
using _Code.Infrastructure.ControlsViewer;
using _Code.Menues.HUD.Animations;
using _Code.Player;
using _Scripts.Services.Sound.Service;

namespace _Code.Menues.HUD;

public interface IHUDView
{
	Gun Gun { get; }

	Ending Ending { get; }

	Camera PlayerCamera { get; }

	event Action OnShootAnimationCompleted;

	void Init(IInputHandlerProvider inputHandlerProvider, INotAHumanSoundService soundService);

	void SetActionsCount(int count, bool isSkipAnim);

	void SetFilledActionsCount(int count);

	void SetActionsActiveState(bool isActive);

	UniTask FadeOverlay(Color color, float time, Ease ease, CancellationToken cancellationToken = default(CancellationToken));

	void SetHintData(string subject, string action, Transform target, ERaycastHintIcon icon);

	UniTask ShowHint();

	UniTask HideHint();

	UniTask AnimateFallAsleep(float[] randomSleepAlphas, float[] randomSleepTimes);

	UniTask WakeUp();

	UniTask PlayAnimation(EHUDAnimation animation);

	void AnimateShootOverlay();

	UniTask ShowGameSaved();

	UniTask HideControlsView();

	UniTaskVoid SetupAndShowControlsView(EControlsList controlsList);

	void SetControlsAvailability(EControl control, bool isAvailable);

	void SetHintAvailability(bool isAvailable);

	void ShowScreamer();

	void SetHintFadedState(bool isFaded);

	EControlsList GetLastControlsList();

	void ShowItemReceivedHint(EConsumable item, int count);

	void ShowItemGivenAwayHint(EConsumable item, int count);

	void EnableDream();

	void DisableDream();

	UniTask ShowGameSavedOnClose();
}
