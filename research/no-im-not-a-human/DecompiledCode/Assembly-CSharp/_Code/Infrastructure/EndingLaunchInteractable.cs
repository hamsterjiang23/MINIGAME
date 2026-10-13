using Cysharp.Threading.Tasks;
using UnityEngine;
using _Code.Infrastructure.Endings;
using _Code.Infrastructure.Endings.Gameplay;
using _Code.Infrastructure.Pause;
using _Code.Infrastructure.Player;
using _Code.Menues.HUD;

namespace _Code.Infrastructure;

public sealed class EndingLaunchInteractable : AInteractableObject
{
	[SerializeField]
	private EEnding _ending;

	private IHUDPresenter _hudPresenter;

	private IPlayerService _playerService;

	private IGameplayEndingManager _gameplayEndingManager;

	private bool _hasAlreadyInteracted;

	public override bool HardConditions => true;

	public override bool SoftConditions => !_hasAlreadyInteracted;

	public void Init(IHUDPresenter hudPresenter, IPauseController pauseController, IPlayerService playerService, IGameplayEndingManager gameplayEndingManager)
	{
		_hudPresenter = hudPresenter;
		_playerService = playerService;
		_gameplayEndingManager = gameplayEndingManager;
		_raycastTarget.Init(hudPresenter, pauseController, () => HardConditions);
	}

	public override void Interact()
	{
		InteractAsync().Forget();
		_hasAlreadyInteracted = true;
	}

	private async UniTask InteractAsync()
	{
		_playerService.MakeImmovable();
		_hudPresenter.HideHint().Forget();
		_raycastTarget.SetLockedState(isLocked: true);
		await _hudPresenter.FadeIn(0.5f);
		_gameplayEndingManager.GetEnding(_ending).ShowClip().Forget();
	}

	public void SetEnding(EEnding ending)
	{
		_ending = ending;
	}
}
