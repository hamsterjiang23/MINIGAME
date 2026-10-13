using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine.SceneManagement;
using _Code.DialogSystem;
using _Code.Infrastructure.DataModel.Models.GameSave;
using _Code.Infrastructure.DayNight;
using _Code.Infrastructure.OtherGameData;
using _Code.Infrastructure._NINAH__Effects;
using _Code.Menues.HUD;
using _Code.Player;
using _Scripts.Services.DataModel;
using _Scripts.Services.Sound;
using _Scripts.Services.Sound.Service;

namespace _Code.Infrastructure._NINAH__Dream
{
	public sealed class DreamController : ASavableClass<DreamSaveData>, IDreamController
	{
		private DreamSaveData _saveData = new DreamSaveData();

		private OtherGameSOData _data;

		private readonly IHUDPresenter _hudPresenter;

		private readonly IDayNightControllerViewProvider _viewProvider;

		private readonly IEffectsController _effectsController;

		private EDream _dreamToShow;

		private readonly INotAHumanSoundService _soundService;

		private readonly WatcherManager _watcherManager;

		private readonly IDataModelService _dataModelService;

		private readonly IDialogManager _dialogManager;

		public bool NeedToShowDream { get; private set; }

		public bool HasSeen(EDream dream)
		{
			return _saveData.SeenDreams.Contains(dream);
		}

		public DreamController(IOtherGameSODataProvider otherGameSoDataProvider, IHUDPresenter hudPresenter, IDayNightControllerViewProvider viewProvider, IEffectsController effectsController, INotAHumanSoundService soundService, WatcherManager watcherManager, IDataModelService dataModelService, IDialogManager dialogManager)
		{
			_data = otherGameSoDataProvider.OtherGameData;
			_hudPresenter = hudPresenter;
			_viewProvider = viewProvider;
			_effectsController = effectsController;
			_soundService = soundService;
			_watcherManager = watcherManager;
			_dataModelService = dataModelService;
			_dialogManager = dialogManager;
			_dialogManager.HadSeenDream -= HasSeen;
			_dialogManager.HadSeenDream += HasSeen;
			LinkSaveDataKey(this, _saveData, _dataModelService.GameSaveDataHandler);
		}

		public void WantToShowDream(EDream dream)
		{
			if (!NeedToShowDream || _dreamToShow != EDream.BodyEater)
			{
				_dreamToShow = dream;
				NeedToShowDream = true;
			}
		}

		public async UniTask ShowDream()
		{
			DreamData dream = _data.Dreams.FirstOrDefault((DreamData x) => x.Dream == _dreamToShow);
			if (dream != null)
			{
				_soundService.FadeVolume(ESoundSource.s1_GameScene_KnockSource, 0f, 0f).Forget();
				if (dream.HasMusic)
				{
					_soundService.FadeIn(ESoundSource.s1_GameScene_2DMusicSourceExtra, dream.Music, 1f, Ease.Unset).Forget();
				}
				_effectsController.SetVolumeActive(isActive: false);
				_hudPresenter.EnableDream();
				_hudPresenter.FadeOut(1f).Forget();
				_watcherManager.EnterState(EWatcherState.Dream);
				InvokeExtraDreamActions(_dreamToShow).Forget();
				SceneManager.LoadScene(dream.SceneIndex, LoadSceneMode.Additive);
				await UniTask.WaitUntil(() => SceneManager.loadedSceneCount == 1);
				_watcherManager.LeaveState(EWatcherState.Dream);
				_hudPresenter.DisableDream();
				_effectsController.SetVolumeActive(isActive: true);
				if (dream.HasMusic)
				{
					_soundService.FadeOut(ESoundSource.s1_GameScene_2DMusicSourceExtra, 1f).Forget();
				}
				_soundService.FadeVolume(ESoundSource.s1_GameScene_KnockSource, 1f, 0f).Forget();
				if (dream.Dream == EDream.BodyEater)
				{
					SteamAchievements.Unlock("BODY_EATER");
				}
				if (dream.Dream == EDream.Mushroom)
				{
					SteamAchievements.Unlock("DREAM_MUSHROOM");
				}
				if (!_saveData.SeenDreams.Contains(dream.Dream))
				{
					_saveData.SeenDreams.Add(dream.Dream);
				}
				NeedToShowDream = false;
			}
		}

		private async UniTaskVoid InvokeExtraDreamActions(EDream dream)
		{
			if (dream == EDream.Mushroom)
			{
				await UniTask.Delay(TimeSpan.FromSeconds(20.0));
				_hudPresenter.FadeIn(0.5f).Forget();
			}
		}

		protected override void OnSaveDataLoad(IGameSaveDataHandler saver)
		{
			DreamSaveData saveData = saver.LoadKey(this) ?? new DreamSaveData();
			_saveData = saveData;
			LinkSaveDataKey(this, _saveData, _dataModelService.GameSaveDataHandler);
		}
	}
}
