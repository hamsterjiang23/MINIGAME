using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using DefaultNamespace.EnumEventBus;
using UnityEngine;
using UnityEngine.SceneManagement;
using _Code.Characters;
using _Code.DialogSystem;
using _Code.Infrastructure.Consumables;
using _Code.Infrastructure.ControlsViewer;
using _Code.Infrastructure.EnumEventBus;
using _Code.Infrastructure.ViewProvider;
using _Code.Menues.HUD.Animations;
using _Code.Player;
using _Scripts.Services.Sound.Service;

namespace _Code.Menues.HUD;

public sealed class HUDPresenter : IHUDPresenter
{
	private IHUDView _view;

	private IDialogManager _dialogManager;

	private readonly ICharactersManager _charactersManager;

	private EControlsList _lastControlsList;

	private readonly IConsumablesController _consumablesController;

	public event Func<int> GetMaxDayActions;

	public event Func<int> GetDayActions;

	public HUDPresenter(IViewProvider view, IDialogManager dialogManager, IInputHandlerProvider inputHandlerProvider, INotAHumanSoundService soundService, ICharactersManager charactersManager, IConsumablesController consumablesController, CommonEnumEventus enumEventus)
	{
		_view = view.HUDView;
		_dialogManager = dialogManager;
		_charactersManager = charactersManager;
		_consumablesController = consumablesController;
		SubscribeEvents();
		_view.Init(inputHandlerProvider, soundService);
		_view.OnShootAnimationCompleted += OnShotAnimationCompleted;
		enumEventus.SubscribeAction<float>(ECommonEvents.HudFadeIn, OnFadeIn);
	}

	private void SubscribeEvents()
	{
		_dialogManager.GunShowed -= GunShow;
		_dialogManager.GunHidden -= GunHide;
		_dialogManager.GunShot -= GunShot;
		_dialogManager.FakedShot -= OnFakeShot;
		_dialogManager.Dead -= OnDeadBySuper;
		_dialogManager.ShowedAura -= OnShowedAura;
		_dialogManager.GivenPovistka -= OnGivenPovistka;
		_dialogManager.FedCat -= OnFedCat;
		_dialogManager.FadedIn -= OnFadeIn;
		_dialogManager.FadedOut -= OnFadeOut;
		_dialogManager.DialogStarted -= OnDialogStarted;
		_dialogManager.SubtitleStarted -= OnDialogStarted;
		_dialogManager.DialogEnded -= OnDialogEnded;
		_charactersManager.ScreamerShowed -= ShowScreamer;
		_dialogManager.BaseDialogLineShowed -= OnBaseDialogLineShowed;
		_dialogManager.ButtonsDialogLineShowed -= OnButtonsDialogLineShowed;
		_dialogManager.ArmpitsWashed -= OnArmpitsWashed;
		_dialogManager.PlayedAnimation -= OnPlayedAnimation;
		_consumablesController.ReceivedWithHint -= ShowItemReceivedHint;
		_consumablesController.GivenAwayWithHint -= ShowItemGivenAwayHint;
		_dialogManager.GunShowed += GunShow;
		_dialogManager.GunHidden += GunHide;
		_dialogManager.GunShot += GunShot;
		_dialogManager.FakedShot += OnFakeShot;
		_dialogManager.Dead += OnDeadBySuper;
		_dialogManager.ShowedAura += OnShowedAura;
		_dialogManager.GivenPovistka += OnGivenPovistka;
		_dialogManager.FedCat += OnFedCat;
		_dialogManager.FadedIn += OnFadeIn;
		_dialogManager.FadedOut += OnFadeOut;
		_dialogManager.DialogStarted += OnDialogStarted;
		_dialogManager.SubtitleStarted += OnDialogStarted;
		_dialogManager.DialogEnded += OnDialogEnded;
		_charactersManager.ScreamerShowed += ShowScreamer;
		_dialogManager.BaseDialogLineShowed += OnBaseDialogLineShowed;
		_dialogManager.ButtonsDialogLineShowed += OnButtonsDialogLineShowed;
		_dialogManager.ArmpitsWashed += OnArmpitsWashed;
		_dialogManager.PlayedAnimation += OnPlayedAnimation;
		_consumablesController.ReceivedWithHint += ShowItemReceivedHint;
		_consumablesController.GivenAwayWithHint += ShowItemGivenAwayHint;
	}

	private void OnFakeShot()
	{
		_view.Gun.FakeShot().Forget();
	}

	private void OnPlayedAnimation(EHUDAnimation animation)
	{
		_view.PlayAnimation(animation);
	}

	private void OnArmpitsWashed()
	{
		PlayAnimation(EHUDAnimation.WatchCleanHands).Forget();
	}

	private void OnButtonsDialogLineShowed()
	{
		SetControlsAvailability(EControl.Listen, isAvailable: false);
		SetControlsAvailability(EControl.Answer, isAvailable: true);
	}

	private void OnBaseDialogLineShowed()
	{
		SetControlsAvailability(EControl.Listen, isAvailable: true);
		SetControlsAvailability(EControl.Answer, isAvailable: false);
	}

	private void OnDialogStarted()
	{
		_lastControlsList = _view.GetLastControlsList();
		_view.SetupAndShowControlsView(EControlsList.InDialog);
	}

	private void OnDialogEnded(bool isEndedDialog, bool isEndedSubtitle)
	{
		_view.SetupAndShowControlsView(_lastControlsList);
	}

	private void OnFadeIn(float duration)
	{
		Debug.Log("FADE");
		FadeIn(duration).Forget();
	}

	private void OnFadeOut(float duration)
	{
		FadeOut(duration).Forget();
	}

	private void OnFedCat()
	{
		_view.PlayAnimation(EHUDAnimation.OpenTin);
	}

	private void OnGivenPovistka()
	{
		_view.PlayAnimation(EHUDAnimation.ThrowPovistka);
	}

	private void OnShowedAura()
	{
		_view.PlayAnimation(EHUDAnimation.CapturePhoto);
	}

	private void OnDeadBySuper(string cause, Camera camera)
	{
		Death(cause, showEndingAfter: false, camera).Forget();
	}

	public async UniTask PlayAnimation(EHUDAnimation animation)
	{
		await _view.PlayAnimation(animation);
	}

	public async UniTask FadeIn(float appearingTime, CancellationToken token = default(CancellationToken))
	{
		await _view.FadeOverlay(Color.black, appearingTime, Ease.OutSine, token);
	}

	public async UniTask FadeOut(float appearingTime, CancellationToken token = default(CancellationToken))
	{
		await _view.FadeOverlay(Color.clear, appearingTime, Ease.InSine, token);
	}

	public void InitActionsCount(bool isSkipAnim = false)
	{
		_view.SetActionsCount(GetMaxDayActions?.Invoke() ?? 0, isSkipAnim);
		_view.SetFilledActionsCount(GetDayActions?.Invoke() ?? 0);
	}

	public void SetActionsCountActive(bool hasActionsCount)
	{
		_view.SetActionsActiveState(hasActionsCount);
	}

	public async UniTask Death(string cause, bool showEndingAfter = false, Camera camera = null)
	{
		if (camera == null)
		{
			camera = _view.PlayerCamera;
		}
		await _view.FadeOverlay(Color.black, 1f, Ease.InSine);
		_dialogManager.DialogEnded += (bool dialog, bool subtitle) =>
		{
			DeathDialogEnded(showEndingAfter);
		};
		_dialogManager.ShowSubtitle(cause, camera, EDialogOverlayType.FullBlack);
		_view.FadeOverlay(Color.clear, 0.1f, Ease.InSine);
	}

	private void DeathDialogEnded(bool showEndingAfter)
	{
		SceneManager.LoadScene(0);
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

	public void SetHintAvailability(bool isAvailable)
	{
		_view.SetHintAvailability(isAvailable);
	}

	public void AnimateFallAsleep(out float[] randomSleepTimes)
	{
		float[] randomSleepAlphas = new float[2]
		{
			UnityEngine.Random.Range(0.7f, 0.8f),
			UnityEngine.Random.Range(0.8f, 0.9f)
		};
		randomSleepTimes = new float[3]
		{
			UnityEngine.Random.Range(0.4f, 0.6f),
			UnityEngine.Random.Range(0.6f, 0.8f),
			UnityEngine.Random.Range(0.8f, 1f)
		};
		_view.AnimateFallAsleep(randomSleepAlphas, randomSleepTimes).Forget();
	}

	public async UniTask WakeUp()
	{
		await _view.WakeUp();
	}

	public void GunShow()
	{
		_view.Gun.LoadGun().Forget();
	}

	public void GunHide()
	{
		_view.Gun.HideGun().Forget();
	}

	public async UniTask ShowGameSaved()
	{
		await _view.ShowGameSaved();
	}

	public async UniTask ShowGameSavedOnClose()
	{
		await _view.ShowGameSavedOnClose();
	}

	private void GunShot()
	{
		_view.Gun.Shot().Forget();
		_view.AnimateShootOverlay();
	}

	private void OnShotAnimationCompleted()
	{
		_dialogManager.CompleteShotAnimation();
	}

	public void HideControlsView()
	{
		_view.HideControlsView();
	}

	public void SetupAndShowControlsView(EControlsList controlsList)
	{
		_view.SetupAndShowControlsView(controlsList);
	}

	public void SetControlsAvailability(EControl control, bool isAvailable)
	{
		_view.SetControlsAvailability(control, isAvailable);
	}

	public void ShowScreamer()
	{
		_view.ShowScreamer();
	}

	public void SetHintFadedState(bool isFaded)
	{
		_view.SetHintFadedState(isFaded);
	}

	public void ShowItemReceivedHint(EConsumable item, int count)
	{
		_view.ShowItemReceivedHint(item, count);
	}

	public void ShowItemGivenAwayHint(EConsumable item, int count)
	{
		_view.ShowItemGivenAwayHint(item, count);
	}

	public void EnableDream()
	{
		_view.EnableDream();
	}

	public void DisableDream()
	{
		_view.DisableDream();
	}

	public void HideAction()
	{
		_view.SetActionsCount(0, isSkipAnim: true);
	}
}
