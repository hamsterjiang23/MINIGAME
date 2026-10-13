using Cysharp.Threading.Tasks;
using RoboRyanTron.SearchableEnum;
using UnityEngine;
using _Code.Characters.DialogSystem;
using _Code.DialogSystem;
using _Code.Infrastructure.Endings.Gameplay;
using _Code.Infrastructure.Pause;
using _Code.Infrastructure.Player;
using _Code.Infrastructure.Sound;
using _Code.Infrastructure.Sound.Settings;
using _Code.Menues.HUD;
using _Scripts.Services.Sound;
using _Scripts.Services.Sound.Service;

namespace _Code.Infrastructure
{
	public sealed class WindowBoardsInteractable : AInteractableObject
	{
		[SerializeField]
		private GameObject _boards;

		[SerializeField]
		private GameObject[] _disableObjects;

		[SerializeField]
		private Camera _subtitleCamera;

		[SerializeField]
		[SearchableEnum]
		private ESound[] _boardingSounds;

		private IHUDPresenter _hudPresenter;

		private INotAHumanSoundService _soundService;

		private IPlayerService _playerService;

		private IGameplayEndingManager _gameplayEndingManager;

		private IDialogManager _dialogManager;

		private bool _hasAlreadyInteracted;

		public override bool HardConditions => true;

		public override bool SoftConditions => true;

		public void Init(IHUDPresenter hudPresenter, IPauseController pauseController, INotAHumanSoundService soundService, IPlayerService playerService, IGameplayEndingManager gameplayEndingManager, IDialogManager dialogManager)
		{
			_hudPresenter = hudPresenter;
			_soundService = soundService;
			_playerService = playerService;
			_dialogManager = dialogManager;
			_gameplayEndingManager = gameplayEndingManager;
			_raycastTarget.Init(hudPresenter, pauseController, () => HardConditions);
		}

		public override void Interact()
		{
			if (!_hasAlreadyInteracted)
			{
				InteractAsync().Forget();
				_hasAlreadyInteracted = true;
			}
		}

		private async UniTask InteractAsync()
		{
			_playerService.MakeImmovable();
			_hudPresenter.HideHint();
			_raycastTarget.SetLockedState(isLocked: true);
			await _hudPresenter.FadeIn(0.5f);
			_boards.gameObject.SetActive(value: true);
			GameObject[] disableObjects = _disableObjects;
			for (int i = 0; i < disableObjects.Length; i++)
			{
				disableObjects[i].SetActive(value: false);
			}
			await _soundService.PlayAsync(ESoundSource.s1_GameScene_2DSFXSource, _boardingSounds.GetRandomElement(), new SoundSettingRandomPitch(0.05f));
			if (!_gameplayEndingManager.TryNailUpWindowForBasement())
			{
				AGameplayEnding actualEnding = _gameplayEndingManager.ActualEnding;
				if (actualEnding is GameplayEndingStayAlive { NailedUpWindowsCount: var nailedUpWindowsCount })
				{
					_dialogManager.ShowSubtitlePopup(nailedUpWindowsCount switch
					{
						1 => EInfoMessageType.WindowNailedUp1, 
						2 => EInfoMessageType.WindowNailedUp2, 
						_ => EInfoMessageType.INeedToNailUpWindows, 
					});
				}
				else if (actualEnding is GameplayEndingStayAliveWithThem { NailedUpWindowsCount: var nailedUpWindowsCount2 })
				{
					_dialogManager.ShowSubtitlePopup(nailedUpWindowsCount2 switch
					{
						1 => EInfoMessageType.WindowNailedUp1, 
						2 => EInfoMessageType.WindowNailedUp2, 
						_ => EInfoMessageType.INeedToNailUpWindows, 
					});
				}
				await _hudPresenter.FadeOut(0.5f);
				_playerService.MakeMovable();
			}
		}
	}
}
